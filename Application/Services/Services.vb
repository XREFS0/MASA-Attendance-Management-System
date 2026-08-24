Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Linq
Imports AttendanceSystem.Common.Utilities
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Domain.Enums
Imports AttendanceSystem.Infrastructure.Repositories
Imports AttendanceSystem.Infrastructure.Security
Imports AttendanceSystem.Application.DTOs
Imports AttendanceSystem.Infrastructure.Database

Namespace Application.Services
    Public Class AuthService
        Private ReadOnly _userRepo As New UserRepository()
        Private ReadOnly _auditRepo As New AuditRepository()

        Public Function Login(username As String, password As String) As (Success As Boolean, Message As String, User As User)
            If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
                Return (False, "Please enter both username and password.", Nothing)
            End If

            Dim user = _userRepo.GetByUsername(username.Trim())
            If user Is Nothing Then
                Return (False, "Invalid username or password.", Nothing)
            End If

            If Not user.IsActive Then
                Return (False, "This account is inactive. Please contact the administrator.", Nothing)
            End If

            If Not PasswordHasher.VerifyPassword(password, user.PasswordHash) Then
                Return (False, "Invalid username or password.", Nothing)
            End If

            _userRepo.UpdateLastLogin(user.Id)
            SessionManager.CurrentUser = user
            _auditRepo.Log("LOGIN", "Auth", $"User '{user.Username}' logged in successfully.")

            Return (True, "Login successful.", user)
        End Function

        Public Function ChangePassword(userId As Integer, currentPassword As String, newPassword As String) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(newPassword) OrElse newPassword.Length < 6 Then
                Return (False, "New password must be at least 6 characters long.")
            End If

            Dim user = _userRepo.GetById(userId)
            If user Is Nothing Then
                Return (False, "User not found.")
            End If

            If Not PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash) Then
                Return (False, "Incorrect current password.")
            End If

            Dim newHash = PasswordHasher.HashPassword(newPassword)
            If _userRepo.UpdatePassword(userId, newHash) Then
                _auditRepo.Log("CHANGE_PASSWORD", "Auth", $"User '{user.Username}' updated their password.")
                Return (True, "Password changed successfully.")
            Else
                Return (False, "Failed to update password. Please try again.")
            End If
        End Function
    End Class

    Public Class AttendanceCalculationService
        Private ReadOnly _shiftRepo As New ShiftRepository()
        Private ReadOnly _holidayRepo As New HolidayRepository()

        Public Sub CalculateAttendanceMetrics(rec As AttendanceRecord, shift As Shift)
            If shift Is Nothing Then
                shift = _shiftRepo.GetById(rec.ShiftId)
            End If

            If shift Is Nothing Then
                rec.WorkedMinutes = 0
                rec.LateMinutes = 0
                rec.EarlyLeaveMinutes = 0
                rec.OvertimeMinutes = 0
                rec.Status = AttendanceStatus.Present
                Return
            End If

            If Not rec.CheckIn.HasValue Then
                If _holidayRepo.IsHoliday(rec.AttendanceDate) Then
                    rec.Status = AttendanceStatus.Holiday
                Else
                    rec.Status = AttendanceStatus.Absent
                End If
                rec.WorkedMinutes = 0
                rec.LateMinutes = 0
                rec.EarlyLeaveMinutes = 0
                rec.OvertimeMinutes = 0
                Return
            End If

            Dim checkInTime = rec.CheckIn.Value.TimeOfDay
            Dim shiftStart = shift.StartTime
            Dim graceThreshold = shiftStart.Add(TimeSpan.FromMinutes(shift.GracePeriodMinutes))

            If checkInTime > graceThreshold Then
                rec.LateMinutes = Math.Max(0, Convert.ToInt32((checkInTime - shiftStart).TotalMinutes))
            Else
                rec.LateMinutes = 0
            End If

            If rec.CheckOut.HasValue Then
                Dim checkOutTime = rec.CheckOut.Value.TimeOfDay
                Dim shiftEnd = shift.EndTime

                Dim totalDuration = (rec.CheckOut.Value - rec.CheckIn.Value).TotalMinutes
                If totalDuration < 0 Then totalDuration = 0

                Dim netWorkedMinutes = totalDuration
                If netWorkedMinutes >= 240 AndAlso shift.BreakDurationMinutes > 0 Then
                    netWorkedMinutes -= shift.BreakDurationMinutes
                End If
                rec.WorkedMinutes = Math.Max(0, Convert.ToInt32(netWorkedMinutes))

                If checkOutTime < shiftEnd Then
                    rec.EarlyLeaveMinutes = Math.Max(0, Convert.ToInt32((shiftEnd - checkOutTime).TotalMinutes))
                Else
                    rec.EarlyLeaveMinutes = 0
                End If

                If checkOutTime > shiftEnd Then
                    rec.OvertimeMinutes = Math.Max(0, Convert.ToInt32((checkOutTime - shiftEnd).TotalMinutes))
                Else
                    rec.OvertimeMinutes = 0
                End If

                Dim workedHours = rec.WorkedMinutes / 60.0
                If workedHours < shift.HalfDayThresholdHours Then
                    rec.Status = AttendanceStatus.HalfDay
                ElseIf rec.LateMinutes > 0 Then
                    rec.Status = AttendanceStatus.Late
                Else
                    rec.Status = AttendanceStatus.Present
                End If
            Else
                rec.WorkedMinutes = 0
                rec.EarlyLeaveMinutes = 0
                rec.OvertimeMinutes = 0
                If rec.LateMinutes > 0 Then
                    rec.Status = AttendanceStatus.Late
                Else
                    rec.Status = AttendanceStatus.Present
                End If
            End If
        End Sub
    End Class

    Public Class AttendanceService
        Private ReadOnly _attendanceRepo As New AttendanceRepository()
        Private ReadOnly _employeeRepo As New EmployeeRepository()
        Private ReadOnly _shiftRepo As New ShiftRepository()
        Private ReadOnly _holidayRepo As New HolidayRepository()
        Private ReadOnly _calcService As New AttendanceCalculationService()
        Private ReadOnly _auditRepo As New AuditRepository()

        Public Function GetAttendance(startDate As DateTime, endDate As DateTime, Optional departmentId As Integer = 0, Optional employeeId As Integer = 0, Optional status As Nullable(Of AttendanceStatus) = Nothing) As List(Of AttendanceRecord)
            Return _attendanceRepo.GetRecords(startDate, endDate, departmentId, employeeId, status)
        End Function

        Public Function CheckIn(employeeCode As String, Optional checkInTime As Nullable(Of DateTime) = Nothing) As (Success As Boolean, Message As String, Record As AttendanceRecord)
            Dim emp = _employeeRepo.GetByCode(employeeCode)
            If emp Is Nothing Then
                Return (False, $"Employee with code '{employeeCode}' was not found.", Nothing)
            End If

            If emp.Status <> EmployeeStatus.Active Then
                Return (False, $"Employee '{emp.FullName}' is not active.", Nothing)
            End If

            Dim nowTime = If(checkInTime.HasValue, checkInTime.Value, DateTime.Now)
            Dim today = nowTime.Date

            Dim existing = _attendanceRepo.GetTodayRecord(emp.Id, today)
            If existing IsNot Nothing AndAlso existing.CheckIn.HasValue Then
                Return (False, $"Employee '{emp.FullName}' has already checked in today at {existing.CheckIn.Value:HH:mm:ss}.", existing)
            End If

            Dim shift = _shiftRepo.GetById(emp.ShiftId)
            Dim record = If(existing, New AttendanceRecord With {
                .EmployeeId = emp.Id,
                .ShiftId = emp.ShiftId,
                .AttendanceDate = today
            })

            record.ShiftId = emp.ShiftId
            record.CheckIn = nowTime
            _calcService.CalculateAttendanceMetrics(record, shift)
            record.Remarks = "Terminal Check-In"

            _attendanceRepo.Upsert(record)
            _auditRepo.Log("CHECK_IN", "Attendance", $"Check-in recorded for '{emp.FullName}' ({emp.EmployeeCode}) at {nowTime:HH:mm:ss}.")

            Return (True, $"Check-in recorded successfully for {emp.FullName}.", record)
        End Function

        Public Function CheckOut(employeeCode As String, Optional checkOutTime As Nullable(Of DateTime) = Nothing) As (Success As Boolean, Message As String, Record As AttendanceRecord)
            Dim emp = _employeeRepo.GetByCode(employeeCode)
            If emp Is Nothing Then
                Return (False, $"Employee with code '{employeeCode}' was not found.", Nothing)
            End If

            Dim nowTime = If(checkOutTime.HasValue, checkOutTime.Value, DateTime.Now)
            Dim today = nowTime.Date

            Dim record = _attendanceRepo.GetTodayRecord(emp.Id, today)
            If record Is Nothing OrElse Not record.CheckIn.HasValue Then
                Return (False, $"Cannot check out. No check-in record found for '{emp.FullName}' today.", Nothing)
            End If

            If record.CheckOut.HasValue Then
                Return (False, $"Employee '{emp.FullName}' has already checked out today at {record.CheckOut.Value:HH:mm:ss}.", record)
            End If

            Dim shift = _shiftRepo.GetById(record.ShiftId)
            record.CheckOut = nowTime
            _calcService.CalculateAttendanceMetrics(record, shift)

            _attendanceRepo.Upsert(record)
            _auditRepo.Log("CHECK_OUT", "Attendance", $"Check-out recorded for '{emp.FullName}' ({emp.EmployeeCode}) at {nowTime:HH:mm:ss}. Worked: {record.WorkedHoursFormatted}.")

            Return (True, $"Check-out recorded successfully for {emp.FullName}. Worked: {record.WorkedHoursFormatted}", record)
        End Function

        Public Function SaveManualRecord(rec As AttendanceRecord) As (Success As Boolean, Message As String)
            Dim emp = _employeeRepo.GetById(rec.EmployeeId)
            If emp Is Nothing Then Return (False, "Selected employee does not exist.")

            Dim shift = _shiftRepo.GetById(rec.ShiftId)
            If shift Is Nothing Then shift = _shiftRepo.GetById(emp.ShiftId)

            _calcService.CalculateAttendanceMetrics(rec, shift)
            _attendanceRepo.Upsert(rec)
            _auditRepo.Log("MANUAL_ATTENDANCE", "Attendance", $"Manual attendance updated for '{emp.FullName}' on {rec.AttendanceDate:yyyy-MM-dd}.")

            Return (True, "Attendance record saved successfully.")
        End Function

        Public Function DeleteRecord(id As Integer) As Boolean
            Dim rec = _attendanceRepo.GetById(id)
            If rec IsNot Nothing Then
                _auditRepo.Log("DELETE_ATTENDANCE", "Attendance", $"Deleted attendance record ID {id} for date {rec.AttendanceDate:yyyy-MM-dd}.")
            End If
            Return _attendanceRepo.Delete(id)
        End Function
    End Class

    Public Class EmployeeService
        Private ReadOnly _empRepo As New EmployeeRepository()
        Private ReadOnly _auditRepo As New AuditRepository()

        Public Function GetAll(Optional search As String = "", Optional deptId As Integer = 0, Optional status As Nullable(Of EmployeeStatus) = Nothing) As List(Of Employee)
            Return _empRepo.GetAll(search, deptId, status)
        End Function

        Public Function GetById(id As Integer) As Employee
            Return _empRepo.GetById(id)
        End Function

        Public Function SaveEmployee(emp As Employee) As (Success As Boolean, Message As String)
            If String.IsNullOrWhiteSpace(emp.EmployeeCode) Then
                Return (False, "Employee code is required.")
            End If
            If String.IsNullOrWhiteSpace(emp.FullName) Then
                Return (False, "Employee full name is required.")
            End If
            If emp.DepartmentId <= 0 Then
                Return (False, "Please select a valid department.")
            End If
            If emp.PositionId <= 0 Then
                Return (False, "Please select a valid position.")
            End If
            If emp.ShiftId <= 0 Then
                Return (False, "Please select an assigned shift.")
            End If
            If Not ValidationHelper.IsValidEmail(emp.Email) Then
                Return (False, "Please enter a valid email address.")
            End If

            If emp.Id = 0 Then
                If _empRepo.ExistsCode(emp.EmployeeCode) Then
                    Return (False, $"Employee code '{emp.EmployeeCode}' is already in use.")
                End If
                Dim newId = _empRepo.Insert(emp)
                _auditRepo.Log("CREATE_EMPLOYEE", "Employees", $"Created employee '{emp.FullName}' ({emp.EmployeeCode}).")
                Return (True, "Employee created successfully.")
            Else
                If _empRepo.ExistsCode(emp.EmployeeCode, emp.Id) Then
                    Return (False, $"Employee code '{emp.EmployeeCode}' is already in use by another employee.")
                End If
                _empRepo.Update(emp)
                _auditRepo.Log("UPDATE_EMPLOYEE", "Employees", $"Updated employee '{emp.FullName}' ({emp.EmployeeCode}).")
                Return (True, "Employee updated successfully.")
            End If
        End Function

        Public Function DeleteEmployee(id As Integer) As (Success As Boolean, Message As String)
            Dim emp = _empRepo.GetById(id)
            If emp Is Nothing Then Return (False, "Employee not found.")

            Try
                If _empRepo.Delete(id) Then
                    _auditRepo.Log("DELETE_EMPLOYEE", "Employees", $"Deleted employee '{emp.FullName}' ({emp.EmployeeCode}).")
                    Return (True, "Employee deleted successfully.")
                Else
                    Return (False, "Failed to delete employee.")
                End If
            Catch ex As Exception
                Return (False, "Cannot delete employee because associated attendance or leave records exist. Please deactivate the employee instead.")
            End Try
        End Function
    End Class

    Public Class DashboardService
        Private ReadOnly _empRepo As New EmployeeRepository()
        Private ReadOnly _attendanceRepo As New AttendanceRepository()
        Private ReadOnly _leaveRepo As New LeaveRepository()

        Public Function GetDashboardStats() As DashboardStatsDto
            Dim today = DateTime.Today
            Dim totalActive = _empRepo.GetTotalCount()
            Dim todayRecords = _attendanceRepo.GetRecords(today, today)

            Dim presentCount = Enumerable.Count(todayRecords, Function(r) r.Status = AttendanceStatus.Present)
            Dim lateCount = Enumerable.Count(todayRecords, Function(r) r.Status = AttendanceStatus.Late)
            Dim halfDayCount = Enumerable.Count(todayRecords, Function(r) r.Status = AttendanceStatus.HalfDay)
            Dim leaveCount = Enumerable.Count(todayRecords, Function(r) r.Status = AttendanceStatus.OnLeave)

            Dim approvedLeaves = _leaveRepo.GetRequests(LeaveStatus.Approved)
            Dim activeLeaves = Enumerable.Count(approvedLeaves, Function(l) today >= l.StartDate.Date AndAlso today <= l.EndDate.Date)
            If activeLeaves > leaveCount Then leaveCount = activeLeaves

            Dim attendedTotal = presentCount + lateCount + halfDayCount
            Dim absentCount = Math.Max(0, totalActive - attendedTotal - leaveCount)

            Dim rate As Double = 0.0
            If totalActive > 0 Then
                rate = Math.Round((attendedTotal / CDbl(totalActive)) * 100.0, 1)
            End If

            Return New DashboardStatsDto With {
                .TotalEmployees = totalActive,
                .PresentToday = presentCount + halfDayCount,
                .LateToday = lateCount,
                .AbsentToday = absentCount,
                .OnLeaveToday = leaveCount,
                .AttendanceRate = rate
            }
        End Function
    End Class

    Public Class LeaveService
        Private ReadOnly _leaveRepo As New LeaveRepository()
        Private ReadOnly _empRepo As New EmployeeRepository()
        Private ReadOnly _auditRepo As New AuditRepository()

        Public Function GetRequests(Optional status As Nullable(Of LeaveStatus) = Nothing, Optional empId As Integer = 0) As List(Of LeaveRequest)
            Return _leaveRepo.GetRequests(status, empId)
        End Function

        Public Function GetLeaveTypes() As List(Of LeaveType)
            Return _leaveRepo.GetLeaveTypes()
        End Function

        Public Function SubmitLeave(req As LeaveRequest) As (Success As Boolean, Message As String)
            If req.EmployeeId <= 0 Then Return (False, "Please select an employee.")
            If req.LeaveTypeId <= 0 Then Return (False, "Please select a leave type.")
            If req.EndDate < req.StartDate Then Return (False, "End date cannot be earlier than start date.")
            If String.IsNullOrWhiteSpace(req.Reason) Then Return (False, "Please provide a reason for leave.")

            req.DaysCount = Convert.ToInt32((req.EndDate.Date - req.StartDate.Date).TotalDays) + 1
            req.Status = LeaveStatus.Pending

            _leaveRepo.InsertRequest(req)
            _auditRepo.Log("LEAVE_REQUEST", "Leaves", $"Leave request submitted for Employee ID {req.EmployeeId} ({req.DaysCount} days).")
            Return (True, "Leave request submitted successfully.")
        End Function

        Public Function ProcessRequest(requestId As Integer, approve As Boolean, notes As String) As (Success As Boolean, Message As String)
            Dim currentUserId = If(SessionManager.CurrentUser IsNot Nothing, SessionManager.CurrentUser.Id, 1)
            Dim newStatus = If(approve, LeaveStatus.Approved, LeaveStatus.Rejected)

            If _leaveRepo.UpdateStatus(requestId, newStatus, currentUserId, notes) Then
                Dim actionStr = If(approve, "Approved", "Rejected")
                _auditRepo.Log("LEAVE_PROCESS", "Leaves", $"Leave request #{requestId} was {actionStr} by user ID {currentUserId}.")
                Return (True, $"Leave request was {actionStr.ToLower()} successfully.")
            Else
                Return (False, "Failed to update leave request status.")
            End If
        End Function
    End Class

    Public Class BackupService
        Private ReadOnly _auditRepo As New AuditRepository()

        Public Function BackupDatabase(destinationPath As String) As (Success As Boolean, Message As String)
            Try
                If File.Exists(destinationPath) Then
                    File.Delete(destinationPath)
                End If

                Dim sql = $"VACUUM INTO '{destinationPath.Replace("'", "''")}';"
                DbHelper.ExecuteNonQuery(sql)

                _auditRepo.Log("DB_BACKUP", "System", $"Database backup created at {destinationPath}.")
                Return (True, "Database backup created successfully.")
            Catch ex As Exception
                Return (False, "Backup failed: " & ex.Message)
            End Try
        End Function

        Public Function RestoreDatabase(sourcePath As String) As (Success As Boolean, Message As String)
            If Not File.Exists(sourcePath) Then
                Return (False, "Selected backup file does not exist.")
            End If

            Try
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools()

                Dim safetyPath = DatabaseContext.DbFilePath & ".safety_backup"
                If File.Exists(DatabaseContext.DbFilePath) Then
                    File.Copy(DatabaseContext.DbFilePath, safetyPath, True)
                End If

                File.Copy(sourcePath, DatabaseContext.DbFilePath, True)
                _auditRepo.Log("DB_RESTORE", "System", $"Database restored from {sourcePath}.")
                Return (True, "Database restored successfully. Changes are now active.")
            Catch ex As Exception
                Return (False, "Restore failed: " & ex.Message)
            End Try
        End Function
    End Class
End Namespace
