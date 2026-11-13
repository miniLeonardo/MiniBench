// Farbschema, dunkle Titelleiste und native Steuerelemente (Teilklasse DiagGui)
public partial class DiagGui
{
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        try { int round = DWMWCP_ROUND; DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(int)); } catch { }
        ApplyTitleBarTheme(UI.IsDark);
        ApplyNativeControlThemes();
    }

    void ApplyNativeControlThemes()
    {
        string subApp = UI.IsDark ? "DarkMode_Explorer" : "Explorer";
        try { SetPreferredAppMode(UI.IsDark ? 2 : 0); } catch { }
        if (content != null && content.IsHandleCreated) try { SetWindowTheme(content.Handle, subApp, null); } catch { }
        if (left != null && left.IsHandleCreated) try { SetWindowTheme(left.Handle, subApp, null); } catch { }
        if (lvDb != null && lvDb.IsHandleCreated) try { SetWindowTheme(lvDb.Handle, subApp, null); } catch { }
        if (lvSens != null && lvSens.IsHandleCreated) try { SetWindowTheme(lvSens.Handle, subApp, null); } catch { }
        if (lvChg != null && lvChg.IsHandleCreated) try { SetWindowTheme(lvChg.Handle, subApp, null); } catch { }
        if (clbCompare != null && clbCompare.IsHandleCreated) try { SetWindowTheme(clbCompare.Handle, subApp, null); } catch { }
        if (txtLog != null && txtLog.IsHandleCreated) try { SetWindowTheme(txtLog.Handle, subApp, null); } catch { }
        ApplyNativeThemeRecursive(this, subApp);
    }

    void ApplyNativeThemeRecursive(Control c, string subApp)
    {
        if (c == null) return;
        if (c.IsHandleCreated)
        {
            if (c is ScrollableControl || c is ListView || c is TextBoxBase || c is ListBox)
            {
                try { SetWindowTheme(c.Handle, subApp, null); } catch { }
            }
        }
        foreach (Control child in c.Controls)
        {
            ApplyNativeThemeRecursive(child, subApp);
        }
    }

    public void EnableDarkListView(ListView lv)
    {
        if (lv == null) return;
        lv.OwnerDraw = true;
        lv.BackColor = UI.Panel;
        lv.ForeColor = UI.Text;
        lv.DrawColumnHeader += delegate(object s, DrawListViewColumnHeaderEventArgs e) {
            Color headerBg = UI.IsDark ? Color.FromArgb(28, 30, 32) : Color.FromArgb(248, 249, 251);
            using (SolidBrush b = new SolidBrush(headerBg)) e.Graphics.FillRectangle(b, e.Bounds);
            using (Pen pen = new Pen(UI.Line)) e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            using (Font f = new Font("Segoe UI Semibold", 8.5f))
                TextRenderer.DrawText(e.Graphics, e.Header.Text.ToUpperInvariant(), f,
                    new Rectangle(e.Bounds.X + UI.S(8), e.Bounds.Y, e.Bounds.Width - UI.S(10), e.Bounds.Height),
                    UI.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        };
        lv.DrawItem += delegate(object s, DrawListViewItemEventArgs e) { e.DrawDefault = true; };
        lv.DrawSubItem += delegate(object s, DrawListViewSubItemEventArgs e) { e.DrawDefault = true; };
        lv.HandleCreated += delegate { try { SetWindowTheme(lv.Handle, UI.IsDark ? "DarkMode_Explorer" : "Explorer", null); } catch { } };
        if (lv.IsHandleCreated) try { SetWindowTheme(lv.Handle, UI.IsDark ? "DarkMode_Explorer" : "Explorer", null); } catch { }
    }

    void ApplyTitleBarTheme(bool dark)
    {
        if (!IsHandleCreated || Handle == IntPtr.Zero) return;
        int darkMode = dark ? 1 : 0;
        try
        {
            int hr = DwmSetWindowAttribute(Handle, 20, ref darkMode, sizeof(int));
            if (hr != 0)
            {
                DwmSetWindowAttribute(Handle, 19, ref darkMode, sizeof(int));
            }
        }
        catch { }
    }


    bool DetectWindowsDarkTheme()
    {
        try
        {
            using (Microsoft.Win32.RegistryKey k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (k != null)
                {
                    object val = k.GetValue("AppsUseLightTheme");
                    if (val is int) return ((int)val) == 0;
                }
            }
        }
        catch { }
        return false;
    }


    void ToggleTheme()
    {
        SetAppTheme(!UI.IsDark);
    }

    void SetAppTheme(bool dark) { WendeFarbschemaAn(dark, true); }

    // speichern = false: nur neu einfärben (Wechsel der Akzentfarbe). Wer das Farbschema nie gewählt hat, folgt weiter Windows.
    void WendeFarbschemaAn(bool dark, bool speichern)
    {
        UI.SetTheme(dark);
        try { SetPreferredAppMode(dark ? 2 : 0); } catch { }
        if (speichern) SaveThemePreference(dark);
        ApplyTitleBarTheme(dark);
        ApplyNativeControlThemes();
        if (btnThemeToggle != null)
        {
            btnThemeToggle.Text = dark ? UI.IcoSonne : UI.IcoMond;
            Tip(btnThemeToggle, dark ? "Zu hellem Farbschema wechseln." : "Zu dunklem Farbschema wechseln.");
        }

        SuspendLayout();
        try
        {
            BackColor = UI.Bg;
            ForeColor = UI.Text;
            ApplyThemeRecursive(this);
            ApplyDiagProfile();
            // ab v3.64: Warnfelder und gedämpfte Beschriftungen tragen eigene Farben
            foreach (Label w in new Label[] { lblAkku, lblRestoreWarn }) if (w != null) { w.ForeColor = UI.Warn; w.BackColor = UI.WarnBg; }
            foreach (Panel b in modBanners) if (b != null) { b.BackColor = UI.InfoBg; foreach (Control k in b.Controls) { if (k is LinkLabel) ((LinkLabel)k).LinkColor = UI.Accent; else k.ForeColor = UI.Text; } }
            if (bandErg != null) { bandErg.BackColor = UI.AccentSoft; if (lblBandErg != null) lblBandErg.ForeColor = UI.Text; }
            // ab v4.08: Zeile zur Referenz und die aufgebaute Ergebnisseite tragen eigene Farben (Warnung, Urteil): neu setzen
            UpdateRefState(); FuelleLetztesErgebnis();
            if (erg != null && ergebnisAufbauten > 0 && !ergVeraltet) FuelleErgebnis();
            UpdateLoadEnabled(); ThemeLog();
            if (umzugRechne != null) umzugRechne();
            if (barAll != null && !done) barAll.Fill = UI.Accent;
            if (barSub != null) barSub.Fill = UI.AccentActive;
        }
        finally
        {
            ResumeLayout(true);
            Invalidate(true);
        }
    }

    void ApplyThemeRecursive(Control c)
    {
        if (c == null) return;

        if (c == head)
        {
            c.BackColor = UI.Header;
        }
        else if (c == contentHost)
        {
            c.BackColor = UI.Line;
        }
        else if (c == content)
        {
            c.BackColor = UI.Panel;
            if (c.IsHandleCreated) try { SetWindowTheme(c.Handle, UI.IsDark ? "DarkMode_Explorer" : "Explorer", null); } catch { }
        }
        else if (c is NavItem)
        {
            c.BackColor = UI.Bg;
            c.Invalidate();
        }
        else if (c is FluentCard || c is ToolRow || c is StartTaskCard || c is SettingRow || c is CategoryPickerItem || c is LaufwerkZeile || c is FeldAuswahl)
        {
            c.BackColor = UI.Panel;
            c.Invalidate();
        }
        else if (c is ToggleSwitch || c is Badge || c is PillLabel || c is StartTaskRow || c is KpiBox)
        {
            c.Invalidate();
        }
        else if (c is GroupHeader)
        {
            c.BackColor = UI.Panel;
            c.Invalidate();
        }
        else if (c is FlatBar)
        {
            c.Invalidate();
        }
        else if (c is StatCard)
        {
            c.BackColor = UI.Bg;
            c.Invalidate();
        }
        else if (c is TabStrip)
        {
            c.BackColor = UI.Bg;
            c.Invalidate();
        }
        else if (c is SensorChart)
        {
            c.BackColor = UI.Panel;
            c.Invalidate();
        }
        else if (c is ListView)
        {
            c.BackColor = UI.Panel;
            c.ForeColor = UI.Text;
            if (c.IsHandleCreated) try { SetWindowTheme(c.Handle, UI.IsDark ? "DarkMode_Explorer" : "Explorer", null); } catch { }
            c.Invalidate();
        }
        else if (c is RichTextBox)
        {
            c.BackColor = UI.Panel;
            c.ForeColor = UI.Text;
        }
        else if (c is TextBox)
        {
            if (c != txtLog)
            {
                c.BackColor = UI.Panel;
                c.ForeColor = UI.Text;
            }
        }
        else if (c is DarkComboBox || c is ComboBox)
        {
            c.BackColor = UI.Panel;
            c.ForeColor = UI.Text;
            c.Invalidate();
        }
        else if (c is CheckedListBox || c is ListBox)
        {
            c.BackColor = UI.Panel;
            c.ForeColor = UI.Text;
            if (c.IsHandleCreated) try { SetWindowTheme(c.Handle, UI.IsDark ? "DarkMode_Explorer" : "Explorer", null); } catch { }
            c.Invalidate();
        }
        else if (c is RadioButton && ((RadioButton)c).Appearance == Appearance.Button)
        {
            StyleSegment((RadioButton)c);
        }
        else if (c is CheckBox || c is RadioButton)
        {
            // ab v4.05: installierte Softwarepakete (Tag "installiert") bleiben grün
            c.ForeColor = (c.Tag as string) == "installiert" ? UI.Ok : c.Enabled ? UI.Text : UI.Muted;
        }
        else if (c is LinkLabel)
        {
            LinkLabel ll = (LinkLabel)c;
            if (ll.LinkColor == Color.FromArgb(28, 29, 31) || ll.LinkColor == Color.FromArgb(245, 246, 247) || ll.LinkColor == Color.Black || ll.LinkColor == SystemColors.ControlText)
            {
                ll.LinkColor = UI.Text;
            }
            else
            {
                ll.LinkColor = UI.Accent;
            }
            ll.ActiveLinkColor = UI.AccentHover;
            ll.ForeColor = UI.Text;
        }
        else if (c is Button)
        {
            Button btn = (Button)c;
            if (btn.Parent != null && btn.Parent.Parent == head)
            {
                btn.BackColor = HeadButtonBack();
                btn.ForeColor = Color.White;
                btn.FlatAppearance.MouseOverBackColor = HeadButtonHover();
            }
            else if (btn is PrimaryButton || btn == btnHtml || btn == btnStart || (btnUndo != null && btn == btnUndo) || (btnDashboard != null && btn == btnDashboard))
            {
                btn.BackColor = UI.Accent;
                btn.ForeColor = UI.OnAccent;
                // Symbol im Knopf (Tag = Zeichen der Symbolschrift) in der neuen Schriftfarbe neu zeichnen
                string glyph = btn.Tag as string;
                if (btn.Image != null && !String.IsNullOrEmpty(glyph)) { Image old = btn.Image; btn.Image = UI.GlyphImage(glyph, UI.OnAccent, old.Width); old.Dispose(); }
                btn.FlatAppearance.MouseOverBackColor = UI.AccentHover;
                btn.FlatAppearance.MouseDownBackColor = UI.AccentActive;
            }
            else
            {
                btn.BackColor = UI.Panel;
                btn.ForeColor = UI.Text;
                btn.FlatAppearance.BorderColor = UI.Line;
                btn.FlatAppearance.MouseOverBackColor = UI.IsDark ? Color.FromArgb(45, 48, 52) : Color.FromArgb(243, 244, 246);
                btn.FlatAppearance.MouseDownBackColor = UI.IsDark ? Color.FromArgb(55, 58, 62) : Color.FromArgb(235, 237, 240);
            }
            btn.Invalidate();
        }
        else if (c is Label)
        {
            Label lbl = (Label)c;
            if ((lbl.Tag as string) == "accent") lbl.ForeColor = UI.Accent;
            else if (lbl.Parent != head && lbl.ForeColor != Color.White)
            {
                Color fc = lbl.ForeColor;
                if (fc == Color.FromArgb(28, 29, 31) || fc == Color.FromArgb(245, 246, 247) || fc == Color.Black || fc == SystemColors.ControlText || fc.ToArgb() == Color.Black.ToArgb())
                {
                    lbl.ForeColor = UI.Text;
                }
                else if (fc == Color.FromArgb(95, 99, 104) || fc == Color.FromArgb(156, 163, 175) || fc == Color.Gray)
                {
                    lbl.ForeColor = UI.Muted;
                }
            }
        }
        else if (c is Panel)
        {
            Color bg = c.BackColor;
            if (bg == Color.FromArgb(249, 249, 251) || bg == Color.FromArgb(24, 25, 26))
            {
                c.BackColor = UI.Bg;
            }
            else if (bg == Color.White || bg == Color.FromArgb(36, 37, 38))
            {
                c.BackColor = UI.Panel;
            }
            else if (bg == Color.FromArgb(229, 231, 235) || bg == Color.FromArgb(58, 59, 60))
            {
                c.BackColor = UI.Line;
            }
            if (((Panel)c).AutoScroll && c.IsHandleCreated)
            {
                try { SetWindowTheme(c.Handle, UI.IsDark ? "DarkMode_Explorer" : "Explorer", null); } catch { }
            }
        }

        foreach (Control child in c.Controls)
        {
            ApplyThemeRecursive(child);
        }
    }

    // DoubleBuffered ist bei Panels und ListViews geschützt; per Reflexion für alle Unterelemente setzen
    static void EnableDoubleBuffer(Control c)
    {
        if (c == null) return;
        if (!(c is TextBoxBase))
        {
            try { PropertyInfo p = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance); if (p != null) p.SetValue(c, true, null); } catch { }
        }
        foreach (Control k in c.Controls) EnableDoubleBuffer(k);
    }
}
