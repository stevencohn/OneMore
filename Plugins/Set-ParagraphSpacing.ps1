<#
.SYNOPSIS
Set the paragraph spacing for all paragraphs in each page of the current hierarchy.

.PARAMETER Path
The path of a OneNote hierarchy XML file. The root migth be one of Notebooks, Notebook,
or Section.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Path,

    # user arguments; override these defaults using the User Arguments field
    # in the Plugins Run dialog
    [double]$Before = 0.0,
    [double]$After = 0.0,
    [double]$Between = 0.0,
    [switch]$Lists,
    [switch]$Tables
)

Begin
{
    $script:SpaceBefore  = $Before
    $script:SpaceAfter   = $After
    $script:SpaceBetween = $Between
    $script:AllowListOEs  = $Lists.IsPresent
    $script:AllowTableOEs = $Tables.IsPresent

    # here we go...

    # create OneNote COM object once
    $script:one = New-Object -ComObject OneNote.Application

    function GetHierarchyXml
    {
        param([string]$Path)

        if (-not (Test-Path $Path))
        {
            throw "Hierarchy XML file not found: $Path"
        }

        [xml]$xml = Get-Content -Path $Path -Raw

        # register namespace for hierarchy XML
        $script:ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
        $ns.AddNamespace("one", "http://schemas.microsoft.com/office/onenote/2013/onenote")

        return $xml
    }

    function GetPageIds
    {
        param([xml]$Hierarchy)

        return $Hierarchy.SelectNodes("//one:Page", $ns) |
            ForEach-Object { $_.ID }
    }

    function LoadPageXml
    {
        param([string]$PageId)

        [xml]$pageXml = ""
        $one.GetPageContent(
            $PageId,
            [ref]$pageXml,
            [Microsoft.Office.Interop.OneNote.PageInfo]::piAll
        )

        # Register namespace for page XML
        $pageNs = New-Object System.Xml.XmlNamespaceManager($pageXml.NameTable)
        $pageNs.AddNamespace("one", "http://schemas.microsoft.com/office/onenote/2013/onenote")

        return @{ Xml = $pageXml; Ns = $pageNs }
    }

    function ShouldProcessOE
    {
        param(
            [System.Xml.XmlElement]$OE,
            [System.Xml.XmlNamespaceManager]$Ns
        )

        # table filtering
        if (-not $AllowTableOEs)
        {
            if ($OE.SelectSingleNode("ancestor::one:Cell", $Ns))
            {
                return $false
            }
        }

        # list filtering
        if (-not $AllowListOEs)
        {
            if ($OE.SelectSingleNode("one:List", $Ns))
            {
                return $false
            }

            if ($OE.SelectSingleNode("ancestor::one:List", $Ns))
            {
                return $false
            }
        }

        return $true
    }

    function ApplyOESpacing
    {
        param([System.Xml.XmlElement]$OE)

        $OE.SetAttribute("spaceBefore",  $SpaceBefore)
        $OE.SetAttribute("spaceAfter",   $SpaceAfter)
        $OE.SetAttribute("spaceBetween", $SpaceBetween)
    }

    function ProcessPage
    {
        param([string]$PageId)

        $pageData = LoadPageXml -PageId $PageId
        $pageXml  = $pageData.Xml
        $pageNs   = $pageData.Ns

        $pageNode = $pageXml.SelectSingleNode("//one:Page", $pageNs)
        if ($pageNode -and $pageNode.name)
        {
            Write-Host "Processing page: $($pageNode.name)"
        }
        else
        {
            Write-Host "Processing page (no name): $PageId"
        }

        # process OE elements
        $oeNodes = $pageXml.SelectNodes("//one:OE", $pageNs)
        foreach ($oe in $oeNodes)
        {
            if (ShouldProcessOE -OE $oe -Ns $pageNs)
            {
                ApplyOESpacing -OE $oe
            }
        }

        $one.UpdatePageContent($pageXml.OuterXml)
    }
}
Process
{
    $hierarchy = GetHierarchyXml -Path $Path
    $pageIds   = GetPageIds -Hierarchy $hierarchy

    foreach ($pageId in $pageIds)
    {
        ProcessPage -PageId $pageId
    }
}
End
{
    Write-Host "Completed updating OE spacing for all eligible pages."
}
