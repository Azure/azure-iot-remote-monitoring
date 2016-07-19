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

   For a national cloud deployment, run the same as above but include CloudName at the end (eg. `build.cmd local debug AzureGermanCloud` or `build.cmd local release mydeployment AzureGermanCloud`)

   This command will:
   * save account name, subscription, connection strings, endpoints and deployment location into the local.config.user file (git is set up to ignore any *.user files)
   * create an Active Directory application in your directory and assign you as role administrator
   * provision the resources and connect the Stream Analytics jobs to the IotHub, EventHub, and Storage account
   
   If build.cmd local fails to provision a required resource, please wait a few minutes and try running the command again.  You can select ‘U’ to use the same settings.
   
   When you start either the web site or the webjob project, the configuration of these services will be retrieved from the local.config.user file.  

## Running End to End after Deployment
1. Open the Remote Monitoring Solution in Visual Studio.
2. Click on Tools -> NuGet Package Manager -> Manage NuGet Packages for this Solution
3. If you see a banner at the top stating “Some NuGet packages are missing from this solution.  Click to restore from your online package sources.”, click on the Restore button.
4. After the NuGet packages have been restored, click on Build -> Build Solution.
5. Open two other instances of the Remote Monitoring Solution in Visual Studio.
6. In each separate instance right click on one of the following projects and select "Set as Startup Project": Web, EventProcessor.WebJob and Simulator.WebJob
7. Run each project in a separate instance of Visual Studio and you should now be able to see and debug the Remote Monitoring Solution locally.

While debugging a single project will allow you to see the code flow for that project, the entire solution will not function unless you have an instance of the Web, Simulator.WebJob, and EventProcessor.WebJob projects all started in separate instances of visual studio. 

Note: you may be able to set [multiple start-up projects][lnk-multistartup] if you're using Visual Studio 2015 RC (this [fix][lnk-fix] was pushed May 5 2015). 

If you experience an endless log-in loop when launching the ‘Web’ project, please try using the “In-Private” browser setting or try a different browser.  You might also need to empty your browser cache.  Hopefully this will let you see the IoT Suite dashboard.

[lnk-multistartup]: https://msdn.microsoft.com/en-us/library/ms165413.aspx
[lnk-fix]: https://github.com/aspnet/Tooling/issues/10
