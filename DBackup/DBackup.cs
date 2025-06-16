using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using IWshRuntimeLibrary;
using System.Web.Script.Serialization;
using System.Net;
using System.Linq;
using Microsoft.Win32;
using System.Reflection;
using File = System.IO.File;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DBackup
{
    public partial class DBackup : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        private Thread backupThread;
        private bool stopThread = false;

        private const string RegistryPath = @"Software\DBackup";

        public DBackup()
        {
            InitializeComponent();

            notifyIcon.Visible = false;

            this.Resize += new EventHandler(DBackup_Resize);
            databases.MouseDown += (s, e) =>
            {
                ListViewItem item = databases.GetItemAt(e.X, e.Y);
                if (item != null)
                {
                    item.Checked = !item.Checked;
                }
            };

            databases.ItemSelectionChanged += (s, e) =>
            {
                e.Item.Selected = false;
            };

        }

        private void DBackup_Load(object sender, EventArgs e)
        {
            LoadMySQLDatabases();
            LoadSettings();
            StartBackupThread();
            if (localPath.Text.Trim() == "")
            {
                localPath.Text = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "DBackup");
                Directory.CreateDirectory(localPath.Text);
            }
        }

        private void DBackup_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopBackupThread();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Program.RunSilent)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                notifyIcon.Visible = true;
                this.Hide();
            }
        }

        #region ----- INTERFACE

        private void DBackup_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon.Visible = true;
                ShowSmartMessage("Aplicația rulează în fundal.");
            }
        }

        private void ascunde_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void inchide_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Sunteți sigur că doriți să închideți aplicația?", "Închidere", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.OK)
            {
                this.Close();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.BringToFront();
        }

        private void DBackup_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        #endregion

        #region ----- LOAD DATABASES
        private void LoadMySQLDatabases()
        {
            string mysqlPath = FindMySQLExecutable();

            if (mysqlPath == null)
            {
                ShowSmartMessage("mysql.exe was not found. Please check if MySQL is installed on this computer.");
                return;
            }

            try
            {
                var excludedDatabases = new HashSet<string>
        {
            "information_schema", "performance_schema", "mysql", "sys", "phpmyadmin", "test", "testdb"
        };

                var showDatabasesInfo = new ProcessStartInfo
                {
                    FileName = mysqlPath,
                    Arguments = "-u root -e \"SHOW DATABASES;\" --batch --silent",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                List<string> databaseNames = new List<string>();
                using (var process = Process.Start(showDatabasesInfo))
                {
                    if (process == null)
                    {
                        ShowSmartMessage("Failed to start MySQL process.");
                        return;
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        ShowSmartMessage("MySQL Error:\n" + error);
                        return;
                    }

                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var dbName in lines)
                    {
                        string trimmed = dbName.Trim();
                        if (!excludedDatabases.Contains(trimmed))
                        {
                            databaseNames.Add(trimmed);
                        }
                    }
                }

                if (databases == null)
                {
                    ShowSmartMessage("ListView 'databases' is null.");
                    return;
                }

                databases.Items.Clear();

                foreach (var dbName in databaseNames)
                {
                    string query = $@"
                SELECT 
                    COUNT(*) AS tables_count,
                    ROUND(COALESCE(SUM(data_length + index_length),0)/1024/1024, 2) AS size_mb
                FROM information_schema.tables
                WHERE table_schema = '{dbName}';";

                    var sizeInfoStartInfo = new ProcessStartInfo
                    {
                        FileName = mysqlPath,
                        Arguments = $"-u root -e \"{query}\" --batch --silent",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var sizeProcess = Process.Start(sizeInfoStartInfo))
                    {
                        if (sizeProcess == null)
                        {
                            ShowSmartMessage("Failed to start MySQL process for size info.");
                            return;
                        }

                        string sizeOutput = sizeProcess.StandardOutput.ReadToEnd();
                        string sizeError = sizeProcess.StandardError.ReadToEnd();
                        sizeProcess.WaitForExit();

                        if (!string.IsNullOrWhiteSpace(sizeError))
                        {
                            ShowSmartMessage("MySQL Error:\n" + sizeError);
                            return;
                        }

                        Console.WriteLine($"DB: {dbName}, Query output:\n{sizeOutput}");

                        string tableCount = "0";
                        string size = "0";

                        var lines = sizeOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Length >= 1)
                        {
                            var parts = lines[0].Split('\t');
                            if (parts.Length >= 2)
                            {
                                tableCount = string.IsNullOrWhiteSpace(parts[0]) || parts[0].ToLower() == "null" ? "0" : parts[0];
                                size = string.IsNullOrWhiteSpace(parts[1]) || parts[1].ToLower() == "null" ? "0" : parts[1];
                            }
                        }

                        var item = new ListViewItem(dbName);
                        item.SubItems.Add(size);
                        item.SubItems.Add(tableCount);
                        databases.Items.Add(item);
                    }
                }

                databases.Refresh();
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Eroare la preluarea bazelor de date: " + ex.Message);
            }
        }

        private string FindMySQLExecutable()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            string[] possiblePaths =
            {
                @"C:\xampp\mysql\bin\mysql.exe",
                @"C:\aristarch\serv\mysql\bin\mysql.exe",
                @"D:\xampp\mysql\bin\mysql.exe",
                @"D:\aristarch\serv\mysql\bin\mysql.exe",
                @"C:\wamp\bin\mysql\mysql5.7.36\bin\mysql.exe",
                @"C:\wamp64\bin\mysql\mysql5.7.36\bin\mysql.exe",
                $@"{programFiles}\MySQL\MySQL Server 8.0\bin\mysql.exe",
                $@"{programFilesX86}\MySQL\MySQL Server 8.0\bin\mysql.exe",
                @"C:\mysql\bin\mysql.exe",
                @"D:\mysql\bin\mysql.exe"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }

            return null;
        }


        #endregion

        private string BackupPath()
        {
            return Path.Combine(localPath.Text.Trim(), "Backups");
        }

        private void browsePath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // Set the default selected path
                string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DBackup");
                folderDialog.Description = "Select a backup folder";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    localPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private string FindMySQLDump()
        {
            string mysqlPath = FindMySQLExecutable();
            if (mysqlPath == null) return null;
            return mysqlPath.Replace("mysql.exe", "mysqldump.exe");
        }

        private void BackupDatabase()
        {
            string mysqldumpPath = FindMySQLDump();
            if (string.IsNullOrEmpty(mysqldumpPath))
            {
                ShowSmartMessage("mysqldump.exe not found.");
                return;
            }

            int retentionDays = int.TryParse(nrdaysbk.Text.Trim(), out int days) ? days : 7;
            string backupRoot = BackupPath();
            string dateTimeStr = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

            foreach (ListViewItem item in databases.Items)
            {
                if (!item.Checked) continue;

                string dbName = item.Text;
                string dbFolder = Path.Combine(backupRoot, dbName);
                Directory.CreateDirectory(dbFolder);

                string sqlFile = Path.Combine(dbFolder, $"{dbName}_{dateTimeStr}.sql");
                string zipFile = Path.ChangeExtension(sqlFile, ".zip");

                var dumpProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = mysqldumpPath,
                        Arguments = $"--user=root {dbName} --result-file=\"{sqlFile}\"",
                        RedirectStandardOutput = false,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                try
                {
                    dumpProcess.Start();
                    dumpProcess.WaitForExit();

                    if (File.Exists(sqlFile))
                    {
                        using (FileStream zipToOpen = new FileStream(zipFile, FileMode.Create))
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                        {
                            archive.CreateEntryFromFile(sqlFile, Path.GetFileName(sqlFile));
                        }

                        File.Delete(sqlFile);
                    }

                    if (autobk.Checked)
                    {
                        var nowUtc = DateTime.UtcNow;

                        string[] zipFiles = Directory.GetFiles(dbFolder, "*.zip");

                        var backups = zipFiles
                            .Select(f =>
                            {
                                string fileName = Path.GetFileNameWithoutExtension(f);
                                var parts = fileName.Split('_');
                                if (parts.Length < 3)
                                    return new { Path = f, TimeStamp = (DateTime?)null };

                                string datePart = parts[parts.Length - 2];
                                string timePart = parts[parts.Length - 1];
                                DateTime dt;
                                bool parsed = DateTime.TryParseExact(datePart + timePart, "yyyyMMddHHmmss",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                    out dt);
                                return new { Path = f, TimeStamp = parsed ? (DateTime?)dt : null };
                            })
                            .Where(x => x.TimeStamp.HasValue)
                            .OrderByDescending(x => x.TimeStamp.Value)
                            .ToList();

                        if (backups.Count > retentionDays)
                        {
                            var candidates = backups.Skip(retentionDays);

                            foreach (var candidate in candidates)
                            {
                                if ((nowUtc - candidate.TimeStamp.Value).TotalDays > retentionDays)
                                {
                                    try
                                    {
                                        File.Delete(candidate.Path);
                                    }
                                    catch
                                    {
                                        // ignore
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowSmartMessage($"Backup error for {dbName}: {ex.Message}");
                }
            }
        }

        private void UploadToFtp()
        {
            string server = serverftp.Text.Trim();
            string user = userftp.Text.Trim();
            string pass = passftp.Text.Trim();
            string baseRemotePath = $"{caleftp.Text.Trim().TrimEnd('/')}";
            int retentionDays = int.TryParse(nrdaysbk.Text.Trim(), out int days) ? days : 7;
            string localBasePath = BackupPath();

            foreach (ListViewItem item in databases.Items)
            {
                if (!item.Checked) continue;

                string db = item.Text;
                string localFolder = Path.Combine(localBasePath, db);
                if (!Directory.Exists(localFolder)) continue;

                string[] zipFiles = Directory.GetFiles(localFolder, "*.zip");
                string remotePath = $"{baseRemotePath}/{db}";

                EnsureFtpDirectory(server, user, pass, remotePath);

                foreach (string zipFile in zipFiles)
                {
                    string fileName = Path.GetFileName(zipFile);
                    string remoteFileUrl = $"ftp://{server}/{remotePath.TrimStart('/')}/{fileName}";

                    try
                    {
                        var req = (FtpWebRequest)WebRequest.Create(remoteFileUrl);
                        req.Method = WebRequestMethods.Ftp.UploadFile;
                        req.Credentials = new NetworkCredential(user, pass);
                        req.UsePassive = true;
                        req.UseBinary = true;
                        req.KeepAlive = false;
                        req.Timeout = 15000;

                        byte[] data = File.ReadAllBytes(zipFile);
                        req.ContentLength = data.Length;

                        using (var stream = req.GetRequestStream())
                            stream.Write(data, 0, data.Length);

                        using ((FtpWebResponse)req.GetResponse()) { }
                    }
                    catch (Exception ex)
                    {
                        ShowSmartMessage($"Upload error for {fileName}: {ex.Message}");
                    }
                }

                if (autobk.Checked)
                {
                    try
                    {
                        var listReq = (FtpWebRequest)WebRequest.Create($"ftp://{server}/{remotePath.TrimStart('/')}");
                        listReq.Method = WebRequestMethods.Ftp.ListDirectory;
                        listReq.Credentials = new NetworkCredential(user, pass);
                        listReq.UsePassive = true;
                        listReq.UseBinary = true;
                        listReq.KeepAlive = false;
                        listReq.Timeout = 15000;

                        List<string> names;
                        using (var r = (FtpWebResponse)listReq.GetResponse())
                        using (var sr = new StreamReader(r.GetResponseStream()))
                            names = sr.ReadToEnd()
                                      .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(Path.GetFileName)
                                      .Where(n => n.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                                      .ToList();

                        var backups = names
                            .Select(n =>
                            {
                                var match = Regex.Match(n, @"_(\d{8})_(\d{6})\.zip$", RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    string datePart = match.Groups[1].Value;
                                    string timePart = match.Groups[2].Value;
                                    if (DateTime.TryParseExact(datePart + timePart, "yyyyMMddHHmmss",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                        out DateTime dt))
                                    {
                                        return new { Name = n, TimeStamp = (DateTime?)dt };
                                    }
                                }
                                return new { Name = n, TimeStamp = (DateTime?)null };
                            })
                            .Where(x => x.TimeStamp.HasValue)
                            .OrderByDescending(x => x.TimeStamp.Value)
                            .ToList();

                        var now = DateTime.UtcNow;
                        var cutoff = now.AddDays(-retentionDays);

                        var newer = backups.Where(b => b.TimeStamp.Value >= cutoff).ToList();
                        var older = backups.Where(b => b.TimeStamp.Value < cutoff).ToList();

                        int excess = backups.Count - retentionDays;
                        if (excess > 0 && older.Count > 0)
                        {
                            var deletable = older.Take(excess).ToList();

                            foreach (var old in deletable)
                            {
                                string url = $"ftp://{server}/{remotePath.TrimStart('/')}/{Uri.EscapeDataString(old.Name)}";
                                try
                                {
                                    Console.WriteLine($"Deleting old backup: {old.Name} (created: {old.TimeStamp})");

                                    var delReq = (FtpWebRequest)WebRequest.Create(url);
                                    delReq.Method = WebRequestMethods.Ftp.DeleteFile;
                                    delReq.Credentials = new NetworkCredential(user, pass);
                                    delReq.UsePassive = true;
                                    delReq.UseBinary = true;
                                    delReq.KeepAlive = false;
                                    delReq.Timeout = 15000;

                                    using ((FtpWebResponse)delReq.GetResponse()) { }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error deleting old backup {old.Name}: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error cleaning FTP folder: {ex.Message}");
                    }
                }
            }
        }

        private void EnsureFtpDirectory(string server, string user, string pass, string remotePath)
        {
            string[] parts = remotePath.Split('/');
            string current = "";

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;
                current += "/" + part;

                try
                {
                    var req = (FtpWebRequest)WebRequest.Create($"ftp://{server}{current}");
                    req.Method = WebRequestMethods.Ftp.MakeDirectory;
                    req.Credentials = new NetworkCredential(user, pass);
                    req.UsePassive = true;
                    req.UseBinary = true;
                    req.KeepAlive = false;
                    req.Timeout = 15000;

                    using (var resp = (FtpWebResponse)req.GetResponse()) { }
                }
                catch (WebException ex)
                {
                    var resp = (FtpWebResponse)ex.Response;
                    if (resp.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        ShowSmartMessage($"Error creating FTP folder ({current}): {resp.StatusDescription}");
                        throw;
                    }
                }
            }
        }

        private void InstallToStartup()
        {
            try
            {
                string destFolder = localPath.Text.Trim();
                Directory.CreateDirectory(destFolder);

                string exePath = Assembly.GetExecutingAssembly().Location;
                string exeName = Path.GetFileName(exePath);
                string destExePath = Path.Combine(destFolder, exeName);

                if (!File.Exists(destExePath))
                {
                    File.Copy(exePath, destExePath, true);
                }

                string taskName = "DBackupAutoStart";
                string taskCommand = $"\"{destExePath}\" silent";
                string arguments = $"/Create /F /RL HIGHEST /SC ONLOGON /TN \"{taskName}\" /TR \"{taskCommand}\"";

                ProcessStartInfo psi = new ProcessStartInfo("schtasks.exe", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                }

                CreateDesktopShortcut(destExePath);

                Process.Start(new ProcessStartInfo(destExePath));
                Application.Exit();
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Error installing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void SaveSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    List<string> selectedDatabases = new List<string>();
                    foreach (ListViewItem item in databases.Items)
                    {
                        if (item.Checked)
                            selectedDatabases.Add(item.Text);
                    }

                    key.SetValue("SelectedDatabases", string.Join(",", selectedDatabases));
                    key.SetValue("LocalPath", localPath.Text.Trim());
                    key.SetValue("AutoBk", autobk.Checked ? "1" : "0");
                    key.SetValue("NrDaysBk", nrdaysbk.Text.Trim());
                    key.SetValue("TimeBackup", timebackup.Text.Trim());
                    key.SetValue("EnableFtp", enableftp.Checked ? "1" : "0");
                    key.SetValue("ServerFtp", serverftp.Text.Trim());
                    key.SetValue("UserFtp", userftp.Text.Trim());
                    key.SetValue("PassFtp", Encrypt(passftp.Text.Trim()));
                    key.SetValue("PathFtp", caleftp.Text.Trim());
                    key.SetValue("Installed", "1");
                }
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Error saving settings: " + ex.Message);
            }
        }

        private void LoadSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key == null) return;

                    string encryptedPass = key.GetValue("PassFtp") as string ?? "";
                    string savedDbs = key.GetValue("SelectedDatabases") as string ?? "";
                    string[] selectedDatabases = savedDbs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (ListViewItem item in databases.Items)
                    {
                        item.Checked = selectedDatabases.Contains(item.Text);
                    }

                    localPath.Text = key.GetValue("LocalPath") as string ?? "";
                    autobk.Checked = (key.GetValue("AutoBk") as string ?? "0") == "1";
                    nrdaysbk.Text = key.GetValue("NrDaysBk") as string ?? "";
                    timebackup.Text = key.GetValue("TimeBackup") as string ?? "";
                    enableftp.Checked = (key.GetValue("EnableFtp") as string ?? "0") == "1";
                    serverftp.Text = key.GetValue("ServerFtp") as string ?? "";
                    userftp.Text = key.GetValue("UserFtp") as string ?? "";
                    passftp.Text = Decrypt(encryptedPass);
                    caleftp.Text = key.GetValue("PathFtp") as string ?? "";

                    string instalatVal = key.GetValue("Installed") as string ?? "0";
                    instalat.Text = "Installed: " + (instalatVal == "1" ? "YES" : "NO");
                }
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Error loading settings: " + ex.Message);
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
                return "";
            }
        }

        private void StartBackupThread()
        {
            backupThread = new Thread(() =>
            {
                while (!stopThread)
                {
                    try
                    {
                        CheckBackupTime();
                    }
                    catch (Exception ex)
                    {
                        ShowSmartMessage("Error checking backup time: " + ex.Message);
                    }

                    Thread.Sleep(60000);
                }
            });

            backupThread.IsBackground = true;
            backupThread.Start();
        }

        private void CheckBackupTime()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\DBackup"))
            {
                if (key == null) return;

                string autobk = key.GetValue("AutoBk") as string ?? "0";
                if (autobk != "1") return;

                string ora = key.GetValue("TimeBackup") as string ?? "00:00";
                if (!TimeSpan.TryParse(ora, out TimeSpan backupTime))
                    return;

                DateTime now = DateTime.Now;
                DateTime scheduledTime = now.Date + backupTime;

                if (scheduledTime.Hour == now.Hour && scheduledTime.Minute == now.Minute)
                {
                    RunBackup();
                }
            }
        }

        private void RunBackup()
        {
            try
            {
                ShowSmartMessage("Backup started...");
                InstallorApply();
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Error creating backup: " + ex.Message);
            }
        }

        private void StopBackupThread()
        {
            stopThread = true;

            if (backupThread != null && backupThread.IsAlive)
            {
                backupThread.Join(1000);
            }
        }

        private void instalare_Click(object sender, EventArgs e)
        {
            InstallorApply();
        }

        private void InstallorApply()
        {
            try
            {
                install.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;

                if (string.IsNullOrWhiteSpace(nrdaysbk.Text))
                {
                    ShowSmartMessage("Loop days number for automatic backup cannot be empty.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(localPath.Text))
                {
                    ShowSmartMessage("Backup Path cannot be empty.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(timebackup.Text))
                {
                    ShowSmartMessage("Time textbox cannot be empty. (ex: 13:00)");
                    return;
                }

                bool hasCheckedDatabase = databases.Items.Cast<ListViewItem>().Any(item => item.Checked);
                if (!hasCheckedDatabase)
                {
                    ShowSmartMessage("Please select at least one database to proceed.");
                    return;
                }

                SaveSettings();
                BackupDatabase();

                if (enableftp.Checked)
                {
                    UploadToFtp();
                }

                ShowSmartMessage("Backup was created.");

                if (instalat.Text == "Installed: NO")
                {
                    var result = MessageBox.Show("Do you want the app to autostart on system boot?",
                                                 "Installing...",
                                                 MessageBoxButtons.OKCancel,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.OK)
                    {
                        InstallToStartup();

                        using (RegistryKey readKey = Registry.CurrentUser.OpenSubKey(RegistryPath))
                        {
                            string instalatVal = readKey?.GetValue("Installed")?.ToString() ?? "0";
                            instalat.Text = "Installed: " + (instalatVal == "1" ? "YES" : "NO");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                install.Enabled = true;
            }
        }

        private void saveSet_Click(object sender, EventArgs e)
        {
            SaveSettings();
            ShowSmartMessage("Settings was saved successfully.", "Saving settings...", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void reset_Click(object sender, EventArgs e)
        {
            try
            {
                RegistryKey cuKey = Registry.CurrentUser.OpenSubKey("Software", true);
                if (cuKey.OpenSubKey("DBackup") != null)
                {
                    cuKey.DeleteSubKeyTree("DBackup");
                }

                Process schtasks = new Process();
                schtasks.StartInfo.FileName = "schtasks.exe";
                schtasks.StartInfo.Arguments = "/Delete /TN \"DBackupAutoStart\" /F";
                schtasks.StartInfo.CreateNoWindow = true;
                schtasks.StartInfo.UseShellExecute = false;
                schtasks.Start();
                schtasks.WaitForExit();

                if (instalat.Text == "Installed: YES")
                {
                    if (DialogResult.Yes == MessageBox.Show("Do you also want to remove the app?", "App uninstalling...", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        string exePath = Assembly.GetExecutingAssembly().Location;
                        string batPath = Path.Combine(Path.GetTempPath(), "delme.bat");
                        string desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DBackup.lnk");

                        File.WriteAllText(batPath, $@"
                            @echo off
                            ping 127.0.0.1 -n 3 > nul
                            del ""{desktopShortcut}""
                            del ""{exePath}""
                            del ""%~f0""
                            ");

                        Process.Start(new ProcessStartInfo("cmd.exe", $"/C \"{batPath}\"") { CreateNoWindow = true });
                        Application.Exit();
                    }
                }

                ShowSmartMessage("Settings was deleted successfully.", "App resetting...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                instalat.Text = ("Installed: NO");
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Error resetting: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void enableftp_CheckedChanged(object sender, EventArgs e)
        {
            serverftp.Enabled = enableftp.Checked;
            userftp.Enabled = enableftp.Checked;
            passftp.Enabled = enableftp.Checked;
            caleftp.Enabled = enableftp.Checked;
        }

        private void autobk_CheckedChanged(object sender, EventArgs e)
        {
            nrdaysbk.Enabled = autobk.Checked;
            timebackup.Enabled = autobk.Checked;
        }

        public void ShowSmartMessage(string message, string title = "Info", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            if (this.WindowState == FormWindowState.Minimized || !this.Visible)
            {
                if (!notifyIcon.Visible)
                    notifyIcon.Visible = true;

                if (notifyIcon.Icon == null)
                    notifyIcon.Icon = SystemIcons.Information;

                notifyIcon.BalloonTipTitle = title;
                notifyIcon.BalloonTipText = message;
                notifyIcon.ShowBalloonTip(3000);
            }
            else
            {
                MessageBox.Show(message, title, buttons, icon);
            }
        }


    }
}