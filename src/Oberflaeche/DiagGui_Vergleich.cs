// Vergleichsdatenbank und Systemvergleich (Teilklasse DiagGui)
public partial class DiagGui
{
    class DbItemComparer : System.Collections.IComparer
    {
        int col;
        bool asc;
        public DbItemComparer(int column, bool ascending) { col = column; asc = ascending; }
        public int Column { get { return col; } }
        public bool Ascending { get { return asc; } }

        public int Compare(object x, object y)
        {
            ListViewItem itemX = x as ListViewItem;
            ListViewItem itemY = y as ListViewItem;
            if (itemX == null || itemY == null) return 0;
            DbEntry dx = itemX.Tag as DbEntry;
            DbEntry dy = itemY.Tag as DbEntry;
            if (dx == null || dy == null) return 0;

            int res = CompareDbEntries(dx, dy, col);
            return asc ? res : -res;
        }
    }

    class DbListView : ListView
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct LVGROUP
        {
            public uint cbSize;
            public uint mask;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszHeader;
            public int cchHeader;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszFooter;
            public int cchFooter;
            public int iGroupId;
            public uint stateMask;
            public uint state;
            public uint uAlign;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszSubtitle;
            public uint cchSubtitle;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszTask;
            public uint cchTask;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszDescriptionTop;
            public uint cchDescriptionTop;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszDescriptionBottom;
            public uint cchDescriptionBottom;
            public int iTitleImage;
            public int iExtendedImage;
            public int iFirstItem;
            public uint cItems;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszSubsetTitle;
            public uint cchSubsetTitle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int left, top, right, bottom; }

        public const int LVM_FIRST = 0x1000;
        public const int LVM_SETGROUPINFO = LVM_FIRST + 147;
        public const int LVM_GETGROUPRECT = LVM_FIRST + 98;
        public const int LVM_GETGROUPSTATE = LVM_FIRST + 92;
        public const uint LVGF_STATE = 0x00000004;
        public const uint LVGS_COLLAPSED = 0x00000001;
        public const uint LVGS_COLLAPSIBLE = 0x00000008;
        public const int LVGGR_LABEL = 2;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref LVGROUP lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref RECT lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        static PropertyInfo groupPropId = typeof(ListViewGroup).GetProperty("ID", BindingFlags.Instance | BindingFlags.NonPublic);

        public event EventHandler GroupStateChanged;

        public DbListView()
        {
            DoubleBuffered = true;
        }

        public static int GetGroupId(ListViewGroup g)
        {
            if (g == null || groupPropId == null) return -1;
            try { return (int)groupPropId.GetValue(g, null); } catch { return -1; }
        }

        public void SetGroupState(ListViewGroup g, bool collapsible, bool collapsed)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;
            if (!IsHandleCreated) { IntPtr h = Handle; }
            int id = GetGroupId(g);
            if (id < 0) return;
            try
            {
                LVGROUP grp = new LVGROUP();
                grp.cbSize = (uint)Marshal.SizeOf(typeof(LVGROUP));
                grp.mask = LVGF_STATE;
                grp.stateMask = LVGS_COLLAPSIBLE | LVGS_COLLAPSED;
                uint state = 0;
                if (collapsible) state |= LVGS_COLLAPSIBLE;
                if (collapsed) state |= LVGS_COLLAPSED;
                grp.state = state;
                SendMessage(Handle, LVM_SETGROUPINFO, id, ref grp);
            }
            catch { }
        }

        public bool IsGroupCollapsed(ListViewGroup g)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return false;
            if (!IsHandleCreated) return false;
            int id = GetGroupId(g);
            if (id < 0) return false;
            try
            {
                int state = SendMessage(Handle, LVM_GETGROUPSTATE, id, (int)LVGS_COLLAPSED);
                return (state & (int)LVGS_COLLAPSED) != 0;
            }
            catch { return false; }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (GroupStateChanged != null) GroupStateChanged(this, EventArgs.Empty);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (GroupStateChanged != null) GroupStateChanged(this, EventArgs.Empty);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // Repaint group headers in Dark Mode with high contrast
            if (m.Msg == 0x000F && UI.IsDark && Groups.Count > 0 && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                PaintGroupHeadersDark();
            }
        }

        void PaintGroupHeadersDark()
        {
            try
            {
                Rectangle client = ClientRectangle;
                using (Graphics g = Graphics.FromHwnd(Handle))
                using (SolidBrush bgBrush = new SolidBrush(BackColor))
                using (Font f = new Font("Segoe UI Semibold", 9.25f))
                {
                    foreach (ListViewGroup grp in Groups)
                    {
                        int id = GetGroupId(grp);
                        if (id < 0) continue;
                        RECT rc = new RECT();
                        rc.top = LVGGR_LABEL;
                        if (SendMessage(Handle, LVM_GETGROUPRECT, id, ref rc) != 0)
                        {
                            if (rc.right > rc.left && rc.bottom > rc.top)
                            {
                                if (rc.bottom <= client.Top || rc.top >= client.Bottom) continue;
                                Rectangle r = new Rectangle(rc.left - 2, rc.top, (rc.right - rc.left) + 6, (rc.bottom - rc.top) + 2);
                                r.Intersect(client);
                                if (r.Width > 0 && r.Height > 0)
                                {
                                    g.FillRectangle(bgBrush, r);
                                    Color textCol = (grp.Name == "REF") ? UI.Text : UI.Accent;
                                    TextRenderer.DrawText(g, grp.Header, f, r, textCol, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }

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
    Button btnCompare, btnDashboard, btnVerlauf, btnDelete, btnOpenReport, btnExport, btnSetRef, btnToggleGroups;

    void OnDbColumnClick(object sender, ColumnClickEventArgs e)
    {
        if (e.Column == dbSortCol)
        {
            dbSortAsc = !dbSortAsc;
        }
        else
        {
            dbSortCol = e.Column;
            dbSortAsc = (e.Column == 0 || e.Column == 3 || e.Column == 4 || e.Column == 5);
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
        ListViewItem lvi = null;
        if (lvDb != null && lvDb.SelectedItems.Count == 1) { lvi = lvDb.SelectedItems[0]; target = (DbEntry)lvi.Tag; }
        else if (lvDb != null && lvDb.CheckedItems.Count == 1) { lvi = lvDb.CheckedItems[0]; target = (DbEntry)lvi.Tag; }
        if (target == null || target.IsReference) return;

        bool isMasterGroup = lvi != null && (lvi.Text.StartsWith("\u25B6 ") || lvi.Text.StartsWith("\u25BC "));
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
        if (!res.StartsWith("1")) MessageBox.Show(this, "Der Name konnte nicht geändert werden.\r\n\r\n" + String.Join("\r\n", lines.ToArray()), "Systemnamen bearbeiten", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    Control BuildDbPage()
    {
        Panel f = new Panel(); f.Dock = DockStyle.Fill; f.BackColor = UI.Panel;
        FlowLayoutPanel top = Page("Datenbank", "Jeder Lauf wird als System gespeichert. Hier lassen sich bereits geprüfte Systeme ohne neuen Benchmark im interaktiven Multi-System-Dashboard gegenüberstellen: Systeme anhaken und \"Im Dashboard vergleichen\" klicken. Ältere Ausgabeordner lassen sich importieren. Ein Klick auf die Spaltenköpfe sortiert die Einträge.", -1);
        top.Dock = DockStyle.Top;
        top.AutoSize = true;
        lblDbPath = Lbl(NasAblage.AblageText(dataDir), 9f, false, UI.Muted); lblDbPath.Margin = new Padding(UI.S(4), 0, UI.S(4), UI.S(4)); top.Controls.Add(lblDbPath);

        FlowLayoutPanel topTools = Row();
        topTools.Margin = new Padding(UI.S(4), 0, UI.S(4), UI.S(8));
        Button imp = UI.Secondary("Importieren ..."); imp.Margin = new Padding(0); imp.Click += delegate { ImportRuns(); };
        Tip(imp, "Importiert Durchläufe aus .minibench-Paketen, .zip-Archiven, .json-Dateien oder Ausgabeordnern früherer Läufe in die Datenbank samt Bericht.");
        btnDbClean = UI.Secondary("Aufräumen ..."); btnDbClean.Margin = new Padding(UI.S(8), 0, 0, 0); btnDbClean.Click += delegate { CleanData(); }; btnDbClean.Enabled = dataDir.Length > 0;
        Tip(btnDbClean, "Räumt nicht vergleichbare oder abgebrochene Läufe auf und verschiebt sie ins Archiv.");
        Button rel = UI.Secondary("Aktualisieren"); rel.Margin = new Padding(UI.S(8), 0, 0, 0); rel.Click += delegate { RefreshAndSync(); };
        Tip(rel, "Liest Datenbank und Referenz neu ein. Ist ein Netzlaufwerk eingerichtet und erreichbar, werden Berichte, Datenbank und Änderungsprotokolle vorher in beide Richtungen abgeglichen.");
        Button open = UI.Secondary("Datenordner"); open.Margin = new Padding(UI.S(8), 0, 0, 0); open.Click += delegate { if (dataDir.Length > 0) OpenShell(dataDir); };
        Tip(open, "Öffnet den Datenordner (Berichte, Datenbank, Tools, Archiv).");
        Button btnNas = UI.Secondary("Netzlaufwerk ..."); btnNas.Margin = new Padding(UI.S(8), 0, 0, 0); btnNas.Click += delegate { ShowConnectNasDialog(); };
        Tip(btnNas, "Richtet ein Netzlaufwerk (NAS) als Spiegel der Berichte und der Datenbank ein oder entfernt es. Gearbeitet wird immer auf dem Stick.");
        btnToggleGroups = UI.Secondary("Alle aufklappen"); btnToggleGroups.Margin = new Padding(UI.S(8), 0, 0, 0); btnToggleGroups.Click += delegate { ToggleAllGroups(); };
        Tip(btnToggleGroups, "Klappt alle GerÃ¤tegruppen in der Datenbank ein oder aus.");
        topTools.Controls.Add(imp); topTools.Controls.Add(btnToggleGroups); topTools.Controls.Add(btnDbClean); topTools.Controls.Add(rel); topTools.Controls.Add(open); topTools.Controls.Add(btnNas);
        top.Controls.Add(topTools);

        FlowLayoutPanel bottom = new FlowLayoutPanel(); bottom.FlowDirection = FlowDirection.TopDown; bottom.WrapContents = false; bottom.AutoSize = true; bottom.BackColor = UI.Panel; bottom.Dock = DockStyle.Bottom;
        Label hint = Lbl("Doppelklick öffnet den Bericht des Laufs. Graue Einträge enthalten keine Benchmark-Werte. Klick auf Spaltenkopf sortiert die Tabelle.", 8.75f, false, UI.Muted); hint.Margin = new Padding(UI.S(4), UI.S(4), UI.S(4), 0); bottom.Controls.Add(hint);
        FlowLayoutPanel b = Row(); b.Margin = new Padding(UI.S(4), UI.S(10), 0, 0);
        // Hinweis: Der statische Bericht "Vergleichen (Klassisch)" ist ab v3.32 vollständig durch das interaktive Dashboard abgelöst
        btnDashboard = UI.Primary("Interaktives Dashboard"); btnDashboard.Margin = new Padding(0); btnDashboard.Padding = new Padding(UI.S(14), UI.S(3), UI.S(14), UI.S(3)); btnDashboard.Font = new Font("Segoe UI Semibold", 9.75f);
        btnDashboard.Click += delegate { OpenDashboard(); };
        Tip(btnDashboard, "Öffnet das interaktive HTML5-Dashboard für detaillierten System- und Benchmark-Vergleich.");
        btnVerlauf = UI.Secondary("Verlauf"); btnVerlauf.Margin = new Padding(UI.S(8), 0, 0, 0);
        btnVerlauf.Click += delegate { OpenDeviceHistory(); };
        btnVerlauf.Enabled = false;
        Tip(btnVerlauf, "Öffnet das interaktive Dashboard im Verlaufsmodus für alle Läufe des ausgewählten Geräts.");
        btnOpenReport = UI.Secondary("Bericht öffnen"); btnOpenReport.Margin = new Padding(UI.S(8), 0, 0, 0);
        btnOpenReport.Click += delegate {
            DbEntry target = null;
            if (lvDb != null && lvDb.SelectedItems.Count > 0) target = (DbEntry)lvDb.SelectedItems[0].Tag;
            else if (lvDb != null && lvDb.CheckedItems.Count == 1) target = (DbEntry)lvDb.CheckedItems[0].Tag;
            if (target != null) OpenEntry(target);
        };
        btnOpenReport.Enabled = false;
        Tip(btnOpenReport, "Öffnet den vollständigen HTML-Diagnosebericht des ausgewählten Systems.");
        btnSetRef = UI.Secondary("Als Referenz ..."); btnSetRef.Margin = new Padding(UI.S(8), 0, 0, 0);
        btnSetRef.Click += delegate { ShowSetRefMenu(); };
        btnSetRef.Enabled = false;
        Tip(btnSetRef, "Legt das ausgewählte System als dauerhafte Vergleichsreferenz (Referenz.json) fest oder öffnet das Dashboard mit diesem System als Basis.");
        btnRename = UI.Secondary("Name ändern ..."); btnRename.Margin = new Padding(UI.S(8), 0, 0, 0);
        btnRename.Click += delegate { RenameSelectedEntry(); };
        btnRename.Enabled = false;
        Tip(btnRename, "Ändert den Anzeigenamen des ausgewählten Systems (z. B. Vor Reinigung) und benennt Datenbankdatei und Berichtsordner passend um. Rechnername und Hardware-Erkennung bleiben.");
        btnExport = UI.Secondary("Lauf exportieren ..."); btnExport.Margin = new Padding(UI.S(8), 0, 0, 0);
        btnExport.Click += delegate { ExportSelected(); };
        btnExport.Enabled = false;
        Tip(btnExport, "Exportiert den ausgewählten Lauf als eigenständiges .minibench-Austauschpaket mit Datenbankeintrag und Bericht.");
        btnDelete = UI.Secondary("Entfernen"); btnDelete.Margin = new Padding(UI.S(8), 0, 0, 0); btnDelete.Click += delegate { DeleteSelected(); };
        Tip(btnDelete, "Verschiebt die angehakten Läufe samt Berichtsordner nach Minibench-Daten\\Archiv\\Entfernt. Beim nächsten Abgleich verschwinden sie auch vom Netzlaufwerk.");
        b.Controls.Add(btnDashboard); b.Controls.Add(btnVerlauf); b.Controls.Add(btnOpenReport); b.Controls.Add(btnSetRef); b.Controls.Add(btnRename); b.Controls.Add(btnExport); b.Controls.Add(btnDelete); bottom.Controls.Add(b);
        lblDbClean = Lbl(DatenpflegeInfo.Length > 0 ? DatenpflegeInfo : "Lasttests vor v2.67 (nicht vergleichbar), abgebrochene und kurze Läufe verschiebt die Datenpflege beim Start nach Minibench-Daten\\Archiv.", 8.75f, false, UI.Muted);
        lblDbClean.Margin = new Padding(UI.S(4), UI.S(8), UI.S(4), 0); bottom.Controls.Add(lblDbClean);
        bottom.Resize += delegate { lblDbClean.MaximumSize = new Size(Math.Max(UI.S(200), bottom.ClientSize.Width - UI.S(10)), 0); };

        DbListView dlv = new DbListView();
        dlv.GroupStateChanged += delegate { UpdateToggleGroupsButton(); };
        lvDb = dlv; lvDb.View = View.Details; lvDb.FullRowSelect = true; lvDb.CheckBoxes = true; lvDb.Dock = DockStyle.Fill; lvDb.BorderStyle = BorderStyle.FixedSingle; lvDb.HideSelection = false; lvDb.ShowItemToolTips = true;
        EnableDarkListView(lvDb);
        string[] cols = new string[] { "System", "Datum", "Gesamt", "Prozessor", "Grafik", "Arbeitsspeicher", "CPU Mehrkern", "RAM Lesen", "GPU", "Befunde K/W/I" };
        int[] w = new int[] { UI.S(135), UI.S(95), UI.S(65), UI.S(125), UI.S(115), UI.S(105), UI.S(80), UI.S(70), UI.S(70), UI.S(62) };
        for (int i = 0; i < cols.Length; i++) lvDb.Columns.Add(cols[i], w[i]);
        lvDb.Resize += delegate {
            int rem = lvDb.ClientSize.Width;
            for (int i = 0; i < lvDb.Columns.Count - 1; i++) rem -= lvDb.Columns[i].Width;
            if (rem > UI.S(60)) lvDb.Columns[lvDb.Columns.Count - 1].Width = rem - 2;
        };
        lvDb.DoubleClick += delegate {
            if (lvDb.SelectedItems.Count == 0) return;
            ListViewItem selIt = lvDb.SelectedItems[0];
            Point pt = lvDb.PointToClient(Cursor.Position);
            ListViewHitTestInfo hit = lvDb.HitTest(pt);
            if (hit.Item != null && (hit.Item.Text.StartsWith("\u25B6 ") || hit.Item.Text.StartsWith("\u25BC ")) && pt.X <= hit.Item.Bounds.Left + UI.S(45))
            {
                DbEntry target = hit.Item.Tag as DbEntry;
                if (target != null)
                {
                    string k = target.IsReference ? "REF" : (String.IsNullOrEmpty(target.GeraetKey) ? "PC:" + (target.Computer ?? "").ToUpperInvariant() : target.GeraetKey);
                    ToggleGroup(k);
                    return;
                }
            }
            OpenEntry((DbEntry)selIt.Tag);
        };
        lvDb.MouseClick += delegate(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) return;
            ListViewHitTestInfo hit = lvDb.HitTest(e.Location);
            if (hit.Item != null && hit.Location != ListViewHitTestLocations.StateImage)
            {
                if ((hit.Item.Text.StartsWith("\u25B6 ") || hit.Item.Text.StartsWith("\u25BC ")) && e.X <= hit.Item.Bounds.Left + UI.S(45))
                {
                    DbEntry target = hit.Item.Tag as DbEntry;
                    if (target != null)
                    {
                        string k = target.IsReference ? "REF" : (String.IsNullOrEmpty(target.GeraetKey) ? "PC:" + (target.Computer ?? "").ToUpperInvariant() : target.GeraetKey);
                        ToggleGroup(k);
                    }
                }
            }
        };
        lvDb.KeyDown += delegate(object sender, KeyEventArgs e) {
            if ((e.KeyCode == Keys.Right || e.KeyCode == Keys.Left) && lvDb.SelectedItems.Count == 1)
            {
                ListViewItem it = lvDb.SelectedItems[0];
                DbEntry target = it.Tag as DbEntry;
                if (target != null)
                {
                    string k = target.IsReference ? "REF" : (String.IsNullOrEmpty(target.GeraetKey) ? "PC:" + (target.Computer ?? "").ToUpperInvariant() : target.GeraetKey);
                    if (e.KeyCode == Keys.Right && it.Text.StartsWith("\u25B6 ")) { ToggleGroup(k); e.Handled = true; }
                    else if (e.KeyCode == Keys.Left && it.Text.StartsWith("\u25BC ")) { ToggleGroup(k); e.Handled = true; }
                }
            }
            else if (e.KeyCode == Keys.F2 && lvDb.SelectedItems.Count == 1)
            {
                RenameSelectedEntry();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && lvDb.SelectedItems.Count > 0)
            {
                OpenEntry((DbEntry)lvDb.SelectedItems[0].Tag);
                e.Handled = true;
            }
        };
        lvDb.ItemChecked += delegate { UpdateDbButtons(); };
        lvDb.SelectedIndexChanged += delegate { UpdateDbButtons(); };
        lvDb.ColumnClick += OnDbColumnClick;
        Tip(lvDb, "Datenbank aller gespeicherten Systeme. Ein Klick auf die Spaltenköpfe sortiert nach Datum, Gesamtwertung, CPU oder GPU.");

        ContextMenu cm = new ContextMenu();
        MenuItem miDashboard = new MenuItem("Im Dashboard vergleichen", delegate { OpenDashboard(); });
        MenuItem miHistory = new MenuItem("Geräte-Verlauf im Dashboard öffnen", delegate { OpenDeviceHistory(); });
        MenuItem miSetRef = new MenuItem("Als Referenz festlegen (persistent)", delegate { SetSelectedAsReference(); });
        MenuItem miDashboardBase = new MenuItem("Als Basis im Dashboard öffnen (temporär)", delegate { OpenDashboardWithBase(); });
        MenuItem miSep1 = new MenuItem("-");
        MenuItem miExpandAll = new MenuItem("Alle aufklappen", delegate { SetAllGroupsCollapsed(false); });
        MenuItem miCollapseAll = new MenuItem("Alle einklappen", delegate { SetAllGroupsCollapsed(true); });
        MenuItem miSep3 = new MenuItem("-");
        MenuItem miRename = new MenuItem("Name ändern ...", delegate { RenameSelectedEntry(); });
        MenuItem miOpen = new MenuItem("Bericht öffnen", delegate { if (lvDb.SelectedItems.Count > 0) OpenEntry((DbEntry)lvDb.SelectedItems[0].Tag); });
        MenuItem miExport = new MenuItem("Lauf exportieren ...", delegate { ExportSelected(); });
        MenuItem miSep2 = new MenuItem("-");
        MenuItem miDelete = new MenuItem("Aus Datenbank entfernen", delegate { DeleteSelected(); });
        cm.MenuItems.Add(miDashboard);
        cm.MenuItems.Add(miHistory);
        cm.MenuItems.Add(miSetRef);
        cm.MenuItems.Add(miDashboardBase);
        cm.MenuItems.Add(miSep1);
        cm.MenuItems.Add(miExpandAll);
        cm.MenuItems.Add(miCollapseAll);
        cm.MenuItems.Add(miSep3);
        cm.MenuItems.Add(miRename);
        cm.MenuItems.Add(miOpen);
        cm.MenuItems.Add(miExport);
        cm.MenuItems.Add(miSep2);
        cm.MenuItems.Add(miDelete);
        cm.Popup += delegate {
            List<string> expandable = GetExpandableKeys();
            miExpandAll.Enabled = expandable.Count > 0 && expandedGroups.Count < expandable.Count;
            miCollapseAll.Enabled = expandable.Count > 0 && expandedGroups.Count > 0;
            int selN = lvDb.SelectedItems.Count;
            int chkN = CheckedEntries().Count;
            miDashboard.Enabled = chkN >= 1 || selN >= 1;
            miHistory.Enabled = (GetSelectedDeviceTarget() != null);
            miSetRef.Enabled = selN == 1 || (selN == 0 && chkN == 1);
            miDashboardBase.Enabled = selN == 1 || (selN == 0 && chkN == 1);
            DbEntry selTarget = null;
            if (selN == 1) selTarget = lvDb.SelectedItems[0].Tag as DbEntry;
            else if (chkN == 1) selTarget = lvDb.CheckedItems[0].Tag as DbEntry;
            miRename.Enabled = (selTarget != null && !selTarget.IsReference && (selN == 1 || (selN == 0 && chkN == 1)));
            miOpen.Enabled = selN == 1;
            miExport.Enabled = selN == 1 || (selN == 0 && chkN == 1);
            miDelete.Enabled = chkN >= 1 || selN >= 1;
        };
        lvDb.ContextMenu = cm;

        f.Controls.Add(lvDb);
        f.Controls.Add(bottom);
        f.Controls.Add(top);
        top.SendToBack();
        bottom.SendToBack();

        FillDbList();
        return f;
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

    ListViewItem CreateDbItem(DbEntry e, string systemText)
    {
        string gesamt = e.OverallScore > 0 ? Num(e.OverallScore, "N0") : "";
        string gpu = e.Get("GPU|REND") > 0 ? Num(e.Get("GPU|REND"), "N0") + " FPS" : (e.Get("GPU|VMB") > 0 ? Num(e.Get("GPU|VMB"), "N0") + " GB/s" : "");
        ListViewItem it = new ListViewItem(new string[] {
            systemText,
            ShortDate(e.Datum),
            gesamt,
            e.Cpu,
            e.Gpu,
            e.Ram,
            Num(e.Get("CPU|MT"), "N0"),
            Num(e.Get("RAM|Lesen"), "N1") + (e.Get("RAM|Lesen") > 0 ? " GB/s" : ""),
            gpu,
            e.Befunde
        });
        it.Tag = e;
        it.ToolTipText = e.DisplayName + (e.Disk.Length > 0 ? "\r\n" + e.Disk : "") + (e.HasBench ? "" : "\r\n(ohne Benchmark-Werte)");
        if (!e.HasBench) it.ForeColor = UI.Muted;
        return it;
    }

    void FillDbList()
    {
        if (lvDb == null) return;

        HashSet<string> checkedPaths = new HashSet<string>();
        foreach (ListViewItem it in lvDb.CheckedItems)
        {
            DbEntry e = it.Tag as DbEntry;
            if (e != null && !String.IsNullOrEmpty(e.Path)) checkedPaths.Add(e.Path);
        }
        string selectedPath = null;
        if (lvDb.SelectedItems.Count > 0)
        {
            DbEntry sel = lvDb.SelectedItems[0].Tag as DbEntry;
            if (sel != null && !String.IsNullOrEmpty(sel.Path)) selectedPath = sel.Path;
        }

        lvDb.BeginUpdate();
        lvDb.ListViewItemSorter = null;
        lvDb.Items.Clear();
        lvDb.Groups.Clear();
        lvDb.ShowGroups = false;

        Dictionary<string, List<DbEntry>> grpMap = new Dictionary<string, List<DbEntry>>();
        List<string> orderedKeys = new List<string>();

        foreach (DbEntry e in db)
        {
            string k = e.IsReference ? "REF" : (String.IsNullOrEmpty(e.GeraetKey) ? "PC:" + (e.Computer ?? "").ToUpperInvariant() : e.GeraetKey);
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

        if (grpMap.ContainsKey("REF")) orderedKeys.Add("REF");

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

                if (entries.Count == 1)
                {
                    ListViewItem it = CreateDbItem(entries[0], "  " + entries[0].DisplayName);
                    if (checkedPaths.Contains(entries[0].Path)) it.Checked = true;
                    if (selectedPath == entries[0].Path) it.Selected = true;
                    lvDb.Items.Add(it);
                }
                else
                {
                    bool expanded = expandedGroups.Contains("REF");
                    string masterTitle = (expanded ? "\u25BC " : "\u25B6 ") + "Referenz-Systeme (" + entries.Count + ")";
                    ListViewItem masterIt = CreateDbItem(refRep, masterTitle);
                    if (checkedPaths.Contains(refRep.Path)) masterIt.Checked = true;
                    if (selectedPath == refRep.Path) masterIt.Selected = true;
                    lvDb.Items.Add(masterIt);

                    if (expanded)
                    {
                        foreach (DbEntry e in entries)
                        {
                            ListViewItem childIt = CreateDbItem(e, "    " + e.DisplayName);
                            if (checkedPaths.Contains(e.Path)) childIt.Checked = true;
                            if (selectedPath == e.Path) childIt.Selected = true;
                            lvDb.Items.Add(childIt);
                        }
                    }
                }
            }
            else
            {
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

                if (entries.Count == 1)
                {
                    ListViewItem it = CreateDbItem(newest, "  " + devName);
                    if (checkedPaths.Contains(newest.Path)) it.Checked = true;
                    if (selectedPath == newest.Path) it.Selected = true;
                    lvDb.Items.Add(it);
                }
                else
                {
                    bool expanded = expandedGroups.Contains(k);
                    string masterTitle = (expanded ? "\u25BC " : "\u25B6 ") + devName + " (" + entries.Count + ")";
                    ListViewItem masterIt = CreateDbItem(newest, masterTitle);
                    if (checkedPaths.Contains(newest.Path)) masterIt.Checked = true;
                    if (selectedPath == newest.Path) masterIt.Selected = true;
                    lvDb.Items.Add(masterIt);

                    if (expanded)
                    {
                        for (int i = 1; i < entries.Count; i++)
                        {
                            DbEntry e = entries[i];
                            ListViewItem childIt = CreateDbItem(e, "    " + e.DisplayName);
                            if (checkedPaths.Contains(e.Path)) childIt.Checked = true;
                            if (selectedPath == e.Path) childIt.Selected = true;
                            lvDb.Items.Add(childIt);
                        }
                    }
                }
            }
        }

        lvDb.EndUpdate();
        UpdateToggleGroupsButton();
        if (navDb != null) { navDb.Desc = db.Count + " Systeme, vergleichen und verwalten"; navDb.Invalidate(); }
        UpdateDbButtons();
    }

    List<DbEntry> CheckedEntries()
    {
        List<DbEntry> l = new List<DbEntry>();
        if (lvDb != null) foreach (ListViewItem it in lvDb.CheckedItems) l.Add((DbEntry)it.Tag);
        return l;
    }

    void UpdateDbButtons()
    {
        if (btnDashboard == null && btnCompare == null) return;
        int n = CheckedEntries().Count;
        if (btnDelete != null) btnDelete.Enabled = n >= 1;
        if (btnCompare != null)
        {
            btnCompare.Enabled = n >= 2;
            btnCompare.Text = n >= 2 ? n + " Systeme vergleichen (Klassisch)" : "Vergleichen (Klassisch)";
        }
        if (btnDashboard != null)
        {
            btnDashboard.Text = n >= 2 ? n + " Systeme im Dashboard vergleichen" : (n == 1 ? "1 System im Dashboard anzeigen" : "Interaktives Dashboard");
        }
        if (btnVerlauf != null)
        {
            btnVerlauf.Enabled = (GetSelectedDeviceTarget() != null);
        }
        if (btnRename != null)
        {
            int selCount = (lvDb != null ? lvDb.SelectedItems.Count : 0);
            DbEntry selTarget = null;
            if (lvDb != null && lvDb.SelectedItems.Count == 1) selTarget = lvDb.SelectedItems[0].Tag as DbEntry;
            else if (lvDb != null && lvDb.CheckedItems.Count == 1) selTarget = lvDb.CheckedItems[0].Tag as DbEntry;
            btnRename.Enabled = (selTarget != null && !selTarget.IsReference && (selCount == 1 || (selCount == 0 && n == 1)));
        }
        if (btnOpenReport != null)
        {
            int selCount = (lvDb != null ? lvDb.SelectedItems.Count : 0);
            btnOpenReport.Enabled = (selCount == 1 || (selCount == 0 && n == 1));
        }
        if (btnExport != null)
        {
            int selCount = (lvDb != null ? lvDb.SelectedItems.Count : 0);
            btnExport.Enabled = (selCount == 1 || (selCount == 0 && n == 1));
        }
        if (btnSetRef != null)
        {
            int selCount = (lvDb != null ? lvDb.SelectedItems.Count : 0);
            btnSetRef.Enabled = (selCount == 1 || (selCount == 0 && n == 1));
        }
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
        if (lvDb != null && lvDb.SelectedItems.Count > 0) target = (DbEntry)lvDb.SelectedItems[0].Tag;
        else if (lvDb != null && lvDb.CheckedItems.Count == 1) target = (DbEntry)lvDb.CheckedItems[0].Tag;
        if (target == null || !target.HasBench)
        {
            MessageBox.Show(this, "Bitte wählen Sie ein System mit Benchmark-Werten aus.", "Referenz festlegen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string msg = String.Format("Möchten Sie das System \"{0}\" ({1})\r\nals dauerhafte Vergleichsreferenz (100 %) festlegen?\r\n\r\nDie Werte werden in Minibench-Daten\\Referenz.json gespeichert.", target.DisplayName, target.Datum);
        if (MessageBox.Show(this, msg, "Referenz festlegen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        try
        {
            if (String.IsNullOrEmpty(dataDir) || !Directory.Exists(dataDir))
            {
                MessageBox.Show(this, "Der Datenordner ist nicht verfügbar.", "Referenz festlegen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string dest = Path.Combine(dataDir, "Referenz.json");
            if (File.Exists(target.Path))
            {
                string raw = File.ReadAllText(target.Path, Encoding.UTF8);
                File.WriteAllText(dest, raw, new UTF8Encoding(false));
                MessageBox.Show(this, "Das System \"" + target.DisplayName + "\" wurde als Referenz gespeichert.", "Referenz festgelegt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ReloadDb();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Fehler beim Speichern der Referenz: " + ex.Message, "Referenz festlegen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    void OpenDashboardWithBase()
    {
        DbEntry target = null;
        if (lvDb != null && lvDb.SelectedItems.Count > 0) target = (DbEntry)lvDb.SelectedItems[0].Tag;
        else if (lvDb != null && lvDb.CheckedItems.Count == 1) target = (DbEntry)lvDb.CheckedItems[0].Tag;
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
            else MessageBox.Show(this, "Das Dashboard konnte nicht geöffnet werden.\r\n\r\n" + String.Join("\r\n", lines.ToArray()), "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (lvDb.SelectedItems.Count > 0)
            {
                foreach (ListViewItem it in lvDb.SelectedItems)
                {
                    DbEntry e = it.Tag as DbEntry;
                    if (e != null && !e.IsReference) targets.Add(e);
                }
            }
            else if (lvDb.CheckedItems.Count > 0)
            {
                foreach (ListViewItem it in lvDb.CheckedItems)
                {
                    DbEntry e = it.Tag as DbEntry;
                    if (e != null && !e.IsReference) targets.Add(e);
                }
            }
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
            else MessageBox.Show(this, "Das Dashboard konnte nicht geöffnet werden.\r\n\r\n" + String.Join("\r\n", lines.ToArray()), "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    void OpenDashboard()
    {
        Cursor = Cursors.WaitCursor;
        List<string> lines = new List<string>(); string res = "";
        List<DbEntry> sel = CheckedEntries();
        if (sel.Count == 0 && lvDb != null && lvDb.SelectedItems.Count > 0)
        {
            foreach (ListViewItem it in lvDb.SelectedItems) sel.Add((DbEntry)it.Tag);
        }
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
            else MessageBox.Show(this, "Das Dashboard konnte nicht geöffnet werden.\r\n\r\n" + String.Join("\r\n", lines.ToArray()), "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ab 3.54: Eintrag und Berichtsordner ins Archiv (Minibench-Daten\Archiv\Entfernt), über den Arbeitsprozess
    void DeleteSelected()
    {
        List<DbEntry> sel = CheckedEntries();
        if (sel.Count == 0) return;
        StringBuilder sb = new StringBuilder();
        foreach (DbEntry e in sel) sb.AppendLine("·  " + e.DisplayName + "  " + e.Datum);
        string nas, ben; bool mitNas = NasAblage.LeseKonfig(dataDir, out nas, out ben);
        string msg = "Diese Läufe entfernen?\r\n\r\n" + sb.ToString() + "\r\nDatenbankeintrag und Berichtsordner werden nach Minibench-Daten\\Archiv\\Entfernt verschoben und lassen sich von dort zurückholen."
            + (mitNas ? " Auf dem Netzlaufwerk verschwinden sie beim nächsten Aktualisieren (dort ebenfalls ins Archiv)." : "");
        if (MessageBox.Show(this, msg, "Läufe entfernen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        List<string> paths = new List<string>();
        foreach (DbEntry e in sel) paths.Add(e.Path);
        List<string> lines;
        string res = RunHelperPumped("-Entfernen " + Q(String.Join("|", paths.ToArray())), out lines, 300);
        ReloadDb();
        string[] r = res.Split('|');
        if (r.Length < 2 || r[1] != "0") MessageBox.Show(this, lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Keine Rückmeldung vom Arbeitsprozess.", "Läufe entfernen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    // Aktualisieren (ab 3.54): mit eingerichtetem und erreichbarem Netzlaufwerk erst abgleichen, dann neu einlesen.
    // Ohne Netzlaufwerk oder unterwegs nur neu einlesen; alles auf dem Stick bleibt nutzbar.
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
        if (fehler.Length > 0) { ReloadDb(); MessageBox.Show(this, "Der gespeicherte Pfad des Netzlaufwerks ist nicht verwendbar: " + fehler + "\r\n\r\nBitte unter \"Netzlaufwerk ...\" korrigieren.", "Aktualisieren", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
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
                if (!da && err.Length > 0) MessageBox.Show(this, err, "Netzlaufwerk verbinden", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (MessageBox.Show(this, frage, "Abgleich mit dem Netzlaufwerk", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
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
            DialogResult wahl = MessageBox.Show(this, frage, "Abgleich mit dem Netzlaufwerk", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
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
        if (besonders) MessageBox.Show(this, kurz + (lines.Count > 0 ? "\r\n\r\n" + String.Join("\r\n", lines.ToArray()) : "") + "\r\n\r\nProtokoll: Minibench-Daten\\Abgleich\\Abgleich.log", "Abgleich mit dem Netzlaufwerk", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    void CleanData()
    {
        if (dataDir.Length == 0) return;
        if (MessageBox.Show(this, "Lasttests vor v2.67 (andere Last, nicht vergleichbar), abgebrochene Läufe ohne Bericht und kurze Läufe (Funktionstest, von Hand nach wenigen Sekunden beendeter Lasttest, Lauf ohne Messwert) sowie ungenutzte Teile von smartmontools nach Minibench-Daten\\Archiv verschieben?\r\n\r\nGelöscht wird nichts außer älteren Übersetzungen im Cache. Das Protokoll steht in Archiv\\Datenpflege.log.", "Daten aufräumen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        Cursor = Cursors.WaitCursor;
        List<string> lines = new List<string>(); string res = "";
        try { res = RunHelper("-Datenpflege", out lines); }
        catch (Exception ex) { lines.Add("Datenpflege fehlgeschlagen: " + ex.Message); }
        Cursor = Cursors.Default;
        ReloadDb();
        if (res.Length > 0) SetText(lblDbClean, "Datenpflege: " + res.Split('|')[0]);
        MessageBox.Show(this, lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Nichts zu archivieren.", "Daten aufräumen", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void ExportSelected()
    {
        DbEntry target = null;
        if (lvDb != null && lvDb.SelectedItems.Count == 1) target = (DbEntry)lvDb.SelectedItems[0].Tag;
        else if (lvDb != null && lvDb.CheckedItems.Count == 1) target = (DbEntry)lvDb.CheckedItems[0].Tag;
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
            MessageBox.Show(this, lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Export abgeschlossen.", "Lauf exportieren", MessageBoxButtons.OK, res.StartsWith("1") ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
    }

    void ImportRuns()
    {
        if (dbDir.Length == 0) { MessageBox.Show(this, "Kein Datenordner verfügbar.", "Leos Minibench"); return; }
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
        MessageBox.Show(this, lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Nichts importiert.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
