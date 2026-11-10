// Seite Benchmark, Referenz und Grafikeinheiten (Teilklasse DiagGui)
public partial class DiagGui
{
    // ab v3.64: Messungen mit kurzer Bezeichnung und Detailzeile, Laufwerke mit Ziel und freiem Platz, Referenz-Zustand neben der Liste
    static readonly string[] benchDetail = new string[] {
        "Einzel- und Mehrkern, AES-256, SHA-256, Kompression",
        "Lesen, Schreiben, Kopieren, Latenz",
        "Rendertest je Grafikeinheit (Bilder/s, 1-%-Low, Punktzahl), Speicherdurchsatz, Desktop-Komposition, PCIe",
        "sequentiell und 4K-Zufallszugriff, Auswahl unten" };

    Control BuildBenchPage()
    {
        FlowLayoutPanel f = Page("Benchmark", "Misst die Leistung der gewählten Komponenten und vergleicht sie mit der Referenz, mit früheren Läufen auf diesem PC und mit bereits geprüften Systemen.", 2);
        f.Controls.Add(Section("Messungen"));
        benchChk = new CheckBox[benchKeys.Length];
        for (int i = 0; i < benchKeys.Length; i++)
        {
            benchChk[i] = Chk(benchText[i], true);
            benchChk[i].CheckedChanged += delegate { clbDisks.Enabled = benchChk[3].Checked; UpdateSummary(); };
            f.Controls.Add(Opt2(benchChk[i], benchDetail[i]));
        }
        // ab v4.03 direkt unter den Messungen: Die Referenz bestimmt alle Prozentwerte der Ergebnisseite und stand vorher
        // ganz unten außerhalb des Sichtbereichs
        f.Controls.Add(Section("Referenz"));
        FlowLayoutPanel r2 = Row(); r2.Controls.Add(RowLabel("Referenz (100 %)", 170));
        cmbRef = Combo(360, new string[0], 0); r2.Controls.Add(cmbRef);
        btnUpdateMed = UI.Secondary("Median aktualisieren");
        btnUpdateMed.Click += delegate {
            if (running) return;
            // ab v4.06: Ergebnis aus @@RESULT und den Zeilen des Arbeitsprozesses. Bis v4.05 suchte die Oberfläche eine eigene
            // Statuszeile, die nie gesendet wurde, und meldete immer "Aktualisierung fehlgeschlagen", auch wenn die
            // Dateien geschrieben waren.
            List<string> lines;
            string res = RunHelperPumped("-MedianAktualisieren", out lines, 180);
            MessageBox.Show(this, MedianErgebnisText(res, lines), "Median-Referenz", MessageBoxButtons.OK, res.Length > 0 && !res.StartsWith("Keine") ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            FillRefLists(); UpdateRefState();
        };
        r2.Controls.Add(btnUpdateMed);
        f.Controls.Add(r2);
        lblRefState = Hint("", 180); f.Controls.Add(lblRefState);
        cmbRef.SelectedIndexChanged += delegate { UpdateRefState(); };
        // ab v4.04 ohne Haken "Dieses System als Referenz festlegen": Das geht in der Datenbank über Aktionen, wo der Lauf
        // mit seinen Werten sichtbar ist (Skriptparameter -ReferenzSpeichern bleibt für Aufrufe ohne Oberfläche)
        f.Controls.Add(Hint("Einen bestimmten Lauf als Referenz festlegen: Seite Datenbank, Lauf markieren, Aktionen \u203A Als Referenz festlegen.", 180));

        f.Controls.Add(Section("Laufwerke"));
        clbDisks = new CheckedListBox(); clbDisks.CheckOnClick = true; clbDisks.Width = UI.S(UI.Gruppenbreite); clbDisks.BorderStyle = BorderStyle.FixedSingle; clbDisks.IntegralHeight = false; clbDisks.Margin = new Padding(UI.S(4), UI.S(2), 0, UI.S(4)); clbDisks.BackColor = UI.Panel; clbDisks.ForeColor = UI.Text;
        foreach (string d in disks)
        {
            string[] x = d.Split('|');
            if (x.Length < 7) continue;
            clbDisks.Items.Add(DiskLine(x), x[6] != "1" && x[5].Length > 0);
            diskNums.Add(x[0]);
        }
        // Höhe nach Anzahl der Einträge (vorher mindestens 48 px, ein Laufwerk stand in einem fast leeren Feld)
        clbDisks.Height = Math.Max(1, Math.Min(8, clbDisks.Items.Count)) * UI.S(21) + UI.S(6);
        clbDisks.ItemCheck += delegate { BeginInvoke(new System.Windows.Forms.MethodInvoker(UpdateSummary)); };
        f.Controls.Add(clbDisks);
        f.Controls.Add(Hint("Je Laufwerk wird mit einer Testdatei auf dessen Volume mit dem meisten freien Platz gemessen (1 GB, im Kurzlauf 256 MB, danach gelöscht). USB-Datenträger sind zunächst abgewählt.", 4));
        f.Controls.Add(Section("Rendertest (Grafik)"));
        FlowLayoutPanel rg0 = Row(); rg0.Controls.Add(RowLabel("Grafikeinheit", 170));
        cmbGpuSel = Combo(420, GpuChoiceTexts(), 0); rg0.Controls.Add(cmbGpuSel); f.Controls.Add(rg0);
        FlowLayoutPanel rg = Row(); rg.Controls.Add(RowLabel("Auflösung", 170));
        cmbGpuRes = Combo(140, new string[] { "1280x720", "1920x1080" }, 0); rg.Controls.Add(cmbGpuRes);
        rg.Controls.Add(RowLabel("Anzeige", 70)); cmbGpuShow = Combo(170, new string[] { "Fenster", "Vollbild", "ohne Anzeige" }, 0); rg.Controls.Add(cmbGpuShow);
        f.Controls.Add(rg);
        f.Controls.Add(Hint("Ergebnis: Ø Bilder/s, 1-%-Low und Punktzahl je Grafikeinheit. Esc im Fenster beendet den Test.", 4));

        clbCompare = new CheckedListBox(); clbCompare.BackColor = UI.Panel; clbCompare.ForeColor = UI.Text;
        FillRefLists();
        return f;
    }

    // Laufwerkszeile: Nummer, Name, Bus, Medium, Größe und je Laufwerksbuchstabe der freie Platz; das Ziel der Messung
    // ist das Volume mit dem meisten freien Platz
    static string DiskLine(string[] x)
    {
        string head = String.Format("Datenträger {0}:  {1}   ({2}, {3}, {4})", x[0], x[1], x[2], x[3], x[4]);
        if (x[5].Length == 0) return head + "   ·   kein Laufwerksbuchstabe";
        List<string> vols = new List<string>(); string best = ""; long bestFree = -1;
        foreach (string l in x[5].Split(','))
        {
            string t = l.Trim(); if (t.Length == 0) continue;
            long free = -1;
            // USB-Datenträger nicht abfragen (ein schlafendes Laufwerk verzögerte sonst den Start)
            if (x.Length < 7 || x[6] != "1") try { DriveInfo di = new DriveInfo(t.Substring(0, 1)); if (di.IsReady) free = di.AvailableFreeSpace; } catch { }
            vols.Add(free >= 0 ? t + " " + GbText(free) + " frei" : t);
            if (free > bestFree) { bestFree = free; best = t; }
        }
        return head + "   ·   " + String.Join(", ", vols.ToArray()) + (vols.Count > 1 && best.Length > 0 ? "   ·   gemessen auf " + best : "");
    }

    static string GbText(long bytes)
    {
        double gb = bytes / 1073741824.0;
        return gb >= 1024 ? (gb / 1024).ToString("0.0", CultureInfo.GetCultureInfo("de-DE")) + " TB" : gb.ToString("0", CultureInfo.GetCultureInfo("de-DE")) + " GB";
    }

    public static string MedianErgebnisText(string result, List<string> lines)
    {
        StringBuilder b = new StringBuilder();
        b.Append(String.IsNullOrEmpty(result) ? "Der Arbeitsprozess hat kein Ergebnis gemeldet; die Median-Referenzen sind unverändert." : result + ".");
        if (lines != null && lines.Count > 0) { b.Append("\r\n\r\n"); b.Append(String.Join("\r\n", lines.ToArray())); }
        return b.ToString();
    }

    // Name, Datum und Rechner einer Referenzdatei im Datenordner; leer, wenn sie fehlt oder unlesbar ist
    string[] RefDatei(string name)
    {
        try
        {
            string f = dataDir.Length > 0 ? Path.Combine(dataDir, name) : "";
            if (f.Length == 0 || !File.Exists(f)) return null;
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> d = js.DeserializeObject(File.ReadAllText(f, Encoding.UTF8)) as Dictionary<string, object>;
            if (d == null) return null;
            object n, dt, c; string dat = d.TryGetValue("Datum", out dt) ? Convert.ToString(dt) : "";
            DateTime x; if (DateTime.TryParseExact(dat.Length >= 10 ? dat.Substring(0, 10) : dat, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out x)) dat = x.ToString("dd.MM.yyyy");
            return new string[] { d.TryGetValue("Name", out n) ? Convert.ToString(n) : name, dat, d.TryGetValue("Computer", out c) ? Convert.ToString(c) : "" };
        }
        catch { return null; }
    }

    void UpdateRefState()
    {
        if (lblRefState == null || cmbRef == null) return;
        int i = cmbRef.SelectedIndex;
        string t; bool warn = false;
        if (i == 0)
        {
            string st = SavedRefText();
            warn = st.Length == 0;
            t = warn ? "Keine Referenz festgelegt: Im Datenordner liegt weder eine eigene Referenz (Referenz.json) noch ein Median. Prozentwerte beziehen sich dann auf das eingebaute Referenzprofil." : st;
        }
        else if (i >= 1 && i <= 3)
        {
            // ab v4.06: zeigt, welcher Median gilt und von wann (vorher nur die Anzahl der Läufe in der Datenbank)
            string datei = i == 1 ? "Referenz_Median.json" : i == 2 ? "Referenz_Notebook.json" : "Referenz_Desktop.json";
            string[] r = RefDatei(datei);
            int n = db.FindAll(delegate(DbEntry e) { return e.HasBench; }).Count;
            t = r != null ? r[0] + (r[1].Length > 0 ? ", berechnet am " + r[1] : "") + " (" + datei + "). \u201EMedian aktualisieren\u201C berechnet ihn aus den " + n + " Läufen mit Benchmark neu."
                          : "Noch nicht gespeichert: Der Lauf bildet den Median aus der Datenbank (" + n + " Läufe mit Benchmark). \u201EMedian aktualisieren\u201C speichert ihn als " + datei + ".";
        }
        else t = "Einzelner gespeicherter Lauf als Bezug.";
        SetText(lblRefState, t);
        lblRefState.ForeColor = warn ? UI.Warn : UI.Muted;
    }

    void FillRefLists()
    {
        int prevSpecial = -1;
        string prevRef = "";
        if (cmbRef != null)
        {
            if (cmbRef.SelectedIndex >= 0 && cmbRef.SelectedIndex <= 3) prevSpecial = cmbRef.SelectedIndex;
            else if (cmbRef.SelectedIndex >= 4 && cmbRef.SelectedIndex - 4 < refItems.Count) prevRef = refItems[cmbRef.SelectedIndex - 4].Path;
            cmbRef.Items.Clear();
            cmbRef.Items.Add("Gespeicherte Referenz");
            cmbRef.Items.Add("Median aller Systeme");
            cmbRef.Items.Add("Median der Notebooks");
            cmbRef.Items.Add("Median der Desktop-PCs");
        }
        refItems.Clear();
        HashSet<string> prevCmp = new HashSet<string>();
        if (clbCompare != null)
        {
            for (int i = 0; i < clbCompare.Items.Count; i++) if (clbCompare.GetItemChecked(i) && i < cmpItems.Count) prevCmp.Add(cmpItems[i].Path);
            clbCompare.Items.Clear(); cmpItems.Clear();
        }
        foreach (DbEntry e in db)
        {
            if (!e.HasBench) continue;
            refItems.Add(e);
            if (cmbRef != null) cmbRef.Items.Add(e.Label);
            if (clbCompare != null) { cmpItems.Add(e); clbCompare.Items.Add(e.Label, prevCmp.Contains(e.Path)); }
        }
        if (cmbRef != null)
        {
            int sel = 0;
            if (prevSpecial >= 0 && prevSpecial < cmbRef.Items.Count) sel = prevSpecial;
            else if (prevRef.Length > 0)
            {
                int idx = refItems.FindIndex(delegate(DbEntry x) { return String.Equals(x.Path, prevRef, StringComparison.OrdinalIgnoreCase); });
                if (idx >= 0) sel = idx + 4;
            }
            if (sel >= 0 && sel < cmbRef.Items.Count) cmbRef.SelectedIndex = sel;
            UpdateRefState();
        }
        if (clbCompare != null)
        {
            if (clbCompare.Items.Count == 0) { clbCompare.Items.Add("(noch keine Systeme mit Benchmark in der Datenbank)"); clbCompare.Enabled = false; }
            else clbCompare.Enabled = true;
            clbCompare.Height = Math.Max(UI.S(48), Math.Min(8, clbCompare.Items.Count) * UI.S(21) + UI.S(6));
        }
    }


    // ------------------------------------------------------------ Grafikeinheiten (ab v2.65)
    static string GpuKind(string n)
    {
        n = (n ?? "").Trim();
        if (n.Length == 0) return "";
        if (System.Text.RegularExpressions.Regex.IsMatch(n, "Virtual|Remote|Indirect|Parsec|spacedesk|IddSampleDriver|Basic Display|Basic Render|Standard-VGA|Hyper-V|VMware|VirtualBox|Citrix|DisplayLink", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return "virtuell";
        if (System.Text.RegularExpressions.Regex.IsMatch(n, "NVIDIA|GeForce|Quadro|Tesla|\\bRTX\\b|\\bGTX\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return "dGPU";
        if (n.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0) return System.Text.RegularExpressions.Regex.IsMatch(n, "Arc(\\(TM\\))?\\s+(Pro\\s+)?[AB]\\d{2,3}", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ? "dGPU" : "iGPU";
        if (System.Text.RegularExpressions.Regex.IsMatch(n, "AMD|ATI|Radeon|FirePro", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return System.Text.RegularExpressions.Regex.IsMatch(n, "Graphics\\s*$|\\b\\d{3}M\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ? "iGPU" : "dGPU";
        return "";
    }

    // Einträge der Auswahl: alle, nur Grafikkarte, nur Prozessorgrafik (wenn vorhanden), dann jede Einheit einzeln
    public static string GetPrimaryGpuName()
    {
        if (GpuNames == null || GpuNames.Length == 0) return "";
        string dGpu = "";
        string iGpu = "";
        string other = "";
        foreach (string g in GpuNames)
        {
            string n = (g ?? "").Trim();
            if (n.Length == 0) continue;
            string kind = GpuKind(n);
            if (kind == "virtuell") continue;
            if (kind == "dGPU" && dGpu.Length == 0) dGpu = n;
            else if (kind == "iGPU" && iGpu.Length == 0) iGpu = n;
            else if (other.Length == 0) other = n;
        }
        if (dGpu.Length > 0) return dGpu;
        if (iGpu.Length > 0) return iGpu;
        if (other.Length > 0) return other;
        foreach (string g in GpuNames)
        {
            string n = (g ?? "").Trim();
            if (n.Length > 0) return n;
        }
        return "";
    }

    string[] GpuChoiceTexts()
    {
        List<string> t = new List<string>(); gpuSelValues.Clear();
        t.Add("alle Grafikeinheiten"); gpuSelValues.Add("Alle");
        List<string> names = new List<string>();
        foreach (string g in GpuNames ?? new string[0]) { string n = (g ?? "").Trim(); if (n.Length > 0 && GpuKind(n) != "virtuell" && !names.Contains(n)) names.Add(n); }
        bool dg = names.Exists(delegate(string n) { return GpuKind(n) == "dGPU"; }), ig = names.Exists(delegate(string n) { return GpuKind(n) == "iGPU"; });
        if (dg && ig) { t.Add("nur Grafikkarte"); gpuSelValues.Add("Grafikkarte"); t.Add("nur Prozessorgrafik"); gpuSelValues.Add("Prozessorgrafik"); }
        if (names.Count > 1) foreach (string n in names) { t.Add("nur " + n); gpuSelValues.Add("Name:" + n); }
        return t.ToArray();
    }

    string GpuChoiceValue(ComboBox c)
    {
        if (c == null || c.SelectedIndex < 0 || c.SelectedIndex >= gpuSelValues.Count) return "Alle";
        return gpuSelValues[c.SelectedIndex];
    }

    void SetGpuChoice(string v)
    {
        int i = gpuSelValues.IndexOf(v ?? "");
        if (i < 0 && (v ?? "").StartsWith("Name:")) i = gpuSelValues.FindIndex(delegate(string x) { return x.StartsWith("Name:") && x.Substring(5).IndexOf(v.Substring(5), StringComparison.OrdinalIgnoreCase) >= 0; });
        if (i < 0) i = 0;
        if (cmbGpuSel != null && i < cmbGpuSel.Items.Count) cmbGpuSel.SelectedIndex = i;
        if (cmbLGpuSel != null && i < cmbLGpuSel.Items.Count) cmbLGpuSel.SelectedIndex = i;
    }

    // Referenz (ab v2.65 nur noch die mit "Dieses System als Referenz festlegen" gespeicherte, keine eingebauten Werte)
    // Text zur gespeicherten Referenz in derselben Reihenfolge wie Get-SavedReference (ab v4.06): eigene Referenz
    // (Referenz.json, kein alter Median), sonst der Median der Geräteklasse, sonst der andere, sonst die alte Ablage
    string SavedRefText()
    {
        string[] r = RefDatei("Referenz.json");
        if (r != null && r[2] != "Median") return "Eigene Referenz: " + r[0] + (r[1].Length > 0 ? " vom " + r[1] : "") + " (Referenz.json, festgelegt in der Datenbank).";
        string dev = HwIsLaptop ? "Referenz_Notebook.json" : "Referenz_Desktop.json", alt = HwIsLaptop ? "Referenz_Desktop.json" : "Referenz_Notebook.json";
        foreach (string f in new string[] { dev, alt, "PC-Diagnose-Referenz.json" })
        {
            string[] m = RefDatei(f);
            if (m != null) return "Keine eigene Referenz festgelegt, es gilt " + m[0] + (m[1].Length > 0 ? " vom " + m[1] : "") + " (" + f + "). Eigene Referenz: Datenbank, Lauf markieren, Aktionen \u203A Als Referenz festlegen.";
        }
        return "";
    }
}
