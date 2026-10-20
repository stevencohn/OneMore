<#
.SYNOPSIS
Installation qualification

.COPYRIGHT
Copyright © 2016 Steven M Cohn. All rights reserved.
#>

[CmdletBinding()]
param ()

Begin
{
    $script:guid = '{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}'
    $script:webview2client = '{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}'
    # CLSID of OneMoreProtocolHandler.CommandHandler (see ComServer.ClassId)
    $script:handlerClsid = '{6F3A9B52-4C1D-4E8A-9D07-2B5E71C0A9F4}'
    $script:modern = ($env:PROCESSOR_ARCHITECTURE -match '64')
    $script:onewow = $false

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

    function WriteValue
    {
        param($text)
        if ($verbose) {
            if ($text) {
                Write-Host "   OK " -Fore DarkGreen -NoNewline
                Write-Host $text -Fore DarkGray
            } else {
                Write-Host "   BAD" -Fore DarkRed
            }
        }
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
        WriteTitle 'Versions'
        $0 = 'Registry::HKEY_CLASSES_ROOT\Excel.Application\CurVer'
        if (-not (HasKey $0)) {
            write-Host 'cannot determine version of Office, assuming 16.0' -Fore Yellow
            $script:offversion = '16.0'
        } else {
            $parts = (Get-ItemPropertyValue -Path $0 -Name '(default)').Split('.')
            $script:offVersion = $parts[$parts.Length - 1] + '.0'
            WriteOK "Office version is $offVersion"
        }

        $0 = 'Registry::HKEY_CLASSES_ROOT\onenote\shell\Open\command'
        if (-not (HasKey $0)) {
            write-Host 'cannot determine shell command path of OneNote'
        } else {
            $script:onewow = (Get-ItemPropertyValue -Path $0 -Name '(default)'
                ).Contains('Program Files (x86)')
        }

        $0 = 'Registry::HKEY_CLASSES_ROOT\OneNote.Application\CurVer'
        if (-not (HasKey $0)) {
            write-Host 'cannot determine version of OneNote'
        } else {
            $parts = (Get-ItemPropertyValue -Path $0 -Name '(default)').Split('.')
            $script:oneVersion = $parts[$parts.Length - 1] + '.0'
            if ($onewow) {
                WriteOK "OneNote version is $oneVersion (32-bit)"
            } else {
                WriteOK "OneNote version is $oneVersion (64-bit)"
            }
        }
    }

    function CheckAppID
    {
        WriteTitle 'AppID'
        $0 = "Registry::HKEY_CLASSES_ROOT\AppID\$guid"
        $ok = (HasKey $0)
        if ($ok) { $ok = (HasValue $0 'DllSurrogate' '') }
        if ($ok) { $dllSurrogate = $lastValue }
        if ($ok) { WriteOK $0 } else { WriteBad $0 }
        WriteValue "DllSurrogate = $dllSurrogate"

        # LaunchPermission is REG_BINARY — decode to SDDL for human-readable output.
        # Absence means COM surrogate launch will be denied for non-admin users on ARM64
        # (DCOM default is more restrictive than on x64).
        if ($ok)
        {
            if (HasProperty $0 'LaunchPermission')
            {
                try
                {
                    $lpBytes = (Get-ItemPropertyValue -Path $0 -Name 'LaunchPermission')
                    $sd = [System.Security.AccessControl.RawSecurityDescriptor]::new($lpBytes, 0)
                    $sddl = $sd.GetSddlForm([System.Security.AccessControl.AccessControlSections]::All)
                    if ($sddl -eq 'O:BAG:BAD:(A;;CCDCSW;;;AU)(A;;CCDCSW;;;SY)(A;;CCDCSW;;;BA)') {
                        WriteValue "LaunchPermission = $sddl"
                    } else {
                        WriteBad "LaunchPermission = (unexpected SDDL: $sddl)"
                    }
                }
                catch
                {
                    WriteValue "LaunchPermission = (could not decode: $($_.Exception.Message))"
                }
            }
            else
            {
                WriteBad 'LaunchPermission missing (ARM64 non-admin loads will fail)'
            }
        }
    }

    function CheckRoot
    {
        WriteTitle 'Root'
        $0 = 'Registry::HKEY_CLASSES_ROOT\onemore'
        $ok = (HasKey $0)
        if ($ok) {
            $ok = (HasValue $0 '(default)' 'URL:OneMore Protocol Handler') -and $ok
            if ($ok) { $defaultValue = $lastValue }
            $ok = (HasValue $0 'URL Protocol' '') -and $ok
            if ($ok) { $urlProtocol = $lastValue }
        }
        if ($ok) { WriteOK $0 } else { WriteBad $0 }

        WriteValue "@ = $defaultValue"; $defaultValue = $null
        WriteValue "URL Protocol = $urlProtocol"
    }

    function CheckShell
    {
        WriteTitle 'Shell'
        # this also covers the virtual node LOCAL_MACHINE\SOFTWARE\Classes\onemore\shell\open\command
        $0 = 'Registry::HKEY_CLASSES_ROOT\onemore\shell\open\command'
        $ok = (HasKey $0)
        if ($ok) { $ok = (HasValue $0 '(default)' '\\OneMoreProtocolHandler.exe"? %1 %2 %3 %4 %5' -match) }
        if ($ok) { $defaultValue = $lastValue }
        if ($ok) { WriteOK "$0" } else { WriteBad $0 }

        WriteValue "@ = $defaultValue"; $defaultValue = $null
    }

    function CheckProtocolMode
    {
        WriteTitle 'Protocol activation'
        $0 = 'Registry::HKEY_CLASSES_ROOT\onemore\shell\open\command'
        $delegate = $null
        if (Test-Path $0) { $delegate = (Get-ItemProperty $0).DelegateExecute }

        # the installer saves the requested mode so an upgrade can restore it; absent for
        # dev setups (setregistry.ps1) and installs that predate it
        $saved = $null
        $mk = 'Registry::HKEY_LOCAL_MACHINE\SOFTWARE\River\OneMoreAddIn'
        if (Test-Path $mk) { $saved = (Get-ItemProperty $mk).ProtocolMode }
        $live = if ($delegate) { 'COM' } else { 'EXE' }
        if ($saved -and $saved -ne $live)
        {
            Write-Host "... saved mode ($saved) differs from live registration ($live)" -Fore Yellow
        }
        WriteValue "saved mode = $(if ($saved) { $saved } else { '(none)' })"

        if (-not $delegate)
        {
            WriteOK 'EXE mode; shell runs OneMoreProtocolHandler.exe as a child of OneNote'
            Write-Host '... blocked where Defender ASR prevents Office child processes; use COM mode there' -Fore DarkGray
            return
        }

        if ($delegate -ne $handlerClsid)
        {
            WriteBad "DelegateExecute = $delegate (expected $handlerClsid)"
            return
        }

        WriteOK "COM mode; DelegateExecute = $delegate"

        $views = @('CLSID')
        if ($onewow) { $views += 'WOW6432Node\CLSID' }

        foreach ($view in $views)
        {
            $1 = "Registry::HKEY_CLASSES_ROOT\$view\$handlerClsid"
            $ok = (HasKey $1)
            if (-not $ok) { WriteBad $1; continue }

            if (Test-Path "$1\InprocServer32")
            {
                # InprocServer32 wins over LocalServer32 and loads the CLR into OneNote
                WriteBad "$1\InprocServer32 (stale; remove it)"
                continue
            }

            $2 = "$1\LocalServer32"
            $ok = (HasKey $2)
            if ($ok) { $ok = (HasValue $2 '(default)' '\\OneMoreProtocolHandler.exe"?$' -match) }
            if ($ok)
            {
                $server = $lastValue.Trim('"')
                $ok = (Test-Path $server)
                if (-not $ok) { Write-Host "... server not found $server" -Fore Yellow }
            }

            if ($ok) { WriteOK $2 } else { WriteBad $2 }
            WriteValue "@ = $lastValue"
        }
    }

    function CheckAddIn
    {
        WriteTitle 'AddIn'
        $0 = 'Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn'
        $ok = (HasKey $0)
        if ($ok) {
            $ok = (HasValue $0 '(default)' 'River.OneMoreAddIn.AddIn') -and $ok
            $1 = "$0\CLSID"
            $ok = (HasValue $1 '(default)' $guid) -and $ok
            $1 = "$0\CurVer"
            $ok = (HasValue $1 '(default)' 'River.OneMoreAddIn.1') -and $ok
        }
        if ($ok) { WriteOK "$0" } else { WriteBad $0 }

        $0 = 'Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn.1'
        $ok = (HasValue $0 '(default)' 'Addin class')
        $1 = "$0\CLSID"
        $ok = (HasValue $1 '(default)' $guid) -and $ok
        if ($ok) { WriteOK "$0" } else { WriteBad $0 }
    }

    function CheckCLSID
    {
        param([boolean] $wow = $false)

        if ($wow) { $clsid = 'WOW6432Node\CLSID' } else { $clsid = 'CLSID' }

        WriteTitle $clsid
        $0 = "Registry::HKEY_CLASSES_ROOT\$clsid\$guid"
        $ok = (HasKey $0)
        if ($ok) {
            $ok = (HasValue $0 '(default)' 'River.OneMoreAddIn.AddIn')
            if ($ok) { $defaultValue = $lastValue }
            $ok = (HasValue $0 'AppID' $guid) -and $ok
            if ($ok) { $appId = $lastValue }
        }
        if ($ok) { WriteOK $0 } else { WriteBad $0 ; return }
        WriteValue "@ = $defaultValue"; $defaultValue = $null
        WriteValue "AppID = $appId"

        $1 = "$0\Implemented Categories\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}"
        $ver = $null
        $ok = (HasKey $0)
        if ($ok) {
            $1 = "$0\InprocServer32"
            $ok = (HasKey $1)
            if ($ok) {
                $ok = (HasValue $1 '(default)' 'mscoree.dll')
                if ($ok) { $defaultValue = $lastValue }
                $ok = (HasValue $1 'ThreadingModel' 'Both') -and $ok
                if ($ok) { $threadingModel = $lastValue }
                
                $ok = (HasValue $1 'CodeBase' '*\River.OneMoreAddIn.dll') -and $ok
                if ($ok) { $script:codeBase = $lastValue }

                $oo = (HasValue $1 'Class' 'River.OneMoreAddIn.AddIn')
                if ($oo) { $class = $lastValue }
                $ok = $oo -and $ok

                $oo = (HasValue $1 'RuntimeVersion' 'v*')
                if ($oo) { $runtimeVersion = $lastValue }
                $ok = $oo -and $ok

                $oo = (HasValue $1 'Assembly' 'River.OneMoreAddIn, Version=*')
                if ($oo) { $assembly = $lastValue }
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
        WriteValue "@ = $defaultValue"; $defaultValue = $null
        WriteValue "Assembly = $assembly"
        WriteValue "Class = $class"
        WriteValue "CodeBase = $codeBase"
        WriteValue "RuntimeVersion = $runtimeVersion"
        WriteValue "ThreadingModel = $threadingModel"

        if ($ver)
        {
            $1 = "$0\InprocServer32\$ver"
            $ok = (HasValue $1 'Assembly' $assembly)
            if ($ok) { $assembly = $lastValue }
            $ok = (HasValue $1 'CodeBase' $codeBase) -and $ok
            if ($ok) { $codeBase = $lastValue }
            $ok = (HasValue $1 'RuntimeVersion' $runtimeVersion) -and $ok
            if ($ok) { $runtimeVersion = $lastValue }
            $ok = (HasValue $1 'Class' $class) -and $ok
            if ($ok) { $class = $lastValue }
            if ($ok) { WriteOK $1 } else { WriteBad $1 }

            WriteValue "Assembly = $assembly"
            WriteValue "Class = $class"
            WriteValue "CodeBase = $codeBase"
            WriteValue "RuntimeVersion = $runtimeVersion"
        }
        else
        {
            Write-Host "skipping $0\InprocServer32\<version>" -Fore Yellow
        }

        $1 = "$0\ProgID"
        $ok = (HasKey $1)
        if ($ok) { $ok = (HasValue $1 '(default)' 'River.OneMoreAddIn') }
        if ($ok) { $defaultValue = $lastValue }
        if ($ok) { WriteOK $1 } else { WriteBad $1 }
        WriteValue "@ = $defaultValue"; $defaultValue = $null

        $1 = "$0\Programmable"
        $ok = (HasKey $1)
        if ($ok) { $ok = (HasValue $1 '(default)' '') }
        if ($ok) { $defaultValue = $lastValue }
        if ($ok) { WriteOK $1 } else { WriteBad $1 }
        WriteValue "@ = $defaultValue"; $defaultValue = $null

        $1 = "$0\TypeLib"
        $ok = (HasKey $1)
        if ($ok) { $ok = (HasValue $1 '(default)' $guid) }
        if ($ok) { $defaultValue = $lastValue }
        if ($ok) { WriteOK $1 } else { WriteBad $1 }
        WriteValue "@ = $defaultValue"; $defaultValue = $null

        $1 = "$0\VersionIndependentProgID"
        $ok = (HasKey $1)
        if ($ok) { $ok = (HasValue $1 '(default)' 'River.OneMoreAddIn') }
        if ($ok) { $defaultValue = $lastValue }
        if ($ok) { WriteOK $1 } else { WriteBad $1 }
        WriteValue "@ = $defaultValue"; $defaultValue = $null
    }

    function CheckMachine
    {
        # Machine-wide AddIns registration — the fallback OneNote uses when HKCU
        # is missing (e.g., Intune/SYSTEM deployments where Active Setup never ran).
        WriteTitle 'Machine'
        $0 = 'Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Office\OneNote\AddIns\River.OneMoreAddIn'

        # only the MSI writes this key; it is just a fallback, so when the per-user key
        # exists (as on dev machines using setregistry.ps1) its absence is only a warning
        $userKey = 'Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\OneNote\AddIns\River.OneMoreAddIn'
        if (-not (Test-Path $0) -and (Test-Path $userKey))
        {
            Write-Host 'WARN ' -Fore Yellow -NoNewLine
            Write-Host "$0 (not installed; HKCU registration covers this machine)"
            return
        }

        $ok = (HasKey $0)
        if ($ok)
        {
            $ok = (HasValue $0 'LoadBehavior' '3') -and $ok
            if ($Ok) { $loadBehavior = $lastValue }
            $ok = (HasValue $0 'Description' 'Add-in for OneNote') -and $ok
            if ($Ok) { $description = $lastValue }
            $ok = (HasValue $0 'FriendlyName' 'OneMoreAddIn') -and $ok
            if ($Ok) { $friendlyName = $lastValue }
        }
        if ($ok) { WriteOK $0 } else { WriteBad $0 }

        WriteValue "LoadBehavior = $loadBehavior"
        WriteValue "Description = $description"
        WriteValue "FriendlyName = $friendlyName"
    }

    function CheckUser
    {
        WriteTitle 'User'
        $0 = "Registry::HKEY_CURRENT_USER\SOFTWARE\Classes\AppID\$guid"
        $ok = (HasKey $0)
        if ($ok) { $ok = (HasValue $0 'DllSurrogate' '') }
        if ($ok) { WriteOK $0 } else { WriteBad $0 }

        $0 = 'Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\OneNote\AddIns\River.OneMoreAddIn'
        $ok = (HasValue $0 'LoadBehavior' '3')
        $ok = (HasValue $0 'Description' 'Add-in for OneNote') -and $ok
        $ok = (HasValue $0 'FriendlyName' 'OneMoreAddIn') -and $ok
        if ($ok) { WriteOK $0 } else {
            WriteBad $0
            Write-Host '... HKCU AddIns missing; HKLM fallback (Machine section above) covers this on SYSTEM/Intune installs' -Fore Yellow
        }

        $0 = 'Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\River.OneMoreAddIn.dll'
        if ($codebase -eq $null) {
            Write-Host 'CLSID codebase is not defined' -Fore Yellow
            WriteBAD $0
        } 
        else {
            $ok = (HasValue $0 'Path' $codeBase)
            if ($ok) { WriteOK $0 } else { WriteBad $0 }
            WriteValue $lastvalue
        }

        $0 = "Registry::HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Office\$offVersion\Common\Security\Trusted Protocols\All Applications\onemore:"
        $ok = (HasKey $0)
        if ($ok) { WriteOK $0 } else { WriteBad $0 }
    }

    function CheckEventLogSource
    {
        WriteTitle 'EventLog Source'
        $0 = 'Registry::HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Application\OneMore'
        $ok = (HasKey $0)
        if ($ok) { $ok = (HasValue $0 'TypesSupported' '7') }
        if ($ok) { $typesSupported = $lastValue }
        if ($ok) { 
            WriteOK $0
            WriteValue "TypesSupported = $typesSupported"
        } else {
            WriteBad $0
        }
    }

    function CheckWebView2
    {
        WriteTitle 'WebView2'

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
                $pv = $lastvalue
                $location = (Get-ItemPropertyValue -Path $path -Name 'location')
                $ok = (Test-Path $location)
                if ($ok) {
                    WriteOK $path
                    WriteValue "location = $location"
                } else {
                    WriteBad $path
                    Write-Host "... location not found $location" -Fore Yellow
                }

                WriteValue "pv = $pv (version)"
            } else {
                WriteBad $path
                Write-Host '... has version 0.0.0.0' -Fore Yellow
            }
        }
        return $ok
    }
}
Process
{
    $script:vcolor = $Host.PrivateData.VerboseForegroundColor
    $Host.PrivateData.VerboseForegroundColor = 'DarkGray'

    $script:verbose = $PSCmdlet.MyInvocation.BoundParameters['Verbose']

    GetVersions
    CheckAppID
    CheckRoot
    CheckShell
    CheckProtocolMode
    CheckAddIn
    CheckCLSID

    if ($onewow) {
        CheckCLSID $true
    }

    CheckMachine
    CheckUser
    CheckEventLogSource
    CheckWebView2
}
End
{
    $Host.PrivateData.VerboseForegroundColor = $vcolor
}
