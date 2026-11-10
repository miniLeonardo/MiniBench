// Laufansicht: Arbeitsprozess, Ereigniskanal, Abschluss (Teilklasse DiagGui)
public partial class DiagGui
{
    // ------------------------------------------------------------ Ablauf
    Panel BuildRun()
    {
        Panel p = new Panel(); p.BackColor = UI.Bg;
        TableLayoutPanel t = new TableLayoutPanel(); t.Dock = DockStyle.Fill; t.ColumnCount = 1; t.BackColor = UI.Bg;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(22)));
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.RowStyles.Add(new RowStyle(SizeType.Absolute, UI.S(16)));
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        TableLayoutPanel stepRow = new TableLayoutPanel(); stepRow.Dock = DockStyle.Fill; stepRow.AutoSize = true; stepRow.ColumnCount = 2; stepRow.BackColor = UI.Bg;
        stepRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); stepRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        // ab v3.65 an die Einstellungsseiten angeglichen: Titelgröße, Karte um die Listen, Fußleiste wie beim Start
        lblStep = Lbl("Wird gestartet ...", UI.SchriftTitel, true, UI.Text); lblStep.AutoEllipsis = true;
        lblCounter = Lbl("", 10f, false, UI.Muted); lblCounter.Anchor = AnchorStyles.Right; lblCounter.Margin = new Padding(UI.S(3), UI.S(6), 0, UI.S(3));
        stepRow.Controls.Add(lblStep, 0, 0); stepRow.Controls.Add(lblCounter, 1, 0);
        t.Controls.Add(stepRow, 0, 0);
        barAll = new FlatBar(); barAll.Dock = DockStyle.Fill; barAll.Margin = new Padding(UI.S(3), UI.S(4), UI.S(3), UI.S(6)); t.Controls.Add(barAll, 0, 1);
        lblSub = Lbl(" ", 9.5f, false, UI.Muted); lblSub.AutoEllipsis = true; lblSub.Margin = new Padding(UI.S(3), UI.S(4), UI.S(3), UI.S(2)); t.Controls.Add(lblSub, 0, 2);
        barSub = new FlatBar(); barSub.Dock = DockStyle.Fill; barSub.Fill = UI.AccentActive; barSub.Margin = new Padding(UI.S(3), UI.S(3), UI.S(3), UI.S(5)); t.Controls.Add(barSub, 0, 3);
        lblPar = Lbl(" ", 9f, false, UI.Muted); lblPar.AutoEllipsis = true; lblPar.Margin = new Padding(UI.S(3), 0, UI.S(3), 0); lblPar.Visible = false; t.Controls.Add(lblPar, 0, 4);

        FlowLayoutPanel cards = new FlowLayoutPanel(); cards.AutoSize = true; cards.BackColor = UI.Bg; cards.Margin = new Padding(0, UI.S(14), 0, UI.S(10));
        cardK = new StatCard("kritisch", UI.Crit); cardW = new StatCard("Warnungen", UI.Warn); cardI = new StatCard("Hinweise", UI.Info); cardT = new StatCard("Tests bestanden", UI.Ok);
        cards.Controls.Add(cardK); cards.Controls.Add(cardW); cards.Controls.Add(cardI); cards.Controls.Add(cardT);
        t.Controls.Add(cards, 0, 5);

        tabs = new TabStrip(); tabs.Dock = DockStyle.Fill; tabs.Items.Add("Befunde"); tabs.Items.Add("Tests"); tabs.Items.Add("Leistung"); tabs.Items.Add("Sensoren"); tabs.Items.Add("Protokoll");
        t.Controls.Add(tabs, 0, 6);

        CardPanel host = new CardPanel(); host.Dock = DockStyle.Fill; host.Padding = new Padding(UI.S(6)); host.Margin = new Padding(0, UI.S(6), 0, UI.S(12));
        lvFind = MakeList(new string[] { "Stufe", "Bereich", "Befund" }, new int[] { UI.S(110), UI.S(140), UI.S(700) });
        lvTests = MakeList(new string[] { "Ergebnis", "Test", "Details" }, new int[] { UI.S(110), UI.S(320), UI.S(520) });
        lvBench = MakeList(new string[] { "Ergebnis", "Messung", "Wert", "Index", "Referenz", "Vergleich" }, new int[] { UI.S(110), UI.S(330), UI.S(160), UI.S(190), UI.S(95), UI.S(220) });
        lvBench.Tag = "bench";
        txtLog = new TextBox(); txtLog.Multiline = true; txtLog.ReadOnly = true; txtLog.ScrollBars = ScrollBars.Vertical; txtLog.WordWrap = true;
        txtLog.Dock = DockStyle.Fill; txtLog.Font = new Font("Consolas", 9.75f); txtLog.BorderStyle = BorderStyle.None;
        ThemeLog();
        Panel sensHost = new Panel(); sensHost.Dock = DockStyle.Fill; sensHost.BackColor = UI.Panel; sensHost.Padding = new Padding(10, 6, 10, 6);
        chartRun = new SensorChart(); chartRun.Dock = DockStyle.Fill; chartRun.WindowSec = 0;
        chartRun.Empty = "Kurven erscheinen hier während des Benchmarks und des Lasttests (Temperatur, Takt, Leistung von CPU und GPU).";
        lblRunSens = Lbl(" ", 9f, false, UI.Muted); lblRunSens.Dock = DockStyle.Top; lblRunSens.AutoSize = false; lblRunSens.Height = 22;
        sensHost.Controls.Add(chartRun); sensHost.Controls.Add(lblRunSens);
        host.Controls.Add(lvFind); host.Controls.Add(lvTests); host.Controls.Add(lvBench); host.Controls.Add(sensHost); host.Controls.Add(txtLog);
        lvTests.Visible = false; lvBench.Visible = false; sensHost.Visible = false; txtLog.Visible = false;
        tabs.SelectedChanged += delegate {
            lvFind.Visible = tabs.Selected == 0; lvTests.Visible = tabs.Selected == 1; lvBench.Visible = tabs.Selected == 2; sensHost.Visible = tabs.Selected == 3; txtLog.Visible = tabs.Selected == 4;
        };
        t.Controls.Add(host, 0, 7);

        TableLayoutPanel foot = new TableLayoutPanel(); foot.Dock = DockStyle.Fill; foot.AutoSize = true; foot.ColumnCount = 2; foot.BackColor = UI.Bg;
        foot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); foot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        lblResult = Lbl("", 10.5f, true, UI.Text); lblResult.Anchor = AnchorStyles.Left; lblResult.AutoEllipsis = true;
        FlowLayoutPanel btns = new FlowLayoutPanel(); btns.AutoSize = true; btns.WrapContents = false; btns.BackColor = UI.Bg; btns.Anchor = AnchorStyles.Right;
        btnStopWait = UI.Secondary("Test beenden"); btnSkipStep = UI.SkipStepButton(); btnCancel = UI.Secondary("Abbrechen");
        btnFolder = UI.Secondary("Ordner öffnen"); btnNew = UI.Secondary("Zurück"); btnKi = UI.Secondary("KI-Datei zeigen");
        btnExportRun = UI.Secondary("Lauf exportieren …"); btnHtml = UI.Primary("Bericht öffnen");
        btnHtml.Margin = new Padding(UI.S(UI.Abstand5), 0, 0, 0); btnHtml.MinimumSize = new Size(UI.S(150), UI.S(44));
        foreach (Button sb in new Button[] { btnStopWait, btnSkipStep, btnCancel, btnFolder, btnNew, btnKi, btnExportRun }) { sb.Margin = new Padding(UI.S(8), UI.S(6), 0, 0); sb.MinimumSize = new Size(0, UI.S(32)); }
        btns.Controls.Add(btnStopWait); btns.Controls.Add(btnSkipStep); btns.Controls.Add(btnCancel); btns.Controls.Add(btnNew); btns.Controls.Add(btnFolder); btns.Controls.Add(btnExportRun); btns.Controls.Add(btnKi); btns.Controls.Add(btnHtml);
        foot.Controls.Add(lblResult, 0, 0); foot.Controls.Add(btns, 1, 0);
        t.Controls.Add(foot, 0, 8);

        btnStopWait.Click += delegate {
            try { Directory.CreateDirectory(cpDir); File.WriteAllText(Path.Combine(cpDir, "stop.flag"), "1"); btnStopWait.Enabled = false; }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Leos Minibench"); }
        };
        btnSkipStep.Click += delegate {
            try { Directory.CreateDirectory(cpDir); File.WriteAllText(Path.Combine(cpDir, "skip.flag"), "1"); btnSkipStep.Enabled = false; }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Leos Minibench"); }
        };
        btnCancel.Click += delegate { AskCancel(); };
        btnHtml.Click += delegate { OpenShell(htmlPath); };
        btnFolder.Click += delegate { OpenShell(outDir); };
        btnKi.Click += delegate { OpenSelect(kiPath); };
        btnExportRun.Click += delegate { ExportCurrentRun(); };
        btnNew.Click += delegate {
            ReloadDb();
            runView.Visible = false;
            if (resultDashboard != null) resultDashboard.Visible = false;
            contentHost.Visible = true;
            if (foot != null) foot.Visible = true;
        };
        p.Controls.Add(t);
        ApplyRunTips();
        return p;
    }

    // Stufe als Etikett: "KRITISCH" wird "Kritisch", kurze Kürzel wie "OK" bleiben
    public static string BadgeCase(string t)
    {
        t = (t ?? "").Trim();
        if (t.Length <= 2) return t.ToUpperInvariant();
        return t.Substring(0, 1).ToUpperInvariant() + t.Substring(1).ToLowerInvariant();
    }

    // Protokollfeld: im Dunkelschema konsolenartig dunkel, im Hellschema hell (vorher in beiden Schemata dunkelblau)
    void ThemeLog()
    {
        if (txtLog == null) return;
        txtLog.BackColor = UI.IsDark ? Color.FromArgb(18, 24, 34) : Color.FromArgb(246, 247, 249);
        txtLog.ForeColor = UI.IsDark ? Color.FromArgb(214, 222, 235) : Color.FromArgb(28, 29, 31);
    }

    ListView MakeList(string[] cols, int[] widths)
    {
        ListView lv = new ListView(); lv.View = View.Details; lv.FullRowSelect = true; lv.Dock = DockStyle.Fill; lv.HideSelection = false;
        lv.HeaderStyle = ColumnHeaderStyle.Nonclickable; lv.ShowItemToolTips = true; lv.BorderStyle = BorderStyle.None; lv.OwnerDraw = true;
        lv.Font = new Font("Segoe UI", 9.75f); lv.BackColor = UI.Panel; lv.ForeColor = UI.Text;
        ImageList rowHeight = new ImageList(); rowHeight.ImageSize = new Size(1, UI.S(34)); lv.SmallImageList = rowHeight;
        for (int i = 0; i < cols.Length; i++) lv.Columns.Add(cols[i], widths[i]);
        lv.DrawColumnHeader += delegate(object s, DrawListViewColumnHeaderEventArgs e) {
            Color headerBg = UI.IsDark ? Color.FromArgb(28, 30, 32) : Color.FromArgb(248, 249, 251);
            using (SolidBrush b = new SolidBrush(headerBg)) e.Graphics.FillRectangle(b, e.Bounds);
            using (Pen pen = new Pen(UI.Line)) e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            using (Font f = new Font("Segoe UI Semibold", 8.5f))
                TextRenderer.DrawText(e.Graphics, e.Header.Text.ToUpperInvariant(), f, new Rectangle(e.Bounds.X + UI.S(10), e.Bounds.Y, e.Bounds.Width - UI.S(12), e.Bounds.Height), UI.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        };
        lv.DrawItem += delegate(object s, DrawListViewItemEventArgs e) { };
        lv.DrawSubItem += delegate(object s, DrawListViewSubItemEventArgs e) {
            Graphics g = e.Graphics;
            bool hdr = "hdr".Equals(e.Item.Tag);
            if (hdr && (e.ColumnIndex == 2 || e.ColumnIndex == 3)) return;
            Rectangle rb = e.Bounds;
            if (hdr && e.ColumnIndex == 1) { for (int ci = 2; ci <= 3 && ci < lv.Columns.Count; ci++) rb.Width += lv.Columns[ci].Width; }
            Color back = e.Item.Selected ? UI.AccentSoft : (hdr ? (UI.IsDark ? Color.FromArgb(32, 34, 38) : Color.FromArgb(243, 245, 249)) : UI.Panel);
            using (SolidBrush b = new SolidBrush(back)) g.FillRectangle(b, rb);
            Color borderLine = UI.IsDark ? Color.FromArgb(45, 48, 52) : Color.FromArgb(236, 239, 243);
            using (Pen pen = new Pen(borderLine)) g.DrawLine(pen, rb.Left, rb.Bottom - 1, rb.Right, rb.Bottom - 1);
            string txt = e.SubItem.Text;
            if (hdr && e.ColumnIndex == 1)
            {
                int w = rb.Width;
                using (Font fb = new Font("Segoe UI Semibold", 10f))
                {
                    Size sz = TextRenderer.MeasureText(g, txt, fb);
                    TextRenderer.DrawText(g, txt, fb, new Rectangle(e.Bounds.X + UI.S(10), e.Bounds.Y, sz.Width + UI.S(4), e.Bounds.Height), UI.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                    string sub = e.Item.SubItems.Count > 2 ? e.Item.SubItems[2].Text : "";
                    if (sub.Length > 0)
                        TextRenderer.DrawText(g, sub, lv.Font, new Rectangle(e.Bounds.X + UI.S(18) + sz.Width, e.Bounds.Y, w - sz.Width - UI.S(26), e.Bounds.Height), UI.Muted,
                            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                }
            }
            else if (hdr && e.ColumnIndex == 4 && txt.Length > 0)
            {
                using (Font fb = new Font("Segoe UI Semibold", 9.75f))
                    TextRenderer.DrawText(g, txt, fb, new Rectangle(e.Bounds.X + UI.S(10), e.Bounds.Y, e.Bounds.Width - UI.S(14), e.Bounds.Height), UI.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
            else if (e.ColumnIndex == 0 && txt.Length == 0) { }
            else if (e.ColumnIndex == 0)
            {
                // ab v3.65 wie die Etiketten der Einstellungsseiten: gleiche Schrift, Höhe und Schreibweise
                Color fg, bg; UI.Level(txt, out fg, out bg);
                string label = BadgeCase(txt);
                using (Font f = new Font("Segoe UI Semibold", 8.25f))
                {
                    Size sz = TextRenderer.MeasureText(g, label, f, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                    int pillH = Math.Max(UI.S(18), sz.Height + UI.S(4));
                    int pillW = sz.Width + UI.S(14);
                    RectangleF pill = new RectangleF(e.Bounds.X + UI.S(8), e.Bounds.Y + (e.Bounds.Height - pillH) / 2f, pillW, pillH);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath gp = UI.Round(pill, (pillH - 1) / 2f)) using (SolidBrush pb = new SolidBrush(bg)) g.FillPath(pb, gp);
                    g.SmoothingMode = SmoothingMode.None;
                    TextRenderer.DrawText(g, label, f, Rectangle.Round(pill), fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
            else if ("bench".Equals(lv.Tag) && e.ColumnIndex == 3)
            {
                int iv; double wv;
                int textW = UI.S(48);
                int barMargin = UI.S(10);
                int barGap = UI.S(6);
                int totalReserve = barMargin + barGap + textW;
                if (txt.StartsWith("W") && double.TryParse(txt.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out wv))
                {
                    // WinSAT: Skala 1,0 bis 9,9, Farbe nach Bewertung
                    Color fc = wv >= 7 ? UI.Ok : wv >= 5 ? UI.Info : wv >= 3.5 ? UI.Warn : UI.Crit;
                    int bw = Math.Max(UI.S(40), e.Bounds.Width - totalReserve);
                    int barH = UI.S(8);
                    RectangleF tr = new RectangleF(e.Bounds.X + barMargin, e.Bounds.Y + (e.Bounds.Height - barH) / 2f, bw, barH);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath gp = UI.Round(tr, UI.SF(4))) using (SolidBrush b0 = new SolidBrush(UI.SkipBg)) g.FillPath(b0, gp);
                    float fw = Math.Max(UI.SF(6f), (float)(bw * (Math.Min(9.9, Math.Max(1.0, wv)) - 1.0) / 8.9));
                    using (GraphicsPath gf = UI.Round(new RectangleF(tr.X, tr.Y, fw, tr.Height), UI.SF(4))) using (SolidBrush b1 = new SolidBrush(fc)) g.FillPath(b1, gf);
                    g.SmoothingMode = SmoothingMode.None;
                    TextRenderer.DrawText(g, wv.ToString("0.0", CultureInfo.GetCultureInfo("de-DE")), (wsBold ?? (wsBold = new Font(lv.Font, FontStyle.Bold))), new Rectangle((int)tr.Right + barGap, e.Bounds.Y, textW, e.Bounds.Height), UI.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                }
                else if (int.TryParse(txt, out iv))
                {
                    Color fg, bg; UI.Level(e.Item.SubItems[0].Text, out fg, out bg);
                    int bw = Math.Max(UI.S(40), e.Bounds.Width - totalReserve);
                    int barH = UI.S(8);
                    RectangleF tr = new RectangleF(e.Bounds.X + barMargin, e.Bounds.Y + (e.Bounds.Height - barH) / 2f, bw, barH);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath gp = UI.Round(tr, UI.SF(4))) using (SolidBrush b0 = new SolidBrush(UI.SkipBg)) g.FillPath(b0, gp);
                    float fw = Math.Max(UI.SF(4f), bw * Math.Min(150, Math.Max(0, iv)) / 150f);
                    using (GraphicsPath gf = UI.Round(new RectangleF(tr.X, tr.Y, fw, tr.Height), UI.SF(4))) using (SolidBrush b1 = new SolidBrush(fg)) g.FillPath(b1, gf);
                    using (Pen pm = new Pen(UI.Muted, UI.SF(2f))) g.DrawLine(pm, tr.X + bw * 100f / 150f, tr.Y - UI.SF(3), tr.X + bw * 100f / 150f, tr.Bottom + UI.SF(3));
                    g.SmoothingMode = SmoothingMode.None;
                    TextRenderer.DrawText(g, txt, lv.Font, new Rectangle((int)tr.Right + barGap, e.Bounds.Y, textW, e.Bounds.Height), UI.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                }
                else if (txt.Length > 0)
                    TextRenderer.DrawText(g, txt, lv.Font, new Rectangle(e.Bounds.X + UI.S(10), e.Bounds.Y, e.Bounds.Width - UI.S(14), e.Bounds.Height), UI.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            else
            {
                TextRenderer.DrawText(g, txt, lv.Font, new Rectangle(e.Bounds.X + UI.S(10), e.Bounds.Y, e.Bounds.Width - UI.S(14), e.Bounds.Height), UI.Text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        };
        lv.Resize += delegate {
            int w = lv.ClientSize.Width; for (int i = 0; i < lv.Columns.Count - 1; i++) w -= lv.Columns[i].Width;
            if (w > UI.S(150)) lv.Columns[lv.Columns.Count - 1].Width = w - 2;
        };
        lv.DoubleClick += delegate {
            if (lv.SelectedItems.Count == 0) return;
            ListViewItem it = lv.SelectedItems[0];
            StringBuilder mb = new StringBuilder();
            for (int i = 0; i < it.SubItems.Count; i++) if (it.SubItems[i].Text.Length > 0) mb.AppendLine(lv.Columns[i].Text + ":  " + it.SubItems[i].Text);
            if (!String.IsNullOrEmpty(it.ToolTipText)) { mb.AppendLine(); mb.AppendLine(it.ToolTipText); }
            MessageBox.Show(this, mb.ToString(), "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        return lv;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ApplyTitleBarTheme(UI.IsDark);
        ApplyNativeControlThemes();
        BringToFront2();
        BeginInvoke(new System.Windows.Forms.MethodInvoker(BringToFront2));
        BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate { ApplyNativeControlThemes(); }));
        // erst nach dem ersten Zeichnen: Startfenster der exe schließen und die Startzeit protokollieren
        BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate { StartLog.Phase("Oberfläche bereit"); StartLog.SignalReady(); StartLog.Finish(dataDir, version, script); }));
        // Zustand der Optimierung erst nach dem Start lesen, damit er die Startzeit nicht verlängert
        System.Windows.Forms.Timer zt = new System.Windows.Forms.Timer(); zt.Interval = 2000;
        zt.Tick += delegate { zt.Stop(); zt.Dispose(); StartOptStateCheck(); };
        zt.Start();
    }

    void BringToFront2()
    {
        try
        {
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            ShowWindow(Handle, 9);
            IntPtr fg = GetForegroundWindow();
            uint fgT = GetWindowThreadProcessId(fg, IntPtr.Zero); uint me = GetCurrentThreadId();
            bool att = fgT != 0 && fgT != me && AttachThreadInput(me, fgT, true);
            BringWindowToTop(Handle); SetForegroundWindow(Handle);
            if (att) AttachThreadInput(me, fgT, false);
            TopMost = true; TopMost = false; Activate();
        }
        catch { }
    }

    static Encoding OemEncoding()
    {
        try { return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage); } catch { return Encoding.Default; }
    }

    void Launch(string args, string title)
    {
        ProcessStartInfo psi = new ProcessStartInfo(psExe, args);
        psi.UseShellExecute = false; psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true; psi.RedirectStandardError = true; psi.RedirectStandardInput = true;
        psi.StandardOutputEncoding = new UTF8Encoding(false);
        psi.StandardErrorEncoding = OemEncoding();
        proc = new Process(); proc.StartInfo = psi; proc.EnableRaisingEvents = true;
        proc.OutputDataReceived += delegate(object s, DataReceivedEventArgs e) { if (e.Data != null) queue.Enqueue(e.Data); else outEof = true; };
        proc.ErrorDataReceived += delegate(object s, DataReceivedEventArgs e) { if (e.Data != null && e.Data.Trim().Length > 0) queue.Enqueue("@@LOG|Red|" + e.Data); };
        proc.Exited += delegate { exited = true; };

        lvFind.Items.Clear(); lvTests.Items.Clear(); lvBench.Items.Clear(); txtLog.Clear();
        cntK = 0; cntW = 0; cntI = 0; testsOk = 0; benchWarn = 0; benchCount = 0; benchHeads.Clear(); outEof = false; exitSeen = DateTime.MinValue; cardK.Count = 0; cardW.Count = 0; cardI.Count = 0; cardT.Count = 0;
        exited = false; done = false; gotDone = false; cancelled = false; htmlPath = ""; outDir = ""; kiPath = ""; kurzPath = "";
        runOutDir = ""; runWorkDir = ""; runArgs = args; runStarted = DateTime.Now;
        lblResult.Text = ""; lblStep.ForeColor = UI.Text; lblStep.Text = "Wird gestartet ..."; lblCounter.Text = title;
        barAll.Value = 0; barAll.Marquee = false; barSub.Value = 0; barSub.Marquee = true; barAll.Fill = UI.Accent;
        tabs.SetText(0, "Befunde"); tabs.SetText(1, "Tests"); tabs.SetText(2, "Leistung"); tabs.SetText(3, "Sensoren"); tabs.Selected = 0;
        chartRun.Clear(); lblRunSens.Text = " ";
        parJobs.Clear(); parOrder.Clear(); lblPar.Text = " "; lblPar.Visible = false;
        try
        {
            proc.Start();
            proc.BeginOutputReadLine(); proc.BeginErrorReadLine();
            try { proc.StandardInput.Close(); } catch { }
        }
        catch (Exception ex) { MessageBox.Show(this, "PowerShell konnte nicht gestartet werden: " + ex.Message, "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
        StopOptStateCheck();   // Messungen nicht durch den Zustandsprozess belasten
        StopSoftwareStateCheck();
        running = true;
        for (int i = 0; i < nav.Count; i++) { nav[i].Running = false; nav[i].Invalidate(); }
        contentHost.Visible = false;
        if (foot != null) foot.Visible = false;
        if (resultDashboard != null) resultDashboard.Visible = false;
        runView.Visible = true;
        btnCancel.Visible = true; btnCancel.Enabled = true;
        btnSkipStep.Visible = true; btnSkipStep.Enabled = false;
        btnStopWait.Visible = false; btnStopWait.Enabled = true;
        btnHtml.Visible = false; btnFolder.Visible = false; btnNew.Visible = false; btnKi.Visible = false; btnExportRun.Visible = false;
        timer.Start();
    }

    static string Get(string[] p, int i) { return (i < p.Length) ? p[i] : ""; }
    static int ToInt(string[] p, int i) { int v; return int.TryParse(Get(p, i), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : 0; }

    void OnTick()
    {
        string line; int n = 0;
        StringBuilder log = new StringBuilder();
        // alle @@-Zeilen seit dem letzten Takt gesammelt verarbeiten: Listen und Layout nur einmal neu zeichnen
        if (!queue.IsEmpty)
        {
            runView.SuspendLayout();
            lvFind.BeginUpdate(); lvTests.BeginUpdate(); lvBench.BeginUpdate();
            try { while (n < 800 && queue.TryDequeue(out line)) { n++; HandleLine(line, log); } }
            finally { lvFind.EndUpdate(); lvTests.EndUpdate(); lvBench.EndUpdate(); runView.ResumeLayout(false); }
        }
        chartRun.Flush();
        if (log.Length > 0)
        {
            if (txtLog.TextLength > 1500000) txtLog.Text = txtLog.Text.Substring(txtLog.TextLength - 500000);
            txtLog.AppendText(log.ToString());
        }
        if (exited && exitSeen == DateTime.MinValue) exitSeen = DateTime.Now;
        bool eof = outEof;
        if (exited && !done && (eof || (DateTime.Now - exitSeen).TotalSeconds > 5) && queue.IsEmpty) Finish();
    }

    void HandleLine(string line, StringBuilder log)
    {
        if (!line.StartsWith("@@")) { log.AppendLine(line); return; }
        string[] p = line.Substring(2).Split('|');
        switch (p[0])
        {
            case "STEP":
                int s = ToInt(p, 1), t = ToInt(p, 2);
                SetText(lblStep, Get(p, 3));
                SetText(lblCounter, String.Format("Schritt {0} von {1}", s, t));
                if (t > 0) barAll.Value = (s - 1) * 100 / t;
                btnSkipStep.Enabled = false;
                // Die Abschnittszeile mit Uhrzeit und Zähler kommt schon als Konsolenausgabe (Add-Section); bis v4.01 stand sie doppelt im Protokoll
                break;
            case "SKIP_ALLOWED":
                btnSkipStep.Enabled = Get(p, 1) == "1";
                break;
            case "SCHRITT_UEBERSPRINGEN":
                btnSkipStep.Enabled = false;
                break;
            case "SUB":
                int pc = ToInt(p, 1);
                if (pc == -9) { SetText(lblSub, " "); barSub.Marquee = false; barSub.Value = 0; }
                else
                {
                    string st = Get(p, 3).Trim();
                    SetText(lblSub, Get(p, 2).Trim() + (st.Length > 0 ? "   ·   " + st : ""));
                    if (pc < 0) barSub.Marquee = true; else { barSub.Marquee = false; barSub.Value = pc; }
                }
                break;
            case "FIND": AddFinding(Get(p, 1), Get(p, 2), Get(p, 3)); break;
            case "TEST": AddTest(Get(p, 1), Get(p, 2), Get(p, 3)); break;
            case "LOG": log.AppendLine(p.Length > 2 ? String.Join("|", p, 2, p.Length - 2) : ""); break;
            case "BENCH": AddBench(p); break;
            case "PAR": UpdatePar(p); break;
            case "BGRP": UpdateBenchGroup(p); break;
            case "OPTE": UpdateOptResult(p); break;
            case "STOP": btnStopWait.Visible = Get(p, 1) == "1"; btnStopWait.Enabled = true; break;
            case "ORDNER": runOutDir = Get(p, 1); runWorkDir = Get(p, 2); break;
            case "SENSLIM": chartRun.SetLimits(p); break;
            case "UMZUG": break;
            case "SENSSTAT": UpdateSensStatusBadge(Get(p, 1), Get(p, 2)); break;
            case "SENSLEAD":
                {
                    bool firstLead = chartRun.Count == 0;
                    double[] r = chartRun.Add(p);
                    List<string> parts = new List<string>();
                    if (!double.IsNaN(r[1])) parts.Add("CPU " + SensorChart.Fmt(r[1], "°C"));
                    if (!double.IsNaN(r[3])) parts.Add(SensorChart.Fmt(r[3], "W"));
                    if (!double.IsNaN(r[2])) parts.Add("Takt " + SensorChart.Fmt(r[2], "MHz"));
                    if (!double.IsNaN(r[9])) parts.Add("höchster Kern " + SensorChart.Fmt(r[9], "MHz"));
                    if (!double.IsNaN(r[4]) || !double.IsNaN(r[5]) || !double.IsNaN(r[6]))
                    {
                        List<string> g = new List<string>();
                        if (!double.IsNaN(r[4])) g.Add(SensorChart.Fmt(r[4], "°C"));
                        if (!double.IsNaN(r[5])) g.Add(SensorChart.Fmt(r[5], "MHz"));
                        if (!double.IsNaN(r[6])) g.Add(SensorChart.Fmt(r[6], "W"));
                        parts.Add("GPU " + String.Join(" ", g.ToArray()));
                    }
                    if (!double.IsNaN(r[10])) parts.Add("iGPU " + SensorChart.Fmt(r[10], "°C"));
                    if (!double.IsNaN(r[7])) parts.Add("Lüfter " + SensorChart.Fmt(r[7], "U/min"));
                    if (!double.IsNaN(r[13])) parts.Add(SensorChart.Fmt(r[13], "Bilder/s") + (!double.IsNaN(r[14]) ? " und " + SensorChart.Fmt(r[14], "Bilder/s") : ""));
                    SetText(lblRunSens, parts.Count > 0 ? "Aktuell: " + String.Join("   ·   ", parts.ToArray()) : "Aktuell: keine Sensorwerte");
                    if (firstLead) tabs.SetText(3, "Sensoren (live)");
                    break;
                }
            case "DONE":
                htmlPath = Get(p, 2); outDir = Get(p, 3); nK = ToInt(p, 4); nW = ToInt(p, 5); nI = ToInt(p, 6); kurzPath = Get(p, 7); kiPath = Get(p, 8); gotDone = true;
                break;
            default: log.AppendLine(line); break;
        }
    }

    // @@PAR|Key|Titel|Status|Dauer: Hintergrundprüfungen im schnellen Modus
    void UpdatePar(string[] p)
    {
        string k = Get(p, 1); if (k.Length == 0) return;
        if (!parJobs.ContainsKey(k)) parOrder.Add(k);
        parJobs[k] = new string[] { Get(p, 2), Get(p, 3), Get(p, 4) };
        List<string> parts = new List<string>();
        foreach (string x in parOrder) { string[] v = parJobs[x]; parts.Add(v[0] + " " + v[1] + (v[2].Length > 0 ? " (" + v[2] + ")" : "")); }
        SetText(lblPar, "Parallel im Hintergrund:   " + String.Join("   ·   ", parts.ToArray()));
        lblPar.Visible = true;
    }

    void AddFinding(string level, string area, string text)
    {
        ListViewItem it = new ListViewItem(new string[] { level, area, text });
        it.ToolTipText = text;
        int pos = lvFind.Items.Count;
        if (level == "KRITISCH") { pos = cntK; cntK++; cardK.Count = cntK; }
        else if (level == "WARNUNG") { pos = cntK + cntW; cntW++; cardW.Count = cntW; }
        else { cntI++; cardI.Count = cntI; }
        lvFind.Items.Insert(pos, it);
        tabs.SetText(0, String.Format("Befunde ({0})", lvFind.Items.Count));
    }

    static string GroupName(string g)
    {
        if (g == "CPU") return "Prozessor"; if (g == "RAM") return "Arbeitsspeicher"; if (g == "GPU") return "Grafik";
        if (g == "Vergleich") return "Vergleich mit anderen Systemen"; if (g == "WinSAT") return "WinSAT"; return g;
    }

    ListViewItem BenchHead(string g)
    {
        ListViewItem h;
        if (benchHeads.TryGetValue(g, out h)) return h;
        h = new ListViewItem(new string[] { "", GroupName(g), "", "", "", "" }); h.Tag = "hdr";
        benchHeads[g] = h; lvBench.Items.Add(h);
        return h;
    }

    void AddBench(string[] p)
    {
        // BENCH|Status|Komponente|Messung|Anzeige|Index|Vergleich|Hinweis|Referenz|Gruppe
        string g = Get(p, 9); if (g.Length == 0) g = Get(p, 2) == "Datenträger" ? "Laufwerke" : Get(p, 2);
        ListViewItem h = BenchHead(g);
        ListViewItem it = new ListViewItem(new string[] { Get(p, 1), Get(p, 3), Get(p, 4), Get(p, 5), Get(p, 8), Get(p, 6) });
        it.ToolTipText = Get(p, 7);
        int pos = h.Index + 1;
        while (pos < lvBench.Items.Count && !"hdr".Equals(lvBench.Items[pos].Tag)) pos++;
        lvBench.Items.Insert(pos, it);
        if (g != "Vergleich")
        {
            benchCount++;
            if (Get(p, 1).Equals("Warnung", StringComparison.OrdinalIgnoreCase)) benchWarn++;
        }
        tabs.SetText(2, benchWarn > 0 ? String.Format("Leistung ({0} auffällig)", benchWarn) : String.Format("Leistung ({0})", benchCount));
    }

    void UpdateBenchGroup(string[] p)
    {
        ListViewItem h = BenchHead(Get(p, 1));
        h.SubItems[0].Text = Get(p, 3);
        h.SubItems[1].Text = Get(p, 2);
        h.SubItems[2].Text = Get(p, 4);
        h.SubItems[4].Text = Get(p, 5);
        h.ToolTipText = Get(p, 4);
        lvBench.Invalidate();
    }

    void AddTest(string name, string status, string detail)
    {
        ListViewItem it = new ListViewItem(new string[] { status, name, detail });
        it.ToolTipText = detail;
        lvTests.Items.Add(it);
        if (status.Equals("OK", StringComparison.OrdinalIgnoreCase) || status.Equals("Repariert", StringComparison.OrdinalIgnoreCase)) { testsOk++; cardT.Count = testsOk; }
        tabs.SetText(1, String.Format("Tests ({0})", lvTests.Items.Count));
    }

    void Finish()
    {
        done = true; running = false; timer.Stop();
        barSub.Marquee = false; barSub.Value = 0; lblSub.Text = " ";
        btnCancel.Visible = false; btnStopWait.Visible = false; btnSkipStep.Visible = false; btnNew.Visible = true;
        if (chartRun.Count > 0) tabs.SetText(3, "Sensoren");
        int code = -1; try { code = proc.ExitCode; } catch { }
        for (int i = 0; i < nav.Count; i++) { if (nav[i].Running) { nav[i].Running = false; nav[i].Invalidate(); } }
        if (gotDone)
        {
            barAll.Value = 100; barAll.Fill = nK > 0 ? UI.Crit : (nW > 0 ? Color.FromArgb(214, 140, 20) : UI.Ok); barAll.Invalidate();
            lblStep.Text = "Abgeschlossen";
            lblCounter.Text = "Bericht und KI-Dateien liegen im Datenordner";
            lblResult.Text = nK > 0 ? "Kritische Befunde gefunden" : (nW > 0 ? "Warnungen vorhanden" : "Keine Auff\u00E4lligkeiten");
            lblResult.ForeColor = nK > 0 ? UI.Crit : (nW > 0 ? UI.Warn : UI.Ok);
            btnHtml.Visible = File.Exists(htmlPath); btnFolder.Visible = Directory.Exists(outDir); btnKi.Visible = File.Exists(kiPath);
            btnExportRun.Visible = Directory.Exists(outDir);

            bool pureMaintenance = !nav[1].Checked && !nav[2].Checked && !nav[3].Checked && (nav[4].Checked || nav[5].Checked);
            if (pureMaintenance)
            {
                tabs.Selected = 1;
            }
            // ab v4.01 immer im Programmfenster (vorher nur mit der Option 1-Fenstermodus)
            else if (File.Exists(htmlPath))
            {
                PopulateResultDashboard();
                runView.Visible = false;
                ZeigeErgebnis();
            }
            else
            {
                if (btnHtml.Visible) OpenShell(htmlPath);
            }
        }
        else
        {
            lastStepText = lblStep.Text + "   ·   " + lblSub.Text;
            lblStep.Text = cancelled ? "Abgebrochen" : "Ohne Bericht beendet (Code " + code + ")";
            lblStep.ForeColor = UI.Crit; lblCounter.Text = "";
            CleanupDriver();
            string saved = SaveFailureLog(code);
            if (saved.Length > 0) { lblResult.Text = "Protokoll gesichert: " + saved; lblResult.ForeColor = UI.Muted; outDir = runOutDir; btnFolder.Visible = Directory.Exists(outDir); }
            if (!cancelled) tabs.Selected = 4;
        }
        try { if (!ContainsFocus) FlashWindow(Handle, true); } catch { }
    }

    void ExportCurrentRun()
    {
        if (String.IsNullOrEmpty(outDir) || !Directory.Exists(outDir)) return;
        using (SaveFileDialog sfd = new SaveFileDialog())
        {
            sfd.Title = "Lauf als Minibench-Paket exportieren";
            sfd.Filter = "Minibench-Paket (*.minibench)|*.minibench|ZIP-Archiv (*.zip)|*.zip|Alle Dateien (*.*)|*.*";
            sfd.FilterIndex = 1;
            string dirName = Path.GetFileName(outDir);
            sfd.FileName = (String.IsNullOrEmpty(dirName) ? "Lauf" : dirName) + ".minibench";
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            Cursor = Cursors.WaitCursor;
            List<string> lines;
            string res = RunHelper("-ExportLauf " + Q(outDir) + " -ExportZiel " + Q(sfd.FileName), out lines);
            Cursor = Cursors.Default;
            MessageBox.Show(this, lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Export abgeschlossen.", "Lauf exportieren", MessageBoxButtons.OK, res.StartsWith("1") ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
    }

    void AskCancel()
    {
        if (!running) return;
        if (MessageBox.Show(this, "Lauf wirklich abbrechen?", "Leos Minibench", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        cancelled = true;
        KillTree();
    }

    void KillTree()
    {
        try
        {
            if (proc != null && !proc.HasExited)
            {
                ProcessStartInfo k = new ProcessStartInfo("taskkill.exe", "/T /F /PID " + proc.Id);
                k.CreateNoWindow = true; k.UseShellExecute = false;
                Process kp = Process.Start(k); kp.WaitForExit(8000);
            }
        }
        catch { }
    }

    void OnClosing(object sender, FormClosingEventArgs e)
    {
        // Live-Ansicht sauber beenden, damit ein vorübergehend installierter Treiber entfernt wird
        if (liveRunning) StopLiveAndWait();
        StopSoftwareStateCheck();
        StopOptStateCheck();
        if (!running) { CleanupDriver(); return; }
        if (MessageBox.Show(this, "Der Lauf ist noch aktiv. Abbrechen und schließen?", "Leos Minibench", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) { e.Cancel = true; return; }
        cancelled = true; KillTree();
        try { if (proc != null) proc.WaitForExit(10000); } catch { }
        CleanupDriver();
    }

    static void OpenShell(string path)
    {
        if (String.IsNullOrEmpty(path)) return;
        try { Process.Start("explorer.exe", "\"" + path + "\""); } catch { }
    }

    static void OpenSelect(string path)
    {
        if (String.IsNullOrEmpty(path)) return;
        try { Process.Start("explorer.exe", "/select,\"" + path + "\""); } catch { }
    }
}
