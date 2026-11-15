// Selbst gezeichnete Liste und Kennzahlen (ab v4.10) für die Seiten Sensoren live, Datenbank und Änderungen.
// Die Liste zeichnet nur die sichtbaren Zeilen und kennt keine Fensterelemente je Zeile: Ihr Aufbau kostet deshalb bei
// tausend Einträgen so viel wie bei zehn. Bis v4.09 standen dort native Listenfelder (ListView); die Seite Änderungen
// zählte beim ersten Anzeigen für jede der rund tausend Zeilen alle Haken neu.

// Eine Zeile der Liste. Was sie zeigt, bestimmt die Seite über Tag, Art und den Zeichner der Liste.
public class ListenZeile
{
    public object Tag;
    public string Art = "";
    public int Ebene, Hoehe;
    // Haken: -1 ohne Kästchen, 0 aus, 1 an, 2 teilweise. Gesperrt: Kästchen sichtbar, aber nicht änderbar.
    public int Haken = -1;
    public bool Gesperrt, Aufklappbar, Offen, Blass, Verweis;
    public bool Waehlbar = true;
    // Grund: 0 Fläche der Seite, 1 Kopfzeile einer Gruppe, 2 Unterzeile
    public int Grund;
    public string Hinweis = "";
}

public class ListenSpalte
{
    public string Titel = "", Hinweis = "";
    public int Breite;
    public bool Rechts, Sortierbar = true;
    // Schlüssel der Spalte für die Seite (Sortierung)
    public int Nr;
    public ListenSpalte(string titel, int breite, bool rechts, int nr) { Titel = titel; Breite = breite; Rechts = rechts; Nr = nr; }
}

public delegate void ZeilenZeichner(Graphics g, ListenZeile z, Rectangle r, bool markiert);

public class ZeilenListe : Control
{
    public readonly List<ListenZeile> Zeilen = new List<ListenZeile>();
    // Spalte 0 nimmt den Rest der Breite, alle weiteren stehen mit fester Breite rechts davon
    public readonly List<ListenSpalte> Spalten = new List<ListenSpalte>();
    public ZeilenZeichner Zeichner;
    public int ZeilenHoehe = UI.S(30);
    // AutoHoehe: Die Liste ist so hoch wie ihr Inhalt und rollt nicht selbst (Seite mit eigenem Bildlauf)
    public bool AutoHoehe, Mehrfach, ChevronPlatz = true, Rahmen = true;
    public string Leer = "";
    public int SortSpalte = -1;
    public bool SortAuf = true;
    public ToolTip Tipp;
    public event Action<ListenZeile> HakenGeklickt, AufklappGeklickt, Geoeffnet, VerweisGeklickt;
    public event Action<int> SpalteGeklickt;
    public event EventHandler MarkierungGeaendert;

    List<ListenZeile> markiert = new List<ListenZeile>();
    ListenZeile fokus, anker, hover;
    int rollen, hoverSpalte = -1;
    bool zieht; int ziehStartY, ziehStartRollen;

    public static readonly Font FText = new Font("Segoe UI", 9.5f), FFett = new Font("Segoe UI Semibold", 9.5f), FKlein = new Font("Segoe UI", 8.75f),
        FKopf = new Font("Segoe UI Semibold", 8.75f), FFest = new Font("Consolas", 8.75f), FEtikett = new Font("Segoe UI Semibold", 8.25f);

    public ZeilenListe()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        TabStop = true; BackColor = UI.Panel; AccessibleRole = AccessibleRole.List;
        Size = new Size(UI.S(400), UI.S(200));
    }

    // ------------------------------------------------------------ Maße
    public int KopfHoehe { get { return Spalten.Count > 0 ? UI.S(28) : 0; } }
    int Rand { get { return Rahmen ? 1 : 0; } }
    int LeisteBreite { get { return UI.S(10); } }
    int HoeheVon(ListenZeile z) { return z.Hoehe > 0 ? z.Hoehe : ZeilenHoehe; }
    public int InhaltHoehe { get { int h = 0; foreach (ListenZeile z in Zeilen) h += HoeheVon(z); return h; } }
    int Sichtbar { get { return Math.Max(0, Height - KopfHoehe - 2 * Rand); } }
    int RollenMax { get { return AutoHoehe ? 0 : Math.Max(0, InhaltHoehe - Sichtbar); } }
    bool LeisteNoetig { get { return RollenMax > 0; } }
    int ZeilenBreite { get { return Width - 2 * Rand - (LeisteNoetig ? LeisteBreite : 0); } }
    public int Rollen { get { return rollen; } set { int v = Math.Max(0, Math.Min(RollenMax, value)); if (v != rollen) { rollen = v; Invalidate(); } } }

    // Höhe für eine Liste, die nicht selbst rollt
    public int WunschHoehe { get { return KopfHoehe + 2 * Rand + Math.Max(InhaltHoehe, Zeilen.Count == 0 && Leer.Length > 0 ? UI.S(56) : 0); } }

    // Nach jeder Änderung an Zeilen aufrufen: hält Markierung und Bildlauf gültig und zeichnet neu
    public void Aktualisiere()
    {
        for (int i = markiert.Count - 1; i >= 0; i--) if (!Zeilen.Contains(markiert[i])) markiert.RemoveAt(i);
        if (fokus != null && !Zeilen.Contains(fokus)) fokus = null;
        if (anker != null && !Zeilen.Contains(anker)) anker = null;
        if (hover != null && !Zeilen.Contains(hover)) hover = null;
        if (AutoHoehe) { int h = WunschHoehe; if (Height != h) Height = h; }
        if (rollen > RollenMax) rollen = RollenMax;
        Invalidate();
    }

    // Lage einer Spalte in einer Zeile (r ist die ganze Zeile ohne Bildlaufleiste)
    public Rectangle Zelle(int spalte, Rectangle r)
    {
        int rechts = r.Right - UI.S(10);
        for (int i = Spalten.Count - 1; i >= 1; i--)
        {
            int w = Spalten[i].Breite;
            if (i == spalte) return new Rectangle(rechts - w, r.Y, w, r.Height);
            rechts -= w;
        }
        return new Rectangle(r.X, r.Y, Math.Max(0, rechts - r.X), r.Height);
    }

    // Beginn des Inhalts einer Zeile hinter Einrückung, Kästchen und Pfeil
    public int InhaltLinks(ListenZeile z)
    {
        int x = UI.S(10) + z.Ebene * UI.S(22);
        if (z.Haken >= 0) x += UI.S(24);
        if (ChevronPlatz || z.Aufklappbar) x += UI.S(18);
        return x;
    }
    Rectangle HakenFeld(ListenZeile z, Rectangle r) { int x = r.X + UI.S(10) + z.Ebene * UI.S(22); return new Rectangle(x, r.Y + (r.Height - UI.S(16)) / 2, UI.S(16), UI.S(16)); }
    Rectangle PfeilFeld(ListenZeile z, Rectangle r) { int x = r.X + UI.S(10) + z.Ebene * UI.S(22) + (z.Haken >= 0 ? UI.S(24) : 0); return new Rectangle(x, r.Y, UI.S(16), r.Height); }

    Rectangle ZeileRechteck(ListenZeile ziel)
    {
        int y = Rand + KopfHoehe - rollen;
        foreach (ListenZeile z in Zeilen) { int h = HoeheVon(z); if (z == ziel) return new Rectangle(Rand, y, ZeilenBreite, h); y += h; }
        return Rectangle.Empty;
    }
    public ListenZeile ZeileBei(int py)
    {
        if (py < Rand + KopfHoehe) return null;
        int y = Rand + KopfHoehe - rollen;
        foreach (ListenZeile z in Zeilen) { int h = HoeheVon(z); if (py >= y && py < y + h) return z; y += h; }
        return null;
    }

    // ------------------------------------------------------------ Markierung
    public List<ListenZeile> Markiert { get { return new List<ListenZeile>(markiert); } }
    public ListenZeile Fokus { get { return fokus; } }
    public void Markiere(ListenZeile z, bool melden)
    {
        markiert.Clear(); if (z != null && z.Waehlbar) { markiert.Add(z); fokus = z; anker = z; }
        Invalidate();
        if (melden && MarkierungGeaendert != null) MarkierungGeaendert(this, EventArgs.Empty);
    }
    public void MarkiereMehrere(List<ListenZeile> l)
    {
        markiert.Clear(); foreach (ListenZeile z in l) if (z.Waehlbar && Zeilen.Contains(z)) markiert.Add(z);
        fokus = markiert.Count > 0 ? markiert[0] : null; anker = fokus; Invalidate();
    }
    public void Zeige(ListenZeile z)
    {
        if (z == null || AutoHoehe) return;
        Rectangle r = ZeileRechteck(z); if (r.IsEmpty) return;
        int oben = Rand + KopfHoehe;
        if (r.Y < oben) Rollen = rollen - (oben - r.Y);
        else if (r.Bottom > Height - Rand) Rollen = rollen + (r.Bottom - (Height - Rand));
    }

    // ------------------------------------------------------------ Ereignisse von außen (auch für den Selbsttest)
    public void LoeseHaken(ListenZeile z) { if (z != null && z.Haken >= 0 && !z.Gesperrt && HakenGeklickt != null) HakenGeklickt(z); }
    public void LoeseAufklappen(ListenZeile z) { if (z != null && z.Aufklappbar && AufklappGeklickt != null) AufklappGeklickt(z); }
    public void LoeseOeffnen(ListenZeile z) { if (z != null && Geoeffnet != null) Geoeffnet(z); }
    public void LoeseVerweis(ListenZeile z) { if (z != null && z.Verweis && VerweisGeklickt != null) VerweisGeklickt(z); }

    // ------------------------------------------------------------ Maus
    int SpalteBei(int px)
    {
        Rectangle r = new Rectangle(Rand, Rand, ZeilenBreite, KopfHoehe);
        for (int i = 0; i < Spalten.Count; i++) { Rectangle c = Zelle(i, r); if (px >= c.X && px < c.Right) return i; }
        return -1;
    }
    Rectangle Schieber()
    {
        if (!LeisteNoetig) return Rectangle.Empty;
        int bahn = Sichtbar, gesamt = Math.Max(1, InhaltHoehe);
        int h = Math.Max(UI.S(24), bahn * bahn / gesamt);
        int y = Rand + KopfHoehe + (RollenMax > 0 ? (int)((long)(bahn - h) * rollen / RollenMax) : 0);
        return new Rectangle(Width - Rand - LeisteBreite + UI.S(2), y, LeisteBreite - UI.S(4), h);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (CanFocus) Focus();
        if (LeisteNoetig && e.X >= Width - Rand - LeisteBreite)
        {
            Rectangle s = Schieber();
            if (e.Y >= s.Y && e.Y < s.Bottom) { zieht = true; ziehStartY = e.Y; ziehStartRollen = rollen; }
            else Rollen = rollen + (e.Y < s.Y ? -Sichtbar : Sichtbar);
            return;
        }
        if (e.Y < Rand + KopfHoehe)
        {
            int sp = SpalteBei(e.X);
            if (e.Button == MouseButtons.Left && sp >= 0 && Spalten[sp].Sortierbar && SpalteGeklickt != null) SpalteGeklickt(Spalten[sp].Nr);
            return;
        }
        ListenZeile z = ZeileBei(e.Y); if (z == null) return;
        Rectangle r = ZeileRechteck(z);
        if (e.Button == MouseButtons.Left)
        {
            if (z.Verweis) { LoeseVerweis(z); return; }
            Rectangle hf = HakenFeld(z, r); hf.Inflate(UI.S(4), UI.S(6));
            if (z.Haken >= 0 && hf.Contains(e.Location)) { if (!z.Gesperrt) LoeseHaken(z); return; }
            if (z.Aufklappbar && PfeilFeld(z, r).Contains(e.Location)) { LoeseAufklappen(z); return; }
            // Kopfzeilen ohne eigene Auswahl klappen bei jedem Klick auf oder zu
            if (z.Aufklappbar && !z.Waehlbar) { LoeseAufklappen(z); return; }
        }
        if (!z.Waehlbar) return;
        bool strg = (ModifierKeys & Keys.Control) != 0, um = (ModifierKeys & Keys.Shift) != 0;
        if (e.Button == MouseButtons.Right) { if (!markiert.Contains(z)) Markiere(z, true); return; }
        if (Mehrfach && strg) { if (markiert.Contains(z)) markiert.Remove(z); else markiert.Add(z); fokus = z; anker = z; }
        else if (Mehrfach && um && anker != null && Zeilen.Contains(anker))
        {
            int a = Zeilen.IndexOf(anker), b = Zeilen.IndexOf(z); if (a > b) { int t = a; a = b; b = t; }
            markiert.Clear(); for (int i = a; i <= b; i++) if (Zeilen[i].Waehlbar) markiert.Add(Zeilen[i]);
            fokus = z;
        }
        else { markiert.Clear(); markiert.Add(z); fokus = z; anker = z; }
        Invalidate();
        if (MarkierungGeaendert != null) MarkierungGeaendert(this, EventArgs.Empty);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (zieht)
        {
            int bahn = Sichtbar - Schieber().Height;
            if (bahn > 0) Rollen = ziehStartRollen + (int)((long)(e.Y - ziehStartY) * RollenMax / bahn);
            return;
        }
        ListenZeile z = e.X < Rand + ZeilenBreite ? ZeileBei(e.Y) : null;
        int sp = e.Y < Rand + KopfHoehe ? SpalteBei(e.X) : -1;
        if (z != hover || sp != hoverSpalte)
        {
            hover = z; hoverSpalte = sp; Invalidate();
            Cursor = (z != null && (z.Verweis || (z.Aufklappbar && !z.Waehlbar))) || (sp >= 0 && Spalten[sp].Sortierbar) ? Cursors.Hand : Cursors.Default;
            if (Tipp != null) Tipp.SetToolTip(this, z != null ? z.Hinweis : sp >= 0 ? Spalten[sp].Hinweis : "");
        }
    }
    protected override void OnMouseUp(MouseEventArgs e) { zieht = false; base.OnMouseUp(e); }
    protected override void OnMouseLeave(EventArgs e) { if (hover != null || hoverSpalte >= 0) { hover = null; hoverSpalte = -1; Invalidate(); } base.OnMouseLeave(e); }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);
        if (e.Button != MouseButtons.Left) return;
        ListenZeile z = ZeileBei(e.Y); if (z == null || z.Verweis) return;
        Rectangle r = ZeileRechteck(z);
        Rectangle hf = HakenFeld(z, r); hf.Inflate(UI.S(4), UI.S(6));
        if (z.Haken >= 0 && hf.Contains(e.Location)) { if (!z.Gesperrt) LoeseHaken(z); return; }
        if (z.Aufklappbar && (PfeilFeld(z, r).Contains(e.Location) || !z.Waehlbar)) { LoeseAufklappen(z); return; }
        LoeseOeffnen(z);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        // Rollt die Liste nicht selbst oder steht sie am Anschlag, bekommt die Seite das Rad
        int vorher = rollen;
        if (!AutoHoehe) Rollen = rollen - Math.Sign(e.Delta) * 3 * ZeilenHoehe;
        HandledMouseEventArgs h = e as HandledMouseEventArgs;
        if (h != null) h.Handled = rollen != vorher;
        if (rollen == vorher) base.OnMouseWheel(e);
    }

    // ------------------------------------------------------------ Tastatur
    protected override bool IsInputKey(Keys k)
    {
        Keys c = k & Keys.KeyCode;
        return c == Keys.Up || c == Keys.Down || c == Keys.Left || c == Keys.Right || c == Keys.Enter || base.IsInputKey(k);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || Zeilen.Count == 0) return;
        int i = fokus != null ? Zeilen.IndexOf(fokus) : -1;
        int seite = Math.Max(1, Sichtbar / Math.Max(1, ZeilenHoehe) - 1);
        int neu = i;
        if (e.KeyCode == Keys.Down) neu = i + 1;
        else if (e.KeyCode == Keys.Up) neu = i - 1;
        else if (e.KeyCode == Keys.PageDown) neu = i + seite;
        else if (e.KeyCode == Keys.PageUp) neu = i - seite;
        else if (e.KeyCode == Keys.Home) neu = 0;
        else if (e.KeyCode == Keys.End) neu = Zeilen.Count - 1;
        else if (e.KeyCode == Keys.Space && fokus != null) { LoeseHaken(fokus); e.Handled = true; return; }
        else if (e.KeyCode == Keys.Right && fokus != null && fokus.Aufklappbar && !fokus.Offen) { LoeseAufklappen(fokus); e.Handled = true; return; }
        else if (e.KeyCode == Keys.Left && fokus != null && fokus.Aufklappbar && fokus.Offen) { LoeseAufklappen(fokus); e.Handled = true; return; }
        else if (e.KeyCode == Keys.Enter && fokus != null) { if (fokus.Verweis) LoeseVerweis(fokus); else LoeseOeffnen(fokus); e.Handled = true; return; }
        else return;
        int richtung = neu >= i ? 1 : -1;
        neu = Math.Max(0, Math.Min(Zeilen.Count - 1, neu));
        while (neu >= 0 && neu < Zeilen.Count && !Zeilen[neu].Waehlbar && !Zeilen[neu].Verweis) neu += richtung;
        if (neu < 0 || neu >= Zeilen.Count) { e.Handled = true; return; }
        ListenZeile z = Zeilen[neu];
        markiert.Clear(); if (z.Waehlbar) markiert.Add(z); fokus = z; anker = z;
        Zeige(z); Invalidate(); e.Handled = true;
        if (MarkierungGeaendert != null) MarkierungGeaendert(this, EventArgs.Empty);
    }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnResize(EventArgs e) { base.OnResize(e); if (rollen > RollenMax) rollen = RollenMax; }

    // ------------------------------------------------------------ Zeichnen
    public static Color KopfGrund { get { return UI.Mix(UI.Panel, UI.Text, UI.IsDark ? 0.045 : 0.035); } }
    public static Color UnterGrund { get { return UI.Mix(UI.Panel, UI.Bg, UI.IsDark ? 0.55 : 0.5); } }
    public static Color Trenner { get { return UI.Mix(UI.Panel, UI.Line, 0.55); } }

    // Text mit Auslassung in einem Rechteck
    public static void Schreibe(Graphics g, string text, Font f, Rectangle r, Color c, bool rechts)
    {
        if (String.IsNullOrEmpty(text) || r.Width <= 0) return;
        TextRenderer.DrawText(g, text, f, r, c, (rechts ? TextFormatFlags.Right : TextFormatFlags.Left) | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
    }
    public static int Breite(string text, Font f) { return String.IsNullOrEmpty(text) ? 0 : TextRenderer.MeasureText(text, f, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine).Width; }
    public static int EtikettBreite(string text) { return String.IsNullOrEmpty(text) ? 0 : TextRenderer.MeasureText(text, FEtikett, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width + UI.S(14); }

    // Etikett wie Badge, linksbündig bei x; liefert die Breite. Stufe "akzent" nutzt die Akzentfarbe.
    public static int Etikett(Graphics g, string text, string stufe, int x, Rectangle zeile, int festeBreite)
    {
        if (String.IsNullOrEmpty(text)) return 0;
        Color fg, bg;
        if (stufe == "akzent") { fg = UI.Accent; bg = UI.AccentSoft; } else UI.BadgeColors(stufe, out fg, out bg);
        int w = festeBreite > 0 ? festeBreite : EtikettBreite(text), h = UI.S(18);
        Rectangle r = new Rectangle(x, zeile.Y + (zeile.Height - h) / 2, w, h);
        using (GraphicsPath p = UI.Round(new RectangleF(r.X, r.Y, r.Width - 1, r.Height - 1), (h - 1) / 2f))
        using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
        TextRenderer.DrawText(g, text, FEtikett, r, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        return w;
    }

    public static void ZeichneHaken(Graphics g, Rectangle r, int zustand, bool gesperrt)
    {
        RectangleF f = new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1);
        using (GraphicsPath p = UI.Round(f, UI.SF(4)))
        {
            if (gesperrt)
            {
                using (SolidBrush b = new SolidBrush(UI.Mix(UI.Panel, UI.Line, 0.35))) g.FillPath(b, p);
                using (Pen pen = new Pen(UI.Line, 1f)) g.DrawPath(pen, p);
                return;
            }
            if (zustand == 0) { using (Pen pen = new Pen(UI.Muted, UI.SF(1.4f))) g.DrawPath(pen, p); return; }
            using (SolidBrush b = new SolidBrush(UI.Accent)) g.FillPath(b, p);
        }
        using (Pen pen = new Pen(UI.OnAccent, UI.SF(1.8f)))
        {
            pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round; pen.LineJoin = LineJoin.Round;
            float w = r.Width, x = r.X, y = r.Y;
            if (zustand == 2) g.DrawLine(pen, x + w * 0.27f, y + w * 0.5f, x + w * 0.73f, y + w * 0.5f);
            else g.DrawLines(pen, new PointF[] { new PointF(x + w * 0.25f, y + w * 0.52f), new PointF(x + w * 0.43f, y + w * 0.70f), new PointF(x + w * 0.76f, y + w * 0.32f) });
        }
    }

    static void ZeichnePfeil(Graphics g, Rectangle r, bool offen, Color c)
    {
        float cx = r.X + r.Width / 2f, cy = r.Y + r.Height / 2f, a = UI.SF(3.5f);
        using (Pen pen = new Pen(c, UI.SF(1.5f)))
        {
            pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round; pen.LineJoin = LineJoin.Round;
            if (offen) g.DrawLines(pen, new PointF[] { new PointF(cx - a, cy - a / 2), new PointF(cx, cy + a / 2), new PointF(cx + a, cy - a / 2) });
            else g.DrawLines(pen, new PointF[] { new PointF(cx - a / 2, cy - a), new PointF(cx + a / 2, cy), new PointF(cx - a / 2, cy + a) });
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(UI.Panel);
        int zb = ZeilenBreite, oben = Rand + KopfHoehe;
        // Zeilen
        Region alt = g.Clip;
        g.SetClip(new Rectangle(Rand, oben, Width - 2 * Rand, Math.Max(0, Height - oben - Rand)));
        int y = oben - rollen;
        using (Pen linie = new Pen(Trenner))
            for (int i = 0; i < Zeilen.Count; i++)
            {
                ListenZeile z = Zeilen[i]; int h = HoeheVon(z);
                if (y + h > oben && y < Height)
                {
                    Rectangle r = new Rectangle(Rand, y, zb, h);
                    bool m = markiert.Contains(z);
                    Color grund = m ? UI.AccentSoft : z.Grund == 1 ? KopfGrund : z.Grund == 2 ? UnterGrund : UI.Panel;
                    if (!m && z == hover && (z.Waehlbar || z.Aufklappbar || z.Verweis)) grund = UI.Mix(grund, UI.Text, 0.05);
                    using (SolidBrush b = new SolidBrush(grund)) g.FillRectangle(b, r);
                    g.DrawLine(linie, r.X, r.Bottom - 1, r.Right, r.Bottom - 1);
                    if (z.Haken >= 0) ZeichneHaken(g, HakenFeld(z, r), z.Haken, z.Gesperrt);
                    if (z.Aufklappbar) ZeichnePfeil(g, PfeilFeld(z, r), z.Offen, UI.Muted);
                    int x = r.X + InhaltLinks(z);
                    if (Zeichner != null) Zeichner(g, z, new Rectangle(x, r.Y, Math.Max(0, r.Right - x), h), m);
                    if (z == fokus && Focused && !m) using (Pen fp = new Pen(UI.Accent)) g.DrawRectangle(fp, r.X, r.Y, r.Width - 1, r.Height - 2);
                }
                y += h;
                if (y > Height) break;
            }
        if (Zeilen.Count == 0 && Leer.Length > 0)
            TextRenderer.DrawText(g, Leer, FText, new Rectangle(Rand + UI.S(12), oben, Width - UI.S(24), Math.Max(UI.S(40), Height - oben - Rand)), UI.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
        g.Clip = alt;
        // Spaltenkopf
        if (KopfHoehe > 0)
        {
            Rectangle kr = new Rectangle(Rand, Rand, zb, KopfHoehe);
            using (SolidBrush b = new SolidBrush(UI.Panel)) g.FillRectangle(b, new Rectangle(Rand, Rand, Width - 2 * Rand, KopfHoehe));
            for (int i = 0; i < Spalten.Count; i++)
            {
                Rectangle c = Zelle(i, kr);
                if (i == 0) { int ein = UI.S(10) + (ChevronPlatz ? UI.S(18) : 0) + (Zeilen.Count > 0 && Zeilen[0].Haken >= 0 ? UI.S(24) : 0); c = new Rectangle(c.X + ein, c.Y, Math.Max(0, c.Width - ein), c.Height); }
                bool aktiv = Spalten[i].Nr == SortSpalte;
                string t = Spalten[i].Titel + (aktiv ? (SortAuf ? " ▴" : " ▾") : "");
                Schreibe(g, t, FKopf, c, aktiv ? UI.Accent : i == hoverSpalte && Spalten[i].Sortierbar ? UI.Text : UI.Muted, Spalten[i].Rechts);
            }
            using (Pen p = new Pen(UI.Line)) g.DrawLine(p, Rand, Rand + KopfHoehe - 1, Width - Rand - 1, Rand + KopfHoehe - 1);
        }
        // Bildlaufleiste
        if (LeisteNoetig)
        {
            Rectangle s = Schieber();
            using (GraphicsPath p = UI.Round(new RectangleF(s.X, s.Y + 2, s.Width, s.Height - 4), s.Width / 2f))
            using (SolidBrush b = new SolidBrush(UI.Mix(UI.Panel, UI.Muted, zieht ? 0.75 : 0.45))) g.FillPath(b, p);
        }
        if (Rahmen) using (Pen p = new Pen(UI.Line)) g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
    }
}

// Kennzahlen nebeneinander (Seite Sensoren live): Titel mit Farbpunkt, Wert mit Einheit, eine Zeile darunter.
public class KennzahlZeile : Control
{
    string[] titel, einheit, wert, unter;
    Color[] farbe;
    public KennzahlZeile(string[] titel, string[] einheit, Color[] farbe)
    {
        this.titel = titel; this.einheit = einheit; this.farbe = farbe;
        wert = new string[titel.Length]; unter = new string[titel.Length];
        for (int i = 0; i < titel.Length; i++) { wert[i] = ""; unter[i] = ""; }
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = UI.Panel; AccessibleRole = AccessibleRole.Grouping;
        Size = new Size(UI.S(780), UI.S(74)); Margin = new Padding(0, 0, 0, UI.S(UI.Abstand3));
    }
    public int Anzahl { get { return titel.Length; } }
    public string Wert(int i) { return wert[i]; }
    public string Unter(int i) { return unter[i]; }
    // wert leer: "kein Wert" (nichts annehmen, was nicht gemessen ist)
    public void Setze(int i, string w, string u)
    {
        if (i < 0 || i >= titel.Length) return;
        w = w ?? ""; u = u ?? "";
        if (wert[i] == w && unter[i] == u) return;
        wert[i] = w; unter[i] = u; Invalidate();
    }
    public void Leere() { for (int i = 0; i < titel.Length; i++) { wert[i] = ""; unter[i] = ""; } Invalidate(); }
    int Je { get { return Width >= UI.S(700) ? titel.Length : Width >= UI.S(420) ? (titel.Length + 1) / 2 : 2; } }
    public int ZeilenAnzahl { get { return (titel.Length + Je - 1) / Je; } }
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        int h = ZeilenAnzahl * UI.S(74) + (ZeilenAnzahl - 1) * UI.S(8);
        if (Height != h) Height = h;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Parent != null ? Parent.BackColor : UI.Panel);
        int je = Je, gap = UI.S(8), kh = UI.S(74);
        int kw = (Width - (je - 1) * gap) / je;
        using (Font ft = new Font("Segoe UI", 8.75f)) using (Font fw = new Font("Segoe UI", 15f, FontStyle.Bold)) using (Font fe = new Font("Segoe UI", 9f)) using (Font fu = new Font("Segoe UI", 8.25f))
            for (int i = 0; i < titel.Length; i++)
            {
                Rectangle r = new Rectangle((i % je) * (kw + gap), (i / je) * (kh + gap), kw, kh);
                using (GraphicsPath p = UI.Round(new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 2, r.Height - 2), UI.SF(8)))
                {
                    using (SolidBrush b = new SolidBrush(ZeilenListe.KopfGrund)) g.FillPath(b, p);
                    using (Pen pen = new Pen(UI.Line)) g.DrawPath(pen, p);
                }
                int x = r.X + UI.S(12);
                using (SolidBrush b = new SolidBrush(farbe[i])) g.FillRectangle(b, x, r.Y + UI.S(13), UI.S(8), UI.S(8));
                TextRenderer.DrawText(g, titel[i], ft, new Rectangle(x + UI.S(13), r.Y + UI.S(8), kw - UI.S(30), UI.S(18)), UI.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                bool da = wert[i].Length > 0;
                string w = da ? wert[i] : "kein Wert";
                Rectangle wr = new Rectangle(x - UI.S(3), r.Y + UI.S(25), kw - UI.S(14), UI.S(28));
                if (da)
                {
                    int ww = TextRenderer.MeasureText(g, w, fw, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
                    TextRenderer.DrawText(g, w, fw, wr, UI.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, einheit[i], fe, new Rectangle(wr.X + ww + UI.S(9), wr.Y + UI.S(4), Math.Max(0, wr.Width - ww - UI.S(9)), UI.S(22)), UI.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
                else TextRenderer.DrawText(g, w, fe, new Rectangle(x, wr.Y, wr.Width, wr.Height), UI.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, unter[i], fu, new Rectangle(x, r.Y + UI.S(52), kw - UI.S(18), UI.S(16)), UI.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
    }
}

// Karte mit selbst gezeichnetem Inhalt und Bedienelementen darunter (Detailbereich der Seite Datenbank)
public class DetailKarte : CardPanel
{
    public Action<Graphics> Zeichne;
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Zeichne != null) Zeichne(e.Graphics);
    }
}
