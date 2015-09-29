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
    $adalPackageDirectories = (Get-ChildItem -Path $nugetPath -Filter "Microsoft.IdentityModel.Clients.ActiveDirectory*" -Directory)
    if($adalPackageDirectories.Length -eq 0)
    {
        Write-Host "Active Directory Authentication Library Nuget doesn't exist. Downloading now ..." -ForegroundColor Yellow
        $nugetDownloadExpression = "& '$nugetPath\nuget.exe' install Microsoft.IdentityModel.Clients.ActiveDirectory -OutputDirectory '$nugetPath' | out-null"
        Invoke-Expression $nugetDownloadExpression
        $adalPackageDirectories = (Get-ChildItem -Path $nugetPath -Filter "Microsoft.IdentityModel.Clients.ActiveDirectory*" -Directory)
    }
    $serviceBusPackageDirectories = (Get-ChildItem -Path $nugetPath -Filter "WindowsAzure.ServiceBus*" -Directory)
    if($serviceBusPackageDirectories.Length -eq 0)
    {
        Write-Host "ServiceBus Library Nuget doesn't exist. Downloading now ..." -ForegroundColor Yellow
        $nugetDownloadExpression = "& '$nugetPath\nuget.exe' install WindowsAzure.ServiceBus -OutputDirectory '$nugetPath' | out-null"
        Invoke-Expression $nugetDownloadExpression
        $serviceBusPackageDirectories = (Get-ChildItem -Path $nugetPath -Filter "WindowsAzure.ServiceBus*" -Directory)
    }

    $ADAL_Assembly = (Get-ChildItem "Microsoft.IdentityModel.Clients.ActiveDirectory.dll" -Path $adalPackageDirectories[$adalPackageDirectories.length-1].FullName -Recurse)[0]
    $ADAL_WindowsForms_Assembly = (Get-ChildItem "Microsoft.IdentityModel.Clients.ActiveDirectory.WindowsForms.dll" -Path $adalPackageDirectories[$adalPackageDirectories.length-1].FullName -Recurse)[0]
    if($ADAL_Assembly.Length -gt 0 -and $ADAL_WindowsForms_Assembly.Length -gt 0)
    {
        Write-Host "Loading ADAL Assemblies ..." -ForegroundColor Green
        [System.Reflection.Assembly]::LoadFrom($ADAL_Assembly.FullName) | out-null
        [System.Reflection.Assembly]::LoadFrom($ADAL_WindowsForms_Assembly.FullName) | out-null
    }
    else
    {
        Write-Host "Fixing Active Directory Authentication Library package directories ..." -ForegroundColor Yellow
        $adalPackageDirectories | Remove-Item -Recurse -Force | Out-Null
        Write-Host "Not able to load ADAL assembly. Delete the Nugets folder in MyDocuments, restart PowerShell session and try again ..."
        $success = $false
    }

    $sb_Assembly = (Get-ChildItem "Microsoft.ServiceBus.dll" -Path $serviceBusPackageDirectories[$serviceBusPackageDirectories.length-1].FullName -Recurse)[0]
    if($sb_Assembly.Length -gt 0)
    {
        Write-Host "Loading ServiceBus Assembly ..." -ForegroundColor Green
        [System.Reflection.Assembly]::LoadFrom($sb_Assembly.FullName) | out-null
    }
    else
    {
        Write-Host "Fixing ServiceBus Library package directories ..." -ForegroundColor Yellow
        $serviceBusPackageDirectories | Remove-Item -Recurse -Force | Out-Null
        Write-Host "Not able to load ServiceBus assembly. Delete the Nugets folder in MyDocuments, restart PowerShell session and try again ..."
        $success = $false
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

function ValidateLocation()
{
    param ([Parameter(Mandatory=$true)][string]$location)
    $locations = Execute-Command -Command ("Get-AzureLocation")
    if ($locations -eq $null)
    {
        Add-AzureAccount
        $locations = Execute-Command -Command ("Get-AzureLocation")
    }
    foreach ($loc in $locations)
    {
        if ($loc.Name -eq $location)
        {
            return $true;
        }
    }
    Write-Warning "$(Get-Date –f $timeStampFormat) - Location $location is not available for this subscription.  Specify different -Location";
    Write-Warning "$(Get-Date –f $timeStampFormat) - Available Locations:";
    foreach ($loc in $locations)
    {
        Write-Warning $loc.Name
    }
    return $false
}

#Executes a command with retries
function Execute-Command($Command, [Array]$ExpectedExceptionList, $maxCommandRetries=5, $writeStatus=$true)
{
    $currentRetry = 0
    $success = $false
    $returnvalue = $Null
    do {
        try
        {
            if ($writeStatus) { Write-Host "$(Get-Date –f $timeStampFormat) - Executing $Command" }
            $returnvalue = Invoke-Expression -Command "$Command" -EV ex -EA Stop 2> $null
            $success = $true
            if ($writeStatus) { Write-Host "$(Get-Date –f $timeStampFormat) - Successfully executed $Command " }
        }
        catch [exception]
        {
            Write-Verbose "$(Get-Date –f $timeStampFormat) - Exception occurred while trying to execute $Command. Exception is: $($ex.Exception.GetType().Name) - $($ex.Exception.Message)"
            Write-Verbose ("Exception: {0}" -f $ex.Exception)
            Write-Verbose ("ErrorDetails: {0}" -f $ex.ErrorDetails)
            Write-Verbose ("PSMessageDetails: {0}" -f $ex.PSMessageDetails)
            Write-Verbose ("InnerException: {0}" -f $ex.Exception.InnerException)
            Write-Verbose ("InnerException Status: {0}" -f $ex.Exception.InnerException.Status)
            Write-Verbose ("Message: {0}" -f $ex.Exception.Message)
            Write-Verbose ("Status: {0}" -f $ex.Exception.Status)
            Write-Verbose ("Error code: {0}" -f $ex.Exception.ErrorCode)
            Write-Verbose "ExceptionList: $($ExpectedExceptionList -ne $Null)"

            if ($ExpectedExceptionList -ne $Null)
            {
                if ($ExpectedExceptionList.Contains($ex.Exception.getType()))
                {
                    Write-Host "$(Get-Date –f $timeStampFormat) - Expected Exception"
                    break
                }
                if ($ExpectedExceptionList.Contains($ex.Exception.getType().Name))
                {
                    Write-Host "$(Get-Date –f $timeStampFormat) - Expected Exception"
                    break
                }
                if ($ExpectedExceptionList.Contains($global:resourceNotFound))
                {
                    Write-Verbose "Checking for resource not found..."
                    if ((IsResourceNotFound $ex))
                    {
                        Write-Host "$(Get-Date –f $timeStampFormat) - Resource not found"
                        break
                    }
                }
            }

            if ($currentRetry++ -gt $maxCommandRetries)
            {
                $message = "Can not execute $Command . The error is:  $ex"
                Write-Warning "$(Get-Date –f $timeStampFormat) $message"
                throw $message
            }

            switch ($ex.Exception.GetType().Name)
            {
                "CommunicationException"
                {
                    Start-Sleep 30
                    Write-Warning "$(Get-Date –f $timeStampFormat) - Caught communication error. Will retry";
                }

                "TimeoutException"
                {
                    Start-Sleep 30
                    Write-Warning ("$(Get-Date –f $timeStampFormat) - $serviceBaseName - Caught communication error. Will retry " + (5 - $numServiceRetries) + " more times");
                }

                "IOException"
                {
                    Write-Warning "$(Get-Date –f $timeStampFormat) - Caught IOException. Will retry";
                    Start-Sleep 30
                }

                "StorageException"
                {
                    Write-Warning "$(Get-Date –f $timeStampFormat) - Caught StorageException. Will retry";
                    Start-Sleep 30
                }

                "WebException"
                {
                    switch ($ex.Exception.Status)
                    {
                        "ConnectFailure"
                        {
                            Start-Sleep 30
                            Write-Warning ("$(Get-Date –f $timeStampFormat) - $serviceBaseName - Caught communication error. Will retry " + (5 - $numServiceRetries) + " more times");
                        }
                        "SecureChannelFailure"
                        {
                            Write-Warning  "SecureChannelFailure - reloading subscription"
                            # Reload subscription
                            $subscriptions = Get-AzureSubscription -EA SilentlyContinue
                            $subscriptions | Remove-AzureSubscription -Confirm:$false -Force -EA SilentlyContinue
                            LoadSubscription($false)
                        }
                        default
                        {
                            throw $ex.Exception
                        }
                    }
                }

                "HttpRequestException"
                {
                    switch ($ex.Exception.InnerException.Status)
                    {
                        "ConnectFailure"
                        {
                            Start-Sleep 30
                            Write-Warning ("$(Get-Date –f $timeStampFormat) - $serviceBaseName - Caught communication error. Will retry " + (5 - $numServiceRetries) + " more times");
                        }
                        "SecureChannelFailure"
                        {
                            Write-Warning  "SecureChannelFailure - reloading subscription"
                            # Reload subscription
                            $subscriptions = Get-AzureSubscription -EA SilentlyContinue
                            $subscriptions | Remove-AzureSubscription -Confirm:$false -Force -EA SilentlyContinue
                            LoadSubscription($false)
                        }
                        default
                        {
                            throw $ex.Exception.InnerException
                        }
                    }
                }

                default
                {
                    throw $ex.Exception
                }
            }
        }
    } while (!$success)
    return $returnvalue
}

function IsResourceNotFound()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] $exc
    )

    write-verbose "GetType.Name $($exc.Exception.GetType().Name)"
    if ($exc.Exception.GetType().Name -eq "ResourceNotFoundException")
    {
        return $true
    }
    write-verbose "ErrorCode $($exc.Exception.ErrorCode)"
    if ($exc.Exception.ErrorCode -eq "ResourceNotFound")
    {
        return $true
    }
    write-verbose "Messsage $($exc.Exception.Message)"
    if ($exc.Exception.Message.StartsWith("ResourceNotFound"))
    {
        return $true
    }

    if ($exc.Exception.GetType().Name -ne "ServiceManagementClientException")
    {
        return $false
    }
    write-verbose "ErrorDetails.Code $($exc.Exception.ErrorDetails.Code)"
    return ($exc.Exception.ErrorDetails.Code -eq 'ResourceNotFound')
}

function GetAzureStorageAccount()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $storageName
    )

    # Try to get service
    $result = Execute-Command -Command ("Get-AzureStorageAccount -StorageAccountName $storageName") -ExpectedExceptionList @($global:resourceNotFound)
    return $result
}


function ValidateAzureStorageAccount()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $serviceBaseName,
        [Parameter(Mandatory=$true,Position=1)] [string] $storageNamePrefix
    )

    # Look for existing account matching pattern
    $storageAccounts = Execute-Command -Command "Get-AzureStorageAccount"
    foreach ($store in $storageAccounts)
    {
        if ($store.StorageAccountName.StartsWith($storageNamePrefix))
        {
            Write-Host ("$(Get-Date –f $timeStampFormat) - Using storage account {0}..." -f $store.StorageAccountName)
            UpdateEnvSetting "$($serviceBaseName)StoreAccountName" $store.StorageAccountName
            return $store.StorageAccountName
        }
    }

    # create account
    $max = 10
    $store = $null
    Write-Host "Storage account not found, will create a new one..."
    $name = $storageNamePrefix
    while ($store -eq $null)
    {
        $request = New-AzureStorageAccount -StorageAccountName $name -Location $global:AllocationRegion -EA SilentlyContinue
        if ($request.OperationStatus -eq "Succeeded")
        {
            $store = GetAzureStorageAccount $name
            Write-Host ("$(Get-Date –f $timeStampFormat) - Created storage account {0}..." -f $store.StorageAccountName)
            PutEnvSetting "$($serviceBaseName)StoreAccountName" $store.StorageAccountName
            return $store.StorageAccountName
        }
        if ($max-- -eq 0)
        {
            Write-Error -Message "Unable to create storage account for prefix $storageNamePrefix"
        }
        $name = "{0}{1:x}" -f $storageNamePrefix, (Get-Date).Millisecond
    }
    exit 1
}

function ValidateAzureServicebusNamespace()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $serviceBaseName,
        [Parameter(Mandatory=$true,Position=1)] [string] $serviceBusNamePrefix,
        [Parameter(Mandatory=$true,Position=2)] [string] $eventHubPath
    )

    # Look for existing account matching pattern
    $namespaces = Execute-Command -Command "Get-AzureSBNamespace"
    $namespace = $null;
    foreach ($sbNamespace in $namespaces)
    {
        if ($sbNamespace.Name.StartsWith($serviceBusNamePrefix))
        {
            Write-Host ("$(Get-Date –f $timeStampFormat) - Using Servicebus namespace {0}..." -f $sbNamespace.Name)
            UpdateEnvSetting "$($serviceBaseName)SBName" $sbNamespace.Name
            UpdateEnvSetting "$($serviceBaseName)SBConnectionString" $sbNamespace.ConnectionString
            UpdateEnvSetting "$($serviceBaseName)EHName" $eventHubPath
            $namespace = $sbNamespace;
            break
        }
    }

    # create account
    $max = 10
    $name = $serviceBusNamePrefix
    while ($namespace -eq $null)
    {
        Write-Host "Servicebus namespace not found, will create a new one..."
        $namespace = Execute-Command -Command ("New-AzureSBNamespace -Name $name -Location '$global:AllocationRegion' -NamespaceType 'Messaging' -CreateACSNamespace:`$false") -ExpectedExceptionList @("CloudException")
        if ($max-- -eq 0)
        {
            Write-Error -Message "Unable to create storage account for prefix $storageNamePrefix"
        }
        $name = "{0}{1:x}" -f $serviceBusNamePrefix, (Get-Date).Millisecond
        Start-Sleep 30
    }

    # check if create failed
    if ($namespace -eq $null)
    {
        exit 1
    }

    [int]$retries = 0
    $name = $namespace.Name
    while ($namespace.Status -ne "Active")
    {
        $retries++
        Write-Host -NoNewline "."
        Start-Sleep 30
        $namespace = Execute-Command -Command ("Get-AzureSBNamespace $name")  -ExpectedExceptionList @($global:resourceNotFound)
        if ($retries -gt 10)
        {
            Write-warning -Message "$(Get-Date –f $timeStampFormat) - Servicebus namespace '$name' did not activate within 5 minutes"
            exit 1
        }
    }

    # create eventhub
    $NamespaceManager = [Microsoft.ServiceBus.NamespaceManager]::CreateFromConnectionString($namespace.ConnectionString);
    if ($NamespaceManager.EventHubExists("$eventHubPath"))
    {
        return $namespace.Name
    }
    $EventHubDescription = New-Object -TypeName Microsoft.ServiceBus.Messaging.EventHubDescription -ArgumentList $eventHubPath
    $EventHubDescription.PartitionCount = 16
    $EventHubDescription.MessageRetentionInDays = 7
    $NamespaceManager.CreateEventHub($EventHubDescription);
    UpdateEnvSetting "$($serviceBaseName)SBName" $namespace.Name
    UpdateEnvSetting "$($serviceBaseName)SBConnectionString" $namespace.ConnectionString
    UpdateEnvSetting "$($serviceBaseName)EHName" $eventHubPath
    return $namespace.Name
}

function UpdateAzureStorageAccountConnectionString()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $serviceBaseName
    )
    $storageAccountName = "$($serviceBaseName)StoreAccountName"
    $connectionStringName = "$($serviceBaseName)StoreAccountConnectionString"
    $storageName = GetEnvSetting $storageAccountName
    $storageKey = Execute-Command -Command ("Get-AzureStorageKey $storageName")
    $connectionString = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}" -f $storageName, $storageKey.Primary
    $cloudStorageAccount = [Microsoft.WindowsAzure.Storage.CloudStorageAccount]::Parse($connectionString)

    # Verify the storage connection string
    if (EnvSettingExists($connectionStringName))
    {
        UpdateEnvSetting $connectionStringName $connectionString
    }
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

function Publish()
{
    Param(
        $serviceName,
        $slot,
        $deploymentLabel,
        $packageLocation,
        $cloudConfigLocation)

    Write-Progress -Activity "Deploying Service" -CurrentOperation "Publish $serviceName" -PercentComplete 0
    $deployment =  Execute-Command -Command ("Get-AzureDeployment -ServiceName $serviceName -Slot $slot") -ExpectedExceptionList @($global:resourceNotFound)

    #check for existing deployment and then either upgrade, otherwise new deployment
    if ($deployment.Name -ne $null)
    {
        Write-Host "$(Get-Date –f $timeStampFormat) - Deployment exists in $servicename.  Upgrading deployment."
        UpgradeDeployment -serviceName $serviceName -slot $slot -deploymentLabel "$deploymentLabel" -packageLocation $packageLocation -cloudConfigLocation $cloudConfigLocation
    }
    else
    {
        CreateNewDeployment -serviceName $serviceName -slot $slot -deploymentLabel "$deploymentLabel" -packageLocation $packageLocation -cloudConfigLocation $cloudConfigLocation
    }
}

function CreateNewDeployment()
{
    Param(
        $serviceName,
        $slot,
        $deploymentLabel,
        $packageLocation,
        $cloudConfigLocation)

    Write-Output "$(Get-Date –f $timeStampFormat) - $serviceName - Creating New Deployment: In progress"
    $retry = 0;
    while ($retry++ -lt 5)
    {
        Write-Progress -Activity "Deploying Service" -CurrentOperation "CreateNewDeployment $serviceName" -PercentComplete $retry
        $completeDeployment =  Execute-Command -Command ("Get-AzureDeployment -ServiceName $serviceName -Slot $slot") -ExpectedExceptionList @($global:resourceNotFound)
        if ($completeDeployment -ne $null)
        {
            if ($completeDeployment.Label -eq $deploymentLabel)
            {
                break;
            }
            # Wrong deployment, remove it
            DeleteDeployment -serviceName $serviceName -slot $slot
        }
        # retry deployment
        $opstat =  Execute-Command -Command ("New-AzureDeployment -Slot $slot -Package $packageLocation -Configuration $cloudConfigLocation -label `"$deploymentLabel`" -ServiceName $serviceName") -ExpectedExceptionList @([System.TimeoutException])
    }
    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Creating New Deployment: In progress"

    if ($completeDeployment -eq $null)
    {
        Write-Error -Category ObjectNotFound -Message "No deployment found after 4 attempts."
        exit 1
    }

    if ($completeDeployment.Label -ne $deploymentLabel)
    {
        Write-Error -Category ObjectNotFound -Message "Wrong deployment found after 4 attempts."
        exit 1
    }

    $completeDeploymentID = $completeDeployment.deploymentid
    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Creating New Deployment: Complete, Deployment ID: $completeDeploymentID"

    StartInstances -serviceName $serviceName -slot $slot -startInstancesRetryCount $maxRetries
}

function UpgradeDeployment()
{
    Param(
        $serviceName,
        $slot,
        $deploymentLabel,
        $packageLocation,
        $cloudConfigLocation)

    Write-Progress -Activity "Deploying Service" -CurrentOperation "UpgradeDeployment $serviceName" -PercentComplete 1
    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Upgrading Deployment: In progress"

    # perform Update-Deployment
    $setdeployment =  Execute-Command -Command ("Set-AzureDeployment -Upgrade -Slot $slot -Package $packageLocation -Configuration $cloudConfigLocation -label `"$deploymentLabel`" -ServiceName $serviceName -Force")

    $completeDeployment =  Execute-Command -Command ("Get-AzureDeployment -ServiceName $serviceName -Slot $slot")

    $completeDeploymentID = $completeDeployment.deploymentid

    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Upgrading Deployment: Complete, Deployment ID: $completeDeploymentID"
}

function DeleteDeployment()
{
    Param(
        $serviceName,
        $slot)

    Write-Progress -Activity "Deploying Service" -CurrentOperation "DeleteDeployment $serviceName" -PercentComplete 1
    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Deleting Deployment: In progress"

    #WARNING - always deletes with force
    $removeDeployment =  Execute-Command -Command ("Remove-AzureDeployment -Slot $slot -ServiceName $serviceName -Force")

    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Deleting Deployment: Complete"
}

function ResetTimer()
{
    $stageStartTime = Get-Date
    $stageTimeoutTime = $stageStartTime.addMinutes($timeoutPerRetryInMins)
    $timeBeforeTimeout = New-TimeSpan $stageStartTime $stageTimeoutTime
    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceFriendlyName - Deployment stage start time is $stageStartTime"
    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceFriendlyName - Deployment stage will timeout at $stageTimeoutTime"
    return $stageTimeoutTime
}

function StartInstances()
{
    Param(
        $serviceName,
        $slot,
        $startInstancesRetryCount)

    $deployment = Execute-Command -Command ("Get-AzureDeployment -ServiceName $serviceName -Slot $slot")
    $runstatus = $deployment.Status
    # For timeout
    $stageTimeoutTime = ResetTimer
    $count=0
    while ($runstatus -eq 'Deploying')
    {
        Write-Progress -Activity "Deploying Service" -CurrentOperation "Wait for deployment: $serviceName" -PercentComplete ($count++)
        Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Deployment status: $runstatus"
        $deployment = Execute-Command -Command ("Get-AzureDeployment -ServiceName $serviceName -Slot $slot")
        $runstatus = $deployment.Status

        # Handle timeout
        $timeBeforeTimeout = new-timespan $(get-date) $stageTimeoutTime
        if($timeBeforeTimeout -lt 0)
        {
            Write-Error "$(Get-Date –f $timeStampFormat) - $serviceName deployment timed out. Abandoning Deployment."
            exit 1
        }
        sleep -Seconds 30
        if ($count -eq 100)
        {
            $count = 90
        }
    }

    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Starting Instances: In progress"
    if ($runstatus -ne 'Running')
    {
        $run =  Execute-Command -Command ("Set-AzureDeployment -Slot $slot -ServiceName $serviceName -Status Running")
    }

    $deployment =  Execute-Command -Command ("Get-AzureDeployment -ServiceName $serviceName -Slot $slot")
    $oldStatusStr = @("") * $deployment.RoleInstanceList.Count

    while (-not(AllInstancesRunning($deployment.RoleInstanceList)))
    {
        Write-Progress -Activity "Deploying Service" -CurrentOperation "Wait for status: $serviceName" -PercentComplete ($count++)
        $i = 1
        foreach ($roleInstance in $deployment.RoleInstanceList)
        {
            $instanceName = $roleInstance.InstanceName
            $instanceStatus = $roleInstance.InstanceStatus

            if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
            {
                $oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
                Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Starting Instance '$instanceName': $instanceStatus"
            }

            $i = $i + 1
        }

        sleep -Seconds 10
        if ($count -eq 100)
        {
            $count = 90
        }

        $deployment =  Execute-Command -Command ("Get-AzureDeployment -ServiceName $serviceName -Slot $slot") -WriteStatus $false

        while ($deployment -eq $null)
        {
            Write-Warning "$(Get-Date –f $timeStampFormat) - $serviceName - $($Error[0])"
            sleep -Seconds 30
            $deployment =  Execute-Command -Command ("Get-AzureDeployment -ServiceName $serviceName -Slot $slot")
        }

        # Handle timeout
        $timeBeforeTimeout = new-timespan $(get-date) $stageTimeoutTime

        if($timeBeforeTimeout -lt 0)
        {
            $startInstancesRetryCount--
            Write-Warning "$(Get-Date –f $timeStampFormat) - $serviceName has still not started. Will reboot the instance(s) and retry $startInstancesRetryCount more times"

            if ($startInstancesRetryCount -le 0)
            {
                Write-Error "$(Get-Date –f $timeStampFormat) - $serviceName has still not started. Max retries exceeded. Abandoning Deployment."
                exit 1
            }

            #reboot the instance, and reset the timer
            foreach ($roleInstance in $deployment.RoleInstanceList)
            {
                $instanceName = $roleInstance.InstanceName
                Execute-Command -Command ("Reset-AzureRoleInstance -ServiceName $serviceName -InstanceName $instanceName -Reboot  -Slot $slot")
            }

            $stageTimeoutTime = ResetTimer
        }
    }

    $i = 1
    foreach ($roleInstance in $deployment.RoleInstanceList)
    {
        $instanceName = $roleInstance.InstanceName
        $instanceStatus = $roleInstance.InstanceStatus

        if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
        {
            $oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
            Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Starting Instance '$instanceName': $instanceStatus"
        }

        $i = $i + 1
    }

    $deployment =  Execute-Command -Command ("Get-AzureDeployment -ServiceName $serviceName -Slot $slot")
    $opstat = $deployment.Status

    Write-Host "$(Get-Date –f $timeStampFormat) - $serviceName - Starting Instances: $opstat"
    Write-Progress -Activity "Deploying Service" -CurrentOperation "Wait for status: $serviceName" -Complete
}

function AllInstancesRunning($roleInstanceList)
{
    foreach ($roleInstance in $roleInstanceList)
    {
        if ($roleInstance.InstanceStatus -ne "ReadyRole")
        {
            return $false
        }
    }

    return $true
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

function GetServices()
{
    $services = @();
    foreach($setting in $global:envSettingsXml.ChildNodes.setting)
    {
        if ($setting.name.EndsWith($global:serviceNameToken))
        {
            $service = $setting.Name.Substring(0, $setting.Name.Length - $global:serviceNameToken.Length)
            Write-Host "$(Get-Date –f $timeStampFormat) - Found Service: $service"
            $services += $service
        }
    }
    return $services
}

function ValidateService()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] $serviceBaseName,
        [Parameter(Mandatory=$true,Position=1)] $environmentName,
        [Parameter(Mandatory=$false,Position=2)][switch] $create = $false
    )

    # Don't provision for local environments
    if ($environmentName -eq "Local")
    {
        return
    }

    $serviceName = GetEnvSetting "$($serviceBaseName)ServiceName"
    if ([string]::IsNullOrEmpty($serviceName))
    {
        $serviceName = "{0}{1}" -f $environmentName, $serviceBaseName
        PutEnvSetting "$($serviceBaseName)ServiceName" $serviceName
    }
    $sslThumbprintName = "$($serviceBaseName)SslThumbprint"
    $certCn = $serviceName + ".azurewebsites.net"

    # Try to get service
    $service = Execute-Command -Command ("Get-AzureService -ServiceName $serviceName") -ExpectedExceptionList @($global:resourceNotFound)
    if ($service -eq $null -and $create)
    {
        $getServiceSucceeded = $false
        [int]$numServiceRetries = 0
        $service = Execute-Command -Command ("New-AzureService -ServiceName $serviceName -Location '$global:AllocationRegion'")
        Write-Host "$(Get-Date –f $timeStampFormat) - $serviceBaseName - service created"

        if (EnvSettingExists($sslThumbprintName))
        {
            if ($global:makeCertPath -eq $null)
            {
                $paths = New-Object 'System.Collections.Generic.List[string]'
                $paths.Add(".")
                $paths.AddRange($env:path.Split(';'))
                $makeCert = get-childitem -filter makecert.exe -Path $paths
                if ($makeCert -ne $null)
                {
                    $global:makeCertPath = $makeCert[0].FullName
                }
                else
                {
                    Write-Warning "$(Get-Date –f $timeStampFormat) - Can't create a certificate for $serviceName, deployment will fail if certificate isn't manually uploaded"
                    Write-Warning "$(Get-Date –f $timeStampFormat) - Unable to find MakeCert.exe in path, will not create certificates"
                    Write-Warning "$(Get-Date –f $timeStampFormat) - MakeCert is available as part of the Windows SDK, which you can download from http://go.microsoft.com/fwlink/p/?linkid=84091."
                }
            }

            if ($global:makeCertPath -ne $null)
            {
                # Add a selfsigned certificate for now.  Ops needs to add official certs (if needed).
                $pass = "{0:x}" -f (get-date).Ticks
                $certPfx = "$global:azurePath\$certCn.pfx"
                $certCer = "$global:azurePath\$certCn.cer"
                & certutil -delstore "My" "$certCn"
                & $global:makeCertPath -n "CN=$certCn" -ss my -sr LocalMachine -r -pe -len 2048 $certCer
                & certutil -p $pass -exportpfx "My" "$certCn" "$certPfx"
                & certutil -delstore "My" "$certCn"
                Execute-Command -Command ("Add-AzureCertificate -ServiceName $serviceName -CertToDeploy `"$certPfx`" -Password $pass")
                Remove-Item $certCer
                Remove-Item $certPfx
                Write-Warning "$(Get-Date –f $timeStampFormat) - $serviceName was created with a private selfsigned certificate."
            }
        }
    }
    if ($service -ne $null)
    {
        if (EnvSettingExists($sslThumbprintName))
        {
            $currentThumbprint = GetEnvSetting $sslThumbprintName
            $certs = Execute-Command -Command ("Get-AzureCertificate -ServiceName $serviceName")
            foreach ($cert in $certs)
            {
                $x509Cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
                $x509Cert.Import([System.Convert]::FromBase64String($cert.Data))
                if ($x509Cert.Thumbprint -eq $currentThumbprint)
                {
                    $thumbprint = $x509Cert.Thumbprint
                    break
                }
                # Gets cert by name for selfsigned certs
                if ($x509Cert.Subject -eq "cn=$certCn")
                {
                    $thumbprint = $x509Cert.Thumbprint
                }
            }
            if ($currentThumbprint -ne $thumbprint -and $thumbprint -ne $null)
            {
                PutEnvSetting $sslThumbprintName $thumbprint
            }
        }
    }
}

function UpdateServiceConfig()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] $serviceName,
        [Parameter(Mandatory=$true,Position=1)] $buildPath
        )
    $configToken = "config:"
    $configFile = "{0}\Services\{1}\pub\ServiceConfiguration.Cloud.cscfg" -f $buildPath, $serviceName
    $config = [xml](cat $configFile)

    # Update settings
    foreach ($node in $config.ServiceConfiguration.Role.ConfigurationSettings.ChildNodes)
    {
        if ($node.value.StartsWith($configToken, 1))
        {
            $node.value = GetEnvSetting ($node.value.Substring($node.value.IndexOf($configToken) + $configToken.Length))
        }
    }

    # Update certificate thumbprints
    foreach ($node in $config.ServiceConfiguration.Role.Certificates.ChildNodes)
    {
        if (EnvSettingExists $node.name)
        {
            $node.thumbprint = GetEnvSetting $node.name
        }
    }
    $config.Save($configFile)
}

function GetAzureAccountInfo()
{
    $account = Add-AzureAccount
    return $account.Id
}

function GetAADTenant()
{
    $account = Get-AzureAccount $global:AzureAccountName
    $tenants = ($account.Tenants -replace '(?:\r\n)',',').split(",")
    if ($tenants.Count -eq 0)
    {
        Write-Error "No Active Directory domains found for '$global:AzureAccountName)'"
        Exit -1
    }
    if ($tenants.Count -eq 1)
    {
        $tenantId = $account.Tenants[0]
    }
    else
    {
        # List Active directories associated with account
        Write-Host "Available Active directories:"
        Write-Host "Tenant ID                             Active Directory"
        Write-Host "---------                             ----------------"
        foreach ($tenant in $tenants)
        {
            $uri = "https://graph.windows.net/{0}/me?api-version=1.6" -f $tenant
            $authResult = GetAuthenticationResult $tenant "https://login.windows.net/" "https://graph.windows.net/" $global:AzureAccountName
            $header = $authResult.CreateAuthorizationHeader()
            $result = Invoke-RestMethod -Method "GET" -Uri $uri -Headers @{"Authorization"=$header;"Content-Type"="application/json"}
            if ($result -ne $null)
            {
                Write-Host "$tenant  $($result.userPrincipalName.Split('@')[1])"
    }
        }

    # Can't determine AADTenant, so prompt
        $tenantId = "notset"
        while (!$account.Tenants.Contains($tenantId))
        {
            $tenantId = Read-Host "Please select a valid TenantId from list"
        }
    }

    # Configure Application
    $uri = "https://graph.windows.net/{0}/applications?api-version=1.6" -f $tenantId
    $searchUri = "{0}&`$filter=identifierUris/any(uri:uri%20eq%20'{1}iotsuite')" -f $uri, [System.Web.HttpUtility]::UrlEncode($global:site)
    $authResult = GetAuthenticationResult $tenantId "https://login.windows.net/" "https://graph.windows.net/" $global:AzureAccountName
    $header = $authResult.CreateAuthorizationHeader()

    # Check for application
    $result = Invoke-RestMethod -Method "GET" -Uri $searchUri -Headers @{"Authorization"=$header;"Content-Type"="application/json"}
    if ($result.value.Count -eq 0)
    {
        $body = ReplaceFileParameters ("{0}\Application.json" -f $global:azurePath) -arguments @($global:site, $global:environmentName)
        $result = Invoke-RestMethod -Method "POST" -Uri $uri -Headers @{"Authorization"=$header;"Content-Type"="application/json"} -Body $body -ErrorAction Stop
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
        $result = Invoke-RestMethod -Method "POST" -Uri $uri -Headers @{"Authorization"=$header;"Content-Type"="application/json"} -Body $body -ErrorAction Stop
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
        $result = Invoke-RestMethod -Method "POST" -Uri $uri -Headers @{"Authorization"=$header;"Content-Type"="application/json"} -Body $body -ErrorAction Stop
        Write-Host "Successfully assigned user to application '$($result.resourceDisplayName)' as role 'Admin'"
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
    $null = ImportLibraries
    $global:environmentName = $environmentName
    if ($environmentName -eq "Local")
    {
        $global:site = "https://localhost:44305/"
    }
    else
    {
        $global:site = "https://{0}.azurewebsites.net/" -f $environmentName
    }

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

    if (!(Test-Path variable:AzureAccountName) -or ((get-azureaccount $global:AzureAccountName) -eq $null))
    {
        $global:AzureAccountName = GetOrSetEnvSetting "AzureAccountName" "GetAzureAccountInfo"
    }

    if (!(Test-Path variable:AADTenant))
    {
        $global:AADTenant = GetOrSetEnvSetting "AADTenant" "GetAADTenant"
        UpdateEnvSetting "AADMetadataAddress" ("https://login.windows.net/{0}/FederationMetadata/2007-06/FederationMetadata.xml" -f $global:AADTenant)
    }

    # Provision AAD for webservice
    UpdateEnvSetting "AADAudience" ($global:site + "iot")
    UpdateEnvSetting "AADRealm" ($global:site + "iot")

    if (!(Test-Path variable:SubscriptionId))
    {
        $accounts = Get-AzureSubscription -ErrorAction SilentlyContinue
        if ($accounts -eq $null)
        {
            $accounts = Get-AzureSubscription -ErrorAction Stop
        }
        $global:SubscriptionId = GetEnvSetting "SubscriptionId"
        if ([string]::IsNullOrEmpty($global:SubscriptionId))
        {
            $global:SubscriptionId = "z"
        }
        while (!$accounts.SubscriptionId.Contains($global:SubscriptionId))
        {
            Write-Host "Available subscriptions:"
            $accounts |ft SubscriptionName, SubscriptionId -au
            $global:SubscriptionId = Read-Host "Please select a valid SubscriptionId from list"
        }
        UpdateEnvSetting "SubscriptionId" $global:SubscriptionId
    }
    Select-AzureSubscription -SubscriptionId $global:SubscriptionId

    if (!(Test-Path variable:AllocationRegion))
    {
        $command = "Read-Host 'Enter Region to deploy resources (eg. West US)'"
        $region = GetOrSetEnvSetting "AllocationRegion" $command
        while (!(ValidateLocation $region))
        {
            $region = Invoke-Expression $command
        }
        UpdateEnvSetting "AllocationRegion" $region
        $global:AllocationRegion = $region
    }
}

function ValidateStreamAnalyticsJob()
{
    Param(
        [Parameter(Mandatory=$true,Position=0)] [string] $jobName,
        [Parameter(Mandatory=$true,Position=1)] [string] $resourceGroup,
        [Parameter(Mandatory=$true,Position=2)] [string] $jobDetails
    )
    $createJob = $true
    $statusMessage = "$(Get-Date –f $timeStampFormat) - Creating Stream Analytics job $jobName..."
    $jobDescription = $jobDetails |ConvertFrom-Json
    $jobResult = Execute-Command -Command ("Get-AzureStreamAnalyticsJob -Name $jobName -ResourceGroupName $resourceGroup") -ExpectedExceptionList @($global:resourceNotFound)
    if ($jobResult -ne $null)
    {
        # Check if job has correct query
        if ($jobResult.Properties.Transformation.Properties.Query -eq $jobDescription.Properties.Transformation.Properties.Query)
        {
            $createJob = $false
        }
        else
        {
            # Delete job that has old query
            $statusMessage = "$(Get-Date –f $timeStampFormat) - Updating Stream Analytics job $jobName..."
            $null = Execute-Command -Command ("Remove-AzureStreamAnalyticsJob -Name $jobName -ResourceGroupName $resourceGroup -Force")
        }
    }

    # Create job and start it if needed
    if ($createJob)
    {
        $tempFile = "{0}\out.json" -f $env:TEMP
        $jobDetails | Out-File $tempFile
        Write-Host $statusMessage
        $null = Execute-Command -Command ("New-AzureStreamAnalyticsJob -ResourceGroupName $resourceGroup -File $tempFile -Name $jobName")
        $null = Execute-Command -Command ("Start-AzureStreamAnalyticsJob -Name $jobName -ResourceGroupName $resourceGroup")
        Remove-Item $tempFile
    }
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

Add-Type @'
using System;
using System.Collections.Generic;
public class StringParser
{
    Dictionary<string, string> kvps;
    public StringParser(string input)
    {
        kvps = new Dictionary<string, string>();
        string[] parts = input.Split(';');
        foreach (string part in parts)
        {
            int keyEnd = part.IndexOf('=');
            if (keyEnd < 0)
            {
                throw new ArgumentException("Invalid pair");
            }
            kvps.Add(part.Substring(0, keyEnd), part.Substring(keyEnd + 1));
        }
    }

    public string GetValue(string key)
    {
        if (kvps.ContainsKey(key))
        {
            return kvps[key];
        }
        return string.Empty;
    }

    public List<string> GetKeys
    {
        get
        {
            return new List<string>(kvps.Keys);
        }
    }
}
'@

# Variable initialization
[int]$global:envSettingsChanges = 0;
$global:timeStampFormat = "o"
$global:resourceNotFound = "ResourceNotFound"
$global:serviceNameToken = "ServiceName"
$global:azurePath = Split-Path $MyInvocation.MyCommand.Path

# Add Servicebus dll before Azure powershell so we use latest version
add-type -path ("{0}\..\..\packages\WindowsAzure.ServiceBus.3.0.1\lib\net45-full\Microsoft.ServiceBus.dll" -f $global:azurePath)

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
        Write-Error -Category ObjectNotFound -Message "Unable to find Azure.psd1 modules. Please install Azure Powershell 2.5.1 or later"
        exit 1
    }
}
