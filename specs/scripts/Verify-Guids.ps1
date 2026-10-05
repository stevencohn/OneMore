<#
.SYNOPSIS
Checks that the page GUIDs stored in the identity catalog equal the ones OneNote reports now.

.DESCRIPTION
Takes a random sample of live pages from identity_page (read-only) that have a stored hyperlink GUID, asks
OneNote for each page's link by its stored page ID, and compares the page-id GUID in that link with the stored
one. A stored page ID can be stale after a notebook is reopened, in which case OneNote reports an error for it;
those are counted separately and are not mismatches.

The GUID in a link is the most persistent page identifier measured (it survives reopen, restart, and move), so
the stored and live values are expected to match. See TechNote - OneNote ID Stability.md.

Nothing is written to OneNote or to the database. OneNote must be running with the notebooks open.

Must be run in Windows PowerShell 5.1 (powershell.exe). PowerShell 7 cannot drive the OneNote COM object.

.PARAMETER Database
The OneMore database. Defaults to the one the add-in uses.

.PARAMETER SQLite
System.Data.SQLite.dll, to read the database. Defaults to the one built with the add-in (OneMore\bin\Debug).

.PARAMETER Sample
How many pages to check. Each costs about 14 ms in OneNote.

.EXAMPLE
powershell.exe -File .\Verify-Guids.ps1 -Sample 200

.NOTES
Exits with 1 if any sampled GUID differed, otherwise 0.
#>

#Requires -Version 5.1

[CmdletBinding()]
param(
    [string] $Database = (Join-Path $env:LOCALAPPDATA 'OneMore\OneMore.db'),
    [string] $SQLite,
    [int] $Sample = 80
)

Begin {
    function ReadSample([int] $count) {
        $command = $connection.CreateCommand()
        $command.CommandText = "select pageKey, pageID, pageGuid, title from identity_page
            where pageGuid is not null and missingSince is null order by random() limit $count"
        $reader = $command.ExecuteReader()
        $rows = @()
        while ($reader.Read()) {
            $rows += [pscustomobject] @{
                Key = $reader.GetValue(0)
                PageID = $reader.GetValue(1)
                Guid = $reader.GetValue(2)
                Title = $reader.GetValue(3)
            }
        }
        $reader.Close()
        return $rows
    }
}

End {
    if ($PSVersionTable.PSEdition -eq 'Core') {
        throw 'Run this in Windows PowerShell 5.1 (powershell.exe); PowerShell 7 cannot drive OneNote.'
    }

    # $PSScriptRoot is empty in a param default in Windows PowerShell 5.1, so the default is made here
    if (-not $SQLite) {
        $SQLite = Join-Path $PSScriptRoot '..\..\OneMore\bin\Debug\System.Data.SQLite.dll'
    }

    Add-Type -Path $SQLite
    $connection = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$Database;Read Only=True")
    $connection.Open()
    $pages = ReadSample $Sample
    $connection.Close()

    $one = New-Object -ComObject OneNote.Application
    $same = 0
    $errors = 0
    $different = @()

    foreach ($page in $pages) {
        [string] $link = ''
        try {
            $one.GetHyperlinkToObject($page.PageID, '', [ref] $link)
        }
        catch {
            $errors++
            continue
        }

        $live = [regex]::Match($link, 'page-id=(\{[0-9A-Fa-f-]+\})').Groups[1].Value.ToUpper()
        if ($live -eq $page.Guid.ToUpper()) {
            $same++
        }
        else {
            $different += $page
        }
    }

    'sampled {0}: stored GUID equals live GUID for {1}; different {2}; OneNote errors {3}' -f
        $pages.Count, $same, $different.Count, $errors

    $different | Select-Object -First 5 | ForEach-Object {
        '  DIFFERENT key={0} "{1}" stored={2}' -f $_.Key, $_.Title, $_.Guid
    }

    if ($different.Count -gt 0) {
        exit 1
    }
}
