Imports System
Imports AttendanceSystem.Domain.Enums

Namespace Domain.Entities
    Public Class User
        Public Property Id As Integer
        Public Property Username As String = String.Empty
        Public Property PasswordHash As String = String.Empty
        Public Property FullName As String = String.Empty
        Public Property Role As UserRole = UserRole.Employee
        Public Property IsActive As Boolean = True
        Public Property EmployeeId As Nullable(Of Integer)
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property LastLoginAt As Nullable(Of DateTime)
    End Class

    Public Class Department
        Public Property Id As Integer
        Public Property Name As String = String.Empty
        Public Property Code As String = String.Empty
        Public Property Description As String = String.Empty
        Public Property IsActive As Boolean = True
        Public Property CreatedAt As DateTime = DateTime.Now

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    Public Class Position
        Public Property Id As Integer
        Public Property DepartmentId As Integer
        Public Property DepartmentName As String = String.Empty
        Public Property Title As String = String.Empty
        Public Property Description As String = String.Empty
        Public Property IsActive As Boolean = True
        Public Property CreatedAt As DateTime = DateTime.Now

        Public Overrides Function ToString() As String
            Return Title
        End Function
    End Class

    Public Class Shift
        Public Property Id As Integer
        Public Property Name As String = String.Empty
        Public Property StartTime As TimeSpan
        Public Property EndTime As TimeSpan
        Public Property GracePeriodMinutes As Integer = 15
        Public Property BreakDurationMinutes As Integer = 60
        Public Property HalfDayThresholdHours As Double = 4.0
        Public Property IsActive As Boolean = True
        Public Property Description As String = String.Empty

        Public ReadOnly Property DisplayTime As String
            Get
                Return $"{Name} ({StartTime:hh\:mm} - {EndTime:hh\:mm})"
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return DisplayTime
        End Function
    End Class

    Public Class Employee
        Public Property Id As Integer
        Public Property EmployeeCode As String = String.Empty
        Public Property FullName As String = String.Empty
        Public Property DepartmentId As Integer
        Public Property DepartmentName As String = String.Empty
        Public Property PositionId As Integer
        Public Property PositionTitle As String = String.Empty
        Public Property ShiftId As Integer
        Public Property ShiftName As String = String.Empty
        Public Property Phone As String = String.Empty
        Public Property Email As String = String.Empty
        Public Property HireDate As DateTime = DateTime.Today
        Public Property Status As EmployeeStatus = EmployeeStatus.Active
        Public Property Notes As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property UpdatedAt As DateTime = DateTime.Now

        Public Overrides Function ToString() As String
            Return $"{EmployeeCode} - {FullName}"
        End Function
    End Class

    Public Class AttendanceRecord
        Public Property Id As Integer
        Public Property EmployeeId As Integer
        Public Property EmployeeCode As String = String.Empty
        Public Property EmployeeName As String = String.Empty
        Public Property DepartmentName As String = String.Empty
        Public Property ShiftId As Integer
        Public Property ShiftName As String = String.Empty
        Public Property AttendanceDate As DateTime = DateTime.Today
        Public Property CheckIn As Nullable(Of DateTime)
        Public Property CheckOut As Nullable(Of DateTime)
        Public Property WorkedMinutes As Integer = 0
        Public Property LateMinutes As Integer = 0
        Public Property EarlyLeaveMinutes As Integer = 0
        Public Property OvertimeMinutes As Integer = 0
        Public Property Status As AttendanceStatus = AttendanceStatus.Absent
        Public Property Remarks As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property UpdatedAt As DateTime = DateTime.Now

        Public ReadOnly Property WorkedHoursFormatted As String
            Get
                Dim h = WorkedMinutes \ 60
                Dim m = WorkedMinutes Mod 60
                Return $"{h:00}:{m:00}"
            End Get
        End Property

        Public ReadOnly Property OvertimeHoursFormatted As String
            Get
                Dim h = OvertimeMinutes \ 60
                Dim m = OvertimeMinutes Mod 60
                Return $"{h:00}:{m:00}"
            End Get
        End Property
    End Class

    Public Class AttendanceCorrection
        Public Property Id As Integer
        Public Property AttendanceId As Integer
        Public Property EmployeeId As Integer
        Public Property OriginalCheckIn As Nullable(Of DateTime)
        Public Property OriginalCheckOut As Nullable(Of DateTime)
        Public Property CorrectedCheckIn As Nullable(Of DateTime)
        Public Property CorrectedCheckOut As Nullable(Of DateTime)
        Public Property Reason As String = String.Empty
        Public Property ApprovedByUserId As Integer
        Public Property ApprovedByUserName As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
    End Class

    Public Class LeaveType
        Public Property Id As Integer
        Public Property Name As String = String.Empty
        Public Property DefaultAllowanceDays As Integer = 14
        Public Property IsPaid As Boolean = True
        Public Property IsActive As Boolean = True

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    Public Class LeaveRequest
        Public Property Id As Integer
        Public Property EmployeeId As Integer
        Public Property EmployeeCode As String = String.Empty
        Public Property EmployeeName As String = String.Empty
        Public Property DepartmentName As String = String.Empty
        Public Property LeaveTypeId As Integer
        Public Property LeaveTypeName As String = String.Empty
        Public Property StartDate As DateTime = DateTime.Today
        Public Property EndDate As DateTime = DateTime.Today
        Public Property DaysCount As Integer = 1
        Public Property Reason As String = String.Empty
        Public Property Status As LeaveStatus = LeaveStatus.Pending
        Public Property ApprovedByUserId As Nullable(Of Integer)
        Public Property ApproverName As String = String.Empty
        Public Property ActionNotes As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property UpdatedAt As DateTime = DateTime.Now
    End Class

    Public Class Holiday
        Public Property Id As Integer
        Public Property Name As String = String.Empty
        Public Property HolidayDate As DateTime = DateTime.Today
        Public Property IsRecurring As Boolean = False
        Public Property Description As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
    End Class

    Public Class AppSetting
        Public Property KeyName As String = String.Empty
        Public Property Value As String = String.Empty
        Public Property Description As String = String.Empty
        Public Property Category As String = String.Empty
        Public Property UpdatedAt As DateTime = DateTime.Now
    End Class

    Public Class AuditLog
        Public Property Id As Integer
        Public Property UserId As Nullable(Of Integer)
        Public Property Username As String = String.Empty
        Public Property Action As String = String.Empty
        Public Property ModuleName As String = String.Empty
        Public Property Details As String = String.Empty
        Public Property Timestamp As DateTime = DateTime.Now
    End Class
End Namespace
