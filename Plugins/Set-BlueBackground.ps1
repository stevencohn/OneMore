[CmdletBinding(SupportsShouldProcess = $true)]

param (
	[Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
	[string] $Path
)

Begin
{
	function UpdatePageXml ($filePath)
	{
		$null = [Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq")

	    $xml = [Xml.Linq.XElement]::Load($filePath)
	    Write-Host "Loaded $filepath"
	    Write-Host 'Setting page color to #ECF3FA'

	    $ns = $xml.GetNamespaceOfPrefix('one')
	    $xml.Element($ns + 'PageSettings').Attribute('color').value = '#ECF3FA'

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