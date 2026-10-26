# Modulvertrag System-Umzug (Vertrag 1). Felder sind in src\Kern\Modulvertrag.ps1 beschrieben.
@{
    Vertrag         = 1
    Name            = 'Umzug'
    Seite           = @{ Titel = 'System-Umzug'; Kurz = 'Daten, Desktop & Programme für Neuinstallation sichern' }
    Admin           = $true
    Risiko          = 'Lesen'
    Neustart        = 'nie'
    Parameter       = @('Umzug', 'UmzugZiel', 'UmzugUmfang', 'UmzugZusatz')
    Datenbankfelder = @()
    Schritte        = @(
        @{ Key = 'Backup'; Typ = 'Sicherung'; Titel = 'System-Umzug & Neuinstallation'; Risiko = 'Lesen'; Neustart = 'nie'; Rueckgaengig = 'keins' }
    )
}