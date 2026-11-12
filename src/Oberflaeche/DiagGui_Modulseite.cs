// Gemeinsamer Rahmen der Modulseiten Benchmark und Diagnose (ab v4.08, Teilklasse DiagGui)
public partial class DiagGui
{
    // Karte mit Titel, optionalen Verweisen rechts im Kopf und Inhalt untereinander. Zeilen, Unterbereiche und Auswahllisten
    // bekommen die Innenbreite der Karte, Texte brechen an ihr um; die Höhe folgt dem Inhalt.
    public class ZeilenKarte
    {
        public readonly CardPanel Karte = new CardPanel();
        public readonly FlowLayoutPanel Inhalt = new FlowLayoutPanel();
        public readonly Panel Kopf = new Panel();
        public readonly Label Titel;
        public readonly FlowLayoutPanel KopfRechts = new FlowLayoutPanel();
        List<Control> breit = new List<Control>();

        public ZeilenKarte(string titel)
        {
            Karte.AutoSize = false; Karte.Padding = new Padding(UI.S(16), UI.S(12), UI.S(16), UI.S(10)); Karte.Margin = new Padding(0, 0, 0, UI.S(UI.Abstand3));
            // GrowAndShrink: in einem Panel ohne eigenes Layout schrumpft ein Element mit AutoSize sonst nie unter seine Startgröße
            Inhalt.FlowDirection = FlowDirection.TopDown; Inhalt.WrapContents = false; Inhalt.AutoSize = true; Inhalt.AutoSizeMode = AutoSizeMode.GrowAndShrink; Inhalt.BackColor = UI.Panel; Inhalt.Margin = new Padding(0);
            Inhalt.Location = new Point(Karte.Padding.Left, Karte.Padding.Top);
            Kopf.BackColor = UI.Panel; Kopf.Height = UI.S(26); Kopf.Margin = new Padding(0, 0, 0, UI.S(6));
            Titel = Lbl(titel, UI.SchriftGruppe + 0.5f, true, UI.Text); Titel.Location = new Point(0, UI.S(1)); Titel.Margin = new Padding(0);
            KopfRechts.AutoSize = true; KopfRechts.AutoSizeMode = AutoSizeMode.GrowAndShrink; KopfRechts.WrapContents = false; KopfRechts.BackColor = UI.Panel; KopfRechts.Margin = new Padding(0);
            Kopf.Controls.Add(Titel); Kopf.Controls.Add(KopfRechts);
            EventHandler lage = delegate { KopfRechts.Location = new Point(Math.Max(Titel.Right + UI.S(8), Kopf.Width - KopfRechts.Width), UI.S(3)); };
            Kopf.Resize += lage; KopfRechts.SizeChanged += lage;
            Inhalt.Controls.Add(Kopf); breit.Add(Kopf);
            Inhalt.SizeChanged += delegate { Passe(); };
            Karte.Controls.Add(Inhalt);
        }

        // Element über die ganze Innenbreite (Zeile, Unterbereich, Auswahlliste)
        public T Zeile<T>(T c) where T : Control { Inhalt.Controls.Add(c); breit.Add(c); return c; }
        // Element in seiner eigenen Breite (Knopf, Verweis); Texte brechen an der Innenbreite um
        public T Frei<T>(T c) where T : Control { Inhalt.Controls.Add(c); return c; }

        // Inhalt bis auf den Kopf entfernen (Ergebnisseite baut ihre Karten je Ergebnis neu); behalten nennt Elemente,
        // die wiederverwendet und deshalb nicht freigegeben werden
        public void Leere(params Control[] behalten)
        {
            Inhalt.SuspendLayout();
            while (Inhalt.Controls.Count > 1)
            {
                Control c = Inhalt.Controls[1]; Inhalt.Controls.RemoveAt(1); breit.Remove(c);
                if (behalten == null || Array.IndexOf(behalten, c) < 0) c.Dispose();
            }
            Inhalt.ResumeLayout(true);
        }

        public int Innenbreite { get { return Math.Max(UI.S(160), Karte.Width - Karte.Padding.Horizontal); } }

        public void Breite(int w)
        {
            if (Karte.Width != w) Karte.Width = w;
            int iw = Innenbreite;
            foreach (Control c in Inhalt.Controls)
            {
                int cw = iw - c.Margin.Horizontal;
                if (breit.Contains(c)) { if (c.Width != cw) c.Width = cw; }
                else if (c is Label) c.MaximumSize = new Size(cw, 0);
            }
            Passe();
        }

        // Mindesthöhe, damit zwei Karten nebeneinander gleich hoch stehen (Ergebnisseite)
        int minHoehe;
        public int MinHoehe { get { return minHoehe; } set { minHoehe = value; Passe(); } }
        public int InhaltHoehe { get { return Inhalt.Height + Karte.Padding.Vertical; } }
        void Passe() { int h = Math.Max(minHoehe, Inhalt.Height + Karte.Padding.Vertical); if (Karte.Height != h) Karte.Height = h; }
    }

    // Seite mit Kopf (Titel und Einleitung links, rechts Anzahl und Dauer) und zwei Spalten mit Karten: links rund 60 %,
    // rechts rund 40 %, unter 900 px Seitenbreite untereinander (wie ToolSpalten auf der Seite Tools).
    public class ModulSeite
    {
        public readonly Panel Kopf = new Panel();
        public readonly Label Anzahl, Dauer;
        public readonly Panel Spalten = new Panel();
        public readonly FlowLayoutPanel Links = new FlowLayoutPanel(), Rechts = new FlowLayoutPanel();
        public readonly List<ZeilenKarte> KartenLinks = new List<ZeilenKarte>(), KartenRechts = new List<ZeilenKarte>();
        // Elemente über die ganze Seitenbreite (Bänder, Auswahl des Umfangs)
        public readonly List<Control> Voll = new List<Control>();
        Label titel, einleitung;
        FlowLayoutPanel rechtsKopf = new FlowLayoutPanel();
        public int Breite { get; private set; }
        public bool Zweispaltig { get; private set; }

        // f ist die Seite aus Page(): Titel und Einleitung wandern in den Kopf, das Band "nicht im Ablauf" bleibt darunter
        public ModulSeite(FlowLayoutPanel f)
        {
            titel = (Label)f.Controls[0]; einleitung = (Label)f.Controls[1];
            f.Controls.Remove(titel); f.Controls.Remove(einleitung);
            Kopf.BackColor = UI.Panel; Kopf.Margin = new Padding(0, 0, 0, UI.S(UI.Abstand2));
            titel.Location = new Point(0, 0); einleitung.Margin = new Padding(0);
            // feste Größe statt AutoSize: Höhe und Umbruch der Einleitung setzt OrdneKopf aus dem gemessenen Text
            einleitung.MaximumSize = Size.Empty; einleitung.AutoSize = false;
            Anzahl = Lbl("", 10.5f, true, UI.Text); Anzahl.Margin = new Padding(0); Anzahl.Anchor = AnchorStyles.Right;
            Dauer = Lbl("", UI.SchriftText, false, UI.Muted); Dauer.Margin = new Padding(0, UI.S(1), 0, 0); Dauer.Anchor = AnchorStyles.Right;
            rechtsKopf.FlowDirection = FlowDirection.TopDown; rechtsKopf.WrapContents = false; rechtsKopf.AutoSize = true; rechtsKopf.AutoSizeMode = AutoSizeMode.GrowAndShrink; rechtsKopf.BackColor = UI.Panel; rechtsKopf.Margin = new Padding(0);
            rechtsKopf.Controls.Add(Anzahl); rechtsKopf.Controls.Add(Dauer);
            Kopf.Controls.Add(titel); Kopf.Controls.Add(einleitung); Kopf.Controls.Add(rechtsKopf);
            rechtsKopf.SizeChanged += delegate { OrdneKopf(); };
            f.Controls.Add(Kopf); f.Controls.SetChildIndex(Kopf, 0);

            Spalten.BackColor = UI.Panel; Spalten.Margin = new Padding(0);
            foreach (FlowLayoutPanel c in new FlowLayoutPanel[] { Links, Rechts })
            {
                c.FlowDirection = FlowDirection.TopDown; c.WrapContents = false; c.AutoSize = true; c.AutoSizeMode = AutoSizeMode.GrowAndShrink; c.BackColor = UI.Panel; c.Margin = new Padding(0);
                c.SizeChanged += delegate { OrdneSpalten(); };
                Spalten.Controls.Add(c);
            }
        }

        public ZeilenKarte Karte(string titelText, bool links)
        {
            ZeilenKarte k = new ZeilenKarte(titelText);
            (links ? KartenLinks : KartenRechts).Add(k);
            (links ? Links : Rechts).Controls.Add(k.Karte);
            return k;
        }

        // Spaltenbreiten bei einer Seitenbreite: links rund 60 %, ab 900 px zweispaltig (sonst beide in voller Breite)
        public static int[] SpaltenBreiten(int w, int abstand)
        {
            if (ToolSpalten(w) < 2) return new int[] { w, w };
            int l = (w - abstand) * 60 / 100;
            return new int[] { l, w - abstand - l };
        }

        // Lage und Höhe des Kopfs aus gemessenen Texten: Die Größe der Beschriftungen stellt sich erst beim nächsten
        // Layout ein, die Einleitung bricht aber sofort an der neuen Breite um.
        void OrdneKopf()
        {
            if (Breite <= 0) return;
            Size ra = rechtsKopf.PreferredSize;
            rechtsKopf.Location = new Point(Math.Max(0, Breite - ra.Width), UI.S(4));
            int ew = Math.Max(UI.S(240), Breite - ra.Width - UI.S(UI.Abstand5));
            int ty = titel.PreferredSize.Height + UI.S(UI.Abstand1);
            int eh = TextRenderer.MeasureText(einleitung.Text, einleitung.Font, new Size(ew, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + UI.S(2);
            einleitung.SetBounds(0, ty, ew, eh);
            int h = Math.Max(ty + eh + UI.S(2), UI.S(4) + ra.Height);
            if (Kopf.Width != Breite || Kopf.Height != h) Kopf.Size = new Size(Breite, h);
        }

        void OrdneSpalten()
        {
            if (Breite <= 0) return;
            int gap = UI.S(UI.Abstand4);
            int[] sb = SpaltenBreiten(Breite, gap);
            Links.Location = new Point(0, 0);
            Rechts.Location = Zweispaltig ? new Point(sb[0] + gap, 0) : new Point(0, Links.Height);
            Size s = new Size(Breite, Zweispaltig ? Math.Max(Links.Height, Rechts.Height) : Links.Height + Rechts.Height);
            if (Spalten.Size != s) Spalten.Size = s;
        }

        public void Ordne(int w)
        {
            Breite = w; Zweispaltig = ToolSpalten(w) >= 2;
            int[] sb = SpaltenBreiten(w, UI.S(UI.Abstand4));
            Spalten.SuspendLayout();
            foreach (ZeilenKarte k in KartenLinks) k.Breite(sb[0]);
            foreach (ZeilenKarte k in KartenRechts) k.Breite(sb[1]);
            Spalten.ResumeLayout(true);
            foreach (Control c in Voll) { if (c is Label) c.MaximumSize = new Size(w - c.Margin.Horizontal, 0); else if (!(c is FeldAuswahl)) c.Width = w - c.Margin.Horizontal; }
            OrdneKopf(); OrdneSpalten();
        }
    }

    // Ordnet die Modulseiten nach der Seitenbreite neu (Größe des Fensters, Wechsel der Seite)
    List<ModulSeite> modulSeiten = new List<ModulSeite>();
    void OrdneModulSeiten() { int w = SeitenBreite(); foreach (ModulSeite m in modulSeiten) m.Ordne(w); }

    ModulSeite NeueModulSeite(FlowLayoutPanel f)
    {
        ModulSeite m = new ModulSeite(f);
        // auch bei geänderter Innenfläche: Erscheint die senkrechte Bildlaufleiste, wird die Seite schmaler, ohne dass
        // sich die Größe des Bereichs ändert
        if (modulSeiten.Count == 0 && content != null) { content.Resize += delegate { OrdneModulSeiten(); }; content.ClientSizeChanged += delegate { if (curPage == 1 || curPage == 2) OrdneModulSeiten(); }; }
        modulSeiten.Add(m);
        return m;
    }

    // Zeile mit Symbol, Titel, Beschreibung und Schalter, verbunden mit einem Kontrollkästchen als Wert (wie auf der Seite
    // Wartung): Vorlagen, Zeitschätzung und Start lesen weiter das Kästchen. nutzer wird nur aufgerufen, wenn der Schalter
    // selbst umgelegt wurde, nicht bei einer Änderung über das Kästchen (Vorlage, Profil).
    SettingRow SchalterZeile(CheckBox wert, string titel, string detail, string symbol, string hinweis, Action nutzer)
    {
        SettingRow row = new SettingRow(titel, detail);
        row.Icon = symbol; row.Checked = wert.Checked;
        row.Toggle.CheckedChanged += delegate
        {
            if (wert.Checked == row.Toggle.Checked) return;
            if (nutzer != null) nutzer();
            wert.Checked = row.Toggle.Checked;
        };
        wert.CheckedChanged += delegate { if (row.Toggle.Checked != wert.Checked) row.Toggle.Checked = wert.Checked; };
        Tip(row, hinweis); Tip(row.Toggle, hinweis);
        return row;
    }

    static Badge DauerEtikett(int minuten) { Badge b = new Badge(MinText(minuten), "skip"); b.Tag = "dauer"; return b; }

    static LinkLabel KartenVerweis(string text)
    {
        LinkLabel l = new LinkLabel(); l.Text = text; l.AutoSize = true; l.Font = new Font("Segoe UI", UI.SchriftHinweis);
        l.LinkColor = UI.Accent; l.ActiveLinkColor = UI.AccentHover; l.LinkBehavior = LinkBehavior.HoverUnderline; l.BackColor = Color.Transparent; l.Margin = new Padding(UI.S(10), 0, 0, 0);
        return l;
    }
}
