<#
.SYNOPSIS
Check or set the Office Bitness/Platform registry values.

.PARAMETER Architecture
The architecture to set. Valid values are 'x86' or 'x64'. Default is 'x86'.

.PARAMETER Check
Check the current Office Bitness/Platform registry values. This is the default action.

.PARAMETER Set
Set the Office Bitness/Platform registry values to the specified Architecture.
#>

[CmdletBinding()]
param (
	[ValidateSet('x86','x64')]
	[string] $Architecture = 'x86',
	[switch] $Check,
	[switch] $Set
)

Begin
{
	$UninstallPath = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*"

	$RegistryPaths = @(
		@{ Path = 'HKLM:\Software\Microsoft\Office\16.0\Outlook'; Key = 'Bitness' },
		@{ Path = 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Office\16.0\Outlook'; Key = 'Bitness' },
		@{ Path = 'HKLM:\Software\Microsoft\Office\ClickToRun\Configuration'; Key = 'Platform' }
	)

	function CheckBitness
	{
		$status = (CheckOffice) ? 'is' : 'is not'
		Write-Host
		Write-Host "... Office $status installed" -Fore Yellow

		Write-Host
		foreach ($item in $registryPaths)
		{
			$path = $item.Path
			$key = $item.Key

			try
			{
				$props = Get-ItemProperty -Path $path -ErrorAction Stop
				Write-Host "... $path`\$key: $($props.$key)"
			}
			catch
			{
				Write-Host "... $path not found or inaccessible" -Fore DarkGray
			}
		}
	}

	function CheckOffice
	{
		$count = (Get-ItemProperty $UninstallPath | where { 
			$_.DisplayName -like "*Office*" } | Measure).Count

		return $count -gt 0
	}

	function SetBitness
	{
		param($Bitness)

		Write-Host
		foreach ($item in $registryPaths)
		{
			$path = $item.Path
			$key = $item.Key

			try
			{
				Write-Host "... setting $path\$key to '$Bitness'"

				# create key if it doesn't exist
				if (-not (Test-Path $path))
				{
					New-Item -Path $path -Force | Out-Null
				}

				Set-ItemProperty -Path $path -Name $key -Value $Bitness -Force
			}
			catch
			{
				Write-Host "*** failed to write to $path`: $_" -Fore Red
			}
		}
	}
	#>
}
Process
{
	if ($Check -or !$Set)
	{
		CheckBitness
		return
	}

	if ($Set)
	{
		if (CheckOffice)
		{
			Write-Host "`nOffice is installed! This may overwrite your Office configuration." -Fore Yellow
			$choice = Read-Host "`n... Are you sure you want to write Bitness/Platform values? (y/n)"
			if ($choice -ne 'y') { return }
		}
   
		SetBitness $Architecture
	}
}
