Param(
    [Parameter(Mandatory=$True,Position=0)]
    $environmentName,
    [Parameter(Mandatory=$True,Position=1)]
    $configuration,
    [Parameter(Mandatory=$False,Position=2)]
    $azureEnvironmentName = "AzureCloud"
    )

# Initialize Azure Cloud Environment
switch($azureEnvironmentName)
{
    "AzureCloud" {
        if ((Get-AzureEnvironment AzureCloud) -eq $null)
        {
            Add-AzureEnvironment –Name AzureCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.windows.net/ -GalleryUrl https://gallery.azure.com/ -ServiceManagementUrl https://management.core.windows.net/ -SqlDatabaseDnsSuffix .database.windows.net -StorageEndpointSuffix core.windows.net -ActiveDirectoryAuthority https://login.microsoftonline.com/ -GraphUrl https://graph.windows.net/ -trafficManagerDnsSuffix trafficmanager.net -AzureKeyVaultDnsSuffix vault.azure.net -AzureKeyVaultServiceEndpointResourceId https://vault.azure.net -ResourceManagerUrl https://management.azure.com/ -ManagementPortalUrl http://go.microsoft.com/fwlink/?LinkId=254433
        }

        if ((Get-AzureRMEnvironment AzureCloud) -eq $null)
        {
            Add-AzureRMEnvironment –Name AzureCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.windows.net/ -GalleryUrl https://gallery.azure.com/ -ServiceManagementUrl https://management.core.windows.net/ -SqlDatabaseDnsSuffix .database.windows.net -StorageEndpointSuffix core.windows.net -ActiveDirectoryAuthority https://login.microsoftonline.com/ -GraphUrl https://graph.windows.net/ -trafficManagerDnsSuffix trafficmanager.net -AzureKeyVaultDnsSuffix vault.azure.net -AzureKeyVaultServiceEndpointResourceId https://vault.azure.net -ResourceManagerUrl https://management.azure.com/ -ManagementPortalUrl http://go.microsoft.com/fwlink/?LinkId=254433
        }

        $global:iotHubSuffix = "azure-devices.net"
        $global:docdbSuffix = "documents.azure.com"
        $global:eventhubSuffix = "servicebus.windows.net"
        $global:websiteSuffix = "azurewebsites.net"
        $global:locations = @("East US", "North Europe", "East Asia", "West US", "West Europe", "Southeast Asia", "Japan East", "Japan West", "Australia East", "Australia Southeast")
    }
    "AzureGermanCloud" {
        if ((Get-AzureEnvironment AzureGermanCloud) -eq $null)
        {
            Add-AzureEnvironment –Name AzureGermanCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.cloudapi.de/ -GalleryUrl https://gallery.cloudapi.de -ServiceManagementUrl https://management.core.cloudapi.de/ -SqlDatabaseDnsSuffix .database.cloudapi.de -StorageEndpointSuffix core.cloudapi.de -ActiveDirectoryAuthority https://login.microsoftonline.de/ -GraphUrl https://graph.cloudapi.de/ -trafficManagerDnsSuffix azuretrafficmanager.de -AzureKeyVaultDnsSuffix vault.microsoftazure.de -AzureKeyVaultServiceEndpointResourceId https://vault.microsoftazure.de -ResourceManagerUrl https://management.microsoftazure.de/ -ManagementPortalUrl https://portal.microsoftazure.de
        }

        if ((Get-AzureRMEnvironment AzureGermanCloud) -eq $null)
        {
            Add-AzureRMEnvironment –Name AzureGermanCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.cloudapi.de/ -GalleryUrl https://gallery.cloudapi.de -ServiceManagementUrl https://management.core.cloudapi.de/ -SqlDatabaseDnsSuffix .database.cloudapi.de -StorageEndpointSuffix core.cloudapi.de -ActiveDirectoryAuthority https://login.microsoftonline.de/ -GraphUrl https://graph.cloudapi.de/ -trafficManagerDnsSuffix azuretrafficmanager.de -AzureKeyVaultDnsSuffix vault.microsoftazure.de -AzureKeyVaultServiceEndpointResourceId https://vault.microsoftazure.de -ResourceManagerUrl https://management.microsoftazure.de/ -ManagementPortalUrl https://portal.microsoftazure.de
        }

        $global:iotHubSuffix = "azure-devices.de"
        $global:docdbSuffix = "documents.microsoftazure.de"
        $global:eventhubSuffix = "servicebus.cloudapi.de​"
        $global:websiteSuffix = "azurewebsites.de"
        $global:locations = @("Germany Central", "Germany Northeast")
    }
	"AzureChinaCloud" {
       if ((Get-AzureEnvironment AzureChinaCloud) -eq $null)
       {
           Add-AzureEnvironment –Name AzureChinaCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.chinacloudapi.cn/ -GalleryUrl https://gallery.chinacloudapi.cn -ServiceManagementUrl https://management.core.chinacloudapi.cn/ -SqlDatabaseDnsSuffix .database.chinacloudapi.cn -StorageEndpointSuffix core.chinacloudapi.cn -ActiveDirectoryAuthority https://login.microsoftonline.cn/ -GraphUrl https://graph.chinacloudapi.cn/ -trafficManagerDnsSuffix azuretrafficmanager.cn -AzureKeyVaultDnsSuffix vault.azure.cn -AzureKeyVaultServiceEndpointResourceId https://vault.azure.cn -ResourceManagerUrl https://management.chinacloudapi.cn/ -ManagementPortalUrl http://go.microsoft.com/fwlink/?LinkId=301902
       }

       if ((Get-AzureRMEnvironment AzureChinaCloud) -eq $null)
       {
           Add-AzureRMEnvironment –Name AzureChinaCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.chinacloudapi.cn/ -GalleryUrl https://gallery.chinacloudapi.cn -ServiceManagementUrl https://management.core.chinacloudapi.cn/ -SqlDatabaseDnsSuffix .database.chinacloudapi.cn -StorageEndpointSuffix core.chinacloudapi.cn -ActiveDirectoryAuthority https://login.microsoftonline.cn/ -GraphUrl https://graph.chinacloudapi.cn/ -trafficManagerDnsSuffix azuretrafficmanager.cn -AzureKeyVaultDnsSuffix vault.azure.cn -AzureKeyVaultServiceEndpointResourceId https://vault.azure.cn -ResourceManagerUrl https://management.chinacloudapi.cn/ -ManagementPortalUrl http://go.microsoft.com/fwlink/?LinkId=301902
       }

       $global:iotHubSuffix = "azure-devices.cn"
       $global:docdbSuffix = "documents.azure.cn"
       $global:eventhubSuffix = "servicebus.chinacloudapi.cn"
       $global:websiteSuffix = "chinacloudsites.cn"
       $global:locations = @("China North", "China East")
	}
    default {throw ("'{0}' is not a supported Azure Cloud environment" -f $azureEnvironmentName)}
}
$global:azureEnvironment = Get-AzureEnvironment $azureEnvironmentName

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
    $global:site = "https://{0}.{1}/" -f $environmentName, $global:websiteSuffix
    $cloudDeploy = $true
}
else
{
    $legacyNameExists = (Find-AzureRmResourceGroup -Tag @{"IotSuiteType" = $suiteType} | ?{$_.ResourceGroupName -eq "IotSuiteLocal"}) -ne $null
    if ($legacyNameExists)
    {
        $suiteName = "IotSuiteLocal"
    }
}

$suiteExists = (Find-AzureRmResourceGroup -Tag @{"IotSuiteType" = $suiteType} | ?{$_.name -eq $suiteName -or $_.ResourceGroupName -eq $suiteName}) -ne $null
$resourceGroupName = (GetResourceGroup -Name $suiteName -Type $suiteType).ResourceGroupName
$storageAccount = GetAzureStorageAccount $suiteName $resourceGroupName $cloudDeploy
$iotHubName = GetAzureIotHubName $suitename $resourceGroupName $cloudDeploy
$eventhubName = GetAzureEventhubName $suitename $resourceGroupName $cloudDeploy
$docDbName = GetAzureDocumentDbName $suitename $resourceGroupName $cloudDeploy

# Setup AAD for webservice
UpdateResourceGroupState $resourceGroupName ProvisionAAD
$global:AADTenant = GetOrSetEnvSetting "AADTenant" "GetAADTenant"
$global:AADClientId = GetEnvSetting "AADClientId"
UpdateEnvSetting "AADInstance" ($global:azureEnvironment.ActiveDirectoryAuthority + "{0}")

# Deploy via Template
UpdateResourceGroupState $resourceGroupName ProvisionAzure
$params = @{ `
    suiteName=$suitename; `
    docDBName=$docDbName; `
    storageName=$($storageAccount.StorageAccountName); `
    iotHubName=$iotHubName; `
    ehName=$eventhubName; `
    storageEndpointSuffix=$($global:azureEnvironment.StorageEndpointSuffix)}

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
        $params += @{storageAccountSku=$($storageSku.Sku.Name)}
    }
    if (ResourceObjectExists $suitename $iotHubName Microsoft.Devices/IotHubs)
    {
        $iotHubSku = GetResourceObject $suitename $iotHubName Microsoft.Devices/IotHubs
        $params += @{iotHubSku=$($iotHubSku.Sku.Name)}
        $params += @{iotHubTier=$($iotHubSku.Sku.Tier)}
    }
    if (ResourceObjectExists $suitename $eventhubName Microsoft.Eventhub/namespaces)
    {
        $eventhubSku = GetResourceObject $suitename $eventhubName Microsoft.Eventhub/namespaces
        $params += @{ehSku=$($eventhubSku.Sku.name)}
    }
}

# Upload WebPackages
if ($cloudDeploy)
{
    $projectRoot = Join-Path $PSScriptRoot "..\.." -Resolve
    $webPackage = UploadFile ("$projectRoot\DeviceAdministration\Web\obj\{0}\Package\Web.zip" -f $configuration) $storageAccount.StorageAccountName $resourceGroupName "WebDeploy" -secure $true
    FixWebJobZip ("$projectRoot\WebJobHost\obj\{0}\Package\WebJobHost.zip" -f $configuration)
    $webJobPackage = UploadFile ("$projectRoot\WebJobHost\obj\{0}\Package\WebJobHost.zip" -f $configuration) $storageAccount.StorageAccountName $resourceGroupName "WebDeploy" -secure $true
    $params += @{ `
        packageUri=$webPackage; `
        webJobPackageUri=$webJobPackage; `
        aadTenant=$($global:AADTenant); `
        aadInstance=$($global:azureEnvironment.ActiveDirectoryAuthority + "{0}"); `
        aadClientId=$($global:AADClientId)}

    # Respect existing Sku values for cloud resources
    if ($suiteExists)
    {
        $webSku = GetResourceObject $suitename $suitename Microsoft.Web/sites
        $params += @{webSku=$($webSku.Properties.Sku)}
        $webPlan = GetResourceObject $suiteName ("{0}-plan" -f $suiteName) Microsoft.Web/serverfarms
        $params += @{webWorkerSize=$($webPlan.Properties.WorkerSizeId)}
        $params += @{webWorkerCount=$($webPlan.Properties.NumberOfWorkers)}
        $jobName = "{0}-jobhost" -f $suitename
        if (ResourceObjectExists $suitename $jobName Microsoft.Web/sites)
        {
            $webJobSku = GetResourceObject $suitename $jobName Microsoft.Web/sites
            $params += @{webJobSku=$($webJobSku.Properties.Sku)}
            $webJobPlan = GetResourceObject $suiteName ("{0}-jobsplan" -f $suiteName) Microsoft.Web/serverfarms
            $params += @{webJobWorkerSize=$($webJobPlan.Properties.WorkerSizeId)}
            $params += @{webJobWorkerCount=$($webJobPlan.Properties.NumberOfWorkers)}
        }
    }

    # Use MapiApiKey if already created or 
    $mapApiKey = GetEnvSetting "MapApiQueryKey"
    if ([string]::IsNullOrEmpty($mapApiKey) -and $azureEnvironmentName -ne "AzureCloud")
    {
        $mapApiKey = "0"
    }
    if (![string]::IsNullOrEmpty($mapApiKey))
    {
        $deploymentTemplatePath = "$(Split-Path $MyInvocation.MyCommand.Path)\RemoteMonitoringMapKey.json"
        $params += @{bingMapsApiKey=$mapApiKey}
    }
}

# Stream analytics does not auto stop, and if already exists should be set to LastOutputEventTime to not lose data
if (StopExistingStreamAnalyticsJobs $resourceGroupName)
{
    $params += @{asaStartBehavior='LastOutputEventTime'}
}

Write-Host "Suite name: $suitename"
Write-Host "DocDb Name: $docDbName"
Write-Host "Storage Name: $($storageAccount.StorageAccountName)"
Write-Host "IotHub Name: $iotHubName"
Write-Host "Eventhub Name: $eventhubName"
Write-Host "AAD Tenant: $($global:AADTenant)"
Write-Host "ResourceGroup Name: $resourceGroupName"
Write-Host "Deployment template path: $deploymentTemplatePath"

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
UpdateEnvSetting "ServiceSBName" $eventhubName
UpdateEnvSetting "ServiceSBConnectionString" $result.Outputs['ehConnectionString'].Value
UpdateEnvSetting "ServiceEHName" $result.Outputs['ehOutName'].Value
UpdateEnvSetting "IotHubName" $result.Outputs['iotHubHostName'].Value
UpdateEnvSetting "IotHubConnectionString" $result.Outputs['iotHubConnectionString'].Value
UpdateEnvSetting "DocDbEndPoint" $result.Outputs['docDbURI'].Value
UpdateEnvSetting "DocDBKey" $result.Outputs['docDbKey'].Value
UpdateEnvSetting "DeviceTableName" "DeviceList"
UpdateEnvSetting "RulesEventHubName" $result.Outputs['ehRuleName'].Value
UpdateEnvSetting "RulesEventHubConnectionString" $result.Outputs['ehConnectionString'].Value
if ($result.Outputs['bingMapsQueryKey'].Value.Length -gt 0 -and $result.Outputs['bingMapsQueryKey'].Value -ne "0")
{
    UpdateEnvSetting "MapApiQueryKey" $result.Outputs['bingMapsQueryKey'].Value
}

Write-Host ("Provisioning and deployment completed successfully, see {0}.config.user for deployment values" -f $environmentName)

if ($environmentName -ne "local")
{
    $maxSleep = 40
    $webEndpoint = "{0}.{1}" -f $environmentName, $global:websiteSuffix
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