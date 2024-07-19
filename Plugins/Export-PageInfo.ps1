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

		$name = $notebook.Attribute('name').Value
		write-host "exporting notebook $name"
		$notebook.Elements($ns + 'Section') | foreach { ExportSection $_ $name }
	}


	function ExportSection
	{
		param([Xml.Linq.XElement]$section, [string]$notebookName)

		$name = $section.Attribute('name').Value
		write-host "exporting section $name"
		$section.Elements($ns + 'Page') | foreach { ExportPage $_ $name $notebookName }

		$section.Elements($ns + 'SectionGroup') | foreach `
		{
			$group = $_.Attribute('name').Value
			ExportSection $_ "$name / $group" $notebookName
		}
	}


	function ExportPage
	{
		param([Xml.Linq.XElement]$page, [string]$sectionName, [string]$notebookName)

		$name = $page.Attribute('name').Value

		# convert Zulu to local time
		$created = (Get-Date $page.Attribute('dateTime').Value).ToString()
		$modified = (Get-Date $page.Attribute('lastModifiedTime').Value).ToString()

		write-host "exporting page $name"
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
			$xml.Elements($ns + 'Notebook') | % ExportNotebook $_
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