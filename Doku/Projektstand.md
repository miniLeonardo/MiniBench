# Leos Minibench: Projektstand

Gemeinsamer Stand für alle Agenten (Claude, Antigravity). Wird am Ende jeder Version aktualisiert (siehe AGENTS.md).
Seit 4.07 steht hier nur noch, was für die nächste Version gebraucht wird: Stand, Aufbau der Oberfläche, offene Befunde,
abgelehnte Vorschläge, Fallstricke. Was eine Version geändert hat, steht in CHANGELOG.md (kurz), in
Doku\Versionshistorie.txt und in Doku\Änderungen_vX.Y.txt (ausführlich); es wird hier nicht wiederholt.

## Stand: Version 4.07 (10.11.2026)
Release-Commit "Release v4.07: Eine Datei je Seite, kürzerer Start, Akzentfarbe und dezente Bewegung" von Claude
vorbereitet (Grundlage 53a4d04, Release v4.06), als git bundle übergeben.

Inhalt von 4.07 (Einzelheiten in Doku\Änderungen_v4.07.txt):
1. Oberfläche in einer Datei je Seite (DiagGui_<Seite>.cs, Teilklasse DiagGui), ohne Änderung am Verhalten.
2. Startzeit: Hardwareabfrage nur mit den gebrauchten Eigenschaften, Datenpflege nur bei geändertem Datenstand,
   Datenbank und Änderungsprotokoll gleichzeitig geladen, Aufbauzeit je Seite in Start.log.
3. Akzentfarbe in den Einstellungen (Blau, Türkis, Violett, Pink, Orange, wie Windows).
4. Dezente Bewegung: Punktzahl, Balken und Ringe der Ergebnisseite laufen auf ihren Wert, Überblendung beim
   Seitenwechsel; abschaltbar in den Einstellungen.
5. Ergebnisseite: Ringanzeige je Komponente; ein Datenbankeintrag ohne Ordner passt nicht mehr auf jeden Lauf.
6. Doku gestrafft (diese Datei, README, AGENTS.md).

## Aufbau der Oberfläche (src\Oberflaeche, Reihenfolge in Oberflaeche.ps1)
DiagGui.cs                 Rahmen: using-Zeilen für alle Teile, Felder, Start, Seitenleiste, Fußleiste, ShowPage, gemeinsame Bausteine
DiagGui_Darstellung.cs     Farbschema, Titelleiste, native Steuerelemente
DiagGui_Einstellungen.cs   Einstellungsdatei, Dialog Einstellungen, Fenstergröße, Netzlaufwerk-Dialog
DiagGui_Update.cs          Update-Prüfung und Austausch der LMB.exe
DiagGui_Startseite.cs, _Diagnose, _Benchmark, _Lasttest, _Wartung, _Optimierung, _Tools, _Sensoren, _Vergleich (Datenbank),
_Umzug, _Aenderungen, _Versionen   je eine Seite
DiagGui_Ablauf.cs          gewählte Module, Zeitschätzung, Start eines Laufs
DiagGui_Vorlagen.cs        Vorlagen
DiagGui_Lauf.cs            Laufansicht, Ereigniskanal (HandleLine), Abschluss
DiagGui_Ergebnis.cs        Ergebnisseite
Bewegung.cs                Zeitgeber für Übergänge, RingAnzeige, Ueberblendung, FarbPunkt
DiagGui_Steuerelemente.cs  Klasse UI (Farben, Akzent, Maße) und eigene Steuerelemente
DiagGui_Modelle.cs, Start.cs, Versionen.cs   Datenmodelle, Programmsymbol und Startprotokoll, Versionshistorie
Neue Seite: eigene Datei DiagGui_<Name>.cs ohne using-Zeilen, in Oberflaeche.ps1 eintragen. Tests suchen im Gesamttext
(Get-GuiText in tests\Hilfen.ps1), nicht in einer bestimmten Datei.

## Versionen bisher (je eine Zeile, neueste zuerst)
4.06 Update-Prüfung und Referenz repariert (53a4d04). 4.05 Bereits aktive Optimierungen gesperrt, Tools-Seite kompakt,
28 Softwarepakete (ebb4f81). 4.04 Veröffentlichen in einem Schritt, Referenz über die Datenbank (2a5cb46). 4.03
Update-Funktion, SMART-Langtest, Datenbank (598b8b1). 4.02 PawnIO-Entfernung, plausiblere Messwerte, Einstellungen auf
einer Seite (2d8cff1). 4.01 Politur: Ergebnisseite, Einstellungen, Startzeit, Skalierung (a77b64f). 4.0 Aufgeräumt
(dd8f43f; Doku\Änderungen_v4.0.txt nennt einige Punkte, die erst 4.01 umgesetzt hat). 3.91 Ergebnis-Dashboard
(e7d9a18). 3.9 Master-Detail, Schalter, Ergebnisseite (0dfbb40). 3.88 bis 3.882 Datenbank-Gruppen (16a08c2, c7358e0,
b582625). 3.84 bis 3.87 GitHub, Sensoren, Update-Prüfung, Läufe je Gerät. 3.8 bis 3.83 System-Umzug. 3.7 bis 3.73
Startseite, Konfigurationsprüfung, Dashboard in Bausteinen, LMB.exe. 3.64 bis 3.67 Gestaltungsregeln der Oberfläche.

## Offene Befunde
1. 4.07 unter Windows bestätigen: Überblendung beim Seitenwechsel (in der Sandbox nicht sichtbar zu prüfen, Mono nimmt
   Kindelemente nicht ins Bild auf), Ringe und Hochzählen auf der Ergebnisseite, Akzentfarbe "wie Windows", Dialog
   Einstellungen bei 125 und 150 %.
2. Startzeit unter Windows messen (Minibench-Daten\Laufzeit\Start.log, zweiter Start, oder LMB.exe -StartAuswertung):
   Phase "Hardwareabfrage abgeschlossen (Abfrage n ms)", "Datenpflege: unverändert", "Seiten aufgebaut (...)".
   Gemessen bis 4.06 (16 Starts): vom Stick 4,2 s, auf ULB-PC10039 von der Festplatte 9 s.
3. Größter verbleibender Anteil am Start: powershell.exe und das Einlesen des Gesamtskripts (2,6 MB, 214.000 Tokens;
   Stick 1,0 s, ULB-PC10039 2,7 s). Abhilfe wäre ein getrennter kleiner Startteil für die Oberfläche (Umbau von
   Zusammenbau, Launcher und $PSCommandPath der Arbeitsprozesse), nicht umgesetzt.
4. Sichtprüfung unter Windows bei 125 und 150 % Skalierung (Mono rechnet fest mit 96 dpi).
5. Kategorie Apps der Optimierung nutzt dasselbe Symbol wie Tools (UI.IcoApp = UI.IcoTools).
6. Rendertest auf sehr schnellen Karten (RTX 3070): hoher Mikroruckler-Anteil und niedriges 1-%-Low nur im Benchmark.
7. Unter Windows noch nicht bestätigt: smartctl -X und PCIe-Abfrage unter Last (4.03), winget export als Administrator,
   GPU-Z über den Uninstall-Eintrag, Menü "…" im dunklen Farbschema (4.05), "Jetzt suchen" hinter dem Proxy, "Median
   aktualisieren" mit Meldung (4.06).
8. Die Sperre der Optimierung gilt bis zum nächsten Start; nach Rückgängig auf der Seite Änderungen erst dann wieder offen.
9. README zeigt noch Aufnahmen älterer Versionen (Doku\Screenshots); echte Windows-Aufnahmen von 4.0x fehlen.

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

## Letzter Commit
Release-Commit v4.07 mit Tag v4.07 (Nachfolger von 53a4d04, Release v4.06), übergeben als MiniBench_v4.07.bundle.
