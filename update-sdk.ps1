<#
.SYNOPSIS
Update the OneMore.csproj reference hints to point to the latest installed version
of the Windows SDK.
#>

param ()

Begin
{
    $script:csproj = $null
    $script:kitsroot = $null
    $script:sdkver = $null
    $script:netpath = $null

    function WriteOK
    {
        param($text)
        Write-Host 'OK ' -Fore Green -NoNewLine
        Write-Host $text
    }

    function GetSDKVersion
	{
        $script:netpath = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()

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

        $fullpath = $kitsroot; 'UnionMetadata', $sdkver, 'Windows.winmd' | `
            ForEach-Object { $fullpath = Join-Path $fullpath $_ }

        $lines = @(Get-Content $csproj)

        for ($i =0; $i -lt $lines.Count; $i++)
        {
            $line = $lines[$i]
            $handled = $false

            # $matches[1]=netpath
            if ($line -match '<HintPath>(.+\\Framework64\\v\d+\.\d+\.\d+\\)System.Runtime.WindowsRuntime.dll</HintPath>')
            {
                $p = (Resolve-Path $matches[1])
                if ($p.ToString() -ne $netpath)
                {
                    WriteOK "updating .NET Framework path: $netpath"
                    $lines[$i] = $line.Replace($matches[1], $netpath)
                    $handled = $true
                }
                else {
                    WriteOK 'NET Framework path is already correct'
                }
            }
            # $matches[1]=sdkpath, $matches[2]==version
            elseif ($line -match '<HintPath>(.+\\)UnionMetadata\\(\d+\.\d+\.\d+\.\d+)\\Windows.winmd</HintPath>')             
            {
                $p = (Resolve-Path $matches[1])
 
                if (!(Test-Path $p))
                {
                    # replace relative path with absolute path
                    WriteOK "applying full path to Windows SDK: $fullpath"
                    $lines[$i] = "<HintPath>$fullpath</HintPath>"
                    $handled = $true
                }
                elseif ($matches[2] -ne $sdkver)
                {
                    WriteOK "patching Windows SDK version: $sdkver"
                    $lines[$i] = $line.Replace($matches[2], $sdkver)
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

        $lines | Out-File $csproj

        return $updated
	}
}
Process
{
    Push-Location OneMore
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
