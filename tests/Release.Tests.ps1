# Release-Metadaten: Die Version in src/Kern/Version.ps1 steht überall oben (Versionen.cs, Doku/Versionshistorie.txt,
# CHANGELOG.md), zu jeder Version gibt es eine Änderungsdatei, und Bauen.cmd committet ohne fest eingetragene Versionen.
# Ersetzt die früheren Versionsprüfungen je Release (VersionXX.Tests.ps1).

BeforeAll {
    . (Join-Path $PSScriptRoot 'Hilfen.ps1')
    $script:Ver = Get-MinibenchVersion
    $script:VersionenCs = Get-SrcText 'Oberflaeche/Versionen.cs'
    $script:Historie = Get-RepoText 'Doku/Versionshistorie.txt'
    $script:Changelog = Get-RepoText 'CHANGELOG.md'
    $script:Bauen = Get-RepoText 'Bauen.cmd'
    $script:Eintraege = @([regex]::Matches($script:VersionenCs, 'new Eintrag\("([^"]+)", "([^"]*)", "([^"]+)"') | ForEach-Object {
            [pscustomobject]@{ Version = $_.Groups[1].Value; Datum = $_.Groups[2].Value; Titel = $_.Groups[3].Value } })
}

Describe 'Aktuelle Version überall gleich' {
    It 'Version.ps1 nennt eine gültige Versionsnummer' {
        $script:Ver | Should -Match '^\d+\.\d+$'
        Get-SrcText 'Kern/Version.ps1' | Should -Match ('\$ScriptVersion\s*=\s*''' + [regex]::Escape($script:Ver) + '''')
    }
    It 'Versionen.cs führt die aktuelle Version als ersten Eintrag, mit Datum und Titel' {
        $script:Eintraege[0].Version | Should -Be $script:Ver
        $script:Eintraege[0].Datum | Should -Match '^\d{2}\.\d{2}\.\d{4}$'
        $script:Eintraege[0].Titel.Length | Should -BeGreaterThan 10
    }
    It 'Doku/Versionshistorie.txt führt die aktuelle Version als ersten Eintrag' {
        [regex]::Match($script:Historie, '(?m)^VERSION\s+([0-9.]+)\s+\(').Groups[1].Value | Should -Be $script:Ver
    }
    It 'CHANGELOG.md führt die aktuelle Version als ersten Eintrag' {
        [regex]::Match($script:Changelog, '(?m)^##\s+v([0-9.]+)\s+\(').Groups[1].Value | Should -Be $script:Ver
    }
    It 'Doku/Änderungen_v<Version>.txt existiert als UTF-8 mit BOM' {
        $p = Join-Path $global:MinibenchRepoRoot (('Doku/' + [char]0x00C4 + 'nderungen_v{0}.txt') -f $script:Ver)
        Test-Path -LiteralPath $p | Should -BeTrue
        $b = [IO.File]::ReadAllBytes($p)
        $b.Length | Should -BeGreaterThan 100
        ($b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF) | Should -BeTrue
    }
}

Describe 'Versionshistorie vollständig' {
    It 'jede Version mit Änderungsdatei hat einen Eintrag in Versionen.cs' {
        $vs = @($script:Eintraege | ForEach-Object { $_.Version })
        foreach ($f in @(Get-ChildItem (Join-Path $global:MinibenchRepoRoot 'Doku') -Filter '*nderungen_v*.txt')) {
            $vs | Should -Contain ($f.BaseName -replace '^.nderungen_v', '') -Because $f.Name
        }
    }
    It 'reicht als Einzelversionen bis 1.0 zurück' {
        $vs = @($script:Eintraege | ForEach-Object { $_.Version })
        foreach ($n in 0..8) { $vs | Should -Contain ('1.{0}' -f $n) }
        $vs | Should -Not -Contain '1.0 bis 1.8'
    }
    It 'Doku/Versionshistorie.txt und CHANGELOG.md enthalten jede Version aus Versionen.cs genau einmal' {
        foreach ($e in $script:Eintraege) {
            ([regex]::Matches($script:Historie, ('(?m)^VERSION {0}\s' -f [regex]::Escape($e.Version)))).Count | Should -Be 1 -Because ('Versionshistorie.txt, ' + $e.Version)
            if ($e.Version -match '^\d+\.\d+$') {
                ([regex]::Matches($script:Changelog, ('(?m)^## v{0}\s' -f [regex]::Escape($e.Version)))).Count | Should -Be 1 -Because ('CHANGELOG.md, ' + $e.Version)
            }
        }
    }
    It 'Einträge stehen absteigend nach Version' {
        $zahlen = @($script:Eintraege | Where-Object { $_.Version -match '^\d+\.\d+$' } | ForEach-Object { [double]::Parse($_.Version, [Globalization.CultureInfo]::InvariantCulture) })
        for ($i = 1; $i -lt $zahlen.Count; $i++) { $zahlen[$i] | Should -BeLessThan $zahlen[$i - 1] }
    }
}

Describe 'Bauen.cmd und Git' {
    It 'nennt keine Versionsnummer fest (Commit-Nachricht kommt aus Versionen.cs)' {
        $script:Bauen | Should -Not -Match '\$ver -eq ''\d'
        $script:Bauen | Should -Match 'new Eintrag\\\("\{0\}"'
        $script:Bauen | Should -Match "'Release v\{0\}: \{1\}'"
    }
    It 'committet nur nach bestandenen Tests und prüft das Ergebnis von git commit' {
        $script:Bauen | Should -Match '\$testsOk = \$false'
        $script:Bauen | Should -Match 'else \{ \$testsOk = \$true;'
        $script:Bauen | Should -Match '-and -not \$testsOk\)'
        $script:Bauen | Should -Match '(?s)git\.exe -C \$Here commit -m \$commitMsg 2>&1\s+if \(\$LASTEXITCODE -eq 0\)'
    }
    It 'beginnt ohne BOM (cmd.exe liest die BOM als Zeichen der ersten Zeile)' {
        $b = [IO.File]::ReadAllBytes((Join-Path $global:MinibenchRepoRoot 'Bauen.cmd'))
        ($b[0] -eq 0xEF -and $b[1] -eq 0xBB) | Should -BeFalse
        [char]$b[0] | Should -Be '<'
    }
    It 'schreibt Gesamtläufe richtig kodiert und nummeriert die Schritte 1 bis 7 je einmal' {
        $script:Bauen | Should -Not -Match ([string][char]0x00C3)
        $script:Bauen | Should -Match 'Gesamtläufe: \{1\}'
        $nr = @([regex]::Matches($script:Bauen, '(?m)^    # (\d)\. ') | ForEach-Object { [int]$_.Groups[1].Value })
        ($nr -join ',') | Should -Be '1,2,3,4,5,6,7'
    }
    It 'nimmt alle getrackten Änderungen und neue Dateien nur aus festen Pfaden auf, ohne Push' {
        $script:Bauen | Should -Match 'git\.exe -C \$Here add -u -- \.'
        $script:Bauen | Should -Match "'Aktueller Build/Stand\.txt', 'Aktueller Build/LeosMinibench\.ps1'"
        $script:Bauen | Should -Match "'\.gitattributes'"
        # nicht der ganze Aktuelle Build (Minibench-Daten, Logs) und kein automatischer Push
        $script:Bauen | Should -Not -Match "'Aktueller Build',"
        $script:Bauen | Should -Not -Match '& git\.exe [^\r\n]*\bpush\b'
        $script:Bauen | Should -Match 'Veroeffentlichen\.ps1'
    }
    It 'bricht bei Abbruch des Testlaufs (Rückgabe 97) ab und committet nicht' {
        $script:Bauen | Should -Match 'elseif \(\$tc -eq 97\) \{ throw'
    }
}

Describe 'Testen.ps1: Rückgabe aus allen Fehlerarten (ab v4.11)' {
    BeforeAll { $script:Testen = Get-RepoText 'tests/Testen.ps1' }
    It 'zählt fehlgeschlagene Tests, nicht geladene Testdateien und Blöcke und wertet einen Abbruch als 97' {
        $script:Testen | Should -Match '\$nTests = \[int\]\$res\.FailedCount'
        $script:Testen | Should -Match '\$nDateien = \[int\]\$res\.FailedContainersCount'
        $script:Testen | Should -Match '\$nBloecke = \[int\]\$res\.FailedBlocksCount'
        $script:Testen | Should -Match '\$fehler = \$nTests \+ \$nDateien \+ \$nBloecke'
        $script:Testen | Should -Match '(?s)try \{\s+\$res = @\(Invoke-Pester -Configuration \$cfg\)'
        $script:Testen | Should -Match 'exit 97'
        $script:Testen | Should -Match 'exit \[math\]::Min\(96, \$fehler\)'
        $script:Testen | Should -Not -Match 'exit \[math\]::Min\(\d+, \[int\]\$res\.FailedCount\)'
    }
    It 'eine Testdatei, die nicht lädt, ergibt eine Rückgabe ungleich 0 und steht in der Zusammenfassung' {
        if (-not (Test-IstWindows)) { Set-ItResult -Skipped -Because 'braucht powershell.exe'; return }
        $tmp = Join-Path ([IO.Path]::GetTempPath()) ('MinibenchTestenRueckgabe_' + $PID)
        New-Item -ItemType Directory -Path $tmp -Force | Out-Null
        try {
            $kaputt = Join-Path $tmp 'Kaputt.Tests.ps1'
            [IO.File]::WriteAllText($kaputt, "throw 'Ladefehler'`r`nDescribe 'x' { It 'y' { 1 | Should -Be 1 } }`r`n")
            $sum = Join-Path $tmp 'Zusammenfassung.txt'
            & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $global:MinibenchRepoRoot 'tests\Testen.ps1') -Kurz -Datei $kaputt -Zusammenfassung $sum *> $null
            $rc = $LASTEXITCODE
            $rc | Should -Not -Be 0
            $rc | Should -Not -Be 99 -Because 'Pester 5 oder neuer ist hier installiert'
            [IO.File]::ReadAllText($sum, [Text.Encoding]::UTF8) | Should -Match '1 Testdateien nicht geladen'
        } finally { Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue }
    }
}

Describe 'Veroeffentlichen.ps1 prüft vor jedem Push (ab v4.11)' {
    BeforeAll {
        $script:Veroeff = Get-RepoText 'Veroeffentlichen.ps1'
        $ast = [Management.Automation.Language.Parser]::ParseInput($script:Veroeff, [ref]$null, [ref]$null)
        foreach ($fn in $ast.FindAll({ param($n) $n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -in 'Get-StandSha', 'Get-StandLmbSha', 'Get-ExeSkriptSha', 'Test-StandWindowsTests' }, $true)) {
            . ([scriptblock]::Create($fn.Extent.Text))
        }
        $script:ErsterPush = $script:Veroeff.IndexOf('push origin HEAD:main')
        $script:Ue = [string][char]0x00FC
    }
    It 'prüft Zweig, Prüfsumme, Tag und Testzeile vor dem ersten Push' {
        $script:ErsterPush | Should -BeGreaterThan 0
        foreach ($p in @('rev-parse --abbrev-ref HEAD', 'Get-StandLmbSha $standText', "Get-StandSha `$standText 'LeosMinibench.ps1'", 'Get-ExeSkriptSha $exe', "(`$tag + '^{commit}')", 'Test-StandWindowsTests $standText')) {
            $i = $script:Veroeff.IndexOf($p)
            $i | Should -BeGreaterThan 0 -Because $p
            $i | Should -BeLessThan $script:ErsterPush -Because $p
        }
        $script:Veroeff | Should -Match "-ne 'main'\) \{ throw"
        $script:Veroeff | Should -Match '\$standSha -ne \$sha\) \{ throw'
        $script:Veroeff | Should -Match '\$ps1Sha -ne \$standPs1\) \{ throw'
        $script:Veroeff | Should -Match '\$exeSkript -ne \$standPs1\) \{ throw'
        $script:Veroeff | Should -Match '\$tagCommit\.Trim\(\) -ne \$head\.Trim\(\)\) \{'
        $script:Veroeff | Should -Match 'param\(\[switch\]\$Entwurf, \[switch\]\$OhnePush, \[switch\]\$OhneWindowsTests\)'
    }
    It 'liest die Prüfsumme von LMB.exe aus Stand.txt' {
        $h = 'A30D462A8CA20E1C8892FB6FC1E270E36D6B6E6FD6DA0AF7242A8AA63DBFC878'
        Get-StandLmbSha ("Dateien`r`n  LMB.exe                  2,817 KB  $h`r`n  LeosMinibench.ps1        2,741 KB  " + ('0' * 64) + "`r`n") | Should -Be $h.ToLowerInvariant()
        Get-StandLmbSha ("  LMB.exe                  2.817 KB  $h") | Should -Be $h.ToLowerInvariant()
        Get-StandLmbSha "  LeosMinibench.ps1   1 KB  $h" | Should -Be ''
        Get-StandLmbSha '' | Should -Be ''
        Get-StandSha ("Dateien`r`n  LMB.exe                  2,817 KB  " + ('0' * 64) + "`r`n  LeosMinibench.ps1        2,741 KB  $h`r`n") 'LeosMinibench.ps1' | Should -Be $h.ToLowerInvariant()
        Get-StandSha "  LMB.exe   1 KB  $h" 'LeosMinibench.ps1' | Should -Be ''
    }
    It 'liest das in eine exe eingebettete Skript (ab v4.14)' {
        if (-not (Test-IstWindows)) { Set-ItResult -Skipped -Because 'CodeDom mit eingebetteter Ressource nur unter Windows'; return }
        $tmp = Join-Path ([IO.Path]::GetTempPath()) ('Minibench_ExeSkript_' + $PID)
        New-Item -ItemType Directory -Path $tmp -Force | Out-Null
        try {
            $ps1 = Join-Path $tmp 'LeosMinibench.ps1'
            [IO.File]::WriteAllText($ps1, "`$ScriptVersion = '9.99'`r`n", (New-Object Text.UTF8Encoding($true)))
            $cp = New-Object System.CodeDom.Compiler.CompilerParameters
            $cp.GenerateExecutable = $true; $cp.OutputAssembly = Join-Path $tmp 'Probe.exe'
            [void]$cp.EmbeddedResources.Add($ps1)
            $r = (New-Object Microsoft.CSharp.CSharpCodeProvider).CompileAssemblyFromSource($cp, 'class P { static void Main() { } }')
            $r.Errors.HasErrors | Should -BeFalse
            Get-ExeSkriptSha $cp.OutputAssembly | Should -Be (Get-FileHash -LiteralPath $ps1 -Algorithm SHA256).Hash.ToLowerInvariant()
            $cp2 = New-Object System.CodeDom.Compiler.CompilerParameters
            $cp2.GenerateExecutable = $true; $cp2.OutputAssembly = Join-Path $tmp 'Ohne.exe'
            [void](New-Object Microsoft.CSharp.CSharpCodeProvider).CompileAssemblyFromSource($cp2, 'class P { static void Main() { } }')
            Get-ExeSkriptSha $cp2.OutputAssembly | Should -Be ''
            Get-ExeSkriptSha $ps1 | Should -Be ''
        } finally { Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue }
    }
    It 'erkennt nur einen Testlauf unter Windows ohne Fehlschläge' {
        Test-StandWindowsTests ("Tests     : 1203 bestanden, 0 fehlgeschlagen, 81 " + $script:Ue + "bersprungen (Pester 6.2.0, PowerShell 5.1.26100.9818) (Laufzeit: 12:34)") | Should -BeTrue
        Test-StandWindowsTests ("Tests     : 1203 bestanden, 2 fehlgeschlagen, 81 " + $script:Ue + "bersprungen (Pester 6.2.0, PowerShell 5.1.26100.9818)") | Should -BeFalse
        Test-StandWindowsTests ("Tests     : 1203 bestanden, 0 fehlgeschlagen, 81 " + $script:Ue + "bersprungen, 1 Testdateien nicht geladen (Pester 6.2.0, PowerShell 5.1.1)") | Should -BeFalse
        Test-StandWindowsTests ("Tests     : Sandbox 941 bestanden, 51 fehlgeschlagen (nur ohne Windows), 81 " + $script:Ue + "bersprungen") | Should -BeFalse
        Test-StandWindowsTests 'Tests     : nicht ausgeführt (Bauen.cmd ohnetests) (Laufzeit: --)' | Should -BeFalse
    }
    It 'überträgt fehlende Release-Tags nur, wenn sie auf einen Commit von main zeigen' {
        $script:Veroeff | Should -Match 'ls-remote --tags origin'
        $script:Veroeff | Should -Match 'merge-base --is-ancestor \$ft HEAD'
        $script:Veroeff | Should -Match '''\^v\\d\+\\\.\\d\+\$'''
        $script:Veroeff.IndexOf('ls-remote --tags origin') | Should -BeGreaterThan $script:ErsterPush
    }
}

Describe 'Keine persönlichen Angaben im Build (ab v4.11)' {
    It 'Bauen.cmd schreibt keinen Rechnernamen in Stand.txt' {
        $script:Bauen | Should -Not -Match 'COMPUTERNAME'
        $script:Bauen | Should -Match "'Gebaut    : \{0:dd\.MM\.yyyy HH:mm\} unter Windows, PowerShell \{1\}'"
    }
    It 'Aktueller Build\Stand.txt nennt den Rechnernamen nicht' {
        $f = Join-Path $global:MinibenchRepoRoot 'Aktueller Build/Stand.txt'
        if (-not (Test-Path -LiteralPath $f)) { Set-ItResult -Skipped -Because 'Aktueller Build fehlt noch'; return }
        $name = [Environment]::MachineName
        if (-not $name) { Set-ItResult -Skipped -Because 'Rechnername unbekannt'; return }
        [IO.File]::ReadAllText($f, [Text.Encoding]::UTF8) | Should -Not -Match ('(?i)\b' + [regex]::Escape($name) + '\b')
    }
}

Describe '.gitattributes (ab v4.11)' {
    It 'legt CRLF für Quelltext und Textdateien fest und behandelt exe als binär' {
        $ga = Get-RepoText '.gitattributes'
        foreach ($m in @('*.cmd', '*.ps1', '*.psd1', '*.cs', '*.txt', '*.md', '*.json')) {
            $ga | Should -Match ('(?m)^' + [regex]::Escape($m) + '\s+text eol=crlf\s*$') -Because $m
        }
        $ga | Should -Match '(?m)^\*\.exe\s+binary\s*$'
        $ga | Should -Match '(?m)^tests/Daten/WinSAT/\*\.xml\s+binary\s*$'
    }
}
