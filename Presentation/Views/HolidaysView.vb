Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Infrastructure.Repositories

Namespace Presentation.Views
    Public Class FormHolidayEdit
        Inherits Form

        Private txtName As TextBox
        Private dtpDate As DateTimePicker
        Private chkRecurring As CheckBox
        Private txtDescription As TextBox
        Private lblError As Label
        Private btnSave As Button
        Private btnCancel As Button

        Private ReadOnly _holidayRepo As New HolidayRepository()

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "Add Official Holiday"
            Me.Size = New Size(440, 360)
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

            Dim lblName As New Label With {.Text = "Holiday Title *", .Location = New Point(24, y), .Size = New Size(130, 18), .Font = UITheme.FontBodyBold}
            txtName = New TextBox With {.Location = New Point(160, y - 2), .Size = New Size(230, 26)}
            pnlContainer.Controls.AddRange(New Control() {lblName, txtName})

            y += gap
            Dim lblDate As New Label With {.Text = "Date *", .Location = New Point(24, y), .Size = New Size(130, 18), .Font = UITheme.FontBodyBold}
            dtpDate = New DateTimePicker With {.Location = New Point(160, y - 2), .Size = New Size(230, 26), .Format = DateTimePickerFormat.Short}
            pnlContainer.Controls.AddRange(New Control() {lblDate, dtpDate})

            y += gap
            Dim lblRec As New Label With {.Text = "Recurs Yearly", .Location = New Point(24, y), .Size = New Size(130, 18), .Font = UITheme.FontBodyBold}
            chkRecurring = New CheckBox With {.Location = New Point(160, y - 2), .Size = New Size(230, 24), .Text = "Annual recurring holiday"}
            pnlContainer.Controls.AddRange(New Control() {lblRec, chkRecurring})

            y += gap
            Dim lblDesc As New Label With {.Text = "Description", .Location = New Point(24, y), .Size = New Size(130, 18), .Font = UITheme.FontBodyBold}
            txtDescription = New TextBox With {.Location = New Point(160, y - 2), .Size = New Size(230, 42), .Multiline = True}
            pnlContainer.Controls.AddRange(New Control() {lblDesc, txtDescription})

            y += 50
            lblError = New Label With {.Location = New Point(24, y), .Size = New Size(370, 22), .ForeColor = UITheme.StatusAbsent, .Font = UITheme.FontSmall}
            pnlContainer.Controls.Add(lblError)

            y += 24
            btnSave = New Button With {.Text = "Save Holiday", .Location = New Point(184, y), .Size = New Size(110, 32)}
            UITheme.ApplyButtonStyle(btnSave, True)
            AddHandler btnSave.Click, AddressOf BtnSave_Click

            btnCancel = New Button With {.Text = "Cancel", .Location = New Point(302, y), .Size = New Size(88, 32)}
            UITheme.ApplyButtonStyle(btnCancel, False)
            AddHandler btnCancel.Click, Sub() Me.Close()

            pnlContainer.Controls.AddRange(New Control() {btnSave, btnCancel})

            Me.Controls.Add(pnlContainer)
            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            lblError.Text = ""
            Dim title = txtName.Text.Trim()
            If String.IsNullOrWhiteSpace(title) Then
                lblError.Text = "Please enter a holiday title."
                Return
            End If

            Dim h As New Holiday With {
                .Name = title,
                .HolidayDate = dtpDate.Value.Date,
                .IsRecurring = chkRecurring.Checked,
                .Description = txtDescription.Text.Trim()
            }

            Try
                _holidayRepo.Insert(h)
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Catch ex As Exception
                lblError.Text = "A holiday already exists on this date."
            End Try
        End Sub
    End Class

    Public Class HolidaysView
        Inherits UserControl

        Private dgvHolidays As DataGridView
        Private btnAdd As Button
        Private btnDelete As Button
        Private ReadOnly _holidayRepo As New HolidayRepository()

        Public Sub New()
            InitializeComponent()
            LoadHolidays()
        End Sub

        Private Sub InitializeComponent()
            Me.Dock = DockStyle.Fill
            Me.BackColor = UITheme.BackgroundColor
            Me.Font = UITheme.FontBody
            Me.Padding = New Padding(16)

            Dim pnlTop As New Panel With {.Dock = DockStyle.Top, .Height = 44, .BackColor = Color.Transparent}
            Dim lblTitle As New Label With {.Text = "Official Holidays & Calendar Management", .Font = UITheme.FontPageTitle, .ForeColor = UITheme.TextPrimary, .Dock = DockStyle.Left, .AutoSize = True, .TextAlign = ContentAlignment.MiddleLeft}

            Dim pnlActions As New FlowLayoutPanel With {.Dock = DockStyle.Right, .Width = 200, .Height = 36}

            btnAdd = New Button With {.Text = "Add Holiday", .Width = 105}
            UITheme.ApplyButtonStyle(btnAdd, True)
            AddHandler btnAdd.Click, AddressOf BtnAdd_Click

            btnDelete = New Button With {.Text = "Delete", .Width = 70}
            UITheme.ApplyButtonStyle(btnDelete, False)
            AddHandler btnDelete.Click, AddressOf BtnDelete_Click

            pnlActions.Controls.AddRange(New Control() {btnAdd, btnDelete})
            pnlTop.Controls.AddRange(New Control() {pnlActions, lblTitle})

            Dim pnlGrid As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(0, 8, 0, 0)}
            dgvHolidays = New DataGridView With {.Dock = DockStyle.Fill}
            UITheme.ApplyGridStyle(dgvHolidays)

            dgvHolidays.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "HolidayDate", .HeaderText = "Date", .Width = 120})
            dgvHolidays.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Name", .HeaderText = "Holiday Title", .Width = 220})
            dgvHolidays.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "IsRecurring", .HeaderText = "Recurrence", .Width = 130})
            dgvHolidays.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Description", .HeaderText = "Description", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            pnlGrid.Controls.Add(dgvHolidays)

            Me.Controls.Add(pnlGrid)
            Me.Controls.Add(pnlTop)
        End Sub

        Public Sub LoadHolidays()
            Dim list = _holidayRepo.GetAll()
            dgvHolidays.Rows.Clear()
            For Each h In list
                Dim rowIndex = dgvHolidays.Rows.Add(h.HolidayDate.ToString("yyyy-MM-dd"), h.Name, If(h.IsRecurring, "Every Year", "Single Occurrence"), h.Description)
                dgvHolidays.Rows(rowIndex).Tag = h
            Next
        End Sub

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            Using frm As New FormHolidayEdit()
                If frm.ShowDialog(Me) = DialogResult.OK Then
                    LoadHolidays()
                End If
            End Using
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
            If dgvHolidays.SelectedRows.Count = 0 Then Return
            Dim h = CType(dgvHolidays.SelectedRows(0).Tag, Holiday)
            If h IsNot Nothing Then
                Dim confirm = MessageBox.Show($"Delete holiday '{h.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If confirm = DialogResult.Yes Then
                    _holidayRepo.Delete(h.Id)
                    LoadHolidays()
                End If
            End If
        End Sub
    End Class
End Namespace
