# Add co administrators on your Azure subscription

Application access is derived from Application roles not Active Directory roles.  You must set it for each user except the original creator (they are assigned admin on creation) under the Application Role settings.

There are two defined roles, and one implicit role.
* Admin - Has full control to add, manage, and remove devices
* ReadOnly - Has ability to view devices
* Implicit ReadOnly - This is the same as ReadOnly, but is granted to all users of your active directory.  
   This was done for convenience during development. You can remove this role by modifying the [RolePermissions.cs](https://github.com/Azure/azure-iot-remote-monitoring/blob/master/DeviceAdministration/Web/Security/RolePermissions.cs)

Exact steps for changing roles (requires AAD Admin permissions):

1. Go to: <https://manage.windowsazure.com>  
2. Select Active Directory  
3. Click the name of your directory (it will highlight on hover over with mouse)  
4. Click Applications  
5. If you DON'T see your application in list, switch the show drop down to "Applications my company owns" and click the check mark  
6. Click the Name of the Application that matches your suite Name (it will highlight on hover over with mouse)  
7. Click Users  
8. Select the user you want to switch roles  
9. Click the Assign button and role (Admin) then click the check mark.  

For more information, see guidance [here](https://msdn.microsoft.com/en-us/library/azure/gg456328.aspx).