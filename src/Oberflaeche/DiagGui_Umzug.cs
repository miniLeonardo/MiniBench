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

    // ab v4.09: Seite im Muster von Benchmark und Diagnose (bis v4.08 Abschnitte mit Kontrollkästchen). Links der Umfang
    // in drei Karten (Benutzer-Bibliotheken, System und Programme, Zusatzordner), rechts Zielmedium und Sicherung mit
    // Fortschritt. Die Kontrollkästchen bleiben als Werte bestehen, angezeigt werden Zeilen mit Schaltern (SchalterZeile);
    // Profile und Zusatzordner, die erst der Schnell-Scan findet, stehen als Auswahlzeilen unter ihrer Zeile bzw. in
    // ihrer Karte.
    internal ModulSeite umzugSeite;
    internal SettingRow[] umzugLibRows;
    internal SettingRow[] umzugSysRows;
    internal UnterBereich subUmzugProfile, subUmzugWlan;
    // Sichtbarkeit der Bereiche unter Profile und WLAN als eigener Zustand (für Tests ohne angezeigtes Fenster)
    internal bool subUmzugProfileOffen, subUmzugWlanOffen = true;
    internal ZeilenKarte karteUmzugZusatz;
    internal Label lblUmzugGroesse, lblUmzugPlatz;
    // Größe und freien Platz neu rechnen und die Farben der Hinweise setzen (auch nach einem Wechsel des Farbschemas)
    internal Action umzugRechne;

    // Zeile zum freien Platz auf dem Ziel. Ohne Zielmedium und bei Zielen ohne lesbaren freien Platz (Netzlaufwerk) steht
    // das da; "ausreichend" gibt es nur mit gemessenem freiem Platz und gemessenem Umfang.
    // stufe: 0 gedämpft, 1 gut, 2 kritisch, 3 Warnung
    public static string UmzugPlatzText(string zielPfad, string dateisystem, long frei, long bedarf, bool gemessen, out int stufe)
    {
        stufe = 0;
        if (String.IsNullOrEmpty(zielPfad)) { stufe = 3; return "Kein Zielmedium gewählt"; }
        if (String.Equals(dateisystem, "Netzwerk", StringComparison.OrdinalIgnoreCase)) return "Netzlaufwerk: Der freie Platz wird nicht geprüft";
        if (frei <= 0) return "Freier Platz auf dem Ziel nicht lesbar";
        string f = "Frei auf dem Ziel: " + (frei / 1073741824.0).ToString("N1") + " GB";
        if (bedarf > frei) { stufe = 2; return f + ", nicht ausreichend"; }
        if (!gemessen) return f;
        stufe = 1; return f + ", ausreichend";
    }

    Control BuildUmzugPage()
    {
        FlowLayoutPanel f = Page("System-Umzug & Neuinstallation", "Sichert pers\u00F6nliche Dateien, Desktop-Layout und Programme vor einer Neuinstallation und stellt sie mit 1 Klick wieder her.", -1);
        umzugSeite = NeueModulSeite(f);

        // 1. Umfang, links: Benutzer-Bibliotheken (Größe je Ordner nach dem Schnell-Scan)
        ZeilenKarte kLib = umzugSeite.Karte("Benutzer-Bibliotheken", true);
        LinkLabel lnkLibsAll = KartenVerweis("Alle an"), lnkLibsNone = KartenVerweis("Alle aus");
        Tip(lnkLibsAll, "Alle Benutzer-Bibliotheken einschalten.");
        Tip(lnkLibsNone, "Alle Benutzer-Bibliotheken ausschalten.");
        kLib.KopfRechts.Controls.Add(lnkLibsAll); kLib.KopfRechts.Controls.Add(lnkLibsNone);

        string[] libTitel = new string[] { "Desktop", "Dokumente", "Downloads", "Bilder", "Musik", "Videos" };
        string[] libScope = new string[] { "Lib_Desktop", "Lib_Dokumente", "Lib_Downloads", "Lib_Bilder", "Lib_Musik", "Lib_Videos" };
        string[] libDetail = new string[] {
            "Dateien und Ordner auf dem Desktop", "Pers\u00F6nliche Dokumente", "Oft gro\u00DF, deshalb zun\u00E4chst aus",
            "Fotos und Bildschirmfotos", "Musiksammlung", "Videosammlung" };
        string[] libIcons = new string[] { UI.IcoBildschirm, UI.IcoDokument, UI.IcoHerunterladen, UI.IcoBild, UI.IcoMusik, UI.IcoVideo };
        string[] libTips = new string[] {
            "Sichert den Desktop-Ordner des aktuellen Benutzers.",
            "Sichert den Dokumente-Ordner des aktuellen Benutzers.",
            "Sichert den Downloads-Ordner (standardm\u00E4\u00DFig deaktiviert wegen oft hohem Datenvolumen).",
            "Sichert den Bilder-Ordner des aktuellen Benutzers.",
            "Sichert den Musik-Ordner des aktuellen Benutzers.",
            "Sichert den Videos-Ordner des aktuellen Benutzers." };
        CheckBox[] libChk = new CheckBox[libTitel.Length]; Badge[] libGroesse = new Badge[libTitel.Length]; long[] libSize = new long[libTitel.Length];
        umzugLibRows = new SettingRow[libTitel.Length];
        for (int i = 0; i < libTitel.Length; i++)
        {
            libChk[i] = Chk(libTitel[i], i != 2);
            umzugLibRows[i] = SchalterZeile(libChk[i], libTitel[i], libDetail[i], libIcons[i], libTips[i], null);
            libGroesse[i] = new Badge("", "skip"); Tip(libGroesse[i], "Gr\u00F6\u00DFe des Ordners, gemessen beim Schnell-Scan.");
            umzugLibRows[i].AddBadge(libGroesse[i]);
            kLib.Zeile(umzugLibRows[i]);
        }

        // System und Programme; Profile und das Klartext-Kennwort der WLAN-Profile stehen unter ihrer Zeile
        ZeilenKarte kSys = umzugSeite.Karte("System und Programme", true);
        CheckBox chkLayout = Chk("Desktop-Layout", true), chkSoftware = Chk("Software-Inventar", true), chkProfiles = Chk("Profile und Konfigurationen", false), chkWlan = Chk("WLAN-Profile", true);
        SettingRow rLayout = SchalterZeile(chkLayout, "Desktop-Layout", "Hintergrundbild und Positionen der Symbole", UI.IcoAnpassen,
            "Sichert das aktive Desktop-Wallpaper sowie die Icon-Koordinaten.", null);
        SettingRow rSoft = SchalterZeile(chkSoftware, "Software-Inventar", "Winget-Paketliste und Bericht \u00FCber Programme, die von Hand zu installieren sind", UI.IcoApp,
            "Erstellt einen Winget-Paketexport und einen Bericht \u00FCber manuell zu installierende Programme.", null);
        SettingRow rProf = SchalterZeile(chkProfiles, "Profile und Konfigurationen", "Browser, E-Mail, Editoren und Werkzeuge", UI.IcoKontakt,
            "Sichert Konfigurationsdateien, Sitzungen und Profile erkannter Anwendungen.", null);
        subUmzugProfile = new UnterBereich(); subUmzugProfile.Visible = false;
        Label lblScanHint = Lbl("Der Schnell-Scan ermittelt installierte Browser, E-Mail-Programme, Editoren und Werkzeuge. Danach lassen sich die Profile hier einzeln w\u00E4hlen.", 8.5f, false, UI.Muted);
        lblScanHint.Margin = new Padding(0, 0, 0, UI.S(2));
        subUmzugProfile.Inhalt.Controls.Add(lblScanHint);
        SettingRow rWlan = SchalterZeile(chkWlan, "WLAN-Profile", "Gespeicherte Netzwerke zur automatischen Wiederherstellung", UI.IcoWlan,
            "Sichert gespeicherte WLAN-Netzwerke zur automatischen Wiederherstellung.", null);
        subUmzugWlan = new UnterBereich();
        LaufwerkZeile zWlanClear = new LaufwerkZeile("WLAN-Passw\u00F6rter im Klartext mitsichern", "", "unverschl\u00FCsselt auf dem Zielmedium", false);
        Tip(zWlanClear, "Achtung: Sichert Netzwerkschl\u00FCssel unverschl\u00FCsselt in XML-Dateien auf dem Zielmedium.");
        subUmzugWlan.Inhalt.Controls.Add(zWlanClear);
        kSys.Zeile(rLayout); kSys.Zeile(rSoft); kSys.Zeile(rProf); kSys.Zeile(subUmzugProfile); kSys.Zeile(rWlan); kSys.Zeile(subUmzugWlan);
        umzugSysRows = new SettingRow[] { rLayout, rSoft, rProf, rWlan };

        // Einschalten fragt nach (der Schlüssel liegt danach lesbar auf dem Zielmedium); bei Nein bleibt die Zeile aus
        zWlanClear.CheckedChanged += delegate {
            if (!zWlanClear.Checked) return;
            DialogResult dr = MessageBox.Show(this,
                "Sicherheits-Warnung:\r\n\r\n" +
                "Wenn Sie diese Option aktivieren, werden alle gespeicherten WLAN-Passw\u00F6rter unverschl\u00FCsselt im Klartext in den exportierten XML-Dateien auf dem Zielmedium gespeichert.\r\n\r\n" +
                "Jeder mit Zugriff auf das Zielmedium kann diese Passw\u00F6rter einsehen.\r\n\r\n" +
                "M\u00F6chten Sie WLAN-Passw\u00F6rter wirklich im Klartext sichern?",
                "WLAN-Passw\u00F6rter im Klartext sichern",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (dr != DialogResult.Yes) zWlanClear.Checked = false;
        };

        // Zusatzordner im Stamm von C: (findet der Schnell-Scan)
        karteUmzugZusatz = umzugSeite.Karte("Zusatzordner auf Laufwerk C:", true);
        Label lblZusatz = Lbl("Der Schnell-Scan sucht weitere Ordner im Stamm von Laufwerk C: und bietet sie hier zur Wahl an.", 8.5f, false, UI.Muted);
        lblZusatz.Margin = new Padding(0, 0, 0, UI.S(2));
        karteUmzugZusatz.Frei(lblZusatz);

        // 2. Zielmedium, rechts
        ZeilenKarte kZiel = umzugSeite.Karte("Zielmedium", false);
        DarkComboBox cmbDest = new DarkComboBox();
        cmbDest.Margin = new Padding(0, 0, 0, UI.S(8));
        Tip(cmbDest, "W\u00E4hlen Sie das Ziellaufwerk oder die Netzwerkfreigabe f\u00FCr die Sicherung aus.");
        kZiel.Zeile(cmbDest);

        Button btnBrowse = UI.Secondary("Durchsuchen \u2026");
        btnBrowse.Margin = new Padding(0, 0, 0, UI.S(8));
        Tip(btnBrowse, "Einen beliebigen Ordner auf einem lokalen Datentr\u00E4ger oder im Netzwerk ausw\u00E4hlen.");
        kZiel.Frei(btnBrowse);

        Label lblFatWarn = Lbl("Achtung: Das Zielmedium ist mit FAT32 formatiert. Einzeldateien ab 4 GB k\u00F6nnen nicht gespeichert werden (NTFS oder exFAT empfohlen).", 8.5f, false, UI.Warn);
        lblFatWarn.Visible = false;
        lblFatWarn.Margin = new Padding(0, 0, 0, UI.S(6));
        Tip(lblFatWarn, "FAT32 unterst\u00FCtzt technisch keine Dateien \u00FCber 4 GB. F\u00FCr gro\u00DFe Sicherungen wird exFAT oder NTFS empfohlen.");
        kZiel.Frei(lblFatWarn);
        Label hZiel = Lbl("Wechsellaufwerk (USB-Stick, externe SSD) oder konfigurierten NAS-Pfad als Sicherungsziel w\u00E4hlen.", 8.5f, false, UI.Muted);
        hZiel.Margin = new Padding(0, 0, 0, UI.S(2));
        kZiel.Frei(hZiel);

        // 3. Sicherung: Größe, freier Platz, Start, Schnell-Scan, Fortschritt und Protokoll
        ZeilenKarte kSich = umzugSeite.Karte("Sicherung", false);
        Label lblSummary = Lbl("Gesamtgr\u00F6\u00DFe: nach dem Schnell-Scan", 10.5f, true, UI.Text);
        lblSummary.Margin = new Padding(0, 0, 0, UI.S(2));
        Tip(lblSummary, "Umfang der gew\u00E4hlten Bereiche: Bibliotheken und Zusatzordner gemessen beim Schnell-Scan, Layout, Software-Inventar und Profile gesch\u00E4tzt.");
        kSich.Frei(lblSummary); lblUmzugGroesse = lblSummary;
        Label lblPlatz = Lbl("", UI.SchriftText, false, UI.Muted);
        lblPlatz.Margin = new Padding(0, 0, 0, UI.S(10));
        Tip(lblPlatz, "Vergleich zwischen dem Umfang der Sicherung und dem freien Speicher auf dem Zielmedium.");
        kSich.Frei(lblPlatz); lblUmzugPlatz = lblPlatz;

        FlowLayoutPanel knoepfe = Row();
        knoepfe.Margin = new Padding(0, 0, 0, UI.S(8));
        Button btnStartUmzug = UI.Primary("Sicherung starten");
        btnStartUmzug.Font = new Font("Segoe UI Semibold", 10f);
        btnStartUmzug.Padding = new Padding(UI.S(14), UI.S(4), UI.S(14), UI.S(4));
        btnStartUmzug.Margin = new Padding(0, 0, UI.S(8), 0);
        Tip(btnStartUmzug, "Startet die Erstellung der System-Umzug-Sicherung und des autarken Wiederherstellungsskripts auf dem gew\u00E4hlten Zielmedium.");
        Button btnStartScan = UI.Secondary("Schnell-Scan");
        btnStartScan.Margin = new Padding(0);
        btnStartScan.Padding = new Padding(UI.S(12), UI.S(4), UI.S(12), UI.S(4));
        Tip(btnStartScan, "Pr\u00FCft diesen PC auf vorhandene Bibliotheken, Desktop-Layout und installierte Anwendungsprofile und misst die Gr\u00F6\u00DFe der Ordner.");
        knoepfe.Controls.Add(btnStartUmzug); knoepfe.Controls.Add(btnStartScan);
        kSich.Frei(knoepfe);

        bool gescannt = false;
        Label lblScanInfo = Lbl("Der Schnell-Scan misst die Bibliotheken und sucht Profile, Programme und weitere Ordner auf diesem PC.", 8.5f, false, UI.Muted);
        lblScanInfo.Margin = new Padding(0, 0, 0, UI.S(4));
        Tip(lblScanInfo, "Status des Vorab-Scans der installierten Anwendungen und Verzeichnisse.");
        kSich.Frei(lblScanInfo);
        Label hSich = Lbl("Alle gew\u00E4hlten Daten und Profile werden in ein autarkes 1-Klick-Wiederherstellungspaket gespiegelt.", 8.5f, false, UI.Muted);
        hSich.Margin = new Padding(0, 0, 0, UI.S(2));
        kSich.Frei(hSich);

        Label lblUmzugProg = Lbl("Bereit zum Start.", 9f, true, UI.Accent);
        lblUmzugProg.Tag = "accent";
        lblUmzugProg.Visible = false;
        lblUmzugProg.Margin = new Padding(0, UI.S(8), 0, UI.S(4));
        Tip(lblUmzugProg, "Zeigt den aktuellen Schritt des Sicherungslaufs an.");
        kSich.Frei(lblUmzugProg);

        FlatBar pbUmzug = new FlatBar();
        pbUmzug.Height = UI.S(8);
        pbUmzug.Fill = UI.Accent;
        pbUmzug.Visible = false;
        pbUmzug.Margin = new Padding(0, 0, 0, UI.S(4));
        Tip(pbUmzug, "Fortschrittsbalken der laufenden Sicherung.");
        kSich.Zeile(pbUmzug);

        Label lblUmzugDetail = Lbl("", 8.5f, false, UI.Muted);
        lblUmzugDetail.Visible = false;
        lblUmzugDetail.Margin = new Padding(0, 0, 0, UI.S(4));
        Tip(lblUmzugDetail, "Zeigt die aktuell kopierte Datei oder das aktuelle Unterverzeichnis an.");
        kSich.Frei(lblUmzugDetail);

        TextBox txtUmzugLog = new TextBox();
        txtUmzugLog.Multiline = true;
        txtUmzugLog.ReadOnly = true;
        txtUmzugLog.ScrollBars = ScrollBars.Vertical;
        txtUmzugLog.BorderStyle = BorderStyle.FixedSingle;
        txtUmzugLog.BackColor = UI.IsDark ? Color.FromArgb(24, 24, 24) : Color.FromArgb(245, 245, 245);
        txtUmzugLog.ForeColor = UI.IsDark ? Color.FromArgb(220, 220, 220) : Color.FromArgb(40, 40, 40);
        txtUmzugLog.Font = new Font("Consolas", 8.25f);
        txtUmzugLog.Height = UI.S(150);
        txtUmzugLog.Margin = new Padding(0, UI.S(4), 0, UI.S(2));
        txtUmzugLog.Visible = false;
        Tip(txtUmzugLog, "Laufendes Protokoll der Sicherung.");
        kSich.Zeile(txtUmzugLog);

        f.Controls.Add(umzugSeite.Spalten);

        // Geschätzter Umfang der Bereiche ohne eigene Messung
        long sizeLayout = 10485760L;
        long sizeSoftware = 20971520L;
        long sizeProfBrowser = 314572800L;
        long sizeProfMail = 209715200L;
        long sizeProfDev = 104857600L;
        long sizeProfTools = 20971520L;
        long sizeProfTerm = 5242880L;
        List<long> customSizes = new List<long>();

        // Auswahlzeilen der Profile (legt der Schnell-Scan an) und der Zusatzordner
        LaufwerkZeile chkProfBrowser = null;
        LaufwerkZeile chkProfMail = null;
        LaufwerkZeile chkProfDev = null;
        LaufwerkZeile chkProfTools = null;
        LaufwerkZeile chkProfTerm = null;
        List<LaufwerkZeile> customBoxes = new List<LaufwerkZeile>();
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
                    // ab v4.09 ohne angenommenen freien Platz: Der Start greift nie auf das NAS zu, die Seite nennt das
                    // (bis v4.08 standen hier fest 100 GB, die Seite meldete damit immer "Ausreichend")
                    destFree.Add(0);
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
            string curPath = (selIdx >= 0 && selIdx < destPaths.Count) ? destPaths[selIdx] : "";
            string curFs = (selIdx >= 0 && selIdx < destFs.Count) ? destFs[selIdx] : "";
            long curFree = (selIdx >= 0 && selIdx < destFree.Count) ? destFree[selIdx] : 0;
            bool isFat = curFs.IndexOf("FAT", StringComparison.OrdinalIgnoreCase) >= 0 && curFs.IndexOf("exFAT", StringComparison.OrdinalIgnoreCase) < 0;
            lblFatWarn.Visible = isFat;
            lblFatWarn.ForeColor = UI.Warn;

            long totalBytes = 0; int gewaehlt = 0, bereiche = libChk.Length + 4 + customBoxes.Count;
            for (int i = 0; i < libChk.Length; i++) if (libChk[i].Checked) { totalBytes += libSize[i]; gewaehlt++; }

            if (chkLayout.Checked) { totalBytes += sizeLayout; gewaehlt++; }
            if (chkSoftware.Checked) { totalBytes += sizeSoftware; gewaehlt++; }
            if (chkProfiles.Checked)
            {
                gewaehlt++;
                if (chkProfBrowser != null && chkProfBrowser.Checked) totalBytes += sizeProfBrowser;
                if (chkProfMail != null && chkProfMail.Checked) totalBytes += sizeProfMail;
                if (chkProfDev != null && chkProfDev.Checked) totalBytes += sizeProfDev;
                if (chkProfTools != null && chkProfTools.Checked) totalBytes += sizeProfTools;
                if (chkProfTerm != null && chkProfTerm.Checked) totalBytes += sizeProfTerm;
            }
            if (chkWlan.Checked) gewaehlt++;

            for (int i = 0; i < customBoxes.Count; i++)
            {
                if (customBoxes[i].Checked)
                {
                    totalBytes += (i < customSizes.Count) ? customSizes[i] : 0;
                    gewaehlt++;
                }
            }

            // Bereiche unter Profile und WLAN nur bei eingeschalteter Zeile
            if (subUmzugProfileOffen != chkProfiles.Checked) { subUmzugProfileOffen = chkProfiles.Checked; subUmzugProfile.Visible = subUmzugProfileOffen; }
            if (subUmzugWlanOffen != chkWlan.Checked) { subUmzugWlanOffen = chkWlan.Checked; subUmzugWlan.Visible = subUmzugWlanOffen; }

            double totalGb = totalBytes / 1073741824.0;
            string sizeStr = (totalGb >= 0.1 || totalBytes == 0) ? totalGb.ToString("N1") + " GB" : FormatByteSize(totalBytes);
            SetText(umzugSeite.Anzahl, gewaehlt + " von " + bereiche + " Bereichen");
            SetText(umzugSeite.Dauer, gescannt ? "rund " + sizeStr : "Gr\u00F6\u00DFe nach dem Schnell-Scan");
            SetText(lblSummary, gescannt ? "Gesamtgr\u00F6\u00DFe: rund " + sizeStr : "Gesamtgr\u00F6\u00DFe: nach dem Schnell-Scan");
            int stufe;
            SetText(lblPlatz, UmzugPlatzText(curPath, curFs, curFree, totalBytes, gescannt, out stufe));
            lblPlatz.ForeColor = stufe == 1 ? UI.Ok : stufe == 2 ? UI.Crit : stufe == 3 ? UI.Warn : UI.Muted;
            lblScanInfo.ForeColor = gescannt ? UI.Ok : UI.Muted;
            pbUmzug.Fill = UI.Accent;
        };
        umzugRechne = updateCalc;

        cmbDest.SelectedIndexChanged += delegate { updateCalc(); };
        lnkLibsAll.LinkClicked += delegate { foreach (CheckBox c in libChk) c.Checked = true; };
        lnkLibsNone.LinkClicked += delegate { foreach (CheckBox c in libChk) c.Checked = false; };
        foreach (CheckBox c in libChk) c.CheckedChanged += delegate { updateCalc(); };
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
        chkWlan.CheckedChanged += delegate {
            if (!chkWlan.Checked) zWlanClear.Checked = false;
            updateCalc();
        };

        // Auswahlzeile eines Profils im Bereich unter "Profile und Konfigurationen"
        Func<string, List<string>, string, LaufwerkZeile> profilZeile = delegate(string titel, List<string> gefunden, string hinweis) {
            string liste = gefunden != null ? String.Join(", ", gefunden.ToArray()) : "";
            LaufwerkZeile z = new LaufwerkZeile(titel, "", liste, true);
            Tip(z, hinweis + (liste.Length > 0 ? "\r\nErkannt: " + liste : ""));
            z.CheckedChanged += delegate { updateCalc(); };
            subUmzugProfile.Inhalt.Controls.Add(z);
            return z;
        };

        Action runScan = delegate {
            btnStartScan.Enabled = false;
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string roamApp = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string userProf = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string[] libPfad = new string[] {
                GetUserShellFolder("Desktop", Environment.SpecialFolder.Desktop),
                GetUserShellFolder("Personal", Environment.SpecialFolder.MyDocuments),
                GetDownloadsPath(),
                GetUserShellFolder("My Pictures", Environment.SpecialFolder.MyPictures),
                GetUserShellFolder("My Music", Environment.SpecialFolder.MyMusic),
                GetUserShellFolder("My Video", Environment.SpecialFolder.MyVideos) };

            // Echte Größenmessung der Bibliotheken: Größe als Etikett, der Ordner als Beschreibung der Zeile
            int libCount = 0;
            for (int i = 0; i < libPfad.Length; i++)
            {
                bool da = !String.IsNullOrEmpty(libPfad[i]) && Directory.Exists(libPfad[i]);
                libSize[i] = SafeDirectorySize(libPfad[i]);
                SetText(libGroesse[i], da ? FormatByteSize(libSize[i]) : "");
                umzugLibRows[i].Description = da ? libPfad[i] : "Ordner auf diesem PC nicht gefunden";
                if (da) libCount++;
            }

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

            gescannt = true;
            lblScanInfo.Text = "\u2713 Scan abgeschlossen: " + totalFound + " Komponenten und Profile auf diesem PC erkannt.";
            lblScanInfo.ForeColor = UI.Ok;

            // Profile: je erkannter Gruppe eine Auswahlzeile unter "Profile und Konfigurationen"
            subUmzugProfile.Inhalt.SuspendLayout();
            subUmzugProfile.Inhalt.Controls.Clear();
            chkProfBrowser = null; chkProfMail = null; chkProfDev = null; chkProfTools = null; chkProfTerm = null;
            if (foundBrowsers.Count > 0) chkProfBrowser = profilZeile("Webbrowser", foundBrowsers, "Sichert Lesezeichen, Sitzungen und Einstellungen der erkannten Webbrowser.");
            if (foundMail.Count > 0) chkProfMail = profilZeile("E-Mail-Programme", foundMail, "Sichert Kontoeinstellungen, Adressb\u00FCcher und E-Mail-Profile von Mozilla Thunderbird.");
            if (foundDev.Count > 0) chkProfDev = profilZeile("Entwicklung und Editoren", foundDev, "Sichert Benutzereinstellungen, Tastenbelegungen und Konfigurationsdateien von Entwicklungswerkzeugen.");
            if (foundTools.Count > 0) chkProfTools = profilZeile("Werkzeuge und Fernzugriff", foundTools, "Sichert Serverlisten, FTP-Verbindungen und Session-Profile g\u00E4ngiger Hilfswerkzeuge.");
            if (hasTerminal) chkProfTerm = profilZeile("Windows Terminal", new List<string>(new string[] { "settings.json" }), "Sichert benutzerdefinierte Profile und Farbschemata des Windows Terminals.");
            if (subUmzugProfile.Inhalt.Controls.Count == 0) {
                Label lblNoProfs = Lbl("Keine Anwendungsprofile auf diesem PC gefunden (Browser, E-Mail, Editoren, Werkzeuge).", 8.5f, false, UI.Muted);
                lblNoProfs.Margin = new Padding(0, 0, 0, UI.S(2));
                Tip(lblNoProfs, "Es wurden keine Standardprofile f\u00FCr Browser, Mail oder Tools gefunden.");
                subUmzugProfile.Inhalt.Controls.Add(lblNoProfs);
            }
            subUmzugProfile.Inhalt.ResumeLayout(true);
            subUmzugProfile.Ordne();

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
                        LaufwerkZeile cb = new LaufwerkZeile("C:\\" + di.Name, "", cSizeStr, false);
                        cb.Tag = di.FullName;
                        Tip(cb, "Sichert das Verzeichnis C:\\" + di.Name + " (" + cSizeStr + ") in den Unterordner C_Laufwerk_Zusatz.");
                        cb.CheckedChanged += delegate { updateCalc(); };
                        customBoxes.Add(cb);
                        customPaths.Add(di.FullName);
                        customSizes.Add(cSize);
                    }
                }
            } catch { }

            karteUmzugZusatz.Leere();
            if (customBoxes.Count > 0) {
                foreach (LaufwerkZeile cb in customBoxes) karteUmzugZusatz.Zeile(cb);
                Label lblCHead = Lbl("Bei Bedarf w\u00E4hlen. Gesichert wird in den Unterordner C_Laufwerk_Zusatz.", 8.5f, false, UI.Muted);
                lblCHead.Margin = new Padding(0, UI.S(2), 0, UI.S(2));
                karteUmzugZusatz.Frei(lblCHead);
            } else {
                Label lblNoC = Lbl("Keine zus\u00E4tzlichen Stammordner auf Laufwerk C: gefunden.", 8.5f, false, UI.Muted);
                lblNoC.Margin = new Padding(0, 0, 0, UI.S(2));
                karteUmzugZusatz.Frei(lblNoC);
            }
            karteUmzugZusatz.Breite(karteUmzugZusatz.Karte.Width);

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
                    // Ordner ohne lesbares Laufwerk (Freigabe \\Server\Ordner): ohne erfundene Angaben
                    string itemText = free > 0 ? p + " (" + (free / 1073741824L) + " GB frei, " + fs + ")" : p;
                    cmbDest.Items.Add(itemText);
                    destPaths.Add(p);
                    destFs.Add(fs);
                    destFree.Add(free);
                    cmbDest.SelectedIndex = cmbDest.Items.Count - 1;
                }
            }
        };

        // Während der Sicherung: Auswahl und Ziel gesperrt, die Schalter zeigen ihren Stand gedämpft
        Action<bool> sperre = delegate(bool an) {
            foreach (ZeilenKarte k in new ZeilenKarte[] { kLib, kSys, karteUmzugZusatz, kZiel }) k.Inhalt.Enabled = !an;
            btnStartUmzug.Enabled = !an;
            btnStartScan.Enabled = !an;
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
            for (int i = 0; i < libChk.Length; i++) if (libChk[i].Checked) scopes.Add(libScope[i]);
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
                if (zWlanClear.Checked) scopes.Add("Wlan_Klartext");
            }

            List<string> chosenCustom = new List<string>();
            for (int i = 0; i < customBoxes.Count; i++)
            {
                if (customBoxes[i].Checked) chosenCustom.Add(customPaths[i]);
            }

            // ab v4.09 zählen die gewählten Zusatzordner (bis v4.08 genügte, dass der Scan welche gefunden hatte: Die
            // Sicherung startete dann auch ganz ohne Auswahl)
            if (scopes.Count == 0 && chosenCustom.Count == 0)
            {
                MessageBox.Show(this, "Bitte w\u00E4hlen Sie mindestens eine Sicherungskomponente aus.", "System-Umzug", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string scopeStr = String.Join(",", scopes.ToArray());
            string customStr = String.Join(";", chosenCustom.ToArray());

            string prompt = "Soll die Sicherung auf das folgende Zielmedium gestartet werden?\r\n\r\n" +
                            "Ziel: " + targetPath + "\r\n" +
                            "Umfang: " + (scopes.Count > 0 ? String.Join(", ", scopes.ToArray()) : "keine") + "\r\n" +
                            (chosenCustom.Count > 0 ? "Zusatzordner: " + chosenCustom.Count + " Ordner\r\n" : "") +
                            "\r\nEs wird ein autarkes Wiederherstellungspaket (Wiederherstellen.cmd) erzeugt.";

            if (MessageBox.Show(this, prompt, "System-Umzug starten", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            sperre(true);

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
                        sperre(false);
                        lblUmzugDetail.Visible = false;

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
