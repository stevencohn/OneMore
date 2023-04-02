<#
.SYNOPSIS
Generates the OneMoreAddin.com static HTML pages by extracting the archived
OneMoreWiki.zip file and wrapping each page in the template.htm file

.PARAMETER zipfile
The path to the OneMoreWiki.zip file
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
    [string] $ZipFile
    )

Begin
{
    $script:RootUrl = 'https://onemoreaddin.com'
    $script:UrlSetSchema = 'http://www.sitemaps.org/schemas/sitemap/0.9'
    $script:FileOrder = '__File_Order.txt'
    $script:ZipName = 'OneMore Wiki'
    $script:sitemap = $null
    $script:smns = $null

    function MakeSiteMap
    {
        $null = [Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq")
        $script:sitemap = [System.Xml.Linq.XElement]::Parse("<urlset xmlns=""$UrlSetSchema""/>")
        $script:smns = $sitemap.GetDefaultNamespace()
        AddToSiteMap $RootUrl 1.0
    }

    function AddToSiteMap
    {
        param([string]$url, [decimal]$priority)
        $date = get-date ((Get-Date).ToUniversalTime()) -Format 'yyyy-MM-ddThh:mm:ss+00:00'
        $sitemap.Add([System.Xml.Linq.XElement]::new($smns + 'url',
            [System.Xml.Linq.XElement]::new($smns + 'loc', [Uri]::EscapeUriString($url)),
            [System.Xml.Linq.XElement]::new($smns + 'lastmod', $date),
            [System.Xml.Linq.XElement]::new($smns + 'priority', $priority.ToString('0.0'))
            )
        )
    }

    function Unpack
    {
        param([string] $file)
        $pref = $global:ProgressPreference
        $global:ProgressPreference = 'SilentlyContinue'
        Write-Host "extracting $file"
        Expand-Archive -Path $file -DestinationPath ./ -Force | Out-Null
        $global:ProgressPreference = $pref
    }

    function MakeSection
    {
        param($sectionName)
        $sectionID = $sectionName.ToLower() -replace ' |\.|%20','-'
        Write-Host "section '$sectionName' ($sectionID)" -ForegroundColor Blue

        $dir = Join-Path $ZipName $sectionName
        $toc, $first = MakeSectionTOC $sectionID $sectionName

        Get-ChildItem $dir -File *.htm | foreach {
            $id = MakePage $sectionID (Get-Item $_.FullName) $toc
        }

        $indexFile = Join-Path $dir 'index.html'
        $meta = "<meta http-equiv=""refresh"" content=""0; url=$first"" />"
        $meta | Out-File $indexFile -Encoding utf8 -Force -Confirm:$false

        return $dir, $sectionID
    }

    function MakeSectionTOC
    {
        param($sectionID, $sectionName)
        $toc = @()
        $first = $null
        $file = (Join-Path $ZipName (Join-Path $sectionName $FileOrder)) | Resolve-Path
        if (Test-Path $file)
        {
            # use FileOrder.txt
            Get-Content $file -Encoding utf8 | foreach {
                $id = $_.ToLower() -replace ' |\.|%20','-'
                $name = "$id`.htm"
                $toc += "<li><a id=""$id"" href=""$name"">$($_)</a></li>"
                if (!$first) { $first = "/$sectionID/$name" }
            }

            #Write-Host "deleting $file" -ForegroundColor Yellow
            Remove-Item $file -Force -Confirm:$false
        }
        else
        {
            Write-Host "file does not exist $file" -ForegroundColor Red
            # no FileOrder.txt so discover HTM files instead
            Get-ChildItem (Join-Path $ZipName $sectionName) -File *.htm | foreach {
                $id = $_.BaseName.ToLower() -replace ' |\.|%20','-'
                $name = "$id`.htm"
                $toc += "<li><a id=""$id"" href=""$name"">$($_)</a></li>"
                if (!$first) { $first = "/$sectionID/$name" }
            }
        }

        return $toc, $first
    }

    function MakePage
    {
        param($sectionID, $file, $toc)
        $pageID = $file.BaseName.ToLower() -replace ' |\.|%20','-'
        Write-Host "page '$($file.BaseName)' ($pageID)"

        $source = $file | Get-Content -Encoding utf8 -Raw
        $html = New-Object -Com 'HTMLFile'
        $html.IHTMLDocument2_write($source)
        $body = $html.all.tags('body') | foreach InnerHtml

        $template = Get-Content -Path template.htm -Encoding utf8 -Raw
        $template = $template.Replace('~TOC~', [string]::join("`n", $toc))
        $template = $template.Replace('~sectionID~', $sectionID)
        $template = $template.Replace('~content~', $body)

        $folderName = "$($file.BaseName)`_files"
        $folderPath = Join-Path $file.Directory $folderName
        if (Test-Path $folderPath)
        {
            $filesID = Join-Path $file.Directory "$pageID`_files"
            CaseRename $folderPath $filesID

            $escaped = $folderName -replace ' ','%20'
            #Write-Host "replace $escaped ($pageID`_files)" -ForegroundColor Yellow
            $template = $template.Replace($escaped, "$pageID`_files")
        }

        $pagePath = Join-Path $file.Directory "$pageID`.htm"
        $template | Out-File $file.FullName -Encoding utf8 -Force -Confirm:$false
        Rename-Item $file.FullName $pagePath -Force -Confirm:$false

        AddToSiteMap "$RootUrl/$sectionID/$pageID`.htm" 0.5

        return $pageID
    }

    function CaseRename
    {
        param($path1, $path2)
        $item = Rename-Item $path1 -NewName "$path1`__x" -PassThru -Force -Confirm:$false
        $fullName = $item.FullName
        if ($item.FullName -match '(.*)\\') { $fullName = $matches[1] }
        Rename-Item $fullName -NewName $path2 -Force -Confirm:$false
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
        $dir, $sectionID = MakeSection $name

        if (Test-Path $name)
        {
            # delete the old section folder
            Remove-Item $name -Recurse -Force -Confirm:$false
        }

        if (Test-Path ./$sectionID)
        {
            Remove-Item ./$sectionID -Recurse -Force -Confirm:$false
        }

        # move the new section folder up a level and rename
        Move-Item $dir ./$sectionID -Force -Confirm:$false
    }

    Write-Host 'saving sitemap.xml'
    $sitemap.ToString() | Out-File 'sitemap.xml'

    Remove-Item $ZipName -Force -Confirm:$false
}
