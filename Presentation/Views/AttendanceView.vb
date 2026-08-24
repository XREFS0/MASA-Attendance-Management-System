Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Domain.Enums
Imports AttendanceSystem.Application.Services
Imports AttendanceSystem.Infrastructure.Repositories
Imports AttendanceSystem.Common.Utilities

Namespace Presentation.Views
    Public Class AttendanceView
        Inherits UserControl

        Private txtEmpCodeCheck As TextBox
        Private btnCheckIn As Button
        Private btnCheckOut As Button
        Private lblTerminalStatus As Label

        Private dtpFrom As DateTimePicker
        Private dtpTo As DateTimePicker
        Private cmbFilterDept As ComboBox
        Private cmbFilterStatus As ComboBox
        Private btnFilter As Button
        Private btnManualEntry As Button
        Private btnEdit As Button
        Private btnDelete As Button
        Private btnExport As Button
        Private dgvAttendance As DataGridView
        Private lblRecordCount As Label

        Private ReadOnly _attendanceService As New AttendanceService()
        Private ReadOnly _deptRepo As New DepartmentRepository()
        Private ReadOnly _empRepo As New EmployeeRepository()

        Public Sub New()
            InitializeComponent()
            LoadFilters()
            LoadAttendanceData()
        End Sub

        Private Sub InitializeComponent()
            Me.Dock = DockStyle.Fill
            Me.BackColor = UITheme.BackgroundColor
            Me.Font = UITheme.FontBody
            Me.Padding = New Padding(16)

            Dim pnlTop As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 44,
                .BackColor = Color.Transparent
            }

            Dim lblTitle As New Label With {
                .Text = "Attendance Management & Check-In Terminal",
                .Font = UITheme.FontPageTitle,
                .ForeColor = UITheme.TextPrimary,
                .Dock = DockStyle.Left,
                .AutoSize = True,
                .TextAlign = ContentAlignment.MiddleLeft
            }

            Dim pnlActionBtns As New FlowLayoutPanel With {
                .Dock = DockStyle.Right,
                .Width = 380,
                .Height = 36
            }

            btnManualEntry = New Button With {.Text = "Manual Entry", .Width = 100}
            UITheme.ApplyButtonStyle(btnManualEntry, True)
            AddHandler btnManualEntry.Click, AddressOf BtnManualEntry_Click

            btnEdit = New Button With {.Text = "Edit Record", .Width = 90}
            UITheme.ApplyButtonStyle(btnEdit, False)
            AddHandler btnEdit.Click, AddressOf BtnEdit_Click

            btnDelete = New Button With {.Text = "Delete", .Width = 70}
            UITheme.ApplyButtonStyle(btnDelete, False)
            AddHandler btnDelete.Click, AddressOf BtnDelete_Click

            btnExport = New Button With {.Text = "Export CSV", .Width = 90}
            UITheme.ApplyButtonStyle(btnExport, False)
            AddHandler btnExport.Click, Sub() CsvExporter.ExportToCsv(dgvAttendance, "Attendance_Log")

            pnlActionBtns.Controls.AddRange(New Control() {btnManualEntry, btnEdit, btnDelete, btnExport})

            pnlTop.Controls.Add(pnlActionBtns)
            pnlTop.Controls.Add(lblTitle)

            Dim pnlTerminal As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 52,
                .BackColor = UITheme.PanelBackground,
                .BorderStyle = BorderStyle.FixedSingle,
                .Padding = New Padding(12, 10, 12, 10),
                .Margin = New Padding(0, 8, 0, 8)
            }

            Dim lblPrompt As New Label With {.Text = "Employee ID / Barcode:", .Location = New Point(12, 14), .AutoSize = True, .Font = UITheme.FontBodyBold}
            txtEmpCodeCheck = New TextBox With {.Location = New Point(165, 12), .Width = 140, .Font = UITheme.FontBodyBold}
            
            btnCheckIn = New Button With {.Text = "Check In", .Location = New Point(315, 10), .Width = 85, .Height = 28}
            UITheme.ApplyButtonStyle(btnCheckIn, True)
            btnCheckIn.BackColor = UITheme.StatusPresent
            AddHandler btnCheckIn.Click, AddressOf BtnCheckIn_Click

            btnCheckOut = New Button With {.Text = "Check Out", .Location = New Point(408, 10), .Width = 90, .Height = 28}
            UITheme.ApplyButtonStyle(btnCheckOut, True)
            btnCheckOut.BackColor = UITheme.PrimaryColor
            AddHandler btnCheckOut.Click, AddressOf BtnCheckOut_Click

            lblTerminalStatus = New Label With {
                .Location = New Point(510, 10),
                .Size = New Size(450, 28),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Font = UITheme.FontBodyBold,
                .ForeColor = UITheme.TextSecondary,
                .Text = "Ready for swipe or manual ID entry."
            }

            pnlTerminal.Controls.AddRange(New Control() {lblPrompt, txtEmpCodeCheck, btnCheckIn, btnCheckOut, lblTerminalStatus})

            Dim pnlFilter As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 46,
                .BackColor = UITheme.PanelBackground,
                .BorderStyle = BorderStyle.FixedSingle,
                .Padding = New Padding(10, 8, 10, 8),
                .Margin = New Padding(0, 6, 0, 8)
            }

            Dim lblFrom As New Label With {.Text = "From:", .Location = New Point(10, 12), .AutoSize = True, .Font = UITheme.FontBodyBold}
            dtpFrom = New DateTimePicker With {.Location = New Point(52, 10), .Width = 110, .Format = DateTimePickerFormat.Short, .Value = DateTime.Today.AddDays(-7)}

            Dim lblTo As New Label With {.Text = "To:", .Location = New Point(170, 12), .AutoSize = True, .Font = UITheme.FontBodyBold}
            dtpTo = New DateTimePicker With {.Location = New Point(196, 10), .Width = 110, .Format = DateTimePickerFormat.Short, .Value = DateTime.Today}

            Dim lblDept As New Label With {.Text = "Department:", .Location = New Point(318, 12), .AutoSize = True, .Font = UITheme.FontBodyBold}
            cmbFilterDept = New ComboBox With {.Location = New Point(400, 10), .Width = 150, .DropDownStyle = ComboBoxStyle.DropDownList}

            Dim lblStatus As New Label With {.Text = "Status:", .Location = New Point(560, 12), .AutoSize = True, .Font = UITheme.FontBodyBold}
            cmbFilterStatus = New ComboBox With {.Location = New Point(610, 10), .Width = 110, .DropDownStyle = ComboBoxStyle.DropDownList}

            btnFilter = New Button With {.Text = "Filter", .Location = New Point(730, 8), .Width = 65, .Height = 28}
            UITheme.ApplyButtonStyle(btnFilter, False)
            AddHandler btnFilter.Click, Sub() LoadAttendanceData()

            lblRecordCount = New Label With {.Dock = DockStyle.Right, .Width = 150, .TextAlign = ContentAlignment.MiddleRight, .ForeColor = UITheme.TextSecondary}

            pnlFilter.Controls.AddRange(New Control() {lblFrom, dtpFrom, lblTo, dtpTo, lblDept, cmbFilterDept, lblStatus, cmbFilterStatus, btnFilter, lblRecordCount})

            Dim pnlGrid As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(0, 8, 0, 0)}
            dgvAttendance = New DataGridView With {.Dock = DockStyle.Fill}
            UITheme.ApplyGridStyle(dgvAttendance)

            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "AttendanceDate", .HeaderText = "Date", .Width = 95})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "EmployeeCode", .HeaderText = "Emp ID", .Width = 95})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "EmployeeName", .HeaderText = "Employee Name", .Width = 150})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "DepartmentName", .HeaderText = "Department", .Width = 130})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "ShiftName", .HeaderText = "Shift", .Width = 110})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "CheckIn", .HeaderText = "Check In", .Width = 85})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "CheckOut", .HeaderText = "Check Out", .Width = 85})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "WorkedHours", .HeaderText = "Worked", .Width = 75})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "LateMinutes", .HeaderText = "Late (Min)", .Width = 85})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "OvertimeMinutes", .HeaderText = "OT (Min)", .Width = 80})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Status", .HeaderText = "Status", .Width = 90})
            dgvAttendance.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Remarks", .HeaderText = "Remarks", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            AddHandler dgvAttendance.CellDoubleClick, Sub() BtnEdit_Click(Nothing, Nothing)

            pnlGrid.Controls.Add(dgvAttendance)

            Me.Controls.Add(pnlGrid)
            Me.Controls.Add(pnlFilter)
            Me.Controls.Add(pnlTerminal)
            Me.Controls.Add(pnlTop)
        End Sub

        Private Sub LoadFilters()
            Dim depts = _deptRepo.GetAll()
            depts.Insert(0, New Department With {.Id = 0, .Name = "-- All Departments --"})
            cmbFilterDept.DataSource = depts
            cmbFilterDept.DisplayMember = "Name"
            cmbFilterDept.ValueMember = "Id"

            cmbFilterStatus.Items.Add("-- All Statuses --")
            For Each itemVal In [Enum].GetValues(GetType(AttendanceStatus))
                cmbFilterStatus.Items.Add(itemVal)
            Next
            cmbFilterStatus.SelectedIndex = 0
        End Sub

        Public Sub LoadAttendanceData()
            Try
                Dim st = dtpFrom.Value.Date
                Dim ed = dtpTo.Value.Date
                Dim deptId = If(cmbFilterDept.SelectedValue IsNot Nothing AndAlso TypeOf cmbFilterDept.SelectedValue Is Integer, CInt(cmbFilterDept.SelectedValue), 0)
                Dim status As Nullable(Of AttendanceStatus) = Nothing

                If cmbFilterStatus.SelectedIndex > 0 Then
                    status = CType(cmbFilterStatus.SelectedItem, AttendanceStatus)
                End If

                Dim records = _attendanceService.GetAttendance(st, ed, deptId, 0, status)
                dgvAttendance.Rows.Clear()

                For Each r In records
                    Dim cinStr = If(r.CheckIn.HasValue, r.CheckIn.Value.ToString("HH:mm:ss"), "--:--")
                    Dim coutStr = If(r.CheckOut.HasValue, r.CheckOut.Value.ToString("HH:mm:ss"), "--:--")

                    Dim rowIndex = dgvAttendance.Rows.Add(r.AttendanceDate.ToString("yyyy-MM-dd"), r.EmployeeCode, r.EmployeeName, r.DepartmentName, r.ShiftName, cinStr, coutStr, r.WorkedHoursFormatted, r.LateMinutes, r.OvertimeMinutes, r.Status.ToString(), r.Remarks)
                    dgvAttendance.Rows(rowIndex).Tag = r

                    Dim cell = dgvAttendance.Rows(rowIndex).Cells(10)
                    Select Case r.Status
                        Case AttendanceStatus.Present
                            cell.Style.ForeColor = UITheme.StatusPresent
                        Case AttendanceStatus.Late
                            cell.Style.ForeColor = UITheme.StatusLate
                        Case AttendanceStatus.Absent
                            cell.Style.ForeColor = UITheme.StatusAbsent
                        Case AttendanceStatus.OnLeave
                            cell.Style.ForeColor = UITheme.StatusLeave
                    End Select
                Next

                lblRecordCount.Text = $"Total: {records.Count} Records"
            Catch ex As Exception
                MessageBox.Show("Error loading attendance: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub BtnCheckIn_Click(sender As Object, e As EventArgs)
            Dim code = txtEmpCodeCheck.Text.Trim()
            If String.IsNullOrWhiteSpace(code) Then
                lblTerminalStatus.ForeColor = UITheme.StatusAbsent
                lblTerminalStatus.Text = "Please enter an Employee ID."
                Return
            End If

            Dim res = _attendanceService.CheckIn(code)
            If res.Success Then
                lblTerminalStatus.ForeColor = UITheme.StatusPresent
                lblTerminalStatus.Text = res.Message
                txtEmpCodeCheck.Clear()
                LoadAttendanceData()
            Else
                lblTerminalStatus.ForeColor = UITheme.StatusAbsent
                lblTerminalStatus.Text = res.Message
            End If
        End Sub

        Private Sub BtnCheckOut_Click(sender As Object, e As EventArgs)
            Dim code = txtEmpCodeCheck.Text.Trim()
            If String.IsNullOrWhiteSpace(code) Then
                lblTerminalStatus.ForeColor = UITheme.StatusAbsent
                lblTerminalStatus.Text = "Please enter an Employee ID."
                Return
            End If

            Dim res = _attendanceService.CheckOut(code)
            If res.Success Then
                lblTerminalStatus.ForeColor = UITheme.StatusPresent
                lblTerminalStatus.Text = res.Message
                txtEmpCodeCheck.Clear()
                LoadAttendanceData()
            Else
                lblTerminalStatus.ForeColor = UITheme.StatusAbsent
                lblTerminalStatus.Text = res.Message
            End If
        End Sub

        Private Sub BtnManualEntry_Click(sender As Object, e As EventArgs)
            Using frm As New FormAttendanceEdit()
                If frm.ShowDialog(Me) = DialogResult.OK Then
                    LoadAttendanceData()
                End If
            End Using
        End Sub

        Private Sub BtnEdit_Click(sender As Object, e As EventArgs)
            If dgvAttendance.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select an attendance record to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim rec = CType(dgvAttendance.SelectedRows(0).Tag, AttendanceRecord)
            If rec IsNot Nothing Then
                Using frm As New FormAttendanceEdit(rec)
                    If frm.ShowDialog(Me) = DialogResult.OK Then
                        LoadAttendanceData()
                    End If
                End Using
            End If
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
            If dgvAttendance.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select an attendance record to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim rec = CType(dgvAttendance.SelectedRows(0).Tag, AttendanceRecord)
            If rec IsNot Nothing Then
                Dim confirm = MessageBox.Show($"Are you sure you want to delete attendance record for '{rec.EmployeeName}' on {rec.AttendanceDate:yyyy-MM-dd}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If confirm = DialogResult.Yes Then
                    _attendanceService.DeleteRecord(rec.Id)
                    LoadAttendanceData()
                End If
            End If
        End Sub
    End Class
End Namespace
