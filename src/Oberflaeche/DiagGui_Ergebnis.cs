// Ergebnisseite nach einem Lauf (Teilklasse DiagGui)
public partial class DiagGui
{
    // ------------------------------------------------------------ Ergebnisseite (ab v3.9, neu in v4.01, Seite ab v4.08)
    // Oben das Urteil mit den wichtigsten Befunden und der Vergleich mit der Referenz, darunter je Komponente ein
    // Hauptwert mit Einordnung. Gezeigt wird nur, was der Lauf gemessen, gemeldet oder gespeichert hat; Details stehen
    // im Bericht.
    // ab v4.08 ist das Ergebnis eine Seite wie jede andere: Eintrag "Ergebnis" in der Seitenleiste unter Start, die
    // Fußleiste bleibt, Wechsel weg und zurück ohne Neuaufbau. Nach einem Programmstart zeigt sie den letzten Lauf
    // dieses PCs aus der Datenbank. Lage und Größe aller Teile setzt OrdneErgebnis selbst (keine verschachtelten
    // Elemente mit AutoSize mehr: Daher kam die unten leere Kopfkarte unter Windows).
    internal Label lblResVerdict;
    Label lblResCounts, lblResScore, lblResScoreSub, lblResRef;
    LinkLabel lnkResRef;
    internal LinkLabel lnkResAll;
    internal CardPanel cardResHero;
    internal UmbruchPanel pnlDashPills;
    internal ZeilenKarte kResFind, kResCmp;
    internal Panel pnlResTiles;
    Panel pnlResAct;
    Button btnDashAgain;
    // ab v4.07: Balken und Ringe, die beim Einblenden der Seite auf ihren Wert laufen, und die Punktzahl als Zahl
    List<PercentBar> resBalken = new List<PercentBar>();
    List<RingAnzeige> resRinge = new List<RingAnzeige>();
    int resScore;

    static readonly string[] resGruppen = new string[] { "CPU", "GPU", "RAM", "Laufwerke" };
    static readonly string[] resNamen = new string[] { "Prozessor", "Grafik", "Arbeitsspeicher", "Laufwerke" };

    // Was die Ergebnisseite zeigt: Werte des eben beendeten Laufs (VomLauf) oder eines Datenbankeintrags
    internal class ErgebnisDaten
    {
        public DbEntry Eintrag;
        public bool VomLauf;
        public int NK = -1, NW = -1, NI = -1;                                // -1: Zähler nicht gespeichert
        public List<string[]> Befunde = new List<string[]>();                // Stufe, Bereich, Text (kritisch und Warnung)
        public Dictionary<string, double> Pct = new Dictionary<string, double>();
        public bool HatVergleich;                                            // Prozentwerte je Gruppe liegen vor
        public string RefName = "", Html = "", Ordner = "", Ki = "", Meta = "", Datum = "";
        public int Score;
    }
    internal ErgebnisDaten erg;
    internal int ergebnisPage = -1, ergebnisAufbauten;
    internal NavItem navErgebnis;
    // Zustand von Eintrag und Band für Tests: Visible eines Elements ist falsch, solange das Fenster nicht angezeigt wird
    internal bool ergebnisNavSichtbar, bandErgGezeigt;
    bool ergVeraltet, ergBewegung;

    Control BuildResultDashboard()
    {
        Panel p = new Panel(); p.BackColor = UI.Panel; p.Size = new Size(UI.S(UI.Gruppenbreite), UI.S(400));

        lblDashTitle = Lbl("Ergebnis", UI.SchriftTitel, true, UI.Text); lblDashTitle.Location = new Point(0, 0);
        lblDashMeta = Lbl("", UI.SchriftText, false, UI.Muted); lblDashMeta.AutoSize = false; lblDashMeta.AutoEllipsis = true;
        p.Controls.Add(lblDashTitle); p.Controls.Add(lblDashMeta);

        // 1. Urteil links, Punktzahl rechts, darunter die Hardware
        cardResHero = new CardPanel(); cardResHero.AutoSize = false; cardResHero.Padding = new Padding(0);
        lblResVerdict = Lbl("", 15f, true, UI.Text); lblResCounts = Lbl("", UI.SchriftText, false, UI.Muted);
        lblResScore = Lbl("", 22f, true, UI.Accent); lblResScore.Tag = "accent"; lblResScore.AutoSize = false; lblResScore.TextAlign = ContentAlignment.MiddleRight;
        lblResScoreSub = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblResScoreSub.AutoSize = false; lblResScoreSub.TextAlign = ContentAlignment.MiddleRight;
        pnlDashPills = new UmbruchPanel();
        cardResHero.Controls.Add(lblResVerdict); cardResHero.Controls.Add(lblResCounts); cardResHero.Controls.Add(lblResScore); cardResHero.Controls.Add(lblResScoreSub); cardResHero.Controls.Add(pnlDashPills);
        p.Controls.Add(cardResHero);

        // 2. Wichtigste Befunde und Vergleich mit der Referenz nebeneinander
        kResFind = new ZeilenKarte("Was zu tun ist"); kResFind.Karte.Margin = new Padding(0);
        lnkResAll = KartenVerweis("Alle Befunde im Bericht"); lnkResAll.Margin = new Padding(0, UI.S(6), 0, 0);
        lnkResAll.LinkClicked += delegate { if (erg != null && File.Exists(erg.Html)) OpenShell(erg.Html); };
        Tip(lnkResAll, "Öffnet den Bericht mit allen Befunden, Messwerten und Erklärungen.");
        kResCmp = new ZeilenKarte("Leistung im Vergleich"); kResCmp.Karte.Margin = new Padding(0);
        lblResRef = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblResRef.Margin = new Padding(0, 0, 0, UI.S(8));
        // ab v4.03: Weg zur Wahl der Referenz direkt von der Ergebnisseite; ab v4.08 mit dem Band "Zurück zum Ergebnis"
        lnkResRef = KartenVerweis("Referenz ändern"); lnkResRef.Margin = new Padding(0);
        lnkResRef.LinkClicked += delegate { ZeigeReferenzwahl(); };
        Tip(lnkResRef, "Öffnet die Seite Benchmark bei der Wahl der Referenz; von dort führt ein Band zurück zum Ergebnis. Eine andere Referenz gilt ab dem nächsten Lauf; im Dashboard lassen sich vorhandene Läufe gegen jede Referenz vergleichen.");
        kResCmp.KopfRechts.Controls.Add(lnkResRef);
        p.Controls.Add(kResFind.Karte); p.Controls.Add(kResCmp.Karte);

        // 3. Komponenten: je ein Hauptwert mit Einordnung
        pnlResTiles = new Panel(); pnlResTiles.BackColor = UI.Panel; p.Controls.Add(pnlResTiles);

        // 4. Knöpfe: links erneut messen, rechts der Bericht als Hauptaktion
        pnlResAct = new Panel(); pnlResAct.BackColor = UI.Panel;
        btnDashAgain = UI.Secondary("Erneut messen");
        btnDashAgain.Click += delegate { ErneutMessen(); };
        Tip(btnDashAgain, "Startet den gewählten Ablauf noch einmal, mit derselben Übersicht vor dem Start wie der Startknopf. Ist links kein Modul angehakt, werden die messenden Module dieses Laufs angehakt (Diagnose, Benchmark, Lasttest).");
        btnDashOpenHtml = UI.Primary("Bericht öffnen");
        btnDashOpenHtml.Click += delegate { if (erg != null && File.Exists(erg.Html)) OpenShell(erg.Html); };
        Tip(btnDashOpenHtml, "Öffnet den vollständigen Bericht im Browser.");
        btnDashVerlauf = UI.Secondary("Verlauf");
        btnDashVerlauf.Click += delegate { if (erg != null && erg.Eintrag != null) OpenDeviceHistory(erg.Eintrag); else OpenDeviceHistory(); };
        Tip(btnDashVerlauf, "Vergleicht diesen Lauf im Dashboard mit früheren Läufen desselben Geräts.");
        btnDashFolder = UI.Secondary("Berichtsordner");
        btnDashFolder.Click += delegate { if (erg != null && Directory.Exists(erg.Ordner)) OpenShell(erg.Ordner); };
        Tip(btnDashFolder, "Öffnet den Berichtsordner im Explorer.");
        btnDashKi = UI.Secondary("KI-Datei");
        btnDashKi.Click += delegate { if (erg != null && File.Exists(erg.Ki)) OpenSelect(erg.Ki); };
        Tip(btnDashKi, "Zeigt die Datei für die KI-Auswertung im Explorer.");
        foreach (Button b in new Button[] { btnDashAgain, btnDashKi, btnDashFolder, btnDashVerlauf, btnDashOpenHtml }) { b.Margin = new Padding(0); pnlResAct.Controls.Add(b); }
        p.Controls.Add(pnlResAct);

        if (content != null)
        {
            content.Resize += delegate { if (curPage == ergebnisPage) OrdneErgebnis(); };
            content.ClientSizeChanged += delegate { if (curPage == ergebnisPage) OrdneErgebnis(); };
        }
        return p;
    }

    // Lage und Größe aller Teile der Ergebnisseite für die Seitenbreite. Alle Höhen sind gemessen oder fest, nichts hängt
    // von AutoSize verschachtelter Elemente ab.
    // Gewollte Sichtbarkeit von Karte und Knöpfen: Visible eines Elements ist falsch, solange seine Seite nicht angezeigt
    // wird, die Seite wird aber auch im Hintergrund geordnet (nach einem Lauf, vor dem ersten Öffnen).
    Dictionary<Control, bool> ergSoll = new Dictionary<Control, bool>();
    void ErgZeige(Control c, bool v) { ergSoll[c] = v; c.Visible = v; }
    bool ErgSoll(Control c) { bool v; return !ergSoll.TryGetValue(c, out v) || v; }

    bool ordnetErgebnis;
    void OrdneErgebnis()
    {
        if (resultDashboard == null || ordnetErgebnis) return;
        ordnetErgebnis = true;
        try
        {
            int w = SeitenBreite(), y = 0, gap = UI.S(UI.Abstand3);
            y = lblDashTitle.PreferredSize.Height + UI.S(2);
            int mh = TextRenderer.MeasureText("Äg", lblDashMeta.Font).Height + UI.S(2);
            lblDashMeta.SetBounds(0, y, w, mh);
            y += mh + UI.S(10);

            // Kopfkarte
            int px = UI.S(20), py = UI.S(16);
            Size vs = lblResVerdict.PreferredSize, cs = lblResCounts.PreferredSize;
            lblResVerdict.Location = new Point(px, py);
            lblResCounts.Location = new Point(px, py + vs.Height + UI.S(2));
            int links = py + vs.Height + UI.S(2) + cs.Height, rechts = py;
            if (lblResScoreSub.Text.Length > 0)
            {
                string ziel = resScore > 0 ? resScore.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("de-DE")) : lblResScore.Text;
                Size ss = TextRenderer.MeasureText(ziel, lblResScore.Font), us = TextRenderer.MeasureText(lblResScoreSub.Text, lblResScoreSub.Font);
                int sw = Math.Max(ss.Width, us.Width) + UI.S(4);
                lblResScore.SetBounds(w - px - sw, py - UI.S(4), sw, ss.Height);
                lblResScoreSub.SetBounds(w - px - sw, py - UI.S(4) + ss.Height, sw, us.Height);
                rechts = py - UI.S(4) + ss.Height + us.Height;
            }
            lblResScore.Visible = lblResScoreSub.Visible = lblResScoreSub.Text.Length > 0;
            int top = Math.Max(links, rechts);
            pnlDashPills.Location = new Point(px, top + gap);
            pnlDashPills.Width = Math.Max(UI.S(200), w - 2 * px); pnlDashPills.Ordne();
            int hh = (pnlDashPills.Controls.Count > 0 ? pnlDashPills.Bottom - UI.S(6) : top) + py;
            cardResHero.SetBounds(0, y, w, hh);
            y += hh + gap;

            // Befunde und Vergleich, gleich hoch
            bool cmp = ErgSoll(kResCmp.Karte);
            int wl = cmp ? (w - gap) * 55 / 100 : w, wr = w - gap - wl;
            kResFind.MinHoehe = 0; kResCmp.MinHoehe = 0;
            kResFind.Breite(wl); if (cmp) kResCmp.Breite(wr);
            // Balken im schmalen Fenster kürzer, damit Prozentzahl und Einordnung daneben in die Karte passen
            int bw = Math.Max(UI.S(50), Math.Min(UI.S(120), kResCmp.Innenbreite - UI.S(120) - UI.S(10) - UI.S(155)));
            foreach (PercentBar pb in resBalken) if (!pb.IsDisposed && pb.Width != bw) pb.Width = bw;
            int kh = Math.Max(kResFind.InhaltHoehe, cmp ? kResCmp.InhaltHoehe : 0);
            kResFind.MinHoehe = kh; kResCmp.MinHoehe = kh;
            kResFind.Karte.Location = new Point(0, y); kResCmp.Karte.Location = new Point(wl + gap, y);
            y += kh + gap;

            // Kacheln gleich breit: vier nebeneinander, bei schmalem Fenster zwei
            int n = pnlResTiles.Controls.Count;
            pnlResTiles.Visible = n > 0;
            if (n > 0)
            {
                int perRow = w >= UI.S(900) ? Math.Min(4, n) : Math.Min(2, n);
                int tw = (w - (perRow - 1) * gap) / perRow, th = UI.S(150), zeilen = (n + perRow - 1) / perRow;
                for (int i = 0; i < n; i++) pnlResTiles.Controls[i].SetBounds((i % perRow) * (tw + gap), (i / perRow) * (th + gap), tw, th);
                pnlResTiles.SetBounds(0, y, w, zeilen * th + (zeilen - 1) * gap);
                y += pnlResTiles.Height + gap;
            }

            // Knöpfe
            int ah = 0; foreach (Control b in pnlResAct.Controls) ah = Math.Max(ah, b.PreferredSize.Height);
            btnDashAgain.Size = btnDashAgain.PreferredSize; btnDashAgain.Location = new Point(0, (ah - btnDashAgain.Height) / 2);
            int rx = w;
            foreach (Button b in new Button[] { btnDashOpenHtml, btnDashVerlauf, btnDashFolder, btnDashKi })
            {
                if (!ErgSoll(b)) continue;
                b.Size = b.PreferredSize; rx -= b.Width; b.Location = new Point(rx, (ah - b.Height) / 2); rx -= UI.S(8);
            }
            pnlResAct.SetBounds(0, y + UI.S(4), w, ah);
            y += ah + UI.S(4) + UI.S(8);
            Size s = new Size(w, y);
            if (resultDashboard.Size != s) resultDashboard.Size = s;
        }
        finally { ordnetErgebnis = false; }
    }

    // Einordnung gegenüber der Referenz, nicht gegenüber einem Durchschnitt: Die Referenz ist ein bestimmter PC
    // (meist ein schneller). Bis v4.01 hieß 32 % unterdurchschnittlich, obwohl nur der Abstand zur Referenz gemeint war.
    public static string RatingWord(double pct)
    {
        if (pct >= 110) return "schneller"; if (pct >= 90) return "gleichauf"; if (pct >= 70) return "etwas langsamer";
        if (pct >= 45) return "langsamer"; return "deutlich langsamer";
    }
    static string RatingLevel(double pct) { return pct >= 90 ? "ok" : pct >= 45 ? "info" : "skip"; }

    // Name der Referenz für "100 % = ...": der mit dem Lauf gespeicherte Name, sonst die geltende Auswahl im Benchmark
    // (bei "Gespeicherte Referenz" der Name der Referenzdatei; bis v4.07 stand dort nur der Text der Auswahl)
    string RefLabelText()
    {
        if (erg != null && erg.RefName.Length > 0) return erg.RefName;
        if (cmbRef == null || cmbRef.SelectedIndex <= 0)
        {
            string[] r = RefDatei("Referenz.json");
            if (r != null && r[2] != "Median" && r[0].Length > 0) return r[0];
            string dev = HwIsLaptop ? "Referenz_Notebook.json" : "Referenz_Desktop.json", alt = HwIsLaptop ? "Referenz_Desktop.json" : "Referenz_Notebook.json";
            foreach (string f in new string[] { dev, alt, "PC-Diagnose-Referenz.json" }) { string[] m = RefDatei(f); if (m != null && m[0].Length > 0) return m[0]; }
            return "eingebautes Referenzprofil";
        }
        return cmbRef.Text.Length > 0 ? cmbRef.Text : "Referenz";
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

    // "[WARNUNG] RAM: XMP inaktiv ..." -> Stufe, Bereich, Text (Form der Befundliste im Datenbankeintrag)
    public static string[] BefundTeile(string zeile)
    {
        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(zeile ?? "", @"^\s*\[([^\]]+)\]\s*([^:]*):\s*(.*)$", System.Text.RegularExpressions.RegexOptions.Singleline);
        if (!m.Success) return new string[] { "", "", (zeile ?? "").Trim() };
        return new string[] { m.Groups[1].Value.Trim().ToUpperInvariant(), m.Groups[2].Value.Trim(), m.Groups[3].Value.Trim() };
    }

    // Kritische Befunde zuerst, dann Warnungen, jeweils in der Reihenfolge des Laufs (Hinweise stehen nur im Bericht)
    public static List<string[]> WichtigeBefunde(List<string[]> alle)
    {
        List<string[]> r = new List<string[]>();
        foreach (string stufe in new string[] { "KRITISCH", "WARNUNG" })
            foreach (string[] b in alle) if (b != null && b.Length >= 3 && b[0] == stufe) r.Add(b);
        return r;
    }

    // Ordner eines Datenbankeintrags auf diesem Datenträger (wie OpenEntry): relativ zum Datenordner, sonst der
    // gleichnamige Ordner unter Berichte; leer, wenn es ihn nicht gibt
    string EintragOrdner(DbEntry e)
    {
        if (e == null || e.Ordner.Length == 0) return "";
        try
        {
            string dir = System.IO.Path.IsPathRooted(e.Ordner) ? e.Ordner : System.IO.Path.Combine(dataDir, e.Ordner);
            if (!Directory.Exists(dir)) { string alt = System.IO.Path.Combine(System.IO.Path.Combine(dataDir, "Berichte"), System.IO.Path.GetFileName(e.Ordner.TrimEnd('\\', '/'))); if (Directory.Exists(alt)) dir = alt; }
            return Directory.Exists(dir) ? dir : "";
        }
        catch { return ""; }
    }

    // Ergebnis aus einem Datenbankeintrag: nur, was dort gespeichert ist. Prozentwerte je Gruppe und der Name der
    // Referenz stehen ab v4.08 im Feld Vergleich; bei älteren Läufen bleibt HatVergleich falsch.
    internal ErgebnisDaten ErgebnisAusEintrag(DbEntry e)
    {
        ErgebnisDaten d = new ErgebnisDaten();
        d.Eintrag = e; d.NK = e.NK; d.NW = e.NW; d.NI = e.NI; d.Score = (int)e.OverallScore; d.Datum = e.Datum;
        List<string[]> alle = new List<string[]>();
        foreach (string z in e.BefundListe) alle.Add(BefundTeile(z));
        d.Befunde = WichtigeBefunde(alle);
        foreach (KeyValuePair<string, double> kv in e.GruppenPct) d.Pct[kv.Key] = kv.Value;
        d.HatVergleich = e.GruppenPct.Count > 0; d.RefName = e.RefName;
        d.Ordner = EintragOrdner(e);
        if (d.Ordner.Length > 0)
        {
            string html = System.IO.Path.Combine(d.Ordner, "Diagnosebericht.html"); if (File.Exists(html)) d.Html = html;
            try { foreach (string k in Directory.GetFiles(d.Ordner, "KI-Analyse_*.txt")) if (k.IndexOf("Kurz", StringComparison.OrdinalIgnoreCase) < 0) { d.Ki = k; break; } } catch { }
        }
        d.Meta = String.Format("{0}  ·  Lauf vom {1}{2}", e.DisplayName, DatumKurz(e.Datum), e.Module.Length > 0 ? "  ·  " + e.Module : "");
        return d;
    }

    // Ergebnis des eben beendeten Laufs: Zähler, Befunde und Prozentwerte aus der Laufansicht, Hardware und Messwerte aus
    // dem Datenbankeintrag dieses Laufs (falls geschrieben)
    ErgebnisDaten ErgebnisAusLauf(DbEntry e)
    {
        ErgebnisDaten d = new ErgebnisDaten();
        d.Eintrag = e; d.VomLauf = true; d.NK = nK; d.NW = nW; d.NI = nI; d.Score = e != null ? (int)e.OverallScore : 0;
        d.Datum = e != null ? e.Datum : DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        List<string[]> alle = new List<string[]>();
        if (lvFind != null)
            foreach (ListViewItem it in lvFind.Items)
                alle.Add(new string[] { it.SubItems[0].Text.ToUpperInvariant(), it.SubItems.Count > 1 ? it.SubItems[1].Text : "", it.SubItems.Count > 2 ? it.SubItems[2].Text : "" });
        d.Befunde = WichtigeBefunde(alle);
        foreach (string g in resGruppen) { double p = GroupRefPct(g); if (!double.IsNaN(p)) d.Pct[g] = p; }
        d.HatVergleich = d.Pct.Count > 0;
        d.RefName = e != null ? e.RefName : "";
        d.Html = htmlPath; d.Ordner = outDir; d.Ki = kiPath;
        d.Meta = String.Format("{0}  ·  Lauf vom {1}{2}{3}",
            e != null ? e.DisplayName : Environment.MachineName, DatumKurz(d.Datum),
            runStarted != DateTime.MinValue ? String.Format("  ·  Dauer {0:mm\\:ss} Min.", DateTime.Now - runStarted) : "",
            e != null && e.Module.Length > 0 ? "  ·  " + e.Module : "");
        return d;
    }

    // Eintrag in der Seitenleiste: sichtbar, sobald es ein Ergebnis gibt, mit der Punktzahl als Pille
    void SetzeErgebnisNav()
    {
        if (navErgebnis == null) return;
        bool da = erg != null;
        string h = da && erg.Score > 0 ? erg.Score.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("de-DE")) : "";
        if (navErgebnis.Hint != h) { navErgebnis.Hint = h; navErgebnis.Invalidate(); }
        ergebnisNavSichtbar = da;
        navErgebnis.Visible = da;
        if (da) Tip(navErgebnis, "Ergebnis: " + (erg.VomLauf ? "der Lauf von eben" : "der letzte Lauf dieses PCs aus der Datenbank") + " (" + DatumKurz(erg.Datum) + ") mit Urteil, Vergleich zur Referenz und den Hauptwerten je Komponente.");
    }

    // Letzter Lauf dieses PCs aus der Datenbank als Ergebnis (nach dem Programmstart, nach Änderungen an der Datenbank
    // und nach einem Lauf ohne Ergebnisseite). Ein Ergebnis des eben beendeten Laufs bleibt stehen.
    internal void LadeErgebnisAusDatenbank()
    {
        if (running || (erg != null && erg.VomLauf)) return;
        DbEntry e = GetCurrentPcLastRun();
        erg = e != null ? ErgebnisAusEintrag(e) : null;
        ergVeraltet = erg != null; ergBewegung = false;
        SetzeErgebnisNav();
        if (curPage == ergebnisPage && ergebnisPage >= 0)
        {
            if (erg == null) ShowPage(0);
            else { FuelleErgebnis(); OrdneErgebnis(); }
        }
    }

    // Öffnet die Ergebnisseite mit dem letzten Lauf (Karte "Letzter Lauf" der Startseite, Kasten auf der Seite Benchmark)
    internal void ZeigeLetztesErgebnis()
    {
        if (running) return;
        if (erg == null) LadeErgebnisAusDatenbank();
        if (erg != null && ergebnisPage >= 0) ShowPage(ergebnisPage);
    }

    // "Referenz ändern": Seite Benchmark bei der Wahl der Referenz, darüber das Band "Zurück zum Ergebnis". Das Band
    // verschwindet beim nächsten Seitenwechsel und beim Start eines Laufs.
    internal void ZeigeReferenzwahl()
    {
        if (running) return;
        ShowPage(2);
        if (bandErg != null && erg != null)
        {
            SetText(lblBandErg, String.Format("Lauf vom {0}{1}  ·  eine andere Referenz gilt ab dem nächsten Lauf", DatumKurz(erg.Datum),
                erg.Score > 0 ? "  ·  " + erg.Score.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("de-DE")) + " Punkte" : ""));
            bandErg.Visible = true; bandErgGezeigt = true;
            OrdneModulSeiten();
        }
        if (cmbRef != null) { cmbRef.Focus(); content.ScrollControlIntoView(cmbRef); }
    }

    void VerbergeErgebnisBand()
    {
        bandErgGezeigt = false;
        if (bandErg != null) bandErg.Visible = false;
        OrdneModulSeiten();
    }

    // "Erneut messen": derselbe Weg wie der Startknopf. Ist kein Modul angehakt (Ergebnis aus der Datenbank nach dem
    // Programmstart), werden die messenden Module des gezeigten Laufs angehakt; Wartung und Optimierung nie von selbst.
    void ErneutMessen()
    {
        if (running) return;
        if (SelectedModules().Count == 0)
        {
            string m = erg != null && erg.Eintrag != null ? erg.Eintrag.Module : "";
            bool etwas = false;
            string[] namen = new string[] { "Diagnose", "Benchmark", "Lasttest" };
            for (int i = 0; i < namen.Length; i++) if (m.IndexOf(namen[i], StringComparison.OrdinalIgnoreCase) >= 0) { nav[i + 1].Checked = true; etwas = true; }
            if (!etwas) nav[2].Checked = true;
        }
        StartRun();
    }

    // Zeigt die Ergebnisseite nach einem Lauf. Punktzahl, Balken und Ringe laufen dabei einmal auf ihren Wert (ab v4.07),
    // bei jeder späteren Rückkehr steht sofort der Endstand (ab v4.08).
    void ZeigeErgebnis()
    {
        if (resultDashboard == null || ergebnisPage < 0) return;
        ShowPage(ergebnisPage);
        contentHost.Visible = true; if (foot != null) foot.Visible = true;
    }

    // Die Werte selbst stehen schon fest; ohne Bewegung (Einstellungen, Remotesitzung) erscheint sofort der Endstand.
    void StarteErgebnisBewegung()
    {
        System.Globalization.CultureInfo de = System.Globalization.CultureInfo.GetCultureInfo("de-DE");
        int ziel = resScore;
        Bewegung.Starte(resultDashboard, Bewegung.DauerErgebnis, delegate(double t) {
            if (ziel > 0 && lblResScore != null) lblResScore.Text = ((int)Math.Round(ziel * t)).ToString("N0", de);
            foreach (PercentBar pb in resBalken) if (!pb.IsDisposed) pb.Anteil = t;
            foreach (RingAnzeige ra in resRinge) if (!ra.IsDisposed) ra.Anteil = t;
        });
    }

    // Beim Wechsel auf die Seite (aus ShowPage): Inhalt nur aufbauen, wenn sich das Ergebnis geändert hat
    void ErgebnisSeiteGezeigt()
    {
        if (erg == null) return;
        if (ergVeraltet) FuelleErgebnis();
        OrdneErgebnis();
        if (ergBewegung) { ergBewegung = false; StarteErgebnisBewegung(); }
    }

    // Nach einem Lauf: Ergebnis aus dem Lauf übernehmen und die Seite aufbauen; die Bewegung startet beim Anzeigen
    void PopulateResultDashboard()
    {
        ReloadDb();
        DbEntry e = null;
        if (!String.IsNullOrEmpty(outDir))
            foreach (DbEntry dbe in db) { if (ErgebnisOrdnerPasst(outDir, dbe.Ordner)) { e = dbe; break; } }
        erg = ErgebnisAusLauf(e);
        FuelleErgebnis();
        ergBewegung = true;
        SetzeErgebnisNav();
        FuelleLetztesErgebnis();
    }

    void FuelleErgebnis()
    {
        if (erg == null || resultDashboard == null) return;
        ergVeraltet = false; ergebnisAufbauten++;
        ErgebnisDaten d = erg; DbEntry e = d.Eintrag;
        System.Globalization.CultureInfo de = System.Globalization.CultureInfo.GetCultureInfo("de-DE");
        resultDashboard.SuspendLayout();
        lblDashMeta.Text = d.Meta;

        // Urteil aus den Befunden des Laufs
        if (d.NK < 0) { lblResVerdict.Text = "Befunde nicht gespeichert"; lblResVerdict.ForeColor = UI.Muted; lblResCounts.Text = "Dieser Eintrag trägt keine Befunde (Import oder älterer Lauf)."; }
        else
        {
            if (d.NK > 0) { lblResVerdict.Text = d.NK == 1 ? "1 kritischer Befund" : d.NK + " kritische Befunde"; lblResVerdict.ForeColor = UI.Crit; }
            else if (d.NW > 0) { lblResVerdict.Text = d.NW == 1 ? "1 Warnung" : d.NW + " Warnungen"; lblResVerdict.ForeColor = UI.Warn; }
            else { lblResVerdict.Text = "Keine Auffälligkeiten"; lblResVerdict.ForeColor = UI.Ok; }
            lblResCounts.Text = String.Format("{0} kritisch  ·  {1} Warnungen  ·  {2} Hinweise", d.NK, d.NW, d.NI);
        }

        resScore = d.Score; resBalken.Clear(); resRinge.Clear();
        lblResScore.Text = d.Score > 0 ? d.Score.ToString("N0", de) : "";
        lblResScoreSub.Text = d.Score > 0 ? "Punkte gesamt" : "";
        lblResScore.ForeColor = UI.Accent;

        pnlDashPills.SuspendLayout();
        while (pnlDashPills.Controls.Count > 0) { Control c = pnlDashPills.Controls[0]; pnlDashPills.Controls.RemoveAt(0); c.Dispose(); }
        if (e != null)
        {
            if (!String.IsNullOrEmpty(e.CpuDisplay)) pnlDashPills.Controls.Add(new PillLabel(e.CpuDisplay));
            if (!String.IsNullOrEmpty(e.Gpu)) pnlDashPills.Controls.Add(new PillLabel(e.Gpu));
            if (!String.IsNullOrEmpty(e.Ram)) pnlDashPills.Controls.Add(new PillLabel(e.Ram));
            foreach (string dp in DiskPills(e.Disk)) { PillLabel pl = new PillLabel(dp); Tip(pl, e.Disk); pnlDashPills.Controls.Add(pl); }
        }
        pnlDashPills.ResumeLayout(false);

        // Wichtigste Befunde: kritische zuerst, dann Warnungen, höchstens fünf; der Verweis auf den Bericht steht immer
        // als letztes Element darunter (bis v4.07 konnte er über dem Text landen)
        FlowLayoutPanel pf = kResFind.Inhalt;
        kResFind.Leere(lnkResAll);
        pf.SuspendLayout();
        int shown = 0;
        foreach (string[] b in d.Befunde)
        {
            if (shown >= 5) break;
            Panel r = new Panel(); r.BackColor = UI.Panel; r.Margin = new Padding(0, 0, 0, UI.S(6));
            Badge bd = new Badge(b[0] == "KRITISCH" ? "Kritisch" : "Warnung", b[0] == "KRITISCH" ? "crit" : "warn"); bd.Width = Math.Max(bd.Width, UI.S(72)); bd.Location = new Point(0, UI.S(1));
            Label lt = Lbl(b[2], UI.SchriftHinweis, false, UI.Text); lt.AutoSize = false; lt.Location = new Point(bd.Width + UI.S(10), UI.S(1));
            Tip(lt, (b[1].Length > 0 ? b[1] + ": " : "") + b[2]);
            r.Controls.Add(bd); r.Controls.Add(lt);
            // Text bricht an der Breite der Karte um, die Zeile wird so hoch wie der Text
            r.Resize += delegate {
                int tw = Math.Max(UI.S(120), r.Width - lt.Left);
                int th = Math.Max(bd.Height, TextRenderer.MeasureText(lt.Text, lt.Font, new Size(tw, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + UI.S(2));
                lt.Size = new Size(tw, th);
                if (r.Height != th + UI.S(2)) r.Height = th + UI.S(2);
            };
            r.Height = bd.Height + UI.S(2);
            kResFind.Zeile(r);
            shown++;
        }
        string hinweis = "";
        if (d.NK < 0) hinweis = "Die Befunde dieses Laufs sind nicht gespeichert.";
        else if (shown == 0 && d.NK + d.NW > 0) hinweis = "Die Befunde dieses Laufs stehen im Bericht.";
        else if (shown == 0) hinweis = d.NI > 0 ? "Keine kritischen Befunde und keine Warnungen. " + (d.NI == 1 ? "1 Hinweis steht" : d.NI + " Hinweise stehen") + " im Bericht." : "Keine kritischen Befunde, keine Warnungen und keine Hinweise.";
        else if (d.NK + d.NW > shown) hinweis = String.Format("und {0} weitere", d.NK + d.NW - shown);
        if (hinweis.Length > 0) { Label l = Lbl(hinweis, shown == 0 ? UI.SchriftText : UI.SchriftHinweis, false, shown == 0 ? UI.Text : UI.Muted); l.Margin = new Padding(0, 0, 0, UI.S(2)); kResFind.Frei(l); }
        lnkResAll.Visible = File.Exists(d.Html);
        pf.Controls.Add(lnkResAll); pf.Controls.SetChildIndex(lnkResAll, pf.Controls.Count - 1);
        pf.ResumeLayout(true);

        // Vergleich mit der Referenz je Komponente (nur mit Benchmark in diesem Lauf)
        FlowLayoutPanel pc = kResCmp.Inhalt;
        kResCmp.Leere(lblResRef);
        pc.SuspendLayout();
        // Verweise der Seite folgen Farbschema und Akzent auch nach einem Wechsel
        foreach (LinkLabel ll in new LinkLabel[] { lnkResRef, lnkResAll }) if (ll != null) { ll.LinkColor = UI.Accent; ll.ActiveLinkColor = UI.AccentHover; }
        bool bench = e != null && e.HasBench;
        if (d.HatVergleich)
        {
            lblResRef.Text = "100 % = " + RefLabelText(); lblResRef.ForeColor = UI.Muted;
            kResCmp.Frei(lblResRef);
            for (int g = 0; g < resGruppen.Length; g++)
            {
                double pct; bool hat = d.Pct.TryGetValue(resGruppen[g], out pct);
                FlowLayoutPanel r = new FlowLayoutPanel(); r.AutoSize = true; r.WrapContents = false; r.BackColor = UI.Panel; r.Margin = new Padding(0, 0, 0, UI.S(6));
                Label ln = Lbl(resNamen[g], UI.SchriftText, false, UI.Text); ln.AutoSize = false; ln.Size = new Size(UI.S(120), UI.S(20)); ln.Margin = new Padding(0);
                r.Controls.Add(ln);
                if (!hat)
                {
                    // Gruppe in diesem Lauf nicht gemessen (abgewählt oder übersprungen): Zeile bleibt, damit das Fehlen auffällt
                    Label lx = Lbl("nicht gemessen", UI.SchriftText, false, UI.Muted); lx.Margin = new Padding(0);
                    r.Controls.Add(lx); kResCmp.Frei(r);
                    continue;
                }
                PercentBar pb = new PercentBar(); pb.Percent = pct; pb.Size = new Size(UI.S(120), UI.S(10)); pb.Margin = new Padding(0, UI.S(6), UI.S(10), 0);
                Tip(pb, "Strich = Referenz (100 %). Langsamer als die Referenz heißt nicht defekt, nur langsamer als dieser PC.");
                Label lv = Lbl(pct.ToString("0") + " %  " + RatingWord(pct), UI.SchriftText, true, UI.Text); lv.Margin = new Padding(0);
                resBalken.Add(pb);
                r.Controls.Add(pb); r.Controls.Add(lv);
                kResCmp.Frei(r);
            }
        }
        else if (bench)
        {
            // Lauf vor v4.08 oder ohne Referenz: nichts nachrechnen, sondern sagen, was fehlt (Vorgabe 11)
            Label l = Lbl("Der Vergleich mit der Referenz ist bei diesem Lauf nicht gespeichert (Lauf vor Version 4.08 oder ohne Referenz). Die Prozentwerte stehen im Bericht; der nächste Lauf speichert sie mit.", UI.SchriftText, false, UI.Muted); l.Margin = new Padding(0, 0, 0, UI.S(4));
            kResCmp.Frei(l);
        }
        pc.ResumeLayout(true);
        ErgZeige(kResCmp.Karte, d.HatVergleich || bench);

        // Komponenten: nur mit Messwerten
        while (pnlResTiles.Controls.Count > 0) { Control c = pnlResTiles.Controls[0]; pnlResTiles.Controls.RemoveAt(0); c.Dispose(); }
        if (e != null)
        {
            double mt = e.Get("CPU|MT"), st = e.Get("CPU|ST");
            if (mt > 0) pnlResTiles.Controls.Add(ResTile("Prozessor", UI.IcoCpu, mt.ToString("N0", de), "Punkte, alle Kerne", st > 0 ? "Ein Kern: " + st.ToString("N0", de) + " Punkte" : "", ErgPct("CPU")));
            double rend = e.Get("GPU|REND"), rend1 = e.Get("GPU|REND1"), vmb = e.Get("GPU|VMB");
            if (rend > 0) pnlResTiles.Controls.Add(ResTile("Grafik", UI.IcoGpu, rend.ToString("N1", de) + " FPS", "Bilder je Sekunde im Rendertest", rend1 > 0 ? "1 % der langsamsten Bilder: " + rend1.ToString("N1", de) + " FPS" : "", ErgPct("GPU")));
            else if (vmb > 0) pnlResTiles.Controls.Add(ResTile("Grafik", UI.IcoGpu, vmb.ToString("N1", de) + " GB/s", "Speicherdurchsatz", "", ErgPct("GPU")));
            double rL = e.Get("RAM|Lesen"), rLat = e.Get("RAM|Latenz");
            if (rL > 0) pnlResTiles.Controls.Add(ResTile("Arbeitsspeicher", UI.IcoRam, rL.ToString("N1", de) + " GB/s", "Lesen", rLat > 0 ? "Latenz: " + rLat.ToString("N0", de) + " ns" : "", ErgPct("RAM")));
            double sr = 0, r1 = 0;
            foreach (KeyValuePair<string, double> kv in e.Werte)
            {
                if (kv.Key.StartsWith("DISK|") && kv.Key.EndsWith("|SR") && kv.Value > sr) sr = kv.Value;
                if (kv.Key.StartsWith("DISK|") && kv.Key.EndsWith("|R1") && kv.Value > r1) r1 = kv.Value;
            }
            if (sr > 0) pnlResTiles.Controls.Add(ResTile("Laufwerk", UI.IcoDisk, sr.ToString("N0", de) + " MB/s", "Lesen am Stück, schnellstes Laufwerk", r1 > 0 ? "4K-Zufallszugriff: " + r1.ToString("N0", de) + " IOPS" : "", ErgPct("Laufwerke")));
        }
        ErgZeige(btnDashVerlauf, e != null);
        ErgZeige(btnDashKi, File.Exists(d.Ki));
        ErgZeige(btnDashFolder, Directory.Exists(d.Ordner));
        btnDashOpenHtml.Enabled = File.Exists(d.Html);
        resultDashboard.ResumeLayout(false);
        OrdneErgebnis();
    }

    // Prozent der Referenz für eine Gruppe des gezeigten Ergebnisses, sonst NaN
    double ErgPct(string g) { double p; return erg != null && erg.Pct.TryGetValue(g, out p) ? p : double.NaN; }
}
