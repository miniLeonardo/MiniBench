# Änderungsprotokoll und Rückgängig. Registry, Dienste und powercfg werden durch Attrappen ersetzt.
BeforeAll {
    . (Join-Path $PSScriptRoot 'Hilfen.ps1')
    Import-MinibenchTestModule -Parts 'Kern\Risiko.ps1', 'Kern\Geraeteidentitaet.ps1', 'Kern\Aenderungen.ps1' -Functions 'Get-SafeName', 'Invoke-External', 'Show-Sub', 'Hide-Sub', 'Write-Heartbeat', 'Send-GuiEvent'
    function New-Run {
        $dir = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $dir | Out-Null
        Set-ModuleVar 'DataDir' $dir
        Set-ModuleVar 'ChangeLog' $null
        Set-ModuleVar 'DeviceIdentity' ([pscustomobject]@{ Id = 'G-aaaaaaaaaaaaaaaaaaaa'; Guete = 'hoch'; Quellen = @() })
        MinibenchTest\Start-ChangeLog
        return $dir
    }
    Mock -ModuleName MinibenchTest Write-Host { }
    function Get-RunFile { Get-ModuleVar 'ChangeFile' }
    function Read-Run { Get-Content -LiteralPath (Get-RunFile) -Raw -Encoding UTF8 | ConvertFrom-Json }
}

Describe 'Registry-Änderung' {
    BeforeEach {
        $null = New-Run
        $script:reg = @{ Vorhanden = $true; Wert = 1; Typ = 'DWord' }
        Mock -ModuleName MinibenchTest Get-RegValueState { [pscustomobject]$script:reg }
        Mock -ModuleName MinibenchTest Set-RegValueState { $script:reg = @{ Vorhanden = $true; Wert = $Value; Typ = $Type } }
        Mock -ModuleName MinibenchTest Remove-RegValue { $script:reg = @{ Vorhanden = $false; Wert = $null; Typ = '' } }
    }
    It 'protokolliert Vorher- und Nachher-Wert, bevor geschrieben wird' {
        $r = MinibenchTest\Set-RegistryValueLogged -Path 'HKLM:\SYSTEM\Test' -Name 'HiberbootEnabled' -Value 0 -Type 'DWord' -Modul 'Reparatur' -Schritt 'Schnellstart' -Titel 'Schnellstart deaktivieren'
        $r.Vorher | Should -Be '1'
        $r.Nachher | Should -Be '0'
        $r.Status | Should -Be 'aktiv'
        Should -Invoke -ModuleName MinibenchTest Set-RegValueState -Times 1
        $j = Read-Run
        $j.Format | Should -Be 'Minibench-Aenderungen/1'
        $j.GeraetId | Should -Be 'G-aaaaaaaaaaaaaaaaaaaa'
        @($j.Eintraege).Count | Should -Be 1
        (Get-RunFile) | Should -Match ([regex]::Escape([IO.Path]::Combine('Änderungen', (MinibenchTest\Get-SafeName $env:COMPUTERNAME))))
    }
    It 'schreibt nichts, wenn der Wert schon stimmt' {
        $script:reg = @{ Vorhanden = $true; Wert = 0; Typ = 'DWord' }
        MinibenchTest\Set-RegistryValueLogged -Path 'HKLM:\SYSTEM\Test' -Name 'HiberbootEnabled' -Value 0 -Modul 'R' -Schritt 'S' -Titel 'T' | Should -BeNullOrEmpty
        Should -Invoke -ModuleName MinibenchTest Set-RegValueState -Times 0
        Get-RunFile | Should -Not -Exist
    }
    It 'nimmt die Änderung zurück und vermerkt das' {
        [void](MinibenchTest\Set-RegistryValueLogged -Path 'HKLM:\SYSTEM\Test' -Name 'HiberbootEnabled' -Value 0 -Modul 'R' -Schritt 'S' -Titel 'T')
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 1
        $script:reg.Wert | Should -Be 1
        $script:reg.Typ | Should -Be 'DWord'
        (Read-Run).Eintraege[0].Status | Should -Be 'rückgängig'
        # ein zweites Mal passiert nichts
        $u2 = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u2.Rueckgaengig | Should -Be 0
        Should -Invoke -ModuleName MinibenchTest Set-RegValueState -Times 2
    }
    It 'überspringt, wenn der Wert inzwischen anders ist' {
        [void](MinibenchTest\Set-RegistryValueLogged -Path 'HKLM:\SYSTEM\Test' -Name 'HiberbootEnabled' -Value 0 -Modul 'R' -Schritt 'S' -Titel 'T')
        $script:reg = @{ Vorhanden = $true; Wert = 5; Typ = 'DWord' }
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Probleme | Should -Be 1
        $script:reg.Wert | Should -Be 5
        (Read-Run).Eintraege[0].Status | Should -Be 'übersprungen'
    }
    It 'entfernt einen Wert, der vorher nicht da war' {
        $script:reg = @{ Vorhanden = $false; Wert = $null; Typ = '' }
        [void](MinibenchTest\Set-RegistryValueLogged -Path 'HKLM:\SOFTWARE\Test' -Name 'Neu' -Value 'abc' -Type 'String' -Modul 'R' -Schritt 'S' -Titel 'T')
        [void](MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile)))
        Should -Invoke -ModuleName MinibenchTest Remove-RegValue -Times 1
        $script:reg.Vorhanden | Should -BeFalse
    }
    It 'stellt Binärwerte nach dem Weg über JSON korrekt wieder her' {
        $script:reg = @{ Vorhanden = $true; Wert = [byte[]](1, 2, 255); Typ = 'Binary' }
        [void](MinibenchTest\Set-RegistryValueLogged -Path 'HKLM:\SOFTWARE\Test' -Name 'Bin' -Value ([byte[]](9, 9)) -Type 'Binary' -Modul 'R' -Schritt 'S' -Titel 'T')
        [void](MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile)))
        ($script:reg.Wert -join ',') | Should -Be '1,2,255'
        $script:reg.Typ | Should -Be 'Binary'
        # Set-RegValueState wandelt vor dem Schreiben in den Registry-Typ um
        (MinibenchTest\ConvertTo-RegValue $script:reg.Wert 'Binary').GetType().Name | Should -Be 'Byte[]'
        (MinibenchTest\ConvertTo-RegValue ([long]0) 'DWord').GetType().Name | Should -Be 'Int32'
        (MinibenchTest\ConvertTo-RegValue @('a', 'b') 'MultiString').GetType().Name | Should -Be 'String[]'
    }
}

Describe 'Schutz beim Rückgängig' {
    BeforeEach {
        $null = New-Run
        $script:reg = @{ Vorhanden = $true; Wert = 1; Typ = 'DWord' }
        Mock -ModuleName MinibenchTest Get-RegValueState { [pscustomobject]$script:reg }
        Mock -ModuleName MinibenchTest Set-RegValueState { $script:reg = @{ Vorhanden = $true; Wert = $Value; Typ = $Type } }
        [void](MinibenchTest\Set-RegistryValueLogged -Path 'HKLM:\SYSTEM\Test' -Name 'X' -Value 0 -Modul 'R' -Schritt 'S' -Titel 'T')
    }
    It 'verweigert Rückgängig auf einem anderen Gerät' {
        Set-ModuleVar 'DeviceIdentity' ([pscustomobject]@{ Id = 'G-bbbbbbbbbbbbbbbbbbbb'; Guete = 'hoch'; Quellen = @() })
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 0
        $u.Probleme | Should -Be 1
        $script:reg.Wert | Should -Be 0
    }
    It 'verweigert Dateien außerhalb des Protokollordners' {
        $fremd = Join-Path $TestDrive 'fremd.json'
        Copy-Item (Get-RunFile) $fremd
        (MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f $fremd)).Probleme | Should -Be 1
        $script:reg.Wert | Should -Be 0
    }
    It 'ignoriert unsinnige Auswahl' {
        (MinibenchTest\Invoke-ChangeUndo ('{0}*abc;;nur-text' -f (Get-RunFile))).Rueckgaengig | Should -Be 0
    }
}

Describe 'Dienst-Starttyp' {
    BeforeEach {
        $null = New-Run
        $script:svc = 'Disabled'
        Mock -ModuleName MinibenchTest Get-ServiceStartType { $script:svc }
        Mock -ModuleName MinibenchTest Set-ServiceStartType { $script:svc = $StartType }
    }
    It 'protokolliert und nimmt zurück' {
        $r = MinibenchTest\Set-ServiceStartTypeLogged -Name 'w32time' -StartType 'Manual' -Modul 'Reparatur' -Schritt 'Zeit' -Titel 'Zeit synchronisieren'
        $r.Vorher | Should -Be 'Disabled'
        $script:svc | Should -Be 'Manual'
        [void](MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile)))
        $script:svc | Should -Be 'Disabled'
    }
    It 'tut nichts, wenn der Starttyp schon stimmt' {
        $script:svc = 'Manual'
        MinibenchTest\Set-ServiceStartTypeLogged -Name 'w32time' -StartType 'Manual' -Modul 'R' -Schritt 'S' -Titel 'T' | Should -BeNullOrEmpty
        Should -Invoke -ModuleName MinibenchTest Set-ServiceStartType -Times 0
    }
}

Describe 'Energiepläne' {
    BeforeEach {
        $null = New-Run
        # Ausgangslage: Ausbalanciert, Höchstleistung und ein eigener Plan (aktiv)
        $script:plans = [ordered]@{ '381b4222-f694-41f0-9685-ff5bb260df2e' = 'Ausbalanciert'; '8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c' = 'Höchstleistung'; '11111111-2222-3333-4444-555555555555' = 'Werkstatt' }
        $script:active = '11111111-2222-3333-4444-555555555555'
        $script:inhalt = @{ '381b4222-f694-41f0-9685-ff5bb260df2e' = 'eigene Einstellungen'; '11111111-2222-3333-4444-555555555555' = 'Werkstatt-Einstellungen' }
        $script:importFehler = $false
        Mock -ModuleName MinibenchTest Invoke-PowerCfg {
            $a = $Arguments
            if ($a -eq '/list') {
                $t = "Bestehende Energieschemas (* Aktiv)`r`n-----------------------------------`r`n"
                foreach ($g in $script:plans.Keys) { $t += ('GUID des Energieschemas: {0}  ({1}){2}' -f $g, $script:plans[$g], $(if ($g -eq $script:active) { ' *' } else { '' })) + "`r`n" }
                return [pscustomobject]@{ ExitCode = 0; Output = $t; Error = '' }
            }
            # Inhalt eines Plans: $script:inhalt[GUID] (Standard "werk"); Export schreibt ihn in die Datei, Import liest ihn zurück
            if ($a -match '^/export "(.+)" (\S+)$') {
                if (-not $script:plans.Contains($Matches[2])) { return [pscustomobject]@{ ExitCode = 1; Output = ''; Error = '' } }
                $c = $(if ($script:inhalt.ContainsKey($Matches[2])) { $script:inhalt[$Matches[2]] } else { 'werk' })
                Set-Content -LiteralPath $Matches[1] -Value $c; return [pscustomobject]@{ ExitCode = 0; Output = ''; Error = '' }
            }
            # wie powercfg: Import mit vorhandener GUID scheitert
            if ($a -match '^/import "(.+)" (\S+)$') {
                if ($script:plans.Contains($Matches[2]) -or $script:importFehler) { return [pscustomobject]@{ ExitCode = 1; Output = ''; Error = '' } }
                $script:plans[$Matches[2]] = 'importiert'; $script:inhalt[$Matches[2]] = (Get-Content -LiteralPath $Matches[1] -Raw).Trim()
                return [pscustomobject]@{ ExitCode = 0; Output = ''; Error = '' }
            }
            # wie powercfg: der aktive Plan lässt sich nicht löschen
            if ($a -match '^/delete (\S+)$') {
                if ($Matches[1] -eq $script:active -or -not $script:plans.Contains($Matches[1])) { return [pscustomobject]@{ ExitCode = 1; Output = ''; Error = '' } }
                $script:plans.Remove($Matches[1]); $script:inhalt.Remove($Matches[1]); return [pscustomobject]@{ ExitCode = 0; Output = ''; Error = '' }
            }
            if ($a -match '^/setactive (\S+)$') { if ($script:plans.Contains($Matches[1])) { $script:active = $Matches[1]; return [pscustomobject]@{ ExitCode = 0 } } else { return [pscustomobject]@{ ExitCode = 1 } } }
            return [pscustomobject]@{ ExitCode = 1; Output = ''; Error = 'unbekannt' }
        }
    }
    It 'sichert alle Pläne in den Ordner des Laufs' {
        $r = MinibenchTest\Backup-PowerSchemesLogged -Modul 'Reparatur' -Schritt 'Energieplaene' -Titel 'Energiesparpläne zurücksetzen'
        $r.Vorher | Should -Match '3 Pläne, aktiv: Werkstatt'
        @(Get-ChildItem (MinibenchTest\Get-ChangeFilesDir) -Filter '*.pow').Count | Should -Be 3
    }
    It 'stellt nach dem Zurücksetzen den eigenen Plan und den aktiven Plan wieder her' {
        [void](MinibenchTest\Backup-PowerSchemesLogged -Modul 'Reparatur' -Schritt 'Energieplaene' -Titel 'Energiesparpläne zurücksetzen')
        # powercfg -restoredefaultschemes: eigener Plan weg, Ausbalanciert aktiv
        $script:plans.Remove('11111111-2222-3333-4444-555555555555'); $script:active = '381b4222-f694-41f0-9685-ff5bb260df2e'
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 1
        $script:plans.Contains('11111111-2222-3333-4444-555555555555') | Should -BeTrue
        $script:active | Should -Be '11111111-2222-3333-4444-555555555555'
        # ab v4.11 alle drei: der fehlende eigene Plan und die beiden noch vorhandenen Standardpläne
        Should -Invoke -ModuleName MinibenchTest Invoke-PowerCfg -ParameterFilter { $Arguments -like '/import*' } -Times 3 -Exactly
        $script:inhalt['11111111-2222-3333-4444-555555555555'] | Should -Be 'Werkstatt-Einstellungen'
    }
    # ab v4.11 (Befund D3): bis v4.10 übersprang Rückgängig Pläne, deren GUID noch existierte
    It 'stellt die Einstellungen eines noch vorhandenen, zurückgesetzten Standardplans wieder her, auch wenn er gerade aktiv ist' {
        [void](MinibenchTest\Backup-PowerSchemesLogged -Modul 'Reparatur' -Schritt 'Energieplaene' -Titel 'Energiesparpläne zurücksetzen')
        # powercfg -restoredefaultschemes: Ausbalanciert auf Werkseinstellung und aktiv, eigener Plan weg
        $script:inhalt['381b4222-f694-41f0-9685-ff5bb260df2e'] = 'werk'
        $script:plans.Remove('11111111-2222-3333-4444-555555555555'); $script:active = '381b4222-f694-41f0-9685-ff5bb260df2e'
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 1
        $script:inhalt['381b4222-f694-41f0-9685-ff5bb260df2e'] | Should -Be 'eigene Einstellungen'
        $script:plans.Contains('381b4222-f694-41f0-9685-ff5bb260df2e') | Should -BeTrue
        $script:plans.Contains('8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c') | Should -BeTrue
        $script:active | Should -Be '11111111-2222-3333-4444-555555555555'
        (Read-Run).Eintraege[0].Status | Should -Be 'rückgängig'
        # Zwischensicherungen räumt die Rücknahme nach Erfolg weg, die Sicherungen des Laufs bleiben
        @(Get-ChildItem (MinibenchTest\Get-ChangeFilesDir) -Filter '*_vor_Ruecknahme.pow').Count | Should -Be 0
        @(Get-ChildItem (MinibenchTest\Get-ChangeFilesDir) -Filter '*.pow').Count | Should -Be 3
    }
    It 'spielt bei gescheitertem Import den Stand vor der Rücknahme wieder ein und lässt den Eintrag aktiv' {
        [void](MinibenchTest\Backup-PowerSchemesLogged -Modul 'Reparatur' -Schritt 'Energieplaene' -Titel 'Energiesparpläne zurücksetzen')
        $script:inhalt['381b4222-f694-41f0-9685-ff5bb260df2e'] = 'werk'
        # erster Import (aus der Sicherung) scheitert, der zweite (Stand vor der Rücknahme) gelingt
        $script:importVersuche = 0
        Mock -ModuleName MinibenchTest Invoke-PowerCfg {
            $a = $Arguments
            if ($a -eq '/list') { return [pscustomobject]@{ ExitCode = 0; Output = "GUID des Energieschemas: 381b4222-f694-41f0-9685-ff5bb260df2e  (Ausbalanciert) *`r`nGUID des Energieschemas: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c  (Höchstleistung)"; Error = '' } }
            if ($a -match '^/export "(.+)" (\S+)$') { Set-Content -LiteralPath $Matches[1] -Value 'werk'; return [pscustomobject]@{ ExitCode = 0 } }
            if ($a -match '^/import "(.+)" (\S+)$') {
                $script:importVersuche++
                if ($Matches[1] -like '*_vor_Ruecknahme.pow') { $script:zurueck = $Matches[2]; return [pscustomobject]@{ ExitCode = 0 } }
                return [pscustomobject]@{ ExitCode = 5 }
            }
            return [pscustomobject]@{ ExitCode = 0 }
        }
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 0
        $script:zurueck | Should -Not -BeNullOrEmpty
        $e = (Read-Run).Eintraege[0]
        $e.Status | Should -Be 'aktiv'
        $e.Ergebnis | Should -Match 'fehlgeschlagen'
        $e.Ergebnis | Should -Match 'Stand vor der Rücknahme wieder eingespielt'
    }
    It 'bricht ab, wenn powercfg keine Pläne liefert' {
        $script:plans = [ordered]@{}
        { MinibenchTest\Backup-PowerSchemesLogged -Modul 'R' -Schritt 'S' -Titel 'T' } | Should -Throw '*keine Energiepläne*'
    }
}

Describe 'Liste der Energiepläne' {
    It 'liest deutsche und englische Ausgabe von powercfg /list' {
        $de = "GUID des Energieschemas: 381b4222-f694-41f0-9685-ff5bb260df2e  (Ausbalanciert) *`r`nGUID des Energieschemas: a1841308-3541-4fab-bc81-f71556f20b4a  (Energiesparmodus)"
        $en = 'Power Scheme GUID: 8C5E7FDA-E8BF-4A96-9A85-A6E23A8C635C  (High performance)'
        $p = @(MinibenchTest\ConvertFrom-PowerCfgList $de)
        $p.Count | Should -Be 2
        $p[0].Aktiv | Should -BeTrue
        $p[1].Name | Should -Be 'Energiesparmodus'
        (MinibenchTest\ConvertFrom-PowerCfgList $en).Guid | Should -Be '8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c'
    }
}

Describe 'Hinweise und Übersicht' {
    It 'Hinweise lassen sich nicht automatisch zurücknehmen' {
        $null = New-Run
        [void](MinibenchTest\Add-ChangeRecord -Modul 'Reparatur' -Schritt 'Wiederherstellungspunkt' -Titel 'Wiederherstellungspunkt' -Risiko 'Lesen' -Art 'Wiederherstellungspunkt' -Ziel 'Computerschutz' -Gegenbefehl 'rstrui.exe' -NurHinweis)
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 0
        (Read-Run).Eintraege[0].Status | Should -Be 'nur Hinweis'
    }
    It 'Läufe ohne Änderung hinterlassen keine Datei' {
        $dir = New-Run
        Join-Path $dir 'Änderungen' | Should -Not -Exist
    }
    It 'listet Einträge aller Läufe, neueste zuerst' {
        $dir = New-Run
        [void](MinibenchTest\Add-ChangeRecord -Modul 'R' -Schritt 'A' -Titel 'Erster' -Risiko 'Aendern' -Art 'Hinweis' -Ziel 'x' -NurHinweis)
        Start-Sleep -Milliseconds 1100
        [void](MinibenchTest\Add-ChangeRecord -Modul 'R' -Schritt 'B' -Titel 'Zweiter' -Risiko 'Aendern' -Art 'Hinweis' -Ziel 'y' -NurHinweis)
        $l = @(MinibenchTest\Get-ChangeEntries (Join-Path $dir 'Änderungen'))
        $l.Count | Should -Be 2
        $l[0].Titel | Should -Be 'Zweiter'
    }
}

Describe 'Vorübergehende Änderung eines abgebrochenen Laufs (GPU-Wahl für winsat.exe)' {
    BeforeEach {
        $null = New-Run
        $script:reg = @{ Vorhanden = $false; Wert = $null; Typ = '' }
        Mock -ModuleName MinibenchTest Get-RegValueState { [pscustomobject]$script:reg }
        Mock -ModuleName MinibenchTest Set-RegValueState { $script:reg = @{ Vorhanden = $true; Wert = $Value; Typ = $Type } }
        Mock -ModuleName MinibenchTest Remove-RegValue { $script:reg = @{ Vorhanden = $false; Wert = $null; Typ = '' } }
    }
    It 'Undo-ChangeNow nimmt sie direkt nach der Messung zurück' {
        $r = MinibenchTest\Set-RegistryValueLogged -Path 'HKCU:\Software\Microsoft\DirectX\UserGpuPreferences' -Name 'C:\Windows\System32\winsat.exe' -Value 'GpuPreference=2;' -Type 'String' -Modul 'Benchmark' -Schritt 'GpuWahl' -Titel 'WinSAT auf die Grafikkarte'
        (MinibenchTest\Undo-ChangeNow $r).Status | Should -Be 'rückgängig'
        $script:reg.Vorhanden | Should -BeFalse
        (Read-Run).Eintraege[0].Status | Should -Be 'rückgängig'
    }
    It 'der nächste Start nimmt eine liegen gebliebene GPU-Wahl zurück, andere aktive Änderungen bleiben' {
        [void](MinibenchTest\Set-RegistryValueLogged -Path 'HKCU:\Software\Microsoft\DirectX\UserGpuPreferences' -Name 'C:\Windows\System32\winsat.exe' -Value 'GpuPreference=2;' -Type 'String' -Modul 'Benchmark' -Schritt 'GpuWahl' -Titel 'WinSAT auf die Grafikkarte')
        [void](MinibenchTest\Add-ChangeRecord -Modul 'Reparatur' -Schritt 'Schnellstart' -Titel 'bleibt' -Risiko 'Aendern' -Art 'Registry' -Ziel 'x' -Daten ([ordered]@{ Pfad = 'HKLM:\X'; Name = 'Y'; VorherVorhanden = $false; NachherWert = 0 }))
        $alt = Get-RunFile
        # neuer Lauf mit eigener Protokolldatei
        Set-ModuleVar 'ChangeLog' $null; Start-Sleep -Milliseconds 1100; MinibenchTest\Start-ChangeLog
        $u = MinibenchTest\Restore-TemporaryChanges
        $u.Rueckgaengig | Should -Be 1
        $script:reg.Vorhanden | Should -BeFalse
        $j = Get-Content -LiteralPath $alt -Raw -Encoding UTF8 | ConvertFrom-Json
        @($j.Eintraege | ForEach-Object { $_.Status }) | Should -Be @('rückgängig', 'aktiv')
        MinibenchTest\Restore-TemporaryChanges | Should -BeNullOrEmpty
    }
}

# Seite Tools (ab v3.53): Softwarepakete über winget im Arbeitsprozess, mit Frist, Änderungsprotokoll und Rückgängig.
# winget selbst ist eine Attrappe (Invoke-Winget); sie merkt sich, was "installiert" ist.
Describe 'Softwarepakete über winget' {
    BeforeAll {
        Import-MinibenchTestModule -Parts 'Kern\Risiko.ps1', 'Kern\Geraeteidentitaet.ps1', 'Kern\Aenderungen.ps1', 'Kern\Softwarepakete.ps1' -Functions 'Get-SafeName', 'Invoke-External', 'Show-Sub', 'Hide-Sub', 'Write-Heartbeat', 'Send-GuiEvent'
        Mock -ModuleName MinibenchTest Write-Host { }
        $script:NichtGefunden = -1978335212   # 0x8A150014
    }
    BeforeEach {
        $null = New-Run
        $script:Inst = @{}
        $script:InstallErgebnis = $null
        $script:ListErgebnis = $null
        Mock -ModuleName MinibenchTest Find-Winget { 'winget.exe' }
        Mock -ModuleName MinibenchTest Invoke-Winget {
            $id = ([string]$Arguments -split ' ')[2]
            if ($Arguments -like 'list *') {
                if ($script:ListErgebnis) { return $script:ListErgebnis }
                if ($script:Inst.ContainsKey($id)) { return [pscustomobject]@{ ExitCode = 0; Output = ('Name  Id  Version`n7-Zip {0} 24.08' -f $id); Error = ''; TimedOut = $false } }
                return [pscustomobject]@{ ExitCode = $script:NichtGefunden; Output = 'Kein installiertes Paket gefunden'; Error = ''; TimedOut = $false }
            }
            if ($Arguments -like 'install *') {
                if ($script:InstallErgebnis) { return $script:InstallErgebnis }
                $script:Inst[$id] = $true; return [pscustomobject]@{ ExitCode = 0; Output = 'Erfolgreich installiert'; Error = ''; TimedOut = $false }
            }
            if ($Arguments -like 'uninstall *') { $script:Inst.Remove($id); return [pscustomobject]@{ ExitCode = 0; Output = 'Erfolgreich deinstalliert'; Error = ''; TimedOut = $false } }
            throw ('unerwarteter Aufruf: ' + $Arguments)
        }
    }
    It 'installiert, protokolliert als Eingriff mit Gegenbefehl und nimmt die Installation über Rückgängig wieder heraus' {
        $r = MinibenchTest\Invoke-SoftwarePakete '7zip.7zip=7-Zip'
        $r.Installiert | Should -Be 1
        $r.Fehler | Should -Be 0
        $script:Inst.ContainsKey('7zip.7zip') | Should -BeTrue
        $e = @((Read-Run).Eintraege)
        $e.Count | Should -Be 1
        $e[0].Art | Should -Be 'Softwarepaket'
        $e[0].Risiko | Should -Be 'Eingriff'
        $e[0].Modul | Should -Be 'Tools'
        $e[0].Gegenbefehl | Should -Be 'winget uninstall --id 7zip.7zip -e'
        $e[0].Daten.PaketId | Should -Be '7zip.7zip'
        $e[0].Status | Should -Be 'aktiv'
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 1
        $script:Inst.ContainsKey('7zip.7zip') | Should -BeFalse
        (Read-Run).Eintraege[0].Status | Should -Be 'rückgängig'
    }
    It 'lässt ein schon installiertes Programm unberührt und protokolliert nichts' {
        $script:Inst['Mozilla.Firefox'] = $true
        $r = MinibenchTest\Invoke-SoftwarePakete 'Mozilla.Firefox=Mozilla Firefox'
        $r.Bereits | Should -Be 1
        $r.Installiert | Should -Be 0
        Should -Invoke -ModuleName MinibenchTest Invoke-Winget -Times 0 -ParameterFilter { $Arguments -like 'install *' }
        Get-RunFile | Should -Not -Exist
    }
    It 'meldet Zeitüberschreitung und Fehlercodes und protokolliert dann nichts' {
        $script:InstallErgebnis = [pscustomobject]@{ ExitCode = -2; Output = ''; Error = ''; TimedOut = $true }
        $a = MinibenchTest\Invoke-SoftwarePakete 'Valve.Steam=Steam'
        $a.Ergebnisse[0].Status | Should -Be 'Zeitüberschreitung'
        $script:InstallErgebnis = [pscustomobject]@{ ExitCode = [int]-1978335217; Output = 'Fehler beim Öffnen der Quelle'; Error = ''; TimedOut = $false }
        $b = MinibenchTest\Invoke-SoftwarePakete 'Discord.Discord=Discord'
        $b.Ergebnisse[0].Status | Should -Be 'fehlgeschlagen'
        $b.Ergebnisse[0].Text | Should -Match '0x8A15000F'
        ($a.Fehler + $b.Fehler) | Should -Be 2
        Get-RunFile | Should -Not -Exist
    }
    It 'installiert nicht, wenn winget den Zustand nicht feststellen kann (defekte Quelle, Frist)' {
        $script:ListErgebnis = [pscustomobject]@{ ExitCode = [int]-1978335217; Output = 'Fehler beim Öffnen der Quelle'; Error = ''; TimedOut = $false }
        $a = MinibenchTest\Invoke-SoftwarePakete 'Google.Chrome=Google Chrome'
        $script:ListErgebnis = [pscustomobject]@{ ExitCode = -2; Output = ''; Error = ''; TimedOut = $true }
        $b = MinibenchTest\Invoke-SoftwarePakete 'Mozilla.Firefox=Mozilla Firefox'
        $a.Ergebnisse[0].Status | Should -Be 'fehlgeschlagen'
        $b.Ergebnisse[0].Status | Should -Be 'fehlgeschlagen'
        Should -Invoke -ModuleName MinibenchTest Invoke-Winget -Times 0 -ParameterFilter { $Arguments -like 'install *' }
        Get-RunFile | Should -Not -Exist
    }
    It 'installiert ohne Aktualisierung eines vorhandenen Programms; "schon installiert" von winget wird nicht protokolliert' {
        $script:InstallErgebnis = [pscustomobject]@{ ExitCode = [int]-1978335135; Output = 'Ein vorhandenes Paket wurde bereits installiert'; Error = ''; TimedOut = $false }   # 0x8A150061
        $r = MinibenchTest\Invoke-SoftwarePakete 'Notepad++.Notepad++=Notepad++'
        $r.Ergebnisse[0].Status | Should -Be 'bereits installiert'
        Should -Invoke -ModuleName MinibenchTest Invoke-Winget -Times 1 -ParameterFilter { $Arguments -like 'install *--no-upgrade*' }
        Get-RunFile | Should -Not -Exist
    }
    It 'Neustart nötig (0x8A150109) gilt als installiert und wird protokolliert' {
        $script:InstallErgebnis = [pscustomobject]@{ ExitCode = [int]-1978334967; Output = 'Neustart erforderlich'; Error = ''; TimedOut = $false }   # 0x8A150109
        $r = MinibenchTest\Invoke-SoftwarePakete 'ONLYOFFICE.DesktopEditors=ONLYOFFICE'
        $r.Installiert | Should -Be 1
        $r.Ergebnisse[0].Text | Should -Match 'Neustart'
        (Read-Run).Eintraege[0].Nachher | Should -Be 'installiert, Neustart nötig'
    }
    It 'Rückgängig lässt den Eintrag aktiv, wenn winget den Zustand nicht feststellen kann' {
        [void](MinibenchTest\Invoke-SoftwarePakete 'VideoLAN.VLC=VLC')
        $script:ListErgebnis = [pscustomobject]@{ ExitCode = [int]-1978335217; Output = ''; Error = ''; TimedOut = $false }
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 0
        (Read-Run).Eintraege[0].Status | Should -Be 'aktiv'
        $script:Inst.ContainsKey('VideoLAN.VLC') | Should -BeTrue
        Should -Invoke -ModuleName MinibenchTest Invoke-Winget -Times 0 -ParameterFilter { $Arguments -like 'uninstall *' }
    }
    It 'gibt ungültige Paketkennungen nicht an winget weiter' {
        $r = MinibenchTest\Invoke-SoftwarePakete 'x" & calc.exe=böse;;Google.Chrome=Google Chrome'
        $r.Ergebnisse[0].Status | Should -Be 'ungültig'
        $r.Ergebnisse[1].Status | Should -Be 'installiert'
        Should -Invoke -ModuleName MinibenchTest Invoke-Winget -Times 0 -ParameterFilter { $Arguments -like '*calc*' }
        MinibenchTest\Test-WingetId 'Notepad++.Notepad++' | Should -BeTrue
        MinibenchTest\Test-WingetId '-e --force' | Should -BeFalse
    }
    It 'ohne winget: jedes Paket fehlgeschlagen mit Begründung' {
        Mock -ModuleName MinibenchTest Find-Winget { '' }
        $r = MinibenchTest\Invoke-SoftwarePakete 'Google.Chrome=Google Chrome'
        $r.Fehler | Should -Be 1
        $r.Ergebnisse[0].Text | Should -Match 'nicht verfügbar'
    }
    It 'Rückgängig überspringt ein inzwischen von Hand entferntes Programm' {
        [void](MinibenchTest\Invoke-SoftwarePakete 'VideoLAN.VLC=VLC')
        $script:Inst.Remove('VideoLAN.VLC')
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 0
        (Read-Run).Eintraege[0].Status | Should -Be 'übersprungen'
        Should -Invoke -ModuleName MinibenchTest Invoke-Winget -Times 0 -ParameterFilter { $Arguments -like 'uninstall *' }
    }
}

Describe 'Aufruf von winget' {
    BeforeAll {
        Import-MinibenchTestModule -Parts 'Kern\Risiko.ps1', 'Kern\Geraeteidentitaet.ps1', 'Kern\Aenderungen.ps1', 'Kern\Softwarepakete.ps1' -Functions 'Get-SafeName', 'Invoke-External', 'Show-Sub', 'Hide-Sub', 'Write-Heartbeat', 'Send-GuiEvent'
    }
    It 'winget läuft mit Frist, gelesener Ausgabe und ohne Reaktion auf skip.flag eines Laufs' {
        Mock -ModuleName MinibenchTest Invoke-External { [pscustomobject]@{ ExitCode = 0; Output = ''; Error = ''; TimedOut = $false } }
        [void](& (Get-Module MinibenchTest) { Invoke-Winget 'winget.exe' 'list --id 7zip.7zip -e' 120 })
        Should -Invoke -ModuleName MinibenchTest Invoke-External -Times 1 -ParameterFilter { $TimeoutSec -eq 120 -and $OhneUeberspringen -and $File -eq 'winget.exe' }
        (Get-ModuleVar 'WingetFristSek') | Should -BeGreaterThan 0
        (Get-ModuleVar 'WingetFristSek') | Should -BeLessOrEqual 3600
    }
}

# Seite Tools (ab v4.05): Paketliste als Tabelle (Kern\Softwarepakete.psd1), installierte Pakete über winget export
Describe 'Paketliste der Seite Tools' {
    BeforeAll {
        Import-MinibenchTestModule -Parts 'Kern\Risiko.ps1', 'Kern\Geraeteidentitaet.ps1', 'Kern\Aenderungen.ps1', 'Kern\Softwarepakete.ps1' -Functions 'Get-SafeName', 'Invoke-External', 'Show-Sub', 'Hide-Sub', 'Write-Heartbeat', 'Send-GuiEvent'
    }
    It 'ist vollständig: Gruppe, Id, Name und Beschreibung je Paket, keine doppelten Ids' {
        $p = @(MinibenchTest\Get-SoftwarePakete)
        $g = @(MinibenchTest\Get-SoftwareGruppen)
        $p.Count | Should -BeGreaterOrEqual 20
        $p.Count | Should -BeLessOrEqual 30
        $g.Count | Should -Be 6
        foreach ($x in $p) {
            foreach ($f in 'Gruppe', 'Id', 'Name', 'Beschreibung') { [string]$x[$f] | Should -Not -BeNullOrEmpty -Because ('{0}: {1}' -f $x.Id, $f) }
            $g | Should -Contain $x.Gruppe
            MinibenchTest\Test-WingetId $x.Id | Should -BeTrue -Because $x.Id
            ([string]$x.Name + [string]$x.Beschreibung) | Should -Not -Match '\|'
        }
        @($p | Group-Object { $_.Id.ToLowerInvariant() } | Where-Object { $_.Count -gt 1 }).Count | Should -Be 0
        foreach ($k in $g) { @($p | Where-Object { $_.Gruppe -eq $k }).Count | Should -BeGreaterThan 0 -Because $k }
    }
    It 'ist eingeschränkte Sprache (nur Daten)' {
        $f = Join-Path $global:MinibenchSrcRoot 'Kern/Softwarepakete.psd1'
        $d = Import-PowerShellDataFile -LiteralPath $f
        @($d.Pakete).Count | Should -Be @(MinibenchTest\Get-SoftwarePakete).Count
    }
    It 'Zeilen für die Oberfläche: je Gruppe G, je Paket P mit fünf Feldern' {
        $z = @(MinibenchTest\Get-SoftwarePaketGuiLines)
        @($z | Where-Object { $_ -like 'G|*' }).Count | Should -Be @(MinibenchTest\Get-SoftwareGruppen).Count
        $pz = @($z | Where-Object { $_ -like 'P|*' })
        $pz.Count | Should -Be @(MinibenchTest\Get-SoftwarePakete).Count
        foreach ($l in $pz) { @($l -split '\|').Count | Should -Be 5 }
    }
    It 'winget export: Ids aus allen Quellen, ohne Unterscheidung von Groß- und Kleinschreibung' {
        $j = '{"Sources":[{"Packages":[{"PackageIdentifier":"7zip.7zip"},{"PackageIdentifier":"Google.Chrome"}],"SourceDetails":{"Name":"winget"}},{"Packages":[{"PackageIdentifier":"9NKSQGP7F2NH"}]}]}'
        $ids = @(MinibenchTest\ConvertFrom-WingetExport $j)
        $ids.Count | Should -Be 3
        ($ids -contains 'google.chrome') | Should -BeTrue
        @(MinibenchTest\ConvertFrom-WingetExport '{"Sources":[]}').Count | Should -Be 0
    }
    It 'Zustand: je Paket ein Ereignis, Anzahl installierter als Ergebnis' {
        $script:ev = [System.Collections.Generic.List[string]]::new()
        Mock -ModuleName MinibenchTest Send-GuiEvent { $script:ev.Add((@($Kind) + @($Parts)) -join "|") }
        Mock -ModuleName MinibenchTest Find-Winget { 'winget.exe' }
        Mock -ModuleName MinibenchTest Get-WingetInstallierteIds { [pscustomobject]@{ Ids = @('7zip.7zip', 'VIDEOLAN.VLC') } }
        $n = MinibenchTest\Invoke-SoftwareZustand
        $n | Should -Be 2
        $script:ev.Count | Should -Be @(MinibenchTest\Get-SoftwarePakete).Count
        $script:ev | Should -Contain 'PAKZ|7zip.7zip|installiert'
        $script:ev | Should -Contain 'PAKZ|VideoLAN.VLC|installiert'
        $script:ev | Should -Contain 'PAKZ|Google.Chrome|nicht installiert'
    }
    It 'Zustand ohne winget oder ohne Export: -1 und keine Ereignisse' {
        $script:ev = [System.Collections.Generic.List[string]]::new()
        Mock -ModuleName MinibenchTest Send-GuiEvent { $script:ev.Add((@($Kind) + @($Parts)) -join "|") }
        Mock -ModuleName MinibenchTest Find-Winget { '' }
        MinibenchTest\Invoke-SoftwareZustand | Should -Be -1
        Mock -ModuleName MinibenchTest Find-Winget { 'winget.exe' }
        Mock -ModuleName MinibenchTest Get-WingetInstallierteIds { $null }
        MinibenchTest\Invoke-SoftwareZustand | Should -Be -1
        $script:ev.Count | Should -Be 0
    }
    It 'Export läuft mit Frist und räumt die Datei weg' {
        Mock -ModuleName MinibenchTest Invoke-External { [pscustomobject]@{ ExitCode = 0; Output = ''; Error = ''; TimedOut = $true } }
        $r = & (Get-Module MinibenchTest) { Get-WingetInstallierteIds 'winget.exe' }
        $r | Should -BeNullOrEmpty
        Should -Invoke -ModuleName MinibenchTest Invoke-External -Times 1 -ParameterFilter { $Arguments -match '^export -o ' -and $TimeoutSec -eq 240 }
        @(Get-ChildItem ([IO.Path]::GetTempPath()) -Filter ('LeosMinibench-winget-{0}.json' -f $PID)).Count | Should -Be 0
    }
}

Describe 'Protokoll aufräumen (Move-ChangeArchiv)' {
    BeforeAll {
        # Protokolldatei mit Einträgen im gegebenen Status; mit Sicherungsordner, wenn -Sicherung
        # ab v4.11 mit GeraetId je PC (Standard "G-<PC>"), -GeraetId '-' lässt sie weg (Protokolle vor Format 2 der Geräteidentität)
        function New-Protokoll([string]$DataDir, [string]$Pc, [string]$Datei, [string]$Lauf, [string[]]$Status, [switch]$Sicherung, [string]$GeraetId = '') {
            $dir = Join-Path (Join-Path $DataDir 'Änderungen') $Pc
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            $i = 0
            $ein = @($Status | ForEach-Object { $i++; [ordered]@{ Id = $i; Zeit = $Lauf; Modul = 'Optimierung'; Schritt = 'S'; Titel = 'T'; Risiko = 'Aendern'; Art = 'Registry'; Ziel = 'Z'; Vorher = '1'; Nachher = '0'; Status = $_; Ergebnis = '' } })
            $gid = $(if ($GeraetId -eq '-') { '' } elseif ($GeraetId) { $GeraetId } else { 'G-' + $Pc.ToUpperInvariant() })
            $log = [ordered]@{ Format = 'Minibench-Aenderungen/1'; Computer = $Pc; GeraetId = $gid; Version = '4.10'; Lauf = $Lauf; Eintraege = $ein }
            $p = Join-Path $dir $Datei
            [IO.File]::WriteAllText($p, ($log | ConvertTo-Json -Depth 6), (New-Object Text.UTF8Encoding($false)))
            if ($Sicherung) { $s = $p -replace '\.json$', ''; New-Item -ItemType Directory -Path $s -Force | Out-Null; Set-Content -LiteralPath (Join-Path $s 'plan.pow') -Value 'x' }
            return $p
        }
        function New-Ordner {
            $d = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $d | Out-Null
            Set-ModuleVar 'DataDir' $d
            Set-ModuleVar 'ChangeFile' ''
            return $d
        }
        $script:jetzt = [datetime]'2026-11-15 12:00:00'
    }
    It 'verschiebt erledigte Läufe ab dem gewählten Alter samt Sicherungsordner und löscht nichts' {
        $d = New-Ordner
        $alt = New-Protokoll $d 'HIER' '20260901_100000.json' '2026-09-01 10:00:00' @('rückgängig', 'nur Hinweis') -Sicherung
        $neu = New-Protokoll $d 'HIER' '20261110_100000.json' '2026-11-10 10:00:00' @('rückgängig')
        $r = MinibenchTest\Move-ChangeArchiv 'Erledigt:30' -Root (Join-Path $d 'Änderungen') -DataDir $d -Computer 'HIER' -GeraetId 'G-HIER' -Jetzt $script:jetzt
        $r.Verschoben | Should -Be 1
        $r.Eintraege | Should -Be 2
        $r.Fehler | Should -Be 0
        Test-Path -LiteralPath $alt | Should -BeFalse
        Test-Path -LiteralPath $neu | Should -BeTrue
        $ziel = Join-Path $r.Archiv 'HIER'
        Test-Path -LiteralPath (Join-Path $ziel '20260901_100000.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path (Join-Path $ziel '20260901_100000') 'plan.pow') | Should -BeTrue
        $r.Archiv | Should -Match ([regex]::Escape([IO.Path]::Combine('Archiv', 'Änderungen')))
        (Get-Content -LiteralPath (Join-Path (Join-Path $d 'Archiv') 'Datenpflege.log') -Raw -Encoding UTF8) | Should -Match 'Änderungsprotokoll aufgeräumt'
    }
    It 'lässt aktive Änderungen dieses PCs immer liegen, auch bei jedem Alter und mit Andere' {
        $d = New-Ordner
        $aktiv = New-Protokoll $d 'HIER' '20260101_100000.json' '2026-01-01 10:00:00' @('aktiv', 'rückgängig')
        $r = MinibenchTest\Move-ChangeArchiv 'Erledigt:0;Andere' -Root (Join-Path $d 'Änderungen') -DataDir $d -Computer 'HIER' -GeraetId 'G-HIER' -Jetzt $script:jetzt
        $r.Verschoben | Should -Be 0
        Test-Path -LiteralPath $aktiv | Should -BeTrue
        ($r.Zeilen -join ' ') | Should -Match 'Nichts zu verschieben'
    }
    It 'verschiebt Protokolle anderer PCs nur mit Andere, dann auch mit aktiven Einträgen, und räumt den leeren Ordner weg' {
        $d = New-Ordner
        $fremd = New-Protokoll $d 'FREMD' '20261101_100000.json' '2026-11-01 10:00:00' @('aktiv', 'aktiv', 'aktiv')
        $hier = New-Protokoll $d 'HIER' '20261101_100000.json' '2026-11-01 10:00:00' @('aktiv')
        $r1 = MinibenchTest\Move-ChangeArchiv 'Erledigt:0' -Root (Join-Path $d 'Änderungen') -DataDir $d -Computer 'HIER' -GeraetId 'G-HIER' -Jetzt $script:jetzt
        $r1.Verschoben | Should -Be 0
        $r2 = MinibenchTest\Move-ChangeArchiv 'Andere' -Root (Join-Path $d 'Änderungen') -DataDir $d -Computer 'hier' -GeraetId 'G-HIER' -Jetzt $script:jetzt
        $r2.Verschoben | Should -Be 1
        $r2.Eintraege | Should -Be 3
        Test-Path -LiteralPath $fremd | Should -BeFalse
        Test-Path -LiteralPath (Split-Path $fremd -Parent) | Should -BeFalse
        Test-Path -LiteralPath $hier | Should -BeTrue
    }
    It 'zählt leere Protokolle als erledigt und lässt das Protokoll des laufenden Prozesses in Ruhe' {
        $d = New-Ordner
        $leer = New-Protokoll $d 'HIER' '20260801_080000.json' '2026-08-01 08:00:00' @()
        $lauf = New-Protokoll $d 'HIER' '20260802_080000.json' '2026-08-02 08:00:00' @()
        Set-ModuleVar 'ChangeFile' $lauf
        $r = MinibenchTest\Move-ChangeArchiv 'Erledigt:30' -Root (Join-Path $d 'Änderungen') -DataDir $d -Computer 'HIER' -GeraetId 'G-HIER' -Jetzt $script:jetzt
        $r.Verschoben | Should -Be 1
        Test-Path -LiteralPath $leer | Should -BeFalse
        Test-Path -LiteralPath $lauf | Should -BeTrue
    }
    It 'meldet unbekannte Angaben als Fehler und verschiebt ohne gültige Angabe nichts' {
        $d = New-Ordner
        $p = New-Protokoll $d 'FREMD' '20260101_100000.json' '2026-01-01 10:00:00' @('rückgängig')
        $r = MinibenchTest\Move-ChangeArchiv 'Alles' -Root (Join-Path $d 'Änderungen') -DataDir $d -Computer 'HIER' -GeraetId 'G-HIER' -Jetzt $script:jetzt
        $r.Fehler | Should -Be 1
        $r.Verschoben | Should -Be 0
        Test-Path -LiteralPath $p | Should -BeTrue
    }
    # ab v4.11 (Befund D5): Zuordnung über die GeraetId, der Name nur bei Protokollen ohne GeraetId
    It 'nach dem Umbenennen des PCs bleiben dessen aktive Protokolle bei Andere liegen; ohne GeraetId zählt der Name' {
        $d = New-Ordner
        $altName = New-Protokoll $d 'ALTNAME' '20261101_100000.json' '2026-11-01 10:00:00' @('aktiv') -GeraetId 'G-HIER'
        $fremd = New-Protokoll $d 'FREMD' '20261102_100000.json' '2026-11-02 10:00:00' @('aktiv') -GeraetId 'G-FREMD'
        $ohneIdHier = New-Protokoll $d 'HIER' '20261103_100000.json' '2026-11-03 10:00:00' @('aktiv') -GeraetId '-'
        $ohneIdFremd = New-Protokoll $d 'NOCHEINER' '20261104_100000.json' '2026-11-04 10:00:00' @('aktiv') -GeraetId '-'
        # gleicher Name, aber anderes Gerät (z. B. neu aufgesetzter PC mit altem Namen)
        $gleicherName = New-Protokoll $d 'HIER' '20261105_100000.json' '2026-11-05 10:00:00' @('aktiv') -GeraetId 'G-ANDERES'
        $r = MinibenchTest\Move-ChangeArchiv 'Andere' -Root (Join-Path $d 'Änderungen') -DataDir $d -Computer 'HIER' -GeraetId 'G-HIER' -Jetzt $script:jetzt
        Test-Path -LiteralPath $altName | Should -BeTrue
        Test-Path -LiteralPath $ohneIdHier | Should -BeTrue
        Test-Path -LiteralPath $fremd | Should -BeFalse
        Test-Path -LiteralPath $ohneIdFremd | Should -BeFalse
        Test-Path -LiteralPath $gleicherName | Should -BeFalse
        $r.Verschoben | Should -Be 3
    }
    # ab v4.11 (Befund D1): "fehlgeschlagen" älterer Versionen ist nicht erledigt
    It 'archiviert Protokolle mit fehlgeschlagenem Rückgängig nicht als erledigt' {
        $d = New-Ordner
        $p = New-Protokoll $d 'HIER' '20260101_100000.json' '2026-01-01 10:00:00' @('rückgängig', 'fehlgeschlagen')
        $r = MinibenchTest\Move-ChangeArchiv 'Erledigt:0' -Root (Join-Path $d 'Änderungen') -DataDir $d -Computer 'HIER' -GeraetId 'G-HIER' -Jetzt $script:jetzt
        $r.Verschoben | Should -Be 0
        Test-Path -LiteralPath $p | Should -BeTrue
    }
    It 'ist als Hilfsmodus ohne Administratorrechte eingetragen und endet in seinem Kernteil' {
        (Get-PartText 'Kern\Aenderungen.ps1') | Should -Match '(?s)if \(\$AenderungenAufraeumen\) \{.*?Send-GuiEvent ''RESULT''.*?exit '
        (Get-PartText 'Kern\Adminrechte.ps1') | Should -Match '-not \$AenderungenAufraeumen'
        (Get-GuiText) | Should -Match '-AenderungenAufraeumen '
    }
}

# ab v4.11 (Befund D1): Ein gescheitertes Rückgängig lässt den Eintrag aktiv, ein neuer Versuch ist möglich
Describe 'Rückgängig nach einem Fehlschlag erneut versuchen' {
    BeforeEach {
        $null = New-Run
        $script:reg = @{ Vorhanden = $true; Wert = 1; Typ = 'DWord' }
        $script:schreibFehler = $false
        Mock -ModuleName MinibenchTest Get-RegValueState { [pscustomobject]$script:reg }
        Mock -ModuleName MinibenchTest Set-RegValueState { if ($script:schreibFehler) { throw 'Zugriff verweigert' }; $script:reg = @{ Vorhanden = $true; Wert = $Value; Typ = $Type } }
        Mock -ModuleName MinibenchTest Remove-RegValue { $script:reg = @{ Vorhanden = $false; Wert = $null; Typ = '' } }
        [void](MinibenchTest\Set-RegistryValueLogged -Path 'HKLM:\SYSTEM\Test' -Name 'X' -Value 0 -Modul 'R' -Schritt 'S' -Titel 'T')
    }
    It 'lässt den Eintrag aktiv, vermerkt den Fehler und schafft es beim zweiten Versuch' {
        $script:schreibFehler = $true
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 0
        $u.Probleme | Should -Be 1
        $e = (Read-Run).Eintraege[0]
        $e.Status | Should -Be 'aktiv'
        $e.Ergebnis | Should -Match 'fehlgeschlagen.*Zugriff verweigert'
        $e.Fehlversuche | Should -Be 1
        $script:schreibFehler = $false
        $u2 = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u2.Rueckgaengig | Should -Be 1
        (Read-Run).Eintraege[0].Status | Should -Be 'rückgängig'
        $script:reg.Wert | Should -Be 1
    }
    It 'nimmt Einträge mit Status "fehlgeschlagen" aus älteren Versionen erneut zurück' {
        $j = Read-Run
        $j.Eintraege[0].Status = 'fehlgeschlagen'
        [IO.File]::WriteAllText((Get-RunFile), ($j | ConvertTo-Json -Depth 8), (New-Object Text.UTF8Encoding($false)))
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 1
        (Read-Run).Eintraege[0].Status | Should -Be 'rückgängig'
    }
    It 'Undo-ChangeNow lässt den Eintrag bei einem Fehler ebenfalls aktiv' {
        $r = @((Get-ModuleVar 'ChangeLog').Eintraege)[0]
        $script:schreibFehler = $true
        (MinibenchTest\Undo-ChangeNow $r).Status | Should -Be 'fehlgeschlagen'
        (Read-Run).Eintraege[0].Status | Should -Be 'aktiv'
        (Read-Run).Eintraege[0].Ergebnis | Should -Match 'direkt nach der Messung'
    }
}

# ab v4.11 (Befund D2): verzögerter Autostart (DelayedAutostart) wird gesichert und wiederhergestellt
Describe 'Dienst mit verzögertem Start' {
    BeforeEach {
        $null = New-Run
        $script:svc = 'Automatic'
        $script:verz = $true
        $script:aufrufe = @()
        Mock -ModuleName MinibenchTest Get-ServiceStartType { $script:svc }
        Mock -ModuleName MinibenchTest Set-ServiceStartType { $script:svc = $StartType }
        Mock -ModuleName MinibenchTest Get-ServiceDelayedAutostart { $script:verz }
        Mock -ModuleName MinibenchTest Invoke-External { $script:aufrufe += ('{0} {1}' -f $File, $Arguments); [pscustomobject]@{ ExitCode = 0; Output = ''; Error = ''; TimedOut = $false } }
    }
    It 'protokolliert "verzögert" und setzt beim Rückgängig auch den verzögerten Start per sc.exe' {
        $r = MinibenchTest\Set-ServiceStartTypeLogged -Name 'MapsBroker' -StartType 'Disabled' -Modul 'Optimierung' -Schritt 'S' -Titel 'T'
        $r.Vorher | Should -Be 'Automatic (verzögert)'
        $r.Daten.VorherVerzoegert | Should -BeTrue
        $r.Gegenbefehl | Should -Match 'start= delayed-auto'
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 1
        $script:svc | Should -Be 'Automatic'
        @($script:aufrufe).Count | Should -Be 1
        $script:aufrufe[0] | Should -Match 'sc\.exe config "MapsBroker" start= delayed-auto$'
        (Read-Run).Eintraege[0].Ergebnis | Should -Match 'verzögert'
    }
    It 'ohne verzögerten Start bleibt es beim einfachen Starttyp, ohne sc.exe' {
        $script:verz = $false
        [void](MinibenchTest\Set-ServiceStartTypeLogged -Name 'w32time' -StartType 'Disabled' -Modul 'R' -Schritt 'S' -Titel 'T')
        (Read-Run).Eintraege[0].Vorher | Should -Be 'Automatic'
        [void](MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile)))
        $script:svc | Should -Be 'Automatic'
        @($script:aufrufe).Count | Should -Be 0
    }
    It 'sc.exe mit Fehlercode lässt den Eintrag aktiv, der neue Versuch setzt nur noch den verzögerten Start' {
        [void](MinibenchTest\Set-ServiceStartTypeLogged -Name 'MapsBroker' -StartType 'Disabled' -Modul 'R' -Schritt 'S' -Titel 'T')
        Mock -ModuleName MinibenchTest Invoke-External { [pscustomobject]@{ ExitCode = 5; Output = ''; Error = ''; TimedOut = $false } }
        (MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))).Rueckgaengig | Should -Be 0
        (Read-Run).Eintraege[0].Status | Should -Be 'aktiv'
        $script:svc | Should -Be 'Automatic'
        $script:verz = $false
        Mock -ModuleName MinibenchTest Invoke-External { $script:aufrufe += ('{0} {1}' -f $File, $Arguments); [pscustomobject]@{ ExitCode = 0; Output = ''; Error = ''; TimedOut = $false } }
        (MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))).Rueckgaengig | Should -Be 1
        (Read-Run).Eintraege[0].Status | Should -Be 'rückgängig'
        @($script:aufrufe).Count | Should -Be 1
        # einmal beim Ändern, einmal beim ersten Rückgängig, beim zweiten nicht mehr
        Should -Invoke -ModuleName MinibenchTest Set-ServiceStartType -Times 2 -Exactly
    }
}

# ab v4.11 (Befund D4): selbst angelegte Registry-Schlüssel werden beim Rückgängig entfernt, wenn sie leer sind
Describe 'Neu angelegte Registry-Schlüssel' {
    BeforeEach {
        $null = New-Run
        $script:reg = @{ Vorhanden = $false; Wert = $null; Typ = '' }
        $script:entfernt = @()
        $script:leer = @{}
        Mock -ModuleName MinibenchTest Get-RegValueState { [pscustomobject]$script:reg }
        Mock -ModuleName MinibenchTest Set-RegValueState {
            Set-ModuleVar 'RegNeueSchluessel' @('HKCU:\Software\Neu', 'HKCU:\Software\Neu\Unter')
            $script:reg = @{ Vorhanden = $true; Wert = $Value; Typ = $Type }
        }
        Mock -ModuleName MinibenchTest Remove-RegValue { $script:reg = @{ Vorhanden = $false; Wert = $null; Typ = '' } }
        Mock -ModuleName MinibenchTest Remove-RegKeyIfEmpty { if ($script:leer[$Path] -eq $false) { return $false }; $script:entfernt += $Path; return $true }
    }
    It 'vermerkt die Schlüssel und entfernt sie beim Rückgängig von unten nach oben' {
        $r = MinibenchTest\Set-RegistryValueLogged -Path 'HKCU:\Software\Neu\Unter' -Name 'Wert' -Value 1 -Modul 'Optimierung' -Schritt 'S' -Titel 'T'
        @($r.Daten.NeueSchluessel) | Should -Be @('HKCU:\Software\Neu', 'HKCU:\Software\Neu\Unter')
        @((Read-Run).Eintraege[0].Daten.NeueSchluessel).Count | Should -Be 2
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 1
        $script:entfernt | Should -Be @('HKCU:\Software\Neu\Unter', 'HKCU:\Software\Neu')
        (Read-Run).Eintraege[0].Ergebnis | Should -Match '2 angelegte'
    }
    It 'hört beim ersten nicht leeren Schlüssel auf' {
        [void](MinibenchTest\Set-RegistryValueLogged -Path 'HKCU:\Software\Neu\Unter' -Name 'Wert' -Value 1 -Modul 'R' -Schritt 'S' -Titel 'T')
        $script:leer['HKCU:\Software\Neu\Unter'] = $false
        [void](MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile)))
        @($script:entfernt).Count | Should -Be 0
    }
    It 'vorhandene Schlüssel werden nicht vermerkt' {
        Mock -ModuleName MinibenchTest Set-RegValueState { $script:reg = @{ Vorhanden = $true; Wert = $Value; Typ = $Type } }
        $r = MinibenchTest\Set-RegistryValueLogged -Path 'HKCU:\Software\Da' -Name 'Wert' -Value 1 -Modul 'R' -Schritt 'S' -Titel 'T'
        $r.Daten.Contains('NeueSchluessel') | Should -BeFalse
        [void](MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile)))
        Should -Invoke -ModuleName MinibenchTest Remove-RegKeyIfEmpty -Times 0
    }
}

Describe 'Neu angelegte Registry-Schlüssel an der echten Registry' {
    It 'Set-RegValueState und Remove-RegKeyIfEmpty an der echten Registry (HKCU, Testschlüssel je Prozess)' {
        if (-not (Test-IstWindows)) { Set-ItResult -Skipped -Because 'Registry gibt es nur unter Windows'; return }
        $basis = 'HKCU:\Software\LeosMinibench-Test-{0}' -f $PID
        $pfad = $basis + '\A\B'
        try {
            & (Get-Module MinibenchTest) { param($p) Set-RegValueState $p 'Wert' 7 'DWord' } $pfad
            @(Get-ModuleVar 'RegNeueSchluessel') | Should -Be @($basis, ($basis + '\A'), $pfad)
            (Get-ItemProperty -LiteralPath $pfad -Name Wert).Wert | Should -Be 7
            MinibenchTest\Remove-RegKeyIfEmpty $pfad | Should -BeFalse -Because 'der Schlüssel hat noch einen Wert'
            Remove-ItemProperty -LiteralPath $pfad -Name Wert
            MinibenchTest\Remove-NeueRegSchluessel @(Get-ModuleVar 'RegNeueSchluessel') | Should -Be 3
            Test-Path -LiteralPath $basis | Should -BeFalse
            # vorhandener Schlüssel: nichts vermerkt
            New-Item -Path $basis -Force | Out-Null
            & (Get-Module MinibenchTest) { param($p) Set-RegValueState $p 'Wert' 'x' 'String' } $basis
            @(Get-ModuleVar 'RegNeueSchluessel').Count | Should -Be 0
        } finally {
            Remove-Item -LiteralPath $basis -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

# ab v4.11 (Befund D5): Restore-TemporaryChanges findet Protokolle über die GeraetId, auch unter dem alten Computernamen
Describe 'Vorübergehende Änderung nach dem Umbenennen des PCs' {
    BeforeAll {
        function New-AltesProtokoll([string]$Pc, [string]$Gid, [string]$Status = 'aktiv') {
            $d = Join-Path (Join-Path $script:dir 'Änderungen') $Pc
            New-Item -ItemType Directory -Path $d -Force | Out-Null
            $e = [ordered]@{ Id = 1; Zeit = '2026-11-01 10:00:00'; Modul = 'Benchmark'; Schritt = 'GpuWahl'; Titel = 'WinSAT auf die Grafikkarte'; Risiko = 'Aendern'; Art = 'Registry'; Ziel = 'x'
                Vorher = '(nicht vorhanden)'; Nachher = 'GpuPreference=2;'; Status = $Status; Ergebnis = ''
                Daten = [ordered]@{ Pfad = 'HKCU:\Software\Microsoft\DirectX\UserGpuPreferences'; Name = 'C:\Windows\System32\winsat.exe'; VorherVorhanden = $false; NachherWert = 'GpuPreference=2;'; NachherTyp = 'String' } }
            $log = [ordered]@{ Format = 'Minibench-Aenderungen/1'; Computer = $Pc; GeraetId = $Gid; Version = '4.10'; Lauf = '2026-11-01 10:00:00'; Eintraege = @($e) }
            $p = Join-Path $d '20261101_100000.json'
            [IO.File]::WriteAllText($p, ($log | ConvertTo-Json -Depth 8), (New-Object Text.UTF8Encoding($false)))
            return $p
        }
    }
    BeforeEach {
        $script:dir = New-Run
        $script:reg = @{ Vorhanden = $true; Wert = 'GpuPreference=2;'; Typ = 'String' }
        Mock -ModuleName MinibenchTest Get-RegValueState { [pscustomobject]$script:reg }
        Mock -ModuleName MinibenchTest Set-RegValueState { $script:reg = @{ Vorhanden = $true; Wert = $Value; Typ = $Type } }
        Mock -ModuleName MinibenchTest Remove-RegValue { $script:reg = @{ Vorhanden = $false; Wert = $null; Typ = '' } }
    }
    It 'nimmt die GPU-Wahl aus dem Protokoll unter dem alten Namen zurück' {
        $p = New-AltesProtokoll 'ALTERNAME' 'G-aaaaaaaaaaaaaaaaaaaa'
        $u = MinibenchTest\Restore-TemporaryChanges
        $u.Rueckgaengig | Should -Be 1
        $script:reg.Vorhanden | Should -BeFalse
        (Get-Content -LiteralPath $p -Raw -Encoding UTF8 | ConvertFrom-Json).Eintraege[0].Status | Should -Be 'rückgängig'
    }
    It 'lässt Protokolle eines anderen Geräts mit gleichem Namen in Ruhe' {
        $null = New-AltesProtokoll (MinibenchTest\Get-SafeName $env:COMPUTERNAME) 'G-bbbbbbbbbbbbbbbbbbbb'
        MinibenchTest\Restore-TemporaryChanges | Should -BeNullOrEmpty
        $script:reg.Vorhanden | Should -BeTrue
    }
    It 'versucht eine früher fehlgeschlagene Rücknahme erneut' {
        $null = New-AltesProtokoll 'ALTERNAME' 'G-aaaaaaaaaaaaaaaaaaaa' 'fehlgeschlagen'
        (MinibenchTest\Restore-TemporaryChanges).Rueckgaengig | Should -Be 1
    }
}

# ab v4.11 (Befund A4): Computerschutz nur einschalten, wenn er aus war, und dann mit Gegenbefehl protokollieren
Describe 'Computerschutz mit Protokoll' {
    BeforeEach {
        $null = New-Run
        $script:schutz = [pscustomobject]@{ Richtlinie = $false; SppLesbar = $true; Spp = @(); Intervall = 0 }
        $script:schalt = @()
        $script:schaltFehler = $false
        Mock -ModuleName MinibenchTest Get-ComputerschutzQuellen { $script:schutz }
        Mock -ModuleName MinibenchTest Set-ComputerschutzZustand {
            if ($script:schaltFehler) { throw 'Richtlinie verhindert das' }
            $script:schalt += ('{0}={1}' -f $Laufwerk, $An)
            $script:schutz = [pscustomobject]@{ Richtlinie = $false; SppLesbar = $script:schutz.SppLesbar; Intervall = [int]$An
                Spp = $(if ($An) { @('\\?\Volume{1234}\:(C%3A)') } else { @() }) }
        }
    }
    It 'liest den Zustand je Laufwerk aus SPP\Clients, sonst aus RPSessionInterval' {
        $an = [pscustomobject]@{ Richtlinie = $false; SppLesbar = $true; Spp = @('\\?\Volume{1}\:(D%3A)', '\\?\Volume{2}\:(C%3A)'); Intervall = 1 }
        MinibenchTest\Get-ComputerschutzStatus 'C:\' $an | Should -Be 'an'
        MinibenchTest\Get-ComputerschutzStatus 'E:\' $an | Should -Be 'aus'
        MinibenchTest\Get-ComputerschutzStatus 'C:\' ([pscustomobject]@{ Richtlinie = $true; SppLesbar = $true; Spp = $an.Spp; Intervall = 1 }) | Should -Be 'aus'
        MinibenchTest\Get-ComputerschutzStatus 'C:\' ([pscustomobject]@{ Richtlinie = $false; SppLesbar = $false; Spp = @(); Intervall = 0 }) | Should -Be 'aus'
        MinibenchTest\Get-ComputerschutzStatus 'C:\' ([pscustomobject]@{ Richtlinie = $false; SppLesbar = $false; Spp = @(); Intervall = 1 }) | Should -Be 'unbekannt'
        MinibenchTest\Get-ComputerschutzStatus 'C:\' ([pscustomobject]@{ Richtlinie = $false; SppLesbar = $false; Spp = @(); Intervall = $null }) | Should -Be 'unbekannt'
    }
    It 'war er aus: einschalten, als Ändern mit Gegenbefehl protokollieren, Rückgängig schaltet wieder aus' {
        MinibenchTest\Enable-ComputerschutzMitProtokoll -Laufwerk 'C:\' | Should -BeTrue
        $script:schalt | Should -Be @('C:\=True')
        $e = (Read-Run).Eintraege[0]
        $e.Art | Should -Be 'Computerschutz'
        $e.Risiko | Should -Be 'Aendern'
        $e.Status | Should -Be 'aktiv'
        $e.Vorher | Should -Be 'aus'
        $e.Gegenbefehl | Should -Match '^Disable-ComputerRestore -Drive "C:\\"'
        $u = MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile))
        $u.Rueckgaengig | Should -Be 1
        $script:schalt | Should -Be @('C:\=True', 'C:\=False')
        (Read-Run).Eintraege[0].Status | Should -Be 'rückgängig'
    }
    It 'war er schon an: nichts schalten, nichts protokollieren' {
        $script:schutz = [pscustomobject]@{ Richtlinie = $false; SppLesbar = $true; Spp = @('\\?\Volume{1}\:(C%3A)'); Intervall = 1 }
        MinibenchTest\Enable-ComputerschutzMitProtokoll -Laufwerk 'C:\' | Should -BeTrue
        @($script:schalt).Count | Should -Be 0
        Get-RunFile | Should -Not -Exist
    }
    It 'Vorher-Zustand nicht lesbar: einschalten, aber nur als Hinweis (kein automatisches Ausschalten)' {
        $script:schutz = [pscustomobject]@{ Richtlinie = $false; SppLesbar = $false; Spp = @(); Intervall = $null }
        MinibenchTest\Enable-ComputerschutzMitProtokoll -Laufwerk 'C:' | Should -BeTrue
        $script:schalt | Should -Be @('C:\=True')
        $e = (Read-Run).Eintraege[0]
        $e.Status | Should -Be 'nur Hinweis'
        $e.Vorher | Should -Be 'nicht lesbar'
    }
    It 'scheitert das Einschalten: $false und kein Eintrag' {
        $script:schaltFehler = $true
        MinibenchTest\Enable-ComputerschutzMitProtokoll -Laufwerk 'C:\' -WarningAction SilentlyContinue | Should -BeFalse
        Get-RunFile | Should -Not -Exist
    }
    It 'Rückgängig überspringt, wenn der Schutz inzwischen schon aus ist' {
        [void](MinibenchTest\Enable-ComputerschutzMitProtokoll -Laufwerk 'C:\')
        $script:schutz = [pscustomobject]@{ Richtlinie = $false; SppLesbar = $true; Spp = @(); Intervall = 0 }
        [void](MinibenchTest\Invoke-ChangeUndo ('{0}*1' -f (Get-RunFile)))
        (Read-Run).Eintraege[0].Status | Should -Be 'übersprungen'
        @($script:schalt).Count | Should -Be 1
    }
    It 'Signatur für die Wartung: -Laufwerk mit dem Systemlaufwerk als Standard; der Wiederherstellungspunkt nutzt die Funktion' {
        (Get-Command MinibenchTest\Enable-ComputerschutzMitProtokoll).Parameters.Keys | Should -Contain 'Laufwerk'
        (Get-PartText 'Kern\Aenderungen.ps1') | Should -Match 'param\(\[string\]\$Laufwerk = "\$env:SystemDrive\\"'
        (Get-PartText 'Kern\Aenderungen.ps1') | Should -Not -Match '(?m)^\s*try \{ Enable-ComputerRestore'
    }
}
