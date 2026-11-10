// Vorlagen (gespeicherte Auswahl) (Teilklasse DiagGui)
public partial class DiagGui
{
    // ------------------------------------------------------------ Voreinstellungen (ab v2.65)
    // Auswahl der Module, Prüfungen, Messungen, Lasttest und Reparaturen als benannte Voreinstellung im Datenordner
    // (Voreinstellungen.json, Format Minibench-Voreinstellungen/1). Eine davon kann beim Start automatisch geladen werden:
    // Wer den Stick weitergibt, legt sie fest, die andere Person klickt nur noch Start.
    ComboBox cmbPreset;
    PillButton btnPresetSave; Button btnPresetDel;
    bool presetFilling, presetLoading;
    string presetActive = "";
    string PresetFile { get { return dataDir.Length > 0 ? Path.Combine(dataDir, "Voreinstellungen.json") : ""; } }
    const string PresetFormat = "Minibench-Voreinstellungen/1";
    static readonly string[] diagProfiles = new string[] { "Voll", "Schnell", "Benutzerdefiniert" };

    Dictionary<string, object> ReadPresetFile()
    {
        try
        {
            if (PresetFile.Length == 0 || !File.Exists(PresetFile)) return null;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(PresetFile, Encoding.UTF8)) as Dictionary<string, object>;
            object fm; if (d == null || !d.TryGetValue("Format", out fm) || Convert.ToString(fm) != PresetFormat) return null;
            return d;
        }
        catch { return null; }
    }

    static Dictionary<string, object> SubDict(Dictionary<string, object> d, string key)
    {
        object o; if (d == null || !d.TryGetValue(key, out o)) return null;
        return o as Dictionary<string, object>;
    }

    void FillPresets(bool autoLoad)
    {
        cachedLeoIds = null;
        presetFilling = true;
        try
        {
            if (pnlCustomPresets != null) pnlCustomPresets.Controls.Clear();
            if (cmbPreset != null) { cmbPreset.Items.Clear(); cmbPreset.Items.Add("(keine)"); }
            Dictionary<string, object> d = ReadPresetFile();
            Dictionary<string, object> e = SubDict(d, "Eintraege");
            object st; string start = d != null && d.TryGetValue("BeimStart", out st) ? Convert.ToString(st) : "";
            List<string> names = new List<string>();
            if (e != null) names.AddRange(e.Keys);
            names.Sort(StringComparer.CurrentCultureIgnoreCase);
            foreach (string n in names)
            {
                if (cmbPreset != null) cmbPreset.Items.Add(n == start ? n + "   (beim Start)" : n);
                if (pnlCustomPresets != null)
                {
                    string pName = n;
                    PillButton pill = new PillButton(n, presetActive == n);
                    pill.Removable = true;
                    pill.Picked += delegate {
                        pillPresetMin.Active = false; pillPresetLeo.Active = false; pillPresetAdv.Active = false; pillPresetReset.Active = false;
                        foreach (Control c in pnlCustomPresets.Controls) { PillButton pb = c as PillButton; if (pb != null) pb.Active = (pb == pill); }
                        ApplyPreset(pName);
                    };
                    pill.RemoveClicked += delegate { DeletePresetByName(pName); };
                    pnlCustomPresets.Controls.Add(pill);
                }
            }
            if (autoLoad && start.Length > 0)
            {
                int i = names.IndexOf(start);
                if (i >= 0)
                {
                    ApplyPreset(start);
                    if (pnlCustomPresets != null)
                    {
                        foreach (Control c in pnlCustomPresets.Controls) { PillButton pb = c as PillButton; if (pb != null) pb.Active = (pb.Text == start); }
                    }
                    if (!LoadStartPagePreference())
                    {
                        int targetPage = 1;
                        for (int m = 1; m <= 5 && m < nav.Count; m++)
                        {
                            if (nav[m].Checked) { targetPage = m; break; }
                        }
                        ShowPage(targetPage);
                    }
                }
            }
            if (btnPresetSave != null) btnPresetSave.Enabled = PresetFile.Length > 0;
        }
        finally { presetFilling = false; }
    }

    void ApplyGlobalPreset(string key)
    {
        pillPresetMin.Active = (key == "M");
        pillPresetLeo.Active = (key == "S");
        pillPresetAdv.Active = (key == "E");
        pillPresetReset.Active = (key == "");
        if (pnlCustomPresets != null)
        {
            foreach (Control c in pnlCustomPresets.Controls) { PillButton pb = c as PillButton; if (pb != null) pb.Active = false; }
        }
        presetActive = "";
        if (key == "")
        {
            for (int i = 1; i <= 5 && i < nav.Count; i++) nav[i].Checked = false;
            foreach (CheckBox c in repChk) c.Checked = false;
            foreach (OptItem it in optItems) if (it.Box != null) it.Box.Checked = false;
        }
        else if (key == "M")
        {
            nav[1].Checked = true; rbSchnell.Checked = true;
            nav[2].Checked = true;
            nav[3].Checked = false;
            nav[4].Checked = false;
            nav[5].Checked = false;
            ApplyOptPreset("M");
        }
        else if (key == "S")
        {
            nav[1].Checked = true; rbVoll.Checked = true;
            nav[2].Checked = true;
            nav[3].Checked = false;
            nav[4].Checked = true;
            for (int i = 0; i < repChk.Length; i++) repChk[i].Checked = repUsual[i];
            nav[5].Checked = true;
            ApplyOptPreset("S");
        }
        else if (key == "E")
        {
            nav[1].Checked = true; rbVoll.Checked = true;
            nav[2].Checked = true;
            nav[3].Checked = true;
            nav[4].Checked = true;
            for (int i = 0; i < repChk.Length; i++) repChk[i].Checked = true;
            nav[5].Checked = true;
            ApplyOptPreset("E");
        }
        UpdateRepairCounts();
        UpdateOptCounts();
        UpdateSummary();
    }

    void DeletePresetByName(string n)
    {
        if (String.IsNullOrEmpty(n)) return;
        if (MessageBox.Show(this, "Vorlage \"" + n + "\" l\u00F6schen?", "Leos Minibench", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            Dictionary<string, object> d = ReadPresetFile(); if (d == null) return;
            Dictionary<string, object> e = SubDict(d, "Eintraege"); if (e != null) e.Remove(n);
            object st; if (d.TryGetValue("BeimStart", out st) && Convert.ToString(st) == n) d["BeimStart"] = "";
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            File.WriteAllText(PresetFile, js.Serialize(d), new UTF8Encoding(false));
            if (presetActive == n) presetActive = "";
            FillPresets(false); UpdateSummary();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Leos Minibench"); }
    }

    string PresetNameOf(int index)
    {
        if (index <= 0 || index >= cmbPreset.Items.Count) return "";
        string t = Convert.ToString(cmbPreset.Items[index]);
        int k = t.IndexOf("   (beim Start)"); return k >= 0 ? t.Substring(0, k) : t;
    }

    void PresetPicked()
    {
        btnPresetDel.Enabled = cmbPreset.SelectedIndex > 0;
        string n = PresetNameOf(cmbPreset.SelectedIndex);
        if (n.Length == 0) { presetActive = ""; UpdateSummary(); return; }
        ApplyPreset(n);
    }

    void ApplyPreset(string name)
    {
        Dictionary<string, object> e = SubDict(SubDict(ReadPresetFile(), "Eintraege"), name);
        Dictionary<string, object> w = SubDict(e, "Werte");
        if (w == null) return;
        presetLoading = true;
        try { ApplySettings(w); presetActive = name; }
        catch { }
        finally { presetLoading = false; }
        UpdateSummary();
    }

    void SavePresetDialog()
    {
        if (PresetFile.Length == 0) { MessageBox.Show(this, "Kein Datenordner verfügbar.", "Leos Minibench"); return; }
        Dictionary<string, object> d = ReadPresetFile();
        object st; string start = d != null && d.TryGetValue("BeimStart", out st) ? Convert.ToString(st) : "";
        using (Form f = new Form())
        {
            f.Text = "Als Vorlage speichern"; f.FormBorderStyle = FormBorderStyle.FixedDialog; f.MaximizeBox = false; f.MinimizeBox = false; f.ShowInTaskbar = false;
            f.StartPosition = FormStartPosition.CenterParent; f.Font = new Font("Segoe UI", 9.5f); f.BackColor = UI.Bg; f.AutoSize = true; f.AutoSizeMode = AutoSizeMode.GrowAndShrink; f.Padding = new Padding(UI.S(16));
            FlowLayoutPanel p = new FlowLayoutPanel(); p.FlowDirection = FlowDirection.TopDown; p.AutoSize = true; p.WrapContents = false; p.Dock = DockStyle.Fill;
            Label l = new Label(); l.AutoSize = true; l.MaximumSize = new Size(UI.S(460), 0); l.Margin = new Padding(0, 0, 0, UI.S(8));
            l.Text = "Speichert die Auswahl aller Seiten (Module, Prüfungen, Messungen, Lasttest, Reparaturen, Sensortreiber) im Datenordner. Laufwerke und Vergleichssysteme bleiben je PC frei.";
            p.Controls.Add(l);
            TextBox tb = new TextBox(); tb.Width = UI.S(320); tb.Text = presetActive.Length > 0 ? presetActive : "Standardprüfung"; p.Controls.Add(tb);
            CheckBox cs = new CheckBox(); cs.AutoSize = true; cs.Margin = new Padding(UI.S(3), UI.S(10), UI.S(3), UI.S(3)); cs.Text = "Beim Start automatisch laden (dann genügt ein Klick auf Start)"; cs.Checked = start.Length == 0 || start == tb.Text; p.Controls.Add(cs);
            FlowLayoutPanel b = new FlowLayoutPanel(); b.AutoSize = true; b.Margin = new Padding(0, UI.S(12), 0, 0);
            Button ok = UI.Primary("Speichern"); ok.DialogResult = DialogResult.OK; Button ab = UI.Secondary("Abbrechen"); ab.DialogResult = DialogResult.Cancel;
            b.Controls.Add(ok); b.Controls.Add(ab); p.Controls.Add(b);
            f.Controls.Add(p); f.AcceptButton = ok; f.CancelButton = ab;
            UI.ThemeDialog(f);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            string name = tb.Text.Trim().Replace("   (beim Start)", "");
            if (name.Length == 0) return;
            try
            {
                if (d == null) d = new Dictionary<string, object>();
                d["Format"] = PresetFormat;
                Dictionary<string, object> e = SubDict(d, "Eintraege");
                if (e == null) { e = new Dictionary<string, object>(); d["Eintraege"] = e; }
                Dictionary<string, object> entry = new Dictionary<string, object>();
                entry["Gespeichert"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                entry["Version"] = version; entry["Computer"] = Environment.MachineName;
                entry["Werte"] = CaptureSettings();
                e[name] = entry;
                if (cs.Checked) d["BeimStart"] = name; else if (start == name) d["BeimStart"] = "";
                Directory.CreateDirectory(dataDir);
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                File.WriteAllText(PresetFile, js.Serialize(d), new UTF8Encoding(false));
                presetActive = name;
                FillPresets(false);
                UpdateSummary();
            }
            catch (Exception ex) { MessageBox.Show(this, "Vorlage nicht gespeichert: " + ex.Message, "Leos Minibench"); }
        }
    }

    void DeletePreset()
    {
        string n = PresetNameOf(cmbPreset.SelectedIndex);
        if (n.Length == 0) return;
        if (MessageBox.Show(this, "Vorlage \"" + n + "\" l\u00F6schen?", "Leos Minibench", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            Dictionary<string, object> d = ReadPresetFile(); if (d == null) return;
            Dictionary<string, object> e = SubDict(d, "Eintraege"); if (e != null) e.Remove(n);
            object st; if (d.TryGetValue("BeimStart", out st) && Convert.ToString(st) == n) d["BeimStart"] = "";
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            File.WriteAllText(PresetFile, js.Serialize(d), new UTF8Encoding(false));
            if (presetActive == n) presetActive = "";
            FillPresets(false); UpdateSummary();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Leos Minibench"); }
    }

    static void PutC(Dictionary<string, object> d, string k, CheckBox c) { if (c != null) d[k] = c.Checked; }
    static void PutCb(Dictionary<string, object> d, string k, ComboBox c) { if (c != null && c.SelectedItem != null) d[k] = Convert.ToString(c.SelectedItem); }
    static bool GetB(Dictionary<string, object> d, string k, out bool v)
    {
        v = false; object o; if (d == null || !d.TryGetValue(k, out o) || o == null) return false;
        if (o is bool) { v = (bool)o; return true; }
        bool b; if (bool.TryParse(Convert.ToString(o), out b)) { v = b; return true; }
        return false;
    }
    // gleiche Wahl trotz geänderter Beschriftung: gleiche Zahl am Anfang oder alter Text beginnt mit dem neuen
    public static bool SameChoice(string item, string old)
    {
        if (String.IsNullOrEmpty(item) || String.IsNullOrEmpty(old)) return false;
        if (old.StartsWith(item, StringComparison.OrdinalIgnoreCase)) return true;
        string a = item.Split(' ')[0], b = old.Split(' ')[0]; int x, y;
        return int.TryParse(a, out x) && int.TryParse(b, out y) && x == y && item.Length > a.Length && old.Length > b.Length && Char.ToLowerInvariant(item[a.Length + 1]) == Char.ToLowerInvariant(old[b.Length + 1]);
    }

    static string GetS(Dictionary<string, object> d, string k) { object o; return d != null && d.TryGetValue(k, out o) && o != null ? Convert.ToString(o) : null; }
    static void GetC(Dictionary<string, object> d, string k, CheckBox c) { bool v; if (c != null && GetB(d, k, out v)) c.Checked = v; }
    static void GetCb(Dictionary<string, object> d, string k, ComboBox c)
    {
        string v = GetS(d, k); if (c == null || v == null) return;
        int i = c.Items.IndexOf(v);
        if (i < 0) for (int j = 0; j < c.Items.Count; j++) if (Convert.ToString(c.Items[j]).StartsWith(v, StringComparison.OrdinalIgnoreCase)) { i = j; break; }
        // Voreinstellungen vor v3.64: längere Texte ("2 Minuten", "automatisch (TjMax, sonst 100 °C)")
        if (i < 0) for (int j = 0; j < c.Items.Count; j++) if (SameChoice(Convert.ToString(c.Items[j]), v)) { i = j; break; }
        if (i >= 0) c.SelectedIndex = i;
    }

    // Alle Einstellungen der Seiten als Schlüssel und Wert (Texte der Auswahllisten, damit Änderungen der Reihenfolge nicht stören)
    public Dictionary<string, object> CaptureSettings()
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        string[] mods = new string[] { "Diagnose", "Benchmark", "Lasttest", "Reparatur", "Optimierung" };
        for (int i = 0; i < mods.Length && (i + 1) < nav.Count; i++) d["Modul." + mods[i]] = nav[i + 1].Checked;
        RadioButton[] rbs = new RadioButton[] { rbVoll, rbSchnell, rbCustom };
        for (int i = 0; i < rbs.Length; i++) if (rbs[i] != null && rbs[i].Checked) d["Diag.Profil"] = diagProfiles[i];
        for (int i = 0; i < diagKeys.Length && diagChk != null; i++) d["Diag." + diagKeys[i]] = diagChk[i].Checked;
        PutC(d, "Diag.SchnellerModus", chkFast); PutC(d, "Diag.Smartmontools", chkInstall); PutC(d, "Diag.Speicherdiagnose", chkMem);
        PutCb(d, "Diag.Tage", cmbDays); PutCb(d, "Diag.SmartMax", cmbSmartMax);
        for (int i = 0; i < benchKeys.Length && benchChk != null; i++) d["Bench." + benchKeys[i]] = benchChk[i].Checked;
        if (cmbRef != null)
        {
            if (cmbRef.SelectedIndex == 1) d["Bench.Referenz"] = "Median";
            else if (cmbRef.SelectedIndex == 2) d["Bench.Referenz"] = "Median:Notebook";
            else if (cmbRef.SelectedIndex == 3) d["Bench.Referenz"] = "Median:Desktop";
            else if (cmbRef.SelectedIndex >= 4 && cmbRef.SelectedIndex - 4 < refItems.Count) d["Bench.Referenz"] = "Datei:" + refItems[cmbRef.SelectedIndex - 4].Path;
            else d["Bench.Referenz"] = "Gespeichert";
        }
        PutCb(d, "Gpu.Aufloesung", cmbGpuRes); PutCb(d, "Gpu.Anzeige", cmbGpuShow); d["Gpu.Auswahl"] = GpuChoiceValue(cmbGpuSel);
        PutC(d, "Last.CPU", chkLCpu); PutCb(d, "Last.CPUDauer", cmbLCpu);
        PutC(d, "Last.RAM", chkLRam); PutCb(d, "Last.RAMDauer", cmbLRam); PutCb(d, "Last.RAMAnteil", cmbLRamPct);
        PutC(d, "Last.GPU", chkLGpu); PutCb(d, "Last.GPUDauer", cmbLGpu);
        PutC(d, "Last.Disk", chkLDisk); PutCb(d, "Last.DiskDauer", cmbLDisk);
        if (cmbLDiskDrive != null && cmbLDiskDrive.SelectedItem != null) d["Last.Laufwerk"] = Convert.ToString(cmbLDiskDrive.SelectedItem).Substring(0, Math.Min(2, Convert.ToString(cmbLDiskDrive.SelectedItem).Length));
        PutCb(d, "Last.AbbruchCpu", cmbLAbortCpu); PutCb(d, "Last.AbbruchGpu", cmbLAbortGpu);
        for (int i = 0; i < repKeys.Length && repChk != null; i++) d["Rep." + repKeys[i]] = repChk[i].Checked;
        PutC(d, "Rep.Wiederherstellungspunkt", chkRestorePoint);
        d["Opt.Auswahl"] = String.Join(",", ChosenOptIds().ToArray());
        PutC(d, "Opt.Wiederherstellungspunkt", chkOptRestore);
        PutC(d, "Allg.Anonymisieren", chkAnon);
        if (cmbDriver != null) d["Sensor.Treiber"] = driverValues[Math.Max(0, cmbDriver.SelectedIndex)];
        if (cmbKeep != null) d["Sensor.Werkzeuge"] = keepValues[Math.Max(0, cmbKeep.SelectedIndex)];
        return d;
    }

    public void ApplySettings(Dictionary<string, object> d)
    {
        string[] mods = new string[] { "Diagnose", "Benchmark", "Lasttest", "Reparatur", "Optimierung" };
        bool v;
        for (int i = 0; i < mods.Length && (i + 1) < nav.Count; i++) if (GetB(d, "Modul." + mods[i], out v)) nav[i + 1].Checked = v;
        string prof = GetS(d, "Diag.Profil");
        RadioButton[] rbs = new RadioButton[] { rbVoll, rbSchnell, rbCustom };
        // Voreinstellungen vor v3.64 mit Funktionstest oder Absturzanalyse: Profil Schnell, ohne die dort abgeschalteten
        // Prüfungen und Optionen zu übernehmen (sonst liefe etwa die Installation von smartmontools unerwartet mit)
        bool alt = false;
        if (prof != null) { int i = Array.IndexOf(diagProfiles, prof); if (i < 0) { i = 1; alt = true; } if (rbs[i] != null) rbs[i].Checked = true; }
        if (!alt)
        {
            for (int i = 0; i < diagKeys.Length && diagChk != null; i++) GetC(d, "Diag." + diagKeys[i], diagChk[i]);
            GetC(d, "Diag.Smartmontools", chkInstall); GetC(d, "Diag.Speicherdiagnose", chkMem);
        }
        GetC(d, "Diag.SchnellerModus", chkFast);
        GetCb(d, "Diag.Tage", cmbDays); GetCb(d, "Diag.SmartMax", cmbSmartMax);
        for (int i = 0; i < benchKeys.Length && benchChk != null; i++) GetC(d, "Bench." + benchKeys[i], benchChk[i]);
        if (cmbRef != null)
        {
            string r = GetS(d, "Bench.Referenz");
            int ri = 0;
            if (r == "Median" || r == "*median") ri = 1;
            else if (r == "Median:Notebook" || r == "*median:notebook") ri = 2;
            else if (r == "Median:Desktop" || r == "*median:desktop") ri = 3;
            else if (r != null && r.StartsWith("Datei:")) {
                int k = refItems.FindIndex(delegate(DbEntry x) { return String.Equals(x.Path, r.Substring(6), StringComparison.OrdinalIgnoreCase); });
                ri = k >= 0 ? k + 4 : 0;
            }
            if (ri >= 0 && ri < cmbRef.Items.Count) cmbRef.SelectedIndex = ri;
        }
        GetCb(d, "Gpu.Aufloesung", cmbGpuRes); GetCb(d, "Gpu.Anzeige", cmbGpuShow);
        string gs = GetS(d, "Gpu.Auswahl"); if (gs != null) SetGpuChoice(gs);
        GetC(d, "Last.CPU", chkLCpu); GetCb(d, "Last.CPUDauer", cmbLCpu);
        GetC(d, "Last.RAM", chkLRam); GetCb(d, "Last.RAMDauer", cmbLRam); GetCb(d, "Last.RAMAnteil", cmbLRamPct);
        GetC(d, "Last.GPU", chkLGpu); GetCb(d, "Last.GPUDauer", cmbLGpu);
        GetC(d, "Last.Disk", chkLDisk); GetCb(d, "Last.DiskDauer", cmbLDisk); GetCb(d, "Last.Laufwerk", cmbLDiskDrive);
        GetCb(d, "Last.AbbruchCpu", cmbLAbortCpu); GetCb(d, "Last.AbbruchGpu", cmbLAbortGpu);
        for (int i = 0; i < repKeys.Length && repChk != null; i++) GetC(d, "Rep." + repKeys[i], repChk[i]);
        UpdateRepairCounts(); UpdateRepairGroupVisibility();
        GetC(d, "Rep.Wiederherstellungspunkt", chkRestorePoint);
        string os = GetS(d, "Opt.Auswahl");
        if (os != null) { List<string> ids = new List<string>(os.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)); foreach (OptItem it in optItems) it.Box.Checked = ids.Contains(it.Id); UpdateOptCounts(); }
        GetC(d, "Opt.Wiederherstellungspunkt", chkOptRestore);
        GetC(d, "Allg.Anonymisieren", chkAnon);
        // Sensortreiber und Hilfswerkzeuge gelten für diesen Lauf, gespeichert je Gerät wird hier nichts
        string tr = GetS(d, "Sensor.Treiber"); if (tr != null && cmbDriver != null) { int i = Array.IndexOf(driverValues, tr); if (i >= 0) cmbDriver.SelectedIndex = i; }
        string kp = GetS(d, "Sensor.Werkzeuge"); if (kp != null && cmbKeep != null) { int i = Array.IndexOf(keepValues, kp); if (i >= 0) cmbKeep.SelectedIndex = i; }
        UpdateLoadEnabled();
    }
}
