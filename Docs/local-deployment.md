# Local deployment and debugging

## Local deployment IoT services

The build.cmd script in the repository builds the solution code and also deploys the required IoT services to your Azure subscription. Local deployment creates the following:
* IotHub - S2
* DocumentDB - Standard
* Storage - Standard GRS
* Servicebus namespace/ 1 Eventhub - Basic throughput unit
* 3 Stream Analytics jobs

## Steps for local deployment
1. Use your Git client to pull the latest version of the solution from this repository. 
2. Open a **Developer Command Prompt for Visual Studio 2013 (or 2015) as an Administrator**
3. Navigate to the repository root directory. 
4. Run `build.cmd local` for an Azure cloud deployment. 

   For a national cloud deployment, run the same as above but include CloudName at the end (eg. `build.cmd local debug AzureGermanyCloud` or `build.cmd local release mydeployment AzureGermanyCloud`)

This command will:
* save account name, subscription, and deployment location into the local.config.user file
* create an Active Directory application in your directory and assign you as role administrator
* provision the resources and connect the Stream Analytics jobs to the IotHub, EventHub, and Storage account

In addition it will save connection strings, end points, etc. in the local.config.user file, which git is setup to ignore (*.user files).  When you start either the web site or the webjob project, the configuration these services need will be retrieved from this file as well.  

If when you run the build.cmd local if it fails to provision a required resource, please wait a few minutes and try running the command again.  You can select ‘U’ to use the same settings.

## Running End to End
5. Open the Remote Monitoring Solution in VS.
6. Click on Tools -> NuGet Package Manager -> Manage NuGet Packages for this Solution
7. If you see a banner at the top stating “Some NuGet packages are missing from this solution.  Click to restore from your online package sources.” Please click on the Restore button.
8. After the NuGet packages have been restored, click on Build -> Build Solution.
9. Open two other instances of the remote monitoring solution in VS.
10. In each separate instance right click on one of the following projects and select ‘Set as Startup Project’: Web, EventProcessor.WebJob and Simulator.WebJob
11. Run each project and you should now be able to see and debug locally the Remote Monitoring Solution.

While debugging a single project will allow you to see the code flow for that project, the entire solution will not function unless you have an instance of the Web, Simulator.WebJob, and EventProcessor.WebJob projects all started in separate instances of visual studio.
