// Seite Benchmark, Referenz und Grafikeinheiten (Teilklasse DiagGui)
public partial class DiagGui
{
    // ab v3.64: Messungen mit kurzer Bezeichnung und Detailzeile, Laufwerke mit Ziel und freiem Platz, Referenz-Zustand neben der Liste
    static readonly string[] benchDetail = new string[] {
        "Einzel- und Mehrkern, AES-256, SHA-256, Kompression",
        "Lesen, Schreiben, Kopieren, Latenz",
        "Rendertest je Grafikeinheit, Speicherdurchsatz, Desktop-Komposition, PCIe",
        "Lesen und Schreiben am Stück, 4K-Zufallszugriff" };
    static readonly string[] benchIcons = new string[] { UI.IcoCpu, UI.IcoRam, UI.IcoGpu, UI.IcoDisk };

    // ab v4.08: Seite im Muster von Wartung und Tools. Links die Karte Messungen mit je einer Zeile (Symbol, Titel,
    // Beschreibung, Dauer, Schalter); die Einstellungen einer Messung stehen unter ihrer Zeile und nur, wenn sie
    // eingeschaltet ist. Rechts Referenz und letztes Ergebnis. Die Kontrollkästchen benchChk bleiben als Werte bestehen
    // (Vorlagen, Zeitschätzung, Start), angezeigt werden Schalter.
    internal ModulSeite benchSeite;
    internal SettingRow[] benchRows;
    Badge[] benchDauer;
    internal UnterBereich subGpu, subDisk;
    internal List<LaufwerkZeile> diskZeilen = new List<LaufwerkZeile>();
    // Sichtbarkeit der Bereiche unter Grafik und Laufwerken als eigener Zustand (für Tests ohne angezeigtes Fenster)
    internal bool subGpuOffen, subDiskOffen;
    LinkLabel lnkBenchAll, lnkBenchNone, lnkRefDb, lnkLetztes, lnkBandErg;
    // Band "Zurück zum Ergebnis": erscheint, wenn die Seite über "Referenz ändern" der Ergebnisseite geöffnet wurde
    internal Panel bandErg;
    Label lblBandErg;
    ZeilenKarte karteLetztes;
    Label lblLetztScore, lblLetztDatum;
    FlowLayoutPanel pnlLetztWerte;

    Control BuildBenchPage()
    {
        FlowLayoutPanel f = Page("Benchmark", "Misst die Leistung der gewählten Komponenten und vergleicht sie mit der Referenz und mit früheren Läufen.", 2);
        benchSeite = NeueModulSeite(f);
        bandErg = new Panel(); bandErg.BackColor = UI.AccentSoft; bandErg.Height = UI.S(34); bandErg.Margin = new Padding(0, 0, 0, UI.S(UI.Abstand3)); bandErg.Visible = false;
        lnkBandErg = new LinkLabel(); lnkBandErg.Text = "\u2190  Zurück zum Ergebnis"; lnkBandErg.AutoSize = true; lnkBandErg.Font = new Font("Segoe UI Semibold", UI.SchriftText);
        lnkBandErg.LinkColor = UI.Accent; lnkBandErg.ActiveLinkColor = UI.AccentHover; lnkBandErg.LinkBehavior = LinkBehavior.HoverUnderline; lnkBandErg.BackColor = Color.Transparent; lnkBandErg.Location = new Point(UI.S(12), UI.S(8));
        lnkBandErg.LinkClicked += delegate { ZeigeLetztesErgebnis(); };
        Tip(lnkBandErg, "Kehrt zur Ergebnisseite zurück. Die dort gezeigten Prozentwerte gehören zur Referenz des Laufs; eine andere Referenz gilt ab dem nächsten Lauf.");
        lblBandErg = Lbl("", UI.SchriftText, false, UI.Text); lblBandErg.Location = new Point(UI.S(190), UI.S(8));
        bandErg.Controls.Add(lnkBandErg); bandErg.Controls.Add(lblBandErg);
        lnkBandErg.SizeChanged += delegate { lblBandErg.Left = lnkBandErg.Right + UI.S(14); };
        f.Controls.Add(bandErg); f.Controls.SetChildIndex(bandErg, 0); benchSeite.Voll.Add(bandErg);

        ZeilenKarte km = benchSeite.Karte("Messungen", true);
        lnkBenchAll = KartenVerweis("Alle an"); lnkBenchNone = KartenVerweis("Alle aus");
        km.KopfRechts.Controls.Add(lnkBenchAll); km.KopfRechts.Controls.Add(lnkBenchNone);
        lnkBenchAll.LinkClicked += delegate { foreach (CheckBox c in benchChk) c.Checked = true; };
        lnkBenchNone.LinkClicked += delegate { foreach (CheckBox c in benchChk) c.Checked = false; };

        benchChk = new CheckBox[benchKeys.Length]; benchRows = new SettingRow[benchKeys.Length]; benchDauer = new Badge[benchKeys.Length];
        for (int i = 0; i < benchKeys.Length; i++)
        {
            benchChk[i] = Chk(benchText[i], true);
            string t; benchTips.TryGetValue(benchKeys[i], out t);
            benchRows[i] = SchalterZeile(benchChk[i], benchText[i], benchDetail[i], benchIcons[i], t, null);
            benchDauer[i] = DauerEtikett(BenchMinuten(i)); BadgeTip(benchDauer[i]); benchRows[i].AddBadge(benchDauer[i]);
            benchChk[i].CheckedChanged += delegate { UpdateSummary(); };
            km.Zeile(benchRows[i]);
            if (i == 2)
            {
                // Rendertest: Grafikeinheit, Auflösung und Anzeige nebeneinander (gleich eingestellt wie im Lasttest)
                subGpu = new UnterBereich(); subGpuOffen = true;
                FeldReihe fr = new FeldReihe();
                cmbGpuSel = Combo(230, GpuChoiceTexts(), 0); cmbGpuRes = Combo(120, new string[] { "1280x720", "1920x1080" }, 0);
                cmbGpuShow = Combo(140, new string[] { "Fenster", "Vollbild", "ohne Anzeige" }, 0);
                fr.Feld("Grafikeinheit", cmbGpuSel); fr.Feld("Auflösung", cmbGpuRes); fr.Feld("Anzeige", cmbGpuShow);
                subGpu.Inhalt.Controls.Add(fr);
                Label hg = Lbl("Ergebnis je Grafikeinheit: Ø Bilder/s, 1-%-Low und Punktzahl. Esc im Fenster beendet den Test.", 8.5f, false, UI.Muted); hg.Margin = new Padding(0, UI.S(2), 0, 0);
                subGpu.Inhalt.Controls.Add(hg);
                km.Zeile(subGpu);
            }
            else if (i == 3)
            {
                subDisk = new UnterBereich(); subDiskOffen = true;
                foreach (string d in disks)
                {
                    string[] x = d.Split('|');
                    if (x.Length < 7) continue;
                    LaufwerkZeile z = new LaufwerkZeile(x[1], x[2], DiskFrei(x), x[6] != "1" && x[5].Length > 0);
                    Tip(z, DiskLine(x));
                    z.CheckedChanged += delegate { UpdateSummary(); };
                    diskZeilen.Add(z); diskNums.Add(x[0]);
                    subDisk.Inhalt.Controls.Add(z);
                }
                Label hd = Lbl("Testdatei 1 GB je Laufwerk auf dem Volume mit dem meisten freien Platz, wird danach gelöscht. USB-Datenträger sind zunächst abgewählt.", 8.5f, false, UI.Muted); hd.Margin = new Padding(0, UI.S(2), 0, 0);
                subDisk.Inhalt.Controls.Add(hd);
                km.Zeile(subDisk);
            }
        }

        // Referenz: bestimmt alle Prozentwerte der Ergebnisseite
        ZeilenKarte kr = benchSeite.Karte("Referenz (100 %)", false);
        cmbRef = Combo(300, new string[0], 0); cmbRef.Margin = new Padding(0, 0, 0, UI.S(6)); kr.Zeile(cmbRef);
        lblRefState = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblRefState.Margin = new Padding(0, 0, 0, UI.S(8)); kr.Frei(lblRefState);
        btnUpdateMed = UI.Secondary("Median aktualisieren"); btnUpdateMed.Margin = new Padding(0, 0, 0, UI.S(8));
        btnUpdateMed.Click += delegate {
            if (running) return;
            // ab v4.06: Ergebnis aus @@RESULT und den Zeilen des Arbeitsprozesses. Bis v4.05 suchte die Oberfläche eine eigene
            // Statuszeile, die nie gesendet wurde, und meldete immer "Aktualisierung fehlgeschlagen", auch wenn die
            // Dateien geschrieben waren.
            List<string> lines;
            string res = RunHelperPumped("-MedianAktualisieren", out lines, 180);
            MessageBox.Show(this, MedianErgebnisText(res, lines), "Median-Referenz", MessageBoxButtons.OK, res.Length > 0 && !res.StartsWith("Keine") ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            FillRefLists(); UpdateRefState();
        };
        kr.Frei(btnUpdateMed);
        cmbRef.SelectedIndexChanged += delegate { UpdateRefState(); };
        // ab v4.04 ohne Haken "Dieses System als Referenz festlegen": Das geht in der Datenbank über Aktionen, wo der Lauf
        // mit seinen Werten sichtbar ist (Skriptparameter -ReferenzSpeichern bleibt für Aufrufe ohne Oberfläche)
        lnkRefDb = KartenVerweis("Eigene Referenz in der Datenbank festlegen"); lnkRefDb.Margin = new Padding(0, 0, 0, UI.S(2));
        lnkRefDb.LinkClicked += delegate { if (!running) ShowPage(9); };
        Tip(lnkRefDb, "Öffnet die Seite Datenbank. Dort einen Lauf markieren, dann Aktionen › Als Referenz festlegen: Dieser Lauf ist danach die eigene Referenz (Referenz.json).");
        kr.Frei(lnkRefDb);

        // Letztes Ergebnis dieses PCs aus der Datenbank (nur, wenn es einen Lauf gibt)
        karteLetztes = benchSeite.Karte("Letztes Ergebnis auf diesem PC", false);
        lblLetztDatum = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblLetztDatum.Margin = new Padding(0, UI.S(2), 0, 0);
        karteLetztes.KopfRechts.Controls.Add(lblLetztDatum);
        lblLetztScore = Lbl("", 17f, true, UI.Accent); lblLetztScore.Tag = "accent"; lblLetztScore.Margin = new Padding(0, 0, 0, UI.S(4)); karteLetztes.Frei(lblLetztScore);
        pnlLetztWerte = new FlowLayoutPanel(); pnlLetztWerte.FlowDirection = FlowDirection.TopDown; pnlLetztWerte.WrapContents = false; pnlLetztWerte.AutoSize = true; pnlLetztWerte.BackColor = UI.Panel; pnlLetztWerte.Margin = new Padding(0);
        karteLetztes.Frei(pnlLetztWerte);
        lnkLetztes = KartenVerweis("Ergebnis ansehen"); lnkLetztes.Margin = new Padding(0, UI.S(6), 0, UI.S(2));
        lnkLetztes.LinkClicked += delegate { ZeigeLetztesErgebnis(); };
        Tip(lnkLetztes, "Öffnet die Ergebnisseite mit dem letzten Lauf dieses PCs aus der Datenbank.");
        karteLetztes.Frei(lnkLetztes);

        f.Controls.Add(benchSeite.Spalten);
        clbCompare = new CheckedListBox(); clbCompare.BackColor = UI.Panel; clbCompare.ForeColor = UI.Text;
        FillRefLists();
        FuelleLetztesErgebnis();
        UpdateBenchSummary();
        benchSeite.Ordne(SeitenBreite());
        return f;
    }

    // Geschätzte Minuten einer Messung (Index wie benchKeys); Laufwerke: eine Minute je gewähltem Datenträger
    int BenchMinuten(int i) { return i == 0 ? 2 : i == 1 ? 1 : i == 2 ? 2 : Math.Max(1, DiskGewaehlt()); }

    int DiskGewaehlt() { int n = 0; foreach (LaufwerkZeile z in diskZeilen) if (z.Checked) n++; return n; }

    // Kopf der Seite, Zeile Laufwerke und die Bereiche unter Grafik und Laufwerken nach der Auswahl
    void UpdateBenchSummary()
    {
        if (benchSeite == null || benchChk == null || benchRows == null) return;
        int n = 0; foreach (CheckBox c in benchChk) if (c.Checked) n++;
        SetText(benchSeite.Anzahl, n + " von " + benchChk.Length + " Messungen");
        SetText(benchSeite.Dauer, n > 0 ? "ca. " + MinText(ModuleMinutes(1)) : "keine Messung gewählt");
        bool gpu = benchChk[2].Checked, disk = benchChk[3].Checked;
        if (subGpu != null && subGpuOffen != gpu) { subGpuOffen = gpu; subGpu.Visible = gpu; }
        bool dz = disk && diskZeilen.Count > 0;
        if (subDisk != null && subDiskOffen != dz) { subDiskOffen = dz; subDisk.Visible = dz; }
        benchRows[3].Description = benchDetail[3] + (diskZeilen.Count > 0 ? "  ·  " + DiskGewaehlt() + " von " + diskZeilen.Count + " gewählt" : "  ·  kein Datenträger gefunden");
        if (benchDauer != null && benchDauer[3] != null) SetText(benchDauer[3], MinText(BenchMinuten(3)));
    }

    // Karte "Letztes Ergebnis auf diesem PC": Punktzahl, Datum und Prozent je Gruppe, soweit gespeichert
    void FuelleLetztesErgebnis()
    {
        if (karteLetztes == null) return;
        DbEntry e = GetCurrentPcLastRun();
        bool da = e != null && (e.OverallScore > 0 || e.GruppenPct.Count > 0);
        karteLetztes.Karte.Visible = da;
        if (!da) return;
        System.Globalization.CultureInfo de = System.Globalization.CultureInfo.GetCultureInfo("de-DE");
        SetText(lblLetztDatum, DatumKurz(e.Datum));
        SetText(lblLetztScore, e.OverallScore > 0 ? ((int)e.OverallScore).ToString("N0", de) + " Punkte" : "ohne Punktzahl");
        pnlLetztWerte.Controls.Clear();
        for (int g = 0; g < resGruppen.Length; g++)
        {
            double pct; if (!e.GruppenPct.TryGetValue(resGruppen[g], out pct)) continue;
            FlowLayoutPanel r = new FlowLayoutPanel(); r.AutoSize = true; r.WrapContents = false; r.BackColor = UI.Panel; r.Margin = new Padding(0, 0, 0, UI.S(4));
            Label ln = Lbl(resNamen[g], UI.SchriftHinweis, false, UI.Text); ln.AutoSize = false; ln.Size = new Size(UI.S(112), UI.S(18)); ln.Margin = new Padding(0);
            PercentBar pb = new PercentBar(); pb.Percent = pct; pb.Size = new Size(UI.S(110), UI.S(8)); pb.Margin = new Padding(0, UI.S(6), UI.S(8), 0);
            Tip(pb, "Strich = Referenz (100 %)" + (e.RefName.Length > 0 ? ": " + e.RefName : "") + ".");
            Label lv = Lbl(pct.ToString("0") + " %", UI.SchriftHinweis, true, UI.Text); lv.Margin = new Padding(0);
            r.Controls.Add(ln); r.Controls.Add(pb); r.Controls.Add(lv);
            pnlLetztWerte.Controls.Add(r);
        }
        if (pnlLetztWerte.Controls.Count == 0 && e.HasBench)
        {
            Label l = Lbl("Prozentwerte je Gruppe sind bei diesem Lauf nicht gespeichert (Lauf vor Version 4.08).", UI.SchriftHinweis, false, UI.Muted); l.Margin = new Padding(0);
            l.MaximumSize = new Size(karteLetztes.Innenbreite, 0);
            pnlLetztWerte.Controls.Add(l);
        }
        karteLetztes.Breite(karteLetztes.Karte.Width);
    }

    // "2026-11-10 14:20" -> "10.11.2026 14:20"; unbekannte Formen bleiben
    public static string DatumKurz(string datum)
    {
        string d = (datum ?? "").Trim(); DateTime x;
        foreach (string fmt in new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd" })
            if (DateTime.TryParseExact(d, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out x)) return x.ToString(fmt.Length > 10 ? "dd.MM.yyyy HH:mm" : "dd.MM.yyyy", CultureInfo.InvariantCulture);
        return d;
    }

    // Freier Platz des Volumes, auf dem gemessen wird ("C: 300 GB frei"); ohne Laufwerksbuchstabe steht das da
    static string DiskFrei(string[] x)
    {
        if (x[5].Length == 0) return "kein Laufwerksbuchstabe";
        string best = ""; long bestFree = -1; string erster = "";
        foreach (string l in x[5].Split(','))
        {
            string t = l.Trim(); if (t.Length == 0) continue;
            if (erster.Length == 0) erster = t;
            long free = -1;
            // USB-Datenträger nicht abfragen (ein schlafendes Laufwerk verzögerte sonst den Start)
            if (x.Length < 7 || x[6] != "1") try { DriveInfo di = new DriveInfo(t.Substring(0, 1)); if (di.IsReady) free = di.AvailableFreeSpace; } catch { }
            if (free > bestFree) { bestFree = free; best = t; }
        }
        if (bestFree >= 0) return best + " " + GbText(bestFree) + " frei";
        return erster + (x[4].Length > 0 ? "  ·  " + x[4] : "");
    }

    // Laufwerkszeile: Nummer, Name, Bus, Medium, Größe und je Laufwerksbuchstabe der freie Platz; das Ziel der Messung
    // ist das Volume mit dem meisten freien Platz
    static string DiskLine(string[] x)
    {
        string head = String.Format("Datenträger {0}:  {1}   ({2}, {3}, {4})", x[0], x[1], x[2], x[3], x[4]);
        if (x[5].Length == 0) return head + "   ·   kein Laufwerksbuchstabe";
        List<string> vols = new List<string>(); string best = ""; long bestFree = -1;
        foreach (string l in x[5].Split(','))
        {
            string t = l.Trim(); if (t.Length == 0) continue;
            long free = -1;
            // USB-Datenträger nicht abfragen (ein schlafendes Laufwerk verzögerte sonst den Start)
            if (x.Length < 7 || x[6] != "1") try { DriveInfo di = new DriveInfo(t.Substring(0, 1)); if (di.IsReady) free = di.AvailableFreeSpace; } catch { }
            vols.Add(free >= 0 ? t + " " + GbText(free) + " frei" : t);
            if (free > bestFree) { bestFree = free; best = t; }
        }
        return head + "   ·   " + String.Join(", ", vols.ToArray()) + (vols.Count > 1 && best.Length > 0 ? "   ·   gemessen auf " + best : "");
    }

    static string GbText(long bytes)
    {
        double gb = bytes / 1073741824.0;
        return gb >= 1024 ? (gb / 1024).ToString("0.0", CultureInfo.GetCultureInfo("de-DE")) + " TB" : gb.ToString("0", CultureInfo.GetCultureInfo("de-DE")) + " GB";
    }

    public static string MedianErgebnisText(string result, List<string> lines)
    {
        StringBuilder b = new StringBuilder();
        b.Append(String.IsNullOrEmpty(result) ? "Der Arbeitsprozess hat kein Ergebnis gemeldet; die Median-Referenzen sind unverändert." : result + ".");
        if (lines != null && lines.Count > 0) { b.Append("\r\n\r\n"); b.Append(String.Join("\r\n", lines.ToArray())); }
        return b.ToString();
    }

    // Name, Datum und Rechner einer Referenzdatei im Datenordner; leer, wenn sie fehlt oder unlesbar ist
    string[] RefDatei(string name)
    {
        try
        {
            string f = dataDir.Length > 0 ? Path.Combine(dataDir, name) : "";
            if (f.Length == 0 || !File.Exists(f)) return null;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(f, Encoding.UTF8)) as Dictionary<string, object>;
            if (d == null) return null;
            object n, dt, c; string dat = d.TryGetValue("Datum", out dt) ? Convert.ToString(dt) : "";
            DateTime x; if (DateTime.TryParseExact(dat.Length >= 10 ? dat.Substring(0, 10) : dat, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out x)) dat = x.ToString("dd.MM.yyyy");
            return new string[] { d.TryGetValue("Name", out n) ? Convert.ToString(n) : name, dat, d.TryGetValue("Computer", out c) ? Convert.ToString(c) : "" };
        }
        catch { return null; }
    }

    void UpdateRefState()
    {
        if (lblRefState == null || cmbRef == null) return;
        int i = cmbRef.SelectedIndex;
        string t; bool warn = false;
        if (i == 0)
        {
            string st = SavedRefText();
            warn = st.Length == 0;
            t = warn ? "Keine Referenz festgelegt: Im Datenordner liegt weder eine eigene Referenz noch ein Median. Prozentwerte beziehen sich auf das eingebaute Referenzprofil." : st;
        }
        else if (i >= 1 && i <= 3)
        {
            // ab v4.06: zeigt, welcher Median gilt und von wann (vorher nur die Anzahl der Läufe in der Datenbank)
            string datei = i == 1 ? "Referenz_Median.json" : i == 2 ? "Referenz_Notebook.json" : "Referenz_Desktop.json";
            string[] r = RefDatei(datei);
            int n = db.FindAll(delegate(DbEntry e) { return e.HasBench; }).Count;
            t = r != null ? r[0] + (r[1].Length > 0 ? ", berechnet am " + r[1] : "") + " (" + datei + "). \u201EMedian aktualisieren\u201C berechnet ihn aus den " + n + " Läufen mit Benchmark neu."
                          : "Noch nicht gespeichert: Der Lauf bildet den Median aus der Datenbank (" + n + " Läufe mit Benchmark). \u201EMedian aktualisieren\u201C speichert ihn als " + datei + ".";
        }
        else t = "Einzelner gespeicherter Lauf als Bezug.";
        SetText(lblRefState, t);
        lblRefState.ForeColor = warn ? UI.Warn : UI.Muted;
    }

    void FillRefLists()
    {
        int prevSpecial = -1;
        string prevRef = "";
        if (cmbRef != null)
        {
            if (cmbRef.SelectedIndex >= 0 && cmbRef.SelectedIndex <= 3) prevSpecial = cmbRef.SelectedIndex;
            else if (cmbRef.SelectedIndex >= 4 && cmbRef.SelectedIndex - 4 < refItems.Count) prevRef = refItems[cmbRef.SelectedIndex - 4].Path;
            cmbRef.Items.Clear();
            cmbRef.Items.Add("Gespeicherte Referenz");
            cmbRef.Items.Add("Median aller Systeme");
            cmbRef.Items.Add("Median der Notebooks");
            cmbRef.Items.Add("Median der Desktop-PCs");
        }
        refItems.Clear();
        HashSet<string> prevCmp = new HashSet<string>();
        if (clbCompare != null)
        {
            for (int i = 0; i < clbCompare.Items.Count; i++) if (clbCompare.GetItemChecked(i) && i < cmpItems.Count) prevCmp.Add(cmpItems[i].Path);
            clbCompare.Items.Clear(); cmpItems.Clear();
        }
        foreach (DbEntry e in db)
        {
            if (!e.HasBench) continue;
            refItems.Add(e);
            if (cmbRef != null) cmbRef.Items.Add(e.Label);
            if (clbCompare != null) { cmpItems.Add(e); clbCompare.Items.Add(e.Label, prevCmp.Contains(e.Path)); }
        }
        if (cmbRef != null)
        {
            int sel = 0;
            if (prevSpecial >= 0 && prevSpecial < cmbRef.Items.Count) sel = prevSpecial;
            else if (prevRef.Length > 0)
            {
                int idx = refItems.FindIndex(delegate(DbEntry x) { return String.Equals(x.Path, prevRef, StringComparison.OrdinalIgnoreCase); });
                if (idx >= 0) sel = idx + 4;
            }
            if (sel >= 0 && sel < cmbRef.Items.Count) cmbRef.SelectedIndex = sel;
            UpdateRefState();
        }
        if (clbCompare != null)
        {
            if (clbCompare.Items.Count == 0) { clbCompare.Items.Add("(noch keine Systeme mit Benchmark in der Datenbank)"); clbCompare.Enabled = false; }
            else clbCompare.Enabled = true;
            clbCompare.Height = Math.Max(UI.S(48), Math.Min(8, clbCompare.Items.Count) * UI.S(21) + UI.S(6));
        }
    }


    // ------------------------------------------------------------ Grafikeinheiten (ab v2.65)
    static string GpuKind(string n)
    {
        n = (n ?? "").Trim();
        if (n.Length == 0) return "";
        if (System.Text.RegularExpressions.Regex.IsMatch(n, "Virtual|Remote|Indirect|Parsec|spacedesk|IddSampleDriver|Basic Display|Basic Render|Standard-VGA|Hyper-V|VMware|VirtualBox|Citrix|DisplayLink", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return "virtuell";
        if (System.Text.RegularExpressions.Regex.IsMatch(n, "NVIDIA|GeForce|Quadro|Tesla|\\bRTX\\b|\\bGTX\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return "dGPU";
        if (n.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0) return System.Text.RegularExpressions.Regex.IsMatch(n, "Arc(\\(TM\\))?\\s+(Pro\\s+)?[AB]\\d{2,3}", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ? "dGPU" : "iGPU";
        if (System.Text.RegularExpressions.Regex.IsMatch(n, "AMD|ATI|Radeon|FirePro", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return System.Text.RegularExpressions.Regex.IsMatch(n, "Graphics\\s*$|\\b\\d{3}M\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ? "iGPU" : "dGPU";
        return "";
    }

    // Einträge der Auswahl: alle, nur Grafikkarte, nur Prozessorgrafik (wenn vorhanden), dann jede Einheit einzeln
    public static string GetPrimaryGpuName()
    {
        if (GpuNames == null || GpuNames.Length == 0) return "";
        string dGpu = "";
        string iGpu = "";
        string other = "";
        foreach (string g in GpuNames)
        {
            string n = (g ?? "").Trim();
            if (n.Length == 0) continue;
            string kind = GpuKind(n);
            if (kind == "virtuell") continue;
            if (kind == "dGPU" && dGpu.Length == 0) dGpu = n;
            else if (kind == "iGPU" && iGpu.Length == 0) iGpu = n;
            else if (other.Length == 0) other = n;
        }
        if (dGpu.Length > 0) return dGpu;
        if (iGpu.Length > 0) return iGpu;
        if (other.Length > 0) return other;
        foreach (string g in GpuNames)
        {
            string n = (g ?? "").Trim();
            if (n.Length > 0) return n;
        }
        return "";
    }

    string[] GpuChoiceTexts()
    {
        List<string> t = new List<string>(); gpuSelValues.Clear();
        t.Add("alle Grafikeinheiten"); gpuSelValues.Add("Alle");
        List<string> names = new List<string>();
        foreach (string g in GpuNames ?? new string[0]) { string n = (g ?? "").Trim(); if (n.Length > 0 && GpuKind(n) != "virtuell" && !names.Contains(n)) names.Add(n); }
        bool dg = names.Exists(delegate(string n) { return GpuKind(n) == "dGPU"; }), ig = names.Exists(delegate(string n) { return GpuKind(n) == "iGPU"; });
        if (dg && ig) { t.Add("nur Grafikkarte"); gpuSelValues.Add("Grafikkarte"); t.Add("nur Prozessorgrafik"); gpuSelValues.Add("Prozessorgrafik"); }
        if (names.Count > 1) foreach (string n in names) { t.Add("nur " + n); gpuSelValues.Add("Name:" + n); }
        return t.ToArray();
    }

    string GpuChoiceValue(ComboBox c)
    {
        if (c == null || c.SelectedIndex < 0 || c.SelectedIndex >= gpuSelValues.Count) return "Alle";
        return gpuSelValues[c.SelectedIndex];
    }

    void SetGpuChoice(string v)
    {
        int i = gpuSelValues.IndexOf(v ?? "");
        if (i < 0 && (v ?? "").StartsWith("Name:")) i = gpuSelValues.FindIndex(delegate(string x) { return x.StartsWith("Name:") && x.Substring(5).IndexOf(v.Substring(5), StringComparison.OrdinalIgnoreCase) >= 0; });
        if (i < 0) i = 0;
        if (cmbGpuSel != null && i < cmbGpuSel.Items.Count) cmbGpuSel.SelectedIndex = i;
        if (cmbLGpuSel != null && i < cmbLGpuSel.Items.Count) cmbLGpuSel.SelectedIndex = i;
    }

    // Referenz (ab v2.65 nur noch die mit "Dieses System als Referenz festlegen" gespeicherte, keine eingebauten Werte)
    // Text zur gespeicherten Referenz in derselben Reihenfolge wie Get-SavedReference (ab v4.06): eigene Referenz
    // (Referenz.json, kein alter Median), sonst der Median der Geräteklasse, sonst der andere, sonst die alte Ablage
    string SavedRefText()
    {
        string[] r = RefDatei("Referenz.json");
        if (r != null && r[2] != "Median") return "Eigene Referenz: " + r[0] + (r[1].Length > 0 ? " vom " + r[1] : "") + " (Referenz.json).";
        string dev = HwIsLaptop ? "Referenz_Notebook.json" : "Referenz_Desktop.json", alt = HwIsLaptop ? "Referenz_Desktop.json" : "Referenz_Notebook.json";
        foreach (string f in new string[] { dev, alt, "PC-Diagnose-Referenz.json" })
        {
            string[] m = RefDatei(f);
            if (m != null) return "Keine eigene Referenz festgelegt, es gilt " + m[0] + (m[1].Length > 0 ? " vom " + m[1] : "") + " (" + f + ").";
        }
        return "";
    }
}
