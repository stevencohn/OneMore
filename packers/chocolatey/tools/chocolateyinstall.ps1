$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/stevencohn/OneMore/releases/download/6.8.4/OneMore_6.8.4_Setupx86.msi'
$url64      = 'https://github.com/stevencohn/OneMore/releases/download/6.8.4/OneMore_6.8.4_Setupx64.msi'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'MSI'
  url           = $url
  url64bit      = $url64
  softwareName  = 'OneMore*'
  checksum      = '1687B2E6FFBC7ACDCF25517D1001F3F94DE864A574165382A869CDC4BCE8D605'
  checksumType  = 'sha256'
  checksum64    = '3848AFD8518A605CF57A2E52E525982CB8CB446C1A3E19EE74F8E61107D4D7CF'
  checksumType64= 'sha256'
  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
