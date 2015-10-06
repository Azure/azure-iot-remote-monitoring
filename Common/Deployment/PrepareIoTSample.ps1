Param(
    [Parameter(Mandatory=$True,Position=0)]
    $environmentName,
    [Parameter(Mandatory=$True,Position=1)]
    $buildPath,
    
    #common override
    $slot = "Staging",
    [string]
    $serviceList = "",

    #seldom used
    $deploymentLabel = "",
    [switch]
    $vipSwap = $true,
    $maxTimeoutInMins = 45
    )

# Initialize library
$environmentName = $environmentName.ToLowerInvariant()
. "$(Split-Path $MyInvocation.MyCommand.Path)\DeploymentLib.ps1"

# Sets Azure Accounts, Region, and AAD
InitializeEnvironment $environmentName

# 






# Validate arguments
[string[]]$services = @();
if ([string]::IsNullOrEmpty($serviceList))
{
    $services = GetServices
}
else
{
    $services = $serviceList.Split(',')
}

if ([string]::IsNullOrEmpty($deploymentLabel))
{
    $deploymentLabel = "AutoDeploy $(Get-Date –f $timeStampFormat)"
}
Write-Host "$(Get-Date –f $timeStampFormat) - EnvironmentName: $environmentName"
Write-Host "$(Get-Date –f $timeStampFormat) - BuildPath: $buildPath"
write-host "$(Get-Date –f $timeStampFormat) - services: $services";
write-host "$(Get-Date –f $timeStampFormat) - slot: $slot";
write-host "$(Get-Date –f $timeStampFormat) - Vip Swap: $vipSwap";
write-host "$(Get-Date –f $timeStampFormat) - deploymentLabel: $deploymentLabel";

# DocDB
# For now statically configure and prompt for end points
$null = GetOrSetEnvSetting  "DocDbEndPoint" "Read-Host 'Enter DocDB URI from http://portal.azure.com'"
$null = GetOrSetEnvSetting "DocDBKey" "Read-Host 'Enter DocDB Primary Key'"

# Storage account for EventProcessing and Deployment
$storeAccountName = GetEnvSetting "ServiceStoreAccountName"
if ([string]::IsNullOrEmpty($storeAccountName))
{
    $storeAccountName = "{0}st" -f $environmentName
}
$storeAccountName = ValidateAzureStorageAccount "Service" $storeAccountName
UpdateAzureStorageAccountConnectionString "Service"
Execute-Command -Command ("Set-AzureSubscription -SubscriptionId $global:SubscriptionId -CurrentStorageAccountName $storeAccountName")

# Servicebus account for EventProcessing
$eventProcessingName = "eventprocessing"
$servicebusName = GetEnvSetting "ServiceSBName"
if ([string]::IsNullOrEmpty($servicebusName))
{
    $servicebusName = "{0}sb" -f $environmentName
}
$null = ValidateAzureServicebusNamespace "Service" $serviceBusName $eventProcessingName

# Provision services
foreach ($service in $services)
{
    ValidateService $service $environmentName $true
}

# ResourceManager based provisioning
$SaveVerbosePreference = $global:VerbosePreference;
$global:VerbosePreference = 'SilentlyContinue';
Switch-AzureMode AzureResourceManager
$global:VerbosePreference = $SaveVerbosePreference;
$resourceGroup = $null
foreach ($rg in Get-AzureResourceGroup)
{
    if ($rg.ResourceGroupName -eq ("{0}-rg" -f $environmentName))
    {
        $resourceGroup = $rg.ResourceGroupName
    }
}
if ($resourceGroup -eq $null)
{
    $resourceGroup = (New-AzureResourceGroup -Name ("{0}-rg" -f $environmentName) -Location $global:AllocationRegion).ResourceGroupName
}

# Stream Analytics
# TelemetryToBlobJob.json replacement - if the arguments in Common\Deployment are changed, this must be updated
$serviceBus = New-Object StringParser(GetEnvSetting "EventHubConnectionString")
$storageAccount = New-Object StringParser(GetEnvSetting "ServiceStoreAccountConnectionString")
$jobDetails = ReplaceFileParameters ("{0}\TelemetryToBlobJob.json" -f $global:azurePath) -arguments @($global:AllocationRegion,
    (GetEnvSetting "EventHubConsumerGroup"),
    $global:EventHubName,
    $serviceBus.GetValue("Endpoint").Split('.')[0].Substring(5),
    $serviceBus.GetValue("SharedAccessKey"),
    $serviceBus.GetValue("SharedAccessKeyName"),
    $storageAccount.GetValue("AccountKey"),
    $storageAccount.GetValue("AccountName"))

[string]$jobName = GetOrSetEnvSetting "StreamAnalyticsTelemetry" ("return '{0}-TelemetryToBlob'" -f $environmentName)
ValidateStreamAnalyticsJob $jobName $resourceGroup $jobDetails

# Stream Analytics
# DeviceInfoFilterJob.json replacement - if the arguments in Common\Deployment are changed, this must be updated
$eventProcessor = New-Object StringParser(GetEnvSetting "ServiceSBConnectionString")
$jobDetails = ReplaceFileParameters ("{0}\DeviceInfoFilterJob.json" -f $global:azurePath) -arguments @($global:AllocationRegion,
    (GetEnvSetting "EventHubConsumerGroup"),
    $global:EventHubName,
    $serviceBus.GetValue("Endpoint").Split('.')[0].Substring(5),
    $serviceBus.GetValue("SharedAccessKey"),
    $serviceBus.GetValue("SharedAccessKeyName"),
    $eventProcessingName,
    $eventProcessor.GetValue("Endpoint").Split('.')[0].Substring(5),
    $eventProcessor.GetValue("SharedAccessKey"),
    $eventProcessor.GetValue("SharedAccessKeyName"))

[string]$jobName = GetOrSetEnvSetting "StreamAnalyticsDeviceInfo" ("return '{0}-DeviceInfoFilterJob'" -f $environmentName)
ValidateStreamAnalyticsJob $jobName $resourceGroup $jobDetails

# Populate dev isolation mode parameters
$null = GetOrSetEnvSetting "DeviceTableName" "`"DeviceList`""
$null = GetOrSetEnvSetting "ObjectTypePrefix" "[string]::Empty"

# Restore Azure Cmdlets to AzureServiceManagement
$SaveVerbosePreference = $global:VerbosePreference;
$global:VerbosePreference = 'SilentlyContinue';
Switch-AzureMode AzureServiceManagement
$global:VerbosePreference = $SaveVerbosePreference;

# This must be the last step before publishing services or the
# config files won't be updated with environment settings
foreach ($service in $services)
{
    #UpdateServiceConfig $service $buildPath
}

# Publish
if ($environmentName -eq "Local")
{
    return
}

Write-Host "$(Get-Date –f $timeStampFormat) - Publishing services in parallel: $services"
foreach ($serviceBaseName in $services)
{
    $serviceName = GetEnvSetting "$($serviceBaseName)$($serviceNameToken)"
    # Try to get service
    $service = Execute-Command -Command ("Get-AzureService -ServiceName $serviceName")
    if ($service -eq $null)
    {
        Write-Error -Category ObjectNotFound -Message "$(Get-Date –f $timeStampFormat) - $serviceBaseName - Error, cannot retrieve service named '$serviceName'"
        exit 1
    }

    # Deploy service
    Write-Host "$(Get-Date –f $timeStampFormat) - Start-Job -Name $serviceBaseName -ScriptBlock `"$global:azurePath\PublishIotSample.ps1 `"$buildPath`" $serviceBaseName $slot `"$deploymentLabel`" $environmentName $([int]$vipSwap.ToBool())`""
    Start-Job -Name $serviceBaseName -ScriptBlock ([ScriptBlock]::Create("$global:azurePath\PublishIotSample.ps1 `"$buildPath`" $serviceBaseName $slot `"$deploymentLabel`" $environmentName $([int]$vipSwap.ToBool())")) | Out-Null
    Start-Sleep 2
}

$jobStartTime = Get-Date 
$jobTimeoutTime = $jobStartTime.addMinutes($maxTimeoutInMins)
$timeBeforeTimeout = New-TimeSpan $jobStartTime $jobTimeoutTime

# Loop until there are no jobs in the running state
While (($(Get-Job -State Running | Measure-Object).Count -gt 0))
{
    $timeBeforeTimeout = new-timespan $(get-date) $jobTimeoutTime
       
    if($timeBeforeTimeout -lt 0)
    {
        Write-Error "$(Get-Date –f $timeStampFormat) - Service deployment has exceeded the timeout period of $maxTimeoutInSecs minutes. Abandoning Deployment."
        exit 1
    }
   
    try{
        Get-Job | Receive-Job
        Start-Sleep 5
    }
    catch [Exception]
    {
        Write-Error $_.Exception.Message
    }
}

# Get any lingering log output
Get-Job | Receive-Job

Write-Host "$(Get-Date –f $timeStampFormat) - Azure Cloud Service deploy script finished."
exit 0