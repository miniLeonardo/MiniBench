# Leos Minibench: Projektstand

Gemeinsamer Stand für alle Agenten (Claude, Antigravity). Wird am Ende jeder Version aktualisiert (siehe AGENTS.md).

## Stand: Version 4.06 (09.11.2026)
Release-Commit "Release v4.06: Update-Prüfung und Referenz repariert" von Claude vorbereitet (Grundlage ebb4f81,
Release v4.05), als git bundle übergeben.

Inhalt von 4.06 (Einzelheiten in Doku\Änderungen_v4.06.txt):
1. Update-Prüfung: "Jetzt suchen" ohne Cache mit Meldung, Cache beim Start 2 h, Ausweg über die Weiterleitung von
   releases/latest und LMB.exe.sha256, Frist 10 s, Proxy mit Anmeldung. Veroeffentlichen.ps1 hängt LMB.exe.sha256 an.
2. Median aktualisieren: echte Meldung (vorher immer "fehlgeschlagen"), Median aller Systeme in Referenz_Median.json,
   eigene Referenz (Referenz.json) hat Vorrang, alter Median ins Archiv, keine Ersatzwerte im Lasttest.

## Vorher: Version 4.05 (09.11.2026)
Release-Commit "Release v4.05: Bereits aktive Optimierungen gesperrt, Tools-Seite kompakt, 28 Softwarepakete" (Commit ebb4f81).

Inhalt von 4.05 (Einzelheiten in Doku\Änderungen_v4.05.txt):
1. Optimierung: bereits aktive Einträge stehen auf An und sind gesperrt (OptItem.Gesperrt, ToggleSwitch.Locked); Vorlagen
   ändern sie nicht; Zähler gewählt und bereits aktiv getrennt (CategoryPickerItem.ActiveCount, Kopf der Kategorie).
2. Optimierung im Lauf: Split-OptBereitsAktiv überspringt aktive Einträge ("bereits aktiv"), ohne offene Einträge kein
   Wiederherstellungspunkt.
3. Seite Tools: ToolRow (kompakte Zeilen, zwei Spalten ab 900 px), Knöpfe im Seitenkopf, Netzlaufwerk entfernt.
4. Werkzeuge CPU-Z, GPU-Z (Platzhalter GPU-Z*.exe, neueste Version), MSI Afterburner.
5. Softwarepakete: Kern\Softwarepakete.psd1 (28 Pakete, 6 Gruppen), Hilfsmodus -SoftwareZustand (winget export, @@PAKZ).

Vorschläge für die nächste Version (aus dem Chat zu 4.05, Leonardo wählt aus): siehe Abschnitt "Offene Vorschläge" unten.

## Vorher: Version 4.04 (08.11.2026)
Release-Commit "Release v4.04: Veröffentlichen in einem Schritt, Referenz über die Datenbank" (Commit 2a5cb46).

Inhalt von 4.04 (Einzelheiten in Doku\Änderungen_v4.04.txt):
1. Seite Benchmark ohne Haken "Dieses System als Referenz festlegen"; Referenz über Datenbank › Aktionen.
2. Veroeffentlichen.ps1 prüft, überträgt main und Tag, richtet gh ein und erstellt das Release in einem Aufruf.
3. .gitignore: Claude outputs.

## Vorher: Version 4.03 (08.11.2026)
Release-Commit "Release v4.03: Update-Funktion, SMART-Langtest, Datenbank und Feinschliff" (Commit 598b8b1).

Inhalt von 4.03 (Einzelheiten in Doku\Änderungen_v4.03.txt):
1. Update-Funktion tauscht LMB.exe (bis v4.02 powershell.exe, es erschien nur ein PowerShell-Fenster), Update.log, Rückweg.
2. Versionsvergleich dezimal (4.03 < 4.1).
3. SMART-Langtest wird beim Überspringen und vor der Datenträgermessung beendet (smartctl -X).
4. PCIe-Anbindung der Grafik während des Rendertests gelesen.
5. Datenbank: Knöpfe erst bei Markierung; Aktionen mit Berichtsordner öffnen und Lauf löschen.
6. Referenz auf der Seite Benchmark oben, Verweis "Referenz ändern" auf der Ergebnisseite.
7. Kopfzeile "Aufräumen" beim Beenden der Sensoren; Legende der Sensorkurven ohne Überlappung, Abstände skaliert.
8. Veroeffentlichen.ps1 für GitHub-Releases.

## Vorher: Version 4.02 (08.11.2026)
Release-Commit "Release v4.02: PawnIO-Entfernung, plausiblere Messwerte, Einstellungen auf einer Seite" (Commit 2d8cff1).

Inhalt von 4.02 (Einzelheiten in Doku\Änderungen_v4.02.txt):
1. PawnIO wird am Laufende wieder entfernt (seit v3.84 falsche Setup-Schalter, Fehlerfenster und Rückgabecode 87).
2. Laufwerke mit Medientyp "Unspecified" über Name und 4K-Zugriffszeit als SSD oder Festplatte eingeordnet.
3. Hinweis, wenn die Datenträgermessung während eines SMART-Langtests läuft.
4. Hintergrundlast nach aktueller CPU-Last; eine Warnung ohne DNS-Server; Autostart-Verzögerer je Programm einmal.
5. Ergebnisseite: Einordnung gegenüber dem Referenz-PC, Name der Referenz, "nicht gemessen", eine Pille je Laufwerk.
6. Einstellungen auf einer Seite mit drei Gruppen.
7. Optimierung: Zustand im Hintergrund nach dem Start, Knopf "Zustand prüfen" entfällt.
8. Protokoll ohne doppelte Abschnittszeilen.

## Vorher: Version 4.01 (07.11.2026)
Release-Commit "Release v4.01: Politur der Oberfläche: Ergebnisseite, Einstellungen, Startzeit und Skalierung" (Commit a77b64f).

Inhalt von 4.01 (Einzelheiten in Doku\Änderungen_v4.01.txt):
1. Umlaute: Doppelkodierungen aus 4.0 in Oberfläche und Doku behoben.
2. Modulwahl nur über den Haken der Seitenleiste; Hinweisband „In den Ablauf aufnehmen“ statt Schalter im Seitenkopf.
3. Fußleiste: Ablauf-Vorlagen Schnellcheck, Standard, Komplett, Alles abwählen; globale Optionen nur in den Einstellungen; Werkzeugseiten nur Ablaufzeile und Startknopf.
4. Ergebnisse immer im Programmfenster (Option 1-Fenstermodus entfällt).
5. Optimierung und Wartung mit Kurztiteln und Symbolen aus Katalog und Vertrag; Etikett Ändern nicht in jeder Zeile.
6. Datenbank ohne Suchfeld, kürzere Texte, Knopf Aufräumen.
7. Einstellungen: Bereiche links in der Kategorieliste, Optionen rechts in einer Karte; dunkle Titelleiste der Dialoge.
8. Ergebnisseite: Urteil aus den Befunden, fünf wichtigste Befunde, Leistung je Gruppe gegenüber der Referenz mit Einordnung, Kacheln mit einem Hauptwert. Feste Texte des alten Dashboards entfernt.
9. Startzeit: Laufwerke über MSFT_PhysicalDisk, Datenpflege parallel zur Hardwareabfrage, Zeilen der Optimierung bei Bedarf, neue Phasen in Start.log.
10. Skalierung: Schriftgrößen nicht mehr doppelt skaliert (UI.SF nur für Pixelmaße).

Hinweis: Doku\Änderungen_v4.0.txt beschreibt einige Punkte, die 4.0 nicht umgesetzt hatte (Suchleiste mit Filter,
Strg+F, kompakte Fußleiste, Kurztitel in der Oberfläche). Sie bleibt als Historie unverändert; 4.01 nennt den tatsächlichen Stand.

## Vorher: Version 4.0 (05.11.2026)
Release-Commit "Release v4.0: Aufgeräumt: Ganzheitliche Vereinheitlichung und Bereinigung der Oberfläche" (Commit dd8f43f) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 4.0 (Einzelheiten in Doku\Änderungen_v4.0.txt):
1. Globales Einstellungs-Menü: Zahnrad-Knopf in der Kopfzeile bündelt alle übergeordneten Konfigurationen (Darstellung, Startseite, 1-Fenstermodus, Datenschutz/Anonymisierung, Pfade & Netzlaufwerk, automatische Updates sowie Über/Danksagungen); die Fußleiste ist radikal auf Vorlagen und Startknopf entschlackt.
2. Einheitliche Einstellungszeilen (SettingRow): Dynamische Höhenberechnung ohne starren Deckel beseitigt abgeschnittene Zeilentexte; deterministisches Spaltenraster [Zustand] → [Risiko] → [Neustart] → [Schalter] verhindert Überlappungen von Badges und Titeln verlässlich.
3. Kategorielisten & Kurztitel: Kategorien in Wartung und Optimierung führen Kurztitel gegen Spaltenüberläufe und sprechende Fachsymbole (UI.IcoShield, UI.IcoClean etc.); einheitliche Pillen-Zähler in Akzentfarbe (X/Y).
4. Entflechtete Datenbank-Bedienung: Haken steuern ausschließlich den Mehrfachvergleich im Dashboard; Zeilenmarkierung steuert Einzelaktionen; neue Filter-Suchleiste, leere Zustandskarte und dynamische Spaltenbreiten.
5. Verbindliches Glossar & Sprache: Bereinigung jeglicher Entwicklersprache (Versionsbezüge, Skriptnamen); verbindliche Begriffe (Lauf, System, Modul, Aufgabe, Einstellung, Vorlage, Ablauf).
6. Dezimale Versionsprüfung & Barrierefreiheit: UI.VersionNumber verhindert Fehlalarme bei Update-Badges (3.882 < 3.91 < 4.0); Screenreader-Attribute (AccessibleRole/AccessibleName) auf allen interaktiven Steuerelementen; Tastatursteuerung mit F5, Strg+F, Strg+Enter.

## Vorher: Version 3.91 (31.10.2026)
Release-Commit "Release v3.91: Ergebnis-Dashboard, schlankes Optimierungs-Layout & Zeichensatz-Korrektur" (Commit e7d9a18) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.91 (Einzelheiten in Doku\Änderungen_v3.91.txt):
1. Ergebnis-Dashboard: Infoboxen/Kacheln vergrößert (Mindesthöhe 132 px, Innenabstand 16/12 px), Tabellenlayout mit AutoSize für Beschriftungen und rechtsbündigen Werten; kein Textabschneiden mehr bei Befundhinweisen oder DPC-Latenzen.
2. Klassifizierung entfernt: Subjektive Klassifizierungszeile ('High-End...', etc.) ersatzlos gestrichen; Gesamtscore steht prominent im Mittelpunkt.
3. Schlankes Optimierungs- und Wartungs-Layout: Technische Registry-Pfade und Hintergrunddetails vollständig in Tooltips ausgelagert; kompakte Zeilenhöhe (42–52 px) mit Kurzbeschreibung, Risikostufen-Badge und Schalter.
4. Seitenaufbau optimiert: Master-Detail-Bereich steht ab sofort an oberster Stelle ohne Scrollen; Voreinstellungen, Werkzeug-Aktionen, Wiederherstellungspunkt und Legende sauber am Seitenende platziert.
5. Vollständige Zeichensatz-Bereinigung: Beseitigung aller UTF-8-Mojibake-Artefakte ('prüfen', 'Speichern …') in Fußleiste, Modulseiten und Dashboard-Diagrammen.

## Vorher: Version 3.9 (31.10.2026) - Experimentell
Release-Commit "Release v3.9: Modernisierung, 1-Fenstermodus, persistente Menüleiste & ReBAR-Korrektur (Experimentell)" (Commit 0dfbb40) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.9 (Einzelheiten in Doku\Änderungen_v3.9.txt):
1. Master-Detail-Layout für Optimierung & Wartung: Übersichtliches zweispaltiges Design mit Kategoriewähler links (inkl. dynamischem Zähler wie '4/9') und Detailkarte rechts mit Schnellwahl ('Alle an' / 'Alle aus').
2. Interaktive Schalter (ToggleSwitch): Moderner Pillen-Schalter mit Anti-Aliasing und klarer Farbcodierung (Akzentfarbe / Grau) ersetzt altbackene Checkboxen.
3. 1-Fenstermodus & Ergebnis-Dashboard: Nach Abschluss wahlweise kein Browser-Popup; Umschaltung auf ein natives Ergebnis-Dashboard mit großer Gesamtwertung, Hardware-Pills und 2x3 Kacheln für CPU, GPU, RAM, Disk, DPC/Latenz und Systeminfo.
4. Persistente Menüleiste: Die linke Navigationsleiste bleibt während des gesamten Durchlaufs sichtbar; aktiver Modulschritt wird visuell mit cyanfarbenem Aktivitätsindikator hervorgehoben.
5. Moderne Schnellwahl-Pills: cmbPreset durch Pill-Buttons ([Minimal] [Leos Empfehlung] [Erweitert] [Auswahl aufheben]) und dynamische Vorlagen-Pills mit [x]-Entfernen ersetzt.
6. Kein Browser bei Wartung/Optimierung: Reine Pflege- und Tuningdurchläufe schließen ohne Öffnen eines externen Webbrowsers ab; Ergebnisse stehen direkt im Programmfenster.
7. Hardware-Erkennung Resizable BAR / SAM: Robuste PnP-Hardwareressourcen-Erkennung (pnputil) filtert virtuelle Treiber aus und erfasst 64-Bit-Adressbereiche über 256 MB verlässlich (behebt Fehlwarnung auf AMD Radeon RX 6800).

## Vorher: Version 3.882 (29.10.2026)
Release-Commit "Release v3.882: Zusammengefasste Systemnamen & Startseiten-Verlaufsknopf" (Commit b582625) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.882 (Einzelheiten in Doku\Änderungen_v3.882.txt):
1. Bearbeitung zusammengefasster Systemnamen: Das Umbenennen von Rechnern mit mehreren Läufen über 'Name ändern ...' (oder F2) aktualisiert alle Läufe des Systems in einem Schritt; benutzerdefinierte Gerätenamen werden in Spalte 1 (System) sowohl im eingeklappten Zustand als auch im Gruppenkopf zuverlässig anstelle des Computernamens angezeigt.
2. Schnelltaste F2: F2 öffnet sofort den Umbenennen-Dialog für das markierte System oder den markierten Einzellauf.
3. Startseiten-Layout & Verlauf-Button: Spaltenbreite im 2x2-Grid auf 55 % / 45 % optimiert, AutoSize für den Tastenbereich und DockStyle.Fill für die Befundanzeige; die Schaltfläche 'Verlauf' wird auf allen DPI-Skalierungen vollständig und unbeschnitten dargestellt.

## Vorher: Version 3.881 (29.10.2026)
Release-Commit "Release v3.881: Startseiten-Kachelrahmen & vollständige Datenbank-Systemdaten" (Commit c7358e0) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.881 (Einzelheiten in Doku\Änderungen_v3.881.txt):
1. Startseiten-Kachelrahmen & Layout: Saubere, unbeschnittene Umrandung aller 4 Kacheln durch PenAlignment.Inset mit 1-px-Sicherheitsabstand; harmonisierte Margins im 2x2-Grid und vergrößerter unterer Innenabstand verhindern Anstoßen an die Fußleiste.
2. Vollständige Systemdaten im eingeklappten Zustand: Abkehr von nativen Win32-Gruppenköpfen (LVGS_COLLAPSED), die Datenzeilen versteckten; kompakte Master-Zeilen zeigen alle Hardware- und Benchmarkdaten des neuesten Laufs in den Spalten 2–10.
3. Überlaufschutz in Spalte 1: Schlanke Formatierung '▶ Name (Anzahl)' bzw. '  Name' (12–18 Zeichen) passt perfekt ohne Textabschneiden in die 135 px Spaltenbreite; Messdatum steht in Spalte 2.
4. Sofortige Interaktion & Aktionsknöpfe: Eingeklappte Rechner sind echte ListViewItems mit Tag = neuester Lauf; Klick aktiviert sofort alle Fußleisten-Aktionen ('Bericht öffnen', 'Verlauf', 'Im Dashboard vergleichen', 'Als Referenz'); Doppelklick öffnet den Bericht.
5. Intuitive Aufklapp-Mechanik: Klick auf den Pfeil oder Doppelklick klappt das System auf ('▼ Name (Anzahl)') und listet alle Einzelläufe eingerückt darunter auf; Toolbar-Schaltfläche ('Alle aufklappen' / 'Alle einklappen'), Kontextmenü und Tastaturnavigation schalten synchron um.
6. Hoher Dark-Mode-Kontrast: Text in Spalte 1 und allen Datenfeldern behält mindestens 7:1-Kontrast im Hell- und Dunkelmodus; Pfeilsymbole in beiden Farbschemata klar sichtbar.

## Vorher: Version 3.88 (29.10.2026)
Release-Commit "Release v3.88: Einklappbare Datenbank-Gruppen, Dark-Mode-Kontrast, Startseiten-Layout & Zeichensatz-Korrektur" (Commit 16a08c2) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.88 (Einzelheiten in Doku\Änderungen_v3.88.txt):
1. Einklappbare Gerätegruppen in der Datenbank: Native Win32-Unterstützung (LVGS_COLLAPSIBLE) mit Windows-Chevrons; Systeme mit mehreren Läufen und Referenzsysteme starten standardmäßig kompakt eingeklappt; neue Aktionen 'Alle aufklappen / Alle einklappen' in Toolbar und Kontextmenü.
2. Dark-Mode-Kontrast für Gruppenköpfe: Gruppenköpfe der Datenbank-ListView werden im Dark Mode mit hohem Kontrast (>= 7:1) in UI.Accent/UI.Text gerendert (beseitigt unlesbares Windows-Theme-Dunkelblau auf schwarzem Grund).
3. Startseiten-Layout: Kachel 'Letzter Lauf' gegen Textumbruch abgesichert; Befundstatus (lblBef) bricht niemals vertikal oder zeichenweise um; Aktions-Schaltfläche prägnant als 'Dashboard' benannt und Buttons kompakt ausgerichtet.
4. Robuste Hardware-Erkennung: Kopf-Steckbrief und Systemübersicht filtern virtuelle Display-Adapter (Virtual Desktop, Parsec, spacedesk, IddSampleDriver, Remote Desktop) aus und zeigen bevorzugt die reale dedizierte Grafikkarte (dGPU, z. B. Radeon RX 6800) bzw. den physischen Grafikbeschleuniger.
5. Stabilität des Mehrkern-Benchmarks: Mehrkern-Skalierungsbefund stuft Leistungsabfälle bei erkannter Hintergrundlast (> 15 % CPU oder Hintergrunddienste) sachlich als INFO statt als irreführende Warnung ein.
6. Vollständige Zeichensatz-Bereinigung: Beseitigung aller UTF-8-Doppelkodierungen (Mojibake) in HTML-Diagnoseberichten, Dashboard-Diagrammen und Vergleichsbausteinen; Unicode-Escape-Sequenzen für Pfeilsymbole der Startseiten-KPIs.

## Vorher: Version 3.87 (29.10.2026)
Release-Commit "Release v3.87: Läufe je Gerät bündeln (Verlauf ohne eigene Seite)" (Commit a08b906) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.87 (Einzelheiten in Doku\Änderungen_v3.87.txt):
1. ListViewGroup-Gruppierung in der Datenbank: Automatische Gruppierung nach Rechnern über Geraet.Id oder Computername mit Gerätename, Anzahl Läufe und jüngstem Messdatum. Referenzprofile in geschlossener Gruppe am Ende. Volle Farbschema-Unterstützung (Hell/Dunkel).
2. Verlauf-Aktionen in Datenbank & Startseite: Schaltfläche 'Verlauf' und Kontextmenü in der Datenbankansicht; Kachel 'Letzter Lauf' auf der Startseite mit Trend-Pfeilen (↑ +X %, ↓ -X %) für 4 Kern-KPIs gegen den direkten Vorläufer.
3. Interaktiver Verlaufsmodus im Dashboard: Chronologischer Timeline-Vergleich desselben Rechners (-DashboardModus Verlauf / ?mode=history), Delta-Badges zum Vorläufer, Zeile „Geändert (Inventar)“ für Modifikationen an BIOS, Treibern, OS-Build und RAM-Takt sowie Zeitreihenkurven mit Vergleichbarkeitsfilter.
4. Neuer Berichtsabschnitt 'Seit dem letzten Lauf': Kernmesswerte-Deltas, Inventaränderungen und geschätzte lineare SSD-Verschleiß-Hochrechnung im HTML-Diagnosebericht und in der KI-Zusammenfassung.
5. Strikte 3-Läufe-Regel für Trend-Befunde: Keine verfrühten Warnbefunde bei 1 oder 2 Läufen; Trend-Befunde (z. B. CPU-Temperaturanstieg) erst ab 3 Läufen bei signifikanter statistischer Überschreitung der Messstreuung (> doppelte Standardabweichung und >= 8 K).

## Vorher: Version 3.86 (29.10.2026)
Release-Commit "Release v3.86: Update-Prüfung, Grafiktreiber-Links, Resizable BAR, geprüfter System-Umzug mit WLAN & Gesamtläufe" (Commit abe9f83) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Release-Commit "Release v3.86: Update-Prüfung, Grafiktreiber-Links, Resizable BAR, geprüfter System-Umzug mit WLAN & Gesamtläufe" (Commit af875cb) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.86 (Einzelheiten in Doku\Änderungen_v3.86.txt):
1. Asynchrone Update-Prüfung & Self-Replace: Lautlose GitHub-Versionsprüfung im Hintergrund mit 24h-Cache; dezenter Badge in der Kopfzeile und Update-Kachel auf der Startseite; Download von LMB.exe nach Minibench-Daten\Update\, SHA-256-Validierung und nahtloser Austausch mit Archivierung nach Minibench-Daten\Archiv\v<alteVersion>\ über Update.cmd; abschaltbar über Einstellungen.
2. Offizielle Grafiktreiber-Links & Laptop-Hinweis: Direkte Hersteller-Downloadlinks für NVIDIA, AMD und Intel; dedizierte Schnell-behebbar-Karte mit 'Treiber laden'-Schaltfläche bei Treibern älter als 18 Monate; klarer OEM-Hinweis bei Hybridgrafik auf Notebooks; zusätzliche Treiberversions-Gegenprüfung via nvidia-smi.
3. Resizable BAR / Smart Access Memory Erkennung: Hardwareseitige Erkennung für NVIDIA (BAR1-Speichergröße), AMD und Intel (64-Bit PCI-Speicherfenster über 256 MB); Verifizierung der Plattformunterstützung (RTX 3000+, RX 6000+, Arc / Ryzen 3000+, Intel 10th Gen+); gelbe Diagnosewarnung und interaktiver BIOS-Leitfaden nur bei inaktivem ReBAR auf unterstützter Hardware.
4. System-Umzug mit WLAN-Profilen: Lautloser Export von WLAN-Profilen per netsh wlan; Klartext-Export von Kennwörtern (key=clear) ausschließlich nach expliziter Bestätigung eines Sicherheitsdialogs; vollautomatischer Import bei Wiederherstellung; strikter Ausschluss bei Anonymisierung und Git-Schutz für Umzugsordner.
5. Geprüfte Sicherung & Trockenlauf: Robocopy-Differenzabgleich (/L) und SHA-256-Stichprobenprüfung für ca. 20 Dateien je Bibliothek mit detailliertem Pruefprotokoll.txt und Berichtsausweis; interaktiver Trockenlauf (-Trockenlauf) in Wiederherstellen.cmd und Wiederherstellen.ps1 listet alle Aktionen ohne Änderungen auf.
6. Gesamtläufe in Bauprozess integriert: Veraltete Parameter bereinigt, neue Integrationsfälle für Wartung, Diagnose und System-Umzug ergänzt; verbindliche Ausführung aller Gesamtläufe in Bauen.cmd vor dem Release mit Laufzeitdokumentation in Stand.txt; Reduzierung der Skips von 99 auf 4.

## Vorher: Version 3.85 (29.10.2026)
Release-Commit "Release v3.85: Sensor-Manifest-Selbstreparatur, lautloser Smartctl-Lifecycle, RTX 5080 Referenz & UI-Standards" (Commit f21b8ad) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.85 (Einzelheiten in Doku\Änderungen_v3.85.txt):
1. Manifest-Selbstreparatur für Sensoren & Tools: Erkennung und automatische Selbstreparatur leerer SHA-256-Prüfsummen ("") in Tools.json (LibreHardwareMonitor und PawnIO); lautloser Abgleich gegen hinterlegte Release-Hashes verhindert Fehlalarme.
2. Bereinigung des Diagnoseberichts: Technische Werkzeug- und Treiberwarnungen ($script:ToolIssues) werden in Start.log verlagert und belasten den kundenorientierten Diagnosebericht nicht mehr.
3. Lautloser Smartmontools-Lifecycle: Bevorzugung vorhandener smartctl.exe im Tools-Ordner ohne winget-Aufrufe; Deinstallation ausschließlich am Gesamtlaufende mit striktem NSIS-Silent-Schalter (/S) ohne Dialogfenster.
4. UI-Standards & Ablauf-Optimierung: Schaltfläche in der Laufansicht in „Zurück“ umbenannt; Grafikstresstest und parallele lesende Hintergrundprüfungen standardmäßig aktiviert.
5. High-End-Referenz RTX 5080 & Desktop-Median: Modernisierung von Desktop_HighEnd.json auf AMD Ryzen 7 9800X3D und NVIDIA GeForce RTX 5080; RX 6800 als weiteres Profil (Desktop_HighEnd_RX6800.json) erhalten; Desktop-Median (Referenz_Desktop.json) neu berechnet und autark eingebettet.

## Vorher: Version 3.84 (28.10.2026)
Release-Commit "Release v3.84: GitHub-Härtung, granulare Umzugs-Messung, lautloser Sensoren-Lifecycle & Text-Feinschliff" (Commit ce16931) auf https://github.com/miniLeonardo/MiniBench (Zweig main).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.84 (Einzelheiten in Doku\Änderungen_v3.84.txt):
1. GitHub-Härtung & Git-Disziplin: Striktes Verbot von Binärdateien (*.zip, *.7z, *.tar, *.rar, *.exe, *.dll, *.sys) im Git-Tracking; vollständige .gitignore-Absicherung verhindert automatisierte Missbrauchs- und Account-Sperren dauerhaft.
2. Granulare Umzugs-Bibliotheken: Einzelauswahl aller Benutzerbibliotheken (Desktop, Dokumente, Downloads, Bilder, Musik, Videos) mit Schnellauswahl (Alle / Keine); Downloads standardmäßig abgewählt.
3. Echte Größenmessung beim Schnell-Scan: Beseitigung statischer 2-GB-Pauschalen; exakte Ordnergrößenermittlung in Bytes für alle Bibliotheken und C:\-Zusatzordner mit dynamischer Größenbeschriftung und Speicherplatzkalkulation.
4. Lautloser Sensoren-Lifecycle: Unterdrückung interaktiver Deinstallationsdialoge von PawnIO via /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /S -uninstall -silent; Treiber bleibt während des gesamten Mehrfachlaufs im Speicher und wird ausschließlich am Gesamtlaufende freigegeben; Protokollierung ohne modale Fehlerdialoge.
5. Text- & Oberflächen-Feinschliff: Checkbox chkAnon als „Personenbezogene Daten entfernen“ beschriftet; Seite Versionen vollständig von Mojibake befreit und auf übersichtliche Stichpunkte (•) umgestellt.
## Vorher: Version 3.83 (23.10.2026)
Release-Commit "Initial commit: Leos Minibench v3.83" (Commit b7ee742) auf https://github.com/miniLeonardo/MiniBench.
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.83 (Einzelheiten in Doku\Änderungen_v3.83.txt):
1. Schnell-Scan per Knopfdruck: Manueller Start der Bibliotheks-, Layout-, Profil- und Ordnererkennung über die Schaltfläche „Schnell-Scan durchführen“ im Bereich Sicherungsumfang statt automatischem Start beim Betreten der Seite.
2. Bündiges Kartenlayout im Lasttest-Stil: Vollständige Umstellung aller drei Umzug-Gruppen (Zielmedium, Sicherungsumfang, Status & Sicherung) auf das bewährte Card(content)-Muster; Gruppenüberschriften (Section) und Erläuterungen (Hint) liegen außerhalb des abgerundeten Kartenpanels (CardPanel), wodurch abgeschnittene Umrandungen, Lücken oder Kerben vollständig beseitigt sind.
3. Beseitigung von Zeichensatzfehlern (Mojibake): Alle Oberflächentexte und Beschriftungen der Umzugsseite in DiagGui_Seiten.cs wurden auf standardisierte Unicode-Escape-Sequenzen (\u00FC, \u00E4, \u00F6, \u00DF, \u2713, \u00B7) umgestellt; Umlaute und Sonderzeichen werden fehlerfrei dargestellt.

## Vorher: Version 3.82 (23.10.2026)
Release-Commit "Release v3.82: Hotfix 2: System-Umzug Prozessablauf, Vorab-Scan, Live-Log & Kartenlayout" (Commit b750f2a).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.82 (Einzelheiten in Doku\Änderungen_v3.82.txt):
1. Prozessablauf & Performancesprung: Behebung einer 15-minütigen Verzögerung durch Verschiebung von Module\Umzug\Ablauf.ps1 vor Ablauf\Start.ps1 im Bauplan und sofortigen Ausstieg nach der Sicherung; Ausschluss von UAC-Erhöhungsversuchen im nicht-privilegierten Kontext (Kern\Adminrechte.ps1); Absicherung externer Aufrufe (winget export, reg.exe) mit Timeouts und Prozess-Terminierung; Teilsicherungen (wie Desktop-Layout) schließen nun in 2 bis 3 Sekunden ab.
2. Kategorisierter Schnell-Scan: Schneller, speicherschonender Vorab-Scan ermittelt vor der Auswahl des Sicherungsumfangs tatsächlich vorhandene Bibliotheken, Layout-Eigenschaften, den Winget-Paketmanager und installierte Softwareprofile (Webbrowser, Thunderbird, Entwickler-Editoren, Tools, Terminal) und visualisiert diese kategorisiert mit Statusmeldung.
3. Echtzeit-Protokollfenster (Live-Log): Neues durchlaufendes Protokollfenster (txtUmzugLog) im Statusbereich zeigt jeden Schritt, kopierte Dateien und Meldungen des Sicherungsprozesses live mit Zeitstempeln an.
4. Bündiges Kartenlayout: Alle drei Kartenpanel (Zielmedium, Sicherungsumfang, Status) auf einheitliche Gruppenbreite (grpW = UI.S(780)) fixiert; Textumbrüche (MaximumSize) aller Labels und Steuerelemente verhindern das Überlappen oder Abschneiden von Rahmenlinien.
5. Arbeitsspeicher-Optimierung: Vollständige Beseitigung synchroner rekursiver Dateizählungen (EnumerateFiles("*", AllDirectories)) beim Öffnen der Seite verhindert sprunghaften RAM-Anstieg.

## Vorher: Version 3.81 (23.10.2026)
Release-Commit "Release v3.81: Hotfix: System-Umzug Optimierung, Profile & Konfigurationen, Performance & Filterung" (Commit 18ff90f).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.81 (Einzelheiten in Doku\Änderungen_v3.81.txt):
1. Robocopy-Multithreading & Performance: Beschleunigung der Dateispiegelung durch parallele Ausführung mit 16 Threads (/MT:16) und Vorankündigung der Datenvolumina je Bibliothek.
2. Live-Transparenz beim Kopieren: Echtzeit-Anzeige der aktuell kopierten Datei bzw. des Unterverzeichnisses unter dem Fortschrittsbalken; Drosselung der Ereignismeldungen auf max. 150 ms für reaktive Oberfläche.
3. Profile & Konfigurationen (Configs): Ausbau des bisherigen Browser-Profil-Bereichs zu einer umfassenden Konfigurationssicherung für Browser (Edge, Chrome, Brave, Firefox, Opera, Vivaldi), E-Mail (Thunderbird), Entwicklung (VS Code, Cursor, Notepad++, Git, PowerShell), Tools (FileZilla, WinSCP, Total Commander, PuTTY) und Windows Terminal mit granularer Auswählbarkeit und autarkem Restore.
4. Erweiterte Blacklist & präzise Speicherplatzkalkulation: Ausschluss von Treiber-, Einrichtungs- und Cache-Ordnern (.cache, AMD, Intel, NVIDIA, inetpub, temp, minidump, wsl) auf C:\; Standard-Deaktivierung gefundener Zusatzordner und exakte Aufsummierung der tatsächlichen Ordnergrößen.
5. Oberflächen-Korrekturen & Sicherheit: Deaktivierung von UseMnemonic bei CheckBoxes stellt kaufmännische Und-Zeichen (&) korrekt dar; Bestätigung und Absicherung des strikt lesenden Quellenzugriffs.

## Vorher: Version 3.8 (23.10.2026)
Release-Commit "Release v3.8: Modul „System-Umzug & Neuinstallation“ (Backup & 1-Klick-Restore)" (Commit e38dee6).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.8 (Einzelheiten in Doku\Änderungen_v3.8.txt):
1. Modul „System-Umzug & Neuinstallation“: Neues Kernmodul (src\Module\Umzug\) und Menüpunkt in der Seitenleiste unter Werkzeuge zur transparenten Sicherung vor einer Windows-Neuinstallation auf Wechsellaufwerke (USB-Stick, externe SSDs) oder Netzlaufwerke (NAS).
2. Autarkes 1-Klick-Wiederherstellungspaket: Automatische Erstellung von Wiederherstellen.cmd und Wiederherstellen.ps1 im Stamm des Sicherungsordners. Ermöglicht 1-Klick-Restore auf dem neu installierten Windows-PC völlig unabhängig von einer Minibench-Installation mit dynamischer Benutzerprofilauflösung ($env:USERPROFILE).
3. Sicherungsumfang & Robocopy-Engine: Zuverlässige Spiegelung von Benutzer-Bibliotheken (Desktop, Dokumente, Downloads, Bilder, Musik, Videos mit Registry User Shell Folders Auflösung), selektiven AppData (Browser-Profile von Edge, Chrome, Brave, Firefox und Thunderbird-Mail) sowie benutzerdefinierten Zusatzordnern auf Laufwerk C:\ (unter Filterung der Windows-Systemblacklist). Robocopy-Parameter /XJ, /COPY:DAT, /R:1 /W:1 und Ausschluss von Temp- und Cache-Verzeichnissen.
4. Desktop-Zustand 1:1: Extraktion des aktiven Hintergrundbilds (TranscodedWallpaper als Wallpaper.jpg) samt WallpaperStyle in Layout.json sowie vollständiger Export der Icon-Koordinaten via Shell-Bags (IconPositionen.reg) und Icons.json.
5. Software-Inventarisierung & winget: Automatischer Paketexport via winget export nach Programme\winget-pakete.json sowie Registry-Vergleich zur Generierung des Berichts Nicht-Winget-Programme.txt für manuelle Softwareinstallationen inklusive Google-Download-Suchlinks.
6. Dateisystem-Schutz & Speicherplatzprüfung: Automatische Erkennung beschreibbarer Wechsellaufwerke, Warnung bei FAT32-Formatierung (4-GB-Dateilimit) und vorzeitige Speicherplatzkalkulation mit visueller Statusanzeige in der WinForms-Oberfläche.

## Vorher: Version 3.73 (17.10.2026)
Release-Commit "Release v3.73: Kompakte Modulnamen, LMB.exe, mathematische Modellkorrektur, Dashboard-Fallback & Bereinigung externer Quellen" (Commit ed8dee5).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.73 (Einzelheiten in Doku\Änderungen_v3.73.txt):
1. Kompakte Modulnamen & LMB.exe: Straffung der Bezeichnungen in Seitenleiste und Fenstertiteln von „Vergleichsdatenbank“ zu „Datenbank“ und von „Systemänderungen“ zu „Änderungen“; Umstellung der ausführbaren Hauptdatei auf LMB.exe (inklusive LMB.cmd und Abwärtskompatibilitäts-Starter LeosMinibench.cmd).
2. Dashboard-Optimierungen: Chronologische Sortierung der Systemauswahl und Vergleichschips standardmäßig absteigend nach Messdatum (neueste Systeme zuerst); automatisches Fallback auf Benchmark-Sensortelemetrie bei fehlender Lasttest-Zeitreihe (Sensorverlauf (Benchmark-Messung)) verhindert leere Diagramme.
3. Mathematische Modellkorrektur: Gesamtsystem-Effizienzscore auf gewichtetes harmonisches Mittel der gemessenen Benchmark-Effizienzen von Prozessor und Grafikkarte umgestellt (strikte Monotonie, Behebung der Marvin-vs-TORRENT-Anomalie); Nutzungsprofil Workstation durch Anhebung des Mehrkern-Gewichts (CPU_MT auf 6,0 bzw. 60–65 % Gesamteinfluss) praxisgerecht kalibriert.
4. Bereinigung externer Quellen: Sämtliche inoffiziellen Drittanbieter-Mirrors im Optimierungsmodul entfernt; Bezug von Fremdwerkzeugen ausschließlich von offiziellen Herstellerseiten, GitHub-Releases oder winget mit fester SHA-256-Prüfsumme im Manifest (Tools.json).
5. Dokumentation & GitHub-Außenwirkung: Neugestaltung der README.md mit vollständiger Modulübersicht, aktualisierten Bildschirmfotos (Startseite_Neu.png, Dashboard_Neu.png) und sichtbarer Danksagung an das Projekt Windows Optimisation Pack von Marvin700; strukturierte CHANGELOG.md (Neu, Verbessert, Behoben).

## Vorher: Version 3.72
Inhalt von 3.72 (Einzelheiten in Doku\Änderungen_v3.72.txt):
1. Neue dreiteilige Startseite (Drei-Fragen-Prinzip): Aufgeräumte Einstiegsseite als Standardansicht ganz oben in der Seitenleiste. Zwei dynamische Zustände: Zustand A (Erststart ohne Datenbankläufe) mit Begrüßung, Hardware-Steckbrief („DIESER PC“) inkl. Inventar-Link, vier interaktiven Aufgabenkarten, Schritt-für-Schritt-Ablauf und Werkzeug-Verweisen; Zustand B (Folgestart mit vorhandener Datenbank) mit Schlüsseldaten in Badges („Willkommen zurück“), 2x2-Kachelmatrix für den letzten Lauf (KPIs, Bericht öffnen, Dashboard-Vergleich), asynchroner Kachel „Schnell behebbar“ für Konfigurationsprüfungen, Aufgabenliste mit Datum des letzten Laufs sowie Werkzeuge-Live-Status. Abschlussleiste mit Option „Beim Start diese Seite zeigen“, Kurzanleitung und „Was ist neu“ (dynamisches Neu-Badge bei Versionswechsel).
2. Interaktiver Schnellstart & Ablaufsteuerung: Klick auf eine Aufgabenkachel wählt die zuständigen Module in der Seitenleiste an, lädt Standardvorgaben und springt zur ersten Seite; Fußleiste zeigt im Leerzustand den deaktivierten Startknopf „Ablauf wählen“ und aktiviert sich automatisch nach Modulauswahl.
3. Vollständige WinSAT-Bereinigung: Entfernung aller veralteten WinSAT-Funktionen (-BenchGpuWahl, chkGpuWahl, WinSAT-Schritt, WinSAT-SVG und Auswertungen im Bericht) zugunsten moderner nativer Messungen.
4. Automatische Standard-Referenz: Automatische Wahl von Referenz_Notebook.json bzw. Referenz_Desktop.json anhand der Hardware-Erkennung (Test-IsNotebookSystem).
5. System-Feinschliff: Sachliche Benennung der Konfigurationsprüfung im Diagnosebericht, asynchrone Hintergrundprüfung von XMP/EXPO, BIOS-Alter und LAN-Link ohne UI-Blockade, durchgehende Dialoggestaltung mit UI.ThemeDialog. Transparente Hintergründe in PillLabel, StartTaskCard und StartTaskRow über ControlStyles.SupportsTransparentBackColor abgesichert.

## Vorher: Version 3.71
Inhalt von 3.71 (Einzelheiten in Doku\Änderungen_v3.71.txt):
1. Präzise Stromverbrauchsschätzung (PSU-Effizienzmodell): Umstellung der DC- und AC-Berechnung auf ein physikalisches Hardware- und Netzteil-Modell mit 230V EU-Effizienzkurven (Unbekannt, 80 PLUS White bis Titanium) und linearer Stützstellen-Interpolation (5 % bis 100 % Netzteilauslastung); interaktive Konfigurationsleiste direkt im HTML-Bericht (Nennleistung in Watt und Effizienzklasse) für Desktops mit Live-Neuberechnung aller Verbrauchswerte, Auslastungsgrade und Stromkosten sowie Speicherung im Browser (localStorage); automatische Erkennung mobiler Plattformen mit OEM-Netzteil-/Akkumodell (~90 % Wirkungsgrad) ohne ATX-Netzteilwahl; detaillierte DC-Erfassung inkl. Mainboard-VRM-Verlusten, AMD-Radeon-TBP-Zuschlägen (+4 W / +15 %), DIMM-Arbeitsspeichermodulen, Datenträgertypen (NVMe/SATA/HDD) und Lüftern sowie Warnung bei Netzteil-Überlastung (> 85 % unter Volllast).
2. System-Startzeit & Nutzungsprofil-Transparenz: Standardmäßig geöffnetes Boot-Performance-Element (<details class="box grp" data-section="system" open>) im HTML-Bericht; saubere Trennung und verständliche Erläuterung der beiden Boot-Phasen aus Event 100 (MainPathBootTime: Desktop/Treiber einsatzbereit vs. BootPostBootTime: Autostart bis vollständiger Leerlauf); Nutzungsprofile (Gaming, Büro, Workstation) mit transparentem Hover-Tooltip zur genauen mathematischen Zusammensetzung und Gewichtung der Komponenten; Referenzsystem-Auswahl (Gespeicherte Referenz, Mediane, Datenbankeinträge) und Referenz-Speichern im Benchmark-Reiter der Benutzeroberfläche wiederhergestellt.
3. Berichts-Feinschliff & Layout-Optimierung: Nutzungsprofile mit farbigem Akzent-Hintergrund (var(--accent-soft)) und neutrale Komponentenkarten (CPU, GPU, RAM, Laufwerke) prominent über den Benchmark-Detailtabellen wiederhergestellt; Facts-Tabelle von redundanten Leistungsschätzungen bereinigt; Block für Gesamtsystemverbrauch und Stromkostenrechner direkt über den Benchmark-Sensoren platziert.
4. Dashboard- & Dark-Mode-Korrekturen: Horizontale und vertikale Trennlinien in Spalte 1 der Matrix-Tabelle im Dark Mode durch kontrastreiche Linienfarben (#444E5E / #363E4D) und z-index-Priorisierung repariert; Emojis (🎮, 💼, 🚀, 📊) beibehalten; Dashboard-Start bei Erstbenutzung ohne Parameterfehler stabilisiert.

## Vorher: Version 3.7
Inhalt von 3.7 (Einzelheiten in Doku\Änderungen_v3.7.txt):
1. Schnell behebbare Konfigurationsfehler: Automatische Erkennung von LAN-Verhandlungsrate gebremst (100 Mbit/s oder Halbduplex an Gigabit/2.5GbE-Adaptern), Monitorkabel an iGPU statt dGPU bei Desktops, Hz-Bremse bei Monitoren >= 75 Hz auf 60 Hz limitiert, inaktivem RAM-Speicherprofil (XMP/EXPO/DOCP) und veraltetem BIOS (> 2 Jahre) mit direkten Hersteller-Supportlinks sowie transparenter Bestätigung der Prüfung im Bericht.
2. System-Startzeit-Analyse (Boot Performance Breakdown): Aufschlüsselung der Windows-Boot-Phasen aus Event 100 (MainPathBootTime, BootPostBootTime, BootDuration) und UEFI/BIOS-POST-Zeit (FwPOSTTime) mit visueller Wasserfall-/Stack-Kaskade im HTML-Bericht (eingeklappt am Berichtsende); Auswertung von Autostart-Verzögerern (Event 101-110 ab 1,0 s) und robuster Fallback ohne Schätzwerte bei unvollständigen Ereignissen oder deaktiviertem Protokoll.
3. Harmonisierte Sensoren-Zustandsmaschine: Konsistente 4-Stufen-Statuslogik (Vollständig, Eingeschränkt, Inaktiv, Fehler) im Skript und in der Benutzeroberfläche; automatische Treiberinitialisierung bei Vorliegen von Administratorrechten ohne zusätzliche Rückfragen und garantierte Treiberentladung bei Programmbeendigung.
4. Benchmark-Vereinfachung & Dashboard: Entfernen redundanter Dropdowns und Checkboxen (Messdauer, Referenz, DB-Speicherung); Benchmark speichert immer in der Vergleichsdatenbank. Beseitigung der Scrollbars in der Vergleichsmatrix mit Wiederholung der Systemnamen am Tabellenende (tfoot), gut sichtbaren Zeilentrennlinien in der Basiskolumne und metrischen Einheiten (°C, %). Stromkostenrechner standardmäßig geöffnet, Details und Zeitbedarf standardmäßig eingeklappt. Redundante Referenz-Spalten und Profilkarten aus dem Einzelbericht entfernt (vollständig durch das interaktive Dashboard abgedeckt).
5. Modulares Dashboard: Vollständige Zerlegung des Dashboard-Quelltexts in separate semantische Bausteine (dashboard.html, dashboard.css, daten.js, matrix.js, profile.js, diagramme.js) mit robuster Einbettung im Zusammenbau-Skript und Syntaxprüfung via node --check.
Inhalt von 3.67 (Einzelheiten in Doku\Änderungen_v3.67.txt):
1. Multi-System-Sensorverlauf: Achsenbeschriftung Temperatur (°C) durch 75 px Randabstand und linksbündige Ausrichtung vollständig sichtbar; Tooltip mit kompakten Systemnamen (getShortSysName), zweizeiligem Layout und stets vorangestelltem Basissystem (auch ohne Zeitreihe).
2. Differenzierte Nutzungsprofile: Gewichtung für Gaming (70 % GPU-FPS, 20 % ST, 10 % RAM/SSD; Kerne > 8 neutral), Büro/Desktop (45 % SSD-Reaktionszeit, 35 % ST, 20 % RAM-Latenz; 0 % GPU/MT) und Workstation (45 % MT, 20 % Instruktionen, 20 % RAM-Bandbreite, 15 % Dauertransfer); redundante Stromverbrauchs-Badges im Profilgitter entfernt.
3. Hervorhebung des Basissystems: Markante Akzent-Umrandung mit Anker-Symbol im Auswahlbereich, durchgehende Hintergrundhervorhebung mit 100-%-Basis-Badge in Hardware- und Matrix-Tabelle sowie Akzent-Rahmen in den Profilkarten.
4. Hardware-Tabelle aufgeräumt: Redundante Berichtszeile entfernt; kompakte Datenträger-Zusammenfassung nach Typ und Gesamtkapazität mit vollständiger Modellliste im Hinweis beim Überfahren.
5. Kalibrierte Moduldauern: Grundzeit der Diagnose auf 0 Min. gesenkt, Zuschläge für Systemprüfungen und WinSAT auf reale SSD-Dauern kalibriert.
6. Dashboard-Feinschliff: Versionspille dynamisch an Skriptversion gekoppelt, kompakte Matrix-Tabellenzeilen und fixierte Tabellenköpfe (sticky) beim Scrollen.

## Vorher: Version 3.66
Inhalt von 3.66 (Einzelheiten in Doku\Änderungen_v3.66.txt):
1. Dark Mode beim Start: SetPreferredAppMode vor Control-Erzeugung, OnShown und ShowPage wenden DarkMode_Explorer rekursiv auf alle Win32-Scrollbars an.
2. Sensor-Status-Badge in der Kopfzeile: Dezentes Badge für aktiven Modus (LHM + PawnIO aktiv oder Windows-Fallback), gekoppelt an Treiberwahl.
3. Dashboard-Telemetrie für Neu-Läufe: Export-BenchDashboardHtml nach Anhang.zip-Erzeugung; extractCsvSeries liest auch .minibench und Fallback-Dateien.
4. Canvas-Diagramm: Zwei getrennte Y-Achsen (links: °C in Blau/Orange, rechts: Watt in Grün/Gelb); zentrierte Infokarte mit Peak-Werten bei Systemen ohne Zeitreihe.
5. GPU-Effizienz: Strikt in FPS/Watt aus Render-FPS (GPU|REND) geteilt durch Maximalleistung (GpuWMax); Beschriftung im Bericht auf FPS/W korrigiert.
6. HTML-Bericht: Sticky-Navigation mit dezentem Schatten und flüssiger Nach-oben-Knopf unten rechts ab 300 px Scrolltiefe.
7. Bildschirmfoto: Doku\Screenshots\Oberflaeche neu.png als Standard-Vorschaubild Oberflaeche.png übernommen.

## Vorher: Version 3.65
Inhalt von 3.65 (Einzelheiten in Doku\Änderungen_v3.65.txt):
1. Einheitliche Karten (CardPanel) für Optionen der Diagnose, Tabellen des Lasttests, Paketauswahl der Tools und Listen der Laufansicht.
2. Wartung im Aufbau der Optimierung (CategoryBlock) mit Gruppenbeschreibung, "alle" und "keine", Etiketten rechts und "Alle aufklappen".
3. Angeglichene Laufansicht mit Kartenrahmen, Etiketten in normaler Schreibweise und themenangepasstem Protokollfeld.
4. Korrekturen nach Windows-Test: Zeitangabe mit Parallelausführung, Diagnose (parallel) in Fußleiste, Optimierungsknöpfe zweizeilig,
   Netzlaufwerk-Karte ohne Textabschnitt, größeres Schildsymbol.
5. Diagnosebericht: KPI-Kopfzeile (.cards) auf minmax(120px, 1fr) mit kompakten Abständen und Schriftgröße angepasst, sodass alle
   sieben Kennzahlen (inklusive Zuverlässigkeit) einzeilig nebeneinander stehen und nicht mehr in eine zweite Zeile umbrechen.

## Vorher: Version 3.64
Inhalt von 3.64 (Einzelheiten in Doku\Änderungen_v3.64.txt):
1. Gestaltungsregeln in der Klasse UI (Abstände, Seitenrand 24 px, Gruppenbreite 780 px, Schriftstufen), neue Steuerelemente
   Badge, GroupHeader und PrimaryButton, Zweizeilen-Optionen (Opt2), UI.OnAccent für Kontrast auf Akzentflächen.
2. Seitenleiste in Ablauf und Werkzeuge, Schalter "beim Start ausführen" im Seitenkopf, flache Kopfzeile mit Symbolknöpfen,
   Seite Systemänderungen, Fußleiste mit Ablauf und Gesamtdauer, Startknopf mit Beschriftung nach Auswahl.
3. Zusammenfassung vor dem Start (StartUebersicht) nur bei Eingriffen, Neustarts, abgewähltem Wiederherstellungspunkt oder
   verwalteten Einträgen auf Domänen-PCs; ersetzt die Rückfragen von Wartung und Optimierung.
4. Diagnose ohne Funktionstest und Nur Absturzanalyse (Skriptparameter bleiben), Lasttest als Tabelle, Wartung und
   Optimierung mit Etiketten, Tools mit einer Bildlaufleiste und Werkzeugkarten mit Zustand.

## Offene Befunde
1. Sichtprüfung unter Windows bei 125 und 150 % Skalierung nach der Schriftkorrektur in 4.01 (in der Sandbox nicht nachstellbar, Mono rechnet fest mit 96 dpi).
2. Kategorie Apps der Optimierung nutzt dasselbe Symbol wie Tools (UI.IcoApp = UI.IcoTools).
3. Startzeit nach den Änderungen in 4.01 unter Windows messen (Start.log, zweiter Start).
4. Rendertest auf sehr schnellen Karten (RTX 3070): hoher Mikroruckler-Anteil und niedriges 1-%-Low nur im Benchmark, nicht im Lasttest.
5. Update-Funktion unter Windows bestätigen, sobald das Release v4.03 mit LMB.exe und Prüfsumme auf GitHub liegt.
6. smartctl -X und PCIe-Abfrage unter Last unter Windows bestätigen.
7. 4.05 unter Windows bestätigen: winget export als Administrator (Pakete im Profil eines anderen Benutzers), GPU-Z über den
   Uninstall-Eintrag, Seite Tools bei 125 und 150 %, Menü "…" im dunklen Farbschema.
8. Die Sperre der Optimierung gilt bis zum nächsten Start; nach Rückgängig auf der Seite Änderungen erst dann wieder offen.

9. 4.06 unter Windows bestätigen: Update 4.05 → 4.06 (vorher Cache\UpdateCheck.json löschen), "Jetzt suchen" hinter dem
   Proxy, "Median aktualisieren" mit Meldung, Anzeige der geltenden Referenz.

## Offene Vorschläge (Stand 4.05, nicht umgesetzt)
Aus 4.03 offen: Datenbank-Spalte System nur mit PC-Name; Restzeit des Laufs aus den Abschnittszeiten; HTML-Bericht mit derselben Kurzfassung wie die Ergebnisseite; KI-Datei
"In Zwischenablage kopieren"; README mit echten Windows-Aufnahmen von 4.0x. Wartung: nur "Schnellstart deaktivieren" hat
einen prüfbaren Zustand, dort wäre dieselbe Sperre wie in der Optimierung sinnvoll. Weitere Vorschläge im Chat zu 4.05.

## Letzter Commit
Release-Commit v4.06 mit Tag v4.06 (Nachfolger von ebb4f81, Release v4.05), übergeben als MiniBench_v4.06.bundle.
