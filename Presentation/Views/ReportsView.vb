Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Domain.Enums
Imports AttendanceSystem.Infrastructure.Repositories
Imports AttendanceSystem.Application.Services
Imports AttendanceSystem.Common.Utilities

Namespace Presentation.Views
    Public Class ReportsView
        Inherits UserControl

        Private cmbReportType As ComboBox
        Private dtpFrom As DateTimePicker
        Private dtpTo As DateTimePicker
        Private cmbDepartment As ComboBox
        Private btnGenerate As Button
        Private btnExport As Button
        Private dgvReport As DataGridView
        Private lblSummaryStats As Label

        Private ReadOnly _attendanceService As New AttendanceService()
        Private ReadOnly _deptRepo As New DepartmentRepository()
        Private ReadOnly _empRepo As New EmployeeRepository()
        Private ReadOnly _leaveRepo As New LeaveRepository()

        Public Sub New()
            InitializeComponent()
            LoadFilters()
            GenerateReport()
        End Sub

        Private Sub InitializeComponent()
            Me.Dock = DockStyle.Fill
            Me.BackColor = UITheme.BackgroundColor
            Me.Font = UITheme.FontBody
            Me.Padding = New Padding(16)

            Dim pnlTop As New Panel With {.Dock = DockStyle.Top, .Height = 44, .BackColor = Color.Transparent}
            Dim lblTitle As New Label With {.Text = "Attendance & Compliance Reports", .Font = UITheme.FontPageTitle, .ForeColor = UITheme.TextPrimary, .Dock = DockStyle.Left, .AutoSize = True, .TextAlign = ContentAlignment.MiddleLeft}

            btnExport = New Button With {.Text = "Export to CSV", .Dock = DockStyle.Right, .Width = 120}
            UITheme.ApplyButtonStyle(btnExport, False)
            AddHandler btnExport.Click, Sub() CsvExporter.ExportToCsv(dgvReport, "Attendance_Report")

            pnlTop.Controls.AddRange(New Control() {btnExport, lblTitle})

            Dim pnlFilter As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 52,
                .BackColor = UITheme.PanelBackground,
                .BorderStyle = BorderStyle.FixedSingle,
                .Padding = New Padding(10, 10, 10, 10),
                .Margin = New Padding(0, 8, 0, 8)
            }

            Dim lblType As New Label With {.Text = "Report:", .Location = New Point(10, 15), .AutoSize = True, .Font = UITheme.FontBodyBold}
            cmbReportType = New ComboBox With {.Location = New Point(65, 12), .Width = 200, .DropDownStyle = ComboBoxStyle.DropDownList}
            cmbReportType.Items.AddRange(New Object() {
                "Daily Attendance Summary",
                "Late & Tardy Records",
                "Overtime Hours Analysis",
                "Absence & Non-Attendance",
                "Employee Monthly Aggregates"
            })
            cmbReportType.SelectedIndex = 0

            Dim lblFrom As New Label With {.Text = "From:", .Location = New Point(280, 15), .AutoSize = True, .Font = UITheme.FontBodyBold}
            dtpFrom = New DateTimePicker With {.Location = New Point(325, 12), .Width = 110, .Format = DateTimePickerFormat.Short, .Value = DateTime.Today.AddDays(-30)}

            Dim lblTo As New Label With {.Text = "To:", .Location = New Point(445, 15), .AutoSize = True, .Font = UITheme.FontBodyBold}
            dtpTo = New DateTimePicker With {.Location = New Point(475, 12), .Width = 110, .Format = DateTimePickerFormat.Short, .Value = DateTime.Today}

            Dim lblDept As New Label With {.Text = "Department:", .Location = New Point(595, 15), .AutoSize = True, .Font = UITheme.FontBodyBold}
            cmbDepartment = New ComboBox With {.Location = New Point(680, 12), .Width = 160, .DropDownStyle = ComboBoxStyle.DropDownList}

            btnGenerate = New Button With {.Text = "Generate", .Location = New Point(855, 10), .Width = 85, .Height = 28}
            UITheme.ApplyButtonStyle(btnGenerate, True)
            AddHandler btnGenerate.Click, Sub() GenerateReport()

            pnlFilter.Controls.AddRange(New Control() {lblType, cmbReportType, lblFrom, dtpFrom, lblTo, dtpTo, lblDept, cmbDepartment, btnGenerate})

            Dim pnlSummary As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 32,
                .BackColor = UITheme.PrimaryLight,
                .Padding = New Padding(12, 6, 12, 6),
                .Margin = New Padding(0, 4, 0, 8)
            }
            lblSummaryStats = New Label With {.Dock = DockStyle.Fill, .Font = UITheme.FontBodyBold, .ForeColor = UITheme.PrimaryDark, .Text = "Summary: Ready"}
            pnlSummary.Controls.Add(lblSummaryStats)

            Dim pnlGrid As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(0, 8, 0, 0)}
            dgvReport = New DataGridView With {.Dock = DockStyle.Fill}
            UITheme.ApplyGridStyle(dgvReport)

            pnlGrid.Controls.Add(dgvReport)

            Me.Controls.Add(pnlGrid)
            Me.Controls.Add(pnlSummary)
            Me.Controls.Add(pnlFilter)
            Me.Controls.Add(pnlTop)
        End Sub

        Private Sub LoadFilters()
            Dim depts = _deptRepo.GetAll()
            depts.Insert(0, New Department With {.Id = 0, .Name = "-- All Departments --"})
            cmbDepartment.DataSource = depts
            cmbDepartment.DisplayMember = "Name"
            cmbDepartment.ValueMember = "Id"
        End Sub

        Public Sub GenerateReport()
            Try
                Dim st = dtpFrom.Value.Date
                Dim ed = dtpTo.Value.Date
                Dim deptId = If(cmbDepartment.SelectedValue IsNot Nothing AndAlso TypeOf cmbDepartment.SelectedValue Is Integer, CInt(cmbDepartment.SelectedValue), 0)
                Dim reportIdx = cmbReportType.SelectedIndex

                dgvReport.Columns.Clear()
                dgvReport.Rows.Clear()

                Select Case reportIdx
                    Case 0 ' Daily Attendance
                        SetupDailyReport(st, ed, deptId)
                    Case 1 ' Late Records
                        SetupLateReport(st, ed, deptId)
                    Case 2 ' Overtime Analysis
                        SetupOvertimeReport(st, ed, deptId)
                    Case 3 ' Absence
                        SetupAbsenceReport(st, ed, deptId)
                    Case 4 ' Monthly Aggregates
                        SetupAggregatesReport(st, ed, deptId)
                End Select
            Catch ex As Exception
                MessageBox.Show("Error generating report: " & ex.Message, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub SetupDailyReport(st As DateTime, ed As DateTime, deptId As Integer)
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Date", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Emp ID", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Employee Name", .Width = 150})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Department", .Width = 140})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Check In", .Width = 85})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Check Out", .Width = 85})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Worked", .Width = 80})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Status", .Width = 90})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Remarks", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            Dim records = _attendanceService.GetAttendance(st, ed, deptId)
            Dim totalWorkedMinutes As Long = 0

            For Each r In records
                Dim cin = If(r.CheckIn.HasValue, r.CheckIn.Value.ToString("HH:mm"), "--:--")
                Dim cout = If(r.CheckOut.HasValue, r.CheckOut.Value.ToString("HH:mm"), "--:--")
                dgvReport.Rows.Add(r.AttendanceDate.ToString("yyyy-MM-dd"), r.EmployeeCode, r.EmployeeName, r.DepartmentName, cin, cout, r.WorkedHoursFormatted, r.Status.ToString(), r.Remarks)
                totalWorkedMinutes += r.WorkedMinutes
            Next

            Dim totalHours = Math.Round(totalWorkedMinutes / 60.0, 1)
            lblSummaryStats.Text = $"Daily Summary: {records.Count} Records Generated | Total Man-Hours: {totalHours} hrs"
        End Sub

        Private Sub SetupLateReport(st As DateTime, ed As DateTime, deptId As Integer)
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Date", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Emp ID", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Employee Name", .Width = 160})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Department", .Width = 150})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Shift", .Width = 120})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Check In", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Late Minutes", .Width = 110})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Remarks", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            Dim records = _attendanceService.GetAttendance(st, ed, deptId).Where(Function(r) r.LateMinutes > 0).ToList()
            Dim totalLateMinutes As Long = 0

            For Each r In records
                Dim cin = If(r.CheckIn.HasValue, r.CheckIn.Value.ToString("HH:mm:ss"), "--:--")
                dgvReport.Rows.Add(r.AttendanceDate.ToString("yyyy-MM-dd"), r.EmployeeCode, r.EmployeeName, r.DepartmentName, r.ShiftName, cin, r.LateMinutes & " mins", r.Remarks)
                totalLateMinutes += r.LateMinutes
            Next

            lblSummaryStats.Text = $"Late Arrivals: {records.Count} Incidents | Total Cumulative Delay: {totalLateMinutes} minutes ({Math.Round(totalLateMinutes / 60.0, 1)} hrs)"
        End Sub

        Private Sub SetupOvertimeReport(st As DateTime, ed As DateTime, deptId As Integer)
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Date", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Emp ID", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Employee Name", .Width = 160})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Department", .Width = 150})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Shift End", .Width = 90})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Check Out", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Overtime", .Width = 110})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Remarks", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            Dim records = _attendanceService.GetAttendance(st, ed, deptId).Where(Function(r) r.OvertimeMinutes > 0).ToList()
            Dim totalOtMinutes As Long = 0

            For Each r In records
                Dim cout = If(r.CheckOut.HasValue, r.CheckOut.Value.ToString("HH:mm:ss"), "--:--")
                dgvReport.Rows.Add(r.AttendanceDate.ToString("yyyy-MM-dd"), r.EmployeeCode, r.EmployeeName, r.DepartmentName, "Scheduled", cout, r.OvertimeHoursFormatted & " hrs (" & r.OvertimeMinutes & "m)", r.Remarks)
                totalOtMinutes += r.OvertimeMinutes
            Next

            lblSummaryStats.Text = $"Overtime Records: {records.Count} Sessions | Total Overtime: {Math.Round(totalOtMinutes / 60.0, 2)} hrs"
        End Sub

        Private Sub SetupAbsenceReport(st As DateTime, ed As DateTime, deptId As Integer)
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Date", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Emp ID", .Width = 95})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Employee Name", .Width = 160})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Department", .Width = 150})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Position", .Width = 150})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Status", .Width = 100})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Remarks", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            Dim records = _attendanceService.GetAttendance(st, ed, deptId, 0, AttendanceStatus.Absent)
            For Each r In records
                dgvReport.Rows.Add(r.AttendanceDate.ToString("yyyy-MM-dd"), r.EmployeeCode, r.EmployeeName, r.DepartmentName, "Staff", "Absent", r.Remarks)
            Next

            lblSummaryStats.Text = $"Absence Report: {records.Count} Unexcused Absence Record(s)"
        End Sub

        Private Sub SetupAggregatesReport(st As DateTime, ed As DateTime, deptId As Integer)
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Emp ID", .Width = 100})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Employee Name", .Width = 160})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Department", .Width = 150})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Days Logged", .Width = 100})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Late Occurrences", .Width = 120})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Total Worked (Hrs)", .Width = 130})
            dgvReport.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Total OT (Hrs)", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            Dim records = _attendanceService.GetAttendance(st, ed, deptId)
            Dim grouped = records.GroupBy(Function(r) r.EmployeeId).ToList()

            For Each g In grouped
                Dim firstRec = g.First()
                Dim daysCount = g.Count(Function(r) r.Status = AttendanceStatus.Present OrElse r.Status = AttendanceStatus.Late)
                Dim lateCount = g.Count(Function(r) r.LateMinutes > 0)
                Dim totalWorkedHrs = Math.Round(g.Sum(Function(r) r.WorkedMinutes) / 60.0, 1)
                Dim totalOtHrs = Math.Round(g.Sum(Function(r) r.OvertimeMinutes) / 60.0, 1)

                dgvReport.Rows.Add(firstRec.EmployeeCode, firstRec.EmployeeName, firstRec.DepartmentName, daysCount, lateCount, totalWorkedHrs & " hrs", totalOtHrs & " hrs")
            Next

            lblSummaryStats.Text = $"Monthly Aggregate: {grouped.Count} Active Employees Aggregated"
        End Sub
    End Class
End Namespace
