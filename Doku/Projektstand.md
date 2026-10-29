# Leos Minibench: Projektstand

Gemeinsamer Stand für alle Agenten (Claude, Antigravity). Wird am Ende jeder Version aktualisiert (siehe AGENTS.md).

## Stand: Version 3.882 (29.10.2026)
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

Inhalt von 3.83 (Einzelheiten in Doku\Ã„nderungen_v3.83.txt):
1. Schnell-Scan per Knopfdruck: Manueller Start der Bibliotheks-, Layout-, Profil- und Ordnererkennung Ã¼ber die SchaltflÃ¤che â€žSchnell-Scan durchfÃ¼hrenâ€œ im Bereich Sicherungsumfang statt automatischem Start beim Betreten der Seite.
2. BÃ¼ndiges Kartenlayout im Lasttest-Stil: VollstÃ¤ndige Umstellung aller drei Umzug-Gruppen (Zielmedium, Sicherungsumfang, Status & Sicherung) auf das bewÃ¤hrte Card(content)-Muster; GruppenÃ¼berschriften (Section) und ErlÃ¤uterungen (Hint) liegen auÃŸerhalb des abgerundeten Kartenpanels (CardPanel), wodurch abgeschnittene Umrandungen, LÃ¼cken oder Kerben vollstÃ¤ndig beseitigt sind.
3. Beseitigung von Zeichensatzfehlern (Mojibake): Alle OberflÃ¤chentexte und Beschriftungen der Umzugsseite in DiagGui_Seiten.cs wurden auf standardisierte Unicode-Escape-Sequenzen (\u00FC, \u00E4, \u00F6, \u00DF, \u2713, \u00B7) umgestellt; Umlaute und Sonderzeichen werden fehlerfrei dargestellt.

## Vorher: Version 3.82 (23.10.2026)
Release-Commit "Release v3.82: Hotfix 2: System-Umzug Prozessablauf, Vorab-Scan, Live-Log & Kartenlayout" (Commit b750f2a).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.82 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.82.txt):
1. Prozessablauf & Performancesprung: Behebung einer 15-minÃƒÂ¼tigen VerzÃƒÂ¶gerung durch Verschiebung von Module\Umzug\Ablauf.ps1 vor Ablauf\Start.ps1 im Bauplan und sofortigen Ausstieg nach der Sicherung; Ausschluss von UAC-ErhÃƒÂ¶hungsversuchen im nicht-privilegierten Kontext (Kern\Adminrechte.ps1); Absicherung externer Aufrufe (winget export, reg.exe) mit Timeouts und Prozess-Terminierung; Teilsicherungen (wie Desktop-Layout) schlieÃƒÅ¸en nun in 2 bis 3 Sekunden ab.
2. Kategorisierter Schnell-Scan: Schneller, speicherschonender Vorab-Scan ermittelt vor der Auswahl des Sicherungsumfangs tatsÃƒÂ¤chlich vorhandene Bibliotheken, Layout-Eigenschaften, den Winget-Paketmanager und installierte Softwareprofile (Webbrowser, Thunderbird, Entwickler-Editoren, Tools, Terminal) und visualisiert diese kategorisiert mit Statusmeldung.
3. Echtzeit-Protokollfenster (Live-Log): Neues durchlaufendes Protokollfenster (txtUmzugLog) im Statusbereich zeigt jeden Schritt, kopierte Dateien und Meldungen des Sicherungsprozesses live mit Zeitstempeln an.
4. BÃƒÂ¼ndiges Kartenlayout: Alle drei Kartenpanel (Zielmedium, Sicherungsumfang, Status) auf einheitliche Gruppenbreite (grpW = UI.S(780)) fixiert; TextumbrÃƒÂ¼che (MaximumSize) aller Labels und Steuerelemente verhindern das ÃƒÅ“berlappen oder Abschneiden von Rahmenlinien.
5. Arbeitsspeicher-Optimierung: VollstÃƒÂ¤ndige Beseitigung synchroner rekursiver DateizÃƒÂ¤hlungen (EnumerateFiles("*", AllDirectories)) beim Ãƒâ€“ffnen der Seite verhindert sprunghaften RAM-Anstieg.

## Vorher: Version 3.81 (23.10.2026)
Release-Commit "Release v3.81: Hotfix: System-Umzug Optimierung, Profile & Konfigurationen, Performance & Filterung" (Commit 18ff90f).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.81 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.81.txt):
1. Robocopy-Multithreading & Performance: Beschleunigung der Dateispiegelung durch parallele AusfÃƒÂ¼hrung mit 16 Threads (/MT:16) und VorankÃƒÂ¼ndigung der Datenvolumina je Bibliothek.
2. Live-Transparenz beim Kopieren: Echtzeit-Anzeige der aktuell kopierten Datei bzw. des Unterverzeichnisses unter dem Fortschrittsbalken; Drosselung der Ereignismeldungen auf max. 150 ms fÃƒÂ¼r reaktive OberflÃƒÂ¤che.
3. Profile & Konfigurationen (Configs): Ausbau des bisherigen Browser-Profil-Bereichs zu einer umfassenden Konfigurationssicherung fÃƒÂ¼r Browser (Edge, Chrome, Brave, Firefox, Opera, Vivaldi), E-Mail (Thunderbird), Entwicklung (VS Code, Cursor, Notepad++, Git, PowerShell), Tools (FileZilla, WinSCP, Total Commander, PuTTY) und Windows Terminal mit granularer AuswÃƒÂ¤hlbarkeit und autarkem Restore.
4. Erweiterte Blacklist & prÃƒÂ¤zise Speicherplatzkalkulation: Ausschluss von Treiber-, Einrichtungs- und Cache-Ordnern (.cache, AMD, Intel, NVIDIA, inetpub, temp, minidump, wsl) auf C:\; Standard-Deaktivierung gefundener Zusatzordner und exakte Aufsummierung der tatsÃƒÂ¤chlichen OrdnergrÃƒÂ¶ÃƒÅ¸en.
5. OberflÃƒÂ¤chen-Korrekturen & Sicherheit: Deaktivierung von UseMnemonic bei CheckBoxes stellt kaufmÃƒÂ¤nnische Und-Zeichen (&) korrekt dar; BestÃƒÂ¤tigung und Absicherung des strikt lesenden Quellenzugriffs.

## Vorher: Version 3.8 (23.10.2026)
Release-Commit "Release v3.8: Modul Ã¢â‚¬Å¾System-Umzug & NeuinstallationÃ¢â‚¬Å“ (Backup & 1-Klick-Restore)" (Commit e38dee6).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.8 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.8.txt):
1. Modul Ã¢â‚¬Å¾System-Umzug & NeuinstallationÃ¢â‚¬Å“: Neues Kernmodul (src\Module\Umzug\) und MenÃƒÂ¼punkt in der Seitenleiste unter Werkzeuge zur transparenten Sicherung vor einer Windows-Neuinstallation auf Wechsellaufwerke (USB-Stick, externe SSDs) oder Netzlaufwerke (NAS).
2. Autarkes 1-Klick-Wiederherstellungspaket: Automatische Erstellung von Wiederherstellen.cmd und Wiederherstellen.ps1 im Stamm des Sicherungsordners. ErmÃƒÂ¶glicht 1-Klick-Restore auf dem neu installierten Windows-PC vÃƒÂ¶llig unabhÃƒÂ¤ngig von einer Minibench-Installation mit dynamischer BenutzerprofilauflÃƒÂ¶sung ($env:USERPROFILE).
3. Sicherungsumfang & Robocopy-Engine: ZuverlÃƒÂ¤ssige Spiegelung von Benutzer-Bibliotheken (Desktop, Dokumente, Downloads, Bilder, Musik, Videos mit Registry User Shell Folders AuflÃƒÂ¶sung), selektiven AppData (Browser-Profile von Edge, Chrome, Brave, Firefox und Thunderbird-Mail) sowie benutzerdefinierten Zusatzordnern auf Laufwerk C:\ (unter Filterung der Windows-Systemblacklist). Robocopy-Parameter /XJ, /COPY:DAT, /R:1 /W:1 und Ausschluss von Temp- und Cache-Verzeichnissen.
4. Desktop-Zustand 1:1: Extraktion des aktiven Hintergrundbilds (TranscodedWallpaper als Wallpaper.jpg) samt WallpaperStyle in Layout.json sowie vollstÃƒÂ¤ndiger Export der Icon-Koordinaten via Shell-Bags (IconPositionen.reg) und Icons.json.
5. Software-Inventarisierung & winget: Automatischer Paketexport via winget export nach Programme\winget-pakete.json sowie Registry-Vergleich zur Generierung des Berichts Nicht-Winget-Programme.txt fÃƒÂ¼r manuelle Softwareinstallationen inklusive Google-Download-Suchlinks.
6. Dateisystem-Schutz & SpeicherplatzprÃƒÂ¼fung: Automatische Erkennung beschreibbarer Wechsellaufwerke, Warnung bei FAT32-Formatierung (4-GB-Dateilimit) und vorzeitige Speicherplatzkalkulation mit visueller Statusanzeige in der WinForms-OberflÃƒÂ¤che.

## Vorher: Version 3.73 (17.10.2026)
Release-Commit "Release v3.73: Kompakte Modulnamen, LMB.exe, mathematische Modellkorrektur, Dashboard-Fallback & Bereinigung externer Quellen" (Commit ed8dee5).
Getestet und gebaut unter Windows mit Windows PowerShell 5.1 und Pester 6.2.0.

Inhalt von 3.73 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.73.txt):
1. Kompakte Modulnamen & LMB.exe: Straffung der Bezeichnungen in Seitenleiste und Fenstertiteln von Ã¢â‚¬Å¾VergleichsdatenbankÃ¢â‚¬Å“ zu Ã¢â‚¬Å¾DatenbankÃ¢â‚¬Å“ und von Ã¢â‚¬Å¾SystemÃƒÂ¤nderungenÃ¢â‚¬Å“ zu Ã¢â‚¬Å¾Ãƒâ€žnderungenÃ¢â‚¬Å“; Umstellung der ausfÃƒÂ¼hrbaren Hauptdatei auf LMB.exe (inklusive LMB.cmd und AbwÃƒÂ¤rtskompatibilitÃƒÂ¤ts-Starter LeosMinibench.cmd).
2. Dashboard-Optimierungen: Chronologische Sortierung der Systemauswahl und Vergleichschips standardmÃƒÂ¤ÃƒÅ¸ig absteigend nach Messdatum (neueste Systeme zuerst); automatisches Fallback auf Benchmark-Sensortelemetrie bei fehlender Lasttest-Zeitreihe (Sensorverlauf (Benchmark-Messung)) verhindert leere Diagramme.
3. Mathematische Modellkorrektur: Gesamtsystem-Effizienzscore auf gewichtetes harmonisches Mittel der gemessenen Benchmark-Effizienzen von Prozessor und Grafikkarte umgestellt (strikte Monotonie, Behebung der Marvin-vs-TORRENT-Anomalie); Nutzungsprofil Workstation durch Anhebung des Mehrkern-Gewichts (CPU_MT auf 6,0 bzw. 60Ã¢â‚¬â€œ65 % Gesamteinfluss) praxisgerecht kalibriert.
4. Bereinigung externer Quellen: SÃƒÂ¤mtliche inoffiziellen Drittanbieter-Mirrors im Optimierungsmodul entfernt; Bezug von Fremdwerkzeugen ausschlieÃƒÅ¸lich von offiziellen Herstellerseiten, GitHub-Releases oder winget mit fester SHA-256-PrÃƒÂ¼fsumme im Manifest (Tools.json).
5. Dokumentation & GitHub-AuÃƒÅ¸enwirkung: Neugestaltung der README.md mit vollstÃƒÂ¤ndiger ModulÃƒÂ¼bersicht, aktualisierten Bildschirmfotos (Startseite_Neu.png, Dashboard_Neu.png) und sichtbarer Danksagung an das Projekt Windows Optimisation Pack von Marvin700; strukturierte CHANGELOG.md (Neu, Verbessert, Behoben).

## Vorher: Version 3.72
Inhalt von 3.72 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.72.txt):
1. Neue dreiteilige Startseite (Drei-Fragen-Prinzip): AufgerÃƒÂ¤umte Einstiegsseite als Standardansicht ganz oben in der Seitenleiste. Zwei dynamische ZustÃƒÂ¤nde: Zustand A (Erststart ohne DatenbanklÃƒÂ¤ufe) mit BegrÃƒÂ¼ÃƒÅ¸ung, Hardware-Steckbrief (Ã¢â‚¬Å¾DIESER PCÃ¢â‚¬Å“) inkl. Inventar-Link, vier interaktiven Aufgabenkarten, Schritt-fÃƒÂ¼r-Schritt-Ablauf und Werkzeug-Verweisen; Zustand B (Folgestart mit vorhandener Datenbank) mit SchlÃƒÂ¼sseldaten in Badges (Ã¢â‚¬Å¾Willkommen zurÃƒÂ¼ckÃ¢â‚¬Å“), 2x2-Kachelmatrix fÃƒÂ¼r den letzten Lauf (KPIs, Bericht ÃƒÂ¶ffnen, Dashboard-Vergleich), asynchroner Kachel Ã¢â‚¬Å¾Schnell behebbarÃ¢â‚¬Å“ fÃƒÂ¼r KonfigurationsprÃƒÂ¼fungen, Aufgabenliste mit Datum des letzten Laufs sowie Werkzeuge-Live-Status. Abschlussleiste mit Option Ã¢â‚¬Å¾Beim Start diese Seite zeigenÃ¢â‚¬Å“, Kurzanleitung und Ã¢â‚¬Å¾Was ist neuÃ¢â‚¬Å“ (dynamisches Neu-Badge bei Versionswechsel).
2. Interaktiver Schnellstart & Ablaufsteuerung: Klick auf eine Aufgabenkachel wÃƒÂ¤hlt die zustÃƒÂ¤ndigen Module in der Seitenleiste an, lÃƒÂ¤dt Standardvorgaben und springt zur ersten Seite; FuÃƒÅ¸leiste zeigt im Leerzustand den deaktivierten Startknopf Ã¢â‚¬Å¾Ablauf wÃƒÂ¤hlenÃ¢â‚¬Å“ und aktiviert sich automatisch nach Modulauswahl.
3. VollstÃƒÂ¤ndige WinSAT-Bereinigung: Entfernung aller veralteten WinSAT-Funktionen (-BenchGpuWahl, chkGpuWahl, WinSAT-Schritt, WinSAT-SVG und Auswertungen im Bericht) zugunsten moderner nativer Messungen.
4. Automatische Standard-Referenz: Automatische Wahl von Referenz_Notebook.json bzw. Referenz_Desktop.json anhand der Hardware-Erkennung (Test-IsNotebookSystem).
5. System-Feinschliff: Sachliche Benennung der KonfigurationsprÃƒÂ¼fung im Diagnosebericht, asynchrone HintergrundprÃƒÂ¼fung von XMP/EXPO, BIOS-Alter und LAN-Link ohne UI-Blockade, durchgehende Dialoggestaltung mit UI.ThemeDialog. Transparente HintergrÃƒÂ¼nde in PillLabel, StartTaskCard und StartTaskRow ÃƒÂ¼ber ControlStyles.SupportsTransparentBackColor abgesichert.

## Vorher: Version 3.71
Inhalt von 3.71 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.71.txt):
1. PrÃƒÂ¤zise StromverbrauchsschÃƒÂ¤tzung (PSU-Effizienzmodell): Umstellung der DC- und AC-Berechnung auf ein physikalisches Hardware- und Netzteil-Modell mit 230V EU-Effizienzkurven (Unbekannt, 80 PLUS White bis Titanium) und linearer StÃƒÂ¼tzstellen-Interpolation (5 % bis 100 % Netzteilauslastung); interaktive Konfigurationsleiste direkt im HTML-Bericht (Nennleistung in Watt und Effizienzklasse) fÃƒÂ¼r Desktops mit Live-Neuberechnung aller Verbrauchswerte, Auslastungsgrade und Stromkosten sowie Speicherung im Browser (localStorage); automatische Erkennung mobiler Plattformen mit OEM-Netzteil-/Akkumodell (~90 % Wirkungsgrad) ohne ATX-Netzteilwahl; detaillierte DC-Erfassung inkl. Mainboard-VRM-Verlusten, AMD-Radeon-TBP-ZuschlÃƒÂ¤gen (+4 W / +15 %), DIMM-Arbeitsspeichermodulen, DatentrÃƒÂ¤gertypen (NVMe/SATA/HDD) und LÃƒÂ¼ftern sowie Warnung bei Netzteil-ÃƒÅ“berlastung (> 85 % unter Volllast).
2. System-Startzeit & Nutzungsprofil-Transparenz: StandardmÃƒÂ¤ÃƒÅ¸ig geÃƒÂ¶ffnetes Boot-Performance-Element (<details class="box grp" data-section="system" open>) im HTML-Bericht; saubere Trennung und verstÃƒÂ¤ndliche ErlÃƒÂ¤uterung der beiden Boot-Phasen aus Event 100 (MainPathBootTime: Desktop/Treiber einsatzbereit vs. BootPostBootTime: Autostart bis vollstÃƒÂ¤ndiger Leerlauf); Nutzungsprofile (Gaming, BÃƒÂ¼ro, Workstation) mit transparentem Hover-Tooltip zur genauen mathematischen Zusammensetzung und Gewichtung der Komponenten; Referenzsystem-Auswahl (Gespeicherte Referenz, Mediane, DatenbankeintrÃƒÂ¤ge) und Referenz-Speichern im Benchmark-Reiter der BenutzeroberflÃƒÂ¤che wiederhergestellt.
3. Berichts-Feinschliff & Layout-Optimierung: Nutzungsprofile mit farbigem Akzent-Hintergrund (var(--accent-soft)) und neutrale Komponentenkarten (CPU, GPU, RAM, Laufwerke) prominent ÃƒÂ¼ber den Benchmark-Detailtabellen wiederhergestellt; Facts-Tabelle von redundanten LeistungsschÃƒÂ¤tzungen bereinigt; Block fÃƒÂ¼r Gesamtsystemverbrauch und Stromkostenrechner direkt ÃƒÂ¼ber den Benchmark-Sensoren platziert.
4. Dashboard- & Dark-Mode-Korrekturen: Horizontale und vertikale Trennlinien in Spalte 1 der Matrix-Tabelle im Dark Mode durch kontrastreiche Linienfarben (#444E5E / #363E4D) und z-index-Priorisierung repariert; Emojis (Ã°Å¸Å½Â®, Ã°Å¸â€™Â¼, Ã°Å¸Å¡â‚¬, Ã°Å¸â€œÅ ) beibehalten; Dashboard-Start bei Erstbenutzung ohne Parameterfehler stabilisiert.

## Vorher: Version 3.7
Inhalt von 3.7 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.7.txt):
1. Schnell behebbare Konfigurationsfehler: Automatische Erkennung von LAN-Verhandlungsrate gebremst (100 Mbit/s oder Halbduplex an Gigabit/2.5GbE-Adaptern), Monitorkabel an iGPU statt dGPU bei Desktops, Hz-Bremse bei Monitoren >= 75 Hz auf 60 Hz limitiert, inaktivem RAM-Speicherprofil (XMP/EXPO/DOCP) und veraltetem BIOS (> 2 Jahre) mit direkten Hersteller-Supportlinks sowie transparenter BestÃƒÂ¤tigung der PrÃƒÂ¼fung im Bericht.
2. System-Startzeit-Analyse (Boot Performance Breakdown): AufschlÃƒÂ¼sselung der Windows-Boot-Phasen aus Event 100 (MainPathBootTime, BootPostBootTime, BootDuration) und UEFI/BIOS-POST-Zeit (FwPOSTTime) mit visueller Wasserfall-/Stack-Kaskade im HTML-Bericht (eingeklappt am Berichtsende); Auswertung von Autostart-VerzÃƒÂ¶gerern (Event 101-110 ab 1,0 s) und robuster Fallback ohne SchÃƒÂ¤tzwerte bei unvollstÃƒÂ¤ndigen Ereignissen oder deaktiviertem Protokoll.
3. Harmonisierte Sensoren-Zustandsmaschine: Konsistente 4-Stufen-Statuslogik (VollstÃƒÂ¤ndig, EingeschrÃƒÂ¤nkt, Inaktiv, Fehler) im Skript und in der BenutzeroberflÃƒÂ¤che; automatische Treiberinitialisierung bei Vorliegen von Administratorrechten ohne zusÃƒÂ¤tzliche RÃƒÂ¼ckfragen und garantierte Treiberentladung bei Programmbeendigung.
4. Benchmark-Vereinfachung & Dashboard: Entfernen redundanter Dropdowns und Checkboxen (Messdauer, Referenz, DB-Speicherung); Benchmark speichert immer in der Vergleichsdatenbank. Beseitigung der Scrollbars in der Vergleichsmatrix mit Wiederholung der Systemnamen am Tabellenende (tfoot), gut sichtbaren Zeilentrennlinien in der Basiskolumne und metrischen Einheiten (Ã‚Â°C, %). Stromkostenrechner standardmÃƒÂ¤ÃƒÅ¸ig geÃƒÂ¶ffnet, Details und Zeitbedarf standardmÃƒÂ¤ÃƒÅ¸ig eingeklappt. Redundante Referenz-Spalten und Profilkarten aus dem Einzelbericht entfernt (vollstÃƒÂ¤ndig durch das interaktive Dashboard abgedeckt).
5. Modulares Dashboard: VollstÃƒÂ¤ndige Zerlegung des Dashboard-Quelltexts in separate semantische Bausteine (dashboard.html, dashboard.css, daten.js, matrix.js, profile.js, diagramme.js) mit robuster Einbettung im Zusammenbau-Skript und SyntaxprÃƒÂ¼fung via node --check.
Inhalt von 3.67 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.67.txt):
1. Multi-System-Sensorverlauf: Achsenbeschriftung Temperatur (Ã‚Â°C) durch 75 px Randabstand und linksbÃƒÂ¼ndige Ausrichtung vollstÃƒÂ¤ndig sichtbar; Tooltip mit kompakten Systemnamen (getShortSysName), zweizeiligem Layout und stets vorangestelltem Basissystem (auch ohne Zeitreihe).
2. Differenzierte Nutzungsprofile: Gewichtung fÃƒÂ¼r Gaming (70 % GPU-FPS, 20 % ST, 10 % RAM/SSD; Kerne > 8 neutral), BÃƒÂ¼ro/Desktop (45 % SSD-Reaktionszeit, 35 % ST, 20 % RAM-Latenz; 0 % GPU/MT) und Workstation (45 % MT, 20 % Instruktionen, 20 % RAM-Bandbreite, 15 % Dauertransfer); redundante Stromverbrauchs-Badges im Profilgitter entfernt.
3. Hervorhebung des Basissystems: Markante Akzent-Umrandung mit Anker-Symbol im Auswahlbereich, durchgehende Hintergrundhervorhebung mit 100-%-Basis-Badge in Hardware- und Matrix-Tabelle sowie Akzent-Rahmen in den Profilkarten.
4. Hardware-Tabelle aufgerÃƒÂ¤umt: Redundante Berichtszeile entfernt; kompakte DatentrÃƒÂ¤ger-Zusammenfassung nach Typ und GesamtkapazitÃƒÂ¤t mit vollstÃƒÂ¤ndiger Modellliste im Hinweis beim ÃƒÅ“berfahren.
5. Kalibrierte Moduldauern: Grundzeit der Diagnose auf 0 Min. gesenkt, ZuschlÃƒÂ¤ge fÃƒÂ¼r SystemprÃƒÂ¼fungen und WinSAT auf reale SSD-Dauern kalibriert.
6. Dashboard-Feinschliff: Versionspille dynamisch an Skriptversion gekoppelt, kompakte Matrix-Tabellenzeilen und fixierte TabellenkÃƒÂ¶pfe (sticky) beim Scrollen.

## Vorher: Version 3.66
Inhalt von 3.66 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.66.txt):
1. Dark Mode beim Start: SetPreferredAppMode vor Control-Erzeugung, OnShown und ShowPage wenden DarkMode_Explorer rekursiv auf alle Win32-Scrollbars an.
2. Sensor-Status-Badge in der Kopfzeile: Dezentes Badge fÃƒÂ¼r aktiven Modus (LHM + PawnIO aktiv oder Windows-Fallback), gekoppelt an Treiberwahl.
3. Dashboard-Telemetrie fÃƒÂ¼r Neu-LÃƒÂ¤ufe: Export-BenchDashboardHtml nach Anhang.zip-Erzeugung; extractCsvSeries liest auch .minibench und Fallback-Dateien.
4. Canvas-Diagramm: Zwei getrennte Y-Achsen (links: Ã‚Â°C in Blau/Orange, rechts: Watt in GrÃƒÂ¼n/Gelb); zentrierte Infokarte mit Peak-Werten bei Systemen ohne Zeitreihe.
5. GPU-Effizienz: Strikt in FPS/Watt aus Render-FPS (GPU|REND) geteilt durch Maximalleistung (GpuWMax); Beschriftung im Bericht auf FPS/W korrigiert.
6. HTML-Bericht: Sticky-Navigation mit dezentem Schatten und flÃƒÂ¼ssiger Nach-oben-Knopf unten rechts ab 300 px Scrolltiefe.
7. Bildschirmfoto: Doku\Screenshots\Oberflaeche neu.png als Standard-Vorschaubild Oberflaeche.png ÃƒÂ¼bernommen.

## Vorher: Version 3.65
Inhalt von 3.65 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.65.txt):
1. Einheitliche Karten (CardPanel) fÃƒÂ¼r Optionen der Diagnose, Tabellen des Lasttests, Paketauswahl der Tools und Listen der Laufansicht.
2. Wartung im Aufbau der Optimierung (CategoryBlock) mit Gruppenbeschreibung, "alle" und "keine", Etiketten rechts und "Alle aufklappen".
3. Angeglichene Laufansicht mit Kartenrahmen, Etiketten in normaler Schreibweise und themenangepasstem Protokollfeld.
4. Korrekturen nach Windows-Test: Zeitangabe mit ParallelausfÃƒÂ¼hrung, Diagnose (parallel) in FuÃƒÅ¸leiste, OptimierungsknÃƒÂ¶pfe zweizeilig,
   Netzlaufwerk-Karte ohne Textabschnitt, grÃƒÂ¶ÃƒÅ¸eres Schildsymbol.
5. Diagnosebericht: KPI-Kopfzeile (.cards) auf minmax(120px, 1fr) mit kompakten AbstÃƒÂ¤nden und SchriftgrÃƒÂ¶ÃƒÅ¸e angepasst, sodass alle
   sieben Kennzahlen (inklusive ZuverlÃƒÂ¤ssigkeit) einzeilig nebeneinander stehen und nicht mehr in eine zweite Zeile umbrechen.

## Vorher: Version 3.64
Inhalt von 3.64 (Einzelheiten in Doku\Ãƒâ€žnderungen_v3.64.txt):
1. Gestaltungsregeln in der Klasse UI (AbstÃƒÂ¤nde, Seitenrand 24 px, Gruppenbreite 780 px, Schriftstufen), neue Steuerelemente
   Badge, GroupHeader und PrimaryButton, Zweizeilen-Optionen (Opt2), UI.OnAccent fÃƒÂ¼r Kontrast auf AkzentflÃƒÂ¤chen.
2. Seitenleiste in Ablauf und Werkzeuge, Schalter "beim Start ausfÃƒÂ¼hren" im Seitenkopf, flache Kopfzeile mit SymbolknÃƒÂ¶pfen,
   Seite SystemÃƒÂ¤nderungen, FuÃƒÅ¸leiste mit Ablauf und Gesamtdauer, Startknopf mit Beschriftung nach Auswahl.
3. Zusammenfassung vor dem Start (StartUebersicht) nur bei Eingriffen, Neustarts, abgewÃƒÂ¤hltem Wiederherstellungspunkt oder
   verwalteten EintrÃƒÂ¤gen auf DomÃƒÂ¤nen-PCs; ersetzt die RÃƒÂ¼ckfragen von Wartung und Optimierung.
4. Diagnose ohne Funktionstest und Nur Absturzanalyse (Skriptparameter bleiben), Lasttest als Tabelle, Wartung und
   Optimierung mit Etiketten, Tools mit einer Bildlaufleiste und Werkzeugkarten mit Zustand.

## Offene Befunde
Keine offenen Befunde.

## Letzter Commit
Release-Commit v3.88: Einklappbare Datenbank-Gruppen, Dark-Mode-Kontrast, Startseiten-Layout & Zeichensatz-Korrektur (Commit 16a08c2)
