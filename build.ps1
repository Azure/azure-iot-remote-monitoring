Param(
    [Parameter(Mandatory=$True,Position=0)]
    $command,
    [Parameter(Mandatory=$True,Position=1)]
    $configuration,
    [Parameter(Mandatory=$True,Position=2)]
    $environmentName,
    [Parameter(Mandatory=$False,Position=3)]
    $msbuild = "${Env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe"
    )

If (!($environmentName -match '^(?![0-9]+$)(?!-)[a-zA-Z0-9-]{3,49}[a-zA-Z0-9]{1,1}$')) { 
    throw 'Invalid EnvironmentName'
}

If ($msBuild -eq $null) {
    $msBuildFallback = 
        "${Env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe",
        "${Env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe",
        "${Env:ProgramFiles(x86)}\MSBuild\10.0\Bin\MSBuild.exe"
    
    $try = -1
    
    do {
        $try += 1
        $msbuild = $msBuildFallback[$try]
    } while (!(Test-Path $msbuild))
}

If (!(Test-Path $msbuild)) {
    Write-Warning "Use -msBuild to specify a custom msbuild path"
    throw "Couldn't find MSBuild at $msbuild"
}

# Build - see :Build in build.cmd
function Build() {
    &$msbuild ("RemoteMonitoring.sln", "/v:m", "/p:Configuration=$configuration", "/t:Clean,Build")
    If ($?) {
        Write-Host "Building RemoteMonitoring.sln succeeded"
    } Else {
        throw "Building RemoteMonitoring.sln failed - See above logs"
    }
}

# Package - see :Package in build.cmd
function Package() {
    &$msbuild ("DeviceAdministration\Web\Web.csproj", "/v:m", "/p:Configuration=$configuration", "/t:Package")
    If (-Not($?)) {
        throw "Packaging DeviceAdministration\Web\Web.csproj failed"
    }
    &$msbuild ("WebJobHost\WebJobHost.csproj", "/v:m", "/p:Configuration=$configuration", "/t:Package")
    If (-Not($?)) {
        throw "Packaging WebJobHost\WebJobHost.csproj failed"
    }
}

# Config - see :Config in build.cmd
function Config() {
    Invoke-Expression ".\Common\Deployment\PrepareIoTSample.ps1 -environmentName $environmentName -configuration $configuration"
}

If ($command -eq 'Build' -or $command -eq 'Cloud') {
    Build
    Package
    Config
}
ElseIf ($command -eq 'Local') {
    Config
}
Else {
    Write-Error "Invalid command $command"
}