Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants
Imports AttendanceSystem.Application.Services

Namespace Presentation.Views
    Public Class FormLogin
        Inherits Form

        Private txtUsername As TextBox
        Private txtPassword As TextBox
        Private btnLogin As Button
        Private btnExit As Button
        Private lblError As Label
        Private ReadOnly _authService As New AuthService()

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "Attendance System — Secure Login"
            Me.Size = New Size(420, 360)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.BackColor = UITheme.BackgroundColor
            Me.Font = UITheme.FontBody

            Dim pnlHeader As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 70,
                .BackColor = UITheme.PrimaryColor,
                .Padding = New Padding(20, 14, 20, 10)
            }

            Dim lblAppTitle As New Label With {
                .Text = "ATTENDANCE MANAGEMENT SYSTEM",
                .ForeColor = Color.White,
                .Font = UITheme.FontAppTitle,
                .Dock = DockStyle.Top,
                .Height = 22
            }

            Dim lblSubTitle As New Label With {
                .Text = "Enterprise Desktop Edition",
                .ForeColor = Color.FromArgb(200, 215, 235),
                .Font = UITheme.FontSmall,
                .Dock = DockStyle.Top,
                .Height = 18
            }

            pnlHeader.Controls.Add(lblSubTitle)
            pnlHeader.Controls.Add(lblAppTitle)

            Dim pnlContent As New Panel With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(24, 20, 24, 20),
                .BackColor = UITheme.PanelBackground
            }

            Dim lblUserPrompt As New Label With {
                .Text = "Username",
                .Location = New Point(24, 18),
                .Size = New Size(350, 18),
                .Font = UITheme.FontBodyBold,
                .ForeColor = UITheme.TextPrimary
            }

            txtUsername = New TextBox With {
                .Location = New Point(24, 38),
                .Size = New Size(350, 26),
                .Font = UITheme.FontBody,
                .Text = "admin"
            }

            Dim lblPassPrompt As New Label With {
                .Text = "Password",
                .Location = New Point(24, 76),
                .Size = New Size(350, 18),
                .Font = UITheme.FontBodyBold,
                .ForeColor = UITheme.TextPrimary
            }

            txtPassword = New TextBox With {
                .Location = New Point(24, 96),
                .Size = New Size(350, 26),
                .Font = UITheme.FontBody,
                .UseSystemPasswordChar = True,
                .Text = "Admin@123"
            }

            lblError = New Label With {
                .Location = New Point(24, 130),
                .Size = New Size(350, 36),
                .Font = UITheme.FontSmall,
                .ForeColor = UITheme.StatusAbsent,
                .Text = ""
            }

            btnLogin = New Button With {
                .Text = "Sign In",
                .Location = New Point(174, 172),
                .Size = New Size(100, 32)
            }
            UITheme.ApplyButtonStyle(btnLogin, True)
            AddHandler btnLogin.Click, AddressOf BtnLogin_Click

            btnExit = New Button With {
                .Text = "Exit",
                .Location = New Point(284, 172),
                .Size = New Size(90, 32)
            }
            UITheme.ApplyButtonStyle(btnExit, False)
            AddHandler btnExit.Click, Sub() Me.Close()

            pnlContent.Controls.Add(lblUserPrompt)
            pnlContent.Controls.Add(txtUsername)
            pnlContent.Controls.Add(lblPassPrompt)
            pnlContent.Controls.Add(txtPassword)
            pnlContent.Controls.Add(lblError)
            pnlContent.Controls.Add(btnLogin)
            pnlContent.Controls.Add(btnExit)

            Me.Controls.Add(pnlContent)
            Me.Controls.Add(pnlHeader)

            Me.AcceptButton = btnLogin
            Me.CancelButton = btnExit
        End Sub

        Private Sub BtnLogin_Click(sender As Object, e As EventArgs)
            lblError.Text = ""
            Dim result = _authService.Login(txtUsername.Text, txtPassword.Text)
            If result.Success Then
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                lblError.Text = result.Message
                txtPassword.SelectAll()
                txtPassword.Focus()
            End If
        End Sub
    End Class
End Namespace
