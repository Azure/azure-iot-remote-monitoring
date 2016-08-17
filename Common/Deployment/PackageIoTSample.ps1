Param(
    [Parameter(Mandatory=$True,Position=0)]
    $configuration
)

$root = Resolve-Path('.');
$webPackageDir = "$root\DeviceAdministration\Web\obj\$configuration\package\Web.zip";
$jobPackageDir = "$root\WebJobHost\obj\$configuration\package\WebJobHost.zip";
$packageDir = "$root\Build_Output\$configuration\package";
. "$(Split-Path $MyInvocation.MyCommand.Path)\DeploymentLib.ps1"

if ((Test-Path $webPackageDir) -ne $TRUE) {
    throw "Failed to find package for DeviceAdministration Web. Did you run 'build.cmd package' command?";
}

if ((Test-Path $jobPackageDir) -ne $TRUE) {
    throw "Failed to find package for WebJobHost. Did you run 'build.cmd package' command?";
}

if (((Test-Path $packageDir) -ne $TRUE)) {
    Write-Host 'Creating package directory $packageDir';
    New-Item -Path $packageDir -ItemType Directory
}

Write-Host 'Cleaning up previously generated packages';
if ((Test-Path "$packageDir\Web.zip") -eq $TRUE) {
    Remove-Item "$packageDir\Web.zip"
}

if ((Test-Path "$packageDir\WebJobHost.zip") -eq $TRUE) {
    Remove-Item "$packageDir\WebJobHost.zip"
}

Write-Host "Copying packages to package directory";
Copy-Item $webPackageDir -Destination $packageDir
Copy-Item $jobPackageDir -Destination $packageDir
FixWebJobZip "$packageDir\WebJobHost.zip"
