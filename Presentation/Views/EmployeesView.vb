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
    Public Class EmployeesView
        Inherits UserControl

        Private dgvEmployees As DataGridView
        Private txtSearch As TextBox
        Private cmbFilterDept As ComboBox
        Private cmbFilterStatus As ComboBox
        Private btnAdd As Button
        Private btnEdit As Button
        Private btnDelete As Button
        Private btnExport As Button
        Private lblRecordCount As Label

        Private ReadOnly _empService As New EmployeeService()
        Private ReadOnly _deptRepo As New DepartmentRepository()
        Private _employeeList As New List(Of Employee)

        Public Sub New()
            InitializeComponent()
            LoadFilters()
            LoadEmployeeData()
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
                .Text = "Employee Directory & Records",
                .Font = UITheme.FontPageTitle,
                .ForeColor = UITheme.TextPrimary,
                .Dock = DockStyle.Left,
                .AutoSize = True,
                .TextAlign = ContentAlignment.MiddleLeft
            }

            Dim pnlActions As New FlowLayoutPanel With {
                .Dock = DockStyle.Right,
                .FlowDirection = FlowDirection.LeftToRight,
                .Width = 430,
                .Height = 36
            }

            btnAdd = New Button With {.Text = "Add Employee", .Width = 115}
            UITheme.ApplyButtonStyle(btnAdd, True)
            AddHandler btnAdd.Click, AddressOf BtnAdd_Click

            btnEdit = New Button With {.Text = "Edit", .Width = 70}
            UITheme.ApplyButtonStyle(btnEdit, False)
            AddHandler btnEdit.Click, AddressOf BtnEdit_Click

            btnDelete = New Button With {.Text = "Delete", .Width = 70}
            UITheme.ApplyButtonStyle(btnDelete, False)
            AddHandler btnDelete.Click, AddressOf BtnDelete_Click

            btnExport = New Button With {.Text = "Export CSV", .Width = 95}
            UITheme.ApplyButtonStyle(btnExport, False)
            AddHandler btnExport.Click, Sub() CsvExporter.ExportToCsv(dgvEmployees, "Employees_List")

            pnlActions.Controls.AddRange(New Control() {btnAdd, btnEdit, btnDelete, btnExport})

            pnlTop.Controls.Add(pnlActions)
            pnlTop.Controls.Add(lblTitle)

            Dim pnlFilter As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 46,
                .BackColor = UITheme.PanelBackground,
                .BorderStyle = BorderStyle.FixedSingle,
                .Padding = New Padding(10, 8, 10, 8),
                .Margin = New Padding(0, 8, 0, 8)
            }

            Dim lblSearch As New Label With {.Text = "Search:", .Location = New Point(10, 12), .AutoSize = True, .Font = UITheme.FontBodyBold}
            txtSearch = New TextBox With {.Location = New Point(68, 10), .Width = 200}
            AddHandler txtSearch.TextChanged, Sub() LoadEmployeeData()

            Dim lblDept As New Label With {.Text = "Department:", .Location = New Point(285, 12), .AutoSize = True, .Font = UITheme.FontBodyBold}
            cmbFilterDept = New ComboBox With {.Location = New Point(370, 10), .Width = 170, .DropDownStyle = ComboBoxStyle.DropDownList}
            AddHandler cmbFilterDept.SelectedIndexChanged, Sub() LoadEmployeeData()

            Dim lblStatus As New Label With {.Text = "Status:", .Location = New Point(555, 12), .AutoSize = True, .Font = UITheme.FontBodyBold}
            cmbFilterStatus = New ComboBox With {.Location = New Point(608, 10), .Width = 120, .DropDownStyle = ComboBoxStyle.DropDownList}
            AddHandler cmbFilterStatus.SelectedIndexChanged, Sub() LoadEmployeeData()

            lblRecordCount = New Label With {.Dock = DockStyle.Right, .Width = 150, .TextAlign = ContentAlignment.MiddleRight, .ForeColor = UITheme.TextSecondary}

            pnlFilter.Controls.AddRange(New Control() {lblSearch, txtSearch, lblDept, cmbFilterDept, lblStatus, cmbFilterStatus, lblRecordCount})

            Dim pnlGrid As New Panel With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(0, 8, 0, 0)
            }

            dgvEmployees = New DataGridView With {
                .Dock = DockStyle.Fill
            }
            UITheme.ApplyGridStyle(dgvEmployees)

            dgvEmployees.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "EmployeeCode", .HeaderText = "Employee ID", .Width = 110})
            dgvEmployees.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "FullName", .HeaderText = "Full Name", .Width = 160})
            dgvEmployees.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "DepartmentName", .HeaderText = "Department", .Width = 140})
            dgvEmployees.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "PositionTitle", .HeaderText = "Position", .Width = 150})
            dgvEmployees.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "ShiftName", .HeaderText = "Shift", .Width = 130})
            dgvEmployees.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Phone", .HeaderText = "Phone", .Width = 120})
            dgvEmployees.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Email", .HeaderText = "Email Address", .Width = 180})
            dgvEmployees.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "HireDate", .HeaderText = "Hire Date", .Width = 95})
            dgvEmployees.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Status", .HeaderText = "Status", .Width = 90})

            AddHandler dgvEmployees.CellDoubleClick, Sub() BtnEdit_Click(Nothing, Nothing)

            pnlGrid.Controls.Add(dgvEmployees)

            Me.Controls.Add(pnlGrid)
            Me.Controls.Add(pnlFilter)
            Me.Controls.Add(pnlTop)
        End Sub

        Private Sub LoadFilters()
            Dim depts = _deptRepo.GetAll()
            depts.Insert(0, New Department With {.Id = 0, .Name = "-- All Departments --"})
            cmbFilterDept.DataSource = depts
            cmbFilterDept.DisplayMember = "Name"
            cmbFilterDept.ValueMember = "Id"

            cmbFilterStatus.Items.Add("-- All Statuses --")
            For Each itemVal In [Enum].GetValues(GetType(EmployeeStatus))
                cmbFilterStatus.Items.Add(itemVal)
            Next
            cmbFilterStatus.SelectedIndex = 0
        End Sub

        Public Sub LoadEmployeeData()
            Try
                Dim search = txtSearch.Text.Trim()
                Dim deptId = If(cmbFilterDept.SelectedValue IsNot Nothing AndAlso TypeOf cmbFilterDept.SelectedValue Is Integer, CInt(cmbFilterDept.SelectedValue), 0)
                Dim status As Nullable(Of EmployeeStatus) = Nothing

                If cmbFilterStatus.SelectedIndex > 0 Then
                    status = CType(cmbFilterStatus.SelectedItem, EmployeeStatus)
                End If

                _employeeList = _empService.GetAll(search, deptId, status)
                dgvEmployees.Rows.Clear()

                For Each e In _employeeList
                    Dim rowIndex = dgvEmployees.Rows.Add(e.EmployeeCode, e.FullName, e.DepartmentName, e.PositionTitle, e.ShiftName, e.Phone, e.Email, e.HireDate.ToString("yyyy-MM-dd"), e.Status.ToString())
                    dgvEmployees.Rows(rowIndex).Tag = e

                    If e.Status <> EmployeeStatus.Active Then
                        dgvEmployees.Rows(rowIndex).DefaultCellStyle.ForeColor = UITheme.TextSecondary
                    End If
                Next

                lblRecordCount.Text = $"Total: {_employeeList.Count} Employees"
            Catch ex As Exception
                MessageBox.Show("Error loading employee data: " & ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            Using frm As New FormEmployeeEdit()
                If frm.ShowDialog(Me) = DialogResult.OK Then
                    LoadEmployeeData()
                End If
            End Using
        End Sub

        Private Sub BtnEdit_Click(sender As Object, e As EventArgs)
            If dgvEmployees.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select an employee to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim emp = CType(dgvEmployees.SelectedRows(0).Tag, Employee)
            If emp IsNot Nothing Then
                Using frm As New FormEmployeeEdit(emp)
                    If frm.ShowDialog(Me) = DialogResult.OK Then
                        LoadEmployeeData()
                    End If
                End Using
            End If
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
            If dgvEmployees.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select an employee to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim emp = CType(dgvEmployees.SelectedRows(0).Tag, Employee)
            If emp IsNot Nothing Then
                Dim confirm = MessageBox.Show($"Are you sure you want to permanently delete employee '{emp.FullName}' ({emp.EmployeeCode})?{Environment.NewLine}If the employee has attendance history, consider setting their status to Inactive instead.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If confirm = DialogResult.Yes Then
                    Dim res = _empService.DeleteEmployee(emp.Id)
                    If res.Success Then
                        LoadEmployeeData()
                    Else
                        MessageBox.Show(res.Message, "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    End If
                End If
            End If
        End Sub
    End Class
End Namespace
