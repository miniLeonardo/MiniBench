# Ideensammlung

Gesammelte Vorschläge, die weder abgelehnt (Doku\Projektstand.md, Abgelehnte Vorschläge) noch schon umgesetzt sind.
Nichts davon ungefragt bauen; Leonardo wählt aus. Gewählte Ideen mit Version markieren, abgelehnte nach
Projektstand.md (Abgelehnte Vorschläge) verschieben.
Seit 21.11.2026 stehen alle Ideen auch als Karten im Nextcloud-Deck (Board "PC DiagnoseBench", gleiche Nummer im
Titel). Datei und Deck parallel pflegen, Ablauf in AGENTS.md (Planung im Nextcloud-Deck).

## Aus der Gesamtprüfung vom 19.11.2026

### Robustheit
1. Gemeinsame atomare JSON-Schreibfunktion (temporäre Datei, dann File.Replace) für Datenbank, Geraete.json,
   Tools.json, Netzwerk.json, Stand.json und Änderungsprotokolle. Heute gemischt: WriteAllText direkt oder Copy über
   das Ziel.
2. Invoke-External mit Job Object, damit alle Kindprozesse (winget, PawnIO-Setup, DISM, SFC) mit der Frist enden
   (behebt zugleich Befund G1 in Doku\Pruefung_4.10.md).
3. Rücknahme-Vertragstest: Jede -Art aus Add-ChangeRecord, die kein Hinweis ist, braucht einen Zweig in
   Undo-ChangeRecord oder Undo-OptChange.
4. Aufbau-Test gegen die Einzelobjekt-Falle von PS 5.1: `$x = ... | Where-Object` ohne @() gefolgt von `$x[0]` oder
   `.Count` (hätte Befund A5 gefunden).
5. Import gehärtet: Duplikatprüfung zusätzlich über GeraetId plus Datum. **Umgesetzt in 4.14.**
6. Änderungsprotokoll mit Status "erneut versuchen" statt endgültigem "fehlgeschlagen" (Teil davon in 4.11).

### Bau, Test, Release
7. Vorabprüfung in Veroeffentlichen.ps1: exe-SHA gleich Stand.txt gleich eingebettete .ps1, Tag auf HEAD, Zweig main,
   Windows-Testzeile mit 0 Fehlschlägen (Teil davon in 4.11). **Umgesetzt in 4.14.**
8. Liste bekannter Sandbox-Abweichungen (tests\Sandbox\Erwartet.txt): Sandbox_Testen.ps1 meldet nur Abweichungen und
   endet sonst mit 0.
9. Bauen.cmd schreibt neue Dateien zuerst in einen Zwischenordner und tauscht am Ende per Umbenennen.
10. Test in Release.Tests: Zu jeder Version in Versionen.cs ab 3.83 existiert ein git-Tag.

### Bedienung
11. "Was ist neu" in der Update-Rückfrage: AvailableUpdateBody wird geholt, aber nirgends gezeigt. **Umgesetzt in 4.14.**
12. Hinweis am Startknopf statt stiller Sperre, solange Sicherung oder Installation im Hintergrund läuft
    ("Start nach Ende der Sicherung möglich"). **Umgesetzt in 4.14.**
13. Live-Ansicht: "Min/Max zurücksetzen" an den Kennzahlen, neuer Messabschnitt ohne Neustart samt PawnIO.
14. Kennzahlkachel der Seite Sensoren live anklickbar: hebt die zugehörige Kurve im Verlauf hervor. **Umgesetzt in 4.14.**
15. Tastatur in der Datenbank: Entf für "Lauf entfernen …" (mit Rückfrage), Leertaste hakt alle markierten Zeilen an. **Umgesetzt in 4.14.**

### Daten und Wartung
16. Herkunft je Wert in Datenbank und Dashboard ("gemessen" oder "geschätzt"), damit Bericht und Dashboard
    Schätzwerte kennzeichnen oder ausblenden (Vorgabe 11).
17. Netzwerkkonfiguration vor "Netzwerk zurücksetzen" und "DNS über HTTPS" als Datei ins Änderungsverzeichnis, mit
    automatischer Rücknahme (Sicherung selbst in 4.11).
18. Eine gemeinsame Funktion für Cache-Bereinigungen (Ziele, Zielbenutzer, Ausnahmen, Protokoll) für Wartung und
    Optimierung; ungenutzte Sonderabläufe in Optimierung\Funktionen.ps1 entfallen.
19. Seite Wartung: Hinweis, wenn "Alte Wiederherstellungspunkte löschen" mit Schritten gewählt ist, deren Rückweg der
    Wiederherstellungspunkt ist.

## Früher vorgeschlagen, nicht gewählt (15.11.2026, nicht abgelehnt)
"Ergebnis ansehen" für jeden Lauf der Datenbank, Veränderung zum vorigen Lauf, Suchfelder und Filter in Sensorliste,
Datenbank und Änderungen, wählbares Zeitfenster der Kurven.
