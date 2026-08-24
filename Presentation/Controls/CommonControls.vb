Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AttendanceSystem.Common.Constants

Namespace Presentation.Controls
    Public Class SectionHeader
        Inherits UserControl

        Private lblTitle As Label
        Private pnlLine As Panel

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Property TitleText As String
            Get
                Return If(lblTitle IsNot Nothing, lblTitle.Text, "")
            End Get
            Set(value As String)
                If lblTitle IsNot Nothing Then lblTitle.Text = value
            End Set
        End Property

        Private Sub InitializeComponent()
            lblTitle = New Label With {
                .Dock = DockStyle.Left,
                .Font = UITheme.FontSectionHeader,
                .ForeColor = UITheme.TextPrimary,
                .AutoSize = True,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Padding = New Padding(0, 0, 8, 0)
            }

            pnlLine = New Panel With {
                .Dock = DockStyle.Fill,
                .Height = 1,
                .BackColor = UITheme.BorderColor,
                .Margin = New Padding(0, 10, 0, 0)
            }

            Dim flow As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .RowCount = 1,
                .ColumnCount = 2,
                .BackColor = Color.Transparent
            }
            flow.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
            flow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))

            Dim lineWrap As New Panel With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(0, 12, 0, 0),
                .BackColor = Color.Transparent
            }
            lineWrap.Controls.Add(pnlLine)

            flow.Controls.Add(lblTitle, 0, 0)
            flow.Controls.Add(lineWrap, 1, 0)

            Me.Controls.Add(flow)
            Me.Height = 28
            Me.BackColor = Color.Transparent
        End Sub
    End Class

    Public Class MetricCard
        Inherits Panel

        Private lblValue As Label
        Private lblTitle As Label

        Public Sub New(title As String, value As String, Optional statusColor As Nullable(Of Color) = Nothing)
            Me.BackColor = UITheme.PanelBackground
            Me.BorderStyle = BorderStyle.FixedSingle
            Me.Padding = New Padding(12)
            Me.Size = New Size(170, 75)

            lblTitle = New Label With {
                .Text = title.ToUpper(),
                .Font = UITheme.FontSmall,
                .ForeColor = UITheme.TextSecondary,
                .Dock = DockStyle.Top,
                .Height = 18
            }

            lblValue = New Label With {
                .Text = value,
                .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
                .ForeColor = If(statusColor.HasValue, statusColor.Value, UITheme.TextPrimary),
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft
            }

            Me.Controls.Add(lblValue)
            Me.Controls.Add(lblTitle)
        End Sub

        Public Sub UpdateValue(value As String)
            lblValue.Text = value
        End Sub
    End Class
End Namespace
