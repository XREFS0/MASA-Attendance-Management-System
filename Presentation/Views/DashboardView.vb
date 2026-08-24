Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Application.Services
Imports AttendanceSystem.Presentation.Controls
Imports AttendanceSystem.Infrastructure.Repositories
Imports AttendanceSystem.Infrastructure.Security

Namespace Presentation.Views
    Public Class DashboardView
        Inherits UserControl

        Private cardTotal As MetricCard
        Private cardPresent As MetricCard
        Private cardLate As MetricCard
        Private cardAbsent As MetricCard
        Private cardLeave As MetricCard
        Private cardRate As MetricCard

        Private dgvRecentActivity As DataGridView
        Private dgvDepartmentBreakdown As DataGridView

        Private ReadOnly _dashService As New DashboardService()
        Private ReadOnly _attendanceRepo As New AttendanceRepository()
        Private ReadOnly _auditRepo As New AuditRepository()

        Public Sub New()
            InitializeComponent()
            LoadDashboardData()
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
                .Text = "Dashboard & Real-time Overview",
                .Font = UITheme.FontPageTitle,
                .ForeColor = UITheme.TextPrimary,
                .Dock = DockStyle.Left,
                .AutoSize = True,
                .TextAlign = ContentAlignment.MiddleLeft
            }

            Dim btnRefresh As New Button With {
                .Text = "Refresh Overview",
                .Dock = DockStyle.Right,
                .Width = 130
            }
            UITheme.ApplyButtonStyle(btnRefresh, False)
            AddHandler btnRefresh.Click, Sub() LoadDashboardData()

            pnlTop.Controls.Add(btnRefresh)
            pnlTop.Controls.Add(lblTitle)

            Dim pnlCards As New FlowLayoutPanel With {
                .Dock = DockStyle.Top,
                .Height = 85,
                .BackColor = Color.Transparent,
                .Padding = New Padding(0, 4, 0, 8),
                .AutoScroll = False
            }

            cardTotal = New MetricCard("Total Staff", "0")
            cardPresent = New MetricCard("Present Today", "0", UITheme.StatusPresent)
            cardLate = New MetricCard("Late Today", "0", UITheme.StatusLate)
            cardAbsent = New MetricCard("Absent Today", "0", UITheme.StatusAbsent)
            cardLeave = New MetricCard("On Leave", "0", UITheme.StatusLeave)
            cardRate = New MetricCard("Attendance Rate", "0%", UITheme.PrimaryColor)

            pnlCards.Controls.AddRange(New Control() {cardTotal, cardPresent, cardLate, cardAbsent, cardLeave, cardRate})

            Dim tableMain As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .RowCount = 1,
                .ColumnCount = 2,
                .Padding = New Padding(0, 8, 0, 0)
            }
            tableMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 60.0F))
            tableMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 40.0F))

            Dim pnlLeft As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = UITheme.PanelBackground,
                .BorderStyle = BorderStyle.FixedSingle,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 0, 8, 0)
            }

            Dim headerRecent As New SectionHeader With {
                .TitleText = "Today's Attendance Logs",
                .Dock = DockStyle.Top
            }

            dgvRecentActivity = New DataGridView With {
                .Dock = DockStyle.Fill
            }
            UITheme.ApplyGridStyle(dgvRecentActivity)

            dgvRecentActivity.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "EmployeeCode", .HeaderText = "Emp ID", .Width = 90})
            dgvRecentActivity.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "EmployeeName", .HeaderText = "Employee Name", .Width = 140})
            dgvRecentActivity.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "DepartmentName", .HeaderText = "Department", .Width = 110})
            dgvRecentActivity.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "CheckIn", .HeaderText = "Check In", .Width = 85})
            dgvRecentActivity.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "CheckOut", .HeaderText = "Check Out", .Width = 85})
            dgvRecentActivity.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Status", .HeaderText = "Status", .Width = 90})

            pnlLeft.Controls.Add(dgvRecentActivity)
            pnlLeft.Controls.Add(headerRecent)

            Dim pnlRight As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = UITheme.PanelBackground,
                .BorderStyle = BorderStyle.FixedSingle,
                .Padding = New Padding(12),
                .Margin = New Padding(8, 0, 0, 0)
            }

            Dim headerAudit As New SectionHeader With {
                .TitleText = "Recent System Audit Trail",
                .Dock = DockStyle.Top
            }

            dgvDepartmentBreakdown = New DataGridView With {
                .Dock = DockStyle.Fill
            }
            UITheme.ApplyGridStyle(dgvDepartmentBreakdown)

            dgvDepartmentBreakdown.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Timestamp", .HeaderText = "Time", .Width = 115})
            dgvDepartmentBreakdown.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Username", .HeaderText = "User", .Width = 85})
            dgvDepartmentBreakdown.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Action", .HeaderText = "Action", .Width = 95})
            dgvDepartmentBreakdown.Columns.Add(New DataGridViewTextBoxColumn With {.DataPropertyName = "Details", .HeaderText = "Details", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})

            pnlRight.Controls.Add(dgvDepartmentBreakdown)
            pnlRight.Controls.Add(headerAudit)

            tableMain.Controls.Add(pnlLeft, 0, 0)
            tableMain.Controls.Add(pnlRight, 1, 0)

            Me.Controls.Add(tableMain)
            Me.Controls.Add(pnlCards)
            Me.Controls.Add(pnlTop)
        End Sub

        Public Sub LoadDashboardData()
            Try
                Dim stats = _dashService.GetDashboardStats()
                cardTotal.UpdateValue(stats.TotalEmployees.ToString())
                cardPresent.UpdateValue(stats.PresentToday.ToString())
                cardLate.UpdateValue(stats.LateToday.ToString())
                cardAbsent.UpdateValue(stats.AbsentToday.ToString())
                cardLeave.UpdateValue(stats.OnLeaveToday.ToString())
                cardRate.UpdateValue(stats.AttendanceRate.ToString("0.0") & "%")

                Dim today = DateTime.Today
                Dim records = _attendanceRepo.GetRecords(today, today)

                dgvRecentActivity.Rows.Clear()
                For Each r In records
                    Dim cinStr = If(r.CheckIn.HasValue, r.CheckIn.Value.ToString("HH:mm"), "--:--")
                    Dim coutStr = If(r.CheckOut.HasValue, r.CheckOut.Value.ToString("HH:mm"), "--:--")
                    Dim rowIndex = dgvRecentActivity.Rows.Add(r.EmployeeCode, r.EmployeeName, r.DepartmentName, cinStr, coutStr, r.Status.ToString())

                    Dim cell = dgvRecentActivity.Rows(rowIndex).Cells(5)
                    Select Case r.Status
                        Case Domain.Enums.AttendanceStatus.Present
                            cell.Style.ForeColor = UITheme.StatusPresent
                        Case Domain.Enums.AttendanceStatus.Late
                            cell.Style.ForeColor = UITheme.StatusLate
                        Case Domain.Enums.AttendanceStatus.Absent
                            cell.Style.ForeColor = UITheme.StatusAbsent
                        Case Domain.Enums.AttendanceStatus.OnLeave
                            cell.Style.ForeColor = UITheme.StatusLeave
                    End Select
                Next

                Dim auditLogs = _auditRepo.GetRecent(25)
                dgvDepartmentBreakdown.Rows.Clear()
                For Each log In auditLogs
                    dgvDepartmentBreakdown.Rows.Add(log.Timestamp.ToString("yyyy-MM-dd HH:mm"), log.Username, log.Action, log.Details)
                Next
            Catch ex As Exception
                MessageBox.Show("Error loading dashboard data: " & ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
    End Class
End Namespace
