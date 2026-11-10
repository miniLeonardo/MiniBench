// Seite Tools: Werkzeuge, Schnellstarter, Softwarepakete (winget) (Teilklasse DiagGui)
public partial class DiagGui
{
    // ab v4.05: kompakte Zeilen statt 68 px hoher Karten, bei breitem Fenster in zwei Spalten (vorher feste Gruppenbreite,
    // die rechte Hälfte blieb leer). Die sechs portablen Werkzeuge stehen zusammen, darunter die Windows-Schnellstarter
    // und die Softwarepakete als Liste in Spalten je Gruppe. Der Abschnitt Netzlaufwerk entfällt (Einstellungen › Daten
    // und Datenschutz). "Neu scannen" und "Tools-Ordner" stehen rechts im Seitenkopf.
    Action toolsLayout;
    Panel toolsKopf;
    List<FlowLayoutPanel> toolsRaster = new List<FlowLayoutPanel>();
    List<FlowLayoutPanel> paketZeilen = new List<FlowLayoutPanel>();
    Dictionary<string, CheckBox> paketBoxen = new Dictionary<string, CheckBox>(StringComparer.OrdinalIgnoreCase);
    Dictionary<string, string[]> paketInfo = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);   // Id -> Name, Beschreibung
    Label lblPaketZustand;
    public static string[] SoftwarePakete = new string[0];   // Zeilen aus Get-SoftwarePaketGuiLines (G|Gruppe, P|Gruppe|Id|Name|Beschreibung)

    // Breite der Inhaltsfläche der Seiten (ohne Seitenrand und Bildlaufleiste)
    int SeitenBreite()
    {
        if (content == null) return UI.S(UI.Gruppenbreite);
        int w = content.ClientSize.Width - content.Padding.Horizontal - UI.S(4);
        return Math.Max(UI.S(UI.Gruppenbreite), w);
    }

    // Zwei Spalten ab dieser Breite, sonst eine (Zeilen bleiben mindestens 440 px breit)
    public static int ToolSpalten(int breite) { return breite >= UI.S(900) ? 2 : 1; }

    Control BuildToolsPage()
    {
        FlowLayoutPanel f = Page("Tools", "Portable Werkzeuge, Windows-Schnellstarter und Softwarepakete über winget.", -1);
        toolsRaster.Clear(); paketZeilen.Clear(); paketBoxen.Clear(); paketInfo.Clear();

        // Seitenkopf: Titel und Einleitung links, die beiden Knöpfe rechts
        Control lt = f.Controls[0], lh = f.Controls[1];
        f.Controls.Clear();
        TableLayoutPanel kopf = new TableLayoutPanel(); kopf.ColumnCount = 2; kopf.RowCount = 1; kopf.AutoSize = true; kopf.Margin = new Padding(0);
        kopf.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); kopf.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        FlowLayoutPanel kt = new FlowLayoutPanel(); kt.FlowDirection = FlowDirection.TopDown; kt.WrapContents = false; kt.AutoSize = true; kt.Margin = new Padding(0);
        kt.Controls.Add(lt); kt.Controls.Add(lh);
        FlowLayoutPanel kb = new FlowLayoutPanel(); kb.AutoSize = true; kb.WrapContents = false; kb.Margin = new Padding(0, UI.S(4), 0, 0); kb.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        Button btnRefreshTools = UI.Secondary("Neu scannen"); btnRefreshTools.Margin = new Padding(0);
        Tip(btnRefreshTools, "Prüft erneut auf portable Werkzeuge im Tools-Ordner und auf Installationen, dazu die installierten Softwarepakete.");
        Button btnOpenToolsDir = UI.Secondary("Tools-Ordner"); btnOpenToolsDir.Margin = new Padding(UI.S(8), 0, 0, 0);
        Tip(btnOpenToolsDir, "Öffnet den Ordner Minibench-Daten\\Tools im Windows Explorer.");
        btnOpenToolsDir.Click += delegate { string td = Path.Combine(dataDir, "Tools"); try { Directory.CreateDirectory(td); } catch { } OpenShell(td); };
        kb.Controls.Add(btnRefreshTools); kb.Controls.Add(btnOpenToolsDir);
        kopf.Controls.Add(kt, 0, 0); kopf.Controls.Add(kb, 1, 0);
        toolsKopf = kopf;
        f.Controls.Add(kopf);

        // 1. Portable Werkzeuge (CPU-Z, GPU-Z, MSI Afterburner, Revo, Partition Wizard, WizTree)
        f.Controls.Add(Section("Portable Werkzeuge"));
        f.Controls.Add(Hint("Liegt ein Werkzeug im Ordner Minibench-Daten\\Tools, startet Leos Minibench diese Version ohne Installation, sonst eine vorhandene Installation. Heruntergeladen wird nichts automatisch.", 0));
        FlowLayoutPanel rw = ToolRaster(); f.Controls.Add(rw);
        Action fuelleWerkzeuge = delegate {
            rw.SuspendLayout();
            rw.Controls.Clear();
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            rw.Controls.Add(ToolZeile("CPU-Z", "Prozessor, Mainboard, Arbeitsspeicher mit Timings.", UI.IcoCpu, "CPU-Z",
                new string[] { "cpuz_x64.exe", "cpuz_x32.exe", "cpuz.exe" }, new string[] { "CPU-Z" },
                new string[] { Path.Combine(pf, @"CPUID\CPU-Z\cpuz.exe"), Path.Combine(pf86, @"CPUID\CPU-Z\cpuz.exe") },
                "https://www.cpuid.com/softwares/cpu-z.html", ""));
            rw.Controls.Add(ToolZeile("GPU-Z", "Grafikkarte mit Takt, Sensoren, PCIe und BIOS.", UI.IcoGpu, "GPU-Z",
                new string[] { "GPU-Z*.exe" }, new string[] { "GPU-Z" },
                new string[] { Path.Combine(pf86, @"GPU-Z\GPU-Z.exe"), Path.Combine(pf, @"GPU-Z\GPU-Z.exe") },
                "https://www.techpowerup.com/gpuz/", "Portabel: GPU-Z.<Version>.exe in den Tools-Ordner legen und beim ersten Start \"Nein\" für die Installation wählen; bei mehreren Dateien gilt die neueste Version."));
            rw.Controls.Add(ToolZeile("MSI Afterburner", "Takt, Lüfterkurve und Overlay der Grafikkarte.", UI.IcoFlame, "MSIAfterburner",
                new string[] { "MSIAfterburner.exe" }, new string[] { "MSI Afterburner" },
                new string[] { Path.Combine(pf86, @"MSI Afterburner\MSIAfterburner.exe"), Path.Combine(pf, @"MSI Afterburner\MSIAfterburner.exe") },
                "https://www.msi.com/Landing/afterburner/graphics-cards", "Nur als Setup erhältlich; es braucht Administratorrechte und bietet RivaTuner Statistics Server (Overlay, Bildratenbegrenzung) zur Installation mit an."));
            rw.Controls.Add(ToolZeile("Revo Uninstaller", "Gründlich deinstallieren, mit Resten.", UI.IcoEntfernen, "RevoUninstaller",
                new string[] { "RevoUPort.exe", "RevoUnin.exe", "RevoUninPro.exe", "RevoUninstaller_Portable.exe" }, new string[] { "Revo Uninstaller" },
                new string[] { Path.Combine(pf, @"VS Revo Group\Revo Uninstaller\RevoUnin.exe"), Path.Combine(pf86, @"VS Revo Group\Revo Uninstaller\RevoUnin.exe"),
                    Path.Combine(pf, @"VS Revo Group\Revo Uninstaller Pro\RevoUninPro.exe"), Path.Combine(pf86, @"VS Revo Group\Revo Uninstaller Pro\RevoUninPro.exe") },
                "https://www.revouninstaller.com/revo-uninstaller-free-download/", ""));
            rw.Controls.Add(ToolZeile("MiniTool Partition Wizard", "Partitionieren und Dateisysteme konvertieren.", UI.IcoDisk, "PartitionWizard",
                new string[] { "partitionwizard.exe", "PartitionWizard.exe", "MiniToolPartitionWizard.exe" }, new string[] { "Partition Wizard", "MiniTool Partition Wizard" },
                new string[] { Path.Combine(pf, @"MiniTool Partition Wizard 12\partitionwizard.exe"), Path.Combine(pf86, @"MiniTool Partition Wizard 12\partitionwizard.exe"),
                    Path.Combine(pf, @"MiniTool Partition Wizard\partitionwizard.exe"), Path.Combine(pf86, @"MiniTool Partition Wizard\partitionwizard.exe") },
                "https://www.partitionwizard.com/free-partition-manager.html", ""));
            rw.Controls.Add(ToolZeile("WizTree", "Speicherplatzanalyse direkt aus der MFT.", UI.IcoTorte, "WizTree",
                new string[] { "WizTree64.exe", "WizTree.exe" }, new string[] { "WizTree" },
                new string[] { Path.Combine(pf, @"WizTree\WizTree64.exe"), Path.Combine(pf, @"WizTree\WizTree.exe"), Path.Combine(pf86, @"WizTree\WizTree.exe") },
                "https://diskanalyzer.com/download", ""));
            rw.ResumeLayout(true);
        };
        fuelleWerkzeuge();

        // 2. Windows-Schnellstarter
        f.Controls.Add(Section("Windows-Schnellstarter"));
        FlowLayoutPanel rs = ToolRaster(); f.Controls.Add(rs);
        ToolRow rUefi = new ToolRow("Ins BIOS/UEFI neu starten", "Sofortiger Neustart in das UEFI-Setup (shutdown /r /fw /t 0), nach Rückfrage.", UI.IcoAdmin);
        Button bUefi = UI.Secondary("Neu starten"); bUefi.MinimumSize = new Size(UI.S(110), 0); bUefi.Margin = new Padding(0);
        Tip(bUefi, "Prüft auf UEFI-Unterstützung und führt nach Bestätigung 'shutdown.exe /r /fw /t 0' aus.");
        bUefi.Click += delegate {
            if (!IsUefiFirmware())
            {
                MessageBox.Show(this, "Dieser Computer verwendet ein klassisches BIOS oder meldet keine Unterstützung für den direkten Neustart in die UEFI-Firmware.", "BIOS/UEFI-Neustart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show(this, "Den Computer jetzt sofort neu starten und die UEFI/BIOS-Einstellungen öffnen?\r\n\r\nVorher alle geöffneten Dokumente und Arbeiten sichern.", "Ins BIOS/UEFI neu starten", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                StartAdminProcess("shutdown.exe", "/r /fw /t 0");
        };
        rUefi.MainButton = bUefi;
        Tip(rUefi, "Startet das System neu und leitet direkt in das UEFI-Setup weiter.");
        rs.Controls.Add(rUefi);
        rs.Controls.Add(LauncherRow("Datenträgerverwaltung", "Partitionen anlegen, verkleinern, Laufwerksbuchstaben (diskmgmt.msc).", UI.IcoOrdner, "diskmgmt.msc", ""));
        rs.Controls.Add(LauncherRow("Geräte-Manager", "Hardware, Treiber und Ressourcen prüfen (devmgmt.msc).", UI.IcoGeraete, "devmgmt.msc", ""));
        rs.Controls.Add(LauncherRow("Zuverlässigkeitsverlauf", "Stabilitätsindex mit Abstürzen und Fehlern im Zeitverlauf (perfmon /rel).", UI.IcoDiag, "perfmon.exe", "/rel"));

        // 3. Softwarepakete (winget), Liste aus Kern\Softwarepakete.psd1
        f.Controls.Add(Section("Softwarepakete installieren"));
        f.Controls.Add(Hint("Stille Installation über den Windows-Paket-Manager (winget). Jede Installation steht danach auf der Seite Änderungen und lässt sich dort wieder entfernen. Schon installierte Programme sind grün und gesperrt.", 0));
        BuildSoftwareListe(f);

        btnRefreshTools.Click += delegate { fuelleWerkzeuge(); ApplyThemeRecursive(rw); if (toolsLayout != null) toolsLayout(); StartSoftwareStateCheck(); };

        toolsLayout = delegate {
            int w = SeitenBreite();
            int sp = ToolSpalten(w), gap = UI.S(12);
            int zw = sp == 2 ? (w - gap) / 2 : w;
            if (toolsKopf != null) toolsKopf.MinimumSize = new Size(w, 0);
            foreach (FlowLayoutPanel r in toolsRaster)
            {
                // Größe selbst setzen: AutoSize behielt die Höhe der einspaltigen Anordnung beim ersten Aufbau
                r.SuspendLayout();
                int h = 0;
                for (int i = 0; i < r.Controls.Count; i++)
                {
                    Control c = r.Controls[i];
                    c.Width = zw;
                    c.Margin = new Padding(0, 0, sp == 2 && i % 2 == 0 ? gap : 0, UI.S(6));
                    if (i % sp == 0) h += c.Height + UI.S(6);
                }
                r.Size = new Size(w + 1, h);
                r.ResumeLayout(true);
            }
            foreach (FlowLayoutPanel z in paketZeilen) { z.MaximumSize = new Size(Math.Max(UI.S(200), w - UI.S(160)), 0); }
        };
        if (content != null) content.Resize += delegate { if (toolsLayout != null) toolsLayout(); };
        toolsLayout();
        return f;
    }

    FlowLayoutPanel ToolRaster()
    {
        FlowLayoutPanel r = new FlowLayoutPanel(); r.FlowDirection = FlowDirection.LeftToRight; r.WrapContents = true; r.AutoSize = false;
        r.BackColor = UI.Panel; r.Margin = new Padding(0); r.Padding = new Padding(0);
        toolsRaster.Add(r);
        return r;
    }

    // Softwarepakete: je Gruppe eine Zeile mit der Überschrift links und den Paketen in gleich breiten Spalten
    void BuildSoftwareListe(FlowLayoutPanel f)
    {
        List<string> gruppen = new List<string>();
        List<string[]> pakete = new List<string[]>();
        LiesPaketListe(SoftwarePakete, gruppen, pakete);
        if (pakete.Count == 0) { f.Controls.Add(Hint("Die Paketliste fehlt (Skript ohne Kern\\Softwarepakete.psd1).", 0)); return; }

        string[] allWgIds = new string[pakete.Count], allWgNames = new string[pakete.Count];
        CheckBox[] allWgBoxes = new CheckBox[pakete.Count];
        TableLayoutPanel t = new TableLayoutPanel(); t.ColumnCount = 2; t.AutoSize = true; t.AutoSizeMode = AutoSizeMode.GrowAndShrink; t.Margin = new Padding(0, 0, 0, UI.S(4));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(156))); t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        int row = 0;
        foreach (string g in gruppen)
        {
            Label lg = Lbl(g, UI.SchriftText, true, UI.Text); lg.Margin = new Padding(0, UI.S(4), UI.S(8), 0);
            FlowLayoutPanel z = new FlowLayoutPanel(); z.AutoSize = true; z.AutoSizeMode = AutoSizeMode.GrowAndShrink; z.WrapContents = true; z.Margin = new Padding(0, 0, 0, UI.S(2));
            for (int i = 0; i < pakete.Count; i++)
            {
                if (pakete[i][0] != g) continue;
                CheckBox cb = Chk(pakete[i][2], false);
                cb.AutoSize = false; cb.Size = new Size(UI.S(154), UI.S(24)); cb.Margin = new Padding(0, 0, UI.S(4), 0); cb.AutoEllipsis = true;
                Tip(cb, PaketTip(pakete[i][2], pakete[i][3], pakete[i][1], false));
                allWgIds[i] = pakete[i][1]; allWgNames[i] = pakete[i][2]; allWgBoxes[i] = cb;
                paketBoxen[pakete[i][1]] = cb; paketInfo[pakete[i][1]] = new string[] { pakete[i][2], pakete[i][3] };
                z.Controls.Add(cb);
            }
            paketZeilen.Add(z);
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(lg, 0, row); t.Controls.Add(z, 1, row); row++;
        }
        f.Controls.Add(t);

        FlowLayoutPanel rowWgActions = Row(); rowWgActions.Margin = new Padding(0, UI.S(6), 0, 0);
        string wingetPath;
        bool hasWinget = IsWingetAvailable(out wingetPath);
        Button btnInstallWg = UI.Primary("Ausgewählte installieren");
        btnInstallWg.Margin = new Padding(0); btnInstallWg.Font = new Font("Segoe UI Semibold", UI.SchriftText); btnInstallWg.Padding = new Padding(UI.S(14), UI.S(3), UI.S(14), UI.S(3));
        LinkLabel lnkWgNone = new LinkLabel(); lnkWgNone.Text = "Auswahl leeren"; lnkWgNone.AutoSize = true;
        lnkWgNone.Margin = new Padding(UI.S(16), UI.S(7), UI.S(8), 0); lnkWgNone.LinkColor = UI.Accent;
        Tip(lnkWgNone, "Alle gewählten Softwarepakete abwählen.");
        Label lblWgStatus = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblWgStatus.Margin = new Padding(UI.S(12), UI.S(7), 0, 0);
        lblPaketZustand = Lbl("", UI.SchriftHinweis, false, UI.Muted); lblPaketZustand.Margin = new Padding(0, UI.S(6), 0, 0);
        EventHandler wgCount = delegate {
            int n = AuswahlPakete(allWgBoxes).Count;
            SetText(btnInstallWg, n > 0 ? "Ausgewählte installieren (" + n + ")" : "Ausgewählte installieren");
        };
        foreach (CheckBox cb in allWgBoxes) cb.CheckedChanged += wgCount;
        lnkWgNone.LinkClicked += delegate { foreach (CheckBox cb in allWgBoxes) if (cb.AutoCheck) cb.Checked = false; };
        if (!hasWinget)
        {
            btnInstallWg.Enabled = false;
            lblWgStatus.Text = "winget ist auf diesem System nicht installiert."; lblWgStatus.ForeColor = UI.Warn;
            Tip(btnInstallWg, "winget ist auf diesem System nicht installiert.");
        }
        else
        {
            Tip(btnInstallWg, "Installiert die markierten Programme über winget im Hintergrund. Jede Installation steht danach auf der Seite Änderungen und lässt sich dort wieder entfernen; schon vorhandene Programme bleiben unberührt.");
            btnInstallWg.Click += delegate {
                List<int> selIndices = AuswahlPakete(allWgBoxes);
                if (selIndices.Count == 0)
                {
                    MessageBox.Show(this, "Bitte mindestens ein Programm zur Installation auswählen.", "Softwarepakete installieren", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                InstallWingetPackagesAsync(selIndices, allWgIds, allWgNames, lblWgStatus, btnInstallWg, allWgBoxes, lnkWgNone);
            };
        }
        rowWgActions.Controls.Add(btnInstallWg); rowWgActions.Controls.Add(lnkWgNone); rowWgActions.Controls.Add(lblWgStatus);
        f.Controls.Add(rowWgActions);
        f.Controls.Add(lblPaketZustand);
    }

    // Zeilen G|Gruppe und P|Gruppe|Id|Name|Beschreibung lesen; Pakete einer nicht genannten Gruppe bekommen eine eigene
    // Gruppe am Ende (nichts geht verloren)
    public static void LiesPaketListe(string[] lines, List<string> gruppen, List<string[]> pakete)
    {
        foreach (string l in lines ?? new string[0])
        {
            string[] x = (l ?? "").Split('|');
            if (x.Length >= 2 && x[0] == "G" && x[1].Length > 0 && !gruppen.Contains(x[1])) gruppen.Add(x[1]);
            else if (x.Length >= 5 && x[0] == "P" && x[2].Length > 0) pakete.Add(new string[] { x[1], x[2], x[3].Length > 0 ? x[3] : x[2], x[4] });
        }
        foreach (string[] p in pakete) if (!gruppen.Contains(p[0])) gruppen.Add(p[0]);
    }

    // Gewählte Pakete ohne schon installierte (die sind angehakt, aber gesperrt)
    public static List<int> AuswahlPakete(CheckBox[] boxen)
    {
        List<int> r = new List<int>();
        for (int i = 0; i < boxen.Length; i++) if (boxen[i] != null && boxen[i].Checked && boxen[i].AutoCheck) r.Add(i);
        return r;
    }

    static string PaketTip(string name, string beschreibung, string id, bool installiert)
    {
        return (installiert ? "Ist auf diesem PC schon installiert (laut winget). " : "") + name + ": " + beschreibung + ". winget-Paket " + id + ".";
    }

    // Paket als installiert kennzeichnen: Haken gesetzt und gesperrt, Schrift grün (nicht ausgegraut)
    public static void MarkierePaket(CheckBox cb, bool installiert)
    {
        if (cb == null) return;
        if (installiert) { cb.AutoCheck = false; cb.Checked = true; cb.Tag = "installiert"; cb.ForeColor = UI.Ok; cb.Cursor = Cursors.Default; }
        else if ((cb.Tag as string) == "installiert") { cb.AutoCheck = true; cb.Checked = false; cb.Tag = null; cb.ForeColor = UI.Text; cb.Cursor = Cursors.Default; }
    }

    // Installierte Pakete im Hintergrund erkennen (-SoftwareZustand, winget export), mit niedriger Priorität wie die
    // Zustandsprüfung der Optimierung. Startet beim ersten Öffnen der Seite Tools, nach "Neu scannen" und nach Installationen.
    Process paketProc;
    bool paketGeprueft;
    void StartSoftwareStateCheck()
    {
        if (paketProc != null || running || paketBoxen.Count == 0 || String.IsNullOrEmpty(psExe) || String.IsNullOrEmpty(script)) return;
        string wp; if (!IsWingetAvailable(out wp)) return;
        paketGeprueft = true;
        List<string> got = new List<string>();
        ProcessStartInfo psi = new ProcessStartInfo(psExe, "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\" -EventMode -SoftwareZustand" + (dataDir.Length > 0 ? " -DatenDir \"" + dataDir.TrimEnd('\\') + "\"" : ""));
        psi.UseShellExecute = false; psi.CreateNoWindow = true; psi.RedirectStandardOutput = true; psi.StandardOutputEncoding = new UTF8Encoding(false);
        Process p = new Process(); p.StartInfo = psi;
        bool fertig = false;
        MethodInvoker abschluss = delegate {
            if (fertig) return; fertig = true;
            List<string> lines; lock (got) lines = new List<string>(got);
            try { BeginInvoke(new MethodInvoker(delegate { ApplySoftwareState(lines); try { p.Dispose(); } catch { } paketProc = null; })); } catch { }
        };
        p.OutputDataReceived += delegate(object s, DataReceivedEventArgs e) {
            if (e.Data == null) { abschluss(); return; }
            if (e.Data.StartsWith("@@PAKZ|") || e.Data.StartsWith("@@RESULT|")) lock (got) got.Add(e.Data);
        };
        try { p.Start(); p.BeginOutputReadLine(); try { p.PriorityClass = ProcessPriorityClass.BelowNormal; } catch { } }
        catch (Exception ex) { SetText(lblPaketZustand, "Installierte Programme nicht lesbar: " + ex.Message); try { p.Dispose(); } catch { } return; }
        paketProc = p;
        SetText(lblPaketZustand, "Installierte Programme werden im Hintergrund erkannt …");
        System.Windows.Forms.Timer wd = new System.Windows.Forms.Timer(); wd.Interval = 300000;
        wd.Tick += delegate { wd.Stop(); wd.Dispose(); if (!fertig) { try { if (!p.HasExited) KillProc(p); } catch { } abschluss(); } };
        wd.Start();
    }

    void StopSoftwareStateCheck()
    {
        Process p = paketProc;
        if (p == null) return;
        try { if (!p.HasExited) KillProc(p); } catch { }
    }

    void ApplySoftwareState(List<string> lines)
    {
        int n = 0, gelesen = 0;
        foreach (string l in lines)
        {
            string id; bool inst;
            if (!ParsePaketZustand(l, out id, out inst)) continue;
            gelesen++;
            CheckBox cb; if (!paketBoxen.TryGetValue(id, out cb)) continue;
            MarkierePaket(cb, inst);
            if (inst) n++;
            string[] info; if (paketInfo.TryGetValue(id, out info)) Tip(cb, PaketTip(info[0], info[1], id, inst));
        }
        SetText(lblPaketZustand, gelesen == 0 ? "Installierte Programme nicht erkannt (winget export ohne Ergebnis)." : n == 0 ? "Keines der Programme ist auf diesem PC installiert." : n + (n == 1 ? " Programm ist" : " Programme sind") + " schon installiert (grün, Haken gesperrt).");
    }

    // @@PAKZ|Id|installiert oder nicht installiert
    public static bool ParsePaketZustand(string line, out string id, out bool installiert)
    {
        id = ""; installiert = false;
        if (line == null || !line.StartsWith("@@PAKZ|")) return false;
        string[] x = line.Split('|');
        if (x.Length < 3 || x[1].Length == 0) return false;
        id = x[1]; installiert = x[2] == "installiert";
        return true;
    }

    // Zeile eines portablen Werkzeugs: Zustand als Etikett, Hauptaktion Starten (gefunden) oder Herunterladen (Herstellerseite),
    // im Menü "…" Ordner öffnen und Herstellerseite
    ToolRow ToolZeile(string title, string desc, string icon, string subDir, string[] exeNames, string[] regNames, string[] direct, string downloadUrl, string zusatz)
    {
        ToolRow c = new ToolRow(title, desc, icon);
        string path = FindToolPath(subDir, exeNames, regNames, direct);
        Button bMain;
        string u = downloadUrl, sd = subDir;
        ContextMenu cm = new ContextMenu();
        if (!String.IsNullOrEmpty(path))
        {
            bool portable = path.IndexOf("Minibench-Daten", StringComparison.OrdinalIgnoreCase) >= 0;
            c.StatusText = portable ? "bereit, portabel" : "installiert"; c.StatusLevel = "ok";
            bMain = UI.Primary("Starten");
            bMain.Image = UI.GlyphImage(UI.IcoAdmin, UI.OnAccent, UI.S(18)); bMain.Tag = UI.IcoAdmin; bMain.ImageAlign = ContentAlignment.MiddleLeft; bMain.TextImageRelation = TextImageRelation.ImageBeforeText;
            Tip(bMain, "Startet " + title + " mit Administratorrechten (" + path + ").");
            string p = path; bMain.Click += delegate { StartAdminProcess(p, ""); };
            cm.MenuItems.Add(new MenuItem("Speicherort öffnen", delegate { OpenSelect(p); }));
        }
        else
        {
            c.StatusText = "nicht gefunden"; c.StatusLevel = "warn";
            bMain = UI.Secondary("Herunterladen");
            Tip(bMain, "Öffnet die Herstellerseite von " + title + " im Standardbrowser. Eine portable Version gehört in Minibench-Daten\\Tools\\" + subDir + ".");
            bMain.Click += delegate { OpenShell(u); };
            cm.MenuItems.Add(new MenuItem("Ordner Tools\\" + subDir + " öffnen", delegate { string d = Path.Combine(dataDir, "Tools\\" + sd); try { Directory.CreateDirectory(d); } catch { } OpenShell(d); }));
        }
        cm.MenuItems.Add(new MenuItem("Herstellerseite öffnen", delegate { OpenShell(u); }));
        bMain.Font = new Font("Segoe UI Semibold", UI.SchriftText); bMain.Padding = new Padding(UI.S(8), UI.S(2), UI.S(8), UI.S(2));
        bMain.MinimumSize = new Size(UI.S(126), 0); bMain.Margin = new Padding(0);
        Button bMore = UI.Secondary("…"); bMore.AutoSize = false; bMore.Size = new Size(UI.S(32), bMain.PreferredSize.Height); bMore.Padding = new Padding(0); bMore.Margin = new Padding(0);
        bMore.Click += delegate { cm.Show(bMore, new Point(0, bMore.Height)); };
        Tip(bMore, "Weitere Aktionen: " + (String.IsNullOrEmpty(path) ? "Ordner für die portable Version öffnen" : "Speicherort öffnen") + ", Herstellerseite öffnen.");
        c.MainButton = bMain; c.MoreButton = bMore;
        Tip(c, title + ": " + desc + (String.IsNullOrEmpty(path) ? " Erwartet in Minibench-Daten\\Tools\\" + subDir + " oder als Installation." : " Gefunden: " + path) + (zusatz.Length > 0 ? " " + zusatz : ""));
        return c;
    }

    // Zeile eines Windows-Schnellstarters mit einem Knopf "Öffnen"
    ToolRow LauncherRow(string title, string desc, string icon, string exe, string args)
    {
        ToolRow c = new ToolRow(title, desc, icon);
        Button b = UI.Secondary("Öffnen"); b.MinimumSize = new Size(UI.S(110), 0); b.Margin = new Padding(0);
        Tip(b, "Öffnet " + title + " (" + exe + (args.Length > 0 ? " " + args : "") + ").");
        string e = exe, a = args; b.Click += delegate { StartAdminProcess(e, a); };
        c.MainButton = b;
        Tip(c, title + ": " + desc);
        return c;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetFirmwareType(out int firmwareType);

    public static bool IsUefiFirmware()
    {
        try
        {
            int fw = 0;
            if (GetFirmwareType(out fw))
            {
                if (fw == 2) return true;
                if (fw == 1) return false;
            }
        }
        catch { }
        try
        {
            using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State"))
            {
                if (rk != null) return true;
            }
        }
        catch { }
        try
        {
            using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control"))
            {
                if (rk != null)
                {
                    object v = rk.GetValue("PEFirmwareType");
                    if (v != null && Convert.ToInt32(v) == 2) return true;
                }
            }
        }
        catch { }
        return false;
    }

    static void StartAdminProcess(string path, string args)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = path;
            if (!String.IsNullOrEmpty(args)) psi.Arguments = args;
            psi.UseShellExecute = true;
            psi.Verb = "runas";
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Das Programm konnte nicht gestartet werden:\r\n\r\n" + ex.Message, "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // Programm in einem Ordner nach Namen suchen (Reihenfolge = Vorrang). Ab v4.05 auch mit Platzhaltern (GPU-Z*.exe):
    // dann gilt die Datei mit der höchsten Versionsnummer im Namen, bei gleicher Nummer die jüngste.
    public static string SucheProgramm(string dir, string[] exeNames)
    {
        if (exeNames == null || String.IsNullOrEmpty(dir)) return null;
        foreach (string en in exeNames)
        {
            try
            {
                if (en.IndexOf('*') >= 0 || en.IndexOf('?') >= 0)
                {
                    string n = NeuesteDatei(Directory.GetFiles(dir, en));
                    if (n != null) return n;
                }
                else
                {
                    string p = Path.Combine(dir, en);
                    if (File.Exists(p)) return p;
                }
            }
            catch { }
        }
        return null;
    }

    // Höchste Versionsnummer im Dateinamen (GPU-Z.2.71.0.exe vor GPU-Z.2.9.0.exe), Dateien ohne Nummer zuletzt
    public static string NeuesteDatei(string[] files)
    {
        if (files == null || files.Length == 0) return null;
        string best = null; long[] bestV = null; DateTime bestT = DateTime.MinValue;
        foreach (string f in files)
        {
            long[] v = VersionImNamen(Path.GetFileNameWithoutExtension(f));
            DateTime t = DateTime.MinValue; try { t = File.GetLastWriteTimeUtc(f); } catch { }
            int c = best == null ? 1 : VergleicheVersion(v, bestV);
            if (c > 0 || (c == 0 && t > bestT)) { best = f; bestV = v; bestT = t; }
        }
        return best;
    }

    static long[] VersionImNamen(string name)
    {
        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(name ?? "", @"\d+(\.\d+)+|\d+");
        if (!m.Success) return new long[0];
        string[] t = m.Value.Split('.');
        long[] r = new long[t.Length];
        for (int i = 0; i < t.Length; i++) long.TryParse(t[i], out r[i]);
        return r;
    }

    static int VergleicheVersion(long[] a, long[] b)
    {
        int n = Math.Max(a.Length, b.Length);
        for (int i = 0; i < n; i++)
        {
            long x = i < a.Length ? a[i] : -1, y = i < b.Length ? b[i] : -1;
            if (x != y) return x > y ? 1 : -1;
        }
        return 0;
    }

    public string FindToolPath(string subDir, string[] exeNames, string[] registryNames, string[] directPaths)
    {
        // 1. Portable im Minibench-Daten Ordner suchen
        if (!String.IsNullOrEmpty(dataDir))
        {
            string tdir = Path.Combine(dataDir, "Tools\\" + subDir);
            if (Directory.Exists(tdir))
            {
                string pt = SucheProgramm(tdir, exeNames);
                if (pt != null) return pt;
                try
                {
                    string[] files = Directory.GetFiles(tdir, "*.exe");
                    if (files.Length > 0) return files[0];
                }
                catch { }
            }
        }

        // 2. Bekannte Installationspfade direkt prüfen
        if (directPaths != null)
        {
            foreach (string dp in directPaths)
            {
                try { if (File.Exists(dp)) return dp; } catch { }
            }
        }

        // 3. Registry Uninstall-Schlüssel durchsuchen
        if (registryNames != null && registryNames.Length > 0)
        {
            string[] ukeys = new string[] {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };
            Microsoft.Win32.RegistryKey[] roots = new Microsoft.Win32.RegistryKey[] {
                Microsoft.Win32.Registry.LocalMachine,
                Microsoft.Win32.Registry.CurrentUser
            };
            foreach (Microsoft.Win32.RegistryKey root in roots)
            {
                foreach (string uk in ukeys)
                {
                    try
                    {
                        using (Microsoft.Win32.RegistryKey key = root.OpenSubKey(uk))
                        {
                            if (key == null) continue;
                            foreach (string subName in key.GetSubKeyNames())
                            {
                                try
                                {
                                    using (Microsoft.Win32.RegistryKey sub = key.OpenSubKey(subName))
                                    {
                                        if (sub == null) continue;
                                        string disp = Convert.ToString(sub.GetValue("DisplayName"));
                                        if (String.IsNullOrEmpty(disp)) continue;
                                        foreach (string rn in registryNames)
                                        {
                                            if (disp.IndexOf(rn, StringComparison.OrdinalIgnoreCase) >= 0)
                                            {
                                                string icon = Convert.ToString(sub.GetValue("DisplayIcon"));
                                                if (!String.IsNullOrEmpty(icon))
                                                {
                                                    string clean = icon.Trim('"', ' ');
                                                    if (clean.IndexOf(',') > 0) clean = clean.Substring(0, clean.IndexOf(',')).Trim();
                                                    if (File.Exists(clean) && clean.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) return clean;
                                                }
                                                string loc = Convert.ToString(sub.GetValue("InstallLocation"));
                                                if (!String.IsNullOrEmpty(loc))
                                                {
                                                    string cleanLoc = loc.Trim('"', ' ');
                                                    if (Directory.Exists(cleanLoc))
                                                    {
                                                        string pl = SucheProgramm(cleanLoc, exeNames);
                                                        if (pl != null) return pl;
                                                        string[] ef = Directory.GetFiles(cleanLoc, "*.exe");
                                                        if (ef.Length > 0) return ef[0];
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }
            }
        }
        return null;
    }

    // Softwarepakete über winget (ab 3.53): Installation im Arbeitsprozess (-SoftwareInstallieren), dort mit Frist je Paket,
    // vollständig gelesener Ausgabe und Eintrag im Änderungsprotokoll (Risikostufe Eingriff, Gegenbefehl winget uninstall).
    // Vorher lief winget direkt aus der Oberfläche: Ausgabe umgeleitet, aber nie gelesen, WaitForExit ohne Frist (Hänger),
    // und nichts stand im Änderungsprotokoll.
    public static string WingetSpec(List<int> selIndices, string[] ids, string[] names)
    {
        List<string> parts = new List<string>();
        foreach (int i in selIndices)
        {
            if (i < 0 || i >= ids.Length) continue;
            string n = (i < names.Length ? names[i] : ids[i]).Replace(";", ",").Replace("=", "-").Replace("\"", "'");
            parts.Add(ids[i] + "=" + n);
        }
        return String.Join(";", parts.ToArray());
    }

    // Frist für den ganzen Hilfsprozess: je Paket die Fristen des Arbeitsprozesses (Kern\Softwarepakete.ps1: list 120 s vorher,
    // install 1200 s, list 120 s nachher) plus Reserve, dazu der Start von PowerShell
    public const int WingetFristPaketSek = 120 + 1200 + 120 + 60;
    public static int WingetTimeoutSec(int count) { return Math.Max(1, count) * WingetFristPaketSek + 120; }

    // @@PAKET|Nr|Anzahl|Id|Status|Text -> Status und Text; false, wenn die Zeile kein Paketereignis ist
    public static bool ParseWingetEvent(string line, out string status, out string text)
    {
        status = ""; text = "";
        if (line == null || !line.StartsWith("@@PAKET|")) return false;
        string[] x = line.Split(new char[] { '|' }, 6);
        if (x.Length < 6) return false;
        status = x[4]; text = x[5].Replace("¦", "|");
        return true;
    }

    public void InstallWingetPackagesAsync(List<int> selIndices, string[] allWgIds, string[] allWgNames, Label lblWgStatus, Button btnInstallWg, CheckBox[] allWgBoxes, LinkLabel lnkWgNone)
    {
        StringBuilder m = new StringBuilder();
        m.AppendLine("Diese Programme werden über winget installiert (Risikostufe Eingriff):");
        m.AppendLine();
        foreach (int i in selIndices) m.AppendLine("·  " + allWgNames[i] + " (" + allWgIds[i] + ")");
        m.AppendLine();
        m.AppendLine("Jede Installation steht danach auf der Seite Änderungen und lässt sich dort wieder entfernen. Schon vorhandene Programme bleiben unberührt. Je Programm gilt eine Frist von 20 Minuten.");
        if (MessageBox.Show(this, m.ToString(), "Softwarepakete installieren", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        btnInstallWg.Enabled = false;
        foreach (CheckBox cb in allWgBoxes) cb.Enabled = false;
        lnkWgNone.Enabled = false;
        StopSoftwareStateCheck();
        lblWgStatus.Text = "winget wird gestartet ..."; lblWgStatus.ForeColor = UI.Accent;

        string spec = WingetSpec(selIndices, allWgIds, allWgNames);
        int timeoutSec = WingetTimeoutSec(selIndices.Count);
        string args = "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\" -EventMode -SoftwareInstallieren \"" + spec + "\"" + (dataDir.Length > 0 ? " -DatenDir \"" + dataDir.TrimEnd('\\') + "\"" : "");

        System.Threading.Thread t = new System.Threading.Thread(delegate() {
            List<string> texte = new List<string>();
            string result = "";
            bool eof = false;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(psExe, args);
                psi.UseShellExecute = false; psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true; psi.StandardOutputEncoding = new UTF8Encoding(false);
                using (Process p = new Process())
                {
                    p.StartInfo = psi;
                    p.OutputDataReceived += delegate(object s, DataReceivedEventArgs e) {
                        string st, tx;
                        if (e.Data == null) { eof = true; return; }
                        if (e.Data.StartsWith("@@RESULT|")) { result = e.Data.Substring(9); return; }
                        if (!ParseWingetEvent(e.Data, out st, out tx)) return;
                        if (st != "läuft") lock (texte) texte.Add(tx);
                        Color c = st == "läuft" ? UI.Accent : (st == "installiert" || st == "bereits installiert") ? UI.Ok : UI.Warn;
                        try { BeginInvoke(new MethodInvoker(delegate { lblWgStatus.Text = tx; lblWgStatus.ForeColor = c; })); } catch { }
                    };
                    p.Start(); p.BeginOutputReadLine();
                    if (!p.WaitForExit(timeoutSec * 1000))
                    {
                        KillProc(p);
                        lock (texte) texte.Add("Abgebrochen: keine Rückmeldung nach " + (timeoutSec / 60) + " Minuten.");
                    }
                    // letzte Ausgabezeilen abholen, höchstens 5 s (hält ein Kindprozess die Ausgabe offen, nicht ewig warten)
                    DateTime eu = DateTime.Now.AddSeconds(5);
                    while (!eof && DateTime.Now < eu) System.Threading.Thread.Sleep(20);
                }
            }
            catch (Exception ex) { lock (texte) texte.Add("Arbeitsprozess nicht gestartet: " + ex.Message); }

            try
            {
                BeginInvoke(new MethodInvoker(delegate {
                    btnInstallWg.Enabled = true;
                    foreach (CheckBox cb in allWgBoxes) { cb.Enabled = true; if ((cb.Tag as string) == "installiert") cb.ForeColor = UI.Ok; }
                    lnkWgNone.Enabled = true;
                    string[] r = result.Split('|');
                    int ok = 0, da = 0, bad = 0;
                    if (r.Length >= 3) { int.TryParse(r[0], out ok); int.TryParse(r[1], out da); int.TryParse(r[2], out bad); }
                    else bad = selIndices.Count;
                    lblWgStatus.Text = String.Format("Abgeschlossen: {0} installiert, {1} schon vorhanden, {2} fehlgeschlagen.", ok, da, bad);
                    lblWgStatus.ForeColor = bad == 0 ? UI.Ok : UI.Warn;
                    ReloadDb();
                    StartSoftwareStateCheck();
                    if (bad > 0)
                    {
                        string all; lock (texte) all = String.Join("\r\n", texte.ToArray());
                        MessageBox.Show(this, all.Length > 0 ? all : "Keine Rückmeldung vom Arbeitsprozess.", "Softwarepakete installieren", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }));
            }
            catch { }
        });
        t.IsBackground = true;
        t.Start();
    }

    public static bool IsWingetAvailable(out string wingetPath)
    {
        wingetPath = "winget.exe";
        try
        {
            string appDataWinget = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\WindowsApps\winget.exe");
            if (File.Exists(appDataWinget)) { wingetPath = appDataWinget; return true; }
            ProcessStartInfo psi = new ProcessStartInfo("where.exe", "winget");
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            using (Process p = Process.Start(psi))
            {
                string o = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode == 0 && !String.IsNullOrEmpty(o))
                {
                    string first = o.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                    if (File.Exists(first)) { wingetPath = first; return true; }
                    return true;
                }
            }
        }
        catch { }
        return false;
    }
}
