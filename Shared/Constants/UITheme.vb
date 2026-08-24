Imports System
Imports System.Drawing

Namespace Common.Constants
    Public Module UITheme
        Public ReadOnly PrimaryColor As Color = Color.FromArgb(43, 76, 126)       ' Slate Navy #2B4C7E
        Public ReadOnly PrimaryDark As Color = Color.FromArgb(30, 54, 90)         ' Deep Slate
        Public ReadOnly PrimaryLight As Color = Color.FromArgb(235, 240, 248)     ' Soft Highlight
        
        Public ReadOnly BackgroundColor As Color = Color.FromArgb(245, 247, 250)  ' Neutral Light #F5F7FA
        Public ReadOnly PanelBackground As Color = Color.White
        Public ReadOnly SidebarBackground As Color = Color.FromArgb(38, 45, 56)    ' Dark Charcoal Sidebar
        Public ReadOnly SidebarSelected As Color = Color.FromArgb(52, 62, 77)
        Public ReadOnly SidebarText As Color = Color.FromArgb(220, 225, 230)
        Public ReadOnly SidebarTextMuted As Color = Color.FromArgb(145, 155, 168)
        
        Public ReadOnly BorderColor As Color = Color.FromArgb(215, 220, 228)       ' Subtle Border
        Public ReadOnly TextPrimary As Color = Color.FromArgb(33, 37, 41)         ' High Contrast Dark
        Public ReadOnly TextSecondary As Color = Color.FromArgb(108, 117, 125)    ' Muted Gray
        
        Public ReadOnly StatusPresent As Color = Color.FromArgb(40, 167, 69)      ' Forest Green
        Public ReadOnly StatusLate As Color = Color.FromArgb(220, 108, 0)         ' Warm Amber/Orange
        Public ReadOnly StatusAbsent As Color = Color.FromArgb(220, 53, 69)       ' Crimson Red
        Public ReadOnly StatusLeave As Color = Color.FromArgb(23, 162, 184)       ' Cyan / Blue
        Public ReadOnly StatusHoliday As Color = Color.FromArgb(111, 66, 193)     ' Purple

        Public ReadOnly FontAppTitle As New Font("Segoe UI", 12.0F, FontStyle.Bold)
        Public ReadOnly FontPageTitle As New Font("Segoe UI", 14.0F, FontStyle.Bold)
        Public ReadOnly FontSectionHeader As New Font("Segoe UI", 10.5F, FontStyle.Bold)
        Public ReadOnly FontBody As New Font("Segoe UI", 9.0F, FontStyle.Regular)
        Public ReadOnly FontBodyBold As New Font("Segoe UI", 9.0F, FontStyle.Bold)
        Public ReadOnly FontSmall As New Font("Segoe UI", 8.0F, FontStyle.Regular)
        Public ReadOnly FontMono As New Font("Consolas", 9.0F, FontStyle.Regular)

        Public Sub ApplyGridStyle(grid As DataGridView)
            grid.BackgroundColor = PanelBackground
            grid.BorderStyle = BorderStyle.FixedSingle
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            grid.GridColor = BorderColor
            grid.RowHeadersVisible = False
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            grid.MultiSelect = False
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.AllowUserToResizeRows = False
            grid.AutoGenerateColumns = False
            grid.Font = FontBody
            grid.RowTemplate.Height = 32

            grid.EnableHeadersVisualStyles = False
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 243, 246)
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary
            grid.ColumnHeadersDefaultCellStyle.Font = FontBodyBold
            grid.ColumnHeadersDefaultCellStyle.Padding = New Padding(6, 4, 6, 4)
            grid.ColumnHeadersHeight = 34
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single

            grid.DefaultCellStyle.BackColor = PanelBackground
            grid.DefaultCellStyle.ForeColor = TextPrimary
            grid.DefaultCellStyle.SelectionBackColor = PrimaryLight
            grid.DefaultCellStyle.SelectionForeColor = PrimaryDark
            grid.DefaultCellStyle.Padding = New Padding(6, 2, 6, 2)

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253)
            grid.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = PrimaryLight
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = PrimaryDark
        End Sub

        Public Sub ApplyButtonStyle(btn As Button, Optional isPrimary As Boolean = False)
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = 1
            btn.Font = FontBodyBold
            btn.Cursor = Cursors.Hand
            btn.Height = 32
            btn.Padding = New Padding(8, 0, 8, 0)
            btn.UseVisualStyleBackColor = False

            If isPrimary Then
                btn.BackColor = PrimaryColor
                btn.ForeColor = Color.White
                btn.FlatAppearance.BorderColor = PrimaryDark
            Else
                btn.BackColor = Color.White
                btn.ForeColor = TextPrimary
                btn.FlatAppearance.BorderColor = BorderColor
            End If
        End Sub
    End Module
End Namespace
