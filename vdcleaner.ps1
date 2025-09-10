<#
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
	[string] $vdproj = '.\OneMoreSetup.vdproj'
)

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

	function GetProjectOutputs
	{
		param($json)
		$outputs = @(
			$json.Deployable.ProjectOutput.PSObject.Properties | `
			where { $_.value.sourcepath -ne '8:' } | `
			foreach { 
				[PSCustomObject]@{
					Key = $_.Name.Substring($_.Name.IndexOf(':') + 1)
					SourcePath = (Split-Path $_.value.sourcepath.Substring(2) -leaf)
				}
			})

		Write-Host "`nProject Outputs" -Fore DarkYellow
		$outputs | foreach { Write-Host "... $($_.Key) $($_.SourcePath)" -Fore DarkGray }
		return $outputs
	}

	function GetDuplicateFiles
	{
		param($json)

		$duplicates = $json.Deployable.File.PSObject.Properties | `
			foreach { [PSCustomObject]@{ key = $_.Name; SourcePath = $_.value.sourcepath } } | `
			sort -property SourcePath | group -property SourcePath | where { $_.count -gt 1 } | `
			foreach {
				[PSCustomObject]@{
					Name = $_.Name.Substring(2)
					Keys = @($_.Group | foreach { $_.Key.substring($_.key.indexof(':')+1) })
				}
			}

		Write-Host "`nDuplicate Files" -Fore DarkYellow
		$duplicates | foreach { Write-Host "... $($_.Name.PadRight(30)) $($_.Keys)" -Fore DarkGray }
		return $duplicates
	}

	function GetFileOwners
	{
		param($key)
		$owner = $json.Hierarchy | where { $_.MsmKey -eq "8:$key" }
		if ($owner) { return $owner.OwnerKey.Substring(2) }
		return $null
	}
}
Process
{
	Push-Location .\OneMoreSetup

	$vdproj = Resolve-Path .\OneMoreSetup.vdproj

	$json = ConvertVDProjToJson $vdproj
	$json | ConvertTo-Json -Depth 100 | Out-File .\OneMoreSetup.vdproj.json

	#$arcFolders = GetArcFolders $json

	$outputs = GetProjectOutputs $json

	$primary = $outputs | where { $_.SourcePath.Contains('OneMoreAddIn') } | select -expand Key
	Write-Host "`nOneNoteAddIn: " -NoNewline -Fore DarkYellow
	Write-Host $primary -Fore DarkCyan

	$duplicates = GetDuplicateFiles $json

	Write-Host "`nRogues Owners" -Fore DarkYellow
	$rogues = @()
	foreach ($duplicate in $duplicates)
	{
		$duplicate.Keys | foreach {
			$owners = GetFileOwners $_
			$name = [System.IO.Path]::GetFileNameWithoutExtension($duplicate.Name)
			Write-Host "$_ $name [$owners]" -Fore DarkGray

			if ($owners -notcontains $primary)
			{
				$rogues += $_
			}
		}
	}

	Write-Host "`nRogues Keys" -Fore DarkYellow
	$rogues | foreach { write-Host $_ -Fore DarkGray }

	Write-Host "`nCleaning" -Fore DarkYellow
	foreach ($rogue in $rogues)
	{
		$json.Hierarchy = $json.Hierarchy | where {
			!$_.MsmKey.EndsWith($rogue) -and !$_.OwnerKey.EndsWith($rogue)
		}

		$json.Deployable.File.PSObject.Properties | `
			where { $_.Name.EndsWith($rogue) } | `
			foreach { $json.Deployable.File.PSObject.Properties.Remove($_.Name) }
	}

	$json | ConvertTo-Json -Depth 100 | Out-File .\OneMoreSetup.clean.json -Encoding UTF8

	$name = [System.IO.Path]::GetFileNameWithoutExtension($vdproj)
	$clean = (Join-Path (Split-Path $vdproj -Parent) $name) + '.clean.vdproj'
	ConvertJsonToVDProj $json $clean
}
End
{
	Pop-Location
}