# Manually setting up roles and assigning permissions in Azure Active Directory (AAD)

## Creating the Application
- Go to AzurePortal:  [https://manage.windowsazure.com](manage.windowsazure.com)
- Navigate the the Active Directory section and select the directory in which you created your solution.
- Go to ‘Applications’; in the ‘show’ drop-down, select “Applications my company owns”
- At the bottom of the page, click “Add”; select to “Add an application my organization is developing”
- Name the application and select the type: “Web application and/or web API”
- For Application properties enter the following:
	- Sign-on URL: `https://<yoursolutionname>.azurewebsites.net`
	- App ID URL: `https://<yoursolutionname>.azurewebsites.net/iotsuite`
- You will see a quick start indicating your app has been added

## Create Roles for the Application
- At the bottom of the page, select “manage manifest” -> “download manifest”
- This will download a .json file locally.  Open the file for editing (in the text editor of your choice).
- In the third line of the .json file, you will see:
	- `"appRoles" : [],`
	- Replace this with the following:
	
    "appRoles": [
    {
    "allowedMemberTypes": [
    "User"
    ],
    "description": "Administrator access to the application",
    "displayName": "Admin",
    "id": "a400a00b-f67c-42b7-ba9a-f73d8c67e433",
    "isEnabled": true,
    "value": "Admin"
    },
    {
    "allowedMemberTypes": [
    "User"
    ],
    "description": "Read only access to device information",
    "displayName": "Read Only",
    "id": "e5bbd0f5-128e-4362-9dd1-8f253c6082d7",
    "isEnabled": true,
    "value": "ReadOnly"
    } ],
- Save the updated .json file (it's ok to overwrite)
- Back in the Azure Management Portal, at the bottom of the page, select "manage manifest" -> "upload manifest"
- Upload the .json file you just saved

You have now created two roles for your application:  Admin and ReadOnly

## Assigning users to the roles

- In the Active Directory page for your application, go to "Users and Groups"
- Select yourself (or other listed user) and on the bottom of the page, select "Assign"
- This will prompt you with a drop down of the roles (Admin, ReadOnly)
- Choose the appropriate group for the user

You now have granted the selected user rights to your remote monitoring solution.  The ReadOnly group will be able to see the dashboard and the device list, but will not be allowed to add devices, change device attributes, or send commands.  Members of the Admin group will have full permissions to all the functionality of the solution.
