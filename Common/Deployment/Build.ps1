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

    .PARAMETER  Services
    Comma separated string of services to deploy, eg. EventProcessor,VendingMachines - default deploys all services

	.PARAMETER  DeploymentLabel
    A label used to describe the deployment - default is timestamped string

	.PARAMETER  Slot
    Either production or staging slot - default is staging

	.PARAMETER  VipSwap
    Indicates if VIP swap (swap staging and production) should occur after successful deployment - default true

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
    build -Target Local -Configuration release -Clean
    Local release, clean deployment

    .EXAMPLE
    build -Target Cloud -Configuration release -EnvirontmentName mydeployment -Publish
    Cloud deployment

    .EXAMPLE
    build -Target Cloud -Configuration release -EnvirontmentName mydeployment -Services EventProcessor -Publish
    Cloud deployment with args

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

    [String]
    $Services = "",

    [String]
    $DeploymentLabel = "",

    [ValidateSet("Production", "Staging")]
    [String]
    $Slot= "Staging",

    [Switch]
    $VipSwap = $true,

    [Switch]
    $Clean = $false
)

# Set the output level to verbose and make the script stop on error
$VerbosePreference = "Continue"
$ErrorActionPreference = "Stop"
$logFolder = "D:\temp\remoteMonitoring"
$logFile = Join-Path $logFolder ("\Build-{0:yyyyMMdd}.log" -f (Get-Date))
if(![System.IO.Directory]::Exists($logFolder))
{
    [System.IO.Directory]::CreateDirectory($logFolder)
}

Function LogWrite
{
   Param ([string]$logstring)

   Add-content $logFile -value $logstring
}

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
        [string[]]$params)

    $msbuild = "$env:windir\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe";
    $buildargs =  @($path) + $params

    Write-Verbose ("Starting build...")
    Write-Verbose($msbuild + $buildargs)
    LogWrite($msbuild + $buildargs)

    $result = &  $msbuild $buildargs
    LogWrite ($result -join "`n")
    if ($LASTEXITCODE -ne 0) 
    {
        throw "MSBuild failed.  Additional detail in the log."
    }
}

try
{
    $scriptPath = Split-Path -parent $PSCommandPath
    $solutionPath = (get-item $scriptPath).Parent.Parent.FullName
    $buildOutput = Join-Path $solutionPath "Build_Output\"
    $buildPath = Join-Path $buildOutput $Configuration

    Write-Verbose ("Starting log {0}" -f $logFile)
    LogWrite "================================================================================"
    LogWrite ("Starting build at {0}" -f  (Get-Date))
    LogWrite "================================================================================"

    # Get the time that script execution starts
    $startTime = Get-Date

    # Verify dependencies
    Write-Verbose "Checking for required files."
    $missingFiles = Get-MissingFiles(("Application.json", "AzureDeployment.json", "ConfigurationTemplate.config", 
        "DeploymentLib.ps1", "DeviceInfoFilterJob.json", "PrepareIoTSample.ps1", "PublishIoTSample.ps1", "RemoteMonitoring.json", "TelemetryToBlobJob.json"))
    if ($missingFiles) {$missingFiles; throw "Required files missing from WebSite subdirectory. Download and upzip the package and try again."}

    # Running Get-AzureWebsite only to verify that Azure credentials in the PS session have not expired (expire 12 hours)
    # If the credentials are expired, cmdlet throws a terminating error that stops the script.
    Write-Verbose "Verifying that Windows Azure credentials in the Windows PowerShell session have not expired."
    Get-AzureWebsite | Out-Null

    # Validate params
    if($Target -eq "Cloud")
    {
        if(-not ($EnvironmentName.Trim()))
        {
            throw "EnvironmentName must be specified for Cloud deployments"
        }
    }
    else
    {
        $EnvironmentName = "Local"
    }

    # Clean
    if($Clean.IsPresent -and (Test-Path $buildOutput))
    {
        [System.IO.Directory]::Delete($buildOutput, 'true')
    }

    # MSBuild - remotemonitoring.sln
    $path = Join-Path $solutionPath "RemoteMonitoring.sln"
    $config = "/p:Configuration={0}" -f $Configuration
    Build -path $path  -params @("/v:m", $config)
    # MSBuild - web.csproj
    $path = Join-Path $solutionPath "DeviceAdministration\Web\Web.csproj" 
    $outPath = "/p:OutputPath={0}" -f $buildOutput
    Build -path $path  -params @("/v:m", "/t:Package", "/P:VisualStudioVersion=14.0", $outPath)

    # Call the prepare script
    if($Publish.IsPresent)
    {
        Write-Verbose "Publishing...Calling PrepareIoTSample.ps1..."
        ./PrepareIoTSample.ps1 -environmentName $EnvironmentName -buildPath $buildPath `
            -slot $Slot -serviceList $Services -deploymentLabel $DeploymentLabel -vipSwap:$VipSwap.IsPresent
        if ($LASTEXITCODE -ne 0) 
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



