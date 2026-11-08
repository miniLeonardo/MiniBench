# Erstellt oder aktualisiert das GitHub-Release der aktuellen Version (ab v4.03).
# Die Update-Funktion von Leos Minibench liest aus dem neuesten Release die Datei LMB.exe und die Zeile
# "SHA-256 (LMB.exe): <Prüfsumme>"; ohne beides kann sie nicht aktualisieren.
#
# Voraussetzungen: GitHub CLI angemeldet (einmalig: winget install GitHub.cli, danach gh auth login),
# Tag vX.Y ist nach GitHub übertragen (git push origin vX.Y), Aktueller Build\LMB.exe gehört zu dieser Version.
# Aufruf im Projektordner: powershell -ExecutionPolicy Bypass -File .\Veroeffentlichen.ps1 [-Entwurf]
param([switch]$Entwurf)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$repo = 'miniLeonardo/MiniBench'

. (Join-Path $root 'src\Kern\Version.ps1')
$tag = 'v' + $ScriptVersion
$exe = Join-Path $root 'Aktueller Build\LMB.exe'

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw 'GitHub CLI (gh) fehlt. Einmalig: winget install GitHub.cli, danach gh auth login.' }
if (-not (Test-Path -LiteralPath $exe)) { throw 'Aktueller Build\LMB.exe fehlt. Zuerst bauen (Bauen.cmd).' }
$stand = Join-Path $root 'Aktueller Build\Stand.txt'
$sv = $(if (Test-Path -LiteralPath $stand) { [regex]::Match([IO.File]::ReadAllText($stand, [Text.Encoding]::UTF8), 'Version\s*:\s*([0-9.]+)').Groups[1].Value } else { '' })
if ($sv -ne $ScriptVersion) { throw ('Aktueller Build ist laut Stand.txt Version {0}, src\Kern\Version.ps1 nennt {1}. Zuerst neu bauen.' -f $sv, $ScriptVersion) }
& git -C $root ls-remote --exit-code --tags origin $tag *> $null
if ($LASTEXITCODE -ne 0) { throw ('Tag {0} ist noch nicht auf GitHub: git tag {0}; git push origin {0}' -f $tag) }

$sha = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToLowerInvariant()

# Titel aus Versionen.cs, Text aus dem Abschnitt der Version in CHANGELOG.md
$vcs = [IO.File]::ReadAllText((Join-Path $root 'src\Oberflaeche\Versionen.cs'), [Text.Encoding]::UTF8)
$titel = [regex]::Match($vcs, ('new Eintrag\("{0}", "[^"]*", "([^"]+)"' -f [regex]::Escape($ScriptVersion))).Groups[1].Value
$cl = [IO.File]::ReadAllText((Join-Path $root 'CHANGELOG.md'), [Text.Encoding]::UTF8)
$m = [regex]::Match($cl, ('(?ms)^## {0} [^\r\n]*\r?\n(.*?)(?=^## v|\z)' -f [regex]::Escape($tag)))
$text = $(if ($m.Success) { $m.Groups[1].Value.Trim() } else { 'Änderungen: Doku\Änderungen_' + $tag + '.txt' })
$notes = $text + "`n`n**Download:** LMB.exe herunterladen und starten (Administratorrechte). Ältere Versionen aktualisieren sich über den Hinweis oben rechts im Programm.`n`nSHA-256 (LMB.exe): " + $sha + "`n"
$nf = Join-Path ([IO.Path]::GetTempPath()) ('Release_' + $tag + '.md')
[IO.File]::WriteAllText($nf, $notes, (New-Object Text.UTF8Encoding($false)))
$name = $(if ($titel) { '{0}: {1}' -f $tag, $titel } else { $tag })

& gh release view $tag --repo $repo *> $null
if ($LASTEXITCODE -eq 0) {
    & gh release upload $tag $exe --repo $repo --clobber
    & gh release edit $tag --repo $repo --title $name --notes-file $nf --latest
} else {
    $a = @('release', 'create', $tag, $exe, '--repo', $repo, '--title', $name, '--notes-file', $nf, '--latest')
    if ($Entwurf) { $a += '--draft' }
    & gh @a
}
if ($LASTEXITCODE -ne 0) { throw 'gh hat einen Fehler gemeldet (siehe oben).' }
Remove-Item -LiteralPath $nf -Force -ErrorAction SilentlyContinue
Write-Host ('Release {0} veröffentlicht, SHA-256 (LMB.exe): {1}' -f $tag, $sha) -ForegroundColor Green
