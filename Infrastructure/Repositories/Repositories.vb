Imports System
Imports System.Data
Imports System.Collections.Generic
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Domain.Enums
Imports AttendanceSystem.Infrastructure.Database
Imports AttendanceSystem.Infrastructure.Security

Namespace Infrastructure.Repositories
    Public Class UserRepository
        Public Function GetByUsername(username As String) As User
            Dim dt = DbHelper.ExecuteDataTable("SELECT * FROM Users WHERE Username = @u LIMIT 1",
                                              DbHelper.CreateParam("@u", username))
            If dt.Rows.Count = 0 Then Return Nothing
            Return MapRow(dt.Rows(0))
        End Function

        Public Function GetById(id As Integer) As User
            Dim dt = DbHelper.ExecuteDataTable("SELECT * FROM Users WHERE Id = @id",
                                              DbHelper.CreateParam("@id", id))
            If dt.Rows.Count = 0 Then Return Nothing
            Return MapRow(dt.Rows(0))
        End Function

        Public Sub UpdateLastLogin(userId As Integer)
            DbHelper.ExecuteNonQuery("UPDATE Users SET LastLoginAt = CURRENT_TIMESTAMP WHERE Id = @id",
                                     DbHelper.CreateParam("@id", userId))
        End Sub

        Public Function GetAll() As List(Of User)
            Dim list As New List(Of User)
            Dim dt = DbHelper.ExecuteDataTable("SELECT * FROM Users ORDER BY Username")
            For Each row As DataRow In dt.Rows
                list.Add(MapRow(row))
            Next
            Return list
        End Function

        Public Function UpdatePassword(userId As Integer, newHash As String) As Boolean
            Dim rows = DbHelper.ExecuteNonQuery("UPDATE Users SET PasswordHash = @p WHERE Id = @id",
                                                DbHelper.CreateParam("@p", newHash),
                                                DbHelper.CreateParam("@id", userId))
            Return rows > 0
        End Function

        Public Function Insert(user As User) As Integer
            Return DbHelper.ExecuteScalar(Of Long)("
                INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, EmployeeId)
                VALUES (@u, @p, @fn, @r, @ia, @eid);
                SELECT last_insert_rowid();",
                DbHelper.CreateParam("@u", user.Username),
                DbHelper.CreateParam("@p", user.PasswordHash),
                DbHelper.CreateParam("@fn", user.FullName),
                DbHelper.CreateParam("@r", CInt(user.Role)),
                DbHelper.CreateParam("@ia", If(user.IsActive, 1, 0)),
                DbHelper.CreateParam("@eid", If(user.EmployeeId.HasValue, user.EmployeeId.Value, DBNull.Value)))
        End Function

        Private Function MapRow(row As DataRow) As User
            Return New User With {
                .Id = Convert.ToInt32(row("Id")),
                .Username = row("Username").ToString(),
                .PasswordHash = row("PasswordHash").ToString(),
                .FullName = row("FullName").ToString(),
                .Role = CType(Convert.ToInt32(row("Role")), UserRole),
                .IsActive = Convert.ToInt32(row("IsActive")) = 1,
                .EmployeeId = If(Convert.IsDBNull(row("EmployeeId")), CType(Nothing, Nullable(Of Integer)), Convert.ToInt32(row("EmployeeId"))),
                .CreatedAt = Convert.ToDateTime(row("CreatedAt")),
                .LastLoginAt = If(Convert.IsDBNull(row("LastLoginAt")), CType(Nothing, Nullable(Of DateTime)), Convert.ToDateTime(row("LastLoginAt")))
            }
        End Function
    End Class

    Public Class DepartmentRepository
        Public Function GetAll() As List(Of Department)
            Dim list As New List(Of Department)
            Dim dt = DbHelper.ExecuteDataTable("SELECT * FROM Departments ORDER BY Name")
            For Each row As DataRow In dt.Rows
                list.Add(New Department With {
                    .Id = Convert.ToInt32(row("Id")),
                    .Name = row("Name").ToString(),
                    .Code = row("Code").ToString(),
                    .Description = If(Convert.IsDBNull(row("Description")), "", row("Description").ToString()),
                    .IsActive = Convert.ToInt32(row("IsActive")) = 1
                })
            Next
            Return list
        End Function

        Public Function Insert(dept As Department) As Integer
            Return DbHelper.ExecuteScalar(Of Long)("
                INSERT INTO Departments (Name, Code, Description, IsActive)
                VALUES (@n, @c, @d, @ia);
                SELECT last_insert_rowid();",
                DbHelper.CreateParam("@n", dept.Name),
                DbHelper.CreateParam("@c", dept.Code),
                DbHelper.CreateParam("@d", dept.Description),
                DbHelper.CreateParam("@ia", If(dept.IsActive, 1, 0)))
        End Function

        Public Function Update(dept As Department) As Boolean
            Return DbHelper.ExecuteNonQuery("
                UPDATE Departments SET Name = @n, Code = @c, Description = @d, IsActive = @ia WHERE Id = @id",
                DbHelper.CreateParam("@n", dept.Name),
                DbHelper.CreateParam("@c", dept.Code),
                DbHelper.CreateParam("@d", dept.Description),
                DbHelper.CreateParam("@ia", If(dept.IsActive, 1, 0)),
                DbHelper.CreateParam("@id", dept.Id)) > 0
        End Function
    End Class

    Public Class PositionRepository
        Public Function GetAll() As List(Of Position)
            Dim list As New List(Of Position)
            Dim dt = DbHelper.ExecuteDataTable("
                SELECT p.*, d.Name AS DepartmentName 
                FROM Positions p
                JOIN Departments d ON p.DepartmentId = d.Id
                ORDER BY d.Name, p.Title")
            For Each row As DataRow In dt.Rows
                list.Add(New Position With {
                    .Id = Convert.ToInt32(row("Id")),
                    .DepartmentId = Convert.ToInt32(row("DepartmentId")),
                    .DepartmentName = row("DepartmentName").ToString(),
                    .Title = row("Title").ToString(),
                    .Description = If(Convert.IsDBNull(row("Description")), "", row("Description").ToString()),
                    .IsActive = Convert.ToInt32(row("IsActive")) = 1
                })
            Next
            Return list
        End Function

        Public Function GetByDepartment(departmentId As Integer) As List(Of Position)
            Dim list As New List(Of Position)
            Dim dt = DbHelper.ExecuteDataTable("
                SELECT p.*, d.Name AS DepartmentName 
                FROM Positions p
                JOIN Departments d ON p.DepartmentId = d.Id
                WHERE p.DepartmentId = @did AND p.IsActive = 1
                ORDER BY p.Title",
                DbHelper.CreateParam("@did", departmentId))
            For Each row As DataRow In dt.Rows
                list.Add(New Position With {
                    .Id = Convert.ToInt32(row("Id")),
                    .DepartmentId = Convert.ToInt32(row("DepartmentId")),
                    .DepartmentName = row("DepartmentName").ToString(),
                    .Title = row("Title").ToString(),
                    .Description = If(Convert.IsDBNull(row("Description")), "", row("Description").ToString()),
                    .IsActive = Convert.ToInt32(row("IsActive")) = 1
                })
            Next
            Return list
        End Function
    End Class

    Public Class ShiftRepository
        Public Function GetAll() As List(Of Shift)
            Dim list As New List(Of Shift)
            Dim dt = DbHelper.ExecuteDataTable("SELECT * FROM Shifts ORDER BY Name")
            For Each row As DataRow In dt.Rows
                list.Add(MapRow(row))
            Next
            Return list
        End Function

        Public Function GetById(id As Integer) As Shift
            Dim dt = DbHelper.ExecuteDataTable("SELECT * FROM Shifts WHERE Id = @id", DbHelper.CreateParam("@id", id))
            If dt.Rows.Count = 0 Then Return Nothing
            Return MapRow(dt.Rows(0))
        End Function

        Public Function Insert(shift As Shift) As Integer
            Return DbHelper.ExecuteScalar(Of Long)("
                INSERT INTO Shifts (Name, StartTime, EndTime, GracePeriodMinutes, BreakDurationMinutes, HalfDayThresholdHours, IsActive, Description)
                VALUES (@n, @st, @et, @gp, @bd, @hd, @ia, @desc);
                SELECT last_insert_rowid();",
                DbHelper.CreateParam("@n", shift.Name),
                DbHelper.CreateParam("@st", shift.StartTime.ToString("hh\:mm")),
                DbHelper.CreateParam("@et", shift.EndTime.ToString("hh\:mm")),
                DbHelper.CreateParam("@gp", shift.GracePeriodMinutes),
                DbHelper.CreateParam("@bd", shift.BreakDurationMinutes),
                DbHelper.CreateParam("@hd", shift.HalfDayThresholdHours),
                DbHelper.CreateParam("@ia", If(shift.IsActive, 1, 0)),
                DbHelper.CreateParam("@desc", shift.Description))
        End Function

        Public Function Update(shift As Shift) As Boolean
            Return DbHelper.ExecuteNonQuery("
                UPDATE Shifts SET Name = @n, StartTime = @st, EndTime = @et, GracePeriodMinutes = @gp,
                       BreakDurationMinutes = @bd, HalfDayThresholdHours = @hd, IsActive = @ia, Description = @desc
                WHERE Id = @id",
                DbHelper.CreateParam("@n", shift.Name),
                DbHelper.CreateParam("@st", shift.StartTime.ToString("hh\:mm")),
                DbHelper.CreateParam("@et", shift.EndTime.ToString("hh\:mm")),
                DbHelper.CreateParam("@gp", shift.GracePeriodMinutes),
                DbHelper.CreateParam("@bd", shift.BreakDurationMinutes),
                DbHelper.CreateParam("@hd", shift.HalfDayThresholdHours),
                DbHelper.CreateParam("@ia", If(shift.IsActive, 1, 0)),
                DbHelper.CreateParam("@desc", shift.Description),
                DbHelper.CreateParam("@id", shift.Id)) > 0
        End Function

        Public Function Delete(id As Integer) As Boolean
            Return DbHelper.ExecuteNonQuery("DELETE FROM Shifts WHERE Id = @id", DbHelper.CreateParam("@id", id)) > 0
        End Function

        Private Function MapRow(row As DataRow) As Shift
            Return New Shift With {
                .Id = Convert.ToInt32(row("Id")),
                .Name = row("Name").ToString(),
                .StartTime = TimeSpan.Parse(row("StartTime").ToString()),
                .EndTime = TimeSpan.Parse(row("EndTime").ToString()),
                .GracePeriodMinutes = Convert.ToInt32(row("GracePeriodMinutes")),
                .BreakDurationMinutes = Convert.ToInt32(row("BreakDurationMinutes")),
                .HalfDayThresholdHours = Convert.ToDouble(row("HalfDayThresholdHours")),
                .IsActive = Convert.ToInt32(row("IsActive")) = 1,
                .Description = If(Convert.IsDBNull(row("Description")), "", row("Description").ToString())
            }
        End Function
    End Class

    Public Class EmployeeRepository
        Public Function GetAll(Optional search As String = "", Optional departmentId As Integer = 0, Optional status As Nullable(Of EmployeeStatus) = Nothing) As List(Of Employee)
            Dim list As New List(Of Employee)
            Dim sql As String = "
                SELECT e.*, d.Name AS DepartmentName, p.Title AS PositionTitle, s.Name AS ShiftName
                FROM Employees e
                JOIN Departments d ON e.DepartmentId = d.Id
                JOIN Positions p ON e.PositionId = p.Id
                JOIN Shifts s ON e.ShiftId = s.Id
                WHERE 1=1 "

            Dim params As New List(Of Microsoft.Data.Sqlite.SqliteParameter)

            If Not String.IsNullOrWhiteSpace(search) Then
                sql &= " AND (e.EmployeeCode LIKE @s OR e.FullName LIKE @s OR e.Email LIKE @s) "
                params.Add(DbHelper.CreateParam("@s", "%" & search.Trim() & "%"))
            End If

            If departmentId > 0 Then
                sql &= " AND e.DepartmentId = @did "
                params.Add(DbHelper.CreateParam("@did", departmentId))
            End If

            If status.HasValue Then
                sql &= " AND e.Status = @st "
                params.Add(DbHelper.CreateParam("@st", CInt(status.Value)))
            End If

            sql &= " ORDER BY e.EmployeeCode ASC"

            Dim dt = DbHelper.ExecuteDataTable(sql, params.ToArray())
            For Each row As DataRow In dt.Rows
                list.Add(MapRow(row))
            Next
            Return list
        End Function

        Public Function GetById(id As Integer) As Employee
            Dim dt = DbHelper.ExecuteDataTable("
                SELECT e.*, d.Name AS DepartmentName, p.Title AS PositionTitle, s.Name AS ShiftName
                FROM Employees e
                JOIN Departments d ON e.DepartmentId = d.Id
                JOIN Positions p ON e.PositionId = p.Id
                JOIN Shifts s ON e.ShiftId = s.Id
                WHERE e.Id = @id",
                DbHelper.CreateParam("@id", id))
            If dt.Rows.Count = 0 Then Return Nothing
            Return MapRow(dt.Rows(0))
        End Function

        Public Function GetByCode(code As String) As Employee
            Dim dt = DbHelper.ExecuteDataTable("
                SELECT e.*, d.Name AS DepartmentName, p.Title AS PositionTitle, s.Name AS ShiftName
                FROM Employees e
                JOIN Departments d ON e.DepartmentId = d.Id
                JOIN Positions p ON e.PositionId = p.Id
                JOIN Shifts s ON e.ShiftId = s.Id
                WHERE e.EmployeeCode = @code",
                DbHelper.CreateParam("@code", code.Trim()))
            If dt.Rows.Count = 0 Then Return Nothing
            Return MapRow(dt.Rows(0))
        End Function

        Public Function ExistsCode(code As String, Optional excludeId As Integer = 0) As Boolean
            Dim sql = "SELECT COUNT(*) FROM Employees WHERE EmployeeCode = @code"
            If excludeId > 0 Then sql &= " AND Id <> " & excludeId
            Return DbHelper.ExecuteScalar(Of Long)(sql, DbHelper.CreateParam("@code", code.Trim())) > 0
        End Function

        Public Function Insert(emp As Employee) As Integer
            Return DbHelper.ExecuteScalar(Of Long)("
                INSERT INTO Employees (EmployeeCode, FullName, DepartmentId, PositionId, ShiftId, Phone, Email, HireDate, Status, Notes, UpdatedAt)
                VALUES (@code, @fn, @did, @pid, @sid, @ph, @em, @hd, @st, @nt, CURRENT_TIMESTAMP);
                SELECT last_insert_rowid();",
                DbHelper.CreateParam("@code", emp.EmployeeCode),
                DbHelper.CreateParam("@fn", emp.FullName),
                DbHelper.CreateParam("@did", emp.DepartmentId),
                DbHelper.CreateParam("@pid", emp.PositionId),
                DbHelper.CreateParam("@sid", emp.ShiftId),
                DbHelper.CreateParam("@ph", emp.Phone),
                DbHelper.CreateParam("@em", emp.Email),
                DbHelper.CreateParam("@hd", emp.HireDate.ToString("yyyy-MM-dd")),
                DbHelper.CreateParam("@st", CInt(emp.Status)),
                DbHelper.CreateParam("@nt", emp.Notes))
        End Function

        Public Function Update(emp As Employee) As Boolean
            Return DbHelper.ExecuteNonQuery("
                UPDATE Employees SET EmployeeCode = @code, FullName = @fn, DepartmentId = @did, PositionId = @pid,
                       ShiftId = @sid, Phone = @ph, Email = @em, HireDate = @hd, Status = @st, Notes = @nt, UpdatedAt = CURRENT_TIMESTAMP
                WHERE Id = @id",
                DbHelper.CreateParam("@code", emp.EmployeeCode),
                DbHelper.CreateParam("@fn", emp.FullName),
                DbHelper.CreateParam("@did", emp.DepartmentId),
                DbHelper.CreateParam("@pid", emp.PositionId),
                DbHelper.CreateParam("@sid", emp.ShiftId),
                DbHelper.CreateParam("@ph", emp.Phone),
                DbHelper.CreateParam("@em", emp.Email),
                DbHelper.CreateParam("@hd", emp.HireDate.ToString("yyyy-MM-dd")),
                DbHelper.CreateParam("@st", CInt(emp.Status)),
                DbHelper.CreateParam("@nt", emp.Notes),
                DbHelper.CreateParam("@id", emp.Id)) > 0
        End Function

        Public Function Delete(id As Integer) As Boolean
            Return DbHelper.ExecuteNonQuery("DELETE FROM Employees WHERE Id = @id", DbHelper.CreateParam("@id", id)) > 0
        End Function

        Public Function GetTotalCount() As Integer
            Return Convert.ToInt32(DbHelper.ExecuteScalar(Of Long)("SELECT COUNT(*) FROM Employees WHERE Status = 1"))
        End Function

        Private Function MapRow(row As DataRow) As Employee
            Return New Employee With {
                .Id = Convert.ToInt32(row("Id")),
                .EmployeeCode = row("EmployeeCode").ToString(),
                .FullName = row("FullName").ToString(),
                .DepartmentId = Convert.ToInt32(row("DepartmentId")),
                .DepartmentName = row("DepartmentName").ToString(),
                .PositionId = Convert.ToInt32(row("PositionId")),
                .PositionTitle = row("PositionTitle").ToString(),
                .ShiftId = Convert.ToInt32(row("ShiftId")),
                .ShiftName = row("ShiftName").ToString(),
                .Phone = If(Convert.IsDBNull(row("Phone")), "", row("Phone").ToString()),
                .Email = If(Convert.IsDBNull(row("Email")), "", row("Email").ToString()),
                .HireDate = DateTime.Parse(row("HireDate").ToString()),
                .Status = CType(Convert.ToInt32(row("Status")), EmployeeStatus),
                .Notes = If(Convert.IsDBNull(row("Notes")), "", row("Notes").ToString()),
                .CreatedAt = Convert.ToDateTime(row("CreatedAt")),
                .UpdatedAt = Convert.ToDateTime(row("UpdatedAt"))
            }
        End Function
    End Class

    Public Class AttendanceRepository
        Public Function GetRecords(startDate As DateTime, endDate As DateTime, Optional departmentId As Integer = 0, Optional employeeId As Integer = 0, Optional status As Nullable(Of AttendanceStatus) = Nothing) As List(Of AttendanceRecord)
            Dim list As New List(Of AttendanceRecord)
            Dim sql As String = "
                SELECT a.*, e.EmployeeCode, e.FullName AS EmployeeName, d.Name AS DepartmentName, s.Name AS ShiftName
                FROM Attendance a
                JOIN Employees e ON a.EmployeeId = e.Id
                JOIN Departments d ON e.DepartmentId = d.Id
                JOIN Shifts s ON a.ShiftId = s.Id
                WHERE a.AttendanceDate >= @st AND a.AttendanceDate <= @ed "

            Dim params As New List(Of Microsoft.Data.Sqlite.SqliteParameter) From {
                DbHelper.CreateParam("@st", startDate.ToString("yyyy-MM-dd")),
                DbHelper.CreateParam("@ed", endDate.ToString("yyyy-MM-dd"))
            }

            If departmentId > 0 Then
                sql &= " AND e.DepartmentId = @did "
                params.Add(DbHelper.CreateParam("@did", departmentId))
            End If

            If employeeId > 0 Then
                sql &= " AND a.EmployeeId = @eid "
                params.Add(DbHelper.CreateParam("@eid", employeeId))
            End If

            If status.HasValue Then
                sql &= " AND a.Status = @stt "
                params.Add(DbHelper.CreateParam("@stt", CInt(status.Value)))
            End If

            sql &= " ORDER BY a.AttendanceDate DESC, e.EmployeeCode ASC"

            Dim dt = DbHelper.ExecuteDataTable(sql, params.ToArray())
            For Each row As DataRow In dt.Rows
                list.Add(MapRow(row))
            Next
            Return list
        End Function

        Public Function GetTodayRecord(employeeId As Integer, targetDate As DateTime) As AttendanceRecord
            Dim dt = DbHelper.ExecuteDataTable("
                SELECT a.*, e.EmployeeCode, e.FullName AS EmployeeName, d.Name AS DepartmentName, s.Name AS ShiftName
                FROM Attendance a
                JOIN Employees e ON a.EmployeeId = e.Id
                JOIN Departments d ON e.DepartmentId = d.Id
                JOIN Shifts s ON a.ShiftId = s.Id
                WHERE a.EmployeeId = @eid AND a.AttendanceDate = @dt",
                DbHelper.CreateParam("@eid", employeeId),
                DbHelper.CreateParam("@dt", targetDate.ToString("yyyy-MM-dd")))

            If dt.Rows.Count = 0 Then Return Nothing
            Return MapRow(dt.Rows(0))
        End Function

        Public Function GetById(id As Integer) As AttendanceRecord
            Dim dt = DbHelper.ExecuteDataTable("
                SELECT a.*, e.EmployeeCode, e.FullName AS EmployeeName, d.Name AS DepartmentName, s.Name AS ShiftName
                FROM Attendance a
                JOIN Employees e ON a.EmployeeId = e.Id
                JOIN Departments d ON e.DepartmentId = d.Id
                JOIN Shifts s ON a.ShiftId = s.Id
                WHERE a.Id = @id",
                DbHelper.CreateParam("@id", id))

            If dt.Rows.Count = 0 Then Return Nothing
            Return MapRow(dt.Rows(0))
        End Function

        Public Function Upsert(rec As AttendanceRecord) As Integer
            Dim sql = "
                INSERT INTO Attendance (EmployeeId, ShiftId, AttendanceDate, CheckIn, CheckOut, WorkedMinutes, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes, Status, Remarks, UpdatedAt)
                VALUES (@eid, @sid, @dt, @cin, @cout, @wm, @lm, @elm, @ot, @st, @rem, CURRENT_TIMESTAMP)
                ON CONFLICT(EmployeeId, AttendanceDate) DO UPDATE SET
                    ShiftId = excluded.ShiftId,
                    CheckIn = excluded.CheckIn,
                    CheckOut = excluded.CheckOut,
                    WorkedMinutes = excluded.WorkedMinutes,
                    LateMinutes = excluded.LateMinutes,
                    EarlyLeaveMinutes = excluded.EarlyLeaveMinutes,
                    OvertimeMinutes = excluded.OvertimeMinutes,
                    Status = excluded.Status,
                    Remarks = excluded.Remarks,
                    UpdatedAt = CURRENT_TIMESTAMP;
                SELECT last_insert_rowid();"

            Return DbHelper.ExecuteScalar(Of Long)(sql,
                DbHelper.CreateParam("@eid", rec.EmployeeId),
                DbHelper.CreateParam("@sid", rec.ShiftId),
                DbHelper.CreateParam("@dt", rec.AttendanceDate.ToString("yyyy-MM-dd")),
                DbHelper.CreateParam("@cin", If(rec.CheckIn.HasValue, rec.CheckIn.Value.ToString("yyyy-MM-dd HH:mm:ss"), DBNull.Value)),
                DbHelper.CreateParam("@cout", If(rec.CheckOut.HasValue, rec.CheckOut.Value.ToString("yyyy-MM-dd HH:mm:ss"), DBNull.Value)),
                DbHelper.CreateParam("@wm", rec.WorkedMinutes),
                DbHelper.CreateParam("@lm", rec.LateMinutes),
                DbHelper.CreateParam("@elm", rec.EarlyLeaveMinutes),
                DbHelper.CreateParam("@ot", rec.OvertimeMinutes),
                DbHelper.CreateParam("@st", CInt(rec.Status)),
                DbHelper.CreateParam("@rem", rec.Remarks))
        End Function

        Public Function Delete(id As Integer) As Boolean
            Return DbHelper.ExecuteNonQuery("DELETE FROM Attendance WHERE Id = @id", DbHelper.CreateParam("@id", id)) > 0
        End Function

        Private Function MapRow(row As DataRow) As AttendanceRecord
            Return New AttendanceRecord With {
                .Id = Convert.ToInt32(row("Id")),
                .EmployeeId = Convert.ToInt32(row("EmployeeId")),
                .EmployeeCode = row("EmployeeCode").ToString(),
                .EmployeeName = row("EmployeeName").ToString(),
                .DepartmentName = row("DepartmentName").ToString(),
                .ShiftId = Convert.ToInt32(row("ShiftId")),
                .ShiftName = row("ShiftName").ToString(),
                .AttendanceDate = DateTime.Parse(row("AttendanceDate").ToString()),
                .CheckIn = If(Convert.IsDBNull(row("CheckIn")) OrElse String.IsNullOrEmpty(row("CheckIn").ToString()), CType(Nothing, Nullable(Of DateTime)), DateTime.Parse(row("CheckIn").ToString())),
                .CheckOut = If(Convert.IsDBNull(row("CheckOut")) OrElse String.IsNullOrEmpty(row("CheckOut").ToString()), CType(Nothing, Nullable(Of DateTime)), DateTime.Parse(row("CheckOut").ToString())),
                .WorkedMinutes = Convert.ToInt32(row("WorkedMinutes")),
                .LateMinutes = Convert.ToInt32(row("LateMinutes")),
                .EarlyLeaveMinutes = Convert.ToInt32(row("EarlyLeaveMinutes")),
                .OvertimeMinutes = Convert.ToInt32(row("OvertimeMinutes")),
                .Status = CType(Convert.ToInt32(row("Status")), AttendanceStatus),
                .Remarks = If(Convert.IsDBNull(row("Remarks")), "", row("Remarks").ToString()),
                .CreatedAt = Convert.ToDateTime(row("CreatedAt")),
                .UpdatedAt = Convert.ToDateTime(row("UpdatedAt"))
            }
        End Function
    End Class

    Public Class LeaveRepository
        Public Function GetLeaveTypes() As List(Of LeaveType)
            Dim list As New List(Of LeaveType)
            Dim dt = DbHelper.ExecuteDataTable("SELECT * FROM LeaveTypes WHERE IsActive = 1 ORDER BY Name")
            For Each row As DataRow In dt.Rows
                list.Add(New LeaveType With {
                    .Id = Convert.ToInt32(row("Id")),
                    .Name = row("Name").ToString(),
                    .DefaultAllowanceDays = Convert.ToInt32(row("DefaultAllowanceDays")),
                    .IsPaid = Convert.ToInt32(row("IsPaid")) = 1,
                    .IsActive = Convert.ToInt32(row("IsActive")) = 1
                })
            Next
            Return list
        End Function

        Public Function GetRequests(Optional status As Nullable(Of LeaveStatus) = Nothing, Optional employeeId As Integer = 0) As List(Of LeaveRequest)
            Dim list As New List(Of LeaveRequest)
            Dim sql = "
                SELECT lr.*, e.EmployeeCode, e.FullName AS EmployeeName, d.Name AS DepartmentName, lt.Name AS LeaveTypeName, u.FullName AS ApproverName
                FROM LeaveRequests lr
                JOIN Employees e ON lr.EmployeeId = e.Id
                JOIN Departments d ON e.DepartmentId = d.Id
                JOIN LeaveTypes lt ON lr.LeaveTypeId = lt.Id
                LEFT JOIN Users u ON lr.ApprovedByUserId = u.Id
                WHERE 1=1 "

            Dim params As New List(Of Microsoft.Data.Sqlite.SqliteParameter)
            If status.HasValue Then
                sql &= " AND lr.Status = @st "
                params.Add(DbHelper.CreateParam("@st", CInt(status.Value)))
            End If
            If employeeId > 0 Then
                sql &= " AND lr.EmployeeId = @eid "
                params.Add(DbHelper.CreateParam("@eid", employeeId))
            End If

            sql &= " ORDER BY lr.CreatedAt DESC"

            Dim dt = DbHelper.ExecuteDataTable(sql, params.ToArray())
            For Each row As DataRow In dt.Rows
                list.Add(MapRequestRow(row))
            Next
            Return list
        End Function

        Public Function InsertRequest(req As LeaveRequest) As Integer
            Return DbHelper.ExecuteScalar(Of Long)("
                INSERT INTO LeaveRequests (EmployeeId, LeaveTypeId, StartDate, EndDate, DaysCount, Reason, Status, ActionNotes)
                VALUES (@eid, @ltid, @st, @ed, @dc, @rsn, @status, @an);
                SELECT last_insert_rowid();",
                DbHelper.CreateParam("@eid", req.EmployeeId),
                DbHelper.CreateParam("@ltid", req.LeaveTypeId),
                DbHelper.CreateParam("@st", req.StartDate.ToString("yyyy-MM-dd")),
                DbHelper.CreateParam("@ed", req.EndDate.ToString("yyyy-MM-dd")),
                DbHelper.CreateParam("@dc", req.DaysCount),
                DbHelper.CreateParam("@rsn", req.Reason),
                DbHelper.CreateParam("@status", CInt(req.Status)),
                DbHelper.CreateParam("@an", req.ActionNotes))
        End Function

        Public Function UpdateStatus(requestId As Integer, status As LeaveStatus, approvedByUserId As Integer, notes As String) As Boolean
            Return DbHelper.ExecuteNonQuery("
                UPDATE LeaveRequests SET Status = @st, ApprovedByUserId = @ap, ActionNotes = @an, UpdatedAt = CURRENT_TIMESTAMP
                WHERE Id = @id",
                DbHelper.CreateParam("@st", CInt(status)),
                DbHelper.CreateParam("@ap", approvedByUserId),
                DbHelper.CreateParam("@an", notes),
                DbHelper.CreateParam("@id", requestId)) > 0
        End Function

        Public Function GetUsedDays(employeeId As Integer, leaveTypeId As Integer, year As Integer) As Integer
            Dim sql = "
                SELECT COALESCE(SUM(DaysCount), 0)
                FROM LeaveRequests
                WHERE EmployeeId = @eid AND LeaveTypeId = @ltid AND Status = 2
                  AND strftime('%Y', StartDate) = @yr"
            Return Convert.ToInt32(DbHelper.ExecuteScalar(Of Long)(sql,
                DbHelper.CreateParam("@eid", employeeId),
                DbHelper.CreateParam("@ltid", leaveTypeId),
                DbHelper.CreateParam("@yr", year.ToString())))
        End Function

        Private Function MapRequestRow(row As DataRow) As LeaveRequest
            Return New LeaveRequest With {
                .Id = Convert.ToInt32(row("Id")),
                .EmployeeId = Convert.ToInt32(row("EmployeeId")),
                .EmployeeCode = row("EmployeeCode").ToString(),
                .EmployeeName = row("EmployeeName").ToString(),
                .DepartmentName = row("DepartmentName").ToString(),
                .LeaveTypeId = Convert.ToInt32(row("LeaveTypeId")),
                .LeaveTypeName = row("LeaveTypeName").ToString(),
                .StartDate = DateTime.Parse(row("StartDate").ToString()),
                .EndDate = DateTime.Parse(row("EndDate").ToString()),
                .DaysCount = Convert.ToInt32(row("DaysCount")),
                .Reason = row("Reason").ToString(),
                .Status = CType(Convert.ToInt32(row("Status")), LeaveStatus),
                .ApprovedByUserId = If(Convert.IsDBNull(row("ApprovedByUserId")), CType(Nothing, Nullable(Of Integer)), Convert.ToInt32(row("ApprovedByUserId"))),
                .ApproverName = If(Convert.IsDBNull(row("ApproverName")), "", row("ApproverName").ToString()),
                .ActionNotes = If(Convert.IsDBNull(row("ActionNotes")), "", row("ActionNotes").ToString()),
                .CreatedAt = Convert.ToDateTime(row("CreatedAt")),
                .UpdatedAt = Convert.ToDateTime(row("UpdatedAt"))
            }
        End Function
    End Class

    Public Class HolidayRepository
        Public Function GetAll(Optional year As Nullable(Of Integer) = Nothing) As List(Of Holiday)
            Dim list As New List(Of Holiday)
            Dim sql = "SELECT * FROM Holidays"
            Dim params As New List(Of Microsoft.Data.Sqlite.SqliteParameter)

            If year.HasValue Then
                sql &= " WHERE strftime('%Y', HolidayDate) = @yr OR IsRecurring = 1"
                params.Add(DbHelper.CreateParam("@yr", year.Value.ToString()))
            End If

            sql &= " ORDER BY HolidayDate ASC"

            Dim dt = DbHelper.ExecuteDataTable(sql, params.ToArray())
            For Each row As DataRow In dt.Rows
                list.Add(New Holiday With {
                    .Id = Convert.ToInt32(row("Id")),
                    .Name = row("Name").ToString(),
                    .HolidayDate = DateTime.Parse(row("HolidayDate").ToString()),
                    .IsRecurring = Convert.ToInt32(row("IsRecurring")) = 1,
                    .Description = If(Convert.IsDBNull(row("Description")), "", row("Description").ToString()),
                    .CreatedAt = Convert.ToDateTime(row("CreatedAt"))
                })
            Next
            Return list
        End Function

        Public Function Insert(h As Holiday) As Integer
            Return DbHelper.ExecuteScalar(Of Long)("
                INSERT INTO Holidays (Name, HolidayDate, IsRecurring, Description)
                VALUES (@n, @hd, @ir, @desc);
                SELECT last_insert_rowid();",
                DbHelper.CreateParam("@n", h.Name),
                DbHelper.CreateParam("@hd", h.HolidayDate.ToString("yyyy-MM-dd")),
                DbHelper.CreateParam("@ir", If(h.IsRecurring, 1, 0)),
                DbHelper.CreateParam("@desc", h.Description))
        End Function

        Public Function Delete(id As Integer) As Boolean
            Return DbHelper.ExecuteNonQuery("DELETE FROM Holidays WHERE Id = @id", DbHelper.CreateParam("@id", id)) > 0
        End Function

        Public Function IsHoliday(dt As DateTime) As Boolean
            Dim dtStr = dt.ToString("yyyy-MM-dd")
            Dim monthDay = dt.ToString("-MM-dd")
            Dim count = DbHelper.ExecuteScalar(Of Long)("
                SELECT COUNT(*) FROM Holidays 
                WHERE HolidayDate = @dt OR (IsRecurring = 1 AND HolidayDate LIKE '%' || @md)",
                DbHelper.CreateParam("@dt", dtStr),
                DbHelper.CreateParam("@md", monthDay))
            Return count > 0
        End Function
    End Class

    Public Class SettingRepository
        Public Function GetAll() As Dictionary(Of String, String)
            Dim dict As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            Dim dt = DbHelper.ExecuteDataTable("SELECT KeyName, Value FROM Settings")
            For Each row As DataRow In dt.Rows
                dict(row("KeyName").ToString()) = row("Value").ToString()
            Next
            Return dict
        End Function

        Public Function GetValue(key As String, defaultValue As String) As String
            Dim val = DbHelper.ExecuteScalar(Of String)("SELECT Value FROM Settings WHERE KeyName = @k",
                                                       DbHelper.CreateParam("@k", key))
            If String.IsNullOrEmpty(val) Then Return defaultValue
            Return val
        End Function

        Public Sub SaveSetting(key As String, value As String, Optional desc As String = "", Optional cat As String = "General")
            DbHelper.ExecuteNonQuery("
                INSERT INTO Settings (KeyName, Value, Description, Category, UpdatedAt)
                VALUES (@k, @v, @d, @c, CURRENT_TIMESTAMP)
                ON CONFLICT(KeyName) DO UPDATE SET Value = excluded.Value, Description = excluded.Description, Category = excluded.Category, UpdatedAt = CURRENT_TIMESTAMP",
                DbHelper.CreateParam("@k", key),
                DbHelper.CreateParam("@v", value),
                DbHelper.CreateParam("@d", desc),
                DbHelper.CreateParam("@c", cat))
        End Sub
    End Class

    Public Class AuditRepository
        Public Sub Log(action As String, moduleName As String, details As String)
            Dim userId As Nullable(Of Integer) = If(SessionManager.CurrentUser IsNot Nothing, SessionManager.CurrentUser.Id, CType(Nothing, Nullable(Of Integer)))
            Dim username As String = If(SessionManager.CurrentUser IsNot Nothing, SessionManager.CurrentUser.Username, "SYSTEM")

            DbHelper.ExecuteNonQuery("
                INSERT INTO AuditLogs (UserId, Username, Action, ModuleName, Details, Timestamp)
                VALUES (@uid, @un, @act, @mod, @det, CURRENT_TIMESTAMP)",
                DbHelper.CreateParam("@uid", If(userId.HasValue, userId.Value, DBNull.Value)),
                DbHelper.CreateParam("@un", username),
                DbHelper.CreateParam("@act", action),
                DbHelper.CreateParam("@mod", moduleName),
                DbHelper.CreateParam("@det", details))
        End Sub

        Public Function GetRecent(limit As Integer) As List(Of AuditLog)
            Dim list As New List(Of AuditLog)
            Dim dt = DbHelper.ExecuteDataTable($"SELECT * FROM AuditLogs ORDER BY Timestamp DESC LIMIT {limit}")
            For Each row As DataRow In dt.Rows
                list.Add(New AuditLog With {
                    .Id = Convert.ToInt32(row("Id")),
                    .UserId = If(Convert.IsDBNull(row("UserId")), CType(Nothing, Nullable(Of Integer)), Convert.ToInt32(row("UserId"))),
                    .Username = If(Convert.IsDBNull(row("Username")), "SYSTEM", row("Username").ToString()),
                    .Action = row("Action").ToString(),
                    .ModuleName = row("ModuleName").ToString(),
                    .Details = If(Convert.IsDBNull(row("Details")), "", row("Details").ToString()),
                    .Timestamp = Convert.ToDateTime(row("Timestamp"))
                })
            Next
            Return list
        End Function
    End Class
End Namespace
