<#
.SYNOPSIS
Update the OneMore.csproj reference hints to point to the latest installed version
of the Windows SDK.

.DESCRIPTION
Values in the csproj may contain MSBuild properties such as $(windir). Before comparing
current and desired values, those properties are expanded the same way MSBuild would
(environment variables are properties), so equivalent values are not reported as changes.
Every entry reports its before and after values, in raw and expanded form.

.COPYRIGHT
Copyright © 2016 Steven M Cohn. All rights reserved.
#>

param(
    [Alias('dryrun')]
    [switch] $WhatIf
)

Begin
{
    $script:DOSDevicesKey = 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\DOS Devices'

    $script:csproj = $null
    $script:kitsroot = $null
    $script:sdkver = $null
    $script:netpath = $null
    $script:basePath = $null
    $script:warned = @{}
    $script:results = @()


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
            Write-Host "mapped drive $letter -> $basePath" -ForegroundColor DarkGray
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

        # preserve the BOM and line endings of the csproj when writing it back
        $bytes = [System.IO.File]::ReadAllBytes($csproj)
        $hasBom = ($bytes.Length -ge 3) -and ($bytes[0] -eq 0xEF) -and ($bytes[1] -eq 0xBB) -and ($bytes[2] -eq 0xBF)
        $raw = [System.IO.File]::ReadAllText($csproj)
        $eol = $raw.Contains("`r`n") ? "`r`n" : "`n"
        $lines = $raw -split '\r?\n'

        for ($i = 0; $i -lt $lines.Count; $i++)
        {
            $line = $lines[$i]

            # $matches[1]=hint path, $matches[2]=path up to and including \Framework64\vN.N.N\
            if ($line -match '<HintPath>((.+\\Framework64\\v\d+\.\d+\.\d+\\)System\.Runtime\.WindowsRuntime\.dll)</HintPath>')
            {
                $old = $matches[1]
                $oldDir = $matches[2]

                # express the runtime directory using $(windir) as the csproj does
                $new = $netpath -ireplace [regex]::Escape($env:SystemRoot), '$$(windir)'

                $changed = PatchEntry $i $line '.NET Framework path' $oldDir $new
                if ($changed)
                {
                    $lines[$i] = $line.Replace($oldDir, $new)
                    $updated = $true
                }
            }
            # $matches[1]=hint path, $matches[2]=version
            elseif ($line -match '<HintPath>(.+\\UnionMetadata\\(\d+\.\d+\.\d+\.\d+)\\Windows\.winmd)</HintPath>')
            {
                $old = $matches[1]
                $oldver = $matches[2]

                $new = Join-Path $kitsroot "UnionMetadata\$sdkver\Windows.winmd"

                $reason = $null
                if ($oldver -ne $sdkver)
                {
                    $reason = "SDK version $oldver -> $sdkver"
                }
                elseif (!(Test-Path (ExpandPath $old) -PathType Leaf))
                {
                    $reason = 'referenced file does not exist'
                }

                $changed = PatchEntry $i $line 'Windows SDK path' $old $new $reason
                if ($changed)
                {
                    $lines[$i] = $line.Replace($old, $new)
                    $updated = $true
                }
            }
        }

        # Updating $lines in memory is super fast. Also, Out-File -Append seems to have a problem
        # where it skips lines because it thinks the csproj is locked, probably if VS is open

        if ($updated -and -not $WhatIf)
        {
            $encoding = New-Object System.Text.UTF8Encoding($hasBom)
            [System.IO.File]::WriteAllText($csproj, ($lines -join $eol), $encoding)
        }

        return $updated
    }


    # Compare the current and desired values of one entry and report before/after details.
    # Returns $true if the csproj value should be replaced.
    function PatchEntry
    {
        param($index, $line, $label, $old, $new, $reason = $null)

        $oldFull = ExpandPath $old
        $newFull = ExpandPath $new

        $same = $oldFull -ieq $newFull
        $missing = $reason -eq 'referenced file does not exist'

        # equivalent when the expanded values match and nothing else is wrong; a missing file
        # with an identical value can't be fixed by rewriting it so just warn
        if ($same -and -not $reason)
        {
            WriteOK "$label is already correct (line $($index + 1))"
            Write-Host "     value   : $old" -ForegroundColor DarkGray
            if ($old -ne $new)
            {
                Write-Host "     equals  : $new (same after expanding properties)" -ForegroundColor DarkGray
            }
            if ($old -ne $oldFull)
            {
                Write-Host "     expands : $oldFull" -ForegroundColor DarkGray
            }
            $script:results += [pscustomobject]@{ Entry = $label; Status = 'unchanged' }
            return $false
        }

        if ($same -and $missing)
        {
            Write-Host "!! $label $oldFull does not exist and the SDK offers no other path" -ForegroundColor Red
            $script:results += [pscustomobject]@{ Entry = $label; Status = 'missing' }
            return $false
        }

        if (-not $reason)
        {
            $reason = 'path differs'
        }

        WriteOK "updating$dry $label (line $($index + 1))"
        Write-Host "     reason  : $reason" -ForegroundColor Yellow
        Write-Host "     before  : $old" -ForegroundColor Red
        if ($old -ne $oldFull)
        {
            Write-Host "     expands : $oldFull" -ForegroundColor DarkGray
        }
        Write-Host "     after   : $new" -ForegroundColor Green
        if ($new -ne $newFull)
        {
            Write-Host "     expands : $newFull" -ForegroundColor DarkGray
        }

        $script:results += [pscustomobject]@{ Entry = $label; Status = "updated$dry" }
        return $true
    }


    # Expand MSBuild properties like $(windir) and normalize for comparison
    function ExpandPath
    {
        param($text)

        $p = ExpandMSBuild $text
        if (-not [System.IO.Path]::IsPathRooted($p))
        {
            $p = Join-Path $basePath $p
        }

        return [System.IO.Path]::GetFullPath($p).TrimEnd('\')
    }


    # MSBuild exposes environment variables as (case-insensitive) properties, e.g. $(windir),
    # $(ProgramFiles(x86)); unknown properties are left as-is and warned about once
    function ExpandMSBuild
    {
        param($text)

        return [regex]::Replace($text, '\$\(([^()]+(?:\([^()]*\))?)\)', {
            param($m)
            $value = [System.Environment]::GetEnvironmentVariable($m.Groups[1].Value)
            if ($null -ne $value)
            {
                return $value
            }

            if (-not $script:warned.ContainsKey($m.Value))
            {
                $script:warned[$m.Value] = $true
                Write-Host "!! unknown MSBuild property $($m.Value), comparing as-is" -ForegroundColor Yellow
            }

            return $m.Value
        })
    }
}
Process
{
    Push-Location OneMore
    GetDriveMapping
    $script:csproj = (Resolve-Path .\OneMore.csproj).Path

    $script:dry = $WhatIf ? ' (dry run)' : ''

    $script:sdkver = GetSDKVersion
    if ($sdkver)
    {
        Write-Host "`nUpdating$dry Windows SDK $sdkver " -NoNewline
        Write-Host "($kitsroot)" -ForegroundColor DarkGray
        Write-Host
        Write-Host "Framework dir: $netpath" -ForegroundColor DarkGray
        Write-Host "Project: $csproj`n" -ForegroundColor DarkGray

        if (PatchReferences)
        {
            Write-Host
            $results | Format-Table -AutoSize | Out-Host
            WriteOK 'done'
        }
        else
        {
            Write-Host
            $results | Format-Table -AutoSize | Out-Host
            WriteOK 'no changes required'
        }
    }

    Pop-Location
}
