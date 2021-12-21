<#
.SYNOPSIS
Update OneMore registry keys to point to the current development directories
intead of the program files install paths
#>

param ()

Begin
{
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
}
Process
{
    $here = Get-Location
    $guid = '{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}'
    $dll = Join-Path $here 'OneMore\bin\x86\Debug\River.OneMoreAddIn.dll'

	$0 = 'Registry::HKEY_CLASSES_ROOT\onemore\shell\open\command'
    if (Test-Path $0)
    {
        $1 = Join-Path $here 'OneMoreProtocolHandler\bin\Debug\OneMoreProtocolHandler.exe'
        Write-Host "setting $0"
	    Set-ItemProperty $0 -Name '(Default)' -Type String -Value "$1 %1 %2 %3 %4 %5"
    }

    $0 = "Registry::HKEY_CLASSES_ROOT\CLSID\$guid\InprocServer32"
    if (Test-Path $0)
    {
        Write-Host "setting $0"
	    Set-ItemProperty $0 -Name CodeBase -Type String -Value $dll
    }

    if (Test-Path $dll)
    {
        $pv = MaKeVersion (Get-Item $dll | % { $_.VersionInfo.ProductVersion })
        $0 = "Registry::HKEY_CLASSES_ROOT\CLSID\$guid\InprocServer32\$pv"
        Write-Host "testing $0"
        if (Test-Path $0)
        {
            Write-Host "setting $0"
            Set-ItemProperty $0 -Name CodeBase -Type String -Value $dll
        }
    }

    $0 = 'Registry::HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\River.OneMoreAddIn.dll'
    if (Test-Path $0)
    {
        Write-Host "setting $0"
	    Set-ItemProperty $0 -Name Path -Type String -Value $dll
    }
}
