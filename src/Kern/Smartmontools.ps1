#region ---------- Vorbereitung: smartmontools ----------
$script:SmartLicense = 'GPL-2.0-or-later'
$script:SmartSource  = 'smartmontools.org, per winget (smartmontools.smartmontools)'
$script:ToolNotes    = New-Object System.Collections.Generic.List[string]

function Find-Smartctl {
    # auf diesem PC installiert (PATH oder Programme): gehört zum PC, nicht zum Stick
    $tools = Get-ToolsDir
    $c = Get-Command smartctl.exe -ErrorAction SilentlyContinue
    if ($c -and -not ($tools -and $c.Source -like ($tools + '*'))) { return $c.Source }
    foreach ($p in @("$env:ProgramFiles\smartmontools\bin\smartctl.exe", "${env:ProgramFiles(x86)}\smartmontools\bin\smartctl.exe")) { if ($p -and (Test-Path -LiteralPath $p)) { return $p } }
    # portabel aus Minibench-Daten\Tools: nur mit passendem Eintrag im Werkzeug-Manifest
    $leg = Register-LegacyTool 'smartctl' @('smartmontools\bin\smartctl.exe', 'smartctl.exe') $script:SmartLicense $script:SmartSource
    if ($leg) { $script:ToolNotes.Add(('smartctl.exe aus Version 2.1 wurde ins Werkzeug-Manifest aufgenommen (SHA-256 {0}…). Ab jetzt läuft nur diese Datei.' -f $leg.SHA256.Substring(0, 12))) }
    $v = Get-VerifiedTool 'smartctl'
    if ($v) { return $v }
    # smartctl.exe neben dem Skript (frühere Ablage) wird nicht mehr ausgeführt
    if ($PSScriptRoot) {
        foreach ($p in @((Join-Path $PSScriptRoot 'smartctl.exe'), (Join-Path $PSScriptRoot 'Tools\smartctl.exe'))) {
            if (Test-Path -LiteralPath $p) { $script:ToolNotes.Add(('{0} liegt außerhalb von Minibench-Daten\Tools und wird nicht ausgeführt. Über "smartmontools holen" neu aufnehmen lassen.' -f $p)); break }
        }
    }
    return $null
}

function Uninstall-Smartmontools {
    $done = $false
    # 1. NSIS-Uninstaller direkt und strikt lautlos mit /S aufrufen (verhindert interaktive Abfrage-Popups)
    $candidates = @(
        "$env:ProgramFiles\smartmontools\uninstall.exe",
        "${env:ProgramFiles(x86)}\smartmontools\uninstall.exe"
    ) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }
    if ($candidates.Count -gt 0) {
        $uninst = $candidates[0]
        $r = Invoke-External -File $uninst -Arguments '/S' -TimeoutSec 120 -Progress 'smartmontools wird deinstalliert'
        if ($r.ExitCode -eq 0 -or -not (Test-Path -LiteralPath $uninst)) {
            $done = $true
        }
    }
    # 2. Absicherung über winget (falls vorhanden und nötig)
    $stillThere = @("$env:ProgramFiles\smartmontools\bin\smartctl.exe", "${env:ProgramFiles(x86)}\smartmontools\bin\smartctl.exe") | Where-Object { $_ -and (Test-Path -LiteralPath $_) }
    if ($stillThere.Count -gt 0) {
        $wg = Get-Command winget.exe -ErrorAction SilentlyContinue
        if (-not $wg) {
            $wp = Get-ChildItem "$env:ProgramFiles\WindowsApps\Microsoft.DesktopAppInstaller_*_x64__8wekyb3d8bbwe\winget.exe" -ErrorAction SilentlyContinue | Sort-Object FullName -Descending | Select-Object -First 1
            if ($wp) { $wg = [pscustomobject]@{ Source = $wp.FullName } }
        }
        if ($wg) {
            $u = Invoke-External -File $wg.Source -Arguments 'uninstall --id smartmontools.smartmontools -e --silent --disable-interactivity --accept-source-agreements' -TimeoutSec 180 -Progress 'smartmontools-Paket wird entfernt'
            if ($u.ExitCode -eq 0) { $done = $true }
        }
    }
    $stillThereAfter = @("$env:ProgramFiles\smartmontools\bin\smartctl.exe", "${env:ProgramFiles(x86)}\smartmontools\bin\smartctl.exe") | Where-Object { $_ -and (Test-Path -LiteralPath $_) }
    return ($stillThereAfter.Count -eq 0)
}
#endregion

