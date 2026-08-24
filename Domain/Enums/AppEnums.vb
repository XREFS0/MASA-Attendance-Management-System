Namespace Domain.Enums
    Public Enum UserRole
        Admin = 1
        HrManager = 2
        Supervisor = 3
        Employee = 4
    End Enum

    Public Enum EmployeeStatus
        Active = 1
        Inactive = 0
        Terminated = 2
        OnProbation = 3
    End Enum

    Public Enum AttendanceStatus
        Present = 1
        Late = 2
        Absent = 3
        HalfDay = 4
        OnLeave = 5
        Holiday = 6
    End Enum

    Public Enum LeaveStatus
        Pending = 1
        Approved = 2
        Rejected = 3
        Cancelled = 4
    End Enum
End Namespace
