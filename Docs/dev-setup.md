# Set up development environment (Windows)

The prerequisites for setting up the remote monitoring preconfigured solution are: 
- Visual Studio 2013 Update 4
- Azure SDK 2.7.1 (You can download this version from [Azure SDK for .NET][azuresdkdownload] and select your version of Visual Studio) 
- Azure Powershell 1.0.3 (see [How to install and configure Azure PowerShell][powershell]) _Note: a reboot is required if you're updating PowerShell_

**Note:** You should verify that the NuGet Package Manager is configured correctly in Visual Studio before you continue:
 1. Launch Visual Studio 2013.
 2. Click **Tools**, and then click **Options**. In the **Options** dialog, click **NuGet Package Manager** and then click **Package Sources**.
 3. Make sure that **nuget.org** is selected in the list of available package sources, and then click **OK**.


[azuresdkdownload]: http://azure.microsoft.com/en-us/downloads/archive-net-downloads/
[powershell]: http://azure.microsoft.com/en-us/documentation/articles/powershell-install-configure/