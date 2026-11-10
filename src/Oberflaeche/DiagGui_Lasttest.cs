// Seite Lasttest (Teilklasse DiagGui)
public partial class DiagGui
{
    // ab v3.64 Lasttest als Tabelle: Komponente, Dauer, Einstellungen; Hinweise stehen bei ihrem Feld statt am Seitenende
    Dictionary<CheckBox, List<Control>> loadParts = new Dictionary<CheckBox, List<Control>>();
    Label lblLoadTotal, lblAkku;
    Badge badgePawn;

    static Label CellLabel(string text, int width)
    {
        Label l = RowLabel(text, width); l.Font = new Font("Segoe UI", UI.SchriftText); return l;
    }

    Control BuildLoadPage()
    {
        FlowLayoutPanel f = Page("Lasttest", "Belastet die gewählten Komponenten gleichzeitig, jede mit eigener Dauer, und zeichnet Temperatur, Takt, Leistung und Lüfter auf. Gemeldet werden Rechen-, Bit- und Datenfehler, WHEA-Fehler und Drosselung. Während des Tests lässt er sich jederzeit beenden.", 3);
        string[] mins = new string[] { "2 Min.", "5 Min.", "10 Min.", "15 Min.", "30 Min.", "60 Min.", "120 Min.", "240 Min." };
        lblAkku = Lbl("  Akkubetrieb: Für den Lasttest das Netzteil anschließen, sonst begrenzt die Energieverwaltung Takt und Leistung.  ", UI.SchriftText, true, UI.Warn);
        lblAkku.BackColor = UI.WarnBg; lblAkku.Padding = new Padding(UI.S(6)); lblAkku.Margin = new Padding(0, UI.S(4), 0, UI.S(4)); lblAkku.Visible = false; f.Controls.Add(lblAkku);
        f.Controls.Add(Section("Komponenten und Dauer"));
        TableLayoutPanel t = LoadTable();
        t.Controls.Add(CellLabel("Komponente", 230), 0, 0); t.Controls.Add(CellLabel("Dauer", 120), 1, 0); t.Controls.Add(CellLabel("Einstellungen", 150), 2, 0);
        foreach (Control c in t.Controls) { c.ForeColor = UI.Muted; c.Font = new Font("Segoe UI Semibold", UI.SchriftHinweis); }

        chkLCpu = Chk("Prozessor", true); cmbLCpu = Combo(110, mins, 0);
        AddLoadRow(t, 1, chkLCpu, "alle Threads mit Ergebnisprüfung", cmbLCpu, null);

        chkLRam = Chk("Arbeitsspeicher", true); cmbLRam = Combo(110, mins, 0);
        cmbLRamPct = Combo(90, new string[] { "25 %", "40 %", "60 %", "75 %" }, 1);
        Label lPct = CellLabel("Anteil am freien RAM", 150);
        AddLoadRow(t, 2, chkLRam, "Mustertest mit Bitfehlerprüfung", cmbLRam, new Control[] { SettingRow(lPct, cmbLRamPct) });

        chkLGpu = Chk("Grafik", true); cmbLGpu = Combo(110, mins, 0);
        cmbLGpuSel = Combo(260, GpuChoiceTexts(), 0);
        cmbLGpuRes = Combo(120, new string[] { "1280x720", "1920x1080" }, 0);
        cmbLGpuShow = Combo(140, new string[] { "Fenster", "Vollbild", "ohne Anzeige" }, 0);
        Label lSel = CellLabel("Grafikeinheit", 100), lRes = CellLabel("Auflösung", 100), lShow = CellLabel("Anzeige", 100);
        AddLoadRow(t, 3, chkLGpu, "Rendertest mit Direct3D 11, Bildprüfung alle 30 s, Erkennung von Treiber-Resets", cmbLGpu, new Control[] { SettingRow(lSel, cmbLGpuSel), SettingRow(lRes, cmbLGpuRes), SettingRow(lShow, cmbLGpuShow) });
        // Einstellungen des Rendertests auf beiden Seiten gleich halten: Benchmark -> Lasttest und Lasttest -> Benchmark
        // (ab 3.53 über CopySelection; vorher setzte sync2 die Grafikeinheit des Lasttests auf den Wert des Benchmarks zurück)
        EventHandler sync1 = delegate { if (syncGpu || cmbGpuRes == null) return; syncGpu = true; try { CopySelection(GpuCombosBench(), GpuCombosLast()); } finally { syncGpu = false; } };
        EventHandler sync2 = delegate { if (syncGpu || cmbGpuRes == null) return; syncGpu = true; try { CopySelection(GpuCombosLast(), GpuCombosBench()); } finally { syncGpu = false; } };
        if (cmbGpuRes != null) { cmbGpuRes.SelectedIndexChanged += sync1; cmbGpuShow.SelectedIndexChanged += sync1; }
        if (cmbGpuSel != null) cmbGpuSel.SelectedIndexChanged += sync1;
        cmbLGpuRes.SelectedIndexChanged += sync2; cmbLGpuShow.SelectedIndexChanged += sync2; cmbLGpuSel.SelectedIndexChanged += sync2;

        chkLDisk = Chk("Datenträger", false); cmbLDisk = Combo(110, mins, 0);
        List<string> letters = new List<string>();
        foreach (string d in disks)
        {
            string[] x = d.Split('|'); if (x.Length < 7 || x[5].Length == 0) continue;
            foreach (string l in x[5].Split(',')) { string tl = l.Trim(); if (tl.Length > 0) letters.Add(tl + "  " + ShortDiskName(x[1]) + (x[4].Length > 0 ? " (" + x[4] + ")" : "")); }
        }
        if (letters.Count == 0) letters.Add("C:");
        letters.Sort();
        cmbLDiskDrive = Combo(260, letters.ToArray(), 0);
        Label lDrv = CellLabel("Laufwerk", 100);
        AddLoadRow(t, 4, chkLDisk, "Schreiben, Lesen und Prüfen jedes Blocks; Testdatei bis 4 GB, wird danach gelöscht", cmbLDisk, new Control[] { SettingRow(lDrv, cmbLDiskDrive) });
        f.Controls.Add(Card(t));
        lblLoadTotal = Lbl("", UI.SchriftText, true, UI.Text); lblLoadTotal.Margin = new Padding(UI.S(4), UI.S(UI.Abstand2), 0, 0); f.Controls.Add(lblLoadTotal);

        foreach (CheckBox c in new CheckBox[] { chkLCpu, chkLRam, chkLGpu, chkLDisk }) c.CheckedChanged += delegate { UpdateLoadEnabled(); UpdateSummary(); };
        foreach (ComboBox c in new ComboBox[] { cmbLCpu, cmbLRam, cmbLGpu, cmbLDisk }) c.SelectedIndexChanged += delegate { UpdateSummary(); };

        f.Controls.Add(Section("Abbruchschwelle"));
        TableLayoutPanel ta = LoadTable();
        cmbLAbortCpu = Combo(140, new string[] { "Automatisch", "85 °C", "90 °C", "95 °C", "100 °C", "aus" }, 0);
        cmbLAbortGpu = Combo(140, new string[] { "85 °C", "90 °C", "95 °C", "aus" }, 1);
        badgePawn = new Badge("PawnIO vorübergehend", "info");
        Tip(badgePawn, "Echte CPU-Temperaturen liefert LibreHardwareMonitor mit dem Treiber PawnIO. Er wird für den Lauf vorübergehend installiert und danach wieder entfernt (einstellbar auf der Seite Sensoren live).");
        ta.Controls.Add(CellLabel("CPU-Temperatur", 230), 0, 0); ta.Controls.Add(cmbLAbortCpu, 1, 0); ta.Controls.Add(badgePawn, 2, 0);
        ta.Controls.Add(CellLabel("GPU-Temperatur", 230), 0, 1); ta.Controls.Add(cmbLAbortGpu, 1, 1);
        f.Controls.Add(Card(ta));
        f.Controls.Add(Hint("Liegt die Temperatur rund 10 Sekunden auf oder über der Schwelle, endet der Test mit einem Befund. Automatisch: TjMax der CPU, sonst 100 °C.", 4));
        UpdateLoadEnabled();
        return f;
    }

    static TableLayoutPanel LoadTable()
    {
        TableLayoutPanel t = new TableLayoutPanel(); t.AutoSize = true; t.AutoSizeMode = AutoSizeMode.GrowAndShrink; t.ColumnCount = 3; t.BackColor = UI.Panel;
        t.Margin = new Padding(0, 0, 0, 0);
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(240))); t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(130))); t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        return t;
    }

    static Control SettingRow(Label l, Control c)
    {
        FlowLayoutPanel r = new FlowLayoutPanel(); r.AutoSize = true; r.WrapContents = false; r.BackColor = UI.Panel; r.Margin = new Padding(0, 0, 0, UI.S(2));
        r.Controls.Add(l); r.Controls.Add(c); return r;
    }

    void AddLoadRow(TableLayoutPanel t, int row, CheckBox chk, string detail, ComboBox dur, Control[] settings)
    {
        Control comp = Opt2(chk, detail);
        Label d = comp.Controls.Count > 1 ? comp.Controls[1] as Label : null;
        if (d != null) d.MaximumSize = new Size(UI.S(216), 0);
        comp.Margin = new Padding(0, UI.S(4), UI.S(4), UI.S(6));
        dur.Margin = new Padding(0, UI.S(6), 0, 0);
        t.Controls.Add(comp, 0, row); t.Controls.Add(dur, 1, row);
        List<Control> parts = new List<Control>(); parts.Add(dur);
        if (settings != null && settings.Length > 0)
        {
            FlowLayoutPanel s = new FlowLayoutPanel(); s.FlowDirection = FlowDirection.TopDown; s.WrapContents = false; s.AutoSize = true; s.BackColor = UI.Panel; s.Margin = new Padding(0, UI.S(4), 0, UI.S(4));
            foreach (Control c in settings) { s.Controls.Add(c); foreach (Control k in c.Controls) parts.Add(k); }
            t.Controls.Add(s, 2, row);
        }
        loadParts[chk] = parts;
    }

    // Elemente einer abgewählten Komponente einheitlich deaktivieren: Auswahllisten aus, Beschriftungen gedämpft
    public static void SetPartEnabled(IEnumerable<Control> parts, bool on)
    {
        if (parts == null) return;
        foreach (Control c in parts)
        {
            if (c is Label) c.ForeColor = on ? UI.Text : DisabledText();
            else c.Enabled = on;
        }
    }

    void UpdateLoadEnabled()
    {
        foreach (KeyValuePair<CheckBox, List<Control>> kv in loadParts) SetPartEnabled(kv.Value, kv.Key.Checked);
        UpdateLoadTotal();
    }

    void UpdateLoadTotal()
    {
        if (lblLoadTotal == null || chkLCpu == null) return;
        int m = LoadTotalMinutes(new bool[] { chkLCpu.Checked, chkLRam.Checked, chkLGpu.Checked, chkLDisk.Checked },
            new int[] { Minutes(cmbLCpu), Minutes(cmbLRam), Minutes(cmbLGpu), Minutes(cmbLDisk) });
        SetText(lblLoadTotal, m == 0 ? "Keine Komponente gewählt" : "Gesamtdauer: " + MinText(m) + " (alle gewählten Komponenten laufen gleichzeitig)");
        lblLoadTotal.ForeColor = m == 0 ? UI.Warn : UI.Text;
    }

    // Hinweise der Seite Lasttest, die sich nach Gerät und Einstellung richten (Akkubetrieb, PawnIO)
    void UpdateLoadExtras()
    {
        try { if (lblAkku != null) lblAkku.Visible = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline; } catch { }
        try { if (badgePawn != null) badgePawn.Text = (cmbDriver == null || cmbDriver.SelectedIndex == 0) && PawnIoVersion().Length == 0 ? "PawnIO vorübergehend" : ""; } catch { }
    }

    // Laufwerksname kurz: Busangaben und Größenangabe im Namen entfallen ("NVMe PC SN820 NVMe WD 4096GB" -> "PC SN820 WD")
    public static string ShortDiskName(string name)
    {
        string n = " " + (name ?? "") + " ";
        n = System.Text.RegularExpressions.Regex.Replace(n, @"(?i)\b(NVMe|SATA|SCSI|ATA|USB|Device|Disk)\b", " ");
        n = System.Text.RegularExpressions.Regex.Replace(n, @"(?i)\b\d+([.,]\d+)?\s?(GB|TB|G|T)\b", " ");
        n = System.Text.RegularExpressions.Regex.Replace(n, @"\s+", " ").Trim();
        return n.Length > 0 ? n : (name ?? "").Trim();
    }
}
