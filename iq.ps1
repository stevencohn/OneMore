<#
.SYNOPSIS
Installation qualification
#>

[CmdletBinding()]
param ()

Begin
{
    $script:guid = '{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}'
    $script:webview2client = '{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}'
    $script:modern = ($env:PROCESSOR_ARCHITECTURE -match '64')

    function WriteTitle
    {
        param($text)
        Write-Host "`n$text..." -Fore DarkCyan
    }

    function WriteOK
    {
        param($text)
        Write-Host 'OK ' -Fore Green -NoNewLine
        Write-Host $text
    }

    function WriteBad
    {
        param($text)
        Write-Host 'BAD ' -Fore Red -NoNewLine
        Write-Host $text -Fore Yellow
    }

    function HasKey
    {
        param($kpath)
        if (-not (Test-Path $kpath))
        {
            write-Host "key not found: $kpath" -Fore Red
            return $false
        }
        return $true
    }

    function HasProperty
    {
        param($kpath, $name)
        $ok = (Get-ItemProperty $kpath).PSObject.Properties.Name -contains $name
        if (-not $ok)
        {
            write-Host "property not found: $kpath\$name" -Fore Red
            return $false
        }
        return $true
    }

    function HasValue
    {
        param($kpath, $name, $value, [switch] $match, [switch] $equals)
        if (-not (HasProperty $kpath $name))
        {
            return $false
        }
        $script:lastValue = (Get-ItemPropertyValue -Path $kpath -Name $name)
        if ($value.Contains('*'))
        {
            if (-not ($lastValue -like $value))
            {
                Write-Host "invalid value: $kpath\$name, '$lastValue' <> '$value'" -Fore Red
                return $false
            }
        }
        else {
            if ($match) { if ($lastvalue -notmatch $value) {
                Write-Host "bad value: $kpath\$name, '$lastValue' !~ '$value'" -Fore Yellow
                return $false
            }}
            elseif ($lastvalue -ne $value) {
                Write-Host "bad value: $kpath\$name, '$lastValue' <> '$value'" -Fore Red
            }
        }
        return $true
    }
    
    function GetVersions
    {
        WriteTitle "Versions"
        $0 = "Registry::HKEY_CLASSES_ROOT\Excel.Application\CurVer"
        if (-not (HasKey $0)) {
            write-Host "cannot determine version of Office, assuming 16.0" -Fore Yellow
            $script:offversion = '16.0'
        } else {
            $parts = (Get-ItemPropertyValue -Path $0 -Name '(default)').Split('.')
            $script:offVersion = $parts[$parts.Length - 1] + ".0"
            WriteOK "Office version is $offVersion"
        }

        $0 = "Registry::HKEY_CLASSES_ROOT\OneNote.Application\CurVer"
        if (-not (HasKey $0)) {
            write-Host "cannot determine version of OneNote"
        } else {
            $parts = (Get-ItemPropertyValue -Path $0 -Name '(default)').Split('.')
            $script:oneVersion = $parts[$parts.Length - 1] + ".0"
            WriteOK "OneNote version is $oneVersion"
        }
    }

    function CheckAppID
    {
        WriteTitle "AppID"
        $0 = "Registry::HKEY_CLASSES_ROOT\AppID\$guid"
        $ok = (HasKey $0)
        if ($ok) { $ok = (HasValue $0 'DllSurrogate' '') }
        if ($ok) { WriteOK $0 } else { WriteBad $0 }
    }

    function CheckRoot
    {
        WriteTitle "Root"
        $0 = "Registry::HKEY_CLASSES_ROOT\onemore"
        $ok = (HasKey $0)
        if ($ok) {
            $ok = (HasValue $0 '(default)' 'URL:OneMore Protocol Handler') -and $ok
            $ok = (HasValue $0 'URL Protocol' '') -and $ok
        }
        if ($ok) { WriteOK $0 } else { WriteBad $0 }
    }

    function CheckShell
    {
        WriteTitle "Shell"
        # this also covers the virtual node LOCAL_MACHINE\SOFTWARE\Classes\onemore\shell\open\command
        $0 = "Registry::HKEY_CLASSES_ROOT\onemore\shell\open\command"
        $ok = (HasKey $0)
        if ($ok) { $ok = (HasValue $0 '(default)' '\\OneMoreProtocolHandler.exe"? %1 %2 %3 %4 %5' -match) }
        if ($ok) { WriteOK "$0" } else { WriteBad $0 }
        Write-Verbose $lastvalue
    }

    function CheckAddIn
    {
        WriteTitle "AddIn"
        $0 = "Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn"
        $ok = (HasKey $0)
        if ($ok) {
            $ok = (HasValue $0 '(default)' 'River.OneMoreAddIn.AddIn') -and $ok
            $1 = "$0\CLSID"
            $ok = (HasValue $1 '(default)' $guid) -and $ok
            $1 = "$0\CurVer"
            $ok = (HasValue $1 '(default)' 'River.OneMoreAddIn.1') -and $ok
        }
        if ($ok) { WriteOK "$0" } else { WriteBad $0 }

        $0 = "Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn.1"
        $ok = (HasValue $0 '(default)' 'Addin class')
        $1 = "$0\CLSID"
        $ok = (HasValue $1 '(default)' $guid) -and $ok
        if ($ok) { WriteOK "$0" } else { WriteBad $0 }
    }

    function CheckCLSID
    {
        WriteTitle "CLSID"
        $0 = "Registry::HKEY_CLASSES_ROOT\CLSID\$guid"
        $ok = (HasKey $0)
        if ($ok) {
            $ok = (HasValue $0 '(default)' 'River.OneMoreAddIn.AddIn')
            $ok = (HasValue $0 'AppID' $guid) -and $ok
        }
        if ($ok) { WriteOK $0 } else { WriteBad $0 }

        $1 = "$0\Implemented Categories\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}"
        $ver = $null
        $ok = (HasKey $0)
        if ($ok) {
            $1 = "$0\InprocServer32"
            $ok = (HasKey $1)
            if ($ok) {
                $ok = (HasValue $1 '(default)' 'mscoree.dll')
                $ok = (HasValue $1 'ThreadingModel' 'Both') -and $ok
                
                $ok = (HasValue $1 'CodeBase' '*\River.OneMoreAddIn.dll') -and $ok
                if ($ok) { $script:codeBase = $lastValue }

                $oo = (HasValue $1 'Class' 'River.OneMoreAddIn.AddIn')
                if ($oo) { $script:class = $lastValue }
                $ok = $oo -and $ok

                $oo = (HasValue $1 'RuntimeVersion' 'v*')
                if ($oo) { $script:runtimeVersion = $lastValue }
                $ok = $oo -and $ok

                $oo = (HasValue $1 'Assembly' 'River.OneMoreAddIn, Version=*')
                if ($oo) { $script:assembly = $lastValue }
                $ok = $oo -and $ok

                if ($oo)
                {
                    if ($assembly -match ',\sVersion=([0-9\.]+),\s')
                    {
                        $ver = $matches[1]
                    }
                }
            }
        }
        if ($ok) { WriteOK $1 } else { WriteBad $1 }

        if ($ver)
        {
            $1 = "$0\InprocServer32\$ver"
            $ok = (HasValue $1 'Assembly' $assembly)
            $ok = (HasValue $1 'CodeBase' $codeBase) -and $ok
            $ok = (HasValue $1 'RuntimeVersion' $runtimeVersion) -and $ok
            $ok = (HasValue $1 'Class' $class) -and $ok
            if ($ok) { WriteOK $1 } else { WriteBad $1 }
        }
        else
        {
            Write-Host "skipping $0\InprocServer32\<version>" -Fore Yellow
        }

        Write-Verbose "Assembly = $assembly"
        Write-Verbose "CodeBase = $codeBase"
        Write-Verbose "RuntimeVersion = runtimeVersion"
        Write-Verbose "Class = $class"

        $1 = "$0\ProgID"
        $ok = (HasKey $1)
        if ($ok) { $ok = (HasValue $1 '(default)' 'River.OneMoreAddIn') }
        if ($ok) { WriteOK $1 } else { WriteBad $1 }

        $1 = "$0\Programmable"
        $ok = (HasKey $1)
        if ($ok) { $ok = (HasValue $1 '(default)' '') }
        if ($ok) { WriteOK $1 } else { WriteBad $1 }

        $1 = "$0\TypeLib"
        $ok = (HasKey $1)
        if ($ok) { $ok = (HasValue $1 '(default)' $guid) }
        if ($ok) { WriteOK $1 } else { WriteBad $1 }

        $1 = "$0\VersionIndependentProgID"
        $ok = (HasKey $1)
        if ($ok) { $ok = (HasValue $1 '(default)' 'River.OneMoreAddIn') }
        if ($ok) { WriteOK $1 } else { WriteBad $1 }
    }

    function CheckUser
    {
        WriteTitle "User"
        $0 = "Registry::HKEY_CURRENT_USER\SOFTWARE\Classes\AppID\$guid"
        $ok = (HasKey $0)
        if ($ok) { $ok = (HasValue $0 'DllSurrogate' '') }
        if ($ok) { WriteOK $0 } else { WriteBad $0 }

        $0 = "Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\OneNote\AddIns\River.OneMoreAddIn"
        $ok = (HasValue $0 'LoadBehavior' '3')
        $ok = (HasValue $0 'Description' 'Extension for OneNote') -and $ok
        $ok = (HasValue $0 'FriendlyName' 'OneMoreAddIn') -and $ok
        if ($ok) { WriteOK $0 } else { WriteBad $0 }

        $0 = "Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\River.OneMoreAddIn.dll"
        $ok = (HasValue $0 'Path' $codeBase)
        if ($ok) { WriteOK $0 } else { WriteBad $0 }
        Write-Verbose $lastvalue

        $0 = "Registry::HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Office\$offVersion\Common\Security\Trusted Protocols\All Applications\onemore:"
        $ok = (HasKey $0)
        if ($ok) { WriteOK $0 } else { WriteBad $0 }
    }

    function CheckWebView2
    {
        WriteTitle "WebView2"

        # either of these keys need to be defined, per
        # > https://docs.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution

        if ($modern) {
            $0 = "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\$webview2client"
        } else {
            $0 = "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\EdgeUpdate\Clients\$webview2client"
        }

        $ok = (checkWebView2Entry $0)
        if (-not $ok) {
            Write-Host '... checking CURRENT_USER' -Fore Yellow
            $0 = "Registry::HKEY_CURRENT_USER\Software\Microsoft\EdgeUpdate\Clients\$webview2client"
            $ok = (checkWebView2Entry $0)
        }

        if (-not $ok) { WriteBad 'WebView2 not installed' }
    }

    function checkWebView2Entry
    {
        param($path)
        $ok = (HasKey $path)
        if ($ok) {
            $ok = (HasValue $path 'pv' '[^0\.0\.0\.0]' -match)
            if ($ok) {
                $location = (Get-ItemPropertyValue -Path $path -Name 'location')
                $ok = (Test-Path $location)
                if ($ok) {
                    WriteOK $path
                    WriteOK "location: $location"
                } else {
                    WriteBad $path
                    Write-Host "... location not found $location" -Fore Yellow
                }
                Write-Verbose "version = $lastvalue"
                Write-Verbose "location = $location"
            } else {
                WriteBad $path
                Write-Host "... has version 0.0.0.0" -Fore Yellow
            }
        }
        return $ok
    }
}
Process
{
    $vcolor = $Host.PrivateData.VerboseForegroundColor
    $Host.PrivateData.VerboseForegroundColor = 'DarkGray'

    GetVersions
    CheckAppID
    CheckRoot
    CheckShell
    CheckAddIn
    CheckCLSID
    CheckUser
    CheckWebView2

    $Host.PrivateData.VerboseForegroundColor = $vcolor
}
