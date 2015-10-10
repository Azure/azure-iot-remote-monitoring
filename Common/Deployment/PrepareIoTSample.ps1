Param(
    [Parameter(Mandatory=$True,Position=0)]
    $environmentName,
    [Parameter(Mandatory=$True,Position=1)]
    $configuration
    )

# Initialize library
$environmentName = $environmentName.ToLowerInvariant()
. "$(Split-Path $MyInvocation.MyCommand.Path)\DeploymentLib.ps1"
Switch-AzureMode AzureResourceManager
Clear-DnsClientCache

# Sets Azure Accounts, Region, Name validation, and AAD application
InitializeEnvironment $environmentName

# Set environment specific variables 
$suitename = "IotSuiteLocal"
$suiteType = "LocalMonitoring"
$deploymentTemplatePath = "$(Split-Path $MyInvocation.MyCommand.Path)\LocalMonitoring.json"
$global:site = "https://localhost:44305/"
$cloudDeploy = $false

if ($environmentName -ne "local")
{
    $suiteName = $environmentName
    $suiteType = "RemoteMonitoring"
    $deploymentTemplatePath = "$(Split-Path $MyInvocation.MyCommand.Path)\RemoteMonitoring.json"
    $global:site = "https://{0}.azurewebsites.net/" -f $environmentName
    [string]$branch = "$(git symbolic-ref --short -q HEAD)"
    $cloudDeploy = $true
}
$resourceGroupName = (GetResourceGroup -Name $suiteName -Type $suiteType).ResourceGroupName
$storageAccount = GetAzureStorageAccount $suiteName $resourceGroupName
$iotHubName = GetAzureIotHubName $suitename $resourceGroupName
$sevicebusName = GetAzureServicebusName $suitename $resourceGroupName

# Setup AAD for webservice
UpdateResourceGroupState $resourceGroupName ProvisionAAD
$global:AADTenant = GetOrSetEnvSetting "AADTenant" "GetAADTenant"
UpdateEnvSetting "AADMetadataAddress" ("https://login.windows.net/{0}/FederationMetadata/2007-06/FederationMetadata.xml" -f $global:AADTenant)
UpdateEnvSetting "AADAudience" ($global:site + "iot")
UpdateEnvSetting "AADRealm" ($global:site + "iot")

# Deploy via Template
UpdateResourceGroupState $resourceGroupName ProvisionAzure
$params = @{ `
    suiteName=$suitename; `
    docDBName=$(GetAzureDocumentDbName $suitename $resourceGroupName); `
    storageName=$($storageAccount.Name); `
    iotHubName=$iotHubName; `
    sbName=$sevicebusName; `
    aadTenant=$($global:AADTenant)}

Write-Host "Suite name: $suitename"
Write-Host "DocDb Name: $(GetAzureDocumentDbName $suitename $resourceGroupName)"
Write-Host "Storage Name: $($storageAccount.Name)"
Write-Host "IotHub Name: $iotHubName"
Write-Host "Servicebus Name: $sevicebusName"
Write-Host "AAD Tenant: $($global:AADTenant)"
Write-Host "ResourceGroup Name: $resourceGroupName"
Write-Host "Deployment template path: $deploymentTemplatePath"

# Upload WebPackages
if ($cloudDeploy)
{
    $webPackage = UploadFile (".\DeviceAdministration\Web\obj\{0}\Package\Web.zip" -f $configuration) $storageAccount.Name $resourceGroupName "WebDeploy"
    $params += @{packageUri=$webPackage}
    $webJobPackage = UploadFile (".\WebJobHost\obj\{0}\Package\WebJobHost.zip" -f $configuration) $storageAccount.Name $resourceGroupName "WebDeploy"
    $params += @{webJobPackageUri=$webJobPackage}
}

Write-Host "Provisioning resources, if this is the first time, this operation can take up 10 minutes..."
$result = New-AzureResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile $deploymentTemplatePath -TemplateParameterObject $params -Verbose

if ($result.ProvisioningState -ne "Succeeded")
{
    UpdateResourceGroupState $resourceGroupName Failed
    throw "Provisioing failed"
}

# Set Config file variables
UpdateResourceGroupState $resourceGroupName Complete
UpdateEnvSetting "ServiceStoreAccountName" $storageAccount.Name
UpdateEnvSetting "ServiceStoreAccountConnectionString" $result.Outputs['storageConnectionString'].Value
UpdateEnvSetting "ServiceSBName" $sevicebusName
UpdateEnvSetting "ServiceSBConnectionString" $result.Outputs['ehConnectionString'].Value
UpdateEnvSetting "ServiceEHName" $result.Outputs['ehOutName'].Value
UpdateEnvSetting "IotHubName" $result.Outputs['iotHubHostName'].Value
UpdateEnvSetting "IotHubConnectionString" $result.Outputs['iotHubConnectionString'].Value
UpdateEnvSetting "DocDbEndPoint" $result.Outputs['docDbURI'].Value
UpdateEnvSetting "DocDBKey" $result.Outputs['docDbKey'].Value
UpdateEnvSetting "DeviceTableName" "DeviceList"
UpdateEnvSetting "RulesEventHubName" $result.Outputs['ehOutName'].Value
UpdateEnvSetting "RulesEventHubConnectionString" $result.Outputs['ehConnectionString'].Value
UpdateEnvSetting "MapApiQueryKey" $result.Outputs['bingMapsQueryKey'].Value

if ($environmentName -ne "local")
{
    start $global:site
}
Write-Host ("Provisioning and deployment completed successfully, see {0}.config.user for deployment values" -f $environmentName)
