# Veröffentlicht die aktuelle Version in einem Schritt (ab v4.03, ab v4.04 mit Push und Einrichtung von gh):
#   1. prüft, dass alle Änderungen committet sind und Aktueller Build zur Version passt,
#   2. überträgt main nach GitHub, legt den Tag vX.Y an (falls nötig) und überträgt ihn,
#   3. installiert bei Bedarf die GitHub CLI (winget) und meldet sie an (Browser, einmalig),
#   4. erstellt oder aktualisiert das GitHub-Release mit Aktueller Build\LMB.exe und der Zeile
#      "SHA-256 (LMB.exe): <Prüfsumme>". Beides braucht die Update-Funktion von Leos Minibench.
# Aufruf im Projektordner: powershell -ExecutionPolicy Bypass -File .\Veroeffentlichen.ps1 [-Entwurf] [-OhnePush]
param([switch]$Entwurf, [switch]$OhnePush)
# Continue statt Stop: Windows PowerShell 5.1 macht aus Ausgaben von git und gh auf stderr sonst Abbrüche.
# Fehler werden über die Rückgabecodes geprüft.
$ErrorActionPreference = 'Continue'
$root = $PSScriptRoot
$repo = 'miniLeonardo/MiniBench'
function Schritt([string]$t) { Write-Host ('  ' + $t) -ForegroundColor Cyan }

. (Join-Path $root 'src\Kern\Version.ps1')
$tag = 'v' + $ScriptVersion
$exe = Join-Path $root 'Aktueller Build\LMB.exe'
Write-Host ''
Write-Host ('Leos Minibench {0} veröffentlichen' -f $ScriptVersion) -ForegroundColor White

# 1. Stand prüfen
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw 'git fehlt.' }
if (-not (Test-Path -LiteralPath $exe)) { throw 'Aktueller Build\LMB.exe fehlt. Zuerst bauen (Bauen.cmd).' }
$stand = Join-Path $root 'Aktueller Build\Stand.txt'
$sv = $(if (Test-Path -LiteralPath $stand) { [regex]::Match([IO.File]::ReadAllText($stand, [Text.Encoding]::UTF8), 'Version\s*:\s*([0-9.]+)').Groups[1].Value } else { '' })
if ($sv -ne $ScriptVersion) { throw ('Aktueller Build ist laut Stand.txt Version {0}, src\Kern\Version.ps1 nennt {1}. Zuerst neu bauen.' -f $sv, $ScriptVersion) }
$offen = @(& git -C $root status --porcelain --untracked-files=no)
if ($offen.Count) { $offen | ForEach-Object { Write-Host ('    ' + $_) }; throw 'Es gibt nicht committete Änderungen (siehe oben). Zuerst committen, z. B. mit Bauen.cmd.' }
Schritt ('Stand geprüft: {0}, Aktueller Build passt' -f $tag)

# 2. main und Tag übertragen
if (-not $OhnePush) {
    & git -C $root push origin HEAD:main
    if ($LASTEXITCODE -ne 0) { throw 'git push origin main ist fehlgeschlagen (siehe oben).' }
    & git -C $root rev-parse -q --verify ('refs/tags/' + $tag) *> $null
    if ($LASTEXITCODE -ne 0) { & git -C $root tag $tag; Schritt ('Tag {0} angelegt' -f $tag) }
    & git -C $root push origin $tag
    if ($LASTEXITCODE -ne 0) { throw ('git push origin {0} ist fehlgeschlagen (siehe oben).' -f $tag) }
    Schritt 'main und Tag auf GitHub'
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
$sha = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToLowerInvariant()
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
