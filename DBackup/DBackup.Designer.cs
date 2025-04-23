using System;
using System.Windows.Forms;

namespace DBackup
{
    partial class DBackup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DBackup));
            this.databases = new System.Windows.Forms.ListView();
            this.dbColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dbSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dbTables = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.localBackuptxt = new System.Windows.Forms.Label();
            this.inchide = new System.Windows.Forms.Button();
            this.minimize = new System.Windows.Forms.Button();
            this.nrdaysbk = new System.Windows.Forms.TextBox();
            this.nrdaysbktxt = new System.Windows.Forms.Label();
            this.foldernametxt = new System.Windows.Forms.Label();
            this.foldername = new System.Windows.Forms.TextBox();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.enableftp = new System.Windows.Forms.CheckBox();
            this.passftptxt = new System.Windows.Forms.Label();
            this.passftp = new System.Windows.Forms.TextBox();
            this.serverftptxt = new System.Windows.Forms.Label();
            this.serverftp = new System.Windows.Forms.TextBox();
            this.userftptxt = new System.Windows.Forms.Label();
            this.userftp = new System.Windows.Forms.TextBox();
            this.caleftptxt = new System.Windows.Forms.Label();
            this.caleftp = new System.Windows.Forms.TextBox();
            this.timebackuptxt = new System.Windows.Forms.Label();
            this.timebackup = new System.Windows.Forms.TextBox();
            this.install = new System.Windows.Forms.Button();
            this.autobk = new System.Windows.Forms.CheckBox();
            this.instalat = new System.Windows.Forms.Label();
            this.reset = new System.Windows.Forms.Button();
            this.saveSet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // databases
            // 
            this.databases.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.databases.BackColor = System.Drawing.Color.White;
            this.databases.CheckBoxes = true;
            this.databases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.dbColumn,
            this.dbSize,
            this.dbTables});
            this.databases.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.databases.ForeColor = System.Drawing.Color.Black;
            this.databases.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.databases.HideSelection = false;
            this.databases.Location = new System.Drawing.Point(16, 54);
            this.databases.Name = "databases";
            this.databases.ShowGroups = false;
            this.databases.Size = new System.Drawing.Size(568, 97);
            this.databases.TabIndex = 0;
            this.databases.UseCompatibleStateImageBehavior = false;
            this.databases.View = System.Windows.Forms.View.Details;
            // 
            // dbColumn
            // 
            this.dbColumn.Text = "Database name";
            this.dbColumn.Width = 250;
            // 
            // dbSize
            // 
            this.dbSize.Text = "Size (MB)";
            this.dbSize.Width = 150;
            // 
            // dbTables
            // 
            this.dbTables.Text = "Tables";
            this.dbTables.Width = 100;
            // 
            // localBackuptxt
            // 
            this.localBackuptxt.AutoSize = true;
            this.localBackuptxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.localBackuptxt.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.localBackuptxt.ForeColor = System.Drawing.Color.White;
            this.localBackuptxt.Location = new System.Drawing.Point(12, 14);
            this.localBackuptxt.Name = "localBackuptxt";
            this.localBackuptxt.Size = new System.Drawing.Size(238, 24);
            this.localBackuptxt.TabIndex = 2;
            this.localBackuptxt.Text = "DBackup - MySQL FTP";
            // 
            // inchide
            // 
            this.inchide.BackColor = System.Drawing.Color.OrangeRed;
            this.inchide.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.inchide.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.inchide.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inchide.ForeColor = System.Drawing.Color.White;
            this.inchide.Location = new System.Drawing.Point(505, 15);
            this.inchide.Name = "inchide";
            this.inchide.Size = new System.Drawing.Size(79, 23);
            this.inchide.TabIndex = 13;
            this.inchide.Text = "EXIT";
            this.inchide.UseVisualStyleBackColor = false;
            this.inchide.Click += new System.EventHandler(this.inchide_Click);
            // 
            // minimize
            // 
            this.minimize.BackColor = System.Drawing.Color.LimeGreen;
            this.minimize.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.minimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.minimize.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.minimize.ForeColor = System.Drawing.Color.White;
            this.minimize.Location = new System.Drawing.Point(15, 292);
            this.minimize.Name = "minimize";
            this.minimize.Size = new System.Drawing.Size(102, 43);
            this.minimize.TabIndex = 14;
            this.minimize.Text = "MINIMIZE";
            this.minimize.UseVisualStyleBackColor = false;
            this.minimize.Click += new System.EventHandler(this.ascunde_Click);
            // 
            // nrdaysbk
            // 
            this.nrdaysbk.BackColor = System.Drawing.Color.White;
            this.nrdaysbk.Enabled = false;
            this.nrdaysbk.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nrdaysbk.ForeColor = System.Drawing.Color.Black;
            this.nrdaysbk.Location = new System.Drawing.Point(442, 156);
            this.nrdaysbk.Name = "nrdaysbk";
            this.nrdaysbk.Size = new System.Drawing.Size(33, 22);
            this.nrdaysbk.TabIndex = 15;
            this.nrdaysbk.Text = "7";
            // 
            // nrdaysbktxt
            // 
            this.nrdaysbktxt.AutoSize = true;
            this.nrdaysbktxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.nrdaysbktxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nrdaysbktxt.ForeColor = System.Drawing.Color.White;
            this.nrdaysbktxt.Location = new System.Drawing.Point(359, 159);
            this.nrdaysbktxt.Name = "nrdaysbktxt";
            this.nrdaysbktxt.Size = new System.Drawing.Size(79, 16);
            this.nrdaysbktxt.TabIndex = 16;
            this.nrdaysbktxt.Text = "Loop (days):";
            // 
            // foldernametxt
            // 
            this.foldernametxt.AutoSize = true;
            this.foldernametxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.foldernametxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.foldernametxt.ForeColor = System.Drawing.Color.White;
            this.foldernametxt.Location = new System.Drawing.Point(13, 159);
            this.foldernametxt.Name = "foldernametxt";
            this.foldernametxt.Size = new System.Drawing.Size(85, 16);
            this.foldernametxt.TabIndex = 18;
            this.foldernametxt.Text = "Folder Name:";
            // 
            // foldername
            // 
            this.foldername.BackColor = System.Drawing.Color.White;
            this.foldername.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.foldername.ForeColor = System.Drawing.Color.Black;
            this.foldername.Location = new System.Drawing.Point(99, 156);
            this.foldername.Name = "foldername";
            this.foldername.Size = new System.Drawing.Size(180, 22);
            this.foldername.TabIndex = 17;
            this.foldername.Text = "Backups";
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "Backup Info";
            this.notifyIcon.BalloonTipTitle = "DBackup";
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "DBackup";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // enableftp
            // 
            this.enableftp.AutoSize = true;
            this.enableftp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.enableftp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.enableftp.ForeColor = System.Drawing.Color.White;
            this.enableftp.Location = new System.Drawing.Point(13, 195);
            this.enableftp.Name = "enableftp";
            this.enableftp.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.enableftp.Size = new System.Drawing.Size(102, 20);
            this.enableftp.TabIndex = 20;
            this.enableftp.Text = "Backup FTP";
            this.enableftp.UseVisualStyleBackColor = false;
            this.enableftp.CheckedChanged += new System.EventHandler(this.enableftp_CheckedChanged);
            // 
            // passftptxt
            // 
            this.passftptxt.AutoSize = true;
            this.passftptxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.passftptxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passftptxt.ForeColor = System.Drawing.Color.White;
            this.passftptxt.Location = new System.Drawing.Point(287, 253);
            this.passftptxt.Name = "passftptxt";
            this.passftptxt.Size = new System.Drawing.Size(69, 16);
            this.passftptxt.TabIndex = 22;
            this.passftptxt.Text = "Pass FTP:";
            // 
            // passftp
            // 
            this.passftp.BackColor = System.Drawing.Color.White;
            this.passftp.Enabled = false;
            this.passftp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passftp.ForeColor = System.Drawing.Color.Black;
            this.passftp.Location = new System.Drawing.Point(360, 250);
            this.passftp.Name = "passftp";
            this.passftp.PasswordChar = '*';
            this.passftp.Size = new System.Drawing.Size(223, 22);
            this.passftp.TabIndex = 21;
            // 
            // serverftptxt
            // 
            this.serverftptxt.AutoSize = true;
            this.serverftptxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.serverftptxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serverftptxt.ForeColor = System.Drawing.Color.White;
            this.serverftptxt.Location = new System.Drawing.Point(13, 225);
            this.serverftptxt.Name = "serverftptxt";
            this.serverftptxt.Size = new System.Drawing.Size(75, 16);
            this.serverftptxt.TabIndex = 24;
            this.serverftptxt.Text = "Server FTP:";
            // 
            // serverftp
            // 
            this.serverftp.BackColor = System.Drawing.Color.White;
            this.serverftp.Enabled = false;
            this.serverftp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serverftp.ForeColor = System.Drawing.Color.Black;
            this.serverftp.Location = new System.Drawing.Point(99, 222);
            this.serverftp.Name = "serverftp";
            this.serverftp.Size = new System.Drawing.Size(180, 22);
            this.serverftp.TabIndex = 23;
            // 
            // userftptxt
            // 
            this.userftptxt.AutoSize = true;
            this.userftptxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.userftptxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.userftptxt.ForeColor = System.Drawing.Color.White;
            this.userftptxt.Location = new System.Drawing.Point(13, 253);
            this.userftptxt.Name = "userftptxt";
            this.userftptxt.Size = new System.Drawing.Size(66, 16);
            this.userftptxt.TabIndex = 26;
            this.userftptxt.Text = "User FTP:";
            // 
            // userftp
            // 
            this.userftp.BackColor = System.Drawing.Color.White;
            this.userftp.Enabled = false;
            this.userftp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.userftp.ForeColor = System.Drawing.Color.Black;
            this.userftp.Location = new System.Drawing.Point(99, 250);
            this.userftp.Name = "userftp";
            this.userftp.Size = new System.Drawing.Size(180, 22);
            this.userftp.TabIndex = 25;
            // 
            // caleftptxt
            // 
            this.caleftptxt.AutoSize = true;
            this.caleftptxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.caleftptxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.caleftptxt.ForeColor = System.Drawing.Color.White;
            this.caleftptxt.Location = new System.Drawing.Point(287, 225);
            this.caleftptxt.Name = "caleftptxt";
            this.caleftptxt.Size = new System.Drawing.Size(66, 16);
            this.caleftptxt.TabIndex = 28;
            this.caleftptxt.Text = "Path FTP:";
            // 
            // caleftp
            // 
            this.caleftp.BackColor = System.Drawing.Color.White;
            this.caleftp.Enabled = false;
            this.caleftp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.caleftp.ForeColor = System.Drawing.Color.Black;
            this.caleftp.Location = new System.Drawing.Point(360, 222);
            this.caleftp.Name = "caleftp";
            this.caleftp.Size = new System.Drawing.Size(223, 22);
            this.caleftp.TabIndex = 27;
            this.caleftp.Text = "Example/UserName";
            // 
            // timebackuptxt
            // 
            this.timebackuptxt.AutoSize = true;
            this.timebackuptxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.timebackuptxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timebackuptxt.ForeColor = System.Drawing.Color.White;
            this.timebackuptxt.Location = new System.Drawing.Point(492, 159);
            this.timebackuptxt.Name = "timebackuptxt";
            this.timebackuptxt.Size = new System.Drawing.Size(39, 16);
            this.timebackuptxt.TabIndex = 31;
            this.timebackuptxt.Text = "Time:";
            // 
            // timebackup
            // 
            this.timebackup.BackColor = System.Drawing.Color.White;
            this.timebackup.Enabled = false;
            this.timebackup.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timebackup.ForeColor = System.Drawing.Color.Black;
            this.timebackup.Location = new System.Drawing.Point(533, 156);
            this.timebackup.Name = "timebackup";
            this.timebackup.Size = new System.Drawing.Size(50, 22);
            this.timebackup.TabIndex = 30;
            this.timebackup.Text = "12:00";
            // 
            // install
            // 
            this.install.BackColor = System.Drawing.Color.OrangeRed;
            this.install.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.install.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.install.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.install.ForeColor = System.Drawing.Color.White;
            this.install.Location = new System.Drawing.Point(434, 292);
            this.install.Name = "install";
            this.install.Size = new System.Drawing.Size(150, 43);
            this.install.TabIndex = 32;
            this.install.Text = "SET BACKUP";
            this.install.UseVisualStyleBackColor = false;
            this.install.Click += new System.EventHandler(this.instalare_Click);
            // 
            // autobk
            // 
            this.autobk.AutoSize = true;
            this.autobk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.autobk.ForeColor = System.Drawing.Color.White;
            this.autobk.Location = new System.Drawing.Point(287, 157);
            this.autobk.Name = "autobk";
            this.autobk.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.autobk.Size = new System.Drawing.Size(55, 20);
            this.autobk.TabIndex = 33;
            this.autobk.Text = "Daily";
            this.autobk.UseVisualStyleBackColor = false;
            this.autobk.CheckedChanged += new System.EventHandler(this.autobk_CheckedChanged);
            // 
            // instalat
            // 
            this.instalat.AutoSize = true;
            this.instalat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.instalat.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.instalat.ForeColor = System.Drawing.Color.White;
            this.instalat.Location = new System.Drawing.Point(124, 305);
            this.instalat.Name = "instalat";
            this.instalat.Size = new System.Drawing.Size(83, 16);
            this.instalat.TabIndex = 34;
            this.instalat.Text = "Installed: NO";
            // 
            // reset
            // 
            this.reset.BackColor = System.Drawing.Color.SteelBlue;
            this.reset.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.reset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.reset.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reset.ForeColor = System.Drawing.Color.White;
            this.reset.Location = new System.Drawing.Point(219, 302);
            this.reset.Name = "reset";
            this.reset.Size = new System.Drawing.Size(60, 23);
            this.reset.TabIndex = 35;
            this.reset.Text = "Reset";
            this.reset.UseVisualStyleBackColor = false;
            this.reset.Click += new System.EventHandler(this.reset_Click);
            // 
            // saveSet
            // 
            this.saveSet.BackColor = System.Drawing.Color.LimeGreen;
            this.saveSet.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.saveSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveSet.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveSet.ForeColor = System.Drawing.Color.White;
            this.saveSet.Location = new System.Drawing.Point(341, 302);
            this.saveSet.Name = "saveSet";
            this.saveSet.Size = new System.Drawing.Size(60, 23);
            this.saveSet.TabIndex = 36;
            this.saveSet.Text = "Save";
            this.saveSet.UseVisualStyleBackColor = false;
            this.saveSet.Click += new System.EventHandler(this.saveSet_Click);
            // 
            // DBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(40)))));
            this.ClientSize = new System.Drawing.Size(600, 350);
            this.Controls.Add(this.saveSet);
            this.Controls.Add(this.reset);
            this.Controls.Add(this.instalat);
            this.Controls.Add(this.autobk);
            this.Controls.Add(this.install);
            this.Controls.Add(this.timebackuptxt);
            this.Controls.Add(this.timebackup);
            this.Controls.Add(this.caleftptxt);
            this.Controls.Add(this.caleftp);
            this.Controls.Add(this.userftptxt);
            this.Controls.Add(this.userftp);
            this.Controls.Add(this.serverftptxt);
            this.Controls.Add(this.serverftp);
            this.Controls.Add(this.passftptxt);
            this.Controls.Add(this.passftp);
            this.Controls.Add(this.enableftp);
            this.Controls.Add(this.foldernametxt);
            this.Controls.Add(this.foldername);
            this.Controls.Add(this.nrdaysbktxt);
            this.Controls.Add(this.nrdaysbk);
            this.Controls.Add(this.minimize);
            this.Controls.Add(this.inchide);
            this.Controls.Add(this.localBackuptxt);
            this.Controls.Add(this.databases);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(600, 350);
            this.MinimumSize = new System.Drawing.Size(600, 350);
            this.Name = "DBackup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DBackup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DBackup_FormClosing);
            this.Load += new System.EventHandler(this.DBackup_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DBackup_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView databases;
        private System.Windows.Forms.Label localBackuptxt;
        private System.Windows.Forms.Button inchide;
        private System.Windows.Forms.Button minimize;
        private System.Windows.Forms.TextBox nrdaysbk;
        private System.Windows.Forms.Label nrdaysbktxt;
        private System.Windows.Forms.Label foldernametxt;
        private System.Windows.Forms.TextBox foldername;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.CheckBox enableftp;
        private System.Windows.Forms.Label passftptxt;
        private System.Windows.Forms.TextBox passftp;
        private System.Windows.Forms.Label serverftptxt;
        private System.Windows.Forms.TextBox serverftp;
        private System.Windows.Forms.Label userftptxt;
        private System.Windows.Forms.TextBox userftp;
        private System.Windows.Forms.Label caleftptxt;
        private System.Windows.Forms.TextBox caleftp;
        private System.Windows.Forms.Label timebackuptxt;
        private System.Windows.Forms.TextBox timebackup;
        private System.Windows.Forms.Button install;
        private System.Windows.Forms.ColumnHeader dbColumn;
        private System.Windows.Forms.ColumnHeader dbSize;
        private System.Windows.Forms.ColumnHeader dbTables;
        private CheckBox autobk;
        private Label instalat;
        private Button reset;
        private Button saveSet;
    }
}

