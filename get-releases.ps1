using namespace System.Net.Http
using namespace System.Net.Http.Headers

# Define a Release object type
class Release {
    [string]$Tag
    [string]$Name
    [datetime]$Published
    [int]$Age
    [int]$Downloads64
    [int]$Downloads86
    [int]$DownloadsArm
}

# Async GitHub fetch function
function Get-ReleasesAsync {
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

    if ($response.IsSuccessStatusCode) {
        $now = Get-Date
        $json = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $dom = $json | ConvertFrom-Json

        foreach ($rel in $dom) {
            $published = [datetime]$rel.published_at

            $release = [Release]::new()
            $release.Tag       = $rel.tag_name
            $release.Name      = $rel.name
            $release.Published = $published
            $release.Age       = ($now - $published).Days

            $now = $published

            foreach ($asset in $rel.assets) {
                $name = $asset.name

                if ($name -match "ARM") {
                    $release.DownloadsArm = $asset.download_count
                }
                elseif ($name -match "64") {
                    $release.Downloads64 = $asset.download_count
                }
                else {
                    $release.Downloads86 = $asset.download_count
                }
            }

            $Releases.Add($release)
            $count++
        }
    }
    else {
        Write-Warning "Status code: $($response.StatusCode)"
    }

    return $count -gt 0
}

# Main logic
$releases = [System.Collections.Generic.List[Release]]::new()

$useSinglePage = $true

if ($useSinglePage) {
    Get-ReleasesAsync -Releases $releases -Page 1 -PerPage 20 | Out-Null
}
else {
    $page = 1
    while (Get-ReleasesAsync -Releases $releases -Page $page -PerPage 100) {
        $page++
    }
    Write-Host "`nUse the Results panel Export menu to export to Excel"
}

$releases | Format-Table
