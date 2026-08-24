Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Domain.Enums
Imports AttendanceSystem.Infrastructure.Repositories
Imports AttendanceSystem.Application.Services

Namespace Presentation.Views
    Public Class FormLeaveRequest
        Inherits Form

        Private cmbEmployee As ComboBox
        Private cmbLeaveType As ComboBox
        Private dtpStart As DateTimePicker
        Private dtpEnd As DateTimePicker
        Private lblDaysCount As Label
        Private txtReason As TextBox
        Private lblError As Label
        Private btnSubmit As Button
        Private btnCancel As Button

        Private ReadOnly _leaveService As New LeaveService()
        Private ReadOnly _empRepo As New EmployeeRepository()

        Public Sub New()
            InitializeComponent()
            LoadDropdowns()
            CalculateDays()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "Submit Employee Leave Request"
            Me.Size = New Size(480, 440)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.BackColor = UITheme.BackgroundColor
            Me.Font = UITheme.FontBody

            Dim pnlContainer As New Panel With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(24, 20, 24, 20),
                .BackColor = UITheme.PanelBackground
            }

            Dim y = 16
            Dim gap = 44

            Dim lblEmp As New Label With {.Text = "Employee *", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            cmbEmployee = New ComboBox With {.Location = New Point(170, y - 2), .Size = New Size(260, 26), .DropDownStyle = ComboBoxStyle.DropDownList}
            pnlContainer.Controls.AddRange(New Control() {lblEmp, cmbEmployee})

            y += gap
            Dim lblType As New Label With {.Text = "Leave Type *", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            cmbLeaveType = New ComboBox With {.Location = New Point(170, y - 2), .Size = New Size(260, 26), .DropDownStyle = ComboBoxStyle.DropDownList}
            pnlContainer.Controls.AddRange(New Control() {lblType, cmbLeaveType})

            y += gap
            Dim lblStart As New Label With {.Text = "Start Date *", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            dtpStart = New DateTimePicker With {.Location = New Point(170, y - 2), .Size = New Size(260, 26), .Format = DateTimePickerFormat.Short}
            AddHandler dtpStart.ValueChanged, Sub() CalculateDays()
            pnlContainer.Controls.AddRange(New Control() {lblStart, dtpStart})

            y += gap
            Dim lblEnd As New Label With {.Text = "End Date *", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            dtpEnd = New DateTimePicker With {.Location = New Point(170, y - 2), .Size = New Size(260, 26), .Format = DateTimePickerFormat.Short}
            AddHandler dtpEnd.ValueChanged, Sub() CalculateDays()
            pnlContainer.Controls.AddRange(New Control() {lblEnd, dtpEnd})

            y += gap
            Dim lblDaysPrompt As New Label With {.Text = "Total Days:", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            lblDaysCount = New Label With {.Text = "1 Day(s)", .Location = New Point(170, y), .Size = New Size(260, 18), .Font = UITheme.FontBodyBold, .ForeColor = UITheme.PrimaryColor}
            pnlContainer.Controls.AddRange(New Control() {lblDaysPrompt, lblDaysCount})

            y += gap
            Dim lblReason As New Label With {.Text = "Reason / Notes *", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            txtReason = New TextBox With {.Location = New Point(170, y - 2), .Size = New Size(260, 44), .Multiline = True}
            pnlContainer.Controls.AddRange(New Control() {lblReason, txtReason})

            y += 52
            lblError = New Label With {.Location = New Point(24, y), .Size = New Size(410, 22), .ForeColor = UITheme.StatusAbsent, .Font = UITheme.FontSmall}
            pnlContainer.Controls.Add(lblError)

            y += 26
            btnSubmit = New Button With {.Text = "Submit Request", .Location = New Point(216, y), .Size = New Size(125, 32)}
            UITheme.ApplyButtonStyle(btnSubmit, True)
            AddHandler btnSubmit.Click, AddressOf BtnSubmit_Click

            btnCancel = New Button With {.Text = "Cancel", .Location = New Point(348, y), .Size = New Size(82, 32)}
            UITheme.ApplyButtonStyle(btnCancel, False)
            AddHandler btnCancel.Click, Sub() Me.Close()

            pnlContainer.Controls.AddRange(New Control() {btnSubmit, btnCancel})

            Me.Controls.Add(pnlContainer)
            Me.AcceptButton = btnSubmit
            Me.CancelButton = btnCancel
        End Sub

        Private Sub LoadDropdowns()
            Dim emps = _empRepo.GetAll()
            cmbEmployee.DataSource = emps
            cmbEmployee.DisplayMember = "FullName"
            cmbEmployee.ValueMember = "Id"

            Dim types = _leaveService.GetLeaveTypes()
            cmbLeaveType.DataSource = types
            cmbLeaveType.DisplayMember = "Name"
            cmbLeaveType.ValueMember = "Id"
        End Sub

        Private Sub CalculateDays()
            Dim days = Convert.ToInt32((dtpEnd.Value.Date - dtpStart.Value.Date).TotalDays) + 1
            If days < 1 Then
                lblDaysCount.Text = "Invalid date range"
                lblDaysCount.ForeColor = UITheme.StatusAbsent
            Else
                lblDaysCount.Text = $"{days} Day(s)"
                lblDaysCount.ForeColor = UITheme.PrimaryColor
            End If
        End Sub

        Private Sub BtnSubmit_Click(sender As Object, e As EventArgs)
            lblError.Text = ""

            If cmbEmployee.SelectedValue Is Nothing Then
                lblError.Text = "Please select an employee."
                Return
            End If

            If cmbLeaveType.SelectedValue Is Nothing Then
                lblError.Text = "Please select a leave type."
                Return
            End If

            If dtpEnd.Value.Date < dtpStart.Value.Date Then
                lblError.Text = "End date cannot be earlier than start date."
                Return
            End If

            If String.IsNullOrWhiteSpace(txtReason.Text) Then
                lblError.Text = "Please provide a reason for leave."
                Return
            End If

            Dim req As New LeaveRequest With {
                .EmployeeId = CInt(cmbEmployee.SelectedValue),
                .LeaveTypeId = CInt(cmbLeaveType.SelectedValue),
                .StartDate = dtpStart.Value.Date,
                .EndDate = dtpEnd.Value.Date,
                .Reason = txtReason.Text.Trim()
            }

            Dim res = _leaveService.SubmitLeave(req)
            If res.Success Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                lblError.Text = res.Message
            End If
        End Sub
    End Class

    Public Class LeavesView
        Inherits UserControl

        Private dgvLeaves As DataGridView
        Private cmbFilterStatus As ComboBox
        Private btnNewRequest As Button
        Private btnApprove As Button
        Private btnReject As Button
        Private lblRecordCount As Label

        Private ReadOnly _leaveService As New LeaveService()

        Public Sub New()
            InitializeComponent()
            LoadLeaves()
        End Sub

        Private Sub InitializeComponent()
            Me.Dock = DockStyle.Fill
            Me.BackColor = UITheme.BackgroundColor
            Me.Font = UITheme.FontBody
            Me.Padding = New Padding(16)

            Dim pnlTop As New Panel With {.Dock = DockStyle.Top, .Height = 44, .BackColor = Color.Transparent}
            Dim lblTitle As New Label With {.Text = "Leave Management & Approval Workflow", .Font = UITheme.FontPageTitle, .ForeColor = UITheme.TextPrimary, .Dock = DockStyle.Left, .AutoSize = True, .TextAlign = ContentAlignment.MiddleLeft}

            Dim pnlActions As New FlowLayoutPanel With {.Dock = DockStyle.Right, .Width = 380, .Height = 36}

            btnNewRequest = New Button With {.Text = "New Leave Request", .Width = 140}
            UITheme.ApplyButtonStyle(btnNewRequest, True)
            AddHandler btnNewRequest.Click, AddressOf BtnNewRequest_Click

            btnApprove = New Button With {.Text = "Approve", .Width = 80}
            UITheme.ApplyButtonStyle(btnApprove, False)
            AddHandler btnApprove.Click, AddressOf BtnApprove_Click

            btnReject = New Button With {.Text = "Reject", .Width = 75}
            UITheme.ApplyButtonStyle(btnReject, False)
            AddHandler btnReject.Click, AddressOf BtnReject_Click

            pnlActions.Controls.AddRange(New Control() {btnNewRequest, btnApprove, btnReject})
            pnlTop.Controls.AddRange(New Control() {pnlActions, lblTitle})

            Dim pnlFilter As New Panel With {.Dock = DockStyle.Top, .Height = 46, .BackColor = UITheme.PanelBackground, .BorderStyle = BorderStyle.FixedSingle, .Padding = New Padding(10, 8, 10, 8), .Margin = New Padding(0, 8, 0, 8)}
            Dim lblStatus As New Label With {.Text = "Filter by Status:", .Location = New Point(10, 12), .AutoSize = True, .Font = UITheme.FontBodyBold}
            cmbFilterStatus = New ComboBox With {.Location = New Point(125, 10), .Width = 160, .DropDownStyle = ComboBoxStyle.DropDownList}
            cmbFilterStatus.Items.Add("-- All Requests --")
            For Each itemVal In [Enum].GetValues(GetType(LeaveStatus))
                cmbFilterStatus.Items.Add(itemVal)
            Next
            cmbFilterStatus.SelectedIndex = 0
            AddHandler cmbFilterStatus.SelectedIndexChanged, Sub() LoadLeaves()

            lblRecordCount = New Label With {.Dock = DockStyle.Right, .Width = 150, .TextAlign = ContentAlignment.MiddleRight, .ForeColor = UITheme.TextSecondary}

            pnlFilter.Controls.AddRange(New Control() {lblStatus, cmbFilterStatus, lblRecordCount})

            Dim pnlGrid As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(0, 8, 0, 0)}
            dgvLeaves = New DataGridView With {.Dock = DockStyle.Fill}
            UITheme.ApplyGridStyle(dgvLeaves)

            dgvLeaves.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "EmployeeCode", .HeaderText = "Emp ID", .Width = 95})
            dgvLeaves.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "EmployeeName", .HeaderText = "Employee Name", .Width = 150})
            dgvLeaves.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "LeaveTypeName", .HeaderText = "Leave Type", .Width = 120})
            dgvLeaves.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "StartDate", .HeaderText = "Start Date", .Width = 95})
            dgvLeaves.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "EndDate", .HeaderText = "End Date", .Width = 95})
            dgvLeaves.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "DaysCount", .HeaderText = "Days", .Width = 60})
            dgvLeaves.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Status", .HeaderText = "Status", .Width = 90})
            dgvLeaves.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "ApproverName", .HeaderText = "Approved By", .Width = 120})
            dgvLeaves.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Reason", .HeaderText = "Reason", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            pnlGrid.Controls.Add(dgvLeaves)

            Me.Controls.Add(pnlGrid)
            Me.Controls.Add(pnlFilter)
            Me.Controls.Add(pnlTop)
        End Sub

        Public Sub LoadLeaves()
            Dim status As Nullable(Of LeaveStatus) = Nothing
            If cmbFilterStatus.SelectedIndex > 0 Then
                status = CType(cmbFilterStatus.SelectedItem, LeaveStatus)
            End If

            Dim list = _leaveService.GetRequests(status)
            dgvLeaves.Rows.Clear()
            For Each req In list
                Dim rowIndex = dgvLeaves.Rows.Add(req.EmployeeCode, req.EmployeeName, req.LeaveTypeName, req.StartDate.ToString("yyyy-MM-dd"), req.EndDate.ToString("yyyy-MM-dd"), req.DaysCount, req.Status.ToString(), req.ApproverName, req.Reason)
                dgvLeaves.Rows(rowIndex).Tag = req

                Dim cell = dgvLeaves.Rows(rowIndex).Cells(6)
                Select Case req.Status
                    Case LeaveStatus.Approved
                        cell.Style.ForeColor = UITheme.StatusPresent
                    Case LeaveStatus.Pending
                        cell.Style.ForeColor = UITheme.StatusLate
                    Case LeaveStatus.Rejected
                        cell.Style.ForeColor = UITheme.StatusAbsent
                End Select
            Next

            lblRecordCount.Text = $"Total: {list.Count} Requests"
        End Sub

        Private Sub BtnNewRequest_Click(sender As Object, e As EventArgs)
            Using frm As New FormLeaveRequest()
                If frm.ShowDialog(Me) = DialogResult.OK Then
                    LoadLeaves()
                End If
            End Using
        End Sub

        Private Sub BtnApprove_Click(sender As Object, e As EventArgs)
            If dgvLeaves.SelectedRows.Count = 0 Then Return
            Dim req = CType(dgvLeaves.SelectedRows(0).Tag, LeaveRequest)
            If req IsNot Nothing Then
                If req.Status <> LeaveStatus.Pending Then
                    MessageBox.Show("This request has already been processed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End If

                Dim confirm = MessageBox.Show($"Approve leave request for '{req.EmployeeName}' ({req.DaysCount} days)?", "Confirm Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If confirm = DialogResult.Yes Then
                    _leaveService.ProcessRequest(req.Id, True, "Approved via Leave Management")
                    LoadLeaves()
                End If
            End If
        End Sub

        Private Sub BtnReject_Click(sender As Object, e As EventArgs)
            If dgvLeaves.SelectedRows.Count = 0 Then Return
            Dim req = CType(dgvLeaves.SelectedRows(0).Tag, LeaveRequest)
            If req IsNot Nothing Then
                If req.Status <> LeaveStatus.Pending Then
                    MessageBox.Show("This request has already been processed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End If

                Dim confirm = MessageBox.Show($"Reject leave request for '{req.EmployeeName}'?", "Confirm Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If confirm = DialogResult.Yes Then
                    _leaveService.ProcessRequest(req.Id, False, "Rejected by Supervisor/HR")
                    LoadLeaves()
                End If
            End If
        End Sub
    End Class
End Namespace
