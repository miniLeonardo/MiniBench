// Einstellungsdatei, Dialog Einstellungen, Fenstergröße, Netzlaufwerk-Dialog (Teilklasse DiagGui)
public partial class DiagGui
{
    string SettingsFile { get { return dataDir.Length > 0 ? Path.Combine(dataDir, "Einstellungen.json") : ""; } }


    // ab v4.07: einzelne Werte der Einstellungsdatei lesen und schreiben (Akzentfarbe, Animationen). Die Datei bleibt
    // wie bisher ein flaches JSON-Objekt; unbekannte Schlüssel anderer Versionen bleiben beim Schreiben erhalten.
    public static Dictionary<string, object> LiesEinstellungen(string datei)
    {
        try
        {
            if (!String.IsNullOrEmpty(datei) && File.Exists(datei))
            {
                Dictionary<string, object> d = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(File.ReadAllText(datei, Encoding.UTF8)) as Dictionary<string, object>;
                if (d != null) return d;
            }
        }
        catch { }
        return new Dictionary<string, object>();
    }

    public static object LiesEinstellung(string datei, string key)
    {
        object v; return LiesEinstellungen(datei).TryGetValue(key, out v) ? v : null;
    }

    public static void SchreibeEinstellung(string datei, string key, object value)
    {
        try
        {
            if (String.IsNullOrEmpty(datei)) return;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> d = null;
            if (File.Exists(datei)) { try { d = js.DeserializeObject(File.ReadAllText(datei, Encoding.UTF8)) as Dictionary<string, object>; } catch { } }
            if (d == null) d = new Dictionary<string, object>();
            d[key] = value;
            File.WriteAllText(datei, js.Serialize(d), Encoding.UTF8);
        }
        catch { }
    }

    // Akzentfarbe und Animationen aus der Einstellungsdatei übernehmen (vor UI.SetTheme aufrufen)
    void LoadAkzentPreference()
    {
        Dictionary<string, object> d = LiesEinstellungen(SettingsFile);
        object a, b;
        UI.Akzent = UI.AkzentGueltig(d.TryGetValue("Akzent", out a) ? a as string : null);
        try { Bewegung.Aktiv = !d.TryGetValue("Animationen", out b) || b == null || Convert.ToBoolean(b); } catch { Bewegung.Aktiv = true; }
    }

    // Akzentfarbe sofort anwenden und speichern
    void SetAkzent(string name)
    {
        name = UI.AkzentGueltig(name);
        if (name == UI.Akzent) return;
        UI.Akzent = name;
        SchreibeEinstellung(SettingsFile, "Akzent", name);
        WendeFarbschemaAn(UI.IsDark, false);
    }

    bool LoadThemePreference()
    {
        try
        {
            if (SettingsFile.Length > 0 && File.Exists(SettingsFile))
            {
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>;
                if (d != null && d.ContainsKey("DarkMode"))
                {
                    return Convert.ToBoolean(d["DarkMode"]);
                }
            }
        }
        catch { }
        return DetectWindowsDarkTheme();
    }

    void SaveThemePreference(bool dark)
    {
        try
        {
            if (SettingsFile.Length == 0) return;
            Dictionary<string, object> d = null;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            if (File.Exists(SettingsFile))
            {
                try { d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>; } catch { }
            }
            if (d == null) d = new Dictionary<string, object>();
            d["DarkMode"] = dark;
            File.WriteAllText(SettingsFile, js.Serialize(d), Encoding.UTF8);
        }
        catch { }
    }

    void LoadPsuPreference()
    {
        try
        {
            if (SettingsFile.Length > 0 && File.Exists(SettingsFile))
            {
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>;
                if (d != null)
                {
                    if (d.ContainsKey("PsuWattage")) psuWattage = Convert.ToInt32(d["PsuWattage"]);
                    if (d.ContainsKey("PsuRating")) psuRating = Convert.ToString(d["PsuRating"]);
                }
            }
        }
        catch { }
    }

    bool LoadStartPagePreference()
    {
        try
        {
            if (SettingsFile.Length > 0 && File.Exists(SettingsFile))
            {
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>;
                if (d != null && d.ContainsKey("StartseiteZeigen"))
                {
                    return Convert.ToBoolean(d["StartseiteZeigen"]);
                }
            }
        }
        catch { }
        return true;
    }

    void SaveStartPagePreference(bool show)
    {
        try
        {
            if (SettingsFile.Length == 0) return;
            Dictionary<string, object> d = null;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            if (File.Exists(SettingsFile))
            {
                try { d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>; } catch { }
            }
            if (d == null) d = new Dictionary<string, object>();
            d["StartseiteZeigen"] = show;
            File.WriteAllText(SettingsFile, js.Serialize(d), Encoding.UTF8);
        }
        catch { }
    }

    string LoadSeenVersionPreference()
    {
        try
        {
            if (SettingsFile.Length > 0 && File.Exists(SettingsFile))
            {
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>;
                if (d != null && d.ContainsKey("GeseheneVersion"))
                {
                    return Convert.ToString(d["GeseheneVersion"]);
                }
            }
        }
        catch { }
        return "";
    }

    void SaveSeenVersionPreference(string ver)
    {
        try
        {
            if (SettingsFile.Length == 0) return;
            Dictionary<string, object> d = null;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            if (File.Exists(SettingsFile))
            {
                try { d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>; } catch { }
            }
            if (d == null) d = new Dictionary<string, object>();
            d["GeseheneVersion"] = ver;
            File.WriteAllText(SettingsFile, js.Serialize(d), Encoding.UTF8);
        }
        catch { }
    }

    bool LoadUpdateCheckPreference()
    {
        try
        {
            if (SettingsFile.Length > 0 && File.Exists(SettingsFile))
            {
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>;
                if (d != null && d.ContainsKey("UpdatePruefung"))
                {
                    return Convert.ToBoolean(d["UpdatePruefung"]);
                }
            }
        }
        catch { }
        return true;
    }

    void SaveUpdateCheckPreference(bool enabled)
    {
        try
        {
            if (SettingsFile.Length == 0) return;
            Dictionary<string, object> d = null;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            if (File.Exists(SettingsFile))
            {
                try { d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>; } catch { }
            }
            if (d == null) d = new Dictionary<string, object>();
            d["UpdatePruefung"] = enabled;
            File.WriteAllText(SettingsFile, js.Serialize(d), Encoding.UTF8);
        }
        catch { }
    }


    // ab v4.01 im Aufbau der Seiten Wartung und Optimierung: Bereiche links, Inhalt als Karte rechts, Titelleiste im
    // Farbschema. Übernommen wird beim Schließen.
    // Einstellungen (ab v4.02 eine Seite): drei Gruppen als Karten mit je einer Zeile pro Option,
    // links Titel und Erklärung, rechts das Bedienelement. Vorher ein Reiter je Option.
    void ShowSettingsDialog()
    {
        using (Form dlg = new Form())
        {
            dlg.Text = "Einstellungen";
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false; dlg.MinimizeBox = false; dlg.ShowInTaskbar = false;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ClientSize = new Size(UI.S(660), Math.Min(UI.S(640), Screen.FromControl(this).WorkingArea.Height - UI.S(80)));
            dlg.Font = new Font("Segoe UI", 9.5f);

            TableLayoutPanel body = new TableLayoutPanel();
            body.Dock = DockStyle.Fill; body.ColumnCount = 1; body.AutoScroll = true; body.BackColor = Color.Transparent;
            body.Padding = new Padding(UI.S(20), UI.S(12), UI.S(20), 0);
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            int textW = UI.S(360);

            Func<string, TableLayoutPanel> Gruppe = delegate(string titel)
            {
                Label lh = Lbl(titel, UI.SchriftText, true, UI.Muted); lh.Margin = new Padding(UI.S(2), UI.S(8), 0, UI.S(6));
                body.Controls.Add(lh);
                CardPanel c = new CardPanel(); c.AutoSize = true; c.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                c.Padding = new Padding(UI.S(16), UI.S(4), UI.S(16), UI.S(4)); c.Margin = new Padding(0, 0, 0, UI.S(6));
                TableLayoutPanel g = new TableLayoutPanel(); g.Dock = DockStyle.Top; g.AutoSize = true; g.ColumnCount = 2; g.RowCount = 0; g.BackColor = UI.Panel;
                g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); g.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                c.Controls.Add(g); body.Controls.Add(c);
                return g;
            };
            // Zeile: Titel und Erklärung links, Bedienelement rechts; zwischen den Zeilen eine Linie
            Func<TableLayoutPanel, string, string, Control, FlowLayoutPanel> Zeile = delegate(TableLayoutPanel g, string titel, string text, Control ctl)
            {
                if (g.RowCount > 0)
                {
                    Panel sep = new Panel(); sep.Height = 1; sep.BackColor = UI.Line; sep.Dock = DockStyle.Fill; sep.Margin = new Padding(0);
                    g.RowCount++; g.Controls.Add(sep, 0, g.RowCount - 1); g.SetColumnSpan(sep, 2);
                }
                FlowLayoutPanel f = new FlowLayoutPanel(); f.FlowDirection = FlowDirection.TopDown; f.WrapContents = false; f.AutoSize = true; f.BackColor = UI.Panel;
                f.Margin = new Padding(0, UI.S(10), UI.S(16), UI.S(10));
                Label lt = Lbl(titel, UI.SchriftText, true, UI.Text); lt.Margin = new Padding(0, 0, 0, UI.S(2)); f.Controls.Add(lt);
                if (!String.IsNullOrEmpty(text)) { Label ld = Lbl(text, UI.SchriftHinweis, false, UI.Muted); ld.MaximumSize = new Size(textW, 0); ld.Margin = new Padding(0); f.Controls.Add(ld); }
                g.RowCount++;
                g.Controls.Add(f, 0, g.RowCount - 1);
                if (ctl != null) { ctl.Anchor = AnchorStyles.Right; g.Controls.Add(ctl, 1, g.RowCount - 1); }
                return f;
            };

            // Allgemein
            TableLayoutPanel gAll = Gruppe("Allgemein");
            FlowLayoutPanel rowTheme = new FlowLayoutPanel(); rowTheme.AutoSize = true; rowTheme.WrapContents = false; rowTheme.BackColor = UI.Panel; rowTheme.Margin = new Padding(0);
            RadioButton rbDark = new RadioButton(); rbDark.Text = "Dunkel"; rbDark.Checked = UI.IsDark;
            RadioButton rbLight = new RadioButton(); rbLight.Text = "Hell"; rbLight.Checked = !UI.IsDark;
            foreach (RadioButton rb in new RadioButton[] { rbDark, rbLight })
            {
                rb.AutoSize = false; rb.Size = new Size(UI.S(84), UI.S(30)); rb.Margin = new Padding(UI.S(4), 0, 0, 0);
                StyleSegment(rb);
                RadioButton r2 = rb; rb.CheckedChanged += delegate { StyleSegment(r2); };
                rowTheme.Controls.Add(rb);
            }
            Tip(rbDark, "Dunkles Farbschema."); Tip(rbLight, "Helles Farbschema.");
            Zeile(gAll, "Farbschema", "Gilt ab dem Schließen. Der Knopf mit der Sonne oben rechts schaltet ebenfalls um.", rowTheme);

            // ab v4.07: Akzentfarbe (wirkt sofort) und Animationen
            FlowLayoutPanel rowAkzent = new FlowLayoutPanel(); rowAkzent.AutoSize = true; rowAkzent.WrapContents = false; rowAkzent.BackColor = UI.Panel; rowAkzent.Margin = new Padding(0);
            List<FarbPunkt> punkte = new List<FarbPunkt>();
            Label lblAkzent = null;
            foreach (string an in UI.AkzentNamen)
            {
                FarbPunkt fp = new FarbPunkt(an); fp.Margin = new Padding(UI.S(2), 0, 0, 0); fp.Gewaehlt = an == UI.Akzent;
                Tip(fp, an == "Windows" ? "Akzentfarbe aus den Windows-Einstellungen, an das Farbschema angepasst." : an == "Blau" ? "Blau (Standard)." : an + ".");
                string name = an;
                fp.Click += delegate {
                    SetAkzent(name);
                    foreach (FarbPunkt q in punkte) q.Gewaehlt = q.Name2 == UI.Akzent;
                    if (lblAkzent != null) SetText(lblAkzent, AkzentText());
                    UI.ThemeDialog(dlg); StyleSegment(rbDark); StyleSegment(rbLight); dlg.Invalidate(true);
                };
                punkte.Add(fp); rowAkzent.Controls.Add(fp);
            }
            FlowLayoutPanel fAkz = Zeile(gAll, "Akzentfarbe", "Farbe für Schalter, Knöpfe, Markierungen und Verweise. Wirkt sofort.", rowAkzent);
            lblAkzent = Lbl(AkzentText(), UI.SchriftHinweis, false, UI.Text); lblAkzent.Margin = new Padding(0, UI.S(4), 0, 0);
            fAkz.Controls.Add(lblAkzent);

            ToggleSwitch tgAnim = new ToggleSwitch(); tgAnim.Checked = Bewegung.Aktiv;
            Tip(tgAnim, "Kurze Übergänge: Überblendung beim Seitenwechsel, Punktzahl, Balken und Ringe der Ergebnisseite laufen auf ihren Wert. Aus: alles erscheint sofort.");
            Zeile(gAll, "Animationen", "Kurze, dezente Übergänge. Bleiben aus, wenn Windows Animationen abgeschaltet hat oder eine Remotesitzung läuft.", tgAnim);

            ToggleSwitch tgStart = new ToggleSwitch(); tgStart.Checked = LoadStartPagePreference();
            Tip(tgStart, "Aus: Leos Minibench öffnet direkt die Seite Diagnose.");
            Zeile(gAll, "Startseite beim Programmstart", "Aus: Das Programm öffnet direkt die Seite Diagnose.", tgStart);

            ToggleSwitch tgUpd = new ToggleSwitch(); tgUpd.Checked = LoadUpdateCheckPreference();
            Tip(tgUpd, "Prüft beim Start im Hintergrund, ob eine neuere Version vorliegt. Heruntergeladen wird nur auf Nachfrage.");
            FlowLayoutPanel fUpd = Zeile(gAll, "Nach Updates suchen", "Beim Start höchstens alle zwei Stunden bei GitHub, heruntergeladen wird nur auf Nachfrage. Installiert: " + this.version + (HasUpdateAvailable ? ", verfügbar: " + AvailableUpdateVersion.TrimStart('v') : "") + ".", tgUpd);
            LinkLabel lnkUpd = new LinkLabel(); lnkUpd.Text = "Jetzt suchen"; lnkUpd.AutoSize = true; lnkUpd.LinkColor = UI.Accent; lnkUpd.ActiveLinkColor = UI.AccentHover; lnkUpd.LinkBehavior = LinkBehavior.HoverUnderline; lnkUpd.Margin = new Padding(0, UI.S(4), 0, 0);
            lnkUpd.LinkClicked += delegate { StartUpdateCheck(true); lnkUpd.Text = "Prüfung läuft, das Ergebnis erscheint gleich"; lnkUpd.Enabled = false; };
            Tip(lnkUpd, "Fragt sofort bei GitHub nach einer neueren Version.");
            fUpd.Controls.Add(lnkUpd);

            // Daten und Datenschutz
            TableLayoutPanel gDat = Gruppe("Daten und Datenschutz");
            FlowLayoutPanel rowDataBtns = new FlowLayoutPanel(); rowDataBtns.AutoSize = true; rowDataBtns.WrapContents = false; rowDataBtns.BackColor = UI.Panel; rowDataBtns.Margin = new Padding(0);
            Button btnOpenData = UI.Secondary("Öffnen"); btnOpenData.Margin = new Padding(0, 0, UI.S(8), 0);
            btnOpenData.Click += delegate { if (dataDir.Length > 0) OpenShell(dataDir); };
            Button btnNasDlg = UI.Secondary("Netzlaufwerk …"); btnNasDlg.Margin = new Padding(0);
            Tip(btnOpenData, "Öffnet den Datenordner im Explorer."); Tip(btnNasDlg, "Netzlaufwerk als Spiegel der Daten einrichten oder ändern. Abgeglichen wird über Aktualisieren in der Datenbank.");
            rowDataBtns.Controls.Add(btnOpenData); rowDataBtns.Controls.Add(btnNasDlg);
            FlowLayoutPanel fDat = Zeile(gDat, "Datenordner", "Berichte, Datenbank, Werkzeuge und Vorlagen.", rowDataBtns);
            Label lblDataDir = Lbl(NasAblage.AblageText(dataDir), UI.SchriftHinweis, false, UI.Text); lblDataDir.MaximumSize = new Size(textW, 0); lblDataDir.Margin = new Padding(0, UI.S(4), 0, 0);
            fDat.Controls.Add(lblDataDir);
            btnNasDlg.Click += delegate { ShowConnectNasDialog(); SetText(lblDataDir, NasAblage.AblageText(dataDir)); };

            ToggleSwitch tgAnon = new ToggleSwitch(); tgAnon.Checked = chkAnon != null && chkAnon.Checked;
            Tip(tgAnon, "Ersetzt Benutzernamen, Computernamen, Seriennummern und Domäne durch neutrale Platzhalter.");
            Zeile(gDat, "KI-Datei anonymisieren", "Entfernt Benutzer- und Computernamen, Seriennummern und Domäne aus der Datei zur KI-Auswertung, die jeder Lauf anlegt.", tgAnon);

            // Über
            TableLayoutPanel gUeber = Gruppe("Über");
            Button btnHist = UI.Secondary("Versionshistorie"); btnHist.Margin = new Padding(0);
            btnHist.Click += delegate { dlg.Close(); if (!running && versionPage >= 0) ShowPage(versionPage); };
            Tip(btnHist, "Schließt die Einstellungen und zeigt, was sich in jeder Version geändert hat.");
            Zeile(gUeber, "Leos Minibench " + this.version, "Danksagungen: Windows Optimisation Pack (Marvin700), Sophia Script (Dmitry Nefedov), O&O ShutUp10++, smartmontools, LibreHardwareMonitor, PawnIO.", btnHist);

            FlowLayoutPanel bottomBar = new FlowLayoutPanel();
            bottomBar.Dock = DockStyle.Bottom; bottomBar.FlowDirection = FlowDirection.RightToLeft; bottomBar.WrapContents = false;
            bottomBar.Height = UI.S(60); bottomBar.Padding = new Padding(0, UI.S(12), UI.S(20), UI.S(12)); bottomBar.BackColor = Color.Transparent;
            Button btnOk = UI.Primary("Schließen"); btnOk.DialogResult = DialogResult.OK; btnOk.Margin = new Padding(0);
            Tip(btnOk, "Übernimmt die Einstellungen und schließt das Fenster.");
            bottomBar.Controls.Add(btnOk);

            dlg.Controls.Add(body);
            dlg.Controls.Add(bottomBar);
            dlg.AcceptButton = btnOk; dlg.CancelButton = btnOk;
            UI.ThemeDialog(dlg);
            StyleSegment(rbDark); StyleSegment(rbLight);
            // Höhe an den Inhalt anpassen, höchstens bis zur Bildschirmhöhe
            int need = body.GetPreferredSize(new Size(dlg.ClientSize.Width, 0)).Height + bottomBar.Height + UI.S(8);
            dlg.ClientSize = new Size(dlg.ClientSize.Width, Math.Min(need, Screen.FromControl(this).WorkingArea.Height - UI.S(80)));

            dlg.ShowDialog(this);

            if (rbDark.Checked != UI.IsDark) ToggleTheme();
            SaveStartPagePreference(tgStart.Checked);
            SaveUpdateCheckPreference(tgUpd.Checked);
            if (tgAnim.Checked != Bewegung.Aktiv) { Bewegung.Aktiv = tgAnim.Checked; SchreibeEinstellung(SettingsFile, "Animationen", Bewegung.Aktiv); }
            if (chkAnon != null) chkAnon.Checked = tgAnon.Checked;
        }
    }

    static string AkzentText() { return "Gewählt: " + (UI.Akzent == "Windows" ? "wie Windows" : UI.Akzent); }

    void SaveWindowPreferences()
    {
        try
        {
            if (SettingsFile.Length == 0) return;
            Dictionary<string, object> d = null;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            if (File.Exists(SettingsFile))
            {
                try { d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>; } catch { }
            }
            if (d == null) d = new Dictionary<string, object>();

            Rectangle b = (WindowState == FormWindowState.Normal) ? Bounds : RestoreBounds;
            if (b.Width > 0 && b.Height > 0)
            {
                d["WindowX"] = b.X;
                d["WindowY"] = b.Y;
                d["WindowW"] = b.Width;
                d["WindowH"] = b.Height;
            }
            d["WindowState"] = (int)WindowState;
            d["LastPage"] = curPage;
            File.WriteAllText(SettingsFile, js.Serialize(d), Encoding.UTF8);
        }
        catch { }
    }

    void LoadWindowPreferences()
    {
        try
        {
            if (SettingsFile.Length > 0 && File.Exists(SettingsFile))
            {
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(SettingsFile, Encoding.UTF8)) as Dictionary<string, object>;
                if (d != null)
                {
                    if (d.ContainsKey("WindowX") && d.ContainsKey("WindowY") && d.ContainsKey("WindowW") && d.ContainsKey("WindowH"))
                    {
                        int x = Convert.ToInt32(d["WindowX"]);
                        int y = Convert.ToInt32(d["WindowY"]);
                        int w = Convert.ToInt32(d["WindowW"]);
                        int h = Convert.ToInt32(d["WindowH"]);
                        Rectangle r = new Rectangle(x, y, w, h);
                        bool onScreen = false;
                        foreach (Screen s in Screen.AllScreens)
                        {
                            if (s.WorkingArea.IntersectsWith(r)) { onScreen = true; break; }
                        }
                        if (onScreen && w >= UI.S(600) && h >= UI.S(400))
                        {
                            StartPosition = FormStartPosition.Manual;
                            Bounds = r;
                        }
                    }
                    if (d.ContainsKey("WindowState"))
                    {
                        int ws = Convert.ToInt32(d["WindowState"]);
                        if (ws == (int)FormWindowState.Maximized) WindowState = FormWindowState.Maximized;
                    }
                }
            }
        }
        catch { }
    }


    // Netzlaufwerk einrichten (ab v3.54): nur Pfad und Benutzer in Minibench-Daten\Netzwerk.json auf dem Stick. Die Ablage
    // bleibt lokal; abgeglichen wird über Aktualisieren auf der Seite Vergleichsdatenbank. Ein Kennwort wird nicht
    // gespeichert, die Verbindung (WNetAddConnection2) ist nicht dauerhaft und endet spätestens mit dem Programm.
    public void ShowConnectNasDialog()
    {
        string altPfad, altBenutzer;
        bool hatKonfig = NasAblage.LeseKonfig(dataDir, out altPfad, out altBenutzer);
        using (Form dlg = new Form())
        {
            dlg.Text = "Netzlaufwerk für den Abgleich einrichten";
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false; dlg.MinimizeBox = false; dlg.ShowInTaskbar = false;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.Font = new Font("Segoe UI", 9.5f);
            dlg.BackColor = UI.Bg; dlg.ForeColor = UI.Text;
            dlg.ClientSize = new Size(UI.S(540), UI.S(400));

            FlowLayoutPanel p = new FlowLayoutPanel();
            p.Dock = DockStyle.Fill; p.FlowDirection = FlowDirection.TopDown; p.WrapContents = false; p.Padding = new Padding(UI.S(16));

            Label lInfo = Lbl("Berichte, Datenbank und Änderungsprotokolle bleiben immer auf dem Stick. Ein Netzlaufwerk ist ein Spiegel dieser Daten: \"Aktualisieren\" auf der Seite Vergleichsdatenbank gleicht beide Seiten ab. Ohne Netzlaufwerk (unterwegs) funktioniert alles wie gewohnt.", 9f, false, UI.Muted);
            lInfo.MaximumSize = new Size(UI.S(500), 0); lInfo.Margin = new Padding(0, 0, 0, UI.S(12));
            p.Controls.Add(lInfo);

            Label lPath = Lbl("Ordner auf dem Netzlaufwerk (\\\\server\\freigabe\\Ordner):", 9.5f, true, UI.Text); lPath.Margin = new Padding(0, 0, 0, UI.S(4)); p.Controls.Add(lPath);
            TextBox tbPath = new TextBox(); tbPath.Width = UI.S(500); tbPath.Margin = new Padding(0, 0, 0, UI.S(10));
            tbPath.Text = hatKonfig ? altPfad : @"\\NAS\Freigabe\Minibench";
            Tip(tbPath, "UNC-Pfad des Ordners auf dem NAS, zum Beispiel \\\\TRUENAS\\Multimedia\\MiniBench. Ein Pfad mit nur einem \\ am Anfang zeigt auf den Stick und wird abgelehnt.");
            p.Controls.Add(tbPath);

            Label lUser = Lbl("Benutzername (optional):", 9f, false, UI.Text); lUser.Margin = new Padding(0, 0, 0, UI.S(2)); p.Controls.Add(lUser);
            TextBox tbUser = new TextBox(); tbUser.Width = UI.S(500); tbUser.Margin = new Padding(0, 0, 0, UI.S(8)); tbUser.Text = altBenutzer;
            Tip(tbUser, "Nur nötig, wenn Windows die Freigabe nicht mit der eigenen Anmeldung öffnet. Wird in Netzwerk.json gespeichert, das Kennwort nicht.");
            p.Controls.Add(tbUser);

            Label lPass = Lbl("Kennwort (optional, wird nicht gespeichert):", 9f, false, UI.Text); lPass.Margin = new Padding(0, 0, 0, UI.S(2)); p.Controls.Add(lPass);
            TextBox tbPass = new TextBox(); tbPass.Width = UI.S(500); tbPass.UseSystemPasswordChar = true; tbPass.Margin = new Padding(0, 0, 0, UI.S(14));
            Tip(tbPass, "Stellt die Verbindung nur für diese Sitzung her (nicht dauerhaft). Beim Aktualisieren fragt Leos Minibench bei Bedarf erneut.");
            p.Controls.Add(tbPass);

            FlowLayoutPanel rowButtons = Row();
            Button btnOk = UI.Primary("Speichern und prüfen"); btnOk.Margin = new Padding(0);
            Tip(btnOk, "Prüft den Pfad, verbindet bei Bedarf mit Benutzer und Kennwort und speichert den Pfad in Netzwerk.json auf dem Stick.");
            Button btnRemove = UI.Secondary("Netzlaufwerk entfernen"); btnRemove.Margin = new Padding(UI.S(8), 0, 0, 0); btnRemove.Enabled = hatKonfig;
            Tip(btnRemove, "Löscht Netzwerk.json. Die Daten auf Stick und Netzlaufwerk bleiben unverändert, es wird nur nicht mehr abgeglichen.");
            Button btnCancel = UI.Secondary("Abbrechen"); btnCancel.Margin = new Padding(UI.S(8), 0, 0, 0);
            Tip(btnCancel, "Schließt den Dialog ohne Änderungen.");
            btnCancel.Click += delegate { dlg.Close(); };
            bool jetztAbgleichen = false;

            btnRemove.Click += delegate {
                if (MessageBox.Show(dlg, "Netzlaufwerk " + altPfad + " entfernen? Die Daten bleiben auf beiden Seiten erhalten, es wird nur nicht mehr abgeglichen.", "Netzlaufwerk entfernen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                try { NasAblage.LoescheKonfig(dataDir); } catch (Exception ex) { MessageBox.Show(dlg, ex.Message, "Netzlaufwerk entfernen", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                dlg.Close();
            };

            btnOk.Click += delegate {
                string pfad = NasAblage.Vereinheitlichen(tbPath.Text);
                string fehler = NasAblage.PfadFehler(pfad);
                if (fehler.Length > 0) { MessageBox.Show(dlg, "Der Pfad ist nicht verwendbar: " + fehler, "Netzlaufwerk einrichten", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (dataDir.Length == 0) { MessageBox.Show(dlg, "Es gibt keinen Datenordner auf dem Stick.", "Netzlaufwerk einrichten", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                Cursor = Cursors.WaitCursor; dlg.Cursor = Cursors.WaitCursor;
                string hinweis = "";
                if (tbPass.Text.Length > 0 || tbUser.Text.Trim().Length > 0)
                {
                    string err = NasAblage.Verbinden(pfad, tbUser.Text.Trim(), tbPass.Text);
                    if (err.Length > 0) hinweis = err;
                }
                dlg.Enabled = false;
                bool ok = NasAblage.OrdnerBereit(pfad, 6000);
                dlg.Enabled = true;
                Cursor = Cursors.Default; dlg.Cursor = Cursors.Default;
                if (!ok)
                {
                    string m = "Der Ordner auf dem Netzlaufwerk ist gerade nicht erreichbar oder lässt sich nicht anlegen" + (hinweis.Length > 0 ? ": " + hinweis : ".") + "\r\n\r\nTrotzdem speichern? Abgeglichen wird dann beim nächsten Aktualisieren im Heimnetz.";
                    if (MessageBox.Show(dlg, m, "Netzlaufwerk einrichten", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                }
                try { NasAblage.SchreibeKonfig(dataDir, pfad, tbUser.Text.Trim()); }
                catch (Exception ex) { MessageBox.Show(dlg, "Netzwerk.json lässt sich nicht schreiben: " + ex.Message, "Netzlaufwerk einrichten", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (ok) jetztAbgleichen = MessageBox.Show(dlg, "Netzlaufwerk gespeichert und erreichbar.\r\n\r\nJetzt abgleichen?", "Netzlaufwerk einrichten", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            };

            rowButtons.Controls.Add(btnOk); rowButtons.Controls.Add(btnRemove); rowButtons.Controls.Add(btnCancel);
            p.Controls.Add(rowButtons);
            dlg.Controls.Add(p);
            dlg.AcceptButton = btnOk; dlg.CancelButton = btnCancel;
            UI.ThemeDialog(dlg);
            dlg.ShowDialog(this);
            if (lblDbPath != null) SetText(lblDbPath, NasAblage.AblageText(dataDir));
            if (jetztAbgleichen) RefreshAndSync();
        }
    }

    // Kennwort für das Netzlaufwerk abfragen (wird nicht gespeichert); null = abgebrochen
    string PromptPassword(string pfad, string benutzer)
    {
        using (Form dlg = new Form())
        {
            dlg.Text = "Netzlaufwerk verbinden"; dlg.FormBorderStyle = FormBorderStyle.FixedDialog; dlg.MaximizeBox = false; dlg.MinimizeBox = false;
            dlg.StartPosition = FormStartPosition.CenterParent; dlg.ClientSize = new Size(UI.S(440), UI.S(150)); dlg.BackColor = UI.Bg; dlg.Font = new Font("Segoe UI", 9f);
            Label l = new Label(); l.Text = "Kennwort für " + benutzer + " auf " + NasAblage.Freigabe(pfad) + ":"; l.Location = new Point(UI.S(16), UI.S(14)); l.Size = new Size(UI.S(408), UI.S(36));
            TextBox tb = new TextBox(); tb.UseSystemPasswordChar = true; tb.Location = new Point(UI.S(16), UI.S(54)); tb.Size = new Size(UI.S(408), UI.S(24));
            Button ok = UI.Primary("Verbinden"); ok.Location = new Point(UI.S(226), UI.S(98)); ok.Size = new Size(UI.S(95), UI.S(32)); ok.DialogResult = DialogResult.OK;
            Button ab = UI.Secondary("Abbrechen"); ab.Location = new Point(UI.S(328), UI.S(98)); ab.Size = new Size(UI.S(96), UI.S(32)); ab.DialogResult = DialogResult.Cancel;
            dlg.Controls.Add(l); dlg.Controls.Add(tb); dlg.Controls.Add(ok); dlg.Controls.Add(ab); dlg.AcceptButton = ok; dlg.CancelButton = ab;
            UI.ThemeDialog(dlg);
            return dlg.ShowDialog(this) == DialogResult.OK ? tb.Text : null;
        }
    }
}
