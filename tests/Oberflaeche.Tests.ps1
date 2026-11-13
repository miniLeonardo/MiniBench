# Oberfläche (src/Oberflaeche, WinForms in C# 5): Übersetzung, Selbsttest der Steuerelemente und Hilfen, Verträge mit dem
# Arbeitsprozess und Projektregeln (Hinweise beim Überfahren).
#
# Die Oberfläche wird je Testsitzung genau einmal übersetzt (Get-GuiUebersetzung in Hilfen.ps1), zusammen mit
# tests/Daten/Oberflaeche/GuiSelbsttest.cs. Der Selbsttest läuft als eigene exe; in dieser Sitzung werden keine Typen geladen.
# Neue Verhaltensprüfungen der Oberfläche gehören in GuiSelbsttest.cs und hier in die Liste der Fälle.

BeforeAll {
    . (Join-Path $PSScriptRoot 'Hilfen.ps1')
    $script:Gui = @(Get-GuiQuellen | ForEach-Object { [IO.File]::ReadAllText($_, [Text.Encoding]::UTF8) }) -join "`n"
}

Describe 'Übersetzung (C# 5)' {
    It 'jede cs-Datei in src/Oberflaeche ist in Oberflaeche.ps1 eingebunden' {
        $dateien = @(Get-ChildItem (Join-Path $global:MinibenchSrcRoot 'Oberflaeche') -Filter '*.cs' | ForEach-Object { $_.Name } | Sort-Object)
        $eingebunden = @(Get-GuiQuellen | ForEach-Object { [IO.Path]::GetFileName($_) } | Sort-Object)
        ($eingebunden -join ',') | Should -Be ($dateien -join ',')
        [IO.Path]::GetFileName(@(Get-GuiQuellen)[0]) | Should -Be 'DiagGui.cs' -Because 'DiagGui.cs trägt die using-Zeilen für alle Teile'
    }
    It 'die Oberfläche lässt sich als Ganzes übersetzen' {
        $u = Get-GuiUebersetzung
        if (-not $u.Ok -and $u.Meldung -eq 'kein C#-Compiler verfügbar') { Set-ItResult -Skipped -Because $u.Meldung; return }
        $u.Ok | Should -BeTrue -Because $u.Meldung
    }
}

Describe 'Selbsttest der Oberfläche' {
    It '<Fall>' -ForEach @(
        @{ Fall = 'UI.Skalierung' }
        @{ Fall = 'UI.Farbschema' }
        @{ Fall = 'ToggleSwitch' }
        @{ Fall = 'ToggleSwitch.Rand' }
        @{ Fall = 'FluentCard' }
        @{ Fall = 'Hinweise.Umbruch' }
        @{ Fall = 'Grafikauswahl.Abgleich' }
        @{ Fall = 'Softwarepakete.Auswahl' }
        @{ Fall = 'Softwarepakete.Ereignisse' }
        @{ Fall = 'Datenbank.Referenzen' }
        @{ Fall = 'Aenderungen.Softwarepaket' }
        @{ Fall = 'Netzlaufwerk.Pfad' }
        @{ Fall = 'Netzlaufwerk.Konfiguration' }
        @{ Fall = 'Netzlaufwerk.Ergebnis' }
        @{ Fall = 'Wartung.Vertragszeile' }
        @{ Fall = 'Dialog.Farbschema' }
        @{ Fall = 'Oberflaeche.Kontrast' }
        @{ Fall = 'Startknopf.Beschriftung' }
        @{ Fall = 'Zeitangabe.Minuten' }
        @{ Fall = 'Lasttest.Gesamtdauer' }
        @{ Fall = 'Lasttest.Seite' }
        @{ Fall = 'Lasttest.Sensoren' }
        @{ Fall = 'Startuebersicht' }
        @{ Fall = 'Wartung.Detailtext' }
        @{ Fall = 'Voreinstellung.Altwerte' }
        @{ Fall = 'Laufwerk.Kurzname' }
        @{ Fall = 'Ablage.Kurzform' }
        @{ Fall = 'Etikett.Risikostufe' }
        @{ Fall = 'Aufklappgruppe' }
        @{ Fall = 'Diagnose.Profile' }
        @{ Fall = 'Etikett.Schreibweise' }
        @{ Fall = 'Karte' }
        @{ Fall = 'StartseitenElemente' }
        @{ Fall = 'UI.VersionNumber' }
        @{ Fall = 'SettingRow.Dynamik' }
        @{ Fall = 'CategoryPickerItem.Kurztitel' }
        @{ Fall = 'UI.SkalierungDpi' }
        @{ Fall = 'UI.IconByName' }
        @{ Fall = 'Update.Versionsvergleich' }
        @{ Fall = 'Ergebnis.Einordnung' }
        @{ Fall = 'Ergebnis.Laufwerkspillen' }
        @{ Fall = 'Optimierung.Sperre' }
        @{ Fall = 'Optimierung.Zaehler' }
        @{ Fall = 'Tools.Platzhalter' }
        @{ Fall = 'Tools.Raster' }
        @{ Fall = 'Softwarepakete.Liste' }
        @{ Fall = 'Softwarepakete.Installiert' }
        @{ Fall = 'Update.Release' }
        @{ Fall = 'Referenz.Median' }
        @{ Fall = 'Akzent.Farben' }
        @{ Fall = 'Akzent.Windows' }
        @{ Fall = 'Bewegung.Zeitgeber' }
        @{ Fall = 'Bewegung.Abgeschaltet' }
        @{ Fall = 'Ringanzeige' }
        @{ Fall = 'Ergebnis.Ordner' }
        @{ Fall = 'Einstellungen.Werte' }
        @{ Fall = 'Modulseite.Rahmen' }
        @{ Fall = 'Laufwerkszeile' }
        @{ Fall = 'Umfang.Auswahl' }
        @{ Fall = 'Umbruch.Hoehe' }
        @{ Fall = 'Ergebnis.Befundzeile' }
        @{ Fall = 'Benchmark.Seite' }
        @{ Fall = 'Diagnose.Seite' }
        @{ Fall = 'Ergebnis.Seitenleiste' }
        @{ Fall = 'Ergebnis.Datenbank' }
        @{ Fall = 'Umzug.Seite' }
        @{ Fall = 'Umzug.Platz' }
        @{ Fall = 'Modulseite.Kopf' }
    ) {
        Assert-GuiSelbsttest $Fall
    }
    It 'jeder Fall des Selbsttests wird hier geprüft' {
        $u = Get-GuiUebersetzung
        if (-not $u.Ok) { Set-ItResult -Skipped -Because 'Oberfläche nicht übersetzt'; return }
        $m = Get-GuiSelbsttest
        if ($null -eq $m) { Set-ItResult -Skipped -Because 'Selbsttest braucht Windows oder eine Anzeige'; return }
        $text = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Oberflaeche.Tests.ps1'), [Text.Encoding]::UTF8)
        $hier = @([regex]::Matches($text, "@\{ Fall = '([^']+)' \}") | ForEach-Object { $_.Groups[1].Value })
        $m.Count | Should -BeGreaterThan 0
        foreach ($k in $m.Keys) { $hier | Should -Contain $k }
    }
}

Describe 'Verträge zwischen Oberfläche und Arbeitsprozess' {
    It 'jeder Parameter, den die Oberfläche an LeosMinibench.ps1 übergibt, gibt es im Skript' {
        $params = Get-ScriptParameters
        $fremd = @('NoProfile', 'ExecutionPolicy', 'File', 'Command')   # Parameter von powershell.exe selbst
        # Versionen.cs ist Historientext, keine Befehlszeile
        $code = @(Get-GuiQuellen | Where-Object { [IO.Path]::GetFileName($_) -ne 'Versionen.cs' } | ForEach-Object { [IO.File]::ReadAllText($_, [Text.Encoding]::UTF8) }) -join "`n"
        $benutzt = @(foreach ($lit in [regex]::Matches($code, '"(?:[^"\\\r\n]|\\.)*"')) {
                foreach ($m in [regex]::Matches($lit.Value, '(?<=[\s"])-([A-Z][A-Za-z]+)\b')) { $m.Groups[1].Value }
            }) | Sort-Object -Unique
        $benutzt.Count | Should -BeGreaterThan 20
        foreach ($p in $benutzt) { if ($fremd -notcontains $p) { $params | Should -Contain $p -Because ('die Oberfläche übergibt -' + $p) } }
    }
    It 'Rendertest-Einstellungen werden zwischen Benchmark und Lasttest in beide Richtungen abgeglichen' {
        # das Verhalten von CopySelection prüft der Selbsttest (Grafikauswahl.Abgleich); hier die Verdrahtung beider Seiten
        $script:Gui | Should -Match 'ComboBox\[\] GpuCombosBench\(\) \{ return new ComboBox\[\] \{ cmbGpuRes, cmbGpuShow, cmbGpuSel \}; \}'
        $script:Gui | Should -Match 'ComboBox\[\] GpuCombosLast\(\) \{ return new ComboBox\[\] \{ cmbLGpuRes, cmbLGpuShow, cmbLGpuSel \}; \}'
        $script:Gui | Should -Match 'sync1 = delegate \{[^\r\n]*CopySelection\(GpuCombosBench\(\), GpuCombosLast\(\)\)'
        $script:Gui | Should -Match 'sync2 = delegate \{[^\r\n]*CopySelection\(GpuCombosLast\(\), GpuCombosBench\(\)\)'
        $script:Gui | Should -Match 'cmbLGpuRes\.SelectedIndexChanged \+= sync2; cmbLGpuShow\.SelectedIndexChanged \+= sync2; cmbLGpuSel\.SelectedIndexChanged \+= sync2;'
    }
    It 'Softwarepakete installiert der Arbeitsprozess, die Oberfläche startet winget nicht selbst' {
        $script:Gui | Should -Match '-SoftwareInstallieren'
        $script:Gui | Should -Not -Match 'install --id'
        $script:Gui | Should -Match '"@@PAKET\|"'
        # ab v4.05: Liste aus Kern\Softwarepakete.psd1, installierte Pakete über -SoftwareZustand im Hintergrund
        $script:Gui | Should -Match '-SoftwareZustand'
        $script:Gui | Should -Match '"@@PAKZ\|"'
        $script:Gui | Should -Not -Match 'new string\[\] \{ "Google\.Chrome"'
        (Get-SrcText 'Oberflaeche/Oberflaeche.ps1') | Should -Match '\[DiagGui\]::SoftwarePakete = \[string\[\]\]\(Get-SoftwarePaketGuiLines\)'
    }
    It 'Update-Prüfung: Jetzt suchen ohne Cache, Ausweg über github.com, Median-Meldung aus dem Ergebnis (ab v4.06)' {
        $script:Gui | Should -Match 'StartUpdateCheck\(true\)'
        $script:Gui | Should -Match 'releases/latest'
        $script:Gui | Should -Match 'AllowAutoRedirect = false'
        $script:Gui | Should -Match 'LMB\.exe\.sha256'
        $script:Gui | Should -Not -Match '@@STATUS'
        $script:Gui | Should -Match 'MedianErgebnisText\(res, lines\)'
        (Get-RepoText 'Veroeffentlichen.ps1') | Should -Match 'release upload \$tag \$exe \$shaFile'
        (Get-RepoText 'Veroeffentlichen.ps1') | Should -Match '''release'', ''create'', \$tag, \$exe, \$shaFile'
    }
    It 'Seite Tools ohne Abschnitt Netzlaufwerk und mit den neuen Werkzeugen (ab v4.05)' {
        $seiten = Get-GuiText
        $a = $seiten.IndexOf('Control BuildToolsPage()'); $e = $seiten.IndexOf('FlowLayoutPanel ToolRaster()')
        $tools = $seiten.Substring($a, $e - $a)
        $tools | Should -Not -Match 'Netzlaufwerk'
        $tools | Should -Not -Match 'Systemänderungen'
        foreach ($w in 'CPU-Z', 'GPU-Z', 'MSI Afterburner', 'Revo Uninstaller', 'MiniTool Partition Wizard', 'WizTree') { $tools | Should -Match ([regex]::Escape('ToolZeile("' + $w + '"')) }
        $tools | Should -Match '"GPU-Z\*\.exe"'
        $tools | Should -Match 'cpuid\.com/softwares/cpu-z\.html'
        $tools | Should -Match 'techpowerup\.com/gpuz'
        $tools | Should -Match 'msi\.com/Landing/afterburner'
        $tools | Should -Match 'RivaTuner'
    }
    It 'jede gespeicherte Voreinstellung wird beim Laden wieder angewendet' {
        $put = @([regex]::Matches($script:Gui, 'Put(?:C|Cb)\(d, "([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
        $get = @([regex]::Matches($script:Gui, 'Get(?:C|Cb)\(d, "([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
        $put.Count | Should -BeGreaterThan 15
        foreach ($k in $put) { $get | Should -Contain $k }
    }
    It 'jede Methode mit new Form() ruft UI.ThemeDialog auf' {
        $csDateien = Get-ChildItem -Path (Join-Path $PSScriptRoot '..\src\Oberflaeche') -Filter '*.cs'
        $methodenMitForm = 0
        foreach ($datei in $csDateien) {
            $text = [IO.File]::ReadAllText($datei.FullName, [Text.Encoding]::UTF8)
            $mMatches = [regex]::Matches($text, '(?m)^\s*(?:public|private|internal|protected|static|\s)*\b[\w<>\[\]]+\s+(\w+)\s*\([^)]*\)\s*\{')
            foreach ($m in $mMatches) {
                $idx = $m.Index + $m.Length
                $depth = 1
                while ($depth -gt 0 -and $idx -lt $text.Length) {
                    if ($text[$idx] -eq '{') { $depth++ }
                    elseif ($text[$idx] -eq '}') { $depth-- }
                    $idx++
                }
                $body = $text.Substring($m.Index, $idx - $m.Index)
                if ($body -match 'new Form\(') {
                    $methodenMitForm++
                    $body | Should -Match 'UI\.ThemeDialog\(' -Because "$($m.Groups[1].Value) in $($datei.Name) erzeugt eine Form und muss UI.ThemeDialog aufrufen"
                }
            }
        }
        $methodenMitForm | Should -BeGreaterOrEqual 5
    }
}

Describe 'Projektregeln der Oberfläche' {
    It 'jedes Bedienelement hat einen Hinweis beim Überfahren (Tip)' {
        # Felder der Oberfläche mit Bedienelementen. Abgedeckt ist ein Feld durch Tip(feld, ...), Tip(feld[...], ...) oder
        # eine foreach-Liste direkt vor Tip(...). Ausnahmen: Teile eigener Steuerelemente (die Karte trägt den Hinweis),
        # das Protokollfeld und die nicht angezeigte Vergleichsliste.
        $ausnahmen = @('actionButton', 'secondaryButton', 'mainButton', 'moreButton', 'toggle', 'txtLog', 'clbCompare')
        $types = 'CheckBox|ComboBox|Button|LinkLabel|TextBox|RadioButton|ToggleSwitch|FluentCard|NumericUpDown|TrackBar|CheckedListBox|DarkComboBox'
        $felder = @([regex]::Matches($script:Gui, "(?m)^    (?:public |private |internal |static |readonly )*(?:$types)(?:\[\])?\s+([\w\s,]+);") |
            ForEach-Object { $_.Groups[1].Value -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ -match '^\w+$' } | Sort-Object -Unique)
        $felder.Count | Should -BeGreaterThan 50
        $mitTip = New-Object 'System.Collections.Generic.HashSet[string]'
        foreach ($m in [regex]::Matches($script:Gui, 'Tip\(\s*(\w+)\s*[,\[]')) { [void]$mitTip.Add($m.Groups[1].Value) }
        foreach ($m in [regex]::Matches($script:Gui, 'foreach \(\w+ (\w+) in new \w+\[\] \{([^}]+)\}\)\s*Tip\(\s*(\w+)\s*,')) {
            if ($m.Groups[1].Value -eq $m.Groups[3].Value) { foreach ($n in $m.Groups[2].Value -split ',') { [void]$mitTip.Add($n.Trim()) } }
        }
        $ohne = @($felder | Where-Object { $ausnahmen -notcontains $_ -and -not $mitTip.Contains($_) })
        ($ohne -join ', ') | Should -BeNullOrEmpty
    }
    It 'Hinweise ohne Spiegelstrich-Aufzählung' {
        foreach ($m in [regex]::Matches($script:Gui, 'Tip\([^,]+, "([^"]+)"')) { $m.Groups[1].Value | Should -Not -Match '(^|\\n)\s*[-–] ' }
    }
    It 'v3.85 Oberflächen-Standards: Zurück-Button, Lasttest-Grafik & parallele Prüfungen' {
        $script:Gui | Should -Match 'btnNew = UI\.Secondary\("Zurück"\);'
        $script:Gui | Should -Match 'Tip\(btnNew, "Kehrt zur Startseite und den Ablauf-Einstellungen zurück\."\);'
        $script:Gui | Should -Match 'chkFast = Chk\("Lesende Prüfungen parallel ausführen", true\);'
        $script:Gui | Should -Match 'chkLGpu = Chk\("Grafik", true\);'
        $script:Gui | Should -Match 'if \(chkLGpu != null\) chkLGpu\.Checked = true;'
    }
}

Describe 'Kategoriesymbole (ab v4.01)' {
    It 'jedes Symbol aus Katalog der Optimierung und Vertrag der Wartung ist eine Konstante in UI' {
        $ui = Get-SrcText 'Oberflaeche/DiagGui_Steuerelemente.cs'
        $namen = @()
        foreach ($pfad in 'Module/Optimierung/Katalog.psd1', 'Module/Wartung/Vertrag.psd1') {
            $namen += @([regex]::Matches((Get-SrcText $pfad), "Symbol\s*=\s*'(\w+)'") | ForEach-Object { $_.Groups[1].Value })
        }
        $namen.Count | Should -BeGreaterThan 10
        foreach ($n in $namen) { $ui | Should -Match ('public const string {0} = ' -f $n) -Because $n }
    }
    It 'Optimierung zeigt Kurztitel und Symbol aus dem Katalog' {
        $gui = Get-GuiText
        $gui | Should -Match 'UI\.IconByName\('
        $gui | Should -Match 'new CategoryPickerItem\(k\[1\], ico, '
    }
    It 'Seitenkopf ohne Schalter "beim Start ausführen", Fußleiste ohne globale Optionen' {
        $gui = Get-GuiText
        $gui | Should -Not -Match 'Lbl\("beim Start ausführen"'
        $gui | Should -Not -Match 'chkSingleWindow'
        $gui | Should -Not -Match 'fo\.Controls\.Add\((chkAnon|chkSingleWindow|lnkAblage)\)'
    }
}

Describe 'Einstellungen und Ergebnisseite (ab v4.02)' {
    It 'Einstellungen auf einer Seite ohne Reiter je Option' {
        $gui = Get-GuiText
        $m = [regex]::Match($gui, '(?s)void ShowSettingsDialog\(\).*?\n    void SaveWindowPreferences')
        $m.Success | Should -BeTrue
        $m.Value | Should -Not -Match 'CategoryPickerItem'
        $m.Value | Should -Match 'Gruppe\("Allgemein"\)'
        $m.Value | Should -Match 'Gruppe\("Daten und Datenschutz"\)'
    }
    It 'Einordnung gegenüber der Referenz ohne Wertung als Durchschnitt' {
        $seiten = Get-GuiText
        $seiten | Should -Not -Match '"Unterdurchschnittlich"|"Hervorragend"'
        $seiten | Should -Match 'static string RatingWord'
        $seiten | Should -Match '"nicht gemessen"'
    }
    It 'Protokoll schreibt Abschnittszeilen nur einmal' {
        $gui = Get-GuiText
        $gui | Should -Not -Match 'log\.AppendLine\(">> "'
    }
}

Describe 'Benchmark ohne Referenz-Haken (ab v4.04)' {
    It 'Referenz wird nur noch in der Datenbank festgelegt' {
        $gui = Get-GuiText
        $gui | Should -Not -Match 'chkRefSave'
        $gui | Should -Match 'Aktionen \\u203A Als Referenz festlegen'
        (Get-SrcText 'Oberflaeche/DiagGui_Vergleich.cs') | Should -Match 'Als Referenz festlegen \(persistent\)'
    }
}

Describe 'Eine Datei je Seite, Startzeit, Akzentfarbe und Bewegung (ab v4.07)' {
    It 'jede Seite der Oberfläche hat eine eigene Datei, keine davon ist größer als 2000 Zeilen' {
        $namen = @(Get-GuiQuellen | ForEach-Object { [IO.Path]::GetFileName($_) })
        foreach ($n in 'DiagGui_Startseite.cs', 'DiagGui_Diagnose.cs', 'DiagGui_Benchmark.cs', 'DiagGui_Lasttest.cs', 'DiagGui_Wartung.cs', 'DiagGui_Optimierung.cs',
            'DiagGui_Tools.cs', 'DiagGui_Sensoren.cs', 'DiagGui_Umzug.cs', 'DiagGui_Vergleich.cs', 'DiagGui_Aenderungen.cs', 'DiagGui_Versionen.cs',
            'DiagGui_Einstellungen.cs', 'DiagGui_Ergebnis.cs', 'DiagGui_Lauf.cs', 'Bewegung.cs') { $namen | Should -Contain $n }
        $namen | Should -Not -Contain 'DiagGui_Seiten.cs'
        foreach ($f in Get-GuiQuellen) {
            if ([IO.Path]::GetFileName($f) -eq 'DiagGui_Steuerelemente.cs') { continue }
            @([IO.File]::ReadAllLines($f)).Count | Should -BeLessThan 2000 -Because ([IO.Path]::GetFileName($f))
        }
    }
    It 'using-Zeilen stehen nur in DiagGui.cs, jede Seitendatei ist eine Teilklasse' {
        foreach ($f in @(Get-GuiQuellen | Select-Object -Skip 1)) {
            $t = [IO.File]::ReadAllText($f, [Text.Encoding]::UTF8)
            $t | Should -Not -Match '(?m)^using\s' -Because ([IO.Path]::GetFileName($f))
            if ([IO.Path]::GetFileName($f) -match '^DiagGui_(?!Steuerelemente|Modelle)') { $t | Should -Match 'public partial class DiagGui' -Because ([IO.Path]::GetFileName($f)) }
        }
    }
    It 'die Hardwareabfrage beim Start fragt nur die gebrauchten Eigenschaften ab' {
        $o = Get-SrcText 'Oberflaeche/Oberflaeche.ps1'
        $a = $o.IndexOf('[void]$hwPs.AddScript({'); $e = $o.IndexOf('$hwHandle = $hwPs.BeginInvoke()')
        $a | Should -BeGreaterThan 0; $e | Should -BeGreaterThan $a
        $block = $o.Substring($a, $e - $a)
        $abfragen = @([regex]::Matches($block, 'Get-CimInstance[^|\r\n)]*'))
        $abfragen.Count | Should -BeGreaterThan 8
        foreach ($q in $abfragen) { $q.Value | Should -Match '-Property ' -Because $q.Value }
        $block | Should -Match 'Win32_Processor -Property Name, NumberOfCores, NumberOfLogicalProcessors'
    }
    It 'Datenbank und Änderungsprotokoll werden beim Start gleichzeitig vorgeladen' {
        $gui = Get-GuiText
        $gui | Should -Match '(?s)changeDir = [^\n]+\n\s+StartDbVorladen\(\);'
        $gui | Should -Match 'dbVorThread\.Join\(\)'
        $gui | Should -Match 'Seiten aufgebaut \('
    }
    It 'Seitenwechsel blendet über, außer bei der Seite Versionen, und die Ergebnisseite startet die Bewegung' {
        $gui = Get-GuiText
        $gui | Should -Match 'i != versionPage && curPage != versionPage\) \? Ueberblendung\.Aufnahme\(content\)'
        $gui | Should -Match 'Ueberblendung\.Starte\(content, vorher\)'
        $gui | Should -Match '(?s)PopulateResultDashboard\(\);\s+runView\.Visible = false;\s+ZeigeErgebnis\(\);'
        $gui | Should -Match 'Bewegung\.Starte\(resultDashboard, Bewegung\.DauerErgebnis'
    }
    It 'Einstellungen bieten Akzentfarbe und Animationen, beides wird vor dem Farbschema geladen' {
        $gui = Get-GuiText
        $gui | Should -Match 'Zeile\(gAll, "Akzentfarbe"'
        $gui | Should -Match 'Zeile\(gAll, "Animationen"'
        $gui | Should -Match '(?s)LoadAkzentPreference\(\);\s+UI\.SetTheme\(isDark\);'
        $gui | Should -Not -Match 'Color\.FromArgb\(100, 180, 246\)|Color\.FromArgb\(0, 100, 190\)' -Because 'feste Blautöne folgen sonst der Akzentfarbe nicht'
    }
}

Describe 'Modulseiten und Ergebnis als Seite (ab v4.08)' {
    It 'Benchmark und Diagnose nutzen den gemeinsamen Rahmen, das Listenfeld der Laufwerke ist ersetzt' {
        $gui = Get-GuiText
        (Get-SrcText 'Oberflaeche/DiagGui_Benchmark.cs') | Should -Match 'benchSeite = NeueModulSeite\(f\);'
        (Get-SrcText 'Oberflaeche/DiagGui_Diagnose.cs') | Should -Match 'diagSeite = NeueModulSeite\(f\);'
        (Get-SrcText 'Oberflaeche/DiagGui_Modulseite.cs') | Should -Match 'SettingRow SchalterZeile\(CheckBox wert'
        $gui | Should -Not -Match 'clbDisks'
        $gui | Should -Not -Match 'GroupHeader diagToggle'
        # Werte für Vorlagen und Start bleiben Kontrollkästchen und Optionsfelder mit den bisherigen Namen
        $gui | Should -Match 'benchChk\[i\] = Chk\(benchText\[i\], true\);'
        $gui | Should -Match 'diagChk\[i\] = Chk\(diagText\[i\], true\);'
        $gui | Should -Match 'new RadioButton\[\] \{ rbVoll, rbSchnell, rbCustom \}' -Because 'Reihenfolge der Profile in den Vorlagen'
    }
    It 'das Ergebnis ist eine Seite mit Eintrag in der Seitenleiste, ohne Knopf Zurück' {
        $gui = Get-GuiText
        $gui | Should -Not -Match 'btnDashBack'
        $gui | Should -Match 'navErgebnis = new NavItem\("Ergebnis", "", UI\.IcoErgebnis\);'
        $gui | Should -Match 'ergebnisPage = pages\.Count; pages\.Add\(resultDashboard\);'
        $gui | Should -Match 'btnDashAgain = UI\.Secondary\("Erneut messen"\);'
        $gui | Should -Match 'if \(ergBewegung\) \{ ergBewegung = false; StarteErgebnisBewegung\(\); \}' -Because 'Bewegung nur beim ersten Anzeigen'
        $gui | Should -Match 'if \(bandErgGezeigt\) VerbergeErgebnisBand\(\);'
        $gui | Should -Not -Match 'resultDashboard\.Visible = false' -Because 'die Seite wird wie jede andere über ShowPage gewechselt'
    }
    It 'der Vergleich mit der Referenz steht im Datenbankeintrag und wird von dort gelesen' {
        (Get-SrcText 'Kern/Datenbank.ps1') | Should -Match "\`$o\['Vergleich'\] = \`$vgl"
        (Get-SrcText 'Kern/Datenbank.ps1') | Should -Match "\`$script:DbFormat = 'PC-Diagnose-DB/2'"
        (Get-SrcText 'Oberflaeche/DiagGui_Modelle.cs') | Should -Match 'd\.TryGetValue\("Vergleich", out vObj\)'
        (Get-GuiText) | Should -Not -Match 'Get-RefPct|GeoMean' -Because 'die Oberfläche rechnet die Prozentwerte nicht nach'
    }
}
