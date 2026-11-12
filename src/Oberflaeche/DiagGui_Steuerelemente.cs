// Grafische Oberflaeche: Steuerelemente
public static class UI
{
    public static bool IsDark { get; set; }
    public static Color Bg { get; set; }
    public static Color Panel { get; set; }
    public static Color Text { get; set; }
    public static Color Muted { get; set; }
    public static Color Line { get; set; }
    public static Color Accent { get; set; }
    public static Color AccentHover { get; set; }
    public static Color AccentActive { get; set; }
    public static Color AccentDark { get; set; }
    public static Color AccentSoft { get; set; }
    public static Color Header { get; set; }
    public static Color Crit { get; set; }
    public static Color CritBg { get; set; }
    public static Color Warn { get; set; }
    public static Color WarnBg { get; set; }
    public static Color Info { get; set; }
    public static Color InfoBg { get; set; }
    public static Color Ok { get; set; }
    public static Color OkBg { get; set; }
    public static Color Skip { get; set; }
    public static Color SkipBg { get; set; }
    // ab v3.64: Schrift auf Akzentflächen (Primärknöpfe, angehakte Kästchen); im Dunkelschema dunkel, sonst weiß
    public static Color OnAccent { get; set; }

    // ab v3.64: Gestaltungsregeln für alle Seiten (Werte in Pixeln bei 96 dpi, immer über S() verwenden)
    public const int Abstand1 = 4, Abstand2 = 8, Abstand3 = 12, Abstand4 = 16, Abstand5 = 24;
    public const int Gruppenbreite = 780;      // Breite von Gruppen, Tabellen und Hinweistexten
    public const int StartGruppenbreite = 860; // Breite für Startseiten-Karten und Raster (ab v3.72)
    public const int Seitenrand = 24;          // Innenabstand jeder Seite links und oben
    public const float SchriftTitel = 15f, SchriftAbschnitt = 11.5f, SchriftGruppe = 10f, SchriftText = 9.5f, SchriftHinweis = 9f;

    public static float DpiScale = 1.0f;

    static UI()
    {
        DpiScale = 1.0f;
        try
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                if (g.DpiX > 0) DpiScale = g.DpiX / 96.0f;
            }
        }
        catch { DpiScale = 1.0f; }
        if (DpiScale <= 0.1f) DpiScale = 1.0f;
        SetTheme(false);
    }

    public static void SetTheme(bool dark)
    {
        IsDark = dark;
        if (dark)
        {
            Bg = Color.FromArgb(24, 25, 26);           // #18191A
            Panel = Color.FromArgb(36, 37, 38);        // #242526
            Text = Color.FromArgb(245, 246, 247);      // #F5F6F7
            Muted = Color.FromArgb(156, 163, 175);     // #9CA3AF
            Line = Color.FromArgb(58, 59, 60);         // #3A3B3C
            Header = Color.FromArgb(18, 19, 20);       // #121314
            Accent = Color.FromArgb(76, 194, 255);     // #4CC2FF
            AccentHover = Color.FromArgb(96, 205, 255);
            AccentActive = Color.FromArgb(60, 160, 220);
            AccentDark = Color.FromArgb(30, 78, 121);
            AccentSoft = Color.FromArgb(35, 52, 70);   // #233446
            Crit = Color.FromArgb(255, 153, 164);      // #FF99A4
            CritBg = Color.FromArgb(68, 39, 38);       // #442726
            Warn = Color.FromArgb(252, 225, 0);        // #FCE100
            WarnBg = Color.FromArgb(63, 51, 22);       // #3F3316
            Info = Color.FromArgb(76, 194, 255);       // #4CC2FF
            InfoBg = Color.FromArgb(35, 52, 70);       // #233446
            Ok = Color.FromArgb(108, 203, 95);         // #6CCB5F
            OkBg = Color.FromArgb(27, 56, 40);         // #1B3828
            Skip = Color.FromArgb(156, 163, 175);     // #9CA3AF
            SkipBg = Color.FromArgb(51, 51, 51);       // #333333
            OnAccent = Color.FromArgb(18, 19, 20);     // #121314 auf #4CC2FF, etwa 9:1
        }
        else
        {
            Bg = Color.FromArgb(249, 249, 251);        // #F9F9FB
            Panel = Color.White;                       // #FFFFFF
            Text = Color.FromArgb(28, 29, 31);         // #1C1D1F
            Muted = Color.FromArgb(95, 99, 104);       // #5F6368
            Line = Color.FromArgb(229, 231, 235);      // #E5E7EB
            Header = Color.FromArgb(22, 30, 46);       // #161E2E
            Accent = Color.FromArgb(0, 103, 192);
            AccentHover = Color.FromArgb(0, 90, 158);
            AccentActive = Color.FromArgb(0, 79, 138);
            AccentDark = Color.FromArgb(0, 79, 138);
            AccentSoft = Color.FromArgb(235, 243, 251);
            Crit = Color.FromArgb(196, 43, 28);
            CritBg = Color.FromArgb(253, 231, 233);
            Warn = Color.FromArgb(157, 93, 0);
            WarnBg = Color.FromArgb(255, 244, 206);
            Info = Color.FromArgb(0, 103, 192);
            InfoBg = Color.FromArgb(235, 243, 251);
            Ok = Color.FromArgb(16, 124, 65);
            OkBg = Color.FromArgb(223, 246, 221);
            Skip = Color.FromArgb(95, 99, 104);
            SkipBg = Color.FromArgb(243, 244, 246);
            OnAccent = Color.White;                    // auf #0067C0, etwa 5,9:1
        }
        SetzeAkzent(dark);
    }

    // ------------------------------------------------------------ Akzentfarbe (ab v4.07)
    // Wählbar in den Einstellungen. "Blau" ist der bisherige Stand und bleibt unverändert. Jede andere Farbe hat je
    // Farbschema einen Grundton; Hover, gedrückt, dunkel und weich werden daraus gemischt. "Windows" übernimmt die
    // Akzentfarbe aus den Windows-Einstellungen. Info-Etiketten (Ändern) bleiben blau: Sie tragen eine Bedeutung.
    public static string Akzent = "Blau";
    public static readonly string[] AkzentNamen = new string[] { "Blau", "Türkis", "Violett", "Pink", "Orange", "Windows" };

    public static string AkzentGueltig(string name)
    {
        foreach (string n in AkzentNamen) if (String.Equals(n, name, StringComparison.OrdinalIgnoreCase)) return n;
        return "Blau";
    }

    // Grundton einer Akzentfarbe: im hellen Schema dunkel genug für weiße Schrift, im dunklen hell genug für dunkle Schrift
    public static Color AkzentGrundton(string name, bool dark)
    {
        switch (AkzentGueltig(name))
        {
            case "Türkis": return dark ? Color.FromArgb(77, 208, 196) : Color.FromArgb(0, 121, 107);
            case "Violett": return dark ? Color.FromArgb(184, 164, 255) : Color.FromArgb(105, 77, 190);
            case "Pink": return dark ? Color.FromArgb(255, 143, 200) : Color.FromArgb(190, 40, 124);
            case "Orange": return dark ? Color.FromArgb(255, 184, 108) : Color.FromArgb(176, 80, 8);
            case "Windows": return MitKontrast(WindowsAkzent(), dark);
            default: return dark ? Color.FromArgb(76, 194, 255) : Color.FromArgb(0, 103, 192);
        }
    }

    // Akzentfarbe aus den Windows-Einstellungen (HKCU\...\DWM\AccentColor, Byte-Reihenfolge ABGR); ohne Wert das eigene Blau
    public static Color WindowsAkzent()
    {
        try
        {
            using (Microsoft.Win32.RegistryKey k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
            {
                object v = k != null ? k.GetValue("AccentColor") : null;
                if (v != null) return AusAbgr(unchecked((uint)Convert.ToInt32(v, CultureInfo.InvariantCulture)));
            }
        }
        catch { }
        return Color.FromArgb(0, 103, 192);
    }

    public static Color AusAbgr(uint abgr) { return Color.FromArgb((int)(abgr & 0xFF), (int)((abgr >> 8) & 0xFF), (int)((abgr >> 16) & 0xFF)); }

    // Mischt a mit b (t = Anteil von b, 0 bis 1)
    public static Color Mix(Color a, Color b, double t)
    {
        if (t < 0) t = 0; if (t > 1) t = 1;
        return Color.FromArgb((int)Math.Round(a.R + (b.R - a.R) * t), (int)Math.Round(a.G + (b.G - a.G) * t), (int)Math.Round(a.B + (b.B - a.B) * t));
    }

    // Beliebige Farbe so weit abdunkeln (hell) oder aufhellen (dunkel), dass Schrift darauf und sie selbst auf der Seite lesbar sind
    public static Color MitKontrast(Color c, bool dark)
    {
        Color ziel = dark ? Color.White : Color.Black;
        Color flaeche = dark ? Color.FromArgb(36, 37, 38) : Color.White;
        Color schrift = dark ? Color.FromArgb(18, 19, 20) : Color.White;
        for (int i = 0; i <= 20; i++)
        {
            Color x = Mix(c, ziel, i / 20.0);
            if (ContrastRatio(x, flaeche) >= 4.5 && ContrastRatio(schrift, x) >= 4.5) return x;
        }
        return dark ? Color.FromArgb(76, 194, 255) : Color.FromArgb(0, 103, 192);
    }

    // Setzt die Akzenttöne des aktuellen Schemas nach UI.Akzent. Blau behält die festen Werte aus SetTheme.
    public static void SetzeAkzent(bool dark)
    {
        Akzent = AkzentGueltig(Akzent);
        if (Akzent == "Blau") return;
        Color a = AkzentGrundton(Akzent, dark);
        Accent = a;
        if (dark)
        {
            AccentHover = Mix(a, Color.White, 0.18);
            AccentActive = Mix(a, Color.Black, 0.18);
            AccentDark = Mix(a, Bg, 0.62);
            AccentSoft = Mix(a, Panel, 0.84);
            OnAccent = Color.FromArgb(18, 19, 20);
        }
        else
        {
            AccentHover = Mix(a, Color.Black, 0.14);
            AccentActive = Mix(a, Color.Black, 0.26);
            AccentDark = AccentActive;
            AccentSoft = Mix(a, Color.White, 0.91);
            OnAccent = Color.White;
        }
    }

    public static int S(int px)
    {
        return (int)Math.Round(px * DpiScale);
    }

    // Nur für Pixelmaße. Schriftgrößen in Punkt nie mit SF skalieren: Windows rechnet Punkt bei 150 % selbst um,
    // SF ergab dort doppelt große Schrift in selbst gezeichneten Elementen und Knöpfen (bis v4.0).
    public static float SF(float px)
    {
        return px * DpiScale;
    }

    public static string SymbolFontFamily = ResolveSymbolFontFamily();
    static string ResolveSymbolFontFamily()
    {
        try
        {
            using (Font f = new Font("Segoe Fluent Icons", 9f))
            {
                if (f.Name == "Segoe Fluent Icons") return "Segoe Fluent Icons";
            }
        }
        catch { }
        try
        {
            using (Font f = new Font("Segoe MDL2 Assets", 9f))
            {
                if (f.Name == "Segoe MDL2 Assets") return "Segoe MDL2 Assets";
            }
        }
        catch { }
        return "Segoe UI Symbol";
    }

    public static Font SymbolFont(float size)
    {
        return new Font(SymbolFontFamily, size);
    }

    public const string IcoCpu = "\uE7F8";
    public const string IcoRam = "\uE7F4";
    public const string IcoGpu = "\uE790";
    public const string IcoDisk = "\uEDA2";
    public const string IcoDiag = "\uE9D9";
    public const string IcoWartung = "\uE90F";
    public const string IcoOpt = "\uE713";
    public const string IcoSens = "\uE950";
    public const string IcoDb = "\uE81E";
    public const string IcoChg = "\uE81C";
    public const string IcoFlame = "\uECAD";
    public const string IcoTools = "\uE71D";
    public const string IcoHome = "\uE80F";
    public const string IcoSpeed = "\uEC4A";
    public const string IcoBackup = "\uEA35";
    public const string IcoShield = "\uE730";
    public const string IcoBell = "\uEA8F";
    public const string IcoFolder = "\uE8B7";
    public const string IcoRefresh = "\uE72C";
    public const string IcoGear = "\uE713";
    public const string IcoApp = "\uE71D";
    public const string IcoCheck = "\uE73E";
    public const string IcoTrash = "\uE74D";
    public const string IcoSearch = "\uE721";
    public const string IcoLock = "\uE72E";
    public const string IcoSun = "\uE706";
    // ab v4.08: Ergebnis (Seitenleiste), Energie und parallele Ausführung, Systemintegrität
    public const string IcoErgebnis = "\uE9D2";
    public const string IcoBlitz = "\uE945";
    public const string IcoDokument = "\uE8A5";

    // Symbol nach Namen (Katalog und Vertrag nennen Felder wie "IcoShield"); unbekannt oder leer: ersatz
    public static string IconByName(string name, string ersatz)
    {
        if (String.IsNullOrEmpty(name)) return ersatz;
        try
        {
            System.Reflection.FieldInfo fi = typeof(UI).GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (fi != null) { string v = fi.GetValue(null) as string; if (!String.IsNullOrEmpty(v)) return v; }
        }
        catch { }
        return ersatz;
    }

    // ab v4.0: Dezimale Versionsnummer zur verlaesslichen Groessenordnung (z. B. 3.882 < 3.9 < 3.91 < 4.0)
    public static double VersionNumber(string v)
    {
        if (string.IsNullOrEmpty(v)) return 0.0;
        string s = v.Trim();
        if (s.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(1).Trim();
        int dot = s.IndexOf('.');
        if (dot < 0)
        {
            double res;
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out res)) return res;
            return 0.0;
        }
        string majStr = s.Substring(0, dot);
        string minStr = s.Substring(dot + 1).Replace(".", "");
        StringBuilder minDigits = new StringBuilder();
        for (int i = 0; i < minStr.Length; i++)
        {
            if (char.IsDigit(minStr[i])) minDigits.Append(minStr[i]);
            else break;
        }
        double maj = 0.0, frac = 0.0;
        double.TryParse(majStr, NumberStyles.Float, CultureInfo.InvariantCulture, out maj);
        if (minDigits.Length > 0)
        {
            double.TryParse("0." + minDigits.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out frac);
        }
        return maj + frac;
    }

    // ab v4.0: Kategoriesymbol aufloesen fuer Katalog- und Vertragsdefinitionen
    public static string SymbolByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return IcoOpt;
        string n = name.Trim().ToLowerInvariant();
        if (n.StartsWith("ico")) n = n.Substring(3);
        if (n == "shield" || n == "sicherheit" || n == "datenschutz") return IcoShield;
        if (n == "bell" || n == "werbung") return IcoBell;
        if (n == "search" || n == "ki" || n == "suche") return IcoSearch;
        if (n == "lock" || n == "berechtigung") return IcoLock;
        if (n == "gear" || n == "hintergrund" || n == "dienste" || n == "opt") return IcoGear;
        if (n == "check" || n == "funktionen") return IcoCheck;
        if (n == "app" || n == "apps") return IcoApp;
        if (n == "folder" || n == "ordner" || n == "bedienung" || n == "systemdateien") return IcoFolder;
        if (n == "sun" || n == "sonne" || n == "darstellung") return IcoSun;
        if (n == "flame" || n == "leistung") return IcoFlame;
        if (n == "refresh" || n == "update") return IcoRefresh;
        if (n == "gpu" || n == "grafik") return IcoGpu;
        if (n == "trash" || n == "entfernen" || n == "bereinigung") return IcoTrash;
        if (n == "cpu") return IcoCpu;
        if (n == "ram") return IcoRam;
        if (n == "disk") return IcoDisk;
        if (n == "diag") return IcoDiag;
        if (n == "wartung") return IcoWartung;
        if (n == "tools") return IcoTools;
        if (n == "home") return IcoHome;
        if (n == "speed") return IcoSpeed;
        if (n == "backup") return IcoBackup;
        return IcoOpt;
    }

    public static GraphicsPath Round(RectangleF r, float rad)
    {
        GraphicsPath p = new GraphicsPath();
        float d = Math.Max(1f, Math.Min(rad * 2, Math.Min(r.Width, r.Height)));
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    public static GraphicsPath Round(Rectangle r, float rad)
    {
        return Round(new RectangleF(r.X, r.Y, r.Width, r.Height), rad);
    }

    public static void Level(string lvl, out Color fg, out Color bg)
    {
        string l = (lvl ?? "").ToUpperInvariant();
        if (l == "KRITISCH" || l == "FEHLER") { fg = Crit; bg = CritBg; }
        else if (l == "WARNUNG") { fg = Warn; bg = WarnBg; }
        else if (l == "OK" || l == "REPARIERT") { fg = Ok; bg = OkBg; }
        else if (l == "INFO") { fg = Info; bg = InfoBg; }
        else { fg = Skip; bg = SkipBg; }
    }

    // Kontrastverhältnis nach WCAG 2.1 (1 bis 21)
    public static double ContrastRatio(Color a, Color b)
    {
        double la = Luminance(a), lb = Luminance(b);
        double hi = Math.Max(la, lb), lo = Math.Min(la, lb);
        return (hi + 0.05) / (lo + 0.05);
    }
    static double Luminance(Color c)
    {
        double[] v = new double[] { c.R / 255.0, c.G / 255.0, c.B / 255.0 };
        for (int i = 0; i < 3; i++) v[i] = v[i] <= 0.03928 ? v[i] / 12.92 : Math.Pow((v[i] + 0.055) / 1.055, 2.4);
        return 0.2126 * v[0] + 0.7152 * v[1] + 0.0722 * v[2];
    }

    // Farbpaar eines Etiketts: info, warn, crit, ok, skip
    public static void BadgeColors(string level, out Color fg, out Color bg)
    {
        string l = (level ?? "").ToLowerInvariant();
        if (l == "warn") { fg = Warn; bg = WarnBg; }
        else if (l == "crit") { fg = Crit; bg = CritBg; }
        else if (l == "ok") { fg = Ok; bg = OkBg; }
        else if (l == "info") { fg = Info; bg = InfoBg; }
        else { fg = Skip; bg = SkipBg; }
    }

    // Risikostufe aus Modulvertrag oder Katalog als Etikett: Ändern blau, Eingriff gelb, Zerstörend rot, Lesen grün
    public static string RiskLevel(string risiko)
    {
        string r = (risiko ?? "").ToLowerInvariant();
        if (r == "aendern" || r == "ändern") return "info";
        if (r == "eingriff") return "warn";
        if (r == "zerstoerend" || r == "zerstörend") return "crit";
        if (r == "lesen") return "ok";
        return "skip";
    }

    // Symbol aus der Symbolschrift als kleines Bild (für Knöpfe mit Symbol vor dem Text)
    public static Bitmap GlyphImage(string glyph, Color color, int size)
    {
        Bitmap bmp = new Bitmap(Math.Max(1, size), Math.Max(1, size));
        // GDI+ statt TextRenderer: TextRenderer (GDI) lässt den Alphakanal auf transparentem Grund bei 0
        using (Graphics g = Graphics.FromImage(bmp))
        using (Font f = SymbolFont(size * 0.72f * 0.75f))
        using (SolidBrush br = new SolidBrush(color))
        using (StringFormat sf = new StringFormat())
        {
            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center;
            g.DrawString(glyph, f, br, new RectangleF(0, 0, size, size), sf);
        }
        return bmp;
    }

    public const string IcoInfo = "";
    public const string IcoSonne = "";
    public const string IcoMond = "";
    public const string IcoAdmin = "";
    public const string IcoOrdner = "";
    public const string IcoLaden = "";
    public const string IcoEntfernen = "";
    public const string IcoTorte = "";
    public const string IcoGeraete = "";
    public const string IcoNetz = "";
    public const string IcoSpeichern = "";
    public const string IcoAuf = "";
    public const string IcoZu = "";
    public const string IcoWarnung = "";

    public static Button Primary(string text)
    {
        Button b = new PrimaryButton(); b.Text = text; b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
        b.BackColor = Accent; b.ForeColor = OnAccent; b.FlatAppearance.MouseOverBackColor = AccentHover; b.FlatAppearance.MouseDownBackColor = AccentActive;
        b.Font = new Font("Segoe UI Semibold", 10.5f); b.AutoSize = true; b.Padding = new Padding(S(18), S(6), S(18), S(6)); b.Cursor = Cursors.Hand; b.UseVisualStyleBackColor = false;
        return b;
    }

    public static Button Secondary(string text)
    {
        Button b = new Button(); b.Text = text; b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderColor = Line;
        b.BackColor = Panel; b.ForeColor = Text; b.FlatAppearance.MouseOverBackColor = IsDark ? Color.FromArgb(45, 48, 52) : Color.FromArgb(243, 244, 246); b.FlatAppearance.MouseDownBackColor = IsDark ? Color.FromArgb(55, 58, 62) : Color.FromArgb(235, 237, 240);
        b.Font = new Font("Segoe UI", 9.75f); b.AutoSize = true; b.Padding = new Padding(S(10), S(3), S(10), S(3)); b.Margin = new Padding(S(8), 0, 0, 0); b.Cursor = Cursors.Hand; b.UseVisualStyleBackColor = false;
        b.Paint += delegate(object s, PaintEventArgs pe) {
            if (!b.Enabled) {
                Graphics g = pe.Graphics;
                Color disBg = IsDark ? Color.FromArgb(32, 33, 35) : Color.FromArgb(243, 244, 246);
                Color disLine = IsDark ? Color.FromArgb(48, 50, 52) : Color.FromArgb(220, 222, 226);
                Color disText = IsDark ? Color.FromArgb(140, 145, 155) : Color.FromArgb(160, 163, 168);
                using (SolidBrush bg = new SolidBrush(disBg)) g.FillRectangle(bg, b.ClientRectangle);
                using (Pen p = new Pen(disLine)) g.DrawRectangle(p, 0, 0, b.Width - 1, b.Height - 1);
                TextRenderer.DrawText(g, b.Text, b.Font, b.ClientRectangle, disText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        };
        return b;
    }

    public static Button SkipStepButton()
    {
        return Secondary("Diesen Schritt überspringen");
    }

    [System.Runtime.InteropServices.DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    // ab v4.01: Titelleiste eines Dialogs im Farbschema (vorher blieb sie im Dunkelschema hell)
    public static void ThemeTitleBar(Form f)
    {
        if (f == null) return;
        if (f.IsHandleCreated) SetTitleBar(f);
        else f.HandleCreated += delegate { SetTitleBar(f); };
    }
    static void SetTitleBar(Form f)
    {
        try { int dark = IsDark ? 1 : 0; if (DwmSetWindowAttribute(f.Handle, 20, ref dark, sizeof(int)) != 0) DwmSetWindowAttribute(f.Handle, 19, ref dark, sizeof(int)); } catch { }
    }

    public static void ThemeDialog(Form f)
    {
        if (f == null) return;
        ThemeTitleBar(f);
        f.BackColor = Bg;
        f.ForeColor = Text;
        if (f.Font == null || f.Font.Name != "Segoe UI")
        {
            f.Font = new Font("Segoe UI", 9.5f);
        }
        ThemeDialogControls(f.Controls);
    }

    public static void ThemeDialogControls(Control.ControlCollection controls)
    {
        if (controls == null) return;
        foreach (Control c in controls)
        {
            ThemeDialogControl(c);
        }
    }

    public static void ThemeDialogControl(Control c)
    {
        if (c == null) return;
        if (c is TextBox || c is ComboBox)
        {
            c.BackColor = Panel;
            c.ForeColor = Text;
        }
        else if (c is LinkLabel)
        {
            LinkLabel ll = (LinkLabel)c;
            ll.LinkColor = Accent;
            ll.ActiveLinkColor = AccentHover;
            ll.ForeColor = Text;
        }
        else if (c is CheckBox || c is RadioButton)
        {
            c.ForeColor = c.Enabled ? Text : Muted;
            c.BackColor = Color.Transparent;
        }
        else if (c is Label)
        {
            Color fc = c.ForeColor;
            if (fc == Color.FromArgb(95, 99, 104) || fc == Color.FromArgb(156, 163, 175) || fc == Color.Gray)
            {
                c.ForeColor = Muted;
            }
            else
            {
                c.ForeColor = Text;
            }
        }
        else if (c is ListView || c is ListBox)
        {
            c.BackColor = Panel;
            c.ForeColor = Text;
        }
        else if (c is Button)
        {
            Button b = (Button)c;
            if (b.ForeColor == SystemColors.ControlText || b.ForeColor.ToArgb() == Color.Black.ToArgb())
            {
                b.ForeColor = Text;
                if (b.BackColor == SystemColors.Control) b.BackColor = Panel;
            }
        }
        else
        {
            if (c.ForeColor == SystemColors.ControlText || c.ForeColor.ToArgb() == Color.Black.ToArgb())
            {
                c.ForeColor = Text;
            }
        }
        if (c.HasChildren)
        {
            ThemeDialogControls(c.Controls);
        }
    }

    // ab v4.0: Thematisierte Rueckfragedialoge fuer konsistenten Hell- und Dunkelmodus (ohne ungestyltes MessageBox.Show)
    public static bool Ask(string text, string title = "Frage", IWin32Window owner = null)
    {
        using (Form f = new Form())
        {
            f.Text = title ?? "Frage";
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.ShowInTaskbar = false;
            f.StartPosition = owner != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;

            int w = S(450);
            Font mf = new Font("Segoe UI", 9.5f);
            Size textSize = TextRenderer.MeasureText(text ?? "", mf, new Size(w - S(40), int.MaxValue), TextFormatFlags.WordBreak);
            int textH = Math.Max(S(40), Math.Min(S(420), textSize.Height + S(10)));
            int clientH = textH + S(74);
            f.ClientSize = new Size(w, clientH);

            Label lbl = new Label();
            lbl.Text = text ?? "";
            lbl.Font = mf;
            lbl.Location = new Point(S(20), S(16));
            lbl.Size = new Size(w - S(40), textH);
            lbl.TextAlign = ContentAlignment.TopLeft;

            Button btnOk = Primary("Ja");
            btnOk.DialogResult = DialogResult.Yes;
            Button btnNo = Secondary("Nein");
            btnNo.DialogResult = DialogResult.No;

            int btnY = clientH - btnOk.Height - S(14);
            btnNo.Location = new Point(w - btnNo.Width - S(20), btnY);
            btnOk.Location = new Point(btnNo.Left - btnOk.Width - S(10), btnY);

            f.Controls.Add(lbl);
            f.Controls.Add(btnOk);
            f.Controls.Add(btnNo);
            f.AcceptButton = btnOk;
            f.CancelButton = btnNo;

            UI.ThemeDialog(f);
            return f.ShowDialog(owner) == DialogResult.Yes;
        }
    }
    public static bool Ask(IWin32Window owner, string text, string title)
    {
        return Ask(text, title, owner);
    }

    // ab v4.0: Thematisierter Hinweisdialog fuer konsistenten Hell- und Dunkelmodus
    public static void Hinweis(string text, string title = "Hinweis", IWin32Window owner = null)
    {
        using (Form f = new Form())
        {
            f.Text = title ?? "Hinweis";
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.ShowInTaskbar = false;
            f.StartPosition = owner != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;

            int w = S(450);
            Font mf = new Font("Segoe UI", 9.5f);
            Size textSize = TextRenderer.MeasureText(text ?? "", mf, new Size(w - S(40), int.MaxValue), TextFormatFlags.WordBreak);
            int textH = Math.Max(S(40), Math.Min(S(420), textSize.Height + S(10)));
            int clientH = textH + S(74);
            f.ClientSize = new Size(w, clientH);

            Label lbl = new Label();
            lbl.Text = text ?? "";
            lbl.Font = mf;
            lbl.Location = new Point(S(20), S(16));
            lbl.Size = new Size(w - S(40), textH);
            lbl.TextAlign = ContentAlignment.TopLeft;

            Button btnOk = Primary("OK");
            btnOk.DialogResult = DialogResult.OK;
            int btnY = clientH - btnOk.Height - S(14);
            btnOk.Location = new Point(w - btnOk.Width - S(20), btnY);

            f.Controls.Add(lbl);
            f.Controls.Add(btnOk);
            f.AcceptButton = btnOk;

            UI.ThemeDialog(f);
            f.ShowDialog(owner);
        }
    }
    public static void Hinweis(IWin32Window owner, string text, string title)
    {
        Hinweis(text, title, owner);
    }
    public static void ShowInfo(string text, string title = "Hinweis", IWin32Window owner = null)
    {
        Hinweis(text, title, owner);
    }
    public static void ShowInfo(IWin32Window owner, string text, string title)
    {
        Hinweis(text, title, owner);
    }

    // ab v4.0: Thematisierter 3-Wege-Dialog (Ja / Nein / Abbrechen)
    public static DialogResult Ask3(string text, string title = "Frage", IWin32Window owner = null)
    {
        using (Form f = new Form())
        {
            f.Text = title ?? "Frage";
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.ShowInTaskbar = false;
            f.StartPosition = owner != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;

            int w = S(500);
            Font mf = new Font("Segoe UI", 9.5f);
            Size textSize = TextRenderer.MeasureText(text ?? "", mf, new Size(w - S(40), int.MaxValue), TextFormatFlags.WordBreak);
            int textH = Math.Max(S(40), Math.Min(S(420), textSize.Height + S(10)));
            int clientH = textH + S(74);
            f.ClientSize = new Size(w, clientH);

            Label lbl = new Label();
            lbl.Text = text ?? "";
            lbl.Font = mf;
            lbl.Location = new Point(S(20), S(16));
            lbl.Size = new Size(w - S(40), textH);
            lbl.TextAlign = ContentAlignment.TopLeft;

            Button btnCancel = Secondary("Abbrechen");
            btnCancel.DialogResult = DialogResult.Cancel;
            Button btnNo = Secondary("Nein");
            btnNo.DialogResult = DialogResult.No;
            Button btnYes = Primary("Ja");
            btnYes.DialogResult = DialogResult.Yes;

            int btnY = clientH - btnYes.Height - S(14);
            btnCancel.Location = new Point(w - btnCancel.Width - S(20), btnY);
            btnNo.Location = new Point(btnCancel.Left - btnNo.Width - S(10), btnY);
            btnYes.Location = new Point(btnNo.Left - btnYes.Width - S(10), btnY);

            f.Controls.Add(lbl);
            f.Controls.Add(btnYes);
            f.Controls.Add(btnNo);
            f.Controls.Add(btnCancel);
            f.AcceptButton = btnYes;
            f.CancelButton = btnCancel;

            UI.ThemeDialog(f);
            return f.ShowDialog(owner);
        }
    }
}

// ab v3.65: Karte mit abgerundetem Rahmen für Gruppen auf allen Seiten (wie FluentCard), Inhalt als Kindelemente
public class CardPanel : Panel
{
    public CardPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = UI.Panel; Padding = new Padding(UI.S(14), UI.S(10), UI.S(14), UI.S(10));
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        Color outer = Parent != null ? Parent.BackColor : UI.Panel;
        using (SolidBrush ob = new SolidBrush(outer)) g.FillRectangle(ob, ClientRectangle);
        using (GraphicsPath p = UI.Round(new RectangleF(0.5f, 0.5f, Width - 2, Height - 2), UI.SF(8)))
        {
            using (SolidBrush b = new SolidBrush(UI.Panel)) g.FillPath(b, p);
            using (Pen pen = new Pen(UI.Line, 1f)) g.DrawPath(pen, p);
        }
    }
}

// ab v3.64: Primärknopf als eigener Typ, damit der Farbwechsel ihn ohne Farbvergleich erkennt
public class PrimaryButton : Button { }

// ab v3.64: kleines farbiges Etikett (Risikostufe, Neustart, Werkzeugstatus, Zustand)
public class Badge : Control
{
    string level = "skip";
    public Badge(string text, string level)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Font = new Font("Segoe UI Semibold", 8.25f);
        AccessibleRole = AccessibleRole.StaticText;
        Margin = new Padding(UI.S(6), UI.S(4), 0, 0);
        this.level = level ?? "skip";
        Text = text ?? "";
        AccessibleName = Text;
        FitSize();
    }
    public string Level { get { return level; } set { level = value ?? "skip"; Invalidate(); } }
    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); AccessibleName = Text; FitSize(); Invalidate(); }
    void FitSize()
    {
        Size t = TextRenderer.MeasureText(Text.Length > 0 ? Text : " ", Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
        Size = new Size(t.Width + UI.S(14), Math.Max(UI.S(18), t.Height + UI.S(4)));
        Visible = Text.Length > 0;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Color fg, bg; UI.BadgeColors(level, out fg, out bg);
        if (!Enabled) { fg = UI.Muted; bg = UI.SkipBg; }
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        using (GraphicsPath p = UI.Round(new RectangleF(0, 0, Width - 1, Height - 1), (Height - 1) / 2f))
        using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
        TextRenderer.DrawText(g, Text, Font, ClientRectangle, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }
}

// ab v3.72: Pill-Etikett für Hardware-Schlüsseldaten auf der Startseite
public class PillLabel : Control
{
    public PillLabel(string text)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Font = new Font("Segoe UI", 8.75f);
        AccessibleRole = AccessibleRole.StaticText;
        Text = text ?? "";
        AccessibleName = Text;
        Margin = new Padding(0, 0, UI.S(6), UI.S(6));
        FitSize();
    }
    void FitSize()
    {
        Size sz = TextRenderer.MeasureText(Text, Font);
        Size = new Size(sz.Width + UI.S(16), Math.Max(UI.S(24), sz.Height + UI.S(6)));
    }
    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); AccessibleName = Text; FitSize(); Invalidate(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        Color bg = UI.IsDark ? Color.FromArgb(42, 44, 48) : Color.FromArgb(235, 238, 242);
        using (GraphicsPath p = UI.Round(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), UI.SF(4)))
        {
            using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
            using (Pen pen = new Pen(UI.Line)) g.DrawPath(pen, p);
        }
        TextRenderer.DrawText(g, Text, Font, ClientRectangle, UI.Text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

// ab v3.72: Aufgabenkachel auf der neuen Startseite
public class StartTaskCard : Control
{
    bool hover;
    string title = "", desc = "", footText = "", badgeText = "", badgeLevel = "ok", icon = "";
    bool recommended;
    public event EventHandler CardClicked;

    public StartTaskCard(string title, string desc, string footText, string badgeText, string badgeLevel, string icon, bool recommended)
    {
        this.title = title ?? "";
        this.desc = desc ?? "";
        this.footText = footText ?? "";
        this.badgeText = badgeText ?? "";
        this.badgeLevel = badgeLevel ?? "ok";
        this.icon = icon ?? "";
        this.recommended = recommended;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
        Cursor = Cursors.Hand;
        BackColor = UI.Panel;
        Size = new Size(UI.S(205), UI.S(215));
        Margin = new Padding(0, 0, UI.S(10), UI.S(14));
    }

    public string Title { get { return title; } set { title = value ?? ""; AccessibleName = title; Invalidate(); } }
    public string Description { get { return desc; } set { desc = value ?? ""; Invalidate(); } }
    public string FootText { get { return footText; } set { footText = value ?? ""; Invalidate(); } }
    public string BadgeText { get { return badgeText; } set { badgeText = value ?? ""; Invalidate(); } }
    public string BadgeLevel { get { return badgeLevel; } set { badgeLevel = value ?? "ok"; Invalidate(); } }
    public bool Recommended { get { return recommended; } set { recommended = value; Invalidate(); } }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && CardClicked != null) CardClicked(this, EventArgs.Empty);
        base.OnMouseClick(e);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) && CardClicked != null) { CardClicked(this, EventArgs.Empty); e.Handled = true; }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        Color border = hover || (recommended && !hover) ? UI.Accent : UI.Line;
        float bWidth = hover ? 1.5f : 1f;

        using (GraphicsPath p = UI.Round(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), UI.SF(8)))
        {
            using (SolidBrush b = new SolidBrush(UI.Panel)) g.FillPath(b, p);
            using (Pen pen = new Pen(border, bWidth)) g.DrawPath(pen, p);
        }

        // Icon Box
        int iconSize = UI.S(34);
        RectangleF iconBox = new RectangleF(UI.S(12), UI.S(12), iconSize, iconSize);
        Color iconBg = UI.IsDark ? Color.FromArgb(46, 50, 56) : Color.FromArgb(235, 240, 250);
        using (GraphicsPath ip = UI.Round(iconBox, UI.SF(6)))
        {
            using (SolidBrush ib = new SolidBrush(iconBg)) g.FillPath(ib, ip);
        }
        using (Font fIcon = UI.SymbolFont(12.5f))
        {
            TextRenderer.DrawText(g, icon, fIcon, new Rectangle((int)iconBox.X, (int)iconBox.Y, (int)iconBox.Width, (int)iconBox.Height), UI.Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // Empfohlen Badge
        if (recommended)
        {
            using (Font fRec = new Font("Segoe UI Semibold", 7.75f))
            {
                Size szR = TextRenderer.MeasureText("Empfohlen", fRec);
                int rW = szR.Width + UI.S(10);
                Rectangle rRec = new Rectangle(Width - rW - UI.S(12), UI.S(12), rW, UI.S(18));
                using (GraphicsPath rp = UI.Round(new RectangleF(rRec.X, rRec.Y, rRec.Width, rRec.Height), UI.SF(4)))
                using (SolidBrush rb = new SolidBrush(UI.InfoBg))
                {
                    g.FillPath(rb, rp);
                }
                TextRenderer.DrawText(g, "Empfohlen", fRec, rRec, UI.Info, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        // Titel (2-zeilig mit WordBreak, Semibold 9.5f)
        using (Font fTitle = new Font("Segoe UI Semibold", 9.5f))
        {
            Rectangle rTitle = new Rectangle(UI.S(12), UI.S(52), Width - UI.S(24), UI.S(34));
            TextRenderer.DrawText(g, title, fTitle, rTitle, UI.Text, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak);
        }

        // Beschreibung
        using (Font fDesc = new Font("Segoe UI", 8.25f))
        {
            Rectangle rDesc = new Rectangle(UI.S(12), UI.S(88), Width - UI.S(24), Height - UI.S(120));
            TextRenderer.DrawText(g, desc, fDesc, rDesc, UI.Muted, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak);
        }

        // Fußzeile mit dynamischer Breitenberechnung
        int footY = Height - UI.S(26);
        int badgeW = 0;
        if (badgeText.Length > 0)
        {
            Color bfg, bbg;
            UI.BadgeColors(badgeLevel, out bfg, out bbg);
            using (Font fBadge = new Font("Segoe UI Semibold", 7.75f))
            {
                Size szB = TextRenderer.MeasureText(badgeText, fBadge);
                badgeW = szB.Width + UI.S(10);
                Rectangle rBadge = new Rectangle(Width - badgeW - UI.S(10), footY, badgeW, UI.S(18));
                using (GraphicsPath bp = UI.Round(new RectangleF(rBadge.X, rBadge.Y, rBadge.Width, rBadge.Height), UI.SF(4)))
                using (SolidBrush bb = new SolidBrush(bbg))
                {
                    g.FillPath(bb, bp);
                }
                TextRenderer.DrawText(g, badgeText, fBadge, rBadge, bfg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        int availFootW = Width - UI.S(22) - badgeW;
        using (Font fFoot = new Font("Segoe UI", 7.5f))
        {
            Rectangle rFoot = new Rectangle(UI.S(10), footY, availFootW, UI.S(18));
            TextRenderer.DrawText(g, footText, fFoot, rFoot, UI.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}

// ab v3.72: Kompakte Aufgabenzeile für State B
public class StartTaskRow : Control
{
    bool hover;
    string title = "", subtitle = "", badgeText = "", badgeLevel = "ok", icon = "";
    public event EventHandler RowClicked;

    public StartTaskRow(string title, string subtitle, string badgeText, string badgeLevel, string icon)
    {
        this.title = title ?? "";
        this.subtitle = subtitle ?? "";
        this.badgeText = badgeText ?? "";
        this.badgeLevel = badgeLevel ?? "ok";
        this.icon = icon ?? "";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        Size = new Size(UI.S(350), UI.S(40));
        Margin = new Padding(0, 0, 0, UI.S(4));
    }

    public string Title { get { return title; } set { title = value ?? ""; AccessibleName = title; Invalidate(); } }
    public string Subtitle { get { return subtitle; } set { subtitle = value ?? ""; Invalidate(); } }
    public string BadgeText { get { return badgeText; } set { badgeText = value ?? ""; Invalidate(); } }
    public string BadgeLevel { get { return badgeLevel; } set { badgeLevel = value ?? "ok"; Invalidate(); } }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && RowClicked != null) RowClicked(this, EventArgs.Empty);
        base.OnMouseClick(e);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) && RowClicked != null) { RowClicked(this, EventArgs.Empty); e.Handled = true; }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        if (hover)
        {
            Color hbg = UI.IsDark ? Color.FromArgb(46, 48, 52) : Color.FromArgb(240, 242, 246);
            using (GraphicsPath p = UI.Round(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), UI.SF(6)))
            using (SolidBrush b = new SolidBrush(hbg))
            {
                g.FillPath(b, p);
            }
        }

        // Icon
        using (Font fIcon = UI.SymbolFont(12f))
        {
            TextRenderer.DrawText(g, icon, fIcon, new Rectangle(UI.S(8), 0, UI.S(24), Height), UI.Accent, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        // Titel & Subtitle
        int textX = UI.S(36);
        using (Font fTitle = new Font("Segoe UI Semibold", 9.25f))
        {
            Rectangle rTitle = new Rectangle(textX, UI.S(3), Width - textX - UI.S(75), UI.S(18));
            TextRenderer.DrawText(g, title, fTitle, rTitle, UI.Text, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
        }
        using (Font fSub = new Font("Segoe UI", 8f))
        {
            Rectangle rSub = new Rectangle(textX, UI.S(20), Width - textX - UI.S(75), UI.S(16));
            TextRenderer.DrawText(g, subtitle, fSub, rSub, UI.Muted, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
        }

        // Badge
        if (badgeText.Length > 0)
        {
            Color bfg, bbg;
            UI.BadgeColors(badgeLevel, out bfg, out bbg);
            using (Font fBadge = new Font("Segoe UI Semibold", 8f))
            {
                Size szB = TextRenderer.MeasureText(badgeText, fBadge);
                Rectangle rBadge = new Rectangle(Width - szB.Width - UI.S(14), (Height - UI.S(18)) / 2, szB.Width + UI.S(10), UI.S(18));
                using (GraphicsPath bp = UI.Round(new RectangleF(rBadge.X, rBadge.Y, rBadge.Width, rBadge.Height), UI.SF(4)))
                using (SolidBrush bb = new SolidBrush(bbg))
                {
                    g.FillPath(bb, bp);
                }
                TextRenderer.DrawText(g, badgeText, fBadge, rBadge, bfg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}

// ab v3.72: KPI-Kachel für Letzter Lauf auf der Startseite
public class KpiBox : Control
{
    string title = "", val = "", delta = "";
    public KpiBox(string title, string val, string delta = null)
    {
        this.title = title ?? "";
        this.val = val ?? "";
        this.delta = delta ?? "";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Size = new Size(UI.S(96), UI.S(64));
        Dock = DockStyle.Fill;
        Margin = new Padding(0, 0, UI.S(4), 0);
    }
    public KpiBox(string title, string val) : this(title, val, null) { }

    public string Title { get { return title; } set { title = value ?? ""; AccessibleName = title; Invalidate(); } }
    public string Value { get { return val; } set { val = value ?? ""; Invalidate(); } }
    public string Delta { get { return delta; } set { delta = value ?? ""; Invalidate(); } }

    public override Size GetPreferredSize(Size proposedSize)
    {
        return new Size(UI.S(96), UI.S(64));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        Color bg = UI.IsDark ? Color.FromArgb(32, 36, 42) : Color.FromArgb(242, 244, 248);
        using (GraphicsPath p = UI.Round(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), UI.SF(6)))
        {
            using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
            using (Pen pen = new Pen(UI.Line)) g.DrawPath(pen, p);
        }

        using (Font fTitle = new Font("Segoe UI", 7.5f))
        {
            Rectangle rTitle = new Rectangle(UI.S(6), UI.S(6), Width - UI.S(12), UI.S(16));
            TextRenderer.DrawText(g, title, fTitle, rTitle, UI.Muted, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        using (Font fVal = new Font("Segoe UI Semibold", 10.75f))
        {
            int hVal = String.IsNullOrEmpty(delta) ? (Height - UI.S(27)) : UI.S(20);
            Rectangle rVal = new Rectangle(UI.S(6), UI.S(21), Width - UI.S(12), hVal);
            TextRenderer.DrawText(g, val, fVal, rVal, UI.Text, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        if (!String.IsNullOrEmpty(delta))
        {
            Color dCol = delta.IndexOf("\u2191") >= 0 ? UI.Ok : (delta.IndexOf("\u2193") >= 0 ? UI.Crit : UI.Muted);
            using (Font fDelta = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            {
                Rectangle rDelta = new Rectangle(UI.S(6), Height - UI.S(18), Width - UI.S(12), UI.S(14));
                TextRenderer.DrawText(g, delta, fDelta, rDelta, dCol, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            }
        }
    }
}

// ab v3.64: Kopfzeile einer Aufklappgruppe, ganze Zeile klickbar, Pfeilsymbol und Zähler "2 von 4 gewählt"
public class GroupHeader : Control
{
    string title = "", count = "";
    bool expanded, hover;
    public event EventHandler Toggled;
    public GroupHeader(string title)
    {
        this.title = title ?? "";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        TabStop = true; Cursor = Cursors.Hand; BackColor = UI.Panel;
        Size = new Size(UI.S(UI.Gruppenbreite), UI.S(32));
        Margin = new Padding(0, UI.S(UI.Abstand2), 0, 0);
    }
    public string Title { get { return title; } set { title = value ?? ""; AccessibleName = title; Invalidate(); } }
    public string CountText { get { return count; } set { if (count == (value ?? "")) return; count = value ?? ""; Invalidate(); } }
    public bool Expanded { get { return expanded; } set { if (expanded == value) return; expanded = value; Invalidate(); } }
    void Flip() { Expanded = !Expanded; if (Toggled != null) Toggled(this, EventArgs.Empty); }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnMouseClick(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { Focus(); Flip(); } base.OnMouseClick(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) { Flip(); e.Handled = true; } base.OnKeyDown(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        Color bg = hover ? (UI.IsDark ? Color.FromArgb(44, 46, 48) : Color.FromArgb(243, 245, 248)) : UI.Panel;
        using (GraphicsPath p = UI.Round(new RectangleF(0, 0, Width - 1, Height - 1), UI.SF(5)))
        using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
        using (Pen line = new Pen(UI.Line)) g.DrawLine(line, 0, Height - 1, Width, Height - 1);
        int x = UI.S(6);
        using (Font fi = UI.SymbolFont(8.5f))
            TextRenderer.DrawText(g, expanded ? UI.IcoAuf : UI.IcoZu, fi, new Rectangle(x, 0, UI.S(16), Height), UI.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        x += UI.S(24);
        using (Font ft = new Font("Segoe UI Semibold", UI.SchriftGruppe + 0.5f))
        {
            Size ts = TextRenderer.MeasureText(g, title, ft);
            TextRenderer.DrawText(g, title, ft, new Rectangle(x, 0, Math.Max(10, Width - x - UI.S(8)), Height), UI.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            x += ts.Width + UI.S(10);
        }
        if (count.Length > 0)
            using (Font fc = new Font("Segoe UI", UI.SchriftHinweis))
                TextRenderer.DrawText(g, count, fc, new Rectangle(x, 0, Math.Max(10, Width - x - UI.S(8)), Height), UI.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if (Focused)
            using (Pen fp = new Pen(UI.Accent, 1f)) { fp.DashStyle = DashStyle.Dot; g.DrawRectangle(fp, 1, 1, Width - 3, Height - 3); }
    }
}

// Moderner Windows 11 DropDown / ComboBox (Dark & Light Mode fähig)
public class DarkComboBox : ComboBox
{
    public DarkComboBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DrawMode = DrawMode.OwnerDrawFixed;
        DropDownStyle = ComboBoxStyle.DropDownList;
        ItemHeight = UI.S(24);
        Font = new Font("Segoe UI", 9.5f);
        Cursor = Cursors.Hand;
        BackColor = UI.Panel;
        ForeColor = UI.Text;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        Color bg = Enabled ? UI.Panel : (UI.IsDark ? Color.FromArgb(30, 31, 32) : Color.FromArgb(243, 244, 246));
        Color border = UI.Line;
        Color fg = Enabled ? UI.Text : (UI.IsDark ? Color.FromArgb(140, 145, 155) : Color.FromArgb(160, 163, 168));
        using (SolidBrush b = new SolidBrush(bg)) g.FillRectangle(b, ClientRectangle);
        using (Pen p = new Pen(border)) g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);

        string txt = SelectedItem != null ? SelectedItem.ToString() : Text;
        int arrowW = UI.S(22);
        Rectangle tr = new Rectangle(UI.S(8), 0, Math.Max(10, Width - arrowW - UI.S(10)), Height);
        TextRenderer.DrawText(g, txt, Font, tr, fg, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        int arrowX = Width - arrowW / 2 - UI.S(2);
        int arrowY = Height / 2;
        int asz = UI.S(4);
        Point[] arrow = new Point[] {
            new Point(arrowX - asz, arrowY - asz / 2),
            new Point(arrowX + asz, arrowY - asz / 2),
            new Point(arrowX, arrowY + asz / 2)
        };
        using (SolidBrush ab = new SolidBrush(UI.Muted)) g.FillPolygon(ab, arrow);
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color bg = sel ? UI.AccentSoft : UI.Panel;
        Color fg = sel ? UI.Accent : UI.Text;
        using (SolidBrush b = new SolidBrush(bg)) e.Graphics.FillRectangle(b, e.Bounds);
        string itemText = Items[e.Index] != null ? Items[e.Index].ToString() : "";
        TextRenderer.DrawText(e.Graphics, itemText, Font,
            new Rectangle(e.Bounds.X + UI.S(6), e.Bounds.Y, e.Bounds.Width - UI.S(10), e.Bounds.Height),
            fg, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

// Moderner Windows 11 Toggle-Switch (Wintoys-Look, ab v3.9 mit UI.S(38) x UI.S(20))
public class ToggleSwitch : Control
{
    bool isChecked;
    bool hover;
    public event EventHandler CheckedChanged;

    public ToggleSwitch()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable, true);
        TabStop = true;
        Cursor = Cursors.Hand;
        AccessibleRole = AccessibleRole.CheckButton;
        AccessibleName = "Schalter";
        Size = new Size(UI.S(38), UI.S(20));
        BackColor = Color.Transparent;
    }

    public bool Checked
    {
        get { return isChecked; }
        set
        {
            if (isChecked != value)
            {
                isChecked = value;
                Invalidate();
                if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            }
        }
    }

    // ab v4.05: gesperrt (Optimierung: Eintrag ist auf diesem PC schon so eingestellt). Der Schalter zeigt seinen Stand
    // in Grün, reagiert nicht auf Klick und Leertaste und wird nicht ausgegraut wie ein deaktiviertes Element.
    bool locked;
    public bool Locked
    {
        get { return locked; }
        set { if (locked != value) { locked = value; Cursor = value ? Cursors.Default : Cursors.Hand; AccessibleDescription = value ? "gesperrt" : null; Invalidate(); } }
    }

    // Umschalten durch den Nutzer (Klick, Leertaste, Klick auf die Zeile); gesperrt ohne Wirkung
    public void UserToggle() { if (!locked) Checked = !Checked; }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Focus();
            UserToggle();
        }
        base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            UserToggle();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int trackW = UI.S(38);
        int trackH = UI.S(20);
        int trackX = (Width - trackW) / 2;
        int trackY = (Height - trackH) / 2;
        RectangleF trackRect = new RectangleF(trackX, trackY, trackW, trackH);
        float rad = trackH / 2f;

        int knobD = UI.S(14);
        int knobY = trackY + (trackH - knobD) / 2;
        int knobX = isChecked ? (trackX + trackW - knobD - UI.S(3)) : (trackX + UI.S(3));

        using (GraphicsPath trackPath = UI.Round(trackRect, rad))
        {
            if (isChecked)
            {
                Color fillColor = locked ? UI.Ok : hover ? UI.AccentHover : UI.Accent;
                using (SolidBrush b = new SolidBrush(fillColor)) g.FillPath(b, trackPath);
                using (Pen p = new Pen(fillColor, 1f)) g.DrawPath(p, trackPath);
            }
            else
            {
                Color borderColor = hover ? (UI.IsDark ? Color.FromArgb(140, 140, 140) : Color.FromArgb(118, 118, 118)) : (UI.IsDark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(209, 213, 219));
                Color trackBg = UI.IsDark ? Color.FromArgb(50, 54, 62) : Color.FromArgb(226, 230, 236);
                using (SolidBrush b = new SolidBrush(trackBg)) g.FillPath(b, trackPath);
                using (Pen p = new Pen(borderColor, UI.SF(1.5f))) g.DrawPath(p, trackPath);
            }
        }

        // Subtiler Schatten des Knopfs
        RectangleF shadowRect = new RectangleF(knobX, knobY + UI.SF(0.5f), knobD, knobD);
        using (SolidBrush sb = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
        {
            g.FillEllipse(sb, shadowRect);
        }

        // Knopffarbe
        RectangleF knobRect = new RectangleF(knobX, knobY, knobD, knobD);
        Color knobColor = isChecked ? Color.White : (UI.IsDark ? Color.FromArgb(215, 220, 226) : Color.White);
        using (SolidBrush kb = new SolidBrush(knobColor))
        {
            g.FillEllipse(kb, knobRect);
        }
        if (!isChecked)
        {
            Color kpCol = UI.IsDark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(118, 118, 118);
            using (Pen kp = new Pen(kpCol, 1f))
            {
                g.DrawEllipse(kp, knobRect);
            }
        }

        // Barrierefreies Fokus-Rechteck
        if (Focused)
        {
            Rectangle fRect = new Rectangle(trackX - UI.S(2), trackY - UI.S(2), trackW + UI.S(4), trackH + UI.S(4));
            using (Pen fp = new Pen(UI.Accent, 1f))
            {
                fp.DashStyle = DashStyle.Dot;
                g.DrawRectangle(fp, fRect);
            }
        }
    }
}

// Moderner Pillen-Knopf (ab v3.9: Presets, Schnellwahl und Filter)
public class PillButton : Control
{
    string text = "";
    string icon = "";
    bool active = false;
    bool hover = false;
    bool removable = false;
    bool closeHover = false;
    public event EventHandler RemoveClicked;
    public event EventHandler Picked;

    public PillButton(string text, bool active)
    {
        this.text = text ?? "";
        this.active = active;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        Font = new Font("Segoe UI Semibold", 9f);
        AccessibleRole = AccessibleRole.PushButton;
        AccessibleName = text;
        Height = UI.S(28);
        Margin = new Padding(0, 0, UI.S(6), 0);
        FitSize();
    }
    public PillButton(string text) : this(text, false) { }

    public bool Active { get { return active; } set { if (active != value) { active = value; Invalidate(); } } }
    public bool Removable { get { return removable; } set { if (removable != value) { removable = value; FitSize(); Invalidate(); } } }
    public string Icon { get { return icon; } set { icon = value ?? ""; FitSize(); Invalidate(); } }
    public override string Text { get { return text; } set { text = value ?? ""; AccessibleName = text; FitSize(); Invalidate(); } }

    void FitSize()
    {
        Size sz = TextRenderer.MeasureText(text, Font);
        int w = sz.Width + UI.S(22);
        if (!String.IsNullOrEmpty(icon)) w += UI.S(16);
        if (removable) w += UI.S(18);
        Size = new Size(Math.Max(UI.S(54), w), UI.S(28));
    }

    Rectangle CloseRect
    {
        get
        {
            int sz = UI.S(14);
            return new Rectangle(Width - sz - UI.S(6), (Height - sz) / 2, sz, sz);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (removable)
        {
            bool ch = CloseRect.Contains(e.Location);
            if (ch != closeHover) { closeHover = ch; Invalidate(); }
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; closeHover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Focus();
            if (removable && CloseRect.Contains(e.Location))
            {
                if (RemoveClicked != null) RemoveClicked(this, EventArgs.Empty);
            }
            else
            {
                if (Picked != null) Picked(this, EventArgs.Empty);
            }
        }
        base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            if (Picked != null) Picked(this, EventArgs.Empty);
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
        float rad = (Height - 1.5f) / 2f;

        Color bg = active ? (hover ? UI.AccentHover : UI.Accent) : (hover ? (UI.IsDark ? Color.FromArgb(48, 52, 58) : Color.FromArgb(235, 238, 243)) : (UI.IsDark ? Color.FromArgb(36, 38, 42) : Color.FromArgb(244, 246, 250)));
        Color border = active ? UI.Accent : (hover ? UI.Accent : UI.Line);
        Color fg = active ? UI.OnAccent : UI.Text;

        using (GraphicsPath p = UI.Round(r, rad))
        {
            using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
            using (Pen pen = new Pen(border, 1f)) g.DrawPath(pen, p);
        }

        int x = UI.S(10);
        if (!String.IsNullOrEmpty(icon))
        {
            int icoSize = UI.S(14);
            using (Font ifont = UI.SymbolFont(9f))
            {
                Rectangle icoR = new Rectangle(x, (Height - icoSize) / 2, icoSize, icoSize);
                TextRenderer.DrawText(g, icon, ifont, icoR, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            x += icoSize + UI.S(5);
        }

        int textW = Width - x - (removable ? UI.S(22) : UI.S(10));
        TextRenderer.DrawText(g, text, Font, new Rectangle(x, 0, textW, Height), fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        if (removable)
        {
            Rectangle cr = CloseRect;
            if (closeHover)
            {
                using (GraphicsPath cp = UI.Round(new RectangleF(cr.X, cr.Y, cr.Width - 1, cr.Height - 1), (cr.Height - 1) / 2f))
                using (SolidBrush cb = new SolidBrush(active ? Color.FromArgb(50, 0, 0, 0) : (UI.IsDark ? Color.FromArgb(70, 75, 80) : Color.FromArgb(210, 215, 220))))
                    g.FillPath(cb, cp);
            }
            using (Font xf = new Font("Segoe UI", 8f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, "×", xf, cr, active ? UI.OnAccent : UI.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }
}

// Kategorien-Wähler für das Master-Detail-Layout (ab v3.9)
public class CategoryPickerItem : Control
{
    string catKey = "";
    string title = "";
    string shortTitle = "";
    string subtitle = "";
    string icon = "";
    int checkedCount = 0;
    int totalCount = 0;
    bool selected = false;
    bool hover = false;
    public event EventHandler Picked;

    public CategoryPickerItem(string title, string icon) : this(title, icon, title) { }

    public CategoryPickerItem(string title, string icon, string shortTitle)
    {
        this.catKey = title ?? "";
        this.title = title ?? "";
        this.icon = icon ?? "";
        this.shortTitle = !string.IsNullOrEmpty(shortTitle) ? shortTitle : this.title;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        Cursor = Cursors.Hand;
        Size = new Size(UI.S(218), UI.S(40));
        Margin = new Padding(0, 0, 0, UI.S(4));
        BackColor = UI.Panel;
        AccessibleRole = AccessibleRole.ListItem;
        AccessibleName = this.shortTitle;
    }

    public CategoryPickerItem(string key, string title, string icon, string shortTitle) : this(title, icon, shortTitle)
    {
        this.catKey = key ?? "";
    }

    public string CategoryKey { get { return catKey; } set { catKey = value ?? ""; } }
    public string CategoryTitle { get { return title; } set { title = value ?? ""; AccessibleName = !String.IsNullOrEmpty(shortTitle) ? shortTitle : title; Invalidate(); } }
    public string ShortTitle { get { return !String.IsNullOrEmpty(shortTitle) ? shortTitle : title; } set { shortTitle = value ?? ""; AccessibleName = !String.IsNullOrEmpty(shortTitle) ? shortTitle : title; Invalidate(); } }
    public string Subtitle { get { return subtitle; } set { subtitle = value ?? ""; Invalidate(); } }
    public string Icon { get { return icon; } set { icon = value ?? ""; Invalidate(); } }
    public int CheckedCount { get { return checkedCount; } set { if (checkedCount != value) { checkedCount = value; Invalidate(); } } }
    public int TotalCount { get { return totalCount; } set { if (totalCount != value) { totalCount = value; Invalidate(); } } }
    public bool Selected { get { return selected; } set { if (selected != value) { selected = value; Invalidate(); } } }
    // ab v4.01: ohne Zähler, etwa für die Bereiche des Einstellungsfensters
    public bool ShowCount = true;

    public void SetCounts(int chk, int total)
    {
        checkedCount = chk;
        totalCount = total;
        Invalidate();
    }

    // ab v4.05: bereits aktive Einträge (Optimierung) als grüne Pille neben dem Zähler, 0 = keine Pille
    int activeCount = 0;
    public int ActiveCount { get { return activeCount; } set { if (activeCount != value) { activeCount = value; Invalidate(); } } }
    public void SetCounts(int chk, int total, int active)
    {
        checkedCount = chk; totalCount = total; activeCount = active;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Focus();
            if (Picked != null) Picked(this, EventArgs.Empty);
        }
        base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            if (Picked != null) Picked(this, EventArgs.Empty);
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        RectangleF r = new RectangleF(1, 1, Width - 3, Height - 3);
        Color fill = selected ? UI.AccentSoft : (hover ? (UI.IsDark ? Color.FromArgb(44, 46, 50) : Color.FromArgb(242, 245, 249)) : UI.Panel);
        Color borderPen = selected ? UI.Accent : (hover ? UI.Line : UI.Panel);

        using (GraphicsPath p = UI.Round(r, UI.SF(6)))
        {
            using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, p);
            using (Pen pen = new Pen(borderPen, 1f)) g.DrawPath(pen, p);
        }

        if (selected)
        {
            int barH = Height - UI.S(14);
            RectangleF barR = new RectangleF(UI.S(3), (Height - barH) / 2f, UI.SF(3.5f), barH);
            using (GraphicsPath bp = UI.Round(barR, UI.SF(1.5f)))
            using (SolidBrush bb = new SolidBrush(UI.Accent))
                g.FillPath(bb, bp);
        }

        int x = UI.S(12);
        if (!String.IsNullOrEmpty(icon))
        {
            int icoSize = UI.S(20);
            int icoY = (Height - icoSize) / 2;
            using (Font ifont = UI.SymbolFont(11f))
            {
                Rectangle icoR = new Rectangle(x, icoY, icoSize, icoSize);
                TextRenderer.DrawText(g, icon, ifont, icoR, selected ? UI.Accent : UI.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            x += icoSize + UI.S(8);
        }

        string badgeText = String.Format("{0}/{1}", checkedCount, totalCount);
        int badgeW = 0;
        if (ShowCount)
        using (Font fb = new Font("Segoe UI Semibold", 8f))
        {
            Size bsz = TextRenderer.MeasureText(g, badgeText, fb, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            badgeW = bsz.Width + UI.S(12);
            int badgeH = Math.Max(UI.S(18), bsz.Height + UI.S(4));
            int badgeX = Width - badgeW - UI.S(8);
            int badgeY = (Height - badgeH) / 2;
            Rectangle br = new Rectangle(badgeX, badgeY, badgeW, badgeH);

            Color bbg = checkedCount > 0 ? (selected ? UI.Accent : (UI.IsDark ? Color.FromArgb(20, 60, 55) : Color.FromArgb(220, 245, 240))) : (UI.IsDark ? Color.FromArgb(42, 45, 50) : Color.FromArgb(230, 233, 238));
            Color bfg = checkedCount > 0 ? (selected ? UI.OnAccent : (UI.IsDark ? Color.FromArgb(0, 210, 180) : Color.FromArgb(0, 128, 110))) : UI.Muted;

            using (GraphicsPath bp = UI.Round(new RectangleF(br.X, br.Y, br.Width - 1, br.Height - 1), (badgeH - 1) / 2f))
            using (SolidBrush sb = new SolidBrush(bbg))
                g.FillPath(sb, bp);

            TextRenderer.DrawText(g, badgeText, fb, br, bfg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            if (activeCount > 0)
            {
                // grüne Pille "5 ✓" links neben dem Zähler
                string at = activeCount.ToString();
                using (Font fi = UI.SymbolFont(7f))
                {
                    Size asz = TextRenderer.MeasureText(g, at, fb, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                    int icoW = UI.S(10);
                    int aw = asz.Width + icoW + UI.S(14);
                    Rectangle ar = new Rectangle(badgeX - aw - UI.S(4), badgeY, aw, badgeH);
                    Color abg = UI.IsDark ? Color.FromArgb(28, 58, 36) : Color.FromArgb(221, 244, 228);
                    Color afg = UI.IsDark ? Color.FromArgb(120, 214, 140) : Color.FromArgb(16, 112, 58);
                    using (GraphicsPath ap = UI.Round(new RectangleF(ar.X, ar.Y, ar.Width - 1, ar.Height - 1), (badgeH - 1) / 2f))
                    using (SolidBrush asb = new SolidBrush(abg))
                        g.FillPath(asb, ap);
                    TextRenderer.DrawText(g, at, fb, new Rectangle(ar.X + UI.S(6), ar.Y, asz.Width + UI.S(2), ar.Height), afg, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, UI.IcoCheck, fi, new Rectangle(ar.X + UI.S(7) + asz.Width, ar.Y, icoW + UI.S(2), ar.Height), afg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    badgeW += aw + UI.S(4);
                }
            }
        }

        int textW = Width - x - badgeW - UI.S(14);
        string displayTitle = !String.IsNullOrEmpty(shortTitle) ? shortTitle : title;
        using (Font ft = new Font("Segoe UI Semibold", 9f))
        {
            Color titleColor = selected ? (UI.IsDark ? UI.Accent : UI.AccentDark) : UI.Text;
            Size tsz = TextRenderer.MeasureText(g, displayTitle, ft, new Size(Math.Max(10, textW), int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            int ty = Math.Max(0, (Height - tsz.Height) / 2);
            TextRenderer.DrawText(g, displayTitle, ft, new Rectangle(x, ty, Math.Max(10, textW), tsz.Height), titleColor, TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}

// ab v4.01: Balken für Prozent der Referenz; der Strich markiert 100 %, über 100 % läuft der Balken bis zum Rand
public class PercentBar : Control
{
    double pct;
    public double Percent { get { return pct; } set { pct = value; Invalidate(); } }
    // ab v4.07: gezeichneter Teil des Werts (0 bis 1), nur für das Einblenden auf der Ergebnisseite
    double anteil = 1;
    public double Anteil { get { return anteil; } set { anteil = value < 0 ? 0 : value > 1 ? 1 : value; Invalidate(); } }
    public double Max = 150;
    public PercentBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        AccessibleRole = AccessibleRole.ProgressBar;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        float h = Height - 1, w = Width - 1;
        Color track = UI.IsDark ? Color.FromArgb(52, 56, 62) : Color.FromArgb(226, 229, 234);
        using (GraphicsPath p = UI.Round(new RectangleF(0, 0, w, h), h / 2f)) using (SolidBrush b = new SolidBrush(track)) g.FillPath(b, p);
        float fw = (float)(Math.Max(0, Math.Min(pct, Max)) / Max * w * anteil);
        // Langsamer als die Referenz ist kein Fehler (die Referenz ist meist ein schneller PC): kein Gelb, nur Grün ab Referenzniveau
        Color fill = pct >= 90 ? UI.Ok : UI.Accent;
        if (fw >= h) using (GraphicsPath p = UI.Round(new RectangleF(0, 0, fw, h), h / 2f)) using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, p);
        float mx = (float)(100.0 / Max * w);
        using (Pen pen = new Pen(UI.Text, 1.5f)) g.DrawLine(pen, mx, -1, mx, h + 1);
        AccessibleName = pct.ToString("0") + " Prozent der Referenz";
    }
}

// Einstellungs-Zeile in der Master-Detail-Ansicht (ab v3.9)
public class SettingRow : Control
{
    string title = "";
    string desc = "";
    ToggleSwitch toggle;
    List<Badge> badges = new List<Badge>();
    bool hover;
    public event EventHandler CheckedChanged;

    public SettingRow(string title, string desc) : this(title, desc, null) { }

    public SettingRow(string title, string desc, ToggleSwitch toggle)
    {
        this.title = title ?? "";
        this.desc = desc ?? "";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        Cursor = Cursors.Hand;
        BackColor = UI.Panel;
        AccessibleRole = AccessibleRole.ListItem;
        AccessibleName = this.title;
        Margin = new Padding(0, 0, 0, UI.S(4));

        this.toggle = toggle ?? new ToggleSwitch();
        this.toggle.CheckedChanged += delegate {
            if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            Invalidate();
        };
        Controls.Add(this.toggle);
        RecalcHeight();
    }

    public string Title { get { return title; } set { title = value ?? ""; AccessibleName = title; Invalidate(); } }
    public string Description { get { return desc; } set { if (desc == (value ?? "")) return; desc = value ?? ""; RecalcHeight(); Invalidate(); } }
    public ToggleSwitch Toggle { get { return toggle; } }
    // ab v4.08: Symbol links vor dem Titel (Seiten Benchmark und Diagnose); leer = Zeile ohne Symbol wie bisher
    string icon = "";
    public string Icon { get { return icon; } set { icon = value ?? ""; RecalcHeight(); Invalidate(); } }
    int TextLinks { get { return icon.Length > 0 ? UI.S(44) : UI.S(12); } }
    public bool Checked
    {
        get { return toggle != null && toggle.Checked; }
        set { if (toggle != null) toggle.Checked = value; }
    }

    public void AddBadge(Badge b)
    {
        if (b == null) return;
        badges.Add(b);
        b.SizeChanged += delegate { LayoutControls(); RecalcHeight(); };
        b.VisibleChanged += delegate { LayoutControls(); RecalcHeight(); };
        b.TextChanged += delegate { LayoutControls(); RecalcHeight(); };
        Controls.Add(b);
        LayoutControls();
        RecalcHeight();
    }

    public void ClearBadges()
    {
        foreach (Badge b in badges) Controls.Remove(b);
        badges.Clear();
        LayoutControls();
        RecalcHeight();
    }

    // Spaltenordnung von rechts nach links (Regel 3.1): [ Zustand ] -> [ Risiko ] -> [ Neustart ] -> [ ToggleSwitch ]
    static int BadgeOrder(Badge b)
    {
        if (b == null) return 0;
        string t = (b.Text ?? "").ToLowerInvariant();
        if (t.EndsWith(" min.") || t.Contains(" std.")) return 5;   // ab v4.08: Dauer direkt neben dem Schalter
        if (t.Contains("neustart")) return 4;
        if (t == "eingriff" || t == "zerstrend" || t == "zerstoerend" || t == "ndern" || t == "aendern" || t == "lesen") return 3;
        if (t == "empfohlen" || t == "standard") return 2;
        return 1; // Zustand z. B. "gesetzt", "aktiv"
    }

    // ab v4.01: Schriften einmal je Programmlauf statt bei jeder Messung und jedem Zeichnen neu (die Optimierung hat
    // über 150 Zeilen); gemessen wird ohne CreateGraphics, das sonst für jede Zeile vorzeitig ein Fensterhandle anlegt
    static Font fontTitel, fontDetail;
    static Font FontTitel { get { if (fontTitel == null) fontTitel = new Font("Segoe UI Semibold", 9.5f); return fontTitel; } }
    static Font FontDetail { get { if (fontDetail == null) fontDetail = new Font("Segoe UI", 8.25f); return fontDetail; } }

    bool inRecalc = false;
    void RecalcHeight()
    {
        if (inRecalc) return;
        inRecalc = true;
        try
        {
            if (Width <= 0) return;
            int curX = TextLinks;
            int rightBound = Width - UI.S(16);
            if (toggle != null) rightBound = Math.Min(rightBound, toggle.Left > 0 ? toggle.Left : (Width - UI.S(54)));
            for (int i = 0; i < badges.Count; i++)
            {
                if (badges[i] != null && badges[i].Visible && !string.IsNullOrEmpty(badges[i].Text) && badges[i].Left > 0 && badges[i].Left < Width)
                    rightBound = Math.Min(rightBound, badges[i].Left);
            }
            rightBound -= UI.S(10);
            int textW = Math.Max(UI.S(100), rightBound - curX);

            {
                Font ft = FontTitel, fd = FontDetail;
                Size tsz = TextRenderer.MeasureText(title ?? "", ft, new Size(textW, int.MaxValue), TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                int titleH = Math.Max(tsz.Height, UI.S(18));
                int targetH;
                if (string.IsNullOrEmpty(desc))
                {
                    targetH = Math.Max(UI.S(42), titleH + UI.S(14));
                }
                else
                {
                    Size dsz = TextRenderer.MeasureText(desc, fd, new Size(textW, int.MaxValue), TextFormatFlags.WordBreak);
                    int maxDescH = (int)Math.Ceiling(fd.Height * 2.2f) + UI.S(2);
                    int descH = Math.Min(dsz.Height, maxDescH);
                    targetH = Math.Max(UI.S(42), UI.S(8) + titleH + UI.S(2) + descH + UI.S(8));
                }
                if (Height != targetH) Height = targetH;
            }
            LayoutControls();
        }
        finally { inRecalc = false; }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RecalcHeight();
    }

    void LayoutControls()
    {
        int curRight = Width - UI.S(16);
        if (toggle != null)
        {
            toggle.Location = new Point(curRight - toggle.Width, (Height - toggle.Height) / 2);
            curRight -= toggle.Width + UI.S(10);
        }
        List<Badge> visibleBadges = new List<Badge>();
        for (int i = 0; i < badges.Count; i++)
        {
            Badge b = badges[i];
            if (b != null && b.Visible && !string.IsNullOrEmpty(b.Text))
            {
                visibleBadges.Add(b);
            }
            else if (b != null)
            {
                b.Location = new Point(Width + 100, 0);
            }
        }
        visibleBadges.Sort(delegate(Badge a, Badge b) {
            return BadgeOrder(b).CompareTo(BadgeOrder(a));
        });
        for (int i = 0; i < visibleBadges.Count; i++)
        {
            Badge b = visibleBadges[i];
            b.Location = new Point(curRight - b.Width, (Height - b.Height) / 2);
            curRight -= b.Width + UI.S(8);
        }
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Focus();
            if (toggle != null) toggle.UserToggle();
        }
        base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            if (toggle != null) toggle.UserToggle();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        RectangleF r = new RectangleF(1, 1, Width - 3, Height - 3);
        Color bg = hover ? (UI.IsDark ? Color.FromArgb(42, 44, 48) : Color.FromArgb(246, 248, 252)) : UI.Panel;
        Color border = hover ? (UI.IsDark ? Color.FromArgb(70, 74, 80) : Color.FromArgb(215, 219, 226)) : UI.Line;

        using (GraphicsPath p = UI.Round(r, UI.SF(6)))
        {
            using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
            using (Pen pen = new Pen(border, 1f)) g.DrawPath(pen, p);
        }

        int curX = TextLinks;
        if (icon.Length > 0)
            using (Font fi = UI.SymbolFont(13f))
                TextRenderer.DrawText(g, icon, fi, new Rectangle(UI.S(10), 0, UI.S(28), Height), UI.Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping);
        int rightBound = Width - UI.S(16);
        if (toggle != null) rightBound = Math.Min(rightBound, toggle.Left);
        for (int i = 0; i < badges.Count; i++)
        {
            if (badges[i] != null && badges[i].Visible && !string.IsNullOrEmpty(badges[i].Text))
                rightBound = Math.Min(rightBound, badges[i].Left);
        }
        rightBound -= UI.S(10);
        int textW = Math.Max(UI.S(100), rightBound - curX);

        {
            Font ft = FontTitel;
            Size tsz = TextRenderer.MeasureText(g, title ?? "", ft, new Size(textW, int.MaxValue), TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            int titleH = Math.Max(tsz.Height, UI.S(18));
            int startY = string.IsNullOrEmpty(desc) ? (Height - titleH) / 2 : UI.S(7);
            TextRenderer.DrawText(g, title ?? "", ft, new Rectangle(curX, startY, textW, titleH), UI.Text, TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);

            if (!string.IsNullOrEmpty(desc))
            {
                {
                    Font fd = FontDetail;
                    int descY = startY + titleH + UI.S(2);
                    int descAvailH = Math.Max(UI.S(12), Height - descY - UI.S(6));
                    TextRenderer.DrawText(g, desc, fd, new Rectangle(curX, descY, textW, descAvailH), UI.Muted, TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
                }
            }
        }

        if (Focused)
        {
            using (Pen fp = new Pen(UI.Accent, 1f))
            {
                fp.DashStyle = DashStyle.Dot;
                g.DrawRectangle(fp, 2, 2, Width - 5, Height - 5);
            }
        }
    }
}


// ab v4.08: kompakte Zeile für einen Datenträger (Seite Benchmark): Haken, Name, Etikett für die Anbindung und rechts der
// freie Platz. Ersetzt das Listenfeld mit Kontrollkästchen, das als einziges Element der Seite einen hellen Rahmen trug.
public class LaufwerkZeile : Control
{
    string name = "", bus = "", rechts = "";
    bool isChecked, hover;
    public event EventHandler CheckedChanged;

    public LaufwerkZeile(string name, string bus, string rechts, bool on)
    {
        this.name = name ?? ""; this.bus = bus ?? ""; this.rechts = rechts ?? ""; isChecked = on;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        TabStop = true; Cursor = Cursors.Hand; BackColor = UI.Panel;
        AccessibleRole = AccessibleRole.CheckButton; AccessibleName = this.name;
        Size = new Size(UI.S(400), UI.S(30)); Margin = new Padding(0, 0, 0, UI.S(4));
    }
    public string Laufwerk { get { return name; } }
    public string Anbindung { get { return bus; } }
    public string Rechts { get { return rechts; } set { rechts = value ?? ""; Invalidate(); } }
    public bool Checked
    {
        get { return isChecked; }
        set { if (isChecked == value) return; isChecked = value; Invalidate(); if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty); }
    }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnMouseClick(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { Focus(); Checked = !Checked; } base.OnMouseClick(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space) { Checked = !Checked; e.Handled = true; } base.OnKeyDown(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        using (SolidBrush ob = new SolidBrush(UI.Panel)) g.FillRectangle(ob, ClientRectangle);
        Color bg = hover ? (UI.IsDark ? Color.FromArgb(42, 44, 48) : Color.FromArgb(246, 248, 252)) : (UI.IsDark ? Color.FromArgb(30, 31, 33) : Color.FromArgb(250, 251, 252));
        using (GraphicsPath p = UI.Round(new RectangleF(0.5f, 0.5f, Width - 2, Height - 2), UI.SF(6)))
        {
            using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
            using (Pen pen = new Pen(UI.Line, 1f)) g.DrawPath(pen, p);
        }
        int bs = UI.S(16);
        Rectangle bx = new Rectangle(UI.S(10), (Height - bs) / 2, bs, bs);
        using (GraphicsPath bp = UI.Round(bx, UI.SF(4)))
        {
            using (SolidBrush bb = new SolidBrush(isChecked ? UI.Accent : UI.Panel)) g.FillPath(bb, bp);
            using (Pen bpen = new Pen(isChecked ? UI.Accent : (UI.IsDark ? Color.FromArgb(90, 95, 105) : Color.FromArgb(160, 170, 185)), UI.SF(1.5f))) g.DrawPath(bpen, bp);
        }
        if (isChecked) using (Pen ck = new Pen(UI.OnAccent, UI.SF(2f))) { ck.StartCap = LineCap.Round; ck.EndCap = LineCap.Round; g.DrawLines(ck, new PointF[] { new PointF(bx.X + bs * 0.24f, bx.Y + bs * 0.52f), new PointF(bx.X + bs * 0.44f, bx.Y + bs * 0.72f), new PointF(bx.X + bs * 0.78f, bx.Y + bs * 0.30f) }); }
        int x = bx.Right + UI.S(9), right = Width - UI.S(10);
        using (Font fr = new Font("Segoe UI", 8.75f))
        {
            if (rechts.Length > 0)
            {
                Size rs = TextRenderer.MeasureText(g, rechts, fr, new Size(int.MaxValue, Height), TextFormatFlags.NoPadding);
                int rw = Math.Min(rs.Width, Math.Max(0, Width / 2));
                TextRenderer.DrawText(g, rechts, fr, new Rectangle(right - rw, 0, rw, Height), UI.Muted, TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                right -= rw + UI.S(10);
            }
            using (Font fn = new Font("Segoe UI Semibold", 9f))
            using (Font fb = new Font("Segoe UI Semibold", 8f))
            {
                Size bsz = bus.Length > 0 ? TextRenderer.MeasureText(g, bus, fb, new Size(int.MaxValue, Height), TextFormatFlags.NoPadding) : Size.Empty;
                int bw = bus.Length > 0 ? bsz.Width + UI.S(14) : 0;
                Size ns = TextRenderer.MeasureText(g, name, fn, new Size(int.MaxValue, Height), TextFormatFlags.NoPadding);
                int nw = Math.Min(ns.Width, Math.Max(UI.S(40), right - x - bw - UI.S(8)));
                TextRenderer.DrawText(g, name, fn, new Rectangle(x, 0, nw, Height), UI.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                if (bus.Length > 0)
                {
                    Color fg, bbg; UI.BadgeColors("skip", out fg, out bbg);
                    RectangleF br = new RectangleF(x + nw + UI.S(8), (Height - UI.S(18)) / 2f, bw, UI.S(18));
                    using (GraphicsPath p = UI.Round(br, br.Height / 2f)) using (SolidBrush b = new SolidBrush(bbg)) g.FillPath(b, p);
                    TextRenderer.DrawText(g, bus, fb, Rectangle.Round(br), fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
        }
        if (Focused) using (Pen fp = new Pen(UI.Accent, 1f)) { fp.DashStyle = DashStyle.Dot; g.DrawRectangle(fp, 2, 2, Width - 5, Height - 5); }
    }
}

// ab v4.08: Auswahl aus wenigen Feldern mit Titel und Unterzeile (Umfang der Diagnose). Das gewählte Feld trägt die
// Akzentfläche; Pfeiltasten wechseln, wenn das Element den Fokus hat.
public class FeldAuswahl : Control
{
    string[] titel, unter;
    int sel, hoverIdx = -1;
    public event EventHandler SelectedChanged;

    public FeldAuswahl(string[] titel, string[] unter)
    {
        this.titel = titel ?? new string[0]; this.unter = unter ?? new string[0];
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        TabStop = true; Cursor = Cursors.Hand; BackColor = UI.Panel;
        AccessibleRole = AccessibleRole.Grouping;
        Size = new Size(UI.S(190) * Math.Max(1, this.titel.Length), UI.S(50)); Margin = new Padding(0, 0, 0, UI.S(UI.Abstand3));
    }
    public int Count { get { return titel.Length; } }
    public int Selected
    {
        get { return sel; }
        set { if (value < 0 || value >= titel.Length || sel == value) return; sel = value; Invalidate(); if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty); }
    }
    public void SetUnterzeile(int i, string text) { if (i >= 0 && i < unter.Length && unter[i] != (text ?? "")) { unter[i] = text ?? ""; Invalidate(); } }
    public string Unterzeile(int i) { return i >= 0 && i < unter.Length ? unter[i] : ""; }
    // Feld an einer Stelle (für Klick und Hinweise); -1 außerhalb
    public int FeldBei(int x) { if (titel.Length == 0 || x < 0 || x >= Width) return -1; return Math.Min(titel.Length - 1, x * titel.Length / Math.Max(1, Width)); }
    protected override void OnMouseMove(MouseEventArgs e) { int h = FeldBei(e.X); if (h != hoverIdx) { hoverIdx = h; Invalidate(); } base.OnMouseMove(e); }
    protected override void OnMouseLeave(EventArgs e) { hoverIdx = -1; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnMouseClick(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { Focus(); int i = FeldBei(e.X); if (i >= 0) Selected = i; } base.OnMouseClick(e); }
    protected override bool IsInputKey(Keys keyData) { return keyData == Keys.Left || keyData == Keys.Right || base.IsInputKey(keyData); }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Left) { Selected = Math.Max(0, sel - 1); e.Handled = true; }
        else if (e.KeyCode == Keys.Right) { Selected = Math.Min(titel.Length - 1, sel + 1); e.Handled = true; }
        base.OnKeyDown(e);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        using (SolidBrush ob = new SolidBrush(UI.Panel)) g.FillRectangle(ob, ClientRectangle);
        int n = titel.Length; if (n == 0) return;
        RectangleF all = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);
        using (GraphicsPath p = UI.Round(all, UI.SF(6)))
        {
            Region alt = g.Clip;
            g.SetClip(p, CombineMode.Intersect);
            for (int i = 0; i < n; i++)
            {
                int x0 = i * Width / n, x1 = (i + 1) * Width / n;
                Color bg = i == sel ? UI.Accent : i == hoverIdx ? (UI.IsDark ? Color.FromArgb(45, 48, 52) : Color.FromArgb(243, 244, 246)) : UI.Panel;
                using (SolidBrush b = new SolidBrush(bg)) g.FillRectangle(b, x0, 0, x1 - x0, Height);
                if (i > 0 && i != sel && i - 1 != sel) using (Pen pl = new Pen(UI.Line)) g.DrawLine(pl, x0, 0, x0, Height);
            }
            g.Clip = alt;
            using (Pen pen = new Pen(UI.Line, 1f)) g.DrawPath(pen, p);
        }
        using (Font ft = new Font("Segoe UI Semibold", UI.SchriftText))
        using (Font fu = new Font("Segoe UI", 8.5f))
            for (int i = 0; i < n; i++)
            {
                int x0 = i * Width / n, x1 = (i + 1) * Width / n;
                bool on = i == sel;
                string u = i < unter.Length ? unter[i] : "";
                int th = ft.Height, uh = u.Length > 0 ? fu.Height : 0, y = (Height - th - uh) / 2;
                TextRenderer.DrawText(g, titel[i], ft, new Rectangle(x0 + UI.S(4), y, x1 - x0 - UI.S(8), th), on ? UI.OnAccent : UI.Text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                // Unterzeile auf der Akzentfläche in derselben Schriftfarbe wie der Titel (Kontrast wie bei Primärknöpfen)
                if (u.Length > 0) TextRenderer.DrawText(g, u, fu, new Rectangle(x0 + UI.S(4), y + th, x1 - x0 - UI.S(8), uh), on ? UI.OnAccent : UI.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            }
        if (Focused) using (Pen fp = new Pen(UI.Accent, 1f)) { fp.DashStyle = DashStyle.Dot; g.DrawRectangle(fp, 2, 2, Width - 5, Height - 5); }
    }
}

// ab v4.08: Bereich mit den Einstellungen einer Messung oder Prüfung, eingerückt unter ihrer Zeile mit einem Strich links.
// Der Inhalt steht untereinander und bekommt die Breite des Bereichs; die Höhe folgt dem Inhalt.
public class UnterBereich : Panel
{
    public readonly FlowLayoutPanel Inhalt = new FlowLayoutPanel();
    public UnterBereich()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = UI.Panel; Margin = new Padding(UI.S(30), 0, 0, UI.S(8));
        Inhalt.FlowDirection = FlowDirection.TopDown; Inhalt.WrapContents = false; Inhalt.AutoSize = true; Inhalt.AutoSizeMode = AutoSizeMode.GrowAndShrink; Inhalt.BackColor = UI.Panel;
        Inhalt.Location = new Point(UI.S(14), UI.S(4)); Inhalt.Margin = new Padding(0);
        Inhalt.SizeChanged += delegate { Passe(); };
        Controls.Add(Inhalt);
        Height = UI.S(20);
    }
    public int InhaltBreite { get { return Math.Max(UI.S(120), Width - UI.S(14)); } }
    void Passe() { int h = Inhalt.Height + UI.S(8); if (Height != h) Height = h; }
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        int w = InhaltBreite;
        foreach (Control c in Inhalt.Controls)
        {
            if (c is Label) c.MaximumSize = new Size(w - c.Margin.Horizontal, 0);
            else if (c.Width != w - c.Margin.Horizontal) c.Width = w - c.Margin.Horizontal;
        }
        Passe();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        using (SolidBrush b = new SolidBrush(UI.Panel)) e.Graphics.FillRectangle(b, ClientRectangle);
        using (SolidBrush l = new SolidBrush(UI.Line)) e.Graphics.FillRectangle(l, 0, 0, UI.S(2), Height);
    }
}

// ab v4.08: Felder mit Beschriftung über dem Element (Auswahllisten), nebeneinander mit Umbruch nach Breite. Die Höhe
// setzt das Element selbst (FlowLayoutPanel mit AutoSize behält bei Umbruch die Höhe des ersten Aufbaus).
public class FeldReihe : Panel
{
    List<Control[]> felder = new List<Control[]>();
    public FeldReihe() { BackColor = UI.Panel; Margin = new Padding(0, 0, 0, UI.S(4)); Height = UI.S(48); }
    public void Feld(string beschriftung, Control c)
    {
        Label l = new Label(); l.Text = beschriftung; l.AutoSize = true; l.ForeColor = UI.Muted; l.BackColor = Color.Transparent; l.UseMnemonic = false;
        l.Font = new Font("Segoe UI", 8.5f); l.Margin = new Padding(0);
        c.Margin = new Padding(0);
        Controls.Add(l); Controls.Add(c); felder.Add(new Control[] { l, c });
        Ordne();
    }
    // Zeilen, die die Felder bei dieser Breite brauchen (für Tests)
    public int Zeilen { get; private set; }
    protected override void OnResize(EventArgs e) { base.OnResize(e); Ordne(); }
    void Ordne()
    {
        int x = 0, y = 0, zeileH = 0, gap = UI.S(14), lh = UI.S(18), zeilen = felder.Count > 0 ? 1 : 0;
        foreach (Control[] f in felder)
        {
            int w = Math.Max(f[1].Width, f[0].PreferredSize.Width);
            if (x > 0 && x + w > Width) { x = 0; y += zeileH + UI.S(8); zeileH = 0; zeilen++; }
            f[0].Location = new Point(x, y); f[1].Location = new Point(x, y + lh);
            zeileH = Math.Max(zeileH, lh + f[1].Height);
            x += w + gap;
        }
        Zeilen = zeilen;
        int h = y + zeileH + UI.S(2);
        if (Height != h) Height = h;
    }
}

// ab v4.08: Elemente nebeneinander mit Umbruch nach Breite (Hardware-Pillen der Ergebnisseite). Die Höhe setzt das
// Element selbst: Ein FlowLayoutPanel mit AutoSize behielt die Höhe des ersten, schmalen Aufbaus, die Kopfkarte der
// Ergebnisseite blieb dadurch unter Windows unten rund 130 px leer.
public class UmbruchPanel : Panel
{
    bool ordnet;
    public UmbruchPanel() { BackColor = UI.Panel; Height = 0; }
    public int Zeilen { get; private set; }
    protected override void OnResize(EventArgs e) { base.OnResize(e); Ordne(); }
    protected override void OnControlAdded(ControlEventArgs e) { base.OnControlAdded(e); Ordne(); }
    protected override void OnControlRemoved(ControlEventArgs e) { base.OnControlRemoved(e); Ordne(); }
    public void Ordne()
    {
        if (ordnet) return;
        ordnet = true;
        try
        {
            int x = 0, y = 0, zh = 0, zeilen = 0;
            // alle Kindelemente zählen: Visible eines Kindes ist falsch, solange die Seite selbst nicht angezeigt wird
            foreach (Control c in Controls)
            {
                int cw = c.Width + c.Margin.Horizontal;
                if (zeilen == 0) zeilen = 1;
                if (x > 0 && x + cw > Width) { x = 0; y += zh; zh = 0; zeilen++; }
                c.Location = new Point(x + c.Margin.Left, y + c.Margin.Top);
                x += cw; zh = Math.Max(zh, c.Height + c.Margin.Vertical);
            }
            Zeilen = zeilen;
            if (Height != y + zh) Height = y + zh;
        }
        finally { ordnet = false; }
    }
}

// Moderne Fluent-Kartenansicht (Wintoys-Look)
public class FluentCard : Control
{
    string title = "";
    string desc = "";
    string icon = "";
    bool hover;
    ToggleSwitch toggle;
    Button actionButton;
    public event EventHandler CheckedChanged;

    public FluentCard(string title, string desc, string icon = "")
    {
        this.title = title ?? "";
        this.desc = desc ?? "";
        this.icon = icon ?? "";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        Cursor = Cursors.Hand;
        BackColor = UI.Panel;
        Size = new Size(UI.S(400), UI.S(64));
        Margin = new Padding(0, 0, 0, UI.S(8));

        toggle = new ToggleSwitch();
        toggle.Location = new Point(Width - toggle.Width - UI.S(16), (Height - toggle.Height) / 2);
        toggle.CheckedChanged += delegate
        {
            if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            Invalidate();
        };
        Controls.Add(toggle);
    }

    public string Title { get { return title; } set { title = value ?? ""; AccessibleName = title; Invalidate(); } }
    public string Description { get { return desc; } set { desc = value ?? ""; Invalidate(); } }
    public string Icon { get { return icon; } set { icon = value ?? ""; Invalidate(); } }
    public ToggleSwitch Toggle { get { return toggle; } }
    // ab v3.64: Zustand als Etikett hinter dem Titel ("Bereit", "Nicht gefunden"), Farbe wie UI.BadgeColors
    string status = "", statusLevel = "skip";
    public string StatusText { get { return status; } set { status = value ?? ""; Invalidate(); } }
    public string StatusLevel { get { return statusLevel; } set { statusLevel = value ?? "skip"; Invalidate(); } }

    public Button ActionButton
    {
        get { return actionButton; }
        set
        {
            if (actionButton != null) Controls.Remove(actionButton);
            actionButton = value;
            if (actionButton != null)
            {
                if (toggle != null) toggle.Visible = false;
                Controls.Add(actionButton);
                LayoutControls();
            }
            else if (toggle != null && secondaryButton == null)
            {
                toggle.Visible = true;
                LayoutControls();
            }
            Invalidate();
        }
    }

    Button secondaryButton;
    public Button SecondaryButton
    {
        get { return secondaryButton; }
        set
        {
            if (secondaryButton != null) Controls.Remove(secondaryButton);
            secondaryButton = value;
            if (secondaryButton != null)
            {
                if (toggle != null) toggle.Visible = false;
                Controls.Add(secondaryButton);
                LayoutControls();
            }
            else if (toggle != null && actionButton == null)
            {
                toggle.Visible = true;
                LayoutControls();
            }
            Invalidate();
        }
    }

    public bool Checked
    {
        get { return toggle != null && toggle.Checked; }
        set { if (toggle != null) toggle.Checked = value; }
    }

    void LayoutControls()
    {
        int curRight = Width - UI.S(16);
        if (actionButton != null && actionButton.Visible)
        {
            actionButton.Location = new Point(curRight - actionButton.Width, (Height - actionButton.Height) / 2);
            curRight -= actionButton.Width + UI.S(8);
        }
        if (secondaryButton != null && secondaryButton.Visible)
        {
            secondaryButton.Location = new Point(curRight - secondaryButton.Width, (Height - secondaryButton.Height) / 2);
            curRight -= secondaryButton.Width + UI.S(8);
        }
        if (toggle != null && toggle.Visible)
        {
            toggle.Location = new Point(curRight - toggle.Width, (Height - toggle.Height) / 2);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutControls();
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Focus();
            if (actionButton == null && secondaryButton == null && toggle != null && toggle.Visible)
            {
                toggle.Checked = !toggle.Checked;
            }
        }
        base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space && actionButton == null && secondaryButton == null && toggle != null && toggle.Visible)
        {
            toggle.Checked = !toggle.Checked;
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        RectangleF r = new RectangleF(1, 1, Width - 3, Height - 3);
        Color bg = hover ? (UI.IsDark ? Color.FromArgb(42, 44, 46) : Color.FromArgb(250, 251, 253)) : UI.Panel;
        Color border = hover ? (UI.IsDark ? Color.FromArgb(80, 82, 85) : Color.FromArgb(209, 213, 219)) : UI.Line;

        using (GraphicsPath p = UI.Round(r, UI.SF(8)))
        {
            using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
            using (Pen pen = new Pen(border, 1f)) g.DrawPath(pen, p);
        }

        int curX = UI.S(16);
        if (!String.IsNullOrEmpty(icon))
        {
            int icoSize = UI.S(28);
            using (Font ifont = UI.SymbolFont(14))
            {
                Rectangle icoRect = new Rectangle(curX, (Height - icoSize) / 2, icoSize, icoSize);
                TextRenderer.DrawText(g, icon, ifont, icoRect, UI.Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping);
            }
            curX += icoSize + UI.S(10);
        }

        int rightBound = Width;
        if (actionButton != null && actionButton.Visible) rightBound = Math.Min(rightBound, actionButton.Left);
        if (secondaryButton != null && secondaryButton.Visible) rightBound = Math.Min(rightBound, secondaryButton.Left);
        if (toggle != null && toggle.Visible) rightBound = Math.Min(rightBound, toggle.Left);
        rightBound -= UI.S(12);
        int textW = Math.Max(UI.S(50), rightBound - curX);

        using (Font ft = new Font("Segoe UI Semibold", 10f))
        {
            Size tsz = TextRenderer.MeasureText(g, title, ft);
            int titleH = Math.Max(tsz.Height, UI.S(18));
            int startY = String.IsNullOrEmpty(desc) ? (Height - titleH) / 2 : UI.S(12);
            TextRenderer.DrawText(g, title, ft, new Rectangle(curX, startY, textW, titleH), UI.Text, TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            if (status.Length > 0)
            {
                using (Font fs = new Font("Segoe UI Semibold", 8.25f))
                {
                    Size ss = TextRenderer.MeasureText(g, status, fs, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                    int bx = curX + Math.Min(tsz.Width, textW) + UI.S(8), bw = ss.Width + UI.S(14), bh = Math.Max(UI.S(18), ss.Height + UI.S(4));
                    if (bx + bw < rightBound)
                    {
                        Color sfg, sbg; UI.BadgeColors(statusLevel, out sfg, out sbg);
                        Rectangle br = new Rectangle(bx, startY + (titleH - bh) / 2, bw, bh);
                        using (GraphicsPath bp = UI.Round(new RectangleF(br.X, br.Y, br.Width - 1, br.Height - 1), (bh - 1) / 2f)) using (SolidBrush sb = new SolidBrush(sbg)) g.FillPath(sb, bp);
                        TextRenderer.DrawText(g, status, fs, br, sfg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }
                }
            }

            if (!String.IsNullOrEmpty(desc))
            {
                using (Font fd = new Font("Segoe UI", 8.5f))
                {
                    int descY = startY + titleH + UI.S(2);
                    TextRenderer.DrawText(g, desc, fd, new Rectangle(curX, descY, textW, Height - descY - UI.S(4)), UI.Muted, TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
                }
            }
        }
    }
}

// ab v4.05: kompakte Zeile der Seite Tools (Werkzeug oder Windows-Schnellstarter). Zwei Textzeilen statt der 68 px hohen
// FluentCard: oben Name und Zustand, darunter eine Zeile Beschreibung; rechts die Hauptaktion und optional "…" mit Menü.
public class ToolRow : Control
{
    string title = "", desc = "", icon = "", status = "", statusLevel = "skip";
    bool hover;
    Button mainButton, moreButton;

    public ToolRow(string title, string desc, string icon)
    {
        this.title = title ?? ""; this.desc = desc ?? ""; this.icon = icon ?? "";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = UI.Panel;
        Size = new Size(UI.S(480), UI.S(50));
        Margin = new Padding(0, 0, 0, UI.S(6));
        AccessibleRole = AccessibleRole.ListItem;
        AccessibleName = this.title;
    }

    public string Title { get { return title; } set { title = value ?? ""; AccessibleName = title; Invalidate(); } }
    public string Description { get { return desc; } set { desc = value ?? ""; Invalidate(); } }
    public string StatusText { get { return status; } set { status = value ?? ""; Invalidate(); } }
    public string StatusLevel { get { return statusLevel; } set { statusLevel = value ?? "skip"; Invalidate(); } }

    public Button MainButton
    {
        get { return mainButton; }
        set { if (mainButton != null) Controls.Remove(mainButton); mainButton = value; if (value != null) Controls.Add(value); LayoutControls(); Invalidate(); }
    }

    // Knopf "…" für seltene Aktionen (Ordner öffnen, Herstellerseite)
    public Button MoreButton
    {
        get { return moreButton; }
        set { if (moreButton != null) Controls.Remove(moreButton); moreButton = value; if (value != null) Controls.Add(value); LayoutControls(); Invalidate(); }
    }

    void LayoutControls()
    {
        int right = Width - UI.S(10);
        if (moreButton != null) { moreButton.Location = new Point(right - moreButton.Width, (Height - moreButton.Height) / 2); right -= moreButton.Width + UI.S(6); }
        if (mainButton != null) mainButton.Location = new Point(right - mainButton.Width, (Height - mainButton.Height) / 2);
    }

    protected override void OnResize(EventArgs e) { base.OnResize(e); LayoutControls(); }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        RectangleF r = new RectangleF(1, 1, Width - 3, Height - 3);
        Color bg = hover ? (UI.IsDark ? Color.FromArgb(42, 44, 46) : Color.FromArgb(250, 251, 253)) : UI.Panel;
        Color border = hover ? (UI.IsDark ? Color.FromArgb(80, 82, 85) : Color.FromArgb(209, 213, 219)) : UI.Line;
        using (GraphicsPath p = UI.Round(r, UI.SF(6)))
        {
            using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, p);
            using (Pen pen = new Pen(border, 1f)) g.DrawPath(pen, p);
        }
        int x = UI.S(12);
        if (icon.Length > 0)
        {
            int ico = UI.S(22);
            using (Font fi = UI.SymbolFont(12f))
                TextRenderer.DrawText(g, icon, fi, new Rectangle(x, (Height - ico) / 2, ico, ico), UI.Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            x += ico + UI.S(10);
        }
        int right = Width - UI.S(10);
        if (mainButton != null) right = Math.Min(right, mainButton.Left);
        if (moreButton != null) right = Math.Min(right, moreButton.Left);
        int textW = Math.Max(UI.S(60), right - UI.S(10) - x);
        Font ft = FontTitel, fd = FontDetail;
        Size tsz = TextRenderer.MeasureText(g, title, ft, new Size(textW, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        int th = tsz.Height, dh = desc.Length > 0 ? fd.Height : 0;
        int y = (Height - th - (dh > 0 ? dh + UI.S(1) : 0)) / 2;
        TextRenderer.DrawText(g, title, ft, new Rectangle(x, y, textW, th), UI.Text, TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        if (status.Length > 0)
        {
            using (Font fs = new Font("Segoe UI Semibold", 8f))
            {
                Size ss = TextRenderer.MeasureText(g, status, fs, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                int bx = x + Math.Min(tsz.Width, textW) + UI.S(8), bw = ss.Width + UI.S(12), bh = Math.Max(UI.S(16), ss.Height + UI.S(2));
                if (bx + bw <= x + textW)
                {
                    Color sfg, sbg; UI.BadgeColors(statusLevel, out sfg, out sbg);
                    Rectangle br = new Rectangle(bx, y + (th - bh) / 2, bw, bh);
                    using (GraphicsPath bp = UI.Round(new RectangleF(br.X, br.Y, br.Width - 1, br.Height - 1), (bh - 1) / 2f)) using (SolidBrush sb = new SolidBrush(sbg)) g.FillPath(sb, bp);
                    TextRenderer.DrawText(g, status, fs, br, sfg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
        }
        if (dh > 0)
            TextRenderer.DrawText(g, desc, fd, new Rectangle(x, y + th + UI.S(1), textW, dh), UI.Muted, TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
    }

    static Font fontTitel, fontDetail;
    static Font FontTitel { get { if (fontTitel == null) fontTitel = new Font("Segoe UI Semibold", 9.5f); return fontTitel; } }
    static Font FontDetail { get { if (fontDetail == null) fontDetail = new Font("Segoe UI", 8.5f); return fontDetail; } }
}

class FlatBar : Control
{
    int val; bool marquee; int pos; Timer anim;
    public Color Fill = UI.Accent;
    public FlatBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        anim = new Timer(); anim.Interval = 25; anim.Tick += delegate { pos = (pos + UI.S(8)) % Math.Max(1, Width + UI.S(160)); Invalidate(); };
    }
    public int Value { get { return val; } set { int v = Math.Max(0, Math.Min(100, value)); if (v != val) { val = v; Invalidate(); } } }
    public bool Marquee
    {
        get { return marquee; }
        set { if (marquee == value) return; marquee = value; if (value) anim.Start(); else anim.Stop(); Invalidate(); }
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        RectangleF r = new RectangleF(0, 0, Width - 1, Height - 1);
        float rad = Height / 2f;
        Color trackColor = UI.IsDark ? Color.FromArgb(48, 50, 54) : Color.FromArgb(226, 231, 238);
        using (GraphicsPath track = UI.Round(r, rad)) using (SolidBrush tb = new SolidBrush(trackColor)) g.FillPath(tb, track);
        using (GraphicsPath clip = UI.Round(r, rad))
        {
            g.SetClip(clip);
            using (SolidBrush fb = new SolidBrush(Fill))
            {
                if (marquee) g.FillRectangle(fb, pos - UI.S(160), 0, UI.S(160), Height);
                else if (val > 0) g.FillRectangle(fb, 0, 0, (Width - 1) * val / 100f, Height);
            }
            g.ResetClip();
        }
    }
}

// Eintrag der Modulnavigation: moderner Windows 11-Sidebar-Stil
class NavItem : Control
{
    bool sel, hover, chk, running;
    public string Title, Desc, Icon;
    // ab v3.64: kurzer Zusatz rechts im Eintrag, etwa die geschätzte Dauer eines angehakten Moduls
    public string Hint = "";
    // ab v4.08: Zusatz als Pille in Akzentschrift (Punktzahl am Eintrag Ergebnis)
    public bool HintPille = false;
    public bool HasCheck = true;
    public event EventHandler Picked;
    public event EventHandler CheckedChanged;
    public NavItem(string title, string desc) : this(title, desc, "") { }
    public NavItem(string title, string desc, string icon)
    {
        Title = title; Desc = desc; Icon = icon ?? "";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        Cursor = Cursors.Hand; Size = new Size(UI.S(226), UI.S(40)); Margin = new Padding(0, 0, 0, UI.S(4)); BackColor = UI.Bg;
    }
    public bool Running { get { return running; } set { if (running != value) { running = value; Invalidate(); } } }
    public bool Selected { get { return sel; } set { sel = value; Invalidate(); } }
    public bool Checked
    {
        get { return chk; }
        set { if (chk == value) return; chk = value; Invalidate(); if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty); }
    }
    Rectangle Box { get { return new Rectangle(UI.S(14), (Height - UI.S(20)) / 2, UI.S(20), UI.S(20)); } }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseClick(MouseEventArgs e)
    {
        Focus();
        Rectangle hit = Box; hit.Inflate(UI.S(8), UI.S(8));
        if (HasCheck && hit.Contains(e.Location)) Checked = !Checked;
        if (Picked != null) Picked(this, e);
        base.OnMouseClick(e);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space && HasCheck) Checked = !Checked;
        if (e.KeyCode == Keys.Enter && Picked != null) Picked(this, e);
        base.OnKeyDown(e);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        RectangleF r = new RectangleF(1, 1, Width - 3, Height - 3);
        // ab v3.64: angezeigte Seite nur über Akzentbalken und AccentSoft, Überfahren nur leicht aufgehellt ohne Rahmenwechsel,
        // ausgeführt wird, was angehakt ist (vorher sahen Überfahren und Auswahl fast gleich aus)
        Color fill = sel ? UI.AccentSoft : (hover ? (UI.IsDark ? Color.FromArgb(40, 41, 43) : Color.FromArgb(244, 246, 249)) : UI.Bg);
        Color borderPen = sel ? UI.AccentSoft : (hover ? UI.Line : UI.Bg);
        using (GraphicsPath p = UI.Round(r, UI.SF(6)))
        {
            using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, p);
            using (Pen pen = new Pen(borderPen, UI.SF(1f))) g.DrawPath(pen, p);
        }

        // Bei Auswahl: Links ein 3 px breiter, abgerundeter blauer Akzentbalken
        if (running)
        {
            int dotSz = UI.S(8);
            int dotX = Width - UI.S(16);
            int dotY = (Height - dotSz) / 2;
            using (SolidBrush db = new SolidBrush(UI.Accent)) g.FillEllipse(db, dotX, dotY, dotSz, dotSz);
        }
        if (sel)
        {
            int barH = Height - UI.S(16);
            RectangleF barR = new RectangleF(UI.S(3), (Height - barH) / 2f, UI.S(3), barH);
            using (GraphicsPath bp = UI.Round(barR, UI.SF(1.5f)))
            using (SolidBrush bb = new SolidBrush(UI.Accent))
                g.FillPath(bb, bp);
        }

        int x = UI.S(14);
        if (HasCheck)
        {
            Rectangle bx = Box;
            using (GraphicsPath bp = UI.Round(bx, UI.SF(5)))
            {
                using (SolidBrush bb = new SolidBrush(chk ? UI.Accent : UI.Panel)) g.FillPath(bb, bp);
                Color chkBorder = chk ? UI.Accent : (UI.IsDark ? Color.FromArgb(90, 95, 105) : Color.FromArgb(160, 170, 185));
                using (Pen bpen = new Pen(chkBorder, UI.SF(1.5f))) g.DrawPath(bpen, bp);
            }
            if (chk) using (Pen ck = new Pen(UI.OnAccent, UI.SF(2.4f))) { ck.StartCap = LineCap.Round; ck.EndCap = LineCap.Round; g.DrawLines(ck, new Point[] { new Point(bx.X + UI.S(5), bx.Y + UI.S(10)), new Point(bx.X + UI.S(9), bx.Y + UI.S(14)), new Point(bx.X + UI.S(15), bx.Y + UI.S(6)) }); }
            x = bx.Right + UI.S(10);
        }

        if (!String.IsNullOrEmpty(Icon))
        {
            int icoSize = UI.S(22);
            int icoY = (Height - icoSize) / 2;
            using (Font ifont = UI.SymbolFont(12.5f))
            {
                Rectangle icoR = new Rectangle(x, icoY, icoSize, icoSize);
                TextRenderer.DrawText(g, Icon, ifont, icoR, sel ? UI.Accent : UI.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping);
            }
            x += icoSize + UI.S(8);
        }

        using (Font ft = new Font("Segoe UI Semibold", 10f))
        {
            Size tsz = TextRenderer.MeasureText(g, Title, ft);
            int th = Math.Max(tsz.Height, UI.S(18));
            int startY = (Height - th) / 2;
            Color titleColor = sel ? (UI.IsDark ? UI.Accent : UI.AccentDark) : UI.Text;
            int right = Width - UI.S(10);
            if (!String.IsNullOrEmpty(Hint))
                using (Font fh = new Font("Segoe UI", 8.5f))
                {
                    Size hs = TextRenderer.MeasureText(g, Hint, fh);
                    if (HintPille)
                    {
                        int pw = hs.Width + UI.S(12), ph = UI.S(20);
                        if (right - pw > x + tsz.Width + UI.S(6))
                        {
                            RectangleF pr = new RectangleF(right - pw, (Height - ph) / 2f, pw, ph);
                            Color pbg = sel ? UI.Panel : (UI.IsDark ? Color.FromArgb(44, 47, 51) : Color.FromArgb(235, 238, 242));
                            using (GraphicsPath pp = UI.Round(pr, ph / 2f)) using (SolidBrush pb = new SolidBrush(pbg)) g.FillPath(pb, pp);
                            TextRenderer.DrawText(g, Hint, fh, Rectangle.Round(pr), UI.IsDark ? UI.Accent : UI.AccentDark, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                            right -= pw + UI.S(6);
                        }
                    }
                    else if (right - hs.Width > x + tsz.Width + UI.S(6))
                    {
                        TextRenderer.DrawText(g, Hint, fh, new Rectangle(right - hs.Width, 0, hs.Width, Height), UI.Muted, TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                        right -= hs.Width + UI.S(6);
                    }
                }
            TextRenderer.DrawText(g, Title, ft, new Rectangle(x, startY, Math.Max(10, right - x), th), titleColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}

class StatCard : Control
{
    public string Caption; public Color Accent; int count;
    public StatCard(string caption, Color accent)
    {
        Caption = caption; Accent = accent;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(UI.S(170), UI.S(72)); Margin = new Padding(0, 0, UI.S(12), 0); BackColor = UI.Bg;
    }
    public int Count { get { return count; } set { count = value; Invalidate(); } }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        RectangleF r = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);
        using (GraphicsPath p = UI.Round(r, UI.SF(8)))
        {
            using (SolidBrush b = new SolidBrush(UI.Panel)) g.FillPath(b, p);
            using (Pen pen = new Pen(UI.Line)) g.DrawPath(pen, p);
            g.SetClip(p);
            using (SolidBrush a = new SolidBrush(Accent)) g.FillRectangle(a, 0, 0, UI.S(5), Height);
            g.ResetClip();
        }
        using (Font fn = new Font("Segoe UI Semibold", 19f))
        {
            Size numSz = TextRenderer.MeasureText(g, count.ToString(), fn);
            int numY = UI.S(6);
            TextRenderer.DrawText(g, count.ToString(), fn, new Point(UI.S(16), numY), count > 0 ? Accent : UI.Muted);
            using (Font fc = new Font("Segoe UI", 8.5f))
            {
                int capY = numY + numSz.Height - UI.S(2);
                TextRenderer.DrawText(g, Caption, fc, new Rectangle(UI.S(16), capY, Width - UI.S(20), Height - capY), UI.Muted, TextFormatFlags.Left | TextFormatFlags.WordBreak);
            }
        }
    }
}

class TabStrip : Control
{
    public List<string> Items = new List<string>();
    int selected;
    public event EventHandler SelectedChanged;
    public TabStrip()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = UI.S(38); Cursor = Cursors.Hand; BackColor = UI.Bg;
    }
    public int Selected { get { return selected; } set { selected = value; Invalidate(); if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty); } }
    public void SetText(int i, string t) { if (Items[i] == t) return; Items[i] = t; Invalidate(); }
    Rectangle ItemRect(Graphics g, int i, Font f)
    {
        int x = 0;
        int pad = UI.S(28);
        for (int k = 0; k <= i; k++)
        {
            int w = TextRenderer.MeasureText(g, Items[k], f).Width + pad;
            if (k == i) return new Rectangle(x, 0, w, Height);
            x += w + UI.S(4);
        }
        return Rectangle.Empty;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        using (Pen line = new Pen(UI.Line)) g.DrawLine(line, 0, Height - 1, Width, Height - 1);
        int barH = UI.S(2);
        int barPad = UI.S(8);
        for (int i = 0; i < Items.Count; i++)
        {
            using (Font f = new Font(i == selected ? "Segoe UI Semibold" : "Segoe UI", 9.5f))
            {
                Rectangle r = ItemRect(g, i, f);
                TextRenderer.DrawText(g, Items[i], f, r, i == selected ? UI.Accent : UI.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                if (i == selected) using (SolidBrush b = new SolidBrush(UI.Accent)) g.FillRectangle(b, r.X + barPad, Height - barH, r.Width - (barPad * 2), barH);
            }
        }
    }
    protected override void OnMouseClick(MouseEventArgs e)
    {
        using (Graphics g = CreateGraphics())
            for (int i = 0; i < Items.Count; i++)
                using (Font f = new Font(i == selected ? "Segoe UI Semibold" : "Segoe UI", 9.5f))
                    if (ItemRect(g, i, f).Contains(e.Location)) { Selected = i; break; }
        base.OnMouseClick(e);
    }
}

// Kurven der Leitwerte (SENSLEAD): drei Felder übereinander für Temperatur, Takt und Leistung, je CPU, GPU und Prozessorgrafik.
// WindowSec > 0 zeigt nur die letzten Sekunden (Live-Seite), 0 den ganzen Verlauf (Lasttest).
// Neuzeichnen gebündelt: Add() merkt nur vor, Flush() (Zeitgeber der Oberfläche) zeichnet höchstens einmal je Takt.
class SensorChart : Control
{
    // Spalten: 0 Zeit, 1 CPU °C, 2 CPU MHz (Task-Manager), 3 CPU W, 4 GPU °C, 5 GPU MHz, 6 GPU W, 7 Lüfter, 8 CPU-Last,
    //          9 höchster Kerntakt, 10 iGPU °C, 11 iGPU MHz, 12 iGPU W, 13 Bilder/s GPU, 14 Bilder/s iGPU (Rendertest, ab v2.6)
    const int Cols = 15;
    bool hasFps;
    List<double[]> rows = new List<double[]>();
    bool dirty;
    public int WindowSec = 600;
    public double CpuLimit = double.NaN, GpuLimit = double.NaN, TjMax = double.NaN;
    public string Empty = "Noch keine Messwerte.";
    static readonly Color CpuColor = Color.FromArgb(37, 99, 235), CoreColor = Color.FromArgb(125, 160, 245), GpuColor = Color.FromArgb(194, 65, 12), IGpuColor = Color.FromArgb(202, 138, 4);
    struct Series { public int Col; public Color Color; public string Label; public bool Dash; public Series(int c, Color k, string l, bool d) { Col = c; Color = k; Label = l; Dash = d; } }
    public SensorChart()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = UI.Panel;
    }
    public int Count { get { return rows.Count; } }
    public void Clear() { rows.Clear(); tOffset = 0; hasFps = false; CpuLimit = double.NaN; GpuLimit = double.NaN; TjMax = double.NaN; dirty = false; Invalidate(); }
    static double D(string s)
    {
        double v; return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : double.NaN;
    }
    // p wie @@SENSLEAD|t|cpuT|cpuMHz|cpuW|gpuT|gpuMHz|gpuW|fan|cpuLoad|coreMax|igpuT|igpuMHz|igpuW (p[0] = "SENSLEAD")
    // Zeitversatz (ab v2.7): Benchmark und Lasttest zählen ihre Sekunden je ab 0. Springt die Zeit zurück, läuft die
    // Kurve hinter dem letzten Punkt weiter, statt sich zu überlagern.
    double tOffset;
    public double[] Add(string[] p)
    {
        double[] r = new double[Cols];
        for (int i = 0; i < Cols; i++) r[i] = (i + 1 < p.Length) ? D(p[i + 1]) : double.NaN;
        if (double.IsNaN(r[0])) return r;
        if (rows.Count > 0)
        {
            double lastT = rows[rows.Count - 1][0];
            if (r[0] + tOffset < lastT - 0.5) tOffset = lastT + 2 - r[0];
        }
        r[0] += tOffset;
        if (!double.IsNaN(r[13]) || !double.IsNaN(r[14])) hasFps = true;
        rows.Add(r);
        if (rows.Count > 30000) rows.RemoveRange(0, rows.Count - 30000);
        dirty = true;
        return r;
    }
    // vom Zeitgeber der Oberfläche aufgerufen: neu zeichnen, wenn seit dem letzten Mal Werte dazukamen
    public void Flush() { if (dirty) { dirty = false; Invalidate(); } }
    public void SetLimits(string[] p)
    {
        CpuLimit = p.Length > 1 ? D(p[1]) : double.NaN; GpuLimit = p.Length > 2 ? D(p[2]) : double.NaN; TjMax = p.Length > 3 ? D(p[3]) : double.NaN;
        Invalidate();
    }
    public double[] Last { get { return rows.Count > 0 ? rows[rows.Count - 1] : null; } }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);
        using (Pen bp = new Pen(UI.Line)) g.DrawRectangle(bp, 0, 0, Width - 1, Height - 1);
        if (rows.Count < 1) { using (Font f = new Font("Segoe UI", 9.75f)) TextRenderer.DrawText(g, Empty, f, ClientRectangle, UI.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak); return; }
        double tMax = rows[rows.Count - 1][0];
        double tMin = WindowSec > 0 ? Math.Max(0, tMax - WindowSec) : rows[0][0];
        if (tMax - tMin < 10) tMax = tMin + 10;
        int n = hasFps ? 4 : 3;
        int h = (Height - 8) / n;
        DrawPanel(g, new Rectangle(0, 4, Width, h), "Temperatur", "°C", new Series[] { new Series(1, CpuColor, "CPU", false), new Series(4, GpuColor, "GPU", false), new Series(10, IGpuColor, "iGPU", true) }, tMin, tMax, true);
        DrawPanel(g, new Rectangle(0, 4 + h, Width, h), "Takt", "MHz", new Series[] { new Series(2, CpuColor, "CPU", false), new Series(9, CoreColor, "höchster Kern", true), new Series(5, GpuColor, "GPU", false), new Series(11, IGpuColor, "iGPU", true) }, tMin, tMax, false);
        DrawPanel(g, new Rectangle(0, 4 + 2 * h, Width, h), "Leistung", "W", new Series[] { new Series(3, CpuColor, "CPU", false), new Series(6, GpuColor, "GPU", false), new Series(12, IGpuColor, "iGPU", true) }, tMin, tMax, false);
        if (hasFps) DrawPanel(g, new Rectangle(0, 4 + 3 * h, Width, h), "Rendertest", "Bilder/s", new Series[] { new Series(13, GpuColor, "GPU", false), new Series(14, IGpuColor, "iGPU", true) }, tMin, tMax, false);
    }

    void DrawPanel(Graphics g, Rectangle r, string title, string unit, Series[] ser, double tMin, double tMax, bool limits)
    {
        // ab v4.03 mit UI.S: feste Pixel ließen bei 150 % Achsenbeschriftung und Legende ineinanderlaufen
        Rectangle plot = new Rectangle(r.X + UI.S(58), r.Y + UI.S(22), r.Width - UI.S(58 + 14), r.Height - UI.S(22 + 18));
        if (plot.Width < 40 || plot.Height < 20) return;
        double lo = double.MaxValue, hi = double.MinValue; bool any = false;
        foreach (double[] row in rows)
        {
            if (row[0] < tMin) continue;
            foreach (Series sr in ser) { double v = row[sr.Col]; if (!double.IsNaN(v)) { lo = Math.Min(lo, v); hi = Math.Max(hi, v); any = true; } }
        }
        if (limits)
        {
            foreach (double v in new double[] { CpuLimit, GpuLimit, TjMax }) if (!double.IsNaN(v) && v > 0 && any) { hi = Math.Max(hi, v); }
        }
        if (unit == "Bilder/s") lo = 0;
        if (unit == "%") { lo = 0; if (hi < 100) hi = 100; }
        int legX = r.X + UI.S(140);
        using (Font ft = new Font("Segoe UI Semibold", 9f))
        {
            string tt = title + " (" + unit + ")";
            TextRenderer.DrawText(g, tt, ft, new Point(r.X + UI.S(8), r.Y + UI.S(2)), UI.Text);
            legX = Math.Max(legX, r.X + UI.S(8) + TextRenderer.MeasureText(tt, ft).Width + UI.S(12));
        }
        double[] last = rows[rows.Count - 1];
        using (Font fl = new Font("Segoe UI", 8.75f))
        {
            int x = legX;
            foreach (Series sr in ser)
            {
                string lv = Fmt(last[sr.Col], unit);
                if (lv.Length == 0) continue;
                string txt = sr.Label + " " + lv;
                using (SolidBrush b = new SolidBrush(sr.Color)) g.FillRectangle(b, x, r.Y + UI.S(7), UI.S(10), UI.S(10));
                TextRenderer.DrawText(g, txt, fl, new Point(x + UI.S(14), r.Y + UI.S(3)), UI.Text);
                x += UI.S(22) + TextRenderer.MeasureText(txt, fl).Width;
            }
            // Abbruchschwelle rechtsbündig; passt sie nicht neben die Legende, die Kurzform, sonst nur die gestrichelten Linien
            if (limits && !double.IsNaN(CpuLimit) && CpuLimit > 0)
            {
                bool gpuL = !double.IsNaN(GpuLimit) && GpuLimit > 0;
                string[] varianten = new string[] {
                    "Abbruch CPU " + CpuLimit.ToString("0") + " °C" + (gpuL ? ", GPU " + GpuLimit.ToString("0") + " °C" : ""),
                    "Abbruch " + CpuLimit.ToString("0") + (gpuL ? "/" + GpuLimit.ToString("0") : "") + " °C" };
                foreach (string lt in varianten)
                {
                    int w = TextRenderer.MeasureText(lt, fl).Width;
                    int lx = r.Right - UI.S(14) - w;
                    if (lx < x + UI.S(4)) continue;
                    TextRenderer.DrawText(g, lt, fl, new Point(lx, r.Y + UI.S(3)), UI.Crit);
                    break;
                }
            }
        }
        if (!any) { using (Font f = new Font("Segoe UI", 8.75f)) TextRenderer.DrawText(g, "kein Wert verfügbar", f, plot, UI.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); return; }
        double span = hi - lo; if (span < 1) span = 1;
        double step = NiceStep(span / 3);
        double y0 = Math.Floor(lo / step) * step, y1 = Math.Ceiling(hi / step) * step; if (y1 <= y0) y1 = y0 + step;
        using (Pen grid = new Pen(UI.IsDark ? Color.FromArgb(45, 48, 52) : Color.FromArgb(236, 239, 243)))
        using (Font fa = new Font("Segoe UI", 7.75f))
        {
            for (double v = y0; v <= y1 + step / 1000; v += step)
            {
                float y = (float)(plot.Bottom - (v - y0) / (y1 - y0) * plot.Height);
                g.DrawLine(grid, plot.Left, y, plot.Right, y);
                TextRenderer.DrawText(g, v.ToString("0"), fa, new Rectangle(r.X, (int)y - UI.S(8), UI.S(52), UI.S(16)), UI.Muted, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
            double ts = 10; foreach (double c in new double[] { 10, 30, 60, 120, 300, 600, 900, 1800, 3600 }) { ts = c; if ((tMax - tMin) / c <= 8) break; }
            for (double t = Math.Ceiling(tMin / ts) * ts; t <= tMax; t += ts)
            {
                float x = (float)(plot.Left + (t - tMin) / (tMax - tMin) * plot.Width);
                g.DrawLine(grid, x, plot.Top, x, plot.Bottom);
                string lab = ts >= 60 ? ((int)(t / 60)).ToString() + " min" : ((int)t).ToString() + " s";
                TextRenderer.DrawText(g, lab, fa, new Rectangle((int)x - UI.S(30), plot.Bottom + 1, UI.S(60), UI.S(16)), UI.Muted, TextFormatFlags.HorizontalCenter);
            }
        }
        if (limits)
        {
            DrawLimit(g, plot, CpuLimit, y0, y1, UI.Crit, DashStyle.Dash);
            DrawLimit(g, plot, GpuLimit, y0, y1, Color.FromArgb(194, 65, 12), DashStyle.Dash);
            if (!double.IsNaN(TjMax) && TjMax != CpuLimit) DrawLimit(g, plot, TjMax, y0, y1, UI.Warn, DashStyle.Dot);
        }
        for (int i = ser.Length - 1; i >= 0; i--) DrawSeries(g, plot, ser[i].Col, tMin, tMax, y0, y1, ser[i].Color, ser[i].Dash);
    }

    static void DrawLimit(Graphics g, Rectangle plot, double v, double y0, double y1, Color c, DashStyle ds)
    {
        if (double.IsNaN(v) || v <= 0 || v < y0 || v > y1) return;
        float y = (float)(plot.Bottom - (v - y0) / (y1 - y0) * plot.Height);
        using (Pen p = new Pen(c, 1.5f)) { p.DashStyle = ds; g.DrawLine(p, plot.Left, y, plot.Right, y); }
    }

    void DrawSeries(Graphics g, Rectangle plot, int k, double tMin, double tMax, double y0, double y1, Color c, bool dash)
    {
        List<PointF> pts = new List<PointF>();
        using (Pen p = new Pen(c, dash ? 1.6f : 2f))
        {
            p.LineJoin = LineJoin.Round;
            if (dash) p.DashStyle = DashStyle.Dash;
            foreach (double[] row in rows)
            {
                if (row[0] < tMin) continue;
                if (double.IsNaN(row[k])) { if (pts.Count > 1) g.DrawLines(p, pts.ToArray()); pts.Clear(); continue; }
                pts.Add(new PointF((float)(plot.Left + (row[0] - tMin) / (tMax - tMin) * plot.Width), (float)(plot.Bottom - (row[k] - y0) / (y1 - y0) * plot.Height)));
            }
            if (pts.Count > 1) g.DrawLines(p, pts.ToArray());
            else if (pts.Count == 1) using (SolidBrush b = new SolidBrush(c)) g.FillEllipse(b, pts[0].X - 2, pts[0].Y - 2, 4, 4);
        }
    }

    static double NiceStep(double raw)
    {
        if (raw <= 0) return 1;
        double mag = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        foreach (double f in new double[] { 1, 2, 2.5, 5, 10 }) if (f * mag >= raw) return f * mag;
        return 10 * mag;
    }

    public static string Fmt(double v, string unit)
    {
        if (double.IsNaN(v)) return "";
        return v.ToString(unit == "W" ? "0.0" : "0", CultureInfo.GetCultureInfo("de-DE")) + " " + unit;
    }
}
