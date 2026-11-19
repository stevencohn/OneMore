<#
.SYNOPSIS
Exports an Office ECS (experimentation/feature flag) config cache from the
registry as JSON.

.DESCRIPTION
Office caches the config it receives from the ECS service under
HKCU\Software\Microsoft\Office\16.0\Common\ExperimentConfigs\Ecs\<app>\ConfigContextData

The JSON is stored as a "chunk set": a base value ("0") plus numbered chunks
("0.1" .. "0.N"), each a string of space-separated decimal ASCII codes.
Concatenating the chunks in numeric order yields:

    Mso::ChecksumRegistry::Data|uint64_t|<checksum>;EcsConfigResponseData|{ ...json... }

This script reassembles one chunk set, strips the header, validates the JSON,
and writes it to a file. Read-only: it never modifies the registry.

.PARAMETER App
Office app key under Ecs, e.g. onenote, word, excel, outlook. Default: onenote.

.PARAMETER ChunkSet
Which chunk set to export ("0" or "1"). Default: 0.

.PARAMETER OutFile
Output path. Default: <App>-ecs-set<ChunkSet>.json in the current directory.

.PARAMETER Pretty
Re-serialize with indentation. Without it the JSON is written exactly as
stored in the registry (smaller, byte-faithful).

.PARAMETER Version
Office version key, default 16.0.

.EXAMPLE
.\Export-OneNoteEcs.ps1

.EXAMPLE
.\Export-OneNoteEcs.ps1 -ChunkSet 1 -Pretty -OutFile C:\temp\onenote-set1.json

.EXAMPLE
.\Export-OneNoteEcs.ps1 -App word
#>
[CmdletBinding()]
param(
	[string]$App = 'onenote',
	[ValidatePattern('^\d+$')]
	[string]$ChunkSet = '0',
	[string]$OutFile,
	[switch]$Pretty,
	[string]$Version = '16.0'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$key = "HKCU:\Software\Microsoft\Office\$Version\Common\ExperimentConfigs\Ecs\$App\ConfigContextData"
if (-not (Test-Path -LiteralPath $key)) {
	throw "Registry key not found: $key"
}

$item = Get-Item -LiteralPath $key
$names = $item.GetValueNames()

# list chunk sets that exist, e.g. "0", "1"
$sets = $names | Where-Object { $_ -match '^\d+(\.\d+)?$' } |
	ForEach-Object { ($_ -split '\.')[0] } | Sort-Object { [int]$_ } -Unique
if ($sets -notcontains $ChunkSet) {
	throw "Chunk set '$ChunkSet' not found. Available sets: $($sets -join ', ')"
}

# base value "N" first, then "N.1" .. "N.k" in numeric order
$chunkNames = @($ChunkSet) + @(
	$names | Where-Object { $_ -match "^$ChunkSet\.(\d+)$" } |
		Sort-Object { [int]($_ -split '\.')[1] }
)

if ($item.GetValueNames() -contains 'ChunkCount') {
	$expected = "$($item.GetValue('ChunkCount'))" -replace '^.*\|', ''
	if ([int]$expected -ne ($chunkNames.Count - 1)) {
		Write-Warning "ChunkCount says $expected but found $($chunkNames.Count - 1) numbered chunks in set $ChunkSet"
	}
}

# decode: each value is space-separated decimal ASCII codes
$sb = [System.Text.StringBuilder]::new()
foreach ($name in $chunkNames) {
	$codes = ("$($item.GetValue($name))").Trim() -split '\s+' | Where-Object { $_ }
	$bytes = [byte[]]($codes | ForEach-Object { [byte][int]$_ })
	[void]$sb.Append([System.Text.Encoding]::ASCII.GetString($bytes))
}
$text = $sb.ToString()

# strip "Mso::ChecksumRegistry::Data|uint64_t|<n>;EcsConfigResponseData|" header
$start = $text.IndexOf('{')
$end = $text.LastIndexOf('}')
if ($start -lt 0 -or $end -le $start) {
	throw 'No JSON object found in decoded data.'
}
$header = $text.Substring(0, $start)
$json = $text.Substring($start, $end - $start + 1)

# validate (throws if malformed)
$parsed = $json | ConvertFrom-Json

if ($Pretty) {
	$json = $parsed | ConvertTo-Json -Depth 100
}

if (-not $OutFile) {
	$OutFile = Join-Path (Get-Location) "$App-ecs-set$ChunkSet.json"
}
$OutFile = [System.IO.Path]::GetFullPath($OutFile)

# UTF-8 without BOM
[System.IO.File]::WriteAllText($OutFile, $json, [System.Text.UTF8Encoding]::new($false))

$flagCount = ([regex]::Matches($json, '"F"\s*:\s*"')).Count
Write-Host "Exported $App chunk set $ChunkSet ($($chunkNames.Count) chunks) -> $OutFile"
Write-Host "  header : $header"
Write-Host "  size   : $($json.Length) chars, $flagCount flag entries"
Write-Host "  top    : $(($parsed.PSObject.Properties.Name) -join ', ')"
