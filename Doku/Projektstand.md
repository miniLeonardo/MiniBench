# Leos Minibench: Projektstand

Gemeinsamer Stand für alle Agenten (Claude, Antigravity). Wird am Ende jeder Version aktualisiert (siehe AGENTS.md).
Seit 4.07 steht hier nur noch, was für die nächste Version gebraucht wird: Stand, Aufbau der Oberfläche, offene Befunde,
abgelehnte Vorschläge, Fallstricke. Was eine Version geändert hat, steht in CHANGELOG.md (kurz), in
Doku\Versionshistorie.txt und in Doku\Änderungen_vX.Y.txt (ausführlich); es wird hier nicht wiederholt.

## Stand: Version 4.08 (12.11.2026)
Release-Commit "Release v4.08: Ergebnis als Seite, Benchmark und Diagnose im Stil der übrigen Seiten" von Claude
vorbereitet (Grundlage ba3489b, Release v4.07), als git bundle übergeben.

Inhalt von 4.08 (Einzelheiten in Doku\Änderungen_v4.08.txt):
1. Ergebnis als Seite: Eintrag "Ergebnis" in der Seitenleiste unter Start, Seite bleibt erreichbar, Bewegung nur beim
   ersten Anzeigen, Band "Zurück zum Ergebnis" auf der Seite Benchmark, "Erneut messen" statt "Zurück", nach dem
   Programmstart der letzte Lauf dieses PCs aus der Datenbank.
2. Datenbankeintrag mit optionalem Feld Vergleich (Name der Referenz, Prozent je Gruppe); Format weiter PC-Diagnose-DB/2.
   Die Oberfläche rechnet Prozentwerte nie nach; Läufe vor 4.08 zeigen "nicht gespeichert".
3. Seiten Benchmark und Diagnose im Muster von Wartung und Tools (Karten, Zeilen mit Schalter, zwei Spalten, ganze
   Breite) auf dem gemeinsamen Rahmen DiagGui_Modulseite.cs.
4. Startfenster zeigt die Version des geladenen Skripts und nennt die Abweichung von LMB.exe.
5. Behoben: leere Kopfkarte der Ergebnisseite unter Windows, Verweis über dem Text, markiertes Modul, Name der Referenz.

## Aufbau der Oberfläche (src\Oberflaeche, Reihenfolge in Oberflaeche.ps1)
DiagGui.cs                 Rahmen: using-Zeilen für alle Teile, Felder, Start, Seitenleiste, Fußleiste, ShowPage, gemeinsame Bausteine
DiagGui_Darstellung.cs     Farbschema, Titelleiste, native Steuerelemente
DiagGui_Einstellungen.cs   Einstellungsdatei, Dialog Einstellungen, Fenstergröße, Netzlaufwerk-Dialog
DiagGui_Update.cs          Update-Prüfung und Austausch der LMB.exe
DiagGui_Modulseite.cs      ab 4.08: gemeinsamer Rahmen der Seiten Benchmark und Diagnose (ModulSeite, ZeilenKarte, SchalterZeile)
DiagGui_Startseite.cs, _Diagnose, _Benchmark, _Lasttest, _Wartung, _Optimierung, _Tools, _Sensoren, _Vergleich (Datenbank),
_Umzug, _Aenderungen, _Versionen   je eine Seite
DiagGui_Ablauf.cs          gewählte Module, Zeitschätzung, Start eines Laufs
DiagGui_Vorlagen.cs        Vorlagen
DiagGui_Lauf.cs            Laufansicht, Ereigniskanal (HandleLine), Abschluss
DiagGui_Ergebnis.cs        Ergebnisseite (ab 4.08 eine Seite hinter Versionen; Eintrag navErgebnis steht außerhalb der Liste nav)
Bewegung.cs                Zeitgeber für Übergänge, RingAnzeige, Ueberblendung, FarbPunkt
DiagGui_Steuerelemente.cs  Klasse UI (Farben, Akzent, Maße) und eigene Steuerelemente
DiagGui_Modelle.cs, Start.cs, Versionen.cs   Datenmodelle, Programmsymbol und Startprotokoll, Versionshistorie
Seiten und Einträge der Seitenleiste teilen sich die Stellen 0 bis 10 (nav[i] gehört zu pages[i], 1 bis 5 sind die
Module), danach folgen Versionen (versionPage) und Ergebnis (ergebnisPage). Viele Stellen sind fest im Code (ShowPage(2),
nav[1].Checked): neue Seiten hinten anhängen, nie dazwischen.
Werte hinter Schaltern: benchChk, diagChk, repChk, chkFast, chkInstall, chkMem und rbVoll, rbSchnell, rbCustom werden
nicht angezeigt, tragen aber die Auswahl für Vorlagen, Zeitschätzung und Start. Sichtbar sind SettingRow und FeldAuswahl.
Neue Seite: eigene Datei DiagGui_<Name>.cs ohne using-Zeilen, in Oberflaeche.ps1 eintragen. Tests suchen im Gesamttext
(Get-GuiText in tests\Hilfen.ps1), nicht in einer bestimmten Datei.

## Versionen bisher (je eine Zeile, neueste zuerst)
4.07 Eine Datei je Seite, kürzerer Start, Akzentfarbe, dezente Bewegung (ba3489b). 4.06 Update-Prüfung und Referenz
repariert (53a4d04). 4.05 Bereits aktive Optimierungen gesperrt, Tools-Seite kompakt,
28 Softwarepakete (ebb4f81). 4.04 Veröffentlichen in einem Schritt, Referenz über die Datenbank (2a5cb46). 4.03
Update-Funktion, SMART-Langtest, Datenbank (598b8b1). 4.02 PawnIO-Entfernung, plausiblere Messwerte, Einstellungen auf
einer Seite (2d8cff1). 4.01 Politur: Ergebnisseite, Einstellungen, Startzeit, Skalierung (a77b64f). 4.0 Aufgeräumt
(dd8f43f; Doku\Änderungen_v4.0.txt nennt einige Punkte, die erst 4.01 umgesetzt hat). 3.91 Ergebnis-Dashboard
(e7d9a18). 3.9 Master-Detail, Schalter, Ergebnisseite (0dfbb40). 3.88 bis 3.882 Datenbank-Gruppen (16a08c2, c7358e0,
b582625). 3.84 bis 3.87 GitHub, Sensoren, Update-Prüfung, Läufe je Gerät. 3.8 bis 3.83 System-Umzug. 3.7 bis 3.73
Startseite, Konfigurationsprüfung, Dashboard in Bausteinen, LMB.exe. 3.64 bis 3.67 Gestaltungsregeln der Oberfläche.

## Offene Befunde
1. 4.08 unter Windows bestätigen: Seiten Benchmark und Diagnose bei 100, 125 und 150 %, Kopfkarte der Ergebnisseite
   ohne Leerraum, Eintrag "Ergebnis" nach dem Start, Band auf der Seite Benchmark, Hinweis im Startfenster bei
   abweichendem Skript, freier Platz in den Laufwerkszeilen. Die Überblendung von 4.07 hat Leonardo am 12.11.2026
   als gut bestätigt.
2. Startzeit: Start.log im Projektordner enthält nur einen Start mit 4.07 (7,9 s mit Übersetzung, Hardwareabfrage
   566 ms, Seiten 745 ms). "Datenpflege: unverändert" beim zweiten Start ist noch nicht belegt.
3. Größter verbleibender Anteil am Start: powershell.exe und das Einlesen des Gesamtskripts (2,6 MB; Stick 1,0 s,
   ULB-PC10039 2,7 s). Abhilfe wäre ein getrennter kleiner Startteil für die Oberfläche (Umbau von Zusammenbau,
   Launcher und $PSCommandPath der Arbeitsprozesse), nicht umgesetzt. Kleiner Schritt davor: die Seite Versionen
   (150 ms, ein langes Textfeld) erst beim ersten Öffnen aufbauen.
4. Seite Lasttest ist noch eine Tabelle mit Kontrollkästchen. Vorschlag im Muster von 4.08: links die Karte
   "Komponenten" mit vier Zeilen (Symbol, Titel, Beschreibung, Schalter), Dauer und Einstellungen der Komponente im
   Bereich unter ihrer Zeile, rechts die Karten "Abbruchschwelle" und "Gesamtdauer", Seitenkopf "3 von 4 Komponenten".
   chkLCpu bis chkLDisk blieben als Werte; UpdateLoadEnabled entfiele zugunsten der Unterbereiche.
5. Ergebnisseite, zweite Stufe: Auswahlfeld oben rechts für frühere Läufe dieses Geräts (ErgebnisAusEintrag kann jeden
   Eintrag zeigen). Nach einem Lauf ist die Laufansicht (Tests, Leistung, Protokoll) nicht mehr erreichbar.
6. Kategorie Apps der Optimierung nutzt dasselbe Symbol wie Tools (UI.IcoApp = UI.IcoTools).
7. Rendertest auf sehr schnellen Karten (RTX 3070): hoher Mikroruckler-Anteil und niedriges 1-%-Low nur im Benchmark.
8. Unter Windows noch nicht bestätigt: smartctl -X und PCIe-Abfrage unter Last (4.03), winget export als Administrator,
   GPU-Z über den Uninstall-Eintrag, Menü "…" im dunklen Farbschema (4.05), "Jetzt suchen" hinter dem Proxy, "Median
   aktualisieren" mit Meldung (4.06), Akzentfarbe "wie Windows", Dialog Einstellungen bei 125 und 150 % (4.07).
9. Die Sperre der Optimierung gilt bis zum nächsten Start; nach Rückgängig auf der Seite Änderungen erst dann wieder offen.
10. README zeigt noch Aufnahmen älterer Versionen (Doku\Screenshots); echte Windows-Aufnahmen von 4.0x fehlen.
11. Akzentfarbe gilt nicht im HTML-Bericht und im Dashboard; Wartung sperrt "Schnellstart deaktivieren" nicht als
    "bereits aktiv" wie die Optimierung.

## Abgelehnte Vorschläge (Entscheidung Leonardo, 10.11.2026): nicht umsetzen und nicht erneut vorschlagen
GitHub Actions bzw. CI; Diagnosepaket (Logs als ZIP); Szenario-Kacheln auf der Startseite; Vorher-Nachher-Messung;
Befehlspalette (Strg+K); Software aktualisieren über winget upgrade; Erweiterungen des Umzugs (Treiber exportieren,
Produktschlüssel, WLAN-Profile, BitLocker-Hinweis); zusätzliche Diagnosepunkte (LAN-Geschwindigkeit, ausstehende
Updates, Autostart-Übersicht); Bericht hochladen oder per QR-Code teilen; Restzeit des Laufs; Kurzfassung oben im
HTML-Bericht; KI-Datei in die Zwischenablage; Ergebniskarte als Bild; Bestenliste und Auszeichnungen;
Sensor-Miniaturfenster; Fortschritt in der Taskleiste und Ton am Laufende; Osterei.
Schon früher abgelehnt: eigenes Netzwerkmodul, Übergabe- oder Aussonderungsbogen, Echtheitstest für USB-Sticks,
Schnelltest für Peripherie, sicheres Löschen, Testmatrix-Dateien.

## Fallstricke
1. Schriftgrößen in Punkt nie mit UI.SF/UI.S skalieren (PerMonitorV2: Windows rechnet Punkt selbst um).
2. this.psExe ist powershell.exe, nicht LMB.exe; den Pfad der exe liefert LEOSMINIBENCH_EXE.
3. FlowLayoutPanel mit AutoSize behält die Höhe des ersten Aufbaus; bei Umbruch nach Breite die Größe selbst setzen.
4. Farben nie fest eintragen, wo der Akzent gemeint ist: UI.Accent, AccentHover, AccentActive, AccentDark, AccentSoft,
   OnAccent. Info-Etiketten bleiben blau (UI.Info).
5. Übergänge nur über Bewegung.Starte (endet immer im Zielzustand, respektiert die Einstellung). Control.DrawToBitmap
   nimmt RichTextBox nicht auf: Die Seite Versionen ist von der Überblendung ausgenommen.
6. Get-MinibenchBuild und Invoke-MinibenchBuild schreiben src\Kern\Referenzen_Eingebettet.ps1 unter Linux in anderer
   Reihenfolge neu: danach git checkout dieser Datei.
7. Neue Abfragen beim Start (Get-CimInstance) immer mit -Property; Win32_Processor ohne -Property ermittelt auch die
   Auslastung (nach verbreiteter Erfahrung rund eine Sekunde).

8. Ein Element mit AutoSize in einem Panel ohne eigenes Layout (Panel, CardPanel) schrumpft nie unter seine Startgröße
   (AutoSizeMode.GrowOnly, 200 x 100): dort AutoSizeMode.GrowAndShrink setzen oder die Größe selbst setzen. In einem
   FlowLayoutPanel als Elternteil tritt das nicht auf.
9. Control.Visible ist falsch, solange ein übergeordnetes Element nicht angezeigt wird (Seite im Hintergrund, Fenster im
   Selbsttest). Layout nie nach Visible der Kinder ordnen, sondern eigenen Zustand führen (ErgSoll, subGpuOffen,
   bandErgGezeigt, ergebnisNavSichtbar).
10. Seiten, die sich selbst ordnen (ModulSeite.Ordne, OrdneErgebnis), hängen an content.Resize und ClientSizeChanged:
    Erscheint die senkrechte Bildlaufleiste, ändert sich nur die Innenfläche.
11. Der Selbsttest baut seit 4.08 die ganze Oberfläche auf (NeueOberflaeche in GuiSelbsttest.cs, ohne Anzeige, mit
    eigenem Datenordner und abgeschalteter Update-Prüfung). Was der Konstruktor von DiagGui aufruft, muss ohne Windows,
    ohne Netz und ohne Systemordner laufen.
12. Mock in einem It gilt im Pester-Nachbau der Sandbox über das It hinaus (echtes Pester begrenzt ihn): Ein neuer Test
    mit Mock Get-CimInstance lässt in der Sandbox einen Nachbarn bestehen, der sonst an WMI scheitert.

## Letzter Commit
Release-Commit v4.08 mit Tag v4.08 (Nachfolger von ba3489b, Release v4.07), übergeben als MiniBench_v4.08.bundle.
