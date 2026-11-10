// Seite Wartung (Teilklasse DiagGui)
public partial class DiagGui
{
    string[] repairGroupOrder = new string[] {
        "Systemdateien und Komponentenspeicher",
        "Bereinigung und Speicherplatz",
        "Windows Update, Netzwerk und Zeit",
        "Dienste, Geräte und Energie"
    };

    string GetRepairGroup(int i)
    {
        if (i >= 0 && i < repGroup.Length && !string.IsNullOrEmpty(repGroup[i])) return repGroup[i];
        if (i >= 0 && i < repKeys.Length)
        {
            string k = repKeys[i];
            if (k == "DismRestore" || k == "Sfc" || k == "Komponentenbereinigung" || k == "Dateisystem") return "Systemdateien und Komponentenspeicher";
            if (k == "Temp" || k == "Datentraegerbereinigung" || k == "UpdateDownloads" || k == "ShaderCache" || k == "Prefetch" || k == "PaketCache" || k == "Absturzabbilder" || k == "Wiederherstellungspunkte") return "Bereinigung und Speicherplatz";
            if (k == "WindowsUpdate" || k == "Netzwerk" || k == "Zeit") return "Windows Update, Netzwerk und Zeit";
            if (k == "WMI" || k == "Leistungszaehler" || k == "Leerlaufaufgaben" || k == "Druck" || k == "Geraete" || k == "Schnellstart" || k == "Energieplaene") return "Dienste, Geräte und Energie";
        }
        return "Dienste, Geräte und Energie";
    }

    // ab v3.64: Risikostufe und Neustart als Etiketten rechts in einer Spalte, Befehle in der Detailzeile, Legende oben,
    // Warnung ohne Wiederherstellungspunkt bei gewählten Eingriffen
    Label lblRestoreWarn;
    Button btnRepAll;

    Control BuildRepairPage()
    {
        FlowLayoutPanel f = Page("Wartung", "Führt die gewählten Wartungs- und Reparaturaufgaben nacheinander aus. Vorher wird auf Wunsch ein Wiederherstellungspunkt angelegt. Aufgaben mit dem Etikett Neustart werden erst danach vollständig wirksam.", 4);

        repChk = new CheckBox[repKeys.Length];
        ToggleSwitch[] localRepToggles = new ToggleSwitch[repKeys.Length];
        for (int i = 0; i < repKeys.Length; i++)
        {
            int idx = i;
            repChk[i] = Chk(SplitDetail(repText[i])[0], repDefault[i]);
            localRepToggles[i] = new ToggleSwitch();
            localRepToggles[i].Checked = repDefault[i];
            localRepToggles[i].CheckedChanged += delegate {
                if (repChk[idx].Checked != localRepToggles[idx].Checked) repChk[idx].Checked = localRepToggles[idx].Checked;
            };
            repChk[i].CheckedChanged += delegate {
                if (localRepToggles[idx].Checked != repChk[idx].Checked) localRepToggles[idx].Checked = repChk[idx].Checked;
                UpdateRepairCounts(); UpdateRestoreWarn(); UpdateSummary();
            };
        }

        TableLayoutPanel md = new TableLayoutPanel();
        md.Width = UI.S(UI.Gruppenbreite);
        md.AutoSize = true;
        md.ColumnCount = 2;
        md.RowCount = 1;
        md.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(240)));
        md.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        md.Margin = new Padding(UI.S(4), UI.S(2), 0, UI.S(8));

        FlowLayoutPanel leftCol = new FlowLayoutPanel();
        leftCol.Dock = DockStyle.Fill;
        leftCol.AutoSize = true;
        leftCol.FlowDirection = FlowDirection.TopDown;
        leftCol.WrapContents = false;
        leftCol.Margin = new Padding(0);

        repDetailCard = new CardPanel();
        repDetailCard.Dock = DockStyle.Fill;
        repDetailCard.AutoSize = true;
        repDetailCard.Padding = new Padding(UI.S(14));
        repDetailCard.Margin = new Padding(UI.S(8), 0, 0, 0);

        TableLayoutPanel cardContent = new TableLayoutPanel();
        cardContent.Dock = DockStyle.Fill;
        cardContent.AutoSize = true;
        cardContent.ColumnCount = 1;

        TableLayoutPanel headRow = new TableLayoutPanel();
        headRow.Dock = DockStyle.Top;
        headRow.AutoSize = true;
        headRow.ColumnCount = 2;
        headRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        headRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        FlowLayoutPanel titleBox = new FlowLayoutPanel();
        titleBox.AutoSize = true;
        titleBox.FlowDirection = FlowDirection.TopDown;
        titleBox.WrapContents = false;
        lblRepCatTitle = Lbl("", 11f, true, UI.Text);
        lblRepCatDesc = Lbl("", UI.SchriftHinweis, false, UI.Muted);
        lblRepCatDesc.Margin = new Padding(0, UI.S(2), 0, 0);
        titleBox.Controls.Add(lblRepCatTitle);
        titleBox.Controls.Add(lblRepCatDesc);

        FlowLayoutPanel linkBox = new FlowLayoutPanel();
        linkBox.AutoSize = true;
        linkBox.WrapContents = false;
        linkBox.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        lnkRepAll = new LinkLabel(); lnkRepAll.Text = "Alle an"; lnkRepAll.AutoSize = true; lnkRepAll.LinkColor = UI.Accent; lnkRepAll.Margin = new Padding(0, UI.S(4), UI.S(10), 0);
        lnkRepNone = new LinkLabel(); lnkRepNone.Text = "Alle aus"; lnkRepNone.AutoSize = true; lnkRepNone.LinkColor = UI.Accent; lnkRepNone.Margin = new Padding(0, UI.S(4), 0, 0);
        linkBox.Controls.Add(lnkRepAll);
        linkBox.Controls.Add(lnkRepNone);

        headRow.Controls.Add(titleBox, 0, 0);
        headRow.Controls.Add(linkBox, 1, 0);
        cardContent.Controls.Add(headRow, 0, 0);

        Panel div = new Panel(); div.Dock = DockStyle.Top; div.Height = 1; div.BackColor = UI.Line; div.Margin = new Padding(0, UI.S(8), 0, UI.S(8));
        cardContent.Controls.Add(div, 0, 1);

        Panel catHost = new Panel();
        catHost.Dock = DockStyle.Fill;
        catHost.AutoSize = true;
        cardContent.Controls.Add(catHost, 0, 2);

        repDetailCard.Controls.Add(cardContent);

        repPickers.Clear();
        repCatPanels.Clear();

        string[] repIcons = new string[] { UI.IcoFolder, UI.IcoTrash, UI.IcoRefresh, UI.IcoGear };
        int grpIdx = 0;
        foreach (string grp in repairGroupOrder)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < repKeys.Length; i++) if (GetRepairGroup(i) == grp) indices.Add(i);
            if (indices.Count == 0) continue;
            string ico = grpIdx < repIcons.Length ? repIcons[grpIdx] : UI.IcoWartung;
            grpIdx++;
            string gd; repairGroupText.TryGetValue(grp, out gd);

            string kurz; if (!repairGroupShort.TryGetValue(grp, out kurz)) kurz = grp;
            CategoryPickerItem picker = new CategoryPickerItem(grp, ico, kurz);
            picker.Subtitle = gd ?? "";
            picker.Width = UI.S(232);
            string gKey = grp;
            picker.Picked += delegate { SelectRepairCategory(gKey); };
            Tip(picker, gd ?? "");
            repPickers[grp] = picker;
            leftCol.Controls.Add(picker);

            FlowLayoutPanel rowsPanel = new FlowLayoutPanel();
            rowsPanel.Dock = DockStyle.Fill;
            rowsPanel.AutoSize = true;
            rowsPanel.FlowDirection = FlowDirection.TopDown;
            rowsPanel.WrapContents = false;
            rowsPanel.Margin = new Padding(0);

            foreach (int idx in indices)
            {
                string[] parts = SplitDetail(repText[idx]);
                string tit = parts[0];
                string desc = parts.Length > 1 && parts[1].Length > 0 ? parts[1] : "";
                SettingRow row = new SettingRow(tit, desc, localRepToggles[idx]);
                row.Width = UI.S(UI.Gruppenbreite - 270);
                string rsk = idx < repRisk.Length ? repRisk[idx] : "";
                if (rsk.Length > 0) row.AddBadge(RiskBadge(rsk));
                string rst = idx < repRestart.Length ? repRestart[idx] : "";
                if (rst.Length > 0) row.AddBadge(RestartBadge(rst));

                ContractStep cs = steps.Find(delegate(ContractStep c) { return (c.Module == "Reparatur" || c.Module == "Wartung") && c.Key == repKeys[idx]; });
                StringBuilder t = new StringBuilder(repText[idx]);
                if (cs != null)
                {
                    t.Append("\r\nRisikostufe: ").Append(ContractStep.RiskLabel(cs.Risiko)).Append(cs.Risiko == "Aendern" ? " (protokolliert, auf der Seite Änderungen rücknehmbar)" : cs.Risiko == "Eingriff" ? " (nicht automatisch umkehrbar, Absicherung über den Wiederherstellungspunkt)" : "");
                    t.Append("\r\nNeustart: ").Append(cs.Neustart == "immer" ? "nötig" : cs.Neustart == "moeglich" ? "kann nötig sein" : "nicht nötig");
                    if (cs.Minuten > 0) t.Append("\r\nDauer: etwa ").Append(cs.Minuten).Append(" Min.");
                }
                string repTip = t.ToString();
                Tip(row, repTip);
                Tip(localRepToggles[idx], repTip);

                rowsPanel.Controls.Add(row);
            }
            rowsPanel.Visible = false;
            catHost.Controls.Add(rowsPanel);
            repCatPanels[grp] = rowsPanel;
        }

        md.Controls.Add(leftCol, 0, 0);
        md.Controls.Add(repDetailCard, 1, 0);
        f.Controls.Add(md);

        lnkRepAll.LinkClicked += delegate {
            if (curRepKey.Length > 0)
            {
                for (int i = 0; i < repKeys.Length; i++) if (GetRepairGroup(i) == curRepKey) repChk[i].Checked = true;
            }
        };
        lnkRepNone.LinkClicked += delegate {
            if (curRepKey.Length > 0)
            {
                for (int i = 0; i < repKeys.Length; i++) if (GetRepairGroup(i) == curRepKey) repChk[i].Checked = false;
            }
        };

        // --- Unterer Bereich: Vorlagen, Absicherung und Risikolegende ---
        f.Controls.Add(Section("Vorlagen"));
        FlowLayoutPanel b = Row(); b.Margin = new Padding(UI.S(4), UI.S(2), 0, UI.S(UI.Abstand2));
        pillRepUsual = new PillButton("Empfohlene Auswahl", false); pillRepUsual.Margin = new Padding(0, 0, UI.S(8), 0);
        pillRepUsual.Picked += delegate {
            for (int i = 0; i < repChk.Length; i++) repChk[i].Checked = repUsual[i];
            UpdateRepairCounts();
        };
        pillRepNone = new PillButton("Auswahl aufheben", false);
        pillRepNone.Picked += delegate {
            foreach (CheckBox c in repChk) c.Checked = false;
            UpdateRepairCounts();
        };
        b.Controls.Add(pillRepUsual); b.Controls.Add(pillRepNone); f.Controls.Add(b);

        f.Controls.Add(Section("Absicherung"));
        chkRestorePoint = Chk("Vorher einen Systemwiederherstellungspunkt anlegen", true); chkRestorePoint.Margin = new Padding(UI.S(4), UI.S(2), 0, UI.S(2)); f.Controls.Add(chkRestorePoint);
        lblRestoreWarn = Lbl("", UI.SchriftText, true, UI.Warn); lblRestoreWarn.BackColor = UI.WarnBg; lblRestoreWarn.Padding = new Padding(UI.S(6)); lblRestoreWarn.Margin = new Padding(UI.S(4), UI.S(4), 0, UI.S(2));
        lblRestoreWarn.MaximumSize = new Size(UI.S(UI.Gruppenbreite), 0); lblRestoreWarn.Visible = false; f.Controls.Add(lblRestoreWarn);
        chkRestorePoint.CheckedChanged += delegate { UpdateRestoreWarn(); UpdateSummary(); };

        f.Controls.Add(RiskLegend());

        if (repairGroupOrder.Length > 0) SelectRepairCategory(repairGroupOrder[0]);
        UpdateRepairCounts();
        UpdateRestoreWarn();
        return f;
    }

    void SelectRepairCategory(string grp)
    {
        curRepKey = grp;
        foreach (KeyValuePair<string, CategoryPickerItem> kv in repPickers)
        {
            kv.Value.Selected = (kv.Key == grp);
        }
        foreach (KeyValuePair<string, Control> kv in repCatPanels)
        {
            kv.Value.Visible = (kv.Key == grp);
        }
        lblRepCatTitle.Text = grp;
        string gd; repairGroupText.TryGetValue(grp, out gd);
        lblRepCatDesc.Text = gd ?? "";
    }

    // ab v3.65 gemeinsamer Aufbau einer Kategorie für Wartung und Optimierung: Kopfzeile mit Zähler, rechts alle und
    // keine, darunter die Beschreibung und die Tabelle der Einträge (Haken links, Etiketten rechts)
    TableLayoutPanel CategoryBlock(FlowLayoutPanel f, string title, string desc, out GroupHeader tg, out LinkLabel all, out LinkLabel none)
    {
        FlowLayoutPanel h = new FlowLayoutPanel(); h.AutoSize = true; h.WrapContents = false; h.BackColor = UI.Panel; h.Margin = new Padding(0, UI.S(UI.Abstand2), 0, 0);
        tg = new GroupHeader(title); tg.Width = UI.S(UI.Gruppenbreite - 110); tg.Margin = new Padding(0);
        all = new LinkLabel(); all.Text = "Alle an"; all.AutoSize = true; all.Margin = new Padding(UI.S(12), UI.S(8), UI.S(8), 0); all.LinkColor = UI.Accent;
        none = new LinkLabel(); none.Text = "Alle aus"; none.AutoSize = true; none.Margin = new Padding(0, UI.S(8), 0, 0); none.LinkColor = UI.Accent;
        h.Controls.Add(tg); h.Controls.Add(all); h.Controls.Add(none);
        f.Controls.Add(h);
        if (!String.IsNullOrEmpty(desc)) f.Controls.Add(Hint(desc, 30));
        TableLayoutPanel body = BadgeTable(); body.Margin = new Padding(UI.S(UI.Abstand4), 0, 0, UI.S(UI.Abstand1)); body.Visible = false;
        f.Controls.Add(body);
        return body;
    }

    // ab v4.0 Kurztitel der Gruppen für die Kategorieliste (wie Kurztitel in Module\Wartung\Vertrag.psd1)
    static readonly Dictionary<string, string> repairGroupShort = new Dictionary<string, string> {
        { "Systemdateien und Komponentenspeicher", "Systemdateien" },
        { "Bereinigung und Speicherplatz", "Bereinigung" },
        { "Windows Update, Netzwerk und Zeit", "Update und Netzwerk" },
        { "Dienste, Geräte und Energie", "Dienste und Energie" } };

    static readonly Dictionary<string, string> repairGroupText = new Dictionary<string, string> {
        { "Systemdateien und Komponentenspeicher", "Windows-Systemdateien, Komponentenspeicher und Dateisystem prüfen und reparieren." },
        { "Bereinigung und Speicherplatz", "Temporäre Dateien, Caches, alte Update-Downloads und Absturzabbilder entfernen." },
        { "Windows Update, Netzwerk und Zeit", "Update-Komponenten, Netzwerkeinstellungen und Zeitsynchronisierung zurücksetzen." },
        { "Dienste, Geräte und Energie", "WMI, Leistungszähler, Druckwarteschlange, Geräteerkennung, Schnellstart und Energiepläne." } };

    // Legende der Risikostufen als Etiketten mit kurzer Erklärung (Wartung und Optimierung)
    Control RiskLegend()
    {
        FlowLayoutPanel l = new FlowLayoutPanel(); l.AutoSize = true; l.FlowDirection = FlowDirection.TopDown; l.WrapContents = false; l.BackColor = UI.Panel; l.Margin = new Padding(UI.S(4), 0, 0, UI.S(2));
        Badge ba = new Badge(ContractStep.RiskLabel("Aendern"), "info"); ba.Margin = new Padding(0, UI.S(2), UI.S(6), 0);
        Label ta = Lbl("protokolliert, auf der Seite Änderungen rücknehmbar", UI.SchriftHinweis, false, UI.Muted); ta.Margin = new Padding(0, UI.S(3), UI.S(UI.Abstand4), 0);
        Badge be = new Badge(ContractStep.RiskLabel("Eingriff"), "warn"); be.Margin = new Padding(0, UI.S(2), UI.S(6), 0);
        Label te = Lbl("nicht automatisch umkehrbar, abgesichert über den Wiederherstellungspunkt", UI.SchriftHinweis, false, UI.Muted); te.Margin = new Padding(0, UI.S(3), 0, 0);
        Tip(ba, "Wird mit Vorher-Wert protokolliert und lässt sich auf der Seite Änderungen zurücknehmen.");
        Tip(be, "Lässt sich nicht automatisch umkehren; der Weg zurück ist der Wiederherstellungspunkt.");
        foreach (Control[] pair in new Control[][] { new Control[] { ba, ta }, new Control[] { be, te } })
        {
            FlowLayoutPanel r = new FlowLayoutPanel(); r.AutoSize = true; r.WrapContents = false; r.BackColor = UI.Panel; r.Margin = new Padding(0, 0, 0, UI.S(2));
            pair[0].Width = Math.Max(pair[0].Width, UI.S(70));
            r.Controls.Add(pair[0]); r.Controls.Add(pair[1]); l.Controls.Add(r);
        }
        return l;
    }

    const int BadgeSpalte = 230;

    static TableLayoutPanel BadgeTable()
    {
        TableLayoutPanel t = new TableLayoutPanel(); t.AutoSize = true; t.AutoSizeMode = AutoSizeMode.GrowAndShrink; t.ColumnCount = 2; t.BackColor = UI.Panel;
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(UI.Gruppenbreite - 16 - BadgeSpalte))); t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(BadgeSpalte)));
        return t;
    }

    Control BadgeCell(params Badge[] badges)
    {
        // Etiketten rechtsbündig; reicht die Spalte nicht (verwaltet, Zustand), brechen sie in eine zweite Zeile um
        FlowLayoutPanel c = new FlowLayoutPanel(); c.AutoSize = true; c.WrapContents = true; c.FlowDirection = FlowDirection.RightToLeft; c.BackColor = UI.Panel; c.Dock = DockStyle.Top; c.Margin = new Padding(0, UI.S(2), 0, 0);
        c.MaximumSize = new Size(UI.S(BadgeSpalte), 0);
        foreach (Badge b in badges) if (b != null)
        {
            b.Margin = new Padding(UI.S(6), UI.S(2), 0, 0);
            BadgeTip(b);
            c.Controls.Add(b);
        }
        return c;
    }

    // Warnung: Eingriffe gewählt, aber kein Wiederherstellungspunkt; Computerschutz aus (soweit lesend feststellbar)
    void UpdateRestoreWarn()
    {
        if (lblRestoreWarn == null || repChk == null) return;
        int ein = 0;
        for (int i = 0; i < repChk.Length; i++) if (repChk[i] != null && repChk[i].Checked && i < repRisk.Length && (repRisk[i] == "Eingriff" || repRisk[i] == "Zerstoerend")) ein++;
        string t = "";
        if (ein > 0 && !chkRestorePoint.Checked) t = "Es sind " + ein + (ein == 1 ? " Eingriff" : " Eingriffe") + " gewählt, aber kein Wiederherstellungspunkt. Ohne ihn gibt es keinen Weg zurück.";
        else if (chkRestorePoint.Checked && SystemRestoreOff()) t = "Der Computerschutz scheint auf diesem PC ausgeschaltet zu sein; der Wiederherstellungspunkt kann dann nicht angelegt werden.";
        SetText(lblRestoreWarn, t);
        lblRestoreWarn.Visible = t.Length > 0;
    }

    // Computerschutz per Richtlinie oder Systemeinstellung aus (nur lesend, ohne Gewähr; unbekannt gilt als an)
    static int systemRestoreOff = -1;
    static bool SystemRestoreOff()
    {
        if (systemRestoreOff >= 0) return systemRestoreOff == 1;
        systemRestoreOff = 0;
        try
        {
            using (Microsoft.Win32.RegistryKey k = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore"))
                if (k != null && (Convert.ToInt32(k.GetValue("DisableSR", 0)) == 1 || Convert.ToInt32(k.GetValue("DisableConfig", 0)) == 1)) systemRestoreOff = 1;
            using (Microsoft.Win32.RegistryKey k = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore"))
                if (k != null) { object v = k.GetValue("RPSessionInterval"); if (v != null && Convert.ToInt32(v) == 0) systemRestoreOff = 1; }
        }
        catch { }
        return systemRestoreOff == 1;
    }

    void UpdateRepairCounts()
    {
        foreach (string grp in repairGroupOrder)
        {
            int sel = 0, tot = 0;
            for (int i = 0; i < repKeys.Length; i++)
            {
                if (GetRepairGroup(i) == grp)
                {
                    tot++;
                    if (repChk[i] != null && repChk[i].Checked) sel++;
                }
            }
            if (repPickers.ContainsKey(grp)) repPickers[grp].SetCounts(sel, tot);
            if (repCatCount.ContainsKey(grp)) repCatCount[grp].CountText = sel + " von " + tot + " gewählt";
        }
    }

    void UpdateRepairGroupVisibility()
    {
        foreach (string grp in repairGroupOrder)
        {
            if (!repCatBody.ContainsKey(grp) || !repCatToggle.ContainsKey(grp)) continue;
            bool hasChecked = false;
            for (int i = 0; i < repKeys.Length; i++)
            {
                if (GetRepairGroup(i) == grp && repChk[i] != null && repChk[i].Checked) { hasChecked = true; break; }
            }
            repCatBody[grp].Visible = hasChecked;
            repCatToggle[grp].Expanded = hasChecked;
        }
    }
}
