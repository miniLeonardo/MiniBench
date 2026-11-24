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
        Fall("ToggleSwitch.Rand", ToggleRand);
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
        Fall("Lasttest.Seite", LasttestSeite);
        Fall("Lasttest.Sensoren", LasttestSensoren);
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
        Fall("Umzug.Seite", UmzugSeite);
        Fall("Umzug.Platz", UmzugPlatz);
        Fall("Modulseite.Kopf", ModulseiteKopf);
        Fall("Liste.Zeilen", ListeZeilen);
        Fall("Aenderungen.Buendelung", AenderungenBuendelung);
        Fall("Aenderungen.Seite", AenderungenSeite);
        Fall("Sensoren.Seite", SensorenSeite);
        Fall("Datenbank.Seite", DatenbankSeite);
        Fall("Versionen.Spaeter", VersionenSpaeter);
        Fall("Hintergrund.Sperre", HintergrundSperre);
        Fall("Update.Pruefsumme", UpdatePruefsumme);
        Fall("Tastenkuerzel", Tastenkuerzel);
        Fall("Ergebnis.Dauer", ErgebnisDauer);
        Fall("Startseite.Neuaufbau", StartseiteNeuaufbau);
        Fall("Aenderungen.Geraet", AenderungenGeraet);
        Fall("Update.WasIstNeu", UpdateWasIstNeu);
        Fall("Datenbank.Tasten", DatenbankTasten);
        Fall("Sensoren.Hervorhebung", SensorenHervorhebung);
        Fall("Hintergrund.Startknopf", HintergrundStartknopf);
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

    // ab v4.11: Protokoll gehört über die Gerätekennung zu diesem PC (auch nach Umbenennen), "fehlgeschlagen" bleibt offen
    static void AenderungenGeraet()
    {
        string alt = ChangeEntry.EigeneGeraetId;
        try
        {
            string hier = System.Environment.MachineName;
            ChangeEntry.EigeneGeraetId = "G-EIGEN";
            Ist(ChangeEntry.IstDiesesGeraet("ALTERNAME", "G-EIGEN"), "umbenannter PC mit gleicher Kennung gilt als dieser PC");
            Ist(!ChangeEntry.IstDiesesGeraet(hier, "G-FREMD"), "gleicher Name, andere Kennung gilt als anderer PC");
            Ist(ChangeEntry.IstDiesesGeraet(hier, ""), "Protokoll ohne Kennung: Name entscheidet");
            Ist(!ChangeEntry.IstDiesesGeraet("ANDERER", ""), "Protokoll ohne Kennung, anderer Name");
            ChangeEntry.EigeneGeraetId = "";
            Ist(ChangeEntry.IstDiesesGeraet(hier, "G-FREMD"), "eigene Kennung unbekannt: Name entscheidet");
            ChangeEntry.EigeneGeraetId = "G-EIGEN";
            ChangeEntry a = new ChangeEntry(); a.Computer = "ALTERNAME"; a.GeraetId = "G-EIGEN"; a.Status = "fehlgeschlagen"; a.FilePath = "x.json"; a.Lauf = "2026-11-01 10:00:00";
            Ist(a.Undoable, "fehlgeschlagener Eintrag dieses Geräts ist erneut zurücknehmbar");
            List<ChangeLauf> l = ChangeLauf.Gruppiere(new List<ChangeEntry> { a }, null);
            Ist(l.Count == 1 && l[0].DieserPc && l[0].Aktiv == 1, "Lauf mit offenem Eintrag gehört zu diesem PC");
            Ist(ChangeLauf.Aufzuraeumen(l, true, 0, true, System.DateTime.Now).Count == 0, "Aufräumen lässt offenes Protokoll dieses Geräts liegen");
        }
        finally { ChangeEntry.EigeneGeraetId = alt; }
    }

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

    // ab v4.09: Die Pille liegt mit Abstand im Element. Bis v4.08 füllte sie es ganz aus; Kontur und Fokusrahmen lagen
    // auf oder außerhalb der Kante und wurden rechts und unten abgeschnitten. In der Zeile steht sie an derselben Stelle.
    static void ToggleRand()
    {
        using (ToggleSwitch t = new ToggleSwitch())
        {
            Rectangle p = t.Pille; int r = UI.S(ToggleSwitch.Rand);
            Ist(p.Width == UI.S(38) && p.Height == UI.S(20), "Pille 38 x 20: " + p);
            Ist(r >= 3 && p.Left >= r && p.Top >= r && p.Right <= t.Width - r && p.Bottom <= t.Height - r, "Abstand der Pille zur Kante: " + p + " in " + t.Size);
            Ist(p.Left - UI.S(2) >= 0 && p.Top - UI.S(2) >= 0 && p.Right + UI.S(2) <= t.Width - 1 && p.Bottom + UI.S(2) <= t.Height - 1, "Fokusrahmen liegt im Element");
            // von außen kleiner gesetzt: Die Pille schrumpft mit, statt an der Kante abgeschnitten zu werden
            t.Size = new Size(UI.S(30), UI.S(16));
            p = t.Pille;
            Ist(p.Left >= r && p.Top >= r && p.Right <= t.Width - r && p.Bottom <= t.Height - r, "kleines Element: Pille bleibt innen: " + p);
        }
        using (ToggleSwitch sw = new ToggleSwitch())
        using (SettingRow row = new SettingRow("Titel", "Beschreibung der Zeile", sw))
        {
            row.Width = 500;
            Ist(sw.Left + sw.Pille.Right == row.Width - UI.S(16), "Pille endet wie bisher 16 px vor dem Rand der Zeile: " + (sw.Left + sw.Pille.Right));
            Ist(sw.Left >= 0 && sw.Right <= row.Width && sw.Top >= 0 && sw.Bottom <= row.Height, "Schalter liegt ganz in der Zeile: " + sw.Bounds + " in " + row.Size);
            Badge b = new Badge("2 Min.", "skip"); row.AddBadge(b);
            Ist(b.Right == sw.Left + sw.Pille.Left - UI.S(10), "Etikett 10 px links der Pille: " + b.Right);
        }
        using (FluentCard c = new FluentCard("Testkarte", "Beschreibung", UI.IcoDiag))
        {
            c.Width = 400;
            Ist(c.Toggle.Left >= 0 && c.Toggle.Right <= c.Width && c.Toggle.Top >= 0 && c.Toggle.Bottom <= c.Height, "Schalter liegt ganz in der Karte");
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

    // ab v4.09: Seite Lasttest im Muster der Seite Benchmark. Schalter und Kontrollkästchen (Werte für Vorlagen und
    // Start) bleiben gleich, Dauer und Einstellungen stehen nur unter eingeschalteten Komponenten, der Kopf nennt Anzahl
    // und Gesamtdauer, Vorlagen mit den bisherigen Schlüsseln
    static void LasttestSeite()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, new string[] { "0|Samsung SSD 980 1TB|NVMe|SSD|931,5 GB|C:|0" }))
            {
                Ist(g.lastRows.Length == 4 && g.lastSub.Length == 4, "vier Komponenten");
                foreach (SettingRow r in g.lastRows) Ist(r.Icon.Length > 0 && r.Title.Length > 0 && r.Description.Length > 0, "Zeile mit Symbol, Titel, Beschreibung: " + r.Title);
                Ist(g.lastRows[0].Checked && g.lastRows[1].Checked && g.lastRows[2].Checked && !g.lastRows[3].Checked, "Vorgabe: Datenträger aus");
                Ist(g.lastSubOffen[0] && g.lastSubOffen[1] && g.lastSubOffen[2] && !g.lastSubOffen[3], "Einstellungen nur unter eingeschalteten Komponenten");
                Ist(g.lastSeite.Anzahl.Text == "3 von 4 Komponenten" && g.lastSeite.Dauer.Text == "Gesamtdauer 2 Min.", "Seitenkopf: " + g.lastSeite.Anzahl.Text + ", " + g.lastSeite.Dauer.Text);
                Ist(g.ModuleMinutes(2) == 3, "Schätzung des Moduls 3 Min. (Gesamtdauer und Vorbereitung): " + g.ModuleMinutes(2));
                // Schalter -> Wert
                g.lastRows[3].Toggle.UserToggle();
                System.Collections.Generic.Dictionary<string, object> v = g.CaptureSettings();
                Ist(System.Convert.ToBoolean(v["Last.Disk"]) && g.lastSubOffen[3] && g.lastSeite.Anzahl.Text == "4 von 4 Komponenten", "Datenträger an: Wert folgt, Einstellungen offen");
                foreach (string k in new string[] { "Last.CPU", "Last.CPUDauer", "Last.RAM", "Last.RAMDauer", "Last.RAMAnteil", "Last.GPU", "Last.GPUDauer", "Last.Disk", "Last.DiskDauer", "Last.Laufwerk", "Last.AbbruchCpu", "Last.AbbruchGpu" })
                    Ist(v.ContainsKey(k), "Schlüssel der Vorlagen: " + k);
                // Wert -> Schalter und Dauer (Vorlage)
                System.Collections.Generic.Dictionary<string, object> alt = new System.Collections.Generic.Dictionary<string, object>();
                alt["Last.CPU"] = false; alt["Last.RAM"] = true; alt["Last.GPU"] = true; alt["Last.Disk"] = false; alt["Last.GPUDauer"] = "15 Min."; alt["Last.RAMDauer"] = "5 Min.";
                g.ApplySettings(alt);
                Ist(!g.lastRows[0].Checked && g.lastRows[1].Checked && g.lastRows[2].Checked && !g.lastRows[3].Checked, "Vorlage stellt die Schalter");
                Ist(!g.lastSubOffen[0] && g.lastSubOffen[2] && !g.lastSubOffen[3], "Bereiche folgen der Vorlage");
                Ist(g.lastSeite.Anzahl.Text == "2 von 4 Komponenten" && g.lastSeite.Dauer.Text == "Gesamtdauer 15 Min.", "längste Dauer bestimmt die Gesamtdauer: " + g.lastSeite.Dauer.Text);
                Ist(g.ModuleMinutes(2) == 16, "Schätzung des Moduls 16 Min.: " + g.ModuleMinutes(2));
                // alle aus
                foreach (SettingRow r in g.lastRows) if (r.Checked) r.Toggle.UserToggle();
                Ist(g.lastSeite.Anzahl.Text == "0 von 4 Komponenten" && g.lastSeite.Dauer.Text == "keine Komponente gewählt", "nichts gewählt: " + g.lastSeite.Dauer.Text);
                g.ShowPage(3);
                Ist(g.lastSeite.Breite > 0 && g.lastSeite.Spalten.Width == g.lastSeite.Breite, "Seite auf Seitenbreite geordnet");
            }
        }
        finally { Raeume(d); }
    }

    // Karte Sensoren der Seite Lasttest: nennt, woher die CPU-Werte kommen, je nach Einstellung und installiertem Treiber
    static void LasttestSensoren()
    {
        string a = DiagGui.LastSensorText(true, ""), b = DiagGui.LastSensorText(true, "2.0.1"), c = DiagGui.LastSensorText(false, "2.0.1");
        Ist(a.Contains("vorübergehend installiert") && a.Contains("wieder entfernt"), "ohne installierten Treiber: " + a);
        Ist(b.Contains("PawnIO 2.0.1 ist installiert") && b.Contains("bleibt unverändert") && !b.Contains("vorübergehend"), "installierter Treiber bleibt: " + b);
        Ist(c.Contains("wird nicht verwendet") && !c.Contains("installiert"), "Treiber abgewählt: " + c);
        Ist(DiagGui.LastSensorText(true, null) == a, "ohne Angabe wie nicht installiert");
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
        Ist(DiagGui.AblageKurz("\\\\nas.example.org\\home\\nutzer\\Desktop\\LMB\\Minibench-Daten") == "Ablage: ...\\LMB\\Minibench-Daten", DiagGui.AblageKurz("\\\\nas.example.org\\home\\nutzer\\Desktop\\LMB\\Minibench-Daten"));
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
    static DiagGui NeueOberflaeche(string d, string[] disks) { return new DiagGui("", "", "", d, "4.10", disks ?? new string[0], new string[0], "G-test"); }

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

    // ab v4.09: Seite System-Umzug im Muster der Seite Benchmark. Zeilen mit Schaltern je Bibliothek und Bereich, der Kopf
    // nennt die Anzahl; die Bereiche unter Profile und WLAN stehen nur bei eingeschalteter Zeile. Ohne Schnell-Scan nennt
    // die Seite keine Größe.
    static void UmzugSeite()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                Ist(g.umzugLibRows.Length == 6 && g.umzugSysRows.Length == 4, "sechs Bibliotheken, vier Bereiche");
                foreach (SettingRow r in g.umzugLibRows) Ist(r.Icon.Length > 0 && r.Title.Length > 0 && r.Description.Length > 0, "Zeile mit Symbol, Titel, Beschreibung: " + r.Title);
                foreach (SettingRow r in g.umzugSysRows) Ist(r.Icon.Length > 0 && r.Title.Length > 0 && r.Description.Length > 0, "Zeile mit Symbol, Titel, Beschreibung: " + r.Title);
                Ist(g.umzugLibRows[0].Checked && !g.umzugLibRows[2].Checked && g.umzugLibRows[5].Checked, "Downloads zunächst aus");
                Ist(g.umzugSysRows[0].Checked && g.umzugSysRows[1].Checked && !g.umzugSysRows[2].Checked && g.umzugSysRows[3].Checked, "Profile zunächst aus");
                Ist(g.umzugSeite.Anzahl.Text == "8 von 10 Bereichen", g.umzugSeite.Anzahl.Text);
                Ist(g.umzugSeite.Dauer.Text == "Größe nach dem Schnell-Scan" && g.lblUmzugGroesse.Text == "Gesamtgröße: nach dem Schnell-Scan", "ohne Scan keine Größe: " + g.lblUmzugGroesse.Text);
                Ist(g.lblUmzugPlatz.Text.Length > 0 && !g.lblUmzugPlatz.Text.Contains("usreichend"), "ohne Scan kein Urteil zum Platz: " + g.lblUmzugPlatz.Text);
                Ist(!g.subUmzugProfileOffen && g.subUmzugWlanOffen, "Bereich unter WLAN offen, unter Profile zu");
                g.umzugSysRows[3].Toggle.UserToggle();
                Ist(!g.subUmzugWlanOffen && g.umzugSeite.Anzahl.Text == "7 von 10 Bereichen", "WLAN aus: Bereich zu, " + g.umzugSeite.Anzahl.Text);
                g.umzugSysRows[2].Toggle.UserToggle(); g.umzugLibRows[2].Toggle.UserToggle();
                Ist(g.subUmzugProfileOffen && g.umzugSeite.Anzahl.Text == "9 von 10 Bereichen", "Profile und Downloads an: " + g.umzugSeite.Anzahl.Text);
                Ist(g.karteUmzugZusatz != null && HatText(g.karteUmzugZusatz.Karte, "Schnell-Scan"), "Karte Zusatzordner nennt den Schnell-Scan");
                // Farbschema wechseln: Die Seite rechnet neu, ohne Fehler und mit gleichem Stand
                g.umzugRechne();
                Ist(g.umzugSeite.Anzahl.Text == "9 von 10 Bereichen", "nach erneutem Rechnen gleich");
                g.ShowPage(8);
                Ist(g.umzugSeite.Breite > 0 && g.umzugSeite.Spalten.Width == g.umzugSeite.Breite, "Seite auf Seitenbreite geordnet");
            }
        }
        finally { Raeume(d); }
    }

    // Zeile zum freien Platz: "ausreichend" nur mit gemessenem Umfang und gemessenem freiem Platz. Bis v4.08 stand bei
    // einem Netzlaufwerk fest "100,0 GB (Ausreichend)" und ohne Zielmedium "0,0 GB (Ausreichend)".
    static void UmzugPlatz()
    {
        int st; long gb = 1073741824L;
        Ist(DiagGui.UmzugPlatzText("", "", 0, 5 * gb, true, out st) == "Kein Zielmedium gewählt" && st == 3, "ohne Ziel");
        string n = DiagGui.UmzugPlatzText("\\\\nas\\sicherung", "Netzwerk", 0, 5 * gb, true, out st);
        Ist(n.Contains("nicht geprüft") && !n.Contains("GB") && st == 0, "Netzlaufwerk ohne erfundenen Platz: " + n);
        string u = DiagGui.UmzugPlatzText("\\\\server\\ordner", "", 0, 5 * gb, true, out st);
        Ist(u.Contains("nicht lesbar") && st == 0, "Freigabe ohne lesbares Laufwerk: " + u);
        string ok = DiagGui.UmzugPlatzText("E:\\", "exFAT", 29 * gb, 5 * gb, true, out st);
        Ist(ok == "Frei auf dem Ziel: 29,0 GB, ausreichend" && st == 1, "genug Platz: " + ok);
        string eng = DiagGui.UmzugPlatzText("E:\\", "exFAT", 3 * gb, 5 * gb, true, out st);
        Ist(eng == "Frei auf dem Ziel: 3,0 GB, nicht ausreichend" && st == 2, "zu wenig Platz: " + eng);
        string vor = DiagGui.UmzugPlatzText("E:\\", "exFAT", 29 * gb, 30 * 1048576L, false, out st);
        Ist(vor == "Frei auf dem Ziel: 29,0 GB" && st == 0, "vor dem Scan ohne Urteil: " + vor);
        string vorEng = DiagGui.UmzugPlatzText("E:\\", "exFAT", gb / 100, 30 * 1048576L, false, out st);
        Ist(vorEng.EndsWith("nicht ausreichend") && st == 2, "zu wenig Platz schon für die geschätzten Bereiche: " + vorEng);
    }

    // Kopf der Modulseiten: Anzahl und Dauer stehen rechtsbündig, auch wenn der Text kürzer wird (bis v4.08 blieb dann
    // rechts eine Lücke, weil sich die Lage nach der noch nicht angepassten Größe eines Felds richtete)
    static void ModulseiteKopf()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                DiagGui.ModulSeite m = g.umzugSeite;
                m.Ordne(1000);
                Ist(m.Anzahl.Left + m.Anzahl.GetPreferredSize(Size.Empty).Width == 1000 && m.Dauer.Left + m.Dauer.GetPreferredSize(Size.Empty).Width == 1000, "rechtsbündig bei 1000");
                int vorher = m.Dauer.Left;
                m.Dauer.Text = "rund 2 GB";
                Ist(m.Dauer.Left > vorher && m.Dauer.Left + m.Dauer.GetPreferredSize(Size.Empty).Width == 1000, "kürzerer Text rückt nach rechts: " + m.Dauer.Left);
                Ist(m.Dauer.Top >= m.Anzahl.Top + m.Anzahl.GetPreferredSize(Size.Empty).Height, "Dauer steht unter der Anzahl");
                Ist(m.Kopf.Height >= m.Dauer.Top + m.Dauer.GetPreferredSize(Size.Empty).Height, "Kopf so hoch wie beide Zeilen");
            }
        }
        finally { Raeume(d); }
    }

    // Selbst gezeichnete Liste (ab v4.10): Maße, Markierung, Ereignisse, Höhe ohne eigenen Bildlauf
    static void ListeZeilen()
    {
        ZeilenListe l = new ZeilenListe(); l.Size = new Size(600, 200); l.ZeilenHoehe = 30;
        l.Spalten.Add(new ListenSpalte("Name", 0, false, 0)); l.Spalten.Add(new ListenSpalte("Wert", 100, true, 7));
        for (int i = 0; i < 20; i++) { ListenZeile z = new ListenZeile(); z.Tag = i; z.Haken = 0; z.Aufklappbar = i == 0; l.Zeilen.Add(z); }
        l.Aktualisiere();
        Ist(l.InhaltHoehe == 600 && l.KopfHoehe > 0, "Inhalt 20 Zeilen: " + l.InhaltHoehe);
        Rectangle r = new Rectangle(0, 0, 580, 30);
        Rectangle c1 = l.Zelle(1, r), c0 = l.Zelle(0, r);
        Ist(c1.Width == 100 && c1.Right <= 580 && c0.Right == c1.X && c0.X == 0, "Spalte 1 rechts, Spalte 0 nimmt den Rest: " + c0 + " " + c1);
        l.Rollen = 100000;
        Ist(l.Rollen == l.InhaltHoehe - (200 - l.KopfHoehe - 2), "Bildlauf endet am Inhalt: " + l.Rollen);
        int haken = 0, auf = 0, mark = 0;
        l.HakenGeklickt += delegate(ListenZeile z) { haken++; }; l.AufklappGeklickt += delegate(ListenZeile z) { auf++; }; l.MarkierungGeaendert += delegate { mark++; };
        l.Zeilen[3].Gesperrt = true;
        l.LoeseHaken(l.Zeilen[2]); l.LoeseHaken(l.Zeilen[3]); l.LoeseAufklappen(l.Zeilen[0]); l.LoeseAufklappen(l.Zeilen[1]);
        Ist(haken == 1 && auf == 1, "gesperrtes Kästchen und Zeile ohne Pfeil melden nichts: " + haken + "/" + auf);
        l.Markiere(l.Zeilen[5], true);
        Ist(mark == 1 && l.Markiert.Count == 1 && l.Fokus == l.Zeilen[5], "Markierung");
        l.Zeilen.RemoveAt(5); l.Aktualisiere();
        Ist(l.Markiert.Count == 0 && l.Fokus == null, "entfernte Zeile ist nicht mehr markiert");
        ZeilenListe a = new ZeilenListe(); a.AutoHoehe = true; a.Rahmen = false; a.ZeilenHoehe = 26;
        for (int i = 0; i < 7; i++) a.Zeilen.Add(new ListenZeile());
        a.Aktualisiere();
        Ist(a.Height == 7 * 26, "Höhe folgt dem Inhalt: " + a.Height);
        KennzahlZeile k = new KennzahlZeile(new string[] { "A", "B" }, new string[] { "W", "W" }, new Color[] { Color.Red, Color.Blue });
        Ist(k.Wert(0) == "" && k.Anzahl == 2, "Kennzahl beginnt ohne Wert");
        k.Setze(1, "42", "Min 1"); Ist(k.Wert(1) == "42" && k.Unter(1) == "Min 1", "Kennzahl gesetzt");
        k.Leere(); Ist(k.Wert(1) == "", "Kennzahl geleert");
    }

    static ChangeEntry Aend(string datei, string pc, string lauf, int id, string schritt, string titel, string art, string status)
    {
        ChangeEntry c = new ChangeEntry(); c.FilePath = datei; c.Computer = pc; c.Lauf = lauf; c.Id = id; c.Zeit = lauf; c.Schritt = schritt; c.Titel = titel; c.Art = art; c.Status = status;
        c.Modul = "Optimierung"; c.Risiko = "Aendern"; c.Ziel = "HKLM:\\Ziel" + id; c.Vorher = "1"; c.Nachher = "0";
        return c;
    }

    // Bündelung des Änderungsprotokolls nach Lauf und Maßnahme und die Regeln des Aufräumens (wie Move-ChangeArchiv)
    static void AenderungenBuendelung()
    {
        string pc = System.Environment.MachineName;
        System.Collections.Generic.List<ChangeEntry> e = new System.Collections.Generic.List<ChangeEntry>();
        e.Add(Aend("a.json", pc, "2026-11-10 14:31:00", 2, "Telemetrie", "Diagnosedaten auf das Minimum", "Registry", "aktiv"));
        e.Add(Aend("a.json", pc, "2026-11-10 14:31:00", 1, "Telemetrie", "Diagnosedaten auf das Minimum", "Registry", "aktiv"));
        e.Add(Aend("a.json", pc, "2026-11-10 14:31:00", 3, "Dienste", "Dienste auf manuell", "Dienststart", "aktiv"));
        e.Add(Aend("a.json", pc, "2026-11-10 14:31:00", 4, "Dienste", "Dienste auf manuell", "Dienststart", "rückgängig"));
        e.Add(Aend("a.json", pc, "2026-11-10 14:31:00", 5, "Punkt", "Wiederherstellungspunkt", "Wiederherstellungspunkt", "nur Hinweis"));
        e.Add(Aend("b.json", pc, "2026-09-01 10:00:00", 1, "X", "Alt", "Registry", "rückgängig"));
        e.Add(Aend("c.json", "ANDERER-PC", "2026-11-12 09:00:00", 1, "X", "Fremd", "Registry", "aktiv"));
        System.Collections.Generic.List<string[]> dateien = new System.Collections.Generic.List<string[]>();
        dateien.Add(new string[] { "leer.json", pc, "2026-08-01 08:00:00" });
        System.Collections.Generic.List<ChangeLauf> l = ChangeLauf.Gruppiere(e, dateien);
        Ist(l.Count == 4 && l[0].FilePath == "c.json" && l[1].FilePath == "a.json" && l[3].FilePath == "leer.json", "vier Läufe, neueste zuerst: " + l.Count + " " + l[0].FilePath);
        ChangeLauf a = l[1];
        Ist(a.Massnahmen.Count == 3 && a.Massnahmen[0].Eintraege.Count == 2 && a.Massnahmen[0].Eintraege[0].Id == 1, "Maßnahmen in der Reihenfolge des Laufs");
        Ist(a.Massnahmen[0].Umfang == "2 Registry-Werte" && a.Massnahmen[1].Umfang == "2 Dienste" && a.Massnahmen[2].Umfang == "HKLM:\\Ziel5", "Umfang: " + a.Massnahmen[0].Umfang + " / " + a.Massnahmen[2].Umfang);
        Ist(a.Massnahmen[0].StatusText == "aktiv" && a.Massnahmen[1].StatusText == "1 von 2 aktiv" && a.Massnahmen[1].Gemischt, "Status der Maßnahme: " + a.Massnahmen[1].StatusText);
        Ist(a.Aktiv == 3 && a.StatusText.StartsWith("3 aktiv") && a.DieserPc && !l[0].DieserPc && a.Module == "Optimierung", "Lauf: " + a.StatusText);
        System.DateTime jetzt = new System.DateTime(2026, 11, 15);
        System.Collections.Generic.List<ChangeLauf> erl = ChangeLauf.Aufzuraeumen(l, true, 30, false, jetzt);
        Ist(erl.Count == 2 && erl.Contains(l[2]) && erl.Contains(l[3]), "erledigt und älter als 30 Tage: der alte und der leere Lauf, " + erl.Count);
        Ist(ChangeLauf.Aufzuraeumen(l, true, 90, false, jetzt).Count == 1, "älter als 90 Tage nur der leere Lauf");
        System.Collections.Generic.List<ChangeLauf> and = ChangeLauf.Aufzuraeumen(l, false, 0, true, jetzt);
        Ist(and.Count == 1 && and[0] == l[0], "andere PCs auch mit aktiven Einträgen");
        Ist(!ChangeLauf.Aufzuraeumen(l, true, 0, true, jetzt).Contains(a), "aktive Änderungen dieses PCs bleiben immer");
        Ist(DiagGui.ChgAufraeumArg(true, 30, true) == "Erledigt:30;Andere" && DiagGui.ChgAufraeumArg(false, 0, true) == "Andere" && DiagGui.ChgAufraeumArg(false, 0, false) == "", "Argument für den Arbeitsprozess");
    }

    static void SchreibeAenderungen(string d, string pc, string datei, string lauf, int massnahmen, int je, string status)
    {
        string dir = System.IO.Path.Combine(System.IO.Path.Combine(d, "Änderungen"), pc);
        System.IO.Directory.CreateDirectory(dir);
        System.Text.StringBuilder b = new System.Text.StringBuilder();
        b.Append("{\"Format\":\"Minibench-Aenderungen/1\",\"Computer\":\"").Append(pc).Append("\",\"GeraetId\":\"").Append(pc == System.Environment.MachineName ? "G-test" : "G-" + pc).Append("\",\"Version\":\"4.10\",\"Lauf\":\"").Append(lauf).Append("\",\"Eintraege\":[");
        int id = 0;
        for (int m = 0; m < massnahmen; m++)
            for (int k = 0; k < je; k++)
            {
                id++; if (id > 1) b.Append(",");
                b.Append("{\"Id\":").Append(id).Append(",\"Zeit\":\"").Append(lauf).Append("\",\"Modul\":\"Optimierung\",\"Schritt\":\"S").Append(m).Append("\",\"Titel\":\"Maßnahme ").Append(m + 1)
                    .Append("\",\"Risiko\":\"Aendern\",\"Art\":\"Registry\",\"Ziel\":\"HKLM:\\\\Test\\\\W").Append(id).Append("\",\"Vorher\":\"1\",\"Nachher\":\"0\",\"Gegenbefehl\":\"x\",\"Status\":\"").Append(status).Append("\",\"Ergebnis\":\"\"}");
            }
        b.Append("]}");
        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, datei), b.ToString(), new System.Text.UTF8Encoding(false));
    }

    static int Zaehle(ZeilenListe l, string art) { int n = 0; foreach (ListenZeile z in l.Zeilen) if (z.Art == art) n++; return n; }
    static DiagGui.ChgZeile ChgErste(DiagGui g, string art) { foreach (ListenZeile z in g.lvChg.Zeilen) if (z.Art == art) return (DiagGui.ChgZeile)z; return null; }

    // Seite Änderungen (ab v4.10): zuerst nur dieser PC, gebündelt; Einträge erst beim Aufklappen; dreiwertige Haken; Band;
    // andere PCs nur zur Ansicht; Aufbau auch mit über tausend Einträgen ohne Wartezeit
    static void AenderungenSeite()
    {
        string d = NeuerDatenordner(); string pc = System.Environment.MachineName;
        try
        {
            SchreibeAenderungen(d, pc, "20261110_143100.json", "2026-11-10 14:31:00", 20, 3, "aktiv");
            SchreibeAenderungen(d, pc, "20261001_100000.json", "2026-10-01 10:00:00", 2, 1, "rückgängig");
            for (int i = 0; i < 8; i++) SchreibeAenderungen(d, "FREMD" + i, "20261020_12000" + i + ".json", "2026-10-20 12:00:0" + i, 40, 4, "aktiv");
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                Ist(g.changeLaeufe.Count == 10 && !g.chgAllePcs, "zehn Läufe geladen, Ansicht dieser PC");
                Ist(Zaehle(g.lvChg, "lauf") == 2 && Zaehle(g.lvChg, "andere") == 1, "zwei Läufe dieses PCs und die Zeile der anderen PCs: " + g.lvChg.Zeilen.Count);
                Ist(Zaehle(g.lvChg, "massnahme") == DiagGui.ChgMassnahmenZuerst && Zaehle(g.lvChg, "mehr") == 1 && Zaehle(g.lvChg, "eintrag") == 0, "neuester Lauf aufgeklappt mit den ersten Maßnahmen, noch ohne Einträge");
                Ist(g.chgSeite.Anzahl.Text.StartsWith("60 Änderungen") && g.chgSeite.Unter.Text.StartsWith("1.342 Einträge"), "Kopf: " + g.chgSeite.Anzahl.Text + " / " + g.chgSeite.Unter.Text);
                Ist(!g.chgSeite.BandSichtbar && !g.btnUndo.Enabled, "ohne Haken kein Band");
                DiagGui.ChgZeile m = ChgErste(g, "massnahme");
                g.ChgKlappe(m);
                Ist(Zaehle(g.lvChg, "eintrag") == 3, "Einträge entstehen beim Aufklappen der Maßnahme");
                g.ChgHakenUm(ChgErste(g, "eintrag"));
                Ist(ChgErste(g, "massnahme").Haken == 2 && ChgErste(g, "lauf").Haken == 2 && g.chgHaken.Count == 1, "ein Eintrag: Maßnahme und Lauf teilweise");
                Ist(g.chgSeite.BandSichtbar && g.btnUndo.Enabled && g.chgSeite.BandText.Text == "1 Änderung angehakt", "Band: " + g.chgSeite.BandText.Text);
                g.ChgHakenUm(ChgErste(g, "massnahme"));
                Ist(ChgErste(g, "massnahme").Haken == 1 && g.chgHaken.Count == 3, "Haken an der Maßnahme wählt alle ihre Einträge");
                g.ChgHakenUm(ChgErste(g, "lauf"));
                Ist(ChgErste(g, "lauf").Haken == 1 && g.chgHaken.Count == 60 && g.btnUndo.Text.StartsWith("60 "), "Haken am Lauf wählt alles Zurücknehmbare: " + g.chgHaken.Count);
                g.ChgHakenUm(ChgErste(g, "lauf"));
                Ist(g.chgHaken.Count == 0 && !g.chgSeite.BandSichtbar, "zweiter Klick hebt auf");
                DiagGui.ChgZeile mehr = ChgErste(g, "mehr");
                g.ChgVerweis(mehr);
                Ist(Zaehle(g.lvChg, "massnahme") == 20 && Zaehle(g.lvChg, "mehr") == 0, "weitere Maßnahmen anzeigen");
                DiagGui.ChgZeile alt = null; foreach (ListenZeile z in g.lvChg.Zeilen) if (z.Art == "lauf") alt = (DiagGui.ChgZeile)z;
                Ist(alt.Gesperrt && alt.Haken == 0, "Lauf ohne aktive Einträge ist gesperrt");
                g.ChgVerweis(ChgErste(g, "andere"));
                Ist(g.chgAllePcs && g.chgUmfang.Selected == 1 && Zaehle(g.lvChg, "lauf") == 10 && Zaehle(g.lvChg, "andere") == 0, "Alle PCs zeigt zehn Läufe");
                DiagGui.ChgZeile fremd = null; foreach (ListenZeile z in g.lvChg.Zeilen) { DiagGui.ChgZeile c = (DiagGui.ChgZeile)z; if (c.Art == "lauf" && !c.Lauf.DieserPc) { fremd = c; break; } }
                Ist(fremd != null && fremd.Gesperrt, "Läufe anderer PCs lassen sich nicht anhaken");
                g.ChgHakenUm(fremd);
                Ist(g.chgHaken.Count == 0, "Klick auf gesperrten Lauf ändert nichts");
                // alle Läufe aufklappen: rund 400 Zeilen aus 1.342 Einträgen, ohne spürbare Wartezeit
                foreach (ChangeLauf l in g.changeLaeufe) { g.chgOffen.Add(l.FilePath); g.chgAlles.Add(l.FilePath); }
                g.FillChangeList();
                Ist(Zaehle(g.lvChg, "massnahme") == 342 && g.chgAufbauMs < 1500, "Aufbau mit allen Maßnahmen: " + Zaehle(g.lvChg, "massnahme") + " Zeilen in " + g.chgAufbauMs + " ms");
            }
        }
        finally { Raeume(d); }
    }

    // Seite Sensoren live (ab v4.10): Schalter und Auswahllisten gleich, Werte der Liste, Kennzahlen, volle Aufzeichnung
    static void SensorenSeite()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                Ist(g.sensSeite != null && g.sensSeite.KartenLinks.Count == 2 && g.sensSeite.KartenRechts.Count == 3, "zwei Karten links, drei rechts");
                Ist(g.rowSensTreiber.Checked == (g.cmbDriver.SelectedIndex == 0) && g.rowSensBehalten.Checked == (g.cmbKeep.SelectedIndex == 1), "Schalter zeigen die Werte der Auswahllisten");
                g.rowSensTreiber.Toggle.Checked = false; g.rowSensBehalten.Toggle.Checked = true;
                Ist(g.cmbDriver.SelectedIndex == 1 && g.cmbKeep.SelectedIndex == 1, "Schalter setzen die Auswahllisten");
                g.cmbDriver.SelectedIndex = 0; g.cmbKeep.SelectedIndex = 0;
                Ist(g.rowSensTreiber.Checked && !g.rowSensBehalten.Checked, "Auswahllisten (Vorlage, Geraete.json) setzen die Schalter");
                Ist(g.sensKpi.Wert(0) == "" && g.lvSens.Zeilen.Count == 0, "ohne Messung keine Werte");
                g.HandleLiveLine("@@SENSDEF|0|Prozessor|Ryzen|Temperatur|Core|°C|LHM");
                g.HandleLiveLine("@@SENSDEF|1|Prozessor|Ryzen|Grenzwert|TjMax|°C|Hersteller");
                g.HandleLiveLine("@@SENSDEF|2|Grafik|Radeon|Temperatur|GPU Core|°C|LHM");
                g.HandleLiveLine("@@SENSVAL|0|0:61;1:95;2:48");
                g.HandleLiveLine("@@SENSVAL|2|0:66;2:47");
                g.HandleLiveLine("@@SENSBAD|2|255|außerhalb des Bereichs");
                g.HandleLiveLine("@@SENSLEAD|0|61|4200|70||||900");
                g.HandleLiveLine("@@SENSLEAD|2|66|4300|82||||950||5000");
                g.FillSensList(); g.UpdateSensStand();
                Ist(g.sensWerte.Count == 3 && g.sensGruppen.Count == 2 && g.lvSens.Zeilen.Count == 5, "zwei Gruppen mit drei Werten: " + g.lvSens.Zeilen.Count);
                Ist(g.sensWerte[0].Aktuell == "66 °C" && g.sensWerte[0].Min == "61 °C" && g.sensWerte[0].Max == "66 °C", "Aktuell, Min, Max: " + g.sensWerte[0].Aktuell + " " + g.sensWerte[0].Min);
                Ist(g.sensWerte[1].Blass && g.sensWerte[1].Art == "Grenzwert (Hersteller)" && g.sensWerte[2].Unplausibel && g.sensWerte[2].Hinweis.Contains("255"), "Grenzwert grau, verworfener Wert mit Grund");
                Ist(g.sensKpi.Wert(0) == "66" && g.sensKpi.Unter(0).StartsWith("Min 61") && g.sensKpi.Unter(1).StartsWith("höchster Kern") && g.sensKpi.Wert(3) == "", "Kennzahlen: " + g.sensKpi.Wert(0) + " / " + g.sensKpi.Unter(0) + " / GPU ohne Wert");
                Ist(g.sensKpi.Wert(5) == "950", "Lüfter: " + g.sensKpi.Wert(5));
                g.sensZu.Add(g.sensGruppen[0]); g.FillSensList();
                Ist(g.lvSens.Zeilen.Count == 3 && !g.lvSens.Zeilen[0].Offen, "zugeklappte Gruppe zeigt nur ihren Kopf");
                Ist(g.lblSensAufz.Text.StartsWith("Dauer 2 s") && g.lblSensAufz.Text.Contains("2 Messzeilen") && !g.sensVoll, "Aufzeichnung: " + g.lblSensAufz.Text);
                for (int i = 0; i < DiagGui.SensZeilenMax; i++) g.HandleLiveLine("@@SENSVAL|" + (3 + i) + "|0:60");
                g.UpdateSensStand();
                Ist(g.sensVoll && g.lblSensAufz.Text.Contains("50.000 Messzeilen"), "volle Aufzeichnung wird gemeldet: " + g.lblSensAufz.Text);
            }
        }
        finally { Raeume(d); }
    }

    static DiagGui.DbZeile DbErste(DiagGui g, bool gruppe) { foreach (ListenZeile z in g.lvDb.Zeilen) { DiagGui.DbZeile x = (DiagGui.DbZeile)z; if (x.Gruppe == gruppe) return x; } return null; }

    // Seite Datenbank (ab v4.10): Gerät steht für den neuesten Lauf, ältere darunter; Haken und Band; Markierung und Detailbereich
    static void DatenbankSeite()
    {
        string d = NeuerDatenordner();
        try
        {
            SchreibeLauf(d, "2026-11-10 14:20", true, true); SchreibeLauf(d, "2026-11-02 19:05", false, true); SchreibeLauf(d, "2026-10-21 17:36", false, false);
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                Ist(g.lvDb.Zeilen.Count == 1 && DbErste(g, true).Anzahl == 3 && DbErste(g, true).Aufklappbar && DbErste(g, true).E.Datum == "2026-11-10 14:20", "ein Gerät mit drei Läufen, Zeile zeigt den neuesten");
                Ist(g.dbSeite.Anzahl.Text.StartsWith("1 Gerät") && g.dbSeite.Anzahl.Text.EndsWith("3 Läufe"), "Kopf: " + g.dbSeite.Anzahl.Text);
                Ist(!g.dbSeite.BandSichtbar && !g.btnDashboard.Enabled && g.SelectedDbEntry() == null && !g.DbSoll(g.btnOpenReport) && !g.DbSoll(g.lnkDbDelete), "ohne Haken und Markierung keine Aktionen");
                g.lvDb.LoeseAufklappen(DbErste(g, true));
                Ist(g.lvDb.Zeilen.Count == 3 && DbErste(g, false).E.Datum == "2026-11-02 19:05", "aufgeklappt folgen die älteren Läufe");
                g.lvDb.LoeseHaken(DbErste(g, true)); g.lvDb.LoeseHaken(DbErste(g, false));
                Ist(g.dbHaken.Count == 2 && g.dbSeite.BandSichtbar && g.btnDashboard.Enabled && g.btnDashboard.Text == "Im Dashboard vergleichen (2)" && g.dbSeite.BandText.Text == "2 Läufe angehakt", "Band: " + g.dbSeite.BandText.Text + " / " + g.btnDashboard.Text);
                g.lvDb.Markiere(DbErste(g, false), true);
                Ist(g.SelectedDbEntry() != null && g.SelectedDbEntry().Datum == "2026-11-02 19:05" && g.DbMarkierte().Count == 1, "Markierung liefert den Lauf");
                // die Seite steht im Hintergrund (Visible der Kinder ist falsch): Die Aktionen gelten trotzdem als sichtbar und werden geordnet
                Ist(g.DbSoll(g.btnOpenReport) && g.DbSoll(g.btnVerlauf) && g.DbSoll(g.lnkDbName) && g.DbSoll(g.lnkDbDelete) && g.btnOpenReport.Top > 60, "Aktionen des markierten Laufs: " + g.btnOpenReport.Top);
                g.lvDb.LoeseAufklappen(DbErste(g, true));
                Ist(g.lvDb.Zeilen.Count == 1 && g.dbHaken.Count == 1 && g.btnDashboard.Text == "Im Dashboard vergleichen (1)", "zugeklappt entfällt der Haken des älteren Laufs");
                g.lvDb.Markiere(DbErste(g, true), true);
                Ist(g.SelectedDbEntry().Datum == "2026-11-10 14:20", "Zeile des Geräts steht für den neuesten Lauf");
                g.OnDbColumnClick(2);
                Ist(g.lvDb.SortSpalte == 2 && g.DbMarkierte().Count == 1, "Sortieren hält die Markierung");
                g.OrdneDbDetail();
                Ist(g.dbDetail.AutoScrollMinSize.Height > 100, "Detailbereich misst seinen Inhalt: " + g.dbDetail.AutoScrollMinSize.Height);
            }
        }
        finally { Raeume(d); }
    }

    // Seite Versionen entsteht erst beim ersten Öffnen (ab v4.10)
    static void VersionenSpaeter()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                Ist(!g.versionenGefuellt, "beim Start noch nicht gefüllt");
                g.ShowPage(g.versionPage);
                Ist(g.versionenGefuellt, "nach dem Öffnen gefüllt");
                g.ShowPage(0);
            }
        }
        finally { Raeume(d); }
    }

    // ------------------------------------------------------------ ab v4.11: Befunde der Gesamtprüfung 4.10
    // C1: Solange System-Umzug, winget oder Update-Download laufen, starten weder Lauf noch Live-Ansicht; Schließen fragt nach
    static void HintergrundSperre()
    {
        string d = NeuerDatenordner();
        System.Collections.Generic.List<string> m = new System.Collections.Generic.List<string>();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                UI.Meldungsprotokoll = m; UI.MeldungsAntwort = false;
                Ist(g.HintergrundSperre("Der Lauf") == "" && g.hintergrund == null, "ohne Hintergrundarbeit frei");
                DiagGui.HintergrundArbeit h = g.HintergrundBeginnen("Die Sicherung für den System-Umzug");
                Ist(h != null && g.HintergrundBeginnen("Die Installation über winget") == null && g.hintergrund == h, "höchstens eine Hintergrundarbeit");
                g.StartRun();
                Ist(m.Count == 1 && m[0].Contains("Die Sicherung für den System-Umzug läuft noch") && m[0].Contains("Der Lauf startet erst danach") && !g.running, "Lauf verweigert: " + (m.Count > 0 ? m[0] : "keine Meldung"));
                g.StartLive();
                Ist(m.Count == 2 && m[1].Contains("Die Live-Ansicht startet erst danach") && !g.liveRunning, "Live-Ansicht verweigert");
                bool abgebrochen = false;
                h.Abbrechen = delegate { abgebrochen = true; };
                FormClosingEventArgs e = new FormClosingEventArgs(CloseReason.UserClosing, false);
                g.OnClosing(g, e);
                Ist(e.Cancel && !abgebrochen && !h.Abgebrochen && g.hintergrund == h && m.Count == 3 && m[2].Contains("Abbrechen und schließen"), "Schließen fragt nach, Nein lässt die Arbeit weiterlaufen");
                UI.MeldungsAntwort = true;
                e = new FormClosingEventArgs(CloseReason.UserClosing, false);
                g.OnClosing(g, e);
                Ist(!e.Cancel && abgebrochen && h.Abgebrochen && g.hintergrund == null, "Ja beendet die Hintergrundarbeit");
                g.HintergrundEnde(h);
                Ist(g.HintergrundSperre("Der Lauf") == "", "danach wieder frei");
            }
        }
        finally { UI.Meldungsprotokoll = null; UI.MeldungsAntwort = false; Raeume(d); }
    }

    // C2: Ohne bekannte SHA-256 kein Austausch, die Datei wird verworfen; C3: Fortschritt im Etikett
    static void UpdatePruefsumme()
    {
        string d = NeuerDatenordner();
        string f = System.IO.Path.Combine(d, "LMB.exe");
        string altUrl = DiagGui.AvailableUpdateUrl, altSha = DiagGui.AvailableUpdateSha256, altVer = DiagGui.AvailableUpdateVersion;
        System.Collections.Generic.List<string> m = new System.Collections.Generic.List<string>();
        try
        {
            string meldung;
            System.IO.File.WriteAllText(f, "neue Version");
            Ist(!DiagGui.PruefeUpdateDatei(f, "", out meldung) && !System.IO.File.Exists(f) && meldung.Contains("Release ohne Prüfsumme, Update von Hand") && meldung.Contains("verworfen"), "ohne Prüfsumme verworfen: " + meldung);
            System.IO.File.WriteAllText(f, "neue Version");
            Ist(!DiagGui.PruefeUpdateDatei(f, new string('0', 64), out meldung) && !System.IO.File.Exists(f) && meldung.Contains("Sicherheitsprüfung fehlgeschlagen"), "falsche Prüfsumme verworfen");
            System.IO.File.WriteAllText(f, "neue Version");
            string sha;
            using (System.Security.Cryptography.SHA256 s = System.Security.Cryptography.SHA256.Create())
                sha = System.BitConverter.ToString(s.ComputeHash(System.IO.File.ReadAllBytes(f))).Replace("-", "");
            Ist(DiagGui.PruefeUpdateDatei(f, sha, out meldung) && System.IO.File.Exists(f) && meldung == "", "passende Prüfsumme (Großbuchstaben) wird angenommen");
            Ist(!DiagGui.PruefeUpdateDatei(System.IO.Path.Combine(d, "fehlt.exe"), sha, out meldung), "fehlende Datei");
            Ist(DiagGui.UpdateFortschrittText(50, 200) == "Update wird geladen: 25 %", DiagGui.UpdateFortschrittText(50, 200));
            Ist(DiagGui.UpdateFortschrittText(1572864, -1) == "Update wird geladen: 1,5 MB", DiagGui.UpdateFortschrittText(1572864, -1));
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                UI.Meldungsprotokoll = m;
                DiagGui.AvailableUpdateUrl = "https://example.invalid/LMB.exe"; DiagGui.AvailableUpdateSha256 = ""; DiagGui.AvailableUpdateVersion = "v9.99";
                g.TriggerUpdateDownload();
                Ist(m.Count == 1 && m[0].Contains("Release ohne Prüfsumme, Update von Hand") && m[0].Contains("releases/tag/v9.99"), "Meldung ohne Prüfsumme: " + (m.Count > 0 ? m[0] : "keine"));
                Ist(g.hintergrund == null && !System.IO.Directory.Exists(System.IO.Path.Combine(d, "Update")), "ohne Prüfsumme wird nichts heruntergeladen");
            }
        }
        finally
        {
            UI.Meldungsprotokoll = null;
            DiagGui.AvailableUpdateUrl = altUrl; DiagGui.AvailableUpdateSha256 = altSha; DiagGui.AvailableUpdateVersion = altVer;
            Raeume(d);
        }
    }

    // ab v4.14 (Idee 11): Die Update-Rückfrage nennt, was die neue Version bringt
    static void UpdateWasIstNeu()
    {
        string body = "Rückmeldung aus einem Praxistest.\n\n### Behoben\n* **Lauf ohne Bericht:** Ein `Speicherfehler` beendete den Lauf.\n* **Absturzanalyse:** nennt [den Prozess](https://x/y).\n\n**Download:** LMB.exe herunterladen und starten.\n\nSHA-256 (LMB.exe): " + new string('a', 64) + "\n";
        string t = DiagGui.WasIstNeuText(body, "v9.99");
        Ist(t == "Rückmeldung aus einem Praxistest.\r\n\r\nBehoben\r\nLauf ohne Bericht: Ein Speicherfehler beendete den Lauf.\r\nAbsturzanalyse: nennt den Prozess.", "Klartext ohne Download und Prüfsumme: " + t.Replace("\r\n", "¶"));
        Ist(DiagGui.WasIstNeuText("", "v9.99") == "" && DiagGui.WasIstNeuText(null, "") == "", "ohne Text leer");
        Ist(DiagGui.WasIstNeuText("SHA-256 (LMB.exe): " + new string('b', 64), "v9.99") == "", "nur Prüfsumme ergibt leer");
        System.Text.StringBuilder lang = new System.Text.StringBuilder("### Neu\n");
        for (int i = 1; i <= 30; i++) lang.Append("* Punkt " + i + "\n");
        string k = DiagGui.WasIstNeuText(lang.ToString(), "v9.99");
        Ist(k.Contains("Punkt 14") && !k.Contains("Punkt 15\r\n") && !k.Contains("Punkt 16") && k.EndsWith("releases/tag/v9.99."), "gekürzt auf " + DiagGui.WasIstNeuZeilen + " Zeilen mit Verweis: " + k.Replace("\r\n", "¶"));

        string d = NeuerDatenordner();
        string altUrl = DiagGui.AvailableUpdateUrl, altSha = DiagGui.AvailableUpdateSha256, altVer = DiagGui.AvailableUpdateVersion, altBody = DiagGui.AvailableUpdateBody;
        System.Collections.Generic.List<string> m = new System.Collections.Generic.List<string>();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                UI.Meldungsprotokoll = m; UI.MeldungsAntwort = false;
                DiagGui.AvailableUpdateUrl = "https://example.invalid/LMB.exe"; DiagGui.AvailableUpdateSha256 = new string('c', 64); DiagGui.AvailableUpdateVersion = "v9.99"; DiagGui.AvailableUpdateBody = body;
                g.TriggerUpdateDownload();
                Ist(m.Count == 1 && m[0].StartsWith("Minibench Update|Update auf Version v9.99") && m[0].Contains("Was ist neu in 9.99:\r\nRückmeldung aus einem Praxistest.") && !m[0].Contains("SHA-256 (LMB.exe)"), "Rückfrage mit Was ist neu: " + (m.Count > 0 ? m[0] : "keine"));
                Ist(g.hintergrund == null, "Nein lädt nichts");
                DiagGui.AvailableUpdateBody = "";
                g.TriggerUpdateDownload();
                Ist(m.Count == 2 && m[1].StartsWith("Minibench Update|Update auf Version v9.99") && !m[1].Contains("Was ist neu"), "ohne Text die bisherige Rückfrage");
            }
        }
        finally
        {
            UI.Meldungsprotokoll = null; UI.MeldungsAntwort = false;
            DiagGui.AvailableUpdateUrl = altUrl; DiagGui.AvailableUpdateSha256 = altSha; DiagGui.AvailableUpdateVersion = altVer; DiagGui.AvailableUpdateBody = altBody;
            Raeume(d);
        }
    }

    // ab v4.14 (Idee 15): Leertaste hakt alle markierten Läufe an oder ab, Entf entfernt markierte Läufe mit Rückfrage
    static void DatenbankTasten()
    {
        string d = NeuerDatenordner();
        System.Collections.Generic.List<string> m = new System.Collections.Generic.List<string>();
        try
        {
            SchreibeLauf(d, "2026-11-10 14:20", true, true); SchreibeLauf(d, "2026-11-02 19:05", false, true); SchreibeLauf(d, "2026-10-21 17:36", false, false);
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                UI.Meldungsprotokoll = m; UI.MeldungsAntwort = false;
                g.lvDb.LoeseAufklappen(DbErste(g, true));
                Ist(g.lvDb.Zeilen.Count == 3, "aufgeklappt drei Zeilen");
                g.lvDb.Markiere(g.lvDb.Zeilen[0], true);
                Ist(!g.DbTaste(Keys.Space) && g.dbHaken.Count == 0, "eine markierte Zeile: Leertaste bleibt bei der Liste");
                g.lvDb.MarkiereMehrere(new List<ListenZeile>(g.lvDb.Zeilen));
                Ist(g.DbTaste(Keys.Space) && g.dbHaken.Count == 3 && g.btnDashboard.Text == "Im Dashboard vergleichen (3)", "Leertaste hakt alle drei an: " + g.dbHaken.Count);
                Ist(g.DbMarkierte().Count == 3, "Markierung bleibt nach dem Neuaufbau");
                Ist(g.DbTaste(Keys.Space) && g.dbHaken.Count == 0, "zweimal: alle Haken entfallen");
                g.lvDb.LoeseHaken(g.lvDb.Zeilen[1]);
                Ist(g.dbHaken.Count == 1 && g.DbTaste(Keys.Space) && g.dbHaken.Count == 3, "teilweise angehakt: alle werden gesetzt");
                g.dbHaken.Clear(); g.FillDbList();
                g.lvDb.MarkiereMehrere(new List<ListenZeile> { g.lvDb.Zeilen[1], g.lvDb.Zeilen[2] });
                Ist(g.DbTaste(Keys.Delete) && m.Count == 1 && m[0].StartsWith("Läufe entfernen|Diese 2 Läufe entfernen?") && m[0].Contains("2026-11-02 19:05") && m[0].Contains("2026-10-21 17:36"), "Entf fragt für beide markierten Läufe: " + (m.Count > 0 ? m[0] : "keine Meldung"));
                Ist(g.db.Count == 3, "ohne Zustimmung wird nichts entfernt");
                g.lvDb.LoeseHaken(g.lvDb.Zeilen[0]);
                Ist(g.DbTaste(Keys.Delete) && m.Count == 2 && m[1].StartsWith("Läufe entfernen|Diesen Lauf entfernen?") && m[1].Contains("2026-11-10 14:20"), "angehakte Läufe gehen vor markierten");
                // Verdrahtung: die Taste erreicht DbTaste über KeyDown der Liste
                g.dbHaken.Clear(); g.FillDbList();
                g.lvDb.MarkiereMehrere(new List<ListenZeile>(g.lvDb.Zeilen));
                typeof(Control).GetMethod("OnKeyDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(g.lvDb, new object[] { new KeyEventArgs(Keys.Space) });
                Ist(g.dbHaken.Count == 3, "Leertaste über die Liste: " + g.dbHaken.Count);
                Ist(g.tip.GetToolTip(g.lvDb).Contains("Entf"), "Hinweis der Liste nennt die Tasten");
            }
        }
        finally { UI.Meldungsprotokoll = null; UI.MeldungsAntwort = false; Raeume(d); }
    }

    // ab v4.14 (Idee 14): Klick auf eine Kennzahl hebt ihre Kurve hervor, ein zweiter hebt auf; Lüfter hat keine Kurve
    static void SensorenHervorhebung()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                Ist(g.chartLive.HervorgehobeneSpalte == -1 && g.sensKpi.Hervorgehoben == -1, "anfangs nichts hervorgehoben");
                Ist(g.sensKpi.IstWaehlbar(0) && g.sensKpi.IstWaehlbar(4) && !g.sensKpi.IstWaehlbar(5), "Kacheln mit Kurve wählbar, Lüfter nicht");
                g.sensKpi.LoeseKachel(3);
                Ist(g.chartLive.HervorgehobeneSpalte == 4 && g.sensKpi.Hervorgehoben == 3, "GPU-Temperatur hebt Spalte 4 hervor: " + g.chartLive.HervorgehobeneSpalte);
                g.sensKpi.LoeseKachel(4);
                Ist(g.chartLive.HervorgehobeneSpalte == 6 && g.sensKpi.Hervorgehoben == 4, "andere Kachel wechselt: GPU-Leistung Spalte 6");
                g.sensKpi.LoeseKachel(5);
                Ist(g.chartLive.HervorgehobeneSpalte == 6, "Lüfter löst nichts aus");
                g.sensKpi.LoeseKachel(4);
                Ist(g.chartLive.HervorgehobeneSpalte == -1 && g.sensKpi.Hervorgehoben == -1, "zweiter Klick hebt auf");
                g.sensKpi.Width = UI.S(800);
                Ist(g.sensKpi.KachelBei(new Point(g.sensKpi.Kachel(2).X + 5, g.sensKpi.Kachel(2).Y + 5)) == 2 && g.sensKpi.KachelBei(new Point(-1, -1)) == -1, "Treffer nach Lage der Kachel");
                // Zeichnen mit Hervorhebung wirft nicht
                g.chartLive.Size = new Size(600, 330);
                g.HandleLiveLine("@@SENSLEAD|0|61|4200|70|50|1800|120|900");
                g.HandleLiveLine("@@SENSLEAD|2|66|4300|82|55|1900|150|950");
                g.sensKpi.LoeseKachel(0);
                using (Bitmap b = new Bitmap(600, 330)) g.chartLive.DrawToBitmap(b, new Rectangle(0, 0, 600, 330));
                using (Bitmap b = new Bitmap(g.sensKpi.Width, g.sensKpi.Height)) g.sensKpi.DrawToBitmap(b, new Rectangle(0, 0, b.Width, b.Height));
            }
        }
        finally { Raeume(d); }
    }

    // ab v4.14 (Idee 12): Fußzeile und Knöpfe zeigen die Sperre durch eine Hintergrundarbeit schon vor dem Klick
    static void HintergrundStartknopf()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                g.nav[2].Checked = true; g.UpdateSummary();
                Ist(g.btnStart.Enabled && g.btnLive.Enabled, "ohne Hintergrundarbeit startbar: " + g.lblSel.Text + " / Start " + g.btnStart.Enabled + " (" + g.btnStart.Text + ") / Live " + g.btnLive.Enabled + " / Benchmark " + g.nav[2].Checked);
                string vorher = g.lblSel.Text;
                DiagGui.HintergrundArbeit h = g.HintergrundBeginnen("Die Sicherung für den System-Umzug");
                Ist(!g.btnStart.Enabled && g.lblSel.Text == "Die Sicherung für den System-Umzug läuft noch, Start danach möglich", "Fußzeile: " + g.lblSel.Text);
                Ist(g.tip.GetToolTip(g.btnStart).Contains("Der Lauf startet erst danach"), "Hinweis am Startknopf: " + g.tip.GetToolTip(g.btnStart));
                Ist(!g.btnLive.Enabled && g.tip.GetToolTip(g.btnLive).Contains("Die Live-Ansicht startet erst danach") && g.lblSensInfo.Text.StartsWith("Die Sicherung für den System-Umzug läuft noch"), "Live-Ansicht gesperrt: " + g.lblSensInfo.Text);
                g.HintergrundEnde(h);
                Ist(g.btnStart.Enabled && g.lblSel.Text == vorher && g.btnLive.Enabled && g.tip.GetToolTip(g.btnLive).StartsWith("Startet oder beendet") && g.lblSensInfo.Text == "Live-Ansicht nicht gestartet.", "frei: Live " + g.btnLive.Enabled + " Info " + g.lblSensInfo.Text + " Tipp " + g.tip.GetToolTip(g.btnLive));
                g.nav[2].Checked = false; g.UpdateSummary();
                h = g.HintergrundBeginnen("Die Installation über winget");
                Ist(!g.btnStart.Enabled && g.lblSel.Text == "Noch kein Ablauf gewählt", "ohne Ablauf bleibt der bisherige Text");
                g.HintergrundEnde(h);
            }
        }
        finally { Raeume(d); }
    }

    // C5: F5 und Strg+Eingabe über ProcessCmdKey, damit sie auch mit Fokus in einem Kind wirken
    static void Tastenkuerzel()
    {
        string d = NeuerDatenordner();
        try
        {
            using (DiagGui g = NeueOberflaeche(d, null))
            using (TextBox tb = new TextBox())
            {
                System.Reflection.MethodInfo mi = typeof(DiagGui).GetMethod("ProcessCmdKey", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new System.Type[] { typeof(Message).MakeByRefType(), typeof(Keys) }, null);
                Ist(mi != null && mi.DeclaringType == typeof(DiagGui), "DiagGui überschreibt ProcessCmdKey");
                Ist((bool)mi.Invoke(g, new object[] { new Message(), Keys.F5 }), "ProcessCmdKey behandelt F5");
                tb.Multiline = true;
                Ist(g.Tastenkuerzel(Keys.F5, tb), "F5 wirkt auch mit Fokus im Textfeld");
                Ist(!g.Tastenkuerzel(Keys.Control | Keys.Enter, tb), "Strg+Eingabe bleibt im mehrzeiligen Textfeld ein Zeilenumbruch");
                Ist(!g.Tastenkuerzel(Keys.Enter, null) && !g.Tastenkuerzel(Keys.A, null) && !g.Tastenkuerzel(Keys.Shift | Keys.F5, null), "andere Tasten gehen an das Element");
            }
        }
        finally { Raeume(d); }
    }

    // C6: Dauer des Laufs über eine Stunde
    static void ErgebnisDauer()
    {
        Ist(DiagGui.LaufDauerText(System.TimeSpan.FromSeconds(3912)) == "1:05:12 Std.", DiagGui.LaufDauerText(System.TimeSpan.FromSeconds(3912)));
        Ist(DiagGui.LaufDauerText(System.TimeSpan.FromSeconds(75)) == "01:15 Min.", DiagGui.LaufDauerText(System.TimeSpan.FromSeconds(75)));
        Ist(DiagGui.LaufDauerText(System.TimeSpan.FromSeconds(3599)) == "59:59 Min.", "knapp unter einer Stunde");
        Ist(DiagGui.LaufDauerText(System.TimeSpan.FromHours(25.5)) == "25:30:00 Std.", DiagGui.LaufDauerText(System.TimeSpan.FromHours(25.5)));
        Ist(DiagGui.LaufDauerText(System.TimeSpan.FromSeconds(-5)) == "00:00 Min.", "negative Dauer");
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern int GetGuiResources(System.IntPtr prozess, int art);
    static int ZaehleElemente(Control c) { int n = 1; foreach (Control k in c.Controls) n += ZaehleElemente(k); return n; }
    static Control MitHinweis(ToolTip t, Control c)
    {
        foreach (Control k in c.Controls)
        {
            if (t.GetToolTip(k).Length > 0) return k;
            Control x = MitHinweis(t, k); if (x != null) return x;
        }
        return null;
    }

    // C7: Startseite gibt beim Neuaufbau die alten Elemente frei und löst ihre Hinweise; Handles wachsen nicht
    static void StartseiteNeuaufbau()
    {
        string d = NeuerDatenordner();
        try
        {
            SchreibeLauf(d, "2026-11-10 14:20", true, true);
            using (DiagGui g = NeueOberflaeche(d, null))
            {
                Control start = g.pages[0];
                g.CreateControl();
                g.ReloadDb();
                int n1 = ZaehleElemente(start);
                Control alt = MitHinweis(g.tip, start);
                Ist(alt != null && n1 > 20, "Startseite mit Elementen und Hinweisen: " + n1);
                System.IntPtr ich = System.Diagnostics.Process.GetCurrentProcess().Handle;
                int user1 = GetGuiResources(ich, 1), gdi1 = GetGuiResources(ich, 0);
                for (int i = 0; i < 5; i++) g.ReloadDb();
                int n2 = ZaehleElemente(start), user2 = GetGuiResources(ich, 1), gdi2 = GetGuiResources(ich, 0);
                Ist(n2 == n1, "Anzahl der Elemente bleibt gleich: " + n1 + " -> " + n2);
                Ist(alt.IsDisposed && alt.Parent == null, "altes Element freigegeben");
                Ist(g.tip.GetToolTip(alt) == "", "Hinweis des alten Elements gelöst");
                Ist(user2 - user1 < 30, "USER-Objekte wachsen nicht: " + user1 + " -> " + user2);
                Ist(gdi2 - gdi1 < 60, "GDI-Objekte wachsen nicht: " + gdi1 + " -> " + gdi2);
            }
        }
        finally { Raeume(d); }
    }
}
