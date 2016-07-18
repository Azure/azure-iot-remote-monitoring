# Cloud deployment

## Cloud deployment IoT services

The build.cmd script in the repository builds the solution code and also deploys the required IoT services to your Azure subscription. Cloud deployment creates the following:
* 1 x IotHub - S2
* 1 x DocumentDB - Standard
* 1 x Storage - Standard GRS
* 1 x Servicebus namespace - basic
* 2 x Eventhub
* 3 x Stream Analytics jobs - standard
* 4 x Website - standard
* 1 x Bing Maps api

## Steps for cloud deployment

1. Use your Git client to pull the latest version of the solution from this repository. 
2. Open a **Developer Command Prompt for VS2013 as an Administrator**. 
3. Navigate to the repository root directory. 
4. Run `build.cmd cloud [debug | release] <deploymentname>` for an Azure cloud deployment. 

   For a national cloud deployment, run the same as above but include CloudName at the end (eg. `build.cmd cloud debug AzureGermanCloud` or `build.cmd cloud release mydeployment AzureGermanCloud`)


This command will:
* save account name, subscription, and deployment location into the <serviceName>.config.user file
* create an Active Directory application in your directory and assign you as role administrator
* provision the resources and connect the Stream Analytics jobs to the IotHub, EventHub, and Storage account
* build and upload the current repo code as a zip package to the storage account
* deploy 2 instances of the website and 2 instances of the webjobs
