<#
.SYNOPSIS
Build both x86 and x64 msi
#>

# CmdletBinding adds -Verbose functionality, SupportsShouldProcess adds -WhatIf
[CmdletBinding(SupportsShouldProcess = $true)]
param ()

Begin
{
    $script:devenv = ''
    $script:vdproj = ''

    function FindVisualStudio
    {
        $0 = 'C:\Program Files (x86)\Microsoft Visual Studio\2019'
        $script:devenv = Join-Path $0 'Enterprise\Common7\IDE\devenv.exe'

        if (!(Test-Path $devenv))
        {
            $script:devenv = Join-Path $0 'Professional\Common7\IDE\devenv.exe'
        }

        if (!(Test-Path $devenv))
        {
            Write-Host 'devenv not found' -ForegroundColor Yellow
            return $false
        }

        return $true
    }

    function PreserveVdproj
    {
        Copy-Item $vdproj .\vdproj.tmp -Force -Confirm:$false
    }

    function RestoreVdproj
    {
        Copy-Item .\vdproj.tmp $vdproj -Force -Confirm:$false
        Remove-Item .\vdproj.tmp -Force
    }

    function Configure ([bool]$target64)
    {
        $lines = @(Get-Content $vdproj)
        
        $productVersion = $lines | `
            Where-Object { $_ -match '"ProductVersion" = "8:(.+?)"' } | `
            ForEach-Object { $matches[1] }
        
        '' | Out-File $vdproj -nonewline

        $lines | ForEach-Object `
        {
            if ($_ -match '"OutputFileName" = "')
            {
                # "OutputFilename" = "8:Debug\\OneMore_v_Setupx86.msi"
                $line = $_.Replace('OneMore_v_', "OneMore_$($productVersion)_")
                if ($target64)
                {
                    $line = $line.Replace('x86', 'x64')
                }
                $line | Out-File $vdproj -Append
            }
            elseif ($target64 -and ($_ -match '"TargetPlatform" = "'))
            {
                # x86 -> "3:0"
                # x64 -> "3:1"
                '"TargetPlatform" = "3:1"' | Out-File $vdproj -Append
            }
            else
            {
                $_ | Out-File $vdproj -Append
            }
        }
    }
    
    function Build
    {
        & $devenv $vdproj /build Debug
        Move-Item .\Debug\*.msi $home\Downloads
    }
}
Process
{
    Push-Location OneMoreSetup
    $script:vdproj = Resolve-Path .\OneMoreSetup.vdproj

    if (FindVisualStudio)
    {
        PreserveVdproj

        Write-Host 'Building x86'
        Configure $false
        Build

        Write-Host 'Building x64'
        Configure $true
        Build

        RestoreVdproj
    }

    Pop-Location
}
