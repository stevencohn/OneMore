<#
.SYNOPSIS
Prepare packer files for publishing.
This should be run after the release/tag are established on Github.

.PARAMETER version
The version string to apply. Must be of the form major.minor or major.minor.patch
#>

param (
    [Parameter(Mandatory = $true)]
    [ValidatePattern({^\d+\.\d+(?:\.\d+)?(?:\-Beta|\-Experimental)?$})]
    [string] $version
    )

Begin
{
}
Process
{
	$release = (curl --request GET https://api.github.com/repos/stevencohn/OneMore/releases/tags/$version) | ConvertFrom-Json

	# choco...

	$0 = '.\packers\chocolatey\onemore.nuspec'
	$xml = [xml](Get-Content $0)
	$xml.package.metadata.version = $version
	$xml.package.metadata.releaseNotes = $release.body
	$xml.OuterXml | Out-File $0 -Force

	$0 = '.\packers\chocolatey\tools\chocolateyinstall.ps1'
	$content = (Get-Content $0) -replace '\d+\.\d+(?:\.\d+)?/OneMore_\d+\.\d+(?:\.\d+)?',"$version/OneMore_$version"
	$checksum = (checksum $home\Downloads\OneMore_$version`_Setupx86.msi)
	$content = $content -replace "(checksum\s+= \')([^\']+)(\')","`$1$version$3"
	$checksum = (checksum $home\Downloads\OneMore_$version`_Setupx64.msi)
	$content = $content -replace "(checksum64\s+= \')([^\']+)(\')","`$1$version$3"
	$content | Out-File $0 -Force
}
