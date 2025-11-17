using DBackup.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace DBackup.Services
{
    public class BackupService
    {
        // Event for logging. The form will subscribe to this.
        public event Action<string, string> OnError;

        public List<DatabaseInfo> GetDatabases(string server, string user, string pass)
        {
            var dbList = new List<DatabaseInfo>();
            var builder = new MySqlConnectionStringBuilder
            {
                Server = server,
                UserID = user,
                Password = pass,
                Database = "information_schema",
                ConnectionTimeout = 5
            };

            string excludedDbs = "'information_schema', 'performance_schema', 'mysql', 'sys', 'phpmyadmin', 'test', 'testdb'";
            string query = $@"
                SELECT 
                    s.schema_name AS DbName,
                    COUNT(t.table_name) AS TableCount,
                    ROUND(COALESCE(SUM(t.data_length + t.index_length), 0) / 1024 / 1024, 2) AS SizeMB
                FROM information_schema.schemata s
                LEFT JOIN information_schema.tables t ON s.schema_name = t.table_schema
                WHERE s.schema_name NOT IN ({excludedDbs})
                GROUP BY s.schema_name;";

            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dbList.Add(new DatabaseInfo
                        {
                            Name = reader.GetString("DbName"),
                            TableCount = reader["TableCount"].ToString(),
                            SizeMB = reader["SizeMB"].ToString()
                        });
                    }
                }
            }
            return dbList;
        }

        public void PerformBackup(SettingsModel settings)
        {
            BackupDatabase(settings);
            if (settings.FtpEnabled)
            {
                UploadToFtp(settings);
            }
        }

        private void BackupDatabase(SettingsModel settings)
        {
            string backupRoot = Path.Combine(settings.LocalPath, "Backups");
            string dateTimeStr = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

            foreach (string dbName in settings.SelectedDatabases)
            {
                string dbFolder = Path.Combine(backupRoot, dbName);
                Directory.CreateDirectory(dbFolder);

                string sqlFile = Path.Combine(dbFolder, $"{dbName}_{dateTimeStr}.sql");
                string zipFile = Path.ChangeExtension(sqlFile, ".zip");

                try
                {
                    // 1. Create SQL dump
                    var builder = new MySqlConnectionStringBuilder
                    {
                        Server = settings.SqlServer,
                        UserID = settings.SqlUser,
                        Password = settings.SqlPass,
                        Database = dbName,
                        AllowUserVariables = true,
                        ConnectionTimeout = 60
                    };

                    using (var conn = new MySqlConnection(builder.ConnectionString))
                    using (var cmd = new MySqlCommand())
                    using (var mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(sqlFile);
                        conn.Close();
                    }

                    // 2. Zip the SQL file
                    if (File.Exists(sqlFile))
                    {
                        using (FileStream zipToOpen = new FileStream(zipFile, FileMode.Create))
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                        {
                            archive.CreateEntryFromFile(sqlFile, Path.GetFileName(sqlFile));
                        }
                        File.Delete(sqlFile);
                    }

                    // 3. Clean up old local backups
                    if (settings.AutoBackupEnabled)
                    {
                        CleanupLocalFiles(dbFolder, settings.RetentionDays);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Backup error for {dbName}: {ex.Message}", "Backup Database");
                }
            }
        }

        private void CleanupLocalFiles(string dbFolder, int minFilesToKeep)
        {
            try
            {
                var timeCutoff = DateTime.UtcNow.AddDays(-minFilesToKeep);

                string[] filePaths = Directory.GetFiles(dbFolder, "*.zip");
                string pattern = @"_(\d{8})_(\d{6})\.zip$";

                var backups = filePaths.Select(path =>
                {
                    var fileName = Path.GetFileName(path);
                    var match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);

                    DateTime? timeStamp = null;
                    if (match.Success)
                    {
                        string dateAndTime = match.Groups[1].Value + match.Groups[2].Value;
                        if (DateTime.TryParseExact(dateAndTime, "yyyyMMddHHmmss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out var dt))
                        {
                            timeStamp = dt;
                        }
                    }

                    return new { Path = path, TimeStamp = timeStamp };
                })
                .Where(x => x.TimeStamp.HasValue)
                .OrderByDescending(x => x.TimeStamp.Value)
                .ToList();

                var filesToProtect = backups
                    .Take(minFilesToKeep)
                    .Select(b => b.Path)
                    .ToHashSet();

                var filesToDelete = backups.Where(b =>
                    b.TimeStamp.Value < timeCutoff &&
                    !filesToProtect.Contains(b.Path))
                    .ToList();

                foreach (var old in filesToDelete)
                {
                    try
                    {
                        File.Delete(old.Path);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke($"Error deleting old local backup {Path.GetFileName(old.Path)}: {ex.Message}", "Local Cleanup");
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, $"Local Cleanup Folder: {Path.GetFileName(dbFolder)}");
            }
        }

        private void UploadToFtp(SettingsModel settings)
        {
            string baseRemotePath = $"{settings.FtpPath.TrimEnd('/')}";
            string localBasePath = Path.Combine(settings.LocalPath, "Backups");

            foreach (string db in settings.SelectedDatabases)
            {
                string localFolder = Path.Combine(localBasePath, db);
                if (!Directory.Exists(localFolder)) continue;

                // 1. Get latest backup
                string[] zipFiles = Directory.GetFiles(localFolder, "*.zip")
                             .OrderByDescending(f => File.GetCreationTimeUtc(f))
                             .Take(1)
                             .ToArray();

                string remotePath = $"{baseRemotePath}/{db}";
                EnsureFtpDirectory(settings.FtpServer, settings.FtpUser, settings.FtpPass, remotePath);

                // 2. Upload the file
                foreach (string zipFile in zipFiles)
                {
                    string fileName = Path.GetFileName(zipFile);
                    string remoteFileUrl = $"ftp://{settings.FtpServer}/{remotePath.TrimStart('/')}/{fileName}";

                    try
                    {
                        var req = (FtpWebRequest)WebRequest.Create(remoteFileUrl);
                        req.Method = WebRequestMethods.Ftp.UploadFile;
                        req.Credentials = new NetworkCredential(settings.FtpUser, settings.FtpPass);
                        req.UsePassive = true;
                        req.UseBinary = true;
                        req.Timeout = 15000;

                        using (var fs = new FileStream(zipFile, FileMode.Open, FileAccess.Read))
                        {
                            req.ContentLength = fs.Length;
                            using (var stream = req.GetRequestStream())
                            {
                                fs.CopyTo(stream);
                            }
                        }
                        using ((FtpWebResponse)req.GetResponse()) { }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke($"Upload error for {fileName}: {ex.Message}", "Upload FTP");
                    }
                }

                // 3. Clean up old FTP backups
                if (settings.AutoBackupEnabled)
                {
                    CleanupFtpFiles(settings, remotePath);
                }
            }
        }

        private void CleanupFtpFiles(SettingsModel settings, string remotePath)
        {
            try
            {
                var listReq = (FtpWebRequest)WebRequest.Create($"ftp://{settings.FtpServer}/{remotePath.TrimStart('/')}");
                listReq.Method = WebRequestMethods.Ftp.ListDirectory;
                listReq.Credentials = new NetworkCredential(settings.FtpUser, settings.FtpPass);
                listReq.UsePassive = true;
                listReq.Timeout = 15000;

                List<string> names;
                using (var r = (FtpWebResponse)listReq.GetResponse())
                using (var sr = new StreamReader(r.GetResponseStream()))
                    names = sr.ReadToEnd()
                              .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(Path.GetFileName)
                              .Where(n => n.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                              .ToList();

                var backups = names.Select(n =>
                {
                    var match = Regex.Match(n, @"_(\d{8})_(\d{6})\.zip$", RegexOptions.IgnoreCase);
                    if (match.Success && DateTime.TryParseExact(match.Groups[1].Value + match.Groups[2].Value, "yyyyMMddHHmmss",
                        CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                    {
                        return new { Name = n, TimeStamp = (DateTime?)dt };
                    }
                    return new { Name = n, TimeStamp = (DateTime?)null };
                })
                .Where(x => x.TimeStamp.HasValue)
                .OrderByDescending(x => x.TimeStamp.Value)
                .ToList();

                var timeCutoff = DateTime.UtcNow.AddDays(-settings.RetentionDays);

                int minFilesToKeep = settings.RetentionDays;

                var filesToConsiderForDeletion = backups.Skip(minFilesToKeep).ToList();

                var filesToDelete = filesToConsiderForDeletion
                    .Where(b => b.TimeStamp.Value < timeCutoff)
                    .ToList();

                foreach (var old in filesToDelete)
                {
                    string url = $"ftp://{settings.FtpServer}/{remotePath.TrimStart('/')}/{Uri.EscapeDataString(old.Name)}";
                    try
                    {
                        var delReq = (FtpWebRequest)WebRequest.Create(url);
                        delReq.Method = WebRequestMethods.Ftp.DeleteFile;
                        delReq.Credentials = new NetworkCredential(settings.FtpUser, settings.FtpPass);
                        delReq.UsePassive = true;
                        delReq.Timeout = 15000;
                        using ((FtpWebResponse)delReq.GetResponse()) { }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke($"Error deleting old backup {old.Name}: {ex.Message}", "FTP Cleanup");
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error cleaning FTP folder: {ex.Message}", "FTP Cleanup Folder");
            }
        }

        private void EnsureFtpDirectory(string server, string user, string pass, string remotePath)
        {
            string current = "";
            foreach (var part in remotePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                current += "/" + part;
                try
                {
                    var req = (FtpWebRequest)WebRequest.Create($"ftp://{server}{current}");
                    req.Method = WebRequestMethods.Ftp.MakeDirectory;
                    req.Credentials = new NetworkCredential(user, pass);
                    req.UsePassive = true;
                    req.Timeout = 15000;
                    using (var resp = (FtpWebResponse)req.GetResponse()) { }
                }
                catch (WebException ex)
                {
                    var resp = (FtpWebResponse)ex.Response;
                    // 550 usually means "directory already exists"
                    if (resp.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        OnError?.Invoke($"Error creating FTP folder ({current}): {resp.StatusDescription}", "FTP");
                        throw;
                    }
                }
            }
        }
    }
}