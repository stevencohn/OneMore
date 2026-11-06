<#
.SYNOPSIS
Regenerates the "OneMore commands by release" markdown report.

.DESCRIPTION
Inventories functional commands from:
  - docs\telemetry\telemetry-events.json   (non-CLI events, minus internal ones)
  - OneMore\AddInCommands.cs               (handlers whose command class has no telemetry event)
  - OneMore\Ribbon\*.xml                   (ribbon / context menu placement)
and uses git history to find, for each command class, the commit that first added it
and the earliest non-beta tag containing that commit.

Release = earliest non-beta tag containing the first-add commit. Tags that were never
created (for example 5.8, 6.0) are folded into the next tag. Commands committed after
the newest tag are reported as "(unreleased)".

File origins follow renames that are 90% or more similar. The last report column shows
the release/date that git's default --follow would give when it differs; those rows
deserve a manual look.

.PARAMETER Output
Path of the markdown file to write. Defaults to Downloads\onemore-commands-by-release.md.

.EXAMPLE
.\command-releases.ps1
.\command-releases.ps1 -Output C:\temp\report.md
#>
[CmdletBinding()]
param(
	[string] $Output = (Join-Path ([Environment]::GetFolderPath('UserProfile')) 'Downloads\onemore-commands-by-release.md')
)

begin
{
	$ErrorActionPreference = 'Stop'
	Set-Location $PSScriptRoot

	# telemetry events that are not user-facing commands
	$internal = @('page-save', 'EmbedSubpageProxy')

	# events whose class is not named <Event>Command and is not reachable via AddInCommands.cs
	$classOverride = @{
	}

	# shared state, filled in by the functions below
	$placement = @{}      # handler name -> list of ribbon placements
	$tagCache = @{}       # sha -> first release tag
	$originSha = @{}      # path -> sha of the commit that first added it
	$originDate = @{}     # path -> date of that commit

	# ----------------------------------------------------------------------------------
	# git helpers
	# ----------------------------------------------------------------------------------

	function Invoke-Git
	{
		$out = & git @args 2>$null
		if ($null -eq $out) { return , @() }
		# the unary comma keeps single-line results as an array instead of a scalar string
		return , @($out)
	}

	function Get-ReleaseTags
	{
		# oldest first, skipping betas
		return @((Invoke-Git tag --sort=creatordate) | Where-Object { $_ -notmatch 'beta' })
	}

	function Get-FirstTag($sha)
	{
		if ($tagCache.ContainsKey($sha)) { return $tagCache[$sha] }
		$tags = @((Invoke-Git tag --contains $sha --sort=creatordate) | Where-Object { $_ -notmatch 'beta' })
		$t = if ($tags.Count -gt 0) { $tags[0] } else { '(unreleased)' }
		$tagCache[$sha] = $t
		return $t
	}

	# Build an origin map by replaying history oldest-first: origin[path] = first-add commit.
	# Only renames that are >= 90% similar carry an origin forward. This avoids two problems:
	#   - "git log --follow" at its default 50% threshold chains new files with similar
	#     boilerplate (for example TelemetryCommand.cs) back to unrelated old files
	#   - a plain per-path log treats every folder move as a brand new file
	function Initialize-OriginMap
	{
		$curSha = $null
		$curDate = $null
		foreach ($line in (& git -c core.quotepath=false log --reverse --no-merges -M90% --name-status '--format=#%H|%ad' --date=short -- '*.cs' 2>$null))
		{
			if ($line.StartsWith('#'))
			{
				$curSha, $curDate = $line.Substring(1) -split '\|'
				continue
			}
			if (-not $line) { continue }
			$cols = $line -split "`t"
			$st = $cols[0]
			if ($st -eq 'A')
			{
				$originSha[$cols[1]] = $curSha; $originDate[$cols[1]] = $curDate
			}
			elseif ($st -eq 'D')
			{
				$originSha.Remove($cols[1]); $originDate.Remove($cols[1])
			}
			elseif ($st.StartsWith('R'))
			{
				$old = $cols[1]; $new = $cols[2]
				if ($originSha.ContainsKey($old))
				{
					$originSha[$new] = $originSha[$old]; $originDate[$new] = $originDate[$old]
					$originSha.Remove($old); $originDate.Remove($old)
				}
				else
				{
					$originSha[$new] = $curSha; $originDate[$new] = $curDate
				}
			}
		}
	}

	# returns @{ Sha; Date } of the commit that first added the class, or $null
	function Get-FirstAdd([string] $class, [string] $file)
	{
		$base = [IO.Path]::GetFileNameWithoutExtension($file)
		if ($base -eq $class)
		{
			if ($originSha.ContainsKey($file)) { return @{ Sha = $originSha[$file]; Date = $originDate[$file] } }
			return $null
		}

		# class shares a file with others; find when the class declaration first appeared there
		$lines = Invoke-Git log -S "class $class" '--format=%H|%ad' --date=short -- $file
		if ($lines.Count -gt 0)
		{
			$sha, $date = $lines[-1] -split '\|'
			return @{ Sha = $sha; Date = $date }
		}
		return $null
	}

	# the older heuristic, shown only as a cross-check: git's default rename-following
	function Get-FollowAdd([string] $file)
	{
		$lines = Invoke-Git log --follow --diff-filter=A '--format=%H|%ad' --date=short -- $file
		if ($lines.Count -eq 0) { return $null }
		$sha, $date = $lines[-1] -split '\|'
		return @{ Sha = $sha; Date = $date }
	}

	# ----------------------------------------------------------------------------------
	# source inventory
	# ----------------------------------------------------------------------------------

	# AddInCommands.cs handlers: Cmd name -> command class
	function Get-HandlerClasses
	{
		$map = @{}
		$addin = Get-Content 'OneMore\AddInCommands.cs' -Raw
		foreach ($m in [regex]::Matches($addin,
			'Task\s+(\w+)Cmd\(IRibbonControl control\)\s*=>\s*await factory\.Run\w*<(\w+)>'))
		{
			$map[$m.Groups[1].Value] = $m.Groups[2].Value
		}
		return $map
	}

	# file base name -> repo path of the first .cs file with that name
	function Get-SourceIndex
	{
		$index = @{}
		foreach ($f in (Invoke-Git ls-files '*.cs'))
		{
			$b = [IO.Path]::GetFileNameWithoutExtension($f)
			if (-not $index.ContainsKey($b)) { $index[$b] = $f }
		}
		return $index
	}

	function Read-Ribbon($node, [string[]] $path, [string] $file)
	{
		$p = $path
		if ($node.LocalName -in 'tab', 'group', 'menu', 'splitButton', 'dynamicMenu', 'gallery', 'contextMenu')
		{
			$id = $node.GetAttribute('id')
			if (-not $id) { $id = $node.GetAttribute('idMso') }
			$p = $path + $id
		}
		$action = $node.GetAttribute('onAction')
		if ($action -and $action.EndsWith('Cmd'))
		{
			$name = $action.Substring(0, $action.Length - 3)
			if (-not $placement.ContainsKey($name))
			{
				$placement[$name] = [System.Collections.Generic.List[object]]::new()
			}
			$placement[$name].Add(@{ File = $file; Path = $p })
		}
		foreach ($child in $node.ChildNodes)
		{
			if ($child.NodeType -eq 'Element') { Read-Ribbon $child $p $file }
		}
	}

	function Initialize-Ribbon
	{
		foreach ($f in 'Ribbon', 'RibbonTabHome', 'RibbonTabOneMore')
		{
			$xml = [xml](Get-Content "OneMore\Ribbon\$f.xml" -Raw)
			Read-Ribbon $xml.DocumentElement @() $f
		}
	}

	function Get-RibbonLocation($name)
	{
		if (-not $placement.ContainsKey($name)) { return '' }
		$set = [System.Collections.Generic.SortedSet[string]]::new()
		foreach ($p in $placement[$name])
		{
			if ($p.File -eq 'Ribbon')
			{
				[void]$set.Add('ctx:' + ($p.Path[-1] -replace '^ContextMenu', ''))
			}
			elseif ($p.File -eq 'RibbonTabOneMore')
			{
				$n = $p.Path | Where-Object { $_ -match '^rib.*(Menu|Group|Button)$' } | Select-Object -Last 1
				if ($n) { [void]$set.Add($n.Substring(3)) }
			}
		}
		return ($set -join ', ')
	}

	# ----------------------------------------------------------------------------------
	# candidate commands
	# ----------------------------------------------------------------------------------

	# returns items: @{ Name; Class; Key } plus the list of names missing from telemetry
	function Get-CommandItems($events, $handlerClasses, $sourceIndex)
	{
		$items = [System.Collections.Generic.List[object]]::new()
		$eventSet = [System.Collections.Generic.HashSet[string]]::new([string[]]$events)

		foreach ($e in $events)
		{
			if ($e.EndsWith('CLI') -or $internal -contains $e) { continue }
			# prefer a class named after the event, then the class the ribbon handler runs
			$class = if ($sourceIndex.ContainsKey("${e}Command")) { "${e}Command" }
				elseif ($handlerClasses.ContainsKey($e)) { $handlerClasses[$e] }
				elseif ($classOverride.ContainsKey($e)) { $classOverride[$e] }
				else { "${e}Command" }
			$items.Add(@{ Name = $e; Class = $class; Key = $e })
		}

		# handlers whose command class has no telemetry event
		$seen = [System.Collections.Generic.HashSet[string]]::new()
		foreach ($i in $items) { [void]$seen.Add($i.Class) }
		$missed = @()
		foreach ($kv in $handlerClasses.GetEnumerator() | Sort-Object Key)
		{
			$class = $kv.Value
			$base = $class -replace 'Command$', ''
			if ($eventSet.Contains($base) -or $seen.Contains($class)) { continue }
			[void]$seen.Add($class)
			$items.Add(@{ Name = $base; Class = $class; Key = $kv.Key })
			$missed += $base
		}

		return @{ Items = $items; Missed = $missed }
	}

	# resolves history for each item; returns @{ Rows; Unresolved }
	function Resolve-Items($items, $sourceIndex)
	{
		$rows = [System.Collections.Generic.List[object]]::new()
		$unresolved = @()
		$n = 0
		foreach ($i in $items)
		{
			$n++
			Write-Progress -Activity 'Reading git history' -Status $i.Name -PercentComplete (100 * $n / $items.Count)
			$file = $sourceIndex[$i.Class]
			if (-not $file)
			{
				$hit = Invoke-Git grep -lE "class $($i.Class)\b" -- '*.cs'
				if ($hit.Count -gt 0) { $file = $hit[0] }
			}
			$add = if ($file) { Get-FirstAdd $i.Class $file } else { $null }
			if (-not $add)
			{
				$unresolved += $i.Name
				continue
			}

			$alt = ''
			if ([IO.Path]::GetFileNameWithoutExtension($file) -eq $i.Class)
			{
				$f = Get-FollowAdd $file
				if ($f -and $f.Date -ne $add.Date) { $alt = "$(Get-FirstTag $f.Sha) / $($f.Date)" }
			}
			$rows.Add([pscustomobject]@{
				Release = Get-FirstTag $add.Sha; Date = $add.Date
				Command = $i.Name; Location = Get-RibbonLocation $i.Key; Alt = $alt })
		}
		Write-Progress -Activity 'Reading git history' -Completed
		return @{ Rows = $rows; Unresolved = $unresolved }
	}

	# Replay is dispatched directly to the factory rather than through a command class
	function Get-ReplayRow
	{
		$replay = Invoke-Git log -S 'ReplayLastAction' '--format=%H|%ad' --date=short -- OneMore
		if ($replay.Count -eq 0) { return $null }
		$sha, $date = $replay[-1] -split '\|'
		return [pscustomobject]@{
			Release = Get-FirstTag $sha; Date = $date
			Command = 'ReplayLastAction'; Location = Get-RibbonLocation 'Replay'; Alt = '' }
	}

	# ----------------------------------------------------------------------------------
	# report
	# ----------------------------------------------------------------------------------

	function Get-ReleaseRank($row, $tagOrder)
	{
		$ix = [array]::IndexOf($tagOrder, $row.Release)
		if ($ix -lt 0) { return 9999 }
		return $ix
	}

	function Format-Report($rows, $tagOrder)
	{
		$sorted = $rows | Sort-Object @{ Expression = { Get-ReleaseRank $_ $tagOrder } }, Date, Command

		$sb = [System.Text.StringBuilder]::new()
		[void]$sb.AppendLine('# OneMore command inventory by release')
		[void]$sb.AppendLine()
		[void]$sb.AppendLine("Generated $(Get-Date -Format 'yyyy-MM-dd') by command-releases.ps1. Release = earliest non-beta tag containing the commit that first added the command class. Empty Location = no direct ribbon/context-menu entry. Origin follows renames that are 90% or more similar; the last column shows the release/date git's default --follow would give when it differs (those rows deserve a manual look).")
		[void]$sb.AppendLine()
		[void]$sb.AppendLine('| Release | Date | Command | Location | git --follow disagrees |')
		[void]$sb.AppendLine('|---|---|---|---|---|')
		foreach ($r in $sorted)
		{
			[void]$sb.AppendLine("| $($r.Release) | $($r.Date) | $($r.Command) | $($r.Location) | $($r.Alt) |")
		}
		return $sb.ToString()
	}
}

process
{
	# main routine
	$tagOrder = Get-ReleaseTags
	$events = (Get-Content 'docs\telemetry\telemetry-events.json' -Raw | ConvertFrom-Json).events
	$handlerClasses = Get-HandlerClasses
	$sourceIndex = Get-SourceIndex

	Initialize-Ribbon
	Initialize-OriginMap

	$candidates = Get-CommandItems $events $handlerClasses $sourceIndex
	$resolved = Resolve-Items $candidates.Items $sourceIndex

	$rows = $resolved.Rows
	$replayRow = Get-ReplayRow
	if ($replayRow) { $rows.Add($replayRow) }

	$markdown = Format-Report $rows $tagOrder

	$dir = Split-Path $Output -Parent
	if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
	Set-Content -Path $Output -Value $markdown -Encoding utf8

	$rowCount = $rows.Count
	$missed = $candidates.Missed
	$unresolved = $resolved.Unresolved
}

end
{
	Write-Host "Wrote $rowCount commands to $Output"
	if ($missed.Count -gt 0)
	{
		Write-Host "In AddInCommands.cs but not in telemetry-events.json: $($missed -join ', ')"
	}
	if ($unresolved.Count -gt 0)
	{
		Write-Host "No source/history found for: $($unresolved -join ', ')" -ForegroundColor Yellow
	}
}
