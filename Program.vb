Imports System
Imports System.Windows.Forms
Imports AttendanceSystem.Infrastructure.Database
Imports AttendanceSystem.Presentation.Views

Namespace AttendanceSystem
    Friend Module Program
        <STAThread()>
        Sub Main()
            System.Windows.Forms.Application.EnableVisualStyles()
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(False)

            Try
                DatabaseInitializer.Initialize()

                Using frmLogin As New FormLogin()
                    If frmLogin.ShowDialog() = DialogResult.OK Then
                        System.Windows.Forms.Application.Run(New FormMain())
                    Else
                        System.Windows.Forms.Application.Exit()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show("An unhandled system error occurred during startup:" & Environment.NewLine & ex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
    End Module
End Namespace
