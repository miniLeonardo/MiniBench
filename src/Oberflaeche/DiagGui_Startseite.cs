// Startseite mit Steckbrief und Schnellprüfungen (Teilklasse DiagGui)
public partial class DiagGui
{
    // ============================================================ Startseite (ab v3.72)
    class QuickFixItem
    {
        public string Level = "ok", Badge = "", Title = "", Desc = "", ButtonText = "", Tip = "";
        public Action Action;
    }


    Control BuildStartPage()
    {
        Panel f = new Panel(); f.Dock = DockStyle.Fill; f.BackColor = UI.Panel; f.AutoScroll = true;
        BuildStartContent(f);
        return f;
    }

    DbEntry GetCurrentPcLastRun()
    {
        DbEntry newest = null;
        DateTime newestDt = DateTime.MinValue;
        string curPc = Environment.MachineName;
        foreach (DbEntry e in db)
        {
            bool isThisPc = false;
            if (!String.IsNullOrEmpty(e.Computer) && (String.Equals(e.Computer, curPc, StringComparison.OrdinalIgnoreCase) || e.Computer.StartsWith(curPc + "_", StringComparison.OrdinalIgnoreCase)))
                isThisPc = true;
            else if (!String.IsNullOrEmpty(e.Name) && String.Equals(e.Name, curPc, StringComparison.OrdinalIgnoreCase))
                isThisPc = true;

            if (isThisPc)
            {
                DateTime dt;
                if (DateTime.TryParse(e.Datum, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt) ||
                    DateTime.TryParseExact(e.Datum, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
                    DateTime.TryParseExact(e.Datum, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
                    DateTime.TryParseExact(e.Datum, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    if (dt > newestDt) { newestDt = dt; newest = e; }
                }
                else if (newest == null)
                {
                    newest = e;
                }
            }
        }
        return newest;
    }

    void BuildStartContent(Panel f)
    {
        f.SuspendLayout();
        f.Controls.Clear();
        DbEntry lastRunThisPc = GetCurrentPcLastRun();
        if (lastRunThisPc == null)
        {
            BuildStartPageA(f);
        }
        else
        {
            BuildStartPageB(f, lastRunThisPc);
        }
        f.ResumeLayout(true);
    }

    void BuildStartPageA(Panel f)
    {
        FlowLayoutPanel p = new FlowLayoutPanel();
        p.FlowDirection = FlowDirection.TopDown;
        p.WrapContents = false;
        p.AutoSize = true;
        p.BackColor = UI.Panel;
        p.Margin = new Padding(0);
        p.Padding = new Padding(0);

        // 1. Kopfbereich & Steckbrief (zweispaltig, Breite StartGruppenbreite)
        TableLayoutPanel headGrid = new TableLayoutPanel();
        headGrid.Width = UI.S(UI.StartGruppenbreite);
        headGrid.Height = UI.S(148);
        headGrid.ColumnCount = 2;
        headGrid.RowCount = 1;
        headGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
        headGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
        headGrid.Margin = new Padding(0, 0, 0, UI.S(14));

        FlowLayoutPanel leftHead = new FlowLayoutPanel();
        leftHead.Dock = DockStyle.Fill;
        leftHead.FlowDirection = FlowDirection.TopDown;
        leftHead.WrapContents = false;
        leftHead.BackColor = UI.Panel;
        leftHead.Margin = new Padding(0);

        Label lblTitle = Lbl("Willkommen bei Leos Minibench", 15.5f, true, UI.Text);
        lblTitle.Margin = new Padding(0, 0, 0, UI.S(6));
        leftHead.Controls.Add(lblTitle);

        Label lblDesc = Lbl("Prüft, misst und pflegt diesen PC. Wähle unten eine Aufgabe oder links einen oder mehrere Abläufe und starte unten rechts. Am Ende steht ein Bericht mit allen Befunden.", 9.25f, false, UI.Muted);
        lblDesc.MaximumSize = new Size(UI.S(460), 0);
        lblDesc.Margin = new Padding(0, 0, 0, UI.S(10));
        leftHead.Controls.Add(lblDesc);

        FlowLayoutPanel statusRow = new FlowLayoutPanel();
        statusRow.AutoSize = true;
        statusRow.WrapContents = false;
        statusRow.BackColor = UI.Panel;
        statusRow.Margin = new Padding(0);

        Label dot1 = Lbl("● ", 9f, true, UI.Ok); dot1.Margin = new Padding(0);
        Label txt1 = Lbl("Prüfungen lesen nur   ", 8.75f, false, UI.Muted); txt1.Margin = new Padding(0);
        Label dot2 = Lbl("● ", 9f, true, UI.Warn); dot2.Margin = new Padding(UI.S(8), 0, 0, 0);
        Label txt2 = Lbl("Änderungen sind gekennzeichnet und umkehrbar", 8.75f, false, UI.Muted); txt2.Margin = new Padding(0);
        Tip(dot1, "Diagnose und Benchmark lesen nur Systemeigenschaften und verändern keine Einstellungen.");
        Tip(txt1, "Diagnose und Benchmark lesen nur Systemeigenschaften und verändern keine Einstellungen.");
        Tip(dot2, "Wartungs- und Optimierungsmaßnahmen sind gekennzeichnet und lassen sich rückgängig machen.");
        Tip(txt2, "Wartungs- und Optimierungsmaßnahmen sind gekennzeichnet und lassen sich rückgängig machen.");
        statusRow.Controls.Add(dot1); statusRow.Controls.Add(txt1); statusRow.Controls.Add(dot2); statusRow.Controls.Add(txt2);
        leftHead.Controls.Add(statusRow);
        headGrid.Controls.Add(leftHead, 0, 0);

        Panel cardPc = new Panel();
        cardPc.Dock = DockStyle.Fill;
        cardPc.BackColor = UI.Panel;
        cardPc.Padding = new Padding(UI.S(14), UI.S(10), UI.S(14), UI.S(10));
        cardPc.Paint += delegate(object sender, PaintEventArgs pe) {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UI.Round(new RectangleF(0.5f, 0.5f, cardPc.Width - 1.5f, cardPc.Height - 1.5f), UI.SF(8)))
            using (Pen pen = new Pen(UI.Line)) pe.Graphics.DrawPath(pen, path);
        };

        TableLayoutPanel pcTop = new TableLayoutPanel();
        pcTop.Dock = DockStyle.Top;
        pcTop.Height = UI.S(24);
        pcTop.ColumnCount = 2;
        pcTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        pcTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        Label lblDpc = Lbl("DIESER PC", 8f, true, UI.Muted);
        lblDpc.Margin = new Padding(0);
        LinkLabel lnkInvA = new LinkLabel();
        lnkInvA.Text = "Inventar";
        lnkInvA.Font = new Font("Segoe UI", 8.5f);
        lnkInvA.LinkColor = UI.Accent;
        lnkInvA.ActiveLinkColor = UI.AccentHover;
        lnkInvA.LinkBehavior = LinkBehavior.HoverUnderline;
        lnkInvA.Anchor = AnchorStyles.Right;
        lnkInvA.AutoSize = true;
        Tip(lnkInvA, "Öffnet das detaillierte Hardware-Inventar dieses Rechners.");
        lnkInvA.LinkClicked += delegate { ShowInventoryDialog(); };
        pcTop.Controls.Add(lblDpc, 0, 0);
        pcTop.Controls.Add(lnkInvA, 1, 0);
        cardPc.Controls.Add(pcTop);

        TableLayoutPanel pcGrid = new TableLayoutPanel();
        pcGrid.Dock = DockStyle.Fill;
        pcGrid.ColumnCount = 2;
        pcGrid.RowCount = 5;
        pcGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(75)));
        pcGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        string cleanCpu = DbEntry.CleanCpuName(HwCpu);
        string cpuSpec = cleanCpu.Length > 0 ? (cleanCpu + (HwCores > 0 ? " · " + HwCores + " Kerne / " + HwThreads + " Threads" : "")) : "Standard-Prozessor";
        string primaryGpu = GetPrimaryGpuName(); string gpuSpec = primaryGpu.Length > 0 ? primaryGpu : "Grafikeinheit";
        string ramSpec = HwRam.Length > 0 ? HwRam : "Arbeitsspeicher";
        string diskSpec = GetDisksSummary();
        string boardSpec = HwBoard.Length > 0 ? HwBoard : "Standard-Mainboard";

        string[][] specs = new string[][] {
            new string[] { "Prozessor", cpuSpec },
            new string[] { "Grafik", gpuSpec },
            new string[] { "Speicher", ramSpec },
            new string[] { "Laufwerke", diskSpec },
            new string[] { "Mainboard", boardSpec }
        };

        for (int i = 0; i < specs.Length; i++)
        {
            Label k = Lbl(specs[i][0], 8.25f, false, UI.Muted);
            k.Margin = new Padding(0, UI.S(1), UI.S(4), UI.S(1));
            Label v = Lbl(specs[i][1], 8.25f, false, UI.Text);
            v.Margin = new Padding(0, UI.S(1), 0, UI.S(1));
            v.AutoEllipsis = true;
            Tip(k, specs[i][0] + ": " + specs[i][1]);
            Tip(v, specs[i][0] + ": " + specs[i][1]);
            pcGrid.Controls.Add(k, 0, i);
            pcGrid.Controls.Add(v, 1, i);
        }
        cardPc.Controls.Add(pcGrid);
        pcGrid.BringToFront();
        headGrid.Controls.Add(cardPc, 1, 0);
        p.Controls.Add(headGrid);

        // 2. Aufgabenkarten
        Label lblStartHead = Lbl("Womit möchtest du anfangen?", 11.5f, true, UI.Text);
        lblStartHead.Margin = new Padding(0, UI.S(4), 0, UI.S(8));
        Tip(lblStartHead, "Wähle eine Aufgabe oder hake links Module an.");
        p.Controls.Add(lblStartHead);

        TableLayoutPanel tasksGrid = new TableLayoutPanel();
        tasksGrid.Width = UI.S(UI.StartGruppenbreite);
        tasksGrid.Height = UI.S(215);
        tasksGrid.ColumnCount = 4;
        tasksGrid.RowCount = 1;
        tasksGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        tasksGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        tasksGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        tasksGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        tasksGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        tasksGrid.Margin = new Padding(0, 0, 0, UI.S(16));

        StartTaskCard c1 = new StartTaskCard("PC durchchecken", "Hardware, Treiber, Leistung und Stabilität prüfen. Ergebnis ist ein ausführlicher Bericht.", "Diagnose, Bench., Last.", "Lesen", "ok", UI.IcoDiag, true);
        c1.Dock = DockStyle.Fill; c1.Margin = new Padding(0, 0, UI.S(10), 0);
        Tip(c1, "Startet Diagnose, Benchmark und Lasttest zur vollständigen Überprüfung von Hardware, Leistung und Stabilität.");
        c1.CardClicked += delegate { SelectTask("PC durchchecken"); };

        StartTaskCard c2 = new StartTaskCard("Leistung messen", "Prozessor, Speicher, Grafik und Laufwerke messen und mit anderen Systemen vergleichen.", "Benchmark · ca. 7 Min.", "Lesen", "ok", UI.IcoSpeed, false);
        c2.Dock = DockStyle.Fill; c2.Margin = new Padding(0, 0, UI.S(10), 0);
        Tip(c2, "Startet den Leistungsbenchmark für CPU, RAM, Grafikkarte und Datenträger.");
        c2.CardClicked += delegate { SelectTask("Leistung messen"); };

        StartTaskCard c3 = new StartTaskCard("Stabilität prüfen", "Volllast mit Temperatur, Takt und Fehlerprüfung. Findet Hitze- und Stabilitätsprobleme.", "Lasttest · ab 2 Min.", "Lesen", "ok", UI.IcoFlame, false);
        c3.Dock = DockStyle.Fill; c3.Margin = new Padding(0, 0, UI.S(10), 0);
        Tip(c3, "Startet den Stabilitätstest unter Volllast zur Erkennung von Überhitzung und Instabilitäten.");
        c3.CardClicked += delegate { SelectTask("Stabilität prüfen"); };

        StartTaskCard c4 = new StartTaskCard("Pflegen und beschleunigen", "Reparieren, aufräumen, Einstellungen verbessern. Jede Änderung lässt sich rückgängig machen.", "Wartung, Optimierung", "Ändert", "warn", UI.IcoWartung, false);
        c4.Dock = DockStyle.Fill; c4.Margin = new Padding(0);
        Tip(c4, "Startet Wartung und Optimierung zur Systembereinigung und Leistungsverbesserung.");
        c4.CardClicked += delegate { SelectTask("Pflegen und beschleunigen"); };

        tasksGrid.Controls.Add(c1, 0, 0);
        tasksGrid.Controls.Add(c2, 1, 0);
        tasksGrid.Controls.Add(c3, 2, 0);
        tasksGrid.Controls.Add(c4, 3, 0);
        p.Controls.Add(tasksGrid);

        // 3. Erklärbereich (zweispaltig, Breite StartGruppenbreite)
        TableLayoutPanel explGrid = new TableLayoutPanel();
        explGrid.Width = UI.S(UI.StartGruppenbreite);
        explGrid.Height = UI.S(154);
        explGrid.ColumnCount = 2;
        explGrid.RowCount = 1;
        explGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
        explGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
        explGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        explGrid.Margin = new Padding(0, 0, 0, UI.S(16));

        Panel cardSteps = new Panel();
        cardSteps.Dock = DockStyle.Fill;
        cardSteps.BackColor = UI.Panel;
        cardSteps.Padding = new Padding(UI.S(14), UI.S(12), UI.S(14), UI.S(12));
        cardSteps.Margin = new Padding(0, 0, UI.S(10), 0);
        cardSteps.Paint += delegate(object sender, PaintEventArgs pe) {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UI.Round(new RectangleF(0.5f, 0.5f, cardSteps.Width - 1.5f, cardSteps.Height - 1.5f), UI.SF(8)))
            using (Pen pen = new Pen(UI.Line)) pe.Graphics.DrawPath(pen, path);
        };

        Label lblStpHead = Lbl("So läuft ein Durchgang", 10.5f, true, UI.Text);
        lblStpHead.Dock = DockStyle.Top;
        lblStpHead.Height = UI.S(24);
        Tip(lblStpHead, "Drei einfache Schritte zur Durchführung.");
        cardSteps.Controls.Add(lblStpHead);

        TableLayoutPanel stepCols = new TableLayoutPanel();
        stepCols.Dock = DockStyle.Fill;
        stepCols.ColumnCount = 3;
        stepCols.RowCount = 1;
        stepCols.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        stepCols.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        stepCols.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.4f));

        string[][] stepData = new string[][] {
            new string[] { "1", "Ablauf wählen", "Links abhaken, mehrere lassen sich kombinieren. Die Seite daneben zeigt die Optionen." },
            new string[] { "2", "Starten", "Unten rechts. Die Fußleiste zeigt vorher die Gesamtdauer." },
            new string[] { "3", "Bericht lesen", "Öffnet sich am Ende. Befunde sind nach kritisch, Warnung und Hinweis sortiert." }
        };

        for (int i = 0; i < 3; i++)
        {
            FlowLayoutPanel sp = new FlowLayoutPanel();
            sp.Dock = DockStyle.Fill;
            sp.FlowDirection = FlowDirection.TopDown;
            sp.WrapContents = false;
            sp.BackColor = UI.Panel;
            sp.Margin = new Padding(0, 0, UI.S(6), 0);

            FlowLayoutPanel badgeRow = new FlowLayoutPanel();
            badgeRow.AutoSize = true;
            badgeRow.WrapContents = false;
            badgeRow.BackColor = UI.Panel;
            badgeRow.Margin = new Padding(0, 0, 0, UI.S(4));

            Badge bNum = new Badge(stepData[i][0], "info");
            bNum.Margin = new Padding(0, 0, UI.S(6), 0);
            Tip(bNum, "Schritt " + stepData[i][0]);
            Label stTitle = Lbl(stepData[i][1], 9f, true, UI.Text);
            Tip(stTitle, stepData[i][1]);
            badgeRow.Controls.Add(bNum);
            badgeRow.Controls.Add(stTitle);
            sp.Controls.Add(badgeRow);

            Label stDesc = Lbl(stepData[i][2], 8f, false, UI.Muted);
            stDesc.MaximumSize = new Size(UI.S(125), 0);
            Tip(stDesc, stepData[i][2]);
            sp.Controls.Add(stDesc);
            stepCols.Controls.Add(sp, i, 0);
        }
        cardSteps.Controls.Add(stepCols);
        stepCols.BringToFront();
        explGrid.Controls.Add(cardSteps, 0, 0);

        Panel cardTools = new Panel();
        cardTools.Dock = DockStyle.Fill;
        cardTools.BackColor = UI.Panel;
        cardTools.Padding = new Padding(UI.S(14), UI.S(12), UI.S(14), UI.S(12));
        cardTools.Margin = new Padding(0);
        cardTools.Paint += delegate(object sender, PaintEventArgs pe) {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UI.Round(new RectangleF(0.5f, 0.5f, cardTools.Width - 1.5f, cardTools.Height - 1.5f), UI.SF(8)))
            using (Pen pen = new Pen(UI.Line)) pe.Graphics.DrawPath(pen, path);
        };

        Label lblToolsHead = Lbl("Werkzeuge", 10.5f, true, UI.Text);
        lblToolsHead.Dock = DockStyle.Top;
        lblToolsHead.Height = UI.S(24);
        Tip(lblToolsHead, "Schnellzugriff auf portable Hilfswerkzeuge und Übersichten.");
        cardTools.Controls.Add(lblToolsHead);

        TableLayoutPanel toolRows = new TableLayoutPanel();
        toolRows.Dock = DockStyle.Fill;
        toolRows.RowCount = 3;
        toolRows.ColumnCount = 1;
        toolRows.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));
        toolRows.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));
        toolRows.RowStyles.Add(new RowStyle(SizeType.Percent, 33.4f));

        Control rowSensA = CreateToolItem(UI.IcoSens, "Sensoren live", "Temperatur, Takt, Lüfter laufend ansehen", delegate { ShowPage(7); }, "Öffnet die Live-Sensoranzeige für Temperaturen, Taktraten und Spannungen.");
        Control rowDbA = CreateToolItem(UI.IcoDb, "Vergleichsdatenbank", db.Count + " Systeme, im Dashboard vergleichen", delegate { ShowPage(9); }, "Öffnet die Vergleichsdatenbank aller gespeicherten Messungen.");
        Control rowToolsA = CreateToolItem(UI.IcoTools, "Tools", "Portable Werkzeuge, Programme, BIOS-Neustart", delegate { ShowPage(6); }, "Öffnet die Werkzeugseite mit portablen Hilfsprogrammen und Windows-Schnellstartern.");

        toolRows.Controls.Add(rowSensA, 0, 0);
        toolRows.Controls.Add(rowDbA, 0, 1);
        toolRows.Controls.Add(rowToolsA, 0, 2);
        cardTools.Controls.Add(toolRows);
        toolRows.BringToFront();
        explGrid.Controls.Add(cardTools, 1, 0);
        p.Controls.Add(explGrid);

        // 4. Abschlussleiste
        p.Controls.Add(BuildStartFooter());

        f.Controls.Add(p);
    }

    void BuildStartPageB(Panel f, DbEntry lastRun)
    {
        FlowLayoutPanel p = new FlowLayoutPanel();
        p.FlowDirection = FlowDirection.TopDown;
        p.WrapContents = false;
        p.AutoSize = true;
        p.BackColor = UI.Panel;
        p.Margin = new Padding(0);
        p.Padding = new Padding(0, 0, 0, UI.S(20));

        // 1. Kopfzeile (ohne redundanten Inventar-Link)
        Label lblWelcome = Lbl("Willkommen zurück", 15.5f, true, UI.Text);
        lblWelcome.Margin = new Padding(0, 0, 0, UI.S(6));
        Tip(lblWelcome, "Startübersicht für dieses bereits früher geprüfte System.");
        p.Controls.Add(lblWelcome);

        // Pills-Reihe mit Hardwaredaten
        FlowLayoutPanel pillsRow = new FlowLayoutPanel();
        pillsRow.Width = UI.S(UI.StartGruppenbreite);
        pillsRow.AutoSize = true;
        pillsRow.WrapContents = true;
        pillsRow.BackColor = UI.Panel;
        pillsRow.Margin = new Padding(0, 0, 0, UI.S(16));

        string cleanCpu = DbEntry.CleanCpuName(HwCpu);
        if (cleanCpu.Length > 0) { PillLabel pill = new PillLabel(cleanCpu); Tip(pill, "Prozessor: " + cleanCpu); pillsRow.Controls.Add(pill); }
        string dispGpu = GetPrimaryGpuName(); if (dispGpu.Length > 0) { PillLabel pill = new PillLabel(dispGpu); Tip(pill, "Grafikkarte: " + dispGpu); pillsRow.Controls.Add(pill); }
        if (HwRam.Length > 0) { PillLabel pill = new PillLabel(HwRam); Tip(pill, "Arbeitsspeicher: " + HwRam); pillsRow.Controls.Add(pill); }
        string disksSummary = GetDisksSummary();
        if (disksSummary.Length > 0) { PillLabel pill = new PillLabel(disksSummary); Tip(pill, "Laufwerke: " + disksSummary); pillsRow.Controls.Add(pill); }
        if (HwBoard.Length > 0) { PillLabel pill = new PillLabel(HwBoard); Tip(pill, "Mainboard: " + HwBoard); pillsRow.Controls.Add(pill); }
        p.Controls.Add(pillsRow);

        // Bestimme letzten Benchmark und Lasttest DIESES PCs
        string lastBenchDate = "";
        string lastLoadDate = "";
        DateTime lastBenchDt = DateTime.MinValue;
        DateTime lastLoadDt = DateTime.MinValue;
        string curPc = Environment.MachineName;

        foreach (DbEntry e in db)
        {
            bool isThisPc = false;
            if (!String.IsNullOrEmpty(e.Computer) && (String.Equals(e.Computer, curPc, StringComparison.OrdinalIgnoreCase) || e.Computer.StartsWith(curPc + "_", StringComparison.OrdinalIgnoreCase)))
                isThisPc = true;
            else if (!String.IsNullOrEmpty(e.Name) && String.Equals(e.Name, curPc, StringComparison.OrdinalIgnoreCase))
                isThisPc = true;

            if (isThisPc)
            {
                DateTime dt;
                if (DateTime.TryParse(e.Datum, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt) ||
                    DateTime.TryParseExact(e.Datum, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
                    DateTime.TryParseExact(e.Datum, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
                    DateTime.TryParseExact(e.Datum, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    if (e.HasBench && dt > lastBenchDt) { lastBenchDt = dt; lastBenchDate = dt.ToString("dd.MM."); }
                    if (e.Module.IndexOf("Lasttest", StringComparison.OrdinalIgnoreCase) >= 0 && dt > lastLoadDt) { lastLoadDt = dt; lastLoadDate = dt.ToString("dd.MM."); }
                }
            }
        }

        // 2x2 Grid (Breite StartGruppenbreite, Höhe 458)
        TableLayoutPanel grid2x2 = new TableLayoutPanel();
        grid2x2.Width = UI.S(UI.StartGruppenbreite);
        grid2x2.Height = UI.S(458);
        grid2x2.ColumnCount = 2;
        grid2x2.RowCount = 2;
        grid2x2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
        grid2x2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
        grid2x2.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(217)));
        grid2x2.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(217)));
        grid2x2.Margin = new Padding(0, 0, 0, UI.S(14));

        // --- Kachel 1 (links oben): Letzter Lauf ---
        Panel cardLast = new Panel();
        cardLast.Dock = DockStyle.Fill;
        cardLast.BackColor = UI.Panel;
        cardLast.Margin = new Padding(0, 0, UI.S(12), UI.S(12));
        cardLast.Padding = new Padding(UI.S(14), UI.S(12), UI.S(14), UI.S(12));
        cardLast.Paint += delegate(object sender, PaintEventArgs pe) {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UI.Round(new RectangleF(1f, 1f, cardLast.Width - 2f, cardLast.Height - 2f), UI.SF(8)))
            using (Pen pen = new Pen(UI.Line))
            {
                pen.Alignment = PenAlignment.Inset;
                pe.Graphics.DrawPath(pen, path);
            }
        };

        TableLayoutPanel lastLayout = new TableLayoutPanel();
        lastLayout.Dock = DockStyle.Fill;
        lastLayout.ColumnCount = 1;
        lastLayout.RowCount = 3;
        lastLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(26)));
        lastLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(74)));
        lastLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(38)));

        // Row 0: Kopfzeile Letzter Lauf
        TableLayoutPanel lastHead = new TableLayoutPanel();
        lastHead.Dock = DockStyle.Fill;
        lastHead.ColumnCount = 2;
        lastHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
        lastHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
        lastHead.Margin = new Padding(0);
        Label lblLTitle = Lbl("Letzter Lauf", 10.5f, true, UI.Text);
        lblLTitle.Margin = new Padding(0);
        string lastRunInfo = lastRun != null ? (FormatRunDate(lastRun.Datum) + " · " + (lastRun.Module.Length > 0 ? (lastRun.Module.IndexOf("Benchmark") >= 0 ? "Benchmark" : (lastRun.Module.IndexOf("Diagnose") >= 0 ? "Diagnose" : "Lauf")) : "Lauf")) : "keine Läufe";
        Label lblLDate = Lbl(lastRunInfo, 8.25f, false, UI.Muted);
        lblLDate.Anchor = AnchorStyles.Right;
        lblLDate.Margin = new Padding(0);
        lblLDate.AutoEllipsis = true;
        Tip(lblLTitle, "Ergebnisse des letzten auf diesem Rechner durchgeführten Laufs.");
        Tip(lblLDate, lastRunInfo);
        lastHead.Controls.Add(lblLTitle, 0, 0);
        lastHead.Controls.Add(lblLDate, 1, 0);
        lastLayout.Controls.Add(lastHead, 0, 0);

        // Row 1: KPI Grid (4 Spalten)
        TableLayoutPanel kpiGrid = new TableLayoutPanel();
        kpiGrid.Dock = DockStyle.Fill;
        kpiGrid.ColumnCount = 4;
        kpiGrid.RowCount = 1;
        kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        kpiGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        kpiGrid.Margin = new Padding(0, UI.S(4), 0, UI.S(6));

        string kpiGesamt = (lastRun != null && lastRun.OverallScore > 0) ? lastRun.OverallScore.ToString("N0", CultureInfo.CurrentCulture) : "-";
        string kpiCpu = (lastRun != null && lastRun.Get("CPU|MT") > 0) ? lastRun.Get("CPU|MT").ToString("N0", CultureInfo.CurrentCulture) : "-";
        string kpiRam = "-";
        if (lastRun != null)
        {
            double rVal = lastRun.Get("RAM|Lesen");
            if (rVal <= 0) rVal = lastRun.Get("RAM|Read");
            if (rVal > 0)
            {
                double gb = rVal > 1000.0 ? rVal / 1000.0 : rVal;
                kpiRam = gb.ToString("N1", CultureInfo.CurrentCulture) + " GB/s";
            }
        }
        string kpiGpu = "-";
        if (lastRun != null)
        {
            double gVal = lastRun.Get("GPU|REND");
            if (gVal <= 0) gVal = lastRun.Get("GPU|FPS");
            if (gVal > 0) kpiGpu = Math.Round(gVal, 0) + " FPS";
        }

        // Finde vorherigen Lauf dieses Rechners (chronologisch aufsteigend)
        List<DbEntry> thisPcRuns = new List<DbEntry>();
        string devKey = lastRun != null ? lastRun.GeraetKey : null;
        foreach (DbEntry e in db)
        {
            if (e.IsReference) continue;
            bool match = false;
            if (!String.IsNullOrEmpty(devKey) && !String.IsNullOrEmpty(e.GeraetKey) && String.Equals(e.GeraetKey, devKey, StringComparison.OrdinalIgnoreCase))
                match = true;
            else if (!String.IsNullOrEmpty(e.Computer) && (String.Equals(e.Computer, curPc, StringComparison.OrdinalIgnoreCase) || e.Computer.StartsWith(curPc + "_", StringComparison.OrdinalIgnoreCase)))
                match = true;
            else if (!String.IsNullOrEmpty(e.Name) && String.Equals(e.Name, curPc, StringComparison.OrdinalIgnoreCase))
                match = true;

            if (match) thisPcRuns.Add(e);
        }
        thisPcRuns.Sort(delegate(DbEntry a, DbEntry b) {
            return String.Compare(a.Datum, b.Datum, StringComparison.Ordinal);
        });

        DbEntry prevRun = null;
        if (thisPcRuns.Count >= 2 && lastRun != null)
        {
            int idx = thisPcRuns.IndexOf(lastRun);
            if (idx > 0) prevRun = thisPcRuns[idx - 1];
            else if (idx < 0) prevRun = thisPcRuns[thisPcRuns.Count - 2];
        }

        string dGesamt = null, dCpu = null, dRam = null, dGpu = null;
        if (prevRun != null && lastRun != null)
        {
            if (prevRun.OverallScore > 0 && lastRun.OverallScore > 0)
            {
                double d = Math.Round(((lastRun.OverallScore - prevRun.OverallScore) / (double)prevRun.OverallScore) * 100.0, 0);
                dGesamt = d > 0 ? ("\u2191 +" + d.ToString("0", CultureInfo.CurrentCulture) + " %") : (d < 0 ? ("\u2193 " + d.ToString("0", CultureInfo.CurrentCulture) + " %") : "\u2192 0 %");
            }
            double prevCpu = prevRun.Get("CPU|MT");
            double curCpu = lastRun.Get("CPU|MT");
            if (prevCpu > 0 && curCpu > 0)
            {
                double d = Math.Round(((curCpu - prevCpu) / prevCpu) * 100.0, 0);
                dCpu = d > 0 ? ("\u2191 +" + d.ToString("0", CultureInfo.CurrentCulture) + " %") : (d < 0 ? ("\u2193 " + d.ToString("0", CultureInfo.CurrentCulture) + " %") : "\u2192 0 %");
            }
            double prevRam = prevRun.Get("RAM|Lesen");
            if (prevRam <= 0) prevRam = prevRun.Get("RAM|Read");
            double curRam = lastRun.Get("RAM|Lesen");
            if (curRam <= 0) curRam = lastRun.Get("RAM|Read");
            if (prevRam > 0 && curRam > 0)
            {
                double d = Math.Round(((curRam - prevRam) / prevRam) * 100.0, 0);
                dRam = d > 0 ? ("\u2191 +" + d.ToString("0", CultureInfo.CurrentCulture) + " %") : (d < 0 ? ("\u2193 " + d.ToString("0", CultureInfo.CurrentCulture) + " %") : "\u2192 0 %");
            }
            double prevGpu = prevRun.Get("GPU|REND");
            if (prevGpu <= 0) prevGpu = prevRun.Get("GPU|FPS");
            double curGpu = lastRun.Get("GPU|REND");
            if (curGpu <= 0) curGpu = lastRun.Get("GPU|FPS");
            if (prevGpu > 0 && curGpu > 0)
            {
                double d = Math.Round(((curGpu - prevGpu) / prevGpu) * 100.0, 0);
                dGpu = d > 0 ? ("\u2191 +" + d.ToString("0", CultureInfo.CurrentCulture) + " %") : (d < 0 ? ("\u2193 " + d.ToString("0", CultureInfo.CurrentCulture) + " %") : "\u2192 0 %");
            }
        }

        KpiBox kpi1 = new KpiBox("Gesamt", kpiGesamt, dGesamt); kpi1.Dock = DockStyle.Fill; Tip(kpi1, "Gesamtpunktzahl des letzten Benchmarklaufs" + (dGesamt != null ? " (" + dGesamt + " zum vorigen Lauf)" : "") + ".");
        KpiBox kpi2 = new KpiBox("CPU Mehrkern", kpiCpu, dCpu); kpi2.Dock = DockStyle.Fill; Tip(kpi2, "CPU-Mehrkern-Punktzahl des letzten Laufs" + (dCpu != null ? " (" + dCpu + " zum vorigen Lauf)" : "") + ".");
        KpiBox kpi3 = new KpiBox("RAM Lesen", kpiRam, dRam); kpi3.Dock = DockStyle.Fill; Tip(kpi3, "Gemessener Speicherdurchsatz beim Lesen" + (dRam != null ? " (" + dRam + " zum vorigen Lauf)" : "") + ".");
        KpiBox kpi4 = new KpiBox("Grafik", kpiGpu, dGpu); kpi4.Dock = DockStyle.Fill; kpi4.Margin = new Padding(0); Tip(kpi4, "Gemessene Bildrate im Grafik-Rendertest" + (dGpu != null ? " (" + dGpu + " zum vorigen Lauf)" : "") + ".");

        kpiGrid.Controls.Add(kpi1, 0, 0);
        kpiGrid.Controls.Add(kpi2, 1, 0);
        kpiGrid.Controls.Add(kpi3, 2, 0);
        kpiGrid.Controls.Add(kpi4, 3, 0);
        lastLayout.Controls.Add(kpiGrid, 0, 1);

        // Row 2: Aktionen & Befundstatus
        TableLayoutPanel actGrid = new TableLayoutPanel();
        actGrid.Dock = DockStyle.Fill;
        actGrid.ColumnCount = 2;
        actGrid.RowCount = 1;
        actGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        actGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        actGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        actGrid.Margin = new Padding(0);

        FlowLayoutPanel pnlButtons = new FlowLayoutPanel();
        pnlButtons.Dock = DockStyle.Fill;
        pnlButtons.WrapContents = false;
        pnlButtons.AutoSize = true;
        pnlButtons.BackColor = UI.Panel;
        pnlButtons.Margin = new Padding(0);

        Button btnOpenRep = new PrimaryButton();
        // ab v4.08 kurz "Bericht": mit dem neuen Knopf "Ergebnis" passte die Zeile sonst nicht mehr neben die Befunde
        btnOpenRep.Text = "Bericht";
        btnOpenRep.FlatStyle = FlatStyle.Flat;
        btnOpenRep.FlatAppearance.BorderSize = 0;
        btnOpenRep.BackColor = UI.Accent;
        btnOpenRep.ForeColor = UI.OnAccent;
        btnOpenRep.FlatAppearance.MouseOverBackColor = UI.AccentHover;
        btnOpenRep.FlatAppearance.MouseDownBackColor = UI.AccentActive;
        btnOpenRep.Font = new Font("Segoe UI Semibold", 8.5f);
        btnOpenRep.Padding = new Padding(UI.S(5), UI.S(3), UI.S(5), UI.S(3));
        btnOpenRep.Margin = new Padding(0, UI.S(2), UI.S(4), 0);
        btnOpenRep.AutoSize = true;
        btnOpenRep.Cursor = Cursors.Hand;
        btnOpenRep.UseVisualStyleBackColor = false;
        btnOpenRep.Enabled = (lastRun != null);
        Tip(btnOpenRep, "Öffnet den vollständigen HTML-Bericht des letzten Laufs.");
        DbEntry targetRun = lastRun;
        btnOpenRep.Click += delegate { if (targetRun != null) OpenEntry(targetRun); };
        pnlButtons.Controls.Add(btnOpenRep);

        // ab v4.08: führt zur Ergebnisseite mit diesem Lauf (Urteil, Vergleich zur Referenz, Hauptwerte)
        Button btnErgebnis = new Button();
        btnErgebnis.Text = "Ergebnis";
        btnErgebnis.FlatStyle = FlatStyle.Flat;
        btnErgebnis.FlatAppearance.BorderColor = UI.Accent;
        btnErgebnis.FlatAppearance.BorderSize = 1;
        btnErgebnis.BackColor = UI.AccentSoft;
        btnErgebnis.ForeColor = UI.Accent;
        btnErgebnis.FlatAppearance.MouseOverBackColor = UI.Accent;
        btnErgebnis.FlatAppearance.MouseDownBackColor = UI.AccentActive;
        btnErgebnis.Font = new Font("Segoe UI Semibold", 8.5f);
        btnErgebnis.Padding = new Padding(UI.S(5), UI.S(3), UI.S(5), UI.S(3));
        btnErgebnis.Margin = new Padding(0, UI.S(2), UI.S(4), 0);
        btnErgebnis.AutoSize = true;
        btnErgebnis.Cursor = Cursors.Hand;
        btnErgebnis.UseVisualStyleBackColor = false;
        btnErgebnis.MouseEnter += delegate { btnErgebnis.ForeColor = UI.OnAccent; };
        btnErgebnis.MouseLeave += delegate { btnErgebnis.ForeColor = UI.Accent; };
        Tip(btnErgebnis, "Öffnet die Ergebnisseite mit dem letzten Lauf dieses PCs: Urteil, Vergleich zur Referenz und Hauptwerte je Komponente.");
        btnErgebnis.Click += delegate { ZeigeLetztesErgebnis(); };
        pnlButtons.Controls.Add(btnErgebnis);

        Button btnDash = new Button();
        btnDash.Text = "Dashboard";
        btnDash.FlatStyle = FlatStyle.Flat;
        btnDash.FlatAppearance.BorderColor = UI.Accent;
        btnDash.FlatAppearance.BorderSize = 1;
        btnDash.BackColor = UI.AccentSoft;
        btnDash.ForeColor = UI.Accent;
        btnDash.FlatAppearance.MouseOverBackColor = UI.Accent;
        btnDash.FlatAppearance.MouseDownBackColor = UI.AccentActive;
        btnDash.Font = new Font("Segoe UI Semibold", 8.5f);
        btnDash.Padding = new Padding(UI.S(5), UI.S(3), UI.S(5), UI.S(3));
        btnDash.Margin = new Padding(0, UI.S(2), UI.S(4), 0);
        btnDash.AutoSize = true;
        btnDash.Cursor = Cursors.Hand;
        btnDash.UseVisualStyleBackColor = false;
        btnDash.MouseEnter += delegate { btnDash.ForeColor = UI.OnAccent; };
        btnDash.MouseLeave += delegate { btnDash.ForeColor = UI.Accent; };
        Tip(btnDash, "Öffnet das interaktive Dashboard zum grafischen Vergleich mit anderen Systemen.");
        btnDash.Click += delegate { OpenDashboard(); };
        pnlButtons.Controls.Add(btnDash);
        Button btnVerlauf = new Button();
        btnVerlauf.Text = "Verlauf";
        btnVerlauf.FlatStyle = FlatStyle.Flat;
        btnVerlauf.FlatAppearance.BorderColor = UI.Accent;
        btnVerlauf.FlatAppearance.BorderSize = 1;
        btnVerlauf.BackColor = UI.AccentSoft;
        btnVerlauf.ForeColor = UI.Accent;
        btnVerlauf.FlatAppearance.MouseOverBackColor = UI.Accent;
        btnVerlauf.FlatAppearance.MouseDownBackColor = UI.AccentActive;
        btnVerlauf.Font = new Font("Segoe UI Semibold", 8.5f);
        btnVerlauf.Padding = new Padding(UI.S(5), UI.S(3), UI.S(5), UI.S(3));
        btnVerlauf.Margin = new Padding(0, UI.S(2), 0, 0);
        btnVerlauf.AutoSize = true;
        btnVerlauf.Cursor = Cursors.Hand;
        btnVerlauf.UseVisualStyleBackColor = false;
        btnVerlauf.MouseEnter += delegate { btnVerlauf.ForeColor = UI.OnAccent; };
        btnVerlauf.MouseLeave += delegate { btnVerlauf.ForeColor = UI.Accent; };
        Tip(btnVerlauf, "\u00D6ffnet das interaktive Dashboard im Verlaufsmodus f\u00FCr diesen PC.");
        DbEntry devHistoryTarget = lastRun;
        btnVerlauf.Click += delegate { if (devHistoryTarget != null) OpenDeviceHistory(devHistoryTarget); };
        pnlButtons.Controls.Add(btnVerlauf);
        actGrid.Controls.Add(pnlButtons, 0, 0);

        string befundeText = "Befunde: keine";
        Color befundeColor = UI.Ok;
        if (lastRun != null && lastRun.Befunde.Length > 0 && lastRun.Befunde != "0 / 0 / 0")
        {
            befundeText = "Befunde: " + lastRun.Befunde;
            befundeColor = UI.Warn;
        }
        Label lblBef = Lbl(befundeText, 8.25f, true, befundeColor);
        lblBef.AutoSize = true;
        lblBef.Dock = DockStyle.Fill;
        lblBef.TextAlign = ContentAlignment.MiddleRight;
        lblBef.Margin = new Padding(UI.S(6), UI.S(4), 0, 0);
        Tip(lblBef, "Befundzusammenfassung (Kritisch / Warnungen / Hinweise) des letzten Laufs.");
        actGrid.Controls.Add(lblBef, 1, 0);
        lastLayout.Controls.Add(actGrid, 0, 2);

        cardLast.Controls.Add(lastLayout);
        grid2x2.Controls.Add(cardLast, 0, 0);

        // --- Kachel 2 (rechts oben): Aufgaben ---
        Panel cardTasks = new Panel();
        cardTasks.Dock = DockStyle.Fill;
        cardTasks.BackColor = UI.Panel;
        cardTasks.Margin = new Padding(0, 0, 0, UI.S(12));
        cardTasks.Padding = new Padding(UI.S(14), UI.S(12), UI.S(14), UI.S(10));
        cardTasks.Paint += delegate(object sender, PaintEventArgs pe) {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UI.Round(new RectangleF(1f, 1f, cardTasks.Width - 2f, cardTasks.Height - 2f), UI.SF(8)))
            using (Pen pen = new Pen(UI.Line))
            {
                pen.Alignment = PenAlignment.Inset;
                pe.Graphics.DrawPath(pen, path);
            }
        };

        TableLayoutPanel tasksLayout = new TableLayoutPanel();
        tasksLayout.Dock = DockStyle.Fill;
        tasksLayout.ColumnCount = 1;
        tasksLayout.RowCount = 2;
        tasksLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(24)));
        tasksLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        Label lblTHead = Lbl("Aufgaben", 10.5f, true, UI.Text);
        lblTHead.Margin = new Padding(0);
        Tip(lblTHead, "Aufgaben zur schnellen Auswahl und Durchführung.");
        tasksLayout.Controls.Add(lblTHead, 0, 0);

        TableLayoutPanel taskList = new TableLayoutPanel();
        taskList.Dock = DockStyle.Fill;
        taskList.RowCount = 4;
        taskList.ColumnCount = 1;
        taskList.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
        taskList.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
        taskList.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
        taskList.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
        taskList.Margin = new Padding(0);

        StartTaskRow tr1 = new StartTaskRow("PC durchchecken", "Diagnose, Benchmark, Lasttest", "Lesen", "ok", UI.IcoDiag);
        tr1.Dock = DockStyle.Fill;
        Tip(tr1, "Startet Diagnose, Benchmark und Lasttest zur vollständigen Überprüfung von Hardware, Leistung und Stabilität.");
        tr1.RowClicked += delegate { SelectTask("PC durchchecken"); };

        string bSub = "Benchmark · ca. 7 Min." + (lastBenchDate.Length > 0 ? " · zuletzt " + lastBenchDate : " · noch nie");
        StartTaskRow tr2 = new StartTaskRow("Leistung messen", bSub, "Lesen", "ok", UI.IcoSpeed);
        tr2.Dock = DockStyle.Fill;
        Tip(tr2, "Startet den Leistungsbenchmark für CPU, RAM, Grafikkarte und Datenträger.");
        tr2.RowClicked += delegate { SelectTask("Leistung messen"); };

        string lSub = "Lasttest · ab 2 Min." + (lastLoadDate.Length > 0 ? " · zuletzt " + lastLoadDate : " · noch nie");
        StartTaskRow tr3 = new StartTaskRow("Stabilität prüfen", lSub, "Lesen", "ok", UI.IcoFlame);
        tr3.Dock = DockStyle.Fill;
        Tip(tr3, "Startet den Stabilitätstest unter Volllast zur Erkennung von Überhitzung und Instabilitäten.");
        tr3.RowClicked += delegate { SelectTask("Stabilität prüfen"); };

        StartTaskRow tr4 = new StartTaskRow("Pflegen und beschleunigen", "Wartung, Optimierung", "Ändert", "warn", UI.IcoWartung);
        tr4.Dock = DockStyle.Fill;
        Tip(tr4, "Startet Wartung und Optimierung zur Systembereinigung und Leistungsverbesserung.");
        tr4.RowClicked += delegate { SelectTask("Pflegen und beschleunigen"); };

        taskList.Controls.Add(tr1, 0, 0);
        taskList.Controls.Add(tr2, 0, 1);
        taskList.Controls.Add(tr3, 0, 2);
        taskList.Controls.Add(tr4, 0, 3);
        tasksLayout.Controls.Add(taskList, 0, 1);
        cardTasks.Controls.Add(tasksLayout);
        grid2x2.Controls.Add(cardTasks, 1, 0);

        // --- Kachel 3 (links unten): Schnell behebbar ---
        Panel cardQuick = new Panel();
        cardQuick.Dock = DockStyle.Fill;
        cardQuick.BackColor = UI.Panel;
        cardQuick.Margin = new Padding(0, 0, UI.S(12), UI.S(12));
        cardQuick.Padding = new Padding(UI.S(14), UI.S(12), UI.S(14), UI.S(10));
        cardQuick.Paint += delegate(object sender, PaintEventArgs pe) {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UI.Round(new RectangleF(1f, 1f, cardQuick.Width - 2f, cardQuick.Height - 2f), UI.SF(8)))
            using (Pen pen = new Pen(UI.Line))
            {
                pen.Alignment = PenAlignment.Inset;
                pe.Graphics.DrawPath(pen, path);
            }
        };

        TableLayoutPanel quickLayout = new TableLayoutPanel();
        quickLayout.Dock = DockStyle.Fill;
        quickLayout.ColumnCount = 1;
        quickLayout.RowCount = 2;
        quickLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(24)));
        quickLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        TableLayoutPanel qHead = new TableLayoutPanel();
        qHead.Dock = DockStyle.Fill;
        qHead.ColumnCount = 2;
        qHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
        qHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
        qHead.Margin = new Padding(0);
        Label lblQTitle = Lbl("Schnell behebbar", 10.5f, true, UI.Text);
        lblQTitle.Margin = new Padding(0);
        Label lblQSub = Lbl("beim Start erkannt, ohne Messung", 8f, false, UI.Muted);
        lblQSub.Anchor = AnchorStyles.Right;
        lblQSub.Margin = new Padding(0);
        Tip(lblQTitle, "Häufige Konfigurationsbremsen, die ohne Neustart oder Messung erkannt werden.");
        Tip(lblQSub, "Konfigurationsprüfungen laufen asynchron im Hintergrund.");
        qHead.Controls.Add(lblQTitle, 0, 0);
        qHead.Controls.Add(lblQSub, 1, 0);
        quickLayout.Controls.Add(qHead, 0, 0);

        Panel pQuickFixes = new Panel();
        pQuickFixes.Dock = DockStyle.Fill;
        pQuickFixes.BackColor = UI.Panel;
        Label lblLoad = Lbl("Konfiguration wird im Hintergrund geprüft ...", 8.5f, false, UI.Muted);
        lblLoad.Location = new Point(0, UI.S(16));
        lblLoad.AutoSize = true;
        Tip(lblLoad, "Prüfungen laufen ...");
        pQuickFixes.Controls.Add(lblLoad);
        quickLayout.Controls.Add(pQuickFixes, 0, 1);
        cardQuick.Controls.Add(quickLayout);
        grid2x2.Controls.Add(cardQuick, 0, 1);

        StartQuickFixChecks(pQuickFixes, lastLoadDt > DateTime.MinValue);

        // --- Kachel 4 (rechts unten): Werkzeuge ---
        Panel cardToolsB = new Panel();
        cardToolsB.Dock = DockStyle.Fill;
        cardToolsB.BackColor = UI.Panel;
        cardToolsB.Margin = new Padding(0, 0, 0, UI.S(12));
        cardToolsB.Padding = new Padding(UI.S(14), UI.S(12), UI.S(14), UI.S(10));
        cardToolsB.Paint += delegate(object sender, PaintEventArgs pe) {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UI.Round(new RectangleF(1f, 1f, cardToolsB.Width - 2f, cardToolsB.Height - 2f), UI.SF(8)))
            using (Pen pen = new Pen(UI.Line))
            {
                pen.Alignment = PenAlignment.Inset;
                pe.Graphics.DrawPath(pen, path);
            }
        };

        TableLayoutPanel toolsLayoutB = new TableLayoutPanel();
        toolsLayoutB.Dock = DockStyle.Fill;
        toolsLayoutB.ColumnCount = 1;
        toolsLayoutB.RowCount = 2;
        toolsLayoutB.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(24)));
        toolsLayoutB.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        Label lblWHead = Lbl("Werkzeuge", 10.5f, true, UI.Text);
        lblWHead.Margin = new Padding(0);
        Tip(lblWHead, "Schnellzugriff auf integrierte Werkzeuge und Übersichten.");
        toolsLayoutB.Controls.Add(lblWHead, 0, 0);

        TableLayoutPanel toolsListB = new TableLayoutPanel();
        toolsListB.Dock = DockStyle.Top;
        toolsListB.Height = UI.S(116);
        toolsListB.RowCount = 3;
        toolsListB.ColumnCount = 1;
        toolsListB.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(38)));
        toolsListB.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(38)));
        toolsListB.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(38)));
        toolsListB.Margin = new Padding(0, UI.S(4), 0, 0);

        Control rowSensB = CreateToolRowB(UI.IcoSens, "Sensoren live", "CPU [°C] · GPU [°C]", delegate { ShowPage(7); }, "Öffnet die Live-Sensoranzeige für Temperaturen, Taktraten und Spannungen.");
        Control rowDbB = CreateToolRowB(UI.IcoDb, "Datenbank", db.Count + " Systeme", delegate { ShowPage(9); }, "Öffnet die Datenbank aller gespeicherten Messungen.");
        int openChanges = 0;
        foreach (ChangeEntry ce in changes) if (ce.Undoable) openChanges++;
        string chgInfo = openChanges == 0 ? "keine offenen" : (openChanges + " offen");
        Control rowChgB = CreateToolRowB(UI.IcoChg, "Änderungen", chgInfo, delegate { ShowPage(10); }, "Öffnet das Protokoll der Systemänderungen mit Rückgängig-Funktion.");

        toolsListB.Controls.Add(rowSensB, 0, 0);
        toolsListB.Controls.Add(rowDbB, 0, 1);
        toolsListB.Controls.Add(rowChgB, 0, 2);
        toolsLayoutB.Controls.Add(toolsListB, 0, 1);
        cardToolsB.Controls.Add(toolsLayoutB);
        grid2x2.Controls.Add(cardToolsB, 1, 1);

        p.Controls.Add(grid2x2);

        // 4. Abschlussleiste
        p.Controls.Add(BuildStartFooter());

        f.Controls.Add(p);
    }

    Control CreateToolRowB(string icon, string title, string rightText, Action click, string tipText)
    {
        Panel row = new Panel();
        row.Dock = DockStyle.Fill;
        row.Cursor = Cursors.Hand;
        row.Margin = new Padding(0, 0, 0, UI.S(4));
        bool hover = false;

        row.MouseEnter += delegate { hover = true; row.Invalidate(); };
        row.MouseLeave += delegate { hover = false; row.Invalidate(); };

        Panel capturedRow = row;
        row.Paint += delegate(object sender, PaintEventArgs pe) {
            if (hover)
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color hbg = UI.IsDark ? Color.FromArgb(46, 48, 52) : Color.FromArgb(240, 242, 246);
                using (GraphicsPath p = UI.Round(new RectangleF(0.5f, 0.5f, capturedRow.Width - 1.5f, capturedRow.Height - 1.5f), UI.SF(6)))
                using (SolidBrush b = new SolidBrush(hbg)) pe.Graphics.FillPath(b, p);
            }
        };

        Label lblIco = Lbl(icon, 11f, false, UI.Accent);
        lblIco.Font = UI.SymbolFont(11f);
        lblIco.Location = new Point(UI.S(6), UI.S(6));
        lblIco.Size = new Size(UI.S(22), UI.S(22));

        Label lblT = Lbl(title, 9f, true, UI.Text);
        lblT.Location = new Point(UI.S(32), UI.S(7));
        lblT.AutoSize = true;

        Label lblR = Lbl(rightText, 8.5f, false, UI.Muted);
        lblR.Dock = DockStyle.Right;
        lblR.TextAlign = ContentAlignment.MiddleRight;
        lblR.Padding = new Padding(0, 0, UI.S(8), 0);
        lblR.AutoSize = false;
        lblR.Width = UI.S(180);

        Tip(row, tipText);
        Tip(lblIco, tipText);
        Tip(lblT, tipText);
        Tip(lblR, tipText);

        EventHandler eh = delegate { if (click != null) click(); };
        row.Click += eh;
        lblIco.Click += eh;
        lblT.Click += eh;
        lblR.Click += eh;

        EventHandler onEnter = delegate { hover = true; capturedRow.Invalidate(); };
        EventHandler onLeave = delegate { hover = false; capturedRow.Invalidate(); };
        lblIco.MouseEnter += onEnter; lblIco.MouseLeave += onLeave;
        lblT.MouseEnter += onEnter; lblT.MouseLeave += onLeave;
        lblR.MouseEnter += onEnter; lblR.MouseLeave += onLeave;

        row.Controls.Add(lblR);
        row.Controls.Add(lblIco);
        row.Controls.Add(lblT);
        return row;
    }

    Control CreateToolItem(string icon, string title, string sub, Action click, string tipText)
    {
        Panel row = new Panel();
        row.Dock = DockStyle.Fill;
        row.Cursor = Cursors.Hand;
        row.Margin = new Padding(0);
        bool hover = false;

        row.MouseEnter += delegate { hover = true; row.Invalidate(); };
        row.MouseLeave += delegate { hover = false; row.Invalidate(); };

        Panel capturedRow = row;
        row.Paint += delegate(object sender, PaintEventArgs pe) {
            if (hover)
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color hbg = UI.IsDark ? Color.FromArgb(46, 48, 52) : Color.FromArgb(240, 242, 246);
                using (GraphicsPath p = UI.Round(new RectangleF(0.5f, 0.5f, capturedRow.Width - 1.5f, capturedRow.Height - 1.5f), UI.SF(6)))
                using (SolidBrush b = new SolidBrush(hbg)) pe.Graphics.FillPath(b, p);
            }
        };

        Label lblIco = Lbl(icon, 11f, false, UI.Accent);
        lblIco.Font = UI.SymbolFont(11f);
        lblIco.Location = new Point(UI.S(6), UI.S(7));
        lblIco.Size = new Size(UI.S(22), UI.S(22));

        Label lblT = Lbl(title, 9f, true, UI.Text);
        lblT.Location = new Point(UI.S(34), UI.S(3));
        lblT.AutoSize = true;

        Label lblS = Lbl(sub, 8f, false, UI.Muted);
        lblS.Location = new Point(UI.S(34), UI.S(20));
        lblS.AutoSize = true;

        Tip(row, tipText);
        Tip(lblIco, tipText);
        Tip(lblT, tipText);
        Tip(lblS, tipText);

        EventHandler eh = delegate { if (click != null) click(); };
        row.Click += eh;
        lblIco.Click += eh;
        lblT.Click += eh;
        lblS.Click += eh;

        EventHandler onEnter = delegate { hover = true; capturedRow.Invalidate(); };
        EventHandler onLeave = delegate { hover = false; capturedRow.Invalidate(); };
        lblIco.MouseEnter += onEnter; lblIco.MouseLeave += onLeave;
        lblT.MouseEnter += onEnter; lblT.MouseLeave += onLeave;
        lblS.MouseEnter += onEnter; lblS.MouseLeave += onLeave;

        row.Controls.Add(lblIco);
        row.Controls.Add(lblT);
        row.Controls.Add(lblS);
        return row;
    }

    Control BuildStartFooter()
    {
        FlowLayoutPanel foot = new FlowLayoutPanel();
        foot.Width = UI.S(UI.StartGruppenbreite);
        foot.AutoSize = true;
        foot.WrapContents = false;
        foot.BackColor = UI.Panel;
        foot.Margin = new Padding(0, UI.S(4), 0, UI.S(14));

        // ab v4.0 steht "Beim Programmstart die Startseite anzeigen" nur noch in den Einstellungen

        string seenVer = LoadSeenVersionPreference();
        bool isNew = (seenVer != this.version);

        LinkLabel lnkNew = new LinkLabel();
        lnkNew.Text = "Was ist neu";
        lnkNew.Font = new Font("Segoe UI", 9f);
        lnkNew.LinkColor = UI.Accent;
        lnkNew.ActiveLinkColor = UI.AccentHover;
        lnkNew.LinkBehavior = LinkBehavior.HoverUnderline;
        lnkNew.AutoSize = true;
        lnkNew.Margin = new Padding(0, UI.S(4), UI.S(6), 0);
        Tip(lnkNew, "Zeigt die Neuerungen dieser Version in der Versionshistorie.");
        lnkNew.LinkClicked += delegate {
            SaveSeenVersionPreference(this.version);
            ShowPage(versionPage);
        };
        foot.Controls.Add(lnkNew);

        if (isNew)
        {
            Badge bNew = new Badge("Neu", "info");
            bNew.Margin = new Padding(0, UI.S(4), UI.S(18), 0);
            Tip(bNew, "Neuerungen in Version " + this.version);
            foot.Controls.Add(bNew);
        }

        LinkLabel lnkGuide = new LinkLabel();
        lnkGuide.Text = "Kurzanleitung";
        lnkGuide.Font = new Font("Segoe UI", 9f);
        lnkGuide.LinkColor = UI.Accent;
        lnkGuide.ActiveLinkColor = UI.AccentHover;
        lnkGuide.LinkBehavior = LinkBehavior.HoverUnderline;
        lnkGuide.AutoSize = true;
        lnkGuide.Margin = new Padding(UI.S(12), UI.S(4), 0, 0);
        Tip(lnkGuide, "Öffnet eine kompakte Kurzanleitung zur Bedienung von Leos Minibench.");
        lnkGuide.LinkClicked += delegate {
            ShowGuideDialog();
        };
        foot.Controls.Add(lnkGuide);

        return foot;
    }

    void SelectTask(string name)
    {
        for (int i = 1; i <= 5 && i < nav.Count; i++) nav[i].Checked = false;

        if (name == "PC durchchecken")
        {
            if (nav.Count > 1) nav[1].Checked = true;
            if (nav.Count > 2) nav[2].Checked = true;
            if (nav.Count > 3) nav[3].Checked = true;
            if (rbVoll != null) rbVoll.Checked = true;
            if (chkLCpu != null) chkLCpu.Checked = true;
            if (chkLRam != null) chkLRam.Checked = true;
            if (chkLGpu != null) chkLGpu.Checked = true;
            ShowPage(1);
        }
        else if (name == "Leistung messen")
        {
            if (nav.Count > 2) nav[2].Checked = true;
            ShowPage(2);
        }
        else if (name == "Stabilität prüfen")
        {
            if (nav.Count > 3) nav[3].Checked = true;
            if (chkLCpu != null) chkLCpu.Checked = true;
            if (chkLRam != null) chkLRam.Checked = true;
            if (chkLGpu != null) chkLGpu.Checked = true;
            ShowPage(3);
        }
        else if (name == "Pflegen und beschleunigen")
        {
            if (nav.Count > 4) nav[4].Checked = true;
            if (nav.Count > 5) nav[5].Checked = true;
            ShowPage(4);
        }
        UpdateSummary();
    }

    string GetDisksSummary()
    {
        if (this.disks == null || this.disks.Length == 0)
        {
            try
            {
                DriveInfo[] drvs = DriveInfo.GetDrives();
                long tot = 0; int cnt = 0;
                foreach (DriveInfo d in drvs)
                {
                    if (d.IsReady && d.DriveType == DriveType.Fixed) { tot += d.TotalSize; cnt++; }
                }
                if (cnt > 0)
                {
                    double tb = tot / (1024.0 * 1024.0 * 1024.0 * 1024.0);
                    return String.Format("{0}x Datenträger · {1:F1} TB", cnt, tb).Replace('.', ',');
                }
            }
            catch { }
            return "Datenträger";
        }

        int nvme = 0, sataSsd = 0, hdd = 0, other = 0;
        double totalBytes = 0;
        foreach (string d in this.disks)
        {
            string[] x = d.Split('|');
            if (x.Length < 7) continue;
            string bus = x[2], med = x[3], sizeStr = x[4];
            if (bus.IndexOf("NVMe", StringComparison.OrdinalIgnoreCase) >= 0) nvme++;
            else if (med.IndexOf("SSD", StringComparison.OrdinalIgnoreCase) >= 0) sataSsd++;
            else if (med.IndexOf("HDD", StringComparison.OrdinalIgnoreCase) >= 0) hdd++;
            else other++;

            if (sizeStr.IndexOf("TB", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                double val;
                string num = sizeStr.Replace("TB", "").Trim().Replace(',', '.');
                if (double.TryParse(num, NumberStyles.Any, CultureInfo.InvariantCulture, out val)) totalBytes += val * 1024.0 * 1024.0 * 1024.0 * 1024.0;
            }
            else if (sizeStr.IndexOf("GB", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                double val;
                string num = sizeStr.Replace("GB", "").Trim().Replace(',', '.');
                if (double.TryParse(num, NumberStyles.Any, CultureInfo.InvariantCulture, out val)) totalBytes += val * 1024.0 * 1024.0 * 1024.0;
            }
        }

        List<string> parts = new List<string>();
        if (nvme > 0) parts.Add(nvme + "x NVMe");
        if (sataSsd > 0) parts.Add(sataSsd + "x SATA-SSD");
        if (hdd > 0) parts.Add(hdd + "x HDD");
        if (parts.Count == 0 && other > 0) parts.Add(other + "x Datenträger");
        if (parts.Count == 0) return "Datenträger";

        string types = String.Join(", ", parts.ToArray());
        double totTb = totalBytes / (1024.0 * 1024.0 * 1024.0 * 1024.0);
        if (totTb >= 1.0)
        {
            return String.Format("{0} · {1:F1} TB", types, totTb).Replace('.', ',');
        }
        double totGb = totalBytes / (1024.0 * 1024.0 * 1024.0);
        if (totGb > 0)
        {
            return String.Format("{0} · {1:F0} GB", types, totGb);
        }
        return types;
    }

    void StartQuickFixChecks(Panel container, bool hadLasttestOnThisPc)
    {
        if (container == null) return;
        System.ComponentModel.BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
        bw.DoWork += delegate(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            List<QuickFixItem> items = new List<QuickFixItem>();

            // 1. RAM Clock / XMP Check
            try
            {
                if (!HwIsLaptop && HwRam.Length > 0)
                {
                    if (HwRam.IndexOf("DDR4-2133", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        HwRam.IndexOf("DDR4-2400", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        QuickFixItem it = new QuickFixItem();
                        it.Level = "warn";
                        it.Badge = "Warnung";
                        it.Title = "Arbeitsspeicher läuft nur mit 2133 MT/s";
                        it.Desc = "Das XMP-Profil ist vermutlich nicht aktiv. Im BIOS einschalten, danach Lasttest Arbeitsspeicher.";
                        it.ButtonText = "Anleitung";
                        it.Action = delegate { ShowXmpInstructionsDialog(); };
                        it.Tip = "Öffnet eine Schritt-für-Schritt-Anleitung zur Aktivierung von XMP/EXPO im BIOS.";
                        items.Add(it);
                    }
                    else if (HwRam.IndexOf("DDR5-4800", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        QuickFixItem it = new QuickFixItem();
                        it.Level = "warn";
                        it.Badge = "Warnung";
                        it.Title = "Arbeitsspeicher läuft nur mit 4800 MT/s";
                        it.Desc = "Das XMP- oder EXPO-Profil ist vermutlich nicht aktiv. Im BIOS einschalten.";
                        it.ButtonText = "Anleitung";
                        it.Action = delegate { ShowXmpInstructionsDialog(); };
                        it.Tip = "Öffnet eine Schritt-für-Schritt-Anleitung zur Aktivierung von XMP/EXPO im BIOS.";
                        items.Add(it);
                    }
                }
            }
            catch { }

            // 2. Lasttest check auf diesem PC
            try
            {
                if (!hadLasttestOnThisPc)
                {
                    QuickFixItem it = new QuickFixItem();
                    it.Level = "info";
                    it.Badge = "Hinweis";
                    it.Title = "Noch kein Lasttest auf diesem PC";
                    it.Desc = "Ein Lasttest zeigt, ob Kühlung und Netzteil unter Dauerlast mithalten.";
                    it.ButtonText = "Einrichten";
                    it.Action = delegate { SelectTask("Stabilität prüfen"); };
                    it.Tip = "Wechselt zur Einrichtung des Stabilitätstests.";
                    items.Add(it);
                }
            }
            catch { }

            // 3. BIOS Date check (> 2 Jahre)
            try
            {
                using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS"))
                {
                    if (rk != null)
                    {
                        object bDate = rk.GetValue("BIOSReleaseDate");
                        if (bDate != null)
                        {
                            string sDate = bDate.ToString().Trim();
                            DateTime dtBios;
                            if (DateTime.TryParse(sDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtBios) ||
                                DateTime.TryParse(sDate, CultureInfo.CurrentCulture, DateTimeStyles.None, out dtBios))
                            {
                                if (DateTime.Now.Subtract(dtBios).TotalDays > 730)
                                {
                                    QuickFixItem it = new QuickFixItem();
                                    it.Level = "info";
                                    it.Badge = "Hinweis";
                                    it.Title = "BIOS-Version ist über 2 Jahre alt (" + dtBios.ToString("dd.MM.yyyy") + ")";
                                    it.Desc = "Ein BIOS-Update bringt oft Fehlerbehebungen und Stabilität.";
                                    it.ButtonText = "Inventar";
                                    it.Action = delegate { ShowInventoryDialog(); };
                                    it.Tip = "Zeigt Mainboard-Modell und BIOS-Stand im Inventar an.";
                                    items.Add(it);
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            // 4. LAN-Link 100 Mbit/s check
            try
            {
                foreach (System.Net.NetworkInformation.NetworkInterface nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                        nic.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet)
                    {
                        if (nic.Speed == 100000000)
                        {
                            QuickFixItem it = new QuickFixItem();
                            it.Level = "warn";
                            it.Badge = "Warnung";
                            it.Title = "LAN-Verbindung verhandelt nur 100 Mbit/s";
                            it.Desc = "Netzwerkkarte oder Switch-Port ist auf 100 Mbit/s gedrosselt (Kabel oder Port prüfen).";
                            it.ButtonText = "Prüfen";
                            it.Action = delegate {
                                try { System.Diagnostics.Process.Start("ncpa.cpl"); } catch { }
                            };
                            it.Tip = "Öffnet die Windows-Netzwerkverbindungen zur Überprüfung der Verbindungseigenschaften.";
                            items.Add(it);
                            break;
                        }
                    }
                }
            }
            catch { }

            
            // 5. Minibench Update verfügbar
            try
            {
                if (HasUpdateAvailable && !String.IsNullOrEmpty(AvailableUpdateVersion))
                {
                    QuickFixItem it = new QuickFixItem();
                    it.Level = "info";
                    it.Badge = "Update";
                    it.Title = "Minibench-Update verfügbar (" + AvailableUpdateVersion + ")";
                    it.Desc = "Eine neuere Version von Leos Minibench ist auf GitHub bereitgestellt. Klicke auf 'Herunterladen' für den automatischen Austausch.";
                    it.ButtonText = "Herunterladen";
                    it.Action = delegate { TriggerUpdateDownload(); };
                    it.Tip = "Lädt die neue LMB.exe herunter, validiert die SHA-256-Prüfsumme und startet die Anwendung neu.";
                    items.Add(it);
                }
            }
            catch { }

            // 6. Grafiktreiber-Alter (> 18 Monate / 548 Tage)
            try
            {
                using (Microsoft.Win32.RegistryKey videoKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"))
                {
                    if (videoKey != null)
                    {
                        foreach (string subName in videoKey.GetSubKeyNames())
                        {
                            if (subName.StartsWith("0"))
                            {
                                using (Microsoft.Win32.RegistryKey subKey = videoKey.OpenSubKey(subName))
                                {
                                    if (subKey == null) continue;
                                    object descObj = subKey.GetValue("DriverDesc");
                                    object dateObj = subKey.GetValue("DriverDate");
                                    object provObj = subKey.GetValue("ProviderName");
                                    string desc = descObj != null ? descObj.ToString().Trim() : "";
                                    string dateStr = dateObj != null ? dateObj.ToString().Trim() : "";
                                    string prov = provObj != null ? provObj.ToString().Trim() : "";

                                    if (String.IsNullOrEmpty(desc) || desc.IndexOf("Remote", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        desc.IndexOf("Basic Display", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                                    DateTime dtDriver;
                                    if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtDriver) ||
                                        DateTime.TryParse(dateStr, CultureInfo.CurrentCulture, DateTimeStyles.None, out dtDriver))
                                    {
                                        if (DateTime.Now.Subtract(dtDriver).TotalDays > 548)
                                        {
                                            string vendor = "Unbekannt";
                                            string dlUrl = "";
                                            if (desc.IndexOf("NVIDIA", StringComparison.OrdinalIgnoreCase) >= 0 || prov.IndexOf("NVIDIA", StringComparison.OrdinalIgnoreCase) >= 0)
                                            {
                                                vendor = "NVIDIA";
                                                dlUrl = "https://www.nvidia.de/Download/index.aspx";
                                            }
                                            else if (desc.IndexOf("AMD", StringComparison.OrdinalIgnoreCase) >= 0 || desc.IndexOf("Radeon", StringComparison.OrdinalIgnoreCase) >= 0 || prov.IndexOf("Advanced Micro Devices", StringComparison.OrdinalIgnoreCase) >= 0)
                                            {
                                                vendor = "AMD";
                                                dlUrl = "https://www.amd.com/de/support";
                                            }
                                            else if (desc.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0 || prov.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0)
                                            {
                                                vendor = "Intel";
                                                dlUrl = "https://www.intel.de/content/www/de/de/download-center/home.html";
                                            }

                                            if (!String.IsNullOrEmpty(dlUrl))
                                            {
                                                QuickFixItem it = new QuickFixItem();
                                                it.Level = "warn";
                                                it.Badge = "Treiber";
                                                it.Title = "Grafiktreiber veraltet (" + desc + ")";
                                                it.Desc = "Der installierte Grafiktreiber ist über 18 Monate alt (" + dtDriver.ToString("dd.MM.yyyy") + ")." +
                                                          (HwIsLaptop ? " Für Notebooks wird vorrangig die Treiberseite des Notebook-Herstellers empfohlen." : "");
                                                it.ButtonText = "Treiber laden";
                                                string targetUrl = dlUrl;
                                                it.Action = delegate {
                                                    try { System.Diagnostics.Process.Start(targetUrl); } catch { }
                                                };
                                                it.Tip = "\u00D6ffnet die offizielle Treiber-Downloadseite von " + vendor + " im Webbrowser.";
                                                items.Add(it);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            // 7. Resizable BAR / SAM Inaktiv
            try
            {
                if (HwRebarSupported && !HwRebarActive)
                {
                    QuickFixItem it = new QuickFixItem();
                    it.Level = "warn";
                    it.Badge = "Leistung";
                    it.Title = "Resizable BAR / SAM inaktiv";
                    it.Desc = "Resizable BAR wird von Grafikkarte und Plattform unterstützt, ist aber im BIOS nicht aktiviert (verschenkt 5 bis 15 % Leistung).";
                    it.ButtonText = "Anleitung";
                    it.Action = delegate { ShowRebarInstructionsDialog(); };
                    it.Tip = "\u00D6ffnet eine Schritt-f\u00FCr-Schritt-Anleitung zur Aktivierung von Resizable BAR / SAM im BIOS/UEFI.";
                    items.Add(it);
                }
            }
            catch { }
e.Result = items;
        };

        bw.RunWorkerCompleted += delegate(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || container.IsDisposed) return;
            List<QuickFixItem> items = e.Result as List<QuickFixItem>;
            if (items == null) return;

            container.SuspendLayout();
            container.Controls.Clear();

            if (items.Count == 0)
            {
                Label lblOk = Lbl("Keine Auffälligkeiten erkannt. Das System ist optimal konfiguriert.", 9f, false, UI.Ok);
                lblOk.Location = new Point(0, UI.S(16));
                lblOk.AutoSize = true;
                Tip(lblOk, "Alle schnellen Konfigurationsprüfungen sind unauffällig.");
                container.Controls.Add(lblOk);
            }
            else
            {
                int curY = 0;
                for (int i = 0; i < items.Count && i < 2; i++)
                {
                    QuickFixItem it = items[i];
                    Panel row = new Panel();
                    row.Location = new Point(0, curY);
                    row.Width = Math.Max(UI.S(300), container.ClientSize.Width);
                    row.Height = UI.S(56);
                    row.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                    Color bbg = it.Level == "warn" ? (UI.IsDark ? Color.FromArgb(54, 44, 18) : Color.FromArgb(254, 249, 231)) : (UI.IsDark ? Color.FromArgb(22, 40, 60) : Color.FromArgb(235, 243, 250));
                    Color borderColor = it.Level == "warn" ? (UI.IsDark ? Color.FromArgb(217, 155, 0) : Color.FromArgb(190, 135, 0)) : (UI.IsDark ? Color.FromArgb(0, 120, 212) : Color.FromArgb(0, 100, 180));

                    Panel capturedRow = row;
                    row.Paint += delegate(object s, PaintEventArgs pe) {
                        pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (GraphicsPath rp = UI.Round(new RectangleF(0.5f, 0.5f, capturedRow.Width - 1.5f, capturedRow.Height - 1.5f), UI.SF(6)))
                        {
                            using (SolidBrush rb = new SolidBrush(bbg)) pe.Graphics.FillPath(rb, rp);
                            using (Pen rpen = new Pen(borderColor)) pe.Graphics.DrawPath(rpen, rp);
                        }
                    };

                    Badge badge = new Badge(it.Badge, it.Level);
                    badge.Location = new Point(UI.S(10), UI.S(12));
                    Tip(badge, it.Badge + ": " + it.Title);

                    int textX = UI.S(78);
                    int actionW = UI.S(78);
                    int textW = Math.Max(UI.S(100), row.Width - textX - actionW - UI.S(10));

                    Label lblT = Lbl(it.Title, 8.75f, true, UI.Text);
                    lblT.AutoSize = false;
                    lblT.Location = new Point(textX, UI.S(8));
                    lblT.Size = new Size(textW, UI.S(18));
                    lblT.AutoEllipsis = true;
                    lblT.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    Tip(lblT, it.Title);

                    Label lblD = Lbl(it.Desc, 7.75f, false, UI.Muted);
                    lblD.AutoSize = false;
                    lblD.Location = new Point(textX, UI.S(27));
                    lblD.Size = new Size(textW, UI.S(24));
                    lblD.AutoEllipsis = true;
                    lblD.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    Tip(lblD, it.Desc);

                    LinkLabel lnkAct = new LinkLabel();
                    lnkAct.Text = it.ButtonText;
                    lnkAct.Font = new Font("Segoe UI Semibold", 8.75f);
                    lnkAct.LinkColor = UI.Accent;
                    lnkAct.ActiveLinkColor = UI.AccentHover;
                    lnkAct.LinkBehavior = LinkBehavior.HoverUnderline;
                    lnkAct.AutoSize = false;
                    lnkAct.Size = new Size(actionW, UI.S(20));
                    lnkAct.TextAlign = ContentAlignment.MiddleRight;
                    lnkAct.Location = new Point(row.Width - actionW - UI.S(10), UI.S(17));
                    lnkAct.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                    Tip(lnkAct, it.Tip);
                    QuickFixItem capturedIt = it;
                    lnkAct.LinkClicked += delegate { if (capturedIt.Action != null) capturedIt.Action(); };

                    row.Resize += delegate {
                        int w = Math.Max(UI.S(100), capturedRow.Width - textX - actionW - UI.S(10));
                        lblT.Size = new Size(w, UI.S(18));
                        lblD.Size = new Size(w, UI.S(24));
                        lnkAct.Location = new Point(capturedRow.Width - actionW - UI.S(10), UI.S(17));
                    };

                    row.Controls.Add(badge);
                    row.Controls.Add(lblT);
                    row.Controls.Add(lblD);
                    row.Controls.Add(lnkAct);
                    container.Controls.Add(row);

                    curY += UI.S(62);
                }
            }
            container.ResumeLayout(true);
        };

        bw.RunWorkerAsync();
    }

    void ShowInventoryDialog()
    {
        using (Form dlg = new Form())
        {
            dlg.Text = "Hardware-Inventar";
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false; dlg.MinimizeBox = false;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ClientSize = new Size(UI.S(560), UI.S(400));
            dlg.BackColor = UI.Bg; dlg.Font = new Font("Segoe UI", 9.5f);

            Panel p = new Panel(); p.Dock = DockStyle.Fill; p.Padding = new Padding(UI.S(16)); p.BackColor = UI.Bg;
            Label lt = Lbl("Übersicht der erkannten Hardware", 12f, true, UI.Text);
            lt.Dock = DockStyle.Top; lt.Height = UI.S(32); p.Controls.Add(lt);

            TableLayoutPanel t = new TableLayoutPanel();
            t.Dock = DockStyle.Fill; t.ColumnCount = 2; t.AutoScroll = true;
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(140)));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            string cleanCpu = DbEntry.CleanCpuName(HwCpu);
            string cpuVal = cleanCpu.Length > 0 ? (cleanCpu + (HwCores > 0 ? " (" + HwCores + " Kerne, " + HwThreads + " Threads)" : "")) : "Standard-Prozessor";

            string[][] rows = new string[][] {
                new string[] { "Computer", Environment.MachineName },
                new string[] { "Betriebssystem", OsName() },
                new string[] { "Prozessor", cpuVal },
                new string[] { "Mainboard", HwBoard.Length > 0 ? HwBoard : "Standard-Mainboard" },
                new string[] { "Arbeitsspeicher", HwRam.Length > 0 ? HwRam : "Arbeitsspeicher" },
                new string[] { "Grafik", GpuNames.Length > 0 ? String.Join(", ", GpuNames) : "Standard-Grafik" },
                new string[] { "Datenträger", GetDisksSummary() }
            };

            for (int i = 0; i < rows.Length; i++)
            {
                Label lKey = Lbl(rows[i][0], 9.25f, true, UI.Muted);
                lKey.Margin = new Padding(0, UI.S(6), UI.S(8), UI.S(6));
                Label lVal = Lbl(rows[i][1], 9.25f, false, UI.Text);
                lVal.Margin = new Padding(0, UI.S(6), 0, UI.S(6));
                t.Controls.Add(lKey, 0, i);
                t.Controls.Add(lVal, 1, i);
            }

            FlowLayoutPanel bf = new FlowLayoutPanel(); bf.Dock = DockStyle.Bottom; bf.Height = UI.S(44); bf.FlowDirection = FlowDirection.RightToLeft;
            Button btnClose = UI.Primary("Schließen"); btnClose.DialogResult = DialogResult.OK;
            Tip(btnClose, "Schließt das Inventarfenster.");
            bf.Controls.Add(btnClose);

            p.Controls.Add(t);
            t.BringToFront();
            p.Controls.Add(bf);
            dlg.Controls.Add(p);
            dlg.AcceptButton = btnClose;
            UI.ThemeDialog(dlg);
            dlg.ShowDialog(this);
        }
    }

    void ShowGuideDialog()
    {
        using (Form dlg = new Form())
        {
            dlg.Text = "Kurzanleitung";
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false; dlg.MinimizeBox = false;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ClientSize = new Size(UI.S(540), UI.S(420));
            dlg.BackColor = UI.Bg; dlg.Font = new Font("Segoe UI", 9.5f);

            Panel p = new Panel(); p.Dock = DockStyle.Fill; p.Padding = new Padding(UI.S(16)); p.BackColor = UI.Bg;
            Label lt = Lbl("So bedienst du Leos Minibench", 12f, true, UI.Text);
            lt.Dock = DockStyle.Top; lt.Height = UI.S(32); p.Controls.Add(lt);

            RichTextBox rt = new RichTextBox(); rt.ReadOnly = true; rt.BorderStyle = BorderStyle.None; rt.BackColor = UI.Bg; rt.ForeColor = UI.Text;
            rt.Dock = DockStyle.Fill; rt.Font = new Font("Segoe UI", 9.5f);
            Font fh = new Font("Segoe UI Semibold", 10f); Font fb = new Font("Segoe UI", 9f);

            string[][] steps = new string[][] {
                new string[] { "1. Ablauf wählen", "Hake in der linken Leiste die gewünschten Module an (Diagnose, Benchmark, Lasttest, Wartung oder Optimierung). Du kannst mehrere Module beliebig kombinieren." },
                new string[] { "2. Starten", "Klicke unten rechts auf den Startknopf. Vor dem Start zeigt die Leiste die geschätzte Gesamtdauer aller gewählten Prüfungen an." },
                new string[] { "3. Bericht lesen", "Nach Abschluss öffnet sich automatisch der detaillierte HTML-Bericht. Alle Befunde sind nach Dringlichkeit (Kritisch, Warnung, Hinweis) gegliedert." },
                new string[] { "4. Änderungen rückgängig machen", "Wartungs- und Optimierungsmaßnahmen werden im Änderungsprotokoll erfasst und können auf der Seite Änderungen einzeln zurückgenommen werden." }
            };

            foreach (string[] st in steps)
            {
                rt.SelectionFont = fh; rt.SelectionColor = UI.Text;
                rt.AppendText(st[0] + "\r\n");
                rt.SelectionFont = fb; rt.SelectionColor = UI.Muted;
                rt.AppendText(st[1] + "\r\n\r\n");
            }
            rt.SelectionStart = 0; rt.ScrollToCaret();

            FlowLayoutPanel bf = new FlowLayoutPanel(); bf.Dock = DockStyle.Bottom; bf.Height = UI.S(44); bf.FlowDirection = FlowDirection.RightToLeft;
            Button btnClose = UI.Primary("Schließen"); btnClose.DialogResult = DialogResult.OK;
            Tip(btnClose, "Schließt die Kurzanleitung.");
            bf.Controls.Add(btnClose);

            p.Controls.Add(rt);
            rt.BringToFront();
            p.Controls.Add(bf);
            dlg.Controls.Add(p);
            dlg.AcceptButton = btnClose;
            UI.ThemeDialog(dlg);
            dlg.ShowDialog(this);
        }
    }

    void ShowXmpInstructionsDialog()
    {
        using (Form dlg = new Form())
        {
            dlg.Text = "XMP / EXPO im BIOS aktivieren";
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false; dlg.MinimizeBox = false;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ClientSize = new Size(UI.S(540), UI.S(400));
            dlg.BackColor = UI.Bg; dlg.Font = new Font("Segoe UI", 9.5f);

            Panel p = new Panel(); p.Dock = DockStyle.Fill; p.Padding = new Padding(UI.S(16)); p.BackColor = UI.Bg;
            Label lt = Lbl("Arbeitsspeicher mit voller Geschwindigkeit betreiben", 12f, true, UI.Text);
            lt.Dock = DockStyle.Top; lt.Height = UI.S(32); p.Controls.Add(lt);

            RichTextBox rt = new RichTextBox(); rt.ReadOnly = true; rt.BorderStyle = BorderStyle.None; rt.BackColor = UI.Bg; rt.ForeColor = UI.Text;
            rt.Dock = DockStyle.Fill; rt.Font = new Font("Segoe UI", 9.5f);
            Font fh = new Font("Segoe UI Semibold", 10f); Font fb = new Font("Segoe UI", 9f);

            string[][] steps = new string[][] {
                new string[] { "Warum ist das wichtig?", "Ohne XMP oder EXPO läuft der Arbeitsspeicher nur mit dem JEDEC-Basistakt (z. B. 2133 MT/s bei DDR4 oder 4800 MT/s bei DDR5). Dadurch verschenkst du spürbar Prozessor- und Spieleleistung." },
                new string[] { "Schritt 1: BIOS aufrufen", "Starte den Computer neu und drücke beim Einschalten wiederholt die Taste Entf oder F2, um ins BIOS bzw. UEFI zu gelangen." },
                new string[] { "Schritt 2: Profil auswählen", "Suche im Menü nach der Option XMP (Intel/manche AMD), DOCP (ASUS AMD) oder EXPO (AMD DDR5) und wähle Profil 1." },
                new string[] { "Schritt 3: Speichern und neu starten", "Drücke Taste F10 zum Speichern und Neustarten. Windows startet nun mit dem höheren Speichertakt." },
                new string[] { "Schritt 4: Stabilität prüfen", "Führe in Leos Minibench einen Lasttest für den Arbeitsspeicher durch, um sicherzustellen, dass das System absolut stabil arbeitet." }
            };

            foreach (string[] st in steps)
            {
                rt.SelectionFont = fh; rt.SelectionColor = UI.Text;
                rt.AppendText(st[0] + "\r\n");
                rt.SelectionFont = fb; rt.SelectionColor = UI.Muted;
                rt.AppendText(st[1] + "\r\n\r\n");
            }
            rt.SelectionStart = 0; rt.ScrollToCaret();

            FlowLayoutPanel bf = new FlowLayoutPanel(); bf.Dock = DockStyle.Bottom; bf.Height = UI.S(44); bf.FlowDirection = FlowDirection.RightToLeft;
            Button btnClose = UI.Primary("Schließen"); btnClose.DialogResult = DialogResult.OK;
            Tip(btnClose, "Schließt die Anleitung.");
            bf.Controls.Add(btnClose);

            p.Controls.Add(rt);
            rt.BringToFront();
            p.Controls.Add(bf);
            dlg.Controls.Add(p);
            dlg.AcceptButton = btnClose;
            UI.ThemeDialog(dlg);
            dlg.ShowDialog(this);
        }
    }

    void ShowRebarInstructionsDialog()
    {
        using (Form dlg = new Form())
        {
            dlg.Text = "Resizable BAR / Smart Access Memory aktivieren";
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false; dlg.MinimizeBox = false;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ClientSize = new Size(UI.S(560), UI.S(440));
            dlg.BackColor = UI.Bg; dlg.Font = new Font("Segoe UI", 9.5f);

            Panel p = new Panel(); p.Dock = DockStyle.Fill; p.Padding = new Padding(UI.S(16)); p.BackColor = UI.Bg;
            Label lt = Lbl("Grafikleistung durch direkten VRAM-Zugriff steigern", 12f, true, UI.Text);
            lt.Dock = DockStyle.Top; lt.Height = UI.S(32); p.Controls.Add(lt);

            RichTextBox rt = new RichTextBox(); rt.ReadOnly = true; rt.BorderStyle = BorderStyle.None; rt.BackColor = UI.Bg; rt.ForeColor = UI.Text;
            rt.Dock = DockStyle.Fill; rt.Font = new Font("Segoe UI", 9.5f);
            Font fh = new Font("Segoe UI Semibold", 10f); Font fb = new Font("Segoe UI", 9f);

            string[][] steps = new string[][] {
                new string[] { "Was ist Resizable BAR / SAM?", "Resizable BAR (bei AMD Smart Access Memory) erlaubt der CPU den vollen, ungedrosselten Zugriff auf den gesamten Grafikspeicher (VRAM) auf einmal anstatt in winzigen 256-MB-Häppchen. Dies bringt je nach Spiel 5 bis 15 % Mehrleistung." },
                new string[] { "Schritt 1: Above 4G Decoding aktivieren", "Starte den PC ins UEFI/BIOS (Taste Entf oder F2). Navigiere zu den PCIe-/Grafik-Optionen (oft unter 'Erweitert' bzw. 'PCI Subsystem') und schalte 'Above 4G Decoding' auf 'Aktiviert' (Enabled)." },
                new string[] { "Schritt 2: Re-Size BAR Support aktivieren", "Sobald Above 4G Decoding aktiv ist, erscheint die Option 'Re-Size BAR Support'. Stelle diese auf 'Auto' oder 'Aktiviert' (Enabled)." },
                new string[] { "Schritt 3: CSM deaktivieren (reines UEFI)", "Resizable BAR erfordert zwingend den nativen UEFI-Modus mit GPT-Partitionstabelle. Deaktiviere das CSM (Compatibility Support Module) im Boot-Menü des BIOS." },
                new string[] { "Schritt 4: Speichern und neu starten", "Drücke Taste F10 zum Speichern und Neustarten. Windows initialisiert die Grafikkarte nun mit vollem BAR1-Adressfenster." }
            };

            foreach (string[] st in steps)
            {
                rt.SelectionFont = fh; rt.SelectionColor = UI.Text;
                rt.AppendText(st[0] + "\r\n");
                rt.SelectionFont = fb; rt.SelectionColor = UI.Muted;
                rt.AppendText(st[1] + "\r\n\r\n");
            }
            rt.SelectionStart = 0; rt.ScrollToCaret();

            FlowLayoutPanel bf = new FlowLayoutPanel(); bf.Dock = DockStyle.Bottom; bf.Height = UI.S(44); bf.FlowDirection = FlowDirection.RightToLeft;
            Button btnClose = UI.Primary("Schlie\u00DFen"); btnClose.DialogResult = DialogResult.OK;
            Tip(btnClose, "Schlie\u00DFt die Anleitung.");
            bf.Controls.Add(btnClose);

            p.Controls.Add(rt);
            rt.BringToFront();
            p.Controls.Add(bf);
            dlg.Controls.Add(p);
            dlg.AcceptButton = btnClose;
            UI.ThemeDialog(dlg);
            dlg.ShowDialog(this);
        }
    }
}
