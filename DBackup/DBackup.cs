using DBackup.Models;
using DBackup.Services;
using MySqlConnector;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using File = System.IO.File;

namespace DBackup
{
    public partial class DBackup : Form
    {
        #region Win32 API & Constants

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        #endregion

        #region Services

        // Services to manage logic
        private readonly SettingsService _settingsService;
        private readonly BackupService _backupService;
        private readonly InstallService _installService;
        private readonly SchedulerService _schedulerService;

        #endregion

        #region Form Constructor & Load/Close Events

        public DBackup()
        {
            InitializeComponent();
            appVersion.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Initialize services
            _settingsService = new SettingsService();
            _installService = new InstallService();
            _schedulerService = new SchedulerService();
            _backupService = new BackupService();

            // Wire up service events
            _backupService.OnError += LogError; // Log errors from the service

            notifyIcon.Visible = false;
            this.Resize += new EventHandler(DBackup_Resize);

            // UI Tweaks
            databases.MouseDown += (s, e) =>
            {
                ListViewItem item = databases.GetItemAt(e.X, e.Y);
                if (item != null) item.Checked = !item.Checked;
            };
            databases.ItemSelectionChanged += (s, e) => { e.Item.Selected = false; };
        }

        private void DBackup_Load(object sender, EventArgs e)
        {
            LoadSettingsToUi();
            LoadMySQLDatabases(); // Try loading with saved settings
            _schedulerService.Start(OnSchedulerTick);

            if (string.IsNullOrWhiteSpace(localPath.Text))
            {
                localPath.Text = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "DBackup");
                Directory.CreateDirectory(localPath.Text);
            }
        }

        private void DBackup_FormClosing(object sender, FormClosingEventArgs e)
        {
            _schedulerService.Stop();
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

        #endregion

        #region UI Event Handlers (Window & Tray)

        private void DBackup_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon.Visible = true;
                ShowSmartMessage("The application is running in the background.");
            }
        }

        private void Hide_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to close the application?",
                                         "Close",
                                         MessageBoxButtons.OKCancel,
                                         MessageBoxIcon.Information);
            if (result == DialogResult.OK)
            {
                this.Close();
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.BringToFront();
        }

        private void DBackup_MouseDown(object sender, MouseEventArgs e)
        {
            // Allow dragging the borderless window
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }


        #endregion

        #region UI Event Handlers (Buttons & Controls)

        private void BrowsePath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a backup folder";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    localPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private async void Install_Click(object sender, EventArgs e)
        {
            try
            {
                install.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;

                var settings = GatherSettingsFromUi();

                #region Validation
                if (string.IsNullOrWhiteSpace(settings.LocalPath))
                {
                    ShowSmartMessage("Backup Path cannot be empty.");
                    return;
                }
                if (!settings.SelectedDatabases.Any())
                {
                    ShowSmartMessage("Please select at least one database.");
                    return;
                }
                if (!TimeSpan.TryParse(timebackup.Text.Trim(), out var result))
                {
                    ShowSmartMessage("Invalid time format. Please use HH:mm (e.g., 08:30 or 23:00).");
                    return;
                }
                #endregion

                // Save settings first
                _settingsService.SaveSettings(settings);

                // Run backup
                await Task.Run(() => _backupService.PerformBackup(settings));

                settings.LastBackupDate = DateTime.Now;
                _settingsService.SaveSettings(settings);

                ShowSmartMessage("Backup was created.");

                // Handle installation
                if (!settings.IsInstalled)
                {
                    var res = MessageBox.Show("Do you want the app to autostart on system boot?",
                                              "Installing...", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                    if (res == DialogResult.OK)
                    {
                        settings.IsInstalled = true;
                        _settingsService.SaveSettings(settings); // Save "Installed" state
                        _installService.InstallToStartup(settings.LocalPath);
                        // App will restart from new location
                    }
                }
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex.Message, "Install or Apply");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                install.Enabled = true;
            }
        }

        private void SaveSet_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = GatherSettingsFromUi();
                _settingsService.SaveSettings(settings);
                ShowSmartMessage("Settings were saved successfully.", "Saving settings...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Error saving settings: " + ex.Message);
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            try
            {
                _settingsService.DeleteSettings();
                _installService.DeleteScheduledTask();

                if (instalat.Text == "Installed: YES")
                {
                    if (DialogResult.Yes == MessageBox.Show("Do you also want to remove the app?", "App uninstalling...", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        _installService.UninstallApplication();
                        // App will exit here
                    }
                }

                ShowSmartMessage("Settings were deleted successfully.", "App resetting...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                instalat.Text = ("Installed: NO");
            }
            catch (Exception ex)
            {
                ShowSmartMessage("Error resetting: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex.Message, "Reset Settings");
            }
        }

        private void ConnectDB_Click(object sender, EventArgs e)
        {
            LoadMySQLDatabases();
        }

        #endregion

        #region UI Event Handlers (Toggles)

        private void Enableftp_CheckedChanged(object sender, EventArgs e)
        {
            serverftp.Enabled = enableftp.Checked;
            userftp.Enabled = enableftp.Checked;
            passftp.Enabled = enableftp.Checked;
            caleftp.Enabled = enableftp.Checked;
        }

        private void Autobk_CheckedChanged(object sender, EventArgs e)
        {
            nrdaysbk.Enabled = autobk.Checked;
            timebackup.Enabled = autobk.Checked;
        }


        #endregion

        #region Core Logic: Service Coordination

        private async void OnSchedulerTick()
        {
            // Load settings directly from registry, not UI
            var settings = _settingsService.LoadSettings();

            if (!settings.AutoBackupEnabled) return;

            DateTime now = DateTime.Now;
            DateTime scheduledTimeToday = now.Date + settings.BackupTime;

            if (now >= scheduledTimeToday && settings.LastBackupDate.Date < now.Date)
            {
                this.Invoke((MethodInvoker)delegate { ShowSmartMessage("Automatic backup started..."); });

                // Run backup on a background thread
                await Task.Run(() => _backupService.PerformBackup(settings));

                // Update and save the last run date
                settings.LastBackupDate = DateTime.Now;
                _settingsService.SaveSettings(settings);
            }
        }

        private void LoadMySQLDatabases()
        {
            string server = sqlServ.Text.Trim();
            string user = sqlUser.Text.Trim();
            string pass = sqlPass.Text.Trim();

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user))
            {
                if (instalat.Text == "Installed: YES")
                    ShowSmartMessage("The MySQL Server and User fields cannot be empty.");
                return;
            }

            try
            {
                var settings = _settingsService.LoadSettings(); // For saved DBs
                var dbs = _backupService.GetDatabases(server, user, pass);

                databases.Items.Clear();
                databases.BeginUpdate();
                foreach (var db in dbs)
                {
                    var item = new ListViewItem(db.Name);
                    item.SubItems.Add(db.SizeMB);
                    item.SubItems.Add(db.TableCount);

                    if (settings.SelectedDatabases.Contains(db.Name))
                    {
                        item.Checked = true;
                    }
                    databases.Items.Add(item);
                }
                databases.EndUpdate();
            }
            catch (MySqlException sqlEx)
            {
                ShowSmartMessage($"Error MySqlConnector ({sqlEx.Number}): {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                ShowSmartMessage("General error: " + ex.Message);
            }
        }

        #endregion

        #region Settings & Model Mapping

        private SettingsModel GatherSettingsFromUi()
        {
            return new SettingsModel
            {
                // DB
                SqlServer = sqlServ.Text.Trim(),
                SqlUser = sqlUser.Text.Trim(),
                SqlPass = sqlPass.Text.Trim(),
                SelectedDatabases = databases.Items.Cast<ListViewItem>()
                                      .Where(i => i.Checked)
                                      .Select(i => i.Text)
                                      .ToList(),
                // Backup
                LocalPath = localPath.Text.Trim(),
                AutoBackupEnabled = autobk.Checked,
                RetentionDays = int.TryParse(nrdaysbk.Text.Trim(), out var days) ? days : 7,
                BackupTime = TimeSpan.TryParse(timebackup.Text.Trim(), out var time) ? time : TimeSpan.Zero,
                LastBackupDate = _settingsService.LoadSettings().LastBackupDate, // Preserve this

                // FTP
                FtpEnabled = enableftp.Checked,
                FtpServer = serverftp.Text.Trim(),
                FtpUser = userftp.Text.Trim(),
                FtpPass = passftp.Text.Trim(),
                FtpPath = caleftp.Text.Trim(),

                // App
                IsInstalled = (instalat.Text == "Installed: YES")
            };
        }

        private void LoadSettingsToUi()
        {
            var settings = _settingsService.LoadSettings();

            // DB
            sqlServ.Text = settings.SqlServer;
            sqlUser.Text = settings.SqlUser;
            sqlPass.Text = settings.SqlPass;
            // Note: SelectedDatabases are handled in LoadMySQLDatabases

            // Backup
            localPath.Text = settings.LocalPath;
            autobk.Checked = settings.AutoBackupEnabled;
            nrdaysbk.Text = settings.RetentionDays.ToString();
            timebackup.Text = settings.BackupTime.ToString(@"hh\:mm");

            // FTP
            enableftp.Checked = settings.FtpEnabled;
            serverftp.Text = settings.FtpServer;
            userftp.Text = settings.FtpUser;
            passftp.Text = settings.FtpPass;
            caleftp.Text = settings.FtpPath;

            // App
            instalat.Text = "Installed: " + (settings.IsInstalled ? "YES" : "NO");
        }

        #endregion

        #region Utility Methods

        public void ShowSmartMessage(string message, string title = "Info", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            // If minimized or hidden, use a Balloon Tip
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
            else // Otherwise, show a standard MessageBox
            {
                MessageBox.Show(message, title, buttons, icon);
            }
        }

        private void LogError(string message, string source)
        {
            try
            {
                string logFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dbackup_error.log");
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{source}] {message}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                // Failsafe in case logging to file fails
                Console.WriteLine("Logging error failed: " + ex.Message);
            }
        }

        #endregion
    }
}