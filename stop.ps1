<#
.SYNOPSIS
Stop the OneMore add-in dllhost process and then the OneNote process

.COPYRIGHT
Copyright © 2016 Steven M Cohn. All rights reserved.
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param(
	[switch]$Close
)

Begin
{
    $guid = "{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}";


    function Assassinate
    {
        param($name)
        $processId = gcim Win32_Process -Verbose:$false | select ProcessId, Name | `
            where { $_.Name -eq $name } | `
            foreach { $_.ProcessId }

        if ($processId)
        {
            Write-Host "... stopping $name" -Fore DarkYellow
            Write-Verbose "taskkill /fi "pid gt 0" /im $name /t /f"
            taskkill /fi "pid gt 0" /im $name /t /f
        }
        elseif (!$Close)
        {
            Write-Host "... $name not found" -Fore DarkGray
        }
    }


    function Euthanize
    {
        param($name)
        # graceful close: send WM_CLOSE, wait up to 8 s, then force-kill if needed
        $p = Get-Process $name -ErrorAction SilentlyContinue
        if ($p)
        {
            Write-Host "... closing $($p.ProcessName)" -Fore DarkYellow
            $p.CloseMainWindow() | Out-Null
            if (-not $p.WaitForExit(5000))
            {
                Write-Host "... force-killing $($p.ProcessName) (timeout)" -Fore Yellow
                $p.Kill($true)
            }
        }
        elseif (!$Close)
        {
            Write-Host "... $name not found" -Fore DarkGray
        }
    }


    function ObliterateDllHost
    {
        # CommandLine == C:\WINDOWS\system32\DllHost.exe /Processid:{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}
        $processId = gcim Win32_Process -Verbose:$false | select ProcessId, CommandLine | `
            where { $_.CommandLine -and $_.CommandLine.Contains($guid) } | `
            foreach { $_.ProcessId }

        if ($processId)
        {
            $processId | foreach {
                Write-Host "... stopping dllhost $_" -Fore DarkYellow
                Write-Verbose "taskkill /pid $_ /f"
                taskkill /pid $_ /f
            }
        }
        elseif (!$Close)
        {
            Write-Host '... dllhost not found' -Fore DarkGray
        }
    }
}
Process
{
	$script:verboseColor = $PSStyle.Formatting.Verbose
	$PSStyle.Formatting.Verbose = $PSStyle.Foreground.BrightBlack

    ObliterateDllHost

    if ($Close)
    {
        foreach ($name in @('OneMoreCalendar', 'OneMoreTray', 'ONENOTE'))
        {
            Euthanize $name
        }
    }

    Assassinate 'OneMoreCalendar.exe'
    Assassinate 'OneMoreTray.exe'
    Assassinate 'ONENOTE.EXE'
}
End
{
	$PSStyle.Formatting.Verbose = $verboseColor
}
