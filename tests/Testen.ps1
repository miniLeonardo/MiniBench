# Führt alle Pester-Tests in diesem Ordner aus. Rückgabe: 0 = alles bestanden, 99 = Pester 5 fehlt,
# 97 = Testlauf abgebrochen (Invoke-Pester ohne Ergebnis), sonst Zahl der Fehlschläge (höchstens 96).
# Als Fehlschlag zählen fehlgeschlagene Tests, Testdateien, die nicht laden (FailedContainers), und fehlgeschlagene
# Blöcke wie BeforeAll oder AfterAll (FailedBlocks); ab v4.11, vorher zählten nur fehlgeschlagene Tests.
# Aufruf über Testen.cmd oder Bauen.cmd. -Kurz zeigt nur Fehlschläge und die Summe.
# -Zusammenfassung <Datei> schreibt das Ergebnis in eine Zeile (für Stand.txt im Aktuellen Build).
# -Gesamtlauf führt unter Windows auch die Gesamtläufe aus (Lauf.Tests.ps1, einige Minuten, Attrappen statt Systemänderungen).
# -Datei <Name[,Name]> führt gezielt einzelne Testdateien aus (z. B. -Datei Oberflaeche oder -Datei Oberflaeche,Release).
# Testen.cmd reicht alle Parameter durch: Testen.cmd -Datei Oberflaeche
param([switch]$Kurz, [string]$Zusammenfassung = '', [switch]$Gesamtlauf, [string]$Datei = '')
if ($Gesamtlauf) { $env:MINIBENCH_GESAMTLAUF = '1' }
$userModules = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'WindowsPowerShell\Modules'
if ($userModules -and (Test-Path $userModules) -and ($env:PSModulePath -notlike "*$userModules*")) {
    $env:PSModulePath = "$userModules;$env:PSModulePath"
}
$pester = Get-Module Pester -ListAvailable | Where-Object { $_.Version -ge [version]'5.0' } | Sort-Object Version -Descending | Select-Object -First 1
if (-not $pester) {
    Write-Host ''
    Write-Host '  Pester 5 ist nicht installiert (Windows bringt nur Pester 3.4 mit). Einmalig in PowerShell ausführen:' -ForegroundColor Yellow
    Write-Host '    Install-Module Pester -Scope CurrentUser -Force -SkipPublisherCheck' -ForegroundColor Yellow
    exit 99
}
Import-Module $pester.Path -Force
$cfg = New-PesterConfiguration
if ($Datei) {
    # eine oder mehrere Dateien, durch Komma getrennt; ".Tests.ps1" darf fehlen (z. B. -Datei Oberflaeche,Release)
    $ziele = @(foreach ($d in ($Datei -split ',')) {
            $t = $d.Trim()
            if (-not $t) { continue }
            if (-not $t.EndsWith('.ps1', [StringComparison]::OrdinalIgnoreCase)) { $t = $t + '.Tests.ps1' }
            if (-not [IO.Path]::IsPathRooted($t)) { $t = Join-Path $PSScriptRoot $t }
            if (-not (Test-Path -LiteralPath $t)) { Write-Host ('  Testdatei nicht gefunden: {0}' -f $t) -ForegroundColor Red; exit 1 }
            $t
        })
    $cfg.Run.Path = [string[]]$ziele
} else {
    $cfg.Run.Path = $PSScriptRoot
}
$cfg.Run.PassThru = $true
$cfg.Output.Verbosity = $(if ($Kurz) { 'Normal' } else { 'Detailed' })
# Ein Abbruch von Invoke-Pester (Ausnahme oder kein Ergebnis) gilt als Fehlschlag (Rückgabe 97), nie als bestanden.
$res = $null; $abbruch = ''
try {
    $res = @(Invoke-Pester -Configuration $cfg) | Where-Object { $_ -and $_.PSObject.Properties['FailedCount'] } | Select-Object -Last 1
} catch { $abbruch = [string]$_.Exception.Message }
if (-not $res -and -not $abbruch) { $abbruch = 'Invoke-Pester hat kein Ergebnis geliefert' }
# übersetzte Oberfläche der Tests (Hilfen.ps1, Get-GuiUebersetzung) wieder entfernen
if ($global:MinibenchGuiUebersetzung) { Remove-Item -LiteralPath $global:MinibenchGuiUebersetzung.Ordner -Recurse -Force -ErrorAction SilentlyContinue; $global:MinibenchGuiUebersetzung = $null; $global:MinibenchGuiSelbsttest = $null }
$nTests = 0; $nDateien = 0; $nBloecke = 0
if ($res) {
    $nTests = [int]$res.FailedCount
    if ($res.PSObject.Properties['FailedContainersCount']) { $nDateien = [int]$res.FailedContainersCount }
    if ($res.PSObject.Properties['FailedBlocksCount']) { $nBloecke = [int]$res.FailedBlocksCount }
    # Pester meldet einen fehlgeschlagenen Lauf, ohne dass einer der Zähler steigt: als ein Fehlschlag werten
    if (($nTests + $nDateien + $nBloecke) -eq 0 -and [string]$res.Result -eq 'Failed') { $nBloecke = 1 }
}
$fehler = $nTests + $nDateien + $nBloecke
$zusatz = ''
if ($nDateien -gt 0) { $zusatz += (', {0} Testdateien nicht geladen' -f $nDateien) }
if ($nBloecke -gt 0) { $zusatz += (', {0} Blöcke fehlgeschlagen' -f $nBloecke) }
if ($abbruch) {
    $line = 'Testlauf abgebrochen: {0} (Pester {1}, PowerShell {2})' -f $abbruch, $pester.Version, $PSVersionTable.PSVersion
} else {
    $line = '{0} bestanden, {1} fehlgeschlagen, {2} übersprungen{3} (Pester {4}, PowerShell {5})' -f $res.PassedCount, $res.FailedCount, $res.SkippedCount, $zusatz, $pester.Version, $PSVersionTable.PSVersion
}
if ($Zusammenfassung) { [IO.File]::WriteAllText($Zusammenfassung, $line) }
Write-Host ''
if ($abbruch) {
    Write-Host ('  Tests: Testlauf abgebrochen: {0} (Pester {1})' -f $abbruch, $pester.Version) -ForegroundColor Red
    exit 97
}
Write-Host ('  Tests: {0} bestanden, {1} fehlgeschlagen, {2} übersprungen{3} (Pester {4})' -f $res.PassedCount, $res.FailedCount, $res.SkippedCount, $zusatz, $pester.Version) -ForegroundColor $(if ($fehler) { 'Red' } else { 'Green' })
exit [math]::Min(96, $fehler)
