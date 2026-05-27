<#
.SYNOPSIS
Set a single consistent version string for each OneMore project, including the installer.

.PARAMETER version
The version string to apply. Must be of the form major.minor.patch.

.COPYRIGHT
Copyright © 2016 Steven M Cohn. All rights reserved.
#>

param (
    [Parameter(Mandatory = $true)]
    [ValidatePattern({^\d+\.\d+\.\d+$})]
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

    $0 = '.\OneMoreCalendar\Properties\AssemblyInfo.cs'
    $content = (Get-Content $0) -replace 'Version = "\d+\.\d+(?:\.\d+)?";',"Version = ""$version"";"
    $content | Out-File $0 -Force

    $0 = '.\OneMoreCli\Properties\AssemblyInfo.cs'
    $content = (Get-Content $0) -replace 'Version = "\d+\.\d+(?:\.\d+)?";',"Version = ""$version"";"
    $content | Out-File $0 -Force

    $0 = '.\OneMoreProtocolHandler\Properties\AssemblyInfo.cs'
    $content = (Get-Content $0) -replace 'Version = "\d+\.\d+(?:\.\d+)?";',"Version = ""$version"";"
    $content | Out-File $0 -Force

    $0 = '.\OneMoreSetupActions\Properties\AssemblyInfo.cs'
    $content = (Get-Content $0) -replace 'Version = "\d+\.\d+(?:\.\d+)?";',"Version = ""$version"";"
    $content | Out-File $0 -Force

    $0 = '.\OneMoreTray\Properties\AssemblyInfo.cs'
    $content = (Get-Content $0) -replace 'Version = "\d+\.\d+(?:\.\d+)?";',"Version = ""$version"";"
    $content | Out-File $0 -Force

    # note that ProductVersion is overridden by build.ps1 using a /p:ProductVersion= argument,
    # so this is just a default value, but make it obvious here that we've bumped the version

    $0 = '.\OneMoreSetup\OneMoreSetup.wixproj'
    $content = (Get-Content $0) -replace '(<ProductVersion[^>]*>)\d+\.\d+(?:\.\d+)?(</ProductVersion>)', "`${1}$version`${2}"
    $content | Out-File $0 -Force
}
