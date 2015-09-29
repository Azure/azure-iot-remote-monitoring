# Customizing the Asset Monitoring sample

## Introduction

The Asset Monitoring IoT Suite sample illustrates how you can perform end-to-end monitoring in a business scenario based on operating multiple vending machines in remote locations. This document describes how you can modify the sample to support a different business context.

This document provides a brief architectural overview of the sample before describing how you might modify it.

## Device Management web app and Device Management RESTful API
This section provides an overview the Device Management web app included in this solution.

### Architectural Overview
Moving up through the layers from the bottom up in the **DeviceManagement.Infrastructure** and **DeviceManagement.Web** projects:
* The _Repository_ layer contains components that communicate with the underlying Azure services and systems such as DocumentDB and IoT Hubs.
* The _BusinessLogic_ layer contains validation and other business logic. It sits atop the _Repository_ layer. This layer throws exceptions when errors arise.
* The _Models_ layer contains basic data transport objects. (These objects are used by all the layers.)
* The _MVC_ layer sits atop the _BusinessLogic_ layer, and provides the web UI for device management.
  - _ViewModels_ in the MVC layer wrap and extend _Models_ from the _Models_ layer to provide UI specific functionality.
* The _REST_ layer sits on top of the _BusinessLogic_ layer, and exposes the device management functionality as a RESTful API. This layer returns errors as part of the serialized **ServiceResponse** object.

### Authentication
* The website and REST API are secured with *OAuth2* using Azure Active Directory.
  - Users must be defined in your Azure Active Directory
  - Roles must be defined as Azure Active Directory Roles
  - The users must be assigned to the roles
* Once a user is authenticated with the website, the security cookies from the website will grant the same level of access to the REST API. (This way JavaScript code running on the client-side of the website can call into the REST API.)
* Access to the REST API is also available to external clients that successfully authenticate and provide a valid Bearer token in the Authorization header.

## IoT Device Simulator
This section provides an overview the device simulator included in this solution.

The simulator is hosted in an Azure worker role and simulates one or more IoT devices that send data to the cloud-based IoT system. Currently the simulator is configured in code — see details below.

### Important classes
* **DeviceBase**: an instance represents a single IoT device. It holds a list of **ITelemetry** instances to send.
* **ITelemetry**: an interface that represents one or more events that a device can send.
  - **StaticTelemetry** (implements **ITelemetry** interface): use this class to send the same static data a given number of times.

  - **DynamicTelemetryBase** (implements **ITelemetry** interface): an abstract base class for dynamic events. Inherit from this class to implement custom logic and to send any type of random or other programmatically-created events.

## Common Modifications

This section offers suggestions on how to extend or customize the sample.

### Website/REST API/Business Logic

The **DeviceManagement.Web** project contains the Device Management web app, the defintion of the REST API for managing devices, and the business logic.

#### Change device data model
This is likely to be the most common change to make to the system.

The core device data model is stored in the Models folder in the **Common** project in the Device.cs and DeviceConfig.cs files. The **Device** class is the root of the device data model. See the Device.cs file for details of the related classes that make up the device data model. These classes are all strongly-typed C# classes.

To make a change to the device data model:

* Edit the **Device** class or related classes to add, remove, or rename properties. These properties are mapped to JSON using rules defined in the Startup.Json.cs file in the **DeviceManagement.Web** project.
* Update the views and business logic to support the updated data model.
* Compile-time checking can help to identify and resolve issues.

The most likely changes to make to the device data model are to the **SystemProperties** and **DeviceConfig** classes.

For example, to add a new property such as **BatteryManufacturer** you should complete the following steps:

* Add the new property to the **SystemProperties** class in the Device.cs file:
```
public string BatteryManufacturer { get; set; }
```

* Add new localized strings for the label and any validation message to the Strings.resx file in the App_GlobalResources folder in the **DeviceManagement.Web** project:
```
"BatteryManufacturerLabel", "Battery Manufacturer"
"BatteryManufacturerMustBeLessThan200Characters", "Battery Manufacturer must be less than 200 characters."
```

* If you want to be able to edit this property in the UI, add it to the RegisteredDeviceModel.cs file in the **DeviceManagement.Web** project:
```
[StringLength(MaxLength, ErrorMessageResourceType = typeof(Strings),
  ErrorMessageResourceName = "BatteryManufacturerMustBeLessThan200Characters")]
public string BatteryManufacturer { get; set; }
```

* Add the new property to the device details pane display in the \_DeviceDetailsSystemProperties.cshtml file:
```
<h4 class="grid_subhead_detail_label">@Strings.BatteryManufacturerLabel</h4>
<p class="grid_detail_value">@Model.DeviceProperties.BatteryManufacturer</p>
```

* If you want to be able to edit this property in the UI, add the new property to the edit system properties view in the EditSystemProperties.cshtml file in the **DeviceManagement.Web** project:
```
@Html.LabelFor(m => m.BatteryManufacturer, @Resources.Strings.BatteryManufacturerLabel)
@Html.EditorFor(model => model.BatteryManufacturer)
@Html.ValidationMessageFor(model => model.BatteryManufacturer)
```

* Update code in the relevant controllers in the **DeviceManagement.Web** project to copy the data. In the DeviceController.cs file in the **GetDeviceDetails** action, add the following code:
```
BatteryManufacturer = device.SystemProperties.BatteryManufacturer,
```
In the **Edit** action, add the following code:
```
device.SystemProperties.BatteryManufacturer = registeredDeviceModel.BatteryManufacturer;
```
In the **EditMetadata** action, add the following code:
```
BatteryManufacturer = device.SystemProperties.BatteryManufacturer,
```

* You can now build and run the solution, and test the results.

#### Edit device types
The device types shown in the first step of add device wizard are currently stored in the **SampleDeviceTypeRepository** class in the **DeviceManagement.Infrastructure** project. You can add or remove Device types here, or you could store the data in an external system and then adapt or replace this class to load device type data from that external system.

*Note: Currently these device types do not impact the behavior of the solution in a significant way — they are only used to display a hyperlink to the setup instructions for that type of device.*

#### Change a command name
The following three commands are called by the controllers in the Device Management web app, and therefore their names are stored in the code:
* `ChangeKey`
* `ChangeConfig`
* `ChangeSystemProperties`

These values are stored in the **SystemCommands** class in the **Common** project. If you need to change the name of one of these commands, change it in this class.

All other commands are data-driven based on the data in the device data model.

#### Change app roles
The web app defines the following permissions in the **Permission** enum in the **DeviceManagement.Web** project:
* ViewDevices
* EditDeviceMetadata
* AddDevices
* RemoveDevices
* DisableEnableDevices
* SendCommandToDevices
* ViewDeviceSecurityKeys
* UpdateDeviceSecurityKeys

The web app defines the following application roles in the **RolePermissions** class in the **DeviceManagement.Web** project:
* ReadOnly
* Contributor
* DeviceManager
* Admin

These roles are associated with the permissions in the **DefineRoles** method in the **RolePermissions** class.

For flexibility, the application implements security at the level of permissions. This makes it easy to change the roles and their associated permissions.

For example, the following code in the **RolePermissions** class grants the AddDevices permission to both the DeviceManager and Admin roles:

```
AssignRolesToPermission(Permission.AddDevices,
  DEVICE_MANAGER_ROLE_NAME,
  ADMIN_ROLE_NAME);
```
To remove the AddDevices permission from the DeviceManager role, change the code to the following:
```
AssignRolesToPermission(Permission.AddDevices,
  ADMIN_ROLE_NAME);
```

#### Create a new app role
First, you must add the role to the JSON manifest in the Azure portal for Active Directory:
* Navigate to the Azure portal.
* Select the Azure Active Directory instance associated with the Device Management web app.
* Click **APPLICATIONS** in the navigation at the top.
* Click on the relevant application for the Device Management web app.
* Click **Manage Manifest**, and then **Download Manifest**.
* Click **Download Manifest**.
* Add the following JSON to the existing appRoles section to add the new **DeviceDeleter** role:
```
{
  "allowedMemberTypes": [
    "User"
  ],
  "description": "Access to remove devices only",
  "displayName": "Device Deleter",
  "id": "c400a00b-f67c-42b7-ba9a-f4fd8c67e433",
  "isEnabled": true,
  "origin": "Application",
  "value": "DeviceDeleter"
},
```
* Make sure that the "id" value is a new GUID.
* Upload the modified manifest to the Azure portal.
* Add the role to the **RolePermissions** class by defining the role:
```
private const string DEVICE_DELETER_ROLE_NAME = "DeviceDeleter";
```
* Add the role to the **\_allRoles** list in the constructor.
* Add the role to each permission it should be associated with in the **DefineRoles** method.
* Assign the role to users using the Azure portal.

### Simulator

#### Add device data to the app.config file for specific devices

Typically, the first customization step is to add specific DeviceID and key values in the device Simulator:
* Open the app.config file for the simulator in the  **Simulator.WorkerRole** project.
* Add the desired DeviceID and key to the &lt;applicationSettings&gt; section. The format for each device is:
  ```
<string>DeviceID,Key</string>
  ```

#### Change the device data model

Changing the device data model for the Simulator is very similar to changing it for the Device Manager web app. In general:
* In the **Common** project, edit the Device.cs and  DeviceConfig.cs classes to add, remove, or change properties.
* Make any related changes necessary in the business layer and repository  layers.

#### Configure telemetry events

The Simulator offers plenty of flexibility around the telemetry events it sends to the cloud.

To configure telemetry events:
* Create an **ITelemetryFactory** implementation (see the  **VendingMachineTelemetryFactory** class for an example). This class populates the **TelemetryEvents** list on a device with **ITelemetry** instances (static or dynamic) which then emit telemetry events.
* Add code in the **PopulateDeviceWithTelemetryEvents** method to create static and/or dynamic telemetry objects. See below for more information.
* **ITelemetry** implementations added to the **TelemetryEvents** list are run one at a time, starting with the first instance in the list.

#### Adding static telemetry events

Static telemetry events are pre-configured events that are sent a specified number of times. You do not need to write much code, but these events offer little flexibility. You should use static events when you do not need randomness or logic when the device sends the events.

* In the **PopulateDeviceWithTelemetryEvents** method in your **ITelemetryFactory** implementation, create a new **StaticTelemetry** instance, passing in an **ILogger** instance for logging.
* Assign values to the properties of the instance to control the behavior:
  * Assign **true** to the **RepeatForever** property if the device should continue sending this event forever, or **false** if the device should go on to the next **ITelemetry** object when it completes sending this event.
  * Assign a number to **RepeatCount** to determine how many times the event should be sent. For example, **5** if you want the simulator to send the event five times.
  * Assign a timespan to **DelayBefore** to set the delay before each event is sent.
  * Assign a string to the **MessageBody** that is sent as the message body.
* Add this instance to the **TelemetryEvents** list on the device.
* Add more static or dynamic telemetry objects as required by your scenario.

#### Add dynamic telemetry events

Dynamic telemetry events are code-based, flexible modules for event generation. They require more code than static telemetry events, but offer greater flexibility. You should use dynamic events when you events require randomness, state-based logic, complex time variation, or other complex situations where a static event isn't powerful or flexible enough.

* Create a new class to represent the dynamic event module. The new class should either inherit from **DynamicTelemetryBase** or implement **ITelemetry**. For examples, see the **StartupTelemetry** or **PresenceTelemetry** classes in the  **Simulator.WorkerRole** project.
* Implement logic in the **SendEventsAsync** method. Typically, this logic should send one or more events, and then wait for a specified time before sending the next event. Note that this method may keep sending events forever (as in the **PresenceTelemetry** class), or it may send events for a while and then stop (the **StartupTelemetry** class sends just a single event).
* In the **PopulateDeviceWithTelemetryEvents** method, create a new instance of the dynamic class, and add it to the device's **TelemetryEvents** list.

#### Add command processing support

Command processing for the simulated device is done using the "Chain of Responsibility" design pattern. Users can implement specific command processor modules by inheriting from the **CommandProcessor** class, and then add these modules to a list. When the device receives a command, it passes it to the first command processor in the list. For this, and all subsequent command processors:
* If that processor can handle the command, then it does *not* call the next processor in the chain.
* If that processor cannot handle the command, it then calls the next processor in the chain (if there is one!).

*Warning: At the end of the chain, the command will "fall off" the end of the chain if none of the processors could handle it. If this is unwanted behavior, please create a simple command processor that throws an exception or logs to an **ILogger** instance and place it at the end of the chain.*

To implement a new **CommandProcessor** type:
* Create a new class which inherits from the **CommandProcessor** class.
* Implement the **HandleCommand** method (see the **VendingMachinePriceChangeCommand** class in the **Simulator.WorkerRole** project for an example implementation).
  * Add logic to determine if the command processor can handle the command.
  * If it can handle it, then handle it. If events need to be sent, send them here.
  * If it cannot handle the command, then check if there is a next command processor in the chain, and if so, call the **HandleCommand** method on the next processor. You could also add logic here to alert you if there isn't a next command processor available.
* Add your new command processor to the chain of processors in the **InitCommandProcessors** method in the **VendingMachineDevice** class.

*Note: Pay special attention to error handling. If an unhandled error occurs, then no other command processors will be called for this command. Review how the example command processor implements error handling; it can be tricky to handle errors inside an async method.*

#### Automate simulator setup (such as deviceId, hub hostname and keys)
There are many possible ways to provision the simulator instances with deviceId values and keys. For example, in load testing, users might want to create large numbers of devices.

The **IVirtualDeviceStorage** in the **Common** project is an interface that defines how the simulators can discover device information such as deviceId and keys. The interface includes two methods to retrieve device information: the **GetDeviceListAsync** method returns a list of **InitialDeviceConfig** objects, and the **GetDeviceAsync** methods retrives a single **InitialDeviceConfig** object.

The sample provides two example implementations in the **AppConfigRepository** and **VirtualDeviceTableStorage** classes in the **Simulator.WorkerRole** project. The **AppConfigRepository** class pulls device data from the app.config file and the **VirtualDeviceTableStorage** class pulls device data from Azure table storage. You could replace these implementations with code to pull data from an alternative data source that suits your specific requirements.

The instance of **IVirtualDeviceStorage** is passed into the **BulkDeviceTester** class in the WorkerRole.cs file as one of a number of dependencies.

#### Modify transport

The transport of events and commands is abstracted by the **ITransport** interface in the **Simulator.WorkerRole** project. This interface defines two methods to send events to the IoT Hub, and one method to receive events from the hub.

To use the simulator with a different hub than the Azure IoT Hub, implement the **ITransport** interface and inject it into the **BulkDeviceTester** class in the WorkerRole.cs file.

#### Modify serialization

By default, the simulator uses JSON serialization. This is abstracted by the **ISerialize** interface in the **Simulator.WorkerRole** project. This interface has a method to serialize a .NET object into a byte array, and a method to deserialize a byte array into a .NET object of a specified type **T**.

The **JsonSerialize** class in the **Simulator.WorkerRole** project is the JSON implementation of **ISerialize**.

If a different serialization is desired, implement the **ISerialize** interface for the desired serialization format. Note that if the format is not supported by the hub, you may need to use a custom gateway to convert the device's format into a format the hub can process.

### Event Processor

Most customizations of the event processor are done by changing the device data model and business, service, and repository layers used by the Device Management web app.

#### Add an additional event type

TBD

## Sharing an IoT Hub in a Team: Developer Isolation

There are two types of collisions that occur when working on a team and sharing an IoT Hub:
* Having multiple copies of the same virtual device running in multiple simulator instances
* Having messages from simulated devices processed by a different user's event processor instance (and vice versa)

In order to support team development, IoT Hubs supports a feature called _developer isolation_. This includes:
* The ability to specify a custom Azure table name for simulated device storage for each team member so that each developer has their own
  isolated list of simulated devices. This is accomplished by entering a user-specific Azure Storage table name for the **DeviceTableName** setting in the solution configuration file.
* The ability to specify a distinct **ObjectTypePrefix** for each team member that is used by the simulator to add routing information to messages. By
  manually configuring separate Azure Stream Analytics (ASA) jobs and event hubs for each team member, the message stream into a given
  event processor can be filtered to only those messages created by their simulated devices. This is accomplished by entering a user-specific prefix, such as **Sue-**,
  for the **ObjectTypePrefix** setting, and using additional Azure components to implement the filtering.

For convenience, both of these settings reside in the local.config.user file.

### Local deployments and Cloud Deployments

The developer isolation feature is primarily intended for use in local development and testing. Running the `build.cmd local` command will default to the values for a non-isolated configuration:
* **DeviceTableName** is "DeviceList"
* **ObjectTypePrefix** is empty

The recommended dev isolation setup workflow is to run the "build.cmd local" command to configure the cloud infrastructure and create a local.user.config file, and
then manually edit separate copies of this file for each team member to configure developer isolation. Much of the setup work involves setting up the new Azure components to implement
the filtering which you must configure manually.

If for some reason you wish to run a cloud deployment in developer isolation mode, this is possible by using the standard `build.cmd cloud` command, and then
editing the configuration for each of the cloud services in the Azure Portal. (Note that the **EventProcessor** cloud service has two
configuration sections, one for the device admin web role and one for the event processor worker role. You must update both of these configuration sections, as well as the Simulator cloud service.)

### Limitation on ASA jobs and developer isolation team size

Due to the limitation of 5 ASA jobs for a given IoT Hub's Event Hub, this technique for isolating simulator messages works well for smaller teams (4 or less).

There is no limit on the number of DeviceTableNames that the system can support, so any number of developers can have their own isolated simulated device lists.

If you have 5 developers, you might consider removing the default TelemetryToBlob ASA job if it is not needed for development or testing, to free up one more ASA slot.

If you have more than 5 developers in a team (or you have additional ASA jobs you need to run in development), we recommend that you isolate those developers who would benefit the most from simulator message isolation (such as developers who are working on changes to the simulator and event processor), and have the other developers work in a shared developer isolation slot. In this case, the team will need to coordinate allocation of developer isolation slots.

### Setup of developer isolation configuration

#### Overview of setup

At a high level, to setup developer isolation you need to configure the following Azure components:
* A custom Azure Stream Analytics (ASA) job (one per team member) to filter the events for that user. This ASA job has:
  - Its input connected to the Event Hub that is part of the IoT Hub.
  - A query which only passes events with that specific developer's object type prefixes.
  - Its output connected to a separate Event Hub for that team member.
* An Event Hub (one per team member) connected to the output of the ASA job to queue up the events for that user for the event processor.
* Configure the event processor for that user to listen to the Event Hub connected to the ASA output, to *process the events for that user*

In addition, by using the **DeviceTableName** setting in the local.config.user file, each user will read and write to an isolated list of their own simulated devices.

#### Naming and sharing

You will need a naming scheme for simulator tables, object type prefixes, ASA jobs, and event hubs. As there will be a few nodes for each team member,
we recommend using developer names or initials for ease of maintenance for smaller teams. For example, for a team member named "Pat", consider:
* **DeviceTableName** could be "DeviceListPat"
* **ObjectTypePrefix** could be "Pat-"
* ASA job name could be "patfilter"
* ASA output alias could be "patoutput"
* Event hub for ASA output could be "pateventhub"

For larger teams (more than 4 or 5) we recommend more generic naming if the message isolation slots will be traded around, such as
"Slot1", "Slot2", "Slot3", and "SharedSlot". (Then developers and testers who are not changing message formats could all
share the "SharedSlot", and team members changing or testing message format changes could use "Slot1", "Slot2", etc.)

Either way, we recommend creating all the local.config.user files on a shared team site (with names such as "local.config.pat.user"),
so that users can easily get a new clean version.

#### Step by step developer isolation setup

* Create a starting cloud configuration and local.config.user file by running `build.cmd local`.
* Create a copy of the local.config.user file for each team member, and manually edit the **DeviceTableName** and **ObjectTypePrefix** to customize it for them (for example, "DeviceListPat" and "Pat-").
* Create a service bus namespace for the event hubs that will listen to the ASA job output:
  * In the https://manage.windowsazure.com [Azure Portal](https://manage.windowsazure.com), click the **SERVICE BUS** node on the left navigation.
  * At the bottom, click **+ CREATE**.
  * Create a new namespace name (such as "teamfiltering"), with the following options:
    * **TYPE** should be "MESSAGING".
    * **MESSAGING TIER** should be "STANDARD"
  * Wait for the new namespace to be created, and then click the namespace name, and then click **EVENT HUBS** on the top navigation.
  * For each team member, create and configure a new event hub to listen to ASA output as follows:
    * Click **+ NEW** at the bottom of the view, then click **Quick Create** to create an event hub with default values (using a name such as "pateventhub").
    * When it has been created, click the new event hub name.
    * Click **CONFIGURE** in the top navigation.
    * Add a new shared access policy called "listen", and associate it with the "LISTEN" permission, then click **SAVE** at the bottom.
* For each developer, manually create an ASA job in the Azure Portal:
  * Click the **Stream Analytics** node on the left navigation
  * Click **+ NEW** at the bottom, and enter the following:
    * JobName: specific to that developer (such as "patfilter")
    * Select an appropriate Region
    * Regional Monitoring Storage Account can be set to a new or existing one.
* Click on the ASA job and configure it as follows:
  * Create an input connected to the IoT Hub's Event Hub as follows:
    * Click **INPUTS** on the top navigation of the ASA job
    * Click **ADD AN INPUT**
    * Choose **Data Stream** on step 1
    * Choose **Event Hub** on step 2
    * On step 3:
      - Create a name for the **Input Alias** (by default, enter "IoTEventStream")
      - For the private preview, choose **Use Event Hub from Another Subscription** for the **Subscription** value
      - For **Service Bus Namespace**, enter the namespace from the Event Hub connection string for the IoT Hub. For example, if
        the connection string supplied by Microsoft is
        "Endpoint=sb://partnersbnamespace4.servicebus.windows.net/;SharedAccessKeyName=listen;SharedAccessKey=/7veZVkdfka3984gDFKDKFDA7GERh01SIsDDKDKFnOwz+Y=",
        then the "Service Bus Namespace is the first part of the endpoint after the "sb://" and before the first dot; in
        this example, "partnersbnamespace4".
      - Enter the IoT Hub's Event Hub name supplied by Microsoft for **Event Hub Name**
      - Enter "listen" for the **Event Hub Policy Name** (or whatever the **SharedAccessKeyName** value from the event hub connections string is)
      - Enter the event hub policy key (this is the value of the **SharedAccessKey** in the event hub connection string)
      - Leave the **Event Hub Consumer Group** empty
    * On step 4:
      - Leave **Event Serialization Format** as "JSON"
      - Leave **Encoding** as "UTF-8"
    * Click the big back button to exit the input view
  * Create the query as follows:
    * Click the **QUERY** top navigation item
    * Enter a query of the following form, where "IoTEventStream" is the name of the input alias for the ASA job,
      and "Pat-" is the **ObjectTypePrefix** for this team member (all the **ObjectType** values from this user's simulated devices
	  will be prefixed with this):
    ````
      SELECT
        *
      FROM
        IoTEventStream
      WHERE
        ObjectType = 'Pat-DeviceInfo-Simulated' OR
        ObjectType = 'Pat-DeviceInfo-HW' OR
        ObjectType = 'Pat-CommandResponse'
    ````
    * Click **SAVE** at the bottom to save the query
    * Click the big back button to exit the query view
  * Create an output as follows:
    * Click the **OUTPUTS** top navigation item
    * Click **ADD AN OUTPUT**
    * Choose **Event Hub** on step 1
    * On step 2:
      - Enter a name of the **Output Alias** ("patoutput")
      - For **Subscription**, choose **Use Event Hub from Current Subscription**
      - **Choose a namespace** should be the namespace for the event hub created above for this team member for ASA job output
      - **Choose an eventhub** should be the event hub created above for this team member's ASA job output ("pateventhub")
      - **Event hub policy name** should be "RootManageSharedAccessKey". (Note that this cannot be "listen", as this is sending
        messages to the event hub)
    * On step 3, accept default values for serialization format and encoding
  * Start the ASA job by clicking the **START** button at the bottom
* Configure the EventProcessor to listen to the event hub connected to the ASA output by editing the local.config.user file as follows:
  * In the Azure Portal, click the **SERVICE BUS** node on the left navigation
  * Click the namespace name created above for the ASA output event hubs
  * Click **EVENT HUBS** on the top navigation
  * Click the name of the event hub for this team member
  * Click **View Connection String** on the right ("quick glance")
  * Copy the connection string value for the "listen" row, and paste it into the local.config.user file for the **ServiceSBConnectionString** value
  * Copy the namespace from the first part of the connection string (as above), and paste into the local.config.user **ServiceSBName** value
  * Enter the name of the event hub into the local.config.user **ServiceEHName** value (like "pateventhub")

At this point, you should be able to launch the EventProcessor and Simulator in the local Visual Studio, create a new simulated device,
and have its messages always routed to your EventProcessor.

#### Troubleshooting

If something goes wrong, a common issue is that events do not arrive at the event processor. In this case, please review the operational
logs for the relevant ASA job for more information.
