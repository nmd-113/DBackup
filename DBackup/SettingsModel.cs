using System;
using System.Collections.Generic;

namespace DBackup.Models
{
    public class SettingsModel
    {
        // MySQL
        public string SqlServer { get; set; } = "localhost";
        public string SqlUser { get; set; } = "root";
        public string SqlPass { get; set; } = "";
        public List<string> SelectedDatabases { get; set; } = new List<string>();

        // Backup
        public string LocalPath { get; set; } = "";
        public bool AutoBackupEnabled { get; set; } = false;
        public int RetentionDays { get; set; } = 7;
        public TimeSpan BackupTime { get; set; } = TimeSpan.Zero;
        public DateTime LastBackupDate { get; set; } = DateTime.MinValue;

        // FTP
        public bool FtpEnabled { get; set; } = false;
        public string FtpServer { get; set; } = "";
        public string FtpUser { get; set; } = "";
        public string FtpPass { get; set; } = "";
        public string FtpPath { get; set; } = "DBackup/Backups";

        // App
        public bool IsInstalled { get; set; } = false;
    }
}