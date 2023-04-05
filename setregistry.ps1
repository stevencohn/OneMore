<#
.SYNOPSIS
Update OneMore registry keys to point to the current development directories
intead of the program files install path

.PARAMETER Reset
Resets the registry settings back to the install path
#>

[CmdletBinding()]
param ([switch] $Reset)

Begin
{
    $script:guid = '{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}'
    $script:catid = '{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}' # HKCR\Component Categories... .NET
    $script:modern = ($env:PROCESSOR_ARCHITECTURE -match '64')
    $script:onewow = $false

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
        Write-Host "= $text" -Fore DarkGray
    }

    function EnsurePath
    {
        param($repath)
        if (!(Test-Path $repath))
        {
            write-Verbose "creating $repath"
            New-Item $repath -Force
        }
        return $repath
    }

    function ReportOneNoteVersion
    {
        Write-Host
        $0 = 'Registry::HKEY_CLASSES_ROOT\onenote\shell\Open\command'
        if (-not (HasKey $0))
        {
            writeBad 'cannot determine shell command path of OneNote'
            return $false
        }
        else
        {
            $script:onewow = (Get-ItemPropertyValue -Path $0 -Name '(default)'
                ).Contains('Program Files (x86)')
        }

        $0 = 'Registry::HKEY_CLASSES_ROOT\OneNote.Application\CurVer'
        if (-not (HasKey $0))
        {
            writeBad 'cannot determine version of OneNote'
            return $false
        }

        $parts = (Get-ItemPropertyValue -Path $0 -Name '(default)').Split('.')
        $script:oneVersion = $parts[$parts.Length - 1] + ".0"

        $0 = 'Registry::HKEY_CLASSES_ROOT\Excel.Application\CurVer'
        if (-not (HasKey $0)) {
            write-Host 'cannot determine version of Office, assuming 16.0' -Fore Yellow
            $script:offversion = '16.0'
        } else {
            $parts = (Get-ItemPropertyValue -Path $0 -Name '(default)').Split('.')
            $script:offVersion = $parts[$parts.Length - 1] + '.0'
        }

        if ($onewow)
        {
            WriteOK "OneNote version is $oneVersion (32-bit), Office version is $offversion"
        }
        else
        {
            WriteOK "OneNote version is $oneVersion (64-bit), Office version is $offversion"
        }

        return $true
    }

    function GetOneNoteProperties
    {
        if (-not $modern)
        {
            $script:onewow = $false
            return
        }

        $0 = 'Registry::HKEY_CLASSES_ROOT\OneNote.Application\CLSID'
        if (!(Test-Path $0))
        {
            WriteBad 'cannot determine CLSID of OneNote'
            return $false
        }
        $clsid = (Get-ItemPropertyValue -Path $0 -Name '(default)')

        $0 = "Registry::HKEY_CLASSES_ROOT\CLSID\$clsid\LocalServer32"
        if (!(Test-Path $0))
        {
            $0 = "Registry::HKEY_CLASSES_ROOT\WOW6432Node\CLSID\$clsid\LocalServer32"
        }

        $server = (Get-ItemPropertyValue -Path $0 -Name '(default)')
        if (!(Test-Path $server))
        {
            WriteBad $server
            return $false
        }

        WriteOK "OneNote found at $server"
        $script:onewow = $server.Contains('Program Files (x86)')
        return $true
    }

    function SetPaths
    {
        if ($Reset)
        {
            $root = $env:ProgramFiles
            $script:addin = Join-Path $root 'River\OneMoreAddIn\River.OneMoreAddIn.dll'
            $script:proto = Join-Path $root 'River\OneMoreAddIn\OneMoreProtocolHandler.exe'
        }
        else
        {
            $script:root = Get-Location
            $script:addin = Join-Path $root 'OneMore\bin\x86\Debug\River.OneMoreAddIn.dll'
            if (!(Test-Path $addin))
            {
                $script:addin = Join-Path $root 'OneMore\bin\x64\Debug\River.OneMoreAddIn.dll'
            }

            $script:proto = Join-Path $root 'OneMoreProtocolHandler\bin\Debug\OneMoreProtocolHandler.exe'
            if (!(Test-Path $addin))
            {
                WriteBad "`nCannot find $addin"
                Write-Host "Build the OneMore solution before calling setregistry.ps1`n" -Fore Yellow
                return $false
            }
        }

        WriteOK "OneMore found at $addin"
        return $true
    }

    # ProductVersion may be truncated such as "4.12" so this expands it to be "4.12.0.0"
    function MakeVersion
    {
        param($version)
        $parts = $version.Split('.')
        for (($i = $parts.Length); $i -lt 4; $i++)
        {
            $version = "$version.0"
        }
        WriteOK "OneMore version is $version"
        return $version
    }

    function SetRoot
    {
        WriteTitle 'Root'
        $0 = (EnsurePath 'Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn')
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value 'River.OneMoreAddIn.AddIn'
        WriteOK $0

        $0 = (EnsurePath 'Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn\CLSID')
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value $guid
        WriteOK $0

        $0 = (EnsurePath 'Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn\CurVer')
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value 'River.OneMoreAddIn.1'
        WriteOK $0

        $0 = (EnsurePath 'Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn.1')
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value 'Addin class'
        WriteOK $0

        $0 = (EnsurePath 'Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn.1\CLSID')
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value $guid
        WriteOK $0
        return $true
    }

    function SetAppID
    {
        WriteTitle 'AppID'
        $0 = (EnsurePath "Registry::HKEY_CLASSES_ROOT\AppID\$guid")
        Set-ItemProperty $0 -Name 'DllSurrogate' -Type String -Value ''
        WriteOK $0
        return $true
    }

    function SetProtocolHandler
    {
        WriteTitle 'Protocol handler'
        $0 = (EnsurePath 'Registry::HKEY_CLASSES_ROOT\onemore')
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value 'URL:OneMore Protocol Handler'
        Set-ItemProperty $0 -Name 'URL Protocol' -Type String -Value ''
        WriteOK $0

        # onemore:// protocol handler registration
        $0 = (EnsurePath 'Registry::HKEY_CLASSES_ROOT\onemore\shell\open\command')
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value "`"$proto`" %1 %2 %3 %4 %5"
        WriteOK $0
        WriteValue "`"$proto`" %1 %2 %3 %4 %5"
        return $true
    }

    function SetCLSID
    {
        param([boolean] $wow = $false)

        if ($wow) { $clsid = 'WOW6432Node\CLSID' } else { $clsid = 'CLSID' }
        WriteTitle $clsid

        $script:root = Get-Location
        $resx = Join-Path $root 'OneMore\Properties\Resources.Designer.cs'
        if (!(Test-Path $resx))
        {
            WriteBad "could not find $resx"
            return $false
        }
        $runtime = Get-Content $resx | where { $_ -match 'Runtime version:(\d+(?:\.\d+){2})' } | foreach { $matches[1] }
        if (!$runtime)
        {
            WriteBad 'could not determine .NET RuntimeVersion'
            return $false
        }

        $0 = (EnsurePath "Registry::HKEY_CLASSES_ROOT\$clsid\$guid")
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value 'River.OneMoreAddIn.AddIn'
        Set-ItemProperty $0 -Name 'AppID' -Type String -Value $guid
        WriteOK $0

        $0 = (EnsurePath "Registry::HKEY_CLASSES_ROOT\$clsid\Implemented Categories\$catid")
        WriteOK $0

        $0 = (EnsurePath "Registry::HKEY_CLASSES_ROOT\$clsid\$guid\InprocServer32")
        $asm = "River.OneMoreAddIn, Version=$pv, Culture=neutral, PublicKeyToken=null"
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value 'mscoree.dll'
        Set-ItemProperty $0 -Name 'Assembly' -Type String -Value $asm
        Set-ItemProperty $0 -Name 'Class' -Type String -Value 'River.OneMoreAddIn.AddIn'
        Set-ItemProperty $0 -Name 'CodeBase' -Type String -Value $addin
        Set-ItemProperty $0 -Name 'RuntimeVersion' -Type String -Value "v$runtime"
        Set-ItemProperty $0 -Name 'ThreadingModel' -Type String -Value 'Both'
        WriteOK $0

        $0 = (EnsurePath "Registry::HKEY_CLASSES_ROOT\$clsid\$guid\InprocServer32\$pv")
        $asm = "River.OneMoreAddIn, Version=$pv, Culture=neutral, PublicKeyToken=null"
        Set-ItemProperty $0 -Name 'Assembly' -Type String -Value $asm
        Set-ItemProperty $0 -Name 'Class' -Type String -Value 'River.OneMoreAddIn.AddIn'
        Set-ItemProperty $0 -Name 'CodeBase' -Type String -Value $addin
        Set-ItemProperty $0 -Name 'RuntimeVersion' -Type String -Value "v$runtime"
        WriteOK $0
        WriteValue $addin

        $0 = (EnsurePath "Registry::HKEY_CLASSES_ROOT\$clsid\$guid\ProgID")
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value 'River.OneMoreAddIn'
        WriteOK $0

        $0 = (EnsurePath "Registry::HKEY_CLASSES_ROOT\$clsid\$guid\Programmable")
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value ''
        WriteOK $0

        $0 = (EnsurePath "Registry::HKEY_CLASSES_ROOT\$clsid\$guid\TypeLib")
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value $guid
        WriteOK $0

        $0 = (EnsurePath "Registry::HKEY_CLASSES_ROOT\$clsid\$guid\VersionIndependentProgID")
        Set-ItemProperty $0 -Name '(Default)' -Type String -Value 'River.OneMoreAddIn'
        WriteOK $0

        return $true
    }

    function SetUser
    {
        WriteTitle 'User'
        $0 = (EnsurePath "Registry::HKEY_CURRENT_USER\SOFTWARE\Classes\AppID\$guid")
        Set-ItemProperty $0 -Name 'DllSurrogate' -Type String -Value ''
        WriteOK $0

        $0 = (EnsurePath 'Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\OneNote\AddIns\River.OneMoreAddIn')
        Set-ItemProperty $0 -Name 'LoadBehavior' -Type DWord -Value 3
        Set-ItemProperty $0 -Name 'Description' -Type String -Value 'Extension for OneNote'
        Set-ItemProperty $0 -Name 'FriendlyName' -Type String -Value 'OneMoreAddIn'
        WriteOK $0

        $0 = (EnsurePath 'Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\River.OneMoreAddIn.dll')
        Set-ItemProperty $0 -Name Path -Type String -Value $addin
        WriteOK $0
        WriteValue $addin

        $0 = (EnsurePath "Registry::HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Office\$offversion\Common\Security\Trusted Protocols\All Applications\onemore:")
        WriteOK $0
        return $true
    }
}
Process
{
    $vcolor = $Host.PrivateData.VerboseForegroundColor
    $Host.PrivateData.VerboseForegroundColor = 'DarkGray'

    if (!(ReportOneNoteVersion)) { return }
    if (!(GetOneNoteProperties)) { return }
    if (!(SetPaths)) { return }

    $script:pv = MaKeVersion (Get-Item $addin | % { $_.VersionInfo.ProductVersion })

    $ok = SetRoot
    $ok = SetAppID
    $ok = SetProtocolHandler
    $ok = SetCLSID

    if ($onewow)
    {
        $ok = SetCLSID $true
    }

    $ok = SetUser

    $Host.PrivateData.VerboseForegroundColor = $vcolor
}
