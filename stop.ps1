<#
.SYNOPSIS
Stop the OneMore add-in dllhost process and then the OneNote process
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param()

Begin
{
    $guid = "{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}";
}
Process
{
	$script:verboseColor = $PSStyle.Formatting.Verbose
	$PSStyle.Formatting.Verbose = $PSStyle.Foreground.BrightBlack

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
    else
    {
        Write-Host '... dllhost not found' -Fore DarkGray
    }

    $processId = gcim Win32_Process -Verbose:$false | select ProcessId, Name | `
        where { $_.Name -eq 'ONENOTE.EXE' } | `
        foreach { $_.ProcessId }

    if ($processId)
    {
        Write-Host "... stopping ONENOTE.EXE" -Fore DarkYellow
        Write-Verbose 'taskkill /fi "pid gt 0" /im ONENOTE.exe /t /f'
        taskkill /fi "pid gt 0" /im ONENOTE.exe /t /f
    }
    else
    {
        Write-Host '... ONENOTE.EXE not found' -Fore DarkGray
    }

    $processId = gcim Win32_Process -Verbose:$false | select ProcessId, Name | `
        where { $_.Name -eq 'OneMoreTray.exe' } | `
        foreach { $_.ProcessId }

    if ($processId)
    {
        Write-Host "... stopping OneMoreTray.exe" -Fore DarkYellow
        Write-Verbose 'taskkill /fi "pid gt 0" /im OneMoreTray.exe /t /f'
        taskkill /fi "pid gt 0" /im OneMoreTray.exe /t /f
    }
    else
    {
        Write-Host '... OneMoreTray.exe not found' -Fore DarkGray
    }
}
End
{
	$PSStyle.Formatting.Verbose = $verboseColor
}
