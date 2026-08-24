Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Infrastructure.Security

Namespace Presentation.Views
    Public Class FormMain
        Inherits Form

        Private pnlSidebar As Panel
        Private pnlHeader As Panel
        Private pnlContent As Panel
        Private statusStrip As StatusStrip
        Private tslUser As ToolStripStatusLabel
        Private tslRole As ToolStripStatusLabel
        Private tslTime As ToolStripStatusLabel
        Private tslDbStatus As ToolStripStatusLabel

        Private btnNavDashboard As Button
        Private btnNavEmployees As Button
        Private btnNavAttendance As Button
        Private btnNavLeaves As Button
        Private btnNavShifts As Button
        Private btnNavHolidays As Button
        Private btnNavReports As Button
        Private btnNavSettings As Button
        Private btnLogout As Button
        Private _currentNavBtn As Button

        Private _viewDashboard As DashboardView
        Private _viewEmployees As EmployeesView
        Private _viewAttendance As AttendanceView
        Private _viewLeaves As LeavesView
        Private _viewShifts As ShiftsView
        Private _viewHolidays As HolidaysView
        Private _viewReports As ReportsView
        Private _viewSettings As SettingsView

        Private timerClock As Timer

        Public Sub New()
            InitializeComponent()
            InitializeViews()
            UpdateUserStatus()

            NavigateTo(btnNavDashboard, _viewDashboard)
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "Vertex Attendance Management System"
            Me.Size = New Size(1280, 800)
            Me.MinimumSize = New Size(1024, 680)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.BackColor = UITheme.BackgroundColor
            Me.Font = UITheme.FontBody

            pnlHeader = New Panel With {
                .Dock = DockStyle.Top,
                .Height = 52,
                .BackColor = UITheme.PrimaryColor,
                .Padding = New Padding(16, 10, 16, 10)
            }

            Dim lblLogoTitle As New Label With {
                .Text = "ATTENDANCE MANAGEMENT SYSTEM",
                .Font = UITheme.FontAppTitle,
                .ForeColor = Color.White,
                .Dock = DockStyle.Left,
                .AutoSize = True,
                .TextAlign = ContentAlignment.MiddleLeft
            }

            Dim lblEdition As New Label With {
                .Text = "ENTERPRISE DESKTOP EDITION",
                .Font = UITheme.FontSmall,
                .ForeColor = Color.FromArgb(190, 210, 235),
                .Dock = DockStyle.Right,
                .AutoSize = True,
                .TextAlign = ContentAlignment.MiddleRight
            }

            pnlHeader.Controls.Add(lblEdition)
            pnlHeader.Controls.Add(lblLogoTitle)

            pnlSidebar = New Panel With {
                .Dock = DockStyle.Left,
                .Width = 200,
                .BackColor = UITheme.SidebarBackground,
                .Padding = New Padding(0, 10, 0, 10)
            }

            Dim navFlow As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.TopDown,
                .WrapContents = False,
                .AutoScroll = False
            }

            btnNavDashboard = CreateNavButton("Dashboard")
            btnNavEmployees = CreateNavButton("Employees")
            btnNavAttendance = CreateNavButton("Attendance")
            btnNavLeaves = CreateNavButton("Leaves")
            btnNavShifts = CreateNavButton("Shifts")
            btnNavHolidays = CreateNavButton("Holidays")
            btnNavReports = CreateNavButton("Reports")
            btnNavSettings = CreateNavButton("Settings")

            AddHandler btnNavDashboard.Click, Sub() NavigateTo(btnNavDashboard, _viewDashboard)
            AddHandler btnNavEmployees.Click, Sub() NavigateTo(btnNavEmployees, _viewEmployees)
            AddHandler btnNavAttendance.Click, Sub() NavigateTo(btnNavAttendance, _viewAttendance)
            AddHandler btnNavLeaves.Click, Sub() NavigateTo(btnNavLeaves, _viewLeaves)
            AddHandler btnNavShifts.Click, Sub() NavigateTo(btnNavShifts, _viewShifts)
            AddHandler btnNavHolidays.Click, Sub() NavigateTo(btnNavHolidays, _viewHolidays)
            AddHandler btnNavReports.Click, Sub() NavigateTo(btnNavReports, _viewReports)
            AddHandler btnNavSettings.Click, Sub() NavigateTo(btnNavSettings, _viewSettings)

            navFlow.Controls.AddRange(New Control() {
                btnNavDashboard,
                btnNavEmployees,
                btnNavAttendance,
                btnNavLeaves,
                btnNavShifts,
                btnNavHolidays,
                btnNavReports,
                btnNavSettings
            })

            Dim pnlSidebarBottom As New Panel With {
                .Dock = DockStyle.Bottom,
                .Height = 50,
                .Padding = New Padding(12, 6, 12, 6)
            }

            btnLogout = New Button With {
                .Text = "Sign Out",
                .Dock = DockStyle.Fill,
                .FlatStyle = FlatStyle.Flat,
                .ForeColor = Color.White,
                .BackColor = Color.FromArgb(60, 68, 80),
                .Font = UITheme.FontBodyBold,
                .Cursor = Cursors.Hand
            }
            btnLogout.FlatAppearance.BorderSize = 0
            AddHandler btnLogout.Click, AddressOf BtnLogout_Click
            pnlSidebarBottom.Controls.Add(btnLogout)

            pnlSidebar.Controls.Add(navFlow)
            pnlSidebar.Controls.Add(pnlSidebarBottom)

            pnlContent = New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = UITheme.BackgroundColor
            }

            statusStrip = New StatusStrip With {
                .BackColor = Color.FromArgb(240, 243, 246),
                .Font = UITheme.FontSmall
            }

            tslUser = New ToolStripStatusLabel With {.Text = "User: Admin", .BorderSides = ToolStripStatusLabelBorderSides.Right, .Padding = New Padding(6, 0, 10, 0)}
            tslRole = New ToolStripStatusLabel With {.Text = "Role: Administrator", .BorderSides = ToolStripStatusLabelBorderSides.Right, .Padding = New Padding(6, 0, 10, 0)}
            tslDbStatus = New ToolStripStatusLabel With {.Text = "Database: SQLite Connected", .BorderSides = ToolStripStatusLabelBorderSides.Right, .Padding = New Padding(6, 0, 10, 0)}
            Dim tslCopyright As New ToolStripStatusLabel With {.Text = "© XREFS0. All Rights Reserved.", .BorderSides = ToolStripStatusLabelBorderSides.Right, .Padding = New Padding(6, 0, 10, 0), .ForeColor = UITheme.TextSecondary}
            tslTime = New ToolStripStatusLabel With {.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), .Spring = True, .TextAlign = ContentAlignment.MiddleRight}

            statusStrip.Items.AddRange(New ToolStripItem() {tslUser, tslRole, tslDbStatus, tslCopyright, tslTime})

            timerClock = New Timer With {.Interval = 1000, .Enabled = True}
            AddHandler timerClock.Tick, Sub() tslTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

            Me.Controls.Add(pnlContent)
            Me.Controls.Add(pnlSidebar)
            Me.Controls.Add(pnlHeader)
            Me.Controls.Add(statusStrip)
        End Sub

        Private Function CreateNavButton(text As String) As Button
            Dim btn As New Button With {
                .Text = "  " & text,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Width = 200,
                .Height = 44,
                .FlatStyle = FlatStyle.Flat,
                .ForeColor = UITheme.SidebarText,
                .BackColor = UITheme.SidebarBackground,
                .Font = UITheme.FontBodyBold,
                .Cursor = Cursors.Hand,
                .Margin = New Padding(0)
            }
            btn.FlatAppearance.BorderSize = 0
            Return btn
        End Function

        Private Sub InitializeViews()
            _viewDashboard = New DashboardView()
            _viewEmployees = New EmployeesView()
            _viewAttendance = New AttendanceView()
            _viewLeaves = New LeavesView()
            _viewShifts = New ShiftsView()
            _viewHolidays = New HolidaysView()
            _viewReports = New ReportsView()
            _viewSettings = New SettingsView()
        End Sub

        Private Sub NavigateTo(btn As Button, view As UserControl)
            If _currentNavBtn IsNot Nothing Then
                _currentNavBtn.BackColor = UITheme.SidebarBackground
                _currentNavBtn.ForeColor = UITheme.SidebarText
            End If

            _currentNavBtn = btn
            _currentNavBtn.BackColor = UITheme.SidebarSelected
            _currentNavBtn.ForeColor = Color.White

            pnlContent.Controls.Clear()
            pnlContent.Controls.Add(view)

            If TypeOf view Is DashboardView Then
                CType(view, DashboardView).LoadDashboardData()
            ElseIf TypeOf view Is AttendanceView Then
                CType(view, AttendanceView).LoadAttendanceData()
            ElseIf TypeOf view Is EmployeesView Then
                CType(view, EmployeesView).LoadEmployeeData()
            ElseIf TypeOf view Is LeavesView Then
                CType(view, LeavesView).LoadLeaves()
            ElseIf TypeOf view Is ShiftsView Then
                CType(view, ShiftsView).LoadShifts()
            ElseIf TypeOf view Is HolidaysView Then
                CType(view, HolidaysView).LoadHolidays()
            ElseIf TypeOf view Is ReportsView Then
                CType(view, ReportsView).GenerateReport()
            End If
        End Sub

        Private Sub UpdateUserStatus()
            If SessionManager.CurrentUser IsNot Nothing Then
                tslUser.Text = "User: " & SessionManager.CurrentUser.Username
                tslRole.Text = "Role: " & SessionManager.CurrentUser.Role.ToString()
            End If
        End Sub

        Private Sub BtnLogout_Click(sender As Object, e As EventArgs)
            Dim confirm = MessageBox.Show("Are you sure you want to sign out?", "Confirm Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If confirm = DialogResult.Yes Then
                SessionManager.Logout()
                Me.Hide()
                Using frmLogin As New FormLogin()
                    If frmLogin.ShowDialog() = DialogResult.OK Then
                        UpdateUserStatus()
                        NavigateTo(btnNavDashboard, _viewDashboard)
                        Me.Show()
                    Else
                        System.Windows.Forms.Application.Exit()
                    End If
                End Using
            End If
        End Sub
    End Class
End Namespace
