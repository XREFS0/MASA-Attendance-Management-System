Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Domain.Enums
Imports AttendanceSystem.Infrastructure.Repositories
Imports AttendanceSystem.Application.Services

Namespace Presentation.Views
    Public Class FormAttendanceEdit
        Inherits Form

        Private cmbEmployee As ComboBox
        Private dtpDate As DateTimePicker
        Private dtpCheckIn As DateTimePicker
        Private dtpCheckOut As DateTimePicker
        Private chkCheckInSet As CheckBox
        Private chkCheckOutSet As CheckBox
        Private cmbStatus As ComboBox
        Private txtRemarks As TextBox
        Private lblError As Label
        Private btnSave As Button
        Private btnCancel As Button

        Private ReadOnly _attendanceService As New AttendanceService()
        Private ReadOnly _empRepo As New EmployeeRepository()
        Private _recordId As Integer = 0
        Private _shiftId As Integer = 1

        Public Sub New(Optional record As AttendanceRecord = Nothing)
            InitializeComponent()
            LoadEmployees()

            If record IsNot Nothing Then
                _recordId = record.Id
                _shiftId = record.ShiftId
                Me.Text = "Edit Attendance Record — " & record.EmployeeName
                cmbEmployee.SelectedValue = record.EmployeeId
                cmbEmployee.Enabled = False
                dtpDate.Value = record.AttendanceDate
                dtpDate.Enabled = False

                If record.CheckIn.HasValue Then
                    chkCheckInSet.Checked = True
                    dtpCheckIn.Value = record.CheckIn.Value
                Else
                    chkCheckInSet.Checked = False
                End If

                If record.CheckOut.HasValue Then
                    chkCheckOutSet.Checked = True
                    dtpCheckOut.Value = record.CheckOut.Value
                Else
                    chkCheckOutSet.Checked = False
                End If

                cmbStatus.SelectedItem = record.Status
                txtRemarks.Text = record.Remarks
            Else
                Me.Text = "Manual Attendance Entry"
                dtpDate.Value = DateTime.Today
                chkCheckInSet.Checked = True
                dtpCheckIn.Value = DateTime.Today.AddHours(8)
                chkCheckOutSet.Checked = True
                dtpCheckOut.Value = DateTime.Today.AddHours(17)
                cmbStatus.SelectedItem = AttendanceStatus.Present
            End If
        End Sub

        Private Sub InitializeComponent()
            Me.Size = New Size(500, 480)
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
            Dim gap = 46

            Dim lblEmp As New Label With {.Text = "Employee *", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            cmbEmployee = New ComboBox With {.Location = New Point(170, y - 2), .Size = New Size(280, 26), .DropDownStyle = ComboBoxStyle.DropDownList}
            pnlContainer.Controls.AddRange(New Control() {lblEmp, cmbEmployee})

            y += gap
            Dim lblDt As New Label With {.Text = "Attendance Date *", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            dtpDate = New DateTimePicker With {.Location = New Point(170, y - 2), .Size = New Size(280, 26), .Format = DateTimePickerFormat.Short}
            pnlContainer.Controls.AddRange(New Control() {lblDt, dtpDate})

            y += gap
            Dim lblCin As New Label With {.Text = "Check In Time", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            chkCheckInSet = New CheckBox With {.Text = "", .Location = New Point(170, y), .Size = New Size(20, 20)}
            dtpCheckIn = New DateTimePicker With {.Location = New Point(195, y - 2), .Size = New Size(255, 26), .Format = DateTimePickerFormat.Time, .ShowUpDown = True}
            AddHandler chkCheckInSet.CheckedChanged, Sub() dtpCheckIn.Enabled = chkCheckInSet.Checked
            pnlContainer.Controls.AddRange(New Control() {lblCin, chkCheckInSet, dtpCheckIn})

            y += gap
            Dim lblCout As New Label With {.Text = "Check Out Time", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            chkCheckOutSet = New CheckBox With {.Text = "", .Location = New Point(170, y), .Size = New Size(20, 20)}
            dtpCheckOut = New DateTimePicker With {.Location = New Point(195, y - 2), .Size = New Size(255, 26), .Format = DateTimePickerFormat.Time, .ShowUpDown = True}
            AddHandler chkCheckOutSet.CheckedChanged, Sub() dtpCheckOut.Enabled = chkCheckOutSet.Checked
            pnlContainer.Controls.AddRange(New Control() {lblCout, chkCheckOutSet, dtpCheckOut})

            y += gap
            Dim lblStatus As New Label With {.Text = "Status *", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            cmbStatus = New ComboBox With {.Location = New Point(170, y - 2), .Size = New Size(280, 26), .DropDownStyle = ComboBoxStyle.DropDownList}
            cmbStatus.DataSource = [Enum].GetValues(GetType(AttendanceStatus))
            pnlContainer.Controls.AddRange(New Control() {lblStatus, cmbStatus})

            y += gap
            Dim lblRemarks As New Label With {.Text = "Remarks / Reason", .Location = New Point(24, y), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            txtRemarks = New TextBox With {.Location = New Point(170, y - 2), .Size = New Size(280, 42), .Multiline = True}
            pnlContainer.Controls.AddRange(New Control() {lblRemarks, txtRemarks})

            y += 50
            lblError = New Label With {.Location = New Point(24, y), .Size = New Size(430, 22), .ForeColor = UITheme.StatusAbsent, .Font = UITheme.FontSmall}
            pnlContainer.Controls.Add(lblError)

            y += 26
            btnSave = New Button With {.Text = "Save Record", .Location = New Point(236, y), .Size = New Size(120, 32)}
            UITheme.ApplyButtonStyle(btnSave, True)
            AddHandler btnSave.Click, AddressOf BtnSave_Click

            btnCancel = New Button With {.Text = "Cancel", .Location = New Point(362, y), .Size = New Size(88, 32)}
            UITheme.ApplyButtonStyle(btnCancel, False)
            AddHandler btnCancel.Click, Sub() Me.Close()

            pnlContainer.Controls.AddRange(New Control() {btnSave, btnCancel})

            Me.Controls.Add(pnlContainer)
            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel
        End Sub

        Private Sub LoadEmployees()
            Dim emps = _empRepo.GetAll()
            cmbEmployee.DataSource = emps
            cmbEmployee.DisplayMember = "FullName"
            cmbEmployee.ValueMember = "Id"
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            lblError.Text = ""

            If cmbEmployee.SelectedValue Is Nothing Then
                lblError.Text = "Please select an employee."
                Return
            End If

            Dim empId = CInt(cmbEmployee.SelectedValue)
            Dim emp = _empRepo.GetById(empId)
            If emp Is Nothing Then Return

            Dim attDate = dtpDate.Value.Date
            Dim checkIn As Nullable(Of DateTime) = Nothing
            Dim checkOut As Nullable(Of DateTime) = Nothing

            If chkCheckInSet.Checked Then
                checkIn = New DateTime(attDate.Year, attDate.Month, attDate.Day, dtpCheckIn.Value.Hour, dtpCheckIn.Value.Minute, dtpCheckIn.Value.Second)
            End If

            If chkCheckOutSet.Checked Then
                checkOut = New DateTime(attDate.Year, attDate.Month, attDate.Day, dtpCheckOut.Value.Hour, dtpCheckOut.Value.Minute, dtpCheckOut.Value.Second)
            End If

            If checkIn.HasValue AndAlso checkOut.HasValue AndAlso checkOut.Value < checkIn.Value Then
                lblError.Text = "Check Out time cannot be earlier than Check In time."
                Return
            End If

            Dim rec As New AttendanceRecord With {
                .Id = _recordId,
                .EmployeeId = empId,
                .ShiftId = If(_recordId > 0, _shiftId, emp.ShiftId),
                .AttendanceDate = attDate,
                .CheckIn = checkIn,
                .CheckOut = checkOut,
                .Status = CType(cmbStatus.SelectedItem, AttendanceStatus),
                .Remarks = txtRemarks.Text.Trim()
            }

            Dim res = _attendanceService.SaveManualRecord(rec)
            If res.Success Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                lblError.Text = res.Message
            End If
        End Sub
    End Class
End Namespace
