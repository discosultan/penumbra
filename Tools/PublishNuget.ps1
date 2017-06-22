# Define variables.
$idWindowsDX = 'MonoGame.Penumbra.WindowsDX'
$idDesktopGL = 'MonoGame.Penumbra.DesktopGL'

$nuspecSuffix = '.nuspec'
$nupkgSuffix = '.nupkg'

$versionRegex = '(?<=(' + $id + '" version\="|\<version\>))\d+.\d+.\d+'
$newVersion = Read-Host 'What is the new version?'

# Define functions.
Function UpdateVersionInFile($fileName)
{
	(Get-Content $fileName) | 
	Foreach-Object {$_ -replace $versionRegex, $newVersion} |
	Out-File $fileName
}
Function GetNuspecFilename($id)
{
	return $id + $nuspecSuffix
}
Function GetNupkgFilename($id)
{
	return $id + '.' + $newVersion + $nupkgSuffix
}

# Replace version numbers in nuspec files.
Write-Host Replacing version numbers
UpdateVersionInFile (GetNuspecFilename $idWindowsDX) $versionRegex $newVersion
UpdateVersionInFile (GetNuspecFilename $idDesktopGL) $versionRegex $newVersion

# Create nuget packages with symbol packages.
Write-Host Creating packages
nuget pack (GetNuspecFilename $idWindowsDX) -symbols
nuget pack (GetNuspecFilename $idDesktopGL) -symbols

# Publish nuget packages to nuget.org and symbol packages to symbolsource.org
Write-Host Publishing packages
nuget push (GetNupkgFilename $idWindowsDX) -Source https://www.nuget.org/api/v2/package
nuget push (GetNupkgFilename $idDesktopGL) -Source https://www.nuget.org/api/v2/package