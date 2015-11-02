<#
    .SYNOPSIS 
    A wrapper script to build and deploy the RemoteMonitoring solution 
            
    .DESCRIPTION
	Builds the RemoteMonitoring solution using the specified settings file (Local or Cloud).

    Requires that Windows Azure PowerShell
    be installed and configured to work with your Windows Azure
    subscription. For details, see "How to install and configure 
    Windows Azure PowerShell" at 

    http://go.microsoft.com/fwlink/?LinkID=320552.

	After building, the script calls PrepareIoTSample.ps1 to do the prep and deployment.

    .PARAMETER  Target
    The deployment target: Local, Cloud

    .PARAMETER  Configuration
    The build configuration: Debug, Release

    .PARAMETER  EnvironmentName
	For cloud deployment, the name of the azure resource group to deploy

    .PARAMETER  Clean
    "Clean" switch indicating to clean before build/config - default is not to clean

    .INPUTS
    System.String

    .OUTPUTS
    None. This script does not return any objects.

    .NOTES
    This script automatically sets the $VerbosePreference to Continue, 
    so all verbose messages are displayed, and the $ErrorActionPreference
    to Stop so that non-terminating errors stop the script.

    .EXAMPLE
    build -Target Local
    Builds and deploys locally

    .EXAMPLE
    build -Target Local -Configuration Release -Clean
    Local release, clean deployment

    .EXAMPLE
    build -Target Cloud -Configuration release -EnvirontmentName mydeployment -Publish
    Cloud deployment

    .LINK
    DeploymentLib.ps1

    .LINK
    PrepareIoTSample.ps1
    
    .LINK
    PublishIoTSample.ps1

    .LINK
    Windows Azure Management Cmdlets (http://go.microsoft.com/fwlink/?LinkID=386337)

    .LINK
    How to install and configure Windows Azure PowerShell (http://go.microsoft.com/fwlink/?LinkID=320552)
#>
Param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Cloud", "Local", IgnoreCase = $false)]
    [String]
    $Target,

    [ValidateSet("debug", "release")]
    [String]
    $Configuration = "debug",

    [Switch]
    $Publish = $false,

    [String]
    $EnvironmentName,

    [Switch]
    $Clean = $false
)
##############################################################################
# Init libraries
##############################################################################
Import-Module "$(Split-Path $MyInvocation.MyCommand.Path)\Invoke-MsBuild.psm1"

##############################################################################
# Globals / Logging
##############################################################################

# Set the output level to verbose and make the script stop on error
$VerbosePreference = "Continue"
$ErrorActionPreference = "Stop"

function Get-MissingFiles ($required)
{
    $Path = Split-Path $MyInvocation.PSCommandPath
    $files = dir $Path | foreach {$_.Name}

    foreach ($r in $required)
    {            
        if ($r -notin $files)
        {
            [PSCustomObject]@{"Name"=$r; "Error"="Missing"}
        }
    }
}

function Build()
{
    Param([string]$path,
        [string] $params)

    Write-Verbose ("Starting build...")
    Write-Verbose("msbuild {0} {1}" -f $path, $params)

    $result = Invoke-MsBuild -Path $path  -Params $params
    if (-Not $result)
    {
        $log = Invoke-MsBuild -Path $path  -Params $params -GetLogPath
        throw "MSBuild failed.  Additional detail in the log: $log"
    }
}

##############################################################################
# Begin Script
##############################################################################

try
{
    $scriptPath = Split-Path -parent $PSCommandPath
    $solutionPath = (get-item $scriptPath).Parent.Parent.FullName
    $buildOutput = Join-Path $solutionPath "Build_Output\"
    $buildPath = Join-Path $buildOutput $Configuration

    Write-Verbose ("Starting log {0}" -f $logFile)

    # Get the time that script execution starts
    $startTime = Get-Date

    # Verify dependencies
    Write-Verbose "Checking for required files."
    $missingFiles = Get-MissingFiles(("Application.json", "ConfigurationTemplate.config", 
        "DeploymentLib.ps1", "LocalMonitoring.json", "PrepareIoTSample.ps1", "RemoteMonitoring.json"))
    if ($missingFiles) {$missingFiles; throw "Required files missing from WebSite subdirectory. Download and upzip the package and try again."}

    # Validate params
    if($Target -eq "Cloud")
    {
        if(-not ($EnvironmentName.Trim()))
        {
            throw "EnvironmentName must be specified for Cloud deployments"
        }
   
       if ($EnvironmentName -notmatch '^(?![0-9]+$)(?!-)[a-zA-Z0-9-]{3,49}[a-zA-Z0-9]{1,1}$')
       {
         Throw "EnvironmentName - $EnvironmentName must start with a letter, end with a letter or number, between 3-50 characters in length, and only contain letters, numbers and dashes"
       }
    }
    else
    {
        $EnvironmentName = "Local"
    }

    # Clean
    Write-Verbose ("Removing {0}" -f $buildOutput)
    if($Clean.IsPresent -and (Test-Path $buildOutput))
    {
        try
        {
            [System.IO.Directory]::Delete($buildOutput, 'true')
        }
        catch [System.UnauthorizedAccessException]
        {
            Write-Verbose "UnauthorizedAccessException performing delete.  Debug process may be in use...continuting..."
        }
    }

    # MSBuild - remotemonitoring.sln
    $path = Join-Path $solutionPath "RemoteMonitoring.sln"
    $config = "/p:Configuration={0}" -f $Configuration
    Build -path $path  -params ("/v:m {0}" -f $config)

    # MSBuild - web.csproj
    $path = Join-Path $solutionPath "DeviceAdministration\Web\Web.csproj" 
    Build -path $path  -params "/v:m /t:Package"

    # MSBuild - WebJobHost.csproj
    $path = Join-Path $solutionPath "WebJobHost\WebJobHost.csproj"
    Build -path $path -params  "/v:m /T:Package"

    # Call the prepare script
    if($Publish.IsPresent)
    {
        Write-Verbose "Publishing...Calling PrepareIoTSample.ps1..."
        ./PrepareIoTSample.ps1 -environmentName $EnvironmentName -Configuration $Configuration 
        if (-not ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq $null)) 
        {
            throw "Publish failed."
        }
    }

    Write-Verbose "Script is complete."

    # Mark the finish time of the script execution
    $finishTime = Get-Date
    # Output the time consumed in seconds
    $TotalTime = ($finishTime - $startTime).TotalSeconds
    Write-Output "Total time used (seconds): $TotalTime"
}
catch
{
    $host.ui.WriteErrorLine("`n" + $_ + "`n")
}


