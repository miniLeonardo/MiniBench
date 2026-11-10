// Dezente Bewegung der Oberfläche (ab v4.07): gemeinsamer Zeitgeber, Ringanzeige, Überblendung beim Seitenwechsel
// und die Farbpunkte der Akzentfarbe.
//
// Grundsätze: Alles ist kurz (höchstens 700 ms) und endet immer im Zielzustand. Ist die Bewegung abgeschaltet
// (Einstellungen), läuft eine Remotesitzung oder hat Windows Animationen abgeschaltet, wird der Zielzustand sofort
// gesetzt. Laufen keine Übergänge, steht der Zeitgeber; die Oberfläche hat dann keinerlei Mehraufwand.
public static class Bewegung
{
    // Einstellung "Animationen" (Einstellungen › Allgemein)
    public static bool Aktiv = true;
    // Bildabstand in ms (rund 60 Bilder je Sekunde; die Uhr von Windows Forms löst nicht feiner auf)
    public const int Takt = 15;
    public static int DauerErgebnis = 700;
    public static int DauerSeite = 140;

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SystemParametersInfo(uint action, uint param, out bool value, uint winIni);
    const uint SPI_GETCLIENTAREAANIMATION = 0x1042;

    static int windowsErlaubt = -1;

    // Windows-Einstellung "Animationseffekte" (Barrierefreiheit › Visuelle Effekte); einmal je Programmlauf gelesen
    static bool WindowsErlaubt()
    {
        if (windowsErlaubt < 0)
        {
            bool an = true;
            try { bool v; if (SystemParametersInfo(SPI_GETCLIENTAREAANIMATION, 0, out v, 0)) an = v; } catch { }
            try { if (SystemInformation.TerminalServerSession) an = false; } catch { }
            windowsErlaubt = an ? 1 : 0;
        }
        return windowsErlaubt == 1;
    }

    public static bool Erlaubt { get { return Aktiv && WindowsErlaubt(); } }

    class Lauf
    {
        public object Ziel; public long Start; public int Dauer;
        public Action<double> Schritt; public Action Ende;
    }

    static readonly List<Lauf> laeufe = new List<Lauf>();
    static readonly Stopwatch uhr = Stopwatch.StartNew();
    static Timer geber;

    public static int Laufend { get { return laeufe.Count; } }

    // Sanftes Auslaufen (kubisch): schnell los, ruhig ins Ziel
    public static double Verlauf(double t)
    {
        if (t <= 0) return 0; if (t >= 1) return 1;
        double u = 1 - t; return 1 - u * u * u;
    }

    // Startet einen Übergang für ziel. schritt bekommt 0 bis 1 (schon mit Verlauf), am Ende genau 1. Ein laufender
    // Übergang desselben Ziels wird ersetzt. Ohne Erlaubnis gilt sofort der Zielzustand.
    public static void Starte(object ziel, int dauerMs, Action<double> schritt, Action ende)
    {
        if (schritt == null) return;
        Stoppe(ziel);
        if (!Erlaubt || dauerMs <= 0)
        {
            try { schritt(1.0); } catch { }
            if (ende != null) try { ende(); } catch { }
            return;
        }
        try { schritt(0.0); } catch { return; }
        Lauf l = new Lauf(); l.Ziel = ziel; l.Start = uhr.ElapsedMilliseconds; l.Dauer = dauerMs; l.Schritt = schritt; l.Ende = ende;
        laeufe.Add(l);
        if (geber == null) { geber = new Timer(); geber.Interval = Takt; geber.Tick += delegate { Tick(uhr.ElapsedMilliseconds); }; }
        geber.Start();
    }

    public static void Starte(object ziel, int dauerMs, Action<double> schritt) { Starte(ziel, dauerMs, schritt, null); }

    // Bricht den Übergang eines Ziels ab, ohne den Zielzustand zu setzen (der Aufrufer setzt gleich einen neuen)
    public static void Stoppe(object ziel)
    {
        for (int i = laeufe.Count - 1; i >= 0; i--) if (Object.ReferenceEquals(laeufe[i].Ziel, ziel)) laeufe.RemoveAt(i);
        if (laeufe.Count == 0 && geber != null) geber.Stop();
    }

    // Ein Bild: jeden Lauf fortschreiben, fertige beenden. Öffentlich für den Selbsttest (Zeit in ms).
    public static void Tick(long jetzt)
    {
        Lauf[] alle = laeufe.ToArray();
        foreach (Lauf l in alle)
        {
            if (!laeufe.Contains(l)) continue;
            double t = l.Dauer > 0 ? (jetzt - l.Start) / (double)l.Dauer : 1;
            bool fertig = t >= 1;
            try { l.Schritt(fertig ? 1.0 : Verlauf(t)); } catch { fertig = true; }
            if (fertig)
            {
                laeufe.Remove(l);
                if (l.Ende != null) try { l.Ende(); } catch { }
            }
        }
        if (laeufe.Count == 0 && geber != null) geber.Stop();
    }

    public static long Jetzt { get { return uhr.ElapsedMilliseconds; } }
}

// Ringanzeige: Anteil eines Messwerts an der Referenz (voller Ring = 100 %). Die Zahl steht in der Mitte.
// Anteil (0 bis 1) ist der gezeichnete Teil des Werts und dient nur der Bewegung beim Einblenden.
public class RingAnzeige : Control
{
    double pct, anteil = 1;
    public double Percent { get { return pct; } set { pct = value; Invalidate(); } }
    public double Anteil { get { return anteil; } set { anteil = value < 0 ? 0 : value > 1 ? 1 : value; Invalidate(); } }

    public RingAnzeige()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(UI.S(52), UI.S(52));
        AccessibleRole = AccessibleRole.ProgressBar;
    }

    // Bogen in Grad für einen Prozentwert (über 100 % bleibt der Ring voll)
    public static float Bogen(double percent, double anteil)
    {
        double p = Math.Max(0, Math.Min(percent, 100)) * Math.Max(0, Math.Min(anteil, 1));
        return (float)(p / 100.0 * 360.0);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(UI.Panel);
        float d = UI.S(5);
        RectangleF r = new RectangleF(d / 2f + 1, d / 2f + 1, Width - d - 2, Height - d - 2);
        Color track = UI.IsDark ? Color.FromArgb(52, 56, 62) : Color.FromArgb(226, 229, 234);
        using (Pen p = new Pen(track, d)) g.DrawEllipse(p, r);
        // wie der Balken im Vergleich: Grün ab Referenzniveau, sonst Akzent (langsamer als die Referenz ist kein Fehler)
        Color fill = pct >= 90 ? UI.Ok : UI.Accent;
        float sweep = Bogen(pct, anteil);
        if (sweep > 0.5f)
            using (Pen p = new Pen(fill, d)) { p.StartCap = LineCap.Round; p.EndCap = LineCap.Round; g.DrawArc(p, r, -90f, sweep); }
        string txt = Math.Round(pct * anteil).ToString("0");
        using (Font f = new Font("Segoe UI Semibold", 9f))
            TextRenderer.DrawText(g, txt, f, new Rectangle(0, 0, Width, Height), UI.Text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        AccessibleName = pct.ToString("0") + " Prozent der Referenz";
    }
}

// Überblendung beim Seitenwechsel: Die bisherige Seite wird als Bild festgehalten und über der neuen ausgeblendet.
// Kostet eine Aufnahme vorher und eine nachher; dauert das auf einem PC länger als ZuLangsamMs, bleibt die
// Überblendung für diesen Programmlauf aus (der Seitenwechsel selbst darf nicht spürbar langsamer werden).
public class Ueberblendung : Control
{
    public const int ZuLangsamMs = 45;
    public const int MaxPixel = 3500000;
    public static bool Gesperrt;
    public static int LetzteAufnahmeMs;

    Bitmap alt, neu; double t;
    // höchstens eine Überblendung zugleich: Ein neuer Seitenwechsel beendet die laufende sofort
    static Ueberblendung aktuell;

    Ueberblendung()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque, true);
        TabStop = false;
    }

    // Bild eines Bereichs, oder null, wenn die Überblendung nicht in Frage kommt
    public static Bitmap Aufnahme(Control c)
    {
        if (Gesperrt || !Bewegung.Erlaubt || c == null || !c.IsHandleCreated || !c.Visible) return null;
        int w = c.ClientSize.Width, h = c.ClientSize.Height;
        if (w < 8 || h < 8 || (long)w * h > MaxPixel) return null;
        Form f = c.FindForm();
        if (f == null || !f.Visible || f.WindowState == FormWindowState.Minimized) return null;
        Stopwatch sw = Stopwatch.StartNew();
        Bitmap b = null;
        try
        {
            b = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            c.DrawToBitmap(b, new Rectangle(0, 0, w, h));
        }
        catch { if (b != null) b.Dispose(); b = null; }
        LetzteAufnahmeMs = (int)sw.ElapsedMilliseconds;
        if (LetzteAufnahmeMs > ZuLangsamMs) { Gesperrt = true; if (b != null) b.Dispose(); return null; }
        return b;
    }

    // Blendet von vorher (Bild des Bereichs vor dem Wechsel) auf den jetzigen Inhalt von c über. vorher wird verbraucht.
    public static void Starte(Control c, Bitmap vorher)
    {
        if (aktuell != null) { Bewegung.Stoppe(aktuell.bereich); aktuell.Schliessen(); }
        if (vorher == null) return;
        Ueberblendung u = null;
        try
        {
            // Die Fußleiste ist auf Werkzeugseiten flacher: Der Bereich kann danach etwas höher sein, das alte Bild liegt dann oben an
            if (c == null || c.Parent == null) { vorher.Dispose(); return; }
            u = new Ueberblendung();
            u.alt = vorher; u.t = 0; u.bereich = c;
            u.Bounds = c.Bounds;
            c.Parent.Controls.Add(u);
            u.BringToFront();
            u.neu = Aufnahme(c);
            if (u.neu == null) { u.Schliessen(); return; }
            Ueberblendung ziel = u; aktuell = u;
            Bewegung.Starte(c, Bewegung.DauerSeite, delegate(double x) { ziel.t = x; if (!ziel.IsDisposed) { ziel.Invalidate(); ziel.Update(); } }, delegate { ziel.Schliessen(); });
        }
        catch { if (u != null) u.Schliessen(); else vorher.Dispose(); }
    }

    Control bereich;

    void Schliessen()
    {
        if (Object.ReferenceEquals(aktuell, this)) aktuell = null;
        try { if (Parent != null) Parent.Controls.Remove(this); } catch { }
        if (alt != null) { alt.Dispose(); alt = null; }
        if (neu != null) { neu.Dispose(); neu = null; }
        try { Dispose(); } catch { }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.CompositingQuality = CompositingQuality.HighSpeed; g.InterpolationMode = InterpolationMode.NearestNeighbor;
        if (neu != null) g.DrawImageUnscaled(neu, 0, 0);
        else g.Clear(UI.Panel);
        if (alt == null || t >= 1) return;
        using (System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes())
        {
            System.Drawing.Imaging.ColorMatrix m = new System.Drawing.Imaging.ColorMatrix();
            m.Matrix33 = (float)(1 - t);
            ia.SetColorMatrix(m);
            g.DrawImage(alt, new Rectangle(0, 0, alt.Width, alt.Height), 0, 0, alt.Width, alt.Height, GraphicsUnit.Pixel, ia);
        }
    }
}

// Farbpunkt für die Wahl der Akzentfarbe: gefüllter Kreis, die gewählte Farbe trägt einen Ring
public class FarbPunkt : Control
{
    public string Name2 = "";
    bool gewaehlt, hover;
    public bool Gewaehlt { get { return gewaehlt; } set { gewaehlt = value; Invalidate(); } }

    public FarbPunkt(string name)
    {
        Name2 = name;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
        Size = new Size(UI.S(30), UI.S(30));
        Cursor = Cursors.Hand; TabStop = true;
        AccessibleRole = AccessibleRole.RadioButton; AccessibleName = "Akzentfarbe " + name;
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) { OnClick(EventArgs.Empty); e.Handled = true; }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(UI.Panel);
        Color c = UI.AkzentGrundton(Name2, UI.IsDark);
        float rand = UI.S(5);
        using (SolidBrush b = new SolidBrush(c)) g.FillEllipse(b, rand, rand, Width - 2 * rand - 1, Height - 2 * rand - 1);
        if (gewaehlt || hover || Focused)
            using (Pen p = new Pen(gewaehlt ? c : UI.Muted, gewaehlt ? UI.S(2) : 1f)) g.DrawEllipse(p, 1.5f, 1.5f, Width - 4, Height - 4);
    }
}
