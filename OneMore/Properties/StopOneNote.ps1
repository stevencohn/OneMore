$guid = '{~guid~}';
$processId = gcim Win32_Process | select ProcessId, CommandLine | where { $_.CommandLine -and $_.CommandLine.Contains($guid) } | foreach { $_.ProcessId }
if ($processId) { taskkill /pid $processId /f }
$processId = gcim Win32_Process | select ProcessId, Name | where { $_.Name -eq 'ONENOTE.EXE' } | foreach { $_.ProcessId }
if ($processId) { taskkill /fi 'pid gt 0' /im ONENOTE.exe /t /f }
