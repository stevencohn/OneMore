<#
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
	[string] $vdproj = '.\OneMoreSetup.vdproj'
)

Begin
{
	. "$PSScriptRoot\vdparser.ps1"

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

	function CleanEntries
	{
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
}
Process
{
	Push-Location .\OneMoreSetup

	$vdproj = Resolve-Path .\OneMoreSetup.vdproj

	$json = ConvertVDProjToJson $vdproj
	# save Json for debugging
	$json | ConvertTo-Json -Depth 100 | Out-File .\OneMoreSetup.vdproj.json

	$outputs = GetProjectOutputs $json

	$primary = $outputs | where { $_.SourcePath.Contains('OneMoreAddIn') } | select -expand Key
	Write-Host "`nOneMoreAddIn: " -NoNewline -Fore DarkYellow
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
				$rogues += "8:$_"
			}
		}
	}

	Write-Host "`nRogues Keys" -Fore DarkYellow
	$rogues | foreach { write-Host $_ -Fore DarkGray }

	CleanEntries $json $rogues
	CleanFiles $json $rogues

	Write-Host "`nSaving" -Fore DarkYellow
	Write-Host "... writing clean json" -Fore DarkGray
	$json | ConvertTo-Json -Depth 100 | Out-File .\OneMoreSetup.clean.json -Encoding UTF8

	Write-Host "... writing clean vdproj" -Fore DarkGray
	$name = [System.IO.Path]::GetFileNameWithoutExtension($vdproj)
	$clean = (Join-Path (Split-Path $vdproj -Parent) $name) + '.clean.vdproj'
	ConvertJsonToVDProj $json $clean
}
End
{
	Pop-Location
}