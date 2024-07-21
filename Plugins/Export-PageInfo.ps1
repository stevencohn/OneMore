<#
.SYNOPSIS
This is a OneMore plugin that extracts reads the hierarchy and generates an Excel CSV file
containing information for every page within that scope.

.PARAMETER Path
The path of a OneNote hierarchy XML file. The root migth be one of Notebooks, Notebook,
or Section. 

.PARAMETER CsvPath
The user argument passed to the script, specifies the full path of the output CSV file.
#>

[CmdletBinding(SupportsShouldProcess = $true)]

param (
	[Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
	[string] $Path,

	[Parameter(Position = 1, Mandatory = $true, ValueFromPipeline = $true)]
	[string] $CsvPath
)

Begin
{
	function ExportNotebook
	{
		param([Xml.Linq.XElement]$notebook)

		$notebookName = $notebook.Attribute('name').Value
		Write-Host "exporting notebook $notebookName"
		$notebook.Elements($ns + 'Section') | foreach { ExportSection $_ $notebookName }

		ExportSectionGroups $notebook $notebookName
	}


	function ExportSectionGroups
	{
		param([Xml.Linq.XElement]$root, [string]$notebookName)

		$root.Elements($ns + 'SectionGroup') | foreach `
		{
			if (!$_.Attribute('isRecycleBin'))
			{
				$groupName = $_.Attribute('name').Value
				$_.Elements($ns + 'Section') | foreach `
				{
					$sectionName = $_.Attribute('name').Value
					$_.Attribute('name').Value = "$groupName / $sectionName"
					ExportSection $_ $notebookName
				}
			}
		}
	}


	function ExportSection
	{
		param([Xml.Linq.XElement]$section, [string]$notebookName)

		$sectionName = $section.Attribute('name').Value
		Write-Host "exporting section $sectionName"
		$section.Elements($ns + 'Page') | foreach { ExportPage $_ $sectionName $notebookName }

		ExportSectionGroups $section $notebookName
	}


	function ExportPage
	{
		param([Xml.Linq.XElement]$page, [string]$sectionName, [string]$notebookName)

		$name = $page.Attribute('name').Value

		# convert Zulu to local time
		$created = (Get-Date $page.Attribute('dateTime').Value).ToString()
		$modified = (Get-Date $page.Attribute('lastModifiedTime').Value).ToString()

		Write-Host "exporting page $name"
		"`"$notebookName`",`"$sectionName`",`"$name`",$created,$modified" | Out-File -FilePath $CsvPath -Append
	}


	function Export($filePath)
	{
		$null = [Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq")

		$script:xml = [Xml.Linq.XElement]::Load($filePath)
		Write-Host "Loaded $filepath"
		Write-Host "Exporting page info to $CsvPath ..."

		# force Excel to use comma as CSV delimeter, opening in columnar mode
		# this will not be part of the spreadsheet; it is discarded after the file is open
		'SEP=,' | Out-File -FilePath $CsvPath

		'Notebook,Section,Page,Created,Modified' | Out-File -FilePath $CsvPath -Append

		$script:ns = $xml.GetNamespaceOfPrefix('one')

		if ($xml.Name.LocalName -eq 'Section')
		{
			ExportSection $xml ''
		}
		elseif ($xml.name.LocalName -eq 'Notebook')
		{
			ExportNotebook $xml
		}
		else
		{
			# all notebooks
			$xml.Elements($ns + 'Notebook') | foreach { ExportNotebook $_ }
		}
	}
}
Process
{
	$filepath = Resolve-Path $Path -ErrorAction SilentlyContinue
	if (!$filepath)
	{
		Write-Host "Could not find file $Path" -ForegroundColor Yellow
		return
	}

	if (!$CsvPath)
	{
		$CsvPath = Join-Path [Environment]::GetFolderPath('MyDocuments') 'OneNote_PageInfo.csv'
	}

	Export $filepath

	Write-Host 'Done'
}