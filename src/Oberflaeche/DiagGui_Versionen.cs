// Seite Info und Versionen (Teilklasse DiagGui)
public partial class DiagGui
{
    Control BuildVersionPage()
    {
        Panel f = new Panel(); f.Dock = DockStyle.Fill; f.BackColor = UI.Panel;
        FlowLayoutPanel top = Page("Info und Versionen", "Leos Minibench " + this.version + " auf " + Environment.MachineName + ", " + OsName() + ". Darunter, was sich von Version zu Version geändert hat, neueste zuerst; ausführlich in den Änderungsdateien im Ordner Doku des Projekts (Änderungen_vX.Y.txt).", -1);
        top.Dock = DockStyle.Top;
        top.AutoSize = true;
        top.Padding = new Padding(0, 0, 0, UI.S(10));
        CheckBox chkAutoUpdate = Chk("Beim Programmstart automatisch nach Minibench-Updates suchen", LoadUpdateCheckPreference());
        Tip(chkAutoUpdate, "Pr\u00FCft beim Start leise im Hintergrund \u00FCber GitHub Releases, ob eine neuere Version verf\u00FCgbar ist (ohne Telemetrie).");
        chkAutoUpdate.Margin = new Padding(UI.S(4), UI.S(4), UI.S(4), UI.S(8));
        chkAutoUpdate.CheckedChanged += delegate {
            SaveUpdateCheckPreference(chkAutoUpdate.Checked);
        };
        top.Controls.Add(chkAutoUpdate);
        RichTextBox rt = new RichTextBox(); rt.ReadOnly = true; rt.BorderStyle = BorderStyle.FixedSingle; rt.BackColor = UI.Panel; rt.ForeColor = UI.Text;
        rt.Dock = DockStyle.Fill; rt.DetectUrls = false; rt.ScrollBars = RichTextBoxScrollBars.Vertical; rt.Font = new Font("Segoe UI", 9.75f);
        Font fh = new Font("Segoe UI Semibold", 11f), fd = new Font("Segoe UI", 9f), fb = new Font("Segoe UI", 9.75f);
        foreach (Versionshistorie.Eintrag e in Versionshistorie.Liste)
        {
            rt.SelectionFont = fh; rt.SelectionColor = UI.Text;
            rt.AppendText("Version " + e.Version + (e.Version == this.version ? "  (diese Version)" : "") + "\r\n");
            rt.SelectionFont = fd; rt.SelectionColor = UI.Muted;
            rt.AppendText(e.Datum + "   \u00B7   " + e.Titel + "\r\n\r\n");
            rt.SelectionFont = fb; rt.SelectionColor = UI.Text;
            rt.AppendText(e.Text + "\r\n\r\n\r\n");
        }
        rt.SelectionStart = 0; rt.ScrollToCaret();
        f.Controls.Add(rt);
        f.Controls.Add(top);
        top.SendToBack();
        return f;
    }
}
