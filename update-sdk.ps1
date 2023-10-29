<#
.SYNOPSIS
Update the OneMore.csproj reference hints to point to the latest installed version
of the Windows SDK.
#>

param ()

Begin
{
    $script:DOSDevicesKey = 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\DOS Devices'

    $script:csproj = $null
    $script:kitsroot = $null
    $script:sdkver = $null
    $script:netpath = $null
    $script:basePath = $null
    $script:forceBase = $false


    function WriteOK
    {
        param($text)
        Write-Host 'OK ' -Fore Green -NoNewLine
        Write-Host $text
    }


    function GetDriveMapping
    {
        # if the current drive is a virtual drive letter (persistent mapping) then the calls to
        # Resolve-Path will not work, so we need to get the actual target of the drive

        $letter = [System.IO.Path]::GetPathRoot($pwd.path)[0] + ':'
        $map = Get-ItemProperty $DOSDevicesKey | `
            Select-Object $letter -Expand $letter -ErrorAction 'SilentlyContinue'

        if ($map -ne $null)
        {
            # \??\C:\Development
            $script:basePath = $map.Substring(4)
            $script:forceBase = $true
        }
        else
        {
            $script:basePath = [System.IO.Path]::GetPathRoot($env:SystemRoot)
        }
    }


    function GetSDKVersion
    {
        # use powershell explicitly because pwsh will return wrong path
        $script:netpath = (powershell -c '[System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()')

        $0 = 'Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Kits\Installed Roots'
        if (!(Test-Path $0))
        {
            Write-Host 'Windows SDK not found!' -ForegroundColor Red
            return $null
        }

        $script:kitsroot = (Get-ItemPropertyValue -Path $0 -Name 'KitsRoot10')

        return (Get-ChildItem $0 -Name | Select-Object -Last 1)
    }


    function PatchReferences
    {
        $updated = $false

        $lines = @(Get-Content $csproj)
        
        for ($i =0; $i -lt $lines.Count; $i++)
        {
            $line = $lines[$i]
            $handled = $false

            # $matches[1]=netpath
            if ($line -match '<HintPath>(.+\\Framework64\\v\d+\.\d+\.\d+\\)System.Runtime.WindowsRuntime.dll</HintPath>')
            {
                $p = [System.IO.Path]::GetFullPath($basePath + $matches[1].ToString())
                if (($p.ToString() -ne $netpath) -or $forceBase)
                {
                    WriteOK "updating .NET Framework path: $netpath"
                    $lines[$i] = $line.Replace($matches[1], $netpath)
                    $handled = $true
                }
                else {
                    WriteOK 'NET Framework path is already correct'
                }
            }
            # $matches[1]=sdkpath, $matches[2]=version
            elseif ($line -match '<HintPath>(.+\\)UnionMetadata\\(\d+\.\d+\.\d+\.\d+)\\Windows.winmd</HintPath>')             
            {
                $p = "$($matches[1])UnionMetadata\\$($matches[2])\\Windows.winmd"
                if (($matches[2] -ne $sdkver) -or ![System.IO.Directory]::Exists($p))
                {
                    # always forces base
                    $fullpath = $kitsroot; 'UnionMetadata', $sdkver, 'Windows.winmd' | `
                        foreach { $fullpath = Join-Path $fullpath $_ }

                    WriteOK "patching Windows SDK path: $fullpath"
                    $lines[$i] = "<HintPath>$fullpath</HintPath>"
                    $handled = $true
                }
                else
                {
                    WriteOK 'Windows SDK is already correct'
                }
            }

            if ($handled)
            {
                $updated = $handled
            }
        }

        # Updating $lines in memory is super fast. Also, Out-File -Append seems to have a problem
        # where it skips lines because it thinks the csproj is locked, probably if VS is open

        if ($updated)
        {
            $lines | Out-File $csproj
        }

        return $updated
    }
}
Process
{
    Push-Location OneMore
    GetDriveMapping
    $script:csproj = Resolve-Path .\OneMore.csproj

    $script:sdkver = GetSDKVersion
    if ($sdkver) 
    {
        Write-Host "`nUpdating Windows SDK $sdkver " -NoNewline
        write-Host "($kitsroot)`n" -ForegroundColor DarkGray

        if (PatchReferences) {
            WriteOK 'done'
        }
        else {
            WriteOK 'no changes required'
        }
    }

    Pop-Location
}
