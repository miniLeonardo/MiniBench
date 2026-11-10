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
        if (lblSensTools == null) return;
        string pv = PawnIoVersion();
        string t;
        if (!LhmPresent) t = "LibreHardwareMonitor fehlt im Tools-Ordner. Ohne die Bibliothek zeigt die Seite nur Ersatzwerte (Windows-Takt, ACPI, nvidia-smi, Datenträger, Akku).";
        else t = "LibreHardwareMonitor liegt im Tools-Ordner (geladen wird nur mit passender Prüfsumme).";
        t += pv.Length > 0 ? "  PawnIO " + pv + " ist auf diesem PC installiert und bleibt unverändert." : (PawnSetupPresent ? "  PawnIO ist nicht installiert; das Setup liegt bereit." : "  PawnIO-Setup fehlt.");
        lblSensTools.Text = t;
        lblSensTools.ForeColor = LhmPresent ? UI.Muted : UI.Warn;
        btnSensTools.Text = LhmPresent && PawnSetupPresent ? "Sensorwerkzeuge erneut holen" : "Sensorwerkzeuge holen";
        UpdateSensStatusBadge();
        StartUpdateCheck();
    }

    Control BuildSensorPage()
    {
        FlowLayoutPanel f = Page("Sensoren live", "Zeigt Temperatur, Takt, Lüfter, Spannung und Leistung laufend an (Kurven der letzten zehn Minuten). CPU-Werte und Mainboard-Lüfter brauchen den Treiber PawnIO; er wird beim Beenden wieder entfernt.", -1);
        lblSensTools = Lbl("", 9f, false, UI.Muted); lblSensTools.MaximumSize = new Size(UI.S(UI.Gruppenbreite), 0); lblSensTools.Margin = new Padding(UI.S(4), 0, UI.S(4), UI.S(8)); f.Controls.Add(lblSensTools);
        FlowLayoutPanel b = Row(); b.Margin = new Padding(UI.S(4), 0, 0, UI.S(8));
        btnLive = UI.Primary("Live-Ansicht starten"); btnLive.Margin = new Padding(0); btnLive.Padding = new Padding(UI.S(14), UI.S(3), UI.S(14), UI.S(3)); btnLive.Font = new Font("Segoe UI Semibold", 9.75f);
        btnLive.Click += delegate { if (liveRunning) StopLive(); else StartLive(); };
        btnSensTools = UI.Secondary("Sensorwerkzeuge holen"); btnSensTools.Margin = new Padding(UI.S(8), 0, 0, 0); btnSensTools.Click += delegate { FetchSensorTools(); };
        btnSensSave = UI.Secondary("Aufzeichnung speichern"); btnSensSave.Margin = new Padding(UI.S(8), 0, 0, 0); btnSensSave.Enabled = false; btnSensSave.Click += delegate { SaveSensorCsv(); };
        b.Controls.Add(btnLive); b.Controls.Add(btnSensTools); b.Controls.Add(btnSensSave);
        f.Controls.Add(b);
        FlowLayoutPanel bd = Row(); bd.Margin = new Padding(UI.S(4), 0, 0, UI.S(8));
        bd.Controls.Add(RowLabel("PawnIO-Treiber", 120));
        cmbDriver = Combo(270, new string[] { "vorübergehend verwenden (Standard)", "nicht verwenden" }, 0); bd.Controls.Add(cmbDriver);
        Label ld = Lbl("gilt für Live-Ansicht, Diagnose, Benchmark und Lasttest; ein vorhandener PawnIO bleibt unverändert", UI.SchriftHinweis, false, UI.Muted); ld.Margin = new Padding(0, UI.S(5), 0, 0); ld.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 420), 0); bd.Controls.Add(ld);
        f.Controls.Add(bd);
        FlowLayoutPanel bk = Row(); bk.Margin = new Padding(UI.S(4), 0, 0, UI.S(8));
        bk.Controls.Add(RowLabel("Hilfswerkzeuge", 120));
        cmbKeep = Combo(270, new string[] { "nach dem Lauf entfernen (Standard)", "auf diesem PC behalten" }, 0); bk.Controls.Add(cmbKeep);
        Label lk = Lbl("PawnIO und smartmontools, die Leos Minibench installiert hat; gilt je Gerät", UI.SchriftHinweis, false, UI.Muted); lk.Margin = new Padding(0, UI.S(5), 0, 0); lk.MaximumSize = new Size(UI.S(UI.Gruppenbreite - 420), 0); bk.Controls.Add(lk);
        f.Controls.Add(bk);
        LoadDeviceTools();
        cmbKeep.SelectedIndexChanged += delegate { SaveDeviceTools(); };
        cmbDriver.SelectedIndexChanged += delegate { SaveDeviceTools(); UpdateSensStatusBadge(); };
        lblSensInfo = Lbl("Live-Ansicht nicht gestartet.", 9f, false, UI.Muted); lblSensInfo.MaximumSize = new Size(UI.S(UI.Gruppenbreite), 0); lblSensInfo.Margin = new Padding(UI.S(4), 0, UI.S(4), UI.S(6)); f.Controls.Add(lblSensInfo);
        chartLive = new SensorChart(); chartLive.Width = UI.S(UI.Gruppenbreite); chartLive.Height = UI.S(330); chartLive.WindowSec = 600; chartLive.Margin = new Padding(UI.S(4), 0, 0, UI.S(8));
        chartLive.Empty = "Noch keine Messwerte. \"Live-Ansicht starten\" öffnet die Sensoren.";
        f.Controls.Add(chartLive);
        lvSens = new ListView(); lvSens.View = View.Details; lvSens.FullRowSelect = true; lvSens.Width = UI.S(UI.Gruppenbreite); lvSens.Height = UI.S(380); lvSens.BorderStyle = BorderStyle.FixedSingle; lvSens.HideSelection = false; lvSens.ShowGroups = true; lvSens.ShowItemToolTips = true; lvSens.Margin = new Padding(UI.S(4), 0, 0, UI.S(4));
        EnableDarkListView(lvSens);
        string[] cols = new string[] { "Sensor", "Art", "Aktuell", "Min", "Max", "Quelle" };
        int[] w = new int[] { UI.S(240), UI.S(110), UI.S(100), UI.S(100), UI.S(100), UI.S(120) };
        for (int i = 0; i < cols.Length; i++) lvSens.Columns.Add(cols[i], w[i], i >= 2 && i <= 4 ? HorizontalAlignment.Right : HorizontalAlignment.Left);
        f.Controls.Add(lvSens);
        Label hint = Lbl("\"Aufzeichnung speichern\" legt alle Werte als CSV unter Berichte\\Sensoren ab. Während eines Laufs ist die Live-Ansicht aus; der Lasttest zeigt seine Kurven im Reiter Sensoren.", 8.75f, false, UI.Muted);
        hint.MaximumSize = new Size(UI.S(UI.Gruppenbreite), 0); hint.Margin = new Padding(UI.S(4), UI.S(4), UI.S(4), 0); f.Controls.Add(hint);
        liveTimer = new Timer(); liveTimer.Interval = 250; liveTimer.Tick += delegate { OnLiveTick(); };
        UpdateSensTools();
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
        sensItems.Clear(); sensMinMax.Clear(); sensUnit.Clear(); sensNames.Clear(); sensRows.Clear(); sensGroups.Clear();
        lvSens.Items.Clear(); lvSens.Groups.Clear(); chartLive.Clear();
        string dummy; while (liveQueue.TryDequeue(out dummy)) { }
        liveExited = false; liveEof = false; liveStopping = false; liveExitSeen = DateTime.MinValue;
        try { liveProc.Start(); liveProc.BeginOutputReadLine(); liveProc.BeginErrorReadLine(); try { liveProc.StandardInput.Close(); } catch { } }
        catch (Exception ex) { MessageBox.Show(this, "PowerShell konnte nicht gestartet werden: " + ex.Message, "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
        liveRunning = true;
        btnLive.Text = "Live-Ansicht beenden"; btnSensTools.Enabled = false; btnSensSave.Enabled = false;
        lblSensInfo.Text = drv ? "Wird gestartet, PawnIO wird vorübergehend installiert ..." : "Wird gestartet ...";
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
            lvSens.BeginUpdate();
            try { while (n < 2000 && liveQueue.TryDequeue(out line)) { n++; HandleLiveLine(line); } }
            finally { lvSens.EndUpdate(); }
        }
        chartLive.Flush();
        bool killed = false;
        if (liveStopping && !liveExited && (DateTime.Now - liveStopAt).TotalSeconds > 240) { KillProc(liveProc); killed = true; }
        if (liveExited && liveExitSeen == DateTime.MinValue) liveExitSeen = DateTime.Now;
        if (liveExited && (liveEof || (DateTime.Now - liveExitSeen).TotalSeconds > 5) && liveQueue.IsEmpty && liveRunning)
        {
            liveRunning = false; liveStopping = false; liveTimer.Stop();
            btnLive.Enabled = true; btnLive.Text = "Live-Ansicht starten"; btnSensTools.Enabled = true; btnSensSave.Enabled = sensRows.Count > 0;
            if (killed || PawnMarkerExists) CleanupDriver();
            UpdateSensTools();
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

    void HandleLiveLine(string line)
    {
        if (!line.StartsWith("@@")) return;
        string[] p = line.Substring(2).Split('|');
        switch (p[0])
        {
            case "SENSDEF":
                {
                    // SENSDEF|idx|Gruppe|Geraet|Art|Name|Einheit|Quelle
                    int idx = ToInt(p, 1);
                    string grp = Get(p, 2) + "  ·  " + Get(p, 3);
                    ListViewGroup lg;
                    if (!sensGroups.TryGetValue(grp, out lg)) { lg = new ListViewGroup(grp, grp); sensGroups[grp] = lg; lvSens.Groups.Add(lg); }
                    string art = Get(p, 4);
                    ListViewItem it = new ListViewItem(new string[] { Get(p, 5), art == "Grenzwert" ? "Grenzwert (Hersteller)" : art, "", "", "", Get(p, 7) }, lg);
                    // Grenzwerte laut Hersteller sind keine Messwerte: grau und ohne Min/Max-Bewertung
                    if (art == "Grenzwert" || art == "Abstand") { it.ForeColor = UI.Muted; it.ToolTipText = art == "Grenzwert" ? "Vorgabe des Herstellers, keine gemessene Temperatur" : "Abstand zur Höchsttemperatur TjMax"; }
                    else it.ToolTipText = "Quelle: " + Get(p, 7);
                    sensItems[idx] = it; sensUnit[idx] = Get(p, 6); sensMinMax[idx] = new double[] { double.MaxValue, double.MinValue };
                    while (sensNames.Count <= idx) sensNames.Add("");
                    sensNames[idx] = (Get(p, 2) + " " + Get(p, 3) + " " + Get(p, 5) + " (" + Get(p, 6) + ")").Replace(";", ",");
                    lvSens.Items.Add(it);
                    break;
                }
            case "SENSVAL":
                {
                    // SENSVAL|Sekunde|idx:wert;idx:wert
                    if (sensRows.Count < 50000) sensRows.Add(Get(p, 1) + "|" + Get(p, 2));
                    foreach (string kv in Get(p, 2).Split(';'))
                    {
                        int c = kv.IndexOf(':'); if (c <= 0) continue;
                        int idx; if (!int.TryParse(kv.Substring(0, c), out idx)) continue;
                        double v = ParseD(kv.Substring(c + 1));
                        ListViewItem it; if (!sensItems.TryGetValue(idx, out it) || double.IsNaN(v)) continue;
                        double[] mm = sensMinMax[idx]; mm[0] = Math.Min(mm[0], v); mm[1] = Math.Max(mm[1], v);
                        string u = sensUnit[idx];
                        string a = FmtSens(v, u); if (it.SubItems[2].Text != a) { it.SubItems[2].Text = a; if (!it.UseItemStyleForSubItems) it.SubItems[2].ForeColor = it.ForeColor; }
                        string mi = FmtSens(mm[0], u); if (it.SubItems[3].Text != mi) it.SubItems[3].Text = mi;
                        string ma = FmtSens(mm[1], u); if (it.SubItems[4].Text != ma) it.SubItems[4].Text = ma;
                    }
                    break;
                }
            case "SENSBAD":
                {
                    // SENSBAD|idx|Rohwert|Grund: unplausibler Wert, wird nicht angezeigt und zählt nicht für Min/Max
                    ListViewItem it; int idx = ToInt(p, 1);
                    if (!sensItems.TryGetValue(idx, out it)) break;
                    if (it.SubItems[2].Text != "unplausibel") { it.SubItems[2].Text = "unplausibel"; it.UseItemStyleForSubItems = false; it.SubItems[2].ForeColor = UI.Crit; }
                    string tip = "Verworfen: " + Get(p, 3) + " (Rohwert " + Get(p, 2) + ")";
                    if (it.ToolTipText != tip) it.ToolTipText = tip;
                    break;
                }
            case "SENSLEAD": chartLive.Add(p); break;
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
