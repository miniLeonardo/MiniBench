// Ergebnisseite nach einem Lauf (Teilklasse DiagGui)
public partial class DiagGui
{
    // ------------------------------------------------------------ Ergebnisseite nach einem Lauf (ab v3.9, neu in v4.01)
    // Oben das Urteil mit den wichtigsten Befunden und der Vergleich mit der Referenz, darunter je Komponente ein
    // Hauptwert mit Einordnung. Gezeigt wird nur, was dieser Lauf gemessen oder gemeldet hat; Details stehen im Bericht.
    Label lblResVerdict, lblResCounts, lblResScore, lblResScoreSub, lblResRef;
    LinkLabel lnkResRef;
    FlowLayoutPanel pnlResFind, pnlResCmp, pnlResTiles;
    TableLayoutPanel tblResMid;
    CardPanel cardResFind, cardResCmp;
    LinkLabel lnkResAll;
    // ab v4.07: Balken und Ringe, die beim Einblenden der Seite auf ihren Wert laufen, und die Punktzahl als Zahl
    List<PercentBar> resBalken = new List<PercentBar>();
    List<RingAnzeige> resRinge = new List<RingAnzeige>();
    int resScore;

    static readonly string[] resGruppen = new string[] { "CPU", "GPU", "RAM", "Laufwerke" };
    static readonly string[] resNamen = new string[] { "Prozessor", "Grafik", "Arbeitsspeicher", "Laufwerke" };

    Control BuildResultDashboard()
    {
        Panel p = new Panel();
        p.Dock = DockStyle.Fill; p.AutoScroll = true; p.BackColor = UI.Bg;
        p.Padding = new Padding(UI.S(UI.Seitenrand), UI.S(UI.Seitenrand), UI.S(UI.Seitenrand), UI.S(20));

        TableLayoutPanel main = new TableLayoutPanel();
        main.Dock = DockStyle.Top; main.AutoSize = true; main.ColumnCount = 1; main.BackColor = UI.Bg;
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        lblDashTitle = Lbl("Ergebnis", UI.SchriftTitel, true, UI.Text); lblDashTitle.Margin = new Padding(0, 0, 0, UI.S(2));
        main.Controls.Add(lblDashTitle);
        lblDashMeta = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblDashMeta.Margin = new Padding(0, 0, 0, UI.S(12));
        main.Controls.Add(lblDashMeta);

        // 1. Urteil links, Punktzahl rechts, darunter die Hardware
        CardPanel hero = new CardPanel(); hero.Dock = DockStyle.Top; hero.AutoSize = true; hero.Padding = new Padding(UI.S(20), UI.S(16), UI.S(20), UI.S(16)); hero.Margin = new Padding(0, 0, 0, UI.S(12));
        TableLayoutPanel ht = new TableLayoutPanel(); ht.Dock = DockStyle.Top; ht.AutoSize = true; ht.ColumnCount = 2; ht.RowCount = 2; ht.BackColor = UI.Panel;
        ht.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); ht.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        FlowLayoutPanel hl = new FlowLayoutPanel(); hl.FlowDirection = FlowDirection.TopDown; hl.WrapContents = false; hl.AutoSize = true; hl.BackColor = UI.Panel; hl.Margin = new Padding(0);
        lblResVerdict = Lbl("", 15f, true, UI.Text); lblResVerdict.Margin = new Padding(0, 0, 0, UI.S(4)); hl.Controls.Add(lblResVerdict);
        lblResCounts = Lbl("", UI.SchriftText, false, UI.Muted); lblResCounts.Margin = new Padding(0); hl.Controls.Add(lblResCounts);
        ht.Controls.Add(hl, 0, 0);
        FlowLayoutPanel hr = new FlowLayoutPanel(); hr.FlowDirection = FlowDirection.TopDown; hr.WrapContents = false; hr.AutoSize = true; hr.BackColor = UI.Panel; hr.Margin = new Padding(UI.S(16), 0, 0, 0); hr.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        lblResScore = Lbl("", 22f, true, UI.Accent); lblResScore.Tag = "accent"; lblResScore.Margin = new Padding(0); lblResScore.Anchor = AnchorStyles.Right; hr.Controls.Add(lblResScore);
        lblResScoreSub = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblResScoreSub.Margin = new Padding(0); lblResScoreSub.Anchor = AnchorStyles.Right; hr.Controls.Add(lblResScoreSub);
        ht.Controls.Add(hr, 1, 0);
        pnlDashPills = new FlowLayoutPanel(); pnlDashPills.AutoSize = true; pnlDashPills.WrapContents = true; pnlDashPills.BackColor = UI.Panel; pnlDashPills.Margin = new Padding(0, UI.S(12), 0, 0);
        ht.Controls.Add(pnlDashPills, 0, 1); ht.SetColumnSpan(pnlDashPills, 2);
        // Pillen umbrechen innerhalb der Karte (vorher wurde die vierte Pille unten abgeschnitten)
        hero.Resize += delegate { pnlDashPills.MaximumSize = new Size(Math.Max(UI.S(200), hero.ClientSize.Width - hero.Padding.Horizontal), 0); };
        hero.Controls.Add(ht);
        main.Controls.Add(hero);

        // 2. Wichtigste Befunde und Vergleich mit der Referenz nebeneinander
        tblResMid = new TableLayoutPanel(); tblResMid.Dock = DockStyle.Top; tblResMid.AutoSize = true; tblResMid.ColumnCount = 2; tblResMid.RowCount = 1; tblResMid.BackColor = UI.Bg; tblResMid.Margin = new Padding(0, 0, 0, UI.S(12));
        tblResMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55)); tblResMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        cardResFind = ResCard("Was zu tun ist", out pnlResFind); cardResFind.Margin = new Padding(0, 0, UI.S(6), 0);
        lnkResAll = new LinkLabel(); lnkResAll.Text = "Alle Befunde im Bericht"; lnkResAll.AutoSize = true; lnkResAll.LinkColor = UI.Accent; lnkResAll.ActiveLinkColor = UI.AccentHover; lnkResAll.LinkBehavior = LinkBehavior.HoverUnderline; lnkResAll.Margin = new Padding(0, UI.S(8), 0, 0);
        lnkResAll.LinkClicked += delegate { if (File.Exists(htmlPath)) OpenShell(htmlPath); };
        Tip(lnkResAll, "Öffnet den Bericht mit allen Befunden, Messwerten und Erklärungen.");
        cardResCmp = ResCard("Leistung im Vergleich", out pnlResCmp); cardResCmp.Margin = new Padding(UI.S(6), 0, 0, 0);
        lblResRef = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblResRef.Margin = new Padding(0, 0, 0, UI.S(2));
        // ab v4.03: Weg zur Wahl der Referenz direkt von der Ergebnisseite (Seite Benchmark, Abschnitt Referenz)
        lnkResRef = new LinkLabel(); lnkResRef.Text = "Referenz ändern"; lnkResRef.AutoSize = true; lnkResRef.LinkColor = UI.Accent; lnkResRef.ActiveLinkColor = UI.AccentHover; lnkResRef.LinkBehavior = LinkBehavior.HoverUnderline; lnkResRef.Margin = new Padding(0, 0, 0, UI.S(10));
        lnkResRef.LinkClicked += delegate {
            btnDashBack.PerformClick();
            ShowPage(2);
            if (cmbRef != null) { cmbRef.Focus(); Control sc = cmbRef.Parent; while (sc != null && !(sc is ScrollableControl && ((ScrollableControl)sc).AutoScroll)) sc = sc.Parent; if (sc != null) ((ScrollableControl)sc).ScrollControlIntoView(cmbRef); }
        };
        Tip(lnkResRef, "Öffnet die Seite Benchmark bei der Wahl der Referenz. Eine andere Referenz gilt ab dem nächsten Lauf; im Dashboard lassen sich vorhandene Läufe gegen jede Referenz vergleichen.");
        tblResMid.Controls.Add(cardResFind, 0, 0); tblResMid.Controls.Add(cardResCmp, 1, 0);
        main.Controls.Add(tblResMid);

        // 3. Komponenten: je ein Hauptwert mit Einordnung
        pnlResTiles = new FlowLayoutPanel(); pnlResTiles.AutoSize = true; pnlResTiles.WrapContents = true; pnlResTiles.BackColor = UI.Bg; pnlResTiles.Margin = new Padding(0, 0, 0, UI.S(12));
        main.Controls.Add(pnlResTiles);

        // 4. Knöpfe: links zurück, rechts der Bericht als Hauptaktion
        TableLayoutPanel act = new TableLayoutPanel(); act.Dock = DockStyle.Top; act.AutoSize = true; act.ColumnCount = 2; act.BackColor = UI.Bg; act.Margin = new Padding(0, UI.S(4), 0, UI.S(12));
        act.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); act.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        btnDashBack = UI.Secondary("Zurück"); btnDashBack.Margin = new Padding(0);
        btnDashBack.Click += delegate {
            resultDashboard.Visible = false; runView.Visible = false; contentHost.Visible = true; foot.Visible = true;
            for (int i = 0; i < nav.Count; i++) nav[i].Running = false;
        };
        Tip(btnDashBack, "Kehrt zur Auswahl des Ablaufs zurück.");
        act.Controls.Add(btnDashBack, 0, 0);
        FlowLayoutPanel ar = new FlowLayoutPanel(); ar.AutoSize = true; ar.WrapContents = false; ar.FlowDirection = FlowDirection.RightToLeft; ar.BackColor = UI.Bg; ar.Margin = new Padding(0);
        btnDashOpenHtml = UI.Primary("Bericht öffnen"); btnDashOpenHtml.Margin = new Padding(0);
        btnDashOpenHtml.Click += delegate { OpenShell(htmlPath); };
        Tip(btnDashOpenHtml, "Öffnet den vollständigen Bericht im Browser.");
        btnDashVerlauf = UI.Secondary("Verlauf"); btnDashVerlauf.Margin = new Padding(0, 0, UI.S(8), 0);
        btnDashVerlauf.Click += delegate { OpenDeviceHistory(); };
        Tip(btnDashVerlauf, "Vergleicht diesen Lauf im Dashboard mit früheren Läufen desselben Geräts.");
        btnDashFolder = UI.Secondary("Berichtsordner"); btnDashFolder.Margin = new Padding(0, 0, UI.S(8), 0);
        btnDashFolder.Click += delegate { OpenShell(outDir); };
        Tip(btnDashFolder, "Öffnet den Berichtsordner im Explorer.");
        btnDashKi = UI.Secondary("KI-Datei"); btnDashKi.Margin = new Padding(0, 0, UI.S(8), 0);
        btnDashKi.Click += delegate { OpenSelect(kiPath); };
        Tip(btnDashKi, "Zeigt die Datei für die KI-Auswertung im Explorer.");
        ar.Controls.Add(btnDashOpenHtml); ar.Controls.Add(btnDashVerlauf); ar.Controls.Add(btnDashFolder); ar.Controls.Add(btnDashKi);
        act.Controls.Add(ar, 1, 0);
        main.Controls.Add(act);

        p.Controls.Add(main);
        p.Resize += delegate { LayoutResTiles(); };
        main.SizeChanged += delegate { LayoutResTiles(); };
        return p;
    }

    CardPanel ResCard(string title, out FlowLayoutPanel body)
    {
        CardPanel c = new CardPanel(); c.Dock = DockStyle.Fill; c.AutoSize = true; c.Padding = new Padding(UI.S(18), UI.S(14), UI.S(18), UI.S(14));
        FlowLayoutPanel f = new FlowLayoutPanel(); f.Dock = DockStyle.Top; f.AutoSize = true; f.FlowDirection = FlowDirection.TopDown; f.WrapContents = false; f.BackColor = UI.Panel;
        Label lt = Lbl(title, UI.SchriftAbschnitt, true, UI.Text); lt.Margin = new Padding(0, 0, 0, UI.S(10)); f.Controls.Add(lt);
        c.Controls.Add(f); body = f;
        return c;
    }

    // Kacheln gleich breit: vier nebeneinander, bei schmalem Fenster zwei
    void LayoutResTiles()
    {
        if (pnlResTiles == null || resultDashboard == null) return;
        Control mainT = pnlResTiles.Parent;
        int avail = (mainT != null ? mainT.ClientSize.Width : resultDashboard.ClientSize.Width - resultDashboard.Padding.Horizontal) - UI.S(6);
        int n = pnlResTiles.Controls.Count; if (n == 0 || avail < UI.S(200)) return;
        int perRow = avail >= UI.S(900) ? Math.Min(4, n) : Math.Min(2, n);
        int w = (avail - (perRow - 1) * UI.S(12)) / perRow;
        for (int i = 0; i < n; i++)
        {
            Control c = pnlResTiles.Controls[i];
            c.Width = w;
            c.Margin = new Padding(0, 0, (i % perRow) == perRow - 1 ? 0 : UI.S(12), UI.S(12));
        }
        pnlResTiles.MaximumSize = new Size(avail + UI.S(4), 0);
        int findW = (cardResCmp != null && cardResCmp.Visible ? avail * 55 / 100 : avail) - UI.S(150);
        foreach (Control c in pnlResFind.Controls) if (c is FlowLayoutPanel) foreach (Control k in c.Controls) if (k is Label && !(k is LinkLabel)) k.MaximumSize = new Size(Math.Max(UI.S(200), findW), 0);
    }

    // Einordnung gegenüber der Referenz, nicht gegenüber einem Durchschnitt: Die Referenz ist ein bestimmter PC
    // (meist ein schneller). Bis v4.01 hieß 32 % unterdurchschnittlich, obwohl nur der Abstand zur Referenz gemeint war.
    public static string RatingWord(double pct)
    {
        if (pct >= 110) return "schneller"; if (pct >= 90) return "gleichauf"; if (pct >= 70) return "etwas langsamer";
        if (pct >= 45) return "langsamer"; return "deutlich langsamer";
    }
    static string RatingLevel(double pct) { return pct >= 90 ? "ok" : pct >= 45 ? "info" : "skip"; }

    // Name der Referenz für "100 % = ..." (gespeicherte Referenz mit Namen, sonst die Auswahl im Benchmark)
    string RefLabelText()
    {
        if (cmbRef == null || cmbRef.SelectedIndex <= 0)
        {
            string s = SavedRefText();
            if (s.StartsWith("Gespeicherte Referenz: ")) return "Referenz-PC " + s.Substring(23);
        }
        return cmbRef != null && cmbRef.Text.Length > 0 ? cmbRef.Text : "Referenz";
    }

    // "Samsung SSD 980 1TB (931,5 GB, NVMe, Healthy); ..." -> eine kurze Pille je Laufwerk
    public static List<string> DiskPills(string disks)
    {
        List<string> res = new List<string>();
        foreach (string part in (disks ?? "").Split(';'))
        {
            string d = part.Trim(); if (d.Length == 0) continue;
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(d, @"^(.*?)\s*\((.*)\)$");
            if (!m.Success) { res.Add(d); continue; }
            // Felder mit ", " trennen: Die Größe selbst enthält ein Dezimalkomma ("1,82 TB")
            string[] f = m.Groups[2].Value.Split(new string[] { ", " }, StringSplitOptions.None);
            string txt = m.Groups[1].Value.Trim();
            if (f.Length > 1 && f[1].Trim().Length > 0 && txt.IndexOf(f[1].Trim(), StringComparison.OrdinalIgnoreCase) < 0) txt += "  ·  " + f[1].Trim();
            if (f.Length > 2 && f[2].Trim().Length > 0 && !f[2].Trim().Equals("Healthy", StringComparison.OrdinalIgnoreCase)) txt += "  ·  " + f[2].Trim();
            res.Add(txt);
        }
        return res;
    }

    // Prozent der Referenz aus der Gruppenzeile des Laufs ("112 %"), sonst NaN
    double GroupRefPct(string g)
    {
        ListViewItem h;
        if (!benchHeads.TryGetValue(g, out h) || h.SubItems.Count < 5) return double.NaN;
        string s = h.SubItems[4].Text.Replace("%", "").Trim();
        double v; return Double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.GetCultureInfo("de-DE"), out v) || Double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v) ? v : double.NaN;
    }

    Control ResTile(string title, string icon, string main, string caption, string second, double pct)
    {
        CardPanel c = new CardPanel(); c.AutoSize = false; c.Height = UI.S(150); c.Padding = new Padding(UI.S(16), UI.S(12), UI.S(16), UI.S(12));
        FlowLayoutPanel f = new FlowLayoutPanel(); f.Dock = DockStyle.Fill; f.FlowDirection = FlowDirection.TopDown; f.WrapContents = false; f.BackColor = UI.Panel;
        FlowLayoutPanel head = new FlowLayoutPanel(); head.AutoSize = true; head.WrapContents = false; head.BackColor = UI.Panel; head.Margin = new Padding(0, 0, 0, UI.S(8));
        Label li = new Label(); li.AutoSize = true; li.Text = icon; li.Font = UI.SymbolFont(10f); li.ForeColor = UI.Accent; li.Tag = "accent"; li.BackColor = Color.Transparent; li.Margin = new Padding(0, UI.S(2), UI.S(6), 0);
        head.Controls.Add(li);
        Label lt = Lbl(title, UI.SchriftText, true, UI.Muted); lt.Margin = new Padding(0); head.Controls.Add(lt);
        f.Controls.Add(head);
        Label lm = Lbl(main, 15f, true, UI.Text); lm.Margin = new Padding(0); f.Controls.Add(lm);
        Label lc = Lbl(caption, UI.SchriftHinweis, false, UI.Muted); lc.Margin = new Padding(0, 0, 0, UI.S(8)); f.Controls.Add(lc);
        if (!double.IsNaN(pct))
        {
            // ab v4.07 steht die Prozentzahl im Ring oben rechts, das Etikett nennt die Einordnung
            Badge b = new Badge(RatingWord(pct), RatingLevel(pct)); b.Margin = new Padding(0, 0, 0, UI.S(6));
            Tip(b, pct.ToString("0") + " % im Vergleich zum Referenz-PC (100 %): " + RefLabelText() + ".");
            f.Controls.Add(b);
            RingAnzeige ring = new RingAnzeige(); ring.Percent = pct; ring.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Tip(ring, pct.ToString("0") + " % des Referenz-PCs. Voller Ring = 100 %: " + RefLabelText() + ".");
            c.Controls.Add(ring); resRinge.Add(ring);
            CardPanel karte = c;
            EventHandler lage = delegate { ring.Location = new Point(karte.ClientSize.Width - karte.Padding.Right - ring.Width, karte.Padding.Top - UI.S(2)); };
            c.Resize += lage; lage(c, EventArgs.Empty);
        }
        if (!String.IsNullOrEmpty(second)) { Label ls = Lbl(second, UI.SchriftHinweis, false, UI.Muted); ls.Margin = new Padding(0); f.Controls.Add(ls); }
        c.Controls.Add(f);
        return c;
    }

    // Gehört der Datenbankeintrag zum Berichtsordner dieses Laufs? Einträge ohne Ordner (importiert, Referenz) passen nie.
    // Bis v4.06 passte ein leerer Ordner auf jeden Lauf (EndsWith("")); dann zeigte die Ergebnisseite den ersten Eintrag.
    public static bool ErgebnisOrdnerPasst(string outDir, string ordner)
    {
        if (String.IsNullOrEmpty(outDir) || String.IsNullOrEmpty(ordner)) return false;
        string a = outDir.Replace('/', '\\').TrimEnd('\\'), b = ordner.Replace('/', '\\').TrimEnd('\\');
        if (b.Length == 0) return false;
        return a.Equals(b, StringComparison.OrdinalIgnoreCase) || a.EndsWith("\\" + b, StringComparison.OrdinalIgnoreCase);
    }

    // Zeigt die Ergebnisseite und lässt Punktzahl, Balken und Ringe einmal auf ihren Wert laufen (ab v4.07).
    // Die Werte selbst stehen schon fest; ohne Bewegung (Einstellungen, Remotesitzung) erscheint sofort der Endstand.
    void ZeigeErgebnis()
    {
        if (resultDashboard == null) return;
        resultDashboard.Visible = true;
        System.Globalization.CultureInfo de = System.Globalization.CultureInfo.GetCultureInfo("de-DE");
        int ziel = resScore;
        // Breite der Punktzahl vorab auf den Endwert festlegen, damit sich beim Hochzählen nichts verschiebt
        if (lblResScore != null)
        {
            lblResScore.MinimumSize = Size.Empty;
            if (ziel > 0) { lblResScore.Text = ziel.ToString("N0", de); lblResScore.MinimumSize = new Size(lblResScore.PreferredWidth, 0); lblResScore.TextAlign = ContentAlignment.MiddleRight; }
        }
        Bewegung.Starte(resultDashboard, Bewegung.DauerErgebnis, delegate(double t) {
            if (ziel > 0 && lblResScore != null) lblResScore.Text = ((int)Math.Round(ziel * t)).ToString("N0", de);
            foreach (PercentBar pb in resBalken) if (!pb.IsDisposed) pb.Anteil = t;
            foreach (RingAnzeige ra in resRinge) if (!ra.IsDisposed) ra.Anteil = t;
        });
    }

    void PopulateResultDashboard()
    {
        ReloadDb();
        DbEntry e = null;
        if (!String.IsNullOrEmpty(outDir))
            foreach (DbEntry dbe in db) { if (ErgebnisOrdnerPasst(outDir, dbe.Ordner)) { e = dbe; break; } }
        System.Globalization.CultureInfo de = System.Globalization.CultureInfo.GetCultureInfo("de-DE");

        lblDashMeta.Text = String.Format("{0}  ·  {1}{2}",
            e != null ? e.DisplayName : Environment.MachineName,
            e != null ? "Lauf vom " + e.Datum : DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
            runStarted != DateTime.MinValue ? String.Format("  ·  Dauer {0:mm\\:ss} Min.", DateTime.Now - runStarted) : "");

        // Urteil aus den Befunden dieses Laufs
        if (nK > 0) { lblResVerdict.Text = nK == 1 ? "1 kritischer Befund" : nK + " kritische Befunde"; lblResVerdict.ForeColor = UI.Crit; }
        else if (nW > 0) { lblResVerdict.Text = nW == 1 ? "1 Warnung" : nW + " Warnungen"; lblResVerdict.ForeColor = UI.Warn; }
        else { lblResVerdict.Text = "Keine Auffälligkeiten"; lblResVerdict.ForeColor = UI.Ok; }
        lblResCounts.Text = String.Format("{0} kritisch  ·  {1} Warnungen  ·  {2} Hinweise", nK, nW, nI);

        int score = e != null ? (int)e.OverallScore : 0;
        resScore = score; resBalken.Clear(); resRinge.Clear();
        lblResScore.Text = score > 0 ? score.ToString("N0", de) : "";
        lblResScoreSub.Text = score > 0 ? "Punkte gesamt" : "";
        lblResScore.ForeColor = UI.Accent;

        pnlDashPills.Controls.Clear();
        if (e != null)
        {
            if (!String.IsNullOrEmpty(e.CpuDisplay)) pnlDashPills.Controls.Add(new PillLabel(e.CpuDisplay));
            if (!String.IsNullOrEmpty(e.Gpu)) pnlDashPills.Controls.Add(new PillLabel(e.Gpu));
            if (!String.IsNullOrEmpty(e.Ram)) pnlDashPills.Controls.Add(new PillLabel(e.Ram));
            foreach (string dp in DiskPills(e.Disk)) { PillLabel pl = new PillLabel(dp); Tip(pl, e.Disk); pnlDashPills.Controls.Add(pl); }
        }

        // Wichtigste Befunde: kritische zuerst, dann Warnungen, höchstens fünf
        while (pnlResFind.Controls.Count > 1) pnlResFind.Controls.RemoveAt(1);
        int shown = 0;
        if (lvFind != null)
            foreach (ListViewItem it in lvFind.Items)
            {
                string lev = it.SubItems[0].Text;
                if (lev != "KRITISCH" && lev != "WARNUNG") continue;
                if (shown >= 5) break;
                FlowLayoutPanel r = new FlowLayoutPanel(); r.AutoSize = true; r.WrapContents = false; r.BackColor = UI.Panel; r.Margin = new Padding(0, 0, 0, UI.S(8));
                Badge b = new Badge(lev == "KRITISCH" ? "Kritisch" : "Warnung", lev == "KRITISCH" ? "crit" : "warn"); b.Margin = new Padding(0, UI.S(1), UI.S(10), 0); b.Width = Math.Max(b.Width, UI.S(72));
                Label lt = Lbl(it.SubItems.Count > 2 ? it.SubItems[2].Text : "", UI.SchriftHinweis, false, UI.Text); lt.Margin = new Padding(0, UI.S(2), 0, 0);
                Tip(lt, (it.SubItems.Count > 1 ? it.SubItems[1].Text + ": " : "") + (it.SubItems.Count > 2 ? it.SubItems[2].Text : ""));
                r.Controls.Add(b); r.Controls.Add(lt); pnlResFind.Controls.Add(r);
                shown++;
            }
        if (shown == 0)
        {
            Label ok = Lbl(nI > 0 ? "Keine kritischen Befunde und keine Warnungen. " + (nI == 1 ? "1 Hinweis" : nI + " Hinweise") + " stehen im Bericht." : "Keine kritischen Befunde, keine Warnungen und keine Hinweise.", UI.SchriftText, false, UI.Text);
            ok.MaximumSize = new Size(UI.S(440), 0); pnlResFind.Controls.Add(ok);
        }
        else if (nK + nW > shown)
        {
            Label more = Lbl(String.Format("und {0} weitere", nK + nW - shown), UI.SchriftHinweis, false, UI.Muted); more.Margin = new Padding(0, 0, 0, UI.S(2)); pnlResFind.Controls.Add(more);
        }
        lnkResAll.Visible = File.Exists(htmlPath);
        pnlResFind.Controls.Add(lnkResAll);

        // Vergleich mit der Referenz je Komponente (nur mit Benchmark in diesem Lauf)
        while (pnlResCmp.Controls.Count > 1) pnlResCmp.Controls.RemoveAt(1);
        // Verweise der Seite folgen Farbschema und Akzent auch nach einem Wechsel (sie behielten bisher die Farbe vom Aufbau)
        foreach (LinkLabel ll in new LinkLabel[] { lnkResRef, lnkResAll }) if (ll != null) { ll.LinkColor = UI.Accent; ll.ActiveLinkColor = UI.AccentHover; }
        lblResRef.Text = "100 % = " + RefLabelText();
        lblResRef.MaximumSize = new Size(UI.S(400), 0);
        pnlResCmp.Controls.Add(lblResRef);
        pnlResCmp.Controls.Add(lnkResRef);
        int cmpRows = 0;
        for (int g = 0; g < resGruppen.Length; g++)
        {
            double pct = GroupRefPct(resGruppen[g]);
            TableLayoutPanel r = new TableLayoutPanel(); r.AutoSize = true; r.ColumnCount = 3; r.BackColor = UI.Panel; r.Margin = new Padding(0, 0, 0, UI.S(8));
            r.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(120))); r.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(130))); r.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Label ln = Lbl(resNamen[g], UI.SchriftText, false, UI.Text); ln.Margin = new Padding(0, UI.S(2), 0, 0);
            r.Controls.Add(ln, 0, 0);
            if (double.IsNaN(pct))
            {
                // Gruppe in diesem Lauf nicht gemessen (abgewählt oder übersprungen): Zeile bleibt, damit das Fehlen auffällt
                Label lx = Lbl("nicht gemessen", UI.SchriftText, false, UI.Muted); lx.Margin = new Padding(0, UI.S(2), 0, 0);
                r.Controls.Add(lx, 1, 0); r.SetColumnSpan(lx, 2);
                pnlResCmp.Controls.Add(r);
                continue;
            }
            PercentBar pb = new PercentBar(); pb.Percent = pct; pb.Size = new Size(UI.S(120), UI.S(10)); pb.Margin = new Padding(0, UI.S(7), UI.S(10), 0);
            Tip(pb, "Strich = Referenz (100 %). Langsamer als die Referenz heißt nicht defekt, nur langsamer als dieser PC.");
            Label lv = Lbl(pct.ToString("0") + " %  " + RatingWord(pct), UI.SchriftText, true, UI.Text); lv.Margin = new Padding(0, UI.S(2), 0, 0);
            resBalken.Add(pb);
            r.Controls.Add(pb, 1, 0); r.Controls.Add(lv, 2, 0);
            pnlResCmp.Controls.Add(r); cmpRows++;
        }
        bool hasCmp = cmpRows > 0;
        cardResCmp.Visible = hasCmp;
        tblResMid.ColumnStyles[0].Width = hasCmp ? 55 : 100; tblResMid.ColumnStyles[1].Width = hasCmp ? 45 : 0;
        cardResFind.Margin = new Padding(0, 0, hasCmp ? UI.S(6) : 0, 0);

        // Komponenten: nur mit Messwerten
        pnlResTiles.Controls.Clear();
        if (e != null)
        {
            double mt = e.Get("CPU|MT"), st = e.Get("CPU|ST");
            if (mt > 0) pnlResTiles.Controls.Add(ResTile("Prozessor", UI.IcoCpu, mt.ToString("N0", de), "Punkte, alle Kerne", st > 0 ? "Ein Kern: " + st.ToString("N0", de) + " Punkte" : "", GroupRefPct("CPU")));
            double rend = e.Get("GPU|REND"), rend1 = e.Get("GPU|REND1"), vmb = e.Get("GPU|VMB");
            if (rend > 0) pnlResTiles.Controls.Add(ResTile("Grafik", UI.IcoGpu, rend.ToString("N1", de) + " FPS", "Bilder je Sekunde im Rendertest", rend1 > 0 ? "1 % der langsamsten Bilder: " + rend1.ToString("N1", de) + " FPS" : "", GroupRefPct("GPU")));
            else if (vmb > 0) pnlResTiles.Controls.Add(ResTile("Grafik", UI.IcoGpu, vmb.ToString("N1", de) + " GB/s", "Speicherdurchsatz", "", GroupRefPct("GPU")));
            double rL = e.Get("RAM|Lesen"), rLat = e.Get("RAM|Latenz");
            if (rL > 0) pnlResTiles.Controls.Add(ResTile("Arbeitsspeicher", UI.IcoRam, rL.ToString("N1", de) + " GB/s", "Lesen", rLat > 0 ? "Latenz: " + rLat.ToString("N0", de) + " ns" : "", GroupRefPct("RAM")));
            double sr = 0, r1 = 0;
            foreach (KeyValuePair<string, double> kv in e.Werte)
            {
                if (kv.Key.StartsWith("DISK|") && kv.Key.EndsWith("|SR") && kv.Value > sr) sr = kv.Value;
                if (kv.Key.StartsWith("DISK|") && kv.Key.EndsWith("|R1") && kv.Value > r1) r1 = kv.Value;
            }
            if (sr > 0) pnlResTiles.Controls.Add(ResTile("Laufwerk", UI.IcoDisk, sr.ToString("N0", de) + " MB/s", "Lesen am Stück, schnellstes Laufwerk", r1 > 0 ? "4K-Zufallszugriff: " + r1.ToString("N0", de) + " IOPS" : "", GroupRefPct("Laufwerke")));
        }
        pnlResTiles.Visible = pnlResTiles.Controls.Count > 0;
        btnDashVerlauf.Visible = e != null;
        btnDashKi.Visible = File.Exists(kiPath);
        btnDashOpenHtml.Enabled = File.Exists(htmlPath);
        LayoutResTiles();
    }
}
