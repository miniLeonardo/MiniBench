// Seite Änderungen (Teilklasse DiagGui)
public partial class DiagGui
{
    // ab v4.10: Die Seite zeigt zuerst nur die Läufe dieses PCs, gebündelt nach Lauf und Maßnahme; die einzelnen Werte
    // (Ziel, vorher, nachher) entstehen erst beim Aufklappen. Bis v4.09 stand hier ein Listenfeld mit einer Zeile je
    // Eintrag aller PCs: Bei rund tausend Einträgen zählte es beim ersten Anzeigen für jede Zeile alle Haken neu.
    public class ChgZeile : ListenZeile
    {
        public ChangeLauf Lauf; public ChangeMassnahme Mass; public ChangeEntry Eintrag;
        public string Schluessel { get { return Art + "|" + (Lauf != null ? Lauf.FilePath : "") + "|" + (Mass != null ? Mass.Key : "") + "|" + (Eintrag != null ? Eintrag.Id.ToString() : ""); } }
    }

    // Maßnahmen eines Laufs, die ohne "weitere anzeigen" sichtbar sind
    public const int ChgMassnahmenZuerst = 12;
    internal ListenSeite chgSeite;
    internal FeldAuswahl chgUmfang;
    internal bool chgAllePcs;
    internal HashSet<string> chgHaken = new HashSet<string>(), chgOffen = new HashSet<string>(), chgAlles = new HashSet<string>();
    bool chgErsterAufbau = true;
    Button btnChgClean;
    // Dauer des letzten Aufbaus der Liste (Start.log, Selbsttest)
    internal long chgAufbauMs;

    Control BuildChangePage()
    {
        chgSeite = new ListenSeite("Änderungen", "Alles, was Leos Minibench an einem PC verändert hat, mit Vorher-Wert. Einträge der Stufe Ändern lassen sich auf dem PC, auf dem sie entstanden sind, zurücknehmen. Eingriffe stehen mit dem Weg zurück als Hinweis in der Liste.", "Rückgängig machen");
        listenSeiten.Add(chgSeite);
        btnUndo = chgSeite.BandKnopf;
        btnUndo.Click += delegate { UndoSelected(); };
        Tip(btnUndo, "Nimmt die angehakten Änderungen auf diesem PC zurück (mit Rückfrage).");
        chgSeite.BandVerweis.LinkClicked += delegate { chgHaken.Clear(); FillChangeList(); };
        Tip(chgSeite.BandVerweis, "Entfernt alle Haken.");

        chgUmfang = new FeldAuswahl(new string[] { "Dieser PC", "Alle PCs" }, new string[] { "", "" });
        chgUmfang.Size = new Size(UI.S(300), UI.S(46)); chgUmfang.Margin = new Padding(0);
        chgUmfang.SelectedChanged += delegate { chgAllePcs = chgUmfang.Selected == 1; FillChangeList(); };
        Tip(chgUmfang, "Dieser PC: nur Läufe, die hier entstanden sind und sich hier zurücknehmen lassen. Alle PCs: zusätzlich die Protokolle anderer PCs aus dem Datenordner, nur zur Ansicht.");
        chgSeite.Links.Controls.Add(chgUmfang);

        Button rel = UI.Secondary("Aktualisieren"); rel.Margin = new Padding(0); rel.Click += delegate { ReloadDb(); };
        Button open = UI.Secondary("Protokollordner"); open.Click += delegate { if (changeDir.Length > 0 && Directory.Exists(changeDir)) OpenShell(changeDir); else if (dataDir.Length > 0) OpenShell(dataDir); };
        btnChgClean = UI.Secondary("Aufräumen …"); btnChgClean.Click += delegate { ChangeAufraeumen(); }; btnChgClean.Enabled = dataDir.Length > 0;
        Tip(rel, "Liest das Änderungsprotokoll neu ein.");
        Tip(open, "Öffnet den Protokollordner" + (changeDir.Length > 0 ? " (" + changeDir + ")." : "."));
        Tip(btnChgClean, "Verschiebt erledigte Läufe und auf Wunsch die Protokolle anderer PCs ins Archiv des Datenordners. Gelöscht wird nichts, aktive Änderungen dieses PCs bleiben immer.");
        chgSeite.Rechts.Controls.Add(rel); chgSeite.Rechts.Controls.Add(open); chgSeite.Rechts.Controls.Add(btnChgClean);

        lvChg = new ZeilenListe(); lvChg.Tipp = tip; lvChg.Zeichner = ZeichneChgZeile; lvChg.ZeilenHoehe = UI.S(32);
        lvChg.Leer = "Auf diesem PC hat Leos Minibench noch nichts verändert.";
        lvChg.HakenGeklickt += delegate(ListenZeile z) { ChgHakenUm((ChgZeile)z); };
        lvChg.AufklappGeklickt += delegate(ListenZeile z) { ChgKlappe((ChgZeile)z); };
        lvChg.Geoeffnet += delegate(ListenZeile z) { if (z.Aufklappbar) ChgKlappe((ChgZeile)z); };
        lvChg.VerweisGeklickt += delegate(ListenZeile z) { ChgVerweis((ChgZeile)z); };
        Tip(lvChg, "Änderungsprotokoll, nach Lauf und Maßnahme gebündelt. Ein Haken an einem Lauf oder einer Maßnahme wählt alle zurücknehmbaren Einträge darunter.");

        Label hint = Lbl("Anhaken lassen sich nur aktive Einträge dieses PCs. Hinweise, bereits Zurückgenommenes und Einträge anderer PCs sind gesperrt.", UI.SchriftHinweis, false, UI.Muted);
        hint.AutoSize = false;
        chgSeite.Fuege(lvChg, null, hint);
        FillChangeList();
        return chgSeite.Seite;
    }

    static string ShortTime(string d)
    {
        DateTime t;
        if (DateTime.TryParseExact(d, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out t)) return t.ToString("dd.MM.yy HH:mm");
        return d;
    }

    static string FormatRunDate(string d)
    {
        if (String.IsNullOrEmpty(d)) return "";
        DateTime t;
        if (DateTime.TryParseExact(d, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out t) ||
            DateTime.TryParse(d, CultureInfo.CurrentCulture, DateTimeStyles.None, out t))
        {
            return t.ToString("dd.MM.yyyy, HH:mm");
        }
        if (DateTime.TryParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out t))
        {
            return t.ToString("dd.MM.yyyy");
        }
        return d;
    }

    static string Mehrzahl(int n, string eins, string viele) { return n.ToString("N0", CultureInfo.GetCultureInfo("de-DE")) + " " + (n == 1 ? eins : viele); }

    // Zustand eines Kästchens aus den zurücknehmbaren Einträgen: 0 aus, 1 an, 2 teilweise; gesperrt ohne solche Einträge
    int ChgHakenStand(List<ChangeEntry> l, out bool gesperrt)
    {
        int n = 0, an = 0;
        foreach (ChangeEntry c in l) if (c.Undoable) { n++; if (chgHaken.Contains(c.Schluessel)) an++; }
        gesperrt = n == 0;
        return an == 0 ? 0 : an == n ? 1 : 2;
    }

    // Baut die sichtbaren Zeilen aus Läufen, aufgeklappten Maßnahmen und deren Einträgen
    internal void FillChangeList()
    {
        if (lvChg == null) return;
        Stopwatch sw = Stopwatch.StartNew();
        // Haken, deren Eintrag es nicht mehr gibt oder der nicht mehr zurücknehmbar ist, entfallen
        HashSet<string> gueltig = new HashSet<string>(); int open = 0, hier = 0;
        foreach (ChangeEntry c in changes) { if (c.Undoable) { open++; gueltig.Add(c.Schluessel); } if (String.Equals(c.Computer, Environment.MachineName, StringComparison.OrdinalIgnoreCase)) hier++; }
        chgHaken.IntersectWith(gueltig);
        string fokus = lvChg.Fokus is ChgZeile ? ((ChgZeile)lvChg.Fokus).Schluessel : "";

        List<ChangeLauf> sicht = new List<ChangeLauf>(); int andereLaeufe = 0, andereEintraege = 0; HashSet<string> anderePcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (ChangeLauf l in changeLaeufe)
        {
            if (l.Eintraege.Count == 0) continue;
            if (l.DieserPc || chgAllePcs) sicht.Add(l);
            if (!l.DieserPc) { andereLaeufe++; andereEintraege += l.Eintraege.Count; anderePcs.Add(l.Computer); }
        }
        // beim ersten Aufbau den neuesten Lauf dieses PCs aufklappen
        if (chgErsterAufbau && changeLaeufe.Count > 0) { chgErsterAufbau = false; foreach (ChangeLauf l in sicht) if (l.DieserPc) { chgOffen.Add(l.FilePath); break; } }

        lvChg.Zeilen.Clear();
        foreach (ChangeLauf l in sicht)
        {
            bool gesperrt; ChgZeile zl = new ChgZeile(); zl.Art = "lauf"; zl.Lauf = l; zl.Tag = l; zl.Grund = 1; zl.Hoehe = UI.S(38); zl.Aufklappbar = true; zl.Offen = chgOffen.Contains(l.FilePath);
            zl.Haken = ChgHakenStand(l.Eintraege, out gesperrt); zl.Gesperrt = gesperrt;
            zl.Hinweis = WrapTip("Lauf vom " + FormatRunDate(l.Zeit) + " auf " + l.Computer + (l.DieserPc ? "" : " (anderer PC, hier nicht zurücknehmbar)") + "\r\nProtokoll: " + l.FilePath);
            lvChg.Zeilen.Add(zl);
            if (!zl.Offen) continue;
            bool alles = chgAlles.Contains(l.FilePath) || l.Massnahmen.Count <= ChgMassnahmenZuerst + 2;
            int zeige = alles ? l.Massnahmen.Count : ChgMassnahmenZuerst;
            for (int i = 0; i < zeige; i++)
            {
                ChangeMassnahme m = l.Massnahmen[i];
                ChgZeile zm = new ChgZeile(); zm.Art = "massnahme"; zm.Lauf = l; zm.Mass = m; zm.Tag = m; zm.Ebene = 1; zm.Aufklappbar = true; zm.Offen = chgOffen.Contains(l.FilePath + "|" + m.Key);
                zm.Haken = ChgHakenStand(m.Eintraege, out gesperrt); zm.Gesperrt = gesperrt;
                zm.Hinweis = WrapTip(ContractStep.RiskLabel(m.Risiko) + " · " + m.Umfang + (m.Eintraege.Count == 1 ? ChgEintragHinweis(m.Eintraege[0]) : ""));
                lvChg.Zeilen.Add(zm);
                if (!zm.Offen) continue;
                foreach (ChangeEntry c in m.Eintraege)
                {
                    ChgZeile ze = new ChgZeile(); ze.Art = "eintrag"; ze.Lauf = l; ze.Mass = m; ze.Eintrag = c; ze.Tag = c; ze.Ebene = 2; ze.Grund = 2; ze.Hoehe = UI.S(26);
                    ze.Haken = chgHaken.Contains(c.Schluessel) ? 1 : 0; ze.Gesperrt = !c.Undoable; ze.Blass = !c.Undoable;
                    ze.Hinweis = WrapTip(ContractStep.RiskLabel(c.Risiko) + " · " + c.Art + " · " + c.Status + "\r\n" + c.Ziel + ChgEintragHinweis(c));
                    lvChg.Zeilen.Add(ze);
                }
            }
            if (!alles)
            {
                ChgZeile zw = new ChgZeile(); zw.Art = "mehr"; zw.Lauf = l; zw.Verweis = true; zw.Waehlbar = false; zw.Ebene = 1; zw.Hoehe = UI.S(30);
                zw.Tag = Mehrzahl(l.Massnahmen.Count - zeige, "weitere Maßnahme", "weitere Maßnahmen") + " anzeigen";
                lvChg.Zeilen.Add(zw);
            }
        }
        if (!chgAllePcs && andereLaeufe > 0)
        {
            ChgZeile za = new ChgZeile(); za.Art = "andere"; za.Verweis = true; za.Waehlbar = false; za.Hoehe = UI.S(38);
            za.Tag = Mehrzahl(anderePcs.Count, "Gerät", "Geräte") + ", " + Mehrzahl(andereLaeufe, "Lauf", "Läufe") + ", " + Mehrzahl(andereEintraege, "Eintrag", "Einträge") + " · nur zur Ansicht, Rückgängig geht nur auf dem jeweiligen PC";
            za.Hinweis = "Zeigt auch die Protokolle anderer PCs aus dem Datenordner.";
            lvChg.Zeilen.Add(za);
        }
        lvChg.Leer = changes.Count == 0 ? "Das Änderungsprotokoll ist leer." : "Auf diesem PC hat Leos Minibench noch nichts verändert.";
        lvChg.Aktualisiere();
        if (fokus.Length > 0) foreach (ListenZeile z in lvChg.Zeilen) if (((ChgZeile)z).Schluessel == fokus) { lvChg.Markiere(z, false); break; }

        chgUmfang.SetUnterzeile(0, Mehrzahl(hier, "Eintrag", "Einträge"));
        chgUmfang.SetUnterzeile(1, Mehrzahl(changes.Count, "Eintrag", "Einträge"));
        SetText(chgSeite.Anzahl, open == 0 ? "Nichts rückgängig zu machen" : open == 1 ? "1 Änderung auf diesem PC rückgängig machbar" : open + " Änderungen auf diesem PC rückgängig machbar");
        SetText(chgSeite.Unter, Mehrzahl(changes.Count, "Eintrag", "Einträge") + " im Protokoll");
        if (navChg != null) { navChg.Desc = open == 1 ? "1 Änderung rückgängig machbar" : open > 1 ? open + " Änderungen rückgängig machbar" : changes.Count + (changes.Count == 1 ? " Eintrag" : " Einträge") + " im Protokoll"; navChg.Invalidate(); }
        UpdateChangeButtons();
        chgAufbauMs = sw.ElapsedMilliseconds;
    }

    static string ChgEintragHinweis(ChangeEntry c)
    {
        return (c.Vorher.Length > 0 || c.Nachher.Length > 0 ? "\r\nVorher: " + c.Vorher + "\r\nNachher: " + c.Nachher : "") + (c.Gegenbefehl.Length > 0 ? "\r\nZurück: " + c.Gegenbefehl : "") + (c.Ergebnis.Length > 0 ? "\r\n" + c.Ergebnis : "");
    }

    internal void ChgHakenUm(ChgZeile z)
    {
        List<ChangeEntry> l = z.Art == "lauf" ? z.Lauf.Eintraege : z.Art == "massnahme" ? z.Mass.Eintraege : new List<ChangeEntry>(new ChangeEntry[] { z.Eintrag });
        bool gesperrt; bool an = ChgHakenStand(l, out gesperrt) != 1;
        if (gesperrt) return;
        foreach (ChangeEntry c in l) if (c.Undoable) { if (an) chgHaken.Add(c.Schluessel); else chgHaken.Remove(c.Schluessel); }
        FillChangeList();
    }

    internal void ChgKlappe(ChgZeile z)
    {
        string k = z.Art == "lauf" ? z.Lauf.FilePath : z.Art == "massnahme" ? z.Lauf.FilePath + "|" + z.Mass.Key : "";
        if (k.Length == 0) return;
        if (!chgOffen.Remove(k)) chgOffen.Add(k);
        FillChangeList();
    }

    internal void ChgVerweis(ChgZeile z)
    {
        if (z.Art == "mehr") { chgAlles.Add(z.Lauf.FilePath); FillChangeList(); }
        else if (z.Art == "andere") chgUmfang.Selected = 1;
    }

    void ZeichneChgZeile(Graphics g, ListenZeile zeile, Rectangle r, bool markiert)
    {
        ChgZeile z = (ChgZeile)zeile;
        int rechts = r.Right - UI.S(12);
        if (z.Art == "lauf")
        {
            ChangeLauf l = z.Lauf;
            string st = l.StatusText; int sw = ZeilenListe.Breite(st, ZeilenListe.FKlein);
            ZeilenListe.Schreibe(g, st, ZeilenListe.FKlein, new Rectangle(rechts - sw, r.Y, sw, r.Height), l.Aktiv > 0 ? UI.Text : UI.Muted, true);
            int x = r.X, grenze = rechts - sw - UI.S(12);
            string d = FormatRunDate(l.Zeit); int dw = ZeilenListe.Breite(d, ZeilenListe.FFett);
            ZeilenListe.Schreibe(g, d, ZeilenListe.FFett, new Rectangle(x, r.Y, Math.Min(dw, grenze - x), r.Height), UI.Text, false); x += dw + UI.S(8);
            if (x < grenze) x += ZeilenListe.Etikett(g, l.Module, "akzent", x, r, 0) + UI.S(8);
            if (chgAllePcs && x < grenze) x += ZeilenListe.Etikett(g, l.Computer, l.DieserPc ? "info" : "skip", x, r, 0) + UI.S(8);
            ZeilenListe.Schreibe(g, Mehrzahl(l.Massnahmen.Count, "Maßnahme", "Maßnahmen") + ", " + Mehrzahl(l.Eintraege.Count, "Eintrag", "Einträge"), ZeilenListe.FKlein, new Rectangle(x, r.Y, grenze - x, r.Height), UI.Muted, false);
        }
        else if (z.Art == "massnahme")
        {
            ChangeMassnahme m = z.Mass;
            int bw = UI.S(112); string st = m.StatusText;
            ZeilenListe.Etikett(g, st, ChangeMassnahme.StatusStufe(st), rechts - bw, r, bw);
            string risiko = ContractStep.RiskLabel(m.Risiko); int rw = ZeilenListe.EtikettBreite(risiko);
            ZeilenListe.Etikett(g, risiko, UI.RiskLevel(m.Risiko), rechts - bw - UI.S(8) - rw, r, 0);
            int grenze = rechts - bw - UI.S(8) - rw - UI.S(12);
            int tw = Math.Min(ZeilenListe.Breite(m.Titel, ZeilenListe.FFett), grenze - r.X);
            ZeilenListe.Schreibe(g, m.Titel, ZeilenListe.FFett, new Rectangle(r.X, r.Y, tw, r.Height), z.Gesperrt ? UI.Muted : UI.Text, false);
            ZeilenListe.Schreibe(g, "·  " + m.Umfang, ZeilenListe.FKlein, new Rectangle(r.X + tw + UI.S(4), r.Y, grenze - r.X - tw - UI.S(4), r.Height), UI.Muted, false);
        }
        else if (z.Art == "eintrag")
        {
            ChangeEntry c = z.Eintrag;
            int nw = UI.S(150), vw = UI.S(170), pw = UI.S(22);
            int x = rechts;
            if (z.Mass.Gemischt) { int sw = UI.S(112); ZeilenListe.Schreibe(g, c.Status, ZeilenListe.FKlein, new Rectangle(x - sw, r.Y, sw, r.Height), UI.Muted, true); x -= sw + UI.S(8); }
            ZeilenListe.Schreibe(g, c.Nachher, ZeilenListe.FKopf, new Rectangle(x - nw, r.Y, nw, r.Height), z.Blass ? UI.Muted : UI.Text, false); x -= nw;
            if (c.Vorher.Length > 0 || c.Nachher.Length > 0) ZeilenListe.Schreibe(g, "→", ZeilenListe.FKlein, new Rectangle(x - pw, r.Y, pw, r.Height), UI.Muted, false);
            x -= pw;
            ZeilenListe.Schreibe(g, c.Vorher, ZeilenListe.FKlein, new Rectangle(x - vw, r.Y, vw - UI.S(6), r.Height), UI.Muted, true); x -= vw;
            ZeilenListe.Schreibe(g, c.Ziel, ZeilenListe.FFest, new Rectangle(r.X, r.Y, x - r.X - UI.S(8), r.Height), z.Blass ? UI.Muted : UI.Text, false);
        }
        else if (z.Art == "mehr")
        {
            TextRenderer.DrawText(g, Convert.ToString(z.Tag), ZeilenListe.FKlein, r, UI.Accent, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
        else if (z.Art == "andere")
        {
            string a = "Anzeigen"; int aw = ZeilenListe.Breite(a, ZeilenListe.FKlein);
            ZeilenListe.Schreibe(g, a, ZeilenListe.FKlein, new Rectangle(rechts - aw, r.Y, aw, r.Height), UI.Accent, true);
            int tw = ZeilenListe.Breite("Andere PCs", ZeilenListe.FFett);
            ZeilenListe.Schreibe(g, "Andere PCs", ZeilenListe.FFett, new Rectangle(r.X, r.Y, tw, r.Height), UI.Text, false);
            ZeilenListe.Schreibe(g, Convert.ToString(z.Tag), ZeilenListe.FKlein, new Rectangle(r.X + tw + UI.S(8), r.Y, rechts - aw - UI.S(12) - r.X - tw - UI.S(8), r.Height), UI.Muted, false);
        }
    }

    List<ChangeEntry> CheckedChanges()
    {
        List<ChangeEntry> l = new List<ChangeEntry>();
        foreach (ChangeEntry c in changes) if (c.Undoable && chgHaken.Contains(c.Schluessel)) l.Add(c);
        return l;
    }

    void UpdateChangeButtons()
    {
        if (btnUndo == null || chgSeite == null) return;
        List<ChangeEntry> sel = CheckedChanges(); int n = sel.Count;
        HashSet<string> mass = new HashSet<string>(); foreach (ChangeEntry c in sel) mass.Add(c.FilePath + "|" + c.Schritt + "\n" + c.Titel);
        btnUndo.Enabled = n > 0;
        btnUndo.Text = n > 1 ? n + " Änderungen rückgängig machen" : "Rückgängig machen";
        SetText(chgSeite.BandText, n == 1 ? "1 Änderung angehakt" : n + " Änderungen angehakt");
        SetText(chgSeite.BandUnter, "aus " + Mehrzahl(mass.Count, "Maßnahme", "Maßnahmen"));
        chgSeite.BandSichtbar = n > 0;
    }

    void UndoSelected()
    {
        List<ChangeEntry> sel = CheckedChanges();
        if (sel.Count == 0) return;
        // Rückfrage nach Maßnahmen gebündelt: Ein Lauf der Optimierung hat über hundert Einträge
        StringBuilder sb = new StringBuilder(); List<string> spec = new List<string>(); List<string> k = new List<string>(); Dictionary<string, int> anz = new Dictionary<string, int>();
        foreach (ChangeEntry c in sel)
        {
            spec.Add(c.FilePath + "*" + c.Id);
            string t = c.Titel; if (!anz.ContainsKey(t)) { anz[t] = 0; k.Add(t); } anz[t]++;
        }
        if (sel.Count <= 12) foreach (ChangeEntry c in sel) sb.AppendLine("·  " + c.Titel + ": " + c.Ziel + " wieder " + c.Vorher);
        else
        {
            for (int i = 0; i < k.Count && i < 20; i++) sb.AppendLine("·  " + k[i] + (anz[k[i]] > 1 ? " (" + anz[k[i]] + " Einträge)" : ""));
            if (k.Count > 20) sb.AppendLine("·  und " + (k.Count - 20) + " weitere Maßnahmen");
        }
        if (MessageBox.Show(this, (sel.Count == 1 ? "Diese Änderung zurücknehmen?" : "Diese " + sel.Count + " Änderungen zurücknehmen?") + "\r\n\r\n" + sb.ToString(), "Rückgängig machen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        // ab 3.53 ohne Einfrieren der Oberfläche: Softwarepakete brauchen für winget uninstall bis zu 20 Minuten je Paket
        int pakete = 0; foreach (ChangeEntry c in sel) if (c.Art == "Softwarepaket") pakete++;
        List<string> lines = new List<string>();
        try { RunHelperPumped("-Rueckgaengig \"" + String.Join(";", spec.ToArray()) + "\"", out lines, 300 + pakete * WingetFristPaketSek); }
        catch (Exception ex) { lines.Add("Rückgängig fehlgeschlagen: " + ex.Message); }
        ReloadDb();
        // ab v4.10: Zustand der Optimierung neu lesen, damit zurückgenommene Einstellungen sofort wieder wählbar sind
        // (bis v4.09 blieb ihr Schalter bis zum nächsten Start als "bereits aktiv" gesperrt)
        StartOptStateCheck();
        MessageBox.Show(this, lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Keine Rückmeldung vom Arbeitsprozess.", "Rückgängig", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ------------------------------------------------------------ Aufräumen
    static readonly int[] chgAlterTage = new int[] { 30, 90, 180, 0 };

    // Argument für den Arbeitsprozess, etwa "Erledigt:30;Andere"
    public static string ChgAufraeumArg(bool erledigt, int tage, bool andere)
    {
        List<string> t = new List<string>();
        if (erledigt) t.Add("Erledigt:" + tage);
        if (andere) t.Add("Andere");
        return String.Join(";", t.ToArray());
    }

    void ChangeAufraeumen()
    {
        if (dataDir.Length == 0) return;
        if (running || liveRunning) { UI.Hinweis("Aufräumen ist erst möglich, wenn der Lauf oder die Live-Ansicht beendet ist: Beide schreiben in das Änderungsprotokoll.", "Protokoll aufräumen", this); return; }
        Form dlg = new Form();
        dlg.Text = "Protokoll aufräumen"; dlg.FormBorderStyle = FormBorderStyle.FixedDialog; dlg.MaximizeBox = false; dlg.MinimizeBox = false;
        dlg.StartPosition = FormStartPosition.CenterParent; dlg.ClientSize = new Size(UI.S(560), UI.S(372)); dlg.BackColor = UI.Bg; dlg.Font = new Font("Segoe UI", 9.5f);
        int x = UI.S(18), w = UI.S(524), y = UI.S(14);
        Label kopf = new Label(); kopf.Text = "Verschiebt Protokolldateien nach Minibench-Daten\\Archiv\\Änderungen. Gelöscht wird nichts; zurücklegen lässt sich eine Datei von Hand in den Ordner Änderungen."; kopf.SetBounds(x, y, w, UI.S(40)); y += UI.S(48);
        CheckBox cbErl = new CheckBox(); cbErl.Text = "Erledigte Läufe aller PCs, älter als"; cbErl.Checked = true; cbErl.AutoSize = true; cbErl.Location = new Point(x, y + UI.S(2));
        ComboBox cmbAlter = Combo(120, new string[] { "30 Tage", "90 Tage", "180 Tage", "jedes Alter" }, 0);
        Label lErl = new Label(); lErl.SetBounds(x + UI.S(20), y + UI.S(30), w - UI.S(20), UI.S(38)); lErl.ForeColor = UI.Muted;
        y += UI.S(76);
        CheckBox cbAnd = new CheckBox(); cbAnd.Text = "Alle Protokolle anderer PCs, auch mit aktiven Änderungen"; cbAnd.Checked = false; cbAnd.AutoSize = true; cbAnd.Location = new Point(x, y);
        Label lAnd = new Label(); lAnd.SetBounds(x + UI.S(20), y + UI.S(26), w - UI.S(20), UI.S(56)); lAnd.ForeColor = UI.Muted;
        y += UI.S(90);
        Label fest = new Label(); fest.Text = "Aktive Änderungen dieses PCs bleiben immer erhalten. Ist ein Netzlaufwerk eingerichtet, verschiebt der nächste Abgleich die Dateien dort ebenfalls ins Archiv, bei vielen Dateien nach Rückfrage."; fest.SetBounds(x, y, w, UI.S(56)); fest.ForeColor = UI.Muted;
        y += UI.S(64);
        Label summe = new Label(); summe.SetBounds(x, y + UI.S(8), UI.S(280), UI.S(22));
        Button ok = UI.Primary("Archivieren"); ok.Font = new Font("Segoe UI Semibold", 9.75f); ok.Padding = new Padding(UI.S(14), UI.S(3), UI.S(14), UI.S(3)); ok.AutoSize = false; ok.SetBounds(UI.S(316), y, UI.S(116), UI.S(34)); ok.DialogResult = DialogResult.OK;
        Button ab = UI.Secondary("Abbrechen"); ab.AutoSize = false; ab.Margin = new Padding(0); ab.SetBounds(UI.S(440), y, UI.S(102), UI.S(34)); ab.DialogResult = DialogResult.Cancel;
        dlg.Controls.Add(kopf); dlg.Controls.Add(cbErl); dlg.Controls.Add(cmbAlter); dlg.Controls.Add(lErl); dlg.Controls.Add(cbAnd); dlg.Controls.Add(lAnd); dlg.Controls.Add(fest); dlg.Controls.Add(summe); dlg.Controls.Add(ok); dlg.Controls.Add(ab);
        cmbAlter.Margin = new Padding(0);
        cmbAlter.Location = new Point(x + cbErl.PreferredSize.Width + UI.S(8), cbErl.Top - UI.S(3));
        dlg.AcceptButton = ok; dlg.CancelButton = ab;
        EventHandler rechne = delegate
        {
            int tage = chgAlterTage[Math.Max(0, Math.Min(chgAlterTage.Length - 1, cmbAlter.SelectedIndex))];
            List<ChangeLauf> erl = ChangeLauf.Aufzuraeumen(changeLaeufe, true, tage, false, DateTime.Now), and = ChangeLauf.Aufzuraeumen(changeLaeufe, false, 0, true, DateTime.Now);
            List<ChangeLauf> alle = ChangeLauf.Aufzuraeumen(changeLaeufe, cbErl.Checked, tage, cbAnd.Checked, DateTime.Now);
            int ee = 0, ae = 0, aa = 0, ge = 0; foreach (ChangeLauf l in erl) ee += l.Eintraege.Count; foreach (ChangeLauf l in and) { ae += l.Eintraege.Count; aa += l.Aktiv; } foreach (ChangeLauf l in alle) ge += l.Eintraege.Count;
            lErl.Text = "Läufe ohne aktive Einträge (nur Hinweise, Zurückgenommenes oder leer): " + Mehrzahl(erl.Count, "Datei", "Dateien") + ", " + Mehrzahl(ee, "Eintrag", "Einträge") + ".";
            lAnd.Text = Mehrzahl(and.Count, "Datei", "Dateien") + ", " + Mehrzahl(ae, "Eintrag", "Einträge") + ", davon " + aa + " aktiv. Rückgängig ist auf jenem PC danach erst wieder möglich, wenn die Datei aus dem Archiv zurückgelegt wird.";
            summe.Text = alle.Count == 0 ? "Nichts zu verschieben." : Mehrzahl(alle.Count, "Datei", "Dateien") + " mit " + Mehrzahl(ge, "Eintrag", "Einträgen") + " werden verschoben.";
            ok.Enabled = alle.Count > 0; cmbAlter.Enabled = cbErl.Checked;
        };
        cbErl.CheckedChanged += rechne; cbAnd.CheckedChanged += rechne; cmbAlter.SelectedIndexChanged += rechne;
        Tip(cbErl, "Läufe, in denen nichts mehr zurückzunehmen ist. Sie dienen nur noch dem Nachlesen.");
        Tip(cmbAlter, "Mindestalter eines erledigten Laufs.");
        Tip(cbAnd, "Protokolle, die auf anderen PCs entstanden sind. Auf diesem PC lassen sie sich nicht zurücknehmen, auf dem jeweiligen PC aber schon, solange die Datei im Ordner Änderungen liegt.");
        UI.ThemeDialog(dlg);
        lErl.ForeColor = UI.Muted; lAnd.ForeColor = UI.Muted; fest.ForeColor = UI.Muted;
        rechne(null, EventArgs.Empty);
        if (dlg.ShowDialog(this) != DialogResult.OK) { dlg.Dispose(); return; }
        string arg = ChgAufraeumArg(cbErl.Checked, chgAlterTage[Math.Max(0, Math.Min(chgAlterTage.Length - 1, cmbAlter.SelectedIndex))], cbAnd.Checked);
        dlg.Dispose();
        if (arg.Length == 0) return;
        List<string> lines = new List<string>(); string res = "";
        try { res = RunHelperPumped("-AenderungenAufraeumen " + Q(arg), out lines, 300); }
        catch (Exception ex) { lines.Add("Aufräumen fehlgeschlagen: " + ex.Message); }
        ReloadDb();
        UI.Hinweis(lines.Count > 0 ? String.Join("\r\n", lines.ToArray()) : "Keine Rückmeldung vom Arbeitsprozess.", "Protokoll aufräumen", this);
    }
}
