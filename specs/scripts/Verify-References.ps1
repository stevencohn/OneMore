<#
.SYNOPSIS
Checks that the references OneMore stores still resolve against the notebooks open in OneNote right now.

.DESCRIPTION
Reads favorites and layout windows from OneMore.db (read-only) and, for each stored reference, asks OneNote
about the stored IDs. This is the check that compares stored values with the LIVE hierarchy; a database that
is merely consistent with itself can still hold IDs that OneNote regenerated.

For a page favorite or layout window it reports
  - whether the stored page ID and section ID still exist (they stop existing when a notebook is reopened),
  - whether the stored link's page and section GUIDs equal those of the link OneNote generates now, and
  - whether the row has a page key.
For a section, section group, or notebook favorite it reports whether the stored section and notebook IDs
still exist.

Nothing is written to OneNote or to the database. OneNote must be running with the notebooks open.

Must be run in Windows PowerShell 5.1 (powershell.exe). PowerShell 7 cannot drive the OneNote COM object.

.PARAMETER Database
The OneMore database. Defaults to the one the add-in uses.

.PARAMETER SQLite
System.Data.SQLite.dll, to read the database. Defaults to the one built with the add-in (OneMore\bin\Debug).

.EXAMPLE
powershell.exe -File .\Verify-References.ps1

.NOTES
Exits with 1 if any stored reference failed, otherwise 0.
#>

#Requires -Version 5.1

[CmdletBinding()]
param(
    [string] $Database = (Join-Path $env:LOCALAPPDATA 'OneMore\OneMore.db'),
    [string] $SQLite
)

Begin {
    function ReadRows([string] $sql) {
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        $reader = $command.ExecuteReader()
        $rows = @()
        while ($reader.Read()) {
            $row = @{}
            for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                $value = $reader.GetValue($i)
                $row[$reader.GetName($i)] = $(if ($value -is [DBNull]) { $null } else { $value })
            }
            $rows += [pscustomobject] $row
        }
        $reader.Close()
        return $rows
    }

    # the link OneNote generates now for a hierarchy ID, or $null if the ID no longer exists
    function LinkOf([string] $id) {
        [string] $link = ''
        try {
            $one.GetHyperlinkToObject($id, '', [ref] $link)
            return $link
        }
        catch {
            return $null
        }
    }

    # the GUID of a link part, such as page-id or section-id, upper-cased for comparison
    function GuidOf([string] $link, [string] $part) {
        return [regex]::Match($link, "$part=(\{[0-9A-Fa-f-]+\})").Groups[1].Value.ToUpper()
    }

    function NotebookExists([string] $id) {
        [string] $xml = ''
        try {
            $one.GetHierarchy($id, 'hsSelf', [ref] $xml, 'xs2013')
            return $true
        }
        catch {
            return $false
        }
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

    $pages = ReadRows "select 'favorite' as kind, favoriteID as id, name, uri, pageID, sectionID, pageKey
        from favorite where pageID is not null"
    $windows = ReadRows "select 'layout window' as kind, windowID as id, name, uri, pageID, sectionID, pageKey
        from layout_window"
    $containers = ReadRows "select 'container favorite' as kind, favoriteID as id, name, sectionID, notebookID
        from favorite where pageID is null"
    $connection.Close()

    $one = New-Object -ComObject OneNote.Application
    $total = 0
    $failed = 0

    foreach ($item in @($pages) + @($windows)) {
        $total++
        $link = LinkOf $item.pageID
        $pageLive = $null -ne $link
        $linkSame = $pageLive -and
            ((GuidOf $link 'page-id') -eq (GuidOf $item.uri 'page-id')) -and
            ((GuidOf $link 'section-id') -eq (GuidOf $item.uri 'section-id'))
        $sectionLive = $null -ne (LinkOf $item.sectionID)

        if (-not ($pageLive -and $linkSame -and $sectionLive)) {
            $failed++
        }

        '{0,-17} {1,3} {2,-28} keyed={3,-5} pageID live={4,-5} sectionID live={5,-5} link==live={6}' -f
            $item.kind, $item.id, $item.name, ($null -ne $item.pageKey), $pageLive, $sectionLive, $linkSame
    }

    foreach ($item in $containers) {
        $total++
        $sectionLive = $null -ne (LinkOf $item.sectionID)
        $notebookLive = NotebookExists $item.notebookID

        if (-not ($sectionLive -and $notebookLive)) {
            $failed++
        }

        '{0,-17} {1,3} {2,-28} sectionID live={3,-5} notebookID live={4}' -f
            $item.kind, $item.id, $item.name, $sectionLive, $notebookLive
    }

    ''
    "checked $total stored references; $failed failed"

    if ($failed -gt 0) {
        exit 1
    }
}
