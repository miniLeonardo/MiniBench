// Seite Optimierung (Teilklasse DiagGui)
public partial class DiagGui
{
    // ------------------------------------------------------------ Seite Optimierung (ab v2.8)
    // Katalog aus dem Skript (Get-OptGuiLines): Kategorien zum Auf- und Zuklappen, je Eintrag ein Haken mit Risikostufe.
    List<OptItem> optItems = new List<OptItem>();
    List<string[]> optCats = new List<string[]>();
    Dictionary<string, GroupHeader> optCatCount = new Dictionary<string, GroupHeader>();
    Dictionary<string, Control> optCatBody = new Dictionary<string, Control>();
    Dictionary<string, GroupHeader> optCatToggle = new Dictionary<string, GroupHeader>();
    CheckBox chkOptRestore;
    Label lblOptInfo, lblOptState;
    Button btnOptTools, btnOptAll;
    List<Button> optPresetButtons = new List<Button>();
    static readonly string[] optPresetTips = new string[] {
        "Minimal: Grundlegende Datenschutz-Einstellungen, Dienste und Aufgaben, Indizierung aus, Caches leeren.",
        "Leos Empfehlung: (empfohlene Optimierungen für Datenschutz, Apps, Dienste und System).",
        "Erweitert: Leos Empfehlung plus Windows-Funktionen, Zusatzfeatures, vorinstallierte Apps und Darstellung.",
        "Hebt die Auswahl in allen Kategorien auf." };

    static readonly string[] defaultMinimal = new string[] {
        "TelemetrieMinimal", "Fehlerberichte", "WerbeId", "Speicheroptimierung", "TaskbarEndTask"
    };
    HashSet<string> cachedMinimalIds;

    HashSet<string> GetMinimalIds()
    {
        if (cachedMinimalIds != null) return cachedMinimalIds;
        HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string id in defaultMinimal) set.Add(id);
        foreach (OptItem it in optItems)
        {
            if (it.Vorlagen != null && it.Vorlagen.IndexOf("M", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                set.Add(it.Id);
            }
        }
        cachedMinimalIds = set;
        return cachedMinimalIds;
    }

    static readonly string[] defaultLeoEmpfehlung = new string[] {
        "TelemetrieMinimal", "TelemetrieDienste", "TelemetrieDiagnoseprotokolle", "Feedback", "WerbeId",
        "MassgeschneiderteErfahrung", "Sprachliste", "Handschrift", "Inventar", "Insiderseite",
        "Nachrichtensicherung", "BluetoothWerbung", "MediaPlayerDiagnose", "Standort", "TippsVorschlaege",
        "Willkommen", "EinstellungenVorschlaege", "AppsStillInstallieren", "Sperrbildschirm", "Sperrkamera",
        "ExplorerWerbung", "Smartphone", "Websuche", "Suchhighlights", "Cortana",
        "Spracherkennung", "Copilot", "Recall", "PaintKI", "AppDiagnose",
        "AppBewegung", "AppStarts", "DiensteSelten", "DiensteSync", "DiensteSmartcard",
        "DiensteEdgeUpdate", "AufgabenTelemetrie", "AufgabenKarten", "AufgabenJugendschutz", "AufgabenXbox",
        "AufgabenGesicht", "CapSchrittaufzeichnung", "CapMathe", "CapIse", "CapHandschrift",
        "AppHilfeTipps", "AppSolitaer", "AppOfficeHub", "AppFeedbackHub", "AppKarten",
        "AppClipchamp", "AppTeamsPrivat", "AppSmartphoneLink", "AppCortana", "AppCopilot",
        "AppPowerAutomate", "AppDevHome", "AppAltlasten", "AppKontakteAufgaben", "AppOutlookNeu",
        "Widgets", "JetztBesprechen", "DunklerModus", "Hintergrundqualitaet", "Laufwerksname",
        "Mausbeschleunigung", "Spieleprioritaet", "EdgeVerknuepfung", "EdgeHintergrund", "OneDriveRichtlinie",
        "OneDriveEntfernen", "DefenderMeldungen", "TaskbarEndTask", "UefiNeustart"
    };
    HashSet<string> cachedLeoIds;

    HashSet<string> GetLeoEmpfehlungIds()
    {
        if (cachedLeoIds != null) return cachedLeoIds;
        HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            Dictionary<string, object> d = ReadPresetFile();
            Dictionary<string, object> e = SubDict(SubDict(d, "Eintraege"), "Leos Empfehlung");
            Dictionary<string, object> w = SubDict(e, "Werte");
            string os = w != null ? GetS(w, "Opt.Auswahl") : null;
            if (!string.IsNullOrEmpty(os))
            {
                string[] parts = os.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string p in parts) set.Add(p.Trim());
            }
        }
        catch { }
        if (set.Count == 0)
        {
            foreach (string id in defaultLeoEmpfehlung) set.Add(id);
        }
        foreach (OptItem it in optItems)
        {
            if (it.Vorlagen != null && it.Vorlagen.IndexOf("S", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                set.Add(it.Id);
            }
        }
        cachedLeoIds = set;
        return cachedLeoIds;
    }

    void ReadOptCatalog()
    {
        foreach (string l in OptKatalog ?? new string[0])
        {
            string[] x = l.Split('|');
            if (x.Length >= 4 && x[0] == "K") optCats.Add(new string[] { x[1], x[2], x[3], x.Length >= 5 ? x[4] : x[2], x.Length >= 6 ? x[5] : "" });
            else if (x.Length >= 11 && x[0] == "E")
            {
                OptItem it = new OptItem(); it.Id = x[1]; it.Kat = x[2]; it.Risiko = x[3]; it.Neustart = x[4]; it.Vorlagen = x[5]; it.Verwaltet = x[6] == "1"; it.Bedingung = x[7];
                int m; int.TryParse(x[8], out m); it.Minuten = m; it.Titel = x[9]; it.Tip = x[10];
                optItems.Add(it);
            }
        }
    }

    // ab v3.64 nur noch der Titel; Risikostufe, Neustart, verwaltet und Zustand stehen als Etiketten daneben
    string OptBoxText(OptItem it)
    {
        if (it.StateBadge != null)
        {
            it.StateBadge.Text = it.Zustand.Length > 0 ? (it.Zustand == "aktiv" ? "bereits aktiv" : it.Zustand) : "";
            it.StateBadge.Level = it.Zustand == "aktiv" ? "ok" : it.Zustand == "teilweise" ? "info" : it.Zustand == "fehlgeschlagen" || it.Zustand == "Fehler" ? "crit" : "skip";
        }
        return it.Titel;
    }

    string OptTipText(OptItem it)
    {
        StringBuilder t = new StringBuilder();
        if (it.Gesperrt) t.Append(OptGesperrtHinweis).Append("\r\n");
        t.Append((it.Tip ?? "").Replace(" ¶", "\r\n").Replace("¶", "\r\n"));
        t.Append("\r\nRisikostufe: ").Append(ContractStep.RiskLabel(it.Risiko)).Append(it.Risiko == "Aendern" ? " (protokolliert, auf der Seite Änderungen einzeln rücknehmbar)" : " (nicht automatisch umkehrbar, Absicherung über den Wiederherstellungspunkt)");
        t.Append("\r\nNeustart: ").Append(it.Neustart == "immer" ? "nötig" : it.Neustart == "moeglich" ? "kann nötig sein" : "nicht nötig");
        List<string> vList = new List<string>();
        if (it.Vorlagen.Contains("M")) vList.Add("Minimal");
        if (GetLeoEmpfehlungIds().Contains(it.Id)) vList.Add("Leos Empfehlung");
        if (it.Vorlagen.Contains("E")) vList.Add("Erweitert");
        if (vList.Count > 0) t.Append("\r\nVorlagen: ").Append(string.Join(", ", vList.ToArray()));
        else t.Append("\r\nIn keiner Vorlage, nur von Hand wählbar.");
        if (it.Verwaltet) t.Append("\r\nBerührt Richtlinien oder zentrale Verwaltung; auf Domänen-PCs nicht in den Vorlagen.");
        if (it.Bedingung.Length > 0) t.Append("\r\nGilt nur für: ").Append(it.Bedingung == "Win10" ? "Windows 10" : it.Bedingung == "Win11" ? "Windows 11" : it.Bedingung == "Desktop" ? "Desktop-PCs" : it.Bedingung == "Nvidia" ? "PCs mit NVIDIA-Grafik" : "PCs mit Windows Terminal");
        if (it.Zustand.Length > 0) t.Append("\r\nZustand auf diesem PC: ").Append(it.Zustand).Append(it.ZustandText.Length > 0 ? " (" + it.ZustandText + ")" : "");
        return t.ToString();
    }

    Control BuildOptPage()
    {
        ReadOptCatalog();
        FlowLayoutPanel f = Page("Optimierung", "Windows-Einstellungen zu Datenschutz, Werbung, Hintergrunddiensten und Leistung. Jede Änderung wird protokolliert und lässt sich auf der Seite Änderungen zurücknehmen.", 5);
        if (optItems.Count == 0) { lblOptInfo = Lbl("Der Katalog der Optimierung fehlt (Skript ohne Modul Optimierung).", 9f, false, UI.Muted); f.Controls.Add(lblOptInfo); return f; }

        TableLayoutPanel md = new TableLayoutPanel();
        md.Width = UI.S(UI.Gruppenbreite);
        md.AutoSize = true;
        md.ColumnCount = 2;
        md.RowCount = 1;
        md.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UI.S(262)));
        md.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        md.Margin = new Padding(UI.S(4), UI.S(2), 0, UI.S(8));

        FlowLayoutPanel leftCol = new FlowLayoutPanel();
        leftCol.Dock = DockStyle.Fill;
        leftCol.AutoSize = true;
        leftCol.FlowDirection = FlowDirection.TopDown;
        leftCol.WrapContents = false;
        leftCol.Margin = new Padding(0);

        optDetailCard = new CardPanel();
        optDetailCard.Dock = DockStyle.Fill;
        optDetailCard.AutoSize = true;
        optDetailCard.Padding = new Padding(UI.S(14));
        optDetailCard.Margin = new Padding(UI.S(8), 0, 0, 0);

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
        lblOptCatTitle = Lbl("", 11f, true, UI.Text);
        lblOptCatDesc = Lbl("", UI.SchriftHinweis, false, UI.Muted);
        lblOptCatDesc.Margin = new Padding(0, UI.S(2), 0, 0);
        lblOptCatCount = Lbl("", UI.SchriftHinweis, true, UI.Muted);
        lblOptCatCount.Margin = new Padding(0, UI.S(4), 0, 0);
        Tip(lblOptCatCount, "Gewählt: wird beim nächsten Lauf geändert. Bereits aktiv: ist auf diesem PC schon so eingestellt, Schalter gesperrt.");
        titleBox.Controls.Add(lblOptCatTitle);
        titleBox.Controls.Add(lblOptCatDesc);
        titleBox.Controls.Add(lblOptCatCount);

        FlowLayoutPanel linkBox = new FlowLayoutPanel();
        linkBox.AutoSize = true;
        linkBox.WrapContents = false;
        linkBox.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        lnkOptAll = new LinkLabel(); lnkOptAll.Text = "Alle an"; lnkOptAll.AutoSize = true; lnkOptAll.LinkColor = UI.Accent; lnkOptAll.Margin = new Padding(0, UI.S(4), UI.S(10), 0);
        lnkOptNone = new LinkLabel(); lnkOptNone.Text = "Alle aus"; lnkOptNone.AutoSize = true; lnkOptNone.LinkColor = UI.Accent; lnkOptNone.Margin = new Padding(0, UI.S(4), 0, 0);
        linkBox.Controls.Add(lnkOptAll);
        linkBox.Controls.Add(lnkOptNone);

        headRow.Controls.Add(titleBox, 0, 0);
        headRow.Controls.Add(linkBox, 1, 0);
        cardContent.Controls.Add(headRow, 0, 0);

        Panel div = new Panel(); div.Dock = DockStyle.Top; div.Height = 1; div.BackColor = UI.Line; div.Margin = new Padding(0, UI.S(8), 0, UI.S(8));
        cardContent.Controls.Add(div, 0, 1);

        Panel catHost = new Panel();
        catHost.Dock = DockStyle.Fill;
        catHost.AutoSize = true;
        cardContent.Controls.Add(catHost, 0, 2);

        optDetailCard.Controls.Add(cardContent);

        optPickers.Clear();
        optCatPanels.Clear();

        foreach (string[] k in optCats)
        {
            string key = k[0];
            List<OptItem> items = optItems.FindAll(delegate(OptItem it) { return it.Kat == key; });
            if (items.Count == 0) continue;

            string ico = UI.IconByName(k.Length > 4 ? k[4] : "", UI.IcoOpt);
            CategoryPickerItem picker = new CategoryPickerItem(k[1], ico, k.Length > 3 ? k[3] : k[1]);
            picker.Subtitle = k[2];
            picker.Width = UI.S(254);
            string gKey = key;
            picker.Picked += delegate { SelectOptCategory(gKey); };
            Tip(picker, k[2]);
            optPickers[key] = picker;
            leftCol.Controls.Add(picker);

            FlowLayoutPanel rowsPanel = new FlowLayoutPanel();
            rowsPanel.Dock = DockStyle.Fill;
            rowsPanel.AutoSize = true;
            rowsPanel.FlowDirection = FlowDirection.TopDown;
            rowsPanel.WrapContents = false;
            rowsPanel.Margin = new Padding(0);

            foreach (OptItem it in items)
            {
                it.Box = Chk(OptBoxText(it), false);
                it.Box.UseMnemonic = false;
                it.Toggle = new ToggleSwitch();
                it.Toggle.Checked = false;
                it.StateBadge = new Badge("", "ok"); it.StateBadge.Tag = "zustand";
                Badge bv = it.Verwaltet && DomainPc ? new Badge("verwaltet", "warn") : null;
                if (bv != null) bv.Tag = "verwaltet";
                it.VerwaltetBadge = bv;

                VerbindeOptEintrag(it, delegate { UpdateOptCounts(); });

            }
            rowsPanel.Visible = false;
            catHost.Controls.Add(rowsPanel);
            optCatPanels[key] = rowsPanel;
        }

        md.Controls.Add(leftCol, 0, 0);
        md.Controls.Add(optDetailCard, 1, 0);
        f.Controls.Add(md);

        lnkOptAll.LinkClicked += delegate {
            if (curOptKey.Length > 0)
            {
                foreach (OptItem it in optItems) if (it.Kat == curOptKey && it.Box != null && !it.Gesperrt) it.Box.Checked = true;
            }
        };
        lnkOptNone.LinkClicked += delegate {
            if (curOptKey.Length > 0)
            {
                foreach (OptItem it in optItems) if (it.Kat == curOptKey && it.Box != null && !it.Gesperrt) it.Box.Checked = false;
            }
        };

        // --- Unterer Bereich: Vorlagen, Werkzeuge, Absicherung und Risikolegende ---
        f.Controls.Add(Section("Vorlagen der Optimierung und Zustand"));

        FlowLayoutPanel bPills = Row(); bPills.Margin = new Padding(UI.S(4), UI.S(2), 0, UI.S(4));
        optPresetPills.Clear();
        string[] pn = new string[] { "Minimal", "Leos Empfehlung", "Erweitert", "Auswahl aufheben" };
        string[] pkeys = new string[] { "M", "S", "E", "" };
        for (int i = 0; i < pn.Length; i++)
        {
            string key = pkeys[i];
            PillButton pb = new PillButton(pn[i], i == 0);
            pb.Margin = new Padding(i == 0 ? 0 : UI.S(8), 0, 0, 0);
            pb.Picked += delegate {
                for (int k = 0; k < optPresetPills.Count; k++) optPresetPills[k].Active = (optPresetPills[k] == pb);
                ApplyOptPreset(key);
            };
            optPresetPills.Add(pb);
            bPills.Controls.Add(pb);
        }
        f.Controls.Add(bPills);

        FlowLayoutPanel bTools = Row(); bTools.Margin = new Padding(UI.S(4), UI.S(4), 0, UI.S(4));
        // ab v4.02 kein Knopf "Zustand prüfen" mehr: Der Zustand wird nach dem Start im Hintergrund gelesen (StartOptStateCheck)
        btnOptTools = UI.Secondary("Werkzeuge holen \u2026"); btnOptTools.Margin = new Padding(0); btnOptTools.Click += delegate { FetchOptTools(); }; bTools.Controls.Add(btnOptTools);
        btnOptAll = UI.Secondary("Alle aufklappen"); btnOptAll.Visible = false;
        f.Controls.Add(bTools);

        lblOptInfo = Lbl("", 9f, false, UI.Muted); lblOptInfo.MaximumSize = new Size(UI.S(UI.Gruppenbreite), 0); lblOptInfo.Margin = new Padding(UI.S(4), UI.S(2), UI.S(4), 0); f.Controls.Add(lblOptInfo);
        lblOptState = Lbl("", 9f, false, UI.Muted); lblOptState.MaximumSize = new Size(UI.S(UI.Gruppenbreite), 0); lblOptState.Margin = new Padding(UI.S(4), UI.S(2), UI.S(4), UI.S(4)); f.Controls.Add(lblOptState);

        f.Controls.Add(Section("Absicherung"));
        chkOptRestore = Chk("Vorher einen Systemwiederherstellungspunkt anlegen", true); f.Controls.Add(chkOptRestore);

        f.Controls.Add(RiskLegend());
        f.Controls.Add(Hint("Einträge ohne Etikett sind Stufe Ändern. verwaltet: ber\u00FChrt Gruppenrichtlinien, auf Dom\u00E4nen-PCs nicht in den Vorlagen. Benutzereinstellungen gelten f\u00FCr den angemeldeten Benutzer.", 4));

        if (optCats.Count > 0) SelectOptCategory(optCats[0][0]);
        ApplyOptPreset("M");
        return f;
    }

    // ab v4.01: Zeilen einer Kategorie entstehen erst, wenn sie zum ersten Mal angezeigt wird. Haken, Schalter und
    // Etiketten gibt es von Anfang an (Vorlagen, Zähler und Zustand arbeiten damit); beim Start entsteht so nur die
    // erste Kategorie statt aller über 150 Zeilen.
    HashSet<string> optRowsBuilt = new HashSet<string>();
    void EnsureOptRows(string key)
    {
        if (optRowsBuilt.Contains(key)) return;
        Control host; if (!optCatPanels.TryGetValue(key, out host)) return;
        optRowsBuilt.Add(key);
        host.SuspendLayout();
        foreach (OptItem it in optItems)
        {
            if (it.Kat != key || it.Toggle == null) continue;
            string shortDesc = it.Tip ?? "";
            int p = shortDesc.IndexOf(" \u00B6");
            if (p >= 0) shortDesc = shortDesc.Substring(0, p).Trim();

            SettingRow row = new SettingRow(it.Titel, shortDesc, it.Toggle);
            it.Row = row;
            if (it.Gesperrt) row.Cursor = Cursors.Default;
            row.Width = UI.S(UI.Gruppenbreite - 270);
            if (it.Risiko.Length > 0 && it.Risiko != "Lesen" && it.Risiko != "Aendern") row.AddBadge(RiskBadge(it.Risiko));
            if (it.Neustart == "immer") row.AddBadge(RestartBadge("immer"));
            if (it.VerwaltetBadge != null) row.AddBadge(it.VerwaltetBadge);
            if (it.StateBadge != null) row.AddBadge(it.StateBadge);

            string fullTip = OptTipText(it);
            Tip(row, fullTip);
            Tip(it.Toggle, fullTip);
            host.Controls.Add(row);
        }
        host.ResumeLayout(true);
    }

    void SelectOptCategory(string key)
    {
        EnsureOptRows(key);
        curOptKey = key;
        foreach (KeyValuePair<string, CategoryPickerItem> kv in optPickers)
        {
            kv.Value.Selected = (kv.Key == key);
        }
        foreach (KeyValuePair<string, Control> kv in optCatPanels)
        {
            kv.Value.Visible = (kv.Key == key);
        }
        foreach (string[] k in optCats)
        {
            if (k[0] == key)
            {
                lblOptCatTitle.Text = k[1];
                lblOptCatDesc.Text = k[2];
                if (lblOptCatCount != null) lblOptCatCount.Text = OptZaehlerText(OptZaehler(optItems, key));
                break;
            }
        }
    }

    void ApplyOptPreset(string key)
    {
        int skip = 0;
        HashSet<string> leoIds = key == "S" ? GetLeoEmpfehlungIds() : null;
        HashSet<string> minIds = key == "M" ? GetMinimalIds() : null;
        foreach (OptItem it in optItems)
        {
            bool on = false;
            if (key == "S")
            {
                on = (leoIds != null && leoIds.Contains(it.Id)) || (it.Vorlagen != null && it.Vorlagen.Contains("S"));
            }
            else if (key == "M")
            {
                on = (minIds != null && minIds.Contains(it.Id)) || (it.Vorlagen != null && it.Vorlagen.Contains("M"));
            }
            else if (key.Length > 0)
            {
                on = it.Vorlagen.Contains(key);
            }
            if (on && DomainPc && it.Verwaltet) { on = false; skip++; }
            if (it.Box != null) it.Box.Checked = on;
        }
        UpdateOptCounts();
        if (lblOptInfo != null)
        {
            string name = key == "M" ? "Minimal" : key == "S" ? "Leos Empfehlung" : key == "E" ? "Erweitert" : "";
            int n = SelectedOptIds().Count, ak = 0;
            foreach (OptItem it in optItems) if (it.Gesperrt) ak++;
            lblOptInfo.Text = (name.Length > 0 ? "Vorlage " + name + ": " + n + " Einträge gewählt." : "Keine Einträge gewählt.")
                + (ak > 0 ? " " + ak + " Einträge sind bereits aktiv und bleiben unverändert." : "")
                + (DomainPc ? "  Dieser PC ist Mitglied einer Domäne" + (skip == 1 ? "; 1 verwalteter Eintrag ist nicht gewählt" : skip > 1 ? "; " + skip + " verwaltete Einträge sind nicht gewählt" : "") + ". Gruppenrichtlinien können Einstellungen wieder überschreiben." : "");
            lblOptInfo.ForeColor = DomainPc ? UI.Warn : UI.Muted;
        }
    }

    // Für den Lauf: gewählt und nicht bereits aktiv (ab v4.05)
    List<string> SelectedOptIds() { return GewaehlteOptIds(optItems); }

    // Für Vorlagen: die Auswahl einschließlich gesperrter Einträge, damit eine Vorlage auf einem anderen PC gleich wirkt
    List<string> ChosenOptIds()
    {
        List<string> r = new List<string>();
        foreach (OptItem it in optItems) if (it.Box != null && it.Box.Checked) r.Add(it.Id);
        return r;
    }

    public const string OptGesperrtHinweis = "Ist auf diesem PC schon so eingestellt. Zur\u00FCcknehmen auf der Seite \u00C4nderungen.";

    public static List<string> GewaehlteOptIds(List<OptItem> items)
    {
        List<string> r = new List<string>();
        foreach (OptItem it in items) if (it.Box != null && it.Box.Checked && !it.Gesperrt) r.Add(it.Id);
        return r;
    }

    // Haken (Auswahl) und Schalter (Anzeige) eines Eintrags verbinden. Gesperrt zeigt der Schalter An und folgt dem
    // Haken nicht; Klicks auf Schalter und Zeile wirken nicht (ToggleSwitch.Locked).
    public static void VerbindeOptEintrag(OptItem it, EventHandler geaendert)
    {
        OptItem r = it;
        it.Toggle.CheckedChanged += delegate {
            if (!r.Gesperrt && r.Box.Checked != r.Toggle.Checked) r.Box.Checked = r.Toggle.Checked;
        };
        it.Box.CheckedChanged += delegate {
            if (!r.Gesperrt && r.Toggle.Checked != r.Box.Checked) r.Toggle.Checked = r.Box.Checked;
            if (geaendert != null) geaendert(r, EventArgs.Empty);
        };
    }

    public static void SperreOptEintrag(OptItem it, bool gesperrt)
    {
        if (it == null || it.Toggle == null) return;
        it.Gesperrt = gesperrt;
        it.Toggle.Locked = gesperrt;
        it.Toggle.Checked = gesperrt || (it.Box != null && it.Box.Checked);
        if (it.Row != null) { it.Row.Cursor = gesperrt ? Cursors.Default : Cursors.Hand; it.Row.Invalidate(); }
    }

    // Zähler einer Kategorie: [0] gewählt (ohne gesperrte), [1] bereits aktiv, [2] Einträge
    public static int[] OptZaehler(List<OptItem> items, string kat)
    {
        int[] z = new int[3];
        foreach (OptItem it in items)
        {
            if (it.Kat != kat) continue;
            z[2]++;
            if (it.Gesperrt) z[1]++;
            else if (it.Box != null && it.Box.Checked) z[0]++;
        }
        return z;
    }

    public static string OptZaehlerText(int[] z)
    {
        return z[0] + " gew\u00E4hlt \u00B7 " + (z[1] > 0 ? z[1] + " bereits aktiv \u00B7 " : "") + z[2] + " Eintr\u00E4ge";
    }

    void UpdateOptCounts()
    {
        foreach (string[] k in optCats)
        {
            string key = k[0];
            int[] z = OptZaehler(optItems, key);
            if (optPickers.ContainsKey(key)) optPickers[key].SetCounts(z[0], z[2], z[1]);
            if (optCatCount.ContainsKey(key)) optCatCount[key].CountText = z[0] + " von " + z[2] + " gew\u00E4hlt";
            if (key == curOptKey && lblOptCatCount != null) lblOptCatCount.Text = OptZaehlerText(z);
        }
    }

    void ToggleOptAll()
    {
        bool open = false;
        foreach (Control p in optCatBody.Values) if (!p.Visible) { open = true; break; }
        foreach (KeyValuePair<string, Control> kv in optCatBody)
        {
            kv.Value.Visible = open;
            optCatToggle[kv.Key].Expanded = open;
        }
        btnOptAll.Text = open ? "Alle zuklappen" : "Alle aufklappen";
    }

    // Zustand der Einträge der Optimierung im Hintergrund lesen (ab v4.02). Bis v4.01 über den Knopf "Zustand prüfen":
    // Der Arbeitsprozess lief bis zu einer Minute, das Fenster war so lange gesperrt und reagierte nur stockend.
    // Jetzt startet er kurz nach dem Öffnen des Fensters mit niedriger Priorität; die Oberfläche bleibt bedienbar,
    // die Etiketten erscheinen, sobald er fertig ist. Ein Lauf, der inzwischen begonnen hat, hat Vorrang.
    Process optStateProc;
    DateTime optStateStarted = DateTime.MinValue;

    void StartOptStateCheck()
    {
        if (optStateProc != null || running || lblOptState == null || optItems.Count == 0 || String.IsNullOrEmpty(psExe) || String.IsNullOrEmpty(script)) return;
        List<string> got = new List<string>();
        ProcessStartInfo psi = new ProcessStartInfo(psExe, "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\" -EventMode -OptimierungZustand" + (dataDir.Length > 0 ? " -DatenDir \"" + dataDir.TrimEnd('\\') + "\"" : ""));
        psi.UseShellExecute = false; psi.CreateNoWindow = true; psi.RedirectStandardOutput = true; psi.StandardOutputEncoding = new UTF8Encoding(false);
        Process p = new Process(); p.StartInfo = psi;
        bool fertig = false;
        MethodInvoker abschluss = delegate {
            if (fertig) return; fertig = true;
            List<string> lines = new List<string>();
            lock (got)
                foreach (string l in got)
                {
                    if (l.StartsWith("@@LOG|")) { string[] x = l.Split(new char[] { '|' }, 3); if (x.Length == 3 && x[2].Trim().Length > 0) lines.Add(x[2].Trim()); }
                    else if (l.StartsWith("@@OPTZ|")) lines.Add(l);
                }
            try { BeginInvoke(new MethodInvoker(delegate { ApplyOptState(lines); try { p.Dispose(); } catch { } optStateProc = null; })); } catch { }
        };
        p.OutputDataReceived += delegate(object s, DataReceivedEventArgs e) {
            if (e.Data == null) { abschluss(); return; }
            lock (got) got.Add(e.Data);
        };
        try
        {
            p.Start(); p.BeginOutputReadLine();
            try { p.PriorityClass = ProcessPriorityClass.BelowNormal; } catch { }
        }
        catch (Exception ex) { SetText(lblOptState, "Zustand nicht lesbar: " + ex.Message); try { p.Dispose(); } catch { } return; }
        optStateProc = p; optStateStarted = DateTime.Now;
        SetText(lblOptState, "Zustand auf diesem PC wird im Hintergrund gelesen \u2026"); lblOptState.ForeColor = UI.Muted;
        // Sicherung: nach fünf Minuten beenden
        System.Windows.Forms.Timer wd = new System.Windows.Forms.Timer(); wd.Interval = 300000;
        wd.Tick += delegate { wd.Stop(); wd.Dispose(); if (!fertig) { try { if (!p.HasExited) KillProc(p); } catch { } abschluss(); } };
        wd.Start();
    }

    // Laufender Zustandsprozess wird beim Start eines Laufs oder beim Schließen beendet
    void StopOptStateCheck()
    {
        Process p = optStateProc;
        if (p == null) return;
        try { if (!p.HasExited) KillProc(p); } catch { }
    }

    void ApplyOptState(List<string> lines)
    {
        // Ein Lauf seit dem Start der Prüfung zeigt seine eigenen Ergebnisse (@@OPTE); die ältere Prüfung nicht darüberlegen
        if (running || (runStarted != DateTime.MinValue && runStarted > optStateStarted)) { SetText(lblOptState, ""); return; }
        int a = 0, t = 0, o = 0, nz = 0, np = 0;
        string user = "";
        foreach (string l in lines)
        {
            if (!l.StartsWith("@@OPTZ|")) { if (l.StartsWith("Benutzereinstellungen")) user = l; continue; }
            string[] x = l.Split(new char[] { '|' }, 4);
            if (x.Length < 3) continue;
            OptItem it = optItems.Find(delegate(OptItem i) { return i.Id == x[1]; });
            if (it == null || it.Box == null) continue;
            it.Zustand = x[2]; it.ZustandText = x.Length > 3 ? x[3].Replace("¦", "|") : "";
            if (it.Zustand == "aktiv") a++; else if (it.Zustand == "teilweise") t++; else if (it.Zustand == "offen") o++; else if (it.Zustand == "nicht zutreffend") nz++; else np++;
            SperreOptEintrag(it, it.Zustand == "aktiv");
            it.Box.Text = OptBoxText(it); Tip(it.Box, OptTipText(it));
            if (it.Row != null) { string tt = OptTipText(it); Tip(it.Row, tt); if (it.Toggle != null) Tip(it.Toggle, tt); }
        }
        SetText(lblOptState, (a + t + o + nz + np) == 0 ? "Zustand nicht lesbar: keine Rückmeldung vom Arbeitsprozess." : "Zustand auf diesem PC: " + a + " bereits aktiv (Schalter gesperrt), " + t + " teilweise, " + o + " offen, " + nz + " nicht zutreffend, " + np + " nicht prüfbar (Bereinigung, Werkzeuge)." + (user.Length > 0 ? "  " + user + "." : ""));
        lblOptState.ForeColor = UI.Muted;
        UpdateOptCounts();
        UpdateSummary();
    }

    // Ergebnis eines Eintrags während des Laufs (@@OPTE|Id|Ergebnis|Änderungen|Details): Häkchen zeigt den neuen Stand
    void UpdateOptResult(string[] p)
    {
        OptItem it = optItems.Find(delegate(OptItem i) { return i.Id == Get(p, 1); });
        if (it == null || it.Box == null) return;
        string erg = Get(p, 2);
        it.Zustand = (erg == "angewendet" || erg == "bereits so" || erg == "bereits aktiv") ? "aktiv" : erg;
        it.ZustandText = Get(p, 4).Replace("¦", "|");
        // ab v4.05: angewendete und bereits aktive Einträge danach gesperrt
        SperreOptEintrag(it, it.Zustand == "aktiv");
        it.Box.Text = OptBoxText(it); Tip(it.Box, OptTipText(it));
        if (it.Row != null) { string tt = OptTipText(it); Tip(it.Row, tt); if (it.Toggle != null) Tip(it.Toggle, tt); }
        UpdateOptCounts();
    }

    void FetchOptTools()
    {
        if (MessageBox.Show(this, "Display Driver Uninstaller (aus dem Windows Optimisation Pack, feste SHA-256-Prüfsumme) und NVIDIA Profile Inspector (neueste Version von GitHub, Prüfsumme wird beim Holen festgehalten) in den Tools-Ordner holen?\r\n\r\nAuf diesem PC wird nichts installiert.", "Werkzeuge für die Optimierung", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        List<string> lines; string res = RunHelperPumped("-OptWerkzeugeHolen", out lines, 900);
        MessageBox.Show(this, lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Keine Rückmeldung vom Arbeitsprozess.", "Werkzeuge für die Optimierung", MessageBoxButtons.OK, res == "1" ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    // Argumente für den Arbeitsprozess; null = nicht starten. Eingriffe und Neustarts gehen in die Zusammenfassung vor dem
    // Start (ab v3.64 statt einer eigenen Rückfrage der Optimierung)
    string OptStartArgs(List<StartPunkt> punkte, List<string> zusatz)
    {
        List<string> ids = SelectedOptIds();
        if (ids.Count == 0) { MessageBox.Show(this, "In der Optimierung ist kein Eintrag ausgewählt.", "Leos Minibench", MessageBoxButtons.OK, MessageBoxIcon.Information); ShowPage(5); return null; }
        int ver = 0;
        foreach (OptItem it in optItems)
        {
            if (!it.Box.Checked || it.Gesperrt) continue;
            if (it.Verwaltet && DomainPc) ver++;
            if (punkte != null) punkte.Add(new StartPunkt("Optimierung", it.Titel, it.Risiko, it.Neustart));
        }
        if (ver > 0 && zusatz != null) zusatz.Add("Domänen-PC: " + ver + " gewählte Einträge der Optimierung berühren Gruppenrichtlinien und können beim nächsten Abgleich überschrieben werden.");
        return " -Optimierungen " + String.Join(",", ids.ToArray()) + (chkOptRestore.Checked ? "" : " -OptOhneWiederherstellungspunkt");
    }

    // Dialog vor dem Start mit Ablauf, Eingriffen, Neustarts und Wiederherstellungspunkt
    bool ConfirmStart(string text, List<StartPunkt> punkte)
    {
        using (Form f = new Form())
        {
            f.Text = "Start bestätigen"; f.FormBorderStyle = FormBorderStyle.FixedDialog; f.MaximizeBox = false; f.MinimizeBox = false; f.ShowInTaskbar = false;
            f.StartPosition = FormStartPosition.CenterParent; f.Font = new Font("Segoe UI", 9.5f); f.BackColor = UI.Bg; f.AutoSize = true; f.AutoSizeMode = AutoSizeMode.GrowAndShrink; f.Padding = new Padding(UI.S(18));
            FlowLayoutPanel p = new FlowLayoutPanel(); p.FlowDirection = FlowDirection.TopDown; p.AutoSize = true; p.WrapContents = false; p.Dock = DockStyle.Fill;
            bool ein = punkte != null && punkte.Exists(delegate(StartPunkt x) { return x.IstEingriff; });
            Label h = new Label(); h.AutoSize = true; h.Font = new Font("Segoe UI Semibold", 11f); h.Margin = new Padding(0, 0, 0, UI.S(10));
            h.Text = ein ? "Dieser Lauf verändert das System" : "Bitte vor dem Start prüfen";
            p.Controls.Add(h);
            Label l = new Label(); l.AutoSize = true; l.MaximumSize = new Size(UI.S(560), 0); l.Margin = new Padding(0, 0, 0, UI.S(12)); l.Text = text; l.UseMnemonic = false;
            p.Controls.Add(l);
            FlowLayoutPanel b = new FlowLayoutPanel(); b.AutoSize = true; b.Margin = new Padding(0, UI.S(4), 0, 0);
            Button ok = UI.Primary("Starten"); ok.DialogResult = DialogResult.OK; Button ab = UI.Secondary("Abbrechen"); ab.DialogResult = DialogResult.Cancel;
            b.Controls.Add(ok); b.Controls.Add(ab); p.Controls.Add(b);
            f.Controls.Add(p); f.AcceptButton = ab; f.CancelButton = ab; f.ActiveControl = ab;
            UI.ThemeDialog(f);
            return f.ShowDialog(this) == DialogResult.OK;
        }
    }
}
