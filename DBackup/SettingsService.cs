using DBackup.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DBackup.Services
{
    public class SettingsService
    {
        private const string RegistryPath = @"Software\DBackup";

        public void SaveSettings(SettingsModel settings)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                // DB
                key.SetValue("SelectedDatabases", string.Join(",", settings.SelectedDatabases));
                key.SetValue("SqlServer", settings.SqlServer);
                key.SetValue("SqlUser", settings.SqlUser);
                key.SetValue("SqlPass", Encrypt(settings.SqlPass));

                // Backup
                key.SetValue("LocalPath", settings.LocalPath);
                key.SetValue("AutoBk", settings.AutoBackupEnabled ? "1" : "0");
                key.SetValue("NrDaysBk", settings.RetentionDays.ToString());
                key.SetValue("TimeBackup", settings.BackupTime.ToString(@"hh\:mm"));
                key.SetValue("LastBackupDate", settings.LastBackupDate.ToString("yyyy-MM-dd"));

                // FTP
                key.SetValue("EnableFtp", settings.FtpEnabled ? "1" : "0");
                key.SetValue("ServerFtp", settings.FtpServer);
                key.SetValue("UserFtp", settings.FtpUser);
                key.SetValue("PassFtp", Encrypt(settings.FtpPass));
                key.SetValue("PathFtp", settings.FtpPath);

                key.SetValue("Installed", settings.IsInstalled ? "1" : "0");
            }
        }

        public SettingsModel LoadSettings()
        {
            var settings = new SettingsModel();

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key == null)
                    return settings; // Return defaults

                // DB
                settings.SqlServer = key.GetValue("SqlServer") as string ?? "localhost";
                settings.SqlUser = key.GetValue("SqlUser") as string ?? "root";
                settings.SqlPass = Decrypt(key.GetValue("SqlPass") as string ?? "");
                string dbs = key.GetValue("SelectedDatabases") as string ?? "";
                settings.SelectedDatabases = dbs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                // Backup
                settings.LocalPath = key.GetValue("LocalPath") as string ?? "";
                settings.AutoBackupEnabled = (key.GetValue("AutoBk") as string ?? "0") == "1";
                int.TryParse(key.GetValue("NrDaysBk") as string ?? "7", out int days);
                settings.RetentionDays = days;
                TimeSpan.TryParse(key.GetValue("TimeBackup") as string ?? "00:00", out TimeSpan time);
                settings.BackupTime = time;
                DateTime.TryParse(key.GetValue("LastBackupDate") as string ?? "", out DateTime lastRun);
                settings.LastBackupDate = lastRun;

                // FTP
                settings.FtpEnabled = (key.GetValue("EnableFtp") as string ?? "0") == "1";
                settings.FtpServer = key.GetValue("ServerFtp") as string ?? "";
                settings.FtpUser = key.GetValue("UserFtp") as string ?? "";
                settings.FtpPass = Decrypt(key.GetValue("PassFtp") as string ?? "");
                settings.FtpPath = key.GetValue("PathFtp") as string ?? "";

                settings.IsInstalled = (key.GetValue("Installed") as string ?? "0") == "1";
            }
            return settings;
        }

        public void DeleteSettings()
        {
            RegistryKey cuKey = Registry.CurrentUser.OpenSubKey("Software", true);
            if (cuKey.OpenSubKey("DBackup") != null)
            {
                cuKey.DeleteSubKeyTree("DBackup");
            }
        }

        private string Encrypt(string plainText)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        private string Decrypt(string encryptedText)
        {
            try
            {
                byte[] data = Convert.FromBase64String(encryptedText);
                byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return ""; // Failed (empty or corrupt)
            }
        }
    }
}