# Modul Optimierung (Windows Optimisation Pack nativ): Katalog, Vorlagen (Minimal, Leos Empfehlung, Erweitert),
# angemeldeter Benutzer und Pfade, Einträge ausführen und über das Änderungsprotokoll zurücknehmen, Bericht.
# Dazu das Modul Wartung (Modulvertrag und Ablauf).
BeforeAll {
    . (Join-Path $PSScriptRoot 'Hilfen.ps1')
    Import-MinibenchTestModule -Parts 'Kern\Risiko.ps1', 'Kern\Geraeteidentitaet.ps1', 'Kern\Aenderungen.ps1', 'Kern\Modulvertrag.ps1', 'Module\Optimierung\Funktionen.ps1' `
        -Functions 'Get-SafeName', 'Invoke-External', 'Show-Sub', 'Hide-Sub', 'Write-Heartbeat', 'Send-GuiEvent', 'Get-OptStepKey', 'Get-SkipSwitch', 'ConvertTo-HtmlText'
    Mock -ModuleName MinibenchTest Write-Host { }
    $katalogText = Get-SrcText 'Module/Optimierung/Katalog.psd1'
    function New-OptRun {
        $dir = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $dir | Out-Null
        Set-ModuleVar 'DataDir' $dir
        Set-ModuleVar 'ChangeLog' $null
        Set-ModuleVar 'DeviceIdentity' ([pscustomobject]@{ Id = 'G-bbbbbbbbbbbbbbbbbbbb'; Guete = 'hoch'; Quellen = @() })
        MinibenchTest\Start-ChangeLog
    }
    $optEnv = [pscustomobject]@{ Build = 26200; Win11 = $true; Desktop = $true; Nvidia = $false; Terminal = $false; Domain = $false; Neustart = $false; Grafik = @()
        User = [pscustomobject]@{ Sid = 'S-1-5-21-1-2-3-1001'; Name = 'PC\anna'; Profil = 'C:\Users\anna'; Eigen = $true; Quelle = 'test' } }
}

Describe 'Katalog der Optimierung' {
    It 'ist eingeschränkte Sprache (nur Daten, keine Befehle)' {
        $sb = [scriptblock]::Create($katalogText)
        { $sb.CheckRestrictedLanguage([string[]]@(), [string[]]@('true', 'false', 'null'), $false) } | Should -Not -Throw
    }
    It 'hat eindeutige Ids, bekannte Kategorien, Stufen, Vorlagen und Bedingungen' {
        $e = @(MinibenchTest\Get-OptEntries)
        $e.Count | Should -BeGreaterThan 150
        # Katalogeinträge sind Hashtables: Group-Object und Where-Object mit Eigenschaftsnamen finden deren Schlüssel unter
        # Windows PowerShell 5.1 nicht (alle Einträge landeten in einer Gruppe), daher überall Skriptblöcke
        $ids = @($e | ForEach-Object { [string]$_.Id })
        @($ids | Where-Object { -not $_ }).Count | Should -Be 0
        @($ids | Sort-Object -Unique).Count | Should -Be $ids.Count
        $kats = @(MinibenchTest\Get-OptCategories | ForEach-Object { $_.Key })
        foreach ($x in $e) {
            $kats | Should -Contain $x.Kat -Because $x.Id
            (@('Aendern', 'Eingriff') -contains $x.Risiko) | Should -BeTrue -Because $x.Id
            [string]$x.Vorlagen | Should -Match '^M?S?E?$' -Because $x.Id
            (@('', 'Win10', 'Win11', 'Desktop', 'Nvidia', 'Terminal') -contains [string]$x.Bedingung) | Should -BeTrue -Because $x.Id
            (@('nie', 'moeglich', 'immer') -contains [string]$x.Neustart) | Should -BeTrue -Because $x.Id
            $x.Titel | Should -Not -BeNullOrEmpty
            $x.Text | Should -Not -BeNullOrEmpty
            $x.Quelle | Should -Not -BeNullOrEmpty
            @($x.Aktionen).Count | Should -BeGreaterThan 0 -Because $x.Id
        }
        foreach ($k in $kats) { @($e | Where-Object { $_.Kat -eq $k }).Count | Should -BeGreaterThan 0 -Because $k }
    }
    It 'enthält keine Kategorie Bereinigung mehr (Bereinigungen gehören zum Modul Wartung)' {
        @(MinibenchTest\Get-OptCategories | ForEach-Object { $_.Key }) | Should -Not -Contain 'Bereinigung'
        @(MinibenchTest\Get-OptEntries | ForEach-Object { $_.Kat }) | Should -Not -Contain 'Bereinigung'
    }
    It 'Stufe Ändern nur mit Aktionen, die sich einzeln zurücknehmen lassen' {
        $undo = 'Defender', 'DnsOverHttps', 'Energieplan', 'Indizierung', 'Laufwerksname', 'NetzwerkEnergie', 'Ruhezustand', 'Speicherreserve', 'Wartungsaufgaben'
        # Zählung schützt vor einer leeren Schleife (Filter, der unter PS 5.1 nichts findet)
        @(MinibenchTest\Get-OptEntries | Where-Object { $_.Risiko -eq 'Aendern' }).Count | Should -BeGreaterThan 100
        foreach ($x in @(MinibenchTest\Get-OptEntries | Where-Object { $_.Risiko -eq 'Aendern' })) {
            foreach ($a in @($x.Aktionen)) {
                (@('Reg', 'RegKey', 'Dienst', 'Aufgabe', 'Feature', 'Sonder') -contains $a.Art) | Should -BeTrue -Because $x.Id
                if ($a.Art -eq 'Sonder') { ($undo -contains $a.Name) | Should -BeTrue -Because $x.Id }
            }
        }
        foreach ($x in @(MinibenchTest\Get-OptEntries | Where-Object { @($_.Aktionen | Where-Object { $_.Art -in 'App', 'Capability' }).Count })) { $x.Risiko | Should -Be 'Eingriff' -Because $x.Id }
    }
    It 'Registry-Aktionen sind vollständig' {
        @(MinibenchTest\Get-OptEntries | ForEach-Object { $_.Aktionen } | Where-Object { $_.Art -eq 'Reg' }).Count | Should -BeGreaterThan 100
        foreach ($x in @(MinibenchTest\Get-OptEntries)) {
            foreach ($a in @($x.Aktionen | Where-Object { $_.Art -eq 'Reg' })) {
                $a.Pfad | Should -Match '^(HKLM:|HKCU:|Registry::HKEY_USERS)\\' -Because $x.Id
                $a.Name | Should -Not -BeNullOrEmpty -Because $x.Id
                (@('DWord', 'QWord', 'String', 'ExpandString', 'Binary', 'MultiString') -contains $a.Typ) | Should -BeTrue -Because ('{0} {1}' -f $x.Id, $a.Name)
            }
        }
    }
    It 'jeder Eintrag gehört zu einem Schritt des Modulvertrags' {
        $keys = @((MinibenchTest\Get-ModuleContract 'Optimierung').Schritte | ForEach-Object { $_.Key })
        foreach ($x in @(MinibenchTest\Get-OptEntries)) { $keys | Should -Contain (MinibenchTest\Get-OptStepKey $x) -Because $x.Id }
    }
    It 'Zeilen für die Oberfläche haben feste Spaltenzahl und keine Zeilenumbrüche' {
        $l = @(MinibenchTest\Get-OptGuiLines)
        @($l | Where-Object { $_ -like 'K|*' }).Count | Should -Be @(MinibenchTest\Get-OptCategories).Count
        foreach ($z in @($l | Where-Object { $_ -like 'E|*' })) {
            @($z -split '\|').Count | Should -Be 11 -Because $z.Substring(0, 30)
            $z | Should -Not -Match '[\r\n]'
        }
    }
    It 'Auswahl meldet Unbekanntes und hält die Katalogreihenfolge' {
        $r = MinibenchTest\Resolve-OptSelection 'Feedback; WerbeId,gibtsnicht'
        $r.Unbekannt | Should -Be @('gibtsnicht')
        $all = @(MinibenchTest\Get-OptEntries | ForEach-Object { $_.Id })
        $all.IndexOf($r.Ids[0]) | Should -BeLessThan $all.IndexOf($r.Ids[1])
    }
}

Describe 'Vorlagen der Optimierung' {
    It 'bauen aufeinander auf (Minimal in Leos Empfehlung in Erweitert)' {
        $m = @(MinibenchTest\Get-OptPreset 'M'); $s = @(MinibenchTest\Get-OptPreset 'S'); $x = @(MinibenchTest\Get-OptPreset 'E')
        $m.Count | Should -BeGreaterThan 0
        $m.Count | Should -BeLessThan $s.Count
        @($m | Where-Object { $s -notcontains $_ }).Count | Should -Be 0
        @($s | Where-Object { $x -notcontains $_ }).Count | Should -Be 0
        # DDU und das Löschen der Wiederherstellungspunkte nur von Hand
        $x | Should -Not -Contain 'Ddu'
        $x | Should -Not -Contain 'Wiederherstellungspunkte'
    }
    It 'Minimal enthält genau die fünf Grundeinstellungen' {
        @(MinibenchTest\Get-OptPreset 'M' | Sort-Object) | Should -Be @('Fehlerberichte', 'Speicheroptimierung', 'TaskbarEndTask', 'TelemetrieMinimal', 'WerbeId')
    }
    It 'Leos Empfehlung enthält Task beenden per Rechtsklick und den UEFI-Neustart' {
        $s = @(MinibenchTest\Get-OptPreset 'S')
        $s | Should -Contain 'TaskbarEndTask'
        $s | Should -Contain 'UefiNeustart'
    }
    It 'auf Domänen-PCs bleiben Einträge, die Richtlinien berühren, außen vor' {
        $verwaltet = @(MinibenchTest\Get-OptEntries | Where-Object { $_.Verwaltet -and ([string]$_.Vorlagen).Contains('S') } | ForEach-Object { $_.Id })
        $verwaltet.Count | Should -BeGreaterThan 0
        $dom = @(MinibenchTest\Get-OptPreset 'S' $true)
        foreach ($id in $verwaltet) { $dom | Should -Not -Contain $id }
        @(MinibenchTest\Get-OptPreset 'S' $false) | Should -Contain $verwaltet[0]
    }
    It 'Task beenden per Rechtsklick ist ein eigener Eintrag für Windows 11 mit Gegenbefehl' {
        $e = MinibenchTest\Get-OptEntry 'TaskbarEndTask'
        $e | Should -Not -BeNullOrEmpty
        $e.Kat | Should -Be 'Bedienung'
        $e.Vorlagen | Should -Be 'MSE'
        $e.Bedingung | Should -Be 'Win11'
        @($e.Aktionen).Count | Should -Be 2
        @($e.Aktionen | Where-Object { $_.Art -eq 'Reg' -and $_.Name -eq 'TaskbarEndTask' -and $_.Wert -eq 1 }).Count | Should -Be 2
        @($e.Gegenbefehl | Where-Object { $_ -match 'TaskbarEndTask' }).Count | Should -BeGreaterThan 0
    }
    It 'Taskleiste gruppieren setzt nur TaskbarGlomLevel' {
        $e = MinibenchTest\Get-OptEntry 'TaskleisteGruppieren'
        $e | Should -Not -BeNullOrEmpty
        @($e.Aktionen | Where-Object { $_.Name -eq 'TaskbarGlomLevel' }).Count | Should -Be 1
        @($e.Aktionen | Where-Object { $_.Name -eq 'TaskbarEndTask' }).Count | Should -Be 0
    }
    It 'UEFI-Neustart legt einen eigenen Kontextmenü-Schlüssel an (rücknehmbar)' {
        $e = MinibenchTest\Get-OptEntry 'UefiNeustart'
        $e | Should -Not -BeNullOrEmpty
        $e.Kat | Should -Be 'Bedienung'
        [string]$e.Vorlagen | Should -Match 'S'
        [string]$e.Vorlagen | Should -Match 'E'
        @($e.Aktionen)[0].Art | Should -Be 'RegKey'
    }
}

Describe 'Benutzer und Pfade der Optimierung' {
    It 'lenkt HKCU auf den angemeldeten Benutzer, wenn Leos Minibench unter einem anderen Konto läuft' {
        $u = [pscustomobject]@{ Sid = 'S-1-5-21-9'; Eigen = $false }
        MinibenchTest\Resolve-OptRegPath 'HKCU:\Software\Test' $u | Should -Be 'Registry::HKEY_USERS\S-1-5-21-9\Software\Test'
        MinibenchTest\Resolve-OptRegPath 'HKCU:\Software\Test' ([pscustomobject]@{ Sid = 'S-1-5-21-9'; Eigen = $true }) | Should -Be 'HKCU:\Software\Test'
        MinibenchTest\Resolve-OptRegPath 'HKLM:\SOFTWARE\X\{SID}' $u | Should -Be 'HKLM:\SOFTWARE\X\S-1-5-21-9'
    }
    It 'setzt Profilpfade des angemeldeten Benutzers ein' {
        $p = MinibenchTest\Expand-OptPath '%TEMP%\x' ([pscustomobject]@{ Profil = (Join-Path $TestDrive 'anna') })
        $p | Should -Match 'anna'
        $p | Should -Match 'Temp'
        MinibenchTest\Expand-OptPath '%localappdata%' ([pscustomobject]@{ Profil = (Join-Path $TestDrive 'a$b') }) | Should -Match ([regex]::Escape('a$b'))
    }
    It 'prüft Bedingungen' {
        $e10 = [pscustomobject]@{ Win11 = $false; Desktop = $false; Nvidia = $false; Terminal = $false }
        MinibenchTest\Test-OptCondition ([pscustomobject]@{ Bedingung = 'Win11' }) $e10 | Should -Match 'Windows 11'
        MinibenchTest\Test-OptCondition ([pscustomobject]@{ Bedingung = 'Desktop' }) $e10 | Should -Match 'Desktop'
        MinibenchTest\Test-OptCondition ([pscustomobject]@{ Bedingung = '' }) $e10 | Should -BeNullOrEmpty
    }
}

Describe 'Einträge ausführen und zurücknehmen' {
    BeforeEach {
        New-OptRun
        $script:reg = @{}
        Mock -ModuleName MinibenchTest Get-RegValueState { $k = $Path + '|' + $Name; if ($script:reg.ContainsKey($k)) { [pscustomobject]$script:reg[$k] } else { [pscustomobject]@{ Vorhanden = $false; Wert = $null; Typ = '' } } }
        Mock -ModuleName MinibenchTest Set-RegValueState { $script:reg[$Path + '|' + $Name] = @{ Vorhanden = $true; Wert = $Value; Typ = $Type } }
        Mock -ModuleName MinibenchTest Remove-RegValue { $script:reg.Remove($Path + '|' + $Name) }
        Set-ModuleVar 'OptRestart' ([System.Collections.Generic.List[string]]::new())
    }
    It 'Registry: angewendet, beim zweiten Mal bereits so, im Protokoll mit Vorher-Wert' {
        $e = [pscustomobject]@{ Id = 'T1'; Titel = 'Test'; Kat = 'Datenschutz'; Risiko = 'Aendern'; Neustart = 'nie'; Bedingung = ''
            Aktionen = @(@{ Art = 'Reg'; Pfad = 'HKLM:\SOFTWARE\Test'; Name = 'A'; Wert = 0; Typ = 'DWord' }, @{ Art = 'Reg'; Pfad = 'HKCU:\Software\Test'; Name = 'B'; Wert = 1; Typ = 'DWord' }) }
        (MinibenchTest\Get-OptEntryState $e $optEnv).Zustand | Should -Be 'offen'
        $r = MinibenchTest\Invoke-OptEntry $e $optEnv
        $r.Ergebnis | Should -Be 'angewendet'
        $r.Aenderungen | Should -Be 2
        (MinibenchTest\Invoke-OptEntry $e $optEnv).Ergebnis | Should -Be 'bereits so'
        (MinibenchTest\Get-OptEntryState $e $optEnv).Zustand | Should -Be 'aktiv'
        $j = Get-Content -LiteralPath (Get-ModuleVar 'ChangeFile') -Raw -Encoding UTF8 | ConvertFrom-Json
        @($j.Eintraege).Count | Should -Be 2
        $j.Eintraege[0].Modul | Should -Be 'Optimierung'
        $j.Eintraege[0].Schritt | Should -Be 'T1'
        $f = Get-ModuleVar 'ChangeFile'
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1;{0}*2' -f $f)
        $u.Rueckgaengig | Should -Be 2
        $script:reg.Count | Should -Be 0
    }
    It 'Lauf überspringt bereits aktive Einträge (ab v4.05), offene und teilweise laufen' {
        $a = [pscustomobject]@{ Id = 'A1'; Titel = 'Schon so'; Kat = 'Datenschutz'; Risiko = 'Aendern'; Neustart = 'nie'; Bedingung = ''; Aktionen = @(@{ Art = 'Reg'; Pfad = 'HKLM:\SOFTWARE\T'; Name = 'A'; Wert = 0; Typ = 'DWord' }) }
        $o = [pscustomobject]@{ Id = 'O1'; Titel = 'Offen'; Kat = 'Datenschutz'; Risiko = 'Aendern'; Neustart = 'nie'; Bedingung = ''; Aktionen = @(@{ Art = 'Reg'; Pfad = 'HKLM:\SOFTWARE\T'; Name = 'B'; Wert = 1; Typ = 'DWord' }) }
        $t = [pscustomobject]@{ Id = 'T3'; Titel = 'Teilweise'; Kat = 'Datenschutz'; Risiko = 'Aendern'; Neustart = 'nie'; Bedingung = ''; Aktionen = @(@{ Art = 'Reg'; Pfad = 'HKLM:\SOFTWARE\T'; Name = 'A'; Wert = 0; Typ = 'DWord' }, @{ Art = 'Reg'; Pfad = 'HKLM:\SOFTWARE\T'; Name = 'C'; Wert = 1; Typ = 'DWord' }) }
        $w = [pscustomobject]@{ Id = 'W1'; Titel = 'Nur Win10'; Kat = 'Bedienung'; Risiko = 'Aendern'; Neustart = 'nie'; Bedingung = 'Win10'; Aktionen = @(@{ Art = 'Reg'; Pfad = 'HKLM:\SOFTWARE\T'; Name = 'A'; Wert = 0; Typ = 'DWord' }) }
        $script:reg['HKLM:\SOFTWARE\T|A'] = @{ Vorhanden = $true; Wert = 0; Typ = 'DWord' }
        $sp = MinibenchTest\Split-OptBereitsAktiv @($a, $o, $t, $w) $optEnv
        @($sp.Aktiv | ForEach-Object { $_.Id }) | Should -Be @('A1')
        @($sp.Offen | ForEach-Object { $_.Id }) | Should -Be @('O1', 'T3', 'W1')
        $sp.Aktiv[0].Ergebnis | Should -Be 'bereits aktiv'
        $sp.Aktiv[0].Details | Should -Match 'übersprungen'
        Should -Invoke -ModuleName MinibenchTest Set-RegValueState -Times 0
    }
    It 'Ablauf: Abgleich vor dem Wiederherstellungspunkt, ohne offene Einträge kein Wiederherstellungspunkt' {
        $a = Get-PartText 'Module\Optimierung\Ablauf.ps1'
        $split = $a.IndexOf('Split-OptBereitsAktiv')
        $rp = $a.IndexOf("Invoke-Section 'Optimierung: Wiederherstellungspunkt anlegen'")
        $split | Should -BeGreaterThan 0
        $rp | Should -BeGreaterThan $split
        $a | Should -Match 'if \(-not \$script:OptZuTun\)'
        $a | Should -Match "Send-GuiEvent 'OPTE' \`$r\.Id \`$r\.Ergebnis"
        (Get-PartText 'Module\Optimierung\Funktionen.ps1') | Should -Match "'bereits aktiv' = 'skip'"
    }
    It 'Bedingung nicht erfüllt: übersprungen, nichts geschrieben' {
        $e = [pscustomobject]@{ Id = 'T2'; Titel = 'Nur Win10'; Kat = 'Bedienung'; Risiko = 'Aendern'; Neustart = 'nie'; Bedingung = 'Win10'; Aktionen = @(@{ Art = 'Reg'; Pfad = 'HKLM:\X'; Name = 'A'; Wert = 1; Typ = 'DWord' }) }
        (MinibenchTest\Invoke-OptEntry $e $optEnv).Ergebnis | Should -Be 'übersprungen'
        Should -Invoke -ModuleName MinibenchTest Set-RegValueState -Times 0
    }
    It 'Dienst, den es nicht gibt: nicht vorhanden statt Fehler' {
        Mock -ModuleName MinibenchTest Get-ServiceStartType { throw 'nicht vorhanden' }
        $e = [pscustomobject]@{ Id = 'T3'; Titel = 'Dienst'; Kat = 'Hintergrund'; Risiko = 'Aendern'; Neustart = 'nie'; Bedingung = ''; Aktionen = @(@{ Art = 'Dienst'; Name = 'GibtsNicht'; Start = 'Disabled' }) }
        (MinibenchTest\Invoke-OptEntry $e $optEnv).Ergebnis | Should -Be 'nicht vorhanden'
    }
    It 'Dienst: Starttyp mit Vorher-Wert, Dienst wird beendet' {
        $script:svc = 'Automatic'
        Mock -ModuleName MinibenchTest Get-ServiceStartType { $script:svc }
        Mock -ModuleName MinibenchTest Set-ServiceStartType { $script:svc = $StartType }
        Mock -ModuleName MinibenchTest Stop-OptService { $true }
        $e = [pscustomobject]@{ Id = 'T4'; Titel = 'Dienst'; Kat = 'Hintergrund'; Risiko = 'Aendern'; Neustart = 'nie'; Bedingung = ''; Aktionen = @(@{ Art = 'Dienst'; Name = 'DiagTrack'; Start = 'Disabled' }) }
        (MinibenchTest\Invoke-OptEntry $e $optEnv).Ergebnis | Should -Be 'angewendet'
        $script:svc | Should -Be 'Disabled'
        Should -Invoke -ModuleName MinibenchTest Stop-OptService -Times 1
    }
    It 'Aufgabe: deaktiviert und über das Protokoll wieder aktiv' {
        $script:task = 'Ready'
        Mock -ModuleName MinibenchTest Get-OptTasks { @([pscustomobject]@{ Pfad = '\Microsoft\Windows\Test\'; Name = 'Sammler'; Zustand = $script:task }) }
        Mock -ModuleName MinibenchTest Disable-OptTask { $script:task = 'Disabled' }
        Mock -ModuleName MinibenchTest Enable-OptTask { $script:task = 'Ready' }
        $e = [pscustomobject]@{ Id = 'T5'; Titel = 'Aufgabe'; Kat = 'Datenschutz'; Risiko = 'Aendern'; Neustart = 'nie'; Bedingung = ''; Aktionen = @(@{ Art = 'Aufgabe'; Pfad = '\Microsoft\Windows\Test\'; Name = 'Sammler' }) }
        (MinibenchTest\Invoke-OptEntry $e $optEnv).Ergebnis | Should -Be 'angewendet'
        $script:task | Should -Be 'Disabled'
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-ModuleVar 'ChangeFile'))
        $u.Rueckgaengig | Should -Be 1
        $script:task | Should -Be 'Ready'
    }
    It 'App: Eingriff, Fehler beim Entfernen landet im Ergebnis' {
        Mock -ModuleName MinibenchTest Get-OptApps { @([pscustomobject]@{ Name = 'Microsoft.BingNews'; Paket = 'Microsoft.BingNews_1.0_x64'; Version = '1.0' }) }
        Mock -ModuleName MinibenchTest Remove-OptApp { throw 'Zugriff verweigert' }
        $e = [pscustomobject]@{ Id = 'T6'; Titel = 'News'; Kat = 'Apps'; Risiko = 'Eingriff'; Neustart = 'nie'; Bedingung = ''; Aktionen = @(@{ Art = 'App'; Muster = 'Microsoft.BingNews' }) }
        $r = MinibenchTest\Invoke-OptEntry $e $optEnv
        $r.Ergebnis | Should -Be 'Fehler'
        $r.Details | Should -Match 'Zugriff verweigert'
    }
    It 'Windows-Funktion: Neustart wird vermerkt' {
        Mock -ModuleName MinibenchTest Get-OptFeatureState { 'Enabled' }
        Mock -ModuleName MinibenchTest Set-OptFeature { $true }
        $e = [pscustomobject]@{ Id = 'T7'; Titel = 'Funktion'; Kat = 'Funktionen'; Risiko = 'Aendern'; Neustart = 'moeglich'; Bedingung = ''; Aktionen = @(@{ Art = 'Feature'; Name = 'WorkFolders-Client' }) }
        $r = MinibenchTest\Invoke-OptEntry $e $optEnv
        $r.Neustart | Should -BeTrue
        @(Get-ModuleVar 'OptRestart') | Should -Contain 'Funktion'
    }
    It 'Rückgängig kennt neue Arten und lässt fremde unberührt' {
        MinibenchTest\Undo-OptChange ([pscustomobject]@{ Art = 'Unbekannt'; Daten = @{} }) | Should -BeNullOrEmpty
        Mock -ModuleName MinibenchTest Test-OptRegKey { $false }
        (MinibenchTest\Undo-OptChange ([pscustomobject]@{ Art = 'RegSchluessel'; Daten = [pscustomobject]@{ Pfad = 'HKCU:\X' } })).Status | Should -Be 'übersprungen'
    }
}

Describe 'Optimierung im Ablauf und im Bericht' {
    It 'Bericht: Zusammenfassung und je Kategorie eine Tabelle, Kategorien mit Fehlern aufgeklappt' {
        $log = [System.Collections.Generic.List[object]]::new()
        $log.Add([pscustomobject]@{ Titel = 'Werbe-ID aus'; Kat = 'Datenschutz'; Ergebnis = 'angewendet'; Aenderungen = 2; Risiko = 'Aendern'; Details = '' })
        $log.Add([pscustomobject]@{ Titel = 'Feedback <aus>'; Kat = 'Datenschutz'; Ergebnis = 'Fehler'; Aenderungen = 0; Risiko = 'Aendern'; Details = 'Zugriff verweigert' })
        Set-ModuleVar 'OptLog' $log
        Set-ModuleVar 'OptMetricsBefore' $null
        Set-ModuleVar 'OptMetricsAfter' $null
        $h = MinibenchTest\New-OptHtml
        $h | Should -Match '1 von 2 Einträgen angewendet, 2 Einzeländerungen'
        $h | Should -Match '<details open><summary><b>Datenschutz und Telemetrie</b>'
        $h | Should -Match 'badge crit">Fehler'
        $h | Should -Match 'Feedback &lt;aus&gt;'
        $h | Should -Not -Match 'Kennzahlen vorher und nachher'
        (Get-PartText 'Bericht\HtmlBericht.ps1') | Should -Match 'New-OptHtml'
    }
    It 'Absturz in einer Kategorie der Optimierung schaltet beim nächsten Lauf genau diese Kategorie ab' {
        $k = @(MinibenchTest\Get-OptCategories) | Where-Object { $_.Key -eq 'Sicherheit' }
        MinibenchTest\Get-SkipSwitch ('Optimierung: ' + $k.Titel) | Should -Be 'Opt8:Sicherheit'
        MinibenchTest\Get-SkipSwitch 'Optimierung: Ergebnis' | Should -BeNullOrEmpty
        MinibenchTest\Get-SkipSwitch 'Defender' | Should -Be 'Opt:Defender'
    }
    It 'Befehle der Wartungsaufgaben sind gültiges PowerShell' {
        $t = @(MinibenchTest\Get-OptMaintenanceTasks)
        $t.Count | Should -BeGreaterThan 0
        foreach ($x in $t) {
            $errs = $null; $tok = $null
            [void][System.Management.Automation.Language.Parser]::ParseInput($x.Befehl, [ref]$tok, [ref]$errs)
            @($errs).Count | Should -Be 0 -Because $x.Name
        }
    }
    It 'Wiederherstellungspunkt: nach dem Löschen alter Punkte wird ein neuer angelegt' {
        $a = Get-PartText 'Module\Optimierung\Ablauf.ps1'
        $del = $a.IndexOf("Invoke-Section 'Optimierung: Alte Wiederherstellungspunkte löschen'")
        $new = $a.IndexOf("Invoke-Section 'Optimierung: Wiederherstellungspunkt anlegen'")
        $del | Should -BeGreaterThan 0
        $new | Should -BeGreaterThan $del
        # der Punkt dieses Laufs ist mit gelöscht: die Nummer wird verworfen, damit danach ein neuer entsteht
        $a.Substring($del, $new - $del) | Should -Match '\$script:RestorePointNo = \$null'
    }
    It 'startet weder Sophia Script noch O&O ShutUp10++' {
        $b = (Get-MinibenchBuild).Text
        $b | Should -Not -Match 'Sophia\.ps1'
        $b | Should -Not -Match 'OOSU10\.exe'
    }
}

Describe 'Modul Wartung' {
    BeforeAll {
        $wartung = MinibenchTest\Get-ModuleContract 'Wartung'
    }
    It 'der Ablauf behandelt jeden Schritt des Vertrags' {
        $ablauf = Get-SrcText 'Module/Wartung/Ablauf.ps1'
        @($wartung.Schritte).Count | Should -BeGreaterThan 0
        foreach ($s in $wartung.Schritte) { $ablauf | Should -Match ("'" + [regex]::Escape($s.Key) + "'") -Because $s.Key }
    }
    It 'die übliche Auswahl enthält nur risikoarme Bereinigungen' {
        $ueblich = @($wartung.Schritte | Where-Object { $_.Ueblich } | ForEach-Object { $_.Key })
        foreach ($k in 'Datentraegerbereinigung', 'Temp', 'ShaderCache', 'UpdateDownloads', 'Absturzabbilder', 'Prefetch', 'WMI') { $ueblich | Should -Contain $k }
        foreach ($k in 'DismRestore', 'Sfc', 'Dateisystem') { $ueblich | Should -Not -Contain $k }
    }
}

# ab v4.11: Befunde A2, A3, A4, A7, E1 bis E4 der Prüfung 4.10. Eigenes Testmodul mit dem Modul Wartung und den
# Werkzeugen; externe Programme (vssadmin, cleanmgr, netsh, Checkpoint-Computer, Download) sind Attrappen.
Describe 'Wartung und Optimierung ab v4.11' {
    BeforeAll {
        Import-MinibenchTestModule -Parts 'Kern\Risiko.ps1', 'Kern\Geraeteidentitaet.ps1', 'Kern\Aenderungen.ps1', 'Kern\Modulvertrag.ps1', 'Kern\Werkzeuge.ps1', 'Module\Optimierung\Funktionen.ps1', 'Module\Wartung\Ablauf.ps1' `
            -Functions 'Get-SafeName', 'Invoke-External', 'Show-Sub', 'Hide-Sub', 'Write-Heartbeat', 'Send-GuiEvent', 'Format-Size', 'Save-WebFile' `
            -Setup @'
function Add-TestResult { }
function Add-Finding { }
function Write-Step { }
function Add-Line { }
function Get-CbsEntries { }
function Checkpoint-Computer { param($Description, $RestorePointType) }
function Enable-ComputerRestore { param($Drive) }
function Enable-ComputerschutzMitProtokoll { param([string]$Laufwerk) return $true }
function Get-NetIPConfiguration { }
function Get-DnsClientServerAddress { }
function Delete-DeliveryOptimizationCache { param([switch]$Force) }
function Get-DeliveryOptimizationStatus { }
function Repair-Volume { param($DriveLetter, [switch]$Scan, [switch]$SpotFix) }
'@
        Mock -ModuleName MinibenchTest Write-Host { }
        function New-WartungRun {
            $dir = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $dir | Out-Null
            Set-ModuleVar 'DataDir' $dir
            Set-ModuleVar 'ToolsDir' (Join-Path $dir 'Tools')
            New-Item -ItemType Directory -Path (Join-Path $dir 'Tools') | Out-Null
            Set-ModuleVar 'ChangeLog' $null
            Set-ModuleVar 'DeviceIdentity' ([pscustomobject]@{ Id = 'G-cccccccccccccccccccc'; Guete = 'hoch'; Quellen = @() })
            MinibenchTest\Start-ChangeLog
            $c = MinibenchTest\Get-ModuleContract 'Wartung'
            $t = [ordered]@{}; $r = @{}
            foreach ($s in $c.Schritte) { $t[$s.Key] = $s.Titel; $r[$s.Key] = $s.Risiko }
            Set-ModuleVar 'RepTitles' $t
            Set-ModuleVar 'RepRisk' $r
            Set-ModuleVar 'RepairLog' ([System.Collections.Generic.List[object]]::new())
            Set-ModuleVar 'RestartNeeded' ([System.Collections.Generic.List[string]]::new())
            Set-ModuleVar 'RestorePointNo' ''
            Set-ModuleVar 'ToolIssues' (New-Object System.Collections.Generic.List[string])
            return $dir
        }
        function Get-Eintraege { @((Get-ModuleVar 'ChangeLog').Eintraege) }
        $script:ok = { param($Out = 'ok') [pscustomobject]@{ ExitCode = 0; Output = $Out; Error = ''; TimedOut = $false } }
    }

    Context 'A2 Wiederherstellungspunkt' {
        It 'alte Punkte werden vor dem neuen Punkt gelöscht, die übrigen Schritte danach' {
            $plan = @(MinibenchTest\Get-WartungPlan @('Temp', 'Druck', 'Wiederherstellungspunkte') @('Wiederherstellungspunkte', 'Druck', 'Temp') $true)
            $plan | Should -Be @('Wiederherstellungspunkte', '@Wiederherstellungspunkt', 'Temp', 'Druck')
            @(MinibenchTest\Get-WartungPlan @('Temp', 'Wiederherstellungspunkte') @('Temp', 'Wiederherstellungspunkte') $false) | Should -Be @('Wiederherstellungspunkte', 'Temp')
            @(MinibenchTest\Get-WartungPlan @('Temp', 'Druck') @('Druck') $true) | Should -Be @('@Wiederherstellungspunkt', 'Druck')
        }
        It 'Nummer nur, wenn Windows den Punkt mit dieser Beschreibung angelegt hat; Computerschutz über das Protokoll' {
            [void](New-WartungRun)
            Mock -ModuleName MinibenchTest Get-ItemProperty { $null }
            Mock -ModuleName MinibenchTest Test-Path { $true }
            Mock -ModuleName MinibenchTest Set-ItemProperty { }
            Mock -ModuleName MinibenchTest Remove-ItemProperty { }
            Mock -ModuleName MinibenchTest Enable-ComputerschutzMitProtokoll { $true }
            Mock -ModuleName MinibenchTest Enable-ComputerRestore { }
            Mock -ModuleName MinibenchTest Checkpoint-Computer { $global:LmbTestPunkt = $Description }
            # Sperrfrist: Windows legt keinen neuen Punkt an, es gibt nur einen alten
            Mock -ModuleName MinibenchTest Get-ComputerRestorePoint { [pscustomobject]@{ Description = 'Windows Update'; SequenceNumber = 7 } }
            MinibenchTest\New-WartungRestorePoint
            Get-ModuleVar 'RestorePointNo' | Should -BeNullOrEmpty
            @(Get-Eintraege | Where-Object { $_.Art -eq 'Wiederherstellungspunkt' }).Count | Should -Be 0
            Should -Invoke -ModuleName MinibenchTest Enable-ComputerschutzMitProtokoll -Times 1 -ParameterFilter { $Laufwerk -eq ($env:SystemDrive + '\') }
            Should -Invoke -ModuleName MinibenchTest Enable-ComputerRestore -Times 0
            # Punkt angelegt: Nummer des eigenen Punkts, nicht des neuesten
            Mock -ModuleName MinibenchTest Get-ComputerRestorePoint { @([pscustomobject]@{ Description = $global:LmbTestPunkt; SequenceNumber = 12 }, [pscustomobject]@{ Description = 'Fremd'; SequenceNumber = 13 }) }
            MinibenchTest\New-WartungRestorePoint
            Get-ModuleVar 'RestorePointNo' | Should -Be '12'
            (@(Get-Eintraege | Where-Object { $_.Art -eq 'Wiederherstellungspunkt' })[0]).Gegenbefehl | Should -Match 'Nr\. 12'
            Remove-Variable -Name LmbTestPunkt -Scope Global -ErrorAction SilentlyContinue
        }
        It 'Optimierung erkennt einen nicht mehr vorhandenen Punkt' {
            Mock -ModuleName MinibenchTest Get-ComputerRestorePoint { [pscustomobject]@{ Description = 'x'; SequenceNumber = 5 } }
            MinibenchTest\Test-RestorePointPresent '5' | Should -BeTrue
            MinibenchTest\Test-RestorePointPresent '9' | Should -BeFalse
            MinibenchTest\Test-RestorePointPresent '' | Should -BeFalse
            Mock -ModuleName MinibenchTest Get-ComputerRestorePoint { throw 'Dienst aus' }
            MinibenchTest\Test-RestorePointPresent '5' | Should -BeNullOrEmpty
            # Ablauf der Optimierung: Prüfung vor dem eigenen Punkt, Nummer nur aus dem wirklich angelegten Punkt
            $a = Get-PartText 'Module\Optimierung\Ablauf.ps1'
            $a.IndexOf('Test-RestorePointPresent $script:RestorePointNo') | Should -BeLessThan $a.IndexOf("Invoke-Section 'Optimierung: Wiederherstellungspunkt anlegen'")
            $a | Should -Match 'Get-RestorePointByDescription \$rpText'
        }
        It 'Wartung ruft Enable-ComputerRestore nur noch als Ersatz auf' {
            $w = Get-SrcText 'Module/Wartung/Ablauf.ps1'
            $w | Should -Match 'Enable-ComputerschutzMitProtokoll -Laufwerk'
            foreach ($m in 'Benchmark', 'Diagnose', 'Lasttest', 'Optimierung', 'Sensoren', 'Umzug') {
                foreach ($f in @(Get-ChildItem (Join-Path $global:MinibenchSrcRoot ('Module\' + $m)) -Filter '*.ps1')) {
                    [IO.File]::ReadAllText($f.FullName, [Text.Encoding]::UTF8) | Should -Not -Match 'Enable-ComputerRestore' -Because $f.Name
                }
            }
        }
    }

    Context 'A3 Datenträgerbereinigung' {
        BeforeEach {
            $script:keys = @('Active Setup Temp Folders', 'DownloadsFolder', 'Recycle Bin', 'Previous Installations', 'Windows ESD installation files', 'Temporary Files') | ForEach-Object { [pscustomobject]@{ PSChildName = $_; PSPath = ('Registry::HKEY_LOCAL_MACHINE\VC\' + $_) } }
            Mock -ModuleName MinibenchTest Get-ChildItem { $script:keys }
            Mock -ModuleName MinibenchTest New-ItemProperty { }
            Mock -ModuleName MinibenchTest Remove-ItemProperty { }
            Mock -ModuleName MinibenchTest Invoke-External { & $script:ok }
        }
        It 'Papierkorb, Windows.old, ESD-Dateien und Downloads sind ausgeschlossen' {
            $k = @(MinibenchTest\Get-CleanmgrKeys | ForEach-Object { $_.PSChildName })
            $k | Should -Be @('Active Setup Temp Folders', 'Temporary Files')
        }
        It 'Optimierung und Wartung setzen keine StateFlags für ausgeschlossene Kategorien' {
            [void](New-WartungRun)
            Mock -ModuleName MinibenchTest Get-OptFreeBytes { 1GB }
            $r = MinibenchTest\Invoke-OptSpecial ([pscustomobject]@{ Id = 'Dtb'; Titel = 'Dtb' }) ([pscustomobject]@{ Art = 'Sonder'; Name = 'Datentraegerbereinigung' }) $null
            $r.Status | Should -Be 'geaendert'
            Should -Invoke -ModuleName MinibenchTest New-ItemProperty -Times 2 -Exactly
            Should -Invoke -ModuleName MinibenchTest New-ItemProperty -Times 0 -Exactly -ParameterFilter { $LiteralPath -match 'Recycle Bin|Previous Installations|ESD|DownloadsFolder' }
            Mock -ModuleName MinibenchTest Get-Volume { [pscustomobject]@{ SizeRemaining = 1GB } }
            MinibenchTest\Invoke-WartungSchritt 'Datentraegerbereinigung'
            Should -Invoke -ModuleName MinibenchTest New-ItemProperty -Times 4 -Exactly
            Should -Invoke -ModuleName MinibenchTest New-ItemProperty -Times 0 -Exactly -ParameterFilter { $LiteralPath -match 'Recycle Bin|Previous Installations|ESD|DownloadsFolder' }
            @(Get-Eintraege | Where-Object { $_.Schritt -eq 'Datentraegerbereinigung' })[0].Ziel | Should -Match 'Papierkorb'
        }
        It 'die geplante Aufgabe schließt dieselben Kategorien aus' {
            $t = @(MinibenchTest\Get-OptMaintenanceTasks | Where-Object { $_.Name -eq 'Datenträgerbereinigung' })[0]
            foreach ($x in 'DownloadsFolder', 'Recycle Bin', 'Previous Installations', 'Windows ESD installation files') { $t.Befehl | Should -Match ([regex]::Escape("'" + $x + "'")) }
            $t.Befehl | Should -Match '-notcontains \$_\.PSChildName'
        }
    }

    Context 'A7 Werkzeuge nur mit fester Prüfsumme' {
        It 'feste Version und SHA-256 für DDU und NVIDIA Profile Inspector, kein "latest"' {
            foreach ($p in (Get-ModuleVar 'OptNpiPin'), (Get-ModuleVar 'OptDduPin')) {
                $p.Url | Should -Not -Match 'latest'
                $p.Url | Should -Match ([regex]::Escape($p.Version))
                $p.SHA256 | Should -Match '^[0-9a-f]{64}$'
            }
            @((Get-ModuleVar 'OptNpiExePins').Keys).Count | Should -BeGreaterThan 0
            @((Get-ModuleVar 'OptDduExePins').Keys).Count | Should -BeGreaterThan 0
            Get-SrcText 'Module/Optimierung/Funktionen.ps1' | Should -Not -Match 'winget(\.exe)? download'
        }
        It 'vorgefundene Programmdatei ohne bekannte Prüfsumme wird abgelehnt, mit bekannter übernommen' {
            $dir = New-WartungRun
            $npi = Join-Path $dir 'Tools\NvidiaProfileInspector\nvidiaProfileInspector.exe'
            New-Item -ItemType Directory -Path (Split-Path $npi -Parent) -Force | Out-Null
            [IO.File]::WriteAllText($npi, 'fremd')
            MinibenchTest\Get-OptNpiExe | Should -BeNullOrEmpty
            @(Get-ModuleVar 'ToolIssues') -join ' ' | Should -Match 'NVIDIA Profile Inspector abgelehnt'
            @((MinibenchTest\Read-ToolManifest).Werkzeuge).Count | Should -Be 0
            $h = MinibenchTest\Get-FileSha256 $npi
            $alt = Get-ModuleVar 'OptNpiExePins'
            try {
                Set-ModuleVar 'OptNpiExePins' ([ordered]@{ $h = '9.9' })
                MinibenchTest\Get-OptNpiExe | Should -Be $npi
                @((MinibenchTest\Read-ToolManifest).Werkzeuge | Where-Object { $_.Name -eq 'NvidiaProfileInspector' }).Count | Should -Be 1
            } finally { Set-ModuleVar 'OptNpiExePins' $alt }
            # schon im Manifest, aber nicht in der Liste (Vertrauen beim ersten Fund bis v4.10): abgelehnt
            MinibenchTest\Get-OptNpiExe | Should -BeNullOrEmpty
        }
        It 'DDU im Tools-Ordner nur mit bekannter Prüfsumme' {
            $dir = New-WartungRun
            $ddu = Join-Path $dir 'Tools\DDU\Display Driver Uninstaller.exe'
            New-Item -ItemType Directory -Path (Split-Path $ddu -Parent) -Force | Out-Null
            [IO.File]::WriteAllText($ddu, 'fremd')
            MinibenchTest\Get-OptDduExe | Should -BeNullOrEmpty
            @(Get-ModuleVar 'ToolIssues') -join ' ' | Should -Match 'DDU abgelehnt'
            [void](MinibenchTest\Register-Tool -Name 'DDU' -Path $ddu -ToolsDir (Join-Path $dir 'Tools'))
            MinibenchTest\Get-OptDduExe | Should -BeNullOrEmpty
            (MinibenchTest\Invoke-OptDdu ([pscustomobject]@{ Id = 'Ddu'; Titel = 'DDU' }) ([pscustomobject]@{ Neustart = $false })).Status | Should -Be 'Fehler'
            $alt = Get-ModuleVar 'OptDduExePins'
            try {
                Set-ModuleVar 'OptDduExePins' ([ordered]@{ (MinibenchTest\Get-FileSha256 $ddu) = '9.9' })
                MinibenchTest\Get-OptDduExe | Should -Be $ddu
            } finally { Set-ModuleVar 'OptDduExePins' $alt }
        }
        It 'Download mit falscher Prüfsumme wird verworfen, das DDU-Archiv nicht ausgeführt' {
            $dir = New-WartungRun
            Mock -ModuleName MinibenchTest Save-WebFile { [IO.File]::WriteAllText($Path, 'manipuliert') }
            Mock -ModuleName MinibenchTest Invoke-External { & $script:ok }
            $r = MinibenchTest\Install-OptTools
            $r.Ok | Should -BeFalse
            ($r.Meldungen -join ' ') | Should -Match 'DDU-Archiv hat eine unerwartete Prüfsumme'
            ($r.Meldungen -join ' ') | Should -Match 'nvidiaProfileInspector\.zip hat eine unerwartete Prüfsumme'
            Should -Invoke -ModuleName MinibenchTest Save-WebFile -Times 2 -Exactly
            Should -Invoke -ModuleName MinibenchTest Invoke-External -Times 0
            Test-Path -LiteralPath (Join-Path $dir 'Tools\NvidiaProfileInspector') | Should -BeFalse
            @((MinibenchTest\Read-ToolManifest).Werkzeuge).Count | Should -Be 0
        }
        It 'DDU-Archiv mit passender Prüfsumme wird entpackt, geprüft und übernommen' {
            $dir = New-WartungRun
            $probe = Join-Path $dir 'probe.bin'
            [IO.File]::WriteAllText($probe, 'sfx-inhalt'); $hSfx = MinibenchTest\Get-FileSha256 $probe
            [IO.File]::WriteAllText($probe, 'ddu-programm'); $hExe = MinibenchTest\Get-FileSha256 $probe
            $altPin = Get-ModuleVar 'OptDduPin'; $altExe = Get-ModuleVar 'OptDduExePins'
            try {
                $pin = $altPin.Clone(); $pin.SHA256 = $hSfx
                Set-ModuleVar 'OptDduPin' $pin
                Set-ModuleVar 'OptDduExePins' ([ordered]@{ $hExe = '9.9' })
                Mock -ModuleName MinibenchTest Save-WebFile { [IO.File]::WriteAllText($Path, 'sfx-inhalt') }
                Mock -ModuleName MinibenchTest Invoke-External {
                    if ($Arguments -match '-o"([^"]+)" -y') { $d = Join-Path $Matches[1] 'DDU v9.9'; New-Item -ItemType Directory -Path $d -Force | Out-Null; [IO.File]::WriteAllText((Join-Path $d 'Display Driver Uninstaller.exe'), 'ddu-programm') }
                    [pscustomobject]@{ ExitCode = 0; Output = ''; Error = ''; TimedOut = $false }
                }
                $r = MinibenchTest\Install-OptTools
                ($r.Meldungen -join ' ') | Should -Match 'Display Driver Uninstaller 9\.9 übernommen'
                MinibenchTest\Get-OptDduExe | Should -Be (Join-Path $dir 'Tools\DDU\Display Driver Uninstaller.exe')
            } finally { Set-ModuleVar 'OptDduPin' $altPin; Set-ModuleVar 'OptDduExePins' $altExe }
        }
    }

    Context 'E1 bis E4 Wartungsschritte' {
        It 'Netzwerk: IP-Konfiguration wird vor dem Zurücksetzen gesichert, Gegenbefehl netsh -f' {
            [void](New-WartungRun)
            $global:LmbTestAufrufe = New-Object System.Collections.Generic.List[string]
            Mock -ModuleName MinibenchTest Invoke-External { $global:LmbTestAufrufe.Add([string]$Arguments); [pscustomobject]@{ ExitCode = 0; Output = $(if ($Arguments -eq 'interface ip dump') { "pushd interface ipv4`r`nset address name=`"Ethernet`" source=static address=192.168.1.20`r`npopd" } else { 'ok' }); Error = ''; TimedOut = $false } }
            MinibenchTest\Invoke-WartungSchritt 'Netzwerk'
            $global:LmbTestAufrufe[0] | Should -Be 'interface ip dump'
            $global:LmbTestAufrufe | Should -Contain 'int ip reset'
            $e = @(Get-Eintraege | Where-Object { $_.Schritt -eq 'Netzwerk' })[0]
            $e.Status | Should -Be 'nur Hinweis'
            $e.Gegenbefehl | Should -Match 'netsh -f "'
            $e.Daten.Sicherung | Should -Match 'Netzwerk_ip_dump\.txt$'
            [IO.File]::ReadAllText($e.Daten.Sicherung) | Should -Match '192\.168\.1\.20'
            Remove-Variable -Name LmbTestAufrufe -Scope Global -ErrorAction SilentlyContinue
        }
        It 'Netzwerk: ohne Sicherung wird nichts zurückgesetzt' {
            [void](New-WartungRun)
            Mock -ModuleName MinibenchTest Invoke-External { [pscustomobject]@{ ExitCode = 1; Output = ''; Error = 'Fehler'; TimedOut = $false } }
            MinibenchTest\Invoke-WartungSchritt 'Netzwerk'
            Should -Invoke -ModuleName MinibenchTest Invoke-External -Times 1 -Exactly
            (Get-ModuleVar 'RepairLog')[0].Ergebnis | Should -Be 'Fehler'
        }
        It 'Temp, Druckaufträge, Komponentenbereinigung, Dateisystem, WMI und Geräte stehen im Änderungsprotokoll' {
            [void](New-WartungRun)
            Set-ModuleVar 'RestorePointNo' '42'
            Mock -ModuleName MinibenchTest Invoke-External { [pscustomobject]@{ ExitCode = 0; Output = $(if ($Arguments -eq '/verifyrepository') { 'Das WMI-Repository ist inkonsistent.' } else { 'ok' }); Error = ''; TimedOut = $false } }
            Mock -ModuleName MinibenchTest Remove-OldFiles { 0.0 }
            Mock -ModuleName MinibenchTest Get-ChildItem { @([pscustomobject]@{ FullName = 'C:\x\1.SPL' }) }
            Mock -ModuleName MinibenchTest Remove-Item { }
            Mock -ModuleName MinibenchTest Stop-Service { }
            Mock -ModuleName MinibenchTest Start-Service { }
            Mock -ModuleName MinibenchTest Get-Service { [pscustomobject]@{ Status = 'Running'; StartType = 'Manual' } }
            Mock -ModuleName MinibenchTest Get-Volume { [pscustomobject]@{ DriveLetter = ($env:SystemDrive.Substring(0, 1)); DriveType = 'Fixed'; FileSystem = 'NTFS'; SizeRemaining = 1GB } }
            Mock -ModuleName MinibenchTest Repair-Volume { 'ErrorsFound' }
            Mock -ModuleName MinibenchTest Get-CimInstance { @() }
            Mock -ModuleName MinibenchTest Start-Sleep { }
            Mock -ModuleName MinibenchTest Test-Path { $true }
            foreach ($k in 'Temp', 'Druck', 'Komponentenbereinigung', 'Dateisystem', 'WMI', 'Geraete') { MinibenchTest\Invoke-WartungSchritt $k }
            $e = @(Get-Eintraege)
            foreach ($k in 'Temp', 'Druck', 'Komponentenbereinigung', 'Dateisystem', 'WMI', 'Geraete') {
                $x = @($e | Where-Object { $_.Schritt -eq $k })
                $x.Count | Should -BeGreaterThan 0 -Because $k
                $x[0].Status | Should -Be 'nur Hinweis' -Because $k
                $x[0].Gegenbefehl | Should -Not -BeNullOrEmpty -Because $k
            }
            @($e | Where-Object { $_.Schritt -eq 'Dateisystem' })[0].Ziel | Should -Match 'fsutil dirty set'
            @($e | Where-Object { $_.Schritt -eq 'WMI' })[0].Gegenbefehl | Should -Match 'Nr\. 42'
            @($e | Where-Object { $_.Schritt -eq 'Temp' })[0].Gegenbefehl | Should -Match 'nicht umkehrbar'
        }
        It 'DISM und SFC nennen den Wiederherstellungspunkt als Weg zurück' {
            [void](New-WartungRun)
            Mock -ModuleName MinibenchTest Invoke-DismJob { [pscustomobject]@{ State = $(if ($Mode -eq 'ScanHealth') { 'Repairable' } else { 'Healthy' }); Restart = $false; Error = '' } }
            Mock -ModuleName MinibenchTest Get-CbsEntries { [pscustomobject]@{ Cannot = @(); Files = @() } }
            Mock -ModuleName MinibenchTest Invoke-External { [pscustomobject]@{ ExitCode = 0; Output = 'Der Windows-Ressourcenschutz hat beschädigte Dateien gefunden und erfolgreich repariert.'; Error = ''; TimedOut = $false } }
            MinibenchTest\Invoke-WartungSchritt 'DismRestore'
            MinibenchTest\Invoke-WartungSchritt 'Sfc'
            foreach ($k in 'DismRestore', 'Sfc') { @(Get-Eintraege | Where-Object { $_.Schritt -eq $k })[0].Gegenbefehl | Should -Match 'kein Wiederherstellungspunkt' -Because $k }
        }
        It 'ShaderCache und Absturzabbilder: Profil des angemeldeten Benutzers, von AMD nur die Cache-Ordner' {
            [void](New-WartungRun)
            $anna = [pscustomobject]@{ Sid = 'S-1-5-21-1'; Name = 'PC\anna'; Profil = 'C:\Users\anna'; Eigen = $false; Quelle = 'test' }
            $p = @(MinibenchTest\Get-WartungShaderCachePfade $anna)
            $p | Should -Contain 'C:\Users\anna\AppData\Local\AMD\DxCache'
            $p | Should -Contain 'C:\Users\anna\AppData\Local\AMD\VkCache'
            $p | Should -Contain 'C:\Users\anna\AppData\LocalLow\AMD\DxcCache'
            $p | Should -Not -Contain 'C:\Users\anna\AppData\Local\AMD'
            $p | Should -Not -Contain 'C:\Users\anna\AppData\LocalLow\AMD'
            foreach ($x in $p) { $x | Should -Match '^C:\\Users\\anna\\' }
            Mock -ModuleName MinibenchTest Get-OptTargetUser { $anna }
            Mock -ModuleName MinibenchTest Test-Path { $true }
            Mock -ModuleName MinibenchTest Clear-Folder { 0.0 }
            MinibenchTest\Invoke-WartungSchritt 'ShaderCache'
            MinibenchTest\Invoke-WartungSchritt 'Absturzabbilder'
            Should -Invoke -ModuleName MinibenchTest Clear-Folder -Times 0 -Exactly -ParameterFilter { $Path -match '\\AMD$' -and $Path -notmatch '^[A-Z]:\\AMD$' }
            Should -Invoke -ModuleName MinibenchTest Clear-Folder -Times 1 -Exactly -ParameterFilter { $Path -eq 'C:\Users\anna\AppData\Local\AMD\GLCache' }
            Should -Invoke -ModuleName MinibenchTest Clear-Folder -Times 1 -Exactly -ParameterFilter { $Path -eq 'C:\Users\anna\AppData\Local\CrashDumps' }
            Should -Invoke -ModuleName MinibenchTest Clear-Folder -Times 0 -Exactly -ParameterFilter { $Path -like ($env:LOCALAPPDATA + '*') -and $env:LOCALAPPDATA -notlike 'C:\Users\anna*' }
        }
    }
}
