<#
.SYNOPSIS
Cleans a Visual Studio Setup project (.vdproj) of duplicate file entries.

.PARAMETER vdproj
Path to the .vdproj file to clean. Default is '.\OneMoreSetup.vdproj

.DESCRIPTION
The output results in a file named 'NAME.clean.vdproj' in the same directory as
the input file. The intermediate json files are preserved as well for analysis.

.NOTES
Doesn't work entirely. No matter what we do to the .vdproj file, a build with VS or devenv
will still produce warnings about duplicate files, even the ones we de-dupe here.
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
	[string] $vdproj = '.\OneMoreSetup.vdproj'
)

Begin
{
	. "$PSScriptRoot\vdparser.ps1"

	# under Windows System File Protection (GAC)
	$Exclusions = @(
		'System.Diagnostics.Tracing.dll',
		'System.IO.Compression.dll',
		'System.IO.Compression.FileSystem.dll',
		'System.Net.Http.dll',
		'System.ObjectModel.dll',
		'System.Net.Http.WebRequest.dll',
		'System.Numerics.Vectors.dll',
		'System.Runtime.dll',
		'System.Runtime.WindowsRuntime.dll',
		'System.Runtime.WindowsRuntime.resources.dll',
		'System.Runtime.WindowsRuntime.UI.Xaml.dll'
	)

	function GetProjectOutputs
	{
		# project outputs are the main outputs from project references in the vdproj.
		# we get their IDs so to back-track dependencies.
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

	function GetRogueFiles
	{
		# get duplicate file references, not associated with the primary OneMoreAddIn
		# assembly ID. these "rogue" files can be removed along with their Entry references
		param(json)
		Write-Host "`nRogues Owners" -Fore DarkYellow
		$duplicates = GetDuplicateFiles $json
		$rogues = @()
		foreach ($duplicate in $duplicates)
		{
			$duplicate.Keys | foreach {
				$owners = GetFileOwners $_
				$name = [System.IO.Path]::GetFileNameWithoutExtension($duplicate.Name)
				Write-Host "$_ $name [$owners]" -Fore DarkGray

				if ($owners -notcontains $primary)
				{
					$rogues += "8:$_"
				}
			}
		}

		Write-Host "`nRogues Keys" -Fore DarkYellow
		$rogues | foreach { write-Host $_ -Fore DarkGray }
		return $rogues
	}

	function GetDuplicateFiles
	{
		# discovers files that are referenced by more than one project output
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
		# gets all owners of the given file, used to eliminate unnecessary Entry items
		param($key)
		$owner = $json.Hierarchy | where { $_.MsmKey -eq "8:$key" }
		if ($owner) { return $owner.OwnerKey.Substring(2) }
		return $null
	}

	function CleanEntries
	{
		# remove all unused Entry items based on rogue/duplicate files
		param($json, $rogues)
		Write-Host "`nCleaning Entries" -Fore DarkYellow

		$count = $json.Hierarchy.count
		Write-Host "... $count Entry items to scan" -Fore DarkGray

		$dead = $json.Hierarchy | where { $_.MsmKey -in $rogues -or $_.OwnerKey -in $rogues }
		$deadCount = $dead.count
		$expected = $count - $deadCount
		Write-Host "... $deadCount Entry items to remove, expecting $expected" -Fore DarkGray

		$json.Hierarchy = $json.Hierarchy | where {
			-not ($_.MsmKey -in $rogues -or $_.OwnerKey -in $rogues)
		}

		if ($json.Hierarchy.Count -eq $expected) {
			Write-Host "... successfully removed $deadCount Entry items" -Fore DarkGreen
		} else {
			Write-Host "... mismatch: $($json.Hierarchy.Count), expected $expected" -Fore DarkRed
		}
	}

	function CleanFiles
	{
		# remove the duplicate File items
		param($json, $rogues)
		Write-Host "`nCleaning Files" -Fore DarkYellow
		$count = ($json.Deployable.File.PSObject.Properties | measure).Count
		$expected = $count - $rogues.Count
		Write-Host "... $($rogues.Count) Files to remove, expecting $expected" -Fore DarkGray

		$json.Deployable.File.PSObject.Properties | `
			where {
				$key = '8:' + $_.Name.Substring($_.Name.IndexOf(':') + 1)
				return $key -in $rogues
			} | `
			foreach { 
				Write-Host "... removing file $($_.Name)" -Fore DarkGray
				$json.Deployable.File.PSObject.Properties.Remove($_.Name)
			}

		$result = ($json.Deployable.File.PSObject.Properties | measure).Count
		if ($result -eq $expected) {
			Write-Host "... successfully removed $($rogues.Count) File items" -Fore DarkGreen
		} else {
			Write-Host "... mismatch: $result, expected $expected" -Fore DarkRed
		}
	}

	function SetExclusions
	{
		# setting Exclude in VS typically is not preserved. this attempts to Exclude
		# known protected files from being included in the installer.
		param($json)
		Write-Host "`nExcluding protected files"
		foreach ($exclusion in $Exclusions)
		{
			$json.Deployable.File.PSObject.Properties | `
				where { $_.value.Sourcepath -eq "8:$exclusion" } | `
				foreach {
					Write-Host "... excluding $exclusion" -Fore DarkGray
					$_.value.Exclude = '8:TRUE'
				}
		}
	}
}
Process
{
	$vdproj = (Resolve-Path $vdproj).Path

	Push-Location (Split-Path $vdproj -Parent)

	$json = ConvertVDProjToJson $vdproj
	# save Json for debugging/analysis
	$json | ConvertTo-Json -Depth 100 | Out-File .\OneMoreSetup.vdproj.json

	$outputs = GetProjectOutputs $json

	$primary = $outputs | where { $_.SourcePath.Contains('OneMoreAddIn') } | select -expand Key
	Write-Host "`nOneMoreAddIn: " -NoNewline -Fore DarkYellow
	Write-Host $primary -Fore DarkCyan

	$rogues = GetRogueFiles $json

	CleanEntries $json $rogues
	CleanFiles $json $rogues

	SetExclusions $json

	Write-Host "`nSaving" -Fore DarkYellow
	Write-Host "... saving clean json" -Fore DarkGray
	# save clean Json for debugging/analysis
	$json | ConvertTo-Json -Depth 100 | Out-File .\OneMoreSetup.clean.json -Encoding UTF8

	$name = [System.IO.Path]::GetFileNameWithoutExtension($vdproj)
	$clean = (Join-Path (Split-Path $vdproj -Parent) $name) + '.clean.vdproj'
	Write-Host "... saving clean vdproj to $clean" -Fore DarkGray
	ConvertJsonToVDProj $json $clean
}
End
{
	Pop-Location
}