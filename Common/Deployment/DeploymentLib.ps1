function ImportLibraries(){
    $success = $true
    $mydocuments = [environment]::getfolderpath("mydocuments")
    $nugetPath = "{0}\Nugets" -f $mydocuments
    if(-not (Test-Path $nugetPath)) {New-Item -Path $nugetPath -ItemType "Directory" | out-null}
    if(-not(Test-Path "$nugetPath\nuget.exe"))
    {
        Write-Host "nuget.exe not found. Downloading from http://www.nuget.org/nuget.exe ..." -ForegroundColor Yellow
        $wc = New-Object System.Net.WebClient
        $wc.DownloadFile("http://www.nuget.org/nuget.exe", "$nugetPath\nuget.exe");
    }

    # ActiveDirectory library
    $success = $success -and (LoadLibrary "Microsoft.IdentityModel.Clients.ActiveDirectory" $nugetPath)

    # Servicebus library
    $success = $success -and (LoadLibrary "WindowsAzure.ServiceBus" $nugetPath "Microsoft.ServiceBus")

    # Storage Library
    $success = $success -and (LoadLibrary "WindowsAzure.Storage" $nugetPath "Microsoft.WindowsAzure.Storage")

    return $success
}

function LoadLibrary()
{
    param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [string]$library,
        [Parameter(Mandatory=$true, Position=1)]
        [string]$nugetPath,
        [Parameter(Mandatory=$false, Position=2)]
        [string]$dllName = $library
    )
    $success = $true
    if (([appdomain]::CurrentDomain.GetAssemblies() | ?{$_.ManifestModule.Name -eq "$dllName.dll"}) -eq $null)
    {
        Write-Host ("Library {0} not found, loading..." -f $library)  -ForegroundColor Yellow
        $packageDirectories = (Get-ChildItem -Path $nugetPath -Filter ("{0}*" -f $library) -Directory)
        if($packageDirectories.Length -eq 0)
        {
            Write-Host ("{0} Library Nuget doesn't exist. Downloading now ..." -f $library) -ForegroundColor Yellow
            $nugetDownloadExpression = "& '$nugetPath\nuget.exe' install $library -OutputDirectory '$nugetPath' -Source https://www.nuget.org/api/v2 | out-null"
            Invoke-Expression $nugetDownloadExpression
            $packageDirectories = (Get-ChildItem -Path $nugetPath -Filter ("{0}*" -f $library) -Directory)
            if ($packageDirectories.Length -eq 0)
            {
                Write-Error ("Unable to find package {0} on Nuget.org" -f $library)
                return $false
            }
        }
        $assemblies = (Get-ChildItem ("{0}.dll" -f $dllName) -Path ($packageDirectories |sort Name -desc)[0].FullName -Recurse)
        if ($assemblies -eq $null)
        {
            Write-Error ("Unable to find {0}.dll assembly for {0} library, is the dll a different name?" -f $library)
            return $false
        }

        # Should figure out how to get correct version
        $assembly = $assemblies[0]
        if($assembly.Length -gt 0)
        {
            Write-Host ("Loading {0} Assembly ..." -f $assembly.Name) -ForegroundColor Green
            [System.Reflection.Assembly]::LoadFrom($assembly.FullName) | out-null
        }
        else
        {
            Write-Host ("Fixing {0} package directories ..." -f $library) -ForegroundColor Yellow
            $packageDirectories | Remove-Item -Recurse -Force | Out-Null
            Write-Error ("Not able to load {0} assembly. Restart PowerShell session and try again ..." -f $library)
            $success = $false
        }
    }
    return $success
}

function GetAuthenticationResult()
{
    param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [string]$tenant,
        [Parameter(Mandatory=$true, Position=1)]
        [string]$authUri,
        [Parameter(Mandatory=$true, Position=2)]
        [string]$resourceUri,
        [Parameter(Mandatory=$false, Position=3)]
        [string]$user = $null,
        [Parameter(Mandatory=$false)]
        [string]$prompt = "Auto"
    )
    $AADClientId = "1950a258-227b-4e31-a9cf-717495945fc2"
    [Uri]$AADRedirectUri = "urn:ietf:wg:oauth:2.0:oob"
    $authority = "{0}{1}" -f $authUri, $tenant
    write-verbose ("Authority: '{0}'" -f $authority)
    $authContext = New-Object "Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext" -ArgumentList $authority,$true
    $userId = [Microsoft.IdentityModel.Clients.ActiveDirectory.UserIdentifier]::AnyUser
    if (![string]::IsNullOrEmpty($user))
    {
        $userId = new-object Microsoft.IdentityModel.Clients.ActiveDirectory.UserIdentifier -ArgumentList $user, "OptionalDisplayableId"
    }
    write-Verbose ("{0}, {1}, {2}, {3}" -f $resourceUri, $AADClientId, $AADRedirectUri, $userId.Id)
    $authResult = $authContext.AcquireToken($resourceUri, $AADClientId, $AADRedirectUri, $prompt, $userId)
    return $authResult
}

function GetSuiteLocation()
{
    Write-Host "Available locations:";
    $regions = @();
    $index = 1
    foreach ($loc in $locations)
    {
        $region = New-Object System.Object
        $region | Add-Member -MemberType NoteProperty -Name "Option" -Value $index
        $region | Add-Member -MemberType NoteProperty -Name "Region" -Value $loc
        $regions += $region
        $index += 1
    }
    
    Write-Host ($regions | Out-String)
    
    $region = "notset"
    while ($region -eq "notset" -or !(ValidateLocation $region))
    {
        try 
        {
            [int]$selectedIndex = Read-Host 'Select an option from the above list'
        }
        catch 
        {
            Write-Host "Must be a number"
            continue
        }
        
        if ($selectedIndex -lt 1 -or $selectedIndex -ge $index)
        {
            continue
        }
        
        $region = $locations[$selectedIndex - 1]
    }
    return $region
}

function ValidateLocation()
{
    param ([Parameter(Mandatory=$true)][string]$location)
        
    foreach ($loc in $global:locations)
    {
        if ($loc.Replace(' ', '').ToLowerInvariant() -eq $location.Replace(' ', '').ToLowerInvariant())
        {
            return $true;
        }
    }
    Write-Warning "$(Get-Date –f $timeStampFormat) - Location $location is not available for this subscription.  Specify different -Location";
    Write-Warning "$(Get-Date –f $timeStampFormat) - Available Locations:";
    foreach ($loc in $locations)
    {
        Write-Warning $loc
    }
    return $false
}

function GetResourceGroup()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $name,
        [Parameter(Mandatory=$true,Position=1)] [string] $type
    )
    $resourceGroup = Find-AzureRmResourceGroup -Tag @{Name="IotSuiteType";Value=$type} | ?{$_.Name -eq $name}
    if ($resourceGroup -eq $null)
    {
        return New-AzureRmResourceGroup -Name $name -Location $global:AllocationRegion -Tag @{Name="IoTSuiteType";Value=$type}, @{Name="IoTSuiteVersion";Value=$global:version}, @{Name="IoTSuiteState";Value="Created"}
    }
    else
    {
    	return Get-AzureRmResourceGroup -Name $name -Location $global:AllocationRegion
    }
}

function UpdateResourceGroupState()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $resourceGroupName,
        [Parameter(Mandatory=$true,Position=1)] [string] $state
    )

    $resourceGroup = Get-AzureRmResourceGroup -ResourceGroupName $resourceGroupName
    if ($resourceGroup -ne $null)
    {
        $tags = $resourceGroup.Tags
        $updated = $false
        foreach ($tag in $tags)
        {
            if ($tag.Name -eq "IoTSuiteState")
            {
                $tag.Value = $state
                $updated = $true
			}
			if ($tag.Name -eq "IoTSuiteVersion" -and $tag.Value -ne $global:version)
			{
                $tag.Value = $global:version
                $updated = $true
			}
        }
        if (!$updated)
        {
            $tags += @{Name="IoTSuiteState";Value=$state}
        }
        $resourceGroup = Set-AzureRmResourceGroup -Name $resourceGroupName -Tag $tags
    }
}

function ValidateResourceName()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $resourceBaseName,
        [Parameter(Mandatory=$true,Position=1)] [string] $resourceType,
        [Parameter(Mandatory=$true,Position=2)] [string] $resourceGroupName,
        [Parameter(Mandatory=$true,Position=3)] [bool] $cloudDeploy
    )

    # Generate a unique name
    $resourceUrl = " "
    switch ($resourceType.ToLowerInvariant())
    {
        "microsoft.devices/iothubs"
        {
            $resourceUrl = "azure-devices.net"
        }
        "microsoft.storage/storageaccounts"
        {
            $resourceUrl = "blob.core.windows.net"
            $resourceBaseName = $resourceBaseName.Substring(0, [System.Math]::Min(19, $resourceBaseName.Length))
        }
        "microsoft.documentdb/databaseaccounts"
        {
            $resourceUrl = "documents.azure.com"
        }
        "microsoft.eventhub/namespaces"
        {
            $resourceUrl = "servicebus.windows.net"
            $resourceBaseName = $resourceBaseName.Substring(0, [System.Math]::Min(35, $resourceBaseName.Length))
        }
        "microsoft.web/sites"
        {
            $resourceUrl = "azurewebsites.net"
        }
        default {}
    }
    
    # Return name for existing resource if exists
    $resources = Find-AzureRmResource -ResourceGroupNameContains $resourceGroupName -ResourceType $resourceType -ResourceNameContains $resourceBaseName
    if ($resources -ne $null)
    {
        foreach($resource in $resources)
        {
            if ($resource.ResourceGroupName -eq $resourceGroupName -and $resource.Name.ToLowerInvariant().StartsWith($resourceBaseName.ToLowerInvariant()))
            {
                return $resource.Name
            }
        }
    }
    
    return GetUniqueResourceName $resourceBaseName $resourceUrl $cloudDeploy
}

function GetUniqueResourceName()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $resourceBaseName,
        [Parameter(Mandatory=$true,Position=1)] [string] $resourceUrl,
        [Parameter(Mandatory=$true,Position=2)] [bool] $cloudDeploy
    )

    if ($cloudDeploy)
    {
        $name = $resourceBaseName
    }
    else
    {
        $name = "{0}{1:x5}" -f $resourceBaseName, (get-random -max 1048575)
    }
    $max = 200
    while (HostEntryExists ("{0}.{1}" -f $name, $resourceUrl))
    {
        $name = "{0}{1:x5}" -f $resourceBaseName, (get-random -max 1048575)
        if ($max-- -le 0)
        {
            throw ("Unable to create unique name for resource {0} for url {1}" -f $resourceBaseName, $resourceUrl)
        }
    }
    ClearDNSCache
    return $name
}

function GetAzureStorageAccount()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $storageBaseName,
        [Parameter(Mandatory=$true,Position=1)] [string] $resourceGroupName,
        [Parameter(Mandatory=$true,Position=2)] [bool] $cloudDeploy,
        [Parameter(Mandatory=$false,Position=3)] [string] $location = $global:AllocationRegion
    )
    $storageTempName = $storageBaseName.ToLowerInvariant().Replace('-','')
    $storageAccountName = ValidateResourceName $storageTempName.Substring(0, [System.Math]::Min(19, $storageTempName.Length)) Microsoft.Storage/storageAccounts $resourceGroupName $cloudDeploy
    $storage = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountName -ErrorAction SilentlyContinue
    if ($storage -eq $null)
    {
        Write-Host "$(Get-Date –f $timeStampFormat) - Creating new storage account: $storageAccountName"
        $storage = New-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -StorageAccountName $storageAccountName -Location $location -Type Standard_GRS
    }
    return $storage
}

function GetAzureDocumentDbName()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $baseName,
        [Parameter(Mandatory=$true,Position=1)] [string] $resourceGroupName,
        [Parameter(Mandatory=$true,Position=2)] [bool] $cloudDeploy
    )
    return ValidateResourceName $baseName.ToLowerInvariant() Microsoft.DocumentDb/databaseAccounts $resourceGroupName $cloudDeploy
}

function GetAzureIotHubName()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $baseName,
        [Parameter(Mandatory=$true,Position=1)] [string] $resourceGroupName,
        [Parameter(Mandatory=$true,Position=2)] [bool] $cloudDeploy
    )
    return ValidateResourceName $baseName Microsoft.Devices/iotHubs $resourceGroupName $cloudDeploy
}

function GetAzureServicebusName()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $baseName,
        [Parameter(Mandatory=$true,Position=1)] [string] $resourceGroupName,
        [Parameter(Mandatory=$true,Position=2)] [bool] $cloudDeploy
    )
    return ValidateResourceName ($baseName.PadRight(6,"x")) Microsoft.Eventhub/namespaces $resourceGroupName $cloudDeploy
}

function StopExistingStreamAnalyticsJobs()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $resourceGroupName
    )
    $sasJobs = Find-AzureRmResource -ResourceGroupNameContains $resourceGroupName -ResourceType Microsoft.StreamAnalytics/streamingjobs
    if ($sasJobs -eq $null)
    {
        return $false
    }
    Write-Host "$(Get-Date –f $timeStampFormat) - Stopping existing Stream Analytics jobs..."
    $returnValue = $true
    foreach ($sasJob in $sasJobs)
    {
        if ($sasJob.ResourceGroupName -eq $resourceGroupName) {
            $null = Stop-AzureRmStreamAnalyticsJob -Name $sasJob.ResourceName -ResourceGroupName $resourceGroupName
            $job = Get-AzureRmStreamAnalyticsJob -Name $sasJob.ResourceName -ResourceGroupName $resourceGroupName
            if ($job.Properties.LastOutputEventTime -eq $null)
            {
                # If the job never has seen data, use JobStartTime
                $returnValue = $false
            }
        }
    }
    return $returnValue
}

function UploadFile()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $filePath,
        [Parameter(Mandatory=$true,Position=1)] [string] $storageAccountName,
        [Parameter(Mandatory=$true,Position=2)] [string] $resourceGroupName,
        [Parameter(Mandatory=$true,Position=3)] [string] $containerName,
        [Parameter(Mandatory=$true,Position=4)] [bool]   $secure
    )
    $maxSleep = 60
    $containerName = $containerName.ToLowerInvariant()
    $file = Get-Item -Path $filePath
    $fileName = $file.Name.ToLowerInvariant()
    $storageAccountKey = (Get-AzureRmStorageAccountKey -StorageAccountName $storageAccountName -ResourceGroupName $resourceGroupName).Key1
    $context = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $storageAccountKey
    if (!(HostEntryExists $context.StorageAccount.BlobEndpoint.Host))
    {
        Write-Host "$(Get-Date –f $timeStampFormat) - Waiting for storage account url to resolve." -NoNewline
        while (!(HostEntryExists $context.StorageAccount.BlobEndpoint.Host))
        {
            Write-Host "." -NoNewline
            ClearDNSCache
            sleep 3
        }
        Write-Host
    }
    $null = New-AzureStorageContainer $ContainerName -Permission Off -Context $context -ErrorAction SilentlyContinue
    $null = Set-AzureStorageBlobContent -Blob $fileName -Container $ContainerName -File $file.FullName -Context $context -Force

    # Generate Uri with sas token
    $storageAccount = [Microsoft.WindowsAzure.Storage.CloudStorageAccount]::Parse(("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}" -f $storageAccountName, $storageAccountKey))
    $blobClient = $storageAccount.CreateCloudBlobClient()
    $container = $blobClient.GetContainerReference($containerName)
    Write-Host ("$(Get-Date –f $timeStampFormat) - Checking container '{0}'." -f $containerName) -NoNewline
    while (!$container.Exists())
    {
        Write-Host "." -NoNewline
        sleep 1
        if ($maxSleep-- -le 0)
        {
            throw ("Timed out waiting for container: {0}" -f $ContainerName)
        }
    }
    Write-Host
    Write-Host ("$(Get-Date –f $timeStampFormat) - Checking blob '{0}'." -f $fileName) -NoNewline
    $blob = $container.GetBlobReference($fileName)
    while (!$blob.Exists())
    {
        Write-Host "." -NoNewline
        sleep 1
        if ($maxSleep-- -le 0)
        {
            throw ("Timed out waiting for blob: {0}" -f $fileName)
        }
    }
    Write-Host
    if ($secure)
    {
        $sasPolicy = New-Object Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy
        $sasPolicy.SharedAccessStartTime = [System.DateTime]::Now.AddMinutes(-5)
        $sasPolicy.SharedAccessExpiryTime = [System.DateTime]::Now.AddHours(24)
        $sasPolicy.Permissions = [Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPermissions]::Read
        $sasToken = $blob.GetSharedAccessSignature($sasPolicy)
    }
    return $blob.Uri.ToString() + $sasToken
}

function EnvSettingExists()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] $settingName
        )
    return ($global:envSettingsXml.Environment.SelectSingleNode("//setting[@name = '$settingName']") -ne $null);
}

function GetOrSetEnvSetting()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $settingName,
        [Parameter(Mandatory=$true,Position=1)] [string] $command
        )

        $settingValue = GetEnvSetting $settingName $false
        if ([string]::IsNullOrEmpty($settingValue))
        {
            $settingValue = Invoke-Expression -Command $command
            $null = PutEnvSetting $settingName $settingValue
        }
        return $settingValue
}

function UpdateEnvSetting()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] $settingName,
        [Parameter(Mandatory=$true,Position=1)] [AllowEmptyString()] $settingValue
        )
    $currentValue = GetEnvSetting $settingName $false
    if ($currentValue -ne $settingValue)
    {
        PutEnvSetting $settingName $settingValue
    }
}

function GetEnvSetting()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $settingName,
        [Parameter(Mandatory=$false,Position=1)][switch] $errorOnNull = $true
        )

    $setting = $global:envSettingsXml.Environment.SelectSingleNode("//setting[@name = '$settingName']")

    if ($setting -eq $null)
    {
        if ($errorOnNull)
        {
            Write-Error -Category ObjectNotFound -Message "Could not locate setting named '$settingName' in environment settings file."
            exit 1
        }
    }
    return $setting.value
}

function PutEnvSetting()
{
    Param(
        [Parameter(Mandatory=$True,Position=0)] [string] $settingName,
        [Parameter(Mandatory=$True,Position=1)] [AllowEmptyString()] [string] $settingValue
        )
        if (EnvSettingExists $settingName)
        {
            Write-Host "$(Get-Date –f $timeStampFormat) - $settingName changed to $settingValue"
            $global:envSettingsXml.Environment.SelectSingleNode("//setting[@name = '$settingName']").value = $settingValue
        }
        else
        {
            Write-Host "$(Get-Date –f $timeStampFormat) - Added $settingName with value $settingValue"
            $node = $envSettingsXml.CreateElement("setting")
            $node.SetAttribute("name", $settingName)
            $node.SetAttribute("value", $settingValue)
            $envSettingsXml.Environment.AppendChild($node)
        }
        $global:envSettingsChanges++
        $envSettingsXml.Save((Get-Item $global:environmentSettingsFile).FullName)
}

function LoadAzureAssembly()
{
    Param([Parameter(Mandatory=$true,Position=0)] $assembly)
    $assemblyPath = $global:azurePath + "\" + $assembly
    if (!(test-path $assemblyPath))
    {
        write-host -Message "$(Get-Date –f $timeStampFormat) - Error unable to locate $assembly."
        exit 1
    }
    [Void] [Reflection.Assembly]::LoadFile($assemblyPath);
}

function GetAzureAccountInfo()
{
    $account = Get-AzureAccount
    
    if ($account -eq $null)
    {
        Write-Host "Signing you into Azure..."
        $account = Add-AzureAccount
    }
    else 
    {
        Write-Host "Signed into Azure already"
    }
    
    $profilePath = Join-Path $PSScriptRoot "..\..\$($account.Id).user"
    $rmProfileLoaded = $false
    
    if (test-path $profilePath) {
        Write-Host "Trying to use saved profile $($profilePath)"
        $rmProfileLoaded = (Select-AzureRmProfile -Path $profilePath) -ne $null
    }
    
    if ($rmProfileLoaded -ne $true) {
        Write-Host "Logging in"
        Login-AzureRmAccount | Out-Null
        Save-AzureRmProfile -Path $profilePath
    }
    
    $id = $account.Id
    return $id
}

function HostEntryExists()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] $hostName
    )
    try
    {
        $hostName = [Net.Dns]::GetHostEntry($hostName)
        if ($hostName -ne $null)
        {
            Write-Verbose ("Found hostname: {0}" -f $hostName)
            return $true
        }
    }
    catch {}
    Write-Verbose ("Did not find hostname: {0}" -f $hostName)
    return $false
}

function ClearDNSCache()
{
    if ($global:ClearDns -eq $null)
    {
        $global:ClearDns = CommandExists Clear-DnsClientCache
    }
    if ($global:ClearDns)
    {
        Clear-DnsClientCache
    }
}

function CommandExists()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] $command
    )
    $oldPreference = $ErrorActionPreference
    $ErrorActionPreference = 'stop'
    try
    {
        if (Get-Command $command)
        {
            return $true
        }
    }
    catch {}
    finally
    {
        $ErrorActionPreference = $oldPreference
    }
    return $false
}

function ReplaceFileParameters()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $filePath,
        [Parameter(Mandatory=$true,Position=1)] [array] $arguments
    )
    $fileContent = cat $filePath | Out-String
    for ($i = 0; $i -lt $arguments.Count; $i++)
    {
        $fileContent = $fileContent.Replace("{$i}", $arguments[$i])
    }
    return $fileContent
}

function GetAADTenant()
{
    $tenants = Get-AzureRmTenant
    if ($tenants.Count -eq 0)
    {
        Write-Error "No Active Directory domains found for '$global:AzureAccountName)'"
        Exit -1
    }
    if ($tenants.Count -eq 1)
    {
        [string]$tenantId = $tenants[0].TenantId
    }
    else
    {
        # List Active directories associated with account
        Write-Host "Available Active Directories:"
        $directories = @()
        $index = 1
        foreach ($tenantObj in $tenants)
        {
            $tenant = $tenantObj.TenantId
            $uri = "https://graph.windows.net/{0}/me?api-version=1.6" -f $tenant
            $authResult = GetAuthenticationResult $tenant $global:aadLoginUrl "https://graph.windows.net/" $global:AzureAccountName -Prompt "Auto"
            $header = $authResult.CreateAuthorizationHeader()
            $result = Invoke-RestMethod -Method "GET" -Uri $uri -Headers @{"Authorization"=$header;"Content-Type"="application/json"}
            if ($result -ne $null)
            {
                $directory = New-Object System.Object
                $directory | Add-Member -MemberType NoteProperty -Name "Option" -Value $index
                $directory | Add-Member -MemberType NoteProperty -Name "Directory Name" -Value ($result.userPrincipalName.Split('@')[1])
                $directory | Add-Member -MemberType NoteProperty -Name "Tenant Id" -Value $tenant
                $directories += $directory
                $index += 1
            }
        }

        [int]$selectedIndex = -1
        write-host ($directories | Out-String)
        while ($selectedIndex -lt 1 -or $selectedIndex -ge $index)
        {
            try
            {
                [int]$selectedIndex = Read-Host "Select an option from the above list"
            }
            catch
            {
                Write-Host "Must be a number"
            }
        }
        $tenantId = $tenants[$selectedIndex - 1].TenantId
    }
    
    # Configure Application
    $uri = "https://graph.windows.net/{0}/applications?api-version=1.6" -f $tenantId
    $searchUri = "{0}&`$filter=identifierUris/any(uri:uri%20eq%20'{1}{2}')" -f $uri, [System.Web.HttpUtility]::UrlEncode($global:site), $global:appName
    $authResult = GetAuthenticationResult $tenantId $global:aadLoginUrl "https://graph.windows.net/" $global:AzureAccountName
    $header = $authResult.CreateAuthorizationHeader()

    # Check for application
    $result = Invoke-RestMethod -Method "GET" -Uri $searchUri -Headers @{"Authorization"=$header;"Content-Type"="application/json"}
    if ($result.value.Count -eq 0)
    {
        $body = ReplaceFileParameters ("{0}\Application.json" -f $global:azurePath) -arguments @($global:site, $global:environmentName)
        $result = Invoke-RestMethod -Method "POST" -Uri $uri -Headers @{"Authorization"=$header;"Content-Type"="application/json"} -Body $body -ErrorAction SilentlyContinue
        if ($result -eq $null)
        {
            throw "Unable to create application'$($global:site)iotsuite'"
        }
        Write-Host "Successfully created application '$($result.displayName)'"
        $applicationId = $result.appId
    }
    else
    {
        Write-Host "Found application '$($result.value[0].displayName)'"
        $applicationId = $result.value[0].appId
    }

    # Check for ServicePrincipal
    $uri = "https://graph.windows.net/{0}/servicePrincipals?api-version=1.6" -f $tenantId
    $searchUri = "{0}&`$filter=appId%20eq%20'{1}'" -f $uri, $applicationId
    $result = Invoke-RestMethod -Method "GET" -Uri $searchUri -Headers @{"Authorization"=$header;"Content-Type"="application/json"}
    if ($result.value.Count -eq 0)
    {
        $body = "{ `"appId`": `"$applicationId`" }"
        $result = Invoke-RestMethod -Method "POST" -Uri $uri -Headers @{"Authorization"=$header;"Content-Type"="application/json"} -Body $body -ErrorAction SilentlyContinue
        if ($result -eq $null)
        {
            throw "Unable to create ServicePrincipal for application '$($global:site)iotsuite'"
        }
        Write-Host "Successfully created ServicePrincipal '$($result.displayName)'"
        $resourceId = $result.objectId
        $roleId = ($result.appRoles| ?{$_.value -eq "admin"}).Id
    }
    else
    {
        Write-Host "Found ServicePrincipal '$($result.value[0].displayName)'"
        $resourceId = $result.value[0].objectId
        $roleId = ($result.value[0].appRoles| ?{$_.value -eq "admin"}).Id
    }

    # Check for Assigned User
    $uri = "https://graph.windows.net/{0}/users/{1}/appRoleAssignments?api-version=1.6" -f $tenantId, $authResult.UserInfo.UniqueId
    $result = Invoke-RestMethod -Method "GET" -Uri $uri -Headers @{"Authorization"=$header;"Content-Type"="application/json"}
    if (($result.value | ?{$_.ResourceId -eq $resourceId}) -eq $null)
    {
        $body = "{ `"id`": `"$roleId`", `"principalId`": `"$($authResult.UserInfo.UniqueId)`", `"resourceId`": `"$resourceId`" }"
        $result = Invoke-RestMethod -Method "POST" -Uri $uri -Headers @{"Authorization"=$header;"Content-Type"="application/json"} -Body $body -ErrorAction SilentlyContinue
        if ($result -eq $null)
        {
            Write-Warning "Unable to create RoleAssignment for application '$($global:site)iotsuite' for current user - will be Implicit Readonly"
        }
        else
        {
            Write-Host "Successfully assigned user to application '$($result.resourceDisplayName)' as role 'Admin'"
        }
    }
    else
    {
        Write-Host "Application already assigned to role 'Admin'"
    }

    return $tenantId
}

function InitializeEnvironment()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] $environmentName
    )
    if ($environmentName.Length -lt 3 -or $environmentName.Length -gt 62)
    {
        throw "Suite name '$environmentName' must be between 3-62 characters"
    }

    if(!(ImportLibraries))
    {
        throw "Failed to load dependent libraries"
    }
    $global:environmentName = $environmentName
    $global:AzureAccountName = GetAzureAccountInfo

    # Validate environment variables
    $global:environmentSettingsFile = "{0}\..\..\{1}.config.user" -f $global:azurePath, $environmentName
    if (!(Test-Path $global:environmentSettingsFile))
    {
        copy ("{0}\ConfigurationTemplate.config" -f $global:azurePath) $global:environmentSettingsFile
        $global:envSettingsXml = [xml](cat $global:environmentSettingsFile)
    }

    if (!(Test-Path variable:envsettingsXml))
    {
        $global:envSettingsXml = [xml](cat $global:environmentSettingsFile)
    }

    if ([string]::IsNullOrEmpty($global:SubscriptionId))
    {
        $accounts = Get-AzureRmSubscription
        $global:SubscriptionId = GetEnvSetting "SubscriptionId"
        
        if ([string]::IsNullOrEmpty($global:SubscriptionId))
        {
            Write-Host "Available subscriptions:"
                $global:index = 0
                $selectedIndex = -1
                $accounts | Format-Table -Property @{name="Option";expression={$global:index;$global:index+=1}},SubscriptionName, SubscriptionId -au
            
            while (!$accounts.SubscriptionId.Contains($global:SubscriptionId))
            {
                try
                {
                    [int]$selectedIndex = Read-Host "Select an option from the above list"
                }
                catch
                {
                    Write-Host "Must be a number"
                    continue
                }
                
                if ($selectedIndex -lt 1 -or $selectedIndex -gt $accounts.length)
                {
                    continue
                }
                
                $global:SubscriptionId = $accounts[$selectedIndex - 1].SubscriptionId
            }
            UpdateEnvSetting "SubscriptionId" $global:SubscriptionId
        }
    }
    
    Select-AzureRmSubscription -SubscriptionId $global:SubscriptionId

    if ([string]::IsNullOrEmpty($global:AllocationRegion))
    {
        $global:AllocationRegion = GetOrSetEnvSetting "AllocationRegion" "GetSuiteLocation"
    }

    # Validate EnvironmentName availability for cloud
    if ($environmentName -ne "local")
    {
        $webResource = $null
        $resourceGroup = Get-AzureRmResourceGroup -Name $environmentName -ErrorAction SilentlyContinue
        if ($resourceGroup -ne $null)
        {
            $webResource = Get-AzureRmResource -ResourceType Microsoft.Web/sites -ResourceGroupName $environmentName -ResourceName $environmentName -ErrorAction SilentlyContinue
        }
        if ($webResource -eq $null)
        {
            if(Test-AzureName -Website $environmentName)
            {
                throw ("HostName {0} is not available" -f $environmentName)
            }
        }
    }
}

# Remove incorrectly duplicated files from the WebJob
function FixWebJobZip()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $filePath
    )
    $zipfile = get-item $filePath
    $zip = [System.IO.Compression.ZipFile]::Open($zipfile.FullName, "Update")

    $entries = $zip.Entries.Where({$_.FullName.Contains("EventProcessor-WebJob/settings.job")})
    foreach ($entry in $entries) { $entry.Delete() }

    $entries = $zip.Entries.Where({$_.FullName.Contains("EventProcessor-WebJob/Simulator")})
    foreach ($entry in $entries) { $entry.Delete() }

    $entries = $zip.Entries.Where({$_.FullName.Contains("DeviceSimulator-WebJob/EventProcessor")})
    foreach ($entry in $entries) { $entry.Delete() }

    $zip.Dispose()
}

# Variable initialization
[int]$global:envSettingsChanges = 0;
$global:timeStampFormat = "o"
$global:resourceNotFound = "ResourceNotFound"
$global:serviceNameToken = "ServiceName"
$global:azurePath = Split-Path $MyInvocation.MyCommand.Path
$global:version = Get-Content ("{0}\..\..\VERSION.txt" -f $global:azurePath)
$global:azureVersion = "1.0.3"
$global:aadLoginUrl = "https://login.windows.net/"
$global:locations = @("East US", "North Europe", "East Asia", "West US", "West Europe", "Southeast Asia")

# Check version
$module = Get-Module -ListAvailable | Where-Object{ $_.Name -eq 'Azure' }
$expected = New-Object System.Version($global:azureVersion)
$comparison = $expected.CompareTo($module.Version)

if ($comparison -eq 1)
{
    throw "Version $($module.Version.Major).$($module.Version.Minor).$($module.Version.Build); update to $($global:azureVersion) and run again."
}
elseif ($comparison -eq -1)
{
    Write-Warning "This script Azure Cmdlets was tested with $($global:azureVersion)"
    Write-Warning "Found $($module.Version.Major).$($module.Version.Minor).$($module.Version.Build) installed; continuing, but errors might occur"
}

# Load System.Web
Add-Type -AssemblyName System.Web

# Load System.IO.Compression.FileSystem
Add-Type -AssemblyName  System.IO.Compression.FileSystem

# Make sure Azure PowerShell modules are loaded
if ((Get-Module | where {$_.Name -match "Azure"}) -eq $Null)
{
    $programFiles = ${Env:ProgramFiles(x86)}
    if ($programFiles -eq $null)
    {
        $programFiles = ${Env:ProgramFiles}
    }
    $modulePath = "$programFiles\Microsoft SDKs\Azure\PowerShell\ServiceManagement\Azure\Azure.psd1"
    if (Test-Path $modulePath)
    {
        Get-ChildItem $modulePath | Import-Module
    }
    else
    {
        throw "Unable to find Azure.psd1 modules. Please install Azure Powershell 2.5.1 or later"
    }
}
