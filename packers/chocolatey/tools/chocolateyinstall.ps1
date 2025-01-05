$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/stevencohn/OneMore/releases/download/6.7.0/OneMore_6.7.0_Setupx86.msi'
$url64      = 'https://github.com/stevencohn/OneMore/releases/download/6.7.0/OneMore_6.7.0_Setupx64.msi'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'MSI'
  url           = $url
  url64bit      = $url64
  softwareName  = 'OneMore*'
  checksum      = '410AD464AE692FA23AD44CD85B8159E2E2775751AA0E413F0C773195C128A856'
  checksumType  = 'sha256'
  checksum64    = 'AF3712289DCB4C67C8429E3C0A095D730B47AD40CD051793DEE460FAF4616314'
  checksumType64= 'sha256'
  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
