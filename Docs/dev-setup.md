# Set up development environment (Windows)

The prerequisites for setting up the remote monitoring preconfigured solution are: 
- Visual Studio 2013 Update 4 or Visual Studio 2015
 - If you are using Visual Studio 2013, also download [Microsoft .NET Framework 4.6.1 Developer Pack and Language Packs][.NET 4.6.1].
- Azure Powershell 2.0.0 or greater (see [How to install and configure Azure PowerShell][azurepowershell]) _Note: a reboot is required if you're installing or updating PowerShell_
- [Windows PowerShell] [windowspowershell] version 4.0 or **higher**. To check for PowerShell version open PowerShell command window and call $PSVersionTable. It will disply the numeric information about the PowerShell version

**Note:** You should verify that the NuGet Package Manager is configured correctly in Visual Studio before you continue:
 1. Launch Visual Studio 2013 or Visual Studio 2015.
 2. Click **Tools**, and then click **Options**. In the **Options** dialog, click **NuGet Package Manager** and then click **Package Sources**.
 3. Make sure that **nuget.org** is selected in the list of available package sources, and then click **OK**.

[.NET 4.6.1]: https://www.microsoft.com/download/details.aspx?id=49978
[azurepowershell]: http://azure.microsoft.com/en-us/documentation/articles/powershell-install-configure/
[windowspowershell]:https://www.microsoft.com/en-us/download/details.aspx?id=40855
