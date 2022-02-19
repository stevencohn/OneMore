<#
.SYNOPSIS
Installation qualification
#>

param (
    [switch] $Registry = $true
)

Begin
{
    $script:guid = '{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}'

    function HasKey
    {
        param($kpath)
        if (-not (Test-Path $kpath))
        {
            write-Host "key not found: $kpath" -ForegroundColor Red
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
            write-Host "property not found: $kpath\$name" -ForegroundColor Red
            return $false
        }
        return $true
    }

    function HasValue
    {
        param($kpath, $name, $value)
        if (-not (HasProperty $kpath $name))
        {
            return $false
        }
		$v = (Get-ItemPropertyValue -Path $kpath -Name $name)
        if ($value.Contains('*'))
        {
            if (-not ($v -like $value))
            {
                Write-Host "invalid value: $kpath\$name, '$v' <> '$value'" -ForegroundColor Red
                return $false
            }
        }
        elseif ($v -ne $value)
        {
            Write-Host "invalid value: $kpath\$name, '$v' <> '$value'" -ForegroundColor Red
            return $false
        }
        return $true
    }

    function CheckAppID
    {
	    $0 = "Registry::HKEY_CLASSES_ROOT\AppID\$guid"
        if (-not (HasKey $0)) { return }
        if (-not (HasValue $0 'DllSurrogate' '')) { return }
        Write-Host "OK $0"
    }

    function CheckRoot
    {
	    $0 = "Registry::HKEY_CLASSES_ROOT\onemore"
        if (-not (HasKey $0)) { return }
        if (-not (HasValue $0 '(default)' 'URL:OneMore Protocol Handler')) { return }
        if (-not (HasValue $0 'URL Protocol' '')) { return }
        Write-Host "OK $0"
    }

    function CheckShell
    {
	    $0 = "Registry::HKEY_CLASSES_ROOT\onemore\shell\open\command"
        if (-not (HasKey $0)) { return }
        if (-not (HasValue $0 '(default)' '*\OneMoreProtocolHandler.exe %1 %2 %3 %4 %5')) { return }
        Write-Host "OK $0"
    }

    function CheckAddIn
    {
	    $0 = "Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn"
        if (-not (HasKey $0)) { return }
        if (-not (HasValue $0 '(default)' 'River.OneMoreAddIn.AddIn')) { return }
	    $1 = "$0\CLSID"
        if (-not (HasValue $1 '(default)' $guid)) { return }
	    $1 = "$0\CurVer"
        if (-not (HasValue $1 '(default)' 'River.OneMoreAddIn.1')) { return }
        Write-Host "OK $0"

	    $0 = "Registry::HKEY_CLASSES_ROOT\River.OneMoreAddIn.1"
        if (-not (HasValue $0 '(default)' 'Addin class')) { return }
	    $1 = "$0\CLSID"
        if (-not (HasValue $1 '(default)' $guid)) { return }
        Write-Host "OK $0"
    }

    function CheckCLSID
    {
	    $0 = "Registry::HKEY_CLASSES_ROOT\CLSID\$guid"
        if (-not (HasKey $0)) { return }
        if (-not (HasValue $0 '(default)' 'River.OneMoreAddIn.AddIn')) { return }
        if (-not (HasValue $0 'AppID' $guid)) { return }

        $1 = "$0\Implemented Categories\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}"
        if (-not (HasKey $0)) { return }

        $1 = "$0\InprocServer32"
        if (-not (HasKey $1)) { return }
        if (-not (HasValue $1 '(default)' 'mscoree.dll')) { return }
        if (-not (HasValue $1 'ThreadingModel' 'Both')) { return }
        if (-not (HasValue $1 'CodeBase' '*\River.OneMoreAddIn.dll')) { return }
        if (-not (HasValue $1 'Class' 'River.OneMoreAddIn.AddIn')) { return }
        if (-not (HasValue $1 'RuntimeVersion' 'v*')) { return }
        if (-not (HasValue $1 'Assembly' 'River.OneMoreAddIn, Version=*')) { return }

        $1 = "$0\InprocServer32\"

        $1 = "$0\ProgID"
        if (-not (HasKey $1)) { return }
        if (-not (HasValue $1 '(default)' 'River.OneMoreAddIn')) { return }

        $1 = "$0\Programmable"
        if (-not (HasKey $1)) { return }
        if (-not (HasValue $1 '(default)' '')) { return }

        $1 = "$0\TypeLib"
        if (-not (HasKey $1)) { return }
        if (-not (HasValue $1 '(default)' $guid)) { return }

        $1 = "$0\VersionIndependentProgID"
        if (-not (HasKey $1)) { return }
        if (-not (HasValue $1 '(default)' 'River.OneMoreAddIn')) { return }

        Write-Host "OK $0"
    }

    function CheckUser
    {
        $0 = "Registry::HKEY_CURRENT_USER\SOFTWARE\Classes\AppID\$guid"
        if (-not (HasKey $0)) { return }
        if (-not (HasValue $0 'DllSurrogate' '')) { return }

        $0 = "Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\OneNote\AddIns\River.OneMoreAddIn"
        if (-not (HasValue $0 'LoadBehavior' '3')) { return }
        if (-not (HasValue $0 'Description' 'Extension for OneNote')) { return }
        if (-not (HasValue $0 'FriendlyName' 'OneMoreAddIn')) { return }

        $0 = "Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\River.OneMoreAddIn.dll"
        if (-not (HasValue $0 'Path' '*\River.OneMoreAddIn.dll')) { return }
    }
}
Process
{
    CheckAppID
    CheckRoot
    CheckShell
    CheckAddIn
    CheckCLSID
    CheckUser
}
