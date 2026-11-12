// Ablauf: gewählte Module, Zeitschätzung, Start eines Laufs (Teilklasse DiagGui)
public partial class DiagGui
{
    // ------------------------------------------------------------ Seite Änderungen
    // ------------------------------------------------------------ Seite Versionen (ab v2.7)
    int SmartMax() { return cmbSmartMax != null && cmbSmartMax.SelectedIndex >= 0 && cmbSmartMax.SelectedIndex < smartMinutes.Length ? smartMinutes[cmbSmartMax.SelectedIndex] : 120; }

    int Minutes(ComboBox c) { int v; return int.TryParse(Convert.ToString(c.SelectedItem).Split(' ')[0], out v) ? v : 2; }

    List<string> SelectedModules()
    {
        List<string> m = new List<string>();
        string[] names = new string[] { "Diagnose", "Benchmark", "Lasttest", "Wartung", "Optimierung" };
        for (int i = 0; i < names.Length; i++) if (nav[i + 1].Checked) m.Add(names[i]);
        return m;
    }

    // Geschätzte Minuten je Modul (Index 0..4 für die 5 Module), ohne Wartezeit auf den SMART-Langtest
    internal int ModuleMinutes(int mod)
    {
        int t = 0;
        if (mod == 0)
        {
            bool[] cur = new bool[diagChk.Length];
            for (int i = 0; i < diagChk.Length; i++) cur[i] = diagChk[i].Checked;
            t = DiagEstimate(cur);
        }
        else if (mod == 1)
        {
            bool kurz = false;
            if (benchChk[0].Checked) t += kurz ? 1 : 2;
            if (benchChk[1].Checked) t += 1;
            if (benchChk[2].Checked) t += 2;
            if (benchChk[3].Checked) t += DiskGewaehlt();
        }
        else if (mod == 2)
        {
            int m = LoadTotalMinutes(new bool[] { chkLCpu.Checked, chkLRam.Checked, chkLGpu.Checked, chkLDisk.Checked },
                new int[] { Minutes(cmbLCpu), Minutes(cmbLRam), Minutes(cmbLGpu), Minutes(cmbLDisk) + 1 });
            t = m + 1;
        }
        else if (mod == 3) { for (int i = 0; i < repChk.Length; i++) if (repChk[i].Checked) t += repMinutes[i]; }
        else if (mod == 4) { int n = 0; foreach (OptItem it in optItems) if (it.Box.Checked && !it.Gesperrt) { n++; t += it.Minuten; } t += 1 + n / 40; }
        return t;
    }

    int EstimateMinutes()
    {
        int t = 0;
        for (int i = 0; i < 5; i++) if (nav[i + 1].Checked) t += ModuleMinutes(i);
        return t;
    }

    // Lasttest: alle gewählten Komponenten laufen gleichzeitig, die Gesamtdauer ist die längste gewählte Dauer
    public static int LoadTotalMinutes(bool[] on, int[] minutes)
    {
        int m = 0;
        for (int i = 0; on != null && minutes != null && i < on.Length && i < minutes.Length; i++) if (on[i]) m = Math.Max(m, minutes[i]);
        return m;
    }

    // Beschriftung des Startknopfs: sagt, was er startet (vorher immer nur "Start", auch auf der Seite Tools)
    public static string StartButtonText(List<string> modules)
    {
        if (modules == null || modules.Count == 0) return "Ablauf wählen";
        if (modules.Count == 1) return modules[0] + " starten";
        return modules.Count + " Module starten";
    }

    // Kurzform für Minuten: "45 Min.", "1 Std. 20 Min."
    public static string MinText(int min)
    {
        min = Math.Max(1, min);
        if (min < 60) return min + " Min.";
        return (min / 60) + " Std." + (min % 60 > 0 ? " " + (min % 60) + " Min." : "");
    }

    void UpdateSummary()
    {
        UpdateDiagSummary();
        UpdateBenchSummary();
        UpdateLoadTotal();
        if (lblSel == null || btnStart == null || diagChk == null || benchChk == null || repChk == null || chkLCpu == null || chkOptRestore == null) return;
        List<string> m = SelectedModules();
        for (int i = 0; i < 5 && (i + 1) < nav.Count; i++) { string h = nav[i + 1].Checked ? "ca. " + MinText(ModuleMinutes(i)) : ""; if (nav[i + 1].Hint != h) { nav[i + 1].Hint = h; nav[i + 1].Invalidate(); } }
        SetText(btnStart, StartButtonText(m));
        if (m.Count == 0)
        {
            lblSel.Text = "Noch kein Ablauf gewählt"; lblSel.ForeColor = UI.Muted; btnStart.Enabled = false;
            Tip(btnStart, "Erst eine Aufgabe wählen oder links ein Modul anhaken.");
            return;
        }
        int min = EstimateMinutes();
        bool smart = nav[1].Checked && diagChk[4].Checked;
        lblSel.ForeColor = UI.Text;
        List<string> shown = new List<string>(m);
        if (nav[1].Checked && chkFast != null && chkFast.Checked && chkFast.Enabled && shown.Count > 0 && shown[0] == "Diagnose") shown[0] = "Diagnose (parallel)";
        lblSel.Text = String.Join("  →  ", shown.ToArray())
            + "   ·   ca. " + MinText(min) + (smart ? ", mit SMART-Langtest höchstens " + MinText(min + SmartMax()) : "");
        btnStart.Enabled = true;
        Tip(btnStart, "Startet " + (m.Count == 1 ? "das Modul " + m[0] : "die Module in dieser Reihenfolge: " + String.Join(", ", m.ToArray())) + ". Geschätzte Dauer ca. " + MinText(min) + (smart ? " zuzüglich Warten auf den SMART-Langtest" : "") + ".");
    }

    static string Q(string s) { return "\"" + (s ?? "").TrimEnd('\\') + "\""; }

    void StartRun()
    {
        List<string> mods = SelectedModules();
        if (mods.Count == 0) return;
        // Live-Ansicht und Lauf greifen auf dieselben Sensoren und denselben Treiber zu
        if (liveRunning && !StopLiveAndWait()) return;
        bool drv = false; runKeep = "";
        if (nav[1].Checked || nav[2].Checked || nav[3].Checked)
        {
            TryAutoFetchSensorTools();
            drv = cmbDriver == null || cmbDriver.SelectedIndex == 0;
        }
        List<StartPunkt> punkte = new List<StartPunkt>(); List<string> wp = new List<string>(), zusatz = new List<string>(); bool ohneWp = false;
        StringBuilder a = new StringBuilder();
        a.Append("-NoProfile -ExecutionPolicy Bypass -File \"").Append(script).Append("\" -EventMode");
        a.Append(" -Module ").Append(String.Join(",", mods.ToArray()));
        if (dataDir.Length > 0) a.Append(" -DatenDir ").Append(Q(dataDir));
        if (!chkAnon.Checked) a.Append(" -KiOhneAnonymisierung");

        if (nav[1].Checked)
        {
            string prof = rbVoll.Checked ? "Voll" : rbSchnell.Checked ? "Schnell" : "Benutzerdefiniert";
            a.Append(" -DiagProfil ").Append(prof);
            List<string> o = new List<string>();
            for (int i = 0; i < diagChk.Length; i++) if (diagChk[i].Checked) o.Add(diagKeys[i]);
            a.Append(" -DiagOptionen ").Append(o.Count > 0 ? String.Join(",", o.ToArray()) : "Keine");
            if (chkInstall.Enabled && chkInstall.Checked) a.Append(" -InstallSmartmontools");
            if (chkMem.Enabled && chkMem.Checked) a.Append(" -ScheduleWindowsMemTest");
            if (cmbDays.Enabled) a.Append(" -EventDays ").Append(Convert.ToString(cmbDays.SelectedItem).Split(' ')[0]);
            if (diagChk[4].Checked) a.Append(" -SmartTimeoutMinutes ").Append(SmartMax());
        }
        if (nav[2].Checked)
        {
            List<string> b = new List<string>();
            for (int i = 0; i < benchChk.Length; i++) if (benchChk[i].Checked) b.Add(benchKeys[i]);
            if (b.Count == 0) { MessageBox.Show(this, "Im Benchmark ist keine Messung ausgewählt.", "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Information); ShowPage(2); return; }
            a.Append(" -BenchTests ").Append(String.Join(",", b.ToArray()));
            if (benchChk[3].Checked)
            {
                List<string> dn = new List<string>();
                for (int i = 0; i < diskZeilen.Count && i < diskNums.Count; i++) if (diskZeilen[i].Checked) dn.Add(diskNums[i]);
                if (dn.Count == 0) { MessageBox.Show(this, "Für den Laufwerks-Benchmark ist kein Laufwerk ausgewählt.", "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Information); ShowPage(2); return; }
                a.Append(" -BenchLaufwerke ").Append(String.Join(",", dn.ToArray()));
            }
            if (cmbRef != null)
            {
                if (cmbRef.SelectedIndex == 1) a.Append(" -ReferenzDatei *median");
                else if (cmbRef.SelectedIndex == 2) a.Append(" -ReferenzDatei *median:notebook");
                else if (cmbRef.SelectedIndex == 3) a.Append(" -ReferenzDatei *median:desktop");
                else if (cmbRef.SelectedIndex >= 4 && cmbRef.SelectedIndex - 4 < refItems.Count) a.Append(" -ReferenzDatei ").Append(Q(refItems[cmbRef.SelectedIndex - 4].Path));
            }
            List<string> cmp = new List<string>();
            if (clbCompare.Enabled) for (int i = 0; i < clbCompare.Items.Count && i < cmpItems.Count; i++) if (clbCompare.GetItemChecked(i)) cmp.Add(cmpItems[i].Path);
            if (cmp.Count > 0) a.Append(" -VergleichDateien ").Append(Q(String.Join(";", cmp.ToArray())));
        }
        if (nav[3].Checked)
        {
            if (!chkLCpu.Checked && !chkLRam.Checked && !chkLGpu.Checked && !chkLDisk.Checked) { MessageBox.Show(this, "Im Lasttest ist keine Komponente ausgewählt.", "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Information); ShowPage(3); return; }
            if (chkLCpu.Checked) a.Append(" -LastCpuMinuten ").Append(Minutes(cmbLCpu));
            if (chkLRam.Checked) a.Append(" -LastRamMinuten ").Append(Minutes(cmbLRam)).Append(" -LastRamProzent ").Append(Convert.ToString(cmbLRamPct.SelectedItem).Split(' ')[0]);
            if (chkLGpu.Checked) a.Append(" -LastGpuMinuten ").Append(Minutes(cmbLGpu));
            if (chkLDisk.Checked) a.Append(" -LastDiskMinuten ").Append(Minutes(cmbLDisk)).Append(" -LastDiskLaufwerk ").Append(Convert.ToString(cmbLDiskDrive.SelectedItem).Substring(0, 1));
            a.Append(" -LastAbbruchCpu ").Append(AbortArg(cmbLAbortCpu)).Append(" -LastAbbruchGpu ").Append(AbortArg(cmbLAbortGpu));
        }
        if (nav[4].Checked)
        {
            List<string> r = new List<string>();
            for (int i = 0; i < repChk.Length; i++) if (repChk[i].Checked) { r.Add(repKeys[i]); punkte.Add(new StartPunkt("Wartung", SplitDetail(repText[i])[0], i < repRisk.Length && repRisk[i].Length > 0 ? repRisk[i] : "Eingriff", i < repRestart.Length ? repRestart[i] : "")); }
            if (r.Count == 0) { MessageBox.Show(this, "Im Wartungsmodul ist nichts ausgewählt.", "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Information); ShowPage(4); return; }
            wp.Add("Wartung|" + (chkRestorePoint.Checked ? "1" : "0")); if (!chkRestorePoint.Checked) ohneWp = true;
            a.Append(" -Wartung ").Append(String.Join(",", r.ToArray()));
            if (!chkRestorePoint.Checked) a.Append(" -OhneWiederherstellungspunkt");
        }
        if (nav[5].Checked)
        {
            string optArgs = OptStartArgs(punkte, zusatz); if (optArgs == null) return;
            wp.Add("Optimierung|" + (chkOptRestore.Checked ? "1" : "0")); if (!chkOptRestore.Checked) ohneWp = true;
            a.Append(optArgs);
        }
        if (nav[1].Checked && chkMem.Enabled && chkMem.Checked) punkte.Add(new StartPunkt("Diagnose", "Windows-Speicherdiagnose beim nächsten Neustart", "Aendern", "immer"));
        // ab v3.64 eine Zusammenfassung vor dem Start, nur bei Eingriffen, Neustarts oder abgewähltem Wiederherstellungspunkt
        if ((StartUebersicht.Noetig(punkte, ohneWp) || zusatz.Count > 0) && !ConfirmStart(StartUebersicht.Text(mods, "ca. " + MinText(EstimateMinutes()), punkte, wp, zusatz), punkte)) return;
        if (nav[1].Checked && chkFast != null && chkFast.Enabled && chkFast.Checked) a.Append(" -SchnellerModus");
        bool gpuBench = nav[2].Checked && benchChk[2].Checked, gpuLoad = nav[3].Checked && chkLGpu.Checked;
        if (gpuBench || gpuLoad)
        {
            ComboBox res = gpuLoad ? cmbLGpuRes : cmbGpuRes, show = gpuLoad ? cmbLGpuShow : cmbGpuShow;
            a.Append(" -GpuAufloesung ").Append(Convert.ToString(res.SelectedItem));
            a.Append(" -GpuAnzeige ").Append(show.SelectedIndex == 1 ? "Vollbild" : show.SelectedIndex == 2 ? "Aus" : "Fenster");
            string gsel = GpuChoiceValue(gpuLoad ? cmbLGpuSel : cmbGpuSel);
            if (gsel != "Alle") a.Append(" -GpuAuswahl ").Append(Q(gsel));
        }
        // ab v2.66 ohne Rückfrage vor dem Grafik-Lasttest (Wunsch aus dem Praxistest); die Abbruchschwelle wirkt weiter, Esc im Vorschaufenster beendet
        if (drv) a.Append(" -SensorTreiber");
        a.Append(KeepArg());
        a.Append(" -PsuWattage ").Append(psuWattage);
        if (!string.IsNullOrEmpty(psuRating)) a.Append(" -PsuRating ").Append(Q(psuRating));
        Launch(a.ToString(), String.Join(" + ", mods.ToArray()));
    }

    // "automatisch ..." = auto, "aus" = aus, sonst die Zahl vor dem Gradzeichen
    static string AbortArg(ComboBox c)
    {
        string s = Convert.ToString(c.SelectedItem);
        if (s.StartsWith("automatisch", StringComparison.OrdinalIgnoreCase)) return "auto";
        if (s == "aus") return "aus";
        return s.Split(' ')[0];
    }

    // ------------------------------------------------------------ Protokoll eines Laufs ohne Bericht (ab v2.65)
    // Endet ein Lauf ohne Bericht (Abbruch, Fehler, Hänger), blieb der Berichtsordner bisher leer und die Rohdaten lagen
    // nur im lokalen TEMP (bei Adminrechten in dem des Admin-Kontos). Jetzt kommen Oberflächenprotokoll, Startargumente,
    // Konsole.log und Checkpoint.log des Arbeitsprozesses in den Berichtsordner.
    string runOutDir = "", runWorkDir = "", runArgs = "", lastStepText = "";
    DateTime runStarted;

    string SaveFailureLog(int code)
    {
        try
        {
            if (runOutDir.Length == 0 || !Directory.Exists(runOutDir)) return "";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("LEOS MINIBENCH " + version + ": LAUF OHNE BERICHT");
            sb.AppendLine("Computer   : " + Environment.MachineName + ", Benutzer " + Environment.UserName);
            sb.AppendLine("Gestartet  : " + runStarted.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            sb.AppendLine("Beendet    : " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture) + (cancelled ? " (abgebrochen)" : ", Rückgabecode " + code));
            sb.AppendLine("Argumente  : " + runArgs);
            sb.AppendLine("Arbeitsordner: " + runWorkDir);
            sb.AppendLine("Letzter Schritt: " + lastStepText);
            sb.AppendLine();
            sb.AppendLine("--- Protokoll der Oberfläche ---");
            sb.AppendLine(txtLog.Text);
            string f = Path.Combine(runOutDir, "Protokoll_ohne_Bericht.txt");
            File.WriteAllText(f, sb.ToString(), new UTF8Encoding(true));
            if (runWorkDir.Length > 0)
            {
                foreach (string n in new string[] { "Konsole.log", "Checkpoint.log" })
                {
                    string src = Path.Combine(Path.Combine(runWorkDir, "Anhang"), n);
                    try { if (File.Exists(src)) using (FileStream i = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) using (FileStream o = File.Create(Path.Combine(runOutDir, n))) i.CopyTo(o); } catch { }
                }
            }
            return f;
        }
        catch { return ""; }
    }
}
