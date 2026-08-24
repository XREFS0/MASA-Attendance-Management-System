Imports System
Imports System.Security.Cryptography
Imports System.Text
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Domain.Enums

Namespace Infrastructure.Security
    Public Module PasswordHasher
        Public Function HashPassword(password As String) As String
            If String.IsNullOrEmpty(password) Then Return String.Empty
            Using sha256 As SHA256 = SHA256.Create()
                Dim bytes As Byte() = Encoding.UTF8.GetBytes(password & "AttendanceSalt_2026")
                Dim hash As Byte() = sha256.ComputeHash(bytes)
                Return Convert.ToBase64String(hash)
            End Using
        End Function

        Public Function VerifyPassword(password As String, storedHash As String) As Boolean
            If String.IsNullOrEmpty(password) OrElse String.IsNullOrEmpty(storedHash) Then
                Return False
            End If
            Dim computed = HashPassword(password)
            Return String.Equals(computed, storedHash, StringComparison.Ordinal)
        End Function
    End Module

    Public Class SessionManager
        Private Shared _currentUser As User

        Public Shared Property CurrentUser As User
            Get
                Return _currentUser
            End Get
            Set(value As User)
                _currentUser = value
            End Set
        End Property

        Public Shared ReadOnly Property IsLoggedIn As Boolean
            Get
                Return _currentUser IsNot Nothing
            End Get
        End Property

        Public Shared ReadOnly Property IsAdmin As Boolean
            Get
                Return _currentUser IsNot Nothing AndAlso _currentUser.Role = UserRole.Admin
            End Get
        End Property

        Public Shared ReadOnly Property CanManageAttendance As Boolean
            Get
                If _currentUser Is Nothing Then Return False
                Return _currentUser.Role = UserRole.Admin OrElse
                       _currentUser.Role = UserRole.HrManager OrElse
                       _currentUser.Role = UserRole.Supervisor
            End Get
        End Property

        Public Shared ReadOnly Property CanManageEmployees As Boolean
            Get
                If _currentUser Is Nothing Then Return False
                Return _currentUser.Role = UserRole.Admin OrElse
                       _currentUser.Role = UserRole.HrManager
            End Get
        End Property

        Public Shared Sub Logout()
            _currentUser = Nothing
        End Sub
    End Class
End Namespace
