# Gesamtläufe des Arbeitsprozesses (Praxistest v2.4: "nur Lasttest ergibt einen leeren Bericht").
# Jeder Lauf startet das zusammengebaute Skript als eigenen Prozess mit -EventMode, wie die Oberfläche es tut.
# Windows-Befehle sind Attrappen (Lauf.Hilfen.ps1), Wartezeiten laufen im Zeitraffer.
# Unter Windows laufen die Gesamtläufe nur mit Testen.cmd -Gesamtlauf (Umgebungsvariable MINIBENCH_GESAMTLAUF=1):
# dort gibt es echte Windows-Werkzeuge, die die Attrappen nicht alle abfangen, und ein Lauf dauert einige Minuten.
BeforeDiscovery {
    $global:LaufSkip = (($env:OS -eq 'Windows_NT') -and $env:MINIBENCH_GESAMTLAUF -ne '1')
    $global:LaufFaelle = @(
        @{ Name = 'nur Lasttest CPU'; Argumente = @('-Module', 'Lasttest', '-LastCpuMinuten', '1'); Abschnitt = 'LASTTEST'; Muster = 'Unterbrechungen der CPU-Last \(eigener Prozess\): (keine|\d+) über 50 ms' }
        @{ Name = 'nur Lasttest RAM'; Argumente = @('-Module', 'Lasttest', '-LastRamMinuten', '1'); Abschnitt = 'LASTTEST' }
        # ab v2.7 mit Sensoren während des Benchmarks (je Abschnitt und gesamt)
        @{ Name = 'nur Benchmark CPU und RAM'; Argumente = @('-Module', 'Benchmark', '-BenchTests', 'CPU,RAM'); Abschnitt = 'BENCHMARK'; Muster = '(?s)--- Sensoren während des Benchmarks -+\r?\n.*Prozessor .*Arbeitsspeicher .*Gesamt über alle Messungen:\r?\n    CPU-Takt: min' }
        @{ Name = 'Diagnose Funktionstest'; Argumente = @('-Module', 'Diagnose', '-DiagProfil', 'Funktionstest'); Abschnitt = 'SYSTEM UND BETRIEBSSYSTEM' }
        @{ Name = 'Benchmark und Lasttest'; Argumente = @('-Module', 'Benchmark,Lasttest', '-BenchTests', 'CPU', '-LastCpuMinuten', '1'); Abschnitt = 'LASTTEST'; Muster = '(?s)Unterbrechungen während der Messungen: .*Unterbrechungen der CPU-Last \(eigener Prozess\): ' }
        # ab v2.6 (Praxistest 02.10.2026: Datenträger- und Grafiklast lieferten kein Ergebnis)
        @{ Name = 'nur Lasttest Datenträger'; Argumente = @('-Module', 'Lasttest', '-LastDiskMinuten', '1', '-LastDiskLaufwerk', 'C'); Abschnitt = 'LASTTEST'; Umgebung = @{ MINIBENCH_TEST_VOLUME = '1' }; Muster = 'Datenträger\s+: \d\d:\d\d:\d\d, Laufwerk C:' }
        @{ Name = 'nur Lasttest Grafik'; Argumente = @('-Module', 'Lasttest', '-LastGpuMinuten', '1', '-GpuAnzeige', 'Aus'); Abschnitt = 'LASTTEST'; Muster = 'Grafik\s+: \d\d:\d\d:\d\d, NVIDIA GeForce RTX 3060 \(Grafikkarte\), 1280x720, Ø [\d.]+ Bilder/s' }
        @{ Name = 'nur Benchmark Grafik'; Argumente = @('-Module', 'Benchmark', '-BenchTests', 'GPU', '-GpuAnzeige', 'Aus'); Abschnitt = 'BENCHMARK: GRAFIK'; Muster = 'Intel\(R\) UHD Graphics 730 \(Prozessorgrafik\): Ø [\d,.]+ Bilder/s' }
        # ab v2.65: Auswahl der Grafikeinheit, Datenträger zusammen mit Grafik (Praxistest 02.10.: startete nicht)
        @{ Name = 'Lasttest nur Prozessorgrafik'; Argumente = @('-Module', 'Lasttest', '-LastGpuMinuten', '1', '-GpuAnzeige', 'Aus', '-GpuAuswahl', 'Prozessorgrafik'); Abschnitt = 'LASTTEST'; Muster = 'Grafik\s+: \d\d:\d\d:\d\d, Intel\(R\) UHD Graphics 730 \(Prozessorgrafik\)' }
        @{ Name = 'Lasttest Datenträger und Grafik'; Argumente = @('-Module', 'Lasttest', '-LastDiskMinuten', '1', '-LastDiskLaufwerk', 'C', '-LastGpuMinuten', '1', '-GpuAnzeige', 'Aus'); Abschnitt = 'LASTTEST'; Umgebung = @{ MINIBENCH_TEST_VOLUME = '1' }; Muster = '(?s)Grafik\s+: \d\d:\d\d:\d\d, NVIDIA.*Datenträger\s+: \d\d:\d\d:\d\d, Laufwerk C:' }
        @{ Name = 'Diagnose im schnellen Modus'; Argumente = @('-Module', 'Diagnose', '-DiagProfil', 'Benutzerdefiniert', '-DiagOptionen', 'Updatesuche,Energieanalyse,RamTest', '-SchnellerModus'); Abschnitt = 'SYSTEM UND BETRIEBSSYSTEM'; Muster = 'Ablauf       : schneller Modus' }
        # ab v2.8: Modul Optimierung (Registry gibt es unter Linux nicht, die Einträge enden als Fehler; geprüft wird der Ablauf)
        @{ Name = 'Optimierung'; Argumente = @('-Module', 'Optimierung', '-Optimierungen', 'WerbeId,TelemetrieDienste,Feedback,UnbekannteId', '-OptOhneWiederherstellungspunkt'); Abschnitt = 'OPTIMIERUNG: ERGEBNIS'; Muster = '(?s)Unbekannt und übergangen \(anderer Katalogstand\?\): UnbekannteId.*Kennzahlen vorher und nachher.*von 3 Einträgen angewendet' }
        # ab v3.86: Neue Integrationsfaelle fuer Wartung und Diagnose vollstaendig
        @{ Name = 'Wartung'; Argumente = @('-Module', 'Wartung', '-Reparaturen', 'Temp,Prefetch,Leerlaufaufgaben', '-OhneWiederherstellungspunkt'); Abschnitt = 'REPARATUR' }
        @{ Name = 'Diagnose vollstaendig'; Argumente = @('-Module', 'Diagnose', '-DiagProfil', 'Benutzerdefiniert', '-DiagOptionen', 'Ereignisse,Integritaet,Netzwerk'); Abschnitt = 'SYSTEM UND BETRIEBSSYSTEM' }
    )
    $global:LaufFehlerFaelle = @(
        @{ Name = 'Lasttest Grafik mit Treiber-Reset'; Argumente = @('-Module', 'Lasttest', '-LastGpuMinuten', '1', '-GpuAnzeige', 'Aus'); Umgebung = @{ MINIBENCH_TEST_GPU_FEHLER = 'reset' }; Muster = 'Treiber-Reset \(TDR\) während des Lasttests auf NVIDIA GeForce RTX 3060' }
        @{ Name = 'Lasttest Grafik mit Bildfehler'; Argumente = @('-Module', 'Lasttest', '-LastGpuMinuten', '1', '-GpuAnzeige', 'Aus'); Umgebung = @{ MINIBENCH_TEST_GPU_FEHLER = 'bild' }; Muster = 'Bildfehler im Lasttests auf' }
        @{ Name = 'Lasttest Grafik mit Bildratengrenze'; Argumente = @('-Module', 'Lasttest', '-LastGpuMinuten', '1', '-GpuAnzeige', 'Aus'); Umgebung = @{ MINIBENCH_TEST_GPU_GRENZE = '1' }; Muster = 'Bildratengrenze aktiv: 238 Bilder/s' }
        @{ Name = 'Benchmark Grafik mit Bildratengrenze'; Argumente = @('-Module', 'Benchmark', '-BenchTests', 'GPU', '-GpuAnzeige', 'Aus'); Umgebung = @{ MINIBENCH_TEST_GPU_GRENZE = '1' }; Muster = 'NVIDIA GeForce RTX 3060 \(Grafikkarte\): Bildratengrenze aktiv' }
        @{ Name = 'Lasttest Grafik ohne Grafikeinheit'; Argumente = @('-Module', 'Lasttest', '-LastGpuMinuten', '1'); Umgebung = @{ MINIBENCH_TEST_GPU = ' ' }; Muster = 'Grafiklast entfällt: Rendertest nicht möglich' }
    )
}

BeforeAll {
    . (Join-Path $PSScriptRoot 'Hilfen.ps1')
    . (Join-Path $PSScriptRoot 'Lauf.Hilfen.ps1')
    $script:Laeufe = @{}
    function Get-Lauf([string]$Name, [string[]]$Argumente, [hashtable]$Umgebung = @{}) {
        if (-not $script:Laeufe.ContainsKey($Name)) { $script:Laeufe[$Name] = Invoke-MinibenchLauf -Argumente $Argumente -Name $Name -TimeoutSec 400 -Umgebung $Umgebung }
        return $script:Laeufe[$Name]
    }
}

AfterAll {
    foreach ($l in @($script:Laeufe.Values)) { if ($l.Wurzel -and (Test-Path -LiteralPath $l.Wurzel)) { Remove-Item -LiteralPath $l.Wurzel -Recurse -Force -ErrorAction SilentlyContinue } }
}

Describe 'Gesamtlauf <Name>' -ForEach $global:LaufFaelle -Skip:$global:LaufSkip {
    BeforeAll { $script:L = Get-Lauf $Name $Argumente $(if ($Umgebung) { $Umgebung } else { @{} }) }
    It 'Bericht enthält das erwartete Ergebnis' -Skip:(-not $Muster) {
        $script:L.Bericht | Should -Match $Muster
    }
    It 'lokaler Arbeitsordner ist nach dem Lauf gelöscht und Anhang.zip liegt im Berichtsordner' {
        @(Get-ChildItem -LiteralPath (Join-Path $script:L.Wurzel 'Temp') -Recurse -Directory -Filter 'Lauf_*' -ErrorAction SilentlyContinue).Count | Should -Be 0
        Join-Path $script:L.Ordner 'Anhang.zip' | Should -Exist
        Join-Path $script:L.Ordner 'Anhang' | Should -Not -Exist
    }
    It 'endet mit DONE und einem Bericht' {
        $script:L.Ereignisse | Where-Object { $_ -like '@@DONE|*' } | Should -Not -BeNullOrEmpty -Because ($script:L.Fehlerausgabe + ($script:L.Ausgabe | Select-Object -Last 15) -join "`n")
        $script:L.Bericht | Should -Not -BeNullOrEmpty
    }
    It 'Bericht hat Inhalt: Abschnitt des Moduls, Befunde und Testergebnisse' {
        $script:L.Bericht | Should -Match ([regex]::Escape($Abschnitt))
        $script:L.Bericht | Should -Match 'ERGEBNIS: \d+ kritisch'
        ($script:L.Bericht -split "`r?`n").Count | Should -BeGreaterThan 40
    }
    It 'kein Abschnitt bricht ab und kein Modul bleibt ohne Ergebnis' {
        $script:L.Bericht | Should -Not -Match 'FEHLER in diesem Abschnitt'
        $script:L.Bericht | Should -Not -Match 'hat kein Ergebnis geliefert'
    }
    It 'Standby-Sperre wird gesetzt und am Ende aufgehoben' {
        $script:L.Bericht | Should -Match 'Standby-Sperre: aktiv'
        @($script:L.Ausgabe | Where-Object { $_ -match 'Standby-Sperre aufgehoben' }).Count | Should -Be 1
    }
    It 'HTML-Bericht entsteht' {
        $script:L.Html | Should -Match '<html'
        # ab v2.7: Benchmark mit eigenem Abschnitt Sensoren
        if ($Argumente -contains 'Benchmark' -or $Argumente -contains 'Benchmark,Lasttest') { $script:L.Html | Should -Match '<h2>Sensoren während des Benchmarks</h2>' }
    }
}

Describe 'Gesamtlauf mit Fehlerfall: <Name>' -ForEach $global:LaufFehlerFaelle -Skip:$global:LaufSkip {
    BeforeAll { $script:L = Get-Lauf $Name $Argumente $Umgebung }
    It 'endet mit Bericht' {
        $script:L.Ereignisse | Where-Object { $_ -like '@@DONE|*' } | Should -Not -BeNullOrEmpty -Because ($script:L.Fehlerausgabe + ($script:L.Ausgabe | Select-Object -Last 15) -join "`n")
    }
    It 'meldet den Fehler als Befund' {
        $script:L.Bericht | Should -Match $Muster
    }
    It 'kein Abschnitt bricht ab' {
        $script:L.Bericht | Should -Not -Match 'FEHLER in diesem Abschnitt'
        $script:L.Bericht | Should -Not -Match 'hat kein Ergebnis geliefert'
    }
}

Describe 'Gesamtlauf System-Umzug' -Skip:$global:LaufSkip {
    BeforeAll {
        $script:UmzugRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('MinibenchTest_Umzug_' + [guid]::NewGuid().ToString('N'))
        [System.IO.Directory]::CreateDirectory($script:UmzugRoot) | Out-Null
        $mockUser = Join-Path $script:UmzugRoot 'SourceUser'
        $mockDocs = Join-Path $mockUser 'Documents'
        [System.IO.Directory]::CreateDirectory($mockDocs) | Out-Null
        [System.IO.File]::WriteAllText((Join-Path $mockDocs 'test1.txt'), 'Hello world 1', [System.Text.Encoding]::UTF8)
        [System.IO.File]::WriteAllText((Join-Path $mockDocs 'test2.bin'), 'Binary sample content', [System.Text.Encoding]::UTF8)

        $script:UmzugDest = Join-Path $script:UmzugRoot 'BackupTarget'
        [System.IO.Directory]::CreateDirectory($script:UmzugDest) | Out-Null

        $builtScript = Join-Path $script:UmzugRoot 'LeosMinibench.ps1'
        [System.IO.File]::WriteAllText($builtScript, (Get-MinibenchBuild).Text, (New-Object System.Text.UTF8Encoding($true)))
        # Frisch gebaut aus src
        # Build-Check erledigt
        # Bereit fuer Ausfuehrung

        $cmdArgs = @(
            '-NoProfile',
            '-ExecutionPolicy', 'Bypass',
            '-File', ('"{0}"' -f $builtScript),
            '-Umzug',
            '-UmzugZiel', ('"{0}"' -f $script:UmzugDest),
            '-UmzugUmfang', 'Layout,Programme'
        )
        $pinfo = New-Object System.Diagnostics.ProcessStartInfo
        $pinfo.FileName = 'powershell.exe'
        $pinfo.Arguments = ($cmdArgs -join ' ')
        $pinfo.UseShellExecute = $false
        $pinfo.RedirectStandardOutput = $true
        $pinfo.RedirectStandardError = $true
        $pinfo.CreateNoWindow = $true
        $script:UmzugProcess = [System.Diagnostics.Process]::Start($pinfo)
        $script:UmzugOut = $script:UmzugProcess.StandardOutput.ReadToEnd()
        $script:UmzugErr = $script:UmzugProcess.StandardError.ReadToEnd()
        $script:UmzugProcess.WaitForExit(60000)
    }

    AfterAll {
        if ($script:UmzugRoot -and (Test-Path -LiteralPath $script:UmzugRoot)) {
            Remove-Item -LiteralPath $script:UmzugRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'beendet erfolgreich mit ExitCode 0' {
        $script:UmzugProcess.ExitCode | Should -Be 0
    }

    It 'erzeugt Sicherungsordner mit Pruefprotokoll und Wiederherstellungsskripten' {
        $bk = Get-ChildItem -LiteralPath $script:UmzugDest -Directory -Filter 'Minibench-Umzug_*' | Select-Object -First 1
        $bk | Should -Not -BeNullOrEmpty
        Join-Path $bk.FullName 'Pruefprotokoll.txt' | Should -Exist
        Join-Path $bk.FullName 'Wiederherstellen.cmd' | Should -Exist
        Join-Path $bk.FullName 'Wiederherstellen.ps1' | Should -Exist

        $proto = Get-Content -LiteralPath (Join-Path $bk.FullName 'Pruefprotokoll.txt') -Raw -Encoding UTF8
        $proto | Should -Match 'Sicherung vollstaendig: Ja'
    }

    It 'unterstuetzt den Trockenlauf in Wiederherstellen.ps1' {
        $bk = Get-ChildItem -LiteralPath $script:UmzugDest -Directory -Filter 'Minibench-Umzug_*' | Select-Object -First 1
        $ps1 = Join-Path $bk.FullName 'Wiederherstellen.ps1'

        $dryOut = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $ps1 -Trockenlauf
        $dryTxt = ($dryOut -join "`n")
        $dryTxt | Should -Match 'MODUS: TROCKENLAUF'
        $dryTxt | Should -Match 'Trockenlauf abgeschlossen'
    }
}
