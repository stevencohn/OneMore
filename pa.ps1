
param($dllpath)

begin
{
	# Function to determine DLL architecture
	function Get-DllArchitecture {
		param (
			[string]$DllPath
		)

		if (-not (Test-Path $DllPath)) {
			Write-Error "File not found: $DllPath"
			return
		}

		# Open the file as a binary stream
		$stream = [System.IO.File]::OpenRead($DllPath)
		try {
			$reader = New-Object System.Reflection.PortableExecutable.PEReader $stream
			$machine = $reader.PEHeaders.CoffHeader.Machine

			switch ([int]$machine) {
				0x014c { "x86" }
				0x0200 { "Itanium" }
				0x8664 { "x64" }
				0x01c4 { "ARM" }
				0xaa64 { "ARM64" }
				default { "Unknown Architecture: $machine" }
			}
		} finally {
			$stream.Close()
		}
	}
}
Process
{
	Get-DllArchitecture $dllpath
}