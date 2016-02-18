Param(
    [Parameter(Mandatory=$True,Position=0)]
    $environmentName,
    [Parameter(Mandatory=$True,Position=1)]
    $configuration
    )
    
# Initialize library
$environmentName = $environmentName.ToLowerInvariant()
. "$(Split-Path $MyInvocation.MyCommand.Path)\DeploymentLib.ps1"
ClearDNSCache

# Sets Azure Accounts, Region, Name validation, and AAD application
InitializeEnvironment $environmentName

# Set environment specific variables 
$suitename = "LocalRM"
$suiteType = "LocalMonitoring"
$deploymentTemplatePath = "$(Split-Path $MyInvocation.MyCommand.Path)\LocalMonitoring.json"
$global:site = "https://localhost:44305/"
$global:appName = "iotsuite"
$cloudDeploy = $false

if ($environmentName -ne "local")
{
    $suiteName = $environmentName
    $suiteType = "RemoteMonitoring"
    $deploymentTemplatePath = "$(Split-Path $MyInvocation.MyCommand.Path)\RemoteMonitoring.json"
    $global:site = "https://{0}.azurewebsites.net/" -f $environmentName
    $cloudDeploy = $true
}
else
{
    $legacyNameExists = (Find-AzureRmResourceGroup -Tag @{Name="IotSuiteType";Value=$suiteType} | ?{$_.ResourceGroupName -eq "IotSuiteLocal"}) -ne $null
    if ($legacyNameExists)
    {
        $suiteName = "IotSuiteLocal"
    }
}

$suiteExists = (Find-AzureRmResourceGroup -Tag @{Name="IotSuiteType";Value=$suiteType} | ?{$_.ResourceGroupName -eq $suiteName}) -ne $null
$resourceGroupName = (GetResourceGroup -Name $suiteName -Type $suiteType).ResourceGroupName
$storageAccount = GetAzureStorageAccount $suiteName $resourceGroupName $cloudDeploy
$iotHubName = GetAzureIotHubName $suitename $resourceGroupName $cloudDeploy
$sevicebusName = GetAzureServicebusName $suitename $resourceGroupName $cloudDeploy
$docDbName = GetAzureDocumentDbName $suitename $resourceGroupName $cloudDeploy

# Setup AAD for webservice
UpdateResourceGroupState $resourceGroupName ProvisionAAD
$global:AADTenant = GetOrSetEnvSetting "AADTenant" "GetAADTenant"
UpdateEnvSetting "AADMetadataAddress" ("https://login.windows.net/{0}/FederationMetadata/2007-06/FederationMetadata.xml" -f $global:AADTenant)
UpdateEnvSetting "AADAudience" ($global:site + $global:appName)
UpdateEnvSetting "AADRealm" ($global:site + $global:appName)

# Deploy via Template
UpdateResourceGroupState $resourceGroupName ProvisionAzure
$params = @{ `
    suiteName=$suitename; `
    docDBName=$docDbName; `
    storageName=$($storageAccount.StorageAccountName); `
    iotHubName=$iotHubName; `
    sbName=$sevicebusName; `
    aadTenant=$($global:AADTenant)}

Write-Host "Suite name: $suitename"
Write-Host "DocDb Name: $docDbName"
Write-Host "Storage Name: $($storageAccount.StorageAccountName)"
Write-Host "IotHub Name: $iotHubName"
Write-Host "Servicebus Name: $sevicebusName"
Write-Host "AAD Tenant: $($global:AADTenant)"
Write-Host "ResourceGroup Name: $resourceGroupName"
Write-Host "Deployment template path: $deploymentTemplatePath"

# Respect existing Sku values
if ($suiteExists)
{
    if (ResourceObjectExists $suitename $docDbName Microsoft.DocumentDb/databaseAccounts)
    {
        $docDbSku = GetResourceObject $suitename $docDbName Microsoft.DocumentDb/databaseAccounts
        $params += @{docDBSku=$($docDbSku.Properties.DatabaseAccountOfferType)}
    }
    if (ResourceObjectExists $suitename $storageAccount.StorageAccountName Microsoft.Storage/storageAccounts)
    {
        $storageSku = GetResourceObject $suitename $storageAccount.StorageAccountName Microsoft.Storage/storageAccounts
        $params += @{storageAccountSku=$($storageSku.Properties.AccountType)}
    }
    if (ResourceObjectExists $suitename $iotHubName Microsoft.Devices/IotHubs)
    {
        $iotHubSku = GetResourceObject $suitename $iotHubName Microsoft.Devices/IotHubs
        $params += @{iotHubSku=$($iotHubSku.Sku.Name)}
        $params += @{iotHubTier=$($iotHubSku.Sku.Tier)}
    }
    if (ResourceObjectExists $suitename $sevicebusName Microsoft.Eventhub/namespaces)
    {
        $servicebusSku = GetResourceObject $suitename $sevicebusName Microsoft.Eventhub/namespaces
        $params += @{sbSku=$($servicebusSku.Properties.MessagingSku)}
    }
}

# Upload WebPackages
if ($cloudDeploy)
{
    $projectRoot = Join-Path $PSScriptRoot "..\.." -Resolve
    $webPackage = UploadFile ("$projectRoot\DeviceAdministration\Web\obj\{0}\Package\Web.zip" -f $configuration) $storageAccount.StorageAccountName $resourceGroupName "WebDeploy" -secure $true
    $params += @{packageUri=$webPackage}
    FixWebJobZip ("$projectRoot\WebJobHost\obj\{0}\Package\WebJobHost.zip" -f $configuration)
    $webJobPackage = UploadFile ("$projectRoot\WebJobHost\obj\{0}\Package\WebJobHost.zip" -f $configuration) $storageAccount.StorageAccountName $resourceGroupName "WebDeploy" -secure $true
    $params += @{webJobPackageUri=$webJobPackage}
    # Respect existing Sku values
    if ($suiteExists)
    {
        $webSku = GetResourceObject $suitename $suitename Microsoft.Web/sites
        $params += @{webSku=$($webSku.Properties.Sku)}
        $webPlan = GetResourceObject $suiteName ("{0}-plan" -f $suiteName) Microsoft.Web/serverfarms
        $params += @{webWorkerSize=$($webPlan.Properties.WorkerSize)}
        $params += @{webWorkerCount=$($webPlan.Properties.NumberOfWorkers)}
        $jobName = "{0}-jobhost" -f $suitename
        if (ResourceObjectExists $suitename $jobName Microsoft.Web/sites)
        {
            $webJobSku = GetResourceObject $suitename $jobName Microsoft.Web/sites
            $params += @{webJobSku=$($webJobSku.Properties.Sku)}
            $webJobPlan = GetResourceObject $suiteName ("{0}-jobsplan" -f $suiteName) Microsoft.Web/serverfarms
            $params += @{webJobWorkerSize=$($webJobPlan.Properties.WorkerSize)}
            $params += @{webJobWorkerCount=$($webJobPlan.Properties.NumberOfWorkers)}
        }
    }
}

# Stream analytics does not auto stop, and if already exists should be set to LastOutputEventTime to not lose data
if (StopExistingStreamAnalyticsJobs $resourceGroupName)
{
    $params += @{asaStartBehavior='LastOutputEventTime'}
}

Write-Host "Provisioning resources, if this is the first time, this operation can take up 10 minutes..."
$result = New-AzureRmResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile $deploymentTemplatePath -TemplateParameterObject $params -Verbose

if ($result.ProvisioningState -ne "Succeeded")
{
    UpdateResourceGroupState $resourceGroupName Failed
    throw "Provisioning failed"
}

# Set Config file variables
UpdateResourceGroupState $resourceGroupName Complete
UpdateEnvSetting "ServiceStoreAccountName" $storageAccount.StorageAccountName
UpdateEnvSetting "ServiceStoreAccountConnectionString" $result.Outputs['storageConnectionString'].Value
UpdateEnvSetting "ServiceSBName" $sevicebusName
UpdateEnvSetting "ServiceSBConnectionString" $result.Outputs['ehConnectionString'].Value
UpdateEnvSetting "ServiceEHName" $result.Outputs['ehOutName'].Value
UpdateEnvSetting "IotHubName" $result.Outputs['iotHubHostName'].Value
UpdateEnvSetting "IotHubConnectionString" $result.Outputs['iotHubConnectionString'].Value
UpdateEnvSetting "DocDbEndPoint" $result.Outputs['docDbURI'].Value
UpdateEnvSetting "DocDBKey" $result.Outputs['docDbKey'].Value
UpdateEnvSetting "DeviceTableName" "DeviceList"
UpdateEnvSetting "RulesEventHubName" $result.Outputs['ehRuleName'].Value
UpdateEnvSetting "RulesEventHubConnectionString" $result.Outputs['ehConnectionString'].Value
if ($result.Outputs['bingMapsQueryKey'].Value.Length -gt 0)
{
    UpdateEnvSetting "MapApiQueryKey" $result.Outputs['bingMapsQueryKey'].Value
}

Write-Host ("Provisioning and deployment completed successfully, see {0}.config.user for deployment values" -f $environmentName)

if ($environmentName -ne "local")
{
    $maxSleep = 40
    $webEndpoint = "{0}.azurewebsites.net" -f $environmentName
    if (!(HostEntryExists $webEndpoint))
    {
        Write-Host "Waiting for website url to resolve." -NoNewline
        while (!(HostEntryExists $webEndpoint))
        {
            Write-Host "." -NoNewline
            Clear-DnsClientCache
            if ($maxSleep-- -le 0)
            {
                Write-Host
                Write-Warning ("website unable to resolve {0}, please wait and try again in 15 minutes" -f $global:site)
                break
            }
            sleep 3
        }
        Write-Host
    }
    if (HostEntryExists $webEndpoint)
    {
        start $global:site
    }
}