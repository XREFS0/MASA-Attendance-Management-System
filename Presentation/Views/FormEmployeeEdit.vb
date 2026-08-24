Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Domain.Entities
Imports AttendanceSystem.Domain.Enums
Imports AttendanceSystem.Infrastructure.Repositories
Imports AttendanceSystem.Application.Services

Namespace Presentation.Views
    Public Class FormEmployeeEdit
        Inherits Form

        Public Property SavedEmployee As Employee

        Private txtCode As TextBox
        Private txtFullName As TextBox
        Private cmbDepartment As ComboBox
        Private cmbPosition As ComboBox
        Private cmbShift As ComboBox
        Private txtPhone As TextBox
        Private txtEmail As TextBox
        Private dtpHireDate As DateTimePicker
        Private cmbStatus As ComboBox
        Private txtNotes As TextBox
        Private lblError As Label
        Private btnSave As Button
        Private btnCancel As Button

        Private ReadOnly _empService As New EmployeeService()
        Private ReadOnly _deptRepo As New DepartmentRepository()
        Private ReadOnly _posRepo As New PositionRepository()
        Private ReadOnly _shiftRepo As New ShiftRepository()
        Private _currentEmpId As Integer = 0

        Public Sub New(Optional employee As Employee = Nothing)
            InitializeComponent()
            LoadDropdowns()

            If employee IsNot Nothing Then
                _currentEmpId = employee.Id
                Me.Text = "Edit Employee — " & employee.EmployeeCode
                txtCode.Text = employee.EmployeeCode
                txtFullName.Text = employee.FullName
                cmbDepartment.SelectedValue = employee.DepartmentId
                LoadPositionsForDept(employee.DepartmentId)
                cmbPosition.SelectedValue = employee.PositionId
                cmbShift.SelectedValue = employee.ShiftId
                txtPhone.Text = employee.Phone
                txtEmail.Text = employee.Email
                dtpHireDate.Value = employee.HireDate
                cmbStatus.SelectedItem = employee.Status
                txtNotes.Text = employee.Notes
            Else
                Me.Text = "Add New Employee"
                Dim count = _empService.GetAll().Count
                txtCode.Text = "EMP-" & (1001 + count).ToString()
                cmbStatus.SelectedItem = EmployeeStatus.Active
                dtpHireDate.Value = DateTime.Today
            End If
        End Sub

        Private Sub InitializeComponent()
            Me.Size = New Size(540, 560)
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

            Dim y = 14
            Dim gap = 46

            Dim lblCode As New Label With {.Text = "Employee ID / Code *", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            txtCode = New TextBox With {.Location = New Point(190, y - 2), .Size = New Size(300, 26)}
            pnlContainer.Controls.AddRange(New Control() {lblCode, txtCode})

            y += gap
            Dim lblName As New Label With {.Text = "Full Name *", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            txtFullName = New TextBox With {.Location = New Point(190, y - 2), .Size = New Size(300, 26)}
            pnlContainer.Controls.AddRange(New Control() {lblName, txtFullName})

            y += gap
            Dim lblDept As New Label With {.Text = "Department *", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            cmbDepartment = New ComboBox With {.Location = New Point(190, y - 2), .Size = New Size(300, 26), .DropDownStyle = ComboBoxStyle.DropDownList}
            AddHandler cmbDepartment.SelectedIndexChanged, AddressOf CmbDepartment_SelectedIndexChanged
            pnlContainer.Controls.AddRange(New Control() {lblDept, cmbDepartment})

            y += gap
            Dim lblPos As New Label With {.Text = "Position *", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            cmbPosition = New ComboBox With {.Location = New Point(190, y - 2), .Size = New Size(300, 26), .DropDownStyle = ComboBoxStyle.DropDownList}
            pnlContainer.Controls.AddRange(New Control() {lblPos, cmbPosition})

            y += gap
            Dim lblShift As New Label With {.Text = "Assigned Shift *", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            cmbShift = New ComboBox With {.Location = New Point(190, y - 2), .Size = New Size(300, 26), .DropDownStyle = ComboBoxStyle.DropDownList}
            pnlContainer.Controls.AddRange(New Control() {lblShift, cmbShift})

            y += gap
            Dim lblPhone As New Label With {.Text = "Phone Number", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            txtPhone = New TextBox With {.Location = New Point(190, y - 2), .Size = New Size(300, 26)}
            pnlContainer.Controls.AddRange(New Control() {lblPhone, txtPhone})

            y += gap
            Dim lblEmail As New Label With {.Text = "Email Address", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            txtEmail = New TextBox With {.Location = New Point(190, y - 2), .Size = New Size(300, 26)}
            pnlContainer.Controls.AddRange(New Control() {lblEmail, txtEmail})

            y += gap
            Dim lblHire As New Label With {.Text = "Hire Date *", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            dtpHireDate = New DateTimePicker With {.Location = New Point(190, y - 2), .Size = New Size(300, 26), .Format = DateTimePickerFormat.Short}
            pnlContainer.Controls.AddRange(New Control() {lblHire, dtpHireDate})

            y += gap
            Dim lblStatus As New Label With {.Text = "Employment Status", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            cmbStatus = New ComboBox With {.Location = New Point(190, y - 2), .Size = New Size(300, 26), .DropDownStyle = ComboBoxStyle.DropDownList}
            pnlContainer.Controls.AddRange(New Control() {lblStatus, cmbStatus})

            y += gap
            Dim lblNotes As New Label With {.Text = "Notes / Remarks", .Location = New Point(24, y), .Size = New Size(160, 18), .Font = UITheme.FontBodyBold}
            txtNotes = New TextBox With {.Location = New Point(190, y - 2), .Size = New Size(300, 42), .Multiline = True}
            pnlContainer.Controls.AddRange(New Control() {lblNotes, txtNotes})

            y += 50
            lblError = New Label With {.Location = New Point(24, y), .Size = New Size(466, 24), .ForeColor = UITheme.StatusAbsent, .Font = UITheme.FontSmall}
            pnlContainer.Controls.Add(lblError)

            y += 28
            btnSave = New Button With {.Text = "Save Employee", .Location = New Point(276, y), .Size = New Size(120, 32)}
            UITheme.ApplyButtonStyle(btnSave, True)
            AddHandler btnSave.Click, AddressOf BtnSave_Click

            btnCancel = New Button With {.Text = "Cancel", .Location = New Point(402, y), .Size = New Size(88, 32)}
            UITheme.ApplyButtonStyle(btnCancel, False)
            AddHandler btnCancel.Click, Sub() Me.Close()

            pnlContainer.Controls.AddRange(New Control() {btnSave, btnCancel})

            Me.Controls.Add(pnlContainer)
            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel
        End Sub

        Private Sub LoadDropdowns()
            Dim depts = _deptRepo.GetAll().Where(Function(d) d.IsActive).ToList()
            cmbDepartment.DataSource = depts
            cmbDepartment.DisplayMember = "Name"
            cmbDepartment.ValueMember = "Id"

            Dim shifts = _shiftRepo.GetAll().Where(Function(s) s.IsActive).ToList()
            cmbShift.DataSource = shifts
            cmbShift.DisplayMember = "DisplayTime"
            cmbShift.ValueMember = "Id"

            cmbStatus.DataSource = [Enum].GetValues(GetType(EmployeeStatus))

            If depts.Count > 0 Then
                LoadPositionsForDept(depts(0).Id)
            End If
        End Sub

        Private Sub CmbDepartment_SelectedIndexChanged(sender As Object, e As EventArgs)
            If cmbDepartment.SelectedValue IsNot Nothing AndAlso TypeOf cmbDepartment.SelectedValue Is Integer Then
                LoadPositionsForDept(CInt(cmbDepartment.SelectedValue))
            End If
        End Sub

        Private Sub LoadPositionsForDept(deptId As Integer)
            Dim positions = _posRepo.GetByDepartment(deptId)
            cmbPosition.DataSource = positions
            cmbPosition.DisplayMember = "Title"
            cmbPosition.ValueMember = "Id"
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            lblError.Text = ""

            Dim emp As New Employee With {
                .Id = _currentEmpId,
                .EmployeeCode = txtCode.Text.Trim(),
                .FullName = txtFullName.Text.Trim(),
                .DepartmentId = If(cmbDepartment.SelectedValue IsNot Nothing, CInt(cmbDepartment.SelectedValue), 0),
                .PositionId = If(cmbPosition.SelectedValue IsNot Nothing, CInt(cmbPosition.SelectedValue), 0),
                .ShiftId = If(cmbShift.SelectedValue IsNot Nothing, CInt(cmbShift.SelectedValue), 0),
                .Phone = txtPhone.Text.Trim(),
                .Email = txtEmail.Text.Trim(),
                .HireDate = dtpHireDate.Value.Date,
                .Status = CType(cmbStatus.SelectedItem, EmployeeStatus),
                .Notes = txtNotes.Text.Trim()
            }

            Dim res = _empService.SaveEmployee(emp)
            If res.Success Then
                SavedEmployee = emp
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                lblError.Text = res.Message
            End If
        End Sub
    End Class
End Namespace
