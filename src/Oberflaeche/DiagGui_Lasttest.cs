// Seite Lasttest (Teilklasse DiagGui)
public partial class DiagGui
{
    // ab v4.09: Seite im Muster von Benchmark und Diagnose (bis v4.08 eine Tabelle mit Kontrollkästchen). Links die Karte
    // Komponenten mit je einer Zeile (Symbol, Titel, Beschreibung, Dauer, Schalter); Dauer und Einstellungen einer
    // Komponente stehen unter ihrer Zeile und nur, wenn sie eingeschaltet ist. Rechts Abbruchschwelle und Sensoren. Die
    // Kontrollkästchen chkLCpu bis chkLDisk bleiben als Werte bestehen (Vorlagen, Zeitschätzung, Start).
    static readonly string[] lastTitel = new string[] { "Prozessor", "Arbeitsspeicher", "Grafik", "Datenträger" };
    static readonly string[] lastDetail = new string[] {
        "Alle Threads mit Ergebnisprüfung",
        "Mustertest mit Bitfehlerprüfung",
        "Rendertest mit Direct3D 11, Bildprüfung alle 30 s, Erkennung von Treiber-Resets",
        "Schreiben, Lesen und Prüfen jedes Blocks; Testdatei bis 4 GB, wird danach gelöscht" };
    static readonly string[] lastIcons = new string[] { UI.IcoCpu, UI.IcoRam, UI.IcoGpu, UI.IcoDisk };
    static readonly string[] lastTips = new string[] {
        "Alle logischen Prozessoren rechnen mit Ergebnisprüfung; Rechenfehler deuten auf Instabilität (Übertaktung, Spannung, Kühlung, Netzteil). Läuft in einem eigenen Prozess ohne Unterbrechungen. Mit Sensoren gibt es Takt- und Temperaturverlauf und den Drosselnachweis. Ersetzt ab v2.7 den CPU-Stabilitätstest der Diagnose.",
        "Mustertest über einen Teil des freien Arbeitsspeichers mit Bitfehlerprüfung. Neben der CPU-Last läuft er mit niedriger Priorität auf einem Viertel der Threads.",
        "Eigener Rendertest mit Direct3D 11 als Dauerlast auf den gewählten Grafikeinheiten gleichzeitig: Bilder/s-Verlauf, Bildprüfung alle 30 Sekunden und Erkennung von Treiber-Resets.",
        "Schreibt eine Testdatei (bis 4 GB), liest sie zurück und prüft jeden Block. Meldet Datenfehler, Ein-/Ausgabefehler und Controllerereignisse. Die Datei wird danach gelöscht." };

    internal ModulSeite lastSeite;
    internal SettingRow[] lastRows;
    internal UnterBereich[] lastSub;
    // Sichtbarkeit der Bereiche unter den Zeilen als eigener Zustand (für Tests ohne angezeigtes Fenster)
    internal bool[] lastSubOffen = new bool[] { true, true, true, true };
    Badge[] lastDauer;
    LinkLabel lnkLastAll, lnkLastNone, lnkLastSens;
    Label lblAkku, lblLastSens;
    Badge badgePawn;

    CheckBox[] LastChk() { return new CheckBox[] { chkLCpu, chkLRam, chkLGpu, chkLDisk }; }
    ComboBox[] LastDauerWahl() { return new ComboBox[] { cmbLCpu, cmbLRam, cmbLGpu, cmbLDisk }; }

    Control BuildLoadPage()
    {
        FlowLayoutPanel f = Page("Lasttest", "Belastet die gewählten Komponenten gleichzeitig, jede mit eigener Dauer, und zeichnet Temperatur, Takt, Leistung und Lüfter auf. Gemeldet werden Rechen-, Bit- und Datenfehler, WHEA-Fehler und Drosselung. Während des Tests lässt er sich jederzeit beenden.", 3);
        lastSeite = NeueModulSeite(f);
        string[] mins = new string[] { "2 Min.", "5 Min.", "10 Min.", "15 Min.", "30 Min.", "60 Min.", "120 Min.", "240 Min." };
        lblAkku = Lbl("Akkubetrieb: Für den Lasttest das Netzteil anschließen, sonst begrenzt die Energieverwaltung Takt und Leistung.", UI.SchriftText, true, UI.Warn);
        lblAkku.BackColor = UI.WarnBg; lblAkku.Padding = new Padding(UI.S(10), UI.S(6), UI.S(10), UI.S(6)); lblAkku.Margin = new Padding(0, 0, 0, UI.S(UI.Abstand3)); lblAkku.Visible = false;
        f.Controls.Add(lblAkku); lastSeite.Voll.Add(lblAkku);

        ZeilenKarte kk = lastSeite.Karte("Komponenten", true);
        lnkLastAll = KartenVerweis("Alle an"); lnkLastNone = KartenVerweis("Alle aus");
        kk.KopfRechts.Controls.Add(lnkLastAll); kk.KopfRechts.Controls.Add(lnkLastNone);
        lnkLastAll.LinkClicked += delegate { foreach (CheckBox c in LastChk()) c.Checked = true; };
        lnkLastNone.LinkClicked += delegate { foreach (CheckBox c in LastChk()) c.Checked = false; };

        chkLCpu = Chk("Prozessor", true); cmbLCpu = Combo(100, mins, 0);
        chkLRam = Chk("Arbeitsspeicher", true); cmbLRam = Combo(100, mins, 0);
        cmbLRamPct = Combo(100, new string[] { "25 %", "40 %", "60 %", "75 %" }, 1);
        chkLGpu = Chk("Grafik", true); cmbLGpu = Combo(100, mins, 0);
        cmbLGpuSel = Combo(210, GpuChoiceTexts(), 0);
        cmbLGpuRes = Combo(105, new string[] { "1280x720", "1920x1080" }, 0);
        cmbLGpuShow = Combo(125, new string[] { "Fenster", "Vollbild", "ohne Anzeige" }, 0);
        // Einstellungen des Rendertests auf beiden Seiten gleich halten: Benchmark -> Lasttest und Lasttest -> Benchmark
        // (ab 3.53 über CopySelection; vorher setzte sync2 die Grafikeinheit des Lasttests auf den Wert des Benchmarks zurück)
        EventHandler sync1 = delegate { if (syncGpu || cmbGpuRes == null) return; syncGpu = true; try { CopySelection(GpuCombosBench(), GpuCombosLast()); } finally { syncGpu = false; } };
        EventHandler sync2 = delegate { if (syncGpu || cmbGpuRes == null) return; syncGpu = true; try { CopySelection(GpuCombosLast(), GpuCombosBench()); } finally { syncGpu = false; } };
        if (cmbGpuRes != null) { cmbGpuRes.SelectedIndexChanged += sync1; cmbGpuShow.SelectedIndexChanged += sync1; }
        if (cmbGpuSel != null) cmbGpuSel.SelectedIndexChanged += sync1;
        cmbLGpuRes.SelectedIndexChanged += sync2; cmbLGpuShow.SelectedIndexChanged += sync2; cmbLGpuSel.SelectedIndexChanged += sync2;

        chkLDisk = Chk("Datenträger", false); cmbLDisk = Combo(100, mins, 0);
        List<string> letters = new List<string>();
        foreach (string d in disks)
        {
            string[] x = d.Split('|'); if (x.Length < 7 || x[5].Length == 0) continue;
            foreach (string l in x[5].Split(',')) { string tl = l.Trim(); if (tl.Length > 0) letters.Add(tl + "  " + ShortDiskName(x[1]) + (x[4].Length > 0 ? " (" + x[4] + ")" : "")); }
        }
        if (letters.Count == 0) letters.Add("C:");
        letters.Sort();
        cmbLDiskDrive = Combo(260, letters.ToArray(), 0);

        CheckBox[] chk = LastChk(); ComboBox[] dauer = LastDauerWahl();
        lastRows = new SettingRow[chk.Length]; lastSub = new UnterBereich[chk.Length]; lastDauer = new Badge[chk.Length];
        for (int i = 0; i < chk.Length; i++)
        {
            lastRows[i] = SchalterZeile(chk[i], lastTitel[i], lastDetail[i], lastIcons[i], lastTips[i], null);
            lastDauer[i] = DauerEtikett(Minutes(dauer[i])); Tip(lastDauer[i], "Gewählte Dauer dieser Komponente.");
            lastRows[i].AddBadge(lastDauer[i]);
            kk.Zeile(lastRows[i]);
            // Dauer und Einstellungen der Komponente nebeneinander, bei schmaler Karte mit Umbruch
            lastSub[i] = new UnterBereich();
            FeldReihe fr = new FeldReihe();
            fr.Feld("Dauer", dauer[i]);
            if (i == 1) fr.Feld("Anteil am freien RAM", cmbLRamPct);
            else if (i == 2) { fr.Feld("Grafikeinheit", cmbLGpuSel); fr.Feld("Auflösung", cmbLGpuRes); fr.Feld("Anzeige", cmbLGpuShow); }
            else if (i == 3) fr.Feld("Laufwerk", cmbLDiskDrive);
            lastSub[i].Inhalt.Controls.Add(fr);
            kk.Zeile(lastSub[i]);
            chk[i].CheckedChanged += delegate { UpdateLoadEnabled(); UpdateSummary(); };
            dauer[i].SelectedIndexChanged += delegate { UpdateSummary(); };
        }
        Label hk = Lbl("Alle gewählten Komponenten starten gleichzeitig, jede endet nach ihrer eigenen Dauer. Die Gesamtdauer ist die längste gewählte Dauer.", 8.5f, false, UI.Muted); hk.Margin = new Padding(0, UI.S(4), 0, UI.S(2));
        kk.Frei(hk);

        ZeilenKarte ka = lastSeite.Karte("Abbruchschwelle", false);
        cmbLAbortCpu = Combo(140, new string[] { "Automatisch", "85 °C", "90 °C", "95 °C", "100 °C", "aus" }, 0);
        cmbLAbortGpu = Combo(140, new string[] { "85 °C", "90 °C", "95 °C", "aus" }, 1);
        FeldReihe fa = new FeldReihe(); fa.Feld("CPU-Temperatur", cmbLAbortCpu); fa.Feld("GPU-Temperatur", cmbLAbortGpu);
        ka.Zeile(fa);
        Label ha = Lbl("Liegt die Temperatur rund 10 Sekunden auf oder über der Schwelle, endet der Test mit einem Befund. Automatisch: TjMax der CPU, sonst 100 °C.", 8.5f, false, UI.Muted); ha.Margin = new Padding(0, UI.S(2), 0, UI.S(2));
        ka.Frei(ha);

        // Sensoren: woher die CPU-Werte des Laufs kommen (Treiber PawnIO, Einstellung auf der Seite Sensoren live)
        ZeilenKarte ks = lastSeite.Karte("Sensoren", false);
        badgePawn = new Badge("PawnIO vorübergehend", "info"); badgePawn.Margin = new Padding(UI.S(6), UI.S(2), 0, 0);
        Tip(badgePawn, "Echte CPU-Temperaturen liefert LibreHardwareMonitor mit dem Treiber PawnIO. Er wird für den Lauf vorübergehend installiert und danach wieder entfernt (einstellbar auf der Seite Sensoren live).");
        ks.KopfRechts.Controls.Add(badgePawn);
        lblLastSens = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblLastSens.Margin = new Padding(0, 0, 0, UI.S(6));
        ks.Frei(lblLastSens);
        lnkLastSens = KartenVerweis("Sensoren live öffnen"); lnkLastSens.Margin = new Padding(0, 0, 0, UI.S(2));
        lnkLastSens.LinkClicked += delegate { if (!running) ShowPage(7); };
        ks.Frei(lnkLastSens);

        f.Controls.Add(lastSeite.Spalten);
        UpdateLoadEnabled();
        UpdateLoadExtras();
        lastSeite.Ordne(SeitenBreite());
        return f;
    }

    // Bereiche unter den Zeilen: nur bei eingeschalteter Komponente sichtbar
    void UpdateLoadEnabled()
    {
        if (lastSub != null && chkLCpu != null)
        {
            CheckBox[] chk = LastChk();
            for (int i = 0; i < chk.Length && i < lastSub.Length; i++)
                if (lastSub[i] != null && lastSubOffen[i] != chk[i].Checked) { lastSubOffen[i] = chk[i].Checked; lastSub[i].Visible = chk[i].Checked; }
        }
        UpdateLoadTotal();
    }

    // Kopf der Seite (Anzahl, Gesamtdauer) und Dauer an jeder Zeile
    void UpdateLoadTotal()
    {
        if (lastSeite == null || chkLCpu == null) return;
        CheckBox[] chk = LastChk(); ComboBox[] dauer = LastDauerWahl();
        bool[] on = new bool[chk.Length]; int[] min = new int[chk.Length]; int n = 0;
        for (int i = 0; i < chk.Length; i++)
        {
            on[i] = chk[i].Checked; min[i] = Minutes(dauer[i]); if (on[i]) n++;
            if (lastDauer != null && lastDauer[i] != null) SetText(lastDauer[i], MinText(min[i]));
        }
        int m = LoadTotalMinutes(on, min);
        SetText(lastSeite.Anzahl, n + " von " + chk.Length + " Komponenten");
        SetText(lastSeite.Dauer, m == 0 ? "keine Komponente gewählt" : "Gesamtdauer " + MinText(m));
    }

    // Hinweise der Seite Lasttest, die sich nach Gerät und Einstellung richten (Akkubetrieb, PawnIO)
    void UpdateLoadExtras()
    {
        try { if (lblAkku != null) lblAkku.Visible = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline; } catch { }
        try
        {
            bool treiber = cmbDriver == null || cmbDriver.SelectedIndex == 0;
            string pawn = PawnIoVersion();
            if (badgePawn != null) badgePawn.Text = treiber && pawn.Length == 0 ? "PawnIO vorübergehend" : "";
            if (lblLastSens != null) SetText(lblLastSens, LastSensorText(treiber, pawn));
        }
        catch { }
    }

    // Text der Karte Sensoren: woher CPU-Temperatur, Takt, Leistung und Lüfter des Laufs kommen
    public static string LastSensorText(bool treiber, string pawnVersion)
    {
        string werte = "CPU-Temperatur, CPU-Takt, CPU-Leistung und Mainboard-Lüfter";
        if (!treiber) return "Der Treiber PawnIO wird nicht verwendet. " + werte + " liefert LibreHardwareMonitor dem Lauf damit nicht.";
        if (String.IsNullOrEmpty(pawnVersion)) return werte + " liefert LibreHardwareMonitor mit dem Treiber PawnIO. Er wird für den Lauf vorübergehend installiert und danach wieder entfernt.";
        return werte + " liefert LibreHardwareMonitor mit dem Treiber PawnIO. PawnIO " + pawnVersion + " ist installiert und bleibt unverändert.";
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
