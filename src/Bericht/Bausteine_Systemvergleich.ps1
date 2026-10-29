#region ---------- Grafische Bausteine für die Berichte ----------
$script:GroupOfKey = @{ 'CPU' = 'Prozessor'; 'RAM' = 'Arbeitsspeicher'; 'GPU' = 'Grafik'; 'DISK' = 'Laufwerke'; 'LW' = 'Laufwerke' }

# Balken 0 bis 150 % mit Markierung bei 100 %
function Get-RefBar($Pct, [string]$Cls = 'info', [switch]$Small) {
    if ($null -eq $Pct -or "$Pct" -eq '') { return '' }
    $w = [math]::Round([math]::Min(150.0, [math]::Max(0.0, [double]$Pct)) / 150.0 * 100.0, 1)
    return [string]::Format($script:Inv, '<span class="rb{0}" title="{1} % der Referenz"><span class="rbf {2}" style="width:{3}%"></span><span class="rbm"></span></span>', $(if ($Small) { ' s' } else { '' }), $Pct, $Cls, $w)
}

# Abweichung farbig: positiv = besser
function Get-DeltaHtml([string]$Text) {
    if (-not $Text) { return '' }
    $m = [regex]::Match($Text, '^([+-]?\d+)')
    $c = ''
    if ($m.Success) { $d = [int]$m.Groups[1].Value; $c = $(if ($d -le -10) { 'warnt' } elseif ($d -ge 3) { 'okt' } else { '' }) }
    return ('<span class="dl {0}">{1}</span>' -f $c, (ConvertTo-HtmlText $Text))
}

# Nutzungsprofile (Gaming / Büro / Workstation) im UserBenchmark-Stil
function New-ProfileCards {
    $gCpu = Get-BenchGroup 'CPU'
    $gGpu = Get-BenchGroup 'GPU'
    $gRam = Get-BenchGroup 'RAM'
    $gDsk = Get-BenchGroup 'Laufwerke'

    # Nur anzeigen, wenn mindestens 3 der 4 Hauptgruppen (CPU, GPU, RAM, Laufwerke) Referenzdaten haben
    $hasRef = 0
    foreach ($g in $gCpu, $gGpu, $gRam, $gDsk) {
        if ($null -ne $g.RefPct -and [double]$g.RefPct -gt 0) { $hasRef++ }
    }
    if ($hasRef -lt 3) { return '' }

    # Einzelwerte für CPU-ST und CPU-MT ermitteln (falls vorhanden, sonst Fallback auf CPU-Gruppenwert)
    $cpuSt = @($script:BenchResults | Where-Object { ($_.Key -eq 'CPU|ST' -or $_.RefKey -eq 'CPU|ST') -and $null -ne $_.RefPct -and [double]$_.RefPct -gt 0 } | Select-Object -First 1).RefPct
    $cpuMt = @($script:BenchResults | Where-Object { ($_.Key -eq 'CPU|MT' -or $_.RefKey -eq 'CPU|MT') -and $null -ne $_.RefPct -and [double]$_.RefPct -gt 0 } | Select-Object -First 1).RefPct
    if ($null -eq $cpuSt -and $null -ne $gCpu.RefPct) { $cpuSt = $gCpu.RefPct }
    if ($null -eq $cpuMt -and $null -ne $gCpu.RefPct) { $cpuMt = $gCpu.RefPct }

    $profiles = @(
        @{ Name = 'Gaming';       Icon = '🎮'; W = @{ 'CPU_ST' = 1.0; 'CPU_MT' = 0.5; 'GPU' = 3.0; 'RAM' = 0.5; 'DISK' = 0.5 } }
        @{ Name = 'Büro/Desktop'; Icon = '💼'; W = @{ 'CPU_ST' = 2.0; 'CPU_MT' = 1.0; 'GPU' = 0.5; 'RAM' = 1.0; 'DISK' = 2.0 } }
        @{ Name = 'Workstation';  Icon = '🚀'; W = @{ 'CPU_ST' = 0.5; 'CPU_MT' = 6.0; 'GPU' = 1.0; 'RAM' = 1.5; 'DISK' = 1.0 } }
    )

    $sb = New-Object System.Text.StringBuilder
    [void]$sb.Append('<div class="ov profile-grid">')
    foreach ($p in $profiles) {
        $vals = [System.Collections.Generic.List[double]]::new()
        $weights = [System.Collections.Generic.List[double]]::new()

        if ($null -ne $cpuSt -and [double]$cpuSt -gt 0) { $vals.Add([double]$cpuSt); $weights.Add($p.W['CPU_ST']) }
        if ($null -ne $cpuMt -and [double]$cpuMt -gt 0) { $vals.Add([double]$cpuMt); $weights.Add($p.W['CPU_MT']) }
        if ($null -ne $gGpu.RefPct -and [double]$gGpu.RefPct -gt 0) { $vals.Add([double]$gGpu.RefPct); $weights.Add($p.W['GPU']) }
        if ($null -ne $gRam.RefPct -and [double]$gRam.RefPct -gt 0) { $vals.Add([double]$gRam.RefPct); $weights.Add($p.W['RAM']) }
        if ($null -ne $gDsk.RefPct -and [double]$gDsk.RefPct -gt 0) { $vals.Add([double]$gDsk.RefPct); $weights.Add($p.W['DISK']) }

        if (-not $vals.Count) { continue }
        $score = Get-GeoMean $vals $weights
        $word = Get-BenchRatingWord $score
        $bar = Get-RefBar $score 'ok'

        # Transparenter Hover-Tooltip zur Zusammensetzung der Bewertung
        $tipLines = [System.Collections.Generic.List[string]]::new()
        $wTotal = 0.0
        foreach ($w in $weights) { $wTotal += $w }
        $tipLines.Add(('Bewertung {0}: {1} % der Referenz ({2})' -f $p.Name, $score, $word))
        $tipLines.Add('Zusammensetzung (gewichtetes geometrisches Mittel):')
        if ($null -ne $gGpu.RefPct -and [double]$gGpu.RefPct -gt 0 -and $p.W['GPU'] -gt 0) {
            $pct = [math]::Round(($p.W['GPU'] / $wTotal) * 100.0)
            $tipLines.Add(('Grafikkarte (GPU): {0} % Gewicht (dieser PC: {1:N0} % der Referenz)' -f $pct, $gGpu.RefPct))
        }
        if ($null -ne $cpuSt -and [double]$cpuSt -gt 0 -and $p.W['CPU_ST'] -gt 0) {
            $pct = [math]::Round(($p.W['CPU_ST'] / $wTotal) * 100.0)
            $tipLines.Add(('CPU Single-Core: {0} % Gewicht (dieser PC: {1:N0} % der Referenz)' -f $pct, $cpuSt))
        }
        if ($null -ne $cpuMt -and [double]$cpuMt -gt 0 -and $p.W['CPU_MT'] -gt 0) {
            $pct = [math]::Round(($p.W['CPU_MT'] / $wTotal) * 100.0)
            $tipLines.Add(('CPU Multi-Core: {0} % Gewicht (dieser PC: {1:N0} % der Referenz)' -f $pct, $cpuMt))
        }
        if ($null -ne $gRam.RefPct -and [double]$gRam.RefPct -gt 0 -and $p.W['RAM'] -gt 0) {
            $pct = [math]::Round(($p.W['RAM'] / $wTotal) * 100.0)
            $tipLines.Add(('Arbeitsspeicher (RAM): {0} % Gewicht (dieser PC: {1:N0} % der Referenz)' -f $pct, $gRam.RefPct))
        }
        if ($null -ne $gDsk.RefPct -and [double]$gDsk.RefPct -gt 0 -and $p.W['DISK'] -gt 0) {
            $pct = [math]::Round(($p.W['DISK'] / $wTotal) * 100.0)
            $tipLines.Add(('Datenträger (SSD): {0} % Gewicht (dieser PC: {1:N0} % der Referenz)' -f $pct, $gDsk.RefPct))
        }
        $tipLines.Add('Berechnung: Relativ zur typischen Leistung der Hardwareklasse (Index 100).')
        $tipText = ($tipLines -join "`n")

        [void]$sb.Append(('<div class="tile profile" title="{0}" style="cursor:help;"><h4>{1} {2}</h4><div class="big">{3}<small> %</small></div><div class="pword">{4}</div>{5}</div>' -f
            (ConvertTo-HtmlText $tipText), $p.Icon, (ConvertTo-HtmlText $p.Name), $score, (ConvertTo-HtmlText $word), $bar))
    }
    [void]$sb.Append('</div>')
    return $sb.ToString()
}

# Kacheln über dem Benchmark: eine je Gruppe mit Referenzbalken
function New-BenchOverview {
    $bcls = @{ OK = 'ok'; Info = 'info'; Warnung = 'warn'; Fehler = 'crit' }
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.Append('<div class="ov">')
    foreach ($gk in $script:BenchGroupOrder) {
        $g = Get-BenchGroup $gk
        if (-not $g.Anzahl) { continue }
        $c = $bcls[$g.Status]; if (-not $c) { $c = 'info' }
        $kopf = $g.Kopf
        if ($null -ne $g.RefPct) {
            $big = '{0}<small> % der Referenz</small>' -f $g.RefPct
            $bar = Get-RefBar $g.RefPct $c
        } else {
            $big = ConvertTo-HtmlText $g.Status
            $bar = ''
        }
        [void]$sb.Append(('<a class="tile t{0}" href="#bg-{1}" onclick="document.getElementById(''bg-{1}'').open=true"><h4>{2}<i class="dot {0}" title="{3}"></i></h4><div class="big">{4}</div>{5}<p>{6}</p></a>' -f
            $c, $gk, (ConvertTo-HtmlText $g.Name), (ConvertTo-HtmlText $g.Status), $big, $bar, (ConvertTo-HtmlText $kopf)))
    }
    [void]$sb.Append('</div>')
    return $sb.ToString()
}

# Eine Messgröße für mehrere Systeme als Balkengruppe, immer aufsteigend sortiert (niedrigster Wert oben, nicht
# gemessene Systeme am Ende). Farbe und Name bleiben dem System zugeordnet; -Own markiert das eigene System.
# Mode Base: Abstand zum eigenen System (positiv = besser), Best: Abstand zum Bestwert, Abs: feste Skala bis AbsMax
function New-MetricBarsHtml {
    param([string]$Label, [string]$Unit, [string]$Fmt, [bool]$LowerBetter, [object[]]$Values, [string[]]$Names, [string]$Mode = 'Best', [double]$AbsMax = 0, [string]$Extra = '', [string]$Hint = '', [int]$Own = -1)
    $valid = @($Values | Where-Object { $null -ne $_ -and [double]$_ -gt 0 } | ForEach-Object { [double]$_ })
    if (-not $valid.Count) { return '' }
    if ($Mode -eq 'Base' -and $Own -lt 0) { $Own = 0 }
    $max = ($valid | Measure-Object -Maximum).Maximum; $min = ($valid | Measure-Object -Minimum).Minimum
    $best = $(if ($LowerBetter) { $min } else { $max })
    $ownV = $(if ($Own -ge 0 -and $Own -lt $Values.Count -and $null -ne $Values[$Own] -and [double]$Values[$Own] -gt 0) { [double]$Values[$Own] } else { $null })
    $sub = @($(if ($Unit) { $Unit }), $(if ($LowerBetter) { 'niedriger ist besser' } else { 'höher ist besser' }), 'aufsteigend sortiert', $Extra) | Where-Object { $_ }
    $order = @(0..($Names.Count - 1) | Sort-Object @{ e = { $v = $(if ($_ -lt $Values.Count) { $Values[$_] } else { $null }); if ($null -ne $v -and [double]$v -gt 0) { 0 } else { 1 } } }, @{ e = { $v = $(if ($_ -lt $Values.Count) { $Values[$_] } else { $null }); if ($null -ne $v) { [double]$v } else { 0 } } }, @{ e = { $_ } })
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.Append(('<div class="mrow"><div class="mlab">{0}<small>{1}</small>{2}</div><div class="mbars">' -f (ConvertTo-HtmlText $Label), (ConvertTo-HtmlText ($sub -join ' · ')), $(if ($Hint) { '<small class="hint">' + (ConvertTo-HtmlText $Hint) + '</small>' } else { '' })))
    foreach ($i in $order) {
        $v = $(if ($i -lt $Values.Count -and $null -ne $Values[$i] -and [double]$Values[$i] -gt 0) { [double]$Values[$i] } else { $null })
        $ownCls = $(if ($i -eq $Own) { ' own' } else { '' })
        if ($null -eq $v) {
            [void]$sb.Append(('<div class="mb{2}"><span class="mn"><i class="sw c{0}"></i>{1}</span><span class="mt"></span><span class="mv na">nicht gemessen</span></div>' -f ($i % 6), (ConvertTo-HtmlText $Names[$i]), $ownCls))
            continue
        }
        $w = $(if ($Mode -eq 'Abs' -and $AbsMax -gt 0) { $v / $AbsMax } elseif ($LowerBetter) { $min / $v } else { $v / $max }) * 100
        $d = ''
        if ($Mode -eq 'Base') {
            if ($i -ne $Own -and $null -ne $ownV) { $rel = Get-RelText $ownV $v $LowerBetter; if ($rel) { $d = Get-DeltaHtml $rel } }
        } elseif ($valid.Count -gt 1) {
            if ($v -eq $best) { $d = '<span class="dl okt">Bestwert</span>' }
            else { $p = $(if ($LowerBetter) { ($best / $v - 1) * 100 } else { ($v / $best - 1) * 100 }); $d = '<span class="dl">{0:+0;-0;0} %</span>' -f $p }
        }
        [void]$sb.Append([string]::Format($script:Inv, '<div class="mb{0}{6}"><span class="mn" title="{1}"><i class="sw c{2}"></i>{1}</span><span class="mt"><span class="mf c{2}" style="width:{3:0.#}%"></span></span><span class="mv">{4}{5}</span></div>',
            $(if ($valid.Count -gt 1 -and $v -eq $best) { ' best' } else { '' }), (ConvertTo-HtmlText $Names[$i]), ($i % 6), [math]::Max(1.5, [math]::Min(100.0, $w)), (ConvertTo-HtmlText (Format-Metric $v $Fmt $Unit)), $d, $ownCls))
    }
    [void]$sb.Append('</div></div>')
    return $sb.ToString()
}

function Get-Short([string]$s, [int]$n) { if (-not $s) { return '' }; if ($s.Length -le $n) { return $s }; return $s.Substring(0, $n - 1).TrimEnd() + '…' }

# Systemvergleich aus der Datenbank, ohne neuen Benchmark
function New-CompareReport([string[]]$Paths) {
    $sys = [System.Collections.Generic.List[object]]::new()
    foreach ($p in $Paths) {
        $j = Read-JsonFile $p
        if (-not $j) { Write-Host ('Nicht lesbar: {0}' -f $p); continue }
        $lw = @($j.Laufwerke | Where-Object { $_ -and [double]$_.SR -gt 0 })
        $sys.Add([pscustomobject]@{ Path = $p; Computer = [string]$j.Computer; Datum = [string]$j.Datum; Name = [string]$j.Name; Hw = $j.Hardware; System = [string]$j.System
            Werte = (ConvertTo-ValueTable $j.Werte); Messwerte = (ConvertTo-ValueTable $j.Messwerte); Messdauer = [string]$j.Messdauer; Version = [string]$j.Version; Quelle = [string]$j.Quelle
            Befunde = $j.Befunde; Laufwerke = $lw; Label = '' })
    }
    if ($sys.Count -lt 2) { Write-Host 'Für einen Vergleich werden mindestens zwei lesbare Systeme benötigt.'; return '' }
    foreach ($s in $sys) {
        $s.Label = $(if (@($sys | Where-Object { $_.Computer -eq $s.Computer }).Count -gt 1) { '{0} {1}' -f $s.Computer, ($s.Datum -replace '^\d{4}-(\d\d)-(\d\d).*$', '$2.$1.') } else { $s.Computer })
        if (-not $s.Label) { $s.Label = Get-Short $s.Name 22 }
    }
    $names = [string[]]@($sys | ForEach-Object { $_.Label })
    # eigenes System hervorheben: der PC, auf dem der Vergleich erstellt wird (neuester Eintrag, falls mehrere)
    $own = -1; for ($i = 0; $i -lt $sys.Count; $i++) { if ($sys[$i].Computer -eq $env:COMPUTERNAME -and ($own -lt 0 -or $sys[$i].Datum -gt $sys[$own].Datum)) { $own = $i } }
    # schnellstes Laufwerk je System (nach sequentiellem Lesen)
    foreach ($s in $sys) {
        $f = $s.Laufwerke | Sort-Object { [double]$_.SR } -Descending | Select-Object -First 1
        if ($f) { foreach ($k in 'SR', 'SW', 'R1', 'R8', 'W1') { $s.Werte['LW|' + $k] = [double]$f.$k }; $s | Add-Member -NotePropertyName Fastest -NotePropertyValue ([string]$f.Laufwerk) -Force }
        else {
            foreach ($k in 'SR', 'SW', 'R1', 'R8', 'W1') { $m = @($s.Werte.Keys | Where-Object { $_ -like "DISK|*|$k" } | ForEach-Object { $s.Werte[$_] } | Measure-Object -Maximum).Maximum; if ($m) { $s.Werte['LW|' + $k] = [double]$m } }
            $s | Add-Member -NotePropertyName Fastest -NotePropertyValue '' -Force
        }
    }
    # Bezug 100 %: gespeicherte Referenz (Referenz.json); fehlt sie oder eine Messgröße darin, je Messgröße das beste
    # der verglichenen Systeme (nach dem schnellsten Laufwerk, damit auch die Laufwerke einen Bezug haben)
    $refObj = $null; try { $refObj = Get-SavedReference } catch { }
    $best = @{}
    foreach ($k in @($sys | ForEach-Object { $_.Werte.Keys } | Select-Object -Unique)) {
        $vs = @($sys | ForEach-Object { [double]$_.Werte[$k] } | Where-Object { $_ -gt 0 })
        if ($vs.Count) { $best[$k] = $(if (Test-LowerBetterKey $k) { ($vs | Measure-Object -Minimum).Minimum } else { ($vs | Measure-Object -Maximum).Maximum }) }
    }
    $ref = $(if ($refObj) { $refObj.Werte } else { $best })
    $refLabel = $(if ($refObj) { [string]$refObj.Name } else { 'je Messgröße das beste der verglichenen Systeme' })
    $lwDefs = @(
        @{ K = 'LW|SR'; N = 'Sequentiell lesen'; U = 'MB/s'; F = 'N0'; R = 'DISK|NVMe4|SR' }
        @{ K = 'LW|SW'; N = 'Sequentiell schreiben'; U = 'MB/s'; F = 'N0'; R = 'DISK|NVMe4|SW' }
        @{ K = 'LW|R1'; N = '4K zufällig lesen QD1'; U = 'IOPS'; F = 'N0'; R = 'DISK|NVMe4|R1' }
        @{ K = 'LW|R8'; N = '4K zufällig lesen, 8 Threads'; U = 'IOPS'; F = 'N0'; R = 'DISK|NVMe4|R8' }
        @{ K = 'LW|W1'; N = '4K zufällig schreiben'; U = 'IOPS'; F = 'N0'; R = 'DISK|NVMe4|W1' }
    )
    # Gesamtbild: gewichtetes geometrisches Mittel je Gruppe in % der Standardreferenz, nur über Messgrößen, die alle Systeme haben
    # Bei der Grafik wird die reale Renderleistung (FPS) 3-fach gegenüber theoretischen Werten gewichtet
    $groups = [ordered]@{ 'Prozessor' = @($script:MetricDefs | Where-Object { $_.K -like 'CPU|*' }); 'Arbeitsspeicher' = @($script:MetricDefs | Where-Object { $_.K -like 'RAM|*' })
        'Grafik' = @($script:MetricDefs | Where-Object { $_.K -like 'GPU|*' }); 'Laufwerke' = @($lwDefs | Where-Object { $_.K -in 'LW|SR', 'LW|SW', 'LW|R1' }) }
    $ovRows = [System.Collections.Generic.List[object]]::new()
    foreach ($gn in $groups.Keys) {
        $defs = @($groups[$gn])
        $common = @($defs | Where-Object { $d = $_; @($sys | Where-Object { $_.Werte[$d.K] -gt 0 }).Count -eq $sys.Count })
        $use = $(if ($common.Count) { $common } else { $defs })
        $vals = @(foreach ($s in $sys) {
            $r = [System.Collections.Generic.List[double]]::new()
            $w = [System.Collections.Generic.List[double]]::new()
            foreach ($d in $use) {
                $rk = $(if ($d.R -and $refObj) { $d.R } else { $d.K }); $v = [double]$s.Werte[$d.K]; $rv = [double]$ref[$rk]
                if ($rv -le 0) { $rv = [double]$best[$d.K] }
                if ($v -gt 0 -and $rv -gt 0) {
                    $r.Add($(if ($d.L) { $rv / $v * 100 } else { $v / $rv * 100 }))
                    # Gemessene FPS-Renderleistung erhält 3-faches Gewicht gegenüber theoretischen Durchsatzwerten
                    $w.Add($(if ($gn -eq 'Grafik' -and $d.K -match 'REND|RPKT') { 3.0 } else { 1.0 }))
                }
            }
            Get-GeoMean $r $w
        })
        if (@($vals | Where-Object { $_ }).Count) { $ovRows.Add([pscustomobject]@{ N = $gn; V = $vals; Basis = (@($use | ForEach-Object { $_.N }) -join ', ') }) }
    }
    # Prozessor und Grafik erhalten im Gesamtwert ein höheres Gewicht (2x) als Arbeitsspeicher und Laufwerke (1x)
    $groupWeights = @{ 'Prozessor' = 2.0; 'Grafik' = 2.0; 'Arbeitsspeicher' = 1.0; 'Laufwerke' = 1.0 }
    $gesamt = @(for ($i = 0; $i -lt $sys.Count; $i++) {
        $x = [System.Collections.Generic.List[double]]::new()
        $gw = [System.Collections.Generic.List[double]]::new()
        foreach ($row in $ovRows) {
            if ($row.V[$i]) {
                $x.Add($row.V[$i])
                $gw.Add($(if ($groupWeights.ContainsKey($row.N)) { $groupWeights[$row.N] } else { 1.0 }))
            }
        }
        if ($x.Count -eq $ovRows.Count) { Get-GeoMean $x $gw } else { $null }
    })

    $sb = New-Object System.Text.StringBuilder
    $title = 'Systemvergleich ' + (($sys | ForEach-Object { $_.Label }) -join ', ')
    [void]$sb.Append(('<!doctype html><html lang="de"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>{0}</title><style>{1}</style></head><body><main>' -f (ConvertTo-HtmlText $title), (Get-ReportCss)))
    [void]$sb.Append(('<header><div><h1>Leos Minibench <span>Systemvergleich</span></h1><p class="meta">{0} Systeme aus der Vergleichsdatenbank &middot; erstellt am {1:dd.MM.yyyy HH:mm} Uhr &middot; Version {2}</p></div></header>' -f $sys.Count, (Get-Date), $ScriptVersion))

    # Systemkarten
    [void]$sb.Append('<section class="syscards">')
    for ($i = 0; $i -lt $sys.Count; $i++) {
        $s = $sys[$i]; $hw = $s.Hw
        $bf = $s.Befunde
        $dat = $s.Datum; try { $dat = [datetime]::ParseExact($s.Datum, 'yyyy-MM-dd HH:mm', $script:Inv).ToString('dd.MM.yyyy HH:mm') } catch { }
        [void]$sb.Append(('<div class="sys k{0}"><h3><i class="sw c{0}"></i>{1}</h3><p class="meta">{2}{3}</p><dl>' -f ($i % 6), (ConvertTo-HtmlText $s.Label), (ConvertTo-HtmlText $dat), $(if ($s.Messdauer -and $s.Messdauer -ne 'normal') { ' &middot; Messdauer ' + (ConvertTo-HtmlText $s.Messdauer) } else { '' })))
        $cpuDisp = Get-CpuAnzeigename $hw.CPU
        if ($hw.Kerne -and $hw.Threads) {
            $cpuDisp = '{0} ({1} Kerne, {2} Threads)' -f $cpuDisp, $hw.Kerne, $hw.Threads
        } elseif ($hw.CPU -match '(\d+)\s*Kerne,\s*(\d+)\s*Threads') {
            $cpuDisp = '{0} ({1} Kerne, {2} Threads)' -f (Get-CpuAnzeigename $hw.CPU), $Matches[1], $Matches[2]
        }
        foreach ($kv in @(@('Prozessor', $cpuDisp), @('RAM', $hw.RAM), @('Grafik', $(if ($hw.GPUGemessen -and [string]$hw.GPUGemessen -notlike ('*' + [string]$hw.GPU + '*')) { '{0} (gemessen: {1})' -f $hw.GPU, $hw.GPUGemessen } elseif ($hw.IGPU) { '{0} + {1}' -f $hw.GPU, $hw.IGPU } else { $hw.GPU })), @('System', $s.System), @('Windows', ($hw.Betriebssystem -replace '^Microsoft\s+', '')), @('Installiert', $hw.WindowsInstalliert), @('Schnellstes LW', $s.Fastest))) {
            if ($kv[1]) { [void]$sb.Append(('<dt>{0}</dt><dd>{1}</dd>' -f $kv[0], (ConvertTo-HtmlText (Get-Short ([string]$kv[1]) 70)))) }
        }
        [void]$sb.Append('</dl>')
        if ($bf) { [void]$sb.Append(('<p class="bfc"><span class="badge crit">{0} kritisch</span> <span class="badge warn">{1} {2}</span> <span class="badge info">{3} {4}</span></p>' -f [int]$bf.Kritisch, [int]$bf.Warnungen, $(if ([int]$bf.Warnungen -eq 1) { 'Warnung' } else { 'Warnungen' }), [int]$bf.Hinweise, $(if ([int]$bf.Hinweise -eq 1) { 'Hinweis' } else { 'Hinweise' }))) }
        [void]$sb.Append('</div>')
    }
    [void]$sb.Append('</section>')

    $notes = @()
    if (@($sys | ForEach-Object { $_.Messdauer } | Select-Object -Unique).Count -gt 1) { $notes += 'Die Messdauer unterscheidet sich (kurz, normal oder unbekannt bei Importen). Kurze Messungen fallen meist einige Prozent niedriger aus.' }
    if (@($sys | Where-Object { $_.Quelle -like 'Import*' }).Count) { $notes += 'Importierte Läufe älterer Versionen enthalten nicht alle Messgrößen.' }
    if ($notes.Count) { [void]$sb.Append(('<p class="note">{0}</p>' -f (ConvertTo-HtmlText ($notes -join ' ')))) }

    if ($ovRows.Count) {
        [void]$sb.Append('<section class="box"><h2>Gesamtbild</h2>')
        [void]$sb.Append(('<p class="note">Geometrisches Mittel je Bereich in Prozent der Referenz ({0}). Gezählt werden nur Messgrößen, die alle Systeme haben. Prozessor und Grafik werden im Gesamtwert höher gewichtet (2×), bei der Grafik zählt die reale Renderleistung dreifach gegenüber Durchsatzwerten. Laufwerke: jeweils das schnellste Laufwerk im Vergleich zu NVMe PCIe 4.0.</p>' -f (ConvertTo-HtmlText $refLabel)))
        foreach ($r in $ovRows) { [void]$sb.Append((New-MetricBarsHtml -Label $r.N -Unit '%' -Fmt 'N0' -LowerBetter $false -Values $r.V -Names $names -Mode 'Best' -Hint $r.Basis -Own $own)) }
        if (@($gesamt | Where-Object { $_ }).Count) { [void]$sb.Append((New-MetricBarsHtml -Label 'Gesamt' -Unit '%' -Fmt 'N0' -LowerBetter $false -Values $gesamt -Names $names -Mode 'Best' -Hint 'Gewichtetes Mittel der Bereiche (Prozessor und Grafik 2×)' -Own $own)) }
        [void]$sb.Append('</section>')
    }

    # Einzelwerte je Bereich
    $sections = [ordered]@{
        'Prozessor' = @($script:MetricDefs | Where-Object { $_.K -like 'CPU|*' })
        'Arbeitsspeicher' = @($script:MetricDefs | Where-Object { $_.K -like 'RAM|*' })
        'Grafik' = @($script:MetricDefs | Where-Object { $_.K -like 'GPU|*' })
        'Laufwerke (jeweils das schnellste)' = $lwDefs
    }
    foreach ($sn in $sections.Keys) {
        $part = New-Object System.Text.StringBuilder
        foreach ($d in $sections[$sn]) {
            $vals = @(foreach ($s in $sys) { $(if ($s.Werte[$d.K] -gt 0) { [double]$s.Werte[$d.K] } else { $null }) })
            [void]$part.Append((New-MetricBarsHtml -Label $d.N -Unit $d.U -Fmt $d.F -LowerBetter ([bool]$d.L) -Values $vals -Names $names -Mode 'Best' -Own $own))
        }
        if ($part.Length) { [void]$sb.Append(('<section class="box"><h2>{0}</h2>{1}</section>' -f (ConvertTo-HtmlText $sn), $part.ToString())) }
    }


    # alle gemessenen Laufwerke
    if (@($sys | Where-Object { $_.Laufwerke.Count }).Count) {
        [void]$sb.Append('<section class="box"><h2>Alle gemessenen Laufwerke</h2><div class="tw"><table class="disks"><thead><tr><th>System</th><th>Laufwerk</th><th>Klasse</th><th class="r">Lesen<br><small>MB/s</small></th><th class="r">Schreiben<br><small>MB/s</small></th><th class="r">4K QD1<br><small>IOPS</small></th><th class="r">4K 8 Thr.<br><small>IOPS</small></th><th class="r">4K schr.<br><small>IOPS</small></th></tr></thead><tbody>')
        $maxSR = [double](@($sys | ForEach-Object { $_.Laufwerke } | ForEach-Object { [double]$_.SR }) | Measure-Object -Maximum).Maximum
        # aufsteigend nach sequentiellem Lesen, das eigene System fett
        $rowsLw = @(for ($i = 0; $i -lt $sys.Count; $i++) { foreach ($d in $sys[$i].Laufwerke) { [pscustomobject]@{ I = $i; D = $d } } }) | Sort-Object { [double]$_.D.SR }
        foreach ($rw in $rowsLw) {
            $i = $rw.I; $d = $rw.D
            $n = { param($v) if ([double]$v -gt 0) { '{0:N0}' -f [double]$v } else { '' } }
            $bw = $(if ($maxSR -gt 0) { [math]::Max(2.0, [double]$d.SR / $maxSR * 100) } else { 0 })
            [void]$sb.Append([string]::Format($script:Inv, '<tr{10}><td class="nw"><i class="sw c{0}"></i>{1}</td><td>{2}</td><td class="muted">{3}</td><td class="num r"><span class="mini"><span class="mf c{0}" style="width:{4:0.#}%"></span></span>{5}</td><td class="num r">{6}</td><td class="num r">{7}</td><td class="num r">{8}</td><td class="num r">{9}</td></tr>',
                ($i % 6), (ConvertTo-HtmlText $sys[$i].Label), (ConvertTo-HtmlText $d.Laufwerk), (ConvertTo-HtmlText $d.Klasse), $bw, (& $n $d.SR), (& $n $d.SW), (& $n $d.R1), (& $n $d.R8), (& $n $d.W1), $(if ($i -eq $own) { ' style="font-weight:700"' } else { '' })))
        }
        [void]$sb.Append('</tbody></table></div></section>')
    }

    # Befunde je System
    $cls = @{ KRITISCH = 'crit'; WARNUNG = 'warn'; INFO = 'info' }
    [void]$sb.Append('<section class="box"><h2>Befunde der Läufe</h2>')
    for ($i = 0; $i -lt $sys.Count; $i++) {
        $s = $sys[$i]; $list = @($s.Befunde.Liste | Where-Object { $_ })
        [void]$sb.Append(('<details><summary><i class="sw c{0}"></i>{1} <span class="gref">{2} Einträge</span></summary>' -f ($i % 6), (ConvertTo-HtmlText $s.Label), $list.Count))
        if ($list.Count) {
            [void]$sb.Append('<table><tbody>')
            foreach ($l in $list) {
                $m = [regex]::Match([string]$l, '^\[(\w+)\]\s*([^:]+):\s*(.*)$')
                if ($m.Success) { $c = $cls[$m.Groups[1].Value.ToUpper()]; if (-not $c) { $c = 'info' }; [void]$sb.Append(('<tr><td><span class="badge {0}">{1}</span></td><td class="nw">{2}</td><td>{3}</td></tr>' -f $c, (ConvertTo-HtmlText $m.Groups[1].Value), (ConvertTo-HtmlText $m.Groups[2].Value), (ConvertTo-HtmlText $m.Groups[3].Value))) }
                else { [void]$sb.Append(('<tr><td colspan="3">{0}</td></tr>' -f (ConvertTo-HtmlText $l))) }
            }
            [void]$sb.Append('</tbody></table>')
        } else { [void]$sb.Append('<p class="empty">Keine Befunde gespeichert.</p>') }
        [void]$sb.Append('</details>')
    }
    [void]$sb.Append('</section>')
    [void]$sb.Append(('<footer>Leos Minibench {0}. Quellen: {1}</footer></main></body></html>' -f $ScriptVersion, (ConvertTo-HtmlText ((@($sys | ForEach-Object { Split-Path $_.Path -Leaf })) -join ', '))))

    $dir = $(if ($script:DataDir) { Join-Path $script:DataDir 'Berichte\Vergleiche' } else { Join-Path $env:TEMP 'LeosMinibench-Vergleiche' })
    New-Item -ItemType Directory -Path $dir -Force -ErrorAction Stop | Out-Null
    $file = Join-Path $dir ('Vergleich_{0}_{1}.html' -f (Get-SafeName ((@($sys | ForEach-Object { $_.Computer } | Select-Object -Unique) | Select-Object -First 4) -join '-')), (Get-Date -Format 'yyyyMMdd_HHmmss'))
    [IO.File]::WriteAllText($file, $sb.ToString(), (New-Object Text.UTF8Encoding($true)))
    Write-Host ('Vergleich gespeichert: {0}' -f $file)
    return $file
}

# Ermittelt Verlaufsdaten fÃ¼r dasselbe GerÃ¤t aus frÃ¼heren Läufen der Datenbank
function Get-DeviceRunComparison {
    param(
        [object[]]$Entries = @(),
        [string]$DeviceId = '',
        [string]$ComputerName = ''
    )
    if (-not $Entries -or -not $Entries.Count) {
        if (Get-Command 'Get-DbEntries' -ErrorAction SilentlyContinue) {
            $Entries = @(Get-DbEntries)
        }
    }
    if (-not $DeviceId -and -not $ComputerName) {
        try {
            $dev = Get-DeviceIdentity
            if ($dev) { $DeviceId = [string]$dev.Id }
        } catch { }
        $ComputerName = $env:COMPUTERNAME
    }

    $allRuns = @(if (Get-Command 'Get-DbDeviceRuns' -ErrorAction SilentlyContinue) { Get-DbDeviceRuns -Entries $Entries -DeviceId $DeviceId -ComputerName $ComputerName } else { @() })
    $nowStr = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
    if ($script:StartTime) { $nowStr = $script:StartTime.ToString('yyyy-MM-dd HH:mm:ss') }
    $earlierRuns = @($allRuns | Where-Object { $null -ne $_.Datum -and $_.Datum -lt $nowStr })

    if (-not $earlierRuns.Count) {
        return [pscustomobject]@{
            HasHistory       = $false
            EarlierCount     = 0
            TotalCount       = 1
            PrevRun          = $null
            PrevDate         = ''
            TimeSpanDays     = 0
            InventoryChanges = @()
            MetricDeltas     = @()
            SsdWear          = $null
            TrendFindings    = @()
        }
    }

    $prev = $earlierRuns[-1]
    $prevDateStr = [string]$prev.Datum
    $days = 0.0
    try {
        $pDt = [datetime]::Parse($prevDateStr)
        $cDt = $(if ($script:StartTime) { $script:StartTime } else { Get-Date })
        $days = [math]::Max(0.0, [math]::Round(($cDt - $pDt).TotalDays, 1))
    } catch { }

    # 1. Inventar-Ã„nderungen ermitteln
    $diffs = [System.Collections.Generic.List[object]]::new()
    $curBios = $(if ($script:Facts -and $script:Facts.Contains('BIOS/UEFI')) { [string]$script:Facts['BIOS/UEFI'] } else { '' })
    $prevBios = $(if ($prev.Hardware -and $prev.Hardware.BiosVersion) { [string]$prev.Hardware.BiosVersion } elseif ($prev.Hardware -and $prev.Hardware.BIOS) { [string]$prev.Hardware.BIOS } else { '' })
    if ($curBios -and $prevBios) {
        $cBVer = if ($curBios -match '([A-Za-z0-9._-]+)\s+vom') { $matches[1] } else { $curBios }
        $pBVer = if ($prevBios -match '([A-Za-z0-9._-]+)\s+vom') { $matches[1] } else { $prevBios }
        if ($cBVer -ne $pBVer) {
            $diffs.Add([pscustomobject]@{ Item = 'BIOS'; Old = $pBVer; New = $cBVer; Text = ('BIOS: {0} &rarr; <b>{1}</b>' -f $pBVer, $cBVer) })
        }
    }

    $curDrv = $(if ($script:Facts -and $script:Facts.Contains('Grafik')) { [string]$script:Facts['Grafik'] } else { '' })
    $prevDrv = $(if ($prev.Hardware -and $prev.Hardware.GpuDriver) { [string]$prev.Hardware.GpuDriver } elseif ($prev.Hardware -and $prev.Hardware.GrafikTreiber) { [string]$prev.Hardware.GrafikTreiber } else { '' })
    if ($curDrv -and $prevDrv) {
        $cDrvVer = if ($curDrv -match '(\d{2,3}\.\d{2})') { $matches[1] } else { $curDrv }
        $pDrvVer = if ($prevDrv -match '(\d{2,3}\.\d{2})') { $matches[1] } else { $prevDrv }
        if ($cDrvVer -ne $pDrvVer) {
            $diffs.Add([pscustomobject]@{ Item = 'Grafiktreiber'; Old = $pDrvVer; New = $cDrvVer; Text = ('Grafiktreiber: {0} &rarr; <b>{1}</b>' -f $pDrvVer, $cDrvVer) })
        }
    }

    $curOs = $(if ($script:Facts -and $script:Facts.Contains('Betriebssystem')) { [string]$script:Facts['Betriebssystem'] } else { '' })
    $prevOs = $(if ($prev.Hardware -and $prev.Hardware.WinBuild) { [string]$prev.Hardware.WinBuild } elseif ($prev.System) { [string]$prev.System } else { '' })
    if ($curOs -and $prevOs) {
        $cBuild = if ($curOs -match 'Build\s+(\d+)') { $matches[1] } else { '' }
        $pBuild = if ($prevOs -match '(\d{5})') { $matches[1] } else { $prevOs }
        if ($cBuild -and $pBuild -and $cBuild -ne $pBuild) {
            $diffs.Add([pscustomobject]@{ Item = 'Windows-Build'; Old = $pBuild; New = $cBuild; Text = ('Windows: Build {0} &rarr; <b>Build {1}</b>' -f $pBuild, $cBuild) })
        }
    }

    $curRam = $(if ($script:Facts -and $script:Facts.Contains('Arbeitsspeicher')) { [string]$script:Facts['Arbeitsspeicher'] } else { '' })
    $prevRam = $(if ($prev.Hardware -and $prev.Hardware.RAM) { [string]$prev.Hardware.RAM } else { '' })
    if ($curRam -and $prevRam) {
        $cSpd = if ($curRam -match '(\d{4,5}\s*MT/s)') { $matches[1] } else { '' }
        $pSpd = if ($prevRam -match '(\d{4,5}\s*MT/s)') { $matches[1] } else { '' }
        if ($cSpd -and $pSpd -and $cSpd -ne $pSpd) {
            $diffs.Add([pscustomobject]@{ Item = 'RAM-Takt'; Old = $pSpd; New = $cSpd; Text = ('RAM: {0} &rarr; <b>{1}</b>' -f $pSpd, $cSpd) })
        }
    }

    # 2. Kernmesswerte-Deltas ermitteln
    $curVals = if (Get-Command 'Get-CurrentRefValues' -ErrorAction SilentlyContinue) { Get-CurrentRefValues } else { @{} }
    $deltas = [System.Collections.Generic.List[object]]::new()
    $metricDefs = @(
        @{ Key = 'CPU|MT'; Name = 'CPU Mehrkern'; Unit = 'Pkt.'; Fmt = 'N0'; Lower = $false }
        @{ Key = 'CPU|ST'; Name = 'CPU Einzelkern'; Unit = 'Pkt.'; Fmt = 'N0'; Lower = $false }
        @{ Key = 'RAM|Lesen'; Name = 'Arbeitsspeicher Lesen'; Unit = 'GB/s'; Fmt = 'N1'; Lower = $false; AltKey = 'RAM|READ' }
        @{ Key = 'GPU|REND'; Name = 'Grafik Renderleistung'; Unit = 'FPS'; Fmt = 'N0'; Lower = $false; AltKey = 'GPU|FPS' }
        @{ Key = 'DISK|NVMe4|SR'; Name = 'SSD Sequentiell lesen'; Unit = 'MB/s'; Fmt = 'N0'; Lower = $false; AltKey = 'LW|SR' }
        @{ Key = 'DISK|NVMe4|R1'; Name = 'SSD 4K Random Read'; Unit = 'IOPS'; Fmt = 'N0'; Lower = $false; AltKey = 'LW|R1' }
    )

    foreach ($m in $metricDefs) {
        $cV = $(if ($curVals.Contains($m.Key)) { [double]$curVals[$m.Key] } elseif ($m.AltKey -and $curVals.Contains($m.AltKey)) { [double]$curVals[$m.AltKey] } else { 0.0 })
        $pV = 0.0
        if ($prev.Werte) {
            if ($prev.Werte.ContainsKey($m.Key)) { $pV = [double]$prev.Werte[$m.Key] }
            elseif ($m.AltKey -and $prev.Werte.ContainsKey($m.AltKey)) { $pV = [double]$prev.Werte[$m.AltKey] }
        }
        if ($pV -le 0 -and $prev.Messwerte) {
            if ($prev.Messwerte.ContainsKey($m.Key)) { $pV = [double]$prev.Messwerte[$m.Key] }
            elseif ($m.AltKey -and $prev.Messwerte.ContainsKey($m.AltKey)) { $pV = [double]$prev.Messwerte[$m.AltKey] }
        }
        if ($cV -gt 0 -and $pV -gt 0) {
            $diffPct = [math]::Round((($cV - $pV) / $pV) * 100.0, 1)
            $diffAbs = [math]::Round($cV - $pV, 1)
            $badgeCls = if ($diffPct -ge 3.0) { 'ok' } elseif ($diffPct -le -5.0) { 'warn' } else { 'neutral' }
            $deltas.Add([pscustomobject]@{
                Key       = $m.Key
                Name      = $m.Name
                Unit      = $m.Unit
                CurrVal   = $cV
                PrevVal   = $pV
                CurrText  = (('{0:' + $m.Fmt + '} {1}') -f $cV, $m.Unit)
                PrevText  = (('{0:' + $m.Fmt + '} {1}') -f $pV, $m.Unit)
                DiffPct   = $diffPct
                DiffAbs   = $diffAbs
                BadgeCls  = $badgeCls
                SignStr   = $(if ($diffPct -ge 0) { '+' } else { '' })
            })
        }
    }

    # 3. SSD-Verschleiß-Hochrechnung
    $ssdWearObj = $null
    $curUsed = $null
    if ($script:SmartData -and $null -ne $script:SmartData.PercentageUsed) { $curUsed = [double]$script:SmartData.PercentageUsed }
    elseif ($curVals.Contains('SSD|Wear')) { $curUsed = [double]$curVals['SSD|Wear'] }
    $prevUsed = $null
    if ($prev.Hardware -and $null -ne $prev.Hardware.SsdWearPct) { $prevUsed = [double]$prev.Hardware.SsdWearPct }
    elseif ($prev.Werte -and $prev.Werte.ContainsKey('SSD|Wear')) { $prevUsed = [double]$prev.Werte['SSD|Wear'] }

    if ($curUsed -ne $null -and $prevUsed -ne $null) {
        $dUsed = $curUsed - $prevUsed
        $projText = ''
        $estYears = $null
        if ($days -gt 0 -and $dUsed -gt 0) {
            $dailyRate = $dUsed / $days
            $remPct = [math]::Max(0.0, 100.0 - $curUsed)
            $remDays = $remPct / $dailyRate
            $estYears = [math]::Round($remDays / 365.25, 1)
            $projText = ('Geschätzte Hochrechnung basierend auf dem bisherigen Schreibaufkommen: voraussichtlich ca. {0:N1} Jahre Restlebensdauer (aktueller Verbrauch: {1:N0} %).' -f $estYears, $curUsed)
        } else {
            $projText = ('Geschätzte Hochrechnung basierend auf dem bisherigen Schreibaufkommen: minimaler Verschleiß zwischen den Läufen ({0:N0} % verbraucht, Restlebensdauer > 10 Jahre).' -f $curUsed)
        }
        $ssdWearObj = [pscustomobject]@{
            HasWearData    = $true
            CurrentUsedPct = $curUsed
            PrevUsedPct    = $prevUsed
            DaysElapsed    = $days
            EstimatedYears = $estYears
            ProjectionText = $projText
        }
    }

    # 4. Trend-Befunde (Strikte 3-Läufe-Regel: erst ab mindestens 3 Läufen)
    $totalCount = $earlierRuns.Count + 1
    $trendFindings = [System.Collections.Generic.List[object]]::new()
    if ($totalCount -ge 3) {
        $curTMax = $(if ($script:Telemetry -and $script:Telemetry.CpuTempMax) { [double]$script:Telemetry.CpuTempMax }
                     elseif ($curVals.Contains('LAST|CpuTempMax')) { [double]$curVals['LAST|CpuTempMax'] }
                     else { $null })

        $compTemps = [System.Collections.Generic.List[double]]::new()
        foreach ($r in $earlierRuns) {
            $t = $null
            if ($r.Hardware -and $r.Hardware.CpuTempMax) { $t = [double]$r.Hardware.CpuTempMax }
            elseif ($r.Werte -and $r.Werte.ContainsKey('LAST|CpuTempMax')) { $t = [double]$r.Werte['LAST|CpuTempMax'] }
            if ($null -ne $t -and $t -gt 30.0 -and $t -lt 115.0) {
                $dauer = $(if ($r.Messdauer -match '(\d+)') { [int]$matches[1] } else { 0 })
                if ($dauer -ge 60 -or -not $dauer) {
                    $compTemps.Add($t)
                }
            }
        }
        if ($curTMax -ne $null -and $compTemps.Count -ge 2) {
            $sum = 0.0; foreach ($t in $compTemps) { $sum += $t }
            $mean = $sum / $compTemps.Count
            $sqDiff = 0.0; foreach ($t in $compTemps) { $sqDiff += [math]::Pow($t - $mean, 2) }
            $stdDev = [math]::Sqrt($sqDiff / [math]::Max(1, ($compTemps.Count - 1)))
            $margin = [math]::Max(4.0, 2.0 * $stdDev)
            $deltaT = $curTMax - $mean
            if ($deltaT -ge 8.0 -and $deltaT -gt $margin) {
                $fObj = [pscustomobject]@{
                    Level = 'WARNUNG'
                    Area  = 'Kühlung'
                    Text  = ('CPU unter gleicher Last {0:N0} K wärmer als im vorherigen Verlauf: Kühlung/WÃ¤rmeleitpaste prüfen' -f [math]::Round($deltaT))
                }
                $trendFindings.Add($fObj)
                if (Get-Command 'Add-Finding' -ErrorAction SilentlyContinue) {
                    Add-Finding $fObj.Level $fObj.Area $fObj.Text
                }
            }
        }
    }

    return [pscustomobject]@{
        HasHistory       = $true
        EarlierCount     = $earlierRuns.Count
        TotalCount       = $totalCount
        PrevRun          = $prev
        PrevDate         = $prevDateStr
        TimeSpanDays     = $days
        InventoryChanges = @($diffs)
        MetricDeltas     = @($deltas)
        SsdWear          = $ssdWearObj
        TrendFindings    = @($trendFindings)
    }
}

# HTML-Abschnitt "Seit dem letzten Lauf" fÃ¼r den Diagnosebericht
function New-RunHistoryHtml {
    $comp = Get-DeviceRunComparison
    if (-not $comp -or -not $comp.HasHistory) { return '' }

    $sb = New-Object System.Text.StringBuilder
    $timeNote = if ($comp.TimeSpanDays -gt 0) { ('vor {0:N0} Tagen am {1}' -f $comp.TimeSpanDays, $comp.PrevDate) } else { ('Lauf vom {0}' -f $comp.PrevDate) }
    $compParam = [System.Uri]::EscapeDataString($env:COMPUTERNAME)

    [void]$sb.Append('<section class="box" data-section="benchmark">')
    [void]$sb.Append(('<div class="bar"><h2>Seit dem letzten Lauf ({0})</h2><a href="../Dashboard.html?mode=history&system={1}" class="tab-btn btn-dash" target="_blank" style="margin-left:auto; text-decoration:none; display:inline-flex; align-items:center; gap:4px; font-weight:600; padding:4px 10px; border-radius:4px; font-size:12px;">Geräte-Verlauf im Dashboard öffnen &rarr;</a></div>' -f
        (ConvertTo-HtmlText $timeNote), $compParam))

    if ($comp.InventoryChanges.Count) {
        [void]$sb.Append('<div style="margin:8px 0 14px; display:flex; flex-wrap:wrap; gap:8px; align-items:center;">')
        [void]$sb.Append('<span style="font-size:12px; font-weight:600; color:var(--text-muted, #6B7280);">Geändert:</span>')
        foreach ($d in $comp.InventoryChanges) {
            [void]$sb.Append(('<span class="badge info" style="font-size:11px; text-transform:none; font-weight:normal; padding:3px 8px; border:1px solid rgba(0,120,212,0.3);">{0}</span>' -f $d.Text))
        }
        [void]$sb.Append('</div>')
    }

    if ($comp.MetricDeltas.Count) {
        [void]$sb.Append('<div class="tw"><table><thead><tr><th>Kernmesswert</th><th class="r">Voriger Lauf</th><th class="r">Aktueller Lauf</th><th>Abweichung</th></tr></thead><tbody>')
        foreach ($m in $comp.MetricDeltas) {
            $badgeCls = switch ($m.BadgeCls) { 'ok' { 'badge ok' } 'warn' { 'badge warn' } default { 'badge neutral' } }
            [void]$sb.Append(('<tr><td><b>{0}</b></td><td class="num r">{1}</td><td class="num r"><b>{2}</b></td><td><span class="{3}">{4}{5:N1} %</span></td></tr>' -f
                (ConvertTo-HtmlText $m.Name), (ConvertTo-HtmlText $m.PrevText), (ConvertTo-HtmlText $m.CurrText), $badgeCls, $m.SignStr, $m.DiffPct))
        }
        [void]$sb.Append('</tbody></table></div>')
    }

    if ($comp.SsdWear -and $comp.SsdWear.ProjectionText) {
        [void]$sb.Append(('<p class="note tight" style="margin-top:10px;"><em>{0}</em></p>' -f (ConvertTo-HtmlText $comp.SsdWear.ProjectionText)))
    }

    [void]$sb.Append('</section>')
    return $sb.ToString()
}
#endregion
