Begin
{
	. "$PSScriptRoot\vdparser.ps1"

	function GetArcFolders
	{
		param($json)

		$folder = $json.Deployable.Folder
		$folders = ($folder | ExplodeNoteProperties | where { $_.Property -eq '8:TARGETDIR' }).Folders
		# $json.Deployable.Folder['TARGETDIR'].Folders['8:x86'].omKey
		$x86 = $folders | ExplodeNoteProperties | where { $_.Name -eq '8:x86' } | select -expand omKey
		# $json.Deployable.Folder['TARGETDIR'].Folders['8:x64'].omKey
		$x64 = $folders | ExplodeNoteProperties | where { $_.Name -eq '8:x64' } | select -expand omKey

		$runtimes = ($folders | ExplodeNoteProperties | where { $_.Name -eq '8:runtimes' }).Folders

		# $json.Deployable.Folder['TARGETDIR'].Folders['8:runtimes']['8:win-x86'].Folders['8:native'].omKey
		$win86 = ($runtimes | `
			ExplodeNoteProperties | where { $_.Name -eq '8:win-x86' }).Folders | `
			ExplodeNoteProperties | where { $_.Name -eq '8:native' } | `
			select -expand omKey

		# $json.Deployable.Folder['TARGETDIR'].Folders['8:runtimes']['8:win-x64'].Folders['8:native'].omKey
		$win64 = ($runtimes | `
			ExplodeNoteProperties | where { $_.Name -eq '8:win-x64' }).Folders | `
			ExplodeNoteProperties | where { $_.Name -eq '8:native' } | `
			select -expand omKey

		$rex = '{[0-9A-F\-]+}:(_[0-9A-F]+)$'
		if ($x86 -match $rex) { $x86 = $matches[1] }
		if ($x64 -match $rex) { $x64 = $matches[1] }
		if ($win86 -match $rex) { $win86 = $matches[1] }
		if ($win64 -match $rex) { $win64 = $matches[1] }

		Write-Host
		Write-Host "... x86 folder: $x86" -Fore DarkGray
		Write-Host "... x64 folder: $x64" -Fore DarkGray
		Write-Host "... win-x86 folder: $win86" -Fore DarkGray
		Write-Host "... win-x64 folder: $win64" -Fore DarkGray

		return [PSCustomObject]@{
			x86 = $x86
			x64 = $x64
			win86 = $win86
			win64 = $win64
		}
	}

	function GetDuplicateFiles
	{
		param($json)
		$files = $json.Deployable.File | ExplodeNoteProperties | foreach {
			[PSCustomObject]@{
				Key = [string]$_.omKey.Substring($_.omKey.IndexOf(':') + 1)
				SourcePath = [string]$_.SourcePath.Substring(2)
			}
		} | sort -property SourcePath | group -property SourcePath | where { $_.Count -gt 1 }

		return $files
	}

	function GetFileOwners
	{
		param($file)
		$owner = $json.Hierarchy | where { $_.MsmKey -eq "8:$($file.Key)" }
		if ($owner) { return $owner.OwnerKey.Substring(2) }
		return $null
	}

	function GetProjectOutputs
	{
		param($json)
		$projects = $json.Deployable.ProjectOutput | ExplodeNoteProperties | where { $_.SourcePath -ne '8:' }
		$outputs = @()
		foreach ($proj in $projects)
		{
			$outputs += [PSCustomObject]@{
				Key = $proj.omKey.Substring($proj.omKey.IndexOf(':') + 1)
				SourcePath = (Split-Path $proj.SourcePath.Substring(2) -leaf)
			}
		}

		return $outputs
	}
}
Process
{
	Push-Location .\OneMoreSetup

	$json = ConvertVDProjToJson .\OneMoreSetup.vdproj

	#$arcFolders = GetArcFolders $json

	$outputs = GetProjectOutputs $json
	$outputs | Format-Table

	$key = $outputs | where { $_.SourcePath.Contains('OneMoreAddIn') } | select -expand Key
	$key

	$groups = GetDuplicateFiles $json
	$groups | Format-Table

	$rogues = @()
	foreach ($group in $groups)
	{
		$group.Group | foreach {
			$owners = GetFileOwners $_
			"$($_.Key) $($group.Name) owners [$owners]"

			if ($owners -notcontains $key)
			{
				$rogues += $_
			}
		}
	}

	$rogues

	foreach ($rogue in $rogues)
	{
		$json.Hierarchy = $json.Hierarchy | where {
			!$_.MsmKey.EndsWith($rogue.Key) -and !$_.OwnerKey.EndsWith($rogue.Key)
		}

		$json.Deployable.File.PSObject.Properties | `
			where { $_.Name.EndsWith($rogue.Key) } | `
			foreach { $json.Deployable.File.PSObject.Properties.Remove($_.Name) }
	}

	$json | ConvertTo-Json -Depth 100 | Out-File .\OneMoreSetup.clean.json -Encoding UTF8
}
End
{
	Pop-Location
}