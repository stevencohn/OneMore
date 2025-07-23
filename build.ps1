<#
.SYNOPSIS
Build both x86 and x64 msi

.PARAMETER AllConfigs
Build x86, x64, and ARM64 kits.

.PARAMETER ARM
Build just the ARM64 kit.

.PARAMETER ConfigBits
Specifies the bitness of the build: 64 or 86, default is 64.
Can also specify 65 as a synonym of -ARM

.PARAMETER Fast
Build just the .csproj projects using default parameters.
This will build OneMore, OneMorCalendar, OneMoreProtocolHandler, and OneMoreSetupActions.

.PARAMETER Prep
Run DisableOutOfProcBuild. This only needs to be run once on a machine, or after upgrading
or reinstalling Visual Studio. It is required to build installer kits from the command line.
#>

# CmdletBinding adds -Verbose functionality, SupportsShouldProcess adds -WhatIf
[CmdletBinding(SupportsShouldProcess = $true)]
param (
	[int] $ConfigBits = 64,
	[switch] $Fast,
	[switch] $AllConfigs,
	[switch] $ARM,
	[switch] $Prep
	)

Begin
{
	$script:devenv = ''
	$script:vsregedit = ''
	$script:vdproj = ''

	function FindVisualStudio
	{
		$cmd = Get-Command devenv -ErrorAction:SilentlyContinue
		if ($cmd -ne $null)
		{
			$script:devenv = $cmd.Source
			$script:ideroot = Split-Path -Parent $devenv
			$script:vsregedit = Join-Path $ideroot 'VSRegEdit.exe'
			return $true
		}

		$0 = 'C:\Program Files\Microsoft Visual Studio\2022'
		if (FindVS $0) { return $true }

		$0 = 'C:\Program Files (x86)\Microsoft Visual Studio\2019'
		return FindVS $0
	}

	function FindVS
	{
		param($vsroot)
		$script:devenv = Join-Path $vsroot 'Enterprise\Common7\IDE\devenv.com'

		if (!(Test-Path $devenv))
		{
			$script:devenv = Join-Path $vsroot 'Professional\Common7\IDE\devenv.com'
		}
		if (!(Test-Path $devenv))
		{
			$script:devenv = Join-Path $vsroot 'Community\Common7\IDE\devenv.com'
		}

		if (!(Test-Path $devenv))
		{
			Write-Host "devenv not found in $vsroot" -ForegroundColor Yellow
			return $false
		}

		$script:ideroot = Split-Path -Parent $devenv
		$script:vsregedit = Join-Path $ideroot 'VSRegEdit.exe'
		return $true
	}

	function DisableOutOfProcBuild
	{
		$0 = Join-Path $ideroot 'CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild'
		if (Test-Path $0)
		{
			Push-Location $0
			if (Test-Path .\DisableOutOfProcBuild.exe) {
				.\DisableOutOfProcBuild.exe
			}
			Pop-Location
			Write-Host '... disabled out-of-proc builds; reboot is recommended'
			return
		}
		Write-Host "*** could not find $0\DisableOutOfProcBuild.exe" -ForegroundColor Yellow
	}

	function PreserveVdproj
	{
		Write-Host '... preserving vdproj' -ForegroundColor DarkGray
		Copy-Item $vdproj .\vdproj.tmp -Force -Confirm:$false
	}

	function RestoreVdproj
	{
		Write-Host '... restoring vdproj' -ForegroundColor DarkGray
		Copy-Item .\vdproj.tmp $vdproj -Force -Confirm:$false
		Remove-Item .\vdproj.tmp -Force
	}

	function Configure
	{
		param([int]$bits)
		$config = $bits -eq 65 ? 'ARM64' : "x$bits"

		$lines = @(Get-Content $vdproj)

		$script:productVersion = $lines | `
			Where-Object { $_ -match '"ProductVersion" = "8:(.+?)"' } | `
			ForEach-Object { $matches[1] }

		Write-Host
		Write-Host "... configuring vdproj for $config build of $productVersion" -ForegroundColor Yellow

		'' | Out-File $vdproj -nonewline

		$lines | ForEach-Object `
		{
			if ($_ -match '"OutputFileName" = "')
			{
				# "OutputFilename" = "8:Debug\\OneMore_v_Setupx64.msi"
				$line = $_.Replace('OneMore_v_', "OneMore_$($productVersion)_")
				$line.Replace('x64', $config) | Out-File $vdproj -Append
			}
			elseif ($_ -match '"DefaultLocation" = "')
			{
				# "DefaultLocation" = "8:[ProgramFilesFolder][Manufacturer]\\[ProductName]"
				if ($bits -ge 64) {
					$_.Replace('ProgramFilesFolder', 'ProgramFiles64Folder') | Out-File $vdproj -Append
				} else {
					$_.Replace('ProgramFiles64Folder', 'ProgramFilesFolder') | Out-File $vdproj -Append
				}
			}
			elseif ($_ -match '"TargetPlatform" = "')
			{
				# x86 -> "3:0"
				# x64 -> "3:1"
				if ($bits -ge 64) {
					'"TargetPlatform" = "3:1"' | Out-File $vdproj -Append
				} else {
					'"TargetPlatform" = "3:0"' | Out-File $vdproj -Append
				}
			}
			#elseif ($_ -match '"SourcePath" = .*WebView2Loader\.dll"$')
			#{
			#    if ($bits -eq 64) {
			#        $_.Replace('\\x86', '\\x64') | Out-File $vdproj -Append
			#        $_.Replace('win-x86', 'win-x64') | Out-File $vdproj -Append
			#    } else {
			#        $_.Replace('\\x64', '\\x86') | Out-File $vdproj -Append
			#        $_.Replace('win-x64', 'win-x86') | Out-File $vdproj -Append
			#    }
			#}
			elseif (($_ -match '"Name" = "8:OneMoreSetupActions --install ') -or `
					($_ -match '"Arguments" = "8:--install '))
			{
				# "Name" = "8:OneMoreSetupActions --install --x86"
				# "Arguments" = "8:--install --x86"
				$_.Replace('x86', $config) | Out-File $vdproj -Append
			}
			elseif ($_ -notmatch '^"Scc')
			{
				$_ | Out-File $vdproj -Append
			}
		}
	}

	function BuildFast
	{
		param([int]$bits)
		Write-Host "... building x$bits DLLs" -ForegroundColor Yellow

		# build...

		Push-Location OneMore
		BuildComponent 'OneMore' $true
		Pop-Location

		Push-Location OneMoreTray
		BuildComponent 'OneMoreTray' $true
		Pop-Location

		Push-Location OneMoreCalendar
		BuildComponent 'OneMoreCalendar' $true
		Pop-Location

		Push-Location OneMoreProtocolHandler
		BuildComponent 'OneMoreProtocolHandler'
		Pop-Location
 
		Push-Location OneMoreSetupActions
		BuildComponent 'OneMoreSetupActions'
		Pop-Location
	}

	function BuildComponent
	{
		param($name, $restore = $false)
		# output file cannot exist before build
		if (Test-Path .\Debug\*)
		{
			Remove-Item .\Debug\*.* -Force -Confirm:$false
		}
		# nuget restore
		if ($restore)
		{
			$cmd = 'nuget restore .\$name.csproj'
			write-Host $cmd -ForegroundColor DarkGray
			nuget restore .\$name.csproj
		}
		$cmd = "$devenv .\$name.csproj /build ""Debug|AnyCPU"" /project $name /projectconfig Debug"
		write-Host $cmd -ForegroundColor DarkGray
		. $devenv .\$name.csproj /build "Debug|AnyCPU" /project $name /projectconfig Debug
	}

	function Build
	{
		param([int]$bits)
		$config = $bits -eq 65 ? 'ARM64' : "x$bits"

		Write-Host "... building $config MSI" -ForegroundColor Yellow

		# output file cannot exist before build
		if (Test-Path .\Debug\*)
		{
			Remove-Item .\Debug\*.* -Force -Confirm:$false
		}

		if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent)
		{
			Write-Host '... enabling MSBuild verbose logging' -ForegroundColor DarkYellow
			$cmd = ". '$vsregedit' set local HKCU General MSBuildLoggerVerbosity dword 4`n"
			write-Host $cmd -ForegroundColor DarkGray
			. $vsregedit set local HKCU General MSBuildLoggerVerbosity dword 4 | Out-Null
		}

		$cmd = "$devenv .\OneMoreSetup.vdproj /build ""Debug|$config"" /project Setup /projectconfig Debug /out `$env:TEMP\OneMoreBuild.log"
		write-Host $cmd -ForegroundColor DarkGray

		# build
		. $devenv .\OneMoreSetup.vdproj /build "Debug|$config" /project Setup /projectconfig Debug /out $env:TEMP\OneMorebuild.log

		# move msi to Downloads for safe-keeping and to allow next Platform build
		Move-Item .\Debug\*.msi $home\Downloads -Force
		Write-Host "... $config MSI copied to $home\Downloads\" -ForegroundColor DarkYellow

		if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent)
		{
			Write-Host '... disabling MSBuild verbose logging' -ForegroundColor DarkYellow
			$cmd = ". '$vsregedit' set local HKCU General MSBuildLoggerVerbosity dword 1`n"
			write-Host $cmd -ForegroundColor DarkGray
			. $vsregedit set local HKCU General MSBuildLoggerVerbosity dword 1 | Out-Null
		}
	}

	function BuildConfig
	{
		param([int]$bits)
		$config = $bits -eq 65 ? 'ARM64' : "x$bits"

		PreserveVdproj

		try
		{
			Configure $bits
			Build $bits

			if ($checkable)
			{
				$sum = (checksum -t sha256 $home\Downloads\OneMore_$($productVersion)_Setup$config.msi)
				Write-Host "... $config checksum: $sum" -ForegroundColor DarkYellow
			}
		}
		finally
		{
			RestoreVdproj
		}
	}
}
Process
{
	if (-not (FindVisualStudio))
	{
		return
	}
	
	if ($Prep)
	{
		DisableOutOfProcBuild
		return
	}

	if ($Fast)
	{
		BuildFast $ConfigBits
		return
	}

	# BUILD

	Push-Location OneMoreSetup
	$script:vdproj = Resolve-Path .\OneMoreSetup.vdproj
		
	git restore $vdproj

	$checkable = Get-Command checksum -ErrorAction SilentlyContinue

	if ($ARM) { $ConfigBits = 65 }

	if ($AllConfigs)
	{
		BuildConfig 86
		BuildConfig 64
		BuildConfig 65
	}
	else
	{
		BuildConfig $ConfigBits
	}

	Pop-Location
}
