<#
.SYNOPSIS
Set the version of OneMore, OneMoreProtocolHandler, and OneMoreSetup

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
    $0 = '.\OneMore\Properties\AssemblyInfo.cs'
    $content = (Get-Content $0) -replace 'Version = "\d+\.\d+(?:\.\d+)?";',"Version = ""$version"";"
    $content | Out-File $0 -Force

    $0 = '.\OneMoreCalendar\Properties\AssemblyInfo.cs'
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
}
