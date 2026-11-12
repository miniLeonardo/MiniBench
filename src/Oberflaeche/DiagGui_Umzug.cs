// Seite System-Umzug (Teilklasse DiagGui)
public partial class DiagGui
{
    static long SafeDirectorySize(string path)
    {
        if (String.IsNullOrEmpty(path) || !Directory.Exists(path)) return 0;
        long total = 0;
        try
        {
            Queue<string> q = new Queue<string>();
            q.Enqueue(path);
            while (q.Count > 0)
            {
                string cur = q.Dequeue();
                try
                {
                    DirectoryInfo di = new DirectoryInfo(cur);
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        try { total += fi.Length; } catch { }
                    }
                    foreach (DirectoryInfo sub in di.GetDirectories())
                    {
                        try
                        {
                            if ((sub.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                            q.Enqueue(sub.FullName);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }
        catch { }
        return total;
    }

    static string FormatByteSize(long bytes)
    {
        if (bytes <= 0) return "0 B";
        if (bytes >= 1073741824L) return (bytes / 1073741824.0).ToString("N1") + " GB";
        if (bytes >= 1048576L) return (bytes / 1048576.0).ToString("N0") + " MB";
        if (bytes >= 1024L) return (bytes / 1024.0).ToString("N0") + " KB";
        return bytes + " B";
    }

    static string GetUserShellFolder(string regKeyName, Environment.SpecialFolder fallback)
    {
        try
        {
            using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders"))
            {
                if (rk != null)
                {
                    object val = rk.GetValue(regKeyName);
                    if (val != null)
                    {
                        string expanded = Environment.ExpandEnvironmentVariables(val.ToString());
                        if (Directory.Exists(expanded)) return expanded;
                    }
                }
            }
        }
        catch { }
        try { return Environment.GetFolderPath(fallback); } catch { return ""; }
    }

    static string GetDownloadsPath()
    {
        try
        {
            using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders"))
            {
                if (rk != null)
                {
                    object val = rk.GetValue("{374DE290-123F-4565-9164-39C4925E467B}");
                    if (val == null) val = rk.GetValue("{7D83EE9B-2244-4E70-B1F5-5393042AF1E4}");
                    if (val != null)
                    {
                        string expanded = Environment.ExpandEnvironmentVariables(val.ToString());
                        if (Directory.Exists(expanded)) return expanded;
                    }
                }
            }
        }
        catch { }
        string def = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        return Directory.Exists(def) ? def : "";
    }

    Control BuildUmzugPage()
    {
        FlowLayoutPanel f = Page("System-Umzug & Neuinstallation", "Sichert pers\u00F6nliche Dateien, Desktop-Layout und Programme vor einer Neuinstallation und stellt sie mit 1 Klick wieder her.", -1);

        // 1. Ziel-Auswahl
        f.Controls.Add(Section("Zielmedium f\u00FCr die Datensicherung"));

        TableLayoutPanel targetTable = new TableLayoutPanel();
        targetTable.AutoSize = true;
        targetTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        targetTable.ColumnCount = 2;
        targetTable.RowCount = 2;
        targetTable.BackColor = UI.Panel;
        targetTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        targetTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        DarkComboBox cmbDest = new DarkComboBox();
        cmbDest.Dock = DockStyle.Fill;
        cmbDest.Margin = new Padding(0, UI.S(2), UI.S(8), UI.S(2));
        Tip(cmbDest, "W\u00E4hlen Sie das Ziellaufwerk oder die Netzwerkfreigabe f\u00FCr die Sicherung aus.");

        Button btnBrowse = UI.Secondary("Durchsuchen ...");
        btnBrowse.Margin = new Padding(0, UI.S(2), 0, UI.S(2));
        Tip(btnBrowse, "Einen beliebigen Ordner auf einem lokalen Datentr\u00E4ger oder im Netzwerk ausw\u00E4hlen.");

        targetTable.Controls.Add(cmbDest, 0, 0);
        targetTable.Controls.Add(btnBrowse, 1, 0);

        Label lblFatWarn = Lbl("Achtung: Das Zielmedium ist mit FAT32 formatiert. Einzeldateien ab 4 GB k\u00F6nnen nicht gespeichert werden (NTFS oder exFAT empfohlen).", 8.5f, false, UI.Warn);
        lblFatWarn.AutoSize = true;
        lblFatWarn.Visible = false;
        lblFatWarn.Margin = new Padding(0, UI.S(6), 0, 0);
        lblFatWarn.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 32), 0);
        Tip(lblFatWarn, "FAT32 unterst\u00FCtzt technisch keine Dateien \u00FCber 4 GB. F\u00FCr gro\u00DFe Sicherungen wird exFAT oder NTFS empfohlen.");
        targetTable.Controls.Add(lblFatWarn, 0, 1);
        targetTable.SetColumnSpan(lblFatWarn, 2);

        f.Controls.Add(Card(targetTable));
        f.Controls.Add(Hint("Wechsellaufwerk (USB-Stick, externe SSD) oder konfigurierten NAS-Pfad als Sicherungsziel w\u00E4hlen.", 4));

        // 2. Umfangs-Auswahl (mit manuellem Schnell-Scan per Knopfdruck)
        f.Controls.Add(Section("Sicherungsumfang"));

        FlowLayoutPanel scopePanel = new FlowLayoutPanel();
        scopePanel.FlowDirection = FlowDirection.TopDown;
        scopePanel.WrapContents = false;
        scopePanel.AutoSize = true;
        scopePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        scopePanel.BackColor = UI.Panel;

        // Zeile für manuellen Schnell-Scan
        FlowLayoutPanel scanRow = Row();
        scanRow.Margin = new Padding(0, 0, 0, UI.S(8));

        Button btnStartScan = UI.Secondary("Schnell-Scan durchf\u00FChren");
        Tip(btnStartScan, "Pr\u00FCft diesen PC auf vorhandene Bibliotheken, Desktop-Layout und installierte Anwendungsprofile.");

        Label lblScanInfo = Lbl("Ermittelt vorab, welche Bibliotheken, Profile und Programme auf diesem PC vorhanden sind.", 8.5f, false, UI.Muted);
        lblScanInfo.Margin = new Padding(UI.S(10), UI.S(4), 0, 0);
        lblScanInfo.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 220), 0);
        Tip(lblScanInfo, "Status des Vorab-Scans der installierten Anwendungen und Verzeichnisse.");

        scanRow.Controls.Add(btnStartScan);
        scanRow.Controls.Add(lblScanInfo);
        scopePanel.Controls.Add(scanRow);

        // Granulare Bibliotheken
        FlowLayoutPanel libHeaderRow = Row();
        libHeaderRow.Margin = new Padding(0, 0, 0, UI.S(2));
        Label lblLibsHead = Lbl("Benutzer-Bibliotheken:", 9.5f, true, UI.Text);
        lblLibsHead.Margin = new Padding(0, UI.S(2), UI.S(8), UI.S(2));
        Tip(lblLibsHead, "Pers\u00F6nliche Standardbibliotheken des Benutzers ausw\u00E4hlen.");

        LinkLabel lnkLibsAll = new LinkLabel();
        lnkLibsAll.Text = "Alle an";
        lnkLibsAll.AutoSize = true;
        lnkLibsAll.Font = new Font("Segoe UI", 8.5f);
        lnkLibsAll.LinkColor = UI.Accent;
        lnkLibsAll.ActiveLinkColor = UI.Accent;
        lnkLibsAll.Margin = new Padding(0, UI.S(4), 0, UI.S(2));
        Tip(lnkLibsAll, "Alle Benutzer-Bibliotheken markieren.");

        Label lblLibsSlash = Lbl("/", 8.5f, false, UI.Muted);
        lblLibsSlash.Margin = new Padding(UI.S(2), UI.S(4), UI.S(2), UI.S(2));

        LinkLabel lnkLibsNone = new LinkLabel();
        lnkLibsNone.Text = "Alle aus";
        lnkLibsNone.AutoSize = true;
        lnkLibsNone.Font = new Font("Segoe UI", 8.5f);
        lnkLibsNone.LinkColor = UI.Accent;
        lnkLibsNone.ActiveLinkColor = UI.Accent;
        lnkLibsNone.Margin = new Padding(0, UI.S(4), 0, UI.S(2));
        Tip(lnkLibsNone, "Alle Benutzer-Bibliotheken abw\u00E4hlen.");

        libHeaderRow.Controls.Add(lblLibsHead);
        libHeaderRow.Controls.Add(lnkLibsAll);
        libHeaderRow.Controls.Add(lblLibsSlash);
        libHeaderRow.Controls.Add(lnkLibsNone);
        scopePanel.Controls.Add(libHeaderRow);

        FlowLayoutPanel libSubFlow = new FlowLayoutPanel();
        libSubFlow.FlowDirection = FlowDirection.TopDown;
        libSubFlow.WrapContents = false;
        libSubFlow.AutoSize = true;
        libSubFlow.Margin = new Padding(UI.S(16), 0, 0, UI.S(4));
        libSubFlow.BackColor = UI.Panel;

        CheckBox chkLibDesktop = Chk("Desktop", true);
        Tip(chkLibDesktop, "Sichert den Desktop-Ordner des aktuellen Benutzers.");
        libSubFlow.Controls.Add(chkLibDesktop);

        CheckBox chkLibDocs = Chk("Dokumente", true);
        Tip(chkLibDocs, "Sichert den Dokumente-Ordner des aktuellen Benutzers.");
        libSubFlow.Controls.Add(chkLibDocs);

        CheckBox chkLibDownloads = Chk("Downloads", false);
        Tip(chkLibDownloads, "Sichert den Downloads-Ordner (standardm\u00E4\u00DFig deaktiviert wegen oft hohem Datenvolumen).");
        libSubFlow.Controls.Add(chkLibDownloads);

        CheckBox chkLibPictures = Chk("Bilder", true);
        Tip(chkLibPictures, "Sichert den Bilder-Ordner des aktuellen Benutzers.");
        libSubFlow.Controls.Add(chkLibPictures);

        CheckBox chkLibMusic = Chk("Musik", true);
        Tip(chkLibMusic, "Sichert den Musik-Ordner des aktuellen Benutzers.");
        libSubFlow.Controls.Add(chkLibMusic);

        CheckBox chkLibVideos = Chk("Videos", true);
        Tip(chkLibVideos, "Sichert den Videos-Ordner des aktuellen Benutzers.");
        libSubFlow.Controls.Add(chkLibVideos);

        scopePanel.Controls.Add(libSubFlow);

        CheckBox chkLayout = Chk("Desktop-Layout (Icon-Positionen & Hintergrundbild)", true);
        Tip(chkLayout, "Sichert das aktive Desktop-Wallpaper sowie die Icon-Koordinaten.");
        scopePanel.Controls.Add(chkLayout);

        CheckBox chkSoftware = Chk("Software-Inventar (Winget-Paketliste & Programm-Report)", true);
        Tip(chkSoftware, "Erstellt einen Winget-Paketexport und einen Bericht \u00FCber manuell zu installierende Programme.");
        scopePanel.Controls.Add(chkSoftware);

        CheckBox chkProfiles = Chk("Profile & Konfigurationen (Configs)", false);
        Tip(chkProfiles, "Sichert Konfigurationsdateien, Sitzungen und Profile erkannter Anwendungen.");
        scopePanel.Controls.Add(chkProfiles);

        FlowLayoutPanel profSubFlow = new FlowLayoutPanel();
        profSubFlow.FlowDirection = FlowDirection.TopDown;
        profSubFlow.WrapContents = false;
        profSubFlow.AutoSize = true;
        profSubFlow.Margin = new Padding(UI.S(24), 0, 0, UI.S(4));
        profSubFlow.BackColor = UI.Panel;

        Label lblScanHint = Lbl("Klicke oben auf 'Schnell-Scan durchf\u00FChren', um installierte Browser, Mail-Clients, Editoren und Tools zu ermitteln.", 8.5f, false, UI.Muted);
        lblScanHint.Margin = new Padding(UI.S(4), UI.S(2), 0, UI.S(4));
        lblScanHint.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 60), 0);
        Tip(lblScanHint, "Hinweis zur Erkennung vorhandener Anwendungsprofile auf diesem Computer.");
        profSubFlow.Controls.Add(lblScanHint);

        scopePanel.Controls.Add(profSubFlow);
        CheckBox chkWlan = Chk("WLAN-Profile (gespeicherte Netzwerke)", true);
        Tip(chkWlan, "Sichert gespeicherte WLAN-Netzwerke zur automatischen Wiederherstellung.");
        scopePanel.Controls.Add(chkWlan);

        FlowLayoutPanel wlanSubFlow = new FlowLayoutPanel();
        wlanSubFlow.FlowDirection = FlowDirection.TopDown;
        wlanSubFlow.WrapContents = false;
        wlanSubFlow.AutoSize = true;
        wlanSubFlow.Margin = new Padding(UI.S(24), 0, 0, UI.S(4));
        wlanSubFlow.BackColor = UI.Panel;

        CheckBox chkWlanClear = Chk("WLAN-Passw\u00F6rter im Klartext mitsichern", false);
        Tip(chkWlanClear, "Achtung: Sichert Netzwerkschl\u00FCssel unverschl\u00FCsselt in XML-Dateien auf dem Zielmedium.");
        wlanSubFlow.Controls.Add(chkWlanClear);
        scopePanel.Controls.Add(wlanSubFlow);

        chkWlan.CheckedChanged += delegate {
            chkWlanClear.Enabled = chkWlan.Checked;
            if (!chkWlan.Checked) chkWlanClear.Checked = false;
        };

        chkWlanClear.Click += delegate {
            if (chkWlanClear.Checked)
            {
                DialogResult dr = MessageBox.Show(this,
                    "Sicherheits-Warnung:\r\n\r\n" +
                    "Wenn Sie diese Option aktivieren, werden alle gespeicherten WLAN-Passw\u00F6rter unverschl\u00FCsselt im Klartext in den exportierten XML-Dateien auf dem Zielmedium gespeichert.\r\n\r\n" +
                    "Jeder mit Zugriff auf das Zielmedium kann diese Passw\u00F6rter einsehen.\r\n\r\n" +
                    "M\u00F6chten Sie WLAN-Passw\u00F6rter wirklich im Klartext sichern?",
                    "WLAN-Passw\u00F6rter im Klartext sichern",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (dr != DialogResult.Yes)
                {
                    chkWlanClear.Checked = false;
                }
            }
        };

        FlowLayoutPanel customPanel = new FlowLayoutPanel();
        customPanel.FlowDirection = FlowDirection.TopDown;
        customPanel.WrapContents = false;
        customPanel.AutoSize = true;
        customPanel.Margin = new Padding(0, UI.S(6), 0, 0);
        customPanel.BackColor = UI.Panel;
        scopePanel.Controls.Add(customPanel);

        f.Controls.Add(Card(scopePanel));
        f.Controls.Add(Hint("Alle gew\u00E4hlten Daten und Profile werden in ein autarkes 1-Klick-Wiederherstellungspaket gespiegelt.", 4));

        // 3. Status- & Startbereich (inkl. Live-Log)
        f.Controls.Add(Section("Status & Sicherung"));

        FlowLayoutPanel statusPanel = new FlowLayoutPanel();
        statusPanel.FlowDirection = FlowDirection.TopDown;
        statusPanel.WrapContents = false;
        statusPanel.AutoSize = true;
        statusPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        statusPanel.BackColor = UI.Panel;

        Label lblSummary = Lbl("Pr\u00FCfe Speicherplatz ...", 9.5f, true, UI.Ok);
        lblSummary.Margin = new Padding(0, 0, 0, UI.S(8));
        lblSummary.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 32), 0);
        Tip(lblSummary, "Vergleich zwischen gemessenem Datenvolumen und freiem Speicher auf dem Zielmedium.");
        statusPanel.Controls.Add(lblSummary);

        Button btnStartUmzug = UI.Primary("Umzug-Sicherung starten");
        btnStartUmzug.Font = new Font("Segoe UI Semibold", 10.5f);
        btnStartUmzug.Padding = new Padding(UI.S(16), UI.S(6), UI.S(16), UI.S(6));
        btnStartUmzug.Margin = new Padding(0, 0, 0, UI.S(8));
        Tip(btnStartUmzug, "Startet die Erstellung der System-Umzug-Sicherung und des autarken Wiederherstellungsskripts auf dem gew\u00E4hlten Zielmedium.");
        statusPanel.Controls.Add(btnStartUmzug);

        Label lblUmzugProg = Lbl("Bereit zum Start.", 9f, true, UI.Accent);
        lblUmzugProg.Visible = false;
        lblUmzugProg.Margin = new Padding(0, 0, 0, UI.S(4));
        lblUmzugProg.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 32), 0);
        Tip(lblUmzugProg, "Zeigt den aktuellen Schritt des Sicherungslaufs an.");
        statusPanel.Controls.Add(lblUmzugProg);

        ProgressBar pbUmzug = new ProgressBar();
        pbUmzug.Width = UI.S(UI.Gruppenbreite - 32);
        pbUmzug.Height = UI.S(18);
        pbUmzug.Minimum = 0;
        pbUmzug.Maximum = 100;
        pbUmzug.Value = 0;
        pbUmzug.Visible = false;
        pbUmzug.Margin = new Padding(0, 0, 0, UI.S(4));
        Tip(pbUmzug, "Fortschrittsbalken der laufenden Sicherung.");
        statusPanel.Controls.Add(pbUmzug);

        Label lblUmzugDetail = Lbl("", 8.5f, false, UI.Muted);
        lblUmzugDetail.Visible = false;
        lblUmzugDetail.Margin = new Padding(0, 0, 0, UI.S(4));
        lblUmzugDetail.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 32), 0);
        Tip(lblUmzugDetail, "Zeigt die aktuell kopierte Datei oder das aktuelle Unterverzeichnis an.");
        statusPanel.Controls.Add(lblUmzugDetail);

        TextBox txtUmzugLog = new TextBox();
        txtUmzugLog.Multiline = true;
        txtUmzugLog.ReadOnly = true;
        txtUmzugLog.ScrollBars = ScrollBars.Vertical;
        txtUmzugLog.BackColor = UI.IsDark ? Color.FromArgb(24, 24, 24) : Color.FromArgb(245, 245, 245);
        txtUmzugLog.ForeColor = UI.IsDark ? Color.FromArgb(220, 220, 220) : Color.FromArgb(40, 40, 40);
        txtUmzugLog.Font = new Font("Consolas", 8.25f);
        txtUmzugLog.Width = UI.S(UI.Gruppenbreite - 32);
        txtUmzugLog.Height = UI.S(130);
        txtUmzugLog.Margin = new Padding(0, UI.S(4), 0, 0);
        txtUmzugLog.Visible = false;
        Tip(txtUmzugLog, "Laufendes Protokoll der Sicherung.");
        statusPanel.Controls.Add(txtUmzugLog);

        f.Controls.Add(Card(statusPanel));

        long sizeDesktop = 0, sizeDocs = 0, sizeDownloads = 0, sizePictures = 0, sizeMusic = 0, sizeVideos = 0;
        long sizeLayout = 10485760L;
        long sizeSoftware = 20971520L;
        long sizeProfBrowser = 314572800L;
        long sizeProfMail = 209715200L;
        long sizeProfDev = 104857600L;
        long sizeProfTools = 20971520L;
        long sizeProfTerm = 5242880L;
        List<long> customSizes = new List<long>();

        CheckBox chkProfBrowser = null;
        CheckBox chkProfMail = null;
        CheckBox chkProfDev = null;
        CheckBox chkProfTools = null;
        CheckBox chkProfTerm = null;
        List<CheckBox> customBoxes = new List<CheckBox>();
        List<string> customPaths = new List<string>();

        List<string> destPaths = new List<string>();
        List<string> destFs = new List<string>();
        List<long> destFree = new List<long>();

        Action refreshDrives = delegate {
            cmbDest.Items.Clear();
            destPaths.Clear();
            destFs.Clear();
            destFree.Clear();

            DriveInfo[] drvs = DriveInfo.GetDrives();
            // Systemlaufwerk einmal ermitteln; ohne Systemordner (Selbsttest unter Mono) bleibt es leer statt den Aufbau abzubrechen
            string sysRoot = ""; try { sysRoot = Path.GetPathRoot(Environment.SystemDirectory); } catch { }
            foreach (DriveInfo d in drvs)
            {
                if (!d.IsReady) continue;
                if (d.DriveType == DriveType.Removable || (d.DriveType == DriveType.Fixed && d.Name != sysRoot))
                {
                    long freeGb = d.AvailableFreeSpace / 1073741824L;
                    string fs = d.DriveFormat;
                    string itemText = "Laufwerk " + d.Name + " (" + (d.DriveType == DriveType.Removable ? "USB" : "Festplatte") + ", " + freeGb + " GB frei, " + fs + ")";
                    cmbDest.Items.Add(itemText);
                    destPaths.Add(d.Name);
                    destFs.Add(fs);
                    destFree.Add(d.AvailableFreeSpace);
                }
            }

            try
            {
                string nasPfad, nasUser;
                if (NasAblage.LeseKonfig(dataDir, out nasPfad, out nasUser) && !String.IsNullOrEmpty(nasPfad))
                {
                    string nasText = "Netzlaufwerk (NAS): " + nasPfad;
                    cmbDest.Items.Add(nasText);
                    destPaths.Add(nasPfad);
                    destFs.Add("Netzwerk");
                    destFree.Add(107374182400L);
                }
            }
            catch { }

            if (cmbDest.Items.Count > 0) cmbDest.SelectedIndex = 0;
            else
            {
                cmbDest.Items.Add("Kein Wechsellaufwerk gefunden (bitte USB-Stick einstecken oder Durchsuchen)");
                destPaths.Add("");
                destFs.Add("");
                destFree.Add(0);
                cmbDest.SelectedIndex = 0;
            }
        };

        refreshDrives();

        Action updateCalc = delegate {
            int selIdx = cmbDest.SelectedIndex;
            string curFs = (selIdx >= 0 && selIdx < destFs.Count) ? destFs[selIdx] : "";
            long curFree = (selIdx >= 0 && selIdx < destFree.Count) ? destFree[selIdx] : 0;
            bool isFat = curFs.IndexOf("FAT", StringComparison.OrdinalIgnoreCase) >= 0;
            lblFatWarn.Visible = isFat;

            long totalBytes = 0;
            if (chkLibDesktop.Checked) totalBytes += sizeDesktop;
            if (chkLibDocs.Checked) totalBytes += sizeDocs;
            if (chkLibDownloads.Checked) totalBytes += sizeDownloads;
            if (chkLibPictures.Checked) totalBytes += sizePictures;
            if (chkLibMusic.Checked) totalBytes += sizeMusic;
            if (chkLibVideos.Checked) totalBytes += sizeVideos;

            if (chkLayout.Checked) totalBytes += sizeLayout;
            if (chkSoftware.Checked) totalBytes += sizeSoftware;
            if (chkProfiles.Checked)
            {
                if (chkProfBrowser != null && chkProfBrowser.Checked) totalBytes += sizeProfBrowser;
                if (chkProfMail != null && chkProfMail.Checked) totalBytes += sizeProfMail;
                if (chkProfDev != null && chkProfDev.Checked) totalBytes += sizeProfDev;
                if (chkProfTools != null && chkProfTools.Checked) totalBytes += sizeProfTools;
                if (chkProfTerm != null && chkProfTerm.Checked) totalBytes += sizeProfTerm;
            }


            for (int i = 0; i < customBoxes.Count; i++)
            {
                if (customBoxes[i].Checked)
                {
                    totalBytes += (i < customSizes.Count) ? customSizes[i] : 0;
                }
            }

            double totalGb = totalBytes / 1073741824.0;
            double freeGb = curFree / 1073741824.0;
            string sizeStr = (totalGb >= 0.1 || totalBytes == 0) ? totalGb.ToString("N1") + " GB" : FormatByteSize(totalBytes);

            if (curFree > 0 && totalBytes > curFree)
            {
                lblSummary.Text = "Gesamtgr\u00F6\u00DFe: " + sizeStr + " \u00B7 Freier Speicher auf Ziel: " + freeGb.ToString("N1") + " GB (Nicht ausreichend Speicherplatz!)";
                lblSummary.ForeColor = UI.Crit;
            }
            else
            {
                lblSummary.Text = "Gesamtgr\u00F6\u00DFe: " + sizeStr + " \u00B7 Freier Speicher auf Ziel: " + freeGb.ToString("N1") + " GB (Ausreichend)";
                lblSummary.ForeColor = UI.Ok;
            }
        };

        cmbDest.SelectedIndexChanged += delegate { updateCalc(); };
        lnkLibsAll.LinkClicked += delegate {
            chkLibDesktop.Checked = true;
            chkLibDocs.Checked = true;
            chkLibDownloads.Checked = true;
            chkLibPictures.Checked = true;
            chkLibMusic.Checked = true;
            chkLibVideos.Checked = true;
            updateCalc();
        };
        lnkLibsNone.LinkClicked += delegate {
            chkLibDesktop.Checked = false;
            chkLibDocs.Checked = false;
            chkLibDownloads.Checked = false;
            chkLibPictures.Checked = false;
            chkLibMusic.Checked = false;
            chkLibVideos.Checked = false;
            updateCalc();
        };
        chkLibDesktop.CheckedChanged += delegate { updateCalc(); };
        chkLibDocs.CheckedChanged += delegate { updateCalc(); };
        chkLibDownloads.CheckedChanged += delegate { updateCalc(); };
        chkLibPictures.CheckedChanged += delegate { updateCalc(); };
        chkLibMusic.CheckedChanged += delegate { updateCalc(); };
        chkLibVideos.CheckedChanged += delegate { updateCalc(); };
        chkLayout.CheckedChanged += delegate { updateCalc(); };
        chkSoftware.CheckedChanged += delegate { updateCalc(); };
        chkProfiles.CheckedChanged += delegate {
            bool on = chkProfiles.Checked;
            if (chkProfBrowser != null) chkProfBrowser.Checked = on;
            if (chkProfMail != null) chkProfMail.Checked = on;
            if (chkProfDev != null) chkProfDev.Checked = on;
            if (chkProfTools != null) chkProfTools.Checked = on;
            if (chkProfTerm != null) chkProfTerm.Checked = on;
            updateCalc();
        };

        Action runScan = delegate {
            btnStartScan.Enabled = false;
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string roamApp = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string userProf = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string pDesktop = GetUserShellFolder("Desktop", Environment.SpecialFolder.Desktop);
            string pDocs = GetUserShellFolder("Personal", Environment.SpecialFolder.MyDocuments);
            string pDownloads = GetDownloadsPath();
            string pPictures = GetUserShellFolder("My Pictures", Environment.SpecialFolder.MyPictures);
            string pMusic = GetUserShellFolder("My Music", Environment.SpecialFolder.MyMusic);
            string pVideos = GetUserShellFolder("My Video", Environment.SpecialFolder.MyVideos);

            // Echte Größenmessung der Bibliotheken
            sizeDesktop = SafeDirectorySize(pDesktop);
            sizeDocs = SafeDirectorySize(pDocs);
            sizeDownloads = SafeDirectorySize(pDownloads);
            sizePictures = SafeDirectorySize(pPictures);
            sizeMusic = SafeDirectorySize(pMusic);
            sizeVideos = SafeDirectorySize(pVideos);

            chkLibDesktop.Text = "Desktop (" + FormatByteSize(sizeDesktop) + ")";
            chkLibDocs.Text = "Dokumente (" + FormatByteSize(sizeDocs) + ")";
            chkLibDownloads.Text = "Downloads (" + FormatByteSize(sizeDownloads) + ")";
            chkLibPictures.Text = "Bilder (" + FormatByteSize(sizePictures) + ")";
            chkLibMusic.Text = "Musik (" + FormatByteSize(sizeMusic) + ")";
            chkLibVideos.Text = "Videos (" + FormatByteSize(sizeVideos) + ")";

            int libCount = 0;
            if (Directory.Exists(pDesktop)) libCount++;
            if (Directory.Exists(pDocs)) libCount++;
            if (Directory.Exists(pDownloads)) libCount++;
            if (Directory.Exists(pPictures)) libCount++;
            if (Directory.Exists(pMusic)) libCount++;
            if (Directory.Exists(pVideos)) libCount++;

            bool hasWp = false;
            try { hasWp = File.Exists(Path.Combine(roamApp, "Microsoft\\Windows\\Themes\\TranscodedWallpaper")); } catch { }

            string wgPath;
            bool hasWg = IsWingetAvailable(out wgPath);

            List<string> foundBrowsers = new List<string>();
            if (Directory.Exists(Path.Combine(localApp, "Microsoft\\Edge\\User Data"))) foundBrowsers.Add("Microsoft Edge");
            if (Directory.Exists(Path.Combine(localApp, "Google\\Chrome\\User Data"))) foundBrowsers.Add("Google Chrome");
            if (Directory.Exists(Path.Combine(roamApp, "Mozilla\\Firefox\\Profiles")) || Directory.Exists(Path.Combine(roamApp, "Mozilla\\Firefox"))) foundBrowsers.Add("Mozilla Firefox");
            if (Directory.Exists(Path.Combine(localApp, "BraveSoftware\\Brave-Browser\\User Data"))) foundBrowsers.Add("Brave Browser");
            if (Directory.Exists(Path.Combine(roamApp, "Opera Software\\Opera Stable"))) foundBrowsers.Add("Opera");
            if (Directory.Exists(Path.Combine(roamApp, "Opera Software\\Opera GX Stable"))) foundBrowsers.Add("Opera GX");
            if (Directory.Exists(Path.Combine(localApp, "Vivaldi\\User Data"))) foundBrowsers.Add("Vivaldi");

            List<string> foundMail = new List<string>();
            if (Directory.Exists(Path.Combine(roamApp, "Thunderbird\\Profiles")) || Directory.Exists(Path.Combine(roamApp, "Thunderbird"))) foundMail.Add("Mozilla Thunderbird");

            List<string> foundDev = new List<string>();
            if (Directory.Exists(Path.Combine(roamApp, "Code\\User"))) foundDev.Add("VS Code");
            if (Directory.Exists(Path.Combine(roamApp, "Cursor\\User"))) foundDev.Add("Cursor Editor");
            if (Directory.Exists(Path.Combine(roamApp, "Notepad++"))) foundDev.Add("Notepad++");
            if (File.Exists(Path.Combine(userProf, ".gitconfig"))) foundDev.Add("Git (.gitconfig)");
            if (Directory.Exists(Path.Combine(docs, "WindowsPowerShell")) || Directory.Exists(Path.Combine(docs, "PowerShell"))) foundDev.Add("PowerShell Profile");

            List<string> foundTools = new List<string>();
            if (Directory.Exists(Path.Combine(roamApp, "FileZilla"))) foundTools.Add("FileZilla");
            if (File.Exists(Path.Combine(roamApp, "WinSCP.ini"))) foundTools.Add("WinSCP");
            if (Directory.Exists(Path.Combine(roamApp, "GHISLER"))) foundTools.Add("Total Commander");
            try {
                using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\SimonTatham\\PuTTY\\Sessions")) {
                    if (rk != null && rk.SubKeyCount > 0) foundTools.Add("PuTTY (" + rk.SubKeyCount + " Sessions)");
                }
            } catch { }

            bool hasTerminal = false;
            try {
                string pkgDir = Path.Combine(localApp, "Packages");
                if (Directory.Exists(pkgDir)) {
                    foreach (string d in Directory.GetDirectories(pkgDir, "Microsoft.WindowsTerminal_*")) {
                        if (File.Exists(Path.Combine(d, "LocalState\\settings.json"))) { hasTerminal = true; break; }
                    }
                }
            } catch { }

            int totalFound = libCount + (hasWp ? 1 : 0) + (hasWg ? 1 : 0) + foundBrowsers.Count + foundMail.Count + foundDev.Count + foundTools.Count + (hasTerminal ? 1 : 0);

            lblScanInfo.Text = "\u2713 Scan abgeschlossen: " + totalFound + " Komponenten und Profile auf diesem PC erkannt.";
            lblScanInfo.ForeColor = UI.Ok;

            profSubFlow.Controls.Clear();

            if (foundBrowsers.Count > 0) {
                chkProfBrowser = Chk("Webbrowser: " + String.Join(", ", foundBrowsers.ToArray()), true);
                Tip(chkProfBrowser, "Sichert Lesezeichen, Sitzungen und Einstellungen der erkannten Webbrowser.");
                chkProfBrowser.CheckedChanged += delegate { updateCalc(); };
                profSubFlow.Controls.Add(chkProfBrowser);
            }
            if (foundMail.Count > 0) {
                chkProfMail = Chk("E-Mail-Clients: " + String.Join(", ", foundMail.ToArray()), true);
                Tip(chkProfMail, "Sichert Kontoeinstellungen, Adressb\u00FCcher und E-Mail-Profile von Mozilla Thunderbird.");
                chkProfMail.CheckedChanged += delegate { updateCalc(); };
                profSubFlow.Controls.Add(chkProfMail);
            }
            if (foundDev.Count > 0) {
                chkProfDev = Chk("Entwickler & Editoren: " + String.Join(", ", foundDev.ToArray()), true);
                Tip(chkProfDev, "Sichert Benutzereinstellungen, Tastenbelegungen und Konfigurationsdateien von Entwicklungswerkzeugen.");
                chkProfDev.CheckedChanged += delegate { updateCalc(); };
                profSubFlow.Controls.Add(chkProfDev);
            }
            if (foundTools.Count > 0) {
                chkProfTools = Chk("Tools & Remote: " + String.Join(", ", foundTools.ToArray()), true);
                Tip(chkProfTools, "Sichert Serverlisten, FTP-Verbindungen und Session-Profile g\u00E4ngiger Hilfswerkzeuge.");
                chkProfTools.CheckedChanged += delegate { updateCalc(); };
                profSubFlow.Controls.Add(chkProfTools);
            }
            if (hasTerminal) {
                chkProfTerm = Chk("Windows Terminal & Konsole (settings.json)", true);
                Tip(chkProfTerm, "Sichert benutzerdefinierte Profile und Farbschemata des Windows Terminals.");
                chkProfTerm.CheckedChanged += delegate { updateCalc(); };
                profSubFlow.Controls.Add(chkProfTerm);
            }
            if (profSubFlow.Controls.Count == 0) {
                Label lblNoProfs = Hint("Keine spezifischen Anwendungsprofile auf diesem PC gefunden.", 0);
                Tip(lblNoProfs, "Es wurden keine Standardprofile f\u00FCr Browser, Mail oder Tools gefunden.");
                profSubFlow.Controls.Add(lblNoProfs);
            }

            customPanel.Controls.Clear();
            customBoxes.Clear();
            customPaths.Clear();
            customSizes.Clear();

            try {
                string[] sysBlack = new string[] {
                    "windows", "program files", "program files (x86)", "programdata",
                    "system volume information", "$recycle.bin", "$winreagent", "recovery",
                    "perflogs", "users", "msocache", "boot", "efi", "$sysreset", "$windows.~bt", "$windows.~ws", "documents and settings", "inetpub", "config.msi", "deliveryoptimization", "amd", "intel", "nvidia", "dell", "hp", "lenovo", "asus", "acer", "realtek", "drivers", "treiber", "swsetup", "drvdwn", "sources", "support", "temp", "tmp", "cache", "caches", "minidump", "crashdumps", "found.000", "found.001", "wsl", "vhds"
                };
                HashSet<string> blSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string b in sysBlack) blSet.Add(b);

                DirectoryInfo cRoot = new DirectoryInfo("C:\\");
                if (cRoot.Exists) {
                    foreach (DirectoryInfo di in cRoot.GetDirectories()) {
                        if (di.Name.StartsWith(".") || di.Name.StartsWith("$") || blSet.Contains(di.Name)) continue;
                        if ((di.Attributes & FileAttributes.System) != 0) continue;
                        if ((di.Attributes & FileAttributes.Hidden) != 0) continue;

                        long cSize = SafeDirectorySize(di.FullName);
                        string cSizeStr = FormatByteSize(cSize);
                        CheckBox cb = Chk("Ordner C:\\" + di.Name + (cSize > 0 ? " (" + cSizeStr + ")" : ""), false);
                        cb.Tag = di.FullName;
                        Tip(cb, "Sichert das Verzeichnis C:\\" + di.Name + " (" + cSizeStr + ") in den Unterordner C_Laufwerk_Zusatz.");
                        cb.CheckedChanged += delegate { updateCalc(); };
                        customBoxes.Add(cb);
                        customPaths.Add(di.FullName);
                        customSizes.Add(cSize);
                    }
                }
            } catch { }

            if (customBoxes.Count > 0) {
                Label lblCHead = Lbl("Erkannte Zusatzordner auf Laufwerk C:\\ (bei Bedarf w\u00E4hlen):", 9f, true, UI.Text);
                lblCHead.Margin = new Padding(0, UI.S(8), 0, UI.S(2));
                Tip(lblCHead, "Zusatzordner auf C:\\.");
                customPanel.Controls.Add(lblCHead);
                foreach (CheckBox cb in customBoxes) customPanel.Controls.Add(cb);
            } else {
                Label lblNoC = Hint("Keine zus\u00E4tzlichen Stammordner auf Laufwerk C:\\ gefunden.", 0);
                Tip(lblNoC, "Keine zus\u00E4tzlichen Ordner auf C:\\ vorhanden.");
                customPanel.Controls.Add(lblNoC);
            }

            btnStartScan.Enabled = true;
            updateCalc();
        };

        btnStartScan.Click += delegate { runScan(); };

        updateCalc();

        btnBrowse.Click += delegate {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "W\u00E4hlen Sie das Zielverzeichnis f\u00FCr die System-Umzug-Sicherung:";
                if (fbd.ShowDialog(this) == DialogResult.OK && !String.IsNullOrEmpty(fbd.SelectedPath))
                {
                    string p = fbd.SelectedPath;
                    string fs = "";
                    long free = 0;
                    try
                    {
                        string root = Path.GetPathRoot(p);
                        DriveInfo di = new DriveInfo(root);
                        fs = di.DriveFormat;
                        free = di.AvailableFreeSpace;
                    }
                    catch { }
                    string itemText = p + " (" + (free / 1073741824L) + " GB frei, " + fs + ")";
                    cmbDest.Items.Add(itemText);
                    destPaths.Add(p);
                    destFs.Add(fs);
                    destFree.Add(free);
                    cmbDest.SelectedIndex = cmbDest.Items.Count - 1;
                }
            }
        };

        btnStartUmzug.Click += delegate {
            int selIdx = cmbDest.SelectedIndex;
            if (selIdx < 0 || selIdx >= destPaths.Count || String.IsNullOrEmpty(destPaths[selIdx]))
            {
                MessageBox.Show(this, "Bitte w\u00E4hlen Sie ein g\u00FCltiges Zielmedium aus.", "System-Umzug", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string targetPath = destPaths[selIdx];

            List<string> scopes = new List<string>();
            if (chkLibDesktop.Checked) scopes.Add("Lib_Desktop");
            if (chkLibDocs.Checked) scopes.Add("Lib_Dokumente");
            if (chkLibDownloads.Checked) scopes.Add("Lib_Downloads");
            if (chkLibPictures.Checked) scopes.Add("Lib_Bilder");
            if (chkLibMusic.Checked) scopes.Add("Lib_Musik");
            if (chkLibVideos.Checked) scopes.Add("Lib_Videos");
            if (chkLayout.Checked) scopes.Add("Layout");
            if (chkSoftware.Checked) scopes.Add("Programme");
            if (chkProfiles.Checked)
            {
                scopes.Add("Profile");
                if (chkProfBrowser != null && chkProfBrowser.Checked) scopes.Add("Profile_Browser");
                if (chkProfMail != null && chkProfMail.Checked) scopes.Add("Profile_Mail");
                if (chkProfDev != null && chkProfDev.Checked) scopes.Add("Profile_Dev");
                if (chkProfTools != null && chkProfTools.Checked) scopes.Add("Profile_Tools");
                if (chkProfTerm != null && chkProfTerm.Checked) scopes.Add("Profile_Terminal");
            }

            if (chkWlan.Checked)
            {
                scopes.Add("Wlan");
                if (chkWlanClear.Checked) scopes.Add("Wlan_Klartext");
            }

            if (scopes.Count == 0 && customBoxes.Count == 0)
            {
                MessageBox.Show(this, "Bitte w\u00E4hlen Sie mindestens eine Sicherungskomponente aus.", "System-Umzug", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<string> chosenCustom = new List<string>();
            for (int i = 0; i < customBoxes.Count; i++)
            {
                if (customBoxes[i].Checked) chosenCustom.Add(customPaths[i]);
            }

            string scopeStr = String.Join(",", scopes.ToArray());
            string customStr = String.Join(";", chosenCustom.ToArray());

            string prompt = "Soll die Sicherung auf das folgende Zielmedium gestartet werden?\r\n\r\n" +
                            "Ziel: " + targetPath + "\r\n" +
                            "Umfang: " + (scopes.Count > 0 ? String.Join(", ", scopes.ToArray()) : "keine") + "\r\n" +
                            (chosenCustom.Count > 0 ? "Zusatzordner: " + chosenCustom.Count + " Ordner\r\n" : "") +
                            "\r\nEs wird ein autarkes Wiederherstellungspaket (Wiederherstellen.cmd) erzeugt.";

            if (MessageBox.Show(this, prompt, "System-Umzug starten", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            btnStartUmzug.Enabled = false;
            btnStartScan.Enabled = false;
            btnBrowse.Enabled = false;
            cmbDest.Enabled = false;
            lnkLibsAll.Enabled = false;
            lnkLibsNone.Enabled = false;
            chkLibDesktop.Enabled = false;
            chkLibDocs.Enabled = false;
            chkLibDownloads.Enabled = false;
            chkLibPictures.Enabled = false;
            chkLibMusic.Enabled = false;
            chkLibVideos.Enabled = false;
            chkLayout.Enabled = false;
            chkSoftware.Enabled = false;
            chkProfiles.Enabled = false;
            chkWlan.Enabled = false;
            chkWlanClear.Enabled = false;
            if (chkProfBrowser != null) chkProfBrowser.Enabled = false;
            if (chkProfMail != null) chkProfMail.Enabled = false;
            if (chkProfDev != null) chkProfDev.Enabled = false;
            if (chkProfTools != null) chkProfTools.Enabled = false;
            if (chkProfTerm != null) chkProfTerm.Enabled = false;
            foreach (CheckBox cb in customBoxes) cb.Enabled = false;

            lblUmzugProg.Text = "Sicherung wird gestartet ...";
            lblUmzugProg.Visible = true;
            pbUmzug.Value = 5;
            pbUmzug.Visible = true;
            txtUmzugLog.Visible = true;
            txtUmzugLog.Clear();
            txtUmzugLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] System-Umzug Sicherung initialisiert ...\r\n");
            txtUmzugLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] Ziel: " + targetPath + "\r\n");
            txtUmzugLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] Umfang: " + scopeStr + (chosenCustom.Count > 0 ? " (Zusatz: " + chosenCustom.Count + " Ordner)" : "") + "\r\n");

            string args = "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\" -EventMode -Umzug -UmzugZiel \"" + targetPath.TrimEnd('\\') + "\" -UmzugUmfang \"" + scopeStr + "\"" + (customStr.Length > 0 ? " -UmzugZusatz \"" + customStr + "\"" : "") + (dataDir.Length > 0 ? " -DatenDir \"" + dataDir.TrimEnd('\\') + "\"" : "");

            System.Threading.Thread t = new System.Threading.Thread(delegate() {
                string createdPath = "";
                bool isSuccess = false;
                string errMsg = "";

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo(psExe, args);
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardOutput = true;
                    psi.StandardOutputEncoding = new UTF8Encoding(false);

                    using (Process p = new Process())
                    {
                        p.StartInfo = psi;
                        p.OutputDataReceived += delegate(object s, DataReceivedEventArgs e) {
                            if (e.Data == null) return;
                            string line = e.Data.Trim();
                            if (line.Length == 0) return;

                            if (line.StartsWith("@@UMZUG|"))
                            {
                                string[] parts = line.Split(new char[] { '|' }, 4);
                                if (parts.Length >= 3)
                                {
                                    int pct;
                                    if (int.TryParse(parts[1], out pct))
                                    {
                                        string statusTxt = parts[2];
                                        string detailTxt = parts.Length >= 4 ? parts[3] : "";
                                        try
                                        {
                                            BeginInvoke(new MethodInvoker(delegate {
                                                pbUmzug.Value = Math.Max(0, Math.Min(100, pct));
                                                lblUmzugProg.Text = statusTxt;
                                                if (!String.IsNullOrEmpty(detailTxt))
                                                {
                                                    lblUmzugDetail.Text = "Kopiere: " + detailTxt;
                                                    lblUmzugDetail.Visible = true;
                                                }
                                                string logLine = "[" + DateTime.Now.ToString("HH:mm:ss") + "] [" + pct + "%] " + statusTxt + (!String.IsNullOrEmpty(detailTxt) ? " -> " + detailTxt : "");
                                                txtUmzugLog.AppendText(logLine + "\r\n");
                                                txtUmzugLog.SelectionStart = txtUmzugLog.TextLength;
                                                txtUmzugLog.ScrollToCaret();
                                            }));
                                        }
                                        catch { }
                                    }
                                }
                            }
                            else if (line.StartsWith("@@LOG|"))
                            {
                                string[] logParts = line.Split(new char[] { '|' }, 3);
                                string logTxt = logParts.Length >= 3 ? logParts[2] : (logParts.Length >= 2 ? logParts[1] : "");
                                if (!String.IsNullOrEmpty(logTxt))
                                {
                                    try
                                    {
                                        BeginInvoke(new MethodInvoker(delegate {
                                            txtUmzugLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + logTxt + "\r\n");
                                            txtUmzugLog.SelectionStart = txtUmzugLog.TextLength;
                                            txtUmzugLog.ScrollToCaret();
                                        }));
                                    }
                                    catch { }
                                }
                            }
                            else if (line.StartsWith("@@RESULT|"))
                            {
                                string[] resParts = line.Split(new char[] { '|' }, 5);
                                if (resParts.Length >= 2)
                                {
                                    isSuccess = resParts[1] == "1";
                                    if (resParts.Length >= 3) createdPath = resParts[2];
                                    if (resParts.Length >= 5) errMsg = resParts[4];
                                }
                            }
                            else if (!line.StartsWith("@@"))
                            {
                                try
                                {
                                    BeginInvoke(new MethodInvoker(delegate {
                                        txtUmzugLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + line + "\r\n");
                                        txtUmzugLog.SelectionStart = txtUmzugLog.TextLength;
                                        txtUmzugLog.ScrollToCaret();
                                    }));
                                }
                                catch { }
                            }
                        };
                        p.Start();
                        p.BeginOutputReadLine();
                        p.WaitForExit();
                        if (p.ExitCode == 0 && String.IsNullOrEmpty(errMsg)) isSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                }

                try
                {
                    BeginInvoke(new MethodInvoker(delegate {
                        btnStartUmzug.Enabled = true;
                        btnStartScan.Enabled = true;
                        btnBrowse.Enabled = true;
                        cmbDest.Enabled = true;
                        lnkLibsAll.Enabled = true;
                        lnkLibsNone.Enabled = true;
                        chkLibDesktop.Enabled = true;
                        chkLibDocs.Enabled = true;
                        chkLibDownloads.Enabled = true;
                        chkLibPictures.Enabled = true;
                        chkLibMusic.Enabled = true;
                        chkLibVideos.Enabled = true;
                        chkLayout.Enabled = true;
                        chkSoftware.Enabled = true;
                        chkProfiles.Enabled = true;
                        chkWlan.Enabled = true;
                        chkWlanClear.Enabled = chkWlan.Checked;
                        if (chkProfBrowser != null) chkProfBrowser.Enabled = true;
                        if (chkProfMail != null) chkProfMail.Enabled = true;
                        if (chkProfDev != null) chkProfDev.Enabled = true;
                        if (chkProfTools != null) chkProfTools.Enabled = true;
                        if (chkProfTerm != null) chkProfTerm.Enabled = true;
                        lblUmzugDetail.Visible = false;
                        foreach (CheckBox cb in customBoxes) cb.Enabled = true;

                        if (isSuccess)
                        {
                            pbUmzug.Value = 100;
                            lblUmzugProg.Text = "Sicherung erfolgreich abgeschlossen.";
                            txtUmzugLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] Sicherung erfolgreich abgeschlossen: " + createdPath + "\r\n");
                            txtUmzugLog.SelectionStart = txtUmzugLog.TextLength;
                            txtUmzugLog.ScrollToCaret();

                            string okMsg = "Die System-Umzug-Sicherung wurde erfolgreich erstellt:\r\n\r\n" +
                                           createdPath + "\r\n\r\n" +
                                           "Auf dem neu installierten Windows-PC einfach die Datei\r\n'Wiederherstellen.cmd' im Sicherungsordner als Administrator starten.\r\n\r\n" +
                                           "M\u00F6chten Sie den Sicherungsordner jetzt im Explorer \u00F6ffnen?";
                            if (MessageBox.Show(this, okMsg, "System-Umzug abgeschlossen", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                            {
                                if (!String.IsNullOrEmpty(createdPath) && Directory.Exists(createdPath))
                                {
                                    OpenShell(createdPath);
                                }
                            }
                        }
                        else
                        {
                            lblUmzugProg.Text = "Fehler bei der Sicherung: " + errMsg;
                            txtUmzugLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] FEHLER: " + errMsg + "\r\n");
                            txtUmzugLog.SelectionStart = txtUmzugLog.TextLength;
                            txtUmzugLog.ScrollToCaret();
                            MessageBox.Show(this, "Die Sicherung konnte nicht erfolgreich abgeschlossen werden:\r\n\r\n" + errMsg, "System-Umzug Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                }
                catch { }
            });
            t.IsBackground = true;
            t.Start();
        };

        return f;
    }
}
