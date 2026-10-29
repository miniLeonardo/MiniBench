#region ---------- System-Umzug & Neuinstallation (ab v3.8, Hotfix v3.81) ----------
# Modul für die transparente Sicherung persönlicher Daten, Bibliotheken, Desktop-Layout,
# Profil-Konfigurationen und Programme vor einer Windows-Neuinstallation inklusive
# autarkem 1-Klick-Restore.
# Alle Operationen lesen strikt von den Quellpfaden; an der Quelle wird nichts verändert.

$script:UmzugSystemBlacklist = @(
    'windows', 'program files', 'program files (x86)', 'programdata',
    'system volume information', '$recycle.bin', '$winreagent', '$sysreset',
    '$windows.~bt', '$windows.~ws', 'recovery', 'perflogs', 'users', 'msocache',
    'boot', 'efi', 'documents and settings', 'inetpub', 'config.msi', 'deliveryoptimization',
    'amd', 'intel', 'nvidia', 'dell', 'hp', 'lenovo', 'asus', 'acer', 'realtek', 'drivers', 'treiber',
    'swsetup', 'drvdwn', 'sources', 'support', 'temp', 'tmp', 'cache', 'caches',
    'minidump', 'crashdumps', 'found.000', 'found.001', 'wsl', 'vhds'
)

function Get-FolderSize([string]$Path) {
    if (-not $Path -or -not (Test-Path -LiteralPath $Path)) { return 0L }
    $size = 0L
    try {
        $col = [System.IO.Directory]::EnumerateFiles($Path, '*', [System.IO.SearchOption]::AllDirectories)
        foreach ($f in $col) {
            try { $size += (New-Object System.IO.FileInfo($f)).Length } catch { }
        }
    } catch { }
    return $size
}

function Get-UmzugCustomFolders {
    $results = @()
    try {
        $rootDirs = Get-ChildItem -LiteralPath 'C:\' -Directory -Force -ErrorAction SilentlyContinue
        foreach ($d in $rootDirs) {
            $name = $d.Name.Trim().ToLowerInvariant()
            if ($name.StartsWith('.') -or $name.StartsWith('$')) { continue }
            if ($script:UmzugSystemBlacklist -contains $name) { continue }
            if ($d.Attributes -band [System.IO.FileAttributes]::System) { continue }
            if ($d.Attributes -band [System.IO.FileAttributes]::Hidden) { continue }
            $results += $d.FullName
        }
    } catch { }
    return @($results)
}

function Copy-UmzugTree([string]$Source, [string]$Destination, [string[]]$ExcludeDirs = @(), [int]$Pct = 0, [string]$StepTitle = '', [string[]]$ExcludeFiles = @()) {
    if (-not (Test-Path -LiteralPath $Source)) { return $false }
    try { [System.IO.Directory]::CreateDirectory($Destination) | Out-Null } catch { }
    $xdList = @('Temp', 'Cache', 'Code Cache', 'GPUCache', 'Service Worker', 'CrashDumps', 'node_modules', 'backup', 'workspaceStorage') + $ExcludeDirs
    $roboArgs = @(
        ('"{0}"' -f $Source.TrimEnd('\')),
        ('"{0}"' -f $Destination.TrimEnd('\')),
        '*.*',
        '/E',
        '/XJ',
        '/R:1',
        '/W:1',
        '/COPY:DAT',
        '/MT:16',
        '/NP',
        '/NDL',
        '/NJH',
        '/NJS',
        '/NC',
        '/NS',
        '/TS'
    )
    if ($xdList.Count -gt 0) {
        $roboArgs += '/XD'
        foreach ($xd in $xdList) { $roboArgs += ('"{0}"' -f $xd) }
    }
    if ($ExcludeFiles -and $ExcludeFiles.Count -gt 0) {
        $roboArgs += '/XF'
        foreach ($xf in $ExcludeFiles) { $roboArgs += ('"{0}"' -f $xf) }
    }
    $pinfo = New-Object System.Diagnostics.ProcessStartInfo
    $pinfo.FileName = 'robocopy.exe'
    $pinfo.Arguments = $roboArgs -join ' '
    $pinfo.UseShellExecute = $false
    $pinfo.CreateNoWindow = $true
    $pinfo.RedirectStandardOutput = $true

    try {
        $p = [System.Diagnostics.Process]::Start($pinfo)
        $lastEvent = [DateTime]::UtcNow
        while (-not $p.StandardOutput.EndOfStream) {
            $line = $p.StandardOutput.ReadLine()
            if ($line -and $line.Trim()) {
                $now = [DateTime]::UtcNow
                if (($now - $lastEvent).TotalMilliseconds -ge 150) {
                    $lastEvent = $now
                    $trimmed = $line.Trim()
                    if ($trimmed -match '\s+([^\t\r\n]+)$') {
                        $fName = $Matches[1]
                    } else {
                        $fName = $trimmed
                    }
                    if ($fName.Length -gt 65) {
                        $fName = '...' + $fName.Substring($fName.Length - 62)
                    }
                    if ($Pct -gt 0 -and $StepTitle) {
                        Send-GuiEvent 'UMZUG' ([string]$Pct) $StepTitle $fName
                    }
                }
            }
        }
        $p.WaitForExit()
        return ($p.ExitCode -lt 8)
    } catch {
        return $false
    }
}

function Test-UmzugTreeDiff([string]$Source, [string]$Destination, [string[]]$ExcludeDirs = @(), [string[]]$ExcludeFiles = @()) {
    if (-not (Test-Path -LiteralPath $Source) -or -not (Test-Path -LiteralPath $Destination)) {
        return @('Quelle oder Ziel existiert nicht')
    }
    $xdList = @('Temp', 'Cache', 'Code Cache', 'GPUCache', 'Service Worker', 'CrashDumps', 'node_modules', 'backup', 'workspaceStorage') + $ExcludeDirs
    $roboArgs = @(
        ('"{0}"' -f $Source.TrimEnd('\')),
        ('"{0}"' -f $Destination.TrimEnd('\')),
        '*.*',
        '/E',
        '/XJ',
        '/L',
        '/NJH',
        '/NJS',
        '/NDL',
        '/NC',
        '/NS',
        '/NP'
    )
    if ($xdList.Count -gt 0) {
        $roboArgs += '/XD'
        foreach ($xd in $xdList) { $roboArgs += ('"{0}"' -f $xd) }
    }
    if ($ExcludeFiles -and $ExcludeFiles.Count -gt 0) {
        $roboArgs += '/XF'
        foreach ($xf in $ExcludeFiles) { $roboArgs += ('"{0}"' -f $xf) }
    }
    $pinfo = New-Object System.Diagnostics.ProcessStartInfo
    $pinfo.FileName = 'robocopy.exe'
    $pinfo.Arguments = $roboArgs -join ' '
    $pinfo.UseShellExecute = $false
    $pinfo.CreateNoWindow = $true
    $pinfo.RedirectStandardOutput = $true
    $diffs = @()
    try {
        $p = [System.Diagnostics.Process]::Start($pinfo)
        while (-not $p.StandardOutput.EndOfStream) {
            $l = $p.StandardOutput.ReadLine()
            if ($l -and $l.Trim()) {
                $diffs += $l.Trim()
            }
        }
        $p.WaitForExit()
    } catch { }
    return @($diffs)
}

function Test-UmzugSha256Samples([string]$Source, [string]$Destination, [int]$SampleCount = 20) {
    if (-not (Test-Path -LiteralPath $Source) -or -not (Test-Path -LiteralPath $Destination)) {
        return @()
    }
    $dstFiles = @(Get-ChildItem -LiteralPath $Destination -File -Recurse -Force -ErrorAction SilentlyContinue)
    if ($dstFiles.Count -eq 0) { return @() }

    $chosen = @()
    if ($dstFiles.Count -le $SampleCount) {
        $chosen = $dstFiles
    } else {
        $sorted = @($dstFiles | Sort-Object Length)
        $small = @($sorted | Select-Object -First 5)
        $large = @($sorted | Select-Object -Last 5)
        $midPool = @($sorted[5..($sorted.Count - 6)])
        $step = [math]::Max(1, [math]::Floor($midPool.Count / 10))
        $mid = @(for ($i = 0; $i -lt $midPool.Count -and $chosen.Count -lt 10; $i += $step) { $midPool[$i] })
        $chosen = @($small + $mid + $large) | Select-Object -Unique -First $SampleCount
    }

    $sha = [System.Security.Cryptography.SHA256]::Create()
    function Get-FileSha([string]$fPath) {
        try {
            $fs = [System.IO.File]::OpenRead($fPath)
            try {
                $b = $sha.ComputeHash($fs)
                return (($b | ForEach-Object { $_.ToString('x2') }) -join '')
            } finally { $fs.Dispose() }
        } catch { return '' }
    }

    $results = @()
    $dstBaseLen = $Destination.TrimEnd('\').Length
    foreach ($df in $chosen) {
        $rel = $df.FullName.Substring($dstBaseLen).TrimStart('\')
        $sf = Join-Path $Source $rel
        $srcHash = Get-FileSha $sf
        $dstHash = Get-FileSha $df.FullName
        $ok = ($srcHash -and $dstHash -and ($srcHash -eq $dstHash))
        $results += [pscustomobject]@{
            Datei     = $rel
            QuellHash = $srcHash
            ZielHash  = $dstHash
            Ok        = $ok
        }
    }
    return @($results)
}

function Invoke-SystemBackup {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Ziel,
        [string]$Umfang = 'Bibliotheken,Layout,Programme,Profile',
        [string]$Zusatz = '',
        [switch]$NonInteractive
    )

    $uList = @($Umfang -split '[,;]+' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $zList = @($Zusatz -split '[,;]+' | ForEach-Object { $_.Trim() } | Where-Object { $_ })

    # Zielpfad auflösen
    $targetBase = $Ziel
    if ($targetBase -eq 'NAS' -or -not $targetBase) {
        if (Get-Command Get-NasKonfig -ErrorAction SilentlyContinue) {
            $nasCfg = Get-NasKonfig
            if ($nasCfg -and $nasCfg.NasPfad) { $targetBase = $nasCfg.NasPfad }
        }
    }
    if (-not $targetBase) {
        Send-GuiEvent 'LOG' 'Rot' 'Fehler: Kein gültiges Zielmedium für den Umzug angegeben.'
        return [pscustomobject]@{ Ok = $false; Pfad = ''; Groesse = 0L; Fehler = 'Kein Zielmedium' }
    }

    try {
        if (-not (Test-Path -LiteralPath $targetBase)) {
            [System.IO.Directory]::CreateDirectory($targetBase) | Out-Null
        }
    } catch {
        Send-GuiEvent 'LOG' 'Rot' ('Zielpfad konnte nicht erstellt werden: {0}' -f $_.Exception.Message)
        return [pscustomobject]@{ Ok = $false; Pfad = ''; Groesse = 0L; Fehler = $_.Exception.Message }
    }

    $safePc = [regex]::Replace($env:COMPUTERNAME, '[^\w\-]', '_')
    $stamp = (Get-Date).ToString('yyyyMMdd_HHmm')
    $backupFolder = Join-Path $targetBase ('Minibench-Umzug_{0}_{1}' -f $safePc, $stamp)

    try {
        [System.IO.Directory]::CreateDirectory($backupFolder) | Out-Null
    } catch {
        Send-GuiEvent 'LOG' 'Rot' ('Sicherungsordner konnte nicht erstellt werden: {0}' -f $_.Exception.Message)
        return [pscustomobject]@{ Ok = $false; Pfad = ''; Groesse = 0L; Fehler = $_.Exception.Message }
    }

    # Dateisystem- und Kapazitätsprüfung
    $root = [System.IO.Path]::GetPathRoot($backupFolder)
    $fsType = ''
    $freeBytes = 0L
    try {
        $di = New-Object System.IO.DriveInfo($root)
        $fsType = [string]$di.DriveFormat
        $freeBytes = [int64]$di.AvailableFreeSpace
    } catch { }

    if ($fsType -match 'FAT32') {
        $fatMsg = 'WARNUNG: Zielmedium ist als FAT32 formatiert (Einzeldateien >= 4 GB können nicht gespeichert werden; NTFS oder exFAT empfohlen).'
        Send-GuiEvent 'LOG' 'Gelb' $fatMsg
        Write-Warning $fatMsg
    }

    Send-GuiEvent 'UMZUG' '5' 'Sicherungsvorbereitung ...' ''
    Send-GuiEvent 'LOG' '' ('Zielordner initialisiert: {0}' -f $backupFolder)

    # 1. Benutzerdaten & Bibliotheken
    $hasLibScope = ($uList -contains 'Bibliotheken' -or ($uList | Where-Object { $_ -like 'Lib_*' }))
    if ($hasLibScope) {
        Send-GuiEvent 'UMZUG' '10' 'Sichere Benutzer-Bibliotheken ...' ''
        Send-GuiEvent 'LOG' '' 'Sichere Standardbibliotheken des aktuellen Benutzers ...'
        $profDest = Join-Path $backupFolder 'Benutzerprofil'
        [System.IO.Directory]::CreateDirectory($profDest) | Out-Null

        $regPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders'
        $reg = Get-ItemProperty -Path $regPath -ErrorAction SilentlyContinue

        $desktopPath  = if ($reg -and $reg.Desktop) { [Environment]::ExpandEnvironmentVariables($reg.Desktop) } else { [Environment]::GetFolderPath('Desktop') }
        $docPath      = if ($reg -and $reg.Personal) { [Environment]::ExpandEnvironmentVariables($reg.Personal) } else { [Environment]::GetFolderPath('MyDocuments') }
        $downPath     = if ($reg -and $reg.'{374DE290-123F-4565-9164-39C4925E467B}') { [Environment]::ExpandEnvironmentVariables($reg.'{374DE290-123F-4565-9164-39C4925E467B}') } else { Join-Path $env:USERPROFILE 'Downloads' }
        $picPath      = if ($reg -and $reg.'My Pictures') { [Environment]::ExpandEnvironmentVariables($reg.'My Pictures') } else { [Environment]::GetFolderPath('MyPictures') }
        $musicPath    = if ($reg -and $reg.'My Music') { [Environment]::ExpandEnvironmentVariables($reg.'My Music') } else { [Environment]::GetFolderPath('MyMusic') }
        $videoPath    = if ($reg -and $reg.'My Video') { [Environment]::ExpandEnvironmentVariables($reg.'My Video') } else { [Environment]::GetFolderPath('MyVideos') }

        $libs = @(
            @{ Name = 'Desktop';   Path = $desktopPath; Step = 12; Key = 'Lib_Desktop' },
            @{ Name = 'Dokumente'; Path = $docPath;     Step = 16; Key = 'Lib_Dokumente' },
            @{ Name = 'Downloads'; Path = $downPath;    Step = 20; Key = 'Lib_Downloads' },
            @{ Name = 'Bilder';    Path = $picPath;     Step = 24; Key = 'Lib_Bilder' },
            @{ Name = 'Musik';     Path = $musicPath;   Step = 28; Key = 'Lib_Musik' },
            @{ Name = 'Videos';    Path = $videoPath;   Step = 32; Key = 'Lib_Videos' }
        )

        $hasGranularLib = [bool]($uList | Where-Object { $_ -like 'Lib_*' })
        $diffReports = @()
        $shaReports = @()

        foreach ($lib in $libs) {
            if ($hasGranularLib -and -not ($uList -contains $lib.Key)) {
                continue
            }
            if ($lib.Path -and (Test-Path -LiteralPath $lib.Path)) {
                $libSize = Get-FolderSize $lib.Path
                $sizeTxt = if ($libSize -ge 1GB) { '{0:N1} GB' -f ($libSize / 1GB) } else { '{0:N0} MB' -f ($libSize / 1MB) }
                $stepMsg = ('Kopiere Bibliothek {0} ({1}) ...' -f $lib.Name, $sizeTxt)
                Send-GuiEvent 'UMZUG' ([string]$lib.Step) $stepMsg ''
                Send-GuiEvent 'LOG' '' ('Spiegele {0} ({1}) -> Benutzerprofil\{2}' -f $lib.Path, $sizeTxt, $lib.Name)
                $destLib = Join-Path $profDest $lib.Name
                Copy-UmzugTree $lib.Path $destLib @() $lib.Step $stepMsg | Out-Null
                $diffs = @(Test-UmzugTreeDiff $lib.Path $destLib)
                $diffReports += [pscustomobject]@{ Name = $lib.Name; Diffs = $diffs; Count = $diffs.Count }
                $samples = @(Test-UmzugSha256Samples $lib.Path $destLib 20)
                $shaReports += [pscustomobject]@{ Name = $lib.Name; Samples = $samples }
            }
        }
    }

    # 2. Profile & Konfigurationen (Configs: Browser, E-Mail, Editoren, Tools)
    $hasProfileScope = ($uList -contains 'Profile' -or $uList -contains 'Browser' -or ($uList | Where-Object { $_ -like 'Profile_*' }))
    if ($hasProfileScope) {
        $doBrowser = ($uList -contains 'Browser' -or $uList -contains 'Profile_Browser' -or ($uList -contains 'Profile' -and -not ($uList | Where-Object { $_ -like 'Profile_*' })))
        $doMail    = ($uList -contains 'Mail' -or $uList -contains 'Profile_Mail' -or ($uList -contains 'Profile' -and -not ($uList | Where-Object { $_ -like 'Profile_*' })))
        $doDev     = ($uList -contains 'DevConfigs' -or $uList -contains 'Profile_Dev' -or ($uList -contains 'Profile' -and -not ($uList | Where-Object { $_ -like 'Profile_*' })))
        $doTools   = ($uList -contains 'ToolConfigs' -or $uList -contains 'Profile_Tools' -or ($uList -contains 'Profile' -and -not ($uList | Where-Object { $_ -like 'Profile_*' })))
        $doTerm    = ($uList -contains 'Terminal' -or $uList -contains 'Profile_Terminal' -or ($uList -contains 'Profile' -and -not ($uList | Where-Object { $_ -like 'Profile_*' })))

        Send-GuiEvent 'UMZUG' '36' 'Sichere Profile & Konfigurationen (Configs) ...' ''
        Send-GuiEvent 'LOG' '' 'Sichere Anwendungs- und Programm-Konfigurationen ...'
        $profDest = Join-Path $backupFolder 'Benutzerprofil'
        [System.IO.Directory]::CreateDirectory($profDest) | Out-Null

        # A. Webbrowser (Lesezeichen & Einstellungen; Passwörter und Tokens werden prinzipbedingt ausgeschlossen)
        if ($doBrowser) {
            $browserExFiles = @('Login Data*', 'Cookies*', 'Web Data*', 'History*', 'formhistory.sqlite', 'logins.json', 'key4.db', 'cert9.db', 'lock', 'parent.lock')
            $browsers = @(
                @{ Name = 'Edge';    Base = $env:LOCALAPPDATA; Sub = 'Microsoft\Edge\User Data';             Dst = 'AppData\Local\Microsoft\Edge\User Data' },
                @{ Name = 'Chrome';  Base = $env:LOCALAPPDATA; Sub = 'Google\Chrome\User Data';               Dst = 'AppData\Local\Google\Chrome\User Data' },
                @{ Name = 'Brave';   Base = $env:LOCALAPPDATA; Sub = 'BraveSoftware\Brave-Browser\User Data';   Dst = 'AppData\Local\BraveSoftware\Brave-Browser\User Data' },
                @{ Name = 'Vivaldi'; Base = $env:LOCALAPPDATA; Sub = 'Vivaldi\User Data';                    Dst = 'AppData\Local\Vivaldi\User Data' },
                @{ Name = 'Firefox'; Base = $env:APPDATA;      Sub = 'Mozilla\Firefox';                      Dst = 'AppData\Roaming\Mozilla\Firefox' },
                @{ Name = 'Opera';   Base = $env:APPDATA;      Sub = 'Opera Software\Opera Stable';          Dst = 'AppData\Roaming\Opera Software\Opera Stable' },
                @{ Name = 'OperaGX'; Base = $env:APPDATA;      Sub = 'Opera Software\Opera GX Stable';       Dst = 'AppData\Roaming\Opera Software\Opera GX Stable' }
            )
            foreach ($b in $browsers) {
                if ($b.Base) {
                    $bSrc = Join-Path $b.Base $b.Sub
                    if (Test-Path -LiteralPath $bSrc) {
                        $stepMsg = ('Sichere Browser-Profil: {0} (ohne Passwörter) ...' -f $b.Name)
                        Send-GuiEvent 'UMZUG' '40' $stepMsg ''
                        Send-GuiEvent 'LOG' '' ('Spiegele {0} -> {1}' -f $bSrc, $b.Dst)
                        Copy-UmzugTree $bSrc (Join-Path $profDest $b.Dst) @() 40 $stepMsg $browserExFiles | Out-Null
                    }
                }
            }
        }

        # B. E-Mail (Thunderbird)
        if ($doMail -and $env:APPDATA) {
            $tbSrc = Join-Path $env:APPDATA 'Thunderbird'
            if (Test-Path -LiteralPath $tbSrc) {
                $stepMsg = 'Sichere E-Mail-Profil: Thunderbird ...'
                Send-GuiEvent 'UMZUG' '44' $stepMsg ''
                Send-GuiEvent 'LOG' '' ('Spiegele {0} -> AppData\Roaming\Thunderbird' -f $tbSrc)
                Copy-UmzugTree $tbSrc (Join-Path $profDest 'AppData\Roaming\Thunderbird') @() 44 $stepMsg | Out-Null
            }
        }

        # C. Entwickler- und Editor-Konfigurationen
        if ($doDev) {
            # VS Code
            if ($env:APPDATA) {
                $codeUser = Join-Path $env:APPDATA 'Code\User'
                if (Test-Path -LiteralPath $codeUser) {
                    $stepMsg = 'Sichere Editor-Konfiguration: VS Code ...'
                    Send-GuiEvent 'UMZUG' '47' $stepMsg ''
                    Copy-UmzugTree $codeUser (Join-Path $profDest 'AppData\Roaming\Code\User') @('workspaceStorage', 'history', 'globalStorage') 47 $stepMsg | Out-Null
                }
                $cursorUser = Join-Path $env:APPDATA 'Cursor\User'
                if (Test-Path -LiteralPath $cursorUser) {
                    $stepMsg = 'Sichere Editor-Konfiguration: Cursor ...'
                    Send-GuiEvent 'UMZUG' '48' $stepMsg ''
                    Copy-UmzugTree $cursorUser (Join-Path $profDest 'AppData\Roaming\Cursor\User') @('workspaceStorage', 'history', 'globalStorage') 48 $stepMsg | Out-Null
                }
                $npp = Join-Path $env:APPDATA 'Notepad++'
                if (Test-Path -LiteralPath $npp) {
                    $stepMsg = 'Sichere Editor-Konfiguration: Notepad++ ...'
                    Send-GuiEvent 'UMZUG' '49' $stepMsg ''
                    Copy-UmzugTree $npp (Join-Path $profDest 'AppData\Roaming\Notepad++') @('backup') 49 $stepMsg | Out-Null
                }
            }
            # Git-Konfiguration
            $gitCfg = Join-Path $env:USERPROFILE '.gitconfig'
            if (Test-Path -LiteralPath $gitCfg) {
                try { Copy-Item -LiteralPath $gitCfg -Destination (Join-Path $profDest '.gitconfig') -Force } catch { }
            }
            $gitIgnore = Join-Path $env:USERPROFILE '.gitignore_global'
            if (Test-Path -LiteralPath $gitIgnore) {
                try { Copy-Item -LiteralPath $gitIgnore -Destination (Join-Path $profDest '.gitignore_global') -Force } catch { }
            }
            # PowerShell-Profile
            $psProfDest = Join-Path $profDest 'PowerShellProfiles'
            $psDirs = @(
                (Join-Path $env:USERPROFILE 'Documents\WindowsPowerShell'),
                (Join-Path $env:USERPROFILE 'Documents\PowerShell')
            )
            foreach ($pd in $psDirs) {
                if (Test-Path -LiteralPath $pd) {
                    $profiles = @(Get-ChildItem -LiteralPath $pd -Filter '*profile*.ps1' -File -ErrorAction SilentlyContinue)
                    if ($profiles.Count -gt 0) {
                        try { [System.IO.Directory]::CreateDirectory($psProfDest) | Out-Null } catch { }
                        foreach ($pr in $profiles) {
                            try { Copy-Item -LiteralPath $pr.FullName -Destination (Join-Path $psProfDest $pr.Name) -Force } catch { }
                        }
                    }
                }
            }
        }

        # D. Werkzeuge & Remote (Einstellungen & Session-Profile; Passwörter werden prinzipbedingt ausgeschlossen)
        if ($doTools) {
            if ($env:APPDATA) {
                $fz = Join-Path $env:APPDATA 'FileZilla'
                if (Test-Path -LiteralPath $fz) {
                    $stepMsg = 'Sichere Tool-Konfiguration: FileZilla (ohne gespeicherte Passwörter) ...'
                    Send-GuiEvent 'UMZUG' '51' $stepMsg ''
                    # Vertrauliche Zugangsdaten ausschließen
                    Copy-UmzugTree $fz (Join-Path $profDest 'AppData\Roaming\FileZilla') @() 51 $stepMsg @('sitemanager.xml', 'recentservers.xml') | Out-Null
                }
                $wscp = Join-Path $env:APPDATA 'WinSCP.ini'
                if (Test-Path -LiteralPath $wscp) {
                    try { Copy-Item -LiteralPath $wscp -Destination (Join-Path $profDest 'AppData\Roaming\WinSCP.ini') -Force } catch { }
                }
                $ghisl = Join-Path $env:APPDATA 'GHISLER'
                if (Test-Path -LiteralPath $ghisl) {
                    $stepMsg = 'Sichere Tool-Konfiguration: Total Commander ...'
                    Send-GuiEvent 'UMZUG' '52' $stepMsg ''
                    Copy-UmzugTree $ghisl (Join-Path $profDest 'AppData\Roaming\GHISLER') @() 52 $stepMsg | Out-Null
                }
            }
            # PuTTY-Verbindungsprofile als strukturierte JSON-Konfiguration (ohne reg.exe export Aufruf und ohne Passwörter)
            try {
                $puttyKey = 'HKCU:\Software\SimonTatham\PuTTY\Sessions'
                if (Test-Path -LiteralPath $puttyKey) {
                    $sessions = @(Get-ChildItem -LiteralPath $puttyKey -ErrorAction SilentlyContinue)
                    if ($sessions.Count -gt 0) {
                        $pList = @()
                        foreach ($s in $sessions) {
                            $props = Get-ItemProperty -LiteralPath $s.PSPath -ErrorAction SilentlyContinue
                            $cleanProps = @{}
                            foreach ($p in $props.PSObject.Properties) {
                                if ($p.Name -notmatch '^PS' -and $p.Name -notmatch 'Password') {
                                    $cleanProps[$p.Name] = $p.Value
                                }
                            }
                            $pList += [pscustomobject]@{ Session = $s.PSChildName; Einstellungen = $cleanProps }
                        }
                        ($pList | ConvertTo-Json -Depth 4) | Out-File -LiteralPath (Join-Path $profDest 'PuTTY_Sessions.json') -Encoding UTF8
                    }
                }
            } catch { }
        }

        # E. Windows Terminal
        if ($doTerm -and $env:LOCALAPPDATA) {
            $wtPkg = Get-ChildItem -LiteralPath (Join-Path $env:LOCALAPPDATA 'Packages') -Filter 'Microsoft.WindowsTerminal_*' -Directory -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($wtPkg) {
                $wtSettings = Join-Path $wtPkg.FullName 'LocalState\settings.json'
                if (Test-Path -LiteralPath $wtSettings) {
                    $wtDest = Join-Path $profDest 'WindowsTerminal'
                    try {
                        [System.IO.Directory]::CreateDirectory($wtDest) | Out-Null
                        Copy-Item -LiteralPath $wtSettings -Destination (Join-Path $wtDest 'settings.json') -Force
                    } catch { }
                }
            }
        }
    }

    # 3. Benutzerdefinierte Ordner auf C:\
    if ($zList.Count -gt 0) {
        $zusatzDest = Join-Path $backupFolder 'C_Laufwerk_Zusatz'
        [System.IO.Directory]::CreateDirectory($zusatzDest) | Out-Null
        foreach ($z in $zList) {
            if (Test-Path -LiteralPath $z) {
                $leaf = Split-Path $z -Leaf
                $zSize = Get-FolderSize $z
                $sizeTxt = if ($zSize -ge 1GB) { '{0:N1} GB' -f ($zSize / 1GB) } else { '{0:N0} MB' -f ($zSize / 1MB) }
                $stepMsg = ('Sichere Zusatzordner {0} ({1}) ...' -f $leaf, $sizeTxt)
                Send-GuiEvent 'UMZUG' '56' $stepMsg ''
                Send-GuiEvent 'LOG' '' ('Spiegele {0} ({1}) -> C_Laufwerk_Zusatz\{2}' -f $z, $sizeTxt, $leaf)
                Copy-UmzugTree $z (Join-Path $zusatzDest $leaf) @() 56 $stepMsg | Out-Null
            }
        }
    }

    # 4. Desktop-Zustand 1:1
    if ($uList -contains 'Layout') {
        Send-GuiEvent 'UMZUG' '68' 'Sichere Desktop-Layout und Hintergrundbild ...' ''
        Send-GuiEvent 'LOG' '' 'Sichere Desktop-Optik, Wallpaper-Datei und Icon-Positionen ...'
        $layoutDest = Join-Path $backupFolder 'Desktop-Layout'
        [System.IO.Directory]::CreateDirectory($layoutDest) | Out-Null

        # Hintergrundbild
        if ($env:APPDATA) {
            $transcoded = Join-Path $env:APPDATA 'Microsoft\Windows\Themes\TranscodedWallpaper'
            if (Test-Path -LiteralPath $transcoded) {
                try { Copy-Item -LiteralPath $transcoded -Destination (Join-Path $layoutDest 'Wallpaper.jpg') -Force } catch { }
            }
        }
        # Layout.json
        $wpStyle = ''
        $wpTile = ''
        try {
            $wpProp = Get-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -ErrorAction SilentlyContinue
            if ($wpProp) {
                $wpStyle = [string]$wpProp.WallpaperStyle
                $wpTile = [string]$wpProp.TileWallpaper
            }
        } catch { }
        $layoutObj = [ordered]@{
            WallpaperStyle = $wpStyle
            TileWallpaper  = $wpTile
            ComputerName   = $env:COMPUTERNAME
            Datum          = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
        }
        ($layoutObj | ConvertTo-Json) | Out-File -LiteralPath (Join-Path $layoutDest 'Layout.json') -Encoding UTF8

        # IconPositionen.reg
        $regFile = Join-Path $layoutDest 'IconPositionen.reg'
        try {
            $pinfo = New-Object System.Diagnostics.ProcessStartInfo
            $pinfo.FileName = 'reg.exe'
            $pinfo.Arguments = ('export "HKCU\Software\Microsoft\Windows\Shell\Bags\1\Desktop" "{0}" /y' -f $regFile)
            $pinfo.UseShellExecute = $false; $pinfo.CreateNoWindow = $true
            $p = [System.Diagnostics.Process]::Start($pinfo); if (-not $p.WaitForExit(30000)) { try { $p.Kill() } catch { } }
        } catch { }

        # Icons.json
        $desktopFiles = @(Get-ChildItem -LiteralPath ([Environment]::GetFolderPath('Desktop')) -Force -ErrorAction SilentlyContinue | Select-Object Name, Length, LastWriteTime)
        $resStr = ''
        try {
            $vc = Get-CimInstance Win32_VideoController -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($vc -and $vc.CurrentHorizontalResolution) {
                $resStr = ('{0}x{1}' -f $vc.CurrentHorizontalResolution, $vc.CurrentVerticalResolution)
            }
        } catch { }
        $iconsObj = [ordered]@{
            ComputerName = $env:COMPUTERNAME
            Aufloesung   = $resStr
            Icons        = $desktopFiles
        }
        ($iconsObj | ConvertTo-Json -Depth 4) | Out-File -LiteralPath (Join-Path $layoutDest 'Icons.json') -Encoding UTF8
    }

    # 5. Software-Inventarisierung & winget
    if ($uList -contains 'Programme') {
        Send-GuiEvent 'UMZUG' '80' 'Inventarisiere Software und Winget-Pakete ...' ''
        Send-GuiEvent 'LOG' '' 'Erstelle Winget-Paketliste und Report nicht katalogisierter Software ...'
        $progDest = Join-Path $backupFolder 'Programme'
        [System.IO.Directory]::CreateDirectory($progDest) | Out-Null

        $wingetExe = if (Get-Command Find-Winget -ErrorAction SilentlyContinue) { Find-Winget } else {
            $cmd = Get-Command winget.exe -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($cmd) { $cmd.Source } else { '' }
        }
        $wingetExportFile = Join-Path $progDest 'winget-pakete.json'
        $wingetIds = @{}
        if ($wingetExe -and (Test-Path -LiteralPath $wingetExe)) {
            try {
                $pinfo = New-Object System.Diagnostics.ProcessStartInfo
                $pinfo.FileName = $wingetExe
                $pinfo.Arguments = ('export -o "{0}" --accept-source-agreements --disable-interactivity' -f $wingetExportFile)
                $pinfo.UseShellExecute = $false; $pinfo.CreateNoWindow = $true
                $p = [System.Diagnostics.Process]::Start($pinfo); if (-not $p.WaitForExit(30000)) { try { $p.Kill() } catch { } }
            } catch { }
        }
        if (Test-Path -LiteralPath $wingetExportFile) {
            try {
                $wgRaw = Get-Content -LiteralPath $wingetExportFile -Raw -Encoding UTF8
                $wgJson = $wgRaw | ConvertFrom-Json
                if ($wgJson.Sources) {
                    foreach ($src in $wgJson.Sources) {
                        foreach ($pkg in $src.Packages) {
                            if ($pkg.PackageIdentifier) { $wingetIds[$pkg.PackageIdentifier.ToLowerInvariant()] = $true }
                        }
                    }
                }
            } catch { }
        }

        # Registry Uninstall
        $regKeys = @(
            'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
            'HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
            'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
        )
        $installedList = New-Object System.Collections.Generic.List[psobject]
        $seen = @{}
        foreach ($rk in $regKeys) {
            foreach ($item in (Get-ItemProperty -Path $rk -ErrorAction SilentlyContinue)) {
                $dName = [string]$item.DisplayName
                if (-not $dName -or $dName.Trim().Length -eq 0) { continue }
                if ($item.SystemComponent -eq 1 -or $item.ParentKeyName) { continue }
                if ($dName -match '^KB\d+' -or $dName -match 'Security Update' -or $dName -match 'Update for Windows') { continue }
                $kLow = $dName.Trim().ToLowerInvariant()
                if ($seen.ContainsKey($kLow)) { continue }
                $seen[$kLow] = $true

                $installedList.Add([pscustomobject]@{
                    Name       = $dName.Trim()
                    Hersteller = ([string]$item.Publisher).Trim()
                    Version    = ([string]$item.DisplayVersion).Trim()
                    Pfad       = ([string]$item.InstallLocation).Trim()
                })
            }
        }

        # Abgleich
        $nichtWinget = New-Object System.Collections.Generic.List[psobject]
        foreach ($app in $installedList) {
            $found = $false
            $nameLow = $app.Name.ToLowerInvariant()
            foreach ($wid in $wingetIds.Keys) {
                if ($wid.Contains($nameLow) -or $nameLow.Contains($wid)) { $found = $true; break }
            }
            if (-not $found) { $nichtWinget.Add($app) }
        }

        $reportFile = Join-Path $progDest 'Nicht-Winget-Programme.txt'
        $sb = New-Object System.Text.StringBuilder
        $sb.AppendLine("NICHT UEBER WINGET VERFUEGBARE PROGRAMME") | Out-Null
        $sb.AppendLine("=========================================") | Out-Null
        $sb.AppendLine(('Computer:      {0}' -f $env:COMPUTERNAME)) | Out-Null
        $sb.AppendLine(('Datum:         {0}' -f (Get-Date).ToString('dd.MM.yyyy HH:mm'))) | Out-Null
        $sb.AppendLine(('Gefunden:      {0} Programme' -f $nichtWinget.Count)) | Out-Null
        $sb.AppendLine() | Out-Null
        $sb.AppendLine("Die folgenden Programme wurden nicht im Winget-Katalog gefunden und muessen") | Out-Null
        $sb.AppendLine("nach der Windows-Neuinstallation bei Bedarf manuell installiert werden:") | Out-Null
        $sb.AppendLine() | Out-Null

        foreach ($nw in ($nichtWinget | Sort-Object Name)) {
            $sb.AppendLine(('Name:          {0}' -f $nw.Name)) | Out-Null
            if ($nw.Hersteller) { $sb.AppendLine(('Hersteller:    {0}' -f $nw.Hersteller)) | Out-Null }
            if ($nw.Version)    { $sb.AppendLine(('Version:       {0}' -f $nw.Version)) | Out-Null }
            if ($nw.Pfad)       { $sb.AppendLine(('Pfad:          {0}' -f $nw.Pfad)) | Out-Null }
            $q = [System.Uri]::EscapeDataString($nw.Name + $(if ($nw.Hersteller) { ' ' + $nw.Hersteller } else { '' }) + ' download')
            $sb.AppendLine(('Download-Suche: https://www.google.com/search?q={0}' -f $q)) | Out-Null
            $sb.AppendLine(('{0}' -f ('-' * 70))) | Out-Null
        }
        [System.IO.File]::WriteAllText($reportFile, $sb.ToString(), [System.Text.Encoding]::UTF8)
    }

    # 5b. WLAN-Netzwerkprofile (ab v3.86)
    if ($uList -contains 'Wlan') {
        if ($script:Anonymisieren -or $Anonymisieren) {
            Send-GuiEvent 'LOG' 'Gelb' 'WLAN-Profile werden aufgrund aktiver Anonymisierung nicht exportiert.'
        } else {
            Send-GuiEvent 'UMZUG' '88' 'Sichere WLAN-Netzwerkprofile ...' ''
            Send-GuiEvent 'LOG' '' 'Exportiere WLAN-Profile ...'
            $wlanDest = Join-Path $backupFolder 'WLAN-Profile'
            try {
                [System.IO.Directory]::CreateDirectory($wlanDest) | Out-Null
                $exportClear = ($uList -contains 'Wlan_Klartext')
                $clearArg = if ($exportClear) { ' key=clear' } else { '' }
                $pinfo = New-Object System.Diagnostics.ProcessStartInfo
                $pinfo.FileName = 'netsh.exe'
                $pinfo.Arguments = ('wlan export profile folder="{0}"{1}' -f $wlanDest, $clearArg)
                $pinfo.UseShellExecute = $false
                $pinfo.CreateNoWindow = $true
                $p = [System.Diagnostics.Process]::Start($pinfo)
                if (-not $p.WaitForExit(30000)) { try { $p.Kill() } catch { } }
                $exportedProfiles = @(Get-ChildItem -LiteralPath $wlanDest -Filter '*.xml' -File -ErrorAction SilentlyContinue)
                $secNote = if ($exportClear) { 'mit Klartext-Schluesseln' } else { 'ohne Schluessel (Standard)' }
                Send-GuiEvent 'LOG' '' ('WLAN-Profile exportiert: {0} Profile ({1})' -f $exportedProfiles.Count, $secNote)
            } catch {
                Send-GuiEvent 'LOG' 'Gelb' ('Fehler beim Export der WLAN-Profile: {0}' -f $_.Exception.Message)
            }
        }
    }

    # 5c. Verifikationsauswertung & Pruefprotokoll.txt (ab v3.86)
    Send-GuiEvent 'UMZUG' '92' 'Pruefe Sicherungsvollstaendigkeit (Diff & SHA-256) ...' ''
    $allDiffsOk = $true
    $allShaOk = $true
    $totalDiffs = 0
    $totalSamples = 0
    $failedSamples = 0

    $protoPath = Join-Path $backupFolder 'Pruefprotokoll.txt'
    $psb = New-Object System.Text.StringBuilder
    $psb.AppendLine('LEOS MINIBENCH: SICHERUNGS-PRUEFPROTOKOLL') | Out-Null
    $psb.AppendLine('=========================================') | Out-Null
    $psb.AppendLine(('Datum:            {0}' -f (Get-Date).ToString('dd.MM.yyyy HH:mm:ss'))) | Out-Null
    $psb.AppendLine(('Computer:         {0}' -f $env:COMPUTERNAME)) | Out-Null
    $psb.AppendLine(('Sicherungsordner: {0}' -f $backupFolder)) | Out-Null
    $psb.AppendLine() | Out-Null

    $psb.AppendLine('1. ROBOCOPY-DIFFERENZABGLEICH') | Out-Null
    $psb.AppendLine('----------------------------') | Out-Null
    if ($diffReports -and $diffReports.Count -gt 0) {
        foreach ($dr in $diffReports) {
            $psb.AppendLine(('Bibliothek: {0} - Abweichungen: {1}' -f $dr.Name, $dr.Count)) | Out-Null
            if ($dr.Count -gt 0) {
                $allDiffsOk = $false
                $totalDiffs += $dr.Count
                foreach ($d in $dr.Diffs) {
                    $psb.AppendLine(('  [ABWEICHUNG] {0}' -f $d)) | Out-Null
                }
            } else {
                $psb.AppendLine('  Keine Differenzen festgestellt (vollstaendig synchron).') | Out-Null
            }
        }
    } else {
        $psb.AppendLine('Keine Bibliotheken fuer Differenzabgleich ausgewaehlt.') | Out-Null
    }
    $psb.AppendLine() | Out-Null

    $psb.AppendLine('2. SHA-256-STICHPROBENPRUEFUNG (ca. 20 Dateien je Bibliothek)') | Out-Null
    $psb.AppendLine('-------------------------------------------------------------') | Out-Null
    if ($shaReports -and $shaReports.Count -gt 0) {
        foreach ($sr in $shaReports) {
            $psb.AppendLine(('Bibliothek: {0} ({1} Stichproben geprueft)' -f $sr.Name, $sr.Samples.Count)) | Out-Null
            foreach ($s in $sr.Samples) {
                $totalSamples++
                if (-not $s.Ok) {
                    $allShaOk = $false
                    $failedSamples++
                    $psb.AppendLine(('  [FEHLER] {0}' -f $s.Datei)) | Out-Null
                    $psb.AppendLine(('           Quelle: {0}' -f $s.QuellHash)) | Out-Null
                    $psb.AppendLine(('           Ziel:   {0}' -f $s.ZielHash)) | Out-Null
                } else {
                    $subHash = if ($s.ZielHash.Length -ge 16) { $s.ZielHash.Substring(0, 16) } else { $s.ZielHash }
                    $psb.AppendLine(('  [OK]     {0} (SHA: {1}...)' -f $s.Datei, $subHash)) | Out-Null
                }
            }
        }
    } else {
        $psb.AppendLine('Keine Dateien fuer SHA-256-Stichproben ausgewaehlt.') | Out-Null
    }
    $psb.AppendLine() | Out-Null

    $sicherungVollstaendig = ($allDiffsOk -and $allShaOk)
    $psb.AppendLine('GESAMTERGEBNIS') | Out-Null
    $psb.AppendLine('==============') | Out-Null
    $psb.AppendLine(('Sicherung vollstaendig: {0}' -f $(if ($sicherungVollstaendig) { 'Ja' } else { 'Nein' }))) | Out-Null
    if (-not $sicherungVollstaendig) {
        $psb.AppendLine(('Details: {0} Dateidifferenzen, {1} fehlerhafte Pruefsummen von {2} Stichproben.' -f $totalDiffs, $failedSamples, $totalSamples)) | Out-Null
    }
    [System.IO.File]::WriteAllText($protoPath, $psb.ToString(), [System.Text.Encoding]::UTF8)

    if ($sicherungVollstaendig) {
        Send-GuiEvent 'LOG' 'Gruen' ('Sicherung vollstaendig: Ja (0 Differenzen, {0} Stichproben OK)' -f $totalSamples)
    } else {
        Send-GuiEvent 'LOG' 'Rot' ('Sicherung unvollstaendig: Nein ({0} Differenzen, {1} fehlerhafte Pruefsummen)' -f $totalDiffs, $failedSamples)
    }

    if (Get-Command Add-Finding -ErrorAction SilentlyContinue) {
        if ($sicherungVollstaendig) {
            Add-Finding 'Info' 'System-Umzug' ('Sicherung vollstaendig: Ja ({0} Stichproben geprueft, keine Differenzen).' -f $totalSamples)
        } else {
            Add-Finding 'Warnung' 'System-Umzug' ('Sicherung unvollstaendig: Nein ({0} Differenzen, {1} fehlerhafte Pruefsummen).' -f $totalDiffs, $failedSamples)
        }
    }

    # 6. Autarkes Wiederherstellungs-Paket (Wiederherstellen.cmd & Wiederherstellen.ps1)
    Send-GuiEvent 'UMZUG' '95' 'Erstelle autarkes Wiederherstellungspaket ...' ''
    Send-GuiEvent 'LOG' '' 'Erzeuge Wiederherstellen.cmd und Wiederherstellen.ps1 ...'

    # Wiederherstellen.cmd (ANSI / ASCII, keine BOM)
    $cmdPath = Join-Path $backupFolder 'Wiederherstellen.cmd'
    $cmdLines = @(
        '@echo off',
        'setlocal',
        'title System-Wiederherstellung',
        'net session >nul 2>&1',
        'if %errorlevel% neq 0 (',
        '    echo Administratorrechte anfordern ...',
        '    powershell.exe -NoProfile -Command "Start-Process cmd -ArgumentList ''/c \"\"%~f0\"\"'' -Verb RunAs"',
        '    exit /b',
        ')',
        'echo ========================================================================',
        'echo           LEOS MINIBENCH: SYSTEM-WIEDERHERSTELLUNG',
        'echo ========================================================================',
        'echo.',
        'echo Bitte waehlen Sie eine Option:',
        'echo   [1] Trockenlauf (Pruefen, was getan wuerde)',
        'echo   [2] Vollstaendige Wiederherstellung durchfuehren',
        'echo   [3] Abbrechen',
        'echo.',
        'set /p opt="Auswahl (1-3): "',
        'if "%opt%"=="1" (',
        '    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Wiederherstellen.ps1" -Trockenlauf',
        ') else if "%opt%"=="2" (',
        '    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Wiederherstellen.ps1"',
        ') else (',
        '    echo Vorgang abgebrochen.',
        ')',
        'pause'
    )
    [System.IO.File]::WriteAllText($cmdPath, ($cmdLines -join "`r`n"), [System.Text.Encoding]::ASCII)

    # Wiederherstellen.ps1 (UTF-8 mit BOM)
    $ps1Path = Join-Path $backupFolder 'Wiederherstellen.ps1'
    $ps1Lines = @(
        '#Requires -Version 5.1',
        '<#',
        '.SYNOPSIS',
        '    Autarke System-Wiederherstellung nach Windows-Neuinstallation.',
        '    Erstellt von Leos Minibench (System-Umzug).',
        '#>',
        '[CmdletBinding()]',
        'param(',
        '    [switch]$Trockenlauf',
        ')',
        '',
        '$Host.UI.RawUI.WindowTitle = ''Leos Minibench: System-Wiederherstellung''',
        'Clear-Host',
        'Write-Host ''========================================================================'' -ForegroundColor Cyan',
        'Write-Host ''          LEOS MINIBENCH: SYSTEM-WIEDERHERSTELLUNG'' -ForegroundColor White',
        'Write-Host ''========================================================================'' -ForegroundColor Cyan',
        'Write-Host ''''',
        'if ($Trockenlauf) {',
        '    Write-Host ''*** MODUS: TROCKENLAUF (Es werden keine Aenderungen vorgenommen) ***'' -ForegroundColor Magenta',
        '    Write-Host ''''',
        '}',
        'Write-Host (''Ziel-Benutzerprofil:   {0}'' -f $env:USERPROFILE) -ForegroundColor Yellow',
        'Write-Host (''Aktueller Computer:    {0}'' -f $env:COMPUTERNAME) -ForegroundColor Yellow',
        'Write-Host (''Sicherungsverzeichnis: {0}'' -f $PSScriptRoot) -ForegroundColor Yellow',
        'Write-Host ''''',
        'Write-Host ''Dieses Skript stellt gesicherte Benutzerdaten, Bibliotheken,''',
        'Write-Host ''Desktop-Optik, Profile und Programme auf diesem System wieder her.''',
        'Write-Host ''''',
        '',
        '$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)',
        'if (-not $isAdmin) {',
        '    Write-Warning ''Fuer vollstaendige Wiederherstellung werden Administratorrechte empfohlen.''',
        '}',
        '',
        '# 1. Benutzerprofil, Bibliotheken & Konfigurationen',
        '$profSrc = Join-Path $PSScriptRoot ''Benutzerprofil''',
        'if (Test-Path -LiteralPath $profSrc) {',
        '    Write-Host ''[1/6] Benutzer-Bibliotheken und Anwendungsdaten zurueckspielen ...'' -ForegroundColor Green',
        '    $dirs = Get-ChildItem -LiteralPath $profSrc -Directory -ErrorAction SilentlyContinue',
        '    foreach ($d in $dirs) {',
        '        if ($d.Name -eq ''AppData'') {',
        '            $appDataSrc = Join-Path $profSrc ''AppData''',
        '            $appDataDst = Join-Path $env:USERPROFILE ''AppData''',
        '            if ($Trockenlauf) {',
        '                Write-Host ''      [TROCKENLAUF] Wuerde Profile und Konfigurationen spiegeln: AppData ->'' $appDataDst',
        '            } else {',
        '                Write-Host ''      Kopiere Profile und Programm-Konfigurationen ...''',
        '                & robocopy.exe "$appDataSrc" "$appDataDst" /E /XJ /R:1 /W:1 /COPY:DAT /MT:16 /NP /NFL /NDL | Out-Null',
        '            }',
        '        } elseif ($d.Name -eq ''PowerShellProfiles'') {',
        '            $psDst = Join-Path $env:USERPROFILE ''Documents\WindowsPowerShell''',
        '            if ($Trockenlauf) {',
        '                Write-Host ''      [TROCKENLAUF] Wuerde PowerShell-Profile kopieren nach:'' $psDst',
        '            } else {',
        '                try { [System.IO.Directory]::CreateDirectory($psDst) | Out-Null } catch { }',
        '                Write-Host ''      Kopiere PowerShell-Profile ...''',
        '                & robocopy.exe "$($d.FullName)" "$psDst" /E /XJ /R:1 /W:1 /COPY:DAT /MT:16 /NP /NFL /NDL | Out-Null',
        '            }',
        '        } elseif ($d.Name -eq ''WindowsTerminal'') {',
        '            if ($env:LOCALAPPDATA) {',
        '                $wtPkg = Get-ChildItem -LiteralPath (Join-Path $env:LOCALAPPDATA ''Packages'') -Filter ''Microsoft.WindowsTerminal_*'' -Directory -ErrorAction SilentlyContinue | Select-Object -First 1',
        '                if ($wtPkg) {',
        '                    $wtTarget = Join-Path $wtPkg.FullName ''LocalState\settings.json''',
        '                    $wtSrc = Join-Path $d.FullName ''settings.json''',
        '                    if (Test-Path $wtSrc) {',
        '                        if ($Trockenlauf) {',
        '                            Write-Host ''      [TROCKENLAUF] Wuerde Windows Terminal Konfiguration kopieren nach:'' $wtTarget',
        '                        } else {',
        '                            Write-Host ''      Aktualisiere Windows Terminal Konfiguration ...''',
        '                            try { Copy-Item -LiteralPath $wtSrc -Destination $wtTarget -Force } catch { }',
        '                        }',
        '                    }',
        '                }',
        '            }',
        '        } else {',
        '            $targetPath = Join-Path $env:USERPROFILE $d.Name',
        '            switch ($d.Name) {',
        '                ''Dokumente'' { if (-not (Test-Path $targetPath) -and (Test-Path "$env:USERPROFILE\Documents")) { $targetPath = "$env:USERPROFILE\Documents" } }',
        '                ''Bilder''    { if (-not (Test-Path $targetPath) -and (Test-Path "$env:USERPROFILE\Pictures"))  { $targetPath = "$env:USERPROFILE\Pictures" } }',
        '                ''Musik''     { if (-not (Test-Path $targetPath) -and (Test-Path "$env:USERPROFILE\Music"))     { $targetPath = "$env:USERPROFILE\Music" } }',
        '                ''Videos''    { if (-not (Test-Path $targetPath) -and (Test-Path "$env:USERPROFILE\Videos"))    { $targetPath = "$env:USERPROFILE\Videos" } }',
        '            }',
        '            if ($Trockenlauf) {',
        '                Write-Host (''      [TROCKENLAUF] Wuerde spiegeln: {0} -> {1}'' -f $d.Name, $targetPath)',
        '            } else {',
        '                Write-Host (''      Spiegele {0} -> {1} ...'' -f $d.Name, $targetPath)',
        '                try { [System.IO.Directory]::CreateDirectory($targetPath) | Out-Null } catch { }',
        '                & robocopy.exe "$($d.FullName)" "$targetPath" /E /XJ /R:1 /W:1 /COPY:DAT /MT:16 /NP /NFL /NDL | Out-Null',
        '            }',
        '        }',
        '    }',
        '    # Einzeldateien im Profil',
        '    $gitCfgSrc = Join-Path $profSrc ''.gitconfig''',
        '    if (Test-Path -LiteralPath $gitCfgSrc) {',
        '        if ($Trockenlauf) {',
        '            Write-Host ''      [TROCKENLAUF] Wuerde .gitconfig kopieren''',
        '        } else {',
        '            try { Copy-Item -LiteralPath $gitCfgSrc -Destination (Join-Path $env:USERPROFILE ''.gitconfig'') -Force } catch { }',
        '        }',
        '    }',
        '    $gitIgnoreSrc = Join-Path $profSrc ''.gitignore_global''',
        '    if (Test-Path -LiteralPath $gitIgnoreSrc) {',
        '        if ($Trockenlauf) {',
        '            Write-Host ''      [TROCKENLAUF] Wuerde .gitignore_global kopieren''',
        '        } else {',
        '            try { Copy-Item -LiteralPath $gitIgnoreSrc -Destination (Join-Path $env:USERPROFILE ''.gitignore_global'') -Force } catch { }',
        '        }',
        '    }',
        '    $puttyJson = Join-Path $profSrc ''PuTTY_Sessions.json''',
        '    if (Test-Path -LiteralPath $puttyJson) {',
        '        if ($Trockenlauf) {',
        '            Write-Host ''      [TROCKENLAUF] Wuerde PuTTY-Verbindungsprofile wiederherstellen''',
        '        } else {',
        '            Write-Host ''      Stelle PuTTY-Verbindungsprofile wieder her ...''',
        '            try {',
        '                $pData = Get-Content -LiteralPath $puttyJson -Raw -Encoding UTF8 | ConvertFrom-Json',
        '                foreach ($item in $pData) {',
        '                    $targetKey = "HKCU:\Software\SimonTatham\PuTTY\Sessions\$($item.Session)"',
        '                    if (-not (Test-Path -LiteralPath $targetKey)) { New-Item -Path $targetKey -Force | Out-Null }',
        '                    foreach ($prop in $item.Einstellungen.PSObject.Properties) {',
        '                        Set-ItemProperty -Path $targetKey -Name $prop.Name -Value $prop.Value -Force -ErrorAction SilentlyContinue',
        '                    }',
        '                }',
        '            } catch { }',
        '        }',
        '    }',
        '    $puttyReg = Join-Path $profSrc ''PuTTY_Sessions.reg''',
        '    if (Test-Path -LiteralPath $puttyReg) {',
        '        if ($Trockenlauf) {',
        '            Write-Host ''      [TROCKENLAUF] Wuerde PuTTY-Sessions Registry importieren''',
        '        } else {',
        '            Write-Host ''      Importiere PuTTY-Sessions ...''',
        '            & reg.exe import "$puttyReg" 2>$null | Out-Null',
        '        }',
        '    }',
        '} else {',
        '    Write-Host ''[1/6] Keine Benutzerprofil-Sicherung vorhanden (uebersprungen).'' -ForegroundColor DarkGray',
        '}',
        '',
        '# 2. WLAN-Profile',
        '$wlanSrc = Join-Path $PSScriptRoot ''WLAN-Profile''',
        'if (Test-Path -LiteralPath $wlanSrc) {',
        '    Write-Host ''[2/6] WLAN-Profile wiederherstellen ...'' -ForegroundColor Green',
        '    $xmlProfiles = @(Get-ChildItem -LiteralPath $wlanSrc -Filter ''*.xml'' -File -ErrorAction SilentlyContinue)',
        '    foreach ($xp in $xmlProfiles) {',
        '        if ($Trockenlauf) {',
        '            Write-Host (''      [TROCKENLAUF] Wuerde WLAN-Profil importieren: {0}'' -f $xp.Name)',
        '        } else {',
        '            Write-Host (''      Importiere WLAN-Profil: {0} ...'' -f $xp.Name)',
        '            & netsh.exe wlan add profile filename="$($xp.FullName)" user=all 2>$null | Out-Null',
        '        }',
        '    }',
        '} else {',
        '    Write-Host ''[2/6] Keine WLAN-Profile gesichert (uebersprungen).'' -ForegroundColor DarkGray',
        '}',
        '',
        '# 3. C:\-Zusatzordner zuruecklegen',
        '$zusatzSrc = Join-Path $PSScriptRoot ''C_Laufwerk_Zusatz''',
        'if (Test-Path -LiteralPath $zusatzSrc) {',
        '    Write-Host ''[3/6] Zusaetzliche C:\-Ordner zuruecklegen ...'' -ForegroundColor Green',
        '    $cDirs = Get-ChildItem -LiteralPath $zusatzSrc -Directory -ErrorAction SilentlyContinue',
        '    foreach ($cd in $cDirs) {',
        '        $cDest = Join-Path ''C:\'' $cd.Name',
        '        if ($Trockenlauf) {',
        '            Write-Host (''      [TROCKENLAUF] Wuerde Ordner spiegeln: {0} -> {1}'' -f $cd.FullName, $cDest)',
        '        } else {',
        '            Write-Host (''      Kopiere {0} -> {1} ...'' -f $cd.FullName, $cDest)',
        '            try { [System.IO.Directory]::CreateDirectory($cDest) | Out-Null } catch { }',
        '            & robocopy.exe "$($cd.FullName)" "$cDest" /E /XJ /R:1 /W:1 /COPY:DAT /MT:16 /NP /NFL /NDL | Out-Null',
        '        }',
        '    }',
        '} else {',
        '    Write-Host ''[3/6] Keine C:\-Zusatzordner vorhanden (uebersprungen).'' -ForegroundColor DarkGray',
        '}',
        '',
        '# 4. Desktop-Optik anwenden',
        '$layoutSrc = Join-Path $PSScriptRoot ''Desktop-Layout''',
        'if (Test-Path -LiteralPath $layoutSrc) {',
        '    Write-Host ''[4/6] Desktop-Optik und Layout anwenden ...'' -ForegroundColor Green',
        '    if ($Trockenlauf) {',
        '        Write-Host ''      [TROCKENLAUF] Wuerde Desktop-Hintergrundbild und Icon-Layout aktivieren''',
        '    } else {',
        '        $wpSrc = Join-Path $layoutSrc ''Wallpaper.jpg''',
        '        if (Test-Path -LiteralPath $wpSrc) {',
        '            $wpLocal = Join-Path $env:APPDATA ''Microsoft\Windows\Themes\TranscodedWallpaper''',
        '            try {',
        '                $wpDir = Split-Path $wpLocal',
        '                if (-not (Test-Path $wpDir)) { [System.IO.Directory]::CreateDirectory($wpDir) | Out-Null }',
        '                Copy-Item -LiteralPath $wpSrc -Destination $wpLocal -Force',
        '                Add-Type -TypeDefinition @"',
        'using System;',
        'using System.Runtime.InteropServices;',
        'public class DiagWallpaperHelper {',
        '    [DllImport("user32.dll", CharSet = CharSet.Auto)]',
        '    public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);',
        '}',
        '"@ -ErrorAction SilentlyContinue',
        '                [DiagWallpaperHelper]::SystemParametersInfo(20, 0, $wpLocal, 3) | Out-Null',
        '                Write-Host ''      Hintergrundbild erfolgreich aktiviert.''',
        '            } catch { }',
        '        }',
        '',
        '        $layoutJsonFile = Join-Path $layoutSrc ''Layout.json''',
        '        if (Test-Path -LiteralPath $layoutJsonFile) {',
        '            try {',
        '                $lj = Get-Content -LiteralPath $layoutJsonFile -Raw -Encoding UTF8 | ConvertFrom-Json',
        '                if ($lj.WallpaperStyle) {',
        '                    Set-ItemProperty -Path ''HKCU:\Control Panel\Desktop'' -Name ''WallpaperStyle'' -Value ([string]$lj.WallpaperStyle) -Force',
        '                }',
        '                if ($lj.TileWallpaper) {',
        '                    Set-ItemProperty -Path ''HKCU:\Control Panel\Desktop'' -Name ''TileWallpaper'' -Value ([string]$lj.TileWallpaper) -Force',
        '                }',
        '            } catch { }',
        '        }',
        '',
        '        $iconReg = Join-Path $layoutSrc ''IconPositionen.reg''',
        '        if (Test-Path -LiteralPath $iconReg) {',
        '            Write-Host ''      Icon-Koordinaten werden in Registry importiert ...''',
        '            & reg.exe import "$iconReg" 2>$null | Out-Null',
        '        }',
        '',
        '        Write-Host ''      Windows Explorer neu starten, um Layout zu uebernehmen ...''',
        '        Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue',
        '        Start-Sleep -Seconds 1',
        '    }',
        '} else {',
        '    Write-Host ''[4/6] Kein Desktop-Layout vorhanden (uebersprungen).'' -ForegroundColor DarkGray',
        '}',
        '',
        '# 5. Programme per winget einspielen',
        '$wingetFile = Join-Path $PSScriptRoot ''Programme\winget-pakete.json''',
        'if (Test-Path -LiteralPath $wingetFile) {',
        '    Write-Host ''[5/6] Installiere Programme ueber Winget-Paketliste ...'' -ForegroundColor Green',
        '    if ($Trockenlauf) {',
        '        Write-Host (''      [TROCKENLAUF] Wuerde winget import ausfuehren: {0}'' -f $wingetFile)',
        '    } else {',
        '        $wgCmd = Get-Command winget.exe -ErrorAction SilentlyContinue | Select-Object -First 1',
        '        if ($wgCmd) {',
        '            Write-Host ''      winget import wird ausgefuehrt ...''',
        '            & $wgCmd.Source import -i "$wingetFile" --accept-package-agreements --accept-source-agreements',
        '        } else {',
        '            Write-Warning ''winget ist auf diesem System nicht verfuegbar.''',
        '        }',
        '    }',
        '} else {',
        '    Write-Host ''[5/6] Keine Winget-Paketliste vorhanden (uebersprungen).'' -ForegroundColor DarkGray',
        '}',
        '',
        '# 6. Manuelle Installationen oeffnen',
        '$nwFile = Join-Path $PSScriptRoot ''Programme\Nicht-Winget-Programme.txt''',
        'if (Test-Path -LiteralPath $nwFile) {',
        '    Write-Host ''[6/6] Liste manuell zu installierender Programme anzeigen ...'' -ForegroundColor Green',
        '    if ($Trockenlauf) {',
        '        Write-Host (''      [TROCKENLAUF] Wuerde Datei in Notepad anzeigen: {0}'' -f $nwFile)',
        '    } else {',
        '        try { Start-Process notepad.exe -ArgumentList "`"$nwFile`"" } catch { }',
        '    }',
        '} else {',
        '    Write-Host ''[6/6] Keine Liste manueller Programme vorhanden (uebersprungen).'' -ForegroundColor DarkGray',
        '}',
        '',
        'Write-Host ''''',
        'Write-Host ''========================================================================'' -ForegroundColor Cyan',
        'if ($Trockenlauf) {',
        '    Write-Host ''      Trockenlauf abgeschlossen (keine Aenderungen vorgenommen)!'' -ForegroundColor Magenta',
        '} else {',
        '    Write-Host ''      Wiederherstellung abgeschlossen!'' -ForegroundColor Green',
        '}',
        'Write-Host ''========================================================================'' -ForegroundColor Cyan',
        'Write-Host '''''
    )
    [System.IO.File]::WriteAllText($ps1Path, ($ps1Lines -join "`r`n"), [System.Text.Encoding]::UTF8)

    $finalSize = Get-FolderSize $backupFolder
    Send-GuiEvent 'UMZUG' '100' 'Sicherung erfolgreich abgeschlossen.' ''
    Send-GuiEvent 'LOG' 'Gruen' ('Sicherung erfolgreich abgeschlossen: {0} ({1:N2} GB)' -f $backupFolder, ($finalSize / 1GB))

    return [pscustomobject]@{
        Ok           = $true
        Pfad         = $backupFolder
        Groesse      = $finalSize
        Vollstaendig = $sicherungVollstaendig
        Fehler       = ''
    }
}
if ($Umzug) {
    $r = Invoke-SystemBackup -Ziel $UmzugZiel -Umfang $UmzugUmfang -Zusatz $UmzugZusatz
    Send-GuiEvent 'RESULT' $(if ($r.Ok) { '1' } else { '0' }) $r.Pfad $r.Groesse $r.Fehler
    exit $(if ($r.Ok) { 0 } else { 1 })
}
#endregion
