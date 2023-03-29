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
    $script:ZipName = 'OneMore Wiki'

    function Unpack
    {
        param([string] $file)
        Expand-Archive -Path $file -DestinationPath ./ -Force
    }

    function MakeSection
    {
        param($sectionName)
        $sectionID = $sectionName.ToLower().Replace(' ', '-').Replace('.', '-')
        Write-Host "Section '$sectionName' ($sectionID)"
        if (!(Test-Path $sectionName))
        {
            New-Item $sectionName -Type Directory -Force
        }

        $toc = @()

        $dir = Join-Path $ZipName $sectionName
        Get-ChildItem $dir -File | foreach {
            $id = MakePage $sectionName $sectionID $_.Name $_.FullName
            $toc += "<a id=""$id"" href=""$($_.Name).htm"">$($_.Name)</a>"
        }

        $toc | Out-File (Join-Path $dir 'toc.htm') -Force -Confirm:$false
    }

    function MakePage
    {
        param($section, $sectionID, $pageName, $pageFile)
        $pageID = $pageName.ToLower().Replace(' ', '-').Replace('.', '-')
        Write-Host "Page '$pageName' ($pageID)"

        $source = Get-Content -Path $pageFile -Raw
        $html = New-Object -Com 'HTMLFile'
        $html.IHTMLDocument2_write($source)
        $body = $html.all.tags('body') | foreach InnerHtml

        $template = Get-Content -Path template.htm -Raw
        $template = $template.Replace('~sectionID~', $sectionID)
        $template = $template.Replace('~pageID~', $pageID)
        $template = $template.Replace('~CONTENT~', $body)

        $template | Out-File $pageFile -Force -Confirm:$false

        return $pageID
    }
}
Process
{
    if ($ZipFile -and (Test-Path $ZipFile))
    {
        Unpack $ZipFile
    }

    Get-ChildItem $ZipName -Directory | foreach {
        MakeSection $_.Name
    }
}
