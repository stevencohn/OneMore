<#
.SYNOPSIS
Starts a PID-exact Performance Monitor collection for OneNote and the dllhost.exe COM
surrogate that hosts the OneMore add-in, plus an optional ETW file-IO trace for OneMore.db.

.DESCRIPTION
Perfmon cannot filter counter instances by command line, so this script finds the dllhost.exe
whose command line contains OneMore's CLSID, resolves the process and .NET CLR instance names
for that PID, and creates two logman collections:

  <Name>     performance counters (.blg) for ONENOTE and the OneMore dllhost
  <Name>IO   Microsoft-Windows-Kernel-File trace (.etl), post-filtered to OneMore.db

Run elevated, after OneNote and OneMore are running (the PID changes when the surrogate
restarts). Stop with -Stop.

.EXAMPLE
.\Start-OneMorePerfmon.ps1
.\Start-OneMorePerfmon.ps1 -Stop

Post-process the IO trace (per-file IO for OneMore.db and its -wal/-shm files):
  tracerpt C:\PerfLogs\OneMore\OneMoreIO.etl -o io.csv -of CSV -y
  then filter rows where the file name matches OneMore.db and the PID matches the dllhost PID.
#>
#Requires -RunAsAdministrator
[CmdletBinding()]
param (
	[string] $Name = 'OneMore',
	[int] $SampleInterval = 5,
	[string] $OutputPath = 'C:\PerfLogs\OneMore',
	[switch] $NoTrace,
	[switch] $Stop
)

$ErrorActionPreference = 'Stop'

# CLSID of River.OneMoreAddIn.AddIn, see OneMore\AddIn.cs
$clsid = '88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61'
$ioName = "${Name}IO"

function Remove-Collection ($n)
{
	logman stop $n 2>&1 | Out-Null
	logman delete $n 2>&1 | Out-Null
}

if ($Stop)
{
	logman stop $Name
	if (-not $NoTrace) { logman stop $ioName }
	return
}

# locate the OneMore surrogate by command line
$hosts = @(Get-CimInstance Win32_Process -Filter "Name='dllhost.exe'" |
	Where-Object { $_.CommandLine -like "*$clsid*" })

if ($hosts.Count -eq 0)
{
	throw "No dllhost.exe with $clsid found; start OneNote and let OneMore load first"
}

$onenote = @(Get-Process -Name ONENOTE -ErrorAction SilentlyContinue)
if ($onenote.Count -eq 0)
{
	throw 'ONENOTE.EXE is not running'
}

# Process V2 instance names are "name:pid"
$procPaths = @()
$procCounters = @(
	'ID Process', '% Processor Time', 'Private Bytes', 'Working Set', 'Virtual Bytes',
	'Handle Count', 'Thread Count',
	'IO Read Operations/sec', 'IO Write Operations/sec', 'IO Data Operations/sec',
	'IO Read Bytes/sec', 'IO Write Bytes/sec', 'IO Data Bytes/sec'
)

$instances = @()
$instances += $hosts | ForEach-Object { "dllhost:$($_.ProcessId)" }
$instances += $onenote | ForEach-Object { "onenote:$($_.Id)" }

foreach ($i in $instances)
{
	foreach ($c in $procCounters) { $procPaths += "\Process V2($i)\$c" }
}

# .NET CLR instance names are "dllhost", "dllhost#1"... unless ProcessNameFormat=1 is set.
# Map PID to instance through the CLR's own "Process ID" counter.
$clrPaths = @()
$clrSets = [ordered]@{
	'.NET CLR Memory' = @(
		'Process ID', '# Bytes in all Heaps', '# Gen 0 Collections', '# Gen 1 Collections',
		'# Gen 2 Collections', '% Time in GC', 'Large Object Heap size')
	'.NET CLR Exceptions' = @('# of Exceps Thrown / sec')
	'.NET CLR LocksAndThreads' = @(
		'# of current logical Threads', 'Current Queue Length', 'Contention Rate / sec')
	'.NET CLR Interop' = @(
		'# of CCWs', '# of Stubs', '# of marshalling',
		'# of TLB imports / sec', '# of TLB exports / sec')
}

$hostPids = $hosts | ForEach-Object { [double]$_.ProcessId }
$clrInstances = @()
try
{
	(Get-Counter '\.NET CLR Memory(*)\Process ID').CounterSamples |
		Where-Object { $hostPids -contains $_.CookedValue } |
		ForEach-Object { $clrInstances += $_.InstanceName }
}
catch
{
	Write-Warning "Unable to read .NET CLR instances: $($_.Exception.Message)"
}

if ($clrInstances.Count -eq 0)
{
	Write-Warning 'No .NET CLR instance found for the OneMore dllhost; CLR counters skipped'
}

foreach ($i in $clrInstances)
{
	foreach ($set in $clrSets.Keys)
	{
		foreach ($c in $clrSets[$set]) { $clrPaths += "\$set($i)\$c" }
	}
}

$sysPaths = @(
	'\Processor(_Total)\% Processor Time',
	'\Memory\Available MBytes',
	'\LogicalDisk(*)\Avg. Disk sec/Read',
	'\LogicalDisk(*)\Avg. Disk sec/Write'
)

$counters = $procPaths + $clrPaths + $sysPaths

New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

Remove-Collection $Name
logman create counter $Name -c $counters -si $SampleInterval -f bin `
	-o (Join-Path $OutputPath $Name) -v mmddhhmm -max 512 | Out-Null
logman start $Name

if (-not $NoTrace)
{
	# Kernel-File keywords: 0x10 FILENAME, 0x20 FILEIO, 0x40 OP_END, 0x80 CREATE,
	# 0x100 READ, 0x200 WRITE. Not filterable by file name at capture time.
	Remove-Collection $ioName
	logman create trace $ioName -p 'Microsoft-Windows-Kernel-File' 0x3F0 0xFF `
		-o (Join-Path $OutputPath "$ioName.etl") -v mmddhhmm -max 256 | Out-Null
	logman start $ioName
}

Write-Host "OneMore dllhost PID(s): $($hosts.ProcessId -join ', ')"
Write-Host "ONENOTE PID(s):         $($onenote.Id -join ', ')"
Write-Host "Counters: $($counters.Count); output: $OutputPath"
Write-Host "Stop with: $($MyInvocation.MyCommand.Name) -Stop"
