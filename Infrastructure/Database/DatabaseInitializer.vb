Imports System
Imports System.IO
Imports AttendanceSystem.Infrastructure.Security

Namespace Infrastructure.Database
    Public Class DatabaseInitializer
        Public Shared Sub Initialize()
            Dim dbExists = File.Exists(DatabaseContext.DbFilePath)

            CreateSchema()

            If Not dbExists OrElse IsTableEmpty("Users") Then
                SeedDefaultData()
            End If
        End Sub

        Private Shared Sub CreateSchema()
            Dim schemaSql As String = "
                CREATE TABLE IF NOT EXISTS Settings (
                    KeyName TEXT PRIMARY KEY,
                    Value TEXT NOT NULL,
                    Description TEXT,
                    Category TEXT,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Departments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    Code TEXT NOT NULL UNIQUE,
                    Description TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Positions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DepartmentId INTEGER NOT NULL,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE RESTRICT
                );

                CREATE TABLE IF NOT EXISTS Shifts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT NOT NULL,
                    GracePeriodMinutes INTEGER NOT NULL DEFAULT 15,
                    BreakDurationMinutes INTEGER NOT NULL DEFAULT 60,
                    HalfDayThresholdHours REAL NOT NULL DEFAULT 4.0,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    Description TEXT
                );

                CREATE TABLE IF NOT EXISTS Employees (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EmployeeCode TEXT NOT NULL UNIQUE,
                    FullName TEXT NOT NULL,
                    DepartmentId INTEGER NOT NULL,
                    PositionId INTEGER NOT NULL,
                    ShiftId INTEGER NOT NULL,
                    Phone TEXT,
                    Email TEXT,
                    HireDate TEXT NOT NULL,
                    Status INTEGER NOT NULL DEFAULT 1,
                    Notes TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE RESTRICT,
                    FOREIGN KEY (PositionId) REFERENCES Positions(Id) ON DELETE RESTRICT,
                    FOREIGN KEY (ShiftId) REFERENCES Shifts(Id) ON DELETE RESTRICT
                );

                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FullName TEXT NOT NULL,
                    Role INTEGER NOT NULL DEFAULT 4,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    EmployeeId INTEGER,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastLoginAt DATETIME,
                    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE SET NULL
                );

                CREATE TABLE IF NOT EXISTS Attendance (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EmployeeId INTEGER NOT NULL,
                    ShiftId INTEGER NOT NULL,
                    AttendanceDate TEXT NOT NULL,
                    CheckIn TEXT,
                    CheckOut TEXT,
                    WorkedMinutes INTEGER NOT NULL DEFAULT 0,
                    LateMinutes INTEGER NOT NULL DEFAULT 0,
                    EarlyLeaveMinutes INTEGER NOT NULL DEFAULT 0,
                    OvertimeMinutes INTEGER NOT NULL DEFAULT 0,
                    Status INTEGER NOT NULL DEFAULT 3,
                    Remarks TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ShiftId) REFERENCES Shifts(Id) ON DELETE RESTRICT,
                    UNIQUE(EmployeeId, AttendanceDate)
                );

                CREATE TABLE IF NOT EXISTS AttendanceCorrections (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AttendanceId INTEGER NOT NULL,
                    EmployeeId INTEGER NOT NULL,
                    OriginalCheckIn TEXT,
                    OriginalCheckOut TEXT,
                    CorrectedCheckIn TEXT,
                    CorrectedCheckOut TEXT,
                    Reason TEXT NOT NULL,
                    ApprovedByUserId INTEGER NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (AttendanceId) REFERENCES Attendance(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id)
                );

                CREATE TABLE IF NOT EXISTS LeaveTypes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    DefaultAllowanceDays INTEGER NOT NULL DEFAULT 14,
                    IsPaid INTEGER NOT NULL DEFAULT 1,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS LeaveRequests (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EmployeeId INTEGER NOT NULL,
                    LeaveTypeId INTEGER NOT NULL,
                    StartDate TEXT NOT NULL,
                    EndDate TEXT NOT NULL,
                    DaysCount INTEGER NOT NULL DEFAULT 1,
                    Reason TEXT NOT NULL,
                    Status INTEGER NOT NULL DEFAULT 1,
                    ApprovedByUserId INTEGER,
                    ActionNotes TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE,
                    FOREIGN KEY (LeaveTypeId) REFERENCES LeaveTypes(Id) ON DELETE RESTRICT,
                    FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id)
                );

                CREATE TABLE IF NOT EXISTS Holidays (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    HolidayDate TEXT NOT NULL UNIQUE,
                    IsRecurring INTEGER NOT NULL DEFAULT 0,
                    Description TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS AuditLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER,
                    Username TEXT,
                    Action TEXT NOT NULL,
                    ModuleName TEXT NOT NULL,
                    Details TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE INDEX IF NOT EXISTS IX_Attendance_Date ON Attendance(AttendanceDate);
                CREATE INDEX IF NOT EXISTS IX_Attendance_Employee ON Attendance(EmployeeId);
                CREATE INDEX IF NOT EXISTS IX_Employees_Department ON Employees(DepartmentId);
                CREATE INDEX IF NOT EXISTS IX_LeaveRequests_Employee ON LeaveRequests(EmployeeId);
                CREATE INDEX IF NOT EXISTS IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
            "

            DbHelper.ExecuteNonQuery(schemaSql)
        End Sub

        Private Shared Function IsTableEmpty(tableName As String) As Boolean
            Dim count = DbHelper.ExecuteScalar(Of Long)($"SELECT COUNT(*) FROM {tableName}")
            Return count = 0
        End Function

        Private Shared Sub SeedDefaultData()
            Dim insertSettings As String = "
                INSERT OR IGNORE INTO Settings (KeyName, Value, Description, Category) VALUES
                ('CompanyName', 'Vertex Technologies Inc.', 'Official company name for reports and interface', 'General'),
                ('WorkStartTime', '08:00', 'Standard default work start time', 'Attendance'),
                ('WorkEndTime', '17:00', 'Standard default work end time', 'Attendance'),
                ('GracePeriodMinutes', '15', 'Allowed grace period in minutes before marked late', 'Attendance'),
                ('DefaultBreakMinutes', '60', 'Standard unpaid or deducted break duration', 'Attendance'),
                ('OvertimeThresholdHours', '8', 'Minimum worked hours before overtime kicks in', 'Attendance'),
                ('WeekendDays', 'Saturday,Sunday', 'Comma separated weekend days', 'Calendar'),
                ('DateFormat', 'yyyy-MM-dd', 'Default system date display format', 'General');
            "
            DbHelper.ExecuteNonQuery(insertSettings)

            Dim insertDepartments As String = "
                INSERT INTO Departments (Name, Code, Description) VALUES
                ('Information Technology', 'IT', 'Software development and technical infrastructure'),
                ('Human Resources', 'HR', 'Personnel management, payroll and compliance'),
                ('Finance & Accounting', 'FIN', 'Corporate finance, budgeting and auditing'),
                ('Sales & Marketing', 'MKT', 'Client relations and business development'),
                ('Operations', 'OPS', 'Day-to-day facilities and client operations');
            "
            DbHelper.ExecuteNonQuery(insertDepartments)

            Dim insertPositions As String = "
                INSERT INTO Positions (DepartmentId, Title, Description) VALUES
                (1, 'Senior Software Engineer', 'Core systems engineering and architecture'),
                (1, 'Database Administrator', 'Database reliability and performance'),
                (1, 'QA Automation Engineer', 'Testing and quality assurance'),
                (2, 'HR Specialist', 'Recruitment and employee relations'),
                (2, 'Payroll Officer', 'Attendance and payroll processing'),
                (3, 'Financial Analyst', 'Budget reporting and analysis'),
                (4, 'Account Executive', 'Enterprise client management'),
                (5, 'Operations Manager', 'Facilities and workflow supervision');
            "
            DbHelper.ExecuteNonQuery(insertPositions)

            Dim insertShifts As String = "
                INSERT INTO Shifts (Name, StartTime, EndTime, GracePeriodMinutes, BreakDurationMinutes, HalfDayThresholdHours, Description) VALUES
                ('Standard Morning', '08:00', '17:00', 15, 60, 4.0, 'Standard 8 AM to 5 PM shift'),
                ('Early Shift', '07:00', '15:30', 10, 30, 4.0, 'Early operations shift'),
                ('Evening Shift', '16:00', '00:00', 15, 60, 4.0, 'Evening technical support shift'),
                ('Night Shift', '00:00', '08:00', 15, 60, 4.0, 'Overnight operations shift');
            "
            DbHelper.ExecuteNonQuery(insertShifts)

            Dim insertLeaveTypes As String = "
                INSERT INTO LeaveTypes (Name, DefaultAllowanceDays, IsPaid) VALUES
                ('Annual Leave', 21, 1),
                ('Sick Leave', 10, 1),
                ('Casual Leave', 7, 1),
                ('Maternity / Paternity', 60, 1),
                ('Unpaid Leave', 30, 0);
            "
            DbHelper.ExecuteNonQuery(insertLeaveTypes)

            Dim adminHash = PasswordHasher.HashPassword("Admin@123")
            Dim hrHash = PasswordHasher.HashPassword("Hr@123")
            Dim insertUsers As String = $"
                INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive) VALUES
                ('admin', '{adminHash}', 'System Administrator', 1, 1),
                ('hrmanager', '{hrHash}', 'Nour El-Din Hassan (HR)', 2, 1);
            "
            DbHelper.ExecuteNonQuery(insertUsers)

            Dim insertEmployees As String = "
                INSERT INTO Employees (EmployeeCode, FullName, DepartmentId, PositionId, ShiftId, Phone, Email, HireDate, Status, Notes) VALUES
                ('EMP-1001', 'Ahmed Mohamed Ali', 1, 1, 1, '+20 100 123 4567', 'ahmed.ali@company.com', '2026-01-10', 1, 'Lead system engineer'),
                ('EMP-1002', 'Mahmoud Ibrahim El-Sayed', 1, 2, 1, '+20 111 234 5678', 'mahmoud.ibrahim@company.com', '2026-01-15', 1, 'Senior DBA'),
                ('EMP-1003', 'Mostafa Tarek Osman', 1, 3, 1, '+20 122 345 6789', 'mostafa.tarek@company.com', '2026-02-01', 1, 'QA specialist'),
                ('EMP-1004', 'Nour El-Din Hassan', 2, 4, 1, '+20 102 456 7890', 'nour.hassan@company.com', '2026-01-05', 1, 'HR Manager'),
                ('EMP-1005', 'Karim Adel Mansour', 2, 5, 1, '+20 114 567 8901', 'karim.adel@company.com', '2026-02-10', 1, 'Payroll officer'),
                ('EMP-1006', 'Mona Khaled Abdel-Rahman', 3, 6, 1, '+20 120 678 9012', 'mona.khaled@company.com', '2026-02-15', 1, 'Financial analyst'),
                ('EMP-1007', 'Omar Youssef El-Shazly', 4, 7, 2, '+20 106 789 0123', 'omar.youssef@company.com', '2026-03-01', 1, 'Sales lead'),
                ('EMP-1008', 'Tariq Mahmoud El-Gohary', 5, 8, 1, '+20 115 890 1234', 'tariq.elgohary@company.com', '2026-01-20', 1, 'Operations manager'),
                ('EMP-1009', 'Yasmine Hany Soliman', 1, 1, 3, '+20 127 901 2345', 'yasmine.hany@company.com', '2026-03-10', 1, 'Evening shift engineer'),
                ('EMP-1010', 'Hassan Amr Radwan', 5, 8, 1, '+20 109 012 3456', 'hassan.radwan@company.com', '2026-03-15', 1, 'Field supervisor');
            "
            DbHelper.ExecuteNonQuery(insertEmployees)

            Dim currentYear = DateTime.Today.Year
            Dim insertHolidays As String = $"
                INSERT INTO Holidays (Name, HolidayDate, IsRecurring, Description) VALUES
                ('New Year''s Day', '{currentYear}-01-01', 1, 'Public Holiday'),
                ('Labor Day', '{currentYear}-05-01', 1, 'International Workers Day'),
                ('Independence Day', '{currentYear}-07-04', 1, 'National Holiday'),
                ('National Day', '{currentYear}-12-02', 1, 'Annual Public Celebration');
            "
            DbHelper.ExecuteNonQuery(insertHolidays)

            SeedSampleAttendance()
        End Sub

        Private Shared Sub SeedSampleAttendance()
            Dim today = DateTime.Today
            For i As Integer = 5 To 0 Step -1
                Dim targetDate = today.AddDays(-i)
                If targetDate.DayOfWeek = DayOfWeek.Saturday OrElse targetDate.DayOfWeek = DayOfWeek.Sunday Then
                    Continue For
                End If

                Dim dateStr = targetDate.ToString("yyyy-MM-dd")

                Dim cin1 = targetDate.ToString("yyyy-MM-dd") & " 07:55:00"
                Dim cout1 = targetDate.ToString("yyyy-MM-dd") & " 17:05:00"
                DbHelper.ExecuteNonQuery("INSERT OR IGNORE INTO Attendance (EmployeeId, ShiftId, AttendanceDate, CheckIn, CheckOut, WorkedMinutes, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes, Status, Remarks) VALUES (1, 1, @dt, @cin, @cout, 490, 0, 0, 10, 1, 'On time')",
                    DbHelper.CreateParam("@dt", dateStr),
                    DbHelper.CreateParam("@cin", cin1),
                    DbHelper.CreateParam("@cout", cout1))

                Dim cin2 = targetDate.ToString("yyyy-MM-dd") & " 08:35:00"
                Dim cout2 = targetDate.ToString("yyyy-MM-dd") & " 17:00:00"
                DbHelper.ExecuteNonQuery("INSERT OR IGNORE INTO Attendance (EmployeeId, ShiftId, AttendanceDate, CheckIn, CheckOut, WorkedMinutes, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes, Status, Remarks) VALUES (2, 1, @dt, @cin, @cout, 445, 35, 0, 0, 2, 'Traffic delay')",
                    DbHelper.CreateParam("@dt", dateStr),
                    DbHelper.CreateParam("@cin", cin2),
                    DbHelper.CreateParam("@cout", cout2))

                Dim cin3 = targetDate.ToString("yyyy-MM-dd") & " 08:00:00"
                Dim cout3 = targetDate.ToString("yyyy-MM-dd") & " 19:30:00"
                DbHelper.ExecuteNonQuery("INSERT OR IGNORE INTO Attendance (EmployeeId, ShiftId, AttendanceDate, CheckIn, CheckOut, WorkedMinutes, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes, Status, Remarks) VALUES (3, 1, @dt, @cin, @cout, 630, 0, 0, 150, 1, 'Approved deployment overtime')",
                    DbHelper.CreateParam("@dt", dateStr),
                    DbHelper.CreateParam("@cin", cin3),
                    DbHelper.CreateParam("@cout", cout3))

                Dim cin4 = targetDate.ToString("yyyy-MM-dd") & " 08:05:00"
                Dim cout4 = targetDate.ToString("yyyy-MM-dd") & " 17:00:00"
                DbHelper.ExecuteNonQuery("INSERT OR IGNORE INTO Attendance (EmployeeId, ShiftId, AttendanceDate, CheckIn, CheckOut, WorkedMinutes, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes, Status, Remarks) VALUES (4, 1, @dt, @cin, @cout, 475, 0, 0, 0, 1, 'Regular day')",
                    DbHelper.CreateParam("@dt", dateStr),
                    DbHelper.CreateParam("@cin", cin4),
                    DbHelper.CreateParam("@cout", cout4))

                Dim cin5 = targetDate.ToString("yyyy-MM-dd") & " 07:50:00"
                Dim cout5 = targetDate.ToString("yyyy-MM-dd") & " 17:00:00"
                DbHelper.ExecuteNonQuery("INSERT OR IGNORE INTO Attendance (EmployeeId, ShiftId, AttendanceDate, CheckIn, CheckOut, WorkedMinutes, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes, Status, Remarks) VALUES (5, 1, @dt, @cin, @cout, 490, 0, 0, 0, 1, 'Regular day')",
                    DbHelper.CreateParam("@dt", dateStr),
                    DbHelper.CreateParam("@cin", cin5),
                    DbHelper.CreateParam("@cout", cout5))
            Next

            DbHelper.ExecuteNonQuery("INSERT OR IGNORE INTO LeaveRequests (EmployeeId, LeaveTypeId, StartDate, EndDate, DaysCount, Reason, Status, ActionNotes) VALUES (6, 1, @st, @ed, 3, 'Annual vacation', 2, 'Approved by HR')",
                DbHelper.CreateParam("@st", today.AddDays(-2).ToString("yyyy-MM-dd")),
                DbHelper.CreateParam("@ed", today.ToString("yyyy-MM-dd")))
        End Sub
    End Class
End Namespace
