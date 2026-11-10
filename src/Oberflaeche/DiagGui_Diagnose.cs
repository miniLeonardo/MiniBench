// Seite Diagnose (Teilklasse DiagGui)
public partial class DiagGui
{
    Control BuildDiagPage()
    {
        FlowLayoutPanel f = Page("Diagnose", "Liest Hardware, Treiber, Sicherheit und Ereignisprotokolle aus, führt die gewählten Prüfungen durch und bewertet alles in einem Bericht. Die Prüfungen verändern nichts am System; gekennzeichnete Optionen nehmen Änderungen vor.", 1);
        if (File.Exists(Path.Combine(cpDir, "laufend.json")))
        {
            Label warn = Lbl("  Der letzte Lauf wurde nicht beendet (Absturz?). Die Absturzanalyse läuft beim nächsten Start automatisch zuerst.  ", 9.75f, true, UI.Crit);
            warn.BackColor = UI.CritBg; warn.Padding = new Padding(UI.S(6)); warn.Margin = new Padding(0, 0, 0, UI.S(8));
            f.Controls.Add(warn);
        }
        // ab v3.64 drei Profile als Segmentschalter; Funktionstest und Nur Absturzanalyse entfallen in der Oberfläche
        // (die Absturzanalyse läuft nach einem unterbrochenen Lauf ohnehin automatisch zuerst)
        f.Controls.Add(Section("Umfang"));
        rbVoll = new RadioButton(); rbSchnell = new RadioButton(); rbCustom = new RadioButton();
        FlowLayoutPanel seg = new FlowLayoutPanel(); seg.AutoSize = true; seg.WrapContents = false; seg.BackColor = UI.Panel; seg.Margin = new Padding(UI.S(4), 0, 0, UI.S(4));
        RadioButton[] rbs = new RadioButton[] { rbVoll, rbSchnell, rbCustom };
        string[] rt = new string[] { "Vollständig", "Schnell", "Benutzerdefiniert" };
        for (int i = 0; i < rbs.Length; i++) {
            rbs[i].Text = rt[i]; rbs[i].AutoSize = false; rbs[i].Size = new Size(UI.S(160), UI.S(34)); rbs[i].Margin = new Padding(0, 0, UI.S(6), 0);
            rbs[i].Cursor = Cursors.Hand; StyleSegment(rbs[i]);
            rbs[i].CheckedChanged += delegate { ApplyDiagProfile(); };
            seg.Controls.Add(rbs[i]);
        }
        f.Controls.Add(seg);
        lblDiagProfil = Hint("", 4); f.Controls.Add(lblDiagProfil);
        chkFast = Chk("Lesende Prüfungen parallel ausführen", true);
        chkFast.CheckedChanged += delegate { UpdateSummary(); };
        Control fastBox = Opt2(chkFast, "Updatesuche, Defender-Schnellscan, Energieanalyse und SMART-Langtest laufen nebenher. Messungen bleiben exklusiv, damit die Werte vergleichbar sind.");
        fastBox.Margin = new Padding(0, UI.S(UI.Abstand2), 0, 0); f.Controls.Add(fastBox);

        diagToggle = new GroupHeader("Prüfungen"); diagToggle.Margin = new Padding(0, UI.S(UI.Abstand3), 0, 0);
        Tip(diagToggle, "Klicken zeigt oder verbirgt die einzelnen Diagnoseprüfungen. Wer eine Prüfung ändert, wechselt zum Profil Benutzerdefiniert.");
        f.Controls.Add(diagToggle);

        diagBody = new FlowLayoutPanel(); diagBody.FlowDirection = FlowDirection.TopDown;
        diagBody.WrapContents = false; diagBody.AutoSize = true; diagBody.BackColor = UI.Panel;
        diagBody.Margin = new Padding(UI.S(UI.Abstand4), UI.S(UI.Abstand1), 0, UI.S(UI.Abstand1)); diagBody.Visible = false;

        diagChk = new CheckBox[diagKeys.Length];
        for (int i = 0; i < diagKeys.Length; i++) {
            int chkIdx = i;
            diagChk[i] = Chk(diagText[i], true);
            diagChk[i].Click += delegate {
                if (!rbCustom.Checked) {
                    rbCustom.Checked = true;
                    diagChk[chkIdx].Checked = !diagChk[chkIdx].Checked;
                }
            };
            diagChk[i].CheckedChanged += delegate { UpdateSummary(); };
            diagBody.Controls.Add(diagChk[i]);
        }
        diagBody.Controls.Add(Hint("Die CPU-Stabilität prüft das Modul Lasttest (Prozessor, ab 2 Min.): eigener Lastprozess, Rechenfehler, Takt- und Temperaturverlauf und Drosselnachweis.", 4));
        f.Controls.Add(diagBody);
        diagToggle.Toggled += delegate { diagBody.Visible = diagToggle.Expanded; };

        f.Controls.Add(Section("Optionen"));
        chkInstall = Chk("smartmontools bei Bedarf installieren oder aus dem Datenordner verwenden", true);
        chkMem = Chk("Windows-Speicherdiagnose beim nächsten Neustart einplanen", false);
        f.Controls.Add(OptionCard("Zusatzwerkzeuge und Speicherdiagnose", UI.IcoTools, new Control[] {
            Opt2(chkInstall, "Liegt smartctl im Tools-Ordner, wird es von dort verwendet, sonst per winget geholt und nach dem Lauf entfernt.", RiskBadge("Eingriff")),
            Opt2(chkMem, "Prüft den gesamten Arbeitsspeicher außerhalb von Windows; das Ergebnis liest der nächste Lauf.", RestartBadge("immer")) }));

        FlowLayoutPanel r = Row(); r.Controls.Add(RowLabel("Ereignisse der letzten", 170));
        cmbDays = Combo(110, new string[] { "3 Tage", "7 Tage", "14 Tage", "30 Tage", "60 Tage" }, 2); r.Controls.Add(cmbDays);
        FlowLayoutPanel rs = Row(); rs.Controls.Add(RowLabel("SMART-Langtest", 170));
        string[] sm = new string[smartMinutes.Length]; for (int i = 0; i < sm.Length; i++) sm[i] = "höchstens " + smartMinutes[i] + " Min. warten";
        cmbSmartMax = Combo(220, sm, 3); cmbSmartMax.SelectedIndexChanged += delegate { UpdateSummary(); }; rs.Controls.Add(cmbSmartMax);
        f.Controls.Add(OptionCard("Prüfzeiträume", UI.IcoDiag, new Control[] { r, rs, Hint("Danach entsteht der Bericht ohne Ergebnis des Langtests; der Test läuft im Laufwerk weiter.", 180) }));
        rbVoll.Checked = true;
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
        int[] add = new int[] { 1, 2, 3, 1, 2, 1, 2, 2 };
        for (int i = 0; sel != null && i < sel.Length && i < add.Length; i++) if (sel[i]) t += add[i];
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
        if (diagToggle == null || diagChk == null) return;
        int sel = 0;
        bool[] cur = new bool[diagChk.Length];
        for (int i = 0; i < diagChk.Length; i++) { cur[i] = diagChk[i] != null && diagChk[i].Checked; if (cur[i]) sel++; }
        string pName = rbVoll != null && rbVoll.Checked ? "Vollständig" : rbSchnell != null && rbSchnell.Checked ? "Schnell" : "Benutzerdefiniert";
        diagToggle.CountText = sel + " von " + diagChk.Length + " gewählt, Profil " + pName;
        if (lblDiagProfil != null)
        {
            string smart = cur.Length > 4 && cur[4] ? ", dazu das Warten auf den SMART-Langtest (höchstens " + SmartMax() + " Min.)" : "";
            string par = chkFast != null && chkFast.Checked ? " mit paralleler Ausführung" : "";
            string t = rbVoll != null && rbVoll.Checked ? "Alle " + diagChk.Length + " Prüfungen, ca. " + DiagEstimate(cur) + " Min." + par + smart
                : rbSchnell != null && rbSchnell.Checked ? "Inventar, Ereignisse, Updatesuche, Netzwerk und kurzer RAM-Test (25 % des freien RAM), ca. " + DiagEstimate(cur) + " Min." + par
                : "Prüfungen unten frei wählen, derzeit " + sel + " gewählt, ca. " + DiagEstimate(cur) + " Min." + par + smart;
            SetText(lblDiagProfil, t);
        }
    }

    void ApplyDiagProfile()
    {
        if (diagChk == null) return;
        bool custom = rbCustom.Checked;
        if (custom && diagBody != null)
        {
            diagBody.Visible = true;
            if (diagToggle != null) diagToggle.Expanded = true;
        }
        bool[] pick = rbVoll.Checked ? diagVoll : rbSchnell.Checked ? diagSchnell : null;
        for (int i = 0; i < diagChk.Length; i++)
        {
            if (pick != null) diagChk[i].Checked = pick[i];
            diagChk[i].AutoCheck = custom;
            diagChk[i].ForeColor = custom ? UI.Text : (UI.IsDark ? Color.FromArgb(195, 200, 210) : Color.FromArgb(80, 85, 95));
        }
        foreach (RadioButton r in new RadioButton[] { rbVoll, rbSchnell, rbCustom }) if (r != null) StyleSegment(r);
        UpdateDiagSummary();
        UpdateSummary();
    }
}
