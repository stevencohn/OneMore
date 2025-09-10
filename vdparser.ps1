
function ConvertVDProjToJson
{
	param($vdproj)

	# read .vdproj, skipping "DeployProject" line
	$lines = (Get-Content $vdproj) | Select-Object -Skip 1

	$depth = 0
	$containerDepth = -1

	$script:vdtext = @('')

	for ($i = 0; $i -lt $lines.Count; $i++)
	{
		$line = $lines[$i]

		# match object names, e.g. "Hierarchy", "Entry", "Deployable"
		if ($line -match '^\s*"([^"]+)"$')
		{
			# Hierarchy.Entry[] is the only collection with duplicate names.
			# So we only need to track Entries and wrap them in a JSON array.

			if ($matches[1] -eq 'Hierarchy')
			{
				$containerDepth = $depth
				$vdtext += "$line`:"
			}
			elseif ($matches[1] -ne 'Entry') # skip Entry object names
			{
				$vdtext += "$line`:"
			}
		}
		# match property lines, e.g. "Name" = "MyApp"
		elseif ($line -match '^(\s*)("[^"]+") = ("(.*)")$')
		{
			$pair = "$($matches[1])$($matches[2]): $($matches[3])"
			if (($i -lt $lines.Count - 1) -and -not $lines[$i+1].EndsWith('}'))
			{
				$vdtext += "$pair,"
			}
			else
			{
				$vdtext += "$pair"
			}
		}
		else
		{
			$tag = $line.Trim()
			if ($tag -eq '{')
			{
				# if we are entering an Entry object, start a JSON array if not already started
				if ($depth -eq $containerDepth)
				{
					$line = $line.Replace('{', '[')
				}

				$depth = $depth + 1
			}
			elseif ($tag -eq '}')
			{
				# if we are exiting an Entry object, close the JSON array
				$depth = $depth - 1
				if ($depth -eq $containerDepth)
				{
					$line = $line.Replace('}', ']')
					$containerDepth = -1
				}

				# add comma if next line is not closing brace or last property line in object
				if (($i -lt $lines.Count - 1) -and 
					($lines[$i+1] -match '^\s*"[^"]+"$' -or $lines[$i+1] -match '^(\s*)("[^"]+"\s*)= ("(.*)")$'))
				{
					$line = "$line,"
				}
			}

			$vdtext += "$line"
		}
	}

	$json = $vdtext | ConvertFrom-Json -Depth 100
	return $json
}

function ConvertJsonToVDProj
{
	param($json, $vdproj)

	'"DeployProject"' | Out-File $vdproj

	$hierarchy = $false

	$text = ($json | ConvertTo-Json -Depth 100) -split "`r`n"
	foreach ($line in $text)
	{
		# object name
		if ($line -match '^(\s*)("[^"]+"): [\[\{]$')
		{
			$matches[1] + $matches[2] | Out-File $vdproj -Append
			$matches[1] + '{' | Out-File $vdproj -Append
			if ($matches[2] -eq '"Hierarchy"')
			{
				$hierarchy = $true
			}
		}
		# convert JSON properties back to vdproj properties
		elseif ($line -match '^(\s*)("[^"]+")\s*:\s*(".*"),?$')
		{
			$matches[1] + $matches[2] + ' = ' + $matches[3] | Out-File $vdproj -Append
		}
		# end of Hierarchy.Entries collection
		elseif ($line -match '^(\s*)],?$')
		{
			$matches[1] + '}' | Out-File $vdproj -Append
			$hierarchy = $false
		}
		# end of object
		elseif ($line -match '^(\s*)},?$')
		{
			$matches[1] + '}' | Out-File $vdproj -Append
		}
		# start of object, perhaps Hierarchy.Entry
		elseif ($line -match '^(\s*){$')
		{
			if ($hierarchy)
			{
				$matches[1] + '"Entry"' | Out-File $vdproj -Append
			}

			$line | Out-File $vdproj -Append
		}
		# empty object
		elseif ($line -match '^(\s*)("[^"]+"): {},?$')
		{
			$matches[1] + $matches[2] | Out-File $vdproj -Append
			$matches[1] + '{' | Out-File $vdproj -Append
			$matches[1] + '}' | Out-File $vdproj -Append
		}
		# unhandled lines
		else
		{
			Write-Host "- $line" -Fore DarkGray
		}
	}
}

function ExplodeNoteProperties
{
	[CmdletBinding()]
	param([Parameter(ValueFromPipeline)]$json)
	Process
	{
		# explode hashtable NoteProperty into object of properties
		$json | Get-Member -MemberType NoteProperty | foreach {
			$omKey = $_.Name
			$obj = $json.$omKey
			# inject omKey property into object to hold the object's name (json key)
			$obj | Add-Member -MemberType NoteProperty -Name 'omKey' -Value $omKey -Force
			Write-Output $obj
		}
	}
}
