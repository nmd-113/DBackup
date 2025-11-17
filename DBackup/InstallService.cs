using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DBackup.Services
{
    public class InstallService
    {
        public void InstallToStartup(string localPath)
        {
            string destFolder = localPath.Trim();
            Directory.CreateDirectory(destFolder);

            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeName = Path.GetFileName(exePath);
            string destExePath = Path.Combine(destFolder, exeName);

            if (!System.IO.File.Exists(destExePath))
            {
                System.IO.File.Copy(exePath, destExePath, true);
            }

            // Create Scheduled Task
            string taskName = "DBackupAutoStart";
            string taskCommand = $"\"{destExePath}\" silent";
            string arguments = $"/Create /F /RL HIGHEST /SC ONLOGON /TN \"{taskName}\" /TR \"{taskCommand}\"";

            ProcessStartInfo psi = new ProcessStartInfo("schtasks.exe", arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
            }

            CreateDesktopShortcut(destExePath);

            // Start the new .exe and exit
            Process.Start(new ProcessStartInfo(destExePath));
            Application.Exit();
        }

        public void DeleteScheduledTask()
        {
            Process schtasks = new Process();
            schtasks.StartInfo.FileName = "schtasks.exe";
            schtasks.StartInfo.Arguments = "/Delete /TN \"DBackupAutoStart\" /F";
            schtasks.StartInfo.CreateNoWindow = true;
            schtasks.StartInfo.UseShellExecute = false;
            schtasks.Start();
            schtasks.WaitForExit();
        }

        public void UninstallApplication()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string batPath = Path.Combine(Path.GetTempPath(), "delme.bat");
            string desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DBackup.lnk");

            // Batch file to delete the app and shortcut
            System.IO.File.WriteAllText(batPath, $@"
                @echo off
                ping 127.0.0.1 -n 3 > nul
                del ""{desktopShortcut}""
                del ""{exePath}""
                del ""%~f0""
                ");

            Process.Start(new ProcessStartInfo("cmd.exe", $"/C \"{batPath}\"") { CreateNoWindow = true });
            Application.Exit();
        }

        private void CreateDesktopShortcut(string exePath)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktop, "DBackup.lnk");

            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.WindowStyle = 1;
            shortcut.Description = "DBackup - MySQL FTP Auto Backup";
            shortcut.IconLocation = exePath;
            shortcut.Save();
        }
    }
}