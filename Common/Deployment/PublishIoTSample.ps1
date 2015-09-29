Param(
	[Parameter(Mandatory=$True,Position=0)]
	$buildPath,
	[Parameter(Mandatory=$True,Position=1)]
	$serviceFriendlyName,
	[Parameter(Mandatory=$True,Position=2)]
	$slot,
	[Parameter(Mandatory=$True,Position=3)]
	$deploymentLabel,
	[Parameter(Mandatory=$True,Position=4)]
	$environmentName,
	[Parameter(Mandatory=$True,Position=5)]
	$vipSwap,
	$timeoutPerRetryInMins = 20,
	$maxRetries = 2
)
$environmentName = $environmentName.ToLowerInvariant()
. "$(Split-Path $MyInvocation.MyCommand.Path)\DeploymentLib.ps1"
InitializeEnvironment $environmentName

$stageStartTime = Get-Date 
$stageTimeoutTime = $stageStartTime.addMinutes($timeoutPerRetryInMins)
$timeBeforeTimeout = New-TimeSpan $stageStartTime $stageTimeoutTime

$serviceName = GetEnvSetting "$($serviceFriendlyName)ServiceName"
$configFile = "{0}\Services\{1}\pub\ServiceConfiguration.Cloud.cscfg" -f $buildPath, $serviceFriendlyName
$packageFile = "{0}\Services\{1}\pub\{1}.cspkg" -f $buildPath, $serviceFriendlyName

Write-Host "$(Get-Date –f $timeStampFormat) - $serviceFriendlyName - Service name for ${serviceFriendlyName}: $serviceName."
Write-Host "$(Get-Date –f $timeStampFormat) - $serviceFriendlyName - Looking for $serviceName."

# Main driver - publish & write progress to activity log
Write-Host "$(Get-Date –f $timeStampFormat) - $serviceFriendlyName - Publishing deployment labelled $deploymentLabel of $serviceFriendlyName to subscription $subscriptionName."
Publish -serviceName $serviceName -slot $slot -deploymentLabel "$deploymentLabel" -packageLocation $packageFile -cloudConfigLocation $configFile

$deployment = Get-AzureDeployment -slot $slot -serviceName $serviceName
if ($vipSwap)
{
	if ($deployment -ne $null -AND $deployment.DeploymentId -ne $null) 
	{ 
		$moveStatus = Move-AzureDeployment -ServiceName $serviceName 
		Write-Host "$(Get-Date –f $timeStampFormat) - $serviceFriendlyName - Vip swap of $serviceName status: $($moveStatus.OperationStatus)"
		$vipSlot = "Production"
		if ($slot -eq $vipSlot)
		{
			$vipSlot = "Staging"
		}
		$deployment = Get-AzureDeployment -slot $vipSlot -serviceName $serviceName
	}
	else 
	{ 
		Write-Output "$(Get-Date –f $timeStampFormat) - $serviceFriendlyName - There is no deployment in $slot slot of $serviceName to swap."
		exit 1
	}
}
$deploymentUrl = $deployment.Url
Write-Host "$(Get-Date –f $timeStampFormat) - $serviceFriendlyName - Created Cloud Service for $serviceName with URL $deploymentUrl."

