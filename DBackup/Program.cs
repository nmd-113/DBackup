using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace DBackup
{
    internal static class Program
    {
        public static bool RunSilent = false;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("silent", StringComparison.OrdinalIgnoreCase))
            {
                RunSilent = true;
                Thread.Sleep(10000);
            }


            if (!IsRunningAsAdministrator())
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Application.ExecutablePath,
                        Verb = "runas",
                        UseShellExecute = true,
                        Arguments = RunSilent ? "silent" : ""
                    });
                }
                catch
                {
                    MessageBox.Show("Administrator rights are required!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DBackup());
        }

        static bool IsRunningAsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}