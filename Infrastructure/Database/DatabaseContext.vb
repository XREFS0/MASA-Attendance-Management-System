Imports System
Imports System.IO
Imports System.Data
Imports Microsoft.Data.Sqlite

Namespace Infrastructure.Database
    Public Class DatabaseContext
        Private Shared _dbFilePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attendance.db")

        Public Shared Property DbFilePath As String
            Get
                Return _dbFilePath
            End Get
            Set(value As String)
                _dbFilePath = value
            End Set
        End Property

        Public Shared ReadOnly Property ConnectionString As String
            Get
                Return $"Data Source={_dbFilePath};Foreign Keys=True;"
            End Get
        End Property

        Public Shared Function CreateConnection() As SqliteConnection
            Dim conn As New SqliteConnection(ConnectionString)
            conn.Open()
            Return conn
        End Function
    End Class

    Public Class DbHelper
        Public Shared Function ExecuteNonQuery(sql As String, ParamArray parameters As SqliteParameter()) As Integer
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = sql
                    If parameters IsNot Nothing Then
                        For Each p In parameters
                            cmd.Parameters.Add(p)
                        Next
                    End If
                    Return cmd.ExecuteNonQuery()
                End Using
            End Using
        End Function

        Public Shared Function ExecuteScalar(Of T)(sql As String, ParamArray parameters As SqliteParameter()) As T
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = sql
                    If parameters IsNot Nothing Then
                        For Each p In parameters
                            cmd.Parameters.Add(p)
                        Next
                    End If
                    Dim result = cmd.ExecuteScalar()
                    If result Is Nothing OrElse Convert.IsDBNull(result) Then
                        Return CType(Nothing, T)
                    End If
                    Return CType(Convert.ChangeType(result, GetType(T)), T)
                End Using
            End Using
        End Function

        Public Shared Function ExecuteDataTable(sql As String, ParamArray parameters As SqliteParameter()) As DataTable
            Dim dt As New DataTable()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = sql
                    If parameters IsNot Nothing Then
                        For Each p In parameters
                            cmd.Parameters.Add(p)
                        Next
                    End If
                    Using reader = cmd.ExecuteReader()
                        For i As Integer = 0 To reader.FieldCount - 1
                            Dim colName = reader.GetName(i)
                            Dim colType = reader.GetFieldType(i)
                            If String.IsNullOrEmpty(colName) Then colName = "Col" & i
                            
                            Dim uniqueName = colName
                            Dim suffix = 1
                            While dt.Columns.Contains(uniqueName)
                                uniqueName = colName & "_" & suffix
                                suffix += 1
                            End While
                            
                            Dim dc As New DataColumn(uniqueName, If(colType, GetType(Object)))
                            dc.AllowDBNull = True
                            dt.Columns.Add(dc)
                        Next

                        While reader.Read()
                            Dim row = dt.NewRow()
                            For i As Integer = 0 To reader.FieldCount - 1
                                If reader.IsDBNull(i) Then
                                    row(i) = DBNull.Value
                                Else
                                    row(i) = reader.GetValue(i)
                                End If
                            Next
                            dt.Rows.Add(row)
                        End While
                    End Using
                End Using
            End Using
            Return dt
        End Function

        Public Shared Function ExecuteTransaction(action As Action(Of SqliteConnection, SqliteTransaction)) As Boolean
            Using conn = DatabaseContext.CreateConnection()
                Using trans = conn.BeginTransaction()
                    Try
                        action(conn, trans)
                        trans.Commit()
                        Return True
                    Catch ex As Exception
                        trans.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        End Function

        Public Shared Function CreateParam(name As String, value As Object) As SqliteParameter
            If value Is Nothing Then
                Return New SqliteParameter(name, DBNull.Value)
            End If
            Return New SqliteParameter(name, value)
        End Function
    End Class
End Namespace
