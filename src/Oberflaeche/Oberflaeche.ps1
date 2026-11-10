#region ---------- Grafische Oberfläche ----------
if (-not $EventMode -and -not $ImportOrdner -and -not $Vergleich -and -not $Rueckgaengig -and -not $SensorLive -and -not $SensorWerkzeugeHolen -and -not $SensorAufraeumen -and -not $OptimierungZustand -and -not $SoftwareZustand -and -not $OptWerkzeugeHolen -and -not $SoftwareInstallieren -and -not $Dashboard -and -not $DashboardExport -and -not $DashboardSysteme -and -not $MedianAktualisieren -and -not $Umzug) {
    Write-StartPhase 'Datenordner gefunden'
    # Hardwareabfragen für die Oberfläche (Datenträgerliste, Geräteidentität) laufen parallel zum Laden der Oberfläche
    $hwPs = $null; $hwHandle = $null
    try {
        $hwPs = [powershell]::Create()
        [void]$hwPs.AddScript({
            # ab v4.07 fragt jede Abfrage nur die gebrauchten Eigenschaften ab. Ohne -Property ermittelt WMI alle, bei
            # Win32_Processor auch LoadPercentage (misst rund eine Sekunde lang die Auslastung).
            $hwUhr = [Diagnostics.Stopwatch]::StartNew()
            $res = [ordered]@{ Ms = 0; Disks = @(); Uuid = ''; Bv = ''; Bp = ''; Bs = ''; Bios = ''; Gpus = @(); Domain = $false; Cpu = ''; Cores = 0; Threads = 0; Ram = ''; Board = ''; IsLaptop = $false }
            # ab v4.01 direkt über CIM statt Get-PhysicalDisk/Get-Partition: diese laden beim Start das Storage-Modul (Sekunden).
            # BusType und MediaType kommen als Zahl; Namen wie bei Get-PhysicalDisk.
            try {
                $busNamen = @{ 0 = 'Unknown'; 1 = 'SCSI'; 2 = 'ATAPI'; 3 = 'ATA'; 4 = '1394'; 5 = 'SSA'; 6 = 'Fibre Channel'; 7 = 'USB'; 8 = 'RAID'; 9 = 'iSCSI'; 10 = 'SAS'; 11 = 'SATA'; 12 = 'SD'; 13 = 'MMC'; 14 = 'Virtual'; 15 = 'File Backed Virtual'; 16 = 'Storage Spaces'; 17 = 'NVMe' }
                $medNamen = @{ 0 = 'Unspecified'; 3 = 'HDD'; 4 = 'SSD'; 5 = 'SCM' }
                $ns = 'root/Microsoft/Windows/Storage'
                $parts = @(Get-CimInstance -Namespace $ns -ClassName MSFT_Partition -Property DiskNumber, DriveLetter -ErrorAction SilentlyContinue)
                $res.Disks = @(foreach ($pd in @(Get-CimInstance -Namespace $ns -ClassName MSFT_PhysicalDisk -Property DeviceId, BusType, MediaType, FriendlyName, Size -ErrorAction Stop | Sort-Object { [int]$_.DeviceId })) {
                    $num = [int]$pd.DeviceId
                    $letters = @($parts | Where-Object { [int]$_.DiskNumber -eq $num -and $_.DriveLetter } | ForEach-Object { '{0}:' -f $_.DriveLetter } | Sort-Object) -join ', '
                    $bus = $(if ($busNamen.ContainsKey([int]$pd.BusType)) { $busNamen[[int]$pd.BusType] } else { [string]$pd.BusType })
                    $med = $(if ($medNamen.ContainsKey([int]$pd.MediaType)) { $medNamen[[int]$pd.MediaType] } else { [string]$pd.MediaType })
                    [pscustomobject]@{ Num = $num; Name = ([string]$pd.FriendlyName).Trim(); Bus = $bus; Medium = $med; Size = [double]$pd.Size; Letters = $letters }
                })
            } catch { }
            try { $res.Uuid = [string](Get-CimInstance Win32_ComputerSystemProduct -Property UUID -ErrorAction Stop).UUID } catch { }
            try { $bb = Get-CimInstance Win32_BaseBoard -Property Manufacturer, Product, SerialNumber -ErrorAction Stop | Select-Object -First 1; $res.Bv = [string]$bb.Manufacturer; $res.Bp = [string]$bb.Product; $res.Bs = [string]$bb.SerialNumber; $res.Board = [string]$bb.Product } catch { }
            try { $res.Bios = [string](Get-CimInstance Win32_BIOS -Property SerialNumber -ErrorAction Stop).SerialNumber } catch { }
            # Grafikeinheiten für die Auswahl im Benchmark und im Lasttest (ab v2.65)
            try { $res.Gpus = @(Get-CimInstance Win32_VideoController -Property Name -ErrorAction Stop | ForEach-Object { ([string]$_.Name).Trim() } | Where-Object { $_ }) } catch { }
            # Domänenmitgliedschaft für die Vorlagen der Optimierung (ab v2.8)
            try { $res.Domain = [bool](Get-CimInstance Win32_ComputerSystem -Property PartOfDomain -ErrorAction Stop).PartOfDomain } catch { }
            try {
                $cpu = Get-CimInstance Win32_Processor -Property Name, NumberOfCores, NumberOfLogicalProcessors -ErrorAction SilentlyContinue | Select-Object -First 1
                if ($cpu) {
                    $res.Cpu = [string]$cpu.Name
                    $res.Cores = [int]$cpu.NumberOfCores
                    $res.Threads = [int]$cpu.NumberOfLogicalProcessors
                }
            } catch { }
            try {
                $mems = @(Get-CimInstance Win32_PhysicalMemory -Property Capacity, ConfiguredClockSpeed, Speed, SMBIOSMemoryType -ErrorAction SilentlyContinue)
                $typeMap = @{ 20='DDR'; 21='DDR2'; 24='DDR3'; 26='DDR4'; 27='LPDDR'; 28='LPDDR2'; 29='LPDDR3'; 30='LPDDR4'; 34='DDR5'; 35='LPDDR5' }
                $totalCap = 0; $spd = 0; $mType = ''
                foreach ($m in $mems) {
                    $totalCap += [double]$m.Capacity
                    if (-not $spd) { $spd = if ($m.ConfiguredClockSpeed) { [int]$m.ConfiguredClockSpeed } else { [int]$m.Speed } }
                    if (-not $mType -and $typeMap.ContainsKey([int]$m.SMBIOSMemoryType)) { $mType = $typeMap[[int]$m.SMBIOSMemoryType] }
                }
                if ($totalCap -gt 0) {
                    $res.Ram = ('{0} GB {1}{2}' -f [math]::Round($totalCap / 1GB), $(if ($mType) { $mType } else { 'RAM' }), $(if ($spd -gt 0) { '-' + $spd } else { '' }))
                }
            } catch { }
            try {
                $chassis = @(Get-CimInstance Win32_SystemEnclosure -Property ChassisTypes -ErrorAction SilentlyContinue | ForEach-Object { $_.ChassisTypes } | ForEach-Object { $_ })
                $res.IsLaptop = [bool]($chassis | Where-Object { $_ -in 8, 9, 10, 11, 12, 14, 30, 31, 32 })
            } catch { }
            $res.Ms = [int]$hwUhr.ElapsedMilliseconds
            [pscustomobject]$res
        })
        $hwHandle = $hwPs.BeginInvoke()
    } catch { $hwPs = $null }
    Add-Type -AssemblyName System.Windows.Forms, System.Drawing
    if ($ExecutionContext.SessionState.LanguageMode -ne 'FullLanguage') {
        [void][Windows.Forms.MessageBox]::Show('PowerShell läuft hier im Constrained Language Mode (AppLocker oder WDAC). Die Oberfläche und die Testroutinen von Leos Minibench sind so nicht verfügbar.', 'Leos Minibench', 'OK', 'Warning')
        exit 1
    }
    $guiCode = @'
#>> EINBINDEN Oberflaeche\DiagGui.cs
#>> EINBINDEN Oberflaeche\DiagGui_Darstellung.cs
#>> EINBINDEN Oberflaeche\DiagGui_Einstellungen.cs
#>> EINBINDEN Oberflaeche\DiagGui_Update.cs
#>> EINBINDEN Oberflaeche\DiagGui_Startseite.cs
#>> EINBINDEN Oberflaeche\DiagGui_Diagnose.cs
#>> EINBINDEN Oberflaeche\DiagGui_Benchmark.cs
#>> EINBINDEN Oberflaeche\DiagGui_Lasttest.cs
#>> EINBINDEN Oberflaeche\DiagGui_Wartung.cs
#>> EINBINDEN Oberflaeche\DiagGui_Optimierung.cs
#>> EINBINDEN Oberflaeche\DiagGui_Tools.cs
#>> EINBINDEN Oberflaeche\DiagGui_Sensoren.cs
#>> EINBINDEN Oberflaeche\DiagGui_Vergleich.cs
#>> EINBINDEN Oberflaeche\DiagGui_Umzug.cs
#>> EINBINDEN Oberflaeche\DiagGui_Aenderungen.cs
#>> EINBINDEN Oberflaeche\DiagGui_Versionen.cs
#>> EINBINDEN Oberflaeche\DiagGui_Ablauf.cs
#>> EINBINDEN Oberflaeche\DiagGui_Vorlagen.cs
#>> EINBINDEN Oberflaeche\DiagGui_Lauf.cs
#>> EINBINDEN Oberflaeche\DiagGui_Ergebnis.cs
#>> EINBINDEN Oberflaeche\Bewegung.cs
#>> EINBINDEN Oberflaeche\DiagGui_Steuerelemente.cs
#>> EINBINDEN Oberflaeche\DiagGui_Modelle.cs
#>> EINBINDEN Oberflaeche\Start.cs
#>> EINBINDEN Oberflaeche\Versionen.cs
'@
    try {
        if (-not ('DiagGui' -as [type])) {
            Write-StartPhase 'Oberfläche wird geladen'
            Add-Type -AssemblyName System.Web.Extensions
            $refs = @([Windows.Forms.Form].Assembly.Location, [Drawing.Color].Assembly.Location, [System.Web.Script.Serialization.JavaScriptSerializer].Assembly.Location)
            Add-CachedType 'LeosMinibench-Oberflaeche' $guiCode $refs
            Write-StartPhase ('Oberfläche geladen ({0})' -f $(if ($script:CacheInfo.Count) { $script:CacheInfo[$script:CacheInfo.Count - 1] } else { 'ohne Cache' }))
        }
        # ab v4.01 vor dem Warten auf die Hardwareabfrage, damit beides gleichzeitig läuft.
        # Datenpflege (ab v2.7): Lasttests vor v2.67, unvollständige und kurze Läufe ins Archiv; frische Läufe (unter 60 Min.) bleiben
        # ab v4.07 nur, wenn sich seit der letzten Prüfung etwas geändert hat (Get-DatenpflegeStand): spart 0,8 bis 1,5 s je Start
        try {
            $dpStand = Get-DatenpflegeStand -DataDir $script:DataDir
            if (Test-DatenpflegeErledigt -DataDir $script:DataDir -Stand $dpStand -MindestAlterMin 60) {
                Write-StartPhase 'Datenpflege: unverändert seit der letzten Prüfung'
            } else {
                $dp = Invoke-Datenpflege -DataDir $script:DataDir -MindestAlterMin 60
                if ($dp.Verschoben -or $dp.Fehler) { [DiagGui]::DatenpflegeInfo = ('Datenpflege beim Start: {0}. Protokoll: {1}' -f $dp.Kurz, (Join-Path $dp.Archiv 'Datenpflege.log')) }
                # Stand nach der Prüfung festhalten (sie kann selbst verschoben haben); bei Fehlern beim nächsten Start erneut prüfen
                if (-not $dp.Fehler) { Save-DatenpflegeStand -DataDir $script:DataDir -Stand (Get-DatenpflegeStand -DataDir $script:DataDir) }
                Write-StartPhase ('Datenpflege: {0}' -f $dp.Kurz)
            }
        } catch { }
        try { [DiagGui]::OptKatalog = [string[]](Get-OptGuiLines) } catch { }
        try { [DiagGui]::SoftwarePakete = [string[]](Get-SoftwarePaketGuiLines) } catch { }
        Write-StartPhase 'Katalog der Optimierung gelesen'
        # Datenträger für die Auswahl im Benchmark und im Lasttest: Nummer|Name|Bus|Medium|Größe|Buchstaben|USB
        $diskList = [System.Collections.Generic.List[string]]::new()
        $hw = $null
        if ($hwPs) { try { if ($hwHandle.AsyncWaitHandle.WaitOne(20000)) { $hw = @($hwPs.EndInvoke($hwHandle)) | Select-Object -First 1 } } catch { }; try { $hwPs.Dispose() } catch { } }
        Write-StartPhase $(if ($hw -and $hw.Ms) { 'Hardwareabfrage abgeschlossen (Abfrage {0} ms)' -f $hw.Ms } else { 'Hardwareabfrage abgeschlossen' })
        foreach ($d in @($(if ($hw) { $hw.Disks }))) {
            $sz = [double]$d.Size
            $szT = $(if ($sz -ge 1e12) { '{0:N1} TB' -f ($sz / 1e12) } elseif ($sz -ge 1e9) { '{0:N0} GB' -f ($sz / 1e9) } else { '{0:N0} MB' -f ($sz / 1e6) })
            $diskList.Add(('{0}|{1}|{2}|{3}|{4}|{5}|{6}' -f $d.Num, (([string]$d.Name) -replace '\|', '/'), $d.Bus, $d.Medium, $szT, $d.Letters, $(if ([string]$d.Bus -eq 'USB') { '1' } else { '0' })))
        }
        if ($hw -and -not $script:DeviceIdentity) { try { $script:DeviceIdentity = ConvertTo-DeviceId -Uuid $hw.Uuid -BoardVendor $hw.Bv -BoardProduct $hw.Bp -BoardSerial $hw.Bs -BiosSerial $hw.Bios -ComputerName $env:COMPUTERNAME } catch { } }
        Write-StartPhase 'Datenträger und Geräteidentität gelesen'
        if ($script:DataDirFallback) { [void][Windows.Forms.MessageBox]::Show(('Der Ordner neben dem Programm ist nicht beschreibbar (USB-Stick schreibgeschützt?). Ersatzweise wird verwendet:' + "`r`n" + $script:DataDir), 'Leos Minibench', 'OK', 'Warning') }
        elseif (-not $script:DataDir) { [void][Windows.Forms.MessageBox]::Show('Es wurde kein beschreibbarer Ordner für Berichte gefunden.', 'Leos Minibench', 'OK', 'Error'); exit 1 }
        New-Item -ItemType Directory -Path $script:CpDir -Force | Out-Null
        $devId = ''; try { $devId = [string](Get-DeviceIdentity).Id } catch { }
        try { [DiagGui]::GpuNames = [string[]]@($(if ($hw) { $hw.Gpus })) } catch { }
        try { [DiagGui]::DomainPc = [bool]($hw -and $hw.Domain) } catch { }
        try {
            if ($hw) {
                [DiagGui]::HwCpu = [string]$hw.Cpu
                [DiagGui]::HwCores = [int]$hw.Cores
                [DiagGui]::HwThreads = [int]$hw.Threads
                [DiagGui]::HwRam = [string]$hw.Ram
                [DiagGui]::HwBoard = [string]$hw.Board
                [DiagGui]::HwIsLaptop = [bool]$hw.IsLaptop
            }
        } catch { }
        Write-StartPhase 'Fenster wird aufgebaut'
        [DiagGui]::Run((Get-Process -Id $PID).Path, $PSCommandPath, $script:CpDir, $script:DataDir, $ScriptVersion, $diskList.ToArray(), [string[]](Get-ContractGuiLines), $devId)
        exit 0
    } catch {
        [void][Windows.Forms.MessageBox]::Show(('Die Oberfläche konnte nicht gestartet werden:' + "`r`n`r`n" + $_.Exception.Message), 'Leos Minibench', 'OK', 'Error')
        exit 1
    }
}
#endregion


