## 💾 DBackup - MySQL FTP Autobackup

**DBackup** este o aplicație desktop Windows simplă și eficientă, dezvoltată în **C# (WinForms)**, concepută pentru a automatiza procesul critic de backup al bazelor de date MySQL. Aplicația criptează datele sensibile (parole), realizează dump-uri SQL, le arhivează ZIP și le încarcă pe un server FTP, fiind ideală pentru utilizatorii care au nevoie de o soluție fiabilă și programată pentru protejarea datelor.

---

## 🚀 Caracteristici Cheie

* **Programare Automată (Scheduler):** Rulează automat backup-uri la o oră zilnică specificată, gestionat de un serviciu intern bazat pe fire de execuție (`SchedulerService.cs`).
* **Conectivitate Modernă:** Utilizează **MySqlConnector** pentru o conexiune rapidă și robustă la bazele de date MySQL.
* **Dump & Arhivare Integrată:** Realizează exportul direct al datelor SQL și arhivează fișierele rezultate în format **ZIP**, economisind spațiu de stocare.
* **Stocare Duală:**
    * **Local:** Salvează arhivele ZIP în directorul ales de utilizator.
    * **FTP:** Încarcă automat cele mai recente backup-uri pe serverul FTP configurat.
* **Securitate:** Credențialele sensibile (parolele MySQL și FTP) sunt criptate folosind **Windows Data Protection API (DPAPI)** înainte de a fi stocate în Registry (`SettingsService.cs`).
* **Auto-Curățare (Retention):** Șterge automat backup-urile vechi (atât local, cât și pe FTP) în funcție de numărul de zile de retenție configurat, pentru a gestiona spațiul de stocare.
* **Instalare Windows:** Poate fi instalată pentru a porni automat la logarea în sistem, utilizând **Scheduled Tasks** (sau Registry).

---

## 🛠️ Tehnologii Utilizate

* **C# (.NET Framework / WinForms):** Baza pentru interfața grafică și logica aplicației.
* **MySQL & MySqlConnector:** Pentru gestionarea conexiunilor și preluarea listei de baze de date.
* **MySqlBackup (Librărie):** Utilizată pentru a genera fișierele de tip SQL Dump.
* **FTP (System.Net.WebRequest):** Pentru comunicarea cu serverul de backup la distanță.
* **Windows Registry & DPAPI:** Pentru stocarea criptată a setărilor.

---

## 📌 Pe Drum (Coming Soon)

* ✉️ Notificări prin e-mail la finalizarea sau eșecul backup-ului.
* ☁️ Suport pentru servicii de stocare în cloud (Google Drive, Dropbox, S3).
* ⚙️ Opțiuni avansate de logare și depanare.

---

## 📄 Licență

MIT License - vezi fișierul [LICENSE](LICENSE) pentru detalii.
