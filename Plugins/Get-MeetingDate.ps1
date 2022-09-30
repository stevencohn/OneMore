<#
.SYNOPSIS
This is a OneMore plugin that extracts the date stamp from the header of a meeting
page imported from Outlook and prefix the page title with the Meeting Date date
in the form yyyy-mm-dd

.PARAMETER Path
The path of a OneNote page XML file. The plugin must update this file in order for chnages
to be applied by OneMore. If no changes are detected then the current page is not updated.

.DESCRIPTION
Possible enhancement is to use Threading.Thread.CurrentUICulture.CultureInfo.ShortDatePattern
to determine the m/d/y order and re-order as necessary to be y/m/d for sortability
#>

[CmdletBinding(SupportsShouldProcess = $true)]

param (
	[Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
	[string] $Path
)

Begin
{
	# match shortdate+shorttime pattern for all cultures
	$pattern = '(\d{1,4}[/\-\.]\d{1,4}[/\-\.]\d{1,4}) \d{1,2}:\d{1,2}'

	function UpdatePageXml ($filePath)
	{
		$null = [Reflection.Assembly]::LoadWithPartialName('System.Xml.Linq')

		$xml = [Xml.Linq.XElement]::Load($filePath)
		Write-Host "Loaded $filepath"

		# <one:Meta name="AppendedText" content="4/22/2021 3:00 PM" />

		$ns = $xml.GetNamespaceOfPrefix('one')
		$xml.Descendants($ns + 'Meta') | ? `
		{
			$_.Attribute('name').Value -eq 'AppendedText' -and `
			$_.Attribute('content').Value -match $pattern
		} | % `
		{
			# presumes there is exactly one occurence of 'Meeting Date:' on the page!

			$date = $Matches[1]
			Write-Host "Found match $date"
			
			$cdata = $xml.Elements($ns + 'Title').Elements($ns + 'OE').Elements($ns + 'T').DescendantNodes() | ? `
			{
				$_.NodeType -eq [Xml.XmlNodeType]::CDATA
			}

			# single XCData in title, meaning cursor is not in title
			if ($cdata.GetType().Name -eq 'XCData')
			{
				$cdata.Value = "$date $($cdata.Value)"
			}
			else
			{
				# handle case where cursor is in title or part of title is selected
				$text = ''
				$cdata | % { $text = "$text$($_.Value)" }

				$c = 0
				$cdata | % { `
					if ($c -eq 0) { $_.Value = "$date $text" } else { $_.Parent.Remove() }
					$c = $c + 1
				}
			}

			Write-Host "set title to [$text]"
		}
		
		$xml.Save($filePath, [Xml.Linq.SaveOptions]::None)
		Write-Host "Saved $filepath"
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

    UpdatePageXml $filepath

    Write-Host 'Done'
}
