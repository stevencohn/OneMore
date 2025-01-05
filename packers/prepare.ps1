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
	$tagUri = "https://api.github.com/repos/stevencohn/OneMore/releases/tags/$version"
}
Process
{
	$release = (curl -s --request GET $tagUri) | ConvertFrom-Json
	if ($release.status)
	{
		Write-Host "** cannot curl releases/tags/$version" -Fore Red
		return
	}

	# choco...

	$0 = Resolve-Path '.\chocolatey\onemore.nuspec'
	$xml = [xml](Get-Content $0)
	$xml.package.metadata.version = $version
	$xml.package.metadata.releaseNotes = $release.body
	$encoding = New-Object System.Text.UTF8Encoding($false)
	$writer = New-Object System.IO.StreamWriter($0, $false, $encoding)
	$writer.NewLine = "`n"
	$xml.Save($writer)
	$writer.Close()

	$0 = '.\chocolatey\tools\chocolateyinstall.ps1'
	$content = (Get-Content $0) -replace '\d+\.\d+(?:\.\d+)?/OneMore_\d+\.\d+(?:\.\d+)?',"$version/OneMore_$version"
	$checksum = (checksum -t sha256 $home\Downloads\OneMore_$version`_Setupx86.msi)
	$content = $content -replace "(checksum\s+=\s+)\'([^']+)(\')","`$1'$checksum`$3"
	$checksum = (checksum -t sha256 $home\Downloads\OneMore_$version`_Setupx64.msi)
	$content = $content -replace "(checksum64\s+=\s+)\'([^']+)(\')","`$1'$checksum`$3"
	$conetnt = $content -replace "`r`n","`n"
	$content | Out-File $0 -Encoding utf8BOM -Force
}
