using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Thread = System.Threading.Thread;
using System.Windows.Forms;

// Rahmen der Oberfläche: Felder, Start, Seitenleiste, Fußleiste und gemeinsame Bausteine. Seit 4.07 hat jede Seite eine
// eigene Datei DiagGui_<Seite>.cs (Teilklasse DiagGui); Reihenfolge der Teile in Oberflaeche.ps1. Die using-Zeilen stehen
// nur hier, weil alle Teile zu einer Übersetzungseinheit zusammengefügt werden.
public partial class DiagGui : Form
{
    [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
    [DllImport("kernel32.dll")] static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr h, int cmd);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll", SetLastError = true)] static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);
    [DllImport("shcore.dll", SetLastError = true)] static extern int SetProcessDpiAwareness(int awareness);
    [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    const int DWMWCP_ROUND = 2;
    [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true)] static extern int SetPreferredAppMode(int appMode);
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)] static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, IntPtr pid);
    [DllImport("user32.dll")] static extern bool AttachThreadInput(uint a, uint b, bool attach);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] static extern bool BringWindowToTop(IntPtr h);
    [DllImport("user32.dll")] static extern bool FlashWindow(IntPtr h, bool invert);

    string psExe, script, cpDir, dataDir, dbDir, version;
    string[] disks;   // Nummer|Name|Bus|Medium|Größe|Buchstaben|USB
    Process proc;
    ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
    Timer timer;
    volatile bool exited, outEof;
    DateTime exitSeen;
    bool running, done, gotDone, cancelled;
    string htmlPath = "", outDir = "", kiPath = "", kurzPath = "";
#pragma warning disable 414
    int nK, nW, nI, cntK, cntW, cntI, testsOk, benchWarn, benchCount;
    Dictionary<string, ListViewItem> benchHeads = new Dictionary<string, ListViewItem>();
    List<DbEntry> db = new List<DbEntry>();
    Font wsBold;

    Panel setupView, runView, content, head, body, contentHost, mainArea;
    internal Panel resultDashboard;
    TableLayoutPanel foot;
    PillButton pillPresetMin, pillPresetLeo, pillPresetAdv, pillPresetReset;
    FlowLayoutPanel pnlPresetPills, pnlCustomPresets;
    Button btnDashOpenHtml, btnDashFolder, btnDashKi, btnDashVerlauf;
    Label lblDashTitle, lblDashMeta;

    List<PillButton> optPresetPills = new List<PillButton>();
    Dictionary<string, CategoryPickerItem> optPickers = new Dictionary<string, CategoryPickerItem>();
    Dictionary<string, CategoryPickerItem> repPickers = new Dictionary<string, CategoryPickerItem>();
    Dictionary<string, Control> optCatPanels = new Dictionary<string, Control>();
    Dictionary<string, Control> repCatPanels = new Dictionary<string, Control>();
    string curOptKey = "", curRepKey = "";
    CardPanel optDetailCard, repDetailCard;
    Label lblOptCatTitle, lblOptCatDesc, lblOptCatCount, lblRepCatTitle, lblRepCatDesc;
    LinkLabel lnkOptAll, lnkOptNone, lnkRepAll, lnkRepNone;
    PillButton pillRepUsual, pillRepNone;
    FlowLayoutPanel left;
    Button btnThemeToggle, btnSettings;
    FlowLayoutPanel fo;
    Panel[] modBanners = new Panel[6];
    Badge badgeSensStatus;
    internal List<NavItem> nav = new List<NavItem>();
    List<Control> pages = new List<Control>();
    internal int curPage;
#pragma warning restore 414

    // Diagnose
    internal RadioButton rbSchnell, rbVoll, rbCustom;
    string[] diagKeys = new string[] { "Ereignisse", "Updatesuche", "Integritaet", "Defender", "SmartLang", "Netzwerk", "RamTest", "Energieanalyse" };
    string[] diagText = new string[] {
        "Ereignisprotokolle, Bluescreens, Absturzabbilder, Zuverlässigkeit",
        "Nach ausstehenden Windows-Updates suchen (bis 3 Minuten)",
        "Dateisystem-Onlinescan, DISM ScanHealth, sfc /verifyonly (nur prüfen)",
        "Microsoft Defender Schnellscan",
        "SMART-Langtest aller Laufwerke (SSD meist < 15 Min., HDD Stunden)",
        "Netzwerktest: Ping, DNS, HTTPS, Download",
        "RAM-Mustertest unter Windows",
        "Energieeffizienzanalyse (60 s) und DxDiag-Bericht" };
    internal CheckBox[] diagChk;
    CheckBox chkInstall, chkMem;
    ComboBox cmbDays, cmbSmartMax;
    static readonly int[] smartMinutes = new int[] { 30, 60, 90, 120, 180, 240, 360 };
    // Hilfswerkzeuge (PawnIO, smartmontools per winget): je Gerät entfernen, behalten oder jedes Mal fragen (Geraete.json)
    string deviceId = "";
    ComboBox cmbKeep;
    // GPU-Rendertest (ab v2.6): Auflösung und Anzeige, auf den Seiten Benchmark und Lasttest gleich eingestellt
    ComboBox cmbGpuRes, cmbGpuShow, cmbLGpuRes, cmbLGpuShow;
    bool syncGpu;
    // Auflösung, Anzeige und Grafikeinheit des Rendertests, je Seite in derselben Reihenfolge (einzelne können null sein)
    ComboBox[] GpuCombosBench() { return new ComboBox[] { cmbGpuRes, cmbGpuShow, cmbGpuSel }; }
    ComboBox[] GpuCombosLast() { return new ComboBox[] { cmbLGpuRes, cmbLGpuShow, cmbLGpuSel }; }
    // Auswahl paarweise übernehmen: nach[i] bekommt den Index von von[i]; fehlende Listen und ungültige Indizes bleiben unberührt
    public static void CopySelection(ComboBox[] von, ComboBox[] nach)
    {
        if (von == null || nach == null) return;
        for (int i = 0; i < von.Length && i < nach.Length; i++)
        {
            ComboBox a = von[i], b = nach[i];
            if (a == null || b == null || a.SelectedIndex < 0 || a.SelectedIndex >= b.Items.Count) continue;
            if (b.SelectedIndex != a.SelectedIndex) b.SelectedIndex = a.SelectedIndex;
        }
    }
    // ab v2.65: Auswahl der Grafikeinheiten (Benchmark und Lasttest gleich), Liste vom Startskript (Win32_VideoController)
    public static string[] GpuNames = new string[0];
    // ab v2.8: Katalog des Moduls Optimierung (Zeilen K|... und E|... aus Get-OptGuiLines) und Domänenmitgliedschaft
    public static string[] OptKatalog = new string[0];
    public static bool DomainPc = false;
    // ab v3.72: Hardware-Schlüsseldaten für die Startseite
    public static string HwCpu = "";
    public static int HwCores = 0, HwThreads = 0;
    public static string HwRam = "", HwBoard = "";
    public static bool HwIsLaptop = false;
    // ab v3.86: Resizable BAR / SAM Status
    public static bool HwRebarSupported = false;
    public static bool HwRebarActive = false;
    // ab v3.86: Update-Status
    public static bool HasUpdateAvailable = false;
    public static string AvailableUpdateVersion = "";
    public static string AvailableUpdateUrl = "";
    public static string AvailableUpdateSha256 = "";
    public static string AvailableUpdateBody = "";
    Badge badgeUpdate;
    ComboBox cmbGpuSel, cmbLGpuSel;
    List<string> gpuSelValues = new List<string>();
    // Schneller Modus (ab v2.6): parallele Prüfungen der Diagnose
    internal CheckBox chkFast;
    Label lblPar;
    Dictionary<string, string[]> parJobs = new Dictionary<string, string[]>();
    List<string> parOrder = new List<string>();
    // Benchmark
    string[] benchKeys = new string[] { "CPU", "RAM", "GPU", "Disk" };
    string[] benchText = new string[] { "Prozessor", "Arbeitsspeicher", "Grafik", "Laufwerke" };
    internal CheckBox[] benchChk;
    CheckedListBox clbCompare;
    List<string> diskNums = new List<string>();
    ComboBox cmbRef;
    Label lblRefState;
    Button btnUpdateMed;
    List<DbEntry> refItems = new List<DbEntry>(), cmpItems = new List<DbEntry>();
    // Lasttest
    CheckBox chkLCpu, chkLRam, chkLGpu, chkLDisk;
    ComboBox cmbLCpu, cmbLRam, cmbLGpu, cmbLDisk, cmbLRamPct, cmbLDiskDrive, cmbLAbortCpu, cmbLAbortGpu;
    // Sensoren (Live-Seite)
    NavItem navStart, navSens, navDb, navChg, navTools, navUmzug;
    Process liveProc;
    ConcurrentQueue<string> liveQueue = new ConcurrentQueue<string>();
    Timer liveTimer;
    volatile bool liveExited, liveEof;
    DateTime liveExitSeen;
    bool liveRunning, liveStopping;
    DateTime liveStopAt;
    Button btnLive, btnSensTools, btnSensSave;
    ComboBox cmbDriver;
    int psuWattage = 650;
    string psuRating = "80 PLUS Bronze";
    Label lblSensTools, lblSensInfo;
    SensorChart chartLive, chartRun;
    ListView lvSens;
    Dictionary<int, ListViewItem> sensItems = new Dictionary<int, ListViewItem>();
    Dictionary<int, double[]> sensMinMax = new Dictionary<int, double[]>();
    Dictionary<int, string> sensUnit = new Dictionary<int, string>();
    List<string> sensNames = new List<string>();
    List<string> sensRows = new List<string>();
    Dictionary<string, ListViewGroup> sensGroups = new Dictionary<string, ListViewGroup>();
    Label lblRunSens;
    // Reparatur
    string[] repKeys = new string[] {
        "DismRestore", "Sfc", "Komponentenbereinigung", "Dateisystem", "WindowsUpdate", "Netzwerk", "Temp", "WMI", "Zeit", "Druck",
        "Geraete", "Schnellstart", "Energieplaene", "Datentraegerbereinigung", "Leistungszaehler", "Leerlaufaufgaben", "ShaderCache", "UpdateDownloads", "Absturzabbilder", "Prefetch",
        "PaketCache", "Wiederherstellungspunkte" };
    string[] repText = new string[] {
        "Komponentenspeicher prüfen und reparieren (DISM ScanHealth, bei Bedarf RestoreHealth)",
        "Systemdateien reparieren (sfc /scannow, nach DISM)",
        "Komponentenspeicher bereinigen (DISM StartComponentCleanup, gibt Platz frei)",
        "Dateisystemfehler beheben (Onlinescan, SpotFix, Systemlaufwerk beim Neustart)",
        "Windows Update zurücksetzen (Dienste, SoftwareDistribution, catroot2)",
        "Netzwerk zurücksetzen (DNS-Cache, Winsock, TCP/IP), Neustart nötig",
        "Temporäre Dateien löschen (älter als 2 Tage, Übermittlungsoptimierung, Fehlerberichte)",
        "WMI-Repository prüfen und bei Bedarf reparieren",
        "Windows-Zeit neu synchronisieren",
        "Druckwarteschlange leeren und Druckspooler neu starten",
        "Geräte neu erkennen lassen (pnputil /scan-devices)",
        "Schnellstart deaktivieren (hilft bei Abstürzen nach dem Einschalten)",
        "Energiesparpläne auf Standard zurücksetzen (eigene Pläne werden vorher gesichert)",
        "Datenträgerbereinigung (cleanmgr mit allen Kategorien außer Downloads)",
        "Leistungszähler aus der Sicherung neu aufbauen (lodctr /r)",
        "Aufgeschobene Windows-Wartungsaufgaben starten (ProcessIdleTasks)",
        "DirectX-, OpenGL-, Intel- und AMD-Shader-Caches leeren",
        "SoftwareDistribution-Download-Ordner leeren (installierte Updates bleiben erhalten)",
        "CrashDumps, MSOCache, RetailDemo und Treiberreste leeren",
        "Prefetch-Ordner leeren (Windows baut die Daten danach neu auf)",
        "Paket-Cache von Installationsprogrammen leeren (Package Cache)",
        "Alte Schattenkopien auf dem Systemlaufwerk löschen" };
    int[] repMinutes = new int[] { 20, 12, 15, 5, 2, 1, 3, 2, 1, 1, 1, 1, 1, 10, 1, 1, 1, 1, 1, 1, 1, 1 };
    CheckBox[] repChk;
    CheckBox chkRestorePoint;
    // Datenbank
    ListView lvDb;
    Label lblDbPath;
    // Modulverträge und Änderungsprotokoll
    Dictionary<string, string[]> modInfo = new Dictionary<string, string[]>();   // Name -> Titel, Kurz, Admin, Risiko, Neustart
    List<ContractStep> steps = new List<ContractStep>();
    string[] repRisk = new string[0], repGroup = new string[0];
    bool[] repDefault = new bool[0], repUsual = new bool[0];
    string[] repRestart = new string[0];
    Dictionary<string, GroupHeader> repCatCount = new Dictionary<string, GroupHeader>();
    Dictionary<string, Control> repCatBody = new Dictionary<string, Control>();
    Dictionary<string, GroupHeader> repCatToggle = new Dictionary<string, GroupHeader>();
    string changeDir = "";
    List<ChangeEntry> changes = new List<ChangeEntry>();
    ListView lvChg;
    Button btnUndo;
    // Fuß
    Label lblSel;
    LinkLabel lnkAblage;
    Action footLayout = delegate { };
    CheckBox chkAnon;
    Button btnStart;
    // ab v2.7: Datenpflege beim Start (vom Skript gesetzt) und Schaltfläche Aufräumen auf der Seite Vergleichsdatenbank
    public static string DatenpflegeInfo = "";
    Label lblDbClean;
    Button btnDbClean;
    // ab v2.7: Seite Versionen (Verweis oben rechts im Kopf, keine eigene Schaltfläche in der Modulliste)
    int versionPage = -1;

    // Hinweise beim Überfahren mit der Maus (ab v2.7): kurze Erklärung zu jedem Bedienelement
    ToolTip tip = NewTip();
    static ToolTip NewTip()
    {
        ToolTip t = new ToolTip(); t.AutoPopDelay = 30000; t.InitialDelay = 450; t.ReshowDelay = 150; t.ShowAlways = true;
        return t;
    }
    // Text für den Hinweis umbrechen (höchstens rund 90 Zeichen je Zeile), vorhandene Zeilenumbrüche bleiben
    public static string WrapTip(string s)
    {
        if (String.IsNullOrEmpty(s)) return "";
        StringBuilder b = new StringBuilder();
        foreach (string para in s.Replace("\r\n", "\n").Split('\n'))
        {
            if (b.Length > 0) b.Append("\r\n");
            int col = 0;
            foreach (string w in para.Split(' '))
            {
                if (w.Length == 0) continue;
                if (col > 0 && col + 1 + w.Length > 90) { b.Append("\r\n"); col = 0; }
                else if (col > 0) { b.Append(' '); col++; }
                b.Append(w); col += w.Length;
            }
        }
        return b.ToString();
    }
    void Tip(Control c, string text) { if (c != null && !String.IsNullOrEmpty(text)) tip.SetToolTip(c, WrapTip(text)); }

    // Ablauf
    Button btnStopWait, btnSkipStep, btnCancel, btnHtml, btnFolder, btnNew, btnKi, btnExportRun;
    Label lblStep, lblCounter, lblSub, lblResult;
    FlatBar barAll, barSub;
    StatCard cardK, cardW, cardI, cardT;
    TabStrip tabs;
    ListView lvFind, lvTests, lvBench;
    TextBox txtLog;

    public static void Run(string psExe, string script, string cpDir, string dataDir, string version, string[] disks, string[] contract)
    {
        Run(psExe, script, cpDir, dataDir, version, disks, contract, "");
    }

    static void EnableDpi()
    {
        try { if (SetProcessDpiAwarenessContext((IntPtr)(-4))) return; } catch { }
        try { if (SetProcessDpiAwareness(2) == 0) return; } catch { }
    }

    public static void Run(string psExe, string script, string cpDir, string dataDir, string version, string[] disks, string[] contract, string deviceId)
    {
        IntPtr con = IntPtr.Zero; bool wasVisible = false;
        EnableDpi();
        try { using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) if (g.DpiX > 0) UI.DpiScale = g.DpiX / 96.0f; } catch { }
        AppSymbol.SetAppId();
        try { Application.EnableVisualStyles(); } catch { }
        try { Application.SetCompatibleTextRenderingDefault(false); } catch { }
        try { con = GetConsoleWindow(); if (con != IntPtr.Zero) { wasVisible = IsWindowVisible(con); ShowWindow(con, 0); } } catch { }
        try { DiagGui g = new DiagGui(psExe, script, cpDir, dataDir, version, disks, contract, deviceId); Application.Run(g); }
        finally { if (con != IntPtr.Zero && wasVisible) ShowWindow(con, 5); }
    }

    static Label Lbl(string text, float size, bool bold, Color color)
    {
        Label l = new Label(); l.Text = text; l.AutoSize = true; l.ForeColor = color; l.BackColor = Color.Transparent; l.UseMnemonic = false;
        l.Font = new Font(bold ? "Segoe UI Semibold" : "Segoe UI", size); return l;
    }

    // Ablage kurz: die letzten zwei Ordner des Datenordners ("LMB\Minibench-Daten"), der volle Pfad steht im Hinweis
    public static string AblageKurz(string dir)
    {
        string d = (dir ?? "").TrimEnd('\\', '/');
        if (d.Length == 0) return "Ablage: kein Datenordner";
        string[] parts = d.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        string kurz = parts.Length >= 2 ? parts[parts.Length - 2] + "\\" + parts[parts.Length - 1] : d;
        return "Ablage: " + (parts.Length > 2 ? "...\\" : "") + kurz;
    }

    static Label SideHead(string text)
    {
        Label l = Lbl(text.ToUpperInvariant(), 8.5f, true, UI.Muted); return l;
    }

    static string OsName()
    {
        try
        {
            using (Microsoft.Win32.RegistryKey k = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                string pn = Convert.ToString(k.GetValue("ProductName")); string dv = Convert.ToString(k.GetValue("DisplayVersion"));
                int build = 0; int.TryParse(Convert.ToString(k.GetValue("CurrentBuildNumber")), out build);
                if (build >= 22000) pn = pn.Replace("Windows 10", "Windows 11");
                return (pn + " " + dv).Trim();
            }
        }
        catch { return "Windows"; }
    }

    public DiagGui(string psExe, string script, string cpDir, string dataDir, string version, string[] disks, string[] contract) : this(psExe, script, cpDir, dataDir, version, disks, contract, "") { }

    public DiagGui(string psExe, string script, string cpDir, string dataDir, string version, string[] disks, string[] contract, string deviceId)
    {
        using (Graphics g = CreateGraphics())
        {
            if (g.DpiX > 0) UI.DpiScale = g.DpiX / 96.0f;
        }
        this.deviceId = deviceId ?? "";
        this.psExe = psExe; this.script = script; this.cpDir = cpDir; this.dataDir = dataDir ?? ""; this.version = version ?? "";
        this.disks = disks ?? new string[0];
        dbDir = this.dataDir.Length > 0 ? Path.Combine(this.dataDir, "Datenbank") : "";
        changeDir = this.dataDir.Length > 0 ? Path.Combine(this.dataDir, "Änderungen") : "";
        StartDbVorladen();
        ReadContract(contract ?? new string[0]);
        Text = "Leos Minibench " + this.version;
        AutoScaleDimensions = new SizeF(96f, 96f); AutoScaleMode = AutoScaleMode.Dpi;
        bool isDark = LoadThemePreference();
        LoadPsuPreference();
        LoadAkzentPreference();
        UI.SetTheme(isDark);
        try { SetPreferredAppMode(isDark ? 2 : 0); } catch { }
        Font = new Font("Segoe UI", 9.75f);
        BackColor = UI.Bg; ForeColor = UI.Text;
        StartPosition = FormStartPosition.CenterScreen;
        Rectangle workArea = Screen.FromPoint(Cursor.Position).WorkingArea;
        int startW = Math.Min(UI.S(1320), (int)(workArea.Width * 0.96));
        int startH = Math.Min(UI.S(860), (int)(workArea.Height * 0.95));
        ClientSize = new Size(startW, startH);
        // ab v3.64 Mindestgröße, bei der jede Seite ohne waagerechte Bildlaufleiste auskommt: Seitenleiste 246, Ränder 90,
        // Gruppenbreite 780 und senkrechte Bildlaufleiste; höchstens 60 % der Arbeitsfläche, damit das Fenster bei hoher
        // Skalierung oder kleinem Bildschirm verkleinerbar bleibt (dann mit waagerechter Bildlaufleiste)
        MinimumSize = new Size(Math.Max(640, Math.Min(UI.S(1160), (int)(workArea.Width * 0.6))), Math.Min(UI.S(600), workArea.Height));
        try { Icon ic = AppSymbol.Get(); if (ic != null) Icon = ic; else Icon = Icon.ExtractAssociatedIcon(psExe); } catch { }
        try { int round = DWMWCP_ROUND; DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(int)); } catch { }

        // ab v3.64: flachere Kopfzeile (56 statt 74 px), Version nur noch in der Titelleiste und auf der Seite Info,
        // rechts Symbolknöpfe für Farbschema und Info statt Emoji-Text und Verweis
        head = new Panel(); head.Dock = DockStyle.Top; head.Height = UI.S(56); head.BackColor = UI.Header;
        PictureBox logo = new PictureBox(); logo.Size = new Size(UI.S(36), UI.S(36)); logo.Location = new Point(UI.S(18), UI.S(10)); logo.SizeMode = PictureBoxSizeMode.Zoom; logo.BackColor = Color.Transparent;
        try { logo.Image = AppSymbol.Image(UI.S(36)); } catch { }
        Label t1 = Lbl("Leos Minibench", 13.5f, true, Color.White); t1.Location = new Point(UI.S(64), UI.S(6));
        Label t2 = Lbl(Environment.MachineName + "   ·   " + OsName(), 8.75f, false, Color.FromArgb(170, 182, 200)); t2.Location = new Point(UI.S(65), UI.S(31));
        head.Controls.Add(logo); head.Controls.Add(t1); head.Controls.Add(t2);
        FlowLayoutPanel hr = new FlowLayoutPanel(); hr.Dock = DockStyle.Right; hr.AutoSize = true; hr.FlowDirection = FlowDirection.RightToLeft; hr.WrapContents = false; hr.BackColor = Color.Transparent; hr.Padding = new Padding(0, UI.S(12), UI.S(14), 0);
        Button btnInfo = HeadButton(UI.IcoInfo);
        btnInfo.Click += delegate {
            // ab v4.08 ist das Ergebnis eine Seite: Von der Seite Versionen führt der Eintrag Ergebnis wieder zurück
            if (!running && versionPage >= 0) ShowPage(versionPage);
        };
        Tip(btnInfo, "Info und Versionshistorie: was sich von Version 1.0 bis " + this.version + " geändert hat.");
        hr.Controls.Add(btnInfo);
        btnSettings = HeadButton(UI.IcoGear);
        btnSettings.Click += delegate { ShowSettingsDialog(); };
        Tip(btnSettings, "Globale Einstellungen für Darstellung, Startverhalten, Ergebnisanzeige, Datenschutz und Speicherorte.");
        hr.Controls.Add(btnSettings);
        btnThemeToggle = HeadButton(UI.IsDark ? UI.IcoSonne : UI.IcoMond);
        btnThemeToggle.Click += delegate { ToggleTheme(); };
        Tip(btnThemeToggle, UI.IsDark ? "Zu hellem Farbschema wechseln." : "Zu dunklem Farbschema wechseln.");
        hr.Controls.Add(btnThemeToggle);
        badgeSensStatus = new Badge("", "ok");
        badgeSensStatus.Margin = new Padding(UI.S(6), UI.S(5), 0, 0);
        Tip(badgeSensStatus, "Aktiver Sensormodus für Hardware-Telemetrie und Systemwerte.");
        hr.Controls.Add(badgeSensStatus);
        badgeUpdate = new Badge("", "warn");
        badgeUpdate.Visible = false;
        badgeUpdate.Margin = new Padding(UI.S(6), UI.S(5), 0, 0);
        badgeUpdate.Cursor = Cursors.Hand;
        badgeUpdate.Click += delegate { TriggerUpdateDownload(); };
        Tip(badgeUpdate, "Ein Update für Leos Minibench ist verfügbar. Klicken zum Herunterladen und Aktualisieren.");
        hr.Controls.Add(badgeUpdate);
        head.Controls.Add(hr);

        body = new Panel(); body.Dock = DockStyle.Fill; body.Padding = new Padding(UI.S(20), UI.S(16), UI.S(20), UI.S(14)); body.BackColor = UI.Bg;
        mainArea = new Panel(); mainArea.Dock = DockStyle.Fill; mainArea.BackColor = UI.Bg;
        LoadDb();
        StartLog.Phase("Datenbank geladen");
        setupView = BuildSetup();
        StartLog.Phase(seitenZeiten.Length > 0 ? "Seiten aufgebaut (" + seitenZeiten + ")" : "Seiten aufgebaut");
        runView = BuildRun();
        FillPresets(true);
        LadeErgebnisAusDatenbank();
        if (LoadStartPagePreference()) ShowPage(0);
        else if (curPage == 0) ShowPage(1);
        UpdateSensStatusBadge();
        StartUpdateCheck();

        left.Dock = DockStyle.Left;
        body.Controls.Add(mainArea);
        body.Controls.Add(left);

        contentHost.Dock = DockStyle.Fill;
        if (foot != null) foot.Dock = DockStyle.Bottom;
        runView.Dock = DockStyle.Fill; runView.Visible = false;

        mainArea.Controls.Add(contentHost);
        if (foot != null) mainArea.Controls.Add(foot);
        mainArea.Controls.Add(runView);
        Controls.Add(body); Controls.Add(head);

        FormClosing += OnClosing;
        // selbst hergestellte Verbindungen zum Netzlaufwerk (Kennwort) nicht über das Programmende hinaus bestehen lassen
        FormClosed += delegate { NasAblage.AlleTrennen(); };
        // gegen Flackern: Doppelpufferung für Formular, Panels und Listen; Laufansicht wird höchstens alle 250 ms aktualisiert
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        EnableDoubleBuffer(this);
        timer = new Timer(); timer.Interval = 250; timer.Tick += delegate { OnTick(); };
        StartLog.Phase("Fenster aufgebaut");
    }


    static Color HeadButtonBack() { return UI.IsDark ? Color.FromArgb(32, 35, 40) : Color.FromArgb(34, 44, 62); }
    static Color HeadButtonHover() { return UI.IsDark ? Color.FromArgb(48, 52, 60) : Color.FromArgb(52, 64, 86); }

    // Symbolknopf der Kopfzeile (Farbschema, Info)
    static Button HeadButton(string glyph)
    {
        Button b = new Button(); b.Text = glyph; b.Font = UI.SymbolFont(11f); b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
        b.BackColor = HeadButtonBack(); b.ForeColor = Color.White; b.FlatAppearance.MouseOverBackColor = HeadButtonHover();
        b.Size = new Size(UI.S(34), UI.S(32)); b.Margin = new Padding(UI.S(6), 0, 0, 0); b.Cursor = Cursors.Hand; b.UseVisualStyleBackColor = false;
        return b;
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.F5)
        {
            if (curPage == 9) RefreshAndSync();
            else FillPresets(false);
            e.Handled = true;
        }
        else if (e.Control && e.KeyCode == Keys.Enter)
        {
            if (btnStart != null && btnStart.Enabled && btnStart.Visible)
            {
                btnStart.PerformClick();
                e.Handled = true;
            }
        }
    }

    // Text nur setzen, wenn er sich ändert (jede Zuweisung zeichnet das Element neu)
    static void SetText(Control c, string t) { if (c != null && c.Text != t) c.Text = t; }

    string seitenZeiten = "";
    static void SeitenZeit(StringBuilder sb, Stopwatch sw, string name)
    {
        if (sb.Length > 0) sb.Append(", ");
        sb.Append(name).Append(' ').Append(sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)).Append(" ms");
        sw.Restart();
    }

    // ab v4.07: Datenbank und Änderungsprotokoll werden gleichzeitig gelesen (zwei unabhängige Ordner mit JSON-Dateien),
    // beim Start schon ab dem Beginn des Konstruktors (StartDbVorladen), während Kopfzeile und Farbschema entstehen.
    List<DbEntry> dbVor; List<ChangeEntry> changesVor; Thread dbVorThread, changesVorThread;

    void StartDbVorladen()
    {
        string d = dbDir, c = changeDir;
        dbVorThread = new Thread(delegate() { try { dbVor = DbEntry.Load(d); } catch { dbVor = null; } });
        changesVorThread = new Thread(delegate() { try { changesVor = ChangeEntry.Load(c); } catch { changesVor = null; } });
        dbVorThread.IsBackground = true; changesVorThread.IsBackground = true;
        dbVorThread.Start(); changesVorThread.Start();
    }

    void LoadDb()
    {
        if (dbVorThread == null) StartDbVorladen();
        try { dbVorThread.Join(); } catch { }
        try { changesVorThread.Join(); } catch { }
        db = dbVor ?? new List<DbEntry>();
        changes = changesVor ?? new List<ChangeEntry>();
        dbVor = null; changesVor = null; dbVorThread = null; changesVorThread = null;
    }

    internal void ReloadDb()
    {
        LoadDb();
        FillDbList();
        FillRefLists();
        FuelleLetztesErgebnis();
        LadeErgebnisAusDatenbank();
        FillChangeList();
        UpdateSensTools();
        if (lblDbPath != null) SetText(lblDbPath, NasAblage.AblageText(dataDir));
        if (pages.Count > 0 && pages[0] is Panel) { BuildStartContent((Panel)pages[0]); }
    }

    // Modulverträge übernehmen: Navigation und Seite Reparatur richten sich danach (ohne Vertrag gelten die festen Listen)
    void ReadContract(string[] lines)
    {
        foreach (string l in lines)
        {
            string[] x = l.Split('|');
            if (x.Length >= 7 && x[0] == "M") modInfo[x[1]] = new string[] { x[2], x[3], x[4], x[5], x[6] };
            else if (x.Length >= 11 && x[0] == "S")
            {
                ContractStep c = new ContractStep(); c.Module = x[1]; c.Key = x[2]; c.Typ = x[3]; c.Risiko = x[4]; c.Neustart = x[5]; c.Rueckgaengig = x[6];
                int m; int.TryParse(x[7], out m); c.Minuten = m; c.Vorauswahl = x[8] == "1"; c.Ueblich = x[9] == "1"; c.Text = x[10];
                if (x.Length >= 12) c.Gruppe = x[11];
                steps.Add(c);
            }
        }
        List<ContractStep> rep = steps.FindAll(delegate(ContractStep c) { return (c.Module == "Reparatur" || c.Module == "Wartung") && c.Typ == "Massnahme"; });
        if (rep.Count > 0)
        {
            repKeys = new string[rep.Count]; repText = new string[rep.Count]; repMinutes = new int[rep.Count]; repRestart = new string[rep.Count];
            repRisk = new string[rep.Count]; repGroup = new string[rep.Count]; repDefault = new bool[rep.Count]; repUsual = new bool[rep.Count];
            for (int i = 0; i < rep.Count; i++)
            {
                repKeys[i] = rep[i].Key; repText[i] = rep[i].Text; repMinutes[i] = rep[i].Minuten;
                repRestart[i] = rep[i].Neustart; repRisk[i] = rep[i].Risiko; repGroup[i] = rep[i].Gruppe; repDefault[i] = rep[i].Vorauswahl; repUsual[i] = rep[i].Ueblich;
            }
        }
        else
        {
            repRestart = new string[repKeys.Length]; for (int i = 0; i < repKeys.Length; i++) repRestart[i] = "";
            repRisk = new string[repKeys.Length]; repGroup = new string[repKeys.Length]; repDefault = new bool[repKeys.Length]; repUsual = new bool[repKeys.Length];
            for (int i = 0; i < repKeys.Length; i++) { repRisk[i] = ""; repGroup[i] = ""; repDefault[i] = i < 2; repUsual[i] = i < 2 || repKeys[i] == "Dateisystem" || repKeys[i] == "Temp" || repKeys[i] == "WMI"; }
        }
    }

    NavItem ModNav(string name, string title, string desc, string icon = "")
    {
        string[] mi; if (modInfo.TryGetValue(name, out mi)) { title = mi[0]; desc = mi[1]; }
        NavItem it = new NavItem(title, desc, icon);
        if (!String.IsNullOrEmpty(desc)) Tip(it, desc);
        return it;
    }

    // ------------------------------------------------------------ Einrichtung
    Panel BuildSetup()
    {
        Panel p = new Panel(); p.BackColor = UI.Bg;

        left = new FlowLayoutPanel(); left.Dock = DockStyle.Left; left.Width = UI.S(246); left.FlowDirection = FlowDirection.TopDown; left.WrapContents = false;
        left.BackColor = UI.Bg; left.Padding = new Padding(0, 0, UI.S(14), 0); left.AutoScroll = true;
        navStart = new NavItem("Start", "", UI.IcoHome); navStart.HasCheck = false;
        Tip(navStart, "Start: Übersicht über diesen PC, empfohlene Aufgaben und letzte Ergebnisse.");
        nav.Add(navStart);
        nav.Add(ModNav("Diagnose", "Diagnose", "Inventar, Prüfungen, Ereignisse", UI.IcoDiag));
        nav.Add(ModNav("Benchmark", "Benchmark", "Wählbare Messungen mit Vergleich", UI.IcoCpu));
        nav.Add(ModNav("Lasttest", "Lasttest", "Komponenten und Dauer wählbar", UI.IcoFlame));
        nav.Add(ModNav("Wartung", "Wartung", "SFC, DISM, Bereinigung und Systempflege", UI.IcoWartung));
        nav.Add(ModNav("Optimierung", "Optimierung", "Datenschutz und Systemanpassungen", UI.IcoOpt));
        navTools = ModNav("Tools", "Tools", "Portable Werkzeuge und Schnellstarter", UI.IcoTools); navTools.HasCheck = false; nav.Add(navTools);
        navSens = ModNav("Sensoren", "Sensoren live", "Temperatur, Takt, Lüfter, Leistung", UI.IcoSens); navSens.HasCheck = false; nav.Add(navSens);
        navUmzug = ModNav("Umzug", "System-Umzug", "Daten, Desktop & Programme für Neuinstallation sichern", UI.IcoBackup); navUmzug.HasCheck = false; nav.Add(navUmzug);
        navDb = new NavItem("Datenbank", db.Count + " gespeicherte Systeme", UI.IcoDb); navDb.HasCheck = false; Tip(navDb, "Datenbank aller gespeicherten Systeme verwalten und im Dashboard vergleichen."); nav.Add(navDb);
        navChg = new NavItem("Änderungen", "Protokoll und Rückgängig", UI.IcoChg); navChg.HasCheck = false; Tip(navChg, "Änderungsprotokoll: was Leos Minibench an diesem PC verändert hat, mit Rückgängig."); nav.Add(navChg);

        left.Controls.Add(navStart);
        // ab v4.08: Ergebnis direkt unter Start, sichtbar sobald es ein Ergebnis gibt (letzter Lauf dieses PCs aus der
        // Datenbank oder der Lauf von eben). Der Eintrag steht außerhalb der Liste nav, deren Stellen den Seiten 0 bis 10
        // und den Modulen entsprechen.
        navErgebnis = new NavItem("Ergebnis", "", UI.IcoErgebnis); navErgebnis.HasCheck = false; navErgebnis.HintPille = true; navErgebnis.Visible = false;
        Tip(navErgebnis, "Ergebnis des letzten Laufs mit Urteil, Vergleich zur Referenz und den Hauptwerten je Komponente.");
        navErgebnis.Picked += delegate { if (!running && erg != null && ergebnisPage >= 0) ShowPage(ergebnisPage); };
        left.Controls.Add(navErgebnis);
        Label lm = SideHead("Ablauf"); lm.Margin = new Padding(UI.S(4), UI.S(UI.Abstand4), 0, UI.S(6)); left.Controls.Add(lm);
        for (int i = 1; i < nav.Count; i++)
        {
            if (nav[i] == navTools) { Label lw = SideHead("Werkzeuge"); lw.Margin = new Padding(UI.S(4), UI.S(UI.Abstand4), 0, UI.S(6)); left.Controls.Add(lw); }
            left.Controls.Add(nav[i]);
        }
        for (int i = 0; i < nav.Count; i++)
        {
            int idx = i;
            nav[i].Picked += delegate {
                if (running)
                {
                    if (nav[idx] == navSens && tabs != null) tabs.Selected = 3;
                    return;
                }
                ShowPage(idx);
            };
            nav[i].CheckedChanged += delegate { UpdateSummary(); };
        }
        // ab v2.8 acht Einträge: bei kleinem Fenster mit senkrechter Bildlaufleiste schmaler statt waagerecht zu scrollen
        left.Layout += delegate { int w = left.ClientSize.Width - left.Padding.Horizontal - 2; if (w > UI.S(150)) { foreach (NavItem n in nav) if (n.Width != w) n.Width = w; if (navErgebnis.Width != w) navErgebnis.Width = w; } };

        content = new Panel(); content.Dock = DockStyle.Fill; content.BackColor = UI.Panel; content.Padding = new Padding(UI.S(UI.Seitenrand), UI.S(UI.Seitenrand), UI.S(UI.Seitenrand), UI.S(20)); content.AutoScroll = true;
        contentHost = new Panel(); contentHost.Dock = DockStyle.Fill; contentHost.BackColor = UI.Line; contentHost.Padding = new Padding(1);
        contentHost.Controls.Add(content);

        // ab v4.07: Aufbauzeit je Seite für Start.log (Zusatz der Phase "Seiten aufgebaut")
        Stopwatch swSeite = Stopwatch.StartNew(); StringBuilder sbSeiten = new StringBuilder();
        pages.Add(BuildStartPage()); SeitenZeit(sbSeiten, swSeite, "Start");
        pages.Add(BuildDiagPage()); SeitenZeit(sbSeiten, swSeite, "Diagnose");
        pages.Add(BuildBenchPage()); SeitenZeit(sbSeiten, swSeite, "Benchmark");
        pages.Add(BuildLoadPage()); SeitenZeit(sbSeiten, swSeite, "Lasttest");
        pages.Add(BuildRepairPage()); SeitenZeit(sbSeiten, swSeite, "Wartung");
        pages.Add(BuildOptPage()); SeitenZeit(sbSeiten, swSeite, "Optimierung");
        pages.Add(BuildToolsPage()); SeitenZeit(sbSeiten, swSeite, "Tools");
        pages.Add(BuildSensorPage()); SeitenZeit(sbSeiten, swSeite, "Sensoren");
        pages.Add(BuildUmzugPage()); SeitenZeit(sbSeiten, swSeite, "Umzug");
        pages.Add(BuildDbPage()); SeitenZeit(sbSeiten, swSeite, "Datenbank");
        pages.Add(BuildChangePage()); SeitenZeit(sbSeiten, swSeite, "Änderungen");
        versionPage = pages.Count; pages.Add(BuildVersionPage()); SeitenZeit(sbSeiten, swSeite, "Versionen");
        resultDashboard = (Panel)BuildResultDashboard(); ergebnisPage = pages.Count; pages.Add(resultDashboard); SeitenZeit(sbSeiten, swSeite, "Ergebnis");
        seitenZeiten = sbSeiten.ToString();
        // ab v3.64 einheitlicher Seitenrand: Seiten ohne Dock stehen an der Position des Innenabstands (vorher bei 0,0,
        // dort wirkte der Innenabstand des Bildlaufbereichs nicht und der Titel klebte am Rahmen)
        foreach (Control c in pages) { c.Visible = false; if (c.Dock == DockStyle.None) c.Location = new Point(UI.S(UI.Seitenrand), UI.S(UI.Seitenrand)); content.Controls.Add(c); }

        // ab v3.64 Fußleiste in zwei Zeilen: oben Module in Ablaufreihenfolge mit Gesamtschätzung, darunter Voreinstellung,
        // Ablage und KI-Datei; der Startknopf steht allein rechts und sagt, was er startet
        foot = new TableLayoutPanel(); foot.Dock = DockStyle.Bottom; foot.Height = UI.S(76); foot.ColumnCount = 2; foot.RowCount = 2; foot.BackColor = UI.Bg; foot.Padding = new Padding(0, UI.S(10), 0, 0);
        foot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); foot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        foot.RowStyles.Add(new RowStyle(SizeType.AutoSize)); foot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lblSel = Lbl("", 10f, true, UI.Text); lblSel.Margin = new Padding(0, 0, 0, UI.S(6)); lblSel.AutoEllipsis = true;
        fo = new FlowLayoutPanel(); fo.AutoSize = true; fo.WrapContents = false; fo.BackColor = UI.Bg; fo.Margin = new Padding(0);
        Label lp = Lbl("Ablauf", UI.SchriftHinweis, false, UI.Muted); lp.Margin = new Padding(0, UI.S(6), UI.S(6), 0); fo.Controls.Add(lp);

        pnlPresetPills = new FlowLayoutPanel(); pnlPresetPills.AutoSize = true; pnlPresetPills.WrapContents = false; pnlPresetPills.BackColor = UI.Bg; pnlPresetPills.Margin = new Padding(0, UI.S(2), UI.S(2), 0);
        // ab v4.01 eigene Namen: dieselben Begriffe stehen auf der Seite Optimierung nur für deren Einstellungen.
        // Beim Start ist noch kein Ablauf gewählt, deshalb ist keine Pille hervorgehoben.
        pillPresetMin = new PillButton("Schnellcheck", false);
        pillPresetLeo = new PillButton("Standard", false);
        pillPresetAdv = new PillButton("Komplett", false);
        pillPresetReset = new PillButton("Alles abwählen", false);

        pillPresetMin.Picked += delegate { ApplyGlobalPreset("M"); };
        pillPresetLeo.Picked += delegate { ApplyGlobalPreset("S"); };
        pillPresetAdv.Picked += delegate { ApplyGlobalPreset("E"); };
        pillPresetReset.Picked += delegate { ApplyGlobalPreset(""); };

        Tip(pillPresetMin, "Diagnose (Profil Schnell) und Benchmark. Optimierung mit der Vorlage Minimal, Modul aber nicht angehakt.");
        Tip(pillPresetLeo, "Diagnose (Vollständig), Benchmark, Wartung mit der üblichen Auswahl und Optimierung mit Leos Empfehlung.");
        Tip(pillPresetAdv, "Alle fünf Module: Diagnose (Vollständig), Benchmark, Lasttest, alle Wartungsaufgaben und Optimierung Erweitert.");
        Tip(pillPresetReset, "Nimmt alle Module aus dem Ablauf und hebt die Auswahl in Wartung und Optimierung auf.");
        pnlPresetPills.Controls.Add(pillPresetMin);
        pnlPresetPills.Controls.Add(pillPresetLeo);
        pnlPresetPills.Controls.Add(pillPresetAdv);
        pnlPresetPills.Controls.Add(pillPresetReset);
        fo.Controls.Add(pnlPresetPills);

        pnlCustomPresets = new FlowLayoutPanel(); pnlCustomPresets.AutoSize = true; pnlCustomPresets.WrapContents = false; pnlCustomPresets.BackColor = UI.Bg; pnlCustomPresets.Margin = new Padding(0, UI.S(2), UI.S(2), 0);
        fo.Controls.Add(pnlCustomPresets);

        // ab v4.0 als Pille wie die Vorlagen daneben (vorher ein eckiger Knopf in anderem Stil)
        btnPresetSave = new PillButton("Als Vorlage speichern \u2026", false); btnPresetSave.Margin = new Padding(UI.S(4), UI.S(2), UI.S(6), 0); fo.Controls.Add(btnPresetSave);
        btnPresetSave.Picked += delegate { SavePresetDialog(); };

        cmbPreset = new ComboBox(); cmbPreset.Visible = false;
        btnPresetDel = new Button(); btnPresetDel.Visible = false;

        lnkAblage = new LinkLabel(); lnkAblage.AutoSize = true; lnkAblage.Font = new Font("Segoe UI", UI.SchriftHinweis); lnkAblage.LinkColor = UI.Accent; lnkAblage.ActiveLinkColor = UI.AccentHover;
        lnkAblage.Text = AblageKurz(this.dataDir); lnkAblage.Margin = new Padding(0, UI.S(7), UI.S(UI.Abstand4), 0); lnkAblage.AutoEllipsis = true; lnkAblage.LinkBehavior = LinkBehavior.HoverUnderline;
        lnkAblage.LinkClicked += delegate { if (this.dataDir.Length > 0) { try { Directory.CreateDirectory(this.dataDir); } catch { } OpenShell(this.dataDir); } };

        chkAnon = Chk("Personenbezogene Daten entfernen", true); chkAnon.Margin = new Padding(0, UI.S(5), UI.S(6), 0);


        btnStart = UI.Primary("Ablauf wählen"); btnStart.Enabled = false; btnStart.Margin = new Padding(UI.S(UI.Abstand5), 0, 0, 0); btnStart.MinimumSize = new Size(UI.S(150), UI.S(44)); btnStart.Anchor = AnchorStyles.Right;
        Tip(btnStart, "Erst eine Aufgabe wählen oder links ein Modul anhaken.");
        btnStart.Click += delegate { StartRun(); };
        foot.Controls.Add(lblSel, 0, 0); foot.Controls.Add(fo, 0, 1); foot.Controls.Add(btnStart, 1, 0); foot.SetRowSpan(btnStart, 2);
        footLayout = delegate
        {
            int avail = foot.ClientSize.Width - btnStart.Width - btnStart.Margin.Horizontal - UI.S(8);
            if (avail <= UI.S(80)) return;
            lblSel.MaximumSize = new Size(avail, UI.S(24));
        };
        foot.Resize += delegate { footLayout(); };
        btnStart.SizeChanged += delegate { footLayout(); };
        ShowPage(0);
        UpdateSummary();
        ApplySetupTips();
        return p;
    }

    // ------------------------------------------------------------ Hinweise beim Überfahren (ab v2.7)
    // Erklärt jedes Bedienelement: was es tut, wie lange es dauert und ob es etwas am PC verändert.
    static readonly Dictionary<string, string> diagTips = new Dictionary<string, string> {
        { "Ereignisse", "Liest die Ereignisprotokolle der gewählten Tage: Bluescreens mit Erklärung des Stoppcodes, unerwartete Abschaltungen (Kernel-Power 41), Absturzabbilder, Datenträger- und Hardwarefehler sowie den Zuverlässigkeitsindex von Windows. Nur lesend, etwa 1 Minute." },
        { "Updatesuche", "Fragt Windows Update nach ausstehenden Updates (bis 3 Minuten). Es wird nichts heruntergeladen oder installiert. Läuft bei paralleler Ausführung nebenher." },
        { "Integritaet", "Prüft das Dateisystem (Onlinescan), den Komponentenspeicher (DISM ScanHealth) und die Systemdateien (sfc /verifyonly). Nur prüfen, nichts reparieren: Reparaturen bietet das Modul Reparatur. Dauer etwa 10 bis 20 Minuten." },
        { "Defender", "Startet einen Schnellscan von Microsoft Defender und meldet Funde. Entfällt, wenn ein anderer Virenschutz aktiv ist. Läuft bei paralleler Ausführung nebenher." },
        { "SmartLang", "Startet in jedem Laufwerk den erweiterten SMART-Selbsttest (smartctl). SSDs brauchen meist unter 15 Minuten, Festplatten Stunden. Wie lange auf das Ergebnis gewartet wird, steht unten unter SMART-Langtest; der Test läuft im Laufwerk danach weiter." },
        { "Netzwerk", "Prüft Ping zum Router und ins Internet, Namensauflösung (DNS), eine HTTPS-Verbindung und die Download-Geschwindigkeit. Etwa 1 Minute, braucht die Leitung kurz für sich." },
        { "RamTest", "Schreibt Bitmuster in einen Teil des freien Arbeitsspeichers und liest sie zurück (Vollständig: 60 % in 3 Durchläufen, Schnell: 25 % in einem). Findet grobe RAM-Fehler; den ganzen Speicher prüft nur die Windows-Speicherdiagnose oder MemTest86." },
        { "Energieanalyse", "Energieeffizienzanalyse von Windows (powercfg /energy, 60 Sekunden) und DxDiag mit Grafik- und Treiberangaben. Beide Berichte liegen danach in Anhang.zip." } };
    static readonly Dictionary<string, string> benchTips = new Dictionary<string, string> {
        { "CPU", "Einzelkern und Mehrkern mit eigener Rechenlast (Punkte), AES-256 und SHA-256 über die Windows-Kryptografie (ein Thread) und ZIP-Kompression auf allen Threads. Dazu der Takt unter Einzel- und Mehrkernlast. Je Messung 5 Sekunden, im Kurzlauf 2." },
        { "RAM", "Lesen, Schreiben und Kopieren auf allen Threads über bis zu 1 GB sowie die Latenz zufälliger Zugriffe über 256 MB. Bewertet wird gegen den theoretischen Wert aus Takt und Kanalzahl." },
        { "GPU", "Eigener Rendertest mit Direct3D 11 auf jeder gewählten Grafikeinheit (Bilder/s, 1-%-Low, Punktzahl), dazu Speicherdurchsatz, Desktop-Komposition, PCIe-Anbindung und Grafikspeicher. Je Grafikeinheit 6 s Aufwärmen und 20 s Messung ohne VSync; vorher prüft eine kurze Gegenprobe, ob eine Bildratengrenze im Treiber bremst." },
        { "Disk", "Sequentiell Lesen und Schreiben (1-MiB-Blöcke) und 4K zufällig (QD1, 8 Threads, Schreiben), jeweils ohne Windows-Cache. Die Testdatei (1 GB, im Kurzlauf 256 MB) wird danach gelöscht. Welche Laufwerke, steht in der Liste darunter." } };

    void ApplySetupTips()
    {
        string[] navTips = new string[] {
            "Start: Übersicht über diesen PC, empfohlene Aufgaben und letzte Ergebnisse.",
            "Diagnose: Inventar, Treiber, Sicherheit, Ereignisprotokolle und die gewählten Prüfungen. Verändert nichts am System. Haken setzen, damit das Modul beim Start läuft.",
            "Benchmark: misst Prozessor, Arbeitsspeicher, Grafik und Laufwerke und vergleicht mit der Referenz und früheren Läufen. Ab v2.7 mit Temperatur, Takt und Leistung je Messung im Bericht.",
            "Lasttest: belastet CPU, RAM, Grafik und Datenträger gleichzeitig mit eigener Dauer, zeichnet Kurven auf und weist Drosselung nach. Hier steckt ab v2.7 auch die Prüfung der CPU-Stabilität.",
            "Wartung: Standardreparaturen wie DISM und SFC sowie Systempflege. Verändert das System; vorher auf Wunsch ein Wiederherstellungspunkt. Jede Maßnahme zeigt ihre Risikostufe.",
            "Optimierung (ab v2.8): das Windows Optimisation Pack nativ, mit Sophia- und O&O-Einstellungen, in Kategorien einzeln wählbar. Ändern ist einzeln rücknehmbar, Eingriffe sichert der Wiederherstellungspunkt ab.",
            "Tools: Portable Hilfswerkzeuge wie Revo Uninstaller, MiniTool Partition Wizard, WizTree sowie Windows-Schnellstarter.",
            "Sensoren live: Temperatur, Takt, Lüfter, Spannung und Leistung laufend anzeigen und aufzeichnen. Kein Modul für den Start.",
            "Datenbank: alle gespeicherten Läufe, Vergleich mehrerer Systeme ohne neuen Benchmark, Import älterer Ordner und Aufräumen.",
            "Änderungen: was Leos Minibench an einem PC verändert hat, mit Vorher-Wert und Rückgängig." };
        for (int i = 0; i < nav.Count && i < navTips.Length; i++) Tip(nav[i], navTips[i]);
        // Diagnose
        Tip(rbVoll, "Alle Prüfungen der Liste, ca. " + DiagMinutes(diagVoll) + " Min., mit SMART-Langtest länger.");
        Tip(rbSchnell, "Inventar, Ereignisse, Updatesuche, Netzwerk und ein kurzer RAM-Test (25 % des freien RAM, ein Durchlauf), ca. " + DiagMinutes(diagSchnell) + " Min.");
        Tip(rbCustom, "Die Prüfungen unten frei wählen.");
        Tip(segUmfang, "Umfang der Diagnose. Schnell: Inventar, Ereignisse, Updatesuche, Netzwerk und ein kurzer RAM-Test (25 % des freien RAM, ein Durchlauf). Vollständig: alle Prüfungen der Liste, mit SMART-Langtest länger. Eigene Auswahl: Prüfungen einzeln wählen; wer einen Schalter umlegt, wechselt dorthin.");
        Tip(lnkDiagAll, "Schaltet alle Prüfungen ein und wechselt zur eigenen Auswahl.");
        Tip(lnkDiagNone, "Schaltet alle Prüfungen aus und wechselt zur eigenen Auswahl; es läuft dann nur das Inventar.");
        Tip(chkFast, "Lesende Prüfungen ohne Messcharakter (Updatesuche, Defender, Energieanalyse, Start des SMART-Langtests) laufen nebenher. Messungen wie RAM-Test, Netzwerk, Benchmark und Lasttest warten immer, bis alles andere fertig ist.");
        for (int i = 0; diagChk != null && i < diagChk.Length && i < diagKeys.Length; i++) { string t; if (diagTips.TryGetValue(diagKeys[i], out t)) Tip(diagChk[i], t); }
        Tip(chkInstall, "smartctl liefert SMART-Werte und den Langtest. Liegt es im Tools-Ordner des Datenordners, wird es von dort verwendet, sonst per winget geholt und nach dem Lauf entfernt (oder auf Wunsch behalten). Risikostufe Eingriff.");
        Tip(chkMem, "Plant die Windows-Speicherdiagnose für den nächsten Neustart ein. Sie prüft den gesamten Arbeitsspeicher außerhalb von Windows; das Ergebnis liest der nächste Lauf aus den Ereignissen.");
        Tip(cmbDays, "Zeitraum für Ereignisprotokolle, Bluescreens und Absturzabbilder.");
        Tip(cmbSmartMax, "So lange wartet der Lauf auf das Ergebnis des SMART-Langtests. Danach entsteht der Bericht ohne Langtest; der Test läuft im Laufwerk weiter.");
        // Benchmark
        for (int i = 0; benchChk != null && i < benchChk.Length && i < benchKeys.Length; i++) { string t; if (benchTips.TryGetValue(benchKeys[i], out t)) Tip(benchChk[i], t); }
        Tip(lnkBenchAll, "Schaltet alle vier Messungen ein.");
        Tip(lnkBenchNone, "Schaltet alle Messungen aus.");
        Tip(cmbGpuSel, "Welche Grafikeinheiten der Rendertest belastet: alle, nur die Grafikkarte, nur die Prozessorgrafik oder eine bestimmte. Gilt für Benchmark und Lasttest.");
        Tip(cmbGpuRes, "Auflösung des Rendertests. Bilder/s sind nur bei gleicher Auflösung vergleichbar, die Punktzahl auch über Auflösungen.");
        Tip(cmbGpuShow, "Fenster: Vorschau des Rendertests. Vollbild: erste Grafikeinheit im Vollbild. Ohne Anzeige: misst unsichtbar (Esc entfällt).");
        Tip(cmbRef, "System oder Medianwert, gegen den die Messungen in Prozent angegeben werden (100 % = Referenz). Gespeicherte Referenz: die eigene Referenz (Referenz.json, festgelegt in der Datenbank), sonst der gespeicherte Median der Geräteklasse; Median: aus allen passenden Läufen der Datenbank.");
        Tip(btnUpdateMed, "Berechnet die Median-Referenzen für alle Systeme, Notebooks und Desktops aus der Datenbank neu und speichert sie als Referenz_Median.json, Referenz_Notebook.json und Referenz_Desktop.json. Die eigene Referenz (Referenz.json) bleibt unverändert.");
        // Lasttest
        Tip(chkLCpu, "Alle logischen Prozessoren rechnen mit Ergebnisprüfung; Rechenfehler deuten auf Instabilität (Übertaktung, Spannung, Kühlung, Netzteil). Läuft in einem eigenen Prozess ohne Unterbrechungen. Mit Sensoren gibt es Takt- und Temperaturverlauf und den Drosselnachweis. Ersetzt ab v2.7 den CPU-Stabilitätstest der Diagnose.");
        Tip(chkLRam, "Mustertest über einen Teil des freien Arbeitsspeichers mit Bitfehlerprüfung. Neben der CPU-Last läuft er mit niedriger Priorität auf einem Viertel der Threads.");
        Tip(cmbLRamPct, "Anteil des freien Arbeitsspeichers, den der RAM-Test belegt. Mehr findet mehr Fehler, lässt Windows aber weniger Luft.");
        Tip(chkLGpu, "Eigener Rendertest mit Direct3D 11 als Dauerlast auf den gewählten Grafikeinheiten gleichzeitig: Bilder/s-Verlauf, Bildprüfung alle 30 Sekunden und Erkennung von Treiber-Resets.");
        Tip(chkLDisk, "Schreibt eine Testdatei (bis 4 GB), liest sie zurück und prüft jeden Block. Meldet Datenfehler, Ein-/Ausgabefehler und Controllerereignisse. Die Datei wird danach gelöscht.");
        Tip(cmbLDiskDrive, "Laufwerk für den Datenträgertest; es braucht mindestens 2 GB freien Platz.");
        foreach (ComboBox c in new ComboBox[] { cmbLCpu, cmbLRam, cmbLGpu, cmbLDisk }) Tip(c, "Dauer dieser Komponente. Alle gewählten Komponenten starten gleichzeitig, jede endet nach ihrer eigenen Dauer.");
        Tip(cmbLGpuSel, "Welche Grafikeinheiten der Rendertest belastet (gleich eingestellt wie im Benchmark).");
        Tip(cmbLGpuRes, "Auflösung des Rendertests (gleich eingestellt wie im Benchmark).");
        Tip(cmbLGpuShow, "Fenster, Vollbild oder ohne Anzeige für den Rendertest (gleich eingestellt wie im Benchmark).");
        Tip(cmbLAbortCpu, "Liegt die CPU-Temperatur über dieser Schwelle, endet der Test mit einem Befund. Automatisch: TjMax der CPU nach rund einer Minute (viele Notebooks laufen planmäßig an TjMax), sonst 100 °C. Feste Werte greifen nach rund 10 Sekunden.");
        Tip(cmbLAbortGpu, "Liegt die GPU-Temperatur rund 10 Sekunden über dieser Schwelle, endet der Test mit einem Befund.");
        // Reparatur
        for (int i = 0; repChk != null && i < repChk.Length && i < repKeys.Length; i++)
        {
            ContractStep cs = steps.Find(delegate(ContractStep c) { return (c.Module == "Reparatur" || c.Module == "Wartung") && c.Key == repKeys[i]; });
            StringBuilder t = new StringBuilder(repText[i]);
            if (cs != null)
            {
                t.Append("\r\nRisikostufe: ").Append(ContractStep.RiskLabel(cs.Risiko)).Append(cs.Risiko == "Aendern" ? " (protokolliert, auf der Seite Änderungen rücknehmbar)" : cs.Risiko == "Eingriff" ? " (nicht automatisch umkehrbar, Absicherung über den Wiederherstellungspunkt)" : "");
                t.Append("\r\nNeustart: ").Append(cs.Neustart == "immer" ? "nötig" : cs.Neustart == "moeglich" ? "kann nötig sein" : "nicht nötig");
                if (cs.Minuten > 0) t.Append("\r\nDauer: etwa ").Append(cs.Minuten).Append(" Min.");
            }
            Tip(repChk[i], t.ToString());
        }
        Tip(chkRestorePoint, "Legt vor den Reparaturen einen Systemwiederherstellungspunkt an. Er ist der Weg zurück für alle Eingriffe dieses Laufs.");
        // Sensoren
        Tip(btnLive, "Öffnet die Sensoren und zeigt die Werte laufend an. Während eines Laufs ist die Live-Ansicht aus.");
        Tip(btnSensTools, "Holt LibreHardwareMonitor und das PawnIO-Setup aus den offiziellen Releases und prüft sie mit festen SHA-256-Werten. Abgelegt wird nur im Tools-Ordner, installiert wird nichts.");
        Tip(btnSensSave, "Speichert alle Werte der Live-Ansicht als CSV unter Berichte\\Sensoren.");
        Tip(cmbDriver, "PawnIO ist der Treiber, den LibreHardwareMonitor für CPU-Temperatur, CPU-Takt, CPU-Leistung und Mainboard-Lüfter braucht. Standardmäßig aktiv für alle Messungen. Ein bereits installierter PawnIO bleibt immer unverändert.");
        Tip(cmbKeep, "Was nach dem Lauf mit PawnIO und smartmontools geschieht, die Leos Minibench installiert hat: nach dem Lauf entfernen (keine Reste) oder auf diesem PC behalten. Gespeichert je Gerät in Geraete.json.");
        // Vergleichsdatenbank, Änderungen, Fuß
        Tip(btnCompare, "Vergleicht die angehakten Systeme in einem HTML-Bericht (mindestens zwei).");
        Tip(btnDbClean, "Datenpflege: verschiebt nicht mehr vergleichbare Lasttests älterer Versionen, abgebrochene Läufe ohne Bericht und kurze Läufe (Funktionstest, Lasttest nach wenigen Sekunden beendet, Lauf ohne Messwert) sowie ungenutzte Teile von smartmontools nach Minibench-Daten\\Archiv. Gelöscht werden nur ältere Übersetzungen im Cache und Reste der Live-Ansicht; das Protokoll steht in Archiv\\Datenpflege.log.");
        Tip(btnUndo, "Nimmt die angehakten Änderungen zurück (nur Stufe Ändern und nur auf dem PC, auf dem sie entstanden sind).");
        Tip(lnkAblage, "Öffnet den Datenordner mit Berichten, Datenbank, Werkzeugen und Vorlagen: " + (dataDir.Length > 0 ? dataDir : "kein Datenordner"));
        Tip(chkAnon, "Ersetzt Benutzernamen, Computernamen, Seriennummern und Domäne in Berichten und Exportdateien durch neutrale Platzhalter.");
        // Optimierung
        Tip(chkOptRestore, "Legt vor der Optimierung einen Systemwiederherstellungspunkt an. Er ist der Weg zurück für Eingriffe wie entfernte Apps und Zusatzfeatures.");
        Tip(btnOptTools, "Holt Display Driver Uninstaller (aus dem Optimisation Pack, feste Prüfsumme) und NVIDIA Profile Inspector in den Tools-Ordner. Installiert wird nichts.");
        Tip(btnOptAll, "Alle Kategorien auf- oder zuklappen.");
        if (btnRepAll == null) btnRepAll = UI.Secondary("");
        Tip(btnRepAll, "Alle Gruppen auf- oder zuklappen.");
        Tip(lnkOptAll, "Alle Optionen dieser Kategorie aktivieren.");
        Tip(lnkOptNone, "Alle Optionen dieser Kategorie deaktivieren.");
        Tip(lnkRepAll, "Alle Maßnahmen dieser Gruppe aktivieren.");
        Tip(lnkRepNone, "Alle Maßnahmen dieser Gruppe deaktivieren.");
        for (int i = 0; i < optPresetButtons.Count; i++) Tip(optPresetButtons[i], optPresetTips[i]);
        foreach (OptItem it in optItems) Tip(it.Box, OptTipText(it));
        Tip(cmbPreset, "Gespeicherte Auswahl aller Seiten laden (Voreinstellungen.json im Datenordner, weitergebbar).");
        Tip(btnPresetSave, "Aktuelle Auswahl aller Seiten unter einem Namen speichern.");
        Tip(btnPresetDel, "Gewählte Vorlage löschen.");
    }

    void ApplyRunTips()
    {
        Tip(btnStopWait, "Beendet den laufenden Test vorzeitig (Lasttest, SMART-Wartezeit). Der Bericht entsteht trotzdem.");
        Tip(btnSkipStep, "Überspringt den aktuellen Diagnoseschritt oder Benchmark und setzt den Lauf sofort mit dem nächsten Schritt fort.");
        Tip(btnCancel, "Bricht den ganzen Lauf ab. Es entsteht nur ein Teilbericht.");
        Tip(btnHtml, "Öffnet den Bericht im Browser.");
        Tip(btnFolder, "Öffnet den Berichtsordner dieses Laufs (Bericht, KI-Dateien, Anhang.zip).");
        Tip(btnExportRun, "Exportiert diesen Durchlauf als gepackte .minibench-Datei für den Austausch oder das Archiv.");
        Tip(btnNew, "Kehrt zur Startseite und den Ablauf-Einstellungen zurück.");
        Tip(btnKi, "Zeigt die KI-Datei im Explorer. Sie enthält Auftrag, Bericht und Rohdaten und lässt sich in ein KI-Modell hochladen.");
        Tip(cardK, "Kritische Befunde: Datenverlust oder Instabilität möglich, bald handeln.");
        Tip(cardW, "Warnungen: sollten behoben werden.");
        Tip(cardI, "Hinweise: zur Kenntnis, meist ohne Handlungsbedarf.");
        Tip(cardT, "Bestandene Tests im Verhältnis zu allen Tests dieses Laufs.");
        Tip(tabs, "Befunde: Ergebnisse nach Dringlichkeit. Tests: jede Prüfung mit Ergebnis. Leistung: Benchmark-Werte. Sensoren: Kurven von Benchmark und Lasttest. Protokoll: Ausgabe des Laufs.");
    }

    internal void ShowPage(int i)
    {
        // ab v4.07: kurze Überblendung von der bisherigen Seite (nur bei sichtbarem Fenster und erlaubter Bewegung)
        // Die Seite Versionen bleibt außen vor: Ihr Textfeld (RichTextBox) lässt sich nicht als Bild aufnehmen.
        Bitmap vorher = (i != curPage && content != null && i != versionPage && curPage != versionPage) ? Ueberblendung.Aufnahme(content) : null;
        curPage = i;
        for (int k = 0; k < nav.Count; k++) nav[k].Selected = (k == i);
        if (navErgebnis != null) navErgebnis.Selected = (i == ergebnisPage);
        // Band "Zurück zum Ergebnis" der Seite Benchmark gilt nur bis zum nächsten Seitenwechsel
        if (bandErgGezeigt) VerbergeErgebnisBand();
        content.AutoScrollPosition = new Point(0, 0);
        for (int k = 0; k < pages.Count; k++) pages[k].Visible = (k == i);
        if (i == ergebnisPage) ErgebnisSeiteGezeigt();
        // ab v4.0: Auf Werkzeugseiten (Tools bis Versionen) nur Ablaufzeile und Startknopf, ohne Vorlagen
        if (fo != null && foot != null) { bool modulSeite = i <= 5; if (fo.Visible != modulSeite) { fo.Visible = modulSeite; foot.Height = UI.S(modulSeite ? 76 : 62); } }
        if (i == 3) UpdateLoadExtras();
        // ab v4.05: installierte Softwarepakete beim ersten Öffnen der Seite Tools im Hintergrund erkennen
        if (i == 6) { if (!paketGeprueft) StartSoftwareStateCheck(); if (toolsLayout != null) toolsLayout(); }
        if (i == 1 || i == 2) OrdneModulSeiten();
        ApplyNativeControlThemes();
        if (vorher != null) Ueberblendung.Starte(content, vorher);
    }

    static CheckBox Chk(string text, bool on)
    {
        CheckBox c = new CheckBox(); c.Text = text; c.AutoSize = true; c.Checked = on; c.ForeColor = UI.Text; c.UseMnemonic = false;
        c.Font = new Font("Segoe UI", 9.5f);
        c.Margin = new Padding(UI.S(4), UI.S(3), 0, UI.S(3));
        return c;
    }

    static ComboBox Combo(int width, string[] items, int sel)
    {
        DarkComboBox c = new DarkComboBox(); c.Width = UI.S(width); c.Margin = new Padding(0, UI.S(1), UI.S(16), UI.S(1));
        foreach (string s in items) c.Items.Add(s);
        if (items.Length > 0) c.SelectedIndex = Math.Min(sel, items.Length - 1);
        return c;
    }

    // ab v4.0: Seitenkopf nur mit Titel und Einleitung. Ob ein Modul läuft, bestimmt allein der Haken in der Seitenleiste
    // (vorher zusätzlich ein Schalter "beim Start ausführen"). Ist das Modul nicht angehakt, steht unter der Einleitung
    // ein Hinweisband mit dem Verweis "In den Ablauf aufnehmen".
    FlowLayoutPanel Page(string title, string hint, int idx)
    {
        FlowLayoutPanel f = new FlowLayoutPanel(); f.FlowDirection = FlowDirection.TopDown; f.WrapContents = false; f.AutoSize = true; f.BackColor = UI.Panel; f.Location = new Point(0, 0);
        Label lt = Lbl(title, UI.SchriftTitel, true, UI.Text); lt.Margin = new Padding(0); f.Controls.Add(lt);
        Label h = Lbl(hint, UI.SchriftText, false, UI.Muted); h.MaximumSize = new Size(UI.S(UI.Gruppenbreite), 0); h.Margin = new Padding(0, UI.S(UI.Abstand1), 0, UI.S(UI.Abstand2)); f.Controls.Add(h);
        if (idx >= 0 && idx < nav.Count && nav[idx].HasCheck)
        {
            FlowLayoutPanel band = new FlowLayoutPanel(); band.AutoSize = true; band.WrapContents = false; band.BackColor = UI.InfoBg;
            band.Padding = new Padding(UI.S(10), UI.S(6), UI.S(10), UI.S(6)); band.Margin = new Padding(0, 0, 0, UI.S(UI.Abstand2)); band.Tag = "banner";
            Label bl = Lbl(title + " ist nicht im Ablauf. Die Auswahl auf dieser Seite wird erst ausgeführt, wenn das Modul links angehakt ist.", UI.SchriftText, false, UI.Text);
            bl.Margin = new Padding(0, UI.S(1), UI.S(12), 0); bl.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 230), 0);
            LinkLabel bk = new LinkLabel(); bk.Text = "In den Ablauf aufnehmen"; bk.AutoSize = true; bk.Font = new Font("Segoe UI Semibold", UI.SchriftText);
            bk.LinkColor = UI.Accent; bk.ActiveLinkColor = UI.AccentHover; bk.LinkBehavior = LinkBehavior.HoverUnderline; bk.BackColor = Color.Transparent; bk.Margin = new Padding(0, UI.S(1), 0, 0);
            int ni = idx;
            bk.LinkClicked += delegate { nav[ni].Checked = true; };
            Tip(bk, "Hakt das Modul in der Seitenleiste an; es läuft dann beim nächsten Start mit.");
            band.Controls.Add(bl); band.Controls.Add(bk);
            band.Visible = !nav[idx].Checked;
            nav[idx].CheckedChanged += delegate { band.Visible = !nav[ni].Checked; };
            if (idx < modBanners.Length) modBanners[idx] = band;
            f.Controls.Add(band);
        }
        return f;
    }

    static Label Section(string text)
    {
        Label l = Lbl(text, UI.SchriftAbschnitt, true, UI.Text); l.Margin = new Padding(0, UI.S(18), 0, UI.S(UI.Abstand2)); return l;
    }

    // Hinweis unter einem Element: mindestens 9 pt, höchstens Gruppenbreite
    static Label Hint(string text, int left)
    {
        Label l = Lbl(text, UI.SchriftHinweis, false, UI.Muted); l.MaximumSize = new Size(UI.S(UI.Gruppenbreite) - UI.S(left), 0);
        l.Margin = new Padding(UI.S(left), UI.S(2), 0, UI.S(UI.Abstand1)); return l;
    }

    // Option mit zwei Zeilen: kurze Bezeichnung, darunter eine kleinere Detailzeile; Etiketten (Risikostufe, Neustart) rechts
    // daneben. Ein Klick auf die Detailzeile schaltet die Option wie ein Klick auf ihren Text.
    Control Opt2(ButtonBase c, string detail, params Badge[] badges)
    {
        FlowLayoutPanel box = new FlowLayoutPanel(); box.FlowDirection = FlowDirection.TopDown; box.WrapContents = false; box.AutoSize = true; box.BackColor = UI.Panel;
        box.Margin = new Padding(0, UI.S(2), 0, UI.S(2));
        FlowLayoutPanel top = new FlowLayoutPanel(); top.AutoSize = true; top.WrapContents = false; top.BackColor = UI.Panel; top.Margin = new Padding(0);
        c.Margin = new Padding(UI.S(4), UI.S(2), 0, 0);
        top.Controls.Add(c);
        if (badges != null) foreach (Badge b in badges) if (b != null && b.Text.Length > 0) { BadgeTip(b); top.Controls.Add(b); }
        box.Controls.Add(top);
        if (!String.IsNullOrEmpty(detail))
        {
            Label d = Lbl(detail, UI.SchriftHinweis, false, UI.Muted); d.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 24), 0);
            d.Margin = new Padding(UI.S(24), 0, 0, UI.S(2)); d.Cursor = Cursors.Hand;
            d.Click += delegate { if (!c.Enabled) return; CheckBox cb = c as CheckBox; RadioButton rb = c as RadioButton; if (cb != null && cb.AutoCheck) cb.Checked = !cb.Checked; else if (rb != null) rb.Checked = true; };
            c.EnabledChanged += delegate { d.ForeColor = c.Enabled ? UI.Muted : DisabledText(); };
            box.Controls.Add(d);
        }
        return box;
    }

    static FlowLayoutPanel Row()
    {
        FlowLayoutPanel r = new FlowLayoutPanel(); r.AutoSize = true; r.WrapContents = false; r.BackColor = UI.Panel; r.Margin = new Padding(UI.S(4), UI.S(2), 0, UI.S(2));
        return r;
    }

    static Label RowLabel(string text, int width)
    {
        Label l = Lbl(text, 9.75f, false, UI.Text); l.AutoSize = false; l.Width = UI.S(width); l.Height = UI.S(24); l.TextAlign = ContentAlignment.MiddleLeft; l.Margin = new Padding(0, UI.S(1), UI.S(6), UI.S(1));
        return l;
    }

    // ab v3.64: Auswahl als Segmentschalter (RadioButton mit Knopfdarstellung), gemeinsam umschaltbar mit dem Farbschema
    static void StyleSegment(RadioButton r)
    {
        r.Appearance = Appearance.Button; r.FlatStyle = FlatStyle.Flat; r.TextAlign = ContentAlignment.MiddleCenter;
        r.FlatAppearance.BorderColor = UI.Line; r.FlatAppearance.BorderSize = 1;
        // gewählt: Akzentfläche mit heller Schrift (vorher Akzentschrift auf blassem Akzent, im Dunkelschema kaum lesbar)
        r.FlatAppearance.CheckedBackColor = UI.Accent;
        r.FlatAppearance.BorderColor = r.Checked ? UI.Accent : UI.Line;
        r.FlatAppearance.MouseOverBackColor = r.Checked ? UI.Accent : (UI.IsDark ? Color.FromArgb(45, 48, 52) : Color.FromArgb(243, 244, 246));
        r.BackColor = r.Checked ? UI.Accent : UI.Panel; r.ForeColor = r.Checked ? UI.OnAccent : UI.Text;
        r.Font = new Font(r.Checked ? "Segoe UI Semibold" : "Segoe UI", UI.SchriftText);
    }

    public static Color DisabledText() { return UI.IsDark ? Color.FromArgb(120, 125, 135) : Color.FromArgb(160, 163, 168); }

    // Text der Vertragszeile in Bezeichnung und Detail teilen: "Titel (Detail)" -> Titel, Detail
    public static string[] SplitDetail(string text)
    {
        string t = (text ?? "").Trim();
        // ", Neustart nötig" steht seit v3.64 als Etikett daneben
        if (t.EndsWith(", Neustart nötig")) t = t.Substring(0, t.Length - ", Neustart nötig".Length);
        int k = t.IndexOf(" (");
        if (k > 0 && t.EndsWith(")") && t.IndexOf('(', k + 2) < 0) return new string[] { t.Substring(0, k), t.Substring(k + 2, t.Length - k - 3) };
        return new string[] { t, "" };
    }

    static Badge RiskBadge(string risiko)
    {
        string r = risiko ?? "";
        if (r.Length == 0 || r == "Lesen") return null;
        Badge b = new Badge(ContractStep.RiskLabel(r), UI.RiskLevel(r)); b.Tag = "risiko"; return b;
    }

    static Badge RestartBadge(string neustart)
    {
        Badge b = neustart == "immer" ? new Badge("Neustart", "skip") : neustart == "moeglich" ? new Badge("Neustart möglich", "skip") : null;
        if (b != null) b.Tag = "neustart";
        return b;
    }

    // Hinweis eines Etiketts nach seiner Art (Risikostufe, Neustart, verwaltet, Zustand)
    void BadgeTip(Badge b)
    {
        if (b == null) return;
        string k = b.Tag as string;
        if (k == "risiko") Tip(b, b.Level == "info" ? "Ändern: wird mit Vorher-Wert protokolliert und lässt sich auf der Seite Änderungen zurücknehmen." : "Eingriff: nicht automatisch umkehrbar; der Weg zurück ist der Wiederherstellungspunkt.");
        else if (k == "neustart") Tip(b, b.Text == "Neustart" ? "Wird erst nach einem Neustart vollständig wirksam." : "Kann einen Neustart brauchen, je nach Zustand des PCs.");
        else if (k == "verwaltet") Tip(b, "Berührt Gruppenrichtlinien oder zentrale Verwaltung; auf Domänen-PCs nicht in den Vorlagen.");
        else if (k == "zustand") Tip(b, "Zustand auf diesem PC, beim Start im Hintergrund gelesen oder vom letzten Lauf.");
        else if (k == "dauer") Tip(b, "Geschätzte Dauer, wenn dieser Punkt eingeschaltet ist.");
        else if (k == "nebenher") Tip(b, "Läuft bei paralleler Ausführung neben den übrigen Prüfungen und verlängert die Diagnose dann nicht.");
    }

    // ab v3.65: Gruppen als abgerundete Karten (CardPanel) wie die Werkzeugkarten
    static Panel OptionCard(string categoryTitle, string icon, Control[] controls)
    {
        CardPanel card = new CardPanel();
        card.Margin = new Padding(0, UI.S(4), 0, UI.S(UI.Abstand2));
        // ab v3.64: alle Gruppen einer Seite gleich breit (vorher schrumpfte jede auf ihren Inhalt)
        card.MinimumSize = new Size(UI.S(UI.Gruppenbreite), 0);
        card.AutoSize = true;
        card.AutoSizeMode = AutoSizeMode.GrowAndShrink;

        FlowLayoutPanel inner = new FlowLayoutPanel();
        inner.FlowDirection = FlowDirection.TopDown;
        inner.WrapContents = false;
        inner.AutoSize = true;
        inner.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        inner.BackColor = UI.Panel;
        inner.Margin = new Padding(0);
        inner.Dock = DockStyle.Fill;

        // Symbol in der Symbolschrift (vorher in Segoe UI gezeichnet, dort erschien es als Punkt vor dem Titel)
        FlowLayoutPanel hr = new FlowLayoutPanel(); hr.AutoSize = true; hr.WrapContents = false; hr.BackColor = UI.Panel; hr.Margin = new Padding(0, 0, 0, UI.S(6));
        if (icon != null && icon.Length > 0)
        {
            Label li = new Label(); li.AutoSize = true; li.Text = icon; li.Font = UI.SymbolFont(11f); li.ForeColor = UI.Accent; li.Tag = "accent"; li.BackColor = Color.Transparent; li.Margin = new Padding(0, UI.S(2), UI.S(6), 0);
            hr.Controls.Add(li);
        }
        Label header = Lbl(categoryTitle, UI.SchriftGruppe, true, UI.Text); header.Margin = new Padding(0);
        hr.Controls.Add(header);
        inner.Controls.Add(hr);

        if (controls != null)
        {
            foreach (Control c in controls)
            {
                if (c != null) inner.Controls.Add(c);
            }
        }

        card.Controls.Add(inner);
        return card;
    }

    // Beliebigen Inhalt (Tabelle, Liste) in eine Karte der Gruppenbreite setzen
    static CardPanel Card(Control content)
    {
        CardPanel card = new CardPanel();
        card.Margin = new Padding(0, UI.S(4), 0, UI.S(UI.Abstand2));
        card.MinimumSize = new Size(UI.S(UI.Gruppenbreite), 0);
        card.AutoSize = true; card.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        content.Margin = new Padding(0); content.Dock = DockStyle.Fill;
        card.Controls.Add(content);
        return card;
    }
}
