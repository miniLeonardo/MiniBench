// Selbsttest der Oberfläche (ab v3.53). Wird von tests/Hilfen.ps1 zusammen mit den Dateien aus src/Oberflaeche zu einer
// eigenen exe übersetzt (keine using-Zeilen: es gelten die von DiagGui.cs am Anfang derselben Übersetzungseinheit).
// Ausgabe je Fall eine Zeile: OK|Name oder FEHL|Name|Grund. Argument 1: Projektordner (für Testdaten aus src).
public static class GuiSelbsttest
{
    static System.Collections.Generic.List<string> outLines = new System.Collections.Generic.List<string>();
    static string root = "";

    [System.STAThread]
    public static int Main(string[] args)
    {
        root = args.Length > 0 ? args[0] : "";
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
        Fall("UI.Skalierung", UiSkalierung);
        Fall("UI.Farbschema", UiFarbschema);
        Fall("ToggleSwitch", ToggleSchalter);
        Fall("FluentCard", FluentKarte);
        Fall("Hinweise.Umbruch", HinweisUmbruch);
        Fall("Grafikauswahl.Abgleich", GrafikAbgleich);
        Fall("Softwarepakete.Auswahl", SoftwareAuswahl);
        Fall("Softwarepakete.Ereignisse", SoftwareEreignisse);
        Fall("Datenbank.Referenzen", DatenbankReferenzen);
        Fall("Aenderungen.Softwarepaket", AenderungSoftwarepaket);
        Fall("Netzlaufwerk.Pfad", NasPfad);
        Fall("Netzlaufwerk.Konfiguration", NasKonfiguration);
        Fall("Netzlaufwerk.Ergebnis", NasErgebnis);
        Fall("Wartung.Vertragszeile", WartungVertragszeile);
        Fall("Dialog.Farbschema", DialogFarbschema);
        Fall("Oberflaeche.Kontrast", Kontrast);
        Fall("Startknopf.Beschriftung", StartknopfBeschriftung);
        Fall("Zeitangabe.Minuten", Zeitangabe);
        Fall("Lasttest.Gesamtdauer", LasttestGesamtdauer);
        Fall("Lasttest.Deaktivierung", LasttestDeaktivierung);
        Fall("Startuebersicht", Startuebersicht);
        Fall("Wartung.Detailtext", WartungDetailtext);
        Fall("Voreinstellung.Altwerte", VoreinstellungAltwerte);
        Fall("Laufwerk.Kurzname", LaufwerkKurzname);
        Fall("Ablage.Kurzform", AblageKurzform);
        Fall("Etikett.Risikostufe", EtikettRisikostufe);
        Fall("Aufklappgruppe", Aufklappgruppe);
        Fall("Diagnose.Profile", DiagnoseProfile);
        Fall("Etikett.Schreibweise", EtikettSchreibweise);
        Fall("Karte", KarteRahmen);
        Fall("StartseitenElemente", StartseitenElemente);
        Fall("UI.VersionNumber", UiVersionsvergleich);
        Fall("SettingRow.Dynamik", SettingRowDynamik);
        Fall("CategoryPickerItem.Kurztitel", CategoryPickerKurztitel);
        Fall("UI.SkalierungDpi", UiSkalierungDpi);
        Fall("UI.IconByName", UiSymbolNachName);
        Fall("Update.Versionsvergleich", UpdateVersionsvergleich);
        Fall("Ergebnis.Einordnung", ErgebnisEinordnung);
        Fall("Ergebnis.Laufwerkspillen", ErgebnisLaufwerkspillen);
        Fall("Optimierung.Sperre", OptimierungSperre);
        Fall("Optimierung.Zaehler", OptimierungZaehler);
        Fall("Tools.Platzhalter", ToolsPlatzhalter);
        Fall("Tools.Raster", ToolsRaster);
        Fall("Softwarepakete.Liste", SoftwareListe);
        Fall("Softwarepakete.Installiert", SoftwareInstalliert);
        Fall("Update.Release", UpdateRelease);
        Fall("Referenz.Median", ReferenzMedian);
        Fall("Akzent.Farben", AkzentFarben);
        Fall("Akzent.Windows", AkzentWindows);
        Fall("Bewegung.Zeitgeber", BewegungZeitgeber);
        Fall("Bewegung.Abgeschaltet", BewegungAbgeschaltet);
        Fall("Ringanzeige", RingBogen);
        Fall("Ergebnis.Ordner", ErgebnisOrdner);
        Fall("Einstellungen.Werte", EinstellungenWerte);
        Fall("Modulseite.Rahmen", ModulseiteRahmen);
        Fall("Laufwerkszeile", LaufwerkszeileSchalter);
        Fall("Umfang.Auswahl", UmfangAuswahl);
        Fall("Umbruch.Hoehe", UmbruchHoehe);
        Fall("Ergebnis.Befundzeile", ErgebnisBefundzeile);
        Fall("Benchmark.Seite", BenchmarkSeite);
        Fall("Diagnose.Seite", DiagnoseSeite);
        Fall("Ergebnis.Seitenleiste", ErgebnisSeitenleiste);
        Fall("Ergebnis.Datenbank", ErgebnisDatenbank);
        foreach (string l in outLines) System.Console.WriteLine(l);
        return 0;
    }

    delegate void Pruefung();
    static void Fall(string name, Pruefung p)
    {
        try { p(); outLines.Add("OK|" + name); }
        catch (System.Exception ex) { outLines.Add("FEHL|" + name + "|" + ex.Message.Replace("\r", " ").Replace("\n", " ")); }
    }
    static void Ist(bool bedingung, string text) { if (!bedingung) throw new System.Exception(text); }

    static void UiSkalierung()
    {
        float alt = UI.DpiScale;
        try
        {
            UI.DpiScale = 1.0f; Ist(UI.S(100) == 100 && UI.SF(50f) == 50f, "100 %");
            UI.DpiScale = 1.5f; Ist(UI.S(100) == 150 && UI.S(74) == 111 && UI.SF(10f) == 15f, "150 %");
            UI.DpiScale = 2.0f; Ist(UI.S(100) == 200 && UI.SF(10f) == 20f, "200 %");
        }
        finally { UI.DpiScale = alt; }
    }

    static void UiFarbschema()
    {
        UI.SetTheme(false);
        Ist(!UI.IsDark && UI.Bg.GetBrightness() > 0.8f && UI.Text.GetBrightness() < 0.3f, "hell: heller Grund, dunkle Schrift");
        UI.SetTheme(true);
        Ist(UI.IsDark && UI.Bg.GetBrightness() < 0.3f && UI.Text.GetBrightness() > 0.8f, "dunkel: dunkler Grund, helle Schrift");
        UI.SetTheme(false);
    }

    static void ToggleSchalter()
    {
        using (ToggleSwitch t = new ToggleSwitch())
        {
            int n = 0; t.CheckedChanged += delegate { n++; };
            Ist(!t.Checked, "Vorgabe aus");
            t.Checked = true; Ist(t.Checked && n == 1, "Einschalten meldet CheckedChanged");
            t.Checked = true; Ist(n == 1, "gleicher Wert meldet nichts");
        }
    }

    static void FluentKarte()
    {
        using (FluentCard c = new FluentCard("Testkarte", "Beschreibung", UI.IcoDiag))
        {
            int n = 0; c.CheckedChanged += delegate { n++; };
            Ist(c.Title == "Testkarte" && c.Description == "Beschreibung" && !c.Checked, "Titel, Text, Vorgabe aus");
            c.Checked = true; Ist(c.Checked && n == 1, "Haken meldet CheckedChanged");
        }
    }

    static void HinweisUmbruch()
    {
        string lang = "";
        for (int i = 0; i < 60; i++) lang += (i > 0 ? " " : "") + "wort";
        string w = DiagGui.WrapTip(lang + "\r\nzweiter Absatz");
        foreach (string l in w.Split(new string[] { "\r\n" }, System.StringSplitOptions.None)) Ist(l.Length <= 90, "Zeile über 90 Zeichen: " + l.Length);
        Ist(w.EndsWith("\r\nzweiter Absatz"), "vorhandener Umbruch bleibt");
        Ist(DiagGui.WrapTip("") == "" && DiagGui.WrapTip(null) == "", "leer bleibt leer");
    }

    static ComboBox Box(int n, int sel)
    {
        ComboBox c = new ComboBox();
        for (int i = 0; i < n; i++) c.Items.Add("Eintrag " + i);
        c.SelectedIndex = sel;
        return c;
    }

    // Benchmark und Lasttest halten Auflösung, Anzeige und Grafikeinheit gleich, in beide Richtungen (Fehler bis 3.52:
    // die Wahl auf der Seite Lasttest sprang auf den Wert des Benchmarks zurück)
    static void GrafikAbgleich()
    {
        ComboBox[] bench = new ComboBox[] { Box(2, 0), Box(3, 0), Box(3, 0) };
        ComboBox[] last = new ComboBox[] { Box(2, 0), Box(3, 0), Box(3, 0) };
        last[2].SelectedIndex = 2;
        DiagGui.CopySelection(last, bench);
        Ist(bench[2].SelectedIndex == 2, "Lasttest -> Benchmark: Grafikeinheit übernommen");
        Ist(last[2].SelectedIndex == 2, "Lasttest behält seine Wahl");
        bench[0].SelectedIndex = 1;
        DiagGui.CopySelection(bench, last);
        Ist(last[0].SelectedIndex == 1 && last[2].SelectedIndex == 2, "Benchmark -> Lasttest: Auflösung übernommen, Grafikeinheit gleich");
        // fehlende Liste (keine Grafikauswahl) und Index außerhalb der Zielliste bleiben ohne Wirkung
        ComboBox[] kurz = new ComboBox[] { Box(2, 1), null, Box(1, 0) };
        DiagGui.CopySelection(last, kurz);
        Ist(kurz[0].SelectedIndex == 1, "erste Liste übernommen");
        Ist(kurz[2].SelectedIndex == 0, "ungültiger Index wird nicht übernommen");
        DiagGui.CopySelection(null, bench); DiagGui.CopySelection(bench, null);
    }

    static void SoftwareAuswahl()
    {
        string[] ids = new string[] { "Google.Chrome", "7zip.7zip", "VideoLAN.VLC" };
        string[] names = new string[] { "Google Chrome", "7-Zip; \"x\"=y", "VLC" };
        string spec = DiagGui.WingetSpec(new System.Collections.Generic.List<int>(new int[] { 0, 1, 7 }), ids, names);
        Ist(spec == "Google.Chrome=Google Chrome;7zip.7zip=7-Zip, 'x'-y", "Spezifikation: " + spec);
        // je Paket list (120 s) + install (1200 s) + list (120 s) im Arbeitsprozess
        Ist(DiagGui.WingetTimeoutSec(3) >= 3 * (120 + 1200 + 120) + 60, "Frist der Oberfläche liegt über den Fristen des Arbeitsprozesses");
    }

    static void SoftwareEreignisse()
    {
        string st, tx;
        Ist(DiagGui.ParseWingetEvent("@@PAKET|1|2|Google.Chrome|installiert|Google Chrome installiert¦ok", out st, out tx), "Paketereignis erkannt");
        Ist(st == "installiert" && tx == "Google Chrome installiert|ok", "Status und Text: " + st + " / " + tx);
        Ist(!DiagGui.ParseWingetEvent("@@RESULT|1|0|0", out st, out tx), "anderes Ereignis wird nicht gelesen");
        Ist(!DiagGui.ParseWingetEvent("@@PAKET|1|2", out st, out tx), "unvollständige Zeile wird nicht gelesen");
    }

    static void DatenbankReferenzen()
    {
        string dir = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(root, "src"), "Daten"), "Referenzen");
        System.Collections.Generic.List<DbEntry> l = DbEntry.Load(dir);
        Ist(l.Count >= 5, "mindestens 5 Referenzprofile, gelesen: " + l.Count);
        foreach (DbEntry e in l)
        {
            Ist(e.Name.Length > 0 && e.Computer.Length > 0 && e.Cpu.Length > 0 && e.Werte.Count > 0, "Profil vollständig: " + e.Path);
            Ist(e.Kerne > 0 && e.Threads > 0, "Kerne und Threads vorhanden: " + e.Path);
        }
    }

    // Ein Eintrag, wie ihn Install-SoftwarePaket schreibt, erscheint auf der Seite Änderungen und ist rückgängig machbar
    static void AenderungSoftwarepaket()
    {
        string dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MinibenchSelbsttest_" + System.Guid.NewGuid().ToString("N"));
        string pc = System.IO.Path.Combine(dir, "PC");
        System.IO.Directory.CreateDirectory(pc);
        try
        {
            string json = "{\"Format\":\"Minibench-Aenderungen/1\",\"Computer\":\"" + System.Environment.MachineName + "\",\"GeraetId\":\"\",\"Version\":\"3.53\",\"Lauf\":\"2026-10-09 10:00:00\",\"Bericht\":\"\",\"Eintraege\":[" +
                "{\"Id\":1,\"Zeit\":\"2026-10-09 10:01:00\",\"Modul\":\"Tools\",\"Schritt\":\"Softwarepaket\",\"Titel\":\"7-Zip installiert (winget)\",\"Risiko\":\"Eingriff\",\"Art\":\"Softwarepaket\",\"Ziel\":\"7zip.7zip\",\"Vorher\":\"nicht installiert\",\"Nachher\":\"installiert\",\"Daten\":{\"PaketId\":\"7zip.7zip\",\"Name\":\"7-Zip\"},\"Gegenbefehl\":\"winget uninstall --id 7zip.7zip -e\",\"Status\":\"aktiv\",\"Ergebnis\":\"\"}]}";
            System.IO.File.WriteAllText(System.IO.Path.Combine(pc, "20261009_100000.json"), json, new System.Text.UTF8Encoding(false));
            System.Collections.Generic.List<ChangeEntry> l = ChangeEntry.Load(dir);
            Ist(l.Count == 1, "ein Eintrag gelesen: " + l.Count);
            Ist(l[0].Art == "Softwarepaket" && l[0].Risiko == "Eingriff" && l[0].Ziel == "7zip.7zip", "Art, Risiko, Ziel");
            Ist(l[0].Undoable, "auf diesem PC rückgängig machbar");
        }
        finally { try { System.IO.Directory.Delete(dir, true); } catch { } }
    }

    // ab v3.54: Netzlaufwerk als Spiegel (NasAblage in DiagGui_Modelle.cs)
    static void NasPfad()
    {
        Ist(NasAblage.PfadFehler(@"\\TRUENAS\Multimedia\MiniBench") == "", "UNC-Pfad gültig");
        Ist(NasAblage.PfadFehler(@"\\nas\freigabe") == "", "Freigabe ohne Unterordner gültig");
        Ist(NasAblage.PfadFehler(@"\TRUENAS\Multimedia\MiniBench").Contains("nur einem"), "ein führendes \\ wird erkannt (Ordner TRUENAS auf dem Stick)");
        Ist(NasAblage.PfadFehler(@"TRUENAS\Multimedia").Length > 0, "relativer Pfad abgelehnt");
        Ist(NasAblage.PfadFehler("").Length > 0 && NasAblage.PfadFehler(null).Length > 0, "leer abgelehnt");
        Ist(NasAblage.Freigabe(@"\\TRUENAS\Multimedia\MiniBench\Daten") == @"\\TRUENAS\Multimedia", "Freigabe aus dem Pfad");
        Ist(NasAblage.Freigabe(@"I:\TRUENAS") == "", "Laufwerk hat keine Freigabe");
        Ist(!NasAblage.Erreichbar(@"\TRUENAS\Multimedia", 200), "ungültiger Pfad gilt nie als erreichbar");
        Ist(!NasAblage.OrdnerBereit(@"\TRUENAS\Multimedia", 200), "ungültiger Pfad wird nie angelegt");
        Ist(NasAblage.Vereinheitlichen(@" \\nas\daten\ ") == @"\\nas\daten" && NasAblage.Vereinheitlichen(@"Z:\") == @"Z:\" && NasAblage.Vereinheitlichen("Z:") == @"Z:\", "Pfade vereinheitlichen (Laufwerksstamm bleibt Z:\\)");
    }

    static void NasKonfiguration()
    {
        string d = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mb_nas_" + System.Guid.NewGuid().ToString("N"));
        try
        {
            string p, b;
            Ist(!NasAblage.LeseKonfig(d, out p, out b), "ohne Netzwerk.json keine Konfiguration");
            Ist(NasAblage.AblageText(d) == "Datenordner: " + d, "ohne Netzlaufwerk nur der Datenordner");
            NasAblage.SchreibeKonfig(d, @"\\TRUENAS\Multimedia\MiniBench\", "leo");
            Ist(NasAblage.LeseKonfig(d, out p, out b) && p == @"\\TRUENAS\Multimedia\MiniBench" && b == "leo", "Pfad und Benutzer gelesen: " + p + " / " + b);
            string t = System.IO.File.ReadAllText(NasAblage.KonfigDatei(d));
            Ist(!t.ToLowerInvariant().Contains("kennwort") && !t.ToLowerInvariant().Contains("passw"), "kein Kennwort in Netzwerk.json");
            Ist(NasAblage.AblageText(d).Contains("noch nicht abgeglichen"), "noch nicht abgeglichen");
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(d, "Abgleich"));
            System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.Combine(d, "Abgleich"), "Stand.json"), "{\"Format\":\"Minibench-Abgleich/1\",\"Zeit\":\"2026-10-09 14:05:33\",\"Dateien\":[]}");
            Ist(NasAblage.LetzterAbgleich(d) == "09.10.2026 14:05", "Zeit des letzten Abgleichs: " + NasAblage.LetzterAbgleich(d));
            Ist(NasAblage.AblageText(d).Contains("zuletzt abgeglichen 09.10.2026 14:05"), "Zeile nennt den letzten Abgleich");
            // Schlüssel früherer Versionen
            System.IO.File.WriteAllText(NasAblage.KonfigDatei(d), "{\"NetzwerkPfad\":\"\\\\\\\\nas\\\\daten\"}");
            Ist(NasAblage.LeseKonfig(d, out p, out b) && p == @"\\nas\daten" && b == "", "Schlüssel NetzwerkPfad: " + p);
            NasAblage.LoescheKonfig(d);
            Ist(!System.IO.File.Exists(NasAblage.KonfigDatei(d)), "Netzwerk.json gelöscht");
        }
        finally { try { System.IO.Directory.Delete(d, true); } catch { } }
    }

    static void NasErgebnis()
    {
        bool ok, erreichbar;
        string t = NasAblage.ErgebnisText("1|3|2|1|0|0|1", out ok, out erreichbar);
        Ist(ok && erreichbar && t == "3 zum Netzlaufwerk, 2 auf den Stick, 1 gelöscht, 0 Konflikte, 0 Fehler", t);
        t = NasAblage.ErgebnisText("0|0|0|0|0|0|0", out ok, out erreichbar);
        Ist(!ok && !erreichbar && t.Contains("nicht erreichbar"), t);
        t = NasAblage.ErgebnisText("0|0|0|0|0|0|1|1", out ok, out erreichbar);
        Ist(!ok && erreichbar && NasAblage.Angehalten("0|0|0|0|0|0|1|1") && t.Contains("angehalten"), "angehalten: " + t);
        Ist(!NasAblage.Angehalten("1|3|2|1|0|0|1|0") && !NasAblage.Angehalten("1|3|2|1|0|0|1"), "nicht angehalten");
        t = NasAblage.ErgebnisText("", out ok, out erreichbar);
        Ist(!ok && t.StartsWith("Keine"), t);
    }

    static void WartungVertragszeile()
    {
        string alt = "S|Wartung|Temp|Massnahme|Eingriff|nie|keins|3|0|1|Temporäre Dateien löschen";
        string neu = "S|Wartung|Temp|Massnahme|Eingriff|nie|keins|3|0|1|Temporäre Dateien löschen|Bereinigung und Speicherplatz";
        string[] xAlt = alt.Split('|');
        string[] xNeu = neu.Split('|');
        ContractStep cAlt = new ContractStep();
        cAlt.Module = xAlt[1]; cAlt.Key = xAlt[2]; cAlt.Typ = xAlt[3]; cAlt.Risiko = xAlt[4]; cAlt.Neustart = xAlt[5]; cAlt.Rueckgaengig = xAlt[6];
        int mAlt; int.TryParse(xAlt[7], out mAlt); cAlt.Minuten = mAlt; cAlt.Vorauswahl = xAlt[8] == "1"; cAlt.Ueblich = xAlt[9] == "1"; cAlt.Text = xAlt[10];
        if (xAlt.Length >= 12) cAlt.Gruppe = xAlt[11];
        Ist(cAlt.Key == "Temp" && cAlt.Gruppe == "", "alt: 11 Felder, Gruppe leer");

        ContractStep cNeu = new ContractStep();
        cNeu.Module = xNeu[1]; cNeu.Key = xNeu[2]; cNeu.Typ = xNeu[3]; cNeu.Risiko = xNeu[4]; cNeu.Neustart = xNeu[5]; cNeu.Rueckgaengig = xNeu[6];
        int mNeu; int.TryParse(xNeu[7], out mNeu); cNeu.Minuten = mNeu; cNeu.Vorauswahl = xNeu[8] == "1"; cNeu.Ueblich = xNeu[9] == "1"; cNeu.Text = xNeu[10];
        if (xNeu.Length >= 12) cNeu.Gruppe = xNeu[11];
        Ist(cNeu.Key == "Temp" && cNeu.Gruppe == "Bereinigung und Speicherplatz", "neu: 12 Felder, Gruppe gelesen");
    }

    static void DialogFarbschema()
    {
        bool altDark = UI.IsDark;
        try
        {
            UI.SetTheme(true);
            using (Form dlg = new Form())
            {
                FlowLayoutPanel p = new FlowLayoutPanel();
                Label l = new Label(); l.Text = "Beispiel";
                CheckBox cb = new CheckBox(); cb.Text = "Option";
                RadioButton rb = new RadioButton(); rb.Text = "Wahl";
                TextBox tb = new TextBox(); tb.Text = "Eingabe";
                ComboBox cmb = new ComboBox(); cmb.Items.Add("Auswahl");
                LinkLabel ll = new LinkLabel(); ll.Text = "Verweis";
                Button btn = new Button(); btn.Text = "Schaltfläche";
                p.Controls.Add(l);
                p.Controls.Add(cb);
                p.Controls.Add(rb);
                p.Controls.Add(tb);
                p.Controls.Add(cmb);
                p.Controls.Add(ll);
                p.Controls.Add(btn);
                dlg.Controls.Add(p);
                UI.ThemeDialog(dlg);

                Ist(dlg.BackColor == UI.Bg, "Dialog BackColor gesetzt");
                Ist(dlg.ForeColor != System.Drawing.Color.Black && dlg.ForeColor != System.Drawing.SystemColors.ControlText, "Dialog ForeColor nicht schwarz");

                System.Collections.Generic.Queue<System.Windows.Forms.Control> q = new System.Collections.Generic.Queue<System.Windows.Forms.Control>();
                q.Enqueue(dlg);
                while (q.Count > 0)
                {
                    System.Windows.Forms.Control c = q.Dequeue();
                    if (c != dlg)
                    {
                        Ist(c.ForeColor != System.Drawing.Color.Black &&
                            c.ForeColor != System.Drawing.SystemColors.ControlText &&
                            c.ForeColor.ToArgb() != System.Drawing.Color.Black.ToArgb(),
                            c.GetType().Name + " hat im dunklen Schema keine schwarze Schrift (ist: " + c.ForeColor + ")");
                    }
                    foreach (System.Windows.Forms.Control kind in c.Controls) q.Enqueue(kind);
                }
            }
        }
        finally
        {
            UI.SetTheme(altDark);
        }
    }

    // ab v3.64: Schrift auf Akzentflächen und gedämpfte Hinweise erreichen in beiden Farbschemata mindestens 4,5:1
    // (bis 3.63 weiße Schrift auf #4CC2FF im Dunkelschema, etwa 2:1)
    static void Kontrast()
    {
        bool alt = UI.IsDark;
        try
        {
            foreach (bool dark in new bool[] { false, true })
            {
                UI.SetTheme(dark);
                string n = dark ? "dunkel: " : "hell: ";
                Ist(UI.ContrastRatio(UI.OnAccent, UI.Accent) >= 4.5, n + "Schrift auf Primärknopf " + UI.ContrastRatio(UI.OnAccent, UI.Accent).ToString("0.0"));
                Ist(UI.ContrastRatio(UI.Muted, UI.Panel) >= 4.5, n + "Hinweis auf Seite");
                Ist(UI.ContrastRatio(UI.Text, UI.Panel) >= 7, n + "Text auf Seite");
                Color fg, bg;
                foreach (string l in new string[] { "info", "warn", "crit", "ok", "skip" }) { UI.BadgeColors(l, out fg, out bg); Ist(UI.ContrastRatio(fg, bg) >= 4.5, n + "Etikett " + l); }
                using (Button b = UI.Primary("x")) Ist(b is PrimaryButton && b.ForeColor == UI.OnAccent && b.BackColor == UI.Accent, n + "UI.Primary nutzt OnAccent");
            }
            Ist(System.Math.Abs(UI.ContrastRatio(Color.White, Color.Black) - 21) < 0.01, "Weiß auf Schwarz 21:1");
        }
        finally { UI.SetTheme(alt); }
    }

    static void StartknopfBeschriftung()
    {
        Ist(DiagGui.StartButtonText(new System.Collections.Generic.List<string>()) == "Ablauf wählen", "kein Modul");
        Ist(DiagGui.StartButtonText(null) == "Ablauf wählen", "null");
        Ist(DiagGui.StartButtonText(new System.Collections.Generic.List<string>(new string[] { "Diagnose" })) == "Diagnose starten", "ein Modul");
        Ist(DiagGui.StartButtonText(new System.Collections.Generic.List<string>(new string[] { "Diagnose", "Lasttest", "Wartung" })) == "3 Module starten", "drei Module");
    }

    static void Zeitangabe()
    {
        Ist(DiagGui.MinText(45) == "45 Min.", "45");
        Ist(DiagGui.MinText(80) == "1 Std. 20 Min.", "80");
        Ist(DiagGui.MinText(120) == "2 Std.", "120");
        Ist(DiagGui.MinText(0) == "1 Min.", "mindestens 1");
    }

    // Komponenten laufen gleichzeitig: Gesamtdauer = längste gewählte Dauer
    static void LasttestGesamtdauer()
    {
        Ist(DiagGui.LoadTotalMinutes(new bool[] { true, false, true, false }, new int[] { 2, 60, 15, 5 }) == 15, "CPU 2 und GPU 15 ergibt 15");
        Ist(DiagGui.LoadTotalMinutes(new bool[] { false, false, false, false }, new int[] { 2, 2, 2, 2 }) == 0, "nichts gewählt");
        Ist(DiagGui.LoadTotalMinutes(new bool[] { true }, new int[] { 240 }) == 240, "eine Komponente");
        Ist(DiagGui.LoadTotalMinutes(null, null) == 0, "ohne Listen");
    }

    // Alles, was zu einer abgewählten Komponente gehört, wird einheitlich deaktiviert (bis 3.63 blieb das Label hell)
    static void LasttestDeaktivierung()
    {
        using (Label l = new Label()) using (ComboBox c = new ComboBox())
        {
            Control[] teile = new Control[] { l, c };
            DiagGui.SetPartEnabled(teile, false);
            Ist(!c.Enabled && l.ForeColor == DiagGui.DisabledText(), "abgewählt: Liste aus, Beschriftung gedämpft");
            DiagGui.SetPartEnabled(teile, true);
            Ist(c.Enabled && l.ForeColor == UI.Text, "gewählt: Liste an, Beschriftung normal");
            DiagGui.SetPartEnabled(null, true);
        }
    }

    // Zusammenfassung vor dem Start nur bei Eingriffen, Neustarts oder ohne Wiederherstellungspunkt
    static void Startuebersicht()
    {
        System.Collections.Generic.List<StartPunkt> nurAendern = new System.Collections.Generic.List<StartPunkt>();
        nurAendern.Add(new StartPunkt("Wartung", "Zeit synchronisieren", "Aendern", "nie"));
        Ist(!StartUebersicht.Noetig(nurAendern, false), "nur Ändern ohne Neustart: kein Dialog");
        Ist(!StartUebersicht.Noetig(null, false), "nichts gewählt: kein Dialog");
        Ist(StartUebersicht.Noetig(nurAendern, true), "ohne Wiederherstellungspunkt: Dialog");
        System.Collections.Generic.List<StartPunkt> ein = new System.Collections.Generic.List<StartPunkt>(nurAendern);
        ein.Add(new StartPunkt("Wartung", "Systemdateien reparieren", "Eingriff", "nie"));
        Ist(StartUebersicht.Noetig(ein, false), "Eingriff: Dialog");
        System.Collections.Generic.List<StartPunkt> neu = new System.Collections.Generic.List<StartPunkt>();
        neu.Add(new StartPunkt("Optimierung", "Schnellstart aus", "Aendern", "moeglich"));
        Ist(StartUebersicht.Noetig(neu, false), "Neustart möglich: Dialog");
        string t = StartUebersicht.Text(new System.Collections.Generic.List<string>(new string[] { "Diagnose", "Wartung" }), "ca. 40 Min.", ein,
            new System.Collections.Generic.List<string>(new string[] { "Wartung|0" }), null);
        Ist(t.StartsWith("Ablauf: Diagnose  →  Wartung, ca. 40 Min."), "Ablauf und Dauer");
        Ist(t.Contains("1 Eingriff ohne automatisches Rückgängig") && t.Contains("Systemdateien reparieren (Wartung)"), "Eingriff genannt");
        Ist(t.Contains("Wartung: Es wird KEIN Wiederherstellungspunkt angelegt."), "Wiederherstellungspunkt");
        Ist(!t.Contains("Zeit synchronisieren"), "Ändern ohne Neustart steht nicht in der Liste");
        Ist(!System.Text.RegularExpressions.Regex.IsMatch(t, @"(^|\n)\s*[-–] "), "ohne Spiegelstriche");
    }

    static void WartungDetailtext()
    {
        string[] a = DiagGui.SplitDetail("Systemdateien reparieren (sfc /scannow, nach DISM)");
        Ist(a[0] == "Systemdateien reparieren" && a[1] == "sfc /scannow, nach DISM", "Titel und Befehl getrennt");
        string[] b = DiagGui.SplitDetail("Netzwerk zurücksetzen (DNS-Cache, Winsock, TCP/IP), Neustart nötig");
        Ist(b[0] == "Netzwerk zurücksetzen" && b[1] == "DNS-Cache, Winsock, TCP/IP", "Neustart steht als Etikett, nicht im Text");
        string[] c = DiagGui.SplitDetail("Windows-Zeit neu synchronisieren");
        Ist(c[0] == "Windows-Zeit neu synchronisieren" && c[1] == "", "ohne Klammer");
        string[] d = DiagGui.SplitDetail("Datenträgerbereinigung (cleanmgr mit allen Kategorien außer Downloads)");
        Ist(d[0] == "Datenträgerbereinigung" && d[1].StartsWith("cleanmgr"), "cleanmgr");
        Ist(DiagGui.SplitDetail(null)[0] == "", "null");
    }

    // Voreinstellungen vor v3.64 speichern die alten Texte der Auswahllisten
    static void VoreinstellungAltwerte()
    {
        Ist(DiagGui.SameChoice("2 Min.", "2 Minuten"), "2 Minuten");
        Ist(DiagGui.SameChoice("120 Min.", "120 Minuten"), "120 Minuten");
        Ist(DiagGui.SameChoice("Automatisch", "automatisch (TjMax, sonst 100 °C)"), "automatisch");
        Ist(!DiagGui.SameChoice("5 Min.", "2 Minuten"), "andere Zahl");
        Ist(!DiagGui.SameChoice("25 %", "25 Minuten"), "andere Einheit");
        Ist(!DiagGui.SameChoice("", "2 Minuten") && !DiagGui.SameChoice("2 Min.", null), "leer");
    }

    static void LaufwerkKurzname()
    {
        Ist(DiagGui.ShortDiskName("NVMe PC SN820 NVMe WD 4096GB") == "PC SN820 WD", DiagGui.ShortDiskName("NVMe PC SN820 NVMe WD 4096GB"));
        Ist(DiagGui.ShortDiskName("Samsung SSD 870 EVO 1TB") == "Samsung SSD 870 EVO", "Samsung");
        Ist(DiagGui.ShortDiskName("NVMe") == "NVMe", "nichts übrig: Originalname");
    }

    static void AblageKurzform()
    {
        Ist(DiagGui.AblageKurz("\\\\ulb.ad.hhu.de\\Home\\wad32muj\\Desktop\\LMB\\Minibench-Daten") == "Ablage: ...\\LMB\\Minibench-Daten", DiagGui.AblageKurz("\\\\ulb.ad.hhu.de\\Home\\wad32muj\\Desktop\\LMB\\Minibench-Daten"));
        Ist(DiagGui.AblageKurz("E:\\Minibench-Daten") == "Ablage: E:\\Minibench-Daten", "Stick");
        Ist(DiagGui.AblageKurz("") == "Ablage: kein Datenordner", "leer");
    }

    static void EtikettRisikostufe()
    {
        Ist(UI.RiskLevel("Aendern") == "info" && UI.RiskLevel("Eingriff") == "warn" && UI.RiskLevel("Zerstoerend") == "crit" && UI.RiskLevel("Lesen") == "ok", "Stufen");
        using (Badge b = new Badge("Eingriff", "warn"))
        {
            Ist(b.Visible && b.Width > b.Height && b.Level == "warn", "sichtbar und breiter als hoch");
            b.Text = ""; Ist(!b.Visible, "ohne Text unsichtbar");
            b.Text = "bereits aktiv"; Ist(b.Visible, "wieder sichtbar");
        }
    }

    static void Aufklappgruppe()
    {
        using (GroupHeader h = new GroupHeader("Prüfungen"))
        {
            int n = 0; h.Toggled += delegate { n++; };
            Ist(!h.Expanded, "Vorgabe zu");
            System.Reflection.MethodInfo flip = typeof(GroupHeader).GetMethod("Flip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            flip.Invoke(h, null);
            Ist(h.Expanded && n == 1, "Klick klappt auf und meldet Toggled");
            h.Expanded = false; Ist(!h.Expanded && n == 1, "Setzen von außen meldet nichts");
            h.CountText = "2 von 4 gewählt"; Ist(h.CountText == "2 von 4 gewählt", "Zähler");
        }
    }

    // Profile der Diagnose: Schätzung passt zur Fußleiste (vorher drei verschiedene Zeitangaben)
    static void DiagnoseProfile()
    {
        Ist(DiagGui.DiagMinutes(new bool[] { true, true, true, true, true, true, true, true }) == 14, "Vollständig 14 Min.");
        Ist(DiagGui.DiagMinutes(new bool[] { true, true, false, false, false, true, true, false }) == 6, "Schnell 6 Min.");
        Ist(DiagGui.DiagMinutes(null) == 0, "ohne Prüfungen Grundzeit");
    }

    // ab v3.65: Stufen in der Laufansicht wie die Etiketten der Einstellungsseiten
    static void EtikettSchreibweise()
    {
        Ist(DiagGui.BadgeCase("KRITISCH") == "Kritisch", "KRITISCH");
        Ist(DiagGui.BadgeCase("warnung") == "Warnung", "warnung");
        Ist(DiagGui.BadgeCase("OK") == "OK" && DiagGui.BadgeCase("ok") == "OK", "OK bleibt Kürzel");
        Ist(DiagGui.BadgeCase(null) == "" && DiagGui.BadgeCase("  ") == "", "leer");
    }

    // ab v3.65: Karten haben den Grund der Seite und lassen Platz für den Rahmen
    static void KarteRahmen()
    {
        using (CardPanel c = new CardPanel())
        {
            Ist(c.BackColor == UI.Panel, "Grund wie die Seite");
            Ist(c.Padding.Left >= UI.S(8) && c.Padding.Top >= UI.S(8), "Innenabstand für den Rahmen");
        }
    }

    // ab v3.72: Startseiten-Elemente (PillLabel, StartTaskCard, StartTaskRow) unterstützen transparente Hintergründe
    static void StartseitenElemente()
    {
        using (PillLabel pill = new PillLabel("Ryzen 5 5600"))
        {
            Ist(pill.Text == "Ryzen 5 5600" && pill.BackColor == Color.Transparent, "PillLabel transparent");
        }
        using (StartTaskCard card = new StartTaskCard("Test", "Beschreibung", "Fußzeile", "Badge", "ok", UI.IcoDiag, true))
        {
            Ist(card.Title == "Test" && card.Recommended, "StartTaskCard initialisiert");
            Ist(card.Width >= UI.S(200), "StartTaskCard vergrößert");
        }
        using (StartTaskRow row = new StartTaskRow("Test", "Untertitel", "Badge", "warn", UI.IcoSpeed))
        {
            Ist(row.Subtitle == "Untertitel" && row.BackColor == Color.Transparent, "StartTaskRow transparent");
        }
        using (KpiBox kpi = new KpiBox("Gesamt", "12.111"))
        {
            Ist(kpi.Title == "Gesamt" && kpi.Value == "12.111" && kpi.BackColor == Color.Transparent, "KpiBox transparent");
            Ist(kpi.Width >= UI.S(90), "KpiBox vergrößert");
        }
        Ist(UI.StartGruppenbreite >= 860, "StartGruppenbreite mindestens 860");
        string[] origGpu = DiagGui.GpuNames;
        try
        {
            DiagGui.GpuNames = new string[] { "Virtual Desktop Monitor", "AMD Radeon RX 6800" };
            Ist(DiagGui.GetPrimaryGpuName() == "AMD Radeon RX 6800", "GetPrimaryGpuName bevorzugt dGPU vor virtueller GPU");
            DiagGui.GpuNames = new string[] { "Parsec Virtual Display Adapter", "Intel(R) UHD Graphics 770" };
            Ist(DiagGui.GetPrimaryGpuName() == "Intel(R) UHD Graphics 770", "GetPrimaryGpuName bevorzugt iGPU vor virtueller GPU");
        }
        finally
        {
            DiagGui.GpuNames = origGpu;
        }
    }

    // ab v4.0: Dezimale Versionsnummer zur verlässlichen Größenordnung (z. B. 3.882 < 3.9 < 3.91 < 4.0)
    // ab v4.01: Katalog und Vertrag nennen Symbole beim Namen; unbekannt oder leer ergibt den Ersatz
    static void UiSymbolNachName()
    {
        Ist(UI.IconByName("IcoShield", "x") == UI.IcoShield, "IcoShield nicht aufgelöst");
        Ist(UI.IconByName("IcoSpeed", "x") == UI.IcoSpeed, "IcoSpeed nicht aufgelöst");
        Ist(UI.IconByName("IcoAdmin", "x") == UI.IcoAdmin, "IcoAdmin nicht aufgelöst");
        Ist(UI.IconByName("GibtEsNicht", "x") == "x", "unbekannter Name muss den Ersatz liefern");
        Ist(UI.IconByName("", "x") == "x", "leerer Name muss den Ersatz liefern");
    }

    static void UiVersionsvergleich()
    {
        Ist(UI.VersionNumber("3.882") < UI.VersionNumber("3.9"), "3.882 < 3.9");
        Ist(UI.VersionNumber("3.9") < UI.VersionNumber("3.91"), "3.9 < 3.91");
        Ist(UI.VersionNumber("3.91") < UI.VersionNumber("4.0"), "3.91 < 4.0");
        Ist(UI.VersionNumber("v3.91") == UI.VersionNumber("3.91"), "v3.91 == 3.91");
        Ist(UI.VersionNumber("4.0") == 4.0, "4.0 == 4.0");
    }

    // ab v4.0: SettingRow Dynamik (deterministische Spalten, kein 52px-Deckel, Badge nicht bei 0,0)
    static void SettingRowDynamik()
    {
        using (ToggleSwitch sw = new ToggleSwitch())
        using (SettingRow row = new SettingRow("Langer Titel für dynamische Prüfung", "Eine längere Beschreibung für dynamische Höhenberechnung.", sw))
        {
            row.Width = 500;
            Badge b = new Badge("", "ok");
            row.AddBadge(b);
            b.Text = "gesetzt";
            Ist(b.Left > 100, "Badge sitzt rechts");
            Ist(row.Height >= UI.S(42), "Mindesthöhe");
        }
    }

    // ab v4.0: CategoryPickerItem nutzt Kurztitel falls vorhanden
    static void CategoryPickerKurztitel()
    {
        using (CategoryPickerItem c = new CategoryPickerItem("Systemdateien und Komponentenspeicher", UI.IcoFolder, "Systemdateien"))
        {
            Ist(c.ShortTitle == "Systemdateien", "Kurztitel bevorzugt");
        }
        using (CategoryPickerItem c = new CategoryPickerItem("Datenschutz", UI.IcoShield))
        {
            Ist(c.ShortTitle == "Datenschutz", "Fallback auf Titel");
        }
    }

    // ab v4.0: UI.SkalierungDpi verifiziert Skalierung bei 100 % und 150 %
    static void UiSkalierungDpi()
    {
        float alt = UI.DpiScale;
        try
        {
            UI.DpiScale = 1.0f;
            int s100 = UI.S(200);
            UI.DpiScale = 1.5f;
            int s150 = UI.S(200);
            Ist(s100 == 200 && s150 == 300, "150 % entspricht dem 1.5-fachen");
        }
        finally { UI.DpiScale = alt; }
    }

    // ab v4.03: Versionen dezimal vergleichen (System.Version hielt 4.03 für neuer als 4.1)
    static void UpdateVersionsvergleich()
    {
        Ist(DiagGui.IsNewerVersion("v4.1", "4.03"), "4.1 ist neuer als 4.03");
        Ist(!DiagGui.IsNewerVersion("v4.03", "4.1"), "4.03 ist nicht neuer als 4.1");
        Ist(DiagGui.IsNewerVersion("3.9", "3.882"), "3.9 ist neuer als 3.882");
        Ist(DiagGui.IsNewerVersion("v4.03", "4.02"), "4.03 ist neuer als 4.02");
        Ist(!DiagGui.IsNewerVersion("v4.02", "4.02"), "gleiche Version ist nicht neuer");
    }

    // ab v4.02: Einordnung gegenüber dem Referenz-PC ohne Wertung als Durchschnitt
    static void ErgebnisEinordnung()
    {
        Ist(DiagGui.RatingWord(32) == "deutlich langsamer", "32 %: " + DiagGui.RatingWord(32));
        Ist(DiagGui.RatingWord(64) == "langsamer", "64 %: " + DiagGui.RatingWord(64));
        Ist(DiagGui.RatingWord(95) == "gleichauf", "95 %: " + DiagGui.RatingWord(95));
        Ist(DiagGui.RatingWord(118) == "schneller", "118 %: " + DiagGui.RatingWord(118));
    }

    // ab v4.02: eine Pille je Laufwerk; Dezimalkomma der Größe darf die Felder nicht trennen
    static void ErgebnisLaufwerkspillen()
    {
        System.Collections.Generic.List<string> p = DiagGui.DiskPills("ST2000DX001 (1,82 TB, SATA, Healthy); Samsung SSD 980 1TB (931,5 GB, NVMe, Warning)");
        Ist(p.Count == 2, "zwei Pillen erwartet, " + p.Count);
        Ist(p[0] == "ST2000DX001  ·  SATA", "erste Pille: " + p[0]);
        Ist(p[1].EndsWith("NVMe  ·  Warning"), "Zustand außer Healthy wird genannt: " + p[1]);
    }

    // ab v4.05: bereits aktive Einträge der Optimierung stehen auf An, sind gesperrt und laufen nicht mit
    static OptItem NeuerOptEintrag(string id, string kat)
    {
        OptItem it = new OptItem(); it.Id = id; it.Kat = kat;
        it.Box = new CheckBox(); it.Toggle = new ToggleSwitch();
        DiagGui.VerbindeOptEintrag(it, null);
        return it;
    }

    static void OptimierungSperre()
    {
        OptItem a = NeuerOptEintrag("A", "K"), b = NeuerOptEintrag("B", "K");
        System.Collections.Generic.List<OptItem> l = new System.Collections.Generic.List<OptItem>(new OptItem[] { a, b });
        a.Box.Checked = true; b.Box.Checked = true;
        Ist(a.Toggle.Checked && b.Toggle.Checked, "Haken und Schalter verbunden");
        DiagGui.SperreOptEintrag(a, true);
        Ist(a.Toggle.Checked && a.Toggle.Locked && a.Gesperrt, "gesperrt steht auf An");
        Ist(a.Toggle.Enabled, "gesperrt ist nicht ausgegraut (Enabled)");
        a.Toggle.UserToggle();
        Ist(a.Toggle.Checked, "Klick ändert gesperrten Schalter nicht");
        SettingRow row = new SettingRow("A", "", a.Toggle);
        row.Checked = row.Checked; // Zeile reicht Klicks über UserToggle weiter
        System.Reflection.MethodInfo klick = typeof(SettingRow).GetMethod("OnMouseClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        klick.Invoke(row, new object[] { new MouseEventArgs(MouseButtons.Left, 1, 5, 5, 0) });
        Ist(a.Toggle.Checked, "Klick auf die Zeile ändert gesperrten Schalter nicht");
        // Vorlage "Alles abwählen": Auswahl ändert sich, Anzeige des gesperrten Eintrags nicht
        a.Box.Checked = false; b.Box.Checked = false;
        Ist(a.Toggle.Checked && !b.Toggle.Checked, "Vorlage lässt gesperrten Schalter auf An");
        a.Box.Checked = true;
        System.Collections.Generic.List<string> ids = DiagGui.GewaehlteOptIds(l);
        Ist(ids.Count == 0, "gesperrte Einträge gehen nicht in den Lauf: " + string.Join(",", ids.ToArray()));
        b.Box.Checked = true;
        ids = DiagGui.GewaehlteOptIds(l);
        Ist(ids.Count == 1 && ids[0] == "B", "nur offene Einträge im Lauf");
        // Entsperren (z. B. Zustand teilweise) zeigt wieder die Auswahl
        a.Box.Checked = false;
        DiagGui.SperreOptEintrag(a, false);
        Ist(!a.Toggle.Checked && !a.Toggle.Locked, "entsperrt folgt der Auswahl");
        a.Toggle.UserToggle();
        Ist(a.Box.Checked, "entsperrt wieder umschaltbar");
        Ist(DiagGui.OptGesperrtHinweis.Contains("Seite \u00C4nderungen"), "Hinweis nennt die Seite Änderungen");
    }

    static void OptimierungZaehler()
    {
        System.Collections.Generic.List<OptItem> l = new System.Collections.Generic.List<OptItem>();
        for (int i = 0; i < 17; i++) l.Add(NeuerOptEintrag("E" + i, "Datenschutz"));
        l.Add(NeuerOptEintrag("X", "Andere"));
        for (int i = 0; i < 3; i++) l[i].Box.Checked = true;
        for (int i = 10; i < 15; i++) DiagGui.SperreOptEintrag(l[i], true);
        l[10].Box.Checked = true; // gewählt und gesperrt zählt als aktiv, nicht als gewählt
        int[] z = DiagGui.OptZaehler(l, "Datenschutz");
        Ist(z[0] == 3 && z[1] == 5 && z[2] == 17, "Zähler: " + z[0] + "/" + z[1] + "/" + z[2]);
        Ist(DiagGui.OptZaehlerText(z) == "3 gew\u00E4hlt \u00B7 5 bereits aktiv \u00B7 17 Eintr\u00E4ge", "Text: " + DiagGui.OptZaehlerText(z));
        Ist(DiagGui.OptZaehlerText(new int[] { 0, 0, 7 }) == "0 gew\u00E4hlt \u00B7 7 Eintr\u00E4ge", "ohne aktive kein Teil bereits aktiv");
        using (CategoryPickerItem p = new CategoryPickerItem("Datenschutz", UI.IcoShield, "Datenschutz"))
        {
            p.SetCounts(z[0], z[2], z[1]);
            Ist(p.CheckedCount == 3 && p.TotalCount == 17 && p.ActiveCount == 5, "Seitenleiste erhält gewählt, gesamt und aktiv");
            p.Width = UI.S(254); p.Height = UI.S(40);
            using (Bitmap bmp = new Bitmap(p.Width, p.Height)) p.DrawToBitmap(bmp, new Rectangle(0, 0, p.Width, p.Height));
        }
    }

    // ab v4.05: GPU-Z liegt mit Versionsnummer im Namen vor (GPU-Z.2.71.0.exe); bei mehreren gilt die neueste
    static void ToolsPlatzhalter()
    {
        Ist(DiagGui.NeuesteDatei(new string[] { @"T\GPU-Z.2.9.0.exe", @"T\GPU-Z.2.71.0.exe", @"T\GPU-Z.2.70.1.exe" }) == @"T\GPU-Z.2.71.0.exe", "höchste Version (2.71.0 vor 2.9.0)");
        Ist(DiagGui.NeuesteDatei(new string[] { @"T\GPU-Z.exe", @"T\GPU-Z.2.1.exe" }) == @"T\GPU-Z.2.1.exe", "Datei ohne Nummer zuletzt");
        Ist(DiagGui.NeuesteDatei(new string[0]) == null, "leere Liste");
        string d = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MinibenchTools_" + System.Diagnostics.Process.GetCurrentProcess().Id);
        try
        {
            System.IO.Directory.CreateDirectory(d);
            foreach (string n in new string[] { "GPU-Z.2.70.0.exe", "GPU-Z.2.71.0.exe", "cpuz_x32.exe", "cpuz_x64.exe" }) System.IO.File.WriteAllText(System.IO.Path.Combine(d, n), "");
            Ist(System.IO.Path.GetFileName(DiagGui.SucheProgramm(d, new string[] { "GPU-Z*.exe" })) == "GPU-Z.2.71.0.exe", "Platzhalter findet die neueste Datei");
            Ist(System.IO.Path.GetFileName(DiagGui.SucheProgramm(d, new string[] { "cpuz_x64.exe", "cpuz_x32.exe" })) == "cpuz_x64.exe", "Reihenfolge der Namen ist Vorrang");
            Ist(DiagGui.SucheProgramm(d, new string[] { "MSIAfterburner.exe" }) == null, "fehlende Datei");
            Ist(DiagGui.SucheProgramm(System.IO.Path.Combine(d, "fehlt"), new string[] { "GPU-Z*.exe" }) == null, "fehlender Ordner");
        }
        finally { try { System.IO.Directory.Delete(d, true); } catch { } }
    }

    static void ToolsRaster()
    {
        Ist(DiagGui.ToolSpalten(UI.S(1100)) == 2, "breites Fenster: zwei Spalten");
        Ist(DiagGui.ToolSpalten(UI.S(780)) == 1, "schmales Fenster: eine Spalte");
        using (ToolRow r = new ToolRow("CPU-Z", "Prozessor", UI.IcoCpu))
        {
            Ist(r.Height <= UI.S(52), "Zeile deutlich unter 68 px: " + r.Height);
            Button b = UI.Primary("Starten"), m = UI.Secondary("\u2026");
            r.MainButton = b; r.MoreButton = m; r.Width = UI.S(500);
            Ist(m.Right <= r.Width && b.Right <= m.Left, "Knöpfe rechts, \u2026 ganz außen");
            r.StatusText = "bereit, portabel"; r.StatusLevel = "ok";
            using (Bitmap bmp = new Bitmap(r.Width, r.Height)) r.DrawToBitmap(bmp, new Rectangle(0, 0, r.Width, r.Height));
        }
    }

    static void SoftwareListe()
    {
        System.Collections.Generic.List<string> g = new System.Collections.Generic.List<string>();
        System.Collections.Generic.List<string[]> p = new System.Collections.Generic.List<string[]>();
        DiagGui.LiesPaketListe(new string[] { "G|Browser", "G|Medien", "P|Browser|Google.Chrome|Google Chrome|Webbrowser", "P|Medien|VideoLAN.VLC|VLC|Wiedergabe", "P|Neu|X.Y||ohne Name", "kaputt", "P|Browser||leer|x" }, g, p);
        Ist(g.Count == 3 && g[0] == "Browser" && g[1] == "Medien" && g[2] == "Neu", "Gruppen in Reihenfolge, unbekannte am Ende: " + string.Join(",", g.ToArray()));
        Ist(p.Count == 3, "Pakete ohne Id werden übergangen: " + p.Count);
        Ist(p[2][2] == "X.Y", "ohne Namen gilt die Id");
    }

    static void SoftwareInstalliert()
    {
        string id; bool inst;
        Ist(DiagGui.ParsePaketZustand("@@PAKZ|7zip.7zip|installiert", out id, out inst) && id == "7zip.7zip" && inst, "installiert");
        Ist(DiagGui.ParsePaketZustand("@@PAKZ|VideoLAN.VLC|nicht installiert", out id, out inst) && !inst, "nicht installiert");
        Ist(!DiagGui.ParsePaketZustand("@@RESULT|2", out id, out inst) && !DiagGui.ParsePaketZustand("@@PAKZ||x", out id, out inst), "andere Zeilen");
        CheckBox a = new CheckBox(), b = new CheckBox();
        CheckBox[] l = new CheckBox[] { a, b };
        a.Checked = true; b.Checked = true;
        DiagGui.MarkierePaket(a, true);
        Ist(a.Checked && !a.AutoCheck && a.Enabled && a.ForeColor == UI.Ok, "installiert: Haken gesperrt, grün, nicht ausgegraut");
        System.Collections.Generic.List<int> sel = DiagGui.AuswahlPakete(l);
        Ist(sel.Count == 1 && sel[0] == 1, "installierte gehen nicht in die Installation");
        DiagGui.MarkierePaket(a, false);
        Ist(!a.Checked && a.AutoCheck, "nach Deinstallation wieder wählbar");
        DiagGui.MarkierePaket(b, false);
        Ist(b.Checked && b.AutoCheck, "nie installiert markiertes Paket behält die Auswahl");
    }

    // ab v4.06: Update-Prüfung liest Tag, Adresse und Prüfsumme aus Text, digest oder LMB.exe.sha256
    static void UpdateRelease()
    {
        string h = "f663f67d146bd8d7ae4544ed9b4c473c06f2cec1c59220dde518555c9adbdcf5";
        DiagGui.UpdateInfo u = DiagGui.LiesReleaseJson("{\"tag_name\":\"v4.06\",\"body\":\"Text\\n\\nSHA-256 (LMB.exe): " + h.ToUpperInvariant() + "\\n\",\"assets\":[{\"name\":\"LMB.exe\",\"browser_download_url\":\"https://x/LMB.exe\"}]}");
        Ist(u.Tag == "v4.06" && u.Url == "https://x/LMB.exe" && u.Sha256 == h && u.Fehler == "", "Prüfsumme aus dem Text: " + u.Sha256);
        u = DiagGui.LiesReleaseJson("{\"tag_name\":\"v4.05\",\"body\":\"Release v4.05\",\"assets\":[{\"name\":\"LMB.exe\",\"browser_download_url\":\"https://x/LMB.exe\",\"digest\":\"sha256:" + h + "\"}]}");
        Ist(u.Sha256 == h, "Prüfsumme aus digest, wenn der Text keine nennt");
        u = DiagGui.LiesReleaseJson("{\"message\":\"API rate limit exceeded\"}");
        Ist(u.Tag == "" && u.Fehler.Length > 0, "Antwort ohne Release ist ein Fehler");
        Ist(DiagGui.TagAusAdresse("https://github.com/miniLeonardo/MiniBench/releases/tag/v4.05") == "v4.05", "Tag aus der Weiterleitung");
        Ist(DiagGui.TagAusAdresse("https://github.com/miniLeonardo/MiniBench/releases") == "", "ohne Tag leer");
        Ist(DiagGui.ShaAusText(h + "  LMB.exe\n") == h, "Format von LMB.exe.sha256");
        Ist(DiagGui.DownloadAdresse("v4.06") == "https://github.com/miniLeonardo/MiniBench/releases/download/v4.06/LMB.exe", "Download-Adresse");
        Ist(DiagGui.UpdateCacheStunden <= 2, "Cache höchstens zwei Stunden");
        Ist(DiagGui.IsNewerVersion("4.05", "4.04") && !DiagGui.IsNewerVersion("v4.04".TrimStart('v'), "4.05"), "Vergleich");
    }

    // ab v4.06: Meldung von "Median aktualisieren" aus dem Ergebnis (vorher immer "fehlgeschlagen")
    static void ReferenzMedian()
    {
        System.Collections.Generic.List<string> l = new System.Collections.Generic.List<string>(new string[] { "Referenz_Median.json: Median aller Systeme aus 5 Systemen, 23 Messgrößen (A, B)" });
        string t = DiagGui.MedianErgebnisText("3 Median-Referenzen gespeichert", l);
        Ist(t.StartsWith("3 Median-Referenzen gespeichert.") && t.Contains("Referenz_Median.json"), "Ergebnis und Zeilen: " + t);
        Ist(DiagGui.MedianErgebnisText("", new System.Collections.Generic.List<string>()).Contains("kein Ergebnis"), "ohne Ergebnis");
    }
    // ab v4.07: jede Akzentfarbe ist in beiden Farbschemata lesbar, Blau bleibt der bisherige Stand
    static void AkzentFarben()
    {
        bool alt = UI.IsDark; string altA = UI.Akzent;
        try
        {
            UI.Akzent = "Blau"; UI.SetTheme(false);
            Ist(UI.Accent == Color.FromArgb(0, 103, 192) && UI.AccentSoft == Color.FromArgb(235, 243, 251), "Blau hell unverändert");
            UI.SetTheme(true);
            Ist(UI.Accent == Color.FromArgb(76, 194, 255) && UI.AccentHover == Color.FromArgb(96, 205, 255), "Blau dunkel unverändert");
            Ist(UI.AkzentNamen.Length >= 5 && UI.AkzentNamen[0] == "Blau", "Liste beginnt mit Blau");
            foreach (string name in UI.AkzentNamen)
                foreach (bool dark in new bool[] { false, true })
                {
                    UI.Akzent = name; UI.SetTheme(dark);
                    string n = name + (dark ? " dunkel: " : " hell: ");
                    Ist(UI.ContrastRatio(UI.OnAccent, UI.Accent) >= 4.5, n + "Schrift auf Knopf " + UI.ContrastRatio(UI.OnAccent, UI.Accent).ToString("0.0"));
                    Ist(UI.ContrastRatio(UI.Accent, UI.Panel) >= 4.5, n + "Verweis auf Seite " + UI.ContrastRatio(UI.Accent, UI.Panel).ToString("0.0"));
                    Ist(UI.ContrastRatio(UI.Accent, UI.AccentSoft) >= 3.5, n + "Akzent auf weicher Fläche " + UI.ContrastRatio(UI.Accent, UI.AccentSoft).ToString("0.0"));
                    Ist(UI.ContrastRatio(UI.Text, UI.AccentSoft) >= 7, n + "Text auf weicher Fläche");
                    Ist(UI.ContrastRatio(UI.OnAccent, UI.AccentHover) >= 4.5, n + "Schrift auf Knopf unter der Maus");
                    using (Button b = UI.Primary("x")) Ist(b.BackColor == UI.Accent && b.ForeColor == UI.OnAccent, n + "Primärknopf folgt dem Akzent");
                }
            Ist(UI.AkzentGueltig("violett") == "Violett" && UI.AkzentGueltig("gibt es nicht") == "Blau" && UI.AkzentGueltig(null) == "Blau", "unbekannte Namen fallen auf Blau");
            UI.Akzent = "Pink"; UI.SetTheme(false); Color pinkHell = UI.Accent; UI.SetTheme(true);
            Ist(UI.Accent != pinkHell, "eigener Ton je Farbschema");
        }
        finally { UI.Akzent = altA; UI.SetTheme(alt); }
    }

    // Akzentfarbe aus Windows: Byte-Reihenfolge ABGR, beliebige Farben werden auf lesbaren Kontrast gebracht
    static void AkzentWindows()
    {
        Ist(UI.AusAbgr(0xFF0078D7) == Color.FromArgb(0xD7, 0x78, 0x00), "ABGR gelesen");
        foreach (Color c in new Color[] { Color.Yellow, Color.White, Color.Black, Color.FromArgb(0, 120, 215), Color.Lime, Color.FromArgb(40, 0, 60) })
            foreach (bool dark in new bool[] { false, true })
            {
                Color x = UI.MitKontrast(c, dark);
                Color flaeche = dark ? Color.FromArgb(36, 37, 38) : Color.White, schrift = dark ? Color.FromArgb(18, 19, 20) : Color.White;
                Ist(UI.ContrastRatio(x, flaeche) >= 4.5 && UI.ContrastRatio(schrift, x) >= 4.5, "Kontrast für " + c + (dark ? " dunkel" : " hell"));
            }
        Ist(UI.Mix(Color.Black, Color.White, 0.5) == Color.FromArgb(128, 128, 128) && UI.Mix(Color.Red, Color.Blue, 0) == Color.FromArgb(255, 0, 0), "Mischen");
    }

    // Übergänge enden immer im Zielzustand, der Zeitgeber steht danach still, ein neuer Lauf ersetzt den alten
    static void BewegungZeitgeber()
    {
        bool alt = Bewegung.Aktiv;
        try
        {
            Bewegung.Aktiv = true;
            if (!Bewegung.Erlaubt) return;   // Remotesitzung oder Windows ohne Animationen: nichts zu prüfen
            object ziel = new object();
            System.Collections.Generic.List<double> werte = new System.Collections.Generic.List<double>(); int ende = 0;
            Bewegung.Starte(ziel, 200, delegate(double t) { werte.Add(t); }, delegate { ende++; });
            Ist(werte.Count == 1 && werte[0] == 0 && Bewegung.Laufend == 1, "beginnt bei 0");
            long start = Bewegung.Jetzt;
            Bewegung.Tick(start + 100);
            Ist(werte.Count == 2 && werte[1] > 0.8 && werte[1] < 1, "läuft sanft aus: nach der halben Zeit über 80 % (" + werte[1] + ")");
            Bewegung.Tick(start + 5000);
            Ist(werte[werte.Count - 1] == 1.0 && ende == 1 && Bewegung.Laufend == 0, "endet genau im Ziel und meldet das Ende einmal");
            Bewegung.Tick(start + 6000);
            Ist(ende == 1, "kein zweites Ende");
            int a = 0, b = 0;
            Bewegung.Starte(ziel, 200, delegate(double t) { a++; });
            Bewegung.Starte(ziel, 200, delegate(double t) { b++; });
            Bewegung.Tick(Bewegung.Jetzt + 5000);
            Ist(Bewegung.Laufend == 0 && a == 1 && b == 2, "neuer Lauf ersetzt den alten (a " + a + ", b " + b + ")");
            Bewegung.Starte(ziel, 200, delegate(double t) { throw new System.Exception("x"); });
            Ist(Bewegung.Laufend == 0, "Fehler im ersten Schritt startet nichts");
            Ist(Bewegung.Verlauf(0) == 0 && Bewegung.Verlauf(1) == 1 && Bewegung.Verlauf(2) == 1 && Bewegung.Verlauf(0.25) < Bewegung.Verlauf(0.5), "Verlauf steigt von 0 auf 1");
            Ist(Bewegung.DauerErgebnis <= 800 && Bewegung.DauerSeite <= 200, "kurze Dauer");
        }
        finally { Bewegung.Aktiv = alt; }
    }

    // Abgeschaltet gilt sofort der Endstand, nichts läuft nach
    static void BewegungAbgeschaltet()
    {
        bool alt = Bewegung.Aktiv;
        try
        {
            Bewegung.Aktiv = false;
            double letzter = -1; int ende = 0;
            Bewegung.Starte(new object(), 500, delegate(double t) { letzter = t; }, delegate { ende++; });
            Ist(letzter == 1.0 && ende == 1 && Bewegung.Laufend == 0, "sofort Endstand");
            using (Panel p = new Panel()) Ist(Ueberblendung.Aufnahme(p) == null, "keine Aufnahme für die Überblendung");
            using (PercentBar pb = new PercentBar()) { pb.Percent = 80; Ist(pb.Anteil == 1, "Balken zeigt ohne Bewegung den vollen Wert"); pb.Anteil = 5; Ist(pb.Anteil == 1, "Anteil begrenzt"); }
        }
        finally { Bewegung.Aktiv = alt; }
    }

    static void RingBogen()
    {
        Ist(RingAnzeige.Bogen(50, 1) == 180f && RingAnzeige.Bogen(100, 1) == 360f && RingAnzeige.Bogen(150, 1) == 360f, "voller Ring = 100 %, darüber bleibt er voll");
        Ist(RingAnzeige.Bogen(80, 0.5) == 144f && RingAnzeige.Bogen(80, 0) == 0f && RingAnzeige.Bogen(-5, 1) == 0f, "Anteil und Untergrenze");
        using (RingAnzeige r = new RingAnzeige())
        {
            r.Percent = 76; Ist(r.Anteil == 1 && r.Percent == 76, "Vorgabe zeigt den Wert ganz");
            r.Anteil = -1; Ist(r.Anteil == 0, "Anteil nicht unter 0");
            using (Bitmap b = new Bitmap(r.Width, r.Height)) r.DrawToBitmap(b, new Rectangle(0, 0, r.Width, r.Height));
        }
    }

    // bis v4.06 passte ein Datenbankeintrag ohne Ordner auf jeden Lauf (EndsWith(""))
    static void ErgebnisOrdner()
    {
        Ist(!DiagGui.ErgebnisOrdnerPasst(@"E:\Minibench-Daten\Berichte\PC1_20261110_101500", ""), "leerer Ordner passt nie");
        Ist(!DiagGui.ErgebnisOrdnerPasst(@"E:\Minibench-Daten\Berichte\PC1_20261110_101500", null) && !DiagGui.ErgebnisOrdnerPasst("", "x"), "ohne Angaben kein Treffer");
        Ist(DiagGui.ErgebnisOrdnerPasst(@"E:\Minibench-Daten\Berichte\PC1_20261110_101500", @"Berichte\PC1_20261110_101500"), "relativer Ordner");
        Ist(DiagGui.ErgebnisOrdnerPasst(@"E:\Minibench-Daten\Berichte\PC1_20261110_101500\", @"e:\minibench-daten\Berichte\PC1_20261110_101500"), "ganzer Pfad, Groß- und Kleinschreibung, Trenner am Ende");
        Ist(DiagGui.ErgebnisOrdnerPasst("E:/Minibench-Daten/Berichte/PC1_20261110_101500", "PC1_20261110_101500"), "nur der Ordnername");
        Ist(!DiagGui.ErgebnisOrdnerPasst(@"E:\Berichte\XPC1_20261110_101500", "PC1_20261110_101500"), "kein Treffer auf ein Namensende");
    }

    // Akzentfarbe und Animationen stehen in Einstellungen.json neben den bisherigen Schlüsseln
    static void EinstellungenWerte()
    {
        string f = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MinibenchEinst_" + System.Guid.NewGuid().ToString("N") + ".json");
        try
        {
            Ist(DiagGui.LiesEinstellung(f, "Akzent") == null, "fehlende Datei");
            System.IO.File.WriteAllText(f, "{\"DarkMode\":true,\"GeseheneVersion\":\"3.72\"}", System.Text.Encoding.UTF8);
            DiagGui.SchreibeEinstellung(f, "Akzent", "Violett");
            DiagGui.SchreibeEinstellung(f, "Animationen", false);
            Ist((DiagGui.LiesEinstellung(f, "Akzent") as string) == "Violett", "Akzent gelesen");
            Ist(System.Convert.ToBoolean(DiagGui.LiesEinstellung(f, "Animationen")) == false, "Animationen gelesen");
            Ist(System.Convert.ToBoolean(DiagGui.LiesEinstellung(f, "DarkMode")) && (DiagGui.LiesEinstellung(f, "GeseheneVersion") as string) == "3.72", "vorhandene Schlüssel bleiben");
            System.IO.File.WriteAllText(f, "kaputt");
            Ist(DiagGui.LiesEinstellung(f, "Akzent") == null, "beschädigte Datei");
            DiagGui.SchreibeEinstellung(f, "Akzent", "Pink");
            Ist((DiagGui.LiesEinstellung(f, "Akzent") as string) == "Pink", "beschädigte Datei wird neu angelegt");
            DiagGui.SchreibeEinstellung("", "Akzent", "Pink");
        }
        finally { try { System.IO.File.Delete(f); } catch { } }
    }

    // ------------------------------------------------------------ ab v4.08: Modulseiten, Ergebnis als Seite
    static string NeuerDatenordner()
    {
        string d = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MinibenchGui_" + System.Diagnostics.Process.GetCurrentProcess().Id + "_" + System.Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(d, "Datenbank"));
        // Update-Prüfung aus (kein Netzwerk im Test), Farbschema fest
        System.IO.File.WriteAllText(System.IO.Path.Combine(d, "Einstellungen.json"), "{\"DarkMode\":false,\"UpdatePruefung\":false,\"StartseiteZeigen\":true}", System.Text.Encoding.UTF8);
        return d;
    }
    static void Raeume(string d) { try { System.IO.Directory.Delete(d, true); } catch { } }
    static DiagGui NeueOberflaeche(string d, string[] disks) { return new DiagGui("", "", "", d, "4.08", disks ?? new string[0], new string[0], "G-test"); }

    // Datenbankeintrag dieses PCs; mitVergleich und mitBefunden steuern die Felder, die es erst ab 4.08 bzw. bei Läufen gibt
    static void SchreibeLauf(string d, string datum, bool mitVergleich, bool mitBefunden)
    {
        string pc = System.Environment.MachineName;
        System.Text.StringBuilder b = new System.Text.StringBuilder();
        b.Append("{\"Format\":\"PC-Diagnose-DB/2\",\"Name\":\"\",\"Computer\":\"").Append(pc).Append("\",\"Datum\":\"").Append(datum).Append("\",\"Quelle\":\"Lauf\",\"Module\":\"Diagnose + Benchmark\",");
        b.Append("\"Hardware\":{\"CPU\":\"AMD Ryzen 5 7600X\",\"Kerne\":6,\"Threads\":12,\"RAM\":\"32 GB DDR5-6000\",\"GPU\":\"Radeon RX 6800\",\"Datentraeger\":\"Samsung SSD 980 1TB (931,5 GB, NVMe, Healthy)\"},");
        b.Append("\"Werte\":{\"CPU|MT\":32433,\"CPU|ST\":3086,\"RAM|Lesen\":61.2,\"GPU|REND\":131.2},\"Ordner\":\"Berichte\\\\X_").Append(datum.Replace(" ", "_").Replace(":", "")).Append("\"");
        if (mitBefunden) b.Append(",\"Befunde\":{\"Kritisch\":1,\"Warnungen\":1,\"Hinweise\":2,\"Liste\":[\"[HINWEIS] Start: Schnellstart ist aktiv.\",\"[WARNUNG] RAM: XMP inaktiv: 2133 statt 3200 MT/s.\",\"[KRITISCH] Laufwerk: SMART meldet Fehler.\",\"[HINWEIS] Treiber: alt.\"]}");
        if (mitVergleich) b.Append(",\"Vergleich\":{\"Referenz\":\"Median Desktop-PCs (7 Systeme)\",\"Stand\":\"2026-11-10\",\"Gruppen\":{\"CPU\":103,\"GPU\":237,\"RAM\":174}}");
        b.Append("}");
        System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.Combine(d, "Datenbank"), pc + "_" + datum.Replace("-", "").Replace(" ", "_").Replace(":", "") + ".json"), b.ToString(), new System.Text.UTF8Encoding(false));
    }

    static bool HatText(Control c, string teil)
    {
        if (c is Label && c.Text.IndexOf(teil, System.StringComparison.Ordinal) >= 0) return true;
        foreach (Control k in c.Controls) if (HatText(k, teil)) return true;
        return false;
    }

    // Gemeinsamer Rahmen der Seiten Benchmark und Diagnose: Spalten, Karte, Unterbereich, Felder mit Umbruch
    static void ModulseiteRahmen()
    {
        int[] b = DiagGui.ModulSeite.SpaltenBreiten(UI.S(1100), UI.S(16));
        Ist(b[0] > b[1] && b[0] + b[1] + UI.S(16) == UI.S(1100), "zwei Spalten füllen die Breite: " + b[0] + " + " + b[1]);
        Ist(System.Math.Abs(b[0] * 100 / (b[0] + b[1]) - 60) <= 1, "links rund 60 %");
        int[] s = DiagGui.ModulSeite.SpaltenBreiten(UI.S(880), UI.S(16));
        Ist(s[0] == UI.S(880) && s[1] == UI.S(880), "unter 900 px eine Spalte in voller Breite");
        DiagGui.ZeilenKarte k = new DiagGui.ZeilenKarte("Messungen");
        using (k.Karte)
        {
            SettingRow r1 = k.Zeile(new SettingRow("Prozessor", "Einzel- und Mehrkern")), r2 = k.Zeile(new SettingRow("Grafik", "Rendertest"));
            UnterBereich u = k.Zeile(new UnterBereich());
            FeldReihe fr = new FeldReihe();
            ComboBox c1 = new DarkComboBox(), c2 = new DarkComboBox(), c3 = new DarkComboBox(); c1.Width = UI.S(230); c2.Width = UI.S(120); c3.Width = UI.S(140);
            fr.Feld("Grafikeinheit", c1); fr.Feld("Auflösung", c2); fr.Feld("Anzeige", c3);
            u.Inhalt.Controls.Add(fr);
            k.Breite(UI.S(640)); k.Karte.PerformLayout(); k.Breite(UI.S(640));
            Ist(k.Karte.Width == UI.S(640) && r1.Width == k.Innenbreite && r2.Width == k.Innenbreite, "Zeilen in Innenbreite");
            Ist(u.Width == k.Innenbreite - u.Margin.Horizontal && u.Margin.Left > 0, "Unterbereich eingerückt");
            Ist(fr.Zeilen == 1, "drei Felder nebeneinander bei 640 px: " + fr.Zeilen);
            int h1 = fr.Height;
            k.Breite(UI.S(420)); k.Karte.PerformLayout(); k.Breite(UI.S(420));
            Ist(fr.Zeilen == 2 && fr.Height > h1, "Umbruch bei 420 px: " + fr.Zeilen + " Zeilen");
            k.Breite(UI.S(640)); k.Karte.PerformLayout(); k.Breite(UI.S(640));
            Ist(fr.Zeilen == 1 && fr.Height == h1, "nach dem Verbreitern wieder eine Zeile und die alte Höhe");
            k.MinHoehe = UI.S(900);
            Ist(k.Karte.Height == UI.S(900), "Mindesthöhe für gleich hohe Karten");
        }
        using (SettingRow r = new SettingRow("Laufwerke", "Lesen und Schreiben"))
        {
            Ist(r.Icon == "", "ohne Symbol wie bisher");
            r.Icon = UI.IcoDisk; r.Width = UI.S(500);
            r.AddBadge(new Badge("läuft nebenher", "skip")); Badge d = new Badge("2 Min.", "skip"); r.AddBadge(d);
            Ist(d.Right <= r.Toggle.Left && d.Left > r.Controls[1].Left, "Dauer steht direkt neben dem Schalter");
            using (Bitmap bmp = new Bitmap(r.Width, r.Height)) r.DrawToBitmap(bmp, new Rectangle(0, 0, r.Width, r.Height));
        }
    }

    static void LaufwerkszeileSchalter()
    {
        using (LaufwerkZeile z = new LaufwerkZeile("Samsung SSD 980 1TB", "NVMe", "C: 300 GB frei", true))
        {
            int n = 0; z.CheckedChanged += delegate { n++; };
            Ist(z.Checked && z.Laufwerk == "Samsung SSD 980 1TB" && z.Anbindung == "NVMe", "Anfangszustand");
            z.Checked = true; Ist(n == 0, "gleicher Wert meldet nichts");
            z.Checked = false; Ist(!z.Checked && n == 1, "abgewählt, ein Ereignis");
            z.Width = UI.S(420);
            using (Bitmap bmp = new Bitmap(z.Width, z.Height)) z.DrawToBitmap(bmp, new Rectangle(0, 0, z.Width, z.Height));
        }
        using (LaufwerkZeile z = new LaufwerkZeile(null, null, null, false)) { z.Width = UI.S(200); using (Bitmap bmp = new Bitmap(z.Width, z.Height)) z.DrawToBitmap(bmp, new Rectangle(0, 0, z.Width, z.Height)); }
    }

    static void UmfangAuswahl()
    {
        using (FeldAuswahl f = new FeldAuswahl(new string[] { "Schnell", "Vollständig", "Eigene Auswahl" }, new string[] { "", "", "Prüfungen einzeln wählen" }))
        {
            int n = 0; f.SelectedChanged += delegate { n++; };
            Ist(f.Count == 3 && f.Selected == 0, "drei Felder, erstes gewählt");
            f.Selected = 2; Ist(f.Selected == 2 && n == 1, "Wahl mit Ereignis");
            f.Selected = 2; f.Selected = 7; f.Selected = -1; Ist(f.Selected == 2 && n == 1, "gleiche und ungültige Wahl ohne Wirkung");
            Ist(f.FeldBei(1) == 0 && f.FeldBei(f.Width / 2) == 1 && f.FeldBei(f.Width - 2) == 2 && f.FeldBei(f.Width + 5) == -1, "Feld an einer Stelle");
            f.SetUnterzeile(0, "4 Prüfungen  ·  ca. 4 Min."); Ist(f.Unterzeile(0).StartsWith("4 Prüfungen"), "Unterzeile");
            using (Bitmap bmp = new Bitmap(f.Width, f.Height)) f.DrawToBitmap(bmp, new Rectangle(0, 0, f.Width, f.Height));
        }
    }

    // D.1 aus 4.07: Die Pillen der Kopfkarte behielten die Höhe des ersten, schmalen Aufbaus
    static void UmbruchHoehe()
    {
        using (UmbruchPanel p = new UmbruchPanel())
        {
            p.Width = UI.S(220);
            for (int i = 0; i < 6; i++) { PillLabel l = new PillLabel("Laufwerk " + i); l.Width = UI.S(100); p.Controls.Add(l); }
            int schmal = p.Height;
            Ist(p.Zeilen == 3 && schmal > 0, "schmal: drei Zeilen, " + p.Zeilen);
            p.Width = UI.S(700);
            Ist(p.Zeilen == 1 && p.Height < schmal && p.Height * 3 <= schmal + UI.S(4), "breit: eine Zeile und entsprechend niedriger (" + p.Height + " statt " + schmal + ")");
            p.Controls.Clear();
            Ist(p.Height == 0 && p.Zeilen == 0, "leer ohne Höhe");
        }
    }

    static void ErgebnisBefundzeile()
    {
        string[] t = DiagGui.BefundTeile("[WARNUNG] RAM: XMP/EXPO inaktiv: Die Module laufen mit 2133 MT/s.");
        Ist(t[0] == "WARNUNG" && t[1] == "RAM" && t[2] == "XMP/EXPO inaktiv: Die Module laufen mit 2133 MT/s.", "Stufe, Bereich, Text: " + System.String.Join("|", t));
        t = DiagGui.BefundTeile("[kritisch] Laufwerk C: SMART meldet Fehler");
        Ist(t[0] == "KRITISCH" && t[1] == "Laufwerk C" && t[2] == "SMART meldet Fehler", "Stufe in Großbuchstaben");
        t = DiagGui.BefundTeile("freier Text"); Ist(t[0] == "" && t[2] == "freier Text", "ohne Klammer bleibt der Text");
        Ist(DiagGui.BefundTeile(null)[2] == "", "leer");
        Ist(DiagGui.DatumKurz("2026-11-10 14:20") == "10.11.2026 14:20" && DiagGui.DatumKurz("2026-11-10") == "10.11.2026" && DiagGui.DatumKurz("2026-11-10 14:20:33") == "10.11.2026 14:20", "Datum");
        Ist(DiagGui.DatumKurz("gestern") == "gestern" && DiagGui.DatumKurz(null) == "", "unbekannte Form bleibt");
        System.Collections.Generic.List<string[]> alle = new System.Collections.Generic.List<string[]>();
        alle.Add(new string[] { "HINWEIS", "A", "h1" }); alle.Add(new string[] { "WARNUNG", "B", "w1" }); alle.Add(new string[] { "KRITISCH", "C", "k1" });
        alle.Add(new string[] { "WARNUNG", "D", "w2" }); alle.Add(new string[] { "KRITISCH", "E", "k2" }); alle.Add(null);
        System.Collections.Generic.List<string[]> w = DiagGui.WichtigeBefunde(alle);
        Ist(w.Count == 4 && w[0][2] == "k1" && w[1][2] == "k2" && w[2][2] == "w1" && w[3][2] == "w2", "kritisch zuerst, sonst Reihenfolge des Laufs");
    }

    // Seite Benchmark: Schalter und Kontrollkästchen (Werte für Vorlagen und Start) bleiben gleich, Bereiche unter Grafik
    // und Laufwerken nur bei eingeschalteter Messung, Zeitschätzung und Vorlagen wie vor dem Umbau
    static void BenchmarkSeite()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, new string[] { "0|Samsung SSD 980 1TB|NVMe|SSD|931,5 GB|C:|0", "1|SanDisk Ultra|SATA|SSD|953,9 GB|D:|0", "2|Innostor|USB|Wechselmedium|28,9 GB|H:|1", "kaputt" }))
            {
                Ist(g.benchChk.Length == 4 && g.benchRows.Length == 4, "vier Messungen");
                Ist(g.diskZeilen.Count == 3 && g.diskZeilen[0].Checked && g.diskZeilen[1].Checked && !g.diskZeilen[2].Checked, "Laufwerke: USB zunächst abgewählt, ungültige Zeile entfällt");
                Ist(g.ModuleMinutes(1) == 2 + 1 + 2 + 2, "Schätzung 7 Min.: " + g.ModuleMinutes(1));
                Ist(g.benchSeite.Anzahl.Text == "4 von 4 Messungen" && g.benchSeite.Dauer.Text == "ca. 7 Min.", "Seitenkopf: " + g.benchSeite.Anzahl.Text + ", " + g.benchSeite.Dauer.Text);
                Ist(g.benchRows[3].Description.EndsWith("2 von 3 gewählt"), "Zeile Laufwerke nennt die Auswahl: " + g.benchRows[3].Description);
                Ist(g.subGpuOffen && g.subDiskOffen, "Bereiche unter Grafik und Laufwerken offen");
                // Schalter -> Wert
                g.benchRows[2].Toggle.UserToggle();
                Ist(!g.benchChk[2].Checked && !g.subGpuOffen && g.subDiskOffen, "Grafik aus: Wert folgt, Rendertest zu");
                Ist(g.benchSeite.Anzahl.Text == "3 von 4 Messungen" && g.ModuleMinutes(1) == 5, "drei Messungen, 5 Min.");
                // Wert -> Schalter (Vorlage)
                g.benchChk[3].Checked = false;
                Ist(!g.benchRows[3].Checked && !g.subDiskOffen && g.ModuleMinutes(1) == 3, "Laufwerke über den Wert aus: Schalter folgt, Liste zu");
                g.benchChk[3].Checked = true; g.diskZeilen[2].Checked = true;
                Ist(g.ModuleMinutes(1) == 6 && g.benchRows[3].Description.EndsWith("3 von 3 gewählt"), "dritter Datenträger zählt mit");
                // Vorlagen mit den bisherigen Schlüsseln
                System.Collections.Generic.Dictionary<string, object> v = g.CaptureSettings();
                Ist(v.ContainsKey("Bench.CPU") && v.ContainsKey("Bench.Disk") && v.ContainsKey("Bench.Referenz") && v.ContainsKey("Gpu.Aufloesung") && v.ContainsKey("Gpu.Auswahl"), "Schlüssel der Vorlagen");
                Ist(System.Convert.ToBoolean(v["Bench.GPU"]) == false && System.Convert.ToBoolean(v["Bench.Disk"]), "Werte der Vorlage");
                System.Collections.Generic.Dictionary<string, object> alt = new System.Collections.Generic.Dictionary<string, object>();
                alt["Bench.CPU"] = false; alt["Bench.RAM"] = true; alt["Bench.GPU"] = true; alt["Bench.Disk"] = false; alt["Gpu.Aufloesung"] = "1920x1080"; alt["Bench.Referenz"] = "Median:Desktop";
                g.ApplySettings(alt);
                Ist(!g.benchRows[0].Checked && g.benchRows[1].Checked && g.benchRows[2].Checked && !g.benchRows[3].Checked, "gespeicherte Vorlage stellt die Schalter");
                Ist(g.subGpuOffen && !g.subDiskOffen && System.Convert.ToString(g.CaptureSettings()["Bench.Referenz"]) == "Median:Desktop" && System.Convert.ToString(g.CaptureSettings()["Gpu.Aufloesung"]) == "1920x1080", "Referenz und Rendertest aus der Vorlage");
                g.ShowPage(2);
                Ist(g.benchSeite.Breite > 0 && g.benchSeite.Spalten.Width == g.benchSeite.Breite, "Seite auf Seitenbreite geordnet");
            }
        }
        finally { Raeume(d); }
    }

    // Seite Diagnose: Profil stellt die Schalter, ein umgelegter Schalter wählt die eigene Auswahl, Vorlagen wie bisher
    static void DiagnoseSeite()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                Ist(g.rbVoll.Checked && g.segUmfang.Selected == 1, "Vorgabe Vollständig");
                Ist(g.diagRows.Length == 8 && g.diagChk.Length == 8, "acht Prüfungen");
                foreach (SettingRow r in g.diagRows) Ist(r.Checked && r.Icon.Length > 0 && r.Title.Length > 0 && r.Description.Length > 0, "Zeile mit Symbol, Kurztitel, Beschreibung: " + r.Title);
                Ist(g.diagSeite.Anzahl.Text == "8 von 8 Prüfungen", g.diagSeite.Anzahl.Text);
                Ist(g.segUmfang.Unterzeile(0).StartsWith("4 Prüfungen") && g.segUmfang.Unterzeile(1).StartsWith("8 Prüfungen"), "Felder nennen Anzahl und Dauer: " + g.segUmfang.Unterzeile(0));
                int voll = g.ModuleMinutes(0);
                Ist(voll == 14 - 5, "Vollständig parallel 9 Min.: " + voll);
                g.chkFast.Checked = false;
                Ist(g.ModuleMinutes(0) == 14, "ohne parallele Ausführung 14 Min.");
                g.chkFast.Checked = true;
                // Auswahl des Umfangs -> Optionsfeld -> Schalter
                g.segUmfang.Selected = 0;
                Ist(g.rbSchnell.Checked && !g.rbVoll.Checked, "Schnell gewählt");
                Ist(g.diagRows[0].Checked && g.diagRows[1].Checked && !g.diagRows[2].Checked && !g.diagRows[3].Checked && !g.diagRows[4].Checked && g.diagRows[5].Checked && g.diagRows[6].Checked && !g.diagRows[7].Checked, "Profil Schnell stellt die Schalter");
                Ist(g.rbSchnell.Checked, "das Profil bleibt Schnell, auch wenn es Schalter umgestellt hat");
                Ist(g.diagSeite.Anzahl.Text == "4 von 8 Prüfungen", g.diagSeite.Anzahl.Text);
                // Schalter umlegen -> eigene Auswahl, die anderen Schalter bleiben
                g.diagRows[2].Toggle.UserToggle();
                Ist(g.rbCustom.Checked && g.segUmfang.Selected == 2, "umgelegter Schalter wechselt zur eigenen Auswahl");
                Ist(g.diagChk[2].Checked && g.diagChk[0].Checked && !g.diagChk[3].Checked, "nur diese Prüfung geändert");
                // Optionsfeld von außen (Vorlage, Startseite)
                g.rbVoll.Checked = true;
                Ist(g.segUmfang.Selected == 1 && !g.rbCustom.Checked, "Auswahl folgt dem Optionsfeld");
                foreach (SettingRow r in g.diagRows) Ist(r.Checked, "Vollständig: alle an");
                System.Collections.Generic.Dictionary<string, object> alt = new System.Collections.Generic.Dictionary<string, object>();
                alt["Diag.Profil"] = "Benutzerdefiniert"; alt["Diag.Ereignisse"] = true; alt["Diag.Updatesuche"] = false; alt["Diag.Integritaet"] = false; alt["Diag.Defender"] = true;
                alt["Diag.SmartLang"] = false; alt["Diag.Netzwerk"] = false; alt["Diag.RamTest"] = false; alt["Diag.Energieanalyse"] = false; alt["Diag.SchnellerModus"] = false; alt["Diag.Speicherdiagnose"] = true;
                g.ApplySettings(alt);
                Ist(g.rbCustom.Checked && g.segUmfang.Selected == 2, "Vorlage mit eigener Auswahl");
                Ist(g.diagRows[0].Checked && !g.diagRows[1].Checked && g.diagRows[3].Checked && !g.diagRows[7].Checked && !g.chkFast.Checked, "Vorlage stellt Schalter und Ausführung");
                System.Collections.Generic.Dictionary<string, object> v = g.CaptureSettings();
                Ist(System.Convert.ToString(v["Diag.Profil"]) == "Benutzerdefiniert" && System.Convert.ToBoolean(v["Diag.Defender"]) && System.Convert.ToBoolean(v["Diag.Speicherdiagnose"]) && !System.Convert.ToBoolean(v["Diag.SchnellerModus"]), "Vorlage wieder gelesen");
                alt.Clear(); alt["Diag.Profil"] = "Funktionstest";
                g.ApplySettings(alt);
                Ist(g.rbSchnell.Checked, "Vorlage vor v3.64 mit entfallenem Profil lädt als Schnell");
            }
            int n = 0; foreach (bool b in DiagGui.diagNebenher) if (b) n++;
            Ist(DiagGui.diagNebenher.Length == 8 && n == 4, "vier Prüfungen laufen nebenher");
        }
        finally { Raeume(d); }
    }

    // Eintrag Ergebnis in der Seitenleiste, Rückkehr ohne Neuaufbau, Band auf der Seite Benchmark
    static void ErgebnisSeitenleiste()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                Ist(g.erg == null && !g.ergebnisNavSichtbar, "ohne Lauf kein Eintrag Ergebnis");
                Ist(g.ergebnisPage > 5 && g.nav.Count == 11 && !g.nav.Contains(g.navErgebnis), "Ergebnis ist eine eigene Seite hinter den bisherigen, die Stellen der Module bleiben");
                g.ZeigeLetztesErgebnis();
                Ist(g.curPage != g.ergebnisPage, "ohne Ergebnis bleibt die Seite");
                SchreibeLauf(d, "2026-11-10 14:20", true, true);
                g.ReloadDb();
                Ist(g.erg != null && g.ergebnisNavSichtbar && !g.erg.VomLauf, "mit Lauf in der Datenbank erscheint der Eintrag");
                Ist(g.navErgebnis.Hint == g.erg.Score.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("de-DE")) && g.erg.Score > 0 && g.navErgebnis.HintPille, "Punktzahl als Pille: " + g.navErgebnis.Hint);
                Ist(g.ergebnisAufbauten == 0, "Aufbau erst beim Öffnen der Seite");
                g.ZeigeLetztesErgebnis();
                Ist(g.curPage == g.ergebnisPage && g.navErgebnis.Selected && g.ergebnisAufbauten == 1, "Seite gezeigt, Eintrag markiert, einmal aufgebaut");
                for (int i = 0; i < g.nav.Count; i++) Ist(!g.nav[i].Selected, "kein Modul markiert, solange das Ergebnis zu sehen ist");
                g.ShowPage(3); Ist(!g.navErgebnis.Selected && g.nav[3].Selected, "Wechsel weg");
                g.ShowPage(g.ergebnisPage);
                Ist(g.ergebnisAufbauten == 1, "Rückkehr ohne Neuaufbau");
                // Referenz ändern: Seite Benchmark mit Band, das Band gilt bis zum nächsten Seitenwechsel
                g.ZeigeReferenzwahl();
                Ist(g.curPage == 2 && g.bandErgGezeigt, "Seite Benchmark mit Band");
                Ist(HatText(g.bandErg, "10.11.2026 14:20") && HatText(g.bandErg, "Punkte") && HatText(g.bandErg, "ab dem nächsten Lauf"), "Band nennt Datum, Punktzahl und den Hinweis zur Referenz");
                g.ZeigeLetztesErgebnis();
                Ist(g.curPage == g.ergebnisPage && !g.bandErgGezeigt && g.ergebnisAufbauten == 1, "zurück zum Ergebnis, Band weg, kein Neuaufbau");
                g.ZeigeReferenzwahl(); g.ShowPage(2);
                Ist(!g.bandErgGezeigt, "Seite Benchmark über die Seitenleiste: ohne Band");
                // neuer Lauf in der Datenbank: Seite baut einmal neu
                SchreibeLauf(d, "2026-11-11 09:00", false, true);
                g.ReloadDb(); g.ShowPage(g.ergebnisPage);
                Ist(g.erg.Datum == "2026-11-11 09:00" && g.ergebnisAufbauten == 2, "neuer letzter Lauf, ein weiterer Aufbau");
            }
        }
        finally { Raeume(d); }
    }

    // Ergebnis aus einem Datenbankeintrag mit den Feldern von 4.08 und aus älteren Einträgen ohne sie
    static void ErgebnisDatenbank()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                SchreibeLauf(d, "2026-11-10 14:20", true, true);
                g.ReloadDb(); g.ZeigeLetztesErgebnis();
                Ist(g.erg.HatVergleich && g.erg.Pct.Count == 3 && g.erg.Pct["CPU"] == 103 && g.erg.Pct["GPU"] == 237 && g.erg.Pct["RAM"] == 174, "Prozent je Gruppe wie gespeichert");
                Ist(g.erg.RefName == "Median Desktop-PCs (7 Systeme)", "Name der Referenz: " + g.erg.RefName);
                Ist(g.erg.NK == 1 && g.erg.NW == 1 && g.erg.NI == 2 && g.erg.Befunde.Count == 2 && g.erg.Befunde[0][0] == "KRITISCH" && g.erg.Befunde[1][1] == "RAM", "Befunde: kritisch zuerst, Hinweise nicht in der Liste");
                Ist(g.lblResVerdict.Text == "1 kritischer Befund", g.lblResVerdict.Text);
                Ist(HatText(g.kResCmp.Karte, "100 % = Median Desktop-PCs (7 Systeme)") && HatText(g.kResCmp.Karte, "103 %") && HatText(g.kResCmp.Karte, "nicht gemessen"), "Vergleich mit Referenz, Werten und der nicht gemessenen Gruppe");
                Ist(HatText(g.kResFind.Karte, "SMART meldet Fehler") && HatText(g.kResFind.Karte, "XMP inaktiv"), "Befunde stehen in der Karte");
                Control letzter = g.kResFind.Inhalt.Controls[g.kResFind.Inhalt.Controls.Count - 1];
                Ist(letzter == g.lnkResAll, "der Verweis auf den Bericht steht unter den Befunden");
                Ist(g.pnlResTiles.Controls.Count == 3, "Kacheln nur für gemessene Komponenten: " + g.pnlResTiles.Controls.Count);
                Ist(g.pnlDashPills.Controls.Count == 4 && g.pnlDashPills.Height > 0 && g.cardResHero.Height < UI.S(160), "Kopfkarte ohne Leerraum: " + g.cardResHero.Height);
                Ist(g.resultDashboard.Height > g.pnlResTiles.Bottom, "Seite reicht bis unter die Knöpfe");

                // Lauf vor 4.08: Befunde gespeichert, Vergleich nicht
                SchreibeLauf(d, "2026-11-11 08:00", false, true);
                g.ReloadDb();
                Ist(!g.erg.HatVergleich && g.erg.Pct.Count == 0 && g.erg.RefName == "", "ohne Feld Vergleich keine Prozentwerte");
                Ist(HatText(g.kResCmp.Karte, "nicht gespeichert") && HatText(g.kResCmp.Karte, "Bericht") && !HatText(g.kResCmp.Karte, "100 % ="), "statt Zahlen steht, was fehlt");
                Ist(g.erg.Befunde.Count == 2 && g.pnlResTiles.Controls.Count == 3, "Befunde und Kacheln bleiben");

                // Eintrag ohne Befunde (Import): nichts erfinden
                SchreibeLauf(d, "2026-11-12 08:00", false, false);
                g.ReloadDb();
                Ist(g.erg.NK < 0 && g.erg.Befunde.Count == 0 && g.lblResVerdict.Text == "Befunde nicht gespeichert", "ohne Befunde: " + g.lblResVerdict.Text);
                Ist(HatText(g.kResFind.Karte, "nicht gespeichert"), "Karte nennt das Fehlen");
            }
            // Lesen der Datenbank selbst: alte Einträge bleiben vollständig, neue Felder sind optional
            SchreibeLauf(d, "2026-11-09 08:00", true, true);
            System.Collections.Generic.List<DbEntry> l = DbEntry.Load(System.IO.Path.Combine(d, "Datenbank"));
            Ist(l.Count == 4, "vier Einträge");
            int mit = 0; foreach (DbEntry e in l) { Ist(e.HasBench && e.Get("CPU|MT") == 32433, "Werte gelesen"); if (e.HatVergleich) mit++; else Ist(e.GruppenPct.Count == 0 && e.RefName == "", "ohne Feld leer"); }
            Ist(mit == 2, "zwei Einträge mit Vergleich: " + mit);
            foreach (string f in System.IO.Directory.GetFiles(System.IO.Path.Combine(root, "tests/Daten/Datenbank".Replace('/', System.IO.Path.DirectorySeparatorChar)), "*.json"))
            {
                string ziel = System.IO.Path.Combine(System.IO.Path.Combine(d, "Datenbank"), System.IO.Path.GetFileName(f)); System.IO.File.Copy(f, ziel, true);
            }
            l = DbEntry.Load(System.IO.Path.Combine(d, "Datenbank"));
            Ist(l.Count == 7, "Einträge im Format 1 und 2 nebeneinander: " + l.Count);
            foreach (DbEntry e in l) if (e.Computer == "TORRENT") Ist(!e.HatVergleich && e.NW >= 0 && e.BefundListe.Count > 0, "Eintrag im Format 1: Befunde gelesen, kein Vergleich");
        }
        finally { Raeume(d); }
    }
}
