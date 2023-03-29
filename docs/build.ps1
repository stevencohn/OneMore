<#
.SYNOPSIS
blah

.PARAMETER zipfile
blah
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
    [string] $ZipFile
    )

Begin
{
    function Unpack
    {
        param([string] $file)
        Expand-Archive -Path $file -DestinationPath ./ -Force
    }

    function Rewrap
    {
        param([string] $file)
        $source = Get-Content -path $file -raw
        $html = New-Object -Com 'HTMLFile'
        $html.IHTMLDocument2_write($source)
        $body = $html.all.tags('body') | foreach InnerHtml
    }
}
Process
{
    if ($ZipFile -and (Test-Path $ZipFile))
    {
        Unpack $ZipFile
    }
}
