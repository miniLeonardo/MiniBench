#region ---------- Änderungsprotokoll mit Rückgängig ----------
# Jede Änderung der Stufe Ändern speichert vor dem Schreiben den Vorher-Wert und das Wissen, wie sie umzukehren ist.
# Ablage je Lauf: Minibench-Daten\Änderungen\<PC>\<Datum>.json, gesicherte Dateien (z. B. Energiepläne) im gleichnamigen Ordner.
# Die Datei wird nach jedem Eintrag sofort geschrieben, damit sie auch einen Absturz mitten im Lauf übersteht.
# Eingriffe ohne automatisches Rückgängig erscheinen ebenfalls, mit dem Weg zurück als Hinweis (z. B. Wiederherstellungspunkt).
#
# Status eines Eintrags: aktiv (rückgängig machbar), rückgängig, übersprungen (Wert inzwischen anders), fehlgeschlagen, nur Hinweis
# ab v4.11: Ein fehlgeschlagenes Rückgängig lässt den Eintrag aktiv (Fehler in Ergebnis, Zähler Fehlversuche), damit ein
# neuer Versuch möglich ist. "fehlgeschlagen" aus älteren Versionen gilt ebenso als offen (Test-ChangeOffen).
# Zuordnung zum Gerät über die GeraetId des Protokolls, der Computername nur für Protokolle ohne GeraetId (Test-ChangeLogDiesesGeraet).
$script:ChangeFormat = 'Minibench-Aenderungen/1'
$script:ChangeFile   = ''
$script:ChangeLog    = $null
$script:ChangeCount  = 0

function Get-ChangeRoot { if ($script:DataDir) { return (Join-Path $script:DataDir 'Änderungen') } else { return '' } }

function Save-ChangeFile([string]$Path, $Log) {
    $dir = Split-Path $Path -Parent
    New-Item -ItemType Directory -Path $dir -Force -ErrorAction Stop | Out-Null
    $tmp = $Path + '.tmp'
    [IO.File]::WriteAllText($tmp, ($Log | ConvertTo-Json -Depth 8), (New-Object Text.UTF8Encoding($false)))
    [IO.File]::Copy($tmp, $Path, $true)
    Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
}

function Read-ChangeFile([string]$Path) {
    try { $j = Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json } catch { return $null }
    if (-not $j -or [string]$j.Format -ne $script:ChangeFormat) { return $null }
    return $j
}

# Lauf beginnen: Die Datei entsteht erst mit dem ersten Eintrag, Läufe ohne Änderungen hinterlassen nichts
function Start-ChangeLog {
    $root = Get-ChangeRoot
    if (-not $root) { $script:ChangeFile = ''; return }
    $stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
    $script:ChangeFile = Join-Path (Join-Path $root (Get-SafeName $env:COMPUTERNAME)) ($stamp + '.json')
    $dev = Get-DeviceIdentity
    $script:ChangeLog = [ordered]@{
        Format = $script:ChangeFormat; Computer = $env:COMPUTERNAME; GeraetId = $dev.Id; Version = $ScriptVersion
        Lauf = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss', [Globalization.CultureInfo]::InvariantCulture); Bericht = [string]$OutputDir; Eintraege = @()
    }
    $script:ChangeCount = 0
}

# Ordner für Sicherungsdateien dieses Laufs (gleicher Name wie die Protokolldatei ohne .json)
function Get-ChangeFilesDir { if ($script:ChangeFile) { return ($script:ChangeFile -replace '\.json$', '') } else { return '' } }

function Add-ChangeRecord {
    param(
        [string]$Modul, [string]$Schritt, [string]$Titel, [string]$Risiko, [string]$Art, [string]$Ziel,
        [string]$Vorher = '', [string]$Nachher = '', $Daten = $null, [string]$Gegenbefehl = '', [switch]$NurHinweis
    )
    if (-not $script:ChangeLog) { Start-ChangeLog }
    if (-not $script:ChangeFile) { return $null }
    $script:ChangeCount++
    $r = [ordered]@{
        Id = $script:ChangeCount; Zeit = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss', [Globalization.CultureInfo]::InvariantCulture)
        Modul = $Modul; Schritt = $Schritt; Titel = $Titel; Risiko = $Risiko; Art = $Art; Ziel = $Ziel; Vorher = $Vorher; Nachher = $Nachher
        Daten = $Daten; Gegenbefehl = $Gegenbefehl; Status = $(if ($NurHinweis) { 'nur Hinweis' } else { 'aktiv' }); Ergebnis = ''
    }
    $script:ChangeLog.Eintraege = @(@($script:ChangeLog.Eintraege) + [pscustomobject]$r)
    try { Save-ChangeFile $script:ChangeFile $script:ChangeLog } catch { Write-Warning ('Änderungsprotokoll konnte nicht geschrieben werden: {0}' -f $_.Exception.Message) }
    return [pscustomobject]$r
}

# ---------- Zugriffe auf das System (einzeln, damit Tests sie ersetzen können) ----------
# Name '(default)' meint den Standardwert eines Schlüssels (ab v2.8, Kontextmenüs des Moduls Optimierung)
function Get-RegValueState([string]$Path, [string]$Name) {
    $none = [pscustomobject]@{ Vorhanden = $false; Wert = $null; Typ = '' }
    try { $k = Get-Item -LiteralPath $Path -ErrorAction Stop } catch { return $none }
    $n = $(if ($Name -eq '(default)') { '' } else { $Name })
    if (@($k.GetValueNames()) -notcontains $n) { return $none }
    return [pscustomobject]@{ Vorhanden = $true; Wert = $k.GetValue($n, $null, 'DoNotExpandEnvironmentNames'); Typ = [string]$k.GetValueKind($n) }
}

# Wert in den Registry-Typ wandeln. Felder mit Komma zurückgeben, sonst zerlegt PowerShell sie in Einzelwerte.
function ConvertTo-RegValue($Value, [string]$Type) {
    switch ($Type) {
        'DWord'       { return [int]$Value }
        'QWord'       { return [long]$Value }
        'Binary'      { return , ([byte[]]@($Value)) }
        'MultiString' { return , ([string[]]@($Value)) }
        default       { return [string]$Value }
    }
}

# ab v4.11: Set-RegValueState vermerkt in $script:RegNeueSchluessel die Schlüssel, die es selbst anlegen musste (oberster
# zuerst). Set-RegistryValueLogged übernimmt sie ins Protokoll, Rückgängig entfernt sie wieder, wenn sie danach leer sind.
# Bewusst keine Rückgabe: Aufrufer im Modul Optimierung rufen Set-RegValueState ohne Zuweisung auf.
$script:RegNeueSchluessel = @()
function Set-RegValueState([string]$Path, [string]$Name, $Value, [string]$Type) {
    $neu = New-Object System.Collections.Generic.List[string]
    if (-not (Test-Path -LiteralPath $Path)) {
        $p = $Path
        while ($p -and -not (Test-Path -LiteralPath $p)) {
            $neu.Insert(0, $p)
            $oben = Split-Path $p -Parent
            if (-not $oben -or $oben -eq $p) { break }
            $p = $oben
        }
        New-Item -Path $Path -Force -ErrorAction Stop | Out-Null
    }
    $script:RegNeueSchluessel = @($neu)
    Set-ItemProperty -LiteralPath $Path -Name $Name -Value (ConvertTo-RegValue $Value $Type) -Type $Type -ErrorAction Stop
}

function Remove-RegValue([string]$Path, [string]$Name) { Remove-ItemProperty -LiteralPath $Path -Name $Name -ErrorAction Stop }

# ab v4.11: Schlüssel nur entfernen, wenn er weder Werte noch Unterschlüssel hat. Rückgabe: $true, wenn entfernt.
function Remove-RegKeyIfEmpty([string]$Path) {
    try { $k = Get-Item -LiteralPath $Path -ErrorAction Stop } catch { return $false }
    if (@($k.GetValueNames()).Count -or $k.SubKeyCount) { return $false }
    Remove-Item -LiteralPath $Path -ErrorAction Stop
    return $true
}

# Selbst angelegte Schlüssel von unten nach oben entfernen, solange sie leer sind. Rückgabe: Zahl der entfernten Schlüssel.
function Remove-NeueRegSchluessel($Pfade) {
    $liste = @(@($Pfade) | Where-Object { $_ } | ForEach-Object { [string]$_ })
    $n = 0
    for ($i = $liste.Count - 1; $i -ge 0; $i--) {
        if (-not (Remove-RegKeyIfEmpty $liste[$i])) { break }
        $n++
    }
    return $n
}

function Get-ServiceStartType([string]$Name) { $s = Get-Service -Name $Name -ErrorAction Stop; return [string]$s.StartType }
function Set-ServiceStartType([string]$Name, [string]$StartType) { Set-Service -Name $Name -StartupType $StartType -ErrorAction Stop }

# ab v4.11: verzögerter Autostart. Get-Service kennt ihn unter PowerShell 5.1 nicht (StartType "Automatic"), er steht im
# Dienstschlüssel als DelayedAutostart = 1. Gesetzt wird er über sc.exe, damit die Dienststeuerung ihn sofort übernimmt.
function Get-ServiceDelayedAutostart([string]$Name) {
    try { $v = (Get-ItemProperty -LiteralPath ('HKLM:\SYSTEM\CurrentControlSet\Services\' + $Name) -Name DelayedAutostart -ErrorAction Stop).DelayedAutostart } catch { return $false }
    return ([int]$v -eq 1)
}
function Set-ServiceDelayedAutostart([string]$Name) {
    $sc = $(if ($env:SystemRoot) { Join-Path $env:SystemRoot 'System32\sc.exe' } else { 'sc.exe' })
    $r = Invoke-External -File $sc -Arguments ('config "{0}" start= delayed-auto' -f $Name) -TimeoutSec 30
    if ($r.ExitCode -ne 0) { throw ('sc.exe config {0} start= delayed-auto meldet {1}' -f $Name, $r.ExitCode) }
}

# ab v4.11: Computerschutz (Systemwiederherstellung) je Laufwerk lesen und schalten.
# Gelesen wird HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SPP\Clients, Wert {09F7EDC5-294E-4180-AF6A-FB0E6A0E9513}:
# Dort führt Windows die geschützten Volumes je Laufwerk ("...:(C%3A)"), genau das ändern Enable-/Disable-ComputerRestore.
# Das ist je Laufwerk eindeutig und schnell, ohne vssadmin (Textausgabe je Sprache) oder WMI. RPSessionInterval gilt nur für
# alle Laufwerke zusammen (0 = überall aus) und dient als Ersatz, wenn SPP\Clients nicht lesbar ist; Get-ComputerRestorePoint
# zeigt nur, ob Punkte existieren. Eine Richtlinie DisableSR = 1 schaltet ihn ab, Einschalten scheitert dann.
$script:SrSchluessel = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore'
$script:SppClients   = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SPP\Clients'
$script:SppSrId      = '{09F7EDC5-294E-4180-AF6A-FB0E6A0E9513}'
function Get-ComputerschutzQuellen {
    $q = [pscustomobject]@{ Richtlinie = $false; SppLesbar = $false; Spp = @(); Intervall = $null }
    try { if ([int](Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore' -Name DisableSR -ErrorAction Stop).DisableSR -eq 1) { $q.Richtlinie = $true } } catch { }
    try { $q.Intervall = [int](Get-ItemProperty -LiteralPath $script:SrSchluessel -Name RPSessionInterval -ErrorAction Stop).RPSessionInterval } catch { }
    try {
        $k = Get-Item -LiteralPath $script:SppClients -ErrorAction Stop
        $q.Spp = @(@($k.GetValue($script:SppSrId, $null)) | Where-Object { $_ } | ForEach-Object { [string]$_ })
        $q.SppLesbar = $true
    } catch [System.Management.Automation.ItemNotFoundException] { $q.SppLesbar = $true } catch { }
    return $q
}
# Zustand aus den Quellen: an, aus oder unbekannt
function Get-ComputerschutzStatus([string]$Laufwerk = ($env:SystemDrive + '\'), $Quellen = $null) {
    if (-not $Quellen) { $Quellen = Get-ComputerschutzQuellen }
    if ($Quellen.Richtlinie) { return 'aus' }
    $bst = ([string]$Laufwerk).Trim().TrimEnd('\', ':')
    if ($Quellen.SppLesbar -and $bst) {
        $muster = '\(' + [regex]::Escape($bst) + '%3A\)'
        if (@($Quellen.Spp | Where-Object { $_ -match $muster }).Count) { return 'an' }
        return 'aus'
    }
    if ($null -ne $Quellen.Intervall -and [int]$Quellen.Intervall -eq 0) { return 'aus' }
    return 'unbekannt'
}
function Set-ComputerschutzZustand([string]$Laufwerk, [bool]$An) {
    if ($An) { Enable-ComputerRestore -Drive $Laufwerk -ErrorAction Stop } else { Disable-ComputerRestore -Drive $Laufwerk -ErrorAction Stop }
}

# Computerschutz einschalten, nur wenn er aus war, mit Eintrag im Änderungsprotokoll (Risiko Ändern, Gegenbefehl
# Disable-ComputerRestore). Ist der Vorher-Zustand nicht lesbar, wird eingeschaltet und nur ein Hinweis protokolliert:
# Ein automatisches Ausschalten könnte sonst einen vorher schon aktiven Schutz samt Wiederherstellungspunkten löschen.
# Rückgabe: $true, wenn der Schutz danach an ist.
function Enable-ComputerschutzMitProtokoll {
    param([string]$Laufwerk = "$env:SystemDrive\", [string]$Modul = 'Wartung')
    if (-not $Laufwerk.EndsWith('\')) { $Laufwerk += '\' }
    $vorher = Get-ComputerschutzStatus $Laufwerk
    if ($vorher -eq 'an') { return $true }
    try { Set-ComputerschutzZustand $Laufwerk $true } catch { Write-Warning ('Computerschutz für {0} ließ sich nicht einschalten: {1}' -f $Laufwerk, $_.Exception.Message); return $false }
    $nachher = Get-ComputerschutzStatus $Laufwerk
    if ($nachher -eq 'aus') { return $false }
    $titel = ('Computerschutz für {0} eingeschaltet' -f $Laufwerk)
    $gegen = ('Disable-ComputerRestore -Drive "{0}" (löscht die Wiederherstellungspunkte dieses Laufwerks)' -f $Laufwerk)
    if ($vorher -eq 'aus') {
        [void](Add-ChangeRecord -Modul $Modul -Schritt 'Computerschutz' -Titel $titel -Risiko 'Aendern' -Art 'Computerschutz' -Ziel ('Computerschutz {0}' -f $Laufwerk) `
            -Vorher 'aus' -Nachher 'an' -Daten ([ordered]@{ Laufwerk = $Laufwerk }) -Gegenbefehl $gegen)
    } else {
        [void](Add-ChangeRecord -Modul $Modul -Schritt 'Computerschutz' -Titel $titel -Risiko 'Aendern' -Art 'Computerschutz' -Ziel ('Computerschutz {0}' -f $Laufwerk) `
            -Vorher 'nicht lesbar' -Nachher 'an' -Daten ([ordered]@{ Laufwerk = $Laufwerk }) -Gegenbefehl ('Nur wenn er vorher aus war: ' + $gegen) -NurHinweis)
    }
    return $true
}

function Invoke-PowerCfg([string]$Arguments) { return (Invoke-External -File 'powercfg.exe' -Arguments $Arguments -TimeoutSec 60) }

# Energiepläne aus powercfg /list: GUID, Name, aktiv
function Get-PowerSchemes {
    $r = Invoke-PowerCfg '/list'
    return @(ConvertFrom-PowerCfgList $r.Output)
}
function ConvertFrom-PowerCfgList([string]$Text) {
    foreach ($ln in @($Text -split "`r?`n")) {
        if ($ln -match '([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\s*\(([^)]*)\)\s*(\*)?') {
            [pscustomobject]@{ Guid = $Matches[1].ToLowerInvariant(); Name = $Matches[2].Trim(); Aktiv = [bool]$Matches[3] }
        }
    }
}

# Werte vergleichbar und lesbar machen (Zahlen, Text, Listen)
function Get-ValueText($Value) {
    if ($null -eq $Value) { return '(nicht vorhanden)' }
    if ($Value -is [array] -or $Value -is [System.Collections.IList]) { return ((@($Value) | ForEach-Object { [string]$_ }) -join ',') }
    return [string]$Value
}

# ---------- Protokollierte Änderungen ----------
# Registry-Wert setzen. Rückgabe: Eintrag des Protokolls, $null wenn der Wert schon stimmte. Fehler beim Schreiben werfen.
function Set-RegistryValueLogged {
    param([string]$Path, [string]$Name, $Value, [string]$Type = 'DWord', [string]$Modul, [string]$Schritt, [string]$Titel)
    $before = Get-RegValueState $Path $Name
    if ($before.Vorhanden -and $before.Typ -eq $Type -and (Get-ValueText $before.Wert) -eq (Get-ValueText (ConvertTo-RegValue $Value $Type))) { return $null }
    $script:RegNeueSchluessel = @()
    Set-RegValueState $Path $Name $Value $Type
    $neu = @($script:RegNeueSchluessel | Where-Object { $_ })
    $daten = [ordered]@{ Pfad = $Path; Name = $Name; VorherVorhanden = [bool]$before.Vorhanden; VorherWert = $before.Wert; VorherTyp = $before.Typ; NachherWert = (ConvertTo-RegValue $Value $Type); NachherTyp = $Type }
    # ab v4.11: selbst angelegte Schlüssel (oberster zuerst), Rückgängig entfernt sie, wenn sie danach leer sind
    if ($neu.Count) { $daten['NeueSchluessel'] = $neu }
    $gegen = $(if ($before.Vorhanden) { 'Set-ItemProperty -LiteralPath ''{0}'' -Name {1} -Value {2} -Type {3}' -f $Path, $Name, (Get-ValueText $before.Wert), $before.Typ } else { 'Remove-ItemProperty -LiteralPath ''{0}'' -Name {1}' -f $Path, $Name })
    return (Add-ChangeRecord -Modul $Modul -Schritt $Schritt -Titel $Titel -Risiko 'Aendern' -Art 'Registry' -Ziel ('{0}\{1}' -f $Path, $Name) `
        -Vorher (Get-ValueText $before.Wert) -Nachher (Get-ValueText $Value) -Daten $daten -Gegenbefehl $gegen)
}

function Set-ServiceStartTypeLogged {
    param([string]$Name, [string]$StartType, [string]$Modul, [string]$Schritt, [string]$Titel)
    $before = Get-ServiceStartType $Name
    if ($before -eq $StartType) { return $null }
    # ab v4.11: verzögerten Start mitsichern (Get-Service meldet ihn als "Automatic")
    $verz = $false
    if ($before -eq 'Automatic') { $verz = [bool](Get-ServiceDelayedAutostart $Name) }
    Set-ServiceStartType $Name $StartType
    $vorherText = $(if ($verz) { 'Automatic (verzögert)' } else { $before })
    $gegen = $(if ($verz) { 'sc.exe config {0} start= delayed-auto' -f $Name } else { 'Set-Service {0} -StartupType {1}' -f $Name, $before })
    return (Add-ChangeRecord -Modul $Modul -Schritt $Schritt -Titel $Titel -Risiko 'Aendern' -Art 'Dienststart' -Ziel ('Dienst {0}, Starttyp' -f $Name) `
        -Vorher $vorherText -Nachher $StartType -Daten ([ordered]@{ Dienst = $Name; Vorher = $before; Nachher = $StartType; VorherVerzoegert = $verz }) -Gegenbefehl $gegen)
}

# Alle Energiepläne sichern, bevor sie zurückgesetzt werden. Rückgängig spielt jeden gesicherten Plan wieder ein (ab v4.11
# auch noch vorhandene Pläne mit gleicher GUID, deren Einstellungen das Zurücksetzen verändert hat) und setzt den aktiven Plan.
function Backup-PowerSchemesLogged {
    param([string]$Modul, [string]$Schritt, [string]$Titel)
    if (-not $script:ChangeLog) { Start-ChangeLog }
    $dir = Get-ChangeFilesDir
    if (-not $dir) { throw 'Kein Datenordner für die Sicherung der Energiepläne.' }
    New-Item -ItemType Directory -Path $dir -Force -ErrorAction Stop | Out-Null
    $schemes = @(Get-PowerSchemes)
    if (-not $schemes.Count) { throw 'powercfg /list lieferte keine Energiepläne.' }
    $saved = @()
    foreach ($s in $schemes) {
        $f = Join-Path $dir ('Energieplan_{0}.pow' -f $s.Guid)
        $r = Invoke-PowerCfg ('/export "{0}" {1}' -f $f, $s.Guid)
        if ($r.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $f)) { throw ('Energieplan {0} ließ sich nicht sichern (powercfg {1}).' -f $s.Name, $r.ExitCode) }
        $saved += [ordered]@{ Guid = $s.Guid; Name = $s.Name; Datei = (Split-Path $f -Leaf) }
    }
    $active = @($schemes | Where-Object Aktiv | Select-Object -First 1)
    $activeGuid = $(if ($active.Count) { $active[0].Guid } else { '' })
    $activeName = $(if ($active.Count) { $active[0].Name } else { '' })
    return (Add-ChangeRecord -Modul $Modul -Schritt $Schritt -Titel $Titel -Risiko 'Aendern' -Art 'Energieplaene' -Ziel 'Energiesparpläne' `
        -Vorher ('{0} Pläne, aktiv: {1}' -f $saved.Count, $activeName) -Nachher 'Windows-Standardpläne' `
        -Daten ([ordered]@{ AktivVorher = $activeGuid; Sicherungen = $saved }) -Gegenbefehl ('je Plan powercfg /delete <GUID> (falls vorhanden) und powercfg /import <Datei> <GUID>, danach powercfg /setactive {0}' -f $activeGuid))
}

# Eine Änderung dieses Laufs sofort zurücknehmen (vorübergehende Einstellung für eine Messung). Rückgabe: Status und Text.
function Undo-ChangeNow($Record) {
    if (-not $Record -or -not $script:ChangeLog) { return $null }
    $e = @($script:ChangeLog.Eintraege | Where-Object { [int]$_.Id -eq [int]$Record.Id }) | Select-Object -First 1
    if (-not $e) { return $null }
    try { $res = Undo-ChangeRecord $e (Get-ChangeFilesDir) } catch { $res = [pscustomobject]@{ Status = 'fehlgeschlagen'; Text = $_.Exception.Message } }
    Set-ChangeUndoErgebnis $e $res ' (direkt nach der Messung)'
    try { Save-ChangeFile $script:ChangeFile $script:ChangeLog } catch { }
    return $res
}

# ab v4.11: Noch zurücknehmbar? "aktiv" und "fehlgeschlagen" (Status älterer Versionen nach gescheitertem Rückgängig)
function Test-ChangeOffen($Entry) { return (@('aktiv', 'fehlgeschlagen') -contains [string]$Entry.Status) }

# ab v4.11: Stammt ein Protokoll von diesem Gerät? Über die GeraetId, der Computername nur, wenn das Protokoll (oder
# dieses Gerät) keine GeraetId hat. So bleibt die Zuordnung nach dem Umbenennen des PCs erhalten.
function Test-ChangeLogDiesesGeraet($Log, [string]$GeraetId, [string]$Computer = $env:COMPUTERNAME) {
    if ([string]$Log.GeraetId -and $GeraetId) { return ([string]$Log.GeraetId -eq $GeraetId) }
    return ([string]$Log.Computer -eq $Computer)
}
function Get-ChangeGeraetId { try { return [string](Get-DeviceIdentity).Id } catch { return '' } }

# Ergebnis eines Rückgängig in den Eintrag schreiben. ab v4.11: Fehlschlag lässt den Eintrag aktiv (neuer Versuch möglich).
function Set-ChangeUndoErgebnis($Entry, $Res, [string]$Zusatz = '') {
    $zeit = (Get-Date).ToString('yyyy-MM-dd HH:mm', [Globalization.CultureInfo]::InvariantCulture)
    if ([string]$Res.Status -eq 'fehlgeschlagen') {
        $n = 1
        if ($Entry.PSObject.Properties['Fehlversuche']) { $n = [int]$Entry.Fehlversuche + 1 }
        $Entry | Add-Member -NotePropertyName Fehlversuche -NotePropertyValue $n -Force
        $Entry.Status = 'aktiv'
        $Entry.Ergebnis = ('{0}: Rückgängig fehlgeschlagen ({1}. Versuch), bleibt aktiv für einen neuen Versuch: {2}{3}' -f $zeit, $n, $Res.Text, $Zusatz)
    } else {
        $Entry.Status = $Res.Status
        $Entry.Ergebnis = ('{0}: {1}{2}' -f $zeit, $Res.Text, $Zusatz)
    }
}

# ---------- Rückgängig ----------
# Einen Eintrag umkehren. Rückgabe: Status und Text. Ändert den Eintrag nicht selbst (das macht Invoke-ChangeUndo).
function Undo-ChangeRecord($Record, [string]$FilesDir) {
    $d = $Record.Daten
    switch ([string]$Record.Art) {
        'Registry' {
            $now = Get-RegValueState $d.Pfad $d.Name
            if (-not $now.Vorhanden -or (Get-ValueText $now.Wert) -ne (Get-ValueText $d.NachherWert)) {
                return [pscustomobject]@{ Status = 'übersprungen'; Text = ('Wert ist inzwischen {0}, nicht {1}; nichts verändert.' -f (Get-ValueText $now.Wert), (Get-ValueText $d.NachherWert)) }
            }
            if ($d.VorherVorhanden) { Set-RegValueState $d.Pfad $d.Name $d.VorherWert $d.VorherTyp; return [pscustomobject]@{ Status = 'rückgängig'; Text = ('{0} wieder auf {1}' -f $d.Name, (Get-ValueText $d.VorherWert)) } }
            Remove-RegValue $d.Pfad $d.Name
            $txt = ('{0} entfernt (war vorher nicht vorhanden)' -f $d.Name)
            # ab v4.11: selbst angelegte Schlüssel wieder entfernen, wenn sie leer sind
            if (@($d.NeueSchluessel | Where-Object { $_ }).Count) {
                $n = 0
                try { $n = Remove-NeueRegSchluessel $d.NeueSchluessel } catch { $txt += ('; angelegter Schlüssel nicht entfernbar: {0}' -f $_.Exception.Message) }
                if ($n) { $txt += ('; {0} angelegte(r) Schlüssel entfernt' -f $n) }
            }
            return [pscustomobject]@{ Status = 'rückgängig'; Text = $txt }
        }
        'Dienststart' {
            $now = Get-ServiceStartType $d.Dienst
            # ab v4.11: neuer Versuch, nachdem der Starttyp schon zurückgesetzt war, aber sc.exe für den verzögerten Start scheiterte
            if ($d.VorherVerzoegert -eq $true -and $now -eq [string]$d.Vorher -and $now -ne [string]$d.Nachher) {
                if (-not (Get-ServiceDelayedAutostart $d.Dienst)) { Set-ServiceDelayedAutostart $d.Dienst }
                return [pscustomobject]@{ Status = 'rückgängig'; Text = ('Starttyp von {0} wieder Automatic (verzögert)' -f $d.Dienst) }
            }
            if ($now -ne [string]$d.Nachher) { return [pscustomobject]@{ Status = 'übersprungen'; Text = ('Starttyp ist inzwischen {0}, nicht {1}; nichts verändert.' -f $now, $d.Nachher) } }
            Set-ServiceStartType $d.Dienst ([string]$d.Vorher)
            # ab v4.11: verzögerten Start wiederherstellen (Einträge älterer Versionen haben das Feld nicht)
            if ($d.VorherVerzoegert -eq $true) {
                Set-ServiceDelayedAutostart $d.Dienst
                return [pscustomobject]@{ Status = 'rückgängig'; Text = ('Starttyp von {0} wieder Automatic (verzögert)' -f $d.Dienst) }
            }
            return [pscustomobject]@{ Status = 'rückgängig'; Text = ('Starttyp von {0} wieder {1}' -f $d.Dienst, $d.Vorher) }
        }
        'Energieplaene' {
            # ab v4.11: auch Pläne, deren GUID noch existiert (z. B. Standardpläne nach powercfg -restoredefaultschemes), aus der
            # Sicherung neu einspielen: powercfg /import kann keinen vorhandenen Plan überschreiben, daher vorher den aktuellen
            # Stand sichern, den Plan löschen (ist er aktiv, erst einen anderen aktivieren) und mit derselben GUID importieren.
            # Scheitert der Import, kommt der gesicherte aktuelle Stand zurück, der Plan geht also nie verloren.
            $schemes = @(Get-PowerSchemes)
            $present = @($schemes | ForEach-Object { $_.Guid })
            $aktivJetzt = [string](@($schemes | Where-Object { $_.Aktiv } | ForEach-Object { $_.Guid }) | Select-Object -First 1)
            $aktivAnfang = $aktivJetzt
            $msgs = @(); $bad = 0
            foreach ($s in @($d.Sicherungen)) {
                $g = ([string]$s.Guid).ToLowerInvariant()
                $f = Join-Path $FilesDir ([string]$s.Datei)
                if (-not (Test-Path -LiteralPath $f)) { $msgs += ('Sicherung von {0} fehlt' -f $s.Name); $bad++; continue }
                if ($present -contains $g) {
                    if ($aktivJetzt -eq $g) {
                        $ausweich = [string](@($present | Where-Object { $_ -ne $g }) | Select-Object -First 1)
                        if (-not $ausweich) { $msgs += ('{0} ist der einzige Plan und aktiv, Einstellungen nicht ersetzbar' -f $s.Name); $bad++; continue }
                        $r = Invoke-PowerCfg ('/setactive {0}' -f $ausweich)
                        if ($r.ExitCode -ne 0) { $msgs += ('{0}: anderer Plan nicht aktivierbar (Code {1})' -f $s.Name, $r.ExitCode); $bad++; continue }
                        $aktivJetzt = $ausweich
                    }
                    $zwischen = Join-Path $FilesDir ('Energieplan_{0}_vor_Ruecknahme.pow' -f $g)
                    $r = Invoke-PowerCfg ('/export "{0}" {1}' -f $zwischen, $g)
                    if ($r.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $zwischen)) { $msgs += ('{0}: aktueller Stand nicht sicherbar (Code {1}), nichts verändert' -f $s.Name, $r.ExitCode); $bad++; continue }
                    $r = Invoke-PowerCfg ('/delete {0}' -f $g)
                    if ($r.ExitCode -ne 0) { $msgs += ('{0} nicht ersetzbar (powercfg /delete, Code {1})' -f $s.Name, $r.ExitCode); $bad++; continue }
                    $r = Invoke-PowerCfg ('/import "{0}" {1}' -f $f, $g)
                    if ($r.ExitCode -eq 0) {
                        $msgs += ('Einstellungen von {0} wiederhergestellt' -f $s.Name)
                        Remove-Item -LiteralPath $zwischen -Force -ErrorAction SilentlyContinue
                    } else {
                        $r2 = Invoke-PowerCfg ('/import "{0}" {1}' -f $zwischen, $g)
                        $msgs += ('{0} nicht importierbar (Code {1}), {2}' -f $s.Name, $r.ExitCode, $(if ($r2.ExitCode -eq 0) { 'Stand vor der Rücknahme wieder eingespielt' } else { 'Plan fehlt jetzt, Sicherungen: ' + $f + ', ' + $zwischen }))
                        $bad++
                    }
                    continue
                }
                $r = Invoke-PowerCfg ('/import "{0}" {1}' -f $f, $g)
                if ($r.ExitCode -eq 0) { $msgs += ('{0} wiederhergestellt' -f $s.Name) } else { $msgs += ('{0} nicht importierbar (Code {1})' -f $s.Name, $r.ExitCode); $bad++ }
            }
            $ziel = $(if ($d.AktivVorher) { [string]$d.AktivVorher } elseif ($aktivJetzt -ne $aktivAnfang) { $aktivAnfang } else { '' })
            if ($ziel) {
                $r = Invoke-PowerCfg ('/setactive {0}' -f $ziel)
                if ($r.ExitCode -eq 0) { $msgs += $(if ($d.AktivVorher) { 'vorher aktiver Plan wieder aktiv' } else { 'aktiver Plan wieder gesetzt' }) } else { $msgs += ('aktiver Plan nicht setzbar (Code {0})' -f $r.ExitCode); $bad++ }
            }
            return [pscustomobject]@{ Status = $(if ($bad) { 'fehlgeschlagen' } else { 'rückgängig' }); Text = ($msgs -join '; ') }
        }
        'Computerschutz' {
            # ab v4.11: Enable-ComputerschutzMitProtokoll
            $lw = [string]$d.Laufwerk
            $now = Get-ComputerschutzStatus $lw
            if ($now -eq 'aus') { return [pscustomobject]@{ Status = 'übersprungen'; Text = ('Computerschutz für {0} ist bereits aus; nichts verändert.' -f $lw) } }
            Set-ComputerschutzZustand $lw $false
            return [pscustomobject]@{ Status = 'rückgängig'; Text = ('Computerschutz für {0} wieder aus (Wiederherstellungspunkte dieses Laufwerks gelöscht)' -f $lw) }
        }
    }
    # ab v3.53: Softwarepakete der Seite Tools (winget uninstall, nur wenn noch installiert)
    if ([string]$Record.Art -eq 'Softwarepaket') { return (Undo-SoftwarePaket $Record) }
    # ab v2.8: Arten des Moduls Optimierung (geplante Aufgaben, Windows-Funktionen, Laufwerke, Energie, Defender, DNS ...)
    if (Get-Command Undo-OptChange -ErrorAction SilentlyContinue) { $r = Undo-OptChange $Record; if ($r) { return $r } }
    return [pscustomobject]@{ Status = [string]$Record.Status; Text = ('Für {0} gibt es kein automatisches Rückgängig: {1}' -f $Record.Art, $Record.Gegenbefehl) }
}

# Wiederherstellungspunkt anlegen und als Hinweis ins Änderungsprotokoll schreiben (ab v2.8 auch für das Modul Optimierung).
# Windows erlaubt sonst nur einen Punkt je 24 Stunden; die Häufigkeitssperre wird dafür kurz aufgehoben.
function New-RestorePointLogged([string]$Modul, [string]$Beschreibung) {
    $rk = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore'
    $old = $null; try { $old = (Get-ItemProperty $rk -Name SystemRestorePointCreationFrequency -ErrorAction Stop).SystemRestorePointCreationFrequency } catch { }
    try {
        # ab v4.11: nur einschalten, wenn er aus war, und dann mit Eintrag und Gegenbefehl im Änderungsprotokoll (Vorgabe 3)
        try { [void](Enable-ComputerschutzMitProtokoll -Laufwerk ($env:SystemDrive + '\') -Modul $Modul) } catch { }
        if (-not (Test-Path $rk)) { New-Item -Path $rk -Force | Out-Null }
        Set-ItemProperty -Path $rk -Name SystemRestorePointCreationFrequency -Value 0 -Type DWord -ErrorAction SilentlyContinue
        Checkpoint-Computer -Description $Beschreibung -RestorePointType MODIFY_SETTINGS -ErrorAction Stop
        # ab v4.11: nur der Punkt mit genau dieser Beschreibung zählt; bis v4.10 der neueste, auch wenn Windows wegen der
        # Sperrfrist keinen neuen angelegt hatte (dann stand eine fremde Nummer im Protokoll)
        $rp = @(Get-ComputerRestorePoint -ErrorAction SilentlyContinue) | Where-Object { [string]$_.Description -eq $Beschreibung } | Sort-Object SequenceNumber -Descending | Select-Object -First 1
        $no = $(if ($rp) { [string]$rp.SequenceNumber } else { '' })
        $txt = $(if ($rp) { 'angelegt: {0} (Nr. {1})' -f $rp.Description, $rp.SequenceNumber } else { 'angelegt' })
        [void](Add-ChangeRecord -Modul $Modul -Schritt 'Wiederherstellungspunkt' -Titel ('Wiederherstellungspunkt vor dem Modul {0}' -f $Modul) -Risiko 'Lesen' -Art 'Wiederherstellungspunkt' `
            -Ziel 'Computerschutz' -Nachher $txt -Daten ([ordered]@{ Nummer = $no }) -Gegenbefehl ('rstrui.exe starten und Punkt Nr. {0} wählen; macht die Eingriffe dieses Laufs rückgängig' -f $no) -NurHinweis)
        return [pscustomobject]@{ Ok = $true; Text = $txt; Nummer = $no }
    } catch {
        return [pscustomobject]@{ Ok = $false; Text = $_.Exception.Message; Nummer = '' }
    } finally {
        if ($null -ne $old) { Set-ItemProperty -Path $rk -Name SystemRestorePointCreationFrequency -Value $old -Type DWord -ErrorAction SilentlyContinue }
        else { Remove-ItemProperty -Path $rk -Name SystemRestorePointCreationFrequency -ErrorAction SilentlyContinue }
    }
}

# Vorübergehende Änderungen, die nur für eine Messung gelten (GPU-Wahl für winsat.exe). Bricht ein Lauf dazwischen ab
# (Absturz, Abbruch in der Oberfläche), nimmt der nächste Start sie zurück.
$script:TemporarySteps = @('GpuWahl')
function Restore-TemporaryChanges {
    $root = Get-ChangeRoot
    if (-not $root -or -not (Test-Path -LiteralPath $root)) { return $null }
    # ab v4.11: alle Ordner durchsuchen und über die GeraetId zuordnen; bis v4.10 nur der Ordner des Computernamens,
    # nach einem Umbenennen des PCs blieben vorübergehende Änderungen stehen
    $gid = Get-ChangeGeraetId
    $spec = @()
    foreach ($f in @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.json' -ErrorAction SilentlyContinue)) {
        if ($script:ChangeFile -and $f.FullName -eq $script:ChangeFile) { continue }
        $j = Read-ChangeFile $f.FullName
        if (-not $j -or -not (Test-ChangeLogDiesesGeraet $j $gid)) { continue }
        foreach ($e in @($j.Eintraege)) { if ((Test-ChangeOffen $e) -and $script:TemporarySteps -contains [string]$e.Schritt) { $spec += ('{0}*{1}' -f $f.FullName, [int]$e.Id) } }
    }
    if (-not $spec.Count) { return $null }
    return (Invoke-ChangeUndo ($spec -join ';'))
}

# Alle Einträge aller Läufe, neueste zuerst (für die Oberfläche und Tests)
function Get-ChangeEntries([string]$Root = (Get-ChangeRoot)) {
    $list = New-Object System.Collections.Generic.List[object]
    if (-not $Root -or -not (Test-Path -LiteralPath $Root)) { return @() }
    foreach ($f in @(Get-ChildItem -LiteralPath $Root -Recurse -File -Filter '*.json' -ErrorAction SilentlyContinue)) {
        $j = Read-ChangeFile $f.FullName
        if (-not $j) { continue }
        foreach ($e in @($j.Eintraege)) {
            $list.Add([pscustomobject]@{ Datei = $f.FullName; Computer = [string]$j.Computer; GeraetId = [string]$j.GeraetId; Lauf = [string]$j.Lauf
                Id = [int]$e.Id; Zeit = [string]$e.Zeit; Modul = [string]$e.Modul; Titel = [string]$e.Titel; Risiko = [string]$e.Risiko; Art = [string]$e.Art
                Ziel = [string]$e.Ziel; Vorher = [string]$e.Vorher; Nachher = [string]$e.Nachher; Status = [string]$e.Status; Ergebnis = [string]$e.Ergebnis })
        }
    }
    return @($list | Sort-Object Zeit, Id -Descending)
}

# Rückgängig für eine Auswahl: "Datei*Id;Datei*Id". Nur auf dem Gerät, auf dem die Änderung gemacht wurde.
function Invoke-ChangeUndo([string]$Spec) {
    $byFile = [ordered]@{}
    foreach ($part in @(([string]$Spec) -split ';' | ForEach-Object { $_.Trim().Trim('"') } | Where-Object { $_ })) {
        $p = $part -split '\*', 2
        if ($p.Count -ne 2 -or -not ($p[1] -as [int])) { Write-Host ('Ungültige Auswahl: {0}' -f $part); continue }
        if (-not $byFile.Contains($p[0])) { $byFile[$p[0]] = New-Object System.Collections.Generic.List[int] }
        $byFile[$p[0]].Add([int]$p[1])
    }
    $gid = Get-ChangeGeraetId
    $done = 0; $fail = 0
    foreach ($file in $byFile.Keys) {
        $root = Get-ChangeRoot
        if (-not $root -or -not ([IO.Path]::GetFullPath($file)).StartsWith([IO.Path]::GetFullPath($root), [StringComparison]::OrdinalIgnoreCase)) { Write-Host ('{0} liegt nicht im Änderungsprotokoll des Datenordners.' -f $file); $fail++; continue }
        $j = Read-ChangeFile $file
        if (-not $j) { Write-Host ('{0} ist kein lesbares Änderungsprotokoll.' -f $file); $fail++; continue }
        $same = Test-ChangeLogDiesesGeraet $j $gid
        if (-not $same) { Write-Host ('{0}: Die Änderungen stammen von {1}, nicht von diesem Gerät. Rückgängig nur dort möglich.' -f (Split-Path $file -Leaf), $j.Computer); $fail++; continue }
        $filesDir = $file -replace '\.json$', ''
        # neueste zuerst, damit aufeinander aufbauende Änderungen in umgekehrter Reihenfolge zurückgenommen werden
        foreach ($e in @($j.Eintraege | Where-Object { $byFile[$file] -contains [int]$_.Id } | Sort-Object { [int]$_.Id } -Descending)) {
            if (-not (Test-ChangeOffen $e)) { Write-Host ('{0}: Status {1}, nichts zu tun.' -f $e.Titel, $e.Status); continue }
            try { $res = Undo-ChangeRecord $e $filesDir } catch { $res = [pscustomobject]@{ Status = 'fehlgeschlagen'; Text = $_.Exception.Message } }
            Set-ChangeUndoErgebnis $e $res
            Write-Host ('{0} ({1}): {2}, {3}' -f $e.Titel, $e.Ziel, $res.Status, $res.Text)
            if ($res.Status -eq 'rückgängig') { $done++ } else { $fail++ }
        }
        try { Save-ChangeFile $file $j } catch { Write-Host ('Protokoll {0} konnte nicht aktualisiert werden: {1}' -f $file, $_.Exception.Message); $fail++ }
    }
    return [pscustomobject]@{ Rueckgaengig = $done; Probleme = $fail }
}
# Aufräumen (ab v4.10, Seite Änderungen): Protokolldateien samt ihrem Ordner mit Sicherungsdateien nach
# Minibench-Daten\Archiv\Änderungen\<Zeit>\<PC> verschieben. Gelöscht wird nichts (Vorgabe 14).
#   Erledigt:<Tage>  Läufe ohne aktiven Eintrag, älter als <Tage> (0 = jedes Alter), von allen PCs
#   Andere           alle Protokolle anderer PCs, auch mit aktiven Einträgen (auf diesem PC nicht zurücknehmbar)
# Protokolle dieses PCs mit aktiven Einträgen bleiben immer: Sonst ginge Rückgängig verloren, und der Start könnte
# vorübergehende Änderungen (Restore-TemporaryChanges) nicht mehr zurücknehmen. Die Oberfläche rechnet dieselben Regeln
# für die Anzeige (ChangeLauf.Aufzuraeumen).
# ab v4.11: "dieser PC" heißt gleiche GeraetId (Name nur bei Protokollen ohne GeraetId, Test-ChangeLogDiesesGeraet), und
# "aktiv" schließt "fehlgeschlagen" älterer Versionen ein (Test-ChangeOffen): Beides gilt als noch zurücknehmbar.
function Move-ChangeArchiv([string]$Spec, [string]$Root = (Get-ChangeRoot), [string]$DataDir = $script:DataDir, [string]$Computer = $env:COMPUTERNAME, [datetime]$Jetzt = (Get-Date), [string]$GeraetId = (Get-ChangeGeraetId)) {
    $res = [pscustomobject]@{ Verschoben = 0; Eintraege = 0; Fehler = 0; Zeilen = (New-Object System.Collections.Generic.List[string]); Archiv = '' }
    $erledigt = $false; $tage = 0; $andere = $false
    foreach ($t in @(([string]$Spec) -split ';' | ForEach-Object { $_.Trim().Trim('"') } | Where-Object { $_ })) {
        if ($t -match '^Erledigt(:(\d+))?$') { $erledigt = $true; $tage = [int]$Matches[2] }
        elseif ($t -eq 'Andere') { $andere = $true }
        else { $res.Fehler++; $res.Zeilen.Add(('Unbekannte Angabe: {0}' -f $t)) }
    }
    if (-not $Root -or -not $DataDir -or -not (Test-Path -LiteralPath $Root) -or (-not $erledigt -and -not $andere)) { $res.Zeilen.Add('Nichts zu verschieben.'); return $res }
    $stempel = $Jetzt.ToString('yyyyMMdd_HHmmss')
    $arch = Join-Path (Join-Path (Join-Path $DataDir 'Archiv') 'Änderungen') $stempel
    $res.Archiv = $arch
    $einzeln = New-Object System.Collections.Generic.List[string]
    $ordner = @{}
    foreach ($f in @(Get-ChildItem -LiteralPath $Root -Recurse -File -Filter '*.json' -ErrorAction SilentlyContinue)) {
        if ($script:ChangeFile -and $f.FullName -eq $script:ChangeFile) { continue }
        $j = Read-ChangeFile $f.FullName
        if (-not $j) { continue }
        $ein = @($j.Eintraege | Where-Object { $_ })
        $aktiv = @($ein | Where-Object { Test-ChangeOffen $_ }).Count
        $dieser = Test-ChangeLogDiesesGeraet $j $GeraetId $Computer
        $zeit = $null; $zt = [string]$j.Lauf
        if (-not $zt -and $ein.Count) { $zt = [string]$ein[0].Zeit }
        $d = [datetime]::MinValue
        foreach ($fmt in 'yyyy-MM-dd HH:mm:ss', 'yyyy-MM-dd HH:mm') {
            if ([datetime]::TryParseExact($zt, $fmt, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::None, [ref]$d)) { $zeit = $d; break }
        }
        $weg = $false
        if ($andere -and -not $dieser) { $weg = $true }
        if ($erledigt -and $aktiv -eq 0 -and ($tage -le 0 -or ($zeit -and ($Jetzt - $zeit).TotalDays -gt $tage))) { $weg = $true }
        if (-not $weg -or ($dieser -and $aktiv -gt 0)) { continue }
        $pc = Split-Path (Split-Path $f.FullName -Parent) -Leaf
        $ziel = Join-Path $arch $pc
        try {
            New-Item -ItemType Directory -Path $ziel -Force -ErrorAction Stop | Out-Null
            $sich = $f.FullName -replace '\.json$', ''
            if (Test-Path -LiteralPath $sich -PathType Container) { Move-Item -LiteralPath $sich -Destination (Join-Path $ziel (Split-Path $sich -Leaf)) -ErrorAction Stop }
            Move-Item -LiteralPath $f.FullName -Destination (Join-Path $ziel $f.Name) -ErrorAction Stop
            $res.Verschoben++; $res.Eintraege += $ein.Count
            $einzeln.Add(('{0}\{1}: {2} Einträge, davon {3} aktiv' -f $pc, $f.Name, $ein.Count, $aktiv))
            $ordner[(Split-Path $f.FullName -Parent)] = $true
        } catch { $res.Fehler++; $res.Zeilen.Add(('{0}\{1}: {2}' -f $pc, $f.Name, $_.Exception.Message)) }
    }
    # leere Ordner der PCs entfernen (enthalten keine Daten mehr)
    foreach ($o in $ordner.Keys) {
        try { if (-not @(Get-ChildItem -LiteralPath $o -Force -ErrorAction Stop).Count) { Remove-Item -LiteralPath $o -Force -ErrorAction Stop } } catch { }
    }
    if ($res.Verschoben) {
        $res.Zeilen.Insert(0, ('{0} Protokolle mit {1} Einträgen nach Archiv\Änderungen\{2} verschoben. Gelöscht wurde nichts.' -f $res.Verschoben, $res.Eintraege, $stempel))
        try {
            $log = New-Object System.Text.StringBuilder
            [void]$log.AppendLine(('{0:yyyy-MM-dd HH:mm} Änderungsprotokoll aufgeräumt v{1} ({2}), Datenordner {3}' -f $Jetzt, $ScriptVersion, $Spec, $DataDir))
            foreach ($z in $einzeln) { [void]$log.AppendLine('  ' + $z) }
            [IO.File]::AppendAllText((Join-Path (Join-Path $DataDir 'Archiv') 'Datenpflege.log'), $log.ToString(), (New-Object Text.UTF8Encoding($true)))
        } catch { }
    } elseif (-not $res.Fehler) { $res.Zeilen.Add('Nichts zu verschieben.') }
    return $res
}

# Hilfsmodus der Oberfläche: -AenderungenAufraeumen "Erledigt:30;Andere", am Ende @@RESULT|verschoben|Fehler
if ($AenderungenAufraeumen) {
    $r = Move-ChangeArchiv ([string]$AenderungenAufraeumen)
    foreach ($z in $r.Zeilen) { Write-Host $z }
    Send-GuiEvent 'RESULT' $r.Verschoben $r.Fehler
    exit $(if ($r.Fehler) { 1 } else { 0 })
}
#endregion
