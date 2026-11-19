# Gesamtprüfung Stand 4.10 (19.11.2026)

Ganzheitliche Prüfung von Kern, Modulen, Berichten, Oberfläche, Bau und Release, angefordert von Leonardo am
19.11.2026. Grundlage: Commit 05ca12f (Release v4.10). Testlauf unter Windows (ULB-PC10039, Pester 6.2, 320 s):
953 bestanden, 1 fehlgeschlagen, 116 übersprungen, 1 ohne Ergebnis (Gesamtläufe nicht dabei).

Spalte "Ziel": 4.11 = in Version 4.11 zu beheben (Auftrag Leonardo, 19.11.2026), offen = für später notiert.
Wer einen Punkt behebt, trägt die Version in die Spalte Stand ein. Ideen stehen getrennt in Doku\Ideen.md.

## A. Dringend (Schaden auf fremden PCs, Verstoß gegen feste Vorgaben)

| Nr | Ziel | Stand | Fundstelle | Befund |
|----|------|-------|------------|--------|
| A1 | 4.11 | 4.11 | Kern\Sensoren.ps1:303 | PawnIO wird trotz Wahl "nein" installiert: Bedingung `$Treiber -or $isAdmin`, Arbeitsprozesse laufen immer als Administrator. Kopfkommentar sagt "nie ungefragt". |
| A2 | 4.11 | 4.11 | Module\Wartung\Ablauf.ps1:375-381, Optimierung\Ablauf.ps1:69 | "Alte Wiederherstellungspunkte löschen" (`vssadmin delete shadows /all`) läuft zuletzt und löscht den eben angelegten Punkt des Laufs; RestorePointNo bleibt gesetzt, Optimierung legt danach keinen neuen an. Protokoll nennt trotzdem "rstrui.exe, Punkt Nr. X". |
| A3 | 4.11 | 4.11 | Module\Wartung\Ablauf.ps1:295-297, Optimierung\Funktionen.ps1:632 | Datenträgerbereinigung setzt StateFlags für alle Kategorien außer DownloadsFolder: Papierkorb, Windows.old (Previous Installations), Windows ESD installation files. Nur Risiko Eingriff/Ändern, als Aufgabe alle 30 Tage. |
| A4 | 4.11 | 4.11 | Kern\Aenderungen.ps1:227, Module\Wartung\Ablauf.ps1:65 | `Enable-ComputerRestore` schaltet Computerschutz ohne Vorher-Zustand, Protokoll und Gegenbefehl ein (Vorgabe 3). |
| A5 | 4.11 | 4.11 | Kern\Smartmontools.ps1:30-34 (auch $stillThere) | `@(...) \| Where-Object` ohne äußeres `@()`: bei einem Treffer ist `$candidates[0]` das Zeichen "C", der NSIS-Deinstaller läuft nie (in PS 5.1 nachgestellt). Rückstand auf fremden PCs (Vorgabe 4). |
| A6 | 4.11 | 4.11 | Kern\Datenbank.ps1:214-226 | Import .minibench: Zielordner aus Datenbank.json.Ordner ungeprüft. `Berichte\..` ergibt Leaf `..`, Copy-Item -Force überschreibt Netzwerk.json, Einstellungen.json, Referenz.json. Vorhandener Berichtsordner wird ohne Archiv überschrieben (Vorgabe 14). |
| A7 | 4.11 | 4.11 | Optimierung\Funktionen.ps1:29, 675, 701-705, 727-733, 745-753 | NVIDIA Profile Inspector "releases/latest" ohne Hash, DDU per winget download oder beliebige DDU.zip, vorgefundene exe ungeprüft aufgenommen (Vertrauen beim ersten Fund). Läuft als Administrator (Vorgabe 5). |
| A8 | 4.11 | 4.11 | Kern\Berichtshilfen.ps1:707-745, Kern\Grundgeruest.ps1:22, Bericht\HtmlBericht.ps1:317, 356-397, Dashboard daten.js:773-782 | Verbrauchsmodell mit festen Annahmen (GPU-Leerlauf 12 W, CPU-Leerlauf 18 %, Teillast +20 W, Gaming min(0,75 x Max, 65 W)); Warnung "Netzteil zu X % ausgelastet" rechnet mit Vorgabe 650 W ohne Eingabe (Vorgabe 11). |
| A9 | 4.11 | 4.11 | Bericht\Bausteine_Systemvergleich.ps1:404-425, 585; Kern\Datenbank.ps1:26-37; HtmlBericht.ps1:784 | "Seit dem letzten Lauf": DB-Einträge ohne WinBuild, Ersatz `$prev.System` ergibt "Build Dell Inc. OptiPlex 7090 -> Build 26100" bei jedem Lauf. Texte (auch BIOS, Treiber) und Referenzname unmaskiert im HTML (Vorgabe 12). |

## B. Bau und Release

| Nr | Ziel | Stand | Fundstelle | Befund |
|----|------|-------|------------|--------|
| B1 | 4.11 | 4.11 | tests\Testen.ps1:45 | Exit-Code nur aus FailedCount: Ladefehler einer Testdatei (FailedContainers) oder Abbruch von Invoke-Pester ($res = $null) ergeben Exit 0, Bauen.cmd committet (mit Pester 6.2 nachgestellt). |
| B2 | 4.11 | 4.11 | Aktueller Build\Stand.txt (4.01 bis 4.10) | Seit 4.01 kein Release unter Windows mit Bauen.cmd gebaut; Stand.txt nennt Sandbox mit 51 bis 52 Fehlschlägen; Gesamtläufe liefen nie. 4.11 wird unter Windows gebaut. |
| B3 | 4.11 | 4.11 | Veroeffentlichen.ps1:24-38 | Prüft nur Existenz der exe und Versionszeile; nicht SHA der exe gegen Stand.txt, nicht ob Tag auf HEAD zeigt, nicht ob Zweig main ist. Alte exe kann als neue Version hinausgehen. |
| B4 | 4.11 | 4.11 | origin | Tag v4.08 fehlt auf GitHub (lokal vorhanden). |
| B5 | 4.11 | 4.11 | Bauen.cmd:396-413 | Kein Archiv\v4.x seit 4.0 (Build in git, Bundle ersetzt Stand.txt). Austausch nicht atomar; Zeile 404 löscht die aktuelle Datei, wenn die Archivdatei schon existiert. |
| B6 | 4.11 | 4.11 | fehlende .gitattributes | Zeilenenden nur über core.autocrlf; SHA der .cmd in Stand.txt passt unter Windows nicht. |
| B7 | 4.11 | 4.11 | GuiSelbsttest.cs:541, tests\Daten\Datenbank\TORRENT_*.json:103, Bauen.cmd:425 | Persönliche Angaben im öffentlichen Repo: UNC-Pfad mit Uni-Kennung, C:\Users\fried, Rechnername in Stand.txt. (Git-Historie behält sie.) |
| B8 | 4.11 | 4.11 | Bauen.cmd:358, 460/476, 486-488, 496 | Doppelt kodiertes "GesamtlÃ¤ufe", doppelter Schritt 5, Kommentar "git push" falsch, Aktualisiere-Berichte.ps1 fehlt in der Commit-Liste. |
| B9 | 4.11 | 4.11 | tests\Datenbank.Tests.ps1:399 | Test "ohne gespeicherte Referenz gilt Desktop-Median" hängt an der echten Bauform (scheitert auf Notebooks); Attrappe für Test-IsNotebookSystem fehlt. |
| B10 | offen | | tests\Aufbau.Tests.ps1:29-36 | "Aktueller Build entspricht src" endet nur als Inconclusive, nie als Fehler. |
| B11 | offen | | Sandbox | 51 Dauer-Fehlschläge in der Sandbox ohne Liste; neue Regressionen fallen nicht auf (siehe Ideen). |
| B12 | offen | | Tests | Keine Verhaltenstests für Remove-OldFiles, Clear-Folder (Wartung), Copy-UmzugTree, Test-UmzugTreeDiff, Test-UmzugSha256Samples, Read/Save-ToolManifest, Get-StepSchedule, Test-StepReady, Test-AbgleichMassenloeschung. |
| B13 | offen | | Tests | Testregel 2 (bekannte Fehler als -Skip-Test) wird nirgends angewandt. |

## C. Oberfläche

| Nr | Ziel | Stand | Fundstelle | Befund |
|----|------|-------|------------|--------|
| C1 | 4.11 | 4.11 | DiagGui_Ablauf.cs:107-111, DiagGui_Tools.cs:619-690, DiagGui_Umzug.cs:688-846, DiagGui_Lauf.cs:583-594 | Umzug-Sicherung und winget-Installation laufen ohne Zustand: Benchmark kann parallel starten (Vorgabe 7), Schließen ohne Warnung, powershell.exe läuft unsichtbar weiter; Umzug WaitForExit ohne Frist. |
| C2 | 4.11 | 4.11 | DiagGui_Update.cs:279-297, 246 | Ohne bekannte Prüfsumme wird die geladene LMB.exe ungeprüft eingesetzt, ohne Warnung in der Rückfrage. |
| C3 | 4.11 | 4.11 | DiagGui_Update.cs:261-265 | WebClient.DownloadFile synchron im Oberflächen-Thread, bis 100 s "Keine Rückmeldung". |
| C4 | 4.11 | 4.11 | DiagGui_Sensoren.cs:515-532 (RunHelper) | ReadToEnd vor WaitForExit(120000): Frist wirkt nie. Aufrufer im UI-Thread: TryAutoFetchSensorTools (Live-Ansicht, Laufstart), Dashboard, Verlauf, Basis, Datenpflege, Import, Export. |
| C5 | 4.11 | 4.11 | DiagGui.cs:460-477 | F5 und Strg+Eingabe in OnKeyDown ohne KeyPreview/ProcessCmdKey: wirkungslos, sobald ein Kind den Fokus hat. |
| C6 | 4.11 | 4.11 | DiagGui_Ergebnis.cs:359 | `{0:mm\:ss}` auf TimeSpan zeigt nur Minuten 0 bis 59. |
| C7 | 4.11 | 4.11 | DiagGui_Startseite.cs:54, DiagGui.cs:521 | Startseite baut bei jedem ReloadDb neu ohne Dispose der alten Elemente: Handles wachsen. |
| C8 | offen | | DiagGui_Aenderungen.cs:293, DiagGui_Vergleich.cs:206, 844, 918, 942, 972 | Alle Pfade/Ids in einer Befehlszeile, Grenze 32.767 Zeichen (viele Einträge rückgängig, großes Dashboard). |
| C9 | offen | | DiagGui_Sensoren.cs:451-459 | StopLiveAndWait mit Kill: liveRunning bleibt wahr, Abschluss der Live-Ansicht kommt nach Laufstart. |
| C10 | offen | | DiagGui_Steuerelemente.cs:2554, DiagGui_Lauf.cs:287, 370 | Marquee der FlatBar mit eigenem 25-ms-Zeitgeber, ohne Bewegung.Erlaubt, auch während Messungen. |
| C11 | offen | | DiagGui_Update.cs:348-370, DiagGui_Lauf.cs:586-594 | Update.cmd wartet nur rund 90 s; Close kann bis 240 s (Live) plus 300 s (Treiber) dauern. Abbruch zwischen move und copy lässt keine LMB.exe zurück. |
| C12 | offen | | DiagGui_Sensoren.cs:338, 269-277 | Sensorliste ohne Tastaturbedienung (TabStop false, Zeilen nicht wählbar). |
| C13 | offen | | DiagGui_Sensoren.cs:646, 655 | Hinweis "Verworfen: …" bleibt nach späterem gültigem Wert stehen. |
| C14 | offen | | DiagGui_Vergleich.cs:65-70, 93-94, 279-280 | expandedGroups behält Schlüssel entfernter Geräte: "Alle aufklappen/einklappen" falsch. |
| C15 | offen | | Liste.cs:366 | `Region alt = g.Clip` ohne Dispose bei jedem Zeichnen. |
| C16 | offen | | DiagGui_Vergleich.cs:120, 1089 | PromptInput und ImportRuns: Dialog und Schrift ohne Dispose. |
| C17 | offen | | DiagGui_Lauf.cs:503, DiagGui_Startseite.cs:1429 | Feste Farbwerte statt UI.Warn und UI.Info. |

## D. Rückgängig und Änderungsprotokoll

| Nr | Ziel | Stand | Fundstelle | Befund |
|----|------|-------|------------|--------|
| D1 | 4.11 | 4.11 | Kern\Aenderungen.ps1:301-303, 338, 348 | Fehlgeschlagenes Rückgängig setzt Status "fehlgeschlagen" endgültig; kein erneuter Versuch, Aufräumen archiviert es als erledigt. |
| D2 | 4.11 | 4.11 | Kern\Aenderungen.ps1:93, 193 | Get-Service StartType kennt unter PS 5.1 keinen verzögerten Start: Rückgängig setzt "Automatic" statt "verzögert" (z. B. MapsBroker). |
| D3 | 4.11 | 4.11 | Kern\Aenderungen.ps1:199-200 | Energiepläne: Rücknahme überspringt Pläne, deren GUID noch existiert; angepasste Standardpläne bleiben auf Werkseinstellung. |
| D4 | 4.11 | 4.11 | Kern\Aenderungen.ps1:87, 187; Optimierung\Funktionen.ps1:305-312 | Selbst angelegte Registry-Schlüssel bleiben nach Rückgängig leer stehen. |
| D5 | 4.11 | 4.11 | Kern\Aenderungen.ps1:251, 339 | Zuordnung über Computernamen statt GeraetId: Nach Umbenennen archiviert "Aufräumen: Andere" aktive Protokolle; Restore-TemporaryChanges findet sie nicht. |
| D6 | offen | | Kern\Aenderungen.ps1:34-35 | Dateiname des Protokolls ohne PID: zwei Prozesse in derselben Sekunde überschreiben sich. |
| D7 | offen | | Kern\Aenderungen.ps1:293 | StartsWith-Pfadprüfung ohne abschließendes Trennzeichen (Änderungen_alt gilt als innen). |

## E. Wartung

| Nr | Ziel | Stand | Fundstelle | Befund |
|----|------|-------|------------|--------|
| E1 | 4.11 | 4.11 | Module\Wartung\Ablauf.ps1:140-230, 260-268 | Ohne Eintrag im Änderungsprotokoll: Temp, Druckaufträge, Komponentenbereinigung, Dateisystem (fsutil dirty set), Netzwerk, WMI, Geräte, DISM, SFC (Vorgabe 3). |
| E2 | 4.11 | 4.11 | Module\Wartung\Ablauf.ps1:200 | `netsh int ip reset` ohne Sicherung: feste IP und DNS gehen verloren. |
| E3 | 4.11 | 4.11 | Module\Wartung\Ablauf.ps1:327 | ShaderCache leert ganzen Ordner %LOCALAPPDATA%\AMD (auch Radeon-Einstellungen). |
| E4 | 4.11 | 4.11 | Module\Wartung\Ablauf.ps1:324-328, 348 | Bereinigt Profil des Admin-Kontos ($env:LOCALAPPDATA) statt des angemeldeten Benutzers (Get-OptTargetUser). |
| E5 | offen | | Wartung\Vertrag.psd1:79-84, 58 | PaketCache und Schattenkopien löschen nur als Eingriff eingestuft (Package Cache bricht MSI-Reparatur). |

## F. Messung und Auswertung

| Nr | Ziel | Stand | Fundstelle | Befund |
|----|------|-------|------------|--------|
| F1 | 4.11 | 4.11 | Module\Benchmark\Ablauf.ps1:205-225 | Hybrid-Erkennung per Name verfehlt i5-1335U, i7-1255U, i7-1360P; i5-12400 gilt fälschlich als hybrid. Falsche Warnung "CPU skaliert schlecht". |
| F2 | 4.11 | 4.11 | Module\Lasttest\Ablauf.ps1:85-86, Diagnose\Ablauf.ps1:941-943 | Freier RAM aus Get-CimCached vom Diagnosebeginn: Test kann mehr belegen als frei ist. |
| F3 | 4.11 | 4.11 | Kern\Referenz_Vergleich.ps1:221-232, 57 ff. | Get-DbEntries übernimmt Sensoren und Lasttest nicht: "Median aktualisieren" schreibt nie Lasttest-Mediane. |
| F4 | offen | | Bericht\Bausteine_Systemvergleich.ps1:198, 220-221 | Fehlt ein Wert in der Referenz, dient der Bestwert der Auswahl als Bezug, Überschrift sagt trotzdem "% der Referenz" (Vorgabe 18). |
| F5 | offen | | Bericht\HtmlBericht.ps1:695-696 | FPS-Kachel liest "1.234 Bilder/s" als 1,234. |
| F6 | offen | | Bericht\HtmlBericht.ps1:705-711 | Kachel "Schnellste SSD" zeigt auch HDD und USB. |

## G. Kern, weitere

| Nr | Ziel | Stand | Fundstelle | Befund |
|----|------|-------|------------|--------|
| G1 | offen | | Kern\Berichtshilfen.ps1:157-174 | Invoke-External beendet bei Frist nur den Hauptprozess; .Result wartet auf Kindprozesse (winget-Installer). |
| G2 | offen | | Kern\Softwarepakete.ps1:61 | Erkennung über Text von `winget list`; lange Ids werden gekürzt, Paket gilt als nicht installiert. |
| G3 | offen | | Kern\Datenpflege.ps1:215-223 | laufend.json älter als 7 Tage wird gelöscht statt archiviert: Absturzspur geht verloren. |
| G4 | offen | | Kern\Ablage.ps1:328 | Wettlauf beim Übernehmen einer verwaisten Abgleichsperre. |
| G5 | offen | | Ablauf\Abschluss.ps1:202, 244, 251 | Laufmarke wird vor Bericht entfernt; Abbruch dazwischen macht Ordner "Unvollständig" und Eintrag "Verwaist". |
| G6 | offen | | Kern\Berichtshilfen.ps1 (Effizienz) | Effizienz-Indizes rechnen intern weiter mit angenommenem GPU-Leerlauf und Gaming-Anteil (in 4.11 bewusst belassen, damit gespeicherte Werte vergleichbar bleiben; Vorgabe 11 und 9 mit Leonardo klären). |
| G7 | offen | | Module\Wartung\Ablauf.ps1 (Save-WartungNetzwerkSicherung) | netsh-Sicherung als ANSI; Adapternamen mit Sonderzeichen beim Einspielen mit netsh -f ungeprüft. |
| G8 | offen | | DiagGui_Sensoren.cs (RunHelperPumped) | Fenster bleibt gesperrt, solange ein Hilfsablauf läuft; Export oder Import über 600 s bricht ab. |
| G9 | offen | | Optimierung\Funktionen.ps1 (Install-OptDduFromDir) | Entpacken des DDU-Selbstentpackers nur mit Attrappen geprüft; Echttest "Werkzeuge holen" fehlt. |
| G10 | offen | | Kern\Aenderungen.ps1 (Enable-ComputerschutzMitProtokoll) | Lesen von SPP\Clients mit Administratorrechten unter Windows bestätigen. |
| G11 | offen | | DiagGui_Update.cs | Update-Download ohne Zeitlimit (Abbruch nur per Klick oder Schließen). |

## Ohne Befund geprüft
Ereigniskanal (Maskierung von | und Zeilenumbruch, jede Art ausgewertet), Threadzugriffe über BeginInvoke, alle
`new Form()` mit UI.ThemeDialog, kein Parse ohne InvariantCulture, Dateigrößen (nur DiagGui_Steuerelemente.cs über
2000), Bewegung.cs, Testregel 3 (Pester-6-Vorgabe-Attrappen), LMB.exe passt zur LeosMinibench.ps1 von 4.10,
Versionsangaben 4.10 überall gleich, Minibench-Daten und Netzwerk.json nicht im Repo.
