// Seite Diagnose (Teilklasse DiagGui)
public partial class DiagGui
{
    // ab v4.08: Kurztitel und Beschreibung je Prüfung für die Zeilen der Karte Prüfungen (Reihenfolge wie diagKeys). Der
    // volle Text (diagText) und die Erklärung (diagTips) stehen im Hinweis beim Überfahren.
    static readonly string[] diagKurz = new string[] {
        "Ereignisse und Abstürze", "Windows-Updates", "Systemintegrität", "Virenscan", "SMART-Langtest", "Netzwerk", "Arbeitsspeicher", "Energie und DxDiag" };
    static readonly string[] diagDetail = new string[] {
        "Ereignisprotokolle, Bluescreens, Absturzabbilder, Zuverlässigkeit",
        "Sucht nach ausstehenden Updates, lädt nichts herunter",
        "Dateisystem-Onlinescan, DISM ScanHealth, sfc /verifyonly (nur prüfen)",
        "Microsoft Defender Schnellscan",
        "Erweiterter Selbsttest aller Laufwerke; SSD meist unter 15 Min., Festplatten Stunden",
        "Ping, DNS, HTTPS, Download",
        "Mustertest unter Windows",
        "Energieeffizienzanalyse (60 s) und DxDiag-Bericht" };
    static readonly string[] diagIcons = new string[] { UI.IcoChg, UI.IcoRefresh, UI.IcoDokument, UI.IcoShield, UI.IcoDisk, UI.IcoNetz, UI.IcoRam, UI.IcoBlitz };
    // Minuten je Prüfung und ob sie bei paralleler Ausführung nebenher läuft (Parallel im Vertrag des Moduls Diagnose)
    static readonly int[] diagDauer = new int[] { 1, 2, 3, 1, 2, 1, 2, 2 };
    public static readonly bool[] diagNebenher = new bool[] { false, true, false, true, true, false, false, true };

    internal ModulSeite diagSeite;
    internal FeldAuswahl segUmfang;
    internal SettingRow[] diagRows;
    Badge[] diagNebenEtikett;
    LinkLabel lnkDiagAll, lnkDiagNone;

    // ab v4.08: Seite im Muster von Wartung und Tools. Oben der Umfang als Auswahl mit drei Feldern, links die Karte
    // Prüfungen (immer sichtbar, je Prüfung eine Zeile mit Schalter), rechts Ausführung, Zusätze und Zeiträume. Die
    // Kontrollkästchen und die drei Optionsfelder bleiben als Werte bestehen (Vorlagen, Zeitschätzung, Start).
    Control BuildDiagPage()
    {
        FlowLayoutPanel f = Page("Diagnose", "Liest Hardware, Treiber, Sicherheit und Ereignisprotokolle aus und führt die gewählten Prüfungen durch. Prüfungen verändern nichts; gekennzeichnete Zusätze nehmen Änderungen vor.", 1);
        diagSeite = NeueModulSeite(f);
        if (File.Exists(Path.Combine(cpDir, "laufend.json")))
        {
            Label warn = Lbl("  Der letzte Lauf wurde nicht beendet (Absturz?). Die Absturzanalyse läuft beim nächsten Start automatisch zuerst.  ", 9.75f, true, UI.Crit);
            warn.BackColor = UI.CritBg; warn.Padding = new Padding(UI.S(6)); warn.Margin = new Padding(0, 0, 0, UI.S(8));
            f.Controls.Add(warn);
        }
        // Umfang: drei Profile. Die Optionsfelder tragen den Wert (Reihenfolge der Vorlagen: Voll, Schnell, Benutzerdefiniert),
        // sichtbar ist die Auswahl mit Anzahl und Dauer je Feld. Funktionstest und Nur Absturzanalyse gibt es seit v3.64
        // nicht mehr in der Oberfläche (die Absturzanalyse läuft nach einem unterbrochenen Lauf automatisch zuerst).
        rbVoll = new RadioButton(); rbSchnell = new RadioButton(); rbCustom = new RadioButton();
        Panel werte = new Panel(); werte.Visible = false; werte.Size = new Size(1, 1); werte.Margin = new Padding(0);
        RadioButton[] rbs = new RadioButton[] { rbSchnell, rbVoll, rbCustom };
        segUmfang = new FeldAuswahl(new string[] { "Schnell", "Vollständig", "Eigene Auswahl" }, new string[] { "", "", "Prüfungen einzeln wählen" });
        segUmfang.Selected = 1;
        for (int i = 0; i < rbs.Length; i++)
        {
            int idx = i;
            rbs[i].AutoSize = false; rbs[i].Size = new Size(1, 1);
            rbs[i].CheckedChanged += delegate { if (rbs[idx].Checked && segUmfang.Selected != idx) segUmfang.Selected = idx; ApplyDiagProfile(); };
            werte.Controls.Add(rbs[i]);
        }
        segUmfang.SelectedChanged += delegate { if (!rbs[segUmfang.Selected].Checked) rbs[segUmfang.Selected].Checked = true; };
        f.Controls.Add(segUmfang); f.Controls.Add(werte);

        ZeilenKarte kp = diagSeite.Karte("Prüfungen", true);
        lnkDiagAll = KartenVerweis("Alle an"); lnkDiagNone = KartenVerweis("Alle aus");
        kp.KopfRechts.Controls.Add(lnkDiagAll); kp.KopfRechts.Controls.Add(lnkDiagNone);
        lnkDiagAll.LinkClicked += delegate { rbCustom.Checked = true; foreach (CheckBox c in diagChk) c.Checked = true; };
        lnkDiagNone.LinkClicked += delegate { rbCustom.Checked = true; foreach (CheckBox c in diagChk) c.Checked = false; };

        diagChk = new CheckBox[diagKeys.Length]; diagRows = new SettingRow[diagKeys.Length]; diagNebenEtikett = new Badge[diagKeys.Length];
        for (int i = 0; i < diagKeys.Length; i++)
        {
            diagChk[i] = Chk(diagText[i], true);
            string t; diagTips.TryGetValue(diagKeys[i], out t);
            // Wer einen Schalter umlegt, wählt damit die eigene Auswahl (wie bisher beim Klick auf ein Kästchen)
            diagRows[i] = SchalterZeile(diagChk[i], diagKurz[i], diagDetail[i], diagIcons[i], diagText[i] + (String.IsNullOrEmpty(t) ? "" : "\r\n" + t), delegate { if (!rbCustom.Checked) rbCustom.Checked = true; });
            if (diagNebenher[i]) { diagNebenEtikett[i] = new Badge("läuft nebenher", "skip"); diagNebenEtikett[i].Tag = "nebenher"; BadgeTip(diagNebenEtikett[i]); diagRows[i].AddBadge(diagNebenEtikett[i]); }
            Badge bd = DauerEtikett(diagDauer[i]); BadgeTip(bd); diagRows[i].AddBadge(bd);
            diagChk[i].CheckedChanged += delegate { UpdateSummary(); };
            kp.Zeile(diagRows[i]);
        }
        Label hc = Lbl("Die CPU-Stabilität prüft das Modul Lasttest (Prozessor, ab 2 Min.): eigener Lastprozess, Rechenfehler, Takt- und Temperaturverlauf und Drosselnachweis.", 8.5f, false, UI.Muted); hc.Margin = new Padding(0, UI.S(4), 0, UI.S(2));
        kp.Frei(hc);

        ZeilenKarte ka = diagSeite.Karte("Ausführung", false);
        chkFast = Chk("Lesende Prüfungen parallel ausführen", true);
        chkFast.CheckedChanged += delegate { UpdateSummary(); };
        ka.Zeile(SchalterZeile(chkFast, "Parallel ausführen", "Lesende Prüfungen laufen nebenher, Messungen bleiben exklusiv", UI.IcoBlitz,
            "Lesende Prüfungen ohne Messcharakter (Updatesuche, Defender, Energieanalyse, Start des SMART-Langtests) laufen nebenher. Messungen wie RAM-Test, Netzwerk, Benchmark und Lasttest warten immer, bis alles andere fertig ist.", null));

        ZeilenKarte kz = diagSeite.Karte("Zusätze", false);
        chkInstall = Chk("smartmontools bei Bedarf installieren oder aus dem Datenordner verwenden", true);
        chkMem = Chk("Windows-Speicherdiagnose beim nächsten Neustart einplanen", false);
        SettingRow rInst = SchalterZeile(chkInstall, "smartmontools holen", "Aus dem Tools-Ordner, sonst per winget; nach dem Lauf entfernt", UI.IcoWartung,
            "smartctl liefert SMART-Werte und den Langtest. Liegt es im Tools-Ordner des Datenordners, wird es von dort verwendet, sonst per winget geholt und nach dem Lauf entfernt (oder auf Wunsch behalten). Risikostufe Eingriff.", null);
        Badge bi = RiskBadge("Eingriff"); BadgeTip(bi); rInst.AddBadge(bi);
        SettingRow rMem = SchalterZeile(chkMem, "Speicherdiagnose", "Windows-Speicherdiagnose beim nächsten Neustart einplanen", UI.IcoRam,
            "Plant die Windows-Speicherdiagnose für den nächsten Neustart ein. Sie prüft den gesamten Arbeitsspeicher außerhalb von Windows; das Ergebnis liest der nächste Lauf aus den Ereignissen.", null);
        Badge bn = RestartBadge("immer"); BadgeTip(bn); rMem.AddBadge(bn);
        kz.Zeile(rInst); kz.Zeile(rMem);

        ZeilenKarte kt = diagSeite.Karte("Zeiträume", false);
        cmbDays = Combo(170, new string[] { "3 Tage", "7 Tage", "14 Tage", "30 Tage", "60 Tage" }, 2);
        string[] sm = new string[smartMinutes.Length]; for (int i = 0; i < sm.Length; i++) sm[i] = "höchstens " + smartMinutes[i] + " Min. warten";
        cmbSmartMax = Combo(220, sm, 3); cmbSmartMax.SelectedIndexChanged += delegate { UpdateSummary(); };
        FeldReihe fz = new FeldReihe(); fz.Feld("Ereignisse der letzten", cmbDays); fz.Feld("SMART-Langtest", cmbSmartMax);
        kt.Zeile(fz);
        Label hz = Lbl("Nach der Wartezeit entsteht der Bericht ohne Ergebnis des Langtests; der Test läuft im Laufwerk weiter.", 8.5f, false, UI.Muted); hz.Margin = new Padding(0, UI.S(2), 0, UI.S(2));
        kt.Frei(hz);

        f.Controls.Add(diagSeite.Spalten);
        rbVoll.Checked = true;
        diagSeite.Ordne(SeitenBreite());
        return f;
    }

    // Ereignisse, Updatesuche, Integritaet, Defender, SmartLang, Netzwerk, RamTest, Energieanalyse
    // (CPU-Stabilität ab v2.7 nur noch im Modul Lasttest)
    static readonly bool[] diagVoll = DiagPreset(true);
    static readonly bool[] diagSchnell = DiagPreset(false);
    static bool[] DiagPreset(bool full)
    {
        bool[] voll = new bool[] { true, true, true, true, true, true, true, true };
        bool[] schnell = new bool[] { true, true, false, false, false, true, true, false };
        return full ? voll : schnell;
    }

    // Geschätzte Minuten der Diagnose ohne Wartezeit auf den SMART-Langtest (Grundlage für Profiltext und Fußleiste)
    public static int DiagMinutes(bool[] sel)
    {
        int t = 0;
        for (int i = 0; sel != null && i < sel.Length && i < diagDauer.Length; i++) if (sel[i]) t += diagDauer[i];
        return t;
    }

    // Schätzung der Diagnose für eine Auswahl, bei paralleler Ausführung abzüglich der nebenher laufenden Prüfungen
    // (ab v3.65 auch für die Zeile unter dem Profil; vorher zeigte sie ohne Abzug eine andere Zahl als die Seitenleiste)
    int DiagEstimate(bool[] sel)
    {
        int t = DiagMinutes(sel);
        // schneller Modus: Updatesuche, Defender und Energieanalyse laufen neben den übrigen Prüfungen
        if (chkFast != null && chkFast.Checked && chkFast.Enabled) { int par = (diagChk[1].Checked ? 2 : 0) + (diagChk[3].Checked ? 1 : 0) + (diagChk[7].Checked ? 2 : 0); t -= Math.Min(par, Math.Max(0, t - 1)); }
        return t;
    }

    void UpdateDiagSummary()
    {
        if (diagSeite == null || diagChk == null || segUmfang == null) return;
        int sel = 0;
        bool[] cur = new bool[diagChk.Length];
        for (int i = 0; i < diagChk.Length; i++) { cur[i] = diagChk[i] != null && diagChk[i].Checked; if (cur[i]) sel++; }
        bool par = chkFast != null && chkFast.Checked;
        string smart = cur.Length > 4 && cur[4] ? " + Langtest bis " + MinText(SmartMax()) : "";
        SetText(diagSeite.Anzahl, sel + " von " + diagChk.Length + " Prüfungen");
        SetText(diagSeite.Dauer, sel > 0 ? "ca. " + MinText(DiagEstimate(cur)) + smart : "nur Inventar, ca. 1 Min.");
        segUmfang.SetUnterzeile(0, DiagProfilText(diagSchnell));
        segUmfang.SetUnterzeile(1, DiagProfilText(diagVoll));
        // "läuft nebenher" nur, solange parallel ausgeführt wird
        for (int i = 0; diagNebenEtikett != null && i < diagNebenEtikett.Length; i++) if (diagNebenEtikett[i] != null && diagNebenEtikett[i].Visible != par) diagNebenEtikett[i].Visible = par;
    }

    // Unterzeile eines Profils in der Auswahl des Umfangs: "4 Prüfungen · ca. 4 Min."
    string DiagProfilText(bool[] pick)
    {
        int n = 0; foreach (bool b in pick) if (b) n++;
        return n + " Prüfungen  ·  ca. " + MinText(DiagEstimateFor(pick));
    }

    // Schätzung für ein Profil (nicht für die aktuelle Auswahl): Abzug der nebenher laufenden Prüfungen aus dem Profil selbst
    int DiagEstimateFor(bool[] pick)
    {
        int t = DiagMinutes(pick);
        if (chkFast != null && chkFast.Checked && chkFast.Enabled) { int par = (pick[1] ? 2 : 0) + (pick[3] ? 1 : 0) + (pick[7] ? 2 : 0); t -= Math.Min(par, Math.Max(0, t - 1)); }
        return t;
    }

    void ApplyDiagProfile()
    {
        if (diagChk == null || rbCustom == null) return;
        bool[] pick = rbVoll.Checked ? diagVoll : rbSchnell.Checked ? diagSchnell : null;
        if (pick != null) for (int i = 0; i < diagChk.Length; i++) diagChk[i].Checked = pick[i];
        UpdateDiagSummary();
        UpdateSummary();
    }
}
