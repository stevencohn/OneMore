# Find resource keys in Resources.resx that are not referenced in OneMore .cs or Ribbon .xml files.

$resx    = Join-Path $PSScriptRoot 'OneMore\Properties\Resources.resx'
$csRoot  = Join-Path $PSScriptRoot 'OneMore'

$csFiles = Get-ChildItem $csRoot -Recurse -Filter '*.cs' |
    Where-Object { $_.Name -ne 'Resources.Designer.cs' }

$csContent = $csFiles | Get-Content -Raw | Out-String

$xmlContent = Get-ChildItem (Join-Path $csRoot 'Ribbon') -Filter '*.xml' |
    Get-Content -Raw |
    Out-String

$jsonContent = Get-Content (Join-Path $csRoot 'Commands\Snippets\Emoji\Emojis.json') -Raw

$combined = $csContent + $xmlContent + $jsonContent

[xml]$xml = Get-Content $resx -Raw
$keys = $xml.root.data.name

# First pass: collect all prop names that appear directly in source
$found = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($key in $keys) {
    $prop = ($key -split '\.')[0]
    if ($combined -match [regex]::Escape($prop)) {
        $null = $found.Add($prop)
    }
}

# Second pass: resolve Localize() suffix strings — prefix with the file's class name
# e.g. "titleLabel" in AboutDialog.cs => AboutDialog_titleLabel
$localizePattern = [regex]'Localize\(new string\[\][^{]*\{([^}]+)\}'
$stringPattern   = [regex]'"([^"]+)"'

foreach ($file in $csFiles) {
    $content   = Get-Content $file.FullName -Raw
    $className = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    foreach ($lm in $localizePattern.Matches($content)) {
        foreach ($sm in $stringPattern.Matches($lm.Groups[1].Value)) {
            $parts = $sm.Groups[1].Value -split '=', 2
            $null = $found.Add("${className}_$($parts[0])")
            if ($parts.Count -eq 2) {
                $null = $found.Add($parts[1])
            }
        }
    }
}

# Ribbon controls: labels and screentips are resolved dynamically via
# ReadString("{id}_Label") / ReadString("{id}_Screentip") in AddinRibbon.cs,
# so if the bare control id appears in a ribbon XML file, mark both suffixed
# variants as found without requiring the literal suffixed string to appear.
foreach ($key in $keys) {
    $prop = ($key -split '\.')[0]
    if ($prop -match '^(.+)_(Label|Screentip)$') {
        $root = $Matches[1]
        $inXml = $xmlContent -match [regex]::Escape($root)
        $inCs  = ($root -match '^rib[a-zA-Z]+Button$') -and ($csContent -match [regex]::Escape($root))
        # GetRibbonLabel/GetRibbonScreentip convert ctx/cts/ctg/pcm/pcs/bar/ct2
        # prefixes to rib at runtime, so ctxFooButton in XML resolves ribFooButton_Label
        if (-not $inXml -and $root -match '^rib([a-zA-Z]+)$') {
            $stem = $Matches[1]
            foreach ($alt in 'ctx','cts','ctg','pcm','pcs','bar','ct2') {
                if ($xmlContent -match [regex]::Escape("${alt}${stem}")) {
                    $inXml = $true; break
                }
            }
        }
        if ($inXml -or $inCs) {
            $null = $found.Add($prop)
        }
    }
}

# Any found _Label implies its _Screentip counterpart is also considered found.
# Any found e_* (emoji icon) implies its Emoji_* resource is also considered found.
$implied = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($prop in $found) {
    if ($prop.EndsWith('_Label')) {
        $null = $implied.Add(($prop -replace '_Label$', '_Screentip'))
    }
    if ($prop -match '^e_(.+)$') {
        $null = $implied.Add(('Emoji_' + $Matches[1]))
    }
}

$unused = foreach ($key in $keys) {
    $prop = ($key -split '\.')[0]
    if (-not $found.Contains($prop) -and -not $implied.Contains($prop)) {
        $key
    }
}

$unused | Sort-Object
Write-Host "`n$($unused.Count) of $($keys.Count) resources appear unused."
