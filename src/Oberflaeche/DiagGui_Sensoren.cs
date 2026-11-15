// Seite Sensoren live und Hilfswerkzeuge der Sensoren (Teilklasse DiagGui)
public partial class DiagGui
{
    // ------------------------------------------------------------ Seite Sensoren (live)
    string ToolsDir { get { return dataDir.Length > 0 ? Path.Combine(dataDir, "Tools") : ""; } }
    bool LhmPresent { get { return ToolsDir.Length > 0 && File.Exists(Path.Combine(ToolsDir, "LibreHardwareMonitor", "LibreHardwareMonitorLib.dll")); } }
    bool PawnSetupPresent { get { return ToolsDir.Length > 0 && File.Exists(Path.Combine(ToolsDir, "PawnIO", "PawnIO_setup.exe")); } }

    // Version des installierten PawnIO (wie LibreHardwareMonitor: Uninstall\PawnIO, sonst 64-Bit-Ansicht), leer = nicht installiert
    static string PawnIoVersion()
    {
        foreach (Microsoft.Win32.RegistryView v in new Microsoft.Win32.RegistryView[] { Microsoft.Win32.RegistryView.Default, Microsoft.Win32.RegistryView.Registry64 })
        {
            try
            {
                using (Microsoft.Win32.RegistryKey b = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, v))
                using (Microsoft.Win32.RegistryKey k = b.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO"))
                    if (k != null) { string s = Convert.ToString(k.GetValue("DisplayVersion")); if (s.Length > 0) return s; }
            }
            catch { }
        }
        return "";
    }

    // Wahl im Startdialog für diesen Lauf ("" = Einstellung der Seite Sensoren bzw. Geraete.json)
    string runKeep = "";

    // ab v3.61: Sensorwerkzeuge bei Bedarf automatisch und lautlos im Hintergrund holen, ohne Popups vor dem Start
    bool lhmAutoFetchAttempted = false;
    void TryAutoFetchSensorTools()
    {
        if (lhmAutoFetchAttempted || ToolsDir.Length == 0) return;
        if (LhmPresent && PawnSetupPresent) return;
        lhmAutoFetchAttempted = true;
        try
        {
            List<string> lines;
            RunHelper("-SensorWerkzeugeHolen", out lines);
            UpdateSensTools();
        }
        catch { }
    }

    // Einstellung für Hilfswerkzeuge je Gerät, gemeinsam mit dem Arbeitsprozess in Geraete.json (Format Minibench-Geraete/1)
    string DeviceFile { get { return dataDir.Length > 0 ? Path.Combine(dataDir, "Geraete.json") : ""; } }
    static readonly string[] keepValues = new string[] { "entfernen", "behalten" };
    static readonly string[] driverValues = new string[] { "verwenden", "nein" };

    Dictionary<string, object> ReadDeviceFile()
    {
        try
        {
            if (DeviceFile.Length == 0 || !File.Exists(DeviceFile)) return null;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(DeviceFile, Encoding.UTF8)) as Dictionary<string, object>;
            object fm; if (d == null || !d.TryGetValue("Format", out fm) || Convert.ToString(fm) != "Minibench-Geraete/1") return null;
            return d;
        }
        catch { return null; }
    }

    void LoadDeviceTools()
    {
        if (deviceId.Length == 0) return;
        Dictionary<string, object> d = ReadDeviceFile(); if (d == null) return;
        object g; if (!d.TryGetValue("Geraete", out g)) return;
        Dictionary<string, object> gd = g as Dictionary<string, object>; object e;
        if (gd == null || !gd.TryGetValue(deviceId, out e)) return;
        Dictionary<string, object> ed = e as Dictionary<string, object>; if (ed == null) return;
        object w; if (cmbKeep != null && ed.TryGetValue("Werkzeuge", out w)) {
            string ws = Convert.ToString(w);
            cmbKeep.SelectedIndex = (ws == "behalten") ? 1 : 0;
        }
        object t; if (cmbDriver != null && ed.TryGetValue("Treiber", out t)) {
            string ts = Convert.ToString(t);
            cmbDriver.SelectedIndex = (ts == "nein") ? 1 : 0;
        }
    }

    void SaveDeviceTools()
    {
        if (deviceId.Length == 0 || DeviceFile.Length == 0 || running || presetLoading) return;
        try
        {
            Dictionary<string, object> d = ReadDeviceFile() ?? new Dictionary<string, object>();
            d["Format"] = "Minibench-Geraete/1";
            object g; Dictionary<string, object> gd = d.TryGetValue("Geraete", out g) ? g as Dictionary<string, object> : null;
            if (gd == null) { gd = new Dictionary<string, object>(); d["Geraete"] = gd; }
            object e; Dictionary<string, object> ed = gd.TryGetValue(deviceId, out e) ? e as Dictionary<string, object> : null;
            if (ed == null) { ed = new Dictionary<string, object>(); ed["Behalten"] = new object[0]; gd[deviceId] = ed; }
            ed["Name"] = Environment.MachineName;
            if (cmbKeep != null) ed["Werkzeuge"] = keepValues[Math.Max(0, Math.Min(keepValues.Length - 1, cmbKeep.SelectedIndex))];
            if (cmbDriver != null) ed["Treiber"] = driverValues[Math.Max(0, Math.Min(driverValues.Length - 1, cmbDriver.SelectedIndex))];
            if (!ed.ContainsKey("Behalten")) ed["Behalten"] = new object[0];
            ed["Geaendert"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            Directory.CreateDirectory(dataDir);
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            File.WriteAllText(DeviceFile, js.Serialize(d), new UTF8Encoding(false));
        }
        catch { }
    }

    // Argument für den Arbeitsprozess: Wahl im Startdialog, sonst feste Einstellung
    string KeepArg()
    {
        string k = runKeep;
        if (k.Length == 0 && cmbKeep != null) k = keepValues[Math.Max(0, Math.Min(keepValues.Length - 1, cmbKeep.SelectedIndex))];
        return k.Length > 0 ? " -WerkzeugeBehalten " + k : "";
    }

    void UpdateSensStatusBadge(string stateOverride = null, string tooltipOverride = null)
    {
        if (badgeSensStatus == null) return;
        if (!String.IsNullOrEmpty(stateOverride))
        {
            SetSensorBadgeState(stateOverride, tooltipOverride);
            return;
        }

        string pv = PawnIoVersion();
        bool drvDisabled = (cmbDriver != null && cmbDriver.SelectedIndex == 1);
        if (drvDisabled)
        {
            SetSensorBadgeState("Inaktiv", "Inaktiv: Sensorüberwachung bzw. Sensortreiber explizit deaktiviert.");
        }
        else if (LhmPresent && (pv.Length > 0 || PawnSetupPresent))
        {
            SetSensorBadgeState("Vollstaendig", "Vollständig (Ring-0-Treiber aktiv): PawnIO / WinRing0 geladen, Live-Taktraten, Package-Power und Hotspot-Temperaturen verfügbar.");
        }
        else
        {
            SetSensorBadgeState("Eingeschraenkt", "Eingeschränkt (Standard-WMI / Leistungszähler): Basis-Takte und Standard-Temperaturen vorhanden, keine direkte VRM/Power-Telemetrie.");
        }
    }

    void SetSensorBadgeState(string state, string tooltip)
    {
        if (badgeSensStatus == null) return;
        SetzeSensorBadge(badgeSensStatus, state, tooltip);
        if (badgeSensSeite != null) SetzeSensorBadge(badgeSensSeite, state, tooltip);
    }

    void SetzeSensorBadge(Badge badgeSensStatus, string state, string tooltip)
    {
        switch (state)
        {
            case "Vollstaendig":
                badgeSensStatus.Text = "● Vollständig";
                badgeSensStatus.Level = "ok";
                Tip(badgeSensStatus, !String.IsNullOrEmpty(tooltip) ? tooltip : "Vollständig (Ring-0-Treiber aktiv): PawnIO / WinRing0 geladen, Live-Taktraten, Package-Power und Hotspot-Temperaturen verfügbar.");
                break;
            case "Eingeschraenkt":
                badgeSensStatus.Text = "● Eingeschränkt";
                badgeSensStatus.Level = "warn";
                Tip(badgeSensStatus, !String.IsNullOrEmpty(tooltip) ? tooltip : "Eingeschränkt (Standard-WMI / Leistungszähler): Basis-Takte und Standard-Temperaturen vorhanden, keine direkte VRM/Power-Telemetrie.");
                break;
            case "Inaktiv":
                badgeSensStatus.Text = "○ Inaktiv";
                badgeSensStatus.Level = "skip";
                Tip(badgeSensStatus, !String.IsNullOrEmpty(tooltip) ? tooltip : "Inaktiv: Sensorüberwachung explizit deaktiviert oder keine kompatible Hardware gefunden.");
                break;
            case "Fehler":
                badgeSensStatus.Text = "● Fehler";
                badgeSensStatus.Level = "crit";
                Tip(badgeSensStatus, !String.IsNullOrEmpty(tooltip) ? tooltip : "Fehler: Treiberabsturz oder Zugriffsverweigerung (Details im Protokoll).");
                break;
            default:
                badgeSensStatus.Text = state;
                badgeSensStatus.Level = "skip";
                Tip(badgeSensStatus, tooltip ?? "");
                break;
        }
    }

    void UpdateSensTools()
    {
        if (rowSensLhm == null) return;
        string pv = PawnIoVersion();
        // ab v4.10 zwei Zeilen mit Etikett statt eines Fließtexts; der volle Text steht im Hinweis beim Überfahren
        string tl = LhmPresent ? "liegt im Tools-Ordner (geladen wird nur mit passender Prüfsumme)" : "fehlt im Tools-Ordner; ohne die Bibliothek zeigt die Seite nur Ersatzwerte (Windows-Takt, ACPI, nvidia-smi, Datenträger, Akku)";
        rowSensLhm.Description = tl; rowSensLhm.StatusText = LhmPresent ? "vorhanden" : "fehlt"; rowSensLhm.StatusLevel = LhmPresent ? "ok" : "warn";
        Tip(rowSensLhm, "LibreHardwareMonitor " + tl + ".");
        string tp = pv.Length > 0 ? "Version " + pv + " ist auf diesem PC installiert und bleibt unverändert" : PawnSetupPresent ? "nicht installiert; das Setup liegt im Tools-Ordner bereit" : "nicht installiert; das Setup fehlt im Tools-Ordner";
        rowSensPawn.Description = tp; rowSensPawn.StatusText = pv.Length > 0 ? "installiert" : PawnSetupPresent ? "bereit" : "fehlt"; rowSensPawn.StatusLevel = pv.Length > 0 ? "ok" : PawnSetupPresent ? "skip" : "warn";
        Tip(rowSensPawn, "PawnIO: " + tp + ".");
        btnSensTools.Text = LhmPresent && PawnSetupPresent ? "Erneut holen" : "Sensorwerkzeuge holen";
        UpdateSensStatusBadge();
        StartUpdateCheck();
    }

    // Einzelner Wert der Sensorliste (ab v4.10 ohne Fensterelement je Zeile)
    public class SensorWert
    {
        public string Gruppe = "", Name = "", Art = "", Quelle = "", Aktuell = "", Min = "", Max = "", Hinweis = "";
        public bool Blass, Unplausibel;
    }

    internal ModulSeite sensSeite;
    internal KennzahlZeile sensKpi;
    internal ToolRow rowSensLhm, rowSensPawn;
    internal SettingRow rowSensTreiber, rowSensBehalten;
    internal Label lblSensAufz, lblSensVoll, lblSensAnzahl;
    Badge badgeSensSeite;
    internal Dictionary<int, SensorWert> sensWerte = new Dictionary<int, SensorWert>();
    internal List<string> sensGruppen = new List<string>();
    internal HashSet<string> sensZu = new HashSet<string>();
    // Höchstzahl der aufgezeichneten Messzeilen; danach nimmt die Aufzeichnung nichts mehr auf und sagt das (bis v4.09 stillschweigend)
    public const int SensZeilenMax = 50000;
    internal bool sensVoll;
    DateTime liveStart = DateTime.MinValue;
    double sensErsteSek = double.NaN, sensLetzteSek = double.NaN;
    // Kennzahlen: Spalte in SENSLEAD, Einheit; Min und Max seit dem Start der Live-Ansicht
    static readonly int[] kpiSpalte = new int[] { 1, 2, 3, 4, 6, 7 };
    static readonly string[] kpiTitel = new string[] { "CPU-Temperatur", "CPU-Takt", "CPU-Leistung", "GPU-Temperatur", "GPU-Leistung", "Lüfter" };
    static readonly string[] kpiEinheit = new string[] { "°C", "MHz", "W", "°C", "W", "U/min" };
    double[] kpiMin = new double[6], kpiMax = new double[6];

    static string KpiZahl(double v) { return double.IsNaN(v) ? "" : Math.Round(v).ToString("N0", CultureInfo.GetCultureInfo("de-DE")); }

    void LeereKennzahlen() { for (int i = 0; i < 6; i++) { kpiMin[i] = double.MaxValue; kpiMax[i] = double.MinValue; } if (sensKpi != null) sensKpi.Leere(); }

    // Kennzahlen aus einer Zeile SENSLEAD (wie sie die Kurve bekommt); fehlt ein Wert, bleibt die Kachel bei "kein Wert"
    internal void SetzeKennzahlen(double[] r)
    {
        if (sensKpi == null || r == null) return;
        for (int i = 0; i < 6; i++)
        {
            double v = kpiSpalte[i] < r.Length ? r[kpiSpalte[i]] : double.NaN;
            if (double.IsNaN(v)) { if (kpiMax[i] == double.MinValue) sensKpi.Setze(i, "", ""); continue; }
            kpiMin[i] = Math.Min(kpiMin[i], v); kpiMax[i] = Math.Max(kpiMax[i], v);
            string unter = "Min " + KpiZahl(kpiMin[i]) + "  \u00B7  Max " + KpiZahl(kpiMax[i]);
            if (i == 1 && r.Length > 9 && !double.IsNaN(r[9])) unter = "höchster Kern " + KpiZahl(r[9]);
            sensKpi.Setze(i, KpiZahl(v), unter);
        }
    }

    static string DauerText(double sek)
    {
        if (double.IsNaN(sek) || sek < 0) return "0 s";
        int s = (int)Math.Round(sek);
        return s >= 3600 ? (s / 3600) + " h " + (s % 3600 / 60) + " min" : s >= 60 ? (s / 60) + " min " + (s % 60) + " s" : s + " s";
    }

    // Kopf rechts, Karte Aufzeichnung und Anzahl der Sensoren
    internal void UpdateSensStand()
    {
        if (sensSeite == null) return;
        if (liveRunning && liveStart != DateTime.MinValue)
        {
            TimeSpan t = DateTime.Now - liveStart;
            SetText(sensSeite.Anzahl, "Live seit " + ((int)t.TotalMinutes).ToString("00") + ":" + t.Seconds.ToString("00"));
        }
        else SetText(sensSeite.Anzahl, "Live-Ansicht aus");
        SetText(sensSeite.Dauer, sensWerte.Count > 0 ? Mehrzahl(sensWerte.Count, "Sensor", "Sensoren") : "keine Messwerte");
        SetText(lblSensAnzahl, sensWerte.Count > 0 ? Mehrzahl(sensWerte.Count, "Wert", "Werte") : "keine Werte");
        string z = sensRows.Count.ToString("N0", CultureInfo.GetCultureInfo("de-DE"));
        SetText(lblSensAufz, sensRows.Count == 0 ? "Noch nichts aufgezeichnet." : "Dauer " + DauerText(sensLetzteSek - sensErsteSek) + "  \u00B7  " + z + (sensRows.Count == 1 ? " Messzeile" : " Messzeilen") + "  \u00B7  " + Mehrzahl(sensWerte.Count, "Sensor", "Sensoren"));
        if (lblSensVoll != null && lblSensVoll.Visible != sensVoll) lblSensVoll.Visible = sensVoll;
    }

    // Zeilen der Sensorliste aus Gruppen und Werten
    internal void FillSensList()
    {
        if (lvSens == null) return;
        lvSens.Zeilen.Clear();
        foreach (string grp in sensGruppen)
        {
            int n = 0; foreach (SensorWert w in sensWerte.Values) if (w.Gruppe == grp) n++;
            ListenZeile kz = new ListenZeile(); kz.Art = "gruppe"; kz.Tag = grp; kz.Grund = 1; kz.Aufklappbar = true; kz.Offen = !sensZu.Contains(grp); kz.Waehlbar = false; kz.Hoehe = UI.S(28);
            kz.Hinweis = Mehrzahl(n, "Wert", "Werte");
            lvSens.Zeilen.Add(kz);
            if (!kz.Offen) continue;
            List<int> ids = new List<int>(sensWerte.Keys); ids.Sort();
            foreach (int id in ids)
            {
                SensorWert w = sensWerte[id]; if (w.Gruppe != grp) continue;
                ListenZeile z = new ListenZeile(); z.Art = "wert"; z.Tag = w; z.Waehlbar = false; z.Hoehe = UI.S(26); z.Blass = w.Blass; z.Hinweis = w.Hinweis;
                lvSens.Zeilen.Add(z);
            }
        }
        lvSens.Aktualisiere();
    }

    void ZeichneSensZeile(Graphics g, ListenZeile z, Rectangle r, bool markiert)
    {
        if (z.Art == "gruppe")
        {
            string grp = Convert.ToString(z.Tag); int p = grp.IndexOf("  \u00B7  ");
            string a = p > 0 ? grp.Substring(0, p) : grp, b = p > 0 ? grp.Substring(p + 5) : "";
            int aw = ZeilenListe.Breite(a, ZeilenListe.FKopf);
            ZeilenListe.Schreibe(g, a, ZeilenListe.FKopf, new Rectangle(r.X, r.Y, aw, r.Height), UI.Text, false);
            if (b.Length > 0) ZeilenListe.Schreibe(g, "\u00B7  " + b + (z.Offen ? "" : "  (" + z.Hinweis + ")"), ZeilenListe.FKlein, new Rectangle(r.X + aw + UI.S(4), r.Y, r.Right - r.X - aw - UI.S(14), r.Height), UI.Muted, false);
            return;
        }
        SensorWert w = (SensorWert)z.Tag;
        Rectangle zeile = new Rectangle(0, r.Y, lvSens.Width, r.Height);
        Color c = w.Blass ? UI.Muted : UI.Text;
        Rectangle c0 = lvSens.Zelle(0, zeile);
        ZeilenListe.Schreibe(g, w.Name, ZeilenListe.FKlein, new Rectangle(r.X, r.Y, Math.Max(0, c0.Right - r.X - UI.S(6)), r.Height), c, false);
        ZeilenListe.Schreibe(g, w.Art, ZeilenListe.FKlein, lvSens.Zelle(1, zeile), UI.Muted, false);
        Rectangle ca = lvSens.Zelle(2, zeile);
        if (w.Unplausibel) { int bw = ZeilenListe.EtikettBreite("unplausibel"); ZeilenListe.Etikett(g, "unplausibel", "crit", ca.Right - bw, r, 0); }
        else ZeilenListe.Schreibe(g, w.Aktuell, ZeilenListe.FKopf, ca, c, true);
        ZeilenListe.Schreibe(g, w.Min, ZeilenListe.FKlein, lvSens.Zelle(3, zeile), UI.Muted, true);
        ZeilenListe.Schreibe(g, w.Max, ZeilenListe.FKlein, lvSens.Zelle(4, zeile), UI.Muted, true);
        Rectangle cq = lvSens.Zelle(5, zeile);
        ZeilenListe.Schreibe(g, w.Quelle, ZeilenListe.FKlein, new Rectangle(cq.X + UI.S(12), cq.Y, cq.Width - UI.S(12), cq.Height), UI.Muted, false);
    }

    // ab v4.10: Seite im Muster der Modulseiten. Oben Kennzahlen, links Verlauf und alle Sensoren, rechts Sensorwerkzeuge,
    // Einstellungen und Aufzeichnung. cmbDriver und cmbKeep bleiben als Werte bestehen (Start eines Laufs, Vorlagen,
    // Geraete.json), angezeigt werden Schalter.
    Control BuildSensorPage()
    {
        FlowLayoutPanel f = Page("Sensoren live", "Zeigt Temperatur, Takt, Lüfter, Spannung und Leistung laufend an. CPU-Werte und Mainboard-Lüfter brauchen den Treiber PawnIO; er wird beim Beenden wieder entfernt. Während eines Laufs ist die Live-Ansicht aus; der Lasttest zeigt seine Kurven im Reiter Sensoren.", -1);
        sensSeite = NeueModulSeite(f);
        btnLive = UI.Primary("Live-Ansicht starten"); btnLive.Margin = new Padding(0); btnLive.Padding = new Padding(UI.S(14), UI.S(4), UI.S(14), UI.S(4)); btnLive.Font = new Font("Segoe UI Semibold", 9.75f);
        btnLive.Click += delegate { if (liveRunning) StopLive(); else StartLive(); };
        Tip(btnLive, "Startet oder beendet die laufende Anzeige aller Sensoren. Beim Beenden wird ein vorübergehend installierter Treiber wieder entfernt.");
        sensSeite.KopfKnopf(btnLive);

        sensKpi = new KennzahlZeile(kpiTitel, kpiEinheit, new Color[] { SensorChart.CpuFarbe, SensorChart.CpuFarbe, SensorChart.CpuFarbe, SensorChart.GpuFarbe, SensorChart.GpuFarbe, Color.FromArgb(156, 163, 175) });
        Tip(sensKpi, "Leitwerte der Live-Ansicht mit dem kleinsten und größten Wert seit dem Start. Ohne Messwert steht \"kein Wert\".");
        f.Controls.Add(sensKpi); sensSeite.Voll.Add(sensKpi);
        LeereKennzahlen();
        lblSensInfo = Lbl("Live-Ansicht nicht gestartet.", UI.SchriftHinweis, false, UI.Muted); lblSensInfo.Margin = new Padding(0, 0, 0, UI.S(UI.Abstand2));
        f.Controls.Add(lblSensInfo); sensSeite.Voll.Add(lblSensInfo);

        // links: Verlauf und alle Sensoren
        ZeilenKarte kv = sensSeite.Karte("Verlauf", true);
        Label lz = Lbl("letzte zehn Minuten", UI.SchriftHinweis, false, UI.Muted); lz.Margin = new Padding(0, UI.S(2), 0, 0); kv.KopfRechts.Controls.Add(lz);
        chartLive = new SensorChart(); chartLive.Rahmen = false; chartLive.Height = UI.S(330); chartLive.WindowSec = 600; chartLive.Margin = new Padding(0, 0, 0, UI.S(2));
        chartLive.Empty = "Noch keine Messwerte. \"Live-Ansicht starten\" öffnet die Sensoren.";
        kv.Zeile(chartLive);

        ZeilenKarte kl = sensSeite.Karte("Alle Sensoren", true);
        lblSensAnzahl = Lbl("keine Werte", UI.SchriftHinweis, false, UI.Muted); lblSensAnzahl.Margin = new Padding(0, UI.S(2), 0, 0); kl.KopfRechts.Controls.Add(lblSensAnzahl);
        lvSens = new ZeilenListe(); lvSens.AutoHoehe = true; lvSens.Rahmen = false; lvSens.Tipp = tip; lvSens.Zeichner = ZeichneSensZeile; lvSens.TabStop = false; lvSens.Margin = new Padding(0, 0, 0, UI.S(2));
        lvSens.Leer = "Noch keine Messwerte.";
        lvSens.Spalten.Add(new ListenSpalte("Sensor", 0, false, 0)); lvSens.Spalten.Add(new ListenSpalte("Art", UI.S(128), false, 1));
        lvSens.Spalten.Add(new ListenSpalte("Aktuell", UI.S(88), true, 2)); lvSens.Spalten.Add(new ListenSpalte("Min", UI.S(80), true, 3)); lvSens.Spalten.Add(new ListenSpalte("Max", UI.S(80), true, 4));
        lvSens.Spalten.Add(new ListenSpalte("    Quelle", UI.S(92), false, 5));
        foreach (ListenSpalte sp in lvSens.Spalten) sp.Sortierbar = false;
        lvSens.AufklappGeklickt += delegate(ListenZeile z) { string grp = Convert.ToString(z.Tag); if (!sensZu.Remove(grp)) sensZu.Add(grp); FillSensList(); };
        Tip(lvSens, "Alle Werte der Live-Ansicht je Gerät mit dem kleinsten und größten Wert seit dem Start. Grenzwerte des Herstellers stehen grau, verworfene Werte als \"unplausibel\" mit dem Grund im Hinweis.");
        kl.Zeile(lvSens);
        lvSens.Aktualisiere();

        // rechts: Sensorwerkzeuge
        ZeilenKarte kw = sensSeite.Karte("Sensorwerkzeuge", false);
        badgeSensSeite = new Badge("", "ok"); badgeSensSeite.Margin = new Padding(UI.S(6), UI.S(2), 0, 0); kw.KopfRechts.Controls.Add(badgeSensSeite);
        rowSensLhm = new ToolRow("LibreHardwareMonitor", "", UI.IcoSens); rowSensPawn = new ToolRow("PawnIO", "", UI.IcoShield);
        kw.Zeile(rowSensLhm); kw.Zeile(rowSensPawn);
        FlowLayoutPanel bt = Row(); bt.Margin = new Padding(0, UI.S(4), 0, UI.S(2));
        btnSensTools = UI.Secondary("Sensorwerkzeuge holen"); btnSensTools.Margin = new Padding(0); btnSensTools.Click += delegate { FetchSensorTools(); };
        bt.Controls.Add(btnSensTools);
        kw.Frei(bt);
        Label hw = Lbl("Aus den offiziellen GitHub-Releases, mit festen SHA-256-Werten geprüft. Abgelegt wird nur im Tools-Ordner.", 8.5f, false, UI.Muted); hw.Margin = new Padding(0, UI.S(4), 0, UI.S(2));
        kw.Frei(hw);

        // rechts: Einstellungen (Schalter als Anzeige, Auswahllisten als Wert)
        ZeilenKarte ke = sensSeite.Karte("Einstellungen", false);
        cmbDriver = Combo(270, new string[] { "vorübergehend verwenden (Standard)", "nicht verwenden" }, 0);
        cmbKeep = Combo(270, new string[] { "nach dem Lauf entfernen (Standard)", "auf diesem PC behalten" }, 0);
        LoadDeviceTools();
        rowSensTreiber = new SettingRow("PawnIO-Treiber vorübergehend verwenden", "Für CPU-Werte und Mainboard-Lüfter. Gilt für Live-Ansicht, Diagnose, Benchmark und Lasttest; ein vorhandener PawnIO bleibt unverändert.");
        rowSensBehalten = new SettingRow("Hilfswerkzeuge auf diesem PC behalten", "PawnIO und smartmontools, die Leos Minibench installiert hat. Aus: nach dem Lauf entfernen. Gilt je Gerät.");
        rowSensTreiber.Checked = cmbDriver.SelectedIndex == 0; rowSensBehalten.Checked = cmbKeep.SelectedIndex == 1;
        rowSensTreiber.Toggle.CheckedChanged += delegate { int i = rowSensTreiber.Toggle.Checked ? 0 : 1; if (cmbDriver.SelectedIndex != i) cmbDriver.SelectedIndex = i; };
        rowSensBehalten.Toggle.CheckedChanged += delegate { int i = rowSensBehalten.Toggle.Checked ? 1 : 0; if (cmbKeep.SelectedIndex != i) cmbKeep.SelectedIndex = i; };
        cmbKeep.SelectedIndexChanged += delegate { bool an = cmbKeep.SelectedIndex == 1; if (rowSensBehalten.Toggle.Checked != an) rowSensBehalten.Toggle.Checked = an; SaveDeviceTools(); };
        cmbDriver.SelectedIndexChanged += delegate { bool an = cmbDriver.SelectedIndex == 0; if (rowSensTreiber.Toggle.Checked != an) rowSensTreiber.Toggle.Checked = an; SaveDeviceTools(); UpdateSensStatusBadge(); };
        ke.Zeile(rowSensTreiber); ke.Zeile(rowSensBehalten);

        // rechts: Aufzeichnung
        ZeilenKarte ka = sensSeite.Karte("Aufzeichnung", false);
        lblSensAufz = Lbl("Noch nichts aufgezeichnet.", UI.SchriftText, false, UI.Text); lblSensAufz.Margin = new Padding(0, 0, 0, UI.S(6));
        ka.Frei(lblSensAufz);
        lblSensVoll = Lbl("Aufzeichnung voll: Nach " + SensZeilenMax.ToString("N0", CultureInfo.GetCultureInfo("de-DE")) + " Messzeilen nimmt sie nichts mehr auf. Die Anzeige läuft weiter; zum Weiterschreiben speichern und die Live-Ansicht neu starten.", UI.SchriftHinweis, false, UI.Warn);
        lblSensVoll.Margin = new Padding(0, 0, 0, UI.S(6)); lblSensVoll.Visible = false;
        ka.Frei(lblSensVoll);
        FlowLayoutPanel bs = Row(); bs.Margin = new Padding(0, 0, 0, UI.S(2));
        btnSensSave = UI.Secondary("Als CSV speichern"); btnSensSave.Margin = new Padding(0); btnSensSave.Enabled = false; btnSensSave.Click += delegate { SaveSensorCsv(); };
        bs.Controls.Add(btnSensSave);
        ka.Frei(bs);
        Label hs = Lbl("Nach dem Beenden der Live-Ansicht. Die Datei liegt danach unter Berichte\\Sensoren.", 8.5f, false, UI.Muted); hs.Margin = new Padding(0, UI.S(4), 0, UI.S(2));
        ka.Frei(hs);

        f.Controls.Add(sensSeite.Spalten);
        liveTimer = new Timer(); liveTimer.Interval = 250; liveTimer.Tick += delegate { OnLiveTick(); };
        UpdateSensTools(); UpdateSensStand();
        sensSeite.Ordne(SeitenBreite());
        return f;
    }

    void FetchSensorTools()
    {
        if (liveRunning) { MessageBox.Show(this, "Bitte zuerst die Live-Ansicht beenden.", "Leos Minibench"); return; }
        if (MessageBox.Show(this, "LibreHardwareMonitor " + "0.9.6 und PawnIO-Setup 2.2.0 aus den offiziellen GitHub-Releases holen?\r\n\r\nDie Dateien werden mit festen SHA-256-Werten geprüft und nur im Tools-Ordner des Datenordners abgelegt. Auf diesem PC wird nichts installiert.", "Sensorwerkzeuge holen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        List<string> lines; string res = RunHelperPumped("-SensorWerkzeugeHolen", out lines, 900);
        UpdateSensTools();
        MessageBox.Show(this, lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Keine Rückmeldung vom Arbeitsprozess.", "Sensorwerkzeuge", MessageBoxButtons.OK, res == "1" ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    void StartLive()
    {
        if (running) return;
        TryAutoFetchSensorTools();
        bool drv = cmbDriver == null || cmbDriver.SelectedIndex == 0;
        try { Directory.CreateDirectory(cpDir); File.Delete(Path.Combine(cpDir, "sensor.stop")); } catch { }
        string args = "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\" -EventMode -SensorLive" + (dataDir.Length > 0 ? " -DatenDir " + Q(dataDir) : "") + (drv ? " -SensorTreiber" : "") + KeepArg();
        ProcessStartInfo psi = new ProcessStartInfo(psExe, args);
        psi.UseShellExecute = false; psi.CreateNoWindow = true; psi.RedirectStandardOutput = true; psi.RedirectStandardError = true; psi.RedirectStandardInput = true;
        psi.StandardOutputEncoding = new UTF8Encoding(false); psi.StandardErrorEncoding = OemEncoding();
        liveProc = new Process(); liveProc.StartInfo = psi; liveProc.EnableRaisingEvents = true;
        liveProc.OutputDataReceived += delegate(object s, DataReceivedEventArgs e) { if (e.Data != null) liveQueue.Enqueue(e.Data); else liveEof = true; };
        liveProc.ErrorDataReceived += delegate(object s, DataReceivedEventArgs e) { if (e.Data != null && e.Data.Trim().Length > 0) liveQueue.Enqueue("@@SENSINFO|" + e.Data.Trim()); };
        liveProc.Exited += delegate { liveExited = true; };
        sensWerte.Clear(); sensMinMax.Clear(); sensUnit.Clear(); sensNames.Clear(); sensRows.Clear(); sensGruppen.Clear(); sensZu.Clear();
        sensVoll = false; sensErsteSek = double.NaN; sensLetzteSek = double.NaN;
        FillSensList(); chartLive.Clear(); LeereKennzahlen();
        string dummy; while (liveQueue.TryDequeue(out dummy)) { }
        liveExited = false; liveEof = false; liveStopping = false; liveExitSeen = DateTime.MinValue;
        try { liveProc.Start(); liveProc.BeginOutputReadLine(); liveProc.BeginErrorReadLine(); try { liveProc.StandardInput.Close(); } catch { } }
        catch (Exception ex) { MessageBox.Show(this, "PowerShell konnte nicht gestartet werden: " + ex.Message, "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
        liveRunning = true; liveStart = DateTime.Now;
        btnLive.Text = "Live-Ansicht beenden"; btnSensTools.Enabled = false; btnSensSave.Enabled = false;
        lblSensInfo.Text = drv ? "Wird gestartet, PawnIO wird vorübergehend installiert ..." : "Wird gestartet ...";
        UpdateSensStand();
        liveTimer.Start();
    }

    void StopLive()
    {
        if (!liveRunning || liveStopping) return;
        liveStopping = true; liveStopAt = DateTime.Now;
        try { Directory.CreateDirectory(cpDir); File.WriteAllText(Path.Combine(cpDir, "sensor.stop"), "1"); } catch { }
        btnLive.Enabled = false; btnLive.Text = "Wird beendet ...";
        lblSensInfo.Text = "Wird beendet, ein vorübergehend installierter Treiber wird entfernt ...";
    }

    // Für Start eines Laufs und Schließen des Fensters: sauber beenden (Treiber entfernen), höchstens 60 Sekunden warten
    bool StopLiveAndWait()
    {
        if (!liveRunning) return true;
        StopLive();
        Cursor = Cursors.WaitCursor; Enabled = false;
        // Installation und Entfernung von PawnIO dürfen je bis zu 3 Minuten dauern
        DateTime until = DateTime.Now.AddSeconds(240);
        while (!liveExited && DateTime.Now < until) { Application.DoEvents(); System.Threading.Thread.Sleep(100); }
        bool killed = !liveExited;
        if (killed) KillProc(liveProc);
        DateTime eofUntil = DateTime.Now.AddSeconds(5);
        while (!liveEof && DateTime.Now < eofUntil) { Application.DoEvents(); System.Threading.Thread.Sleep(50); }
        liveExited = true;
        Enabled = true; Cursor = Cursors.Default;
        OnLiveTick();
        if (killed) CleanupDriver();
        return true;
    }

    void OnLiveTick()
    {
        string line; int n = 0;
        if (!liveQueue.IsEmpty)
        {
            sensListeNeu = false;
            while (n < 2000 && liveQueue.TryDequeue(out line)) { n++; HandleLiveLine(line); }
            if (sensListeNeu) FillSensList(); else lvSens.Invalidate();
        }
        chartLive.Flush();
        UpdateSensStand();
        bool killed = false;
        if (liveStopping && !liveExited && (DateTime.Now - liveStopAt).TotalSeconds > 240) { KillProc(liveProc); killed = true; }
        if (liveExited && liveExitSeen == DateTime.MinValue) liveExitSeen = DateTime.Now;
        if (liveExited && (liveEof || (DateTime.Now - liveExitSeen).TotalSeconds > 5) && liveQueue.IsEmpty && liveRunning)
        {
            liveRunning = false; liveStopping = false; liveTimer.Stop();
            btnLive.Enabled = true; btnLive.Text = "Live-Ansicht starten"; btnSensTools.Enabled = true; btnSensSave.Enabled = sensRows.Count > 0;
            if (killed || PawnMarkerExists) CleanupDriver();
            UpdateSensTools(); UpdateSensStand();
            ReloadDb();
        }
    }

    // Markierung eines vorübergehend installierten PawnIO (wie Get-PawnIoMarker im Skript)
    string PawnMarker
    {
        get
        {
            if (dataDir.Length == 0) return "";
            string pc = Environment.GetEnvironmentVariable("COMPUTERNAME"); if (String.IsNullOrEmpty(pc)) pc = Environment.MachineName;
            string n = System.Text.RegularExpressions.Regex.Replace(pc, "[\\/:*?\"<>|\\s]+", "_").Trim('_');
            return Path.Combine(Path.Combine(dataDir, "Laufzeit"), "PawnIO_" + n + ".txt");
        }
    }
    bool PawnMarkerExists { get { string m = PawnMarker; return m.Length > 0 && File.Exists(m); } }

    // Treiberrest eines abgebrochenen Arbeitsprozesses sofort entfernen statt erst beim nächsten Start
    void CleanupDriver()
    {
        if (running) return;
        if (!PawnMarkerExists) return;
        List<string> lines;
        string ok = RunHelperPumped("-SensorAufraeumen", out lines, 300);
        string t = lines.Count > 0 ? String.Join(" ", lines.ToArray()) : "PawnIO-Treiber: keine Rückmeldung";
        if (lblSensInfo != null) lblSensInfo.Text = t;
        if (running || done) { lblCounter.Text = ok == "1" ? "PawnIO-Treiber wieder entfernt" : "PawnIO-Treiber noch installiert"; }
        if (ok != "1" && txtLog != null)
        {
            try { txtLog.AppendText("\r\n[Sensoren] Warnung: PawnIO-Treiber konnte nicht entfernt werden: " + t + "\r\n"); } catch { }
        }
    }

    string RunHelper(string args, out List<string> lines)
    {
        lines = new List<string>();
        ProcessStartInfo psi = new ProcessStartInfo(psExe, "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\" -EventMode " + args + " -DatenDir \"" + dataDir.TrimEnd('\\') + "\"");
        psi.UseShellExecute = false; psi.CreateNoWindow = true; psi.RedirectStandardOutput = true; psi.StandardOutputEncoding = new UTF8Encoding(false);
        string result = "";
        using (Process p = Process.Start(psi))
        {
            string all = p.StandardOutput.ReadToEnd(); p.WaitForExit(120000);
            foreach (string line in all.Split('\n'))
            {
                string l = line.TrimEnd('\r');
                if (l.StartsWith("@@LOG|")) { string[] x = l.Split(new char[] { '|' }, 3); if (x.Length == 3 && x[2].Trim().Length > 0) lines.Add(x[2].Trim()); }
                else if (l.StartsWith("@@RESULT|")) result = l.Substring(9);
            }
        }
        return result;
    }

    // Wie RunHelper, aber ohne die Oberfläche einzufrieren (Downloads, Treiber entfernen)
    string RunHelperPumped(string args, out List<string> lines, int timeoutSec) { return RunHelperPumped(args, out lines, timeoutSec, null); }

    // Wie RunHelper, aber ohne die Oberfläche einzufrieren; Fortschritt des Abgleichs (@@ABGLEICH|Nr|Anzahl|Datei) im Label
    string RunHelperPumped(string args, out List<string> lines, int timeoutSec, Label progress)
    {
        List<string> got = new List<string>();
        string result = "";
        ProcessStartInfo psi = new ProcessStartInfo(psExe, "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\" -EventMode " + args + (dataDir.Length > 0 ? " -DatenDir \"" + dataDir.TrimEnd('\\') + "\"" : ""));
        psi.UseShellExecute = false; psi.CreateNoWindow = true; psi.RedirectStandardOutput = true; psi.StandardOutputEncoding = new UTF8Encoding(false);
        bool eof = false;
        Cursor = Cursors.WaitCursor; bool was = Enabled; Enabled = false;
        try
        {
            using (Process p = new Process())
            {
                p.StartInfo = psi;
                p.OutputDataReceived += delegate(object s, DataReceivedEventArgs e) {
                    if (e.Data == null) { eof = true; return; }
                    if (progress != null && e.Data.StartsWith("@@ABGLEICH|"))
                    {
                        string[] x = e.Data.Split(new char[] { '|' }, 4);
                        if (x.Length == 4) { string t = "Abgleich: " + x[1] + " von " + x[2] + ", " + x[3].Replace("¦", "|"); try { BeginInvoke(new MethodInvoker(delegate { SetText(progress, t); })); } catch { } }
                        return;
                    }
                    lock (got) got.Add(e.Data);
                };
                p.Start(); p.BeginOutputReadLine();
                DateTime until = DateTime.Now.AddSeconds(timeoutSec);
                while (!p.HasExited && DateTime.Now < until) { Application.DoEvents(); System.Threading.Thread.Sleep(50); }
                if (!p.HasExited) { KillProc(p); lock (got) got.Add("@@LOG||Abgebrochen nach " + timeoutSec + " Sekunden."); }
                DateTime eu = DateTime.Now.AddSeconds(5);
                while (!eof && DateTime.Now < eu) { Application.DoEvents(); System.Threading.Thread.Sleep(20); }
            }
        }
        catch (Exception ex) { lock (got) got.Add("@@LOG||" + ex.Message); }
        finally { Enabled = was; Cursor = Cursors.Default; }
        lines = new List<string>();
        lock (got)
            foreach (string l in got)
            {
                if (l.StartsWith("@@LOG|")) { string[] x = l.Split(new char[] { '|' }, 3); if (x.Length == 3 && x[2].Trim().Length > 0) lines.Add(x[2].Trim()); }
                else if (l.StartsWith("@@RESULT|")) result = l.Substring(9);
                else if (l.StartsWith("@@OPTZ|")) lines.Add(l);
            }
        return result;
    }

    static void KillProc(Process p)
    {
        try
        {
            if (p != null && !p.HasExited)
            {
                ProcessStartInfo k = new ProcessStartInfo("taskkill.exe", "/T /F /PID " + p.Id); k.CreateNoWindow = true; k.UseShellExecute = false;
                Process kp = Process.Start(k); kp.WaitForExit(8000);
            }
        }
        catch { }
    }

    static double ParseD(string s) { double v; return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : double.NaN; }

    static string FmtSens(double v, string unit)
    {
        if (double.IsNaN(v)) return "";
        string f = unit == "V" ? "0.000" : (unit == "W" || unit == "A") ? "0.0" : "0";
        return v.ToString(f, CultureInfo.GetCultureInfo("de-DE")) + " " + unit;
    }

    bool sensListeNeu;
    internal void HandleLiveLine(string line)
    {
        if (!line.StartsWith("@@")) return;
        string[] p = line.Substring(2).Split('|');
        switch (p[0])
        {
            case "SENSDEF":
                {
                    // SENSDEF|idx|Gruppe|Geraet|Art|Name|Einheit|Quelle
                    int idx = ToInt(p, 1);
                    string grp = Get(p, 2) + "  \u00B7  " + Get(p, 3);
                    if (!sensGruppen.Contains(grp)) sensGruppen.Add(grp);
                    string art = Get(p, 4);
                    SensorWert w = new SensorWert(); w.Gruppe = grp; w.Name = Get(p, 5); w.Art = art == "Grenzwert" ? "Grenzwert (Hersteller)" : art; w.Quelle = Get(p, 7);
                    // Grenzwerte laut Hersteller sind keine Messwerte: grau und ohne Min/Max-Bewertung
                    if (art == "Grenzwert" || art == "Abstand") { w.Blass = true; w.Hinweis = art == "Grenzwert" ? "Vorgabe des Herstellers, keine gemessene Temperatur" : "Abstand zur Höchsttemperatur TjMax"; }
                    else w.Hinweis = "Quelle: " + Get(p, 7);
                    sensWerte[idx] = w; sensUnit[idx] = Get(p, 6); sensMinMax[idx] = new double[] { double.MaxValue, double.MinValue };
                    while (sensNames.Count <= idx) sensNames.Add("");
                    sensNames[idx] = (Get(p, 2) + " " + Get(p, 3) + " " + Get(p, 5) + " (" + Get(p, 6) + ")").Replace(";", ",");
                    sensListeNeu = true;
                    break;
                }
            case "SENSVAL":
                {
                    // SENSVAL|Sekunde|idx:wert;idx:wert
                    if (sensRows.Count < SensZeilenMax)
                    {
                        sensRows.Add(Get(p, 1) + "|" + Get(p, 2));
                        double sek = ParseD(Get(p, 1));
                        if (!double.IsNaN(sek)) { if (double.IsNaN(sensErsteSek)) sensErsteSek = sek; sensLetzteSek = sek; }
                    }
                    else sensVoll = true;
                    foreach (string kv in Get(p, 2).Split(';'))
                    {
                        int c = kv.IndexOf(':'); if (c <= 0) continue;
                        int idx; if (!int.TryParse(kv.Substring(0, c), out idx)) continue;
                        double v = ParseD(kv.Substring(c + 1));
                        SensorWert w; if (!sensWerte.TryGetValue(idx, out w) || double.IsNaN(v)) continue;
                        double[] mm = sensMinMax[idx]; mm[0] = Math.Min(mm[0], v); mm[1] = Math.Max(mm[1], v);
                        string u = sensUnit[idx];
                        w.Aktuell = FmtSens(v, u); w.Unplausibel = false; w.Min = FmtSens(mm[0], u); w.Max = FmtSens(mm[1], u);
                    }
                    break;
                }
            case "SENSBAD":
                {
                    // SENSBAD|idx|Rohwert|Grund: unplausibler Wert, wird nicht angezeigt und zählt nicht für Min/Max
                    SensorWert w; int idx = ToInt(p, 1);
                    if (!sensWerte.TryGetValue(idx, out w)) break;
                    w.Unplausibel = true; w.Hinweis = "Verworfen: " + Get(p, 3) + " (Rohwert " + Get(p, 2) + ")";
                    foreach (ListenZeile z in lvSens.Zeilen) if (z.Tag == w) z.Hinweis = w.Hinweis;
                    break;
                }
            case "SENSLEAD": SetzeKennzahlen(chartLive.Add(p)); break;
            case "SENSINFO": lblSensInfo.Text = Get(p, 1); break;
            case "SENSSTAT": UpdateSensStatusBadge(Get(p, 1), Get(p, 2)); break;
            case "SENSEND": lblSensInfo.Text = "Beendet. " + Get(p, 1); break;
            case "LOG": { string t = p.Length > 2 ? String.Join("|", p, 2, p.Length - 2).Trim() : ""; if (t.Length > 0) lblSensInfo.Text = t; break; }
        }
    }

    void SaveSensorCsv()
    {
        if (sensRows.Count == 0 || dataDir.Length == 0) return;
        try
        {
            string dir = Path.Combine(Path.Combine(dataDir, "Berichte"), "Sensoren");
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, Environment.MachineName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
            StringBuilder sb = new StringBuilder();
            sb.Append("Sekunde"); foreach (string n in sensNames) sb.Append(';').Append(n); sb.AppendLine();
            foreach (string r in sensRows)
            {
                string[] x = r.Split(new char[] { '|' }, 2);
                string[] cells = new string[sensNames.Count];
                foreach (string kv in (x.Length > 1 ? x[1] : "").Split(';'))
                {
                    int c = kv.IndexOf(':'); int idx;
                    if (c > 0 && int.TryParse(kv.Substring(0, c), out idx) && idx < cells.Length) cells[idx] = kv.Substring(c + 1).Replace('.', ',');
                }
                sb.Append(x[0]); foreach (string c in cells) sb.Append(';').Append(c ?? ""); sb.AppendLine();
            }
            File.WriteAllText(file, sb.ToString(), new UTF8Encoding(true));
            OpenSelect(file);
        }
        catch (Exception ex) { MessageBox.Show(this, "Speichern fehlgeschlagen: " + ex.Message, "Leos Minibench"); }
    }
}
