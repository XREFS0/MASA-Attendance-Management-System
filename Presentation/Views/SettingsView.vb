Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Infrastructure.Repositories
Imports AttendanceSystem.Application.Services
Imports AttendanceSystem.Infrastructure.Security

Namespace Presentation.Views
    Public Class SettingsView
        Inherits UserControl

        Private txtCompanyName As TextBox
        Private dtpStartTime As DateTimePicker
        Private dtpEndTime As DateTimePicker
        Private numGrace As NumericUpDown
        Private numBreak As NumericUpDown
        Private numOtThreshold As NumericUpDown
        Private txtWeekendDays As TextBox
        Private btnSaveSettings As Button

        Private btnBackupDb As Button
        Private btnRestoreDb As Button
        Private txtCurrentPass As TextBox
        Private txtNewPass As TextBox
        Private btnChangePass As Button
        Private lblPassMsg As Label

        Private ReadOnly _settingRepo As New SettingRepository()
        Private ReadOnly _backupService As New BackupService()
        Private ReadOnly _authService As New AuthService()

        Public Sub New()
            InitializeComponent()
            LoadSettings()
        End Sub

        Private Sub InitializeComponent()
            Me.Dock = DockStyle.Fill
            Me.BackColor = UITheme.BackgroundColor
            Me.Font = UITheme.FontBody
            Me.Padding = New Padding(16)
            Me.AutoScroll = True

            Dim pnlTop As New Panel With {.Dock = DockStyle.Top, .Height = 44, .BackColor = Color.Transparent}
            Dim lblTitle As New Label With {.Text = "System Configuration & Database Maintenance", .Font = UITheme.FontPageTitle, .ForeColor = UITheme.TextPrimary, .Dock = DockStyle.Left, .AutoSize = True, .TextAlign = ContentAlignment.MiddleLeft}
            pnlTop.Controls.Add(lblTitle)

            Dim pnlContent As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 580,
                .BackColor = UITheme.PanelBackground,
                .BorderStyle = BorderStyle.FixedSingle,
                .Padding = New Padding(20)
            }

            Dim lblSec1 As New Label With {.Text = "Company & Attendance Rules", .Font = UITheme.FontSectionHeader, .ForeColor = UITheme.PrimaryDark, .Location = New Point(20, 16), .AutoSize = True}

            Dim y = 46
            Dim gap = 38

            Dim lblComp As New Label With {.Text = "Company Name:", .Location = New Point(24, y), .Size = New Size(150, 18), .Font = UITheme.FontBodyBold}
            txtCompanyName = New TextBox With {.Location = New Point(180, y - 2), .Size = New Size(280, 26)}

            y += gap
            Dim lblStart As New Label With {.Text = "Default Work Start:", .Location = New Point(24, y), .Size = New Size(150, 18), .Font = UITheme.FontBodyBold}
            dtpStartTime = New DateTimePicker With {.Location = New Point(180, y - 2), .Size = New Size(120, 26), .Format = DateTimePickerFormat.Time, .ShowUpDown = True}

            y += gap
            Dim lblEnd As New Label With {.Text = "Default Work End:", .Location = New Point(24, y), .Size = New Size(150, 18), .Font = UITheme.FontBodyBold}
            dtpEndTime = New DateTimePicker With {.Location = New Point(180, y - 2), .Size = New Size(120, 26), .Format = DateTimePickerFormat.Time, .ShowUpDown = True}

            y += gap
            Dim lblGrace As New Label With {.Text = "Grace Period (Min):", .Location = New Point(24, y), .Size = New Size(150, 18), .Font = UITheme.FontBodyBold}
            numGrace = New NumericUpDown With {.Location = New Point(180, y - 2), .Size = New Size(120, 26), .Minimum = 0, .Maximum = 60, .Value = 15}

            y += gap
            Dim lblBreak As New Label With {.Text = "Break Duration (Min):", .Location = New Point(24, y), .Size = New Size(150, 18), .Font = UITheme.FontBodyBold}
            numBreak = New NumericUpDown With {.Location = New Point(180, y - 2), .Size = New Size(120, 26), .Minimum = 0, .Maximum = 120, .Value = 60}

            y += gap
            Dim lblOt As New Label With {.Text = "Daily Hours for OT:", .Location = New Point(24, y), .Size = New Size(150, 18), .Font = UITheme.FontBodyBold}
            numOtThreshold = New NumericUpDown With {.Location = New Point(180, y - 2), .Size = New Size(120, 26), .Minimum = 4, .Maximum = 12, .Value = 8}

            y += gap
            Dim lblWk As New Label With {.Text = "Weekend Days:", .Location = New Point(24, y), .Size = New Size(150, 18), .Font = UITheme.FontBodyBold}
            txtWeekendDays = New TextBox With {.Location = New Point(180, y - 2), .Size = New Size(280, 26), .Text = "Saturday,Sunday"}

            y += gap + 6
            btnSaveSettings = New Button With {.Text = "Save System Rules", .Location = New Point(180, y), .Size = New Size(150, 32)}
            UITheme.ApplyButtonStyle(btnSaveSettings, True)
            AddHandler btnSaveSettings.Click, AddressOf BtnSaveSettings_Click

            pnlContent.Controls.AddRange(New Control() {lblSec1, lblComp, txtCompanyName, lblStart, dtpStartTime, lblEnd, dtpEndTime, lblGrace, numGrace, lblBreak, numBreak, lblOt, numOtThreshold, lblWk, txtWeekendDays, btnSaveSettings})

            Dim lblSec2 As New Label With {.Text = "Database Maintenance", .Font = UITheme.FontSectionHeader, .ForeColor = UITheme.PrimaryDark, .Location = New Point(520, 16), .AutoSize = True}

            Dim lblBackupDesc As New Label With {
                .Text = "Perform a non-locking backup of the SQLite database or restore from an existing file.",
                .Location = New Point(520, 46),
                .Size = New Size(380, 36),
                .ForeColor = UITheme.TextSecondary
            }

            btnBackupDb = New Button With {.Text = "Backup Database Now", .Location = New Point(520, 90), .Size = New Size(170, 32)}
            UITheme.ApplyButtonStyle(btnBackupDb, False)
            AddHandler btnBackupDb.Click, AddressOf BtnBackupDb_Click

            btnRestoreDb = New Button With {.Text = "Restore Database...", .Location = New Point(700, 90), .Size = New Size(160, 32)}
            UITheme.ApplyButtonStyle(btnRestoreDb, False)
            AddHandler btnRestoreDb.Click, AddressOf BtnRestoreDb_Click

            Dim lblSec3 As New Label With {.Text = "Account Security", .Font = UITheme.FontSectionHeader, .ForeColor = UITheme.PrimaryDark, .Location = New Point(520, 150), .AutoSize = True}

            Dim lblCurrP As New Label With {.Text = "Current Password:", .Location = New Point(520, 185), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            txtCurrentPass = New TextBox With {.Location = New Point(665, 182), .Size = New Size(195, 26), .UseSystemPasswordChar = True}

            Dim lblNewP As New Label With {.Text = "New Password:", .Location = New Point(520, 222), .Size = New Size(140, 18), .Font = UITheme.FontBodyBold}
            txtNewPass = New TextBox With {.Location = New Point(665, 220), .Size = New Size(195, 26), .UseSystemPasswordChar = True}

            btnChangePass = New Button With {.Text = "Update Password", .Location = New Point(665, 258), .Size = New Size(140, 32)}
            UITheme.ApplyButtonStyle(btnChangePass, True)
            AddHandler btnChangePass.Click, AddressOf BtnChangePass_Click

            lblPassMsg = New Label With {.Location = New Point(520, 300), .Size = New Size(360, 36), .ForeColor = UITheme.StatusAbsent, .Font = UITheme.FontSmall}

            pnlContent.Controls.AddRange(New Control() {lblSec2, lblBackupDesc, btnBackupDb, btnRestoreDb, lblSec3, lblCurrP, txtCurrentPass, lblNewP, txtNewPass, btnChangePass, lblPassMsg})

            Me.Controls.Add(pnlContent)
            Me.Controls.Add(pnlTop)
        End Sub

        Private Sub LoadSettings()
            txtCompanyName.Text = _settingRepo.GetValue("CompanyName", "Vertex Technologies Inc.")
            numGrace.Value = CDec(_settingRepo.GetValue("GracePeriodMinutes", "15"))
            numBreak.Value = CDec(_settingRepo.GetValue("DefaultBreakMinutes", "60"))
            numOtThreshold.Value = CDec(_settingRepo.GetValue("OvertimeThresholdHours", "8"))
            txtWeekendDays.Text = _settingRepo.GetValue("WeekendDays", "Saturday,Sunday")
        End Sub

        Private Sub BtnSaveSettings_Click(sender As Object, e As EventArgs)
            _settingRepo.SaveSetting("CompanyName", txtCompanyName.Text.Trim(), "Company name", "General")
            _settingRepo.SaveSetting("GracePeriodMinutes", numGrace.Value.ToString(), "Allowed grace minutes", "Attendance")
            _settingRepo.SaveSetting("DefaultBreakMinutes", numBreak.Value.ToString(), "Default break minutes", "Attendance")
            _settingRepo.SaveSetting("OvertimeThresholdHours", numOtThreshold.Value.ToString(), "OT threshold hours", "Attendance")
            _settingRepo.SaveSetting("WeekendDays", txtWeekendDays.Text.Trim(), "Weekend day names", "Calendar")

            MessageBox.Show("System configuration updated successfully.", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub

        Private Sub BtnBackupDb_Click(sender As Object, e As EventArgs)
            Using sfd As New SaveFileDialog()
                sfd.Filter = "SQLite Database (*.db)|*.db"
                sfd.FileName = "AttendanceBackup_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".db"

                If sfd.ShowDialog() = DialogResult.OK Then
                    Dim res = _backupService.BackupDatabase(sfd.FileName)
                    If res.Success Then
                        MessageBox.Show(res.Message, "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        MessageBox.Show(res.Message, "Backup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                End If
            End Using
        End Sub

        Private Sub BtnRestoreDb_Click(sender As Object, e As EventArgs)
            Using ofd As New OpenFileDialog()
                ofd.Filter = "SQLite Database (*.db)|*.db"
                If ofd.ShowDialog() = DialogResult.OK Then
                    Dim confirm = MessageBox.Show("Restoring will replace the active database. A safety backup will be created automatically." & Environment.NewLine & "Are you sure you want to proceed?", "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                    If confirm = DialogResult.Yes Then
                        Dim res = _backupService.RestoreDatabase(ofd.FileName)
                        If res.Success Then
                            MessageBox.Show(res.Message, "Restore Succeeded", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Else
                            MessageBox.Show(res.Message, "Restore Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        End If
                    End If
                End If
            End Using
        End Sub

        Private Sub BtnChangePass_Click(sender As Object, e As EventArgs)
            lblPassMsg.Text = ""
            If SessionManager.CurrentUser Is Nothing Then Return

            Dim res = _authService.ChangePassword(SessionManager.CurrentUser.Id, txtCurrentPass.Text, txtNewPass.Text)
            If res.Success Then
                lblPassMsg.ForeColor = UITheme.StatusPresent
                lblPassMsg.Text = res.Message
                txtCurrentPass.Clear()
                txtNewPass.Clear()
            Else
                lblPassMsg.ForeColor = UITheme.StatusAbsent
                lblPassMsg.Text = res.Message
            End If
        End Sub
    End Class
End Namespace
