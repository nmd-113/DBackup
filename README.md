## 💾 DBackup - MySQL FTP Autobackup

**DBackup** is a simple Windows desktop app that helps you automatically back up your MySQL databases.

It creates SQL backups, compresses them into ZIP files, saves them locally, and can also upload them to an FTP server. It is made for users who want an easy and reliable way to protect their database data without doing manual backups every day.

---

## 🚀 Main Features

* **Automatic daily backups**  
  Choose a backup time, and DBackup will run automatically every day.

* **MySQL database backup**  
  Connects to your MySQL server and creates SQL backup files for your selected databases.

* **ZIP compression**  
  Backup files are compressed into ZIP archives to save storage space.

* **Local backup storage**  
  Saves backups to a folder chosen by the user.

* **FTP upload**  
  Automatically uploads backup ZIP files to your configured FTP server.

* **Encrypted passwords**  
  MySQL and FTP passwords are stored securely using Windows encryption.

* **Automatic cleanup**  
  Old backups can be deleted automatically based on your retention settings, both locally and on FTP.

* **Start with Windows**  
  DBackup can be configured to start automatically when you log in to Windows.

---

## 🛠️ Built With

* **C# WinForms** - Windows desktop interface
* **MySqlConnector** - MySQL connection
* **MySqlBackup** - SQL backup creation
* **ZIP compression** - Backup archiving
* **FTP upload** - Remote backup storage
* **Windows DPAPI** - Secure password encryption

---

## 📌 Coming Soon

* Email notifications for successful or failed backups
* Cloud backup support, such as Google Drive, Dropbox, or S3
* Advanced logging and troubleshooting options

---

## 📄 License

MIT License - see the [LICENSE](LICENSE) file for details.
