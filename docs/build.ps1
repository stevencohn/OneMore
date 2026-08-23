<#
.SYNOPSIS
Generates the OneMoreAddin.com static HTML pages by extracting the archived
OneMoreWiki.zip file and wrapping each page in the template.htm file

.PARAMETER zipfile
The path to the OneMoreWiki.zip file
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
    [string] $ZipFile = '.\OneMore Wiki.zip',
	[switch] $Compare
)

Begin
{
    $script:RootUrl = 'https://onemoreaddin.com'
    $script:UrlSetSchema = 'http://www.sitemaps.org/schemas/sitemap/0.9'
    $script:FileOrder = '__File_Order.txt'
    $script:ZipName = 'OneMore Wiki'
    $script:secmap = @{}
    $script:sitemap = $null
    $script:smns = $null
    $script:writable = $true
    $script:DevelopersTOC = $null
    $script:PageLog = @()

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

    function MakeSectionMap
    {
        Get-ChildItem $ZipName -Directory | foreach {
            $secmap.Add($_.BaseName, ($_.BaseName.ToLower() -replace ' |\.|%20', '-'))
        }
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
        $sectionID = $sectionName.ToLower() -replace ' |\.|%20', '-'
        Write-Host "section '$sectionName' ($sectionID)" -ForegroundColor Blue

        $script:skips = @()

        $dir = Join-Path $ZipName $sectionName
        $toc, $first = MakeSectionTOC $sectionID $sectionName

        if ($sectionID -eq 'developers')
        {
            # remember this TOC so we can also patch the hand-written telemetry/index.html sidebar
            $script:DevelopersTOC = $toc
        }

        Get-ChildItem $dir -File *.htm | foreach {
            if (-not ($skips -contains $_.FullName))
            {
                MakePage $sectionID $_.Name $_.FullName $toc
            }
        }

        $indexFile = Join-Path $dir 'index.html'

        $meta = @"
<!DOCTYPE html>
<html lang="en">
 <head>
  <meta charset="utf-8">
  <meta http-equiv="refresh" content="0; url=$first">
  <title>Redirecting...</title>
 </head>
 <body></body>
</html>
"@

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
            $basepath = $file | Split-Path -Parent

            # use FileOrder.txt
            Get-Content $file -Encoding utf8 | foreach {
                $id = $_.ToLower() -replace ' |\.|%20', '-'
                $name = "$_`.htm"
                if ($name.Contains('#skipwiki'))
                {
                    $script:skips += (Join-Path $basepath $name)
                }
                else
                {
                    $toc += "<li><a id=""$id"" href=""$name"">$($_)</a></li>"
                    if (!$first) { $first = "/$sectionID/$name" }
                }
            }

            #Write-Host "deleting $file" -ForegroundColor Yellow
            Remove-Item $file -Force -Confirm:$false
        }
        else
        {
            Write-Host "file does not exist $file" -ForegroundColor Red
            # no FileOrder.txt so discover HTM files instead
            Get-ChildItem (Join-Path $ZipName $sectionName) -File *.htm | foreach {
                $id = $_.BaseName.ToLower() -replace ' |\.|%20', '-'
                $name = "$($_.BaseName)`.htm"
                if ($name.Contains('#skipwiki'))
                {
                    $script:skips += (Join-Path $basepath $name)
                }
                else
                {
                    $toc += "<li><a id=""$id"" href=""$name"">$($_)</a></li>"
                    if (!$first) { $first = "/$sectionID/$name" }
                }
            }
        }

        return $toc, $first
    }

    function GetPageModified
    {
        # each page starts with a title heading followed by one or two citation
        # lines: a creation date, and optionally "Last updated on <date>". Both
        # lines share this exact style, so pull them out and parse whichever
        # date applies rather than trusting filesystem timestamps (which reflect
        # zip extraction, not the page's real OneNote edit history)
        param($source, $pageFile)

        $found = [regex]::Matches($source, '<P style="FONT-SIZE: 10pt; FONT-FAMILY: Calibri; COLOR: #767676; MARGIN: 0in">(.*?)</P>')
        if ($found.Count -gt 0)
        {
            $dateText = $found[0].Groups[1].Value.Trim()
            if ($found.Count -gt 1)
            {
                $second = $found[1].Groups[1].Value.Trim()
                if ($second -match '^Last updated on\s+(.+)$')
                {
                    $dateText = $Matches[1].Trim()
                }
            }

            try
            {
                return [datetime]::Parse($dateText, [System.Globalization.CultureInfo]::InvariantCulture)
            }
            catch
            {
                Write-Host "could not parse page date '$dateText' in $pageFile" -ForegroundColor Yellow
            }
        }

        return (Get-Item $pageFile).LastWriteTime
    }

    function MakePage
    {
        param($sectionID, $pageName, $pageFile, $toc)
        $name = [System.IO.Path]::GetFileNameWithoutExtension($pageName)
        $pageID = $name.ToLower() -replace ' |\.|%20', '-'
        Write-Host "page '$name' ($pageID)"

        $source = Get-Content -Path $pageFile -Encoding utf8 -Raw
        $html = New-Object -Com 'HTMLFile'

        try
        {
            if ($writable)
            {
                $html.IHTMLDocument2_write($source)
            }
            else
            {
                $html.write([System.Text.Encoding]::Unicode.GetBytes($source))
            }
        }
        catch
        {
            $html.write([System.Text.Encoding]::Unicode.GetBytes($source))
            $script:writable = $false
        }

        $body = $html.all.tags('body')

        PatchSectionRefs $body
        PatchImageRefs $body $sectionID
        $inner = $body | foreach InnerHtml
        $modified = GetPageModified $inner $pageFile

        $template = Get-Content -Path template.htm -Encoding utf8 -Raw
        $template = $template.Replace('~PAGE_TITLE~', $name)
        $template = $template.Replace('~P~', [string]::join("`n", $toc))
        $template = $template.Replace('~TOC~', [string]::join("`n", $toc))
        $template = $template.Replace('~sectionID~', $sectionID)
        $template = $template.Replace('~content~', $inner)

        $template | Out-File $pageFile -Encoding utf8 -Force -Confirm:$false

        AddToSiteMap "$RootUrl/$sectionID/$name`.htm" 0.5

        $script:PageLog += [pscustomobject]@{
            Title    = "$sectionID/$name"
            Url      = "/$sectionID/$name`.htm"
            Modified = $modified
        }
    }

    function PatchSectionRefs
    {
        param($body)
        $body | where { $_.all } | foreach {
            $_.all.tags('a') | foreach {
                $href = $_.attributes['href']
                if ($href.textContent -match '\.\.(/[^/]+/).+')
                {
                    $m = $matches[1]
                    $deslashed = $m -replace '/',''
                    if ($secmap.ContainsKey($deslashed))
                    {
                        $slashed = "/$($secmap[$deslashed])/"
                        #write-host "uri $($href.textContent)" -ForegroundColor Green
                        $href.textContent = $href.textContent -replace $m, $slashed
                        #write-host "uri $($href.textContent)" -ForegroundColor DarkGreen
                    }
                }
            } }
    }

    function PatchImageRefs
    {
        # rewrite page-relative image src values (e.g. "PageName_files/image001.png")
        # to be root-relative, so they still resolve when reused out of context, such as
        # in pagefind search-result thumbnails rendered on an unrelated page
        param($body, $sectionID)
        $body | where { $_.all } | foreach {
            $_.all.tags('img') | foreach {
                $src = $_.attributes['src']
                if ($src.textContent -and $src.textContent -notmatch '^(https?:)?//|^/|^data:')
                {
                    $src.textContent = "/$sectionID/$($src.textContent)"
                }
            } }
    }

    function RunPagefind
    {
        if (-not (Get-Command pagefind -ErrorAction SilentlyContinue))
        {
            Write-Host 'pagefind not found on PATH; skipping search index build' -ForegroundColor Yellow
            return
        }
        Write-Host 'building pagefind search index' -ForegroundColor Blue
        pagefind --site . --glob '**/*.{html,htm}'
    }

    function UpdateTelemetrySidebar
    {
        param($toc)

        $file = '.\telemetry\index.html'
        if (-not (Test-Path $file))
        {
            Write-Host "$file not found; skipping telemetry sidebar update" -ForegroundColor Yellow
            return
        }

        Write-Host 'updating telemetry sidebar from developers TOC' -ForegroundColor Blue

        # telemetry/index.html lives outside the developers folder so hrefs must be rooted there
        $items = $toc | foreach { $_ -replace 'href="', 'href="/developers/' }
        $list = [string]::join("`n", $items)

        $content = Get-Content -Path $file -Encoding utf8 -Raw
        $updated = $content.Replace('~TOC~', $list)
        $updated | Out-File $file -Encoding utf8 -Force -Confirm:$false
    }

    function UpdateChangelog
    {
        $file = '.\get-started\About This Web Site.htm'
        if (-not (Test-Path $file))
        {
            Write-Warning "$file not found; skipping changelog table update"
            return
        }

        if ($script:PageLog.Count -eq 0)
        {
            Write-Warning 'no pages were recorded; skipping changelog table update'
            return
        }

        $content = Get-Content -Path $file -Encoding utf8 -Raw
        if ($content -notmatch [regex]::Escape('~CHANGELOG~'))
        {
            Write-Warning "~CHANGELOG~ token not found in $file; table left unchanged"
            return
        }

        Write-Host "updating changelog table ($($script:PageLog.Count) pages)" -ForegroundColor Blue

        $rows = ($script:PageLog | Sort-Object @{Expression = 'Modified'; Descending = $true}, @{Expression = 'Title'; Descending = $false} | foreach {
            $when = Get-Date $_.Modified -Format 'MMMM d, yyyy'
@"
<TR>
<TD style="BORDER-TOP: #a3a3a3 1pt solid; BORDER-RIGHT: #a3a3a3 1pt solid; VERTICAL-ALIGN: top; BORDER-BOTTOM: #a3a3a3 1pt solid; PADDING-BOTTOM: 2pt; PADDING-TOP: 2pt; PADDING-LEFT: 3pt; BORDER-LEFT: #a3a3a3 1pt solid; PADDING-RIGHT: 3pt; WHITE-SPACE: nowrap">
<P lang=yo style="FONT-SIZE: 11.5pt; FONT-FAMILY: Calibri; MARGIN: 0in"><A href="$($_.Url)">$($_.Title)</A></P></TD>
<TD style="BORDER-TOP: #a3a3a3 1pt solid; BORDER-RIGHT: #a3a3a3 1pt solid; VERTICAL-ALIGN: top; BORDER-BOTTOM: #a3a3a3 1pt solid; PADDING-BOTTOM: 2pt; PADDING-TOP: 2pt; PADDING-LEFT: 3pt; BORDER-LEFT: #a3a3a3 1pt solid; PADDING-RIGHT: 3pt; WHITE-SPACE: nowrap">
<P lang=yo style="FONT-SIZE: 11.5pt; FONT-FAMILY: Calibri; MARGIN: 0in">$when</P></TD></TR>
"@
        }) -join "`n"

        $table = @"
<DIV style="DIRECTION: ltr">
<TABLE title="" style="BORDER-TOP: #a3a3a3 1pt solid; BORDER-RIGHT: #a3a3a3 1pt solid; BORDER-COLLAPSE: collapse; BORDER-BOTTOM: #a3a3a3 1pt solid; DIRECTION: ltr; BORDER-LEFT: #a3a3a3 1pt solid" cellSpacing=0 cellPadding=0 summary="" border=1 valign="top">
<TBODY>
<TR>
<TD style="BORDER-TOP: #a3a3a3 1pt solid; BORDER-RIGHT: #a3a3a3 1pt solid; VERTICAL-ALIGN: top; BORDER-BOTTOM: #a3a3a3 1pt solid; PADDING-BOTTOM: 2pt; PADDING-TOP: 2pt; PADDING-LEFT: 3pt; BORDER-LEFT: #a3a3a3 1pt solid; PADDING-RIGHT: 3pt; BACKGROUND-COLOR: #e5e0ec">
<P lang=yo style="FONT-SIZE: 11.5pt; FONT-FAMILY: Calibri; MARGIN: 0in"><SPAN style="FONT-WEIGHT: bold">Page</SPAN></P></TD>
<TD style="BORDER-TOP: #a3a3a3 1pt solid; BORDER-RIGHT: #a3a3a3 1pt solid; VERTICAL-ALIGN: top; BORDER-BOTTOM: #a3a3a3 1pt solid; PADDING-BOTTOM: 2pt; PADDING-TOP: 2pt; PADDING-LEFT: 3pt; BORDER-LEFT: #a3a3a3 1pt solid; PADDING-RIGHT: 3pt; BACKGROUND-COLOR: #e5e0ec">
<P lang=yo style="FONT-SIZE: 11.5pt; FONT-FAMILY: Calibri; MARGIN: 0in"><SPAN style="FONT-WEIGHT: bold">Last Modified</SPAN></P></TD></TR>
$rows
</TBODY></TABLE></DIV>
"@

        $updated = $content.Replace('~CHANGELOG~', $table)
        $updated | Out-File $file -Encoding utf8 -Force -Confirm:$false
    }

	function CompareFolders
	{
		$here = (Get-Location).path
		if ($here -ne 'C:\GitHub\OneMore\docs')
		{
			$ans = 'y'
			if (-not $Compare)
			{
				Write-Host
				$ans = Read-Host 'compare? (Y/N) [Y]'
			}
			if ($ans -ne 'n')
			{
				."$($env:ProgramFiles)\Beyond Compare 5\BCompare.exe" $here 'C:\GitHub\OneMore\docs'
			}
		}
		elseif ($Compare)
		{
			Write-Host 'cannot compare from source folder'
		}
	}
}
Process
{
	if ($Compare)
	{
		CompareFolders
		return
	}

    MakeSiteMap

    if ($ZipFile -and (Test-Path $ZipFile))
    {
        $script:ZipName = (Get-Item $ZipFile).BaseName
        Unpack $ZipFile
    }

    MakeSectionMap

    Get-ChildItem $ZipName -Directory | foreach {
        $name = $_.Name
        $dir, $sectionID = MakeSection $name

        if (Test-Path $name)
        {
            # delete the old section folder
            Remove-Item $name -Recurse -Force -Confirm:$false
        }

        if ($sectionID -and (Test-Path ./$sectionID))
        {
            Remove-Item ./$sectionID -Recurse -Force -Confirm:$false
        }

        if ($dir)
        {
            # move the new section folder up a level and rename
            Move-Item $dir ./$sectionID -Force -Confirm:$false
        }
    }

    if ($script:DevelopersTOC)
    {
        UpdateTelemetrySidebar $script:DevelopersTOC
    }

    UpdateChangelog

    Write-Host 'saving sitemap.xml'
    $sitemap.ToString() | Out-File 'sitemap.xml'

    RunPagefind

    Remove-Item $ZipName -Force -Confirm:$false

	CompareFolders
}
