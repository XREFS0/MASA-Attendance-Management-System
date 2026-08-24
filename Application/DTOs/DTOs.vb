Imports System
Imports System.Collections.Generic
Imports AttendanceSystem.Domain.Enums

Namespace Application.DTOs
    Public Class DashboardStatsDto
        Public Property TotalEmployees As Integer
        Public Property PresentToday As Integer
        Public Property LateToday As Integer
        Public Property AbsentToday As Integer
        Public Property OnLeaveToday As Integer
        Public Property AttendanceRate As Double
    End Class

    Public Class DepartmentAttendanceDto
        Public Property DepartmentName As String
        Public Property TotalStaff As Integer
        Public Property PresentCount As Integer
        Public Property LateCount As Integer
        Public Property AbsentCount As Integer
    End Class

    Public Class EmployeeAttendanceSummaryDto
        Public Property EmployeeCode As String
        Public Property FullName As String
        Public Property DepartmentName As String
        Public Property TotalDays As Integer
        Public Property PresentDays As Integer
        Public Property LateDays As Integer
        Public Property AbsentDays As Integer
        Public Property TotalWorkedHours As Double
        Public Property TotalOvertimeHours As Double
    End Class
End Namespace
