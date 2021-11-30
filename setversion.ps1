<#
.SYNOPSIS
Set the version of OneMore, OneMoreProtocolHandler, and OneMoreSetup

.PARAMETER version
The version string to apply. Must be of the form major.minor or major.minor.patch
#>

param (
    [Parameter(Mandatory = $true)]
    [ValidatePattern({^\d+\.\d+(?:\.\d+)?$})]
    [string] $version
    )

Begin
{
}
Process
{
    $0 = '.\OneMore\Properties\AssemblyInfo.cs'
    $content = (Get-Content $0) -replace 'Version = "\d+\.\d+(?:\.\d+)?";',"Version = ""$version"";"
    $content | Out-File $0 -Force

    $0 = '.\OneMoreProtocolHandler\Properties\AssemblyInfo.cs'
    $content = (Get-Content $0) -replace 'Version = "\d+\.\d+(?:\.\d+)?";',"Version = ""$version"";"
    $content | Out-File $0 -Force

    $0 = '.\OneMoreSetupActions\Properties\AssemblyInfo.cs'
    $content = (Get-Content $0) -replace 'Version = "\d+\.\d+(?:\.\d+)?";',"Version = ""$version"";"
    $content | Out-File $0 -Force

    $0 = '.\OneMoreSetup\OneMoreSetup.vdproj'
    $guid1 = [Guid]::NewGuid().ToString('B').ToUpper()
    $guid2 = [Guid]::NewGuid().ToString('B').ToUpper()
    $content = (Get-Content $0) -replace '"ProductVersion" = "8:\d+\.\d+(?:\.\d+)?"',"""ProductVersion"" = ""8:$version"""
    $content = $content -replace '"ProductCode" = "8:{[A-F0-9-]+}"',"""ProductCode"" = ""8:$guid1"""
    $content = $content -replace '"PackageCode" = "8:{[A-F0-9-]+}"',"""PackageCode"" = ""8:$guid2"""
    $content | Out-File $0 -Force
}
