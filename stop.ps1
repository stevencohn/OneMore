<#
.SYNOPSIS
Stop the OneMore add-in dllhost process and then the OneNote process
#>

param()

Begin
{
	$guid = "{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}";
}
Process
{
	# CommandLine == C:\WINDOWS\system32\DllHost.exe /Processid:{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}
	$processId = gcim Win32_Process | select ProcessId, CommandLine | `
		where { $_.CommandLine -and $_.CommandLine.Contains($guid) } | `
		foreach { $_.ProcessId }

	if ($processId)
	{
		# terminate dllhost
		taskkill /pid $processId
	}

	# terminates the OneNote process
	taskkill /fi "pid gt 0" /im ONENOTE.exe /t /f
}
