// Vergleichsdatenbank und Systemvergleich (Teilklasse DiagGui)
public partial class DiagGui
{
    // ab v4.10: Zeile der Datenbankliste. Gruppe = Zeile eines Geräts (steht für dessen neuesten Lauf), sonst ein
    // älterer Lauf darunter. Bis v4.09 ein natives Listenfeld mit eigenen Gruppenköpfen (DbListView).
    public class DbZeile : ListenZeile
    {
        public DbEntry E; public string Key = "", Name = ""; public bool Gruppe; public int Anzahl = 1;
    }

    internal ListenSeite dbSeite;
    internal DetailKarte dbDetail;
    internal HashSet<string> dbHaken = new HashSet<string>();
    internal LinkLabel lnkDbName, lnkDbRef, lnkDbBasis, lnkDbExport, lnkDbDelete;
    Button btnDbFolder;
    Label lblDbHint;
    FlowLayoutPanel dbFuss;

    static string DbKey(DbEntry e) { return e.IsReference ? "REF" : (String.IsNullOrEmpty(e.GeraetKey) ? "PC:" + (e.Computer ?? "").ToUpperInvariant() : e.GeraetKey); }

    // markierte Läufe (Zeilen der Liste), in der Reihenfolge der Liste
    internal List<DbEntry> DbMarkierte()
    {
        List<DbEntry> l = new List<DbEntry>();
        if (lvDb != null) foreach (ListenZeile z in lvDb.Zeilen) if (lvDb.Markiert.Contains(z)) { DbZeile d = z as DbZeile; if (d != null && d.E != null) l.Add(d.E); }
        return l;
    }
    DbZeile DbMarkierteZeile() { if (lvDb == null) return null; List<ListenZeile> m = lvDb.Markiert; return m.Count == 1 ? m[0] as DbZeile : null; }

    HashSet<string> expandedGroups = new HashSet<string>();

    List<string> GetExpandableKeys()
    {
        List<string> keys = new List<string>();
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (DbEntry e in db)
        {
            string k = e.IsReference ? "REF" : (String.IsNullOrEmpty(e.GeraetKey) ? "PC:" + (e.Computer ?? "").ToUpperInvariant() : e.GeraetKey);
            if (!counts.ContainsKey(k)) counts[k] = 0;
            counts[k]++;
        }
        foreach (KeyValuePair<string, int> kv in counts)
        {
            if (kv.Value > 1) keys.Add(kv.Key);
        }
        return keys;
    }

    void SetAllGroupsCollapsed(bool collapse)
    {
        if (collapse)
        {
            expandedGroups.Clear();
        }
        else
        {
            foreach (string k in GetExpandableKeys())
            {
                expandedGroups.Add(k);
            }
        }
        FillDbList();
    }

    void ToggleAllGroups()
    {
        List<string> expandable = GetExpandableKeys();
        bool anyCollapsed = (expandedGroups.Count < expandable.Count);
        SetAllGroupsCollapsed(!anyCollapsed);
    }

    void ToggleGroup(string k)
    {
        if (expandedGroups.Contains(k))
            expandedGroups.Remove(k);
        else
            expandedGroups.Add(k);
        FillDbList();
    }

    void UpdateToggleGroupsButton()
    {
        if (btnToggleGroups == null) return;
        List<string> expandable = GetExpandableKeys();
        if (expandable.Count == 0)
        {
            btnToggleGroups.Enabled = false;
            btnToggleGroups.Text = "Alle aufklappen";
            Tip(btnToggleGroups, "Keine Gerätegruppen mit mehreren Läufen vorhanden.");
            return;
        }
        btnToggleGroups.Enabled = true;
        bool anyCollapsed = (expandedGroups.Count < expandable.Count);
        btnToggleGroups.Text = anyCollapsed ? "Alle aufklappen" : "Alle einklappen";
        Tip(btnToggleGroups, anyCollapsed ? "Klappt alle Gerätegruppen in der Datenbank auf." : "Klappt alle Gerätegruppen in der Datenbank ein.");
    }

    int dbSortCol = -1;
    bool dbSortAsc = true;
    Button btnRename;
    internal Button btnCompare, btnDashboard, btnVerlauf, btnOpenReport, btnExport, btnSetRef, btnToggleGroups;

    // Spalte: 0 System, 1 Datum, 2 Punkte, 3 CPU, 4 GPU, 5 RAM, 6 CPU multi, 7 RAM lesen, 8 Grafiktest, 9 Befunde
    internal void OnDbColumnClick(int spalte)
    {
        if (spalte == dbSortCol)
        {
            dbSortAsc = !dbSortAsc;
        }
        else
        {
            dbSortCol = spalte;
            dbSortAsc = (spalte == 0 || spalte == 3 || spalte == 4 || spalte == 5);
        }
        FillDbList();
    }

    string PromptInput(string prompt, string title, string defaultValue)
    {
        Form dlg = new Form();
        dlg.Text = title;
        dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
        dlg.MaximizeBox = false; dlg.MinimizeBox = false;
        dlg.StartPosition = FormStartPosition.CenterParent;
        dlg.ClientSize = new Size(UI.S(440), UI.S(145));
        dlg.BackColor = UI.Bg; dlg.Font = new Font("Segoe UI", 9f);

        Label lbl = new Label();
        lbl.Text = prompt;
        lbl.Location = new Point(UI.S(16), UI.S(14));
        lbl.Size = new Size(UI.S(408), UI.S(30));

        TextBox tb = new TextBox();
        tb.Text = defaultValue ?? "";
        tb.Location = new Point(UI.S(16), UI.S(48));
        tb.Size = new Size(UI.S(408), UI.S(24));
        tb.BackColor = UI.Panel;
        tb.ForeColor = UI.Text;
        tb.BorderStyle = BorderStyle.FixedSingle;

        Button btnOk = UI.Primary("Speichern");
        btnOk.Location = new Point(UI.S(226), UI.S(92));
        btnOk.Size = new Size(UI.S(95), UI.S(32));
        btnOk.DialogResult = DialogResult.OK;

        Button btnCancel = UI.Secondary("Abbrechen");
        btnCancel.Location = new Point(UI.S(328), UI.S(92));
        btnCancel.Size = new Size(UI.S(96), UI.S(32));
        btnCancel.DialogResult = DialogResult.Cancel;

        dlg.Controls.Add(lbl); dlg.Controls.Add(tb); dlg.Controls.Add(btnOk); dlg.Controls.Add(btnCancel);
        dlg.AcceptButton = btnOk; dlg.CancelButton = btnCancel;
        UI.ThemeDialog(dlg);

        return dlg.ShowDialog(this) == DialogResult.OK ? tb.Text : null;
    }

    void RenameSelectedEntry()
    {
        DbEntry target = null;
        DbZeile lvi = DbMarkierteZeile();
        if (lvi != null) target = lvi.E;
        else if (CheckedEntries().Count == 1) { target = CheckedEntries()[0]; foreach (ListenZeile z in lvDb.Zeilen) { DbZeile d = z as DbZeile; if (d != null && d.E == target) { lvi = d; break; } } }
        if (target == null || target.IsReference) return;

        // Zeile eines Geräts mit mehreren Läufen: Der Name gilt für alle Läufe des Geräts
        bool isMasterGroup = lvi != null && lvi.Gruppe && lvi.Anzahl > 1;
        List<string> pathsToRename = new List<string>();
        string currentName = "";

        if (isMasterGroup)
        {
            string devKey = !String.IsNullOrEmpty(target.GeraetKey) ? target.GeraetKey : "PC:" + (target.Computer ?? "").ToUpperInvariant();
            foreach (DbEntry e in db)
            {
                if (e.IsReference) continue;
                string ek = !String.IsNullOrEmpty(e.GeraetKey) ? e.GeraetKey : "PC:" + (e.Computer ?? "").ToUpperInvariant();
                if (ek == devKey)
                {
                    if (!String.IsNullOrEmpty(e.Path) && !pathsToRename.Contains(e.Path)) pathsToRename.Add(e.Path);
                    if (String.IsNullOrEmpty(currentName) && !String.IsNullOrEmpty(e.Name)) currentName = e.Name;
                }
            }
        }

        if (pathsToRename.Count == 0 && !String.IsNullOrEmpty(target.Path))
        {
            pathsToRename.Add(target.Path);
        }
        if (String.IsNullOrEmpty(currentName)) currentName = target.DisplayName;

        string promptText = (isMasterGroup && pathsToRename.Count > 1)
            ? "Anzeigename für dieses Gerät (" + pathsToRename.Count + " Läufe) eingeben:\r\nRechnername bleibt: " + target.Computer
            : "Anzeigename für dieses System eingeben (z. B. Vor Reinigung oder Neuer Treiber):\r\nRechnername bleibt: " + target.Computer;

        string newName = PromptInput(promptText, "Systemnamen bearbeiten", currentName);
        if (newName == null) return;
        newName = newName.Trim();
        if (newName.Length == 0 || newName == currentName) return;

        // ab 3.54: Datenbankdatei und Berichtsordner bekommen den neuen Namen (Name_Datum_Zeit), über den Arbeitsprozess
        // ein führendes - hielte powershell.exe -File für einen Parameternamen
        newName = newName.Replace("\"", "'").Replace("\r", " ").Replace("\n", " ").TrimEnd('\\').Trim().TrimStart('-', ' ');
        if (newName.Length == 0) return;
        List<string> lines;
        string res = RunHelperPumped("-Umbenennen " + Q(String.Join("|", pathsToRename.ToArray())) + " -NeuerName " + Q(newName), out lines, 120);
        ReloadDb();
        if (!res.StartsWith("1")) UI.Hinweis("Der Name konnte nicht geändert werden.\r\n\r\n" + String.Join("\r\n", lines.ToArray()), "Systemnamen bearbeiten", this);
    }

    Control BuildDbPage()
    {
        dbSeite = new ListenSeite("Datenbank", "Alle gespeicherten Läufe, nach Gerät gebündelt. Zum Vergleichen Läufe anhaken und \"Im Dashboard vergleichen\" wählen. Doppelklick öffnet den Bericht, ein Klick auf einen Spaltenkopf sortiert.", "Im Dashboard vergleichen");
        listenSeiten.Add(dbSeite);
        btnDashboard = dbSeite.BandKnopf;
        btnDashboard.Click += delegate { OpenDashboard(); };
        btnDashboard.Enabled = false;
        Tip(btnDashboard, "Öffnet das interaktive HTML5-Dashboard für detaillierten System- und Benchmark-Vergleich der ausgewählten Läufe.");
        dbSeite.BandVerweis.LinkClicked += delegate { dbHaken.Clear(); FillDbList(); };
        Tip(dbSeite.BandVerweis, "Entfernt alle Haken.");

        lblDbHint = Lbl("Graue Einträge enthalten keine Benchmark-Werte.", UI.SchriftHinweis, false, UI.Muted); lblDbHint.Margin = new Padding(0, UI.S(6), 0, 0);
        dbSeite.Links.Controls.Add(lblDbHint);

        Button imp = UI.Secondary("Importieren …"); imp.Margin = new Padding(0); imp.Click += delegate { ImportRuns(); };
        Tip(imp, "Importiert Durchläufe aus .minibench-Paketen, .zip-Archiven, .json-Dateien oder Ausgabeordnern früherer Läufe in die Datenbank samt Bericht.");
        btnToggleGroups = UI.Secondary("Alle aufklappen"); btnToggleGroups.Margin = new Padding(UI.S(8), 0, 0, 0); btnToggleGroups.Click += delegate { ToggleAllGroups(); };
        Tip(btnToggleGroups, "Klappt alle Gerätegruppen in der Datenbank ein oder aus.");
        Button rel = UI.Secondary("Aktualisieren"); rel.Margin = new Padding(UI.S(8), 0, 0, 0); rel.Click += delegate { RefreshAndSync(); };
        Tip(rel, "Liest Datenbank und Referenz neu ein. Ist ein Netzlaufwerk eingerichtet und erreichbar, werden Berichte, Datenbank und Änderungsprotokolle vorher in beide Richtungen abgeglichen.");
        btnDbClean = UI.Secondary("Aufräumen …"); btnDbClean.Margin = new Padding(UI.S(8), 0, 0, 0); btnDbClean.Click += delegate { CleanData(); }; btnDbClean.Enabled = dataDir.Length > 0;
        dbSeite.Rechts.Controls.Add(imp); dbSeite.Rechts.Controls.Add(btnToggleGroups); dbSeite.Rechts.Controls.Add(rel); dbSeite.Rechts.Controls.Add(btnDbClean);

        // Fuß: Ablage und Meldung der Datenpflege
        dbFuss = new FlowLayoutPanel(); dbFuss.FlowDirection = FlowDirection.TopDown; dbFuss.WrapContents = false; dbFuss.AutoSize = true; dbFuss.AutoSizeMode = AutoSizeMode.GrowAndShrink; dbFuss.BackColor = UI.Panel;
        lblDbPath = Lbl(NasAblage.AblageText(dataDir), UI.SchriftHinweis, false, UI.Muted); lblDbPath.Margin = new Padding(0, 0, 0, 0); dbFuss.Controls.Add(lblDbPath);
        lblDbClean = Lbl(DatenpflegeInfo, UI.SchriftHinweis, false, UI.Muted); lblDbClean.Visible = DatenpflegeInfo.Length > 0; lblDbClean.Margin = new Padding(0, UI.S(2), 0, 0); dbFuss.Controls.Add(lblDbClean);
        dbSeite.Seite.Resize += delegate { int w = Math.Max(UI.S(200), dbSeite.Seite.ClientSize.Width); lblDbPath.MaximumSize = new Size(w, 0); lblDbClean.MaximumSize = new Size(w, 0); };
        dbFuss.SizeChanged += delegate { dbSeite.Ordne(); };

        lvDb = new ZeilenListe(); lvDb.Tipp = tip; lvDb.Zeichner = ZeichneDbZeile; lvDb.Mehrfach = true; lvDb.ZeilenHoehe = UI.S(44);
        lvDb.Leer = "Noch keine Läufe gespeichert. Ein Lauf mit Benchmark legt den ersten Eintrag an; \"Importieren …\" holt frühere Läufe.";
        lvDb.Spalten.Add(new ListenSpalte("System", 0, false, 0)); lvDb.Spalten.Add(new ListenSpalte("Zuletzt", UI.S(104), false, 1));
        lvDb.Spalten.Add(new ListenSpalte("Punkte", UI.S(70), true, 2)); lvDb.Spalten.Add(new ListenSpalte("CPU multi", UI.S(80), true, 6));
        lvDb.Spalten.Add(new ListenSpalte("RAM lesen", UI.S(88), true, 7)); lvDb.Spalten.Add(new ListenSpalte("Grafik", UI.S(82), true, 8));
        lvDb.Spalten.Add(new ListenSpalte("    Befunde", UI.S(86), false, 9));
        lvDb.Spalten[6].Hinweis = "Befunde des Laufs: kritisch / Warnungen / Hinweise";
        lvDb.Spalten[5].Hinweis = "Bilder je Sekunde im Rendertest, sonst der Speicherdurchsatz der Grafikeinheit";
        lvDb.SpalteGeklickt += OnDbColumnClick;
        lvDb.HakenGeklickt += delegate(ListenZeile z) { DbZeile d = (DbZeile)z; if (d.E == null || String.IsNullOrEmpty(d.E.Path)) return; if (!dbHaken.Remove(d.E.Path)) dbHaken.Add(d.E.Path); FillDbList(); };
        lvDb.AufklappGeklickt += delegate(ListenZeile z) { ToggleGroup(((DbZeile)z).Key); };
        lvDb.Geoeffnet += delegate(ListenZeile z) { DbZeile d = (DbZeile)z; if (d.E != null) OpenEntry(d.E); };
        lvDb.MarkierungGeaendert += delegate { UpdateDbButtons(); };
        lvDb.KeyDown += delegate(object sender, KeyEventArgs e) { if (DbTaste(e.KeyData)) e.Handled = true; };
        Tip(lvDb, "Datenbank aller gespeicherten Systeme. Ein Klick auf die Spaltenköpfe sortiert nach Datum, Gesamtwertung oder Messwert; nach CPU, GPU und RAM sortiert das Kontextmenü. Tasten: Leertaste hakt die markierten Läufe an oder ab, Entf entfernt sie (mit Rückfrage), F2 benennt um.");

        ContextMenu cm = new ContextMenu();
        MenuItem miDashboard = new MenuItem("Im Dashboard vergleichen", delegate { OpenDashboard(); });
        MenuItem miHistory = new MenuItem("Geräte-Verlauf im Dashboard öffnen", delegate { OpenDeviceHistory(); });
        MenuItem miSetRef = new MenuItem("Als Referenz festlegen (persistent)", delegate { SetSelectedAsReference(); });
        MenuItem miDashboardBase = new MenuItem("Als Basis im Dashboard öffnen (temporär)", delegate { OpenDashboardWithBase(); });
        MenuItem miExpandAll = new MenuItem("Alle aufklappen", delegate { SetAllGroupsCollapsed(false); });
        MenuItem miCollapseAll = new MenuItem("Alle einklappen", delegate { SetAllGroupsCollapsed(true); });
        MenuItem miSort = new MenuItem("Sortieren nach");
        miSort.MenuItems.Add(new MenuItem("Prozessor", delegate { OnDbColumnClick(3); }));
        miSort.MenuItems.Add(new MenuItem("Grafik", delegate { OnDbColumnClick(4); }));
        miSort.MenuItems.Add(new MenuItem("Arbeitsspeicher", delegate { OnDbColumnClick(5); }));
        miSort.MenuItems.Add(new MenuItem("Datum, neueste zuerst", delegate { dbSortCol = -1; dbSortAsc = true; FillDbList(); }));
        MenuItem miRename = new MenuItem("Name ändern ...", delegate { RenameSelectedEntry(); });
        MenuItem miOpen = new MenuItem("Bericht öffnen", delegate { DbEntry s = SelectedDbEntry(); if (s != null) OpenEntry(s); });
        MenuItem miFolder = new MenuItem("Berichtsordner öffnen", delegate { DbEntry s = SelectedDbEntry(); if (s != null) OpenEntryFolder(s); });
        MenuItem miExport = new MenuItem("Lauf exportieren ...", delegate { ExportSelected(); });
        MenuItem miDelete = new MenuItem("Aus Datenbank entfernen", delegate { DeleteSelected(); });
        cm.MenuItems.AddRange(new MenuItem[] { miDashboard, miHistory, miSetRef, miDashboardBase, new MenuItem("-"), miExpandAll, miCollapseAll, miSort, new MenuItem("-"), miRename, miOpen, miFolder, miExport, new MenuItem("-"), miDelete });
        cm.Popup += delegate {
            List<string> expandable = GetExpandableKeys();
            miExpandAll.Enabled = expandable.Count > 0 && expandedGroups.Count < expandable.Count;
            miCollapseAll.Enabled = expandable.Count > 0 && expandedGroups.Count > 0;
            List<DbEntry> selL = DbMarkierte(), chkL = CheckedEntries();
            int selN = selL.Count, chkN = chkL.Count;
            miDashboard.Enabled = chkN >= 1 || selN >= 1;
            miHistory.Enabled = (GetSelectedDeviceTarget() != null);
            miSetRef.Enabled = selN == 1 || (selN == 0 && chkN == 1);
            miDashboardBase.Enabled = selN == 1 || (selN == 0 && chkN == 1);
            DbEntry selTarget = null;
            if (selN == 1) selTarget = selL[0];
            else if (chkN == 1) selTarget = chkL[0];
            miRename.Enabled = (selTarget != null && !selTarget.IsReference && (selN == 1 || (selN == 0 && chkN == 1)));
            miOpen.Enabled = selN == 1; miFolder.Enabled = selN == 1;
            miExport.Enabled = selN == 1 || (selN == 0 && chkN == 1);
            miDelete.Enabled = chkN >= 1 || selN >= 1;
            miDelete.Text = chkN > 1 ? String.Format("Angehakte Läufe entfernen ({0})", chkN) : "Aus Datenbank entfernen";
        };
        lvDb.ContextMenu = cm;

        // Detailbereich rechts: der markierte Lauf mit den Aktionen, die bis v4.09 im Menü "Aktionen" standen
        dbDetail = new DetailKarte(); dbDetail.Zeichne = ZeichneDbDetail; dbDetail.AutoScroll = true; dbDetail.Padding = new Padding(0);
        dbDetail.Scroll += delegate { dbDetail.Invalidate(); }; dbDetail.MouseWheel += delegate { dbDetail.Invalidate(); };
        dbSeite.SeitenteilBreite = 320;
        lnkDbName = KartenVerweis("Name ändern"); lnkDbName.Margin = new Padding(0); lnkDbName.LinkClicked += delegate { RenameSelectedEntry(); };
        Tip(lnkDbName, "Ändert den Anzeigenamen des markierten Systems (F2). In der Zeile eines Geräts gilt der Name für alle seine Läufe.");
        btnOpenReport = UI.Secondary("Bericht öffnen"); btnOpenReport.Margin = new Padding(0);
        btnOpenReport.Click += delegate { DbEntry s = SelectedDbEntry(); if (s != null) OpenEntry(s); };
        Tip(btnOpenReport, "Öffnet den vollständigen HTML-Diagnosebericht des markierten Systems.");
        btnVerlauf = UI.Secondary("Verlauf"); btnVerlauf.Margin = new Padding(0);
        btnVerlauf.Click += delegate { OpenDeviceHistory(); };
        Tip(btnVerlauf, "Öffnet das interaktive Dashboard im Verlaufsmodus für alle Läufe des markierten Geräts.");
        btnDbFolder = UI.Secondary("Ordner"); btnDbFolder.Margin = new Padding(0);
        btnDbFolder.Click += delegate { DbEntry s = SelectedDbEntry(); if (s != null) OpenEntryFolder(s); };
        Tip(btnDbFolder, "Öffnet den Berichtsordner des markierten Laufs im Explorer.");
        lnkDbRef = KartenVerweis("Als Referenz festlegen (persistent)"); lnkDbRef.LinkClicked += delegate { SetSelectedAsReference(); };
        Tip(lnkDbRef, "Legt das markierte System als dauerhafte Vergleichsreferenz (100 %) fest.");
        lnkDbBasis = KartenVerweis("Als Basis im Dashboard öffnen (temporär)"); lnkDbBasis.LinkClicked += delegate { OpenDashboardWithBase(); };
        Tip(lnkDbBasis, "Öffnet das Dashboard mit dem markierten Lauf als Vergleichsbasis, zusammen mit den angehakten Läufen. Die gespeicherte Referenz bleibt unverändert.");
        lnkDbExport = KartenVerweis("Lauf exportieren …"); lnkDbExport.LinkClicked += delegate { ExportSelected(); };
        Tip(lnkDbExport, "Exportiert den markierten Lauf als eigenständiges Paket.");
        lnkDbDelete = KartenVerweis("Lauf entfernen …"); lnkDbDelete.LinkClicked += delegate { DeleteSelected(); };
        Tip(lnkDbDelete, "Verschiebt den markierten Lauf oder die angehakten Läufe samt Berichtsordner ins Archiv des Datenordners (mit Rückfrage).");
        foreach (LinkLabel l in new LinkLabel[] { lnkDbRef, lnkDbBasis, lnkDbExport, lnkDbDelete }) l.Margin = new Padding(0);
        foreach (Button b in new Button[] { btnOpenReport, btnVerlauf, btnDbFolder }) b.Padding = new Padding(UI.S(6), UI.S(3), UI.S(6), UI.S(3));
        foreach (Control c in new Control[] { lnkDbName, btnOpenReport, btnVerlauf, btnDbFolder, lnkDbRef, lnkDbBasis, lnkDbExport, lnkDbDelete }) dbDetail.Controls.Add(c);
        dbDetail.Resize += delegate { OrdneDbDetail(); };

        // unbenutzte Knöpfe früherer Versionen bleiben als Felder für die Hinweise bestehen
        btnSetRef = UI.Secondary("Als Referenz ..."); Tip(btnSetRef, "Legt das markierte System als dauerhafte Vergleichsreferenz fest.");
        btnRename = UI.Secondary("Name ändern ..."); Tip(btnRename, "Ändert den Anzeigenamen des markierten Systems.");
        btnExport = UI.Secondary("Lauf exportieren ..."); Tip(btnExport, "Exportiert den markierten Lauf als eigenständiges Paket.");
        btnCompare = UI.Secondary("Vergleichen");

        dbSeite.Fuege(lvDb, dbDetail, dbFuss);
        FillDbList();
        return dbSeite.Seite;
    }

    // Befunde "0 / 2 / 5" farbig: kritisch rot und Warnungen gelb, sobald es welche gibt
    static void ZeichneBefunde(Graphics g, string befunde, Rectangle r, bool blass)
    {
        string[] t = (befunde ?? "").Split('/');
        int k, w, h;
        if (t.Length != 3 || !int.TryParse(t[0].Trim(), out k) || !int.TryParse(t[1].Trim(), out w) || !int.TryParse(t[2].Trim(), out h)) { ZeilenListe.Schreibe(g, befunde, ZeilenListe.FKlein, r, UI.Muted, false); return; }
        int x = r.X;
        string[] teile = new string[] { k.ToString(), "/", w.ToString(), "/", h.ToString() };
        Color[] farbe = new Color[] { k > 0 && !blass ? UI.Crit : UI.Muted, UI.Muted, w > 0 && !blass ? UI.Warn : UI.Muted, UI.Muted, UI.Muted };
        for (int i = 0; i < teile.Length; i++)
        {
            Font f = (i == 0 && k > 0) || (i == 2 && w > 0) ? ZeilenListe.FKopf : ZeilenListe.FKlein;
            int tw = TextRenderer.MeasureText(g, teile[i], f, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            TextRenderer.DrawText(g, teile[i], f, new Rectangle(x, r.Y, tw + UI.S(2), r.Height), farbe[i], TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            x += tw + UI.S(4);
        }
    }

    static string DbHardware(DbEntry e)
    {
        List<string> t = new List<string>();
        string c = DbEntry.CleanCpuName(e.Cpu); if (c.Length > 0) t.Add(c);
        if (e.Gpu.Length > 0) t.Add(e.Gpu);
        if (e.Ram.Length > 0) t.Add(e.Ram);
        return String.Join("  ·  ", t.ToArray());
    }

    static string DbGrafikWert(DbEntry e) { return e.Get("GPU|REND") > 0 ? Num(e.Get("GPU|REND"), "N0") + " FPS" : (e.Get("GPU|VMB") > 0 ? Num(e.Get("GPU|VMB"), "N0") + " GB/s" : ""); }

    void ZeichneDbZeile(Graphics g, ListenZeile zeile, Rectangle r, bool markiert)
    {
        DbZeile z = (DbZeile)zeile; DbEntry e = z.E; if (e == null) return;
        Rectangle voll = new Rectangle(0, r.Y, r.Right, r.Height);
        Color c = z.Blass ? UI.Muted : UI.Text;
        Rectangle c0 = lvDb.Zelle(0, voll);
        int grenze = c0.Right - UI.S(8);
        if (z.Gruppe)
        {
            int halb = r.Height / 2;
            string et = e.IsReference ? (z.Anzahl > 1 ? z.Anzahl + " Referenzen" : "Referenz") : (z.Anzahl > 1 ? z.Anzahl + " Läufe" : "");
            int ew = ZeilenListe.EtikettBreite(et);
            int nw = Math.Min(ZeilenListe.Breite(z.Name, ZeilenListe.FFett), Math.Max(UI.S(40), grenze - r.X - (ew > 0 ? ew + UI.S(8) : 0)));
            Rectangle oben = new Rectangle(r.X, r.Y + UI.S(4), nw, halb - UI.S(2));
            ZeilenListe.Schreibe(g, z.Name, ZeilenListe.FFett, oben, c, false);
            if (ew > 0) ZeilenListe.Etikett(g, et, e.IsReference ? "akzent" : "skip", r.X + nw + UI.S(8), oben, 0);
            ZeilenListe.Schreibe(g, DbHardware(e), ZeilenListe.FKlein, new Rectangle(r.X, r.Y + halb, grenze - r.X, halb - UI.S(4)), UI.Muted, false);
        }
        else ZeilenListe.Schreibe(g, z.Name, ZeilenListe.FText, new Rectangle(r.X, r.Y, grenze - r.X, r.Height), c, false);
        Color wc = z.Gruppe ? c : UI.Muted;
        ZeilenListe.Schreibe(g, ShortDate(e.Datum), ZeilenListe.FKlein, lvDb.Zelle(1, voll), wc, false);
        ZeilenListe.Schreibe(g, e.OverallScore > 0 ? Num(e.OverallScore, "N0") : "", z.Gruppe ? ZeilenListe.FFett : ZeilenListe.FText, lvDb.Zelle(2, voll), c, true);
        ZeilenListe.Schreibe(g, Num(e.Get("CPU|MT"), "N0"), ZeilenListe.FKlein, lvDb.Zelle(3, voll), wc, true);
        ZeilenListe.Schreibe(g, Num(e.Get("RAM|Lesen"), "N1") + (e.Get("RAM|Lesen") > 0 ? " GB/s" : ""), ZeilenListe.FKlein, lvDb.Zelle(4, voll), wc, true);
        ZeilenListe.Schreibe(g, DbGrafikWert(e), ZeilenListe.FKlein, lvDb.Zelle(5, voll), wc, true);
        Rectangle cb = lvDb.Zelle(6, voll);
        ZeichneBefunde(g, e.Befunde, new Rectangle(cb.X + UI.S(14), cb.Y, cb.Width - UI.S(14), cb.Height), z.Blass);
    }

    // Sichtbarkeit der Aktionen als eigener Zustand: Control.Visible ist falsch, solange die Seite im Hintergrund steht
    // (Neuladen nach einem Lauf), und der Detailbereich ordnete sich dann ohne seine Knöpfe
    internal Dictionary<Control, bool> dbSoll = new Dictionary<Control, bool>();
    void DbZeige(Control c, bool an) { dbSoll[c] = an; if (c.Visible != an) c.Visible = an; }
    internal bool DbSoll(Control c) { bool an; return dbSoll.TryGetValue(c, out an) && an; }

    DbZeile NeueDbZeile(DbEntry e, string name, string key, bool gruppe, int anzahl)
    {
        DbZeile z = new DbZeile(); z.E = e; z.Tag = e; z.Name = name; z.Key = key; z.Gruppe = gruppe; z.Anzahl = anzahl; z.Art = gruppe ? "geraet" : "lauf";
        z.Haken = dbHaken.Contains(e.Path) ? 1 : 0;
        bool hasBench = e.HasBench || e.OverallScore > 0;
        z.Blass = !hasBench;
        z.Hinweis = WrapTip(e.DisplayName + (e.Disk.Length > 0 ? "\r\n" + e.Disk : "") + (hasBench ? "" : "\r\n(ohne Benchmark-Werte)"));
        if (gruppe) { z.Hoehe = UI.S(44); z.Aufklappbar = anzahl > 1; z.Offen = expandedGroups.Contains(key); }
        else { z.Hoehe = UI.S(28); z.Ebene = 1; z.Grund = 2; }
        return z;
    }

    internal void FillDbList()
    {
        if (lvDb == null) return;

        // Markierung über den Neuaufbau halten (Pfad und Art der Zeile)
        List<string> selKeys = new List<string>();
        foreach (ListenZeile z in lvDb.Markiert) { DbZeile d = z as DbZeile; if (d != null && d.E != null) selKeys.Add((d.Gruppe ? "G|" : "L|") + d.E.Path); }

        Dictionary<string, List<DbEntry>> grpMap = new Dictionary<string, List<DbEntry>>();
        List<string> orderedKeys = new List<string>();

        foreach (DbEntry e in db)
        {
            string k = DbKey(e);
            if (!grpMap.ContainsKey(k))
            {
                grpMap[k] = new List<DbEntry>();
                if (k != "REF") orderedKeys.Add(k);
            }
            grpMap[k].Add(e);
        }

        foreach (KeyValuePair<string, List<DbEntry>> kv in grpMap)
        {
            kv.Value.Sort(delegate(DbEntry a, DbEntry b) {
                return String.Compare(b.Datum, a.Datum, StringComparison.Ordinal);
            });
        }

        orderedKeys.Sort(delegate(string k1, string k2) {
            DbEntry e1 = grpMap[k1].Count > 0 ? grpMap[k1][0] : null;
            DbEntry e2 = grpMap[k2].Count > 0 ? grpMap[k2][0] : null;
            if (e1 == null && e2 == null) return 0;
            if (e1 == null) return 1;
            if (e2 == null) return -1;
            int r = CompareDbEntries(e1, e2, dbSortCol >= 0 ? dbSortCol : 1);
            if (dbSortCol < 0) return -r;
            return dbSortAsc ? r : -r;
        });

        // ab v4.10 steht die Referenz oben (bis v4.09 am Ende der Liste)
        if (grpMap.ContainsKey("REF")) orderedKeys.Insert(0, "REF");

        lvDb.Zeilen.Clear();
        int geraete = 0;
        foreach (string k in orderedKeys)
        {
            List<DbEntry> entries = grpMap[k];
            if (entries.Count == 0) continue;

            if (k == "REF")
            {
                DbEntry refRep = null;
                foreach (DbEntry re in entries)
                {
                    if (re.DisplayName.IndexOf("Median", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        refRep = re;
                        break;
                    }
                }
                if (refRep == null) refRep = entries[0];

                if (entries.Count == 1) lvDb.Zeilen.Add(NeueDbZeile(entries[0], entries[0].DisplayName, k, true, 1));
                else
                {
                    DbZeile kopf = NeueDbZeile(refRep, "Referenz-Systeme", k, true, entries.Count);
                    lvDb.Zeilen.Add(kopf);
                    if (kopf.Offen) foreach (DbEntry e in entries) lvDb.Zeilen.Add(NeueDbZeile(e, e.DisplayName, k, false, 1));
                }
            }
            else
            {
                geraete++;
                DbEntry newest = entries[0];
                string devName = "";
                foreach (DbEntry e in entries)
                {
                    if (!String.IsNullOrEmpty(e.Name))
                    {
                        devName = e.Name;
                        break;
                    }
                }
                if (String.IsNullOrEmpty(devName))
                {
                    devName = !String.IsNullOrEmpty(newest.DisplayName) ? newest.DisplayName : (!String.IsNullOrEmpty(newest.Computer) ? newest.Computer : "System");
                }

                DbZeile kopf = NeueDbZeile(newest, devName, k, true, entries.Count);
                lvDb.Zeilen.Add(kopf);
                // wie bisher steht die Zeile des Geräts für den neuesten Lauf, darunter folgen die älteren
                if (kopf.Offen) for (int i = 1; i < entries.Count; i++) lvDb.Zeilen.Add(NeueDbZeile(entries[i], entries[i].DisplayName, k, false, 1));
            }
        }

        // Haken gelten nur für sichtbare Zeilen (wie bisher): Beim Zuklappen eines Geräts entfallen die seiner älteren Läufe
        HashSet<string> sichtbar = new HashSet<string>();
        foreach (ListenZeile z in lvDb.Zeilen) sichtbar.Add(((DbZeile)z).E.Path);
        dbHaken.IntersectWith(sichtbar);
        lvDb.SortSpalte = dbSortCol < 0 ? 1 : dbSortCol; lvDb.SortAuf = dbSortCol < 0 ? false : dbSortAsc;
        lvDb.Aktualisiere();
        List<ListenZeile> wieder = new List<ListenZeile>();
        foreach (ListenZeile z in lvDb.Zeilen) { DbZeile d = (DbZeile)z; if (selKeys.Contains((d.Gruppe ? "G|" : "L|") + d.E.Path)) wieder.Add(z); }
        lvDb.MarkiereMehrere(wieder);

        int laeufe = 0; foreach (DbEntry e in db) if (!e.IsReference) laeufe++;
        SetText(dbSeite.Anzahl, Mehrzahl(geraete, "Gerät", "Geräte") + "  ·  " + Mehrzahl(laeufe, "Lauf", "Läufe"));
        SetText(dbSeite.Unter, AblageKurz(dataDir));
        UpdateToggleGroupsButton();
        if (navDb != null) { navDb.Desc = db.Count + " Systeme, vergleichen und verwalten"; navDb.Invalidate(); }
        UpdateDbButtons();
    }

    List<DbEntry> CheckedEntries()
    {
        List<DbEntry> l = new List<DbEntry>();
        if (lvDb == null) return l;
        // in der Reihenfolge der Liste; eine Zeile je Lauf
        foreach (ListenZeile z in lvDb.Zeilen) { DbZeile d = z as DbZeile; if (d != null && d.E != null && dbHaken.Contains(d.E.Path) && !l.Contains(d.E)) l.Add(d.E); }
        return l;
    }

    internal void UpdateDbButtons()
    {
        if (btnDashboard == null || dbSeite == null) return;
        List<DbEntry> chk = CheckedEntries();
        int chkN = chk.Count;
        int selN = DbMarkierte().Count;

        btnDashboard.Enabled = (chkN >= 1);
        btnDashboard.Text = (chkN >= 1 ? String.Format("Im Dashboard vergleichen ({0})", chkN) : "Im Dashboard vergleichen");
        List<string> namen = new List<string>(); foreach (DbEntry e in chk) if (!namen.Contains(e.DisplayName)) namen.Add(e.DisplayName);
        SetText(dbSeite.BandText, chkN == 1 ? "1 Lauf angehakt" : chkN + " Läufe angehakt");
        SetText(dbSeite.BandUnter, String.Join(", ", namen.ToArray()));
        dbSeite.BandSichtbar = chkN >= 1;

        // Aktionen des Detailbereichs: für genau einen markierten Lauf; Entfernen auch für angehakte Läufe
        DbEntry sel = SelectedDbEntry();
        foreach (Control c in new Control[] { btnOpenReport, btnDbFolder, lnkDbRef, lnkDbBasis, lnkDbExport }) DbZeige(c, selN == 1);
        DbZeige(btnVerlauf, selN == 1 && GetSelectedDeviceTarget() != null);
        DbZeige(lnkDbName, selN == 1 && sel != null && !sel.IsReference);
        DbZeige(lnkDbDelete, selN == 1 || chkN > 0);
        SetText(lnkDbDelete, chkN > 1 ? "Angehakte Läufe entfernen (" + chkN + ") …" : chkN == 1 ? "Angehakten Lauf entfernen …" : "Lauf entfernen …");
        OrdneDbDetail();
    }

    internal DbEntry SelectedDbEntry() { List<DbEntry> m = DbMarkierte(); return m.Count == 1 ? m[0] : null; }

    // ------------------------------------------------------------ Detailbereich
    static readonly string[] dbGruppen = new string[] { "CPU", "GPU", "RAM", "Laufwerke" };

    static string DbBefundText(DbEntry e)
    {
        if (e.NK >= 0 && e.NW >= 0 && e.NI >= 0) return e.NK + " kritisch  ·  " + Mehrzahl(e.NW, "Warnung", "Warnungen") + "  ·  " + Mehrzahl(e.NI, "Hinweis", "Hinweise");
        string[] t = (e.Befunde ?? "").Split('/'); int k, w, h;
        if (t.Length == 3 && int.TryParse(t[0].Trim(), out k) && int.TryParse(t[1].Trim(), out w) && int.TryParse(t[2].Trim(), out h)) return k + " kritisch  ·  " + Mehrzahl(w, "Warnung", "Warnungen") + "  ·  " + Mehrzahl(h, "Hinweis", "Hinweise");
        return e.Befunde.Length > 0 ? e.Befunde : "nicht gespeichert";
    }

    // Zeichnet den Inhalt des Detailbereichs ab der Höhe oben (Bildlauf) und liefert die untere Kante; g == null misst nur
    int DbDetailInhalt(Graphics g, int oben)
    {
        int x = UI.S(16), w = Math.Max(UI.S(120), dbDetail.ClientSize.Width - UI.S(32)), y = oben + UI.S(14);
        List<DbEntry> m = DbMarkierte();
        TextFormatFlags um = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
        if (m.Count != 1)
        {
            string t = m.Count == 0 ? "Einen Lauf markieren, um Einzelheiten und Aktionen zu sehen." : m.Count + " Läufe markiert. Zum Vergleichen anhaken oder das Kontextmenü nutzen.";
            int th = TextRenderer.MeasureText(t, ZeilenListe.FText, new Size(w, int.MaxValue), um).Height;
            if (g != null) TextRenderer.DrawText(g, t, ZeilenListe.FText, new Rectangle(x, y, w, th), UI.Muted, um);
            return y + th + UI.S(12);
        }
        DbEntry e = m[0]; DbZeile z = DbMarkierteZeile();
        using (Font fn = new Font("Segoe UI Semibold", 11.5f)) using (Font fp = new Font("Segoe UI", 18f, FontStyle.Bold))
        {
            string name = z != null && z.Gruppe ? z.Name : e.DisplayName;
            int nh = fn.Height + UI.S(2);
            if (g != null) ZeilenListe.Schreibe(g, name, fn, new Rectangle(x - UI.S(2), y, w - (DbSoll(lnkDbName) ? lnkDbName.PreferredSize.Width + UI.S(8) : 0), nh), UI.Text, false);
            y += nh + UI.S(2);
            string zeile = FormatRunDate(e.Datum) + (z != null && z.Gruppe && z.Anzahl > 1 && !e.IsReference ? "  ·  neuester von " + z.Anzahl + " Läufen" : "") + (e.Module.Length > 0 ? "\r\n" + e.Module : "");
            Size zs = TextRenderer.MeasureText(zeile, ZeilenListe.FKlein, new Size(w, int.MaxValue), um);
            int zh = Math.Min(zs.Height, ZeilenListe.FKlein.Height * 4 + UI.S(2));
            if (g != null) TextRenderer.DrawText(g, zeile, ZeilenListe.FKlein, new Rectangle(x, y, w, zh), UI.Muted, um);
            y += zh + UI.S(10);
            if (e.OverallScore > 0)
            {
                string p = Num(e.OverallScore, "N0"); int ph = fp.Height;
                if (g != null)
                {
                    int pw = TextRenderer.MeasureText(g, p, fp, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
                    TextRenderer.DrawText(g, p, fp, new Rectangle(x - UI.S(3), y, w, ph), UI.Accent, TextFormatFlags.Left | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, "Punkte", ZeilenListe.FKlein, new Rectangle(x + pw + UI.S(8), y + ph - ZeilenListe.FKlein.Height - UI.S(5), UI.S(80), ZeilenListe.FKlein.Height + 2), UI.Muted, TextFormatFlags.Left);
                }
                y += ph + UI.S(4);
            }
            else
            {
                if (g != null) TextRenderer.DrawText(g, "ohne Benchmark-Werte", ZeilenListe.FText, new Rectangle(x, y, w, UI.S(20)), UI.Muted, TextFormatFlags.Left);
                y += UI.S(24);
            }
            // Vergleich mit der Referenz, wie ihn der Lauf gespeichert hat (ab v4.08)
            if (e.HatVergleich && e.GruppenPct.Count > 0)
            {
                if (g != null) ZeilenListe.Schreibe(g, "gegen " + (e.RefName.Length > 0 ? e.RefName : "die Referenz des Laufs"), ZeilenListe.FKlein, new Rectangle(x, y, w, UI.S(18)), UI.Muted, false);
                y += UI.S(20);
                List<string> keys = new List<string>(); foreach (string k in dbGruppen) if (e.GruppenPct.ContainsKey(k)) keys.Add(k);
                foreach (string k in e.GruppenPct.Keys) if (!keys.Contains(k)) keys.Add(k);
                foreach (string k in keys)
                {
                    if (g != null)
                    {
                        double pct = e.GruppenPct[k]; int lw = UI.S(104), vw = UI.S(48), bh = UI.S(6);
                        ZeilenListe.Schreibe(g, GroupName(k), ZeilenListe.FKlein, new Rectangle(x, y, lw, UI.S(20)), UI.Text, false);
                        ZeilenListe.Schreibe(g, pct.ToString("0", CultureInfo.GetCultureInfo("de-DE")) + " %", ZeilenListe.FKlein, new Rectangle(x + w - vw, y, vw, UI.S(20)), UI.Text, true);
                        Rectangle bar = new Rectangle(x + lw, y + (UI.S(20) - bh) / 2, Math.Max(UI.S(20), w - lw - vw - UI.S(6)), bh);
                        // Balken bis 200 %, Marke bei 100 %
                        using (SolidBrush bb = new SolidBrush(UI.Mix(UI.Panel, UI.Muted, 0.28))) g.FillRectangle(bb, bar);
                        using (SolidBrush bf = new SolidBrush(pct >= 95 ? UI.Ok : pct >= 70 ? UI.Warn : UI.Crit)) g.FillRectangle(bf, bar.X, bar.Y, (int)(bar.Width * Math.Min(200.0, Math.Max(0.0, pct)) / 200.0), bar.Height);
                        using (SolidBrush bm = new SolidBrush(UI.Text)) g.FillRectangle(bm, bar.X + bar.Width / 2, bar.Y - UI.S(2), Math.Max(1, UI.S(1)), bar.Height + UI.S(4));
                    }
                    y += UI.S(22);
                }
            }
            else if (e.OverallScore > 0 && !e.IsReference)
            {
                string t = "Vergleich mit der Referenz: nicht gespeichert (Lauf vor Version 4.08 oder Import). Der Bericht nennt ihn.";
                int th = TextRenderer.MeasureText(t, ZeilenListe.FKlein, new Size(w, int.MaxValue), um).Height;
                if (g != null) TextRenderer.DrawText(g, t, ZeilenListe.FKlein, new Rectangle(x, y, w, th), UI.Muted, um);
                y += th + UI.S(2);
            }
            y += UI.S(6);
            if (g != null) using (Pen pl = new Pen(UI.Line)) g.DrawLine(pl, x, y, x + w, y);
            y += UI.S(8);
            string[] kn = new string[] { "Prozessor", "Grafik", "Speicher", "Laufwerke", "Befunde" };
            string[] kw = new string[] { e.CpuDisplay, e.Gpu, e.Ram, e.Disk, e.IsReference ? "" : DbBefundText(e) };
            int kl = UI.S(74);
            for (int i = 0; i < kn.Length; i++)
            {
                if (kw[i].Length == 0) continue;
                Size ws = TextRenderer.MeasureText(kw[i], ZeilenListe.FKlein, new Size(w - kl, int.MaxValue), um);
                int wh = Math.Min(Math.Max(ws.Height, ZeilenListe.FKlein.Height), ZeilenListe.FKlein.Height * 3 + UI.S(2));
                if (g != null)
                {
                    TextRenderer.DrawText(g, kn[i], ZeilenListe.FKlein, new Rectangle(x, y, kl, wh), UI.Muted, TextFormatFlags.Left | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, kw[i], ZeilenListe.FKlein, new Rectangle(x + kl, y, w - kl, wh), UI.Text, um);
                }
                y += wh + UI.S(4);
            }
        }
        return y + UI.S(8);
    }

    void ZeichneDbDetail(Graphics g) { DbDetailInhalt(g, dbDetail.AutoScrollPosition.Y); }

    // Knöpfe und Verweise unter dem gezeichneten Inhalt
    internal void OrdneDbDetail()
    {
        if (dbDetail == null || lnkDbName == null) return;
        // Lagen im Bildlauf: Kindelemente stehen um die Bildlaufposition versetzt
        int ab = dbDetail.AutoScrollPosition.Y;
        int x = UI.S(16), y = DbDetailInhalt(null, ab), w = Math.Max(UI.S(120), dbDetail.ClientSize.Width - UI.S(32));
        lnkDbName.Location = new Point(dbDetail.ClientSize.Width - UI.S(16) - lnkDbName.PreferredSize.Width, ab + UI.S(18));
        int bx = x, zeile = 0;
        foreach (Button b in new Button[] { btnOpenReport, btnVerlauf, btnDbFolder })
        {
            if (!DbSoll(b)) continue;
            Size s = b.GetPreferredSize(Size.Empty);
            if (bx > x && bx + s.Width > x + w) { bx = x; y += zeile + UI.S(6); zeile = 0; }
            b.Location = new Point(bx, y); bx += s.Width + UI.S(8); zeile = Math.Max(zeile, s.Height);
        }
        if (zeile > 0) y += zeile + UI.S(12);
        foreach (LinkLabel l in new LinkLabel[] { lnkDbRef, lnkDbBasis, lnkDbExport, lnkDbDelete })
        {
            if (!DbSoll(l)) continue;
            l.Location = new Point(x - UI.S(2), y); y += l.PreferredSize.Height + UI.S(6);
        }
        Size min = new Size(0, y - ab + UI.S(8));
        if (dbDetail.AutoScrollMinSize != min) dbDetail.AutoScrollMinSize = min;
        dbDetail.Invalidate();
    }

    static string ShortDate(string d)
    {
        DateTime t;
        if (DateTime.TryParseExact(d, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out t)) return t.ToString("dd.MM.yy HH:mm");
        return d;
    }

    static string Num(double v, string fmt) { return v > 0 ? v.ToString(fmt, CultureInfo.GetCultureInfo("de-DE")) : ""; }

    static string FormatGroupDate(string d)
    {
        if (String.IsNullOrEmpty(d)) return "";
        DateTime dt;
        if (DateTime.TryParseExact(d, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
            DateTime.TryParseExact(d, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
            DateTime.TryParse(d, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
        {
            return dt.ToString("dd.MM.yyyy");
        }
        string[] parts = d.Split(' ', 'T');
        if (parts.Length > 0 && parts[0].Contains("-"))
        {
            string[] dp = parts[0].Split('-');
            if (dp.Length == 3) return dp[2] + "." + dp[1] + "." + dp[0];
        }
        return d;
    }

    static int CompareDbEntries(DbEntry dx, DbEntry dy, int col)
    {
        if (dx == null && dy == null) return 0;
        if (dx == null) return -1;
        if (dy == null) return 1;
        int res = 0;
        switch (col)
        {
            case 0:
                res = String.Compare(dx.DisplayName, dy.DisplayName, StringComparison.CurrentCultureIgnoreCase);
                break;
            case 1:
                res = String.Compare(dx.Datum, dy.Datum, StringComparison.Ordinal);
                break;
            case 2:
                res = dx.OverallScore.CompareTo(dy.OverallScore);
                break;
            case 3:
                res = String.Compare(dx.Cpu, dy.Cpu, StringComparison.CurrentCultureIgnoreCase);
                break;
            case 4:
                res = String.Compare(dx.Gpu, dy.Gpu, StringComparison.CurrentCultureIgnoreCase);
                break;
            case 5:
                res = String.Compare(dx.Ram, dy.Ram, StringComparison.CurrentCultureIgnoreCase);
                break;
            case 6:
                res = dx.Get("CPU|MT").CompareTo(dy.Get("CPU|MT"));
                break;
            case 7:
                res = dx.Get("RAM|Lesen").CompareTo(dy.Get("RAM|Lesen"));
                break;
            case 8:
                double gx = dx.Get("GPU|REND") > 0 ? dx.Get("GPU|REND") : dx.Get("GPU|VMB");
                double gy = dy.Get("GPU|REND") > 0 ? dy.Get("GPU|REND") : dy.Get("GPU|VMB");
                res = gx.CompareTo(gy);
                break;
            case 9:
                res = String.Compare(dx.Befunde, dy.Befunde, StringComparison.OrdinalIgnoreCase);
                break;
            default:
                res = String.Compare(dx.Datum, dy.Datum, StringComparison.Ordinal);
                break;
        }
        return res;
    }

    // Berichtsordner eines Laufs im Explorer (auch für Einträge mit altem UNC-Pfad, dann der gleichnamige Ordner im Datenordner)
    void OpenEntryFolder(DbEntry e)
    {
        if (e == null) return;
        if (e.Ordner.Length > 0)
        {
            string dir = System.IO.Path.IsPathRooted(e.Ordner) ? e.Ordner : System.IO.Path.Combine(dataDir, e.Ordner);
            if (!Directory.Exists(dir)) { string alt = System.IO.Path.Combine(System.IO.Path.Combine(dataDir, "Berichte"), System.IO.Path.GetFileName(e.Ordner.TrimEnd('\\', '/'))); if (Directory.Exists(alt)) dir = alt; }
            if (Directory.Exists(dir)) { OpenShell(dir); return; }
        }
        OpenSelect(e.Path);
    }
    void ShowSetRefMenu()
    {
        ContextMenu m = new ContextMenu();
        m.MenuItems.Add("Als Referenz festlegen (persistent)", delegate { SetSelectedAsReference(); });
        m.MenuItems.Add("Als Basis im Dashboard öffnen (temporär)", delegate { OpenDashboardWithBase(); });
        m.Show(btnSetRef, new Point(0, btnSetRef.Height));
    }

    void SetSelectedAsReference()
    {
        DbEntry target = null;
        if (DbMarkierte().Count > 0) target = DbMarkierte()[0];
        else if (CheckedEntries().Count == 1) target = CheckedEntries()[0];
        if (target == null || !target.HasBench)
        {
            UI.Hinweis("Bitte wählen Sie ein System mit Benchmark-Werten aus.", "Referenz festlegen", this);
            return;
        }

        string msg = String.Format("Möchten Sie das System \"{0}\" ({1})\r\nals dauerhafte Vergleichsreferenz (100 %) festlegen?\r\n\r\nDie Werte werden in Minibench-Daten\\Referenz.json gespeichert.", target.DisplayName, target.Datum);
        if (!UI.Ask(msg, "Referenz festlegen", this)) return;

        try
        {
            if (String.IsNullOrEmpty(dataDir) || !Directory.Exists(dataDir))
            {
                UI.Hinweis("Der Datenordner ist nicht verfügbar.", "Referenz festlegen", this);
                return;
            }
            string dest = Path.Combine(dataDir, "Referenz.json");
            if (File.Exists(target.Path))
            {
                string raw = File.ReadAllText(target.Path, Encoding.UTF8);
                File.WriteAllText(dest, raw, new UTF8Encoding(false));
                UI.Hinweis("Das System \"" + target.DisplayName + "\" wurde als Referenz gespeichert. Sie gilt ab dem nächsten Lauf, wenn auf der Seite Benchmark \u201EGespeicherte Referenz\u201C gewählt ist, und hat dort Vorrang vor den Median-Referenzen.", "Referenz festgelegt", this);
                ReloadDb();
                UpdateRefState();
            }
        }
        catch (Exception ex)
        {
            UI.Hinweis("Fehler beim Speichern der Referenz: " + ex.Message, "Referenz festlegen", this);
        }
    }

    void OpenDashboardWithBase()
    {
        DbEntry target = null;
        if (DbMarkierte().Count > 0) target = DbMarkierte()[0];
        else if (CheckedEntries().Count == 1) target = CheckedEntries()[0];
        if (target == null) return;

        List<DbEntry> sel = CheckedEntries();
        List<string> paths = new List<string>();
        paths.Add(target.Path);
        foreach (DbEntry e in sel)
        {
            if (e.Path != target.Path && !paths.Contains(e.Path)) paths.Add(e.Path);
        }

        Cursor = Cursors.WaitCursor;
        List<string> lines = new List<string>(); string res = "";
        string args = "-Dashboard -DashboardSysteme \"" + String.Join(";", paths.ToArray()) + "\"";
        try { res = RunHelper(args, out lines); }
        catch (Exception ex) { lines.Add("Dashboard-Aufruf fehlgeschlagen: " + ex.Message); }
        Cursor = Cursors.Default;
        if (res.Length > 0 && File.Exists(res)) OpenShell(res);
        else
        {
            string dashPath = System.IO.Path.Combine(dataDir, "Berichte\\Dashboard.html");
            if (File.Exists(dashPath)) OpenShell(dashPath);
            else UI.Hinweis("Das Dashboard konnte nicht geöffnet werden.\r\n\r\n" + String.Join("\r\n", lines.ToArray()), "Leos Minibench", this);
        }
    }

    void OpenEntry(DbEntry e)
    {
        if (e == null) return;
        if (e.Ordner.Length > 0)
        {
            string dir = System.IO.Path.IsPathRooted(e.Ordner) ? e.Ordner : System.IO.Path.Combine(dataDir, e.Ordner);
            // Einträge aus der Zeit, als die Ablage auf dem NAS lag, können einen UNC-Pfad tragen: dann der gleichnamige Ordner auf dem Stick
            if (!Directory.Exists(dir)) { string alt = System.IO.Path.Combine(System.IO.Path.Combine(dataDir, "Berichte"), System.IO.Path.GetFileName(e.Ordner.TrimEnd('\\', '/'))); if (Directory.Exists(alt)) dir = alt; }
            string html = System.IO.Path.Combine(dir, "Diagnosebericht.html");
            if (File.Exists(html)) { OpenShell(html); return; }
            if (Directory.Exists(dir)) { OpenShell(dir); return; }
        }
        OpenSelect(e.Path);
    }

    // Vergleichen (Klassisch): In v3.32 vollständig durch das interaktive Multi-System-Dashboard abgelöst
    void CompareSelected()
    {
        OpenDashboard();
    }

    DbEntry GetSelectedDeviceTarget()
    {
        List<DbEntry> targets = new List<DbEntry>();
        if (lvDb != null)
        {
            List<DbEntry> quelle = DbMarkierte();
            if (quelle.Count == 0) quelle = CheckedEntries();
            foreach (DbEntry e in quelle) if (e != null && !e.IsReference) targets.Add(e);
        }
        if (targets.Count == 0) return null;
        string firstKey = !String.IsNullOrEmpty(targets[0].GeraetKey) ? targets[0].GeraetKey : targets[0].Computer;
        if (String.IsNullOrEmpty(firstKey) || firstKey == "REF") return null;
        for (int i = 1; i < targets.Count; i++)
        {
            string k = !String.IsNullOrEmpty(targets[i].GeraetKey) ? targets[i].GeraetKey : targets[i].Computer;
            if (!String.Equals(k, firstKey, StringComparison.OrdinalIgnoreCase)) return null;
        }
        return targets[0];
    }

    public void OpenDeviceHistory(DbEntry specificTarget = null)
    {
        DbEntry target = specificTarget != null ? specificTarget : GetSelectedDeviceTarget();
        if (target == null) return;
        string devKey = target.GeraetKey;
        if (String.IsNullOrEmpty(devKey)) return;

        List<DbEntry> runs = new List<DbEntry>();
        foreach (DbEntry e in db)
        {
            if (!e.IsReference && e.GeraetKey == devKey) runs.Add(e);
        }
        runs.Sort(delegate(DbEntry a, DbEntry b) { return String.Compare(a.Datum, b.Datum, StringComparison.Ordinal); });
        if (runs.Count == 0) return;

        List<string> paths = new List<string>();
        foreach (DbEntry e in runs) paths.Add(e.Path);

        Cursor = Cursors.WaitCursor;
        List<string> lines = new List<string>(); string res = "";
        string args = "-Dashboard -DashboardModus Verlauf -DashboardSysteme \"" + String.Join(";", paths.ToArray()) + "\"";
        try { res = RunHelper(args, out lines); }
        catch (Exception ex) { lines.Add("Dashboard-Aufruf fehlgeschlagen: " + ex.Message); }
        Cursor = Cursors.Default;
        if (res.Length > 0 && File.Exists(res)) OpenShell(res);
        else
        {
            string dashPath = System.IO.Path.Combine(dataDir, "Berichte\\Dashboard.html");
            if (File.Exists(dashPath)) OpenShell(dashPath);
            else UI.Hinweis("Das Dashboard konnte nicht geöffnet werden.\r\n\r\n" + String.Join("\r\n", lines.ToArray()), "Leos Minibench", this);
        }
    }

    void OpenDashboard()
    {
        Cursor = Cursors.WaitCursor;
        List<string> lines = new List<string>(); string res = "";
        List<DbEntry> sel = CheckedEntries();
        if (sel.Count == 0) sel.AddRange(DbMarkierte());
        string args = "-Dashboard";
        if (sel.Count > 0)
        {
            List<string> paths = new List<string>();
            foreach (DbEntry e in sel) paths.Add(e.Path);
            args += " -DashboardSysteme \"" + String.Join(";", paths.ToArray()) + "\"";
        }
        try { res = RunHelper(args, out lines); }
        catch (Exception ex) { lines.Add("Dashboard-Aufruf fehlgeschlagen: " + ex.Message); }
        Cursor = Cursors.Default;
        if (res.Length > 0 && File.Exists(res)) OpenShell(res);
        else
        {
            string dashPath = System.IO.Path.Combine(dataDir, "Berichte\\Dashboard.html");
            if (File.Exists(dashPath)) OpenShell(dashPath);
            else UI.Hinweis("Das Dashboard konnte nicht geöffnet werden.\r\n\r\n" + String.Join("\r\n", lines.ToArray()), "Leos Minibench", this);
        }
    }

    // ab v4.14: Tasten der Datenbankliste. F2 benennt um (wie bisher). Entf entfernt die angehakten Läufe, ohne Haken die
    // markierten (immer mit Rückfrage). Leertaste bei mehreren markierten Zeilen: Sind alle angehakt, entfallen die Haken,
    // sonst werden alle gesetzt; bei einer Zeile hakt die Liste selbst die Zeile mit dem Fokus an (Liste.cs). Die Zeile
    // eines Geräts steht wie beim Klick auf ihr Kästchen für seinen neuesten Lauf.
    internal bool DbTaste(Keys k)
    {
        if (lvDb == null) return false;
        if (k == Keys.F2 && lvDb.Markiert.Count == 1) { RenameSelectedEntry(); return true; }
        if (k == Keys.Delete && (lvDb.Markiert.Count > 0 || CheckedEntries().Count > 0)) { DeleteSelected(true); return true; }
        if (k == Keys.Space && lvDb.Markiert.Count > 1)
        {
            List<string> pfade = new List<string>();
            foreach (DbEntry e in DbMarkierte()) if (!String.IsNullOrEmpty(e.Path) && !pfade.Contains(e.Path)) pfade.Add(e.Path);
            if (pfade.Count == 0) return true;
            bool alle = true;
            foreach (string p in pfade) if (!dbHaken.Contains(p)) { alle = false; break; }
            foreach (string p in pfade) { if (alle) dbHaken.Remove(p); else dbHaken.Add(p); }
            FillDbList();
            return true;
        }
        return false;
    }

    // ab 3.54: Eintrag und Berichtsordner ins Archiv (Minibench-Daten\Archiv\Entfernt), über den Arbeitsprozess.
    // ab v4.14 mit markierteOhneHaken (Taste Entf): ohne angehakte Läufe gelten alle markierten, nicht nur genau einer.
    void DeleteSelected(bool markierteOhneHaken = false)
    {
        List<DbEntry> sel = CheckedEntries();
        if (sel.Count == 0 && (DbMarkierte().Count == 1 || markierteOhneHaken))
            foreach (DbEntry m in DbMarkierte()) if (!String.IsNullOrEmpty(m.Path) && !sel.Contains(m)) sel.Add(m);
        if (sel.Count == 0) return;
        StringBuilder sb = new StringBuilder();
        foreach (DbEntry e in sel) sb.AppendLine("·  " + e.DisplayName + "  " + e.Datum);
        string nas, ben; bool mitNas = NasAblage.LeseKonfig(dataDir, out nas, out ben);
        string msg = (sel.Count > 1 ? "Diese " + sel.Count + " Läufe entfernen?" : "Diesen Lauf entfernen?") + "\r\n\r\n" + sb.ToString() + "\r\nDatenbankeintrag und Berichtsordner werden nach Minibench-Daten\\Archiv\\Entfernt verschoben und lassen sich von dort zurückholen."
            + (mitNas ? " Auf dem Netzlaufwerk verschwinden sie beim nächsten Aktualisieren (dort ebenfalls ins Archiv)." : "");
        if (!UI.Ask(msg, "Läufe entfernen", this)) return;
        List<string> paths = new List<string>();
        foreach (DbEntry e in sel) paths.Add(e.Path);
        List<string> lines;
        string res = RunHelperPumped("-Entfernen " + Q(String.Join("|", paths.ToArray())), out lines, 300);
        ReloadDb();
        string[] r = res.Split('|');
        if (r.Length < 2 || r[1] != "0") UI.Hinweis(lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Keine Rückmeldung vom Arbeitsprozess.", "Läufe entfernen", this);
    }
    bool abgleichLaeuft = false;
    void RefreshAndSync()
    {
        if (abgleichLaeuft) return;
        abgleichLaeuft = true;
        try { AbgleichenUndLaden(); } finally { abgleichLaeuft = false; }
    }

    void AbgleichenUndLaden()
    {
        string pfad, benutzer;
        if (dataDir.Length == 0 || !NasAblage.LeseKonfig(dataDir, out pfad, out benutzer)) { ReloadDb(); return; }
        string fehler = NasAblage.PfadFehler(pfad);
        if (fehler.Length > 0) { ReloadDb(); UI.Hinweis("Der gespeicherte Pfad des Netzlaufwerks ist nicht verwendbar: " + fehler + "\r\n\r\nBitte unter \"Netzlaufwerk ...\" korrigieren.", "Aktualisieren", this); return; }
        SetText(lblDbPath, "Netzlaufwerk " + pfad + " wird geprüft ...");
        bool warAn = Enabled; Enabled = false;
        bool da;
        try { da = NasAblage.Erreichbar(pfad, 6000); } finally { Enabled = warAn; }
        if (!da && benutzer.Length > 0)
        {
            string pw = PromptPassword(pfad, benutzer);
            if (pw != null)
            {
                string err = NasAblage.Verbinden(pfad, benutzer, pw);
                Enabled = false;
                try { da = err.Length == 0 && NasAblage.Erreichbar(pfad, 6000); } finally { Enabled = warAn; }
                if (!da && err.Length > 0) UI.Hinweis(err, "Netzlaufwerk verbinden", this);
            }
        }
        if (!da)
        {
            ReloadDb();
            SetText(lblDbPath, NasAblage.AblageText(dataDir) + "   ·   gerade nicht erreichbar");
            return;
        }
        List<string> lines;
        string res = RunHelperPumped("-Abgleich", out lines, 3600, lblDbPath);
        string[] r0 = res.Split('|');
        if (r0.Length >= 7 && r0[6] == "0")
        {
            // Freigabe erreichbar, Ordner aber weg, obwohl schon abgeglichen: nicht stillschweigend neu anlegen
            string frage = "Die Freigabe ist erreichbar, der Ordner " + pfad + " aber nicht, obwohl schon abgeglichen wurde.\r\n\r\n"
                + "Ist die Freigabe richtig eingehängt und der Ordner nicht verschoben? Nur wenn das Netzlaufwerk neu eingerichtet wurde: "
                + "Ordner neu anlegen und beide Seiten zusammenführen? Dabei wird nichts gelöscht.";
            if (UI.Ask(frage, "Abgleich mit dem Netzlaufwerk", this))
                res = RunHelperPumped("-Abgleich -AbgleichNeu", out lines, 3600, lblDbPath);
            else { ReloadDb(); SetText(lblDbPath, NasAblage.AblageText(dataDir) + "   ·   Ordner auf dem Netzlaufwerk fehlt, nichts geändert"); return; }
        }
        if (NasAblage.Angehalten(res))
        {
            // Schutz vor Massenlöschung: nichts wurde geändert; nur auf ausdrücklichen Wunsch fortsetzen
            string frage = "Der Abgleich wurde angehalten, nichts wurde geändert:\r\n\r\n" + String.Join("\r\n", lines.ToArray())
                + "\r\n\r\nIst die Freigabe richtig eingehängt und der Ordner nicht verschoben?\r\n\r\n"
                + "Ja: Die Daten wurden bewusst entfernt. Die fehlenden Dateien werden auf der anderen Seite ins Archiv verschoben (Archiv\\Abgleich).\r\n"
                + "Nein: Beide Seiten zusammenführen, nichts entfernen (z. B. Netzlaufwerk neu eingerichtet oder geleert).\r\n"
                + "Abbrechen: nichts tun.";
            DialogResult wahl = UI.Ask3(frage, "Abgleich mit dem Netzlaufwerk", this);
            if (wahl == DialogResult.Yes) res = RunHelperPumped("-Abgleich -AbgleichLoeschen", out lines, 3600, lblDbPath);
            else if (wahl == DialogResult.No) res = RunHelperPumped("-Abgleich -AbgleichNeu", out lines, 3600, lblDbPath);
            else { ReloadDb(); SetText(lblDbPath, NasAblage.AblageText(dataDir) + "   ·   Abgleich angehalten, nichts geändert"); return; }
        }
        ReloadDb();
        bool ok, erreichbar;
        string kurz = NasAblage.ErgebnisText(res, out ok, out erreichbar);
        SetText(lblDbPath, NasAblage.AblageText(dataDir) + "   ·   " + kurz);
        // Konflikte, Fehler und Abbrüche anzeigen; ein reiner Abgleich ohne Besonderheiten steht nur in der Zeile
        string[] x = res.Split('|');
        bool besonders = !ok || !erreichbar || (x.Length >= 6 && (x[4] != "0" || x[5] != "0"));
        if (besonders) UI.Hinweis(kurz + (lines.Count > 0 ? "\r\n\r\n" + String.Join("\r\n", lines.ToArray()) : "") + "\r\n\r\nProtokoll: Minibench-Daten\\Abgleich\\Abgleich.log", "Abgleich mit dem Netzlaufwerk", this);
    }

    void CleanData()
    {
        if (dataDir.Length == 0) return;
        if (!UI.Ask("Nicht vergleichbare, abgebrochene und unvollständige Läufe nach Minibench-Daten\\Archiv verschieben?\r\n\r\nGelöscht wird nichts außer älteren Übersetzungen im Cache. Das Protokoll steht in Archiv\\Datenpflege.log.", "Daten aufräumen", this)) return;
        Cursor = Cursors.WaitCursor;
        List<string> lines = new List<string>(); string res = "";
        try { res = RunHelper("-Datenpflege", out lines); }
        catch (Exception ex) { lines.Add("Datenpflege fehlgeschlagen: " + ex.Message); }
        Cursor = Cursors.Default;
        ReloadDb();
        if (res.Length > 0) SetText(lblDbClean, "Datenpflege: " + res.Split('|')[0]);
        UI.Hinweis(lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Nichts zu archivieren.", "Daten aufräumen", this);
    }

    void ExportSelected()
    {
        DbEntry target = null;
        if (DbMarkierte().Count == 1) target = DbMarkierte()[0];
        else if (CheckedEntries().Count == 1) target = CheckedEntries()[0];
        if (target == null) return;

        using (SaveFileDialog sfd = new SaveFileDialog())
        {
            sfd.Title = "Lauf als Minibench-Paket exportieren";
            sfd.Filter = "Minibench-Paket (*.minibench)|*.minibench|ZIP-Archiv (*.zip)|*.zip|Alle Dateien (*.*)|*.*";
            sfd.FilterIndex = 1;
            string safeDate = target.Datum != null ? target.Datum.Replace("-", "").Replace(":", "").Replace(" ", "_") : "Lauf";
            sfd.FileName = (target.Computer ?? "System") + "_" + safeDate + ".minibench";
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            Cursor = Cursors.WaitCursor;
            List<string> lines;
            string res = RunHelper("-ExportLauf " + Q(target.Path) + " -ExportZiel " + Q(sfd.FileName), out lines);
            Cursor = Cursors.Default;
            UI.Hinweis(lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Export abgeschlossen.", "Lauf exportieren", this);
        }
    }

    void ImportRuns()
    {
        if (dbDir.Length == 0) { UI.Hinweis("Kein Datenordner verfügbar.", "Leos Minibench", this); return; }
        Form dlg = new Form();
        dlg.Text = "Durchläufe importieren";
        dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
        dlg.MaximizeBox = false; dlg.MinimizeBox = false;
        dlg.StartPosition = FormStartPosition.CenterParent;
        dlg.ClientSize = new Size(UI.S(480), UI.S(160));
        dlg.BackColor = UI.Bg; dlg.Font = new Font("Segoe UI", 9.5f);

        Label lbl = new Label();
        lbl.Text = "Möchten Sie einzelne Laufpakete (.minibench, .zip) bzw. Datenbankdateien (.json) oder einen ganzen Ausgabeordner importieren?";
        lbl.Location = new Point(UI.S(16), UI.S(14));
        lbl.Size = new Size(UI.S(448), UI.S(42));
        lbl.ForeColor = UI.Text;

        Button btnFiles = UI.Primary("Dateien wählen ...");
        btnFiles.Location = new Point(UI.S(16), UI.S(75));
        btnFiles.Size = new Size(UI.S(160), UI.S(34));

        Button btnFolder = UI.Secondary("Ordner wählen ...");
        btnFolder.Location = new Point(UI.S(184), UI.S(75));
        btnFolder.Size = new Size(UI.S(150), UI.S(34));

        Button btnCancel = UI.Secondary("Abbrechen");
        btnCancel.Location = new Point(UI.S(342), UI.S(75));
        btnCancel.Size = new Size(UI.S(120), UI.S(34));
        btnCancel.DialogResult = DialogResult.Cancel;

        int choice = 0;
        btnFiles.Click += delegate { choice = 1; dlg.DialogResult = DialogResult.OK; dlg.Close(); };
        btnFolder.Click += delegate { choice = 2; dlg.DialogResult = DialogResult.OK; dlg.Close(); };

        dlg.Controls.Add(lbl); dlg.Controls.Add(btnFiles); dlg.Controls.Add(btnFolder); dlg.Controls.Add(btnCancel);
        dlg.CancelButton = btnCancel;
        UI.ThemeDialog(dlg);

        if (dlg.ShowDialog(this) != DialogResult.OK || choice == 0) return;

        string targetPaths = "";
        if (choice == 1)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Laufdateien zum Importieren wählen";
                ofd.Filter = "Unterstützte Dateien (*.minibench;*.zip;*.json)|*.minibench;*.zip;*.json|Minibench-Pakete (*.minibench)|*.minibench|ZIP-Archive (*.zip)|*.zip|Datenbankdateien (*.json)|*.json|Alle Dateien (*.*)|*.*";
                ofd.Multiselect = true;
                if (ofd.ShowDialog(this) != DialogResult.OK || ofd.FileNames.Length == 0) return;
                targetPaths = String.Join("|", ofd.FileNames);
            }
        }
        else if (choice == 2)
        {
            using (FolderBrowserDialog fb = new FolderBrowserDialog())
            {
                fb.Description = "Ausgabeordner eines früheren Laufs (oder einen Ordner mit mehreren davon) wählen";
                if (fb.ShowDialog(this) != DialogResult.OK || string.IsNullOrEmpty(fb.SelectedPath)) return;
                targetPaths = fb.SelectedPath.TrimEnd('\\');
            }
        }
        if (targetPaths.Length == 0) return;

        Cursor = Cursors.WaitCursor;
        List<string> lines = new List<string>();
        try { RunHelper("-ImportOrdner " + Q(targetPaths), out lines); }
        catch (Exception ex) { lines.Add("Import fehlgeschlagen: " + ex.Message); }
        Cursor = Cursors.Default;
        ReloadDb();
        UI.Hinweis(lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Nichts importiert.", "Import", this);
    }
}
