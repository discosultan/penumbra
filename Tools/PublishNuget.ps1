# Define variables.
$id = 'MonoGame.Penumbra.WindowsDX'

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
UpdateVersionInFile (GetNuspecFilename $id) $versionRegex $newVersion

# Create nuget packages with symbol packages.
Write-Host Creating packages
.\nuget pack (GetNuspecFilename $id) -symbols

# Publish nuget packages to nuget.org and symbol packages to symbolsource.org
Write-Host Publishing packages
.\nuget push (GetNupkgFilename $id) -Source https://www.nuget.org/api/v2/package