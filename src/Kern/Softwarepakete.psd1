# Softwarepakete der Seite Tools (ab v4.05): Gruppe, winget-Id, Name in der Oberfläche, Beschreibung im Hinweis.
# Die Ids sind im winget-Paketverzeichnis (github.com/microsoft/winget-pkgs) geprüft. Reihenfolge der Gruppen und
# Pakete wie in der Oberfläche. Nur Daten, keine Befehle (eingeschränkte Sprache).
@{
    Gruppen = @('Browser', 'Kommunikation', 'Spiele', 'Office und PDF', 'Medien', 'System und Laufzeiten')
    Pakete  = @(
        @{ Gruppe = 'Browser'; Id = 'Google.Chrome'; Name = 'Google Chrome'; Beschreibung = 'Webbrowser von Google' }
        @{ Gruppe = 'Browser'; Id = 'Mozilla.Firefox'; Name = 'Mozilla Firefox'; Beschreibung = 'Webbrowser von Mozilla' }
        @{ Gruppe = 'Browser'; Id = 'Brave.Brave'; Name = 'Brave'; Beschreibung = 'Webbrowser auf Chromium-Grundlage mit Werbeblocker' }
        @{ Gruppe = 'Browser'; Id = 'Opera.Opera'; Name = 'Opera'; Beschreibung = 'Webbrowser auf Chromium-Grundlage' }
        @{ Gruppe = 'Kommunikation'; Id = 'Discord.Discord'; Name = 'Discord'; Beschreibung = 'Chat und Sprachkanäle' }
        @{ Gruppe = 'Kommunikation'; Id = 'Zoom.Zoom'; Name = 'Zoom'; Beschreibung = 'Videokonferenzen' }
        @{ Gruppe = 'Kommunikation'; Id = 'Microsoft.Teams'; Name = 'Microsoft Teams'; Beschreibung = 'Teams für Arbeit und Schule' }
        @{ Gruppe = 'Kommunikation'; Id = 'Mozilla.Thunderbird'; Name = 'Thunderbird'; Beschreibung = 'E-Mail und Kalender von Mozilla' }
        @{ Gruppe = 'Kommunikation'; Id = 'Telegram.TelegramDesktop'; Name = 'Telegram'; Beschreibung = 'Messenger für den Desktop' }
        @{ Gruppe = 'Spiele'; Id = 'Valve.Steam'; Name = 'Steam'; Beschreibung = 'Spieleplattform von Valve' }
        @{ Gruppe = 'Spiele'; Id = 'EpicGames.EpicGamesLauncher'; Name = 'Epic Games Launcher'; Beschreibung = 'Spieleplattform von Epic Games' }
        @{ Gruppe = 'Spiele'; Id = 'GOG.Galaxy'; Name = 'GOG Galaxy'; Beschreibung = 'Spieleplattform von GOG' }
        @{ Gruppe = 'Spiele'; Id = 'ElectronicArts.EADesktop'; Name = 'EA app'; Beschreibung = 'Spieleplattform von Electronic Arts' }
        @{ Gruppe = 'Spiele'; Id = 'Ubisoft.Connect'; Name = 'Ubisoft Connect'; Beschreibung = 'Spieleplattform von Ubisoft' }
        @{ Gruppe = 'Office und PDF'; Id = 'TheDocumentFoundation.LibreOffice'; Name = 'LibreOffice'; Beschreibung = 'Office-Paket mit Textverarbeitung, Tabellen und Präsentationen' }
        @{ Gruppe = 'Office und PDF'; Id = 'ONLYOFFICE.DesktopEditors'; Name = 'ONLYOFFICE'; Beschreibung = 'Office-Programme mit hoher Kompatibilität zu Microsoft-Formaten' }
        @{ Gruppe = 'Office und PDF'; Id = 'Adobe.Acrobat.Reader.64-bit'; Name = 'Adobe Acrobat Reader'; Beschreibung = 'PDF-Anzeige von Adobe (64 Bit)' }
        @{ Gruppe = 'Office und PDF'; Id = 'SumatraPDF.SumatraPDF'; Name = 'SumatraPDF'; Beschreibung = 'Schlanke PDF- und E-Book-Anzeige' }
        @{ Gruppe = 'Office und PDF'; Id = 'Notepad++.Notepad++'; Name = 'Notepad++'; Beschreibung = 'Texteditor mit Syntaxhervorhebung' }
        @{ Gruppe = 'Medien'; Id = 'VideoLAN.VLC'; Name = 'VLC'; Beschreibung = 'Medienwiedergabe für fast alle Formate' }
        @{ Gruppe = 'Medien'; Id = 'OBSProject.OBSStudio'; Name = 'OBS Studio'; Beschreibung = 'Bildschirmaufnahme und Streaming' }
        @{ Gruppe = 'Medien'; Id = 'GIMP.GIMP.3'; Name = 'GIMP'; Beschreibung = 'Bildbearbeitung (Version 3)' }
        @{ Gruppe = 'System und Laufzeiten'; Id = '7zip.7zip'; Name = '7-Zip'; Beschreibung = 'Packprogramm' }
        @{ Gruppe = 'System und Laufzeiten'; Id = 'RARLab.WinRAR'; Name = 'WinRAR'; Beschreibung = 'Packprogramm für RAR und ZIP (Testversion, Lizenz für dauerhafte Nutzung)' }
        @{ Gruppe = 'System und Laufzeiten'; Id = 'Microsoft.PowerToys'; Name = 'PowerToys'; Beschreibung = 'Werkzeugsammlung von Microsoft' }
        @{ Gruppe = 'System und Laufzeiten'; Id = 'KeePassXCTeam.KeePassXC'; Name = 'KeePassXC'; Beschreibung = 'Passwortverwaltung, Datenbank liegt lokal' }
        @{ Gruppe = 'System und Laufzeiten'; Id = 'Microsoft.VCRedist.2015+.x64'; Name = 'Visual C++ 2015+ x64'; Beschreibung = 'Laufzeitbibliotheken, die viele Programme und Spiele brauchen' }
        @{ Gruppe = 'System und Laufzeiten'; Id = 'Microsoft.DotNet.DesktopRuntime.10'; Name = '.NET Runtime 10'; Beschreibung = '.NET Desktop Runtime 10, Laufzeit für .NET-Programme mit Oberfläche (aktuelle LTS)' }
    )
}
