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
    $script:RootUrl = 'https://onemoreaddin.com'
    $script:FileOrder = '__File_Order.txt'
    $script:ZipName = 'OneMore Wiki'
    $script:sitemap = $null

    function MakeSiteMap
    {
        $null = [Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq")
        $script:sitemap = [XElement]::Parse('<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"/>')
        AddToSiteMap $RootUrl 1.0
    }

    function AddToSiteMap
    {
        param([string]$url, [decimal]$priority)
        $date = get-date ((Get-Date).ToUniversalTime()) -format 'yyyy-MM-ddThh:mm:ss+00:00'
        $sitemap.Add([XElement]::new(([XNamespace]'http://www.sitemaps.org/schemas/sitemap/0.9') + 'url',
            [XElement]::new('loc', [Uri]::EscapeUriString($url)),
            [XElement]::new('lastmod', $date),
            [XElement]::new('priority', $priority.ToString('0.0'))
            )
        )
    }

    function Unpack
    {
        param([string] $file)
        $ProgressPreference = 'SilentlyContinue'
        Expand-Archive -Path $file -DestinationPath ./ -Force
        $ProgressPreference = 'Continue'
    }

    function MakeSection
    {
        param($sectionName)
        $sectionID = $sectionName.ToLower().Replace(' ', '-').Replace('.', '-')
        Write-Host "Section '$sectionName' ($sectionID)" -ForegroundColor Blue
        $toc = @()
        $first = $null

        $dir = Join-Path $ZipName $sectionName
        Get-ChildItem $dir -File | foreach {
            if ($_.Name -ne $FileOrder)
            {
                $id = MakePage $sectionName $sectionID $_.Name $_.FullName
                $toc += "<a id=""$id"" href=""$($_.Name)"">$($_.BaseName)</a>"
                if (!$first) { $first = $_.Name }
            }
        }

        $forder = Join-Path $dir $FileOrder
        if (Test-Path $forder)
        {
            $toc = @()
            $first = $null

            Get-Content $forder -Encoding utf8 | foreach {
                $id = $_.ToLower().Replace(' ', '-').Replace('.', '-')
                $toc += "<a id=""$id"" href=""$($_).htm"">$($_)</a>"
                if (!$first) { $first = "$($_).htm" }
            }

            Remove-Item $forder -Force -Confirm:$false
        }

        $tocFile = Join-Path $dir 'toc.htm'
        $toc | Out-File $tocFile -Encoding utf8 -Force -Confirm:$false
        Write-Host "TOC '$tocFile'" -ForegroundColor DarkGray

        $indexFile = Join-Path $dir 'index.html'
        $meta = "<meta http-equiv=""refresh"" content=""0; url=$first"" />"
        $meta | Out-File $indexFile -Encoding utf8 -Force -Confirm:$false
    }

    function MakePage
    {
        param($section, $sectionID, $pageName, $pageFile)
        $name = [System.IO.Path]::GetFileNameWithoutExtension($pageName)
        $pageID = $name.ToLower().Replace(' ', '-').Replace('.', '-')
        Write-Host "Page '$name' ($pageID)"

        $source = Get-Content -Path $pageFile -Encoding utf8 -Raw
        $html = New-Object -Com 'HTMLFile'
        $html.IHTMLDocument2_write($source)
        $body = $html.all.tags('body') | foreach InnerHtml

        $template = Get-Content -Path template.htm -Encoding utf8 -Raw
        $template = $template.Replace('~sectionID~', $sectionID)
        $template = $template.Replace('~pageID~', $pageID)
        $template = $template.Replace('~CONTENT~', $body)

        $template | Out-File $pageFile -Encoding utf8 -Force -Confirm:$false

        AddToSiteMap "$RootUrl/$section/$name`.htm" 0.5

        return $pageID
    }
}
Process
{
    MakeSiteMap

    if ($ZipFile -and (Test-Path $ZipFile))
    {
        $script:ZipName = (Get-Item $ZipFile).BaseName
        Unpack $ZipFile
    }

    Get-ChildItem $ZipName -Directory | foreach {
        $name = $_.Name
        MakeSection $name

        if (Test-Path $name)
        {
            # delete the old section folder
            Remove-Item $name -Recurse -Force -Confirm:$false
        }

        # move the new section folder up a level
        Move-Item (Join-Path $ZipName $name) . -Force -Confirm:$false
    }

    $sitemap.ToString() | Out-File 'sitemap.xml'

    Remove-Item $ZipName -Force -Confirm:$false
}
