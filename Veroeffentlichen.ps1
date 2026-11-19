# Veröffentlicht die aktuelle Version in einem Schritt (ab v4.03, ab v4.04 mit Push und Einrichtung von gh):
#   1. prüft, dass alle Änderungen committet sind und Aktueller Build zur Version passt; ab v4.11 außerdem vor jedem
#      Push: Zweig main, LMB.exe passt zur Prüfsumme in Stand.txt, ein vorhandener Tag vX.Y zeigt auf HEAD und
#      Stand.txt nennt einen Testlauf unter Windows ohne Fehlschläge (überspringbar mit -OhneWindowsTests),
#   2. überträgt main nach GitHub, legt den Tag vX.Y an (falls nötig) und überträgt ihn; ab v4.11 auch ältere
#      Release-Tags vX.Y, die auf GitHub fehlen, sofern sie auf einen Commit von main zeigen,
#   3. installiert bei Bedarf die GitHub CLI (winget) und meldet sie an (Browser, einmalig),
#   4. erstellt oder aktualisiert das GitHub-Release mit Aktueller Build\LMB.exe und der Zeile
#      "SHA-256 (LMB.exe): <Prüfsumme>". Beides braucht die Update-Funktion von Leos Minibench.
# Aufruf im Projektordner: powershell -ExecutionPolicy Bypass -File .\Veroeffentlichen.ps1 [-Entwurf] [-OhnePush] [-OhneWindowsTests]
param([switch]$Entwurf, [switch]$OhnePush, [switch]$OhneWindowsTests)
# Continue statt Stop: Windows PowerShell 5.1 macht aus Ausgaben von git und gh auf stderr sonst Abbrüche.
# Fehler werden über die Rückgabecodes geprüft.
$ErrorActionPreference = 'Continue'
$root = $PSScriptRoot
$repo = 'miniLeonardo/MiniBench'
function Schritt([string]$t) { Write-Host ('  ' + $t) -ForegroundColor Cyan }
# Prüfsumme von LMB.exe aus Stand.txt (Zeile "  LMB.exe   2,817 KB  <sha256>", geschrieben von Bauen.cmd); leer, wenn sie fehlt.
function Get-StandLmbSha([string]$StandText) {
    $m = [regex]::Match([string]$StandText, '(?m)^\s*LMB\.exe\s+[\d.,]+\s*KB\s+([0-9a-fA-F]{64})\s*$')
    if ($m.Success) { return $m.Groups[1].Value.ToLowerInvariant() }
    return ''
}
# Stand.txt nennt einen Testlauf von Bauen.cmd unter Windows ohne Fehlschläge: Zeile aus tests\Testen.ps1
# ("Tests     : 1234 bestanden, 0 fehlgeschlagen, 5 übersprungen (Pester 6.2.0, PowerShell 5.1.x)"). Nicht geladene
# Testdateien oder fehlgeschlagene Blöcke stehen zwischen "übersprungen" und der Klammer und passen deshalb nicht.
function Test-StandWindowsTests([string]$StandText) {
    return ([string]$StandText -match ('(?m)^Tests\s*:\s*[1-9]\d* bestanden, 0 fehlgeschlagen, \d+ ' + [char]0x00FC + 'bersprungen \(Pester \d+(\.\d+)*, PowerShell \d+(\.\d+)*\)'))
}

. (Join-Path $root 'src\Kern\Version.ps1')
$tag = 'v' + $ScriptVersion
$exe = Join-Path $root 'Aktueller Build\LMB.exe'
Write-Host ''
Write-Host ('Leos Minibench {0} veröffentlichen' -f $ScriptVersion) -ForegroundColor White

# 1. Stand prüfen (alles vor dem ersten Push; jede Prüfung bricht mit einer Meldung ab)
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw 'git fehlt.' }
if (-not (Test-Path -LiteralPath $exe)) { throw 'Aktueller Build\LMB.exe fehlt. Zuerst bauen (Bauen.cmd).' }
$stand = Join-Path $root 'Aktueller Build\Stand.txt'
$standText = $(if (Test-Path -LiteralPath $stand) { [IO.File]::ReadAllText($stand, [Text.Encoding]::UTF8) } else { '' })
$sv = [regex]::Match($standText, 'Version\s*:\s*([0-9.]+)').Groups[1].Value
if ($sv -ne $ScriptVersion) { throw ('Aktueller Build ist laut Stand.txt Version {0}, src\Kern\Version.ps1 nennt {1}. Zuerst neu bauen.' -f $sv, $ScriptVersion) }
# (a) nur vom Zweig main
$zweig = [string](& git -C $root rev-parse --abbrev-ref HEAD)
if ($zweig.Trim() -ne 'main') { throw ('Veröffentlicht wird nur vom Zweig main, ausgecheckt ist "{0}". Zuerst "git switch main".' -f $zweig.Trim()) }
$offen = @(& git -C $root status --porcelain --untracked-files=no)
if ($offen.Count) { $offen | ForEach-Object { Write-Host ('    ' + $_) }; throw 'Es gibt nicht committete Änderungen (siehe oben). Zuerst committen, z. B. mit Bauen.cmd.' }
# (b) LMB.exe ist die gebaute Datei, nicht eine ältere exe mit passender Stand.txt
$sha = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToLowerInvariant()
$standSha = Get-StandLmbSha $standText
if (-not $standSha) { throw 'Stand.txt nennt keine Prüfsumme für LMB.exe. Zuerst neu bauen (Bauen.cmd).' }
if ($standSha -ne $sha) { throw ('Aktueller Build\LMB.exe passt nicht zu Stand.txt (Datei {0}, Stand.txt {1}). Die exe stammt nicht aus dem letzten Bau; neu bauen (Bauen.cmd).' -f $sha, $standSha) }
# (c) ein schon vorhandener Tag vX.Y muss auf HEAD zeigen
$head = [string](& git -C $root rev-parse HEAD)
$tagCommit = [string](& git -C $root rev-parse -q --verify ($tag + '^{commit}') 2>$null)
if ($LASTEXITCODE -eq 0 -and $tagCommit -and $tagCommit.Trim() -ne $head.Trim()) {
    throw ('Der Tag {0} zeigt auf {1}, HEAD ist {2}. Der Tag gehört zu einem anderen Stand; prüfen und ggf. mit "git tag -d {0}" entfernen.' -f $tag, $tagCommit.Trim().Substring(0, 7), $head.Trim().Substring(0, 7))
}
# (d) Stand.txt belegt einen Testlauf unter Windows ohne Fehlschläge
if (Test-StandWindowsTests $standText) {
    Schritt 'Stand.txt nennt einen Testlauf unter Windows ohne Fehlschläge'
} elseif ($OhneWindowsTests) {
    Write-Host '  Hinweis: Stand.txt nennt keinen Testlauf unter Windows ohne Fehlschläge; übersprungen (-OhneWindowsTests).' -ForegroundColor Yellow
} else {
    $tz = [regex]::Match($standText, '(?m)^Tests\s*:\s*([^\r\n]*)').Groups[1].Value
    throw ('Stand.txt nennt keinen Testlauf unter Windows ohne Fehlschläge (Tests: {0}). Mit Bauen.cmd unter Windows bauen oder bewusst -OhneWindowsTests angeben.' -f $(if ($tz) { $tz } else { 'keine Angabe' }))
}
Schritt ('Stand geprüft: {0}, Zweig main, Aktueller Build passt (SHA-256 wie in Stand.txt)' -f $tag)

# 2. main und Tag übertragen
if (-not $OhnePush) {
    & git -C $root push origin HEAD:main
    if ($LASTEXITCODE -ne 0) { throw 'git push origin main ist fehlgeschlagen (siehe oben).' }
    & git -C $root rev-parse -q --verify ('refs/tags/' + $tag) *> $null
    if ($LASTEXITCODE -ne 0) { & git -C $root tag $tag; Schritt ('Tag {0} angelegt' -f $tag) }
    & git -C $root push origin $tag
    if ($LASTEXITCODE -ne 0) { throw ('git push origin {0} ist fehlgeschlagen (siehe oben).' -f $tag) }
    Schritt 'main und Tag auf GitHub'
    # ab v4.11: lokale Release-Tags vX.Y, die auf GitHub fehlen (z. B. v4.08), mit übertragen. Nur Tags auf Commits
    # von main: main ist eben übertragen, der Tag macht also nichts Neues öffentlich. Andere werden nur gemeldet.
    $remoteTags = @(& git -C $root ls-remote --tags origin 2>$null | ForEach-Object { ([string]$_ -split '\s+')[1] -replace '^refs/tags/', '' -replace '\^\{\}$', '' })
    if ($LASTEXITCODE -eq 0) {
        $fehlend = @(& git -C $root tag --list 'v*' | Where-Object { $_ -match '^v\d+\.\d+$' -and $remoteTags -notcontains $_ })
        $mitMain = @(); $ohneMain = @()
        foreach ($ft in $fehlend) {
            & git -C $root merge-base --is-ancestor $ft HEAD *> $null
            if ($LASTEXITCODE -eq 0) { $mitMain += $ft } else { $ohneMain += $ft }
        }
        if ($mitMain.Count) {
            & git -C $root push origin @($mitMain)
            if ($LASTEXITCODE -eq 0) { Schritt ('fehlende Tags übertragen: {0}' -f ($mitMain -join ', ')) }
            else { Write-Host ('  Hinweis: fehlende Tags {0} ließen sich nicht übertragen (siehe oben).' -f ($mitMain -join ', ')) -ForegroundColor Yellow }
        }
        if ($ohneMain.Count) { Write-Host ('  Hinweis: lokale Tags ohne Commit auf main nicht übertragen: {0}' -f ($ohneMain -join ', ')) -ForegroundColor Yellow }
    } else {
        Write-Host '  Hinweis: Tags auf GitHub nicht abfragbar, fehlende ältere Tags nicht geprüft.' -ForegroundColor Yellow
    }
}

# 3. GitHub CLI einrichten
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) { throw 'GitHub CLI fehlt und winget ist nicht vorhanden. GitHub CLI von https://cli.github.com installieren.' }
    Schritt 'GitHub CLI wird installiert (winget) ...'
    & winget install --id GitHub.cli -e --accept-source-agreements --accept-package-agreements
    foreach ($p in @("$env:ProgramFiles\GitHub CLI", "${env:LOCALAPPDATA}\Programs\GitHub CLI")) { if (Test-Path -LiteralPath (Join-Path $p 'gh.exe')) { $env:Path = $p + ';' + $env:Path } }
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw 'GitHub CLI wurde installiert, ist aber in diesem Fenster noch nicht bekannt. PowerShell neu öffnen und das Skript erneut starten.' }
}
& gh auth status --hostname github.com *> $null
if ($LASTEXITCODE -ne 0) {
    Schritt 'Anmeldung bei GitHub: Der Browser öffnet sich, dort den angezeigten Code bestätigen.'
    & gh auth login --hostname github.com --git-protocol https --web
    if ($LASTEXITCODE -ne 0) { throw 'Anmeldung bei GitHub fehlgeschlagen.' }
}
Schritt 'GitHub CLI angemeldet'

# 4. Release
$vcs = [IO.File]::ReadAllText((Join-Path $root 'src\Oberflaeche\Versionen.cs'), [Text.Encoding]::UTF8)
$titel = [regex]::Match($vcs, ('new Eintrag\("{0}", "[^"]*", "([^"]+)"' -f [regex]::Escape($ScriptVersion))).Groups[1].Value
$cl = [IO.File]::ReadAllText((Join-Path $root 'CHANGELOG.md'), [Text.Encoding]::UTF8)
$m = [regex]::Match($cl, ('(?ms)^## {0} [^\r\n]*\r?\n(.*?)(?=^## v|\z)' -f [regex]::Escape($tag)))
$text = $(if ($m.Success) { $m.Groups[1].Value.Trim() } else { 'Änderungen: Doku\Änderungen_' + $tag + '.txt' })
$notes = $text + "`n`n**Download:** LMB.exe herunterladen und starten (Administratorrechte). Ältere Versionen aktualisieren sich über den Hinweis oben rechts im Programm.`n`nSHA-256 (LMB.exe): " + $sha + "`n"
$nf = Join-Path ([IO.Path]::GetTempPath()) ('Release_' + $tag + '.md')
[IO.File]::WriteAllText($nf, $notes, (New-Object Text.UTF8Encoding($false)))
$name = $(if ($titel) { '{0}: {1}' -f $tag, $titel } else { $tag })
# ab v4.06: Prüfsumme zusätzlich als Anhang LMB.exe.sha256. Die Update-Prüfung liest sie, wenn die GitHub-API nicht
# antwortet (Grenze von 60 Abfragen je Stunde) oder der Text des Releases keine SHA-256-Zeile enthält.
$shaDir = Join-Path ([IO.Path]::GetTempPath()) ('Minibench_' + $tag)
New-Item -ItemType Directory -Path $shaDir -Force | Out-Null
$shaFile = Join-Path $shaDir 'LMB.exe.sha256'
[IO.File]::WriteAllText($shaFile, ('{0}  LMB.exe' -f $sha) + "`n", (New-Object Text.UTF8Encoding($false)))

& gh release view $tag --repo $repo *> $null
if ($LASTEXITCODE -eq 0) {
    & gh release upload $tag $exe $shaFile --repo $repo --clobber
    if ($LASTEXITCODE -eq 0) { & gh release edit $tag --repo $repo --title $name --notes-file $nf --latest }
} else {
    $a = @('release', 'create', $tag, $exe, $shaFile, '--repo', $repo, '--title', $name, '--notes-file', $nf, '--latest', '--verify-tag')
    if ($Entwurf) { $a += '--draft' }
    & gh @a
}
if ($LASTEXITCODE -ne 0) { throw 'gh hat einen Fehler gemeldet (siehe oben).' }
Remove-Item -LiteralPath $nf -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $shaDir -Recurse -Force -ErrorAction SilentlyContinue
Write-Host ('Release {0} veröffentlicht: https://github.com/{1}/releases/tag/{0}' -f $tag, $repo) -ForegroundColor Green
Write-Host ('SHA-256 (LMB.exe): {0}' -f $sha)
