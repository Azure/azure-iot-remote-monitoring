function SelectSubscriptionId()
{
    $subsId = "not set"
    $subscriptions = Get-AzureRMSubscription
    Write-Host "Available subscriptions:"
    $global:index = 0
    $selectedIndex = -1
    Write-Host ($subscriptions | Format-Table -Property @{name="Option";expression={$global:index;$global:index+=1}},SubscriptionName, SubscriptionId -au | Out-String)
    while (!$subscriptions.SubscriptionId.Contains($subsId))
    {   
        try
        {
            [int]$selectedIndex = Read-Host "`nSelect an option from the above list"
        }
        catch
        {
            Write-Host "Must be a number"
            continue
        }
    
        if ($selectedIndex -lt 1 -or $selectedIndex -gt $subscriptions.length)
        {
            continue
        }

        $subsId = $subscriptions[$selectedIndex - 1].SubscriptionId
        return $subsId
    }

}

function SelectSolution()
{
    $resultName = "not set"
    $solutions = Find-AzureRmResourceGroup -Tag @{IotSuiteType = "RemoteMonitoring"}
    if(!$solutions)
    {
        Write-Host "No available solutions!"
        Exit
    }
    Write-Host "Available solutions:"
    $global:index = 0
    $selectedIndex = -1
    Write-Host ($solutions | Format-Table -Property @{name="Option";expression={$global:index;$global:index+=1}},name, id -au | Out-String)
    while (!$solutions.name.Contains($resultName))
    {   
        try
        {
            [int]$selectedIndex = Read-Host "`nSelect an option from the above list"
        }
        catch
        {
            Write-Host "Must be a number"
            continue
        }
        if ($selectedIndex -lt 1 -or $selectedIndex -gt ([Array]$solutions).Length)
        {
            continue
        }

        $resultName = $solutions[$selectedIndex - 1].name
        return $resultName
    }

}

function SelectEnvironmentName()
{
    $resultName = "not set"
    $environments = Get-AzureEnvironment
    Write-Host "Available environment:"
    $global:index = 0
    $selectedIndex = -1
    Write-Host ($environments | Format-Table -Property @{name="Option";expression={$global:index;$global:index+=1}}, Name | Out-String)
    while (!$environments.Contains($resultName))
    {   
        try
        {
            [int]$selectedIndex = Read-Host "`nSelect an option from the above list"
        }
        catch
        {
            Write-Host "Must be a number"
            continue
        }
    
        if ($selectedIndex -lt 1 -or $selectedIndex -gt $environments.length)
        {
            continue
        }

        $resultName = $environments[$selectedIndex - 1]
        return $resultName.Name
    }

}

$azureEnvironmentName = SelectEnvironmentName
Add-AzureRmAccount -EnvironmentName $azureEnvironmentName
$subscriptionId = SelectSubscriptionId
select-AzureRmSubscription -SubscriptionId $subscriptionId
$resourceGroupName = SelectSolution
$saContext = (Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -Name $resourceGroupName).Context
$table = Get-AzureStorageTable -Name "NameCacheList" -Context $saContext
Get-AzureStorageTableRowAll -table $table | Remove-AzureStorageTableRow -table $table
write-Output "clean all records successfully!"