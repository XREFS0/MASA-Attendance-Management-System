Imports System
Imports System.Text
Imports System.IO
Imports System.Windows.Forms
Imports System.Text.RegularExpressions

Namespace Common.Utilities
    Public Module ValidationHelper
        Private ReadOnly EmailRegex As New Regex("^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase Or RegexOptions.Compiled)

        Public Function IsValidEmail(email As String) As Boolean
            If String.IsNullOrWhiteSpace(email) Then Return True ' Optional fields can be empty
            Return EmailRegex.IsMatch(email.Trim())
        End Function

        Public Function IsValidPhone(phone As String) As Boolean
            If String.IsNullOrWhiteSpace(phone) Then Return True
            Return phone.Length >= 6 AndAlso phone.Length <= 25
        End Function

        Public Function IsRequired(value As String) As Boolean
            Return Not String.IsNullOrWhiteSpace(value)
        End Function
    End Module

    Public Module CsvExporter
        Public Function ExportToCsv(grid As DataGridView, defaultFileName As String) As Boolean
            Using sfd As New SaveFileDialog()
                sfd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
                sfd.FileName = defaultFileName & "_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"

                If sfd.ShowDialog() = DialogResult.OK Then
                    Try
                        Dim sb As New StringBuilder()

                        Dim visibleCols = grid.Columns.Cast(Of DataGridViewColumn)().Where(Function(c) c.Visible).ToList()
                        Dim headers = visibleCols.Select(Function(c) EscapeCsvField(c.HeaderText))
                        sb.AppendLine(String.Join(",", headers))

                        For Each row As DataGridViewRow In grid.Rows
                            If Not row.IsNewRow Then
                                Dim cells = visibleCols.Select(Function(c) EscapeCsvField(If(row.Cells(c.Index).Value?.ToString(), "")))
                                sb.AppendLine(String.Join(",", cells))
                            End If
                        Next

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8)
                        MessageBox.Show("Data exported successfully to: " & Environment.NewLine & sfd.FileName, "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Return True
                    Catch ex As Exception
                        MessageBox.Show("Failed to export data: " & ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return False
                    End Try
                End If
            End Using
            Return False
        End Function

        Private Function EscapeCsvField(value As String) As String
            If String.IsNullOrEmpty(value) Then Return """"""
            If value.Contains(",") OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) Then
                Return """" & value.Replace("""", """""") & """"
            End If
            Return """" & value & """"
        End Function
    End Module
End Namespace
