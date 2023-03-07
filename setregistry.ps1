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

    function SetOneNoteProperties
    {
        if (-not $modern) {
            $script:onewow = $false
            return
        }

        $0 = 'Registry::HKEY_CLASSES_ROOT\OneNote.Application\CLSID'
        $clsid = (Get-ItemPropertyValue -Path $0 -Name '(default)')

        $0 = "Registry::HKEY_CLASSES_ROOT\CLSID\$clsid\LocalServer32"
        if (!(Test-Path $0)) {
            $0 = "Registry::HKEY_CLASSES_ROOT\WOW6432Node\CLSID\$clsid\LocalServer32"
        }

        $server = (Get-ItemPropertyValue -Path $0 -Name '(default)')
        $script:onewow = $server.Contains('Program Files (x86)')
    }

    function SetPaths
    {
        if ($Reset) {
            $root = $env:ProgramFiles
            $script:addin = Join-Path $root 'River\OneMoreAddIn\River.OneMoreAddIn.dll'
            $script:proto = Join-Path $root 'River\OneMoreAddIn\OneMoreProtocolHandler.exe'
        }
        else {
            $script:root = Get-Location
            $script:addin = Join-Path $root 'OneMore\bin\x86\Debug\River.OneMoreAddIn.dll'
            $script:proto = Join-Path $root 'OneMoreProtocolHandler\bin\Debug\OneMoreProtocolHandler.exe'
            if (!(Test-Path $addin))
            {
                Write-Host "cannot find $addin" -Fore Yellow
                Write-Host 'Build the OneMore solution before calling setregistry.ps1' -Fore Yellow
                return
            }
        }
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
        return $version
    }

    function SetProtocolHandler
    {
        WriteTitle 'Protocol handler'
        # onemore:// protocol handler registration
	    $0 = 'Registry::HKEY_CLASSES_ROOT\onemore\shell\open\command'
        if (Test-Path $0)
        {
	        Set-ItemProperty $0 -Name '(Default)' -Type String -Value "`"$proto`" %1 %2 %3 %4 %5"
            WriteOK $0
            Write-Verbose "`"$proto`" %1 %2 %3 %4 %5"
        }
    }

    function SetCLSID
    {
        param([boolean] $wow = $false)

        if ($wow) { $clsid = 'WOW6432Node\CLSID' } else { $clsid = 'CLSID' }

        WriteTitle $clsid
        $0 = "Registry::HKEY_CLASSES_ROOT\$clsid\$guid\InprocServer32"
        if (Test-Path $0)
        {
            $asm = "River.OneMoreAddIn, Version=$pv, Culture=neutral, PublicKeyToken=null"
	        Set-ItemProperty $0 -Name Assembly -Type String -Value $asm
	        Set-ItemProperty $0 -Name CodeBase -Type String -Value $addin
            WriteOK $0
            Write-Verbose $addin
        }

        $1 = "Registry::HKEY_CLASSES_ROOT\$clsid\$guid\InprocServer32\$pv"
        if (!(Test-Path $1))
        {
            write-Host "creating $1" -Fore Yellow
            New-Item -Path $0 -Name $pv | Out-Null
            $asm = "River.OneMoreAddIn, Version=$pv, Culture=neutral, PublicKeyToken=null"
	        Set-ItemProperty $1 -Name 'Assembly' -Type String -Value $asm
            Set-ItemProperty $1 -Name 'Class' -Type String -Value 'River.OneMoreAddIn.AddIn'
            Set-ItemProperty $1 -Name 'RuntimeVersion' (Get-ItemPropertyValue $0 -Name 'RuntimeVersion')
        }
        Set-ItemProperty $1 -Name CodeBase -Type String -Value $addin
        WriteOK $1
    }

    function SetAppPath
    {
        WriteTitle 'App Paths'
        $0 = 'Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\River.OneMoreAddIn.dll'
        if (Test-Path $0)
        {
	        Set-ItemProperty $0 -Name Path -Type String -Value $addin
            WriteOK $0
            Write-Verbose $addin
        }
    }
}
Process
{
    $vcolor = $Host.PrivateData.VerboseForegroundColor
    $Host.PrivateData.VerboseForegroundColor = 'DarkGray'

    SetOneNoteProperties
    SetPaths

    $script:pv = MaKeVersion (Get-Item $addin | % { $_.VersionInfo.ProductVersion })

    SetProtocolHandler
    SetCLSID

    if ($onewow) {
        SetCLSID $true
    }

    SetAppPath

    $Host.PrivateData.VerboseForegroundColor = $vcolor
}
