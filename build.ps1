<#
.SYNOPSIS
Build both x86 and x64 msi

.PARAMETER ConfigBits
Specifies the bitness of the build: 64 or 86, default is 64

.PARAMETER Both
Build both x86 and x64 kits.

.PARAMETER Prep
Run DisableOutOfProcBuild. This only needs to be run once on a machine, or after upgrading
or reinstalling Visual Studio. It is required to build installer kits from the command line.
#>

# CmdletBinding adds -Verbose functionality, SupportsShouldProcess adds -WhatIf
[CmdletBinding(SupportsShouldProcess = $true)]
param (
    [int] $configbits = 64,
    [switch] $both,
    [switch] $prep
    )

Begin
{
    $script:devenv = ''
    $script:vdproj = ''

    function FindVisualStudio
    {
        $cmd = Get-Command devenv -ErrorAction:SilentlyContinue
        if ($cmd -ne $null)
        {
            $script:devenv = $cmd.Source
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

    function Configure ([int]$bitness)
    {
        $lines = @(Get-Content $vdproj)

        $productVersion = $lines | `
            Where-Object { $_ -match '"ProductVersion" = "8:(.+?)"' } | `
            ForEach-Object { $matches[1] }

        Write-Host
        Write-Host "... configuring vdproj for x$bitness build of $productVersion" -ForegroundColor Yellow

        '' | Out-File $vdproj -nonewline

        $lines | ForEach-Object `
        {
            if ($_ -match '"OutputFileName" = "')
            {
                # "OutputFilename" = "8:Debug\\OneMore_v_Setupx86.msi"
                $line = $_.Replace('OneMore_v_', "OneMore_$($productVersion)_")
                if ($bitness -eq 64) {
                    $line.Replace('x86', 'x64') | Out-File $vdproj -Append
                } else {
                    $line.Replace('x64', 'x86') | Out-File $vdproj -Append
                }
            }
            elseif ($_ -match '"DefaultLocation" = "')
            {
                # "DefaultLocation" = "8:[ProgramFilesFolder][Manufacturer]\\[ProductName]"
                if ($bitness -eq 64) {
                    $_.Replace('ProgramFilesFolder', 'ProgramFiles64Folder') | Out-File $vdproj -Append
                } else {
                    $_.Replace('ProgramFiles64Folder', 'ProgramFilesFolder') | Out-File $vdproj -Append
                }
            }
            elseif ($_ -match '"TargetPlatform" = "')
            {
                # x86 -> "3:0"
                # x64 -> "3:1"
                if ($bitness -eq 64) {
                    '"TargetPlatform" = "3:1"' | Out-File $vdproj -Append
                } else {
                    '"TargetPlatform" = "3:0"' | Out-File $vdproj -Append
                }
            }
            elseif ($_ -match '"SourcePath" = .*WebView2Loader\.dll"$')
            {
                if ($bitness -eq 64) {
                    $_.Replace('\\x86', '\\x64') | Out-File $vdproj -Append
                } else {
                    $_.Replace('\\x64', '\\x86') | Out-File $vdproj -Append
                }
            }
            else
            {
                $_ | Out-File $vdproj -Append
            }
        }
    }

    function Build ([int]$bitness)
    {
        Write-Host "... building x$bitness MSI" -ForegroundColor Yellow

        # output file cannot exist before build
        if (Test-Path .\Debug\*)
        {
            Remove-Item .\Debug\*.* -Force -Confirm:$false
        }

        $cmd = "$devenv .\OneMoreSetup.vdproj /build ""Debug|x$bitness"" /project Setup /projectconfig Debug"
        write-Host $cmd -ForegroundColor DarkGray

        # build
        . $devenv .\OneMoreSetup.vdproj /build "Debug|x$bitness" /project Setup /projectconfig Debug

        # move msi to Downloads for safe-keeping and to allow next Platform build
        Move-Item .\Debug\*.msi $home\Downloads -Force
        Write-Host "... x$bitness MSI copied to $home\Downloads\" -ForegroundColor DarkYellow
    }
}
Process
{
    if (-not (FindVisualStudio))
    {
        return
    }
    
    if ($prep)
    {
        DisableOutOfProcBuild
        return
    }

    Push-Location OneMoreSetup
    $script:vdproj = Resolve-Path .\OneMoreSetup.vdproj

    PreserveVdproj

    if ($configbits -eq 86 -or $both)
    {
        Configure 86
        Build 86
    }

    if ($configBits -eq 64 -or $both)
    {
        Configure 64
        Build 64
    }

    RestoreVdproj
    Pop-Location
}
