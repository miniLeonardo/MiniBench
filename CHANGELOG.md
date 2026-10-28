# Changelog - Leos Minibench

Alle wesentlichen Ã„nderungen an Leos Minibench werden in diesem Dokument festgehalten.

## v3.84 (28.10.2026)

### Neu
* **Granulare Bibliotheks-Auswahl beim System-Umzug:** Individuelle Checkboxen für alle Standardbibliotheken (Desktop, Dokumente, Downloads, Bilder, Musik, Videos) mit Schnellauswahl (Alle / Keine). Der Downloads-Ordner ist standardmäßig abgewählt, um unnötige Datenberge bei Neuinstallationen zu vermeiden.
* **Echte Größenmessung im Schnell-Scan:** Vollständiger Verzicht auf statische 2-GB-Pauschalen. Beim Klick auf Schnell-Scan durchführen werden die exakten Ordnergrößen aller Bibliotheken und erkannten C:\*-Zusatzordner gemessen und dynamisch in den Checkbox-Beschriftungen sowie der Speicherplatzkalkulation angezeigt.

### Verbessert
* **GitHub-Härtung & Account-Schutz:** Striktes Verbot und vollständige .gitignore-Absicherung gegen das Einchecken von Binärarchiven (*.zip, *.7z, *.tar, *.rar), ausführbaren Dateien (*.exe, *.dll, *.sys) oder Treibern zur dauerhaften Vermeidung automatisierter GitHub-Sperren.
* **Text- & Oberflächen-Feinschliff:** Fußleisten-Option präzise und kompakt in Personenbezogene Daten entfernen umbenannt; dynamisches Layout der Fußleiste angepasst.
* **Versionsseite mit Stichpunkten:** Alle Versionseinträge auf der Seite Info und Versionen von Fließtext auf übersichtliche, lesbare Aufzählungspunkte (•) umgestellt und Zwischenabstände für schnelle Erfassbarkeit optimiert.

### Behoben
* **Lautloser Sensoren-Lifecycle:** Beseitigung störender Deinstallationsfenster von PawnIO während Prüfläufen oder beim Modulwechsel. Der Treiber bleibt während aller gewählten Schritte im Speicher und wird erst am Gesamtlaufende mit umfassenden Silent-Parametern (/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /S -uninstall -silent) freigegeben; Deinstallationsstatus wird sachlich protokolliert statt blockierende Fehlerfenster zu öffnen.
* **Beseitigung von Encoding-Fehlern (Mojibake):** Historische Zeichensatzfehler in Versionen.cs restlos bereinigt und Datei als sauberes UTF-8 mit BOM gespeichert.

## v3.83 (23.10.2026)

### Neu
* **Manueller Schnell-Scan per Knopfdruck:** Der Vorab-Scan zur Erkennung vorhandener Bibliotheken, Profile und Ordner wird nicht mehr automatisch beim Betreten der Seite ausgeführt, sondern startet gezielt über die Schaltfläche `Schnell-Scan durchführen` im Sicherungsumfang.

### Verbessert
* **Kartenlayout im Lasttest-Stil:** Vereinheitlichung aller drei Umzug-Gruppen auf das `Card(content)`-Muster mit abgerundetem `CardPanel`, konsistenten Außen- und Innenabständen sowie bündigen Rahmenlinien ohne Abschneiden oder Versatz auf allen DPI-Skalierungen.

### Behoben
* **Beseitigung von Zeichensatzfehlern (Mojibake):** Durch Umstellung aller Texte und Beschriftungen der Umzugsseite auf Unicode-Escape-Sequenzen in C# wurden fehlerhafte Umlaute und Sonderzeichen restlos beseitigt.

## v3.82 (23.10.2026)

### Neu
* **Kategorisierter Schnell-Scan vor der Sicherung:** Schneller, speicherschonender Vorab-Scan zur automatischen Erkennung vorhandener Standardbibliotheken, Desktop-Layout-Eigenschaften, des Winget-Paketmanagers und installierter Anwendungsprofile (Webbrowser, Thunderbird, Entwicklungsumgebungen, Hilfswerkzeuge, Windows Terminal) mit sofortiger Anzeige von Erkennungsstatus und Anzahl.
* **Echtzeit-Protokollfenster (Live-Log):** Durchlaufendes Protokollfenster (`txtUmzugLog`) im Statusbereich der BenutzeroberflÃ¤che zeigt jeden Schritt, kopierte Dateien, Verzeichnisse und RÃ¼ckmeldungen des Sicherungsprozesses in Echtzeit mit Zeitstempeln an.

### Verbessert
* **Kartenlayout & Rahmen-BÃ¼ndigkeit:** Alle Kartenpanel der Umzug-Seite auf einheitliche Gruppenbreite (`grpW`) und bÃ¼ndige InnenabstÃ¤nde skaliert; TextumbrÃ¼che fÃ¼r alle Labels und Beschreibungen verhindern das Ãœberlappen oder Abschneiden von Rahmenlinien.
* **Prozessablauf & Skriptarchitektur:** Einordnung von `Module\Umzug\Ablauf.ps1` vor `Ablauf\Start.ps1` im Bauplan garantiert, dass der Umzug-Hilfsmodus vor der Diagnose-Initialisierung startet und nach der Sicherung unmittelbar beendet wird.

### Behoben
* **Behebung von 15-minÃ¼tiger VerzÃ¶gerung & HÃ¤nger:** Beseitigung eines Programmablauf-Fehlers, durch den im Umzug-Hilfsmodus unbeabsichtigt die vollstÃ¤ndige Systemdiagnose ausgefÃ¼hrt wurde; der Sicherungslauf (z. B. Desktop-Layout) schlieÃŸt nun in wenigen Sekunden ab.
* **UAC-Berechtigungsabfrage:** Hilfsmodus `-Umzug` in `Adminrechte.ps1` aufgenommen, um unsichtbare UAC-ErhÃ¶hungsversuche im nicht-privilegierten Kindprozess zu verhindern.
* **Arbeitsspeicher-Spitzen beseitigt:** Entfernung synchroner rekursiver DateizÃ¤hlungen (`EnumerateFiles("*", AllDirectories)`) beim Ã–ffnen der Seite; Reduzierung des RAM-Verbrauchs auf ein Minimum.
* **Zeitbegrenzung fÃ¼r externe Aufrufe:** Externe Werkzeug-Aufrufe (`winget export`, `reg.exe export`) mit festen Timeouts (60s bzw. 15s) und Prozess-Terminierung gegen HÃ¤nger abgesichert.

## v3.81 (23.10.2026)

### Neu
* **Profile & Konfigurationen (Configs):** Erweiterung der selektiven Konfigurationssicherung um Webbrowser (Edge, Chrome, Brave, Firefox, Opera, Opera GX, Vivaldi), E-Mail (Thunderbird), Entwickler-Tools und Editoren (VS Code, Cursor, Notepad++, Git-Config, PowerShell-Profile), Hilfswerkzeuge (FileZilla, WinSCP, PuTTY-Sessions) sowie Windows Terminal inklusive 1-Klick-Restore im Wiederherstellungsskript.
* **Granulare Auswahl & Transparenz:** Granulare Unterauswahl fÃ¼r Profile und Konfigurationen; Live-Streaming der aktuell kopierten Dateien und Unterverzeichnisse wÃ¤hrend des Sicherungslaufs im Statusbereich der BenutzeroberflÃ¤che.

### Verbessert
* **Sicherungs-Performance (Multithreading):** Beschleunigung der Robocopy-KopiervorgÃ¤nge durch parallele AusfÃ¼hrung mit 16 Threads (/MT:16).
* **Erweiterte Blacklist fÃ¼r Stammordner auf C:\:** ZuverlÃ¤ssiger Ausschluss von Treiber-, Cache- und Einrichtungsordnern (unter anderem .cache, AMD, Intel, NVIDIA, inetpub, temp, minidump, wsl) aus der Auswahlliste.
* **PrÃ¤zise Speicherplatzkalkulation:** Exakte Einberechnung der tatsÃ¤chlichen OrdnergrÃ¶ÃŸen ausgewÃ¤hlter Zusatzordner in die Gesamtvolumen-SchÃ¤tzung.
* **Standard-Deaktivierung von Zusatzordnern:** ZusÃ¤tzliche Stammordner auf C:\ sind standardmÃ¤ÃŸig abgewÃ¤hlt, um unbeabsichtigt riesige Sicherungen (z. B. Spieleordner) zu verhindern.

### Behoben
* **Ampersand-Darstellung in Optionen:** Behebung von durch WinForms-Mnemonics verschluckten Und-Zeichen (&) in Beschriftungen von Optionsfeldern (c.UseMnemonic = false).
* **Strikte UnverÃ¤nderlichkeit der Quelle:** ÃœberprÃ¼fung und Absicherung, dass sÃ¤mtliche Sicherungsroutinen ausschlieÃŸlich lesend auf die Quellpfade zugreifen.

## v3.8 (23.10.2026)

### Neu
* **Modul â€žSystem-Umzug & Neuinstallationâ€œ:** Neues Werkzeugmodul (src\Module\Umzug\) zur strukturierten Sicherung vor einer Windows-Neuinstallation auf USB-Wechsellaufwerke oder konfigurierte NAS-Netzlaufwerke.
* **Autarkes 1-Klick-Wiederherstellungspaket:** Erzeugt im Sicherungsordner Wiederherstellen.cmd und Wiederherstellen.ps1. ErmÃ¶glicht auf dem neu installierten PC die vollstÃ¤ndige Wiederherstellung von Benutzerdaten, Desktop-Optik und Programmen ohne Minibench-Installation und mit dynamischer BenutzerprofilauflÃ¶sung ($env:USERPROFILE).
* **Desktop-Zustand 1:1 sichern & wiederherstellen:** Sichert das aktive Hintergrundbild (Wallpaper.jpg) samt WallpaperStyle in Layout.json und exportiert Desktop-Icon-Koordinaten aus den Shell-Bags (IconPositionen.reg, Icons.json).
* **Software-Inventarisierung & Winget-Export:** Automatischer Export installierter Pakete via winget export (winget-pakete.json) sowie Registry-Vergleich mit Ausgabe von Nicht-Winget-Programme.txt fÃ¼r manuell zu installierende Anwendungen inklusive Download-Suchlinks.

### Verbessert
* **Dateisystem-Schutz & KapazitÃ¤tsprÃ¼fung:** Automatische Erkennung aller beschreibbaren Wechsellaufwerke, Warnung bei FAT32-Formatierung vor dem 4-GB-Dateilimit und interaktive SpeicherplatzprÃ¼fung (BenÃ¶tigt vs. Frei) in der BenutzeroberflÃ¤che.
* **Robocopy-Absicherung:** Verwendung von /XJ (Schutz vor Junction-Schleifen), /R:1 /W:1 /COPY:DAT (Zugriffsschutz ohne SID-Ãœbernahme) sowie standardmÃ¤ÃŸiger Ausschluss von temporÃ¤ren Verzeichnissen und Browser-Caches (Temp, Cache, Code Cache, GPUCache, Service Worker, CrashDumps, 
ode_modules).
* **Zusatzordner auf Root-Ebene:** Automatischer Scan von C:\ nach benutzerdefinierten Verzeichnissen unter Ausschluss der Windows-Systemblacklist zur selektiven Sicherung nach C_Laufwerk_Zusatz\.

### Behoben
* **Vermeidung von Datenverlusten bei Neuinstallation:** VollstÃ¤ndige Transparenz Ã¼ber alle gesicherten Bibliotheken, Desktop-ZustÃ¤nde und Software-BestÃ¤nde.

## v3.73 (17.10.2026)

### Neu
* **Kompakte Benennung in der BenutzeroberflÃ¤che:** MenÃ¼punkt und Kopfzeilen von â€žVergleichsdatenbankâ€œ zu prÃ¤gnantem â€žDatenbankâ€œ und von â€žSystemÃ¤nderungenâ€œ zu â€žÃ„nderungenâ€œ gestrafft.
* **Neue ausfÃ¼hrbare Hauptdatei `LMB.exe`:** Kompaktierung des BinÃ¤rdateinamens von `LeosMinibench.exe` auf `LMB.exe`. Zur nahtlosen AbwÃ¤rtskompatibilitÃ¤t stehen weiterhin `LMB.cmd` und `LeosMinibench.cmd` als Starter bereit.
* **Sensor-Fallback im Dashboard:** Liegt fÃ¼r ein System keine Lasttest-Verlaufsreihe vor, bindet das interaktive Dashboard automatisch die Sensortelemetrie des Benchmarks (`Sensorverlauf (Benchmark-Messung)`) ein, sodass Diagramme nicht mehr leer bleiben.

### Verbessert
* **Chronologische Systemauswahl im Dashboard:** Die System-Dropdowns und Vergleichschips sortieren gespeicherte Systeme standardmÃ¤ÃŸig absteigend nach Datum (neueste Messung ganz oben) mit UnterstÃ¼tzung fÃ¼r deutsche und ISO-Datumsformate.
* **Mathematische Modellierung der Gesamtsystem-Effizienz:** Berechnung des Gesamteffizienzindex Ã¼ber das gewichtete harmonische Mittel der gemessenen Benchmark-Effizienzen von Prozessor und Grafikkarte unter BerÃ¼cksichtigung der 230V-Wirkleistung. Systeme mit hÃ¶herer CPU- und GPU-Einzeleffizienz erzielen mathematisch garantiert stets einen hÃ¶heren Gesamtindex (Behebung der Marvin-vs-TORRENT-Anomalie).
* **Workstation-Nutzungsprofil kalibriert:** Anhebung der Gewichtung fÃ¼r Mehrkern-Rechenleistung (`CPU_MT`) von 3,0 auf 6,0 (ca. 60â€“65 % Gesamteinfluss), wodurch 8- und 12-Kern-Systeme bei rechenintensiven Aufgaben den realen Leistungsvorsprung widerspiegeln.
* **Bereinigung externer Quellen:** VollstÃ¤ndige Entfernung privater Repository-Mirrors im Optimierungsmodul; Bezug von Fremdwerkzeugen (wie DDU) ausschlieÃŸlich Ã¼ber offizielle Herstellerseiten, GitHub-Releases oder `winget` mit fester SHA-256-PrÃ¼fsumme im Manifest.
* **GitHub-AuÃŸenwirkung:** VollstÃ¤ndige Ãœberarbeitung der `README.md` mit detaillierter Feature-Ãœbersicht, aktualisierten Bildschirmfotos sowie respektvoller Danksagung an das Projekt Windows Optimisation Pack von Marvin700.

### Behoben
* **Leere Graphen bei reinen Benchmark-Systemen:** Dashboard zeigt nun Telemetriekurven auch dann an, wenn ausschlieÃŸlich ein Benchmarklauf durchgefÃ¼hrt wurde.
* **Verzerrte Gesamteffizienz bei High-End-Grafikkarten:** Bisherige Spitzenlast-Nennerverzerrung durch harmonische Mittelwertbildung bereinigt.
* **UnterschÃ¤tzte Mehrkernleistung bei Workstation-Workloads:** Workstation-Index spiegelt nun das reale Multithreading-Potenzial wider.

## v3.72 (16.10.2026)

* **Neue dreiteilige Startseite (Drei-Fragen-Prinzip):** EinfÃ¼hrung einer modernen, intuitiven Einstiegsseite als Standardansicht beim Start der OberflÃ¤che. Zwei dynamische Betriebsmodi:
  * **Zustand A (Erststart):** Hardware-Steckbrief (â€žDieser PCâ€œ) mit kompakter Ãœbersicht (CPU, GPU, RAM, Laufwerke, Mainboard) und Direktlink zum Detail-Inventar; vier interaktive Aufgabenkarten (â€žPC durchcheckenâ€œ, â€žLeistung messenâ€œ, â€žStabilitÃ¤t prÃ¼fenâ€œ, â€žPflegen und beschleunigenâ€œ) mit Hervorhebung und Tooltips; zweispaltiger ErklÃ¤rbereich (â€žSo lÃ¤uft ein Durchgangâ€œ mit 3 Schritten und â€žWerkzeugeâ€œ-Direktzugriff).
  * **Zustand B (Folgestarts bei vorhandener Datenbank):** SchnellÃ¼bersicht der SchlÃ¼sseldaten als Badges/Pills (â€žWillkommen zurÃ¼ckâ€œ); 2x2-Kachelmatrix mit Kachel â€žLetzter Laufâ€œ (Gesamtscore, CPU Mehrkern, RAM Lesen, GPU FPS, Befund-Status, Bericht Ã¶ffnen und Dashboard-Vergleich), Kachel â€žSchnell behebbarâ€œ (asynchron im Hintergrund geladene KonfigurationsprÃ¼fungen), kompakter Aufgabenliste mit Datum des letzten Laufs sowie Live-Status der Werkzeuge.
  * **Abschlussleiste:** Einstellung â€žBeim Start diese Seite zeigenâ€œ (persistent in `Einstellungen.json`), Direktlink zur Kurzanleitung sowie â€žWas ist neuâ€œ mit dynamischer Badge-Benachrichtigung fÃ¼r neue Versionen (`GeseheneVersion`).
* **Interaktiver Schnellstart & Ablaufsteuerung:** Direkter Klick auf eine Aufgabenkarte wÃ¤hlt die zugehÃ¶rigen Module in der Seitenleiste an (z. B. Diagnose fÃ¼r PC durchchecken, Wartung und Optimierung fÃ¼r Pflegen), setzt empfohlene Vorgaben und navigiert sofort zur ersten Arbeitsseite; FuÃŸleiste zeigt im Leerzustand den deaktivierten Startknopf â€žAblauf wÃ¤hlenâ€œ und wechselt bei Modulwahl nahtlos auf den aktiven Startknopf (â€žDiagnose startenâ€œ).
* **VollstÃ¤ndige WinSAT-Bereinigung:** VollstÃ¤ndige Entfernung alter WinSAT-Funktionen (`-BenchGpuWahl`, `chkGpuWahl`, WinSAT-Benchmarkschritt, WinSAT-SVG und Auswertungen im Bericht) zugunsten moderner nativer Messungen.
* **Automatische Standard-Referenz:** Automatische Wahl von `Referenz_Notebook.json` (Notebooks) bzw. `Referenz_Desktop.json` (Desktop-PCs) bei Standard-BenchmarklÃ¤ufen anhand der Hardware-Erkennung (`Test-IsNotebookSystem`).
* **System-Feinschliff & KonfigurationsprÃ¼fung:** Sachliche Benennung des Diagnose-PrÃ¼fschritts als â€žKonfigurationsprÃ¼fungâ€œ im HTML-Bericht; asynchrone PrÃ¼fung von XMP/EXPO, BIOS-Alter und Netzwerkverbindung ohne Blockierung der OberflÃ¤che; konsistente Anwendung von `UI.ThemeDialog` auf allen neuen Dialogen.

## v3.71 (15.10.2026)

* **PrÃ¤zise StromverbrauchsschÃ¤tzung (PSU-Effizienzmodell):** Umstellung der internen DC- und externen AC-Berechnung auf ein physikalisches Hardware- und Netzteil-Modell mit 230V EU-Effizienzkurven (`Unbekannt`, `80 PLUS White`, `Bronze`, `Silver`, `Gold`, `Platinum`, `Titanium`) und linearer StÃ¼tzstellen-Interpolation (5 % bis 100 % Netzteilauslastung). Interaktive Netzteil-Konfigurationsleiste direkt im HTML-Bericht (Nennleistung in Watt und Effizienzklasse) mit Live-Neuberechnung aller Verbrauchswerte, Auslastungsgrade und Stromkosten sowie browserseitiger Persistenz (`localStorage`). Detaillierte DC-Erfassung inklusive Mainboard-VRM-Wandlerverlusten, AMD-Radeon-TBP-ZuschlÃ¤gen, DIMM-Arbeitsspeichermodulen, DatentrÃ¤gertypen (NVMe/SATA/HDD) und LÃ¼ftern sowie automatischer Ãœberlastwarnung (> 85 % Auslastung unter Volllast).
* **System-Startzeit & Nutzungsprofil-Transparenz:** Das Boot-Performance-Element `<details class="box grp" data-section="system">` ist im HTML-Bericht standardmÃ¤ÃŸig geÃ¶ffnet. Eindeutige Trennung und verstÃ¤ndliche ErlÃ¤uterung der beiden Boot-Phasen aus Event 100: `MainPathBootTime` (Dauer bis Desktop/Login-Screen) und `BootPostBootTime` (Autostart-Initialisierung bis Leerlauf). Nutzungsprofile (`Gaming`, `BÃ¼ro`, `Workstation`) im Benchmark mit transparentem Hover-Tooltip zur genauen mathematischen Zusammensetzung und Gewichtung der Komponenten.
* **Berichts-Feinschliff & Layout-Optimierung:** Nutzungsprofile mit dezentem Akzent-Hintergrund und neutrale Komponentenkarten (CPU, GPU, RAM, Laufwerke) prominent Ã¼ber den Benchmark-Detailtabellen wiederhergestellt; kompakte Facts-Tabelle im Systemkopf von redundanten StromverbrauchsschÃ¤tzungen bereinigt; der Block fÃ¼r Gesamtsystemverbrauch und interaktiven Stromkostenkalkulator steht nun direkt Ã¼ber dem Abschnitt â€žSensoren wÃ¤hrend des Benchmarksâ€œ.
* **Dashboard & Dark Mode Korrekturen:** Horizontale und vertikale Trennlinien in Spalte 1 der Matrix-Tabelle im Dark Mode durch kontrastreiche Rahmenfarben (`#444E5E` / `#363E4D`) und `z-index`-Priorisierung repariert; Emojis (`ðŸŽ®`, `ðŸ’¼`, `ðŸš€`, `ðŸ“Š`) beibehalten.

## v3.7 (15.10.2026)

* **Schnell behebbare Konfigurationsfehler:** Automatische Erkennung typischer Einbremser: LAN-Verhandlungsrate gebremst auf 100 Mbit/s oder Halbduplex an Gigabit/2.5GbE-Adaptern, Monitor am Mainboard (iGPU) angeschlossen trotz aktiver dedizierter Grafikkarte, Hz-Bremse bei Monitoren >= 75 Hz auf 60 Hz limitiert, inaktives Speicherprofil (XMP/EXPO/DOCP) unter potentiellem Nenntakt und veraltetem BIOS (> 24 Monate) mit direkten Hersteller-Supportlinks sowie transparenter BestÃ¤tigung der PrÃ¼fung im Bericht.
* **System-Startzeit-Analyse (Boot Performance Breakdown):** AufschlÃ¼sselung der Windows-Boot-Phasen aus Event 100 (`MainPathBootTime`, `BootPostBootTime`, `BootDuration`) und der UEFI/BIOS-POST-Zeit (`FwPOSTTime`) mit visueller Wasserfall-/Stack-Kaskade im HTML-Bericht (eingeklappt am Berichtsende); Auswertung von Autostart-VerzÃ¶gerern (Event 101-110 ab 1,0 s) und robuster Fallback ohne SchÃ¤tzwerte bei inaktivem Protokoll.
* **Harmonisierte Sensoren-Zustandsmaschine:** Konsistente 4-Stufen-Statuslogik (`VollstÃ¤ndig`, `EingeschrÃ¤nkt`, `Inaktiv`, `Fehler`) im Skript und in der BenutzeroberflÃ¤che; automatische Treiberinitialisierung bei Vorliegen von Administratorrechten ohne zusÃ¤tzliche RÃ¼ckfragen und garantierte Treiberentladung bei Programmbeendigung.
* **Benchmark-Vereinfachung & Dashboard:** Entfernen redundanter Dropdowns und Checkboxen (Messdauer, Referenz, DB-Speicherung); Benchmark speichert immer in der Vergleichsdatenbank. Beseitigung der Scrollbars in der Vergleichsmatrix mit Wiederholung der Systemnamen am Tabellenende, gut sichtbaren Zeilentrennlinien in der Basiskolumne und metrischen Einheiten (Â°C, %). Stromkostenrechner standardmÃ¤ÃŸig geÃ¶ffnet, Details und Zeitbedarf standardmÃ¤ÃŸig eingeklappt.
* **Modulares Dashboard:** VollstÃ¤ndige Zerlegung des Dashboard-Quelltexts in separate semantische Bausteine (`dashboard.html`, `dashboard.css`, `daten.js`, `matrix.js`, `profile.js`, `diagramme.js`) mit robuster Einbettung im Zusammenbau-Skript und SyntaxprÃ¼fung via `node --check` (falls Node.js verfÃ¼gbar).

## v3.67 (14.10.2026)

* **Multi-System-Sensorverlauf:** Achsenbeschriftung `Temperatur (Â°C)` durch vergrÃ¶ÃŸertes linkes Padding (75 px) und linksbÃ¼ndige Ausrichtung vollstÃ¤ndig sichtbar; Tooltip mit kompakten Systemnamen (`getShortSysName`), zweizeiligem Layout ohne EinheitenumbrÃ¼che und stets vorangestelltem Basissystem (auch bei Systemen ohne Zeitreihe).
* **Differenzierte Nutzungsprofile:** Gewichtung fÃ¼r Gaming (70 % GPU-FPS, 20 % ST, 10 % RAM/SSD), BÃ¼ro/Desktop (45 % SSD 4K-Reaktionszeit, 35 % ST, 20 % RAM-Latenz; 0 % GPU/MT) und Workstation (45 % MT, 20 % Instruktionen, 20 % RAM-Bandbreite, 15 % Dauertransfer); redundante Stromverbrauchs-Badges im Profilgitter entfernt.
* **Basissystem-Hervorhebung:** Markante Akzent-Umrandung mit Anker-Symbol im Auswahlbereich, Spaltenhervorhebung (`col-base`) mit `100 % Basis`-Badge in Hardware- und Matrix-Tabelle sowie Akzent-Rahmen in den Profilkarten.
* **Hardware-Tabelle aufgerÃ¤umt:** Redundante Berichtszeile entfernt; kompakte DatentrÃ¤ger-Zusammenfassung nach Typ und GesamtkapazitÃ¤t (`formatDiskSummary`) mit vollstÃ¤ndiger Modellliste im Hinweis beim Ãœberfahren.
* **LaufzeitschÃ¤tzungen kalibriert:** Diagnose-Grundzeit in der OberflÃ¤che auf 0 Min. gesenkt, ZeitzuschlÃ¤ge fÃ¼r Dateisystem/DISM/sfc (3 Min.), SMART (2 Min.), Defender (1 Min.) und WinSAT-Benchmark (1 Min.) auf realistische SSD-Messdauern angepasst.
* **Dashboard-Feinschliff:** HTML-Kopfzeile dynamisch an die Skriptversion gekoppelt, kompakte Matrix-Tabellenzeilen und fixierte TabellenkÃ¶pfe (`position: sticky`) beim vertikalen Scrollen.

## v3.66 (14.10.2026)

* **Dark Mode beim Start:** Win32-Bildlaufleisten (`ScrollBar`) in Inhaltsbereich, Karten, Listen und Protokollfeldern werden Ã¼ber `SetPreferredAppMode` und `ApplyNativeControlThemes` im `OnShown`-Ereignis bereits beim ersten Ã–ffnen der Anwendung fehlerfrei im dunklen Design gerendert.
* **Sensor-Status-Badge:** Dezentes Badge in der Kopfzeile signalisiert den aktuellen Treiberstatus (`LHM + PawnIO aktiv` in GrÃ¼n bzw. `Windows-Fallback` in Gelb).
* **Dashboard-Telemetrie fÃ¼r Neu-LÃ¤ufe:** Die Erzeugung von `Dashboard.html` erfolgt nach der Erstellung von `Anhang.zip`; `extractCsvSeries` liest Zeitreihen zusÃ¤tzlich direkt aus `.minibench`-Paketen sowie dem temporÃ¤ren Arbeitsverzeichnis ein.
* **Dual-Achsen & Info-Kachel im Dashboard-Chart:** Canvas-Diagramm mit zwei getrennten Y-Achsen (links: Temperatur in Â°C, rechts: Leistung in Watt); Systeme ohne Zeitreihe zeigen eine formatierte Infokarte mit den gemessenen Peak-Werten.
* **GPU-Effizienz in FPS/Watt:** Berechnung der GPU-Effizienz (`EFF_GPU`) strikt aus Render-FPS (`GPU|REND`) geteilt durch Maximalleistung (`GpuWMax`), synthetische Renderpunkte werden nicht mehr fÃ¤lschlich als FPS/Watt ausgewiesen; Beschriftung im Bericht auf `FPS/W` angepasst.
* **HTML-Bericht:** Fixierte Abschnittsnavigation (`position: sticky`) mit leichtem Schattenwurf und dezenter â€žNach obenâ€œ-SchaltflÃ¤che unten rechts.
* **Vorschaubild:** Aktualisierter Screenshot der Ã¼berarbeiteten BenutzeroberflÃ¤che (`Doku\Screenshots\Oberflaeche.png`).

## v3.65 (14.10.2026)

* **Einheitliche Karten:** neues Steuerelement `CardPanel` (abgerundeter Rahmen wie `FluentCard`) fÃ¼r die Optionen der Diagnose, die Tabellen des Lasttests, die Paketauswahl der Tools und die Listen der Laufansicht.
* **Wartung wie Optimierung:** gemeinsamer Aufbau `CategoryBlock` mit Beschreibung je Gruppe, â€žalleâ€œ und â€žkeineâ€œ, Etiketten rechts und â€žAlle aufklappenâ€œ.
* **Laufansicht angeglichen:** TitelgrÃ¶ÃŸe, Karte um die Listen, Etiketten in normaler Schreibweise (`BadgeCase`), â€žBericht Ã¶ffnenâ€œ als groÃŸer Hauptknopf, Protokollfeld und Fortschrittsbalken folgen dem Farbschema.
* **Korrekturen:** Zeitangabe unter dem Diagnoseprofil mit paralleler AusfÃ¼hrung, â€žDiagnose (parallel)â€œ in der FuÃŸleiste, KnÃ¶pfe der Optimierung in zwei Zeilen, Einzahl bei einem verwalteten Eintrag, Netzlaufwerk-Karte, Schildsymbol.
* **Diagnosebericht KPI-Kopfzeile:** Spaltenbreite der KPI-Karten (`.cards`) auf `minmax(120px, 1fr)` mit kompakten AbstÃ¤nden und SchriftgrÃ¶ÃŸe optimiert, sodass alle sieben Kennzahlen (inkl. ZuverlÃ¤ssigkeit) einzeilig ohne Zeilenumbruch dargestellt werden.
* **Tests:** zwei neue FÃ¤lle im Selbsttest der OberflÃ¤che (Etikett.Schreibweise, Karte).

## v3.64 (14.10.2026)

* **Navigation und Kopfzeile:** Seitenleiste in die Gruppen â€žAblaufâ€œ und â€žWerkzeugeâ€œ geteilt, angezeigte Seite nur Ã¼ber Akzentbalken, Dauer je angehaktem Modul. Flachere Kopfzeile mit SymbolknÃ¶pfen fÃ¼r Farbschema und Info; die Seite â€žÃ„nderungenâ€œ heiÃŸt â€žSystemÃ¤nderungenâ€œ.
* **FuÃŸleiste und Start:** Module in Ablaufreihenfolge mit Gesamtdauer, Startknopf mit Beschriftung nach Auswahl und Kontrast ab 4,5:1 in beiden Farbschemata (`UI.OnAccent`). Eine Zusammenfassung vor dem Start erscheint nur bei Eingriffen, Neustarts oder abgewÃ¤hltem Wiederherstellungspunkt (`StartUebersicht`).
* **Diagnose:** Profile als Segmentschalter, Funktionstest und Nur Absturzanalyse in der OberflÃ¤che entfernt (die Parameter des Skripts bleiben), â€žLesende PrÃ¼fungen parallel ausfÃ¼hrenâ€œ statt â€žSchneller Modusâ€œ, Aufklappgruppe â€žPrÃ¼fungenâ€œ, Etiketten fÃ¼r verÃ¤ndernde Optionen.
* **Benchmark und Lasttest:** Detailzeilen statt Klammertexten, Laufwerksliste mit freiem Platz, Referenz-Zustand als eigene Zeile; Lasttest als Tabelle mit Gesamtdauer, Hinweisen am Feld, Akkuhinweis und PawnIO-Etikett.
* **Wartung, Optimierung und Tools:** Etiketten fÃ¼r Risikostufe, Neustart, verwaltet und Zustand (`Badge`), Aufklappgruppen (`GroupHeader`), Warnung bei Eingriffen ohne Wiederherstellungspunkt; Tools ohne doppelte Bildlaufleiste, Werkzeugkarten mit Zustand, Netzlaufwerk als eigener Bereich.
* **Tests:** 13 neue FÃ¤lle im Selbsttest der OberflÃ¤che (Kontrast, Startknopf, Gesamtdauer, Deaktivierung, StartÃ¼bersicht, alte Voreinstellungen und weitere).

## v3.63 (13.10.2026)

* **GeschÃ¤tzter Gesamtsystemverbrauch (Desktop & Notebook):** Realistische Modellierung der internen Gleichstromleistung (DC) und des tatsÃ¤chlichen Verbrauchs an der Steckdose (AC) unter BerÃ¼cksichtigung von Sensorwerten (CPU-Package, GPU-Board), PlattformzuschlÃ¤gen (Mainboard/Chipsatz, LÃ¼fter, RAM-Auslastung, SSD-AktivitÃ¤t), Wandlerverlusten (VRM) und Netzteil-Wirkungsgraden (88 % Desktop, 90 % Notebook). Transparente Tooltips schlÃ¼sseln alle Teilkomponenten detailliert auf.
* **Effizienz-Kennzahlen (Punkte pro Watt):** Faire Leistungsbewertung Ã¼ber Effizienz-Indizes fÃ¼r Mehrkern-Prozessor (`CPU|MT / CpuW`), Grafikeinheit (`GPU / GpuW`) und das Gesamtsystem (`Overall / System-Watt`). Ausweisung in HTML-Bericht und Systemvergleich.
* **Interaktiver Stromkostenrechner im HTML-Bericht:** Unmittelbare Hochrechnung der jÃ¤hrlichen Betriebskosten mit AnpassungsmÃ¶glichkeiten fÃ¼r tÃ¤gliche Nutzungsdauer (Alltagsbetrieb, Volllast) und Strompreis in Cent pro Kilowattstunde zur direkten Wirtschaftlichkeitsbewertung.
* **Dashboard-Integration & Leistungskurven:** Neues Diagramm-Profil â€žLeistung (W)â€œ im Multi-System Dashboard visualisiert die VerbrauchsverlÃ¤ufe im Zeitreihenvergleich. System-Profilkarten, Spezifikationstabelle und Vergleichsmatrix enthalten Verbrauchskennzahlen fÃ¼r Leerlauf und Last.
* **Robuste Sensor-Absicherung & automatisierte Tests:** Saubere Einstufung als `n/v (kein Leistungssensor)` bei Ã¤lteren oder sensorlosen Plattformen ohne Division durch Null; `Testen.cmd` schlieÃŸt nach TestlÃ¤ufen automatisch ohne Tastendruck ab.

## v3.62 (13.10.2026)

* **Bedarfsorientierter Lauf-Export (`.minibench`):** Das `.minibench`-Austauschpaket wird nicht mehr standardmÃ¤ÃŸig bei jedem Durchlauf erzeugt, sondern nur wenn gewÃ¼nscht. Neben der Vergleichsdatenbank bietet die Ergebnisansicht nach dem Lauf eine neue SchaltflÃ¤che â€žLauf exportieren â€¦â€œ, die das Paket inklusive Datenbankeintrag und Bericht an einem frei wÃ¤hlbaren Ort speichert.
* **Redundante Textberichte vermieden:** Sofern der interaktive HTML-Bericht erfolgreich erzeugt wurde, entfÃ¤llt das Schreiben von `Diagnosebericht.txt`. Bei Fehlern wÃ¤hrend der HTML-Erstellung fungiert die Datei weiterhin als automatischer RÃ¼ckfall.
* **Kerne und Threads in allen Referenzprofilen:** Alle vorinstallierten Referenzsysteme (`Desktop_HighEnd`, `Desktop_Mittelklasse`, `MiniPC_APU`, `Notebook_Standard`, `Workstation_Mobil`) wurden um die realen Kerne und logischen Threads (`Hardware.Kerne`, `Hardware.Threads`) ergÃ¤nzt und in `Kern\Referenzen_Eingebettet.ps1` synchronisiert.
* **Kompakter Lasttest als Standard (2 Minuten):** Die Vorauswahl der Testdauer auf der Seite Lasttest wurde fÃ¼r Prozessor, Arbeitsspeicher, Grafikkarte und DatentrÃ¤ger von 15 Minuten auf 2 Minuten herabgesetzt.
* **Automatischer Git-Push & Release-Workflow:** `Bauen.cmd` committet nach erfolgreichen Pester-Tests nicht nur lokal und aktualisiert `README.md`, sondern pusht das Release direkt nach GitHub (`git push`). Die Entwicklerrichtlinien in `AGENTS.md` wurden aktualisiert.

## v3.61 (13.10.2026)

* **All-in-One-Austauschformat (`.minibench`):** Jeder erfolgreiche Durchlauf erzeugt im Berichtsordner automatisch ein komprimiertes Komplettpaket (`.minibench`), das `Datenbank.json`, `Diagnosebericht.html`, `Diagnosebericht.txt`, `Anhang.zip` und Telemetrie bÃ¼ndelt. Ãœber die SchaltflÃ¤che und das KontextmenÃ¼ â€žLauf exportieren â€¦â€œ auf der Seite Vergleichsdatenbank kann jeder Lauf exportiert werden (`-ExportLauf`). Die veraltete `KI-Kurzfassung.txt` und die zugehÃ¶rige Zwischenablage-SchaltflÃ¤che wurden restlos entfernt.
* **Erweiterter Import & Doppelklick-VerknÃ¼pfung:** Der Import-Dialog unterstÃ¼tzt neben Verzeichnissen auch Austauschpakete (`.minibench`, `.zip`) und einzelne `.json`-Dateien. Berichtsordner werden beim Import automatisch nach `Minibench-Daten\Berichte` Ã¼bertragen und das Feld `Ordner` im Datenbankeintrag verknÃ¼pft. Ein Doppelklick auf importierte LÃ¤ufe in der Vergleichsdatenbank Ã¶ffnet direkt den HTML-Bericht.
* **Lasttest-Telemetrie in Referenzsystemen:** Die eingebetteten Referenzprofile (`Desktop_HighEnd`, `Desktop_Mittelklasse`, `Desktop_GTX1070`, `MiniPC_APU`, `Notebook_Standard`, `Workstation_Mobil`) enthalten strukturierte Lasttest-Referenzwerte (HÃ¶chsttemperaturen, TDP/Leistungsaufnahme, TaktstabilitÃ¤t). HTML-Bericht und Dashboard-Matrix weisen Lasttest-Deltas (K und W) sowie Drosselungsverhalten transparent aus. `-MedianAktualisieren` ermittelt Lasttest-Mediane aus der Datenbank.
* **Unterbrechungsfreier Durchlauf-Start:** Die vorgeschalteten BestÃ¤tigungsdialoge (`AskDriver`, `AskSensorTools`) vor dem Start von Diagnose, Benchmark und Lasttest wurden entfernt. VollstÃ¤ndige Sensormessung mit PawnIO-Treiber ist standardmÃ¤ÃŸig aktiv (`drv = true`). Fehlen Sensorwerkzeuge, werden sie bei bestehender Internetverbindung lautlos nachgeladen, andernfalls wird ohne Blockade auf Windows-/ACPI-Sensoren zurÃ¼ckgegriffen. Die manuelle Treibereinstellung bleibt auf der Seite â€žSensoren liveâ€œ erhalten.

## v3.6 (10.10.2026)

* **Median-Referenz je GerÃ¤teklasse:** Neben der Systemreferenz (`Referenz.json`) und dem dynamischen Gesamtmedian stehen feste Mediane fÃ¼r Desktop und Notebook zur VerfÃ¼gung (`Referenz_Desktop.json`, `Referenz_Notebook.json`). Beide werden bei Bedarf auf Knopfdruck ("Median aktualisieren" auf der Vergleichsseite) oder Ã¼ber `-MedianAktualisieren` aus allen bereinigten Daten neu berechnet.
* **WartungsmaÃŸnahmen gegliedert:** Die 23 MaÃŸnahmen sind in vier fachliche Gruppen gegliedert (Systemdateien und Komponentenspeicher, Bereinigung und Speicherplatz, Windows Update/Netzwerk/Zeit, Dienste/GerÃ¤te/Energie). Jede Gruppe besitzt einen ausklappbaren Bereich mit Auswahlanzeige ("x von y gewÃ¤hlt"). Die SchaltflÃ¤chen "Ãœbliche Auswahl" und "Keine" aktualisieren alle Gruppen.
* **Diagnose-PrÃ¼fungen beim Start eingeklappt:** Der Bereich der PrÃ¼fungen ist standardmÃ¤ÃŸig geschlossen und zeigt eine dynamische Zusammenfassung der gewÃ¤hlten PrÃ¼fungen und des aktiven Profils. Bei Wahl des Profils "Benutzerdefiniert" Ã¶ffnet sich der Bereich automatisch.
* **Dunkles Farbschema ohne Schwarz auf Dunkel:** Dialoge und Formulare werden Ã¼ber die Hilfsmethode `UI.ThemeDialog` einheitlich formatiert. Kind-Elemente (Labels, CheckBoxen, RadioButtons, Eingabefelder) erhalten kontrastreiche Farben in beiden Themes.
* **Kerne und Threads in Spezifikation und Berichten:** Hardware-Objekte speichern reale Prozessorkerne und logische Threads (`Hardware.Kerne`, `Hardware.Threads`). Ã„ltere DatensÃ¤tze lesen diese aus `Diagnosebericht.txt` nach.
* **Herstellerbereinigte CPU-Anzeigenamen:** `Get-CpuAnzeigename` entfernt redundante HerstellerzusÃ¤tze (Intel, AMD, Qualcomm) und Taktraten aus der Anzeige ("Ryzen 5 7600X", "Core i5-8265U", "Snapdragon X Elite X1E78100"). Die internen DatenbankschlÃ¼ssel bleiben unverÃ¤ndert.
* **Dashboard & Telemetrie Ã¼berarbeitet:** Die Profilkarten sortieren Systeme nach Punktzahl (Basissystem an seiner Punktzahl hervorgehoben). Der Multi-System-Sensorverlauf steht vor den Befunden und enthÃ¤lt Reiter fÃ¼r CPU und GPU (inklusive Takt, Leistungsaufnahme und iGPU-Metriken im Tooltip). Synthetische Kurven wurden entfernt; fehlende Werte erscheinen als LÃ¼cke in der Linie.
* **Sensor- und PraxistesthÃ¤rtung:** Vor Benchmark und Lasttest prÃ¼ft `Test-HintergrundlastVorMessung` die Systemlast (Ã¼ber 15 % unterdrÃ¼ckt die Leerlauf-Einstufung und hebt Warnschwellen auf 90/80 Â°C). AkkuverschleiÃŸ-Berechnung nutzt die historische MaximalkapazitÃ¤t als Fallback und warnt bei sprunghaften AbfÃ¤llen. Windows 11 wird im Upgrade-Verlauf an der Buildnummer (>= 22000) erkannt. Hybridgrafik (Nvidia Optimus) markiert die dedizierte GPU als sekundÃ¤r ohne primÃ¤re Desktop-AuflÃ¶sung. Unplausible Sensorwerte (< 5 Â°C Komponententemperatur, doppelte Leerlaufspannungen > 1.5 V) werden gefiltert. Akkubetrieb wÃ¤hrend des Benchmarks wird vermerkt.

## v3.54 (09.10.2026)

* **Datenordner immer auf dem Stick:** `Resolve-DataDir` liest `Netzwerk.json` nicht mehr und ruft kein `net use`. Der Start ist ohne NAS so schnell wie mit; Tools, Cache und Laufzeit liegen immer im Datenordner auf dem Stick (vorher wurden Tools auf dem NAS gesucht, Sensoren fehlten).
* **Netzlaufwerk als Spiegel (`Kern\Ablage.ps1`):** Abgleich nur Ã¼ber Aktualisieren (Hilfsmodus `-Abgleich`), in beide Richtungen gegen den Stand des letzten Abgleichs (`Minibench-Daten\Abgleich\Stand.json`). Neuere Fassung gilt, Ã¤ltere nach `Archiv\Abgleich\<Zeit>\Konflikte`; LÃ¶schungen werden als Verschiebung ins Archiv Ã¼bertragen, Ã„nderung schlÃ¤gt LÃ¶schung. Schutz vor MassenlÃ¶schung (hÃ¤lt an; `-AbgleichLoeschen` bestÃ¤tigt, `-AbgleichNeu` fÃ¼hrt ohne Entfernen zusammen), Abbruch ohne Ã„nderung bei Lesefehlern, Sperre `Abgleich.lock`, Toleranz fÃ¼r Zeitverschiebung auf FAT-Sticks.
* **Netzlaufwerk einrichten:** `Netzwerk.json` nur im Datenordner auf dem Stick, Kennwort nie gespeichert, Verbindung Ã¼ber `WNetAddConnection2` ohne `/persistent`, beim Beenden getrennt. Pfade mit nur einem fÃ¼hrenden `\` werden abgelehnt (Ursache des Ordners `TRUENAS` auf dem Stick).
* **Vergleichsdatenbank:** Entfernen verschiebt Eintrag und Berichtsordner nach `Archiv\Entfernt\<Zeit>` (`-Entfernen`); Name Ã¤ndern benennt Datenbankdatei und Berichtsordner um und fÃ¼hrt Verweise in den Ã„nderungsprotokollen nach (`-Umbenennen`).
* **Datenpflege:** PawnIO-Merker und `Start.log` bleiben stehen; `-Include` ohne `-Recurse` ersetzt (fand unter PS 5.1 nichts).
* **Tests:** neue Datei `Ablage.Tests.ps1`, Datenordner-Tests in `Datenbank.Tests.ps1` neu gefasst, drei FÃ¤lle im Selbsttest der OberflÃ¤che.

## v3.53 (09.10.2026)

* **Softwarepakete (Seite Tools) Ã¼ber das Ã„nderungsprotokoll:** winget lÃ¤uft im Arbeitsprozess (`-SoftwareInstallieren`) mit Frist je Paket (20 Minuten) und vollstÃ¤ndig gelesener Ausgabe. Vorher konnte die Installation unbegrenzt hÃ¤ngen, weil die umgeleitete Ausgabe nie gelesen wurde. Jede Installation ist ein Eingriff im Ã„nderungsprotokoll mit Gegenbefehl `winget uninstall`; die Seite Ã„nderungen nimmt sie zurÃ¼ck. Schon vorhandene Programme werden weder angefasst noch protokolliert. Vor dem Start fragt die OberflÃ¤che nach.
* **Grafikauswahl im Lasttest:** Die Wahl auf der Seite Lasttest sprang auf den Wert des Benchmarks zurÃ¼ck (vertauschte Zuweisung). Beide Seiten gleichen AuflÃ¶sung, Anzeige und Grafikeinheit jetzt Ã¼ber `CopySelection` in beide Richtungen ab.
* **Absturzabbilder:** Parameter ab 0x8000000000000000 (Kerneladressen) brachen das Lesen eines Minidumps ab.
* **Testsuite nach Fachgebieten:** Die Dateien `Version26.Tests.ps1` bis `Version352.Tests.ps1` und `Dashboard.Tests.ps1` sind aufgelÃ¶st. Neu: `Ablauf`, `Bericht`, `Messung`, `Optimierung`, `Release`; erweitert: `Oberflaeche`, `Sensoren`, `Datenbank`, `Aufbau`, `Auswertung`, `Modulvertrag`, `Werkzeuge`, `Aenderungen`. Die OberflÃ¤che wird je Testlauf einmal Ã¼bersetzt und mit einem Selbsttest geprÃ¼ft. `Testen.cmd -Datei Oberflaeche,Release` startet einzelne Dateien.
* **Bauen.cmd:** Commit-Nachricht aus `Versionen.cs`, Commit nur nach bestandenen Tests, Ergebnis von `git commit` wird geprÃ¼ft, keine BOM mehr am Dateianfang.

## v3.52 (07.10.2026)

* **Sensorwerkzeuge, Binaries & Cache strikt lokal gebunden (NAS-HÃ¤rtung):**
  * **Trennung von Daten und Code:** Berichte ($script:ReportDir) und Datenbanken ($script:DbDir) dÃ¼rfen auf konfigurierte Netzlaufwerke/NAS-Pfade zeigen. AusfÃ¼hrbare Dateien, Treiber und temporÃ¤re Caches ($script:LocalDataDir, $script:CacheDir, $script:ToolsDir, $script:CpDir) werden strikt lokal (auf dem USB-Stick bzw. im lokalen %TEMP% / %LOCALAPPDATA%) isoliert.
  * **Behebung von Pfadformat- & CAS-Fehlern:** LÃ¶st Ausnahmen ("Das angegebene Pfadformat wird nicht unterstÃ¼tzt.") und .NET Code Access Security-Blockaden beim Zugriff auf externe Werkzeuge (LibreHardwareMonitor, PawnIO, smartctl, Add-CachedType).
  * **HÃ¤rtung von Get-FileSha256:** VollstÃ¤ndige Pfadnormalisierung via [System.IO.Path]::GetFullPath und robuste Fehlerabfangung bei Datei-HashprÃ¼fungen.
* **Fix: Verlinkung im Dashboard auf Diagnoseberichte:**
  * **Relative PfadauflÃ¶sung:** Verlinkungen aus Dashboard.html auf Diagnosebericht.html werden relativ zu Dashboard.html aufgelÃ¶st (<Lauf-Ordner>/Diagnosebericht.html) mit standardkonformen VorwÃ¤rtsslashes (/), wodurch ERR_FILE_NOT_FOUND und doppelte Berichte/-PrÃ¤fixe behoben werden.
* **Optimierte Datenpflege & Bereinigung alter/verwaister EintrÃ¤ge:**
  * **Archivierung verwaister DB-EintrÃ¤ge:** DatenbankeintrÃ¤ge ohne zugehÃ¶rigen Berichtsordner oder mit beschÃ¤digter/leerer (0 Byte) JSON-Datei werden sauber nach Archiv\Verwaist\Datenbank bzw. Archiv\Beschaedigt\Datenbank verschoben.
  * **GrÃ¼ndliche Laufzeit-Bereinigung:** Verwaiste Locks (*.lock), abgebrochene Checkpoints (checkpoint*.json), temporÃ¤re Dateien (*.tmp) und Signaldaten im Verzeichnis Laufzeit/ werden rÃ¼ckstandslos bereinigt.
  * **Detaillierte Statusmeldung & Protokoll:** Berechnung und Anzeige des freigegebenen Speicherplatzes in Megabyte in der BenutzeroberflÃ¤che und im Protokoll Archiv\Datenpflege.log.
* **OberflÃ¤chen-Bereinigung & optimiertes Layout:**
  * **Redundante Benchmark-Checkliste entfernt:** Die veraltete KontrollkÃ¤stchen-Liste ("Bereits geprÃ¼fte Systeme einblenden") auf der Benchmark-Startseite wurde entfernt; Vergleichsaufgaben werden vollstÃ¤ndig durch das moderne Dashboard und die Vergleichsdatenbank abgedeckt.
  * **Kategorisierte Optionen in Fluent-Karten:** Diagnose-Optionen auf der Startseite in zwei Ã¼bersichtliche Karten gegliedert ("Zusatzwerkzeuge & Speicherdiagnose" und "PrÃ¼fzeitrÃ¤ume & Schwellenwerte").
  * **VergrÃ¶ÃŸerte StartfenstergrÃ¶ÃŸe:** Auf 1320x860 erweitert, um stÃ¶rende Scrollbalken beim ersten Starten der Anwendung vollstÃ¤ndig zu vermeiden.
* **VollstÃ¤ndige Versionshistorie (1.0 bis 1.8):**
  * LÃ¼ckenlose Dokumentation aller 9 Ur-Versionen von 1.0 bis 1.8 mit konkreten Changelogs und Neuerungen nachgepflegt.

## v3.51 (07.10.2026)

* **Dashboard-Berichtsverlinkung & Schnellzugriff:**
  * **Interaktive Berichtsaufrufe:** Im Multi-System-Dashboard (`Dashboard.html`) fÃ¼hren Klicks auf beliebige Systemnamen in Spezifikationstabelle, Profil-Cards, Benchmark-Matrix und BefundÃ¼bersicht direkt zum zugehÃ¶rigen Diagnosebericht (`Diagnosebericht.html`).
  * **Berichts-Buttons & Spalte:** TabellenkÃ¶pfe und eine dedizierte Diagnosebericht-Zeile bieten direkte `ðŸ“„ Bericht`-Links zur schnellen Detailanalyse.
  * **Neuer 'Bericht Ã¶ffnen'-Button in der BenutzeroberflÃ¤che:** Auf der Vergleichsseite (`BuildDbPage`) steht neben dem Dashboard-Button nun ein direkter Button "Bericht Ã¶ffnen" zur VerfÃ¼gung, der den vollstÃ¤ndigen HTML-Bericht des ausgewÃ¤hlten Systems mit einem Klick im Browser Ã¶ffnet.
* **Referenzsystem-Deduplizierung:**
  * **Keine doppelten EintrÃ¤ge:** Referenzprofile (`Desktop_HighEnd`, `Notebook_Mittelklasse`, etc.) werden beim Multi-System-Export (`Export-BenchDashboardData`) aus der Datenbankliste herausgefiltert und erscheinen im Dashboard ausschlieÃŸlich einmalig in der dedizierten Sektion "Referenzprofile (Eingebettet)" mit Stern-Symbol (`â­`).
  * **Robuste Eindeutigkeit im JavaScript:** Clientseitiges deduplizierendes System-Mapping verhindert doppelte Rendering-EintrÃ¤ge in Dropdowns und Kacheln.
* **Persistente Netzlaufwerk- & NAS-Verbindung:**
  * **Redundante Speicherung:** Die Netzwerkkonfiguration (`Netzwerk.json`) wird redundant an allen lokalen Speicherorten (neben der Minibench-Executable, im Datenordner, im Benutzer-Dokumentenverzeichnis und in AppData) gesichert.
  * **Automatischer Reconnect:** Beim Start von Minibench stellt `Resolve-DataDir` nicht verbundene oder abgelaufene UNC-Freigaben automatisch im Hintergrund Ã¼ber `net use /persistent:yes` wieder her, sodass Berichte und Datenbanken ohne manuelles erneutes Verbinden sofort bereitstehen.
  * **Dynamische Pfadaktualisierung:** Die BenutzeroberflÃ¤che aktualisiert die Pfadanzeige nach dem Wechsel des Datenordners live ohne Neustart.

## v3.5 (06.10.2026)

* **Fehlerbehebung & Optimierungs-Presets:**
  * **Entkopplung der Taskleisten-Gruppierung:** `TaskleisteGruppieren` in `Katalog.psd1` bereinigt; steuert nun ausschlieÃŸlich die reine Gruppierung (`TaskbarGlomLevel`) ohne ungewollte Nebeneffekte.
  * **EigenstÃ¤ndiger Eintrag 'Taskleiste: Task beenden per Rechtsklick':** `TaskbarEndTask` als eigenstÃ¤ndiger Eintrag mit Vorlagen `MSE` in Kategorie `Explorer, Taskleiste und Start` definiert; setzt `TaskbarEndTask` = 1 in `TaskbarDeveloperSettings` und `Advanced` (Gegenbefehl: 0).
  * **Schlankes Minimal-Preset als Standardvorauswahl:** Preset **Minimal** (`M`) auf ein kompaktes Basispaket gestrafft (Minimaltelemetrie, Fehlerberichte, Werbe-ID, Speicheroptimierung, TaskbarEndTask). `BuildOptPage()` wÃ¤hlt nun beim Start standardmÃ¤ÃŸig `ApplyOptPreset("M")` vor. "Leos Empfehlung" (`S`) bleibt das umfassende Standardpaket.
* **Erweitertes responsives Fensterlayout:**
  * **GrÃ¶ÃŸere Startabmessungen:** Responsiv vergrÃ¶ÃŸert auf `Math.Min(UI.S(1220), (int)(WorkingArea.Width * 0.95))` Breite und `Math.Min(UI.S(740), (int)(WorkingArea.Height * 0.92))` HÃ¶he (`MinimumSize`: `880x560`). Bietet sofort mehr Raum fÃ¼r Tabellen, Kacheln und Vergleiche, ohne auf 1080p-Notebooks mit Skalierung Ã¼berzulaufen.
* **Modul 'Tools': Softwarepakete installieren (winget):**
  * **Integrierter Software-Paket-Manager:** Neuer Abschnitt auf der Tools-Seite zur einfachen 1-Klick-Installation populÃ¤rer Standardsoftware via Windows Package Manager (`winget.exe`).
  * **Paketauswahl:** Web-Browser (Google Chrome, Mozilla Firefox, Opera), Gaming & Chat (Steam, Discord) und ProduktivitÃ¤t (Notepad++, ONLYOFFICE Desktop Editors, 7-Zip, VLC Media Player).
  * **HintergrundprÃ¼fung & asynchroner Installer:** PrÃ¼ft beim Start im Hintergrund auf Vorhandensein von `winget.exe` (mit klarer Hinweismeldung und deaktiviertem Button, falls nicht vorhanden). AusfÃ¼hrung lÃ¤uft asynchron mit Live-Fortschritt im UI-Status.
* **NAS- & Netzlaufwerk-Integration:**
  * **Konfigurierbare Netzlaufwerke:** UnterstÃ¼tzung fÃ¼r `Minibench-Daten\Netzwerk.json` (`NasPfad` / `NetzwerkPfad`) in `Resolve-DataDir`.
  * **Robuster lokaler Fallback:** Automatische PrÃ¼fung auf Schreibrechte (`Test-WritableDir`). Ist das NAS nicht erreichbar oder schreibgeschÃ¼tzt, fÃ¤llt Minibench automatisch und ohne Fehlerdialog auf den lokalen Datenordner zurÃ¼ck.
  * **1-Klick-Einbindung im UI:** Neuer Dialog "Netzlaufwerk / NAS verbinden" auf der Tools- und Datenbankseite mit Eingabe fÃ¼r UNC-Pfade, optionalen Anmeldedaten (`net use`), Verbindungstest und direktem Live-Umschalten des Datenverzeichnisses.
* **Interaktive Berichtsdiagramme & Befunde-Bereinigung:**
  * **Hoverbare Sensor-Diagramme:** Sensor-Charts (Temperatur, Takt, Leistung, Auslastung) im Hauptbericht (`Diagnosebericht.html`) mit derselben interaktiven Hover-Tooltip-Logik wie im Dashboard: Beim Bewegen der Maus Ã¼ber Kurven oder Messpunkte werden Zeitstempel, Â°C, GHz und Watt an genau diesem Punkt angezeigt (100 % inline ohne externe Bibliotheken).
  * **Bereinigte Befunde-Ansicht:** Entfernung des Text-Suchfelds zugunsten aufgerÃ¤umter, praktischer Filter-Badges (Alle, Kritisch, Warnung, Info) und klarer TrefferzÃ¤hlung.

## v3.4 (06.10.2026)

* **Neues Modul 'Tools' in der BenutzeroberflÃ¤che:**
  * **Navigationspunkt 'Tools':** Neuer MenÃ¼punkt in der linken Navigation direkt unter 'Optimierung' und vor 'Sensoren' mit modernem Fluent-Design (`UI.IcoTools`).
  * **Portable Werkzeuge:** Integrierte Erkennung und Starter fÃ¼r Revo Uninstaller Portable (`Minibench-Daten\Tools\RevoUninstaller\*.exe`), MiniTool Partition Wizard (`Minibench-Daten\Tools\PartitionWizard\*.exe`) und WizTree Portable (`Minibench-Daten\Tools\WizTree\*.exe`) inklusive PrÃ¼fung auf lokale Systeminstallationen, 'Starten (Admin)'-SchaltflÃ¤chen und Schnell-Links zum Herunterladen bzw. Ordner Ã¶ffnen.
  * **System-Shortcuts & Schnellstarter:**
    * 'Ins BIOS/UEFI neu starten': FÃ¼hrt nach BestÃ¤tigung `shutdown.exe /r /fw /t 0` aus; prÃ¼ft vorab Ã¼ber native Win32 `GetFirmwareType` und Registry auf UEFI-UnterstÃ¼tzung.
    * Schnellzugriff auf native Windows-Konsolen: DatentrÃ¤gerverwaltung (`diskmgmt.msc`), GerÃ¤te-Manager (`devmgmt.msc`) und ZuverlÃ¤ssigkeitsverlauf (`perfmon /rel`).
  * **Volle Dark-Mode- und Tooltip-UnterstÃ¼tzung:** Nahtlose optische Integration im Hell- und Dunkelmodus (`UI.IsDark`) sowie konsequente Tooltips (`Tip(...)`) auf allen Steuerelementen.
* **Optimierungen: 'Leos Empfehlung' erweitert:**
  * **Task beenden per Rechtsklick (`TaskbarEndTask`):** Neue Optimierung in der Kategorie `Bedienung` fÃ¼r Windows 11 in den Vorlagen M, S (Leos Empfehlung) und E. ErmÃ¶glicht das sofortige Beenden hÃ¤ngender Programme direkt Ã¼ber das KontextmenÃ¼ der Taskleiste (`HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings` und `HKCU:\...\Advanced`, DWord 1). VollstÃ¤ndig dokumentiert und rÃ¼ckgÃ¤ngig machbar.
  * **UEFI-Neustart Desktop-Shortcut (`UefiNeustart`):** Desktop-KontextmenÃ¼eintrag fÃ¼r den direkten Neustart in die Firmware in den Vorlagen S und E mit Protokollierung und Wiederherstellungsoption.
* **Interaktiver Hauptbericht (Diagnosebericht.html) & Designangleichung:**
  * **Abschnitt-Navigation:** Schnelle Reiter-Leiste am Kopf des Berichts zur gezielten Ansicht von SystemÃ¼bersicht, Benchmark, Befunden, Hardware oder Sensoren.
  * **Live-Suche & Stufen-Filter fÃ¼r Befunde:** Sofortiges Durchsuchen von Befunden nach Text und Filtern nach Schweregraden (Kritisch, Warnung, Info) mit dynamischer TrefferzÃ¤hlung.
  * **Interaktive Sensor-Chart Tooltips:** Pure Vanilla JavaScript Hover-Tooltips fÃ¼r Messpunkte in Takt-, Temperatur-, Rendertest- und Durchsatz-Diagrammen ohne externe Bibliotheken.
  * **Designangleichung & krÃ¤ftige Farbwelt:** Synchronisierung der CSS-Farbpaletten von `Dashboard.html` und `Diagnosebericht.html` auf Basis von `Stil.ps1` mit satten Badges, klaren Kontrasten und gestochen scharfer Typografie.
  * **Direktverlinkung zum Vergleichsdashboard:** Neue SchaltflÃ¤che in der Bericht-Navigation und im Benchmark-Vergleichsbereich verlinkt direkt auf das interaktive `Dashboard.html` mit dem aktuellen PC als vorausgewÃ¤hltem Basissystem (`?system=...`).
* **Hotfix & Detailkorrekturen:**
  * **Optimierungsauswahl 'Leos Empfehlung':** `TaskbarEndTask` und `UefiNeustart` in `defaultLeoEmpfehlung` aufgenommen und Auswahllogik in der GUI abgesichert, sodass die Option bei Wahl des Presets zuverlÃ¤ssig markiert wird.
  * **Symbol fÃ¼r Modul 'Tools':** Eigenes Segoe MDL2-Symbol `\uE71D` (AllApps, 4 Kacheln) implementiert und an die linke Navigation Ã¼bergeben (eindeutige Unterscheidung von der Wartungsseite `\uE90F`).
  * **System-Vorauswahl im Dashboard:** `Dashboard.html` wertet URL-Parameter `?system=` bzw. Hash-Fragmente aus und schaltet das Basissystem automatisch auf den angegebenen PC um.

## v3.32 (06.10.2026)

* **Dark-Mode-Feinschliff fÃ¼r die WinForms-OberflÃ¤che:**
  * **Native dunkle Win32-Scrollbars:** Aktivierung des Windows-eigenen Fluent-Dunkelmodus fÃ¼r Scrollbars Ã¼ber `SetPreferredAppMode(2)` (ForceDark) und `SetWindowTheme(hWnd, "DarkMode_Explorer", null)`. Beseitigt grell-weiÃŸe Scrollbalken in Inhaltsbereichen (`content`), Navigationsleisten (`left`), `ListView`-Listen und `CheckedListBox`-Steuerelementen ohne Fremdbibliotheken oder Eingriffe ins Gesamtsystem.
  * **Eigene `DarkComboBox`-Komponente:** Ersatz der standardmÃ¤ÃŸigen WinForms-ComboBoxen durch eine vollstÃ¤ndig darkmode-fÃ¤hige Dropdown-Komponente mit sauber abgedunkeltem Hintergrund (`UI.Panel`), dezentem Rahmen (`UI.Line`), hochauflÃ¶send gezeichnetem Pfeilsymbol und owner-drawn Auswahlelementen im Fluent-Design.
  * **Dunkle TabellenkÃ¶pfe (`SysHeader32`):** ListView-SpaltenkÃ¶pfe werden Ã¼ber `EnableDarkListView` im Dark Mode sauber abgedunkelt gezeichnet (`#1C1E20` mit dezentem Segmenttrenner und Beschriftung in Segoe UI Semibold), wodurch weiÃŸe Kopfzeilen in allen Tabellen entfallen.
  * **Optimierte Textkontraste & Deaktivierte Steuerelemente:** Deaktivierte SchaltflÃ¤chen (wie "Entfernen") und Optionen nutzen ein eigens gezeichnetes Farbschema (`#8C919B` auf dezentem Panel-Hintergrund) anstelle des unleserlichen Windows-GDI-Schattendrucks ("schwarzer/grauer Text auf dunklem Grund"). Diagnose-Optionen schalten bei Klick im Vorgaben-Modus barrierefrei auf benutzerdefinierte Auswahl um.
* **Konsolidierung des Systemvergleichs:**
  * **VollstÃ¤ndige Dashboard-AblÃ¶sung:** Das interaktive Multi-System-Dashboard deckt nun alle Funktionen des Ã¤lteren Vergleichssystems ab (Hardwaredaten im Direktvergleich, lÃ¼ckenlose Benchmark-Matrix mit Bestwert-Hervorhebung, Befunde-GegenÃ¼berstellung und Multi-Kurven-Telemetrie).
  * **Bereinigung redundanter Pfade:** Die statische Vergleichsfunktion wurde aus der BenutzeroberflÃ¤che und der Befehlszeile nahtlos auf das interaktive Dashboard konsolidiert.
  * **Aktualisierte Vergleichsdatenbank:** Die Vergleichsseite bietet nun eine fokussierte, einheitliche primÃ¤re AktionsschaltflÃ¤che ("Im Dashboard vergleichen") mit dynamischer Systemanzahlanzeige sowie ein angepasstes KontextmenÃ¼.

## v3.31 (06.10.2026)

* **Multi-System-Vergleich im interaktiven Dashboard (N >= 2):**
  * Umstellung der starren 2-System-Auswahl auf flexible Multi-System-Selektion: Ein beliebiges Basissystem plus beliebig viele Vergleichssysteme Ã¼ber interaktive System-Chips mit SchaltflÃ¤chen zur Schnellauswahl.
  * VollstÃ¤ndige Hardware-Spezifikationen im Direktvergleich: Ãœbersichtstabelle aller gewÃ¤hlten Systeme nebeneinander (Rechnername, CPU, RAM, GPU, DatentrÃ¤ger, Betriebssystem, Mainboard, Installationsdatum) Ã¤quivalent zum klassischen Vergleichsbericht.
  * VollstÃ¤ndige Benchmark-Matrix: Synoptische GegenÃ¼berstellung sÃ¤mtlicher vorliegender Messwerte (CPU Single/Multi/AES/SHA/Deflate, RAM Lesen/Schreiben/Kopieren/Latenz, GPU FPS/1%-Low/0,1%-Low/Mikroruckler, DatentrÃ¤ger sequentiell und 4K) mit automatischer Bestwert-Hervorhebung (`ðŸ‘‘ Bestwert`).
  * Synoptischer Befund-Vergleich: GegenÃ¼berstellung aller Diagnose-Befunde (Kritisch, Warnungen, Hinweise) aufgeschlÃ¼sselt nach den verglichenen Systemen.
  * Interaktiver Canvas-Chart fÃ¼r mehrere Systeme: Umschaltbare Takt- und Temperaturkurven (Temperatur-Modus, Takt-Modus, Einzel-Detailansicht) mit Farbcodierung je System und simultanem Multi-System-Tooltip beim Ãœberfahren mit der Maus.
* **Paralleler Betriebsmodus in der OberflÃ¤che:**
  * Gleichberechtigte Koexistenz beider Vergleichswege in der Toolbar und im KontextmenÃ¼ der Vergleichsseite:
    * *Vergleichen (Klassisch)* erzeugt den bewÃ¤hrten statischen `Vergleichsbericht.html`.
    * *Interaktives Dashboard* Ã¼bergibt alle angehakten Systeme an `Show-BenchDashboard` und Ã¶ffnet das interaktive Dashboard mit vorausgewÃ¤hlten Systemen.
* **Nativer Dark Mode fÃ¼r die MiniBench-App (DiagGui.cs):**
  * Dynamisches Farbsystem in `DiagGui_Steuerelemente.cs` (`UI.IsDark`, `UI.SetTheme(bool dark)`): Saubere Umschaltung zwischen hellem Modus (`#F9F9FB`, `#FFFFFF`, `#1C1D1F`, `#5F6368`, `#E5E7EB`, `#161E2E`) und augenschonendem dunklem Modus (`#18191A`, `#242526`, `#F5F6F7`, `#9CA3AF`, `#3A3B3C`, `#121314`) inklusive angepasster Statusfarben und SteuerflÃ¤chen.
  * Titelleisten-Umschalter: Diskreter Theme-Schalter (â˜€ï¸/ðŸŒ™) in der Kopfzeile neben der Versionshistorie mit Mouseover-Tooltip (`Tip`).
  * Windows DWM Immersive Dark Mode: Dynamische Umschaltung der Windows-Fensterleiste via `DwmSetWindowAttribute` (`DWMWA_USE_IMMERSIVE_DARK_MODE`).
  * Einstellungs-Persistierung: Speicherung der gewÃ¤hlten Theme-PrÃ¤ferenz in `Minibench-Daten/Einstellungen.json` mit automatischem Fallback auf das Windows-Systemdesign (`AppsUseLightTheme`).
  * Flackerfreie Neuzeichnung durch rekursive Steuerelement-Aktualisierung und Doppelpufferung.
* **Build-Prozess & Git-Automatisierung:**
  * Erweiterung von `Bauen.cmd`: Nach erfolgreichem Bau und bestandenen Tests wird das Git-Repository geprÃ¼ft, Quell- und Dokumentationsdateien gestaged und ein lokaler Release-Commit formatiert erzeugt.

## v3.3 (06.10.2026)

* **Interaktives Benchmark- & Diagnose-Dashboard:**
  * VollstÃ¤ndig offline-fÃ¤higes HTML5-Dashboard (`Dashboard.html`) mit reinem Vanilla-JS und modularem CSS ohne externe CDN-AbhÃ¤ngigkeiten.
  * Neue PowerShell-Integrationsfunktionen in `src/Bericht/Bausteine_Dashboard.ps1`: `Export-BenchDashboardData` (aggregiert DatenbanklÃ¤ufe, Systemmetadaten, Benchmark-Keys, Disk-Werte und Telemetrie), `Get-BenchDashboardHtmlTemplate`, `New-BenchDashboardHtml` und `Export-BenchDashboardHtml`.
  * Integration in GUI und CLI: SchaltflÃ¤che *Dashboard* mit Tooltip auf der Vergleichsseite in `DiagGui_Vergleich.cs` sowie CLI-Parameter `-Dashboard` und `-DashboardExport <Pfad>`.
* **Dual-Axis Telemetrie-Visualisierung (Pure Canvas):**
  * HochprÃ¤ziser interaktiver 2-Achsen-Canvas-Chart fÃ¼r Takt (GHz/MHz) und CPU-Temperatur (Â°C) Ã¼ber den Lasttest-Zeitverlauf.
  * Automatische Extraktion und Dekomprimierung von `Lasttest-Verlauf.csv` aus den Berichts-AnhÃ¤ngen (`Anhang.zip`).
  * Visuelle Warnbalken und Markierungen bei thermischer oder leistungsbezogener Prozessordrosselung (Thermal / Power Throttling) sowie TjMax-Referenzlinie.
  * FlÃ¼ssige Hover-Fadenkreuze und Tooltip-Karten an der Cursor-Position mit exakten Sensor- und Taktratenwerten.
* **Fluent 2 / Wintoys-UI & Systemvergleich:**
  * Designkonforme Umsetzung im Fluent 2-Design: Karten mit dezentem 1px-Rahmen (`#E5E7EB`), sanften Rundungen (8â€“10 px) und Windows 11-Akzentblau (`#0067C0`).
  * Responsiver Theme-Switch (Dark Mode `#202020` / Light Mode `#F9F9FB`) mit `localStorage`-Persistierung.
  * Interaktive Systemauswahl: Beliebiges Zielsystem aus der lokalen Datenbank gegen alle 5 integrierten Referenzsysteme (Desktop High-End, Desktop Mittelklasse, Mini-PC, Notebook Standard, Workstation Mobil).
  * Prominente Score-Kacheln fÃ¼r Gaming, BÃ¼ro/Desktop und Workstation sowie Komponenten-Pillen mit farblicher Trendbewertung (GrÃ¼n bei Zuwachs, Rot bei Abfall).
* **UI-Bereinigung & Barrierefreiheit (Navigation & Vergleichsseite):**
  * AufgerÃ¤umte Vergleichsdatenbank: Wartungs- und Importwerkzeuge in eine obere Werkzeugleiste (`topTools`) direkt unter dem Datenbankpfad verlegt.
  * Fokussierte Haupt-Aktionsleiste am unteren Fensterrand mit 4 Kernaktionen (*Vergleichen*, *Dashboard*, *Name Ã¤ndern ...*, *Entfernen*) sowie nativem Rechtsklick-KontextmenÃ¼ fÃ¼r alle Zeilenaktionen.
  * Navigationsleiste ohne Untertitel-Clipping: Kurzbeschreibungen von den NavItem-Buttons entfernt, um Textabschneiden bei hohen Display-Skalierungen (150 % / 200 %) zu verhindern; Standard-Fluent-HÃ¶he von `UI.S(40)` mit zentrierten Segoe-Icons und Checkboxen; vollstÃ¤ndige Modulbeschreibungen weiterhin als Hover-Tooltips verfÃ¼gbar.
  * Fehlerbehebung Systemvergleich im Dashboard: Beseitigung einer Namenskollision bei `$idleSens` in `Export-BenchDashboardData`, strikte Typisierung von System-IDs und defensiver String-ID-Abgleich im Frontend-JavaScript.

## v3.2 (06.10.2026)

* **Fluent 2 / Wintoys-Look & Design-Modernisierung:**
  * Modernes Windows 11 Fluent 2-Farbschema: Hintergrund `#F9F9FB`, weiÃŸe Karten mit feinem 1px-Rahmen (`#E5E7EB`), Windows 11-Akzentfarben (`#0067C0`, Hover `#005A9E`, Active `#004F8A`, Auswahl `#EBF3FB`) und weiche Pastell-Badges.
  * Neues `ToggleSwitch`-Steuerelement: PillenfÃ¶rmige Spur (`UI.S(20)` HÃ¶he, `UI.S(38)` Breite), runder Schieber mit dezentem Schatten, Windows-AkzentfÃ¼llung im aktiven Zustand, barrierefreie Tastaturbedienung mit Leertaste und Tooltip-UnterstÃ¼tzung.
  * Neue `FluentCard`-Komponente: Kartenansicht fÃ¼r Optionen im Wintoys-Stil mit Segoe-Symbol, Titel, Untertitel und eingebetteten Toggle-Schaltern oder AktionsschaltflÃ¤chen (Umschaltung per Klick auf die Karte).
  * Gestochen scharfe Segoe-Icons: Einbindung von `Segoe Fluent Icons` (mit Fallback auf `Segoe MDL2 Assets`) fÃ¼r Navigation und Hardware-Kategorien.
  * Modernisierte Navigation (`NavItem`): Windows 11-Sidebar-Stil mit 6 px Eckenrundung, aktivem 3 px-Akzentbalken, sanftem Auswahlhintergrund und Segoe-Icons.
* **Windows 11 DWM-Integration & Notebook-Sicherheit:**
  * Echte Windows 11-Fensterabrundung Ã¼ber DWM P/Invoke (`DWMWA_WINDOW_CORNER_PREFERENCE = 33`, `DWMWCP_ROUND = 2`).
  * Saubere DPI-Initialisierung Ã¼ber `SetProcessDpiAwarenessContext(-4)` (PerMonitorV2) und `SetProcessDpiAwareness(2)` ohne das veraltete `SetProcessDPIAware()`.
  * Dynamische Fensterberechnung anhand des sichtbaren Arbeitsbereichs (`WorkingArea * 0.94` Breite, `WorkingArea * 0.88` HÃ¶he) und `MinimumSize` von `860x540` px gegen Ãœberlauf auf 1080p-Notebooks bei 150 % Skalierung.
* **Zentraler DPI-Helper & Tabellen-OwnerDraw:**
  * Zentraler DPI-Helper in `UI` mit `UI.DpiScale`, `UI.S(px)` und `UI.SF(px)`.
  * VollstÃ¤ndig responsive Seiten Versionen, Ã„nderungen und Vergleichsdatenbank mit `DockStyle.Fill` und automatischer Restbreiten-AusfÃ¼llung der ListViews.
  * Tabellen-OwnerDraw in der Laufansicht mit skalierter ZeilenhÃ¶he (`34 px`), zentrierten Badge-Pillen und verbreiterten Messwertbereichen (`48 px`) fÃ¼r WinSAT- und Index-Balken (`8 px` HÃ¶he).

## v3.1 (05.10.2026)

* **Vergleichsseite & Deserialisierung:** Fehlerbehebung beim Laden von Systemen in der BenutzeroberflÃ¤che; strukturierte Fehlerbehandlung und Logging im JSON-Parser (`DbEntry.Load`).
* **Eingebettete Referenzdaten:** FÃ¼nf anonyme Referenzprofile (Desktop High-End bis Notebook Standard) direkt im Skript eingebettet; automatisches Entpacken bei leerem Datenbankordner fÃ¼r sofortige Systemvergleiche ab dem Erststart.
* **Prozessor-Benchmark (CPU-Regression behoben):** Verbindlicher Start Ã¼ber Windows PowerShell 5.1; Beseitigung von RyuJIT-Laufzeitverlangsamungen und Sensor-Abfragekollisionen unter Last.
* **Grafik-Benchmark (Robustheit bei niedrigen Bildraten):** Fehlerbehebung beim Rendertest auf Einsteiger- und Mobil-GPUs (z. B. Intel UHD Graphics 620 auf Dell-Notebooks); kein Verwerfen von Messwerten bei aktiver Vorschau; Fallback-Erkennung und erweiterte Direct3D 11 Feature-Levels (11_1, 9_3) fÃ¼r Hybrid-Grafiksysteme.

## v3.0 (05.10.2026)

* **Sicheres Schritt-Ãœberspringen:** Gezieltes Ãœberspringen langwieriger Einzelschritte (`-Skippable`) mit dynamischer Freigabe (`@@SKIP_ALLOWED`) und geordnetem Hintergrund-Job-Abbruch.
* **Frametime-Latenzen:** Erfassung von 0,1 %-Low FPS und Mikroruckler-Anteil (Frames Ã¼ber 50 ms) in Messwerten, Kurventabellen und HTML-Bericht.
* **Benchmark-Detailtabellen:** Bereinigte Darstellung im HTML-Bericht mit bewÃ¤hrter tabellarischer Struktur (Messung, Wert, Index, Referenz, Vergleich, Ergebnis) bei Beibehaltung des Gesamtleistungs-Banners und der Profilkarten.
* **Referenzprofile:** FÃ¼nf bereinigte Referenzsysteme als Standardauswahl; Desktop Mittelklasse dient als Standard-Vergleich.

## v2.95 (05.10.2026)

* **Anonyme Vergleichsdaten fest eingebaut:** FÃ¼nf bereinigte Referenzsysteme von Desktop High-End bis Notebook Standard werden bei der Ersteinrichtung automatisch in die lokale Vergleichsdatenbank kopiert.
* **Gesamtleistungs-Banner:** Prominente Leistungskarte im Header des HTML-Berichts mit Gesamtprozentwert relativ zum Referenzsystem.
* **Benchmark-Darstellung im UserBenchmark-Stil:**
  * Drei Nutzungsprofile (Gaming, BÃ¼ro/Desktop, Workstation) mit gewichteten geometrischen Mittelwerten und BewertungswÃ¶rtern.
  * Prominente Komponenten-KÃ¶pfe in den Benchmark-Details mit genauer Hardwarebezeichnung, Prozentwert und Referenzbalken.
  * Modernes Spalten-Layout fÃ¼r MessgrÃ¶ÃŸen von Prozessor, Arbeitsspeicher und Grafikkarte.
* **Wartung & Bedienung:**
  * Standardauswahl im Modul Wartung auf risikoarme Bereinigungen vorkonfiguriert.
  * Schnellwahlschalter â€žÃœbliche Auswahlâ€œ und â€žAlle abwÃ¤hlenâ€œ direkt Ã¼ber die MaÃŸnahmenliste versetzt und mit Tooltips versehen.
  * Neue SchaltflÃ¤che â€žDiesen Schritt Ã¼berspringenâ€œ wÃ¤hrend des Durchlaufs mit Ereignis `@@SCHRITT_UEBERSPRINGEN`.
* **Minidump Crash Inspector:** BinÃ¤re Erkennung und Analyse von Windows-Absturzabbildern (`.dmp`) inklusive Bugcheck-Mapping und Reparaturempfehlungen in Diagnose, Bericht und KI-Datei.
* **Grafiktreiber-Gesundheitscheck:** ÃœberprÃ¼fung auf Microsoft Basic Display Adapter, Alter des Treibers (> 18 Monate) und TreiberabstÃ¼rze (Event ID 4101) mit DDU-Empfehlung.
* **Durchlaufzeit-Optimierung:** Konsequente Nutzung von Zwischenspeichern (`Get-CimCached`) fÃ¼r unverÃ¤nderliche WMI- und CIM-Systemabfragen.

## v2.9 (05.10.2026)

* **ARM64 & High-DPI:** Hardware-Erkennung mit ARM64-UnterstÃ¼tzung und Warnhinweisen; Per-Monitor V2 DPI-Awareness im Anwendungsmanifest.
* **Modularisierung:** Aufteilung der C#-OberflÃ¤che in spezialisierte Teilklassen fÃ¼r Steuerelemente, Modelle, Vergleich und Seiten.
* **Wartungsmodul:** Reparaturmodul in Wartung umbenannt mit 22 eigenstÃ¤ndigen Wartungsschritten und Bereinigung der Redundanzen im Optimierungskatalog.
* **Preset Leos Empfehlung:** Neues Standard-Preset fÃ¼r Windows-Optimierungen.
* **Neugewichtung:** Doppelte Gewichtung von CPU und GPU sowie dreifache Gewichtung der gemessenen Renderleistung im Gesamtergebnis.
* **Vergleichsseite:** Editierbare Systembezeichnungen und interaktive Spaltensortierung in der Vergleichsdatenbank.

## v2.8 (03.10.2026)

* **Modul Optimierung:** 163 native Einstellungen in 14 Kategorien mit Vorlagen (Minimal, Standard, Erweitert), Wiederherstellungspunkt und RÃ¼ckgÃ¤ngig-Funktion.
* **Sensoren ohne LibreHardwareMonitor:** Fallback auf Windows- und Treiberquellen fÃ¼r GPU-Temperatur, Takt, LÃ¼fter und Auslastung.
* **Berichtsverbesserungen:** Diagramme ohne Ã¼berlappende Texte, WinSAT-Entkopplung von Benchmark-Unterbrechungen, NVMe-KlassenschÃ¤tzung.

## v2.7 (03.10.2026)

* **Bedienung & Barrierefreiheit:** ErklÃ¤rung aller Bedienelemente per Maus-Tooltip.
* **Sensoren im Benchmark:** Durchgehende Aufzeichnung von Temperatur, Takt und Leistungsaufnahme wÃ¤hrend aller Messungen.
* **Datenpflege & Archivierung:** Automatische Archivierung Ã¤lterer oder unvollstÃ¤ndiger LÃ¤ufe; Quell- und Build-Archivierung in `Bauen.cmd`.
* **Versionsansicht:** Neue Informationsseite mit vollstÃ¤ndiger Versionshistorie im Programm.

## v2.67 (03.10.2026)

* **GleichmÃ¤ÃŸige Last im eigenen Prozess:** Auslagerung von CPU- und RAM-Last in einen separaten PowerShell-Hintergrundprozess ohne Unterbrechungen durch Speicherbereinigung.
* **Optimierte Rechenschleifen:** Sinus-Berechnung als Polynom ohne wiederholte Bibliotheksaufrufe.

## v2.66 (03.10.2026)

* **Unterbrechungsmessung:** Protokollierung von Latenzspitzen Ã¼ber 50 ms wÃ¤hrend Last- und Benchmarks.
* **Robustes Sensor-Polling:** Toleranz bei verspÃ¤teten Abfragen und asynchrones Auslesen von DatentrÃ¤gern.
* **Erweiterter KI-Auftrag:** Bereitstellung vollstÃ¤ndiger PowerShell-Befehle und Klickpfade in der Analyse-Datei.

## v2.65 (02.10.2026)

* **Praxistest-Anpassungen:** VollstÃ¤ndige GPU-Messwerte, RAM-Stresstest im Hintergrund, flÃ¼ssige Rendervorschau und Warnung bei aktiver Bildratengrenze.
* **Flexibler Lasttest:** Frei wÃ¤hlbare Dauer ab 2 Minuten und gezielte Auswahl der Grafikeinheit.

## v2.6 (02.10.2026)

* **Direct3D 11 Rendertest:** Eigener hardwarebeschleunigter 3D-Test zur Erfassung realer Bildraten.
* **Schneller Modus:** Parallele AusfÃ¼hrung von RoutineprÃ¼fungen zur drastischen VerkÃ¼rzung der Gesamtlaufzeit.
* **Zero-Footprint & Temp-AusfÃ¼hrung:** VollstÃ¤ndiger Betrieb im temporÃ¤ren Verzeichnis zur Entlastung des USB-Sticks.

## v2.4 (02.10.2026)

* **Hardware-Vertiefung:** Trennung von dedizierter und integrierter Grafikkarte, exakte Taktmessung Ã¼ber PDH, Akkubericht mit VerschleiÃŸanalyse und Standby-Sperre.

## v2.3 (01.10.2026)

* **Sensor-Integration:** Einbindung von LibreHardwareMonitor und PawnIO-Treiber mit SicherheitsprÃ¼fungen und Hash-Validierung.
* **EchtzeitÃ¼berwachung:** Neuer Reiter fÃ¼r Live-Sensordaten und Drosselnachweis unter Volllast.

## v2.2 (01.10.2026)

* **Modulare Architektur:** Quelltextaufteilung (`src/`), ModulvertrÃ¤ge (`Vertrag.psd1`), Risikoklassifizierung aller Aktionen und Pester-Testsuite.
* **ReversibilitÃ¤t:** Strukturiertes Ã„nderungsprotokoll mit Rollback-Funktion.

## v2.0 - v2.1 (01.10.2026)

* **Leos Minibench:** EinfÃ¼hrung des Namens, Single-File-EXE fÃ¼r USB-Betrieb, Diagnose-, Benchmark-, Lasttest- und Reparaturmodule sowie HTML-/KI-Berichte.

## v1.8 (30.09.2026)

* **Gruppierter Benchmark und Referenz:**
  * Der Benchmark ist in vier ausklappbare BlÃ¶cke gegliedert, im Bericht und in der OberflÃ¤che. Laufwerke stehen kompakt in einer Zeile pro Laufwerk. Neue Messungen:
  * CPU: AES-256, SHA-256, Kompression
  * RAM: Kopieren
  * GPU: PCIe-Anbindung, Grafikspeicher
  * Laufwerke: 4K schreiben

## v1.7 (29.09.2026)

* **Benchmark-Korrektur und schlankere Ausgabe:**
  * Fehler behoben, durch den nur die CPU gemessen wurde. Jeder Teil des Benchmarks lÃ¤uft jetzt abgesichert fÃ¼r sich. Die Ausgabe ist entschlackt: Im Ordner liegen nur noch der HTML- und der Textbericht, alles andere steckt in Anhang.zip.

## v1.6 (28.09.2026)

* **Benchmark und Lasttest:**
  * Benchmark fÃ¼r CPU (Einzel- und Mehrkern, Takt), RAM (Lesen, Schreiben, Latenz), GPU (WinSAT) und Laufwerke (sequentiell und 4K, ohne Windows-Cache). Jeder Wert bekommt einen Index fÃ¼r seine Hardwareklasse und wird mit frÃ¼heren LÃ¤ufen verglichen. PCIe-Anbindung von Grafikkarte und NVMe wird geprÃ¼ft. Lasttest mit wÃ¤hlbarer Dauer, Takt- und Temperaturkurve, Drosselungserkennung und Abbruchknopf.

## v1.5 (27.09.2026)

* **Kurztest und neues Design:**
  * Kurztest-Modus, der alles Langwierige Ã¼berspringt. Die OberflÃ¤che ist komplett Ã¼berarbeitet, mit Kacheln, Statuskarten und Reitern, und Ã¶ffnet sich im Vordergrund. Die exe fordert Administratorrechte selbst an. Dazu Korrekturen aus dem Code-Review.

## v1.4 (26.09.2026)

* **OberflÃ¤che und HTML-Bericht:**
  * Optionale grafische OberflÃ¤che mit Live-Befunden, Testergebnissen und Protokoll. Grafischer Endbericht als HTML mit Hell- und Dunkelmodus. Ein Build-Skript kompiliert die exe direkt auf dem Desktop, ganz ohne Download.

## v1.3 (25.09.2026)

* **Absturzsicherheit und Korrekturen:**
  * Checkpoints werden direkt auf die Platte geschrieben, damit sie auch einen Absturz Ã¼berstehen. Nach einem abgebrochenen Lauf startet eine Absturzanalyse: Bluescreen-Stoppcode, Kernel-Power 41, WHEA-Fehler und der Schritt, in dem der PC ausfiel. Neue Option Absturzanalyse (-AnalyzeLastRun). Behoben wurden die Fehler aus dem ersten Praxislauf: Auswertung von SFC, Fehlalarme bei NTFS und Store-Apps, SMART-Meldung, Firmwaretyp und Konsolenkodierung.

## v1.2 (24.09.2026)

* **Kodierung:**
  * Das Skript repariert sich selbst, wenn die UTF-8-Kennung (BOM) beim Kopieren verloren geht. Hashtable-SchlÃ¼ssel mit Umlauten stehen jetzt in AnfÃ¼hrungszeichen.

## v1.1 (23.09.2026)

* **Starten und Fortschritt:**
  * Start-CMD mit MenÃ¼ und eine exe als Starter. Die Administratorrechte holt sich die CMD selbst, und das Fenster bleibt bei Fehlern offen. Klare Fortschrittsbalken fÃ¼r den Gesamtlauf und den einzelnen Schritt.

## v1.0 (22.09.2026)

* **Grundversion:**
  * Diagnoseskript fÃ¼r Windows 11. Es erfasst die Hardware und das System: Firmware, TPM, BitLocker, CPU, RAM-Module, GPU, DatentrÃ¤ger, SMART, Akku, Netzwerk, Treiber, Updates, Sicherheit, Autostart und Software. Dazu kommen Tests: WinSAT, chkdsk-Onlinescan, DISM und SFC, Defender-Schnellscan, SMART-Langtest, RAM-Mustertest, CPU-StabilitÃ¤tstest, Netzwerk- und Energieanalyse sowie die Auswertung der Ereignisprotokolle. Der komplette Bericht landet in der Zwischenablage und als Textdatei auf dem Desktop.
