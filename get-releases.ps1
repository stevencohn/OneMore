<#
Gets the release history of OneMore from GitHub, including prerelease versions. 
By default, only the most recent 20 releases are fetched, but the -All switch can be used to 
fetch all releases.

.PARAMETER All
Include all releases, including prerelease versions. 
By default, only the most recent 20 releases are included.

.PARAMETER Released
Only include released versions, excluding prerelease versions.
#>

using namespace System.Net.Http
using namespace System.Net.Http.Headers

[CmdletBinding()]
param(
    [switch]$Released,
    [switch]$All
)

Begin
{
    $releases = [System.Collections.Generic.List[Release]]::new()

    class Release {
        [string]$Tag
        [string]$Name
        [datetime]$Published
        [int]$Age
        [bool]$Prerelease
        [int]$Downloads64
        [int]$Downloads86
        [int]$DownloadsArm
    }

    function GetReleases
    {
        param(
            [System.Collections.Generic.List[Release]]$Releases,
            [int]$Page,
            [int]$PerPage
        )

        Write-Host "... fetching page $Page"

        $url = "https://api.github.com/repos/stevencohn/OneMore/releases?page=$Page&per_page=$PerPage"
        $count = 0

        $client = [HttpClient]::new()
        $client.DefaultRequestHeaders.UserAgent.Add(
            [ProductInfoHeaderValue]::new("OneMore", "1.0")
        )

        $response = $client.GetAsync($url).GetAwaiter().GetResult()

        if ($response.IsSuccessStatusCode)
        {
            $json = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            $dom = $json | ConvertFrom-Json

            foreach ($rel in $dom)
            {
                if ($Released -and $rel.prerelease) { continue }

                $published = [datetime]$rel.published_at

                $release = [Release]::new()
                $release.Tag        = $rel.tag_name
                $release.Name       = $rel.name
                $release.Published  = $published
                $release.Prerelease = $rel.prerelease

                foreach ($asset in $rel.assets)
                {
                    $name = $asset.name

                    if ($name -match "ARM") { $release.DownloadsArm = $asset.download_count }
                    elseif ($name -match "64") { $release.Downloads64 = $asset.download_count }
                    else { $release.Downloads86 = $asset.download_count }
                }

                $Releases.Add($release)
                $count++
            }
        }
        else
        {
            Write-Warning "Status code: $($response.StatusCode)"
        }

        return $count -gt 0
    }


    function PatchAges
    {
        # prereleases age against whatever immediately superseded them
        # stable releases age against the next stable release, skipping any prereleases
        $today = Get-Date
        for ($i = 0; $i -lt $releases.Count; $i++)
        {
            if ($releases[$i].Prerelease)
            {
                $releases[$i].Age =
                    if ($i -eq 0) { ($today - $releases[$i].Published).Days }
                    else { ($releases[$i - 1].Published - $releases[$i].Published).Days }
            }
            else
            {
                $nextStable = -1
                for ($j = $i - 1; $j -ge 0; $j--)
                {
                    if (-not $releases[$j].Prerelease) { $nextStable = $j; break }
                }

                $releases[$i].Age =
                    if ($nextStable -eq -1) { ($today - $releases[$i].Published).Days }
                    else { ($releases[$nextStable].Published - $releases[$i].Published).Days }
            }
        }
    }


    function Report
    {
        $tagW   = [Math]::Max("Tag".Length,          ($releases | foreach { $_.Tag.Length }                  | measure -Maximum).Maximum)
        $nameW  = [Math]::Max("Name".Length,         ($releases | foreach { $_.Name.Length }                 | measure -Maximum).Maximum)
        $pubW   = [Math]::Max("Published".Length,    ($releases | foreach { $_.Published.ToString().Length } | measure -Maximum).Maximum)
        $ageW   = [Math]::Max("Age".Length,          ($releases | foreach { "$($_.Age)".Length }             | measure -Maximum).Maximum)
        $dl64W  = [Math]::Max("Downloads64".Length,  ($releases | foreach { "$($_.Downloads64)".Length }     | measure -Maximum).Maximum)
        $dl86W  = [Math]::Max("Downloads86".Length,  ($releases | foreach { "$($_.Downloads86)".Length }     | measure -Maximum).Maximum)
        $dlArmW = [Math]::Max("DownloadsArm".Length, ($releases | foreach { "$($_.DownloadsArm)".Length }    | measure -Maximum).Maximum)

        if ($nameW -gt 60) { $nameW = 60 }

        $fmt = "{0,-$tagW}  {1,-$nameW}  {2,-$pubW}  {3,$ageW}  {4,$dl64W}  {5,$dl86W}  {6,$dlArmW}"

        Write-Host
        Write-Host ($fmt -f "Tag", "Name", "Published", "Age", "Downloads64", "Downloads86", "DownloadsArm") -fo Green
        Write-Host ($fmt -f ("-" * $tagW), ("-" * $nameW), ("-" * $pubW), ("-" * $ageW), ("-" * $dl64W), ("-" * $dl86W), ("-" * $dlArmW))

        foreach ($r in $releases)
        {
            $name = $r.Name
            if ($name.Length -gt 60) { $name = $name.Substring(0, 57) + "..." }

            $line = $fmt -f $r.Tag, $name, $r.Published.ToString(), $r.Age, $r.Downloads64, $r.Downloads86, $r.DownloadsArm
            if ($r.Prerelease) {
                Write-Host $line -ForegroundColor DarkGray
            } else {
                Write-Host $line
            }
        }
    }
}
Process
{
    if ($All)
    {
        $page = 1
        while (GetReleases $releases $page 100)
        {
            $page++
        }
        Write-Host "`nUse the Results panel Export menu to export to Excel"
    }
    else
    {
        GetReleases $releases 1 20 | Out-Null
    }

    PatchAges
    Report
}
