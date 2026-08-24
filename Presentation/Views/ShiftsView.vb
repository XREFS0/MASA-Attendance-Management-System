Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Infrastructure.Repositories

Namespace Presentation.Views
    Public Class FormShiftEdit
        Inherits Form

        Private txtName As TextBox
        Private dtpStart As DateTimePicker
        Private dtpEnd As DateTimePicker
        Private numGrace As NumericUpDown
        Private numBreak As NumericUpDown
        Private numHalfDay As NumericUpDown
        Private chkActive As CheckBox
        Private txtDesc As TextBox
        Private lblError As Label
        Private btnSave As Button
        Private btnCancel As Button

        Private ReadOnly _shiftRepo As New ShiftRepository()
        Private _shiftId As Integer = 0

        Public Sub New(Optional shift As Shift = Nothing)
            InitializeComponent()

            If shift IsNot Nothing Then
                _shiftId = shift.Id
                Me.Text = "Edit Shift — " & shift.Name
                txtName.Text = shift.Name
                dtpStart.Value = DateTime.Today.Add(shift.StartTime)
                dtpEnd.Value = DateTime.Today.Add(shift.EndTime)
                numGrace.Value = shift.GracePeriodMinutes
                numBreak.Value = shift.BreakDurationMinutes
                numHalfDay.Value = CDec(shift.HalfDayThresholdHours)
                chkActive.Checked = shift.IsActive
                txtDesc.Text = shift.Description
            Else
                Me.Text = "Create New Shift"
                dtpStart.Value = DateTime.Today.AddHours(8)
                dtpEnd.Value = DateTime.Today.AddHours(17)
                numGrace.Value = 15
                numBreak.Value = 60
                numHalfDay.Value = 4
                chkActive.Checked = True
            End If
        End Sub

        Private Sub InitializeComponent()
            Me.Size = New Size(480, 470)
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

            Dim lblName As New Label With {.Text = "Shift Name *", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            txtName = New TextBox With {.Location = New Point(190, y - 2), .Size = New Size(240, 26)}
            pnlContainer.Controls.AddRange(New Control() {lblName, txtName})

            y += gap
            Dim lblStart As New Label With {.Text = "Start Time *", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            dtpStart = New DateTimePicker With {.Location = New Point(190, y - 2), .Size = New Size(240, 26), .Format = DateTimePickerFormat.Time, .ShowUpDown = True}
            pnlContainer.Controls.AddRange(New Control() {lblStart, dtpStart})

            y += gap
            Dim lblEnd As New Label With {.Text = "End Time *", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            dtpEnd = New DateTimePicker With {.Location = New Point(190, y - 2), .Size = New Size(240, 26), .Format = DateTimePickerFormat.Time, .ShowUpDown = True}
            pnlContainer.Controls.AddRange(New Control() {lblEnd, dtpEnd})

            y += gap
            Dim lblGrace As New Label With {.Text = "Grace Period (Minutes)", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            numGrace = New NumericUpDown With {.Location = New Point(190, y - 2), .Size = New Size(240, 26), .Minimum = 0, .Maximum = 120, .Value = 15}
            pnlContainer.Controls.AddRange(New Control() {lblGrace, numGrace})

            y += gap
            Dim lblBreak As New Label With {.Text = "Break (Minutes)", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            numBreak = New NumericUpDown With {.Location = New Point(190, y - 2), .Size = New Size(240, 26), .Minimum = 0, .Maximum = 180, .Value = 60}
            pnlContainer.Controls.AddRange(New Control() {lblBreak, numBreak})

            y += gap
            Dim lblHalf As New Label With {.Text = "Half Day Min (Hours)", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            numHalfDay = New NumericUpDown With {.Location = New Point(190, y - 2), .Size = New Size(240, 26), .DecimalPlaces = 1, .Minimum = 1, .Maximum = 12, .Value = 4}
            pnlContainer.Controls.AddRange(New Control() {lblHalf, numHalfDay})

            y += gap
            Dim lblAct As New Label With {.Text = "Is Active", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            chkActive = New CheckBox With {.Location = New Point(190, y - 2), .Size = New Size(30, 24), .Checked = True}
            pnlContainer.Controls.AddRange(New Control() {lblAct, chkActive})

            y += 40
            lblError = New Label With {.Location = New Point(24, y), .Size = New Size(400, 22), .ForeColor = UITheme.StatusAbsent, .Font = UITheme.FontSmall}
            pnlContainer.Controls.Add(lblError)

            y += 28
            btnSave = New Button With {.Text = "Save Shift", .Location = New Point(222, y), .Size = New Size(110, 32)}
            UITheme.ApplyButtonStyle(btnSave, True)
            AddHandler btnSave.Click, AddressOf BtnSave_Click

            btnCancel = New Button With {.Text = "Cancel", .Location = New Point(342, y), .Size = New Size(88, 32)}
            UITheme.ApplyButtonStyle(btnCancel, False)
            AddHandler btnCancel.Click, Sub() Me.Close()

            pnlContainer.Controls.AddRange(New Control() {btnSave, btnCancel})

            Me.Controls.Add(pnlContainer)
            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            lblError.Text = ""
            Dim name = txtName.Text.Trim()
            If String.IsNullOrWhiteSpace(name) Then
                lblError.Text = "Shift name is required."
                Return
            End If

            Dim shift As New Shift With {
                .Id = _shiftId,
                .Name = name,
                .StartTime = dtpStart.Value.TimeOfDay,
                .EndTime = dtpEnd.Value.TimeOfDay,
                .GracePeriodMinutes = CInt(numGrace.Value),
                .BreakDurationMinutes = CInt(numBreak.Value),
                .HalfDayThresholdHours = CDbl(numHalfDay.Value),
                .IsActive = chkActive.Checked,
                .Description = ""
            }

            If _shiftId = 0 Then
                _shiftRepo.Insert(shift)
            Else
                _shiftRepo.Update(shift)
            End If

            Me.DialogResult = DialogResult.OK
            Me.Close()
        End Sub
    End Class

    Public Class ShiftsView
        Inherits UserControl

        Private dgvShifts As DataGridView
        Private btnAdd As Button
        Private btnEdit As Button
        Private btnDelete As Button
        Private ReadOnly _shiftRepo As New ShiftRepository()

        Public Sub New()
            InitializeComponent()
            LoadShifts()
        End Sub

        Private Sub InitializeComponent()
            Me.Dock = DockStyle.Fill
            Me.BackColor = UITheme.BackgroundColor
            Me.Font = UITheme.FontBody
            Me.Padding = New Padding(16)

            Dim pnlTop As New Panel With {.Dock = DockStyle.Top, .Height = 44, .BackColor = Color.Transparent}
            Dim lblTitle As New Label With {.Text = "Shift Management & Working Schedules", .Font = UITheme.FontPageTitle, .ForeColor = UITheme.TextPrimary, .Dock = DockStyle.Left, .AutoSize = True, .TextAlign = ContentAlignment.MiddleLeft}

            Dim pnlActions As New FlowLayoutPanel With {.Dock = DockStyle.Right, .Width = 280, .Height = 36}

            btnAdd = New Button With {.Text = "Add Shift", .Width = 90}
            UITheme.ApplyButtonStyle(btnAdd, True)
            AddHandler btnAdd.Click, AddressOf BtnAdd_Click

            btnEdit = New Button With {.Text = "Edit", .Width = 70}
            UITheme.ApplyButtonStyle(btnEdit, False)
            AddHandler btnEdit.Click, AddressOf BtnEdit_Click

            btnDelete = New Button With {.Text = "Delete", .Width = 70}
            UITheme.ApplyButtonStyle(btnDelete, False)
            AddHandler btnDelete.Click, AddressOf BtnDelete_Click

            pnlActions.Controls.AddRange(New Control() {btnAdd, btnEdit, btnDelete})
            pnlTop.Controls.AddRange(New Control() {pnlActions, lblTitle})

            Dim pnlGrid As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(0, 8, 0, 0)}
            dgvShifts = New DataGridView With {.Dock = DockStyle.Fill}
            UITheme.ApplyGridStyle(dgvShifts)

            dgvShifts.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Name", .HeaderText = "Shift Name", .Width = 180})
            dgvShifts.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "StartTime", .HeaderText = "Start Time", .Width = 110})
            dgvShifts.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "EndTime", .HeaderText = "End Time", .Width = 110})
            dgvShifts.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "GracePeriod", .HeaderText = "Grace (Min)", .Width = 110})
            dgvShifts.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "BreakDuration", .HeaderText = "Break (Min)", .Width = 110})
            dgvShifts.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "HalfDayThreshold", .HeaderText = "Half Day Min (Hrs)", .Width = 140})
            dgvShifts.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "IsActive", .HeaderText = "Status", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            AddHandler dgvShifts.CellDoubleClick, Sub() BtnEdit_Click(Nothing, Nothing)

            pnlGrid.Controls.Add(dgvShifts)

            Me.Controls.Add(pnlGrid)
            Me.Controls.Add(pnlTop)
        End Sub

        Public Sub LoadShifts()
            Dim list = _shiftRepo.GetAll()
            dgvShifts.Rows.Clear()
            For Each s In list
                Dim rowIndex = dgvShifts.Rows.Add(s.Name, s.StartTime.ToString("hh\:mm"), s.EndTime.ToString("hh\:mm"), s.GracePeriodMinutes, s.BreakDurationMinutes, s.HalfDayThresholdHours, If(s.IsActive, "Active", "Inactive"))
                dgvShifts.Rows(rowIndex).Tag = s
            Next
        End Sub

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            Using frm As New FormShiftEdit()
                If frm.ShowDialog(Me) = DialogResult.OK Then
                    LoadShifts()
                End If
            End Using
        End Sub

        Private Sub BtnEdit_Click(sender As Object, e As EventArgs)
            If dgvShifts.SelectedRows.Count = 0 Then Return
            Dim s = CType(dgvShifts.SelectedRows(0).Tag, Shift)
            If s IsNot Nothing Then
                Using frm As New FormShiftEdit(s)
                    If frm.ShowDialog(Me) = DialogResult.OK Then
                        LoadShifts()
                    End If
                End Using
            End If
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
            If dgvShifts.SelectedRows.Count = 0 Then Return
            Dim s = CType(dgvShifts.SelectedRows(0).Tag, Shift)
            If s IsNot Nothing Then
                Dim confirm = MessageBox.Show($"Delete shift '{s.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If confirm = DialogResult.Yes Then
                    _shiftRepo.Delete(s.Id)
                    LoadShifts()
                End If
            End If
        End Sub
    End Class
End Namespace
