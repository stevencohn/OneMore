<#
.SYNOPSIS
Install the InstallerProjects VS Extension to support vdproj Deployment Project builds
#>

param ()

Begin
{

	function InstallVSExtensions
	{
		$root = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
		$installer = "$root\Common7\IDE\vsixinstaller.exe"

		# TODO: update these versions every now and then...
		
		InstallVsix $installer 'InstallerProjects' 'VisualStudioClient/vsextensions/MicrosoftVisualStudio2017InstallerProjects/1.0.0/vspackage'

		Write-Host
		Write-Host '... Waiting a minute for the VSIXInstaller processes to complete' -ForegroundColor Yellow
		Start-Sleep 60
	}


	function InstallVsix
	{
		param($installer, $name, $uri)
		Write-Host "... installing $name extension in the background" -ForegroundColor Yellow

		$url = "https://marketplace.visualstudio.com/_apis/public/gallery/publishers/$uri"
		$vsix = "$($env:TEMP)\$name`.vsix"

		# download package directly from VS Marketplace and install
		[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]'Ssl3,Tls,Tls11,Tls12'
		$progressPreference = 'silentlyContinue'
		Invoke-WebRequest $url -OutFile $vsix
		$progressPreference = 'Continue'

		& $installer /quiet /norepair $vsix
	}
}
Process
{
	InstallVSExtensions
}
