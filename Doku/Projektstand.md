# Leos Minibench: Projektstand

Gemeinsamer Stand für alle Agenten (Claude, Antigravity). Wird am Ende jeder Version aktualisiert (siehe AGENTS.md).
Seit 4.07 steht hier nur noch, was für die nächste Version gebraucht wird: Stand, Aufbau der Oberfläche, offene Befunde,
abgelehnte Vorschläge, Fallstricke. Was eine Version geändert hat, steht in CHANGELOG.md (kurz), in
Doku\Versionshistorie.txt und in Doku\Änderungen_vX.Y.txt (ausführlich); es wird hier nicht wiederholt.

## Stand: Version 4.13 (21.11.2026)
Release-Commit "Release v4.13: Praxistest vom 20.11.2026: Sensorfehler im Grafiktreiber beendet den Lauf nicht mehr",
unter Windows (ULB-PC10039) mit Bauen.cmd gebaut.

Inhalt von 4.13 (Doku\Änderungen_v4.13.txt): Eine Zugriffsverletzung im AMD-Treiber (LibreHardwareMonitor,
ADL2_Adapter_FrameMetrics_Get) beendete den Arbeitsprozess; jetzt fängt DiagSensors.SafeUpdate sie ab und sperrt das
Gerät. Die Absturzanalyse wertet einen PowerShell-Absturz vor einem späteren Neustart als Prozessabsturz und lässt
keinen Abschnitt mehr aus.

Inhalt von 4.12 (Doku\Änderungen_v4.12.txt): Rückmeldungen aus dem ersten Praxistest. Wartung übernimmt DISM ScanHealth
der Diagnose, ACPI-Thermalzonen mit 5 s Zeitlimit (erste WMI-Abfrage dauerte 102 s), PawnIO still und nur im Abschluss
entfernt, neuer Wartungsschritt AltinstallationPapierkorb (Papierkorb, Windows.old, ESD-Dateien; Wunsch Leonardo: die
Datenträgerbereinigung lässt sie aus, der eigene Schritt löscht sie nach Auswahl).

Inhalt von 4.11 (Einzelheiten in Doku\Änderungen_v4.11.txt): Fehlerbehebungsversion nach der Gesamtprüfung vom
19.11.2026. Alle Befunde der Stufen Dringend bis Messung behoben (PawnIO ohne Zustimmung, Wiederherstellungspunkt,
Datenträgerbereinigung, Computerschutz, smartmontools, Import, Prüfsummen der Optimierungswerkzeuge, Verbrauchsmodell,
Systemvergleich, Rückgängig, Wartung, Hybrid-Erkennung, freier RAM, Oberfläche, Bau und Veröffentlichung).

**Zuerst lesen:** Doku\Pruefung_4.10.md (alle Befunde der Gesamtprüfung mit Fundstelle; Spalte Ziel "offen" sind die
Kandidaten für die nächsten Versionen) und Doku\Ideen.md (gesammelte Ideen, nicht ungefragt bauen). Beide aktuell
halten: behobene Befunde mit Version in die Spalte Stand, gewählte Ideen markieren.

## Aufbau der Oberfläche (src\Oberflaeche, Reihenfolge in Oberflaeche.ps1)
DiagGui.cs                 Rahmen: using-Zeilen für alle Teile, Felder, Start, Seitenleiste, Fußleiste, ShowPage, gemeinsame Bausteine
DiagGui_Darstellung.cs     Farbschema, Titelleiste, native Steuerelemente
DiagGui_Einstellungen.cs   Einstellungsdatei, Dialog Einstellungen, Fenstergröße, Netzlaufwerk-Dialog
DiagGui_Update.cs          Update-Prüfung und Austausch der LMB.exe
DiagGui_Modulseite.cs      gemeinsamer Rahmen der Seiten Diagnose, Benchmark, Lasttest, Sensoren live, System-Umzug (ModulSeite, ZeilenKarte,
                           SchalterZeile) und der Seiten Datenbank und Änderungen (ListenSeite: Kopf, Leiste, Band, Liste, Seitenteil)
DiagGui_Startseite.cs, _Diagnose, _Benchmark, _Lasttest, _Wartung, _Optimierung, _Tools, _Sensoren, _Vergleich (Datenbank),
_Umzug, _Aenderungen, _Versionen   je eine Seite
DiagGui_Ablauf.cs          gewählte Module, Zeitschätzung, Start eines Laufs
DiagGui_Vorlagen.cs        Vorlagen
DiagGui_Lauf.cs            Laufansicht, Ereigniskanal (HandleLine), Abschluss
DiagGui_Ergebnis.cs        Ergebnisseite (ab 4.08 eine Seite hinter Versionen; Eintrag navErgebnis steht außerhalb der Liste nav)
Bewegung.cs                Zeitgeber für Übergänge, RingAnzeige, Ueberblendung, FarbPunkt
Liste.cs                   ZeilenListe (selbst gezeichnete Liste mit Kästchen, Aufklappen, Spaltenkopf, eigenem Bildlauf), KennzahlZeile, DetailKarte (ab 4.10)
DiagGui_Steuerelemente.cs  Klasse UI (Farben, Akzent, Maße) und eigene Steuerelemente
DiagGui_Modelle.cs, Start.cs, Versionen.cs   Datenmodelle, Programmsymbol und Startprotokoll, Versionshistorie
Seiten und Einträge der Seitenleiste teilen sich die Stellen 0 bis 10 (nav[i] gehört zu pages[i], 1 bis 5 sind die
Module), danach folgen Versionen (versionPage) und Ergebnis (ergebnisPage). Viele Stellen sind fest im Code (ShowPage(2),
nav[1].Checked): neue Seiten hinten anhängen, nie dazwischen.
Werte hinter Schaltern: benchChk, diagChk, repChk, chkFast, chkInstall, chkMem, chkLCpu bis chkLDisk und rbVoll,
rbSchnell, rbCustom werden nicht angezeigt, tragen aber die Auswahl für Vorlagen, Zeitschätzung und Start. Sichtbar sind
SettingRow und FeldAuswahl. Die Seite System-Umzug hält ihre Werte als lokale Kontrollkästchen in BuildUmzugPage.
Seiten mit dem gemeinsamen Rahmen stehen in IstModulSeite (1, 2, 3, 7, 8): ShowPage und die Größenänderung ordnen nur sie.
Listen der Seiten Sensoren, Datenbank und Änderungen: Die Seite baut bei jeder Änderung die sichtbaren Zeilen neu
(FillSensList, FillDbList, FillChangeList) und zeichnet sie über den Zeichner der Liste; Zustand (Haken, aufgeklappt,
Markierung) hält die Seite in eigenen Mengen (chgHaken, chgOffen, dbHaken, expandedGroups, sensZu), nicht in der Zeile.
cmbDriver und cmbKeep (Seite Sensoren) werden nicht angezeigt, tragen aber die Werte hinter den beiden Schaltern.
Neue Seite: eigene Datei DiagGui_<Name>.cs ohne using-Zeilen, in Oberflaeche.ps1 eintragen. Tests suchen im Gesamttext
(Get-GuiText in tests\Hilfen.ps1), nicht in einer bestimmten Datei.

## Versionen bisher (je eine Zeile, neueste zuerst)
4.13 Sensorfehler im Grafiktreiber beendet den Lauf nicht mehr, Absturzanalyse erkennt Prozessabstürze.
4.12 Praxistest 4.11: kürzere Läufe, PawnIO still, eigener Schritt für Windows.old (c4a9829). 4.11 Sicherheit und Rücknahme: Befunde der Gesamtprüfung behoben (594bd08, nicht veröffentlicht). 4.10 Sensoren, Datenbank und Änderungen im Stil der übrigen Seiten (05ca12f).
4.09 Lasttest und System-Umzug im Stil der übrigen Seiten, Schalter ohne Beschnitt (34f5ef3).
4.08 Ergebnis als Seite, Benchmark und Diagnose im Stil der übrigen Seiten (c9a16bb). 4.07 Eine Datei je Seite,
kürzerer Start, Akzentfarbe, dezente Bewegung (ba3489b). 4.06 Update-Prüfung und Referenz
repariert (53a4d04). 4.05 Bereits aktive Optimierungen gesperrt, Tools-Seite kompakt,
28 Softwarepakete (ebb4f81). 4.04 Veröffentlichen in einem Schritt, Referenz über die Datenbank (2a5cb46). 4.03
Update-Funktion, SMART-Langtest, Datenbank (598b8b1). 4.02 PawnIO-Entfernung, plausiblere Messwerte, Einstellungen auf
einer Seite (2d8cff1). 4.01 Politur: Ergebnisseite, Einstellungen, Startzeit, Skalierung (a77b64f). 4.0 Aufgeräumt
(dd8f43f; Doku\Änderungen_v4.0.txt nennt einige Punkte, die erst 4.01 umgesetzt hat). 3.91 Ergebnis-Dashboard
(e7d9a18). 3.9 Master-Detail, Schalter, Ergebnisseite (0dfbb40). 3.88 bis 3.882 Datenbank-Gruppen (16a08c2, c7358e0,
b582625). 3.84 bis 3.87 GitHub, Sensoren, Update-Prüfung, Läufe je Gerät. 3.8 bis 3.83 System-Umzug. 3.7 bis 3.73
Startseite, Konfigurationsprüfung, Dashboard in Bausteinen, LMB.exe. 3.64 bis 3.67 Gestaltungsregeln der Oberfläche.

## Offene Befunde
Weitere offene Befunde aus der Gesamtprüfung vom 19.11.2026: Doku\Pruefung_4.10.md (Ziel offen, G6 bis G11 aus 4.11).
1. 4.10 unter Windows bestätigen: Seiten Sensoren live, Datenbank und Änderungen bei 100, 125 und 150 %, Mausrad und
   Tastatur in den Listen, Kontextmenü der Datenbank, Bildlaufleiste des Detailbereichs im dunklen Schema, Live-Ansicht
   mit echten Sensoren (Kennzahlen, Liste, volle Aufzeichnung), "Aufräumen …" mit anschließendem Abgleich, Wartezeit der
   Seite Änderungen mit dem echten Protokoll (Ursache aus dem Quelltext abgeleitet, nicht gemessen), Sperre der
   Optimierung nach Rückgängig.
2. 4.09 unter Windows bestätigen: Seiten Lasttest und System-Umzug bei 100, 125 und 150 %, die acht neuen Symbole der
   Seite System-Umzug (UI.IcoBildschirm bis UI.IcoWlan, in der Sandbox nur Ersatzzeichen), Schnell-Scan und eine
   Sicherung mit der neuen Seite, Schalter ohne Beschnitt auf allen Seiten und im Dialog Einstellungen.
3. Von 4.08 unter Windows bestätigt (Aufnahme Leonardo, 13.11.2026): Eintrag "Ergebnis" mit Punktzahl, Titelzeile 4.08.
   Noch offen: Seiten Benchmark und Diagnose bei 125 und 150 %, Kopfkarte der Ergebnisseite ohne Leerraum, Band auf
   der Seite Benchmark, Hinweis im Startfenster bei abweichendem Skript, freier Platz in den Laufwerkszeilen.
4. Startzeit: Start.log vom 13.11.2026 (4.08, ULB-PC10039): 4,0 bis 26,2 s je Start, Hardwareabfrage 0,8 bis 1,4 s, Seiten
   zusammen rund 0,7 s (Lasttest 170 ms, Versionen 108 ms, Benchmark 90 ms). "Datenpflege: unverändert" kam nie; die
   Ursache ist in 4.09 behoben und mit Tests belegt. Start.log vom 13.11.2026 enthält erst einen Start mit 4.09
   ("nichts zu archivieren"), der Beleg unter Windows bleibt der zweite Start. Versionen baut seit 4.10 beim Start
   nur noch Kopf und Schalter auf.
5. Größter verbleibender Anteil am Start: powershell.exe und das Einlesen des Gesamtskripts (2,6 MB; Stick 1,0 s,
   ULB-PC10039 2,7 s). Abhilfe wäre ein getrennter kleiner Startteil für die Oberfläche (Umbau von Zusammenbau,
   Launcher und $PSCommandPath der Arbeitsprozesse), nicht umgesetzt.
6. System-Umzug: Der Schnell-Scan misst im Vordergrund (Fenster reagiert so lange nicht); Layout, Software-Inventar und
   Profile gehen mit festen Schätzwerten in die Gesamtgröße ein.
7. Ergebnisseite, zweite Stufe: Auswahlfeld oben rechts für frühere Läufe dieses Geräts (ErgebnisAusEintrag kann jeden
   Eintrag zeigen). Nach einem Lauf ist die Laufansicht (Tests, Leistung, Protokoll) nicht mehr erreichbar.
8. Kategorie Apps der Optimierung nutzt dasselbe Symbol wie Tools (UI.IcoApp = UI.IcoTools).
9. Rendertest auf sehr schnellen Karten (RTX 3070): hoher Mikroruckler-Anteil und niedriges 1-%-Low nur im Benchmark.
10. Unter Windows noch nicht bestätigt: smartctl -X und PCIe-Abfrage unter Last (4.03), winget export als Administrator,
   GPU-Z über den Uninstall-Eintrag, Menü "…" im dunklen Farbschema (4.05), "Jetzt suchen" hinter dem Proxy, "Median
   aktualisieren" mit Meldung (4.06), Akzentfarbe "wie Windows", Dialog Einstellungen bei 125 und 150 % (4.07).
11. Archivierte Änderungsprotokolle (Archiv\Änderungen) lassen sich nur von Hand zurücklegen.
12. README zeigt noch Aufnahmen älterer Versionen (Doku\Screenshots); echte Windows-Aufnahmen von 4.0x fehlen.
13. Akzentfarbe gilt nicht im HTML-Bericht und im Dashboard; Wartung sperrt "Schnellstart deaktivieren" nicht als
    "bereits aktiv" wie die Optimierung.
14. Am 15.11.2026 vorgeschlagen und von Leonardo nicht gewählt (nicht abgelehnt, nicht ungefragt bauen): "Ergebnis
    ansehen" für jeden Lauf der Datenbank, Veränderung zum vorigen Lauf, Suchfelder und Filter in Sensorliste, Datenbank
    und Änderungen, wählbares Zeitfenster der Kurven.
15. 4.11 unter Windows von Hand bestätigen: siehe Grenzen in Doku\Änderungen_v4.11.txt (Hintergrundsperre, Update
   mit Fortschritt, Tastenkürzel, Computerschutz mit Administratorrechten, DDU holen).
16. 4.12 von Hand bestätigen: Wartungsschritt AltinstallationPapierkorb auf einem PC mit Windows.old; Lauf mit Diagnose
    (Integrität) und Wartung DismRestore ohne zweiten ScanHealth; keine Pause vor dem CPU-Benchmark.
17. 4.13 auf dem PC des Praxistests vom 20.11.2026 (Radeon RX 9060 XT) bestätigen: Lauf mit Benchmark endet mit Bericht,
    Befund "Sensoren von ... abgeschaltet" falls der Treiberfehler wieder auftritt. Neuere LibreHardwareMonitor-Version
    prüfen, ob sie FrameMetrics auf RDNA 4 meidet.

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
13. Ein selbst gezeichnetes Element nie genau so groß machen wie seine Form: Geglättete Kontur und Fokusrahmen brauchen
    Rand im Element (ToggleSwitch.Rand, bis 4.08 rechts und unten abgeschnitten).
14. Lage aus dem gemessenen Text setzen (GetPreferredSize), nicht aus der Größe eines Felds mit AutoSize: Die stellt
    sich erst beim nächsten Layout ein (Kopf der Modulseiten, Lücke rechts bis 4.08).
15. Ein Stand, der entscheidet, ob eine Arbeit entfallen kann, darf nichts zählen, was diese Arbeit selbst verändert
    und der Start wieder herstellt (Datenpflege und leerer Ordner Laufzeit\<PC>, 4.07 und 4.08).
16. Die Oberfläche nennt keine angenommenen Werte: kein fester freier Platz für ein Ziel, das nicht abgefragt wird,
    kein "ausreichend" ohne Messung (Vorgabe 11 gilt auch für die Seite System-Umzug).

17. Eine Zeile mit Kontrollkästchen je Datensatz in einem nativen Listenfeld meldet ihren Haken beim Einfügen und beim
    Anlegen des Fensters: Wer dort alle Haken zählt, zählt quadratisch (Seite Änderungen bis 4.09). Für lange Listen
    ZeilenListe nehmen und Zustand in eigenen Mengen halten.
18. TextRenderer.DrawText beachtet keine Verschiebung des Graphics-Objekts: Selbst gezeichneter Inhalt in einem Bereich
    mit Bildlauf bekommt die Bildlaufposition als Versatz (DbDetailInhalt), Kindelemente stehen um sie versetzt.
19. Ein Protokoll anderer PCs ist auf diesem PC nicht zurücknehmbar, auf seinem PC aber schon: Der Datenordner wandert
    mit dem Stick. Aufräumen wählt solche Dateien deshalb nie von selbst.
20. Die Sandbox-Fotos brauchen für Seiten mit laufender Live-Ansicht liveRunning = false vor dem Schließen, sonst wartet
    OnClosing vier Minuten auf den Arbeitsprozess.
21. Die erste Abfrage einer WMI-Leistungsklasse (Win32_PerfFormatted*) kann minutenlang hängen, wenn WMI die Zählerliste
    neu aufbaut. Neue Abfragen dieser Art mit Zeitlimit (Muster Get-ThermalZoneData, Get-StorageTempReadings) oder über PDH.
22. Aufrufe in native Treiber (LibreHardwareMonitor, P/Invoke) können eine AccessViolationException auslösen, die ein
    gewöhnliches catch in .NET 4 nicht erreicht: Der Prozess endet mit 0xE0434352. Solche Aufrufe in eine Methode mit
    [HandleProcessCorruptedStateExceptions, SecurityCritical] legen (Muster DiagSensors.SafeUpdate).

## Letzter Commit
Release-Commit v4.13 (lokal von Bauen.cmd, Tag und Übertragung mit Veroeffentlichen.ps1). Vorgänger:
c4a9829 Release v4.12: Praxistest 4.11: kürzere Läufe, PawnIO still, eigener Schritt für Windows.old,
594bd08 Release v4.11: Sicherheit und Rücknahme: Befunde der Gesamtprüfung behoben (nicht veröffentlicht),
05ca12f Release v4.10: Sensoren, Datenbank und Änderungen im Stil der übrigen Seiten, Protokoll aufräumen
