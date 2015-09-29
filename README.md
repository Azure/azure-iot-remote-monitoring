#Microsoft Azure IoT Solution

##Contents of this repository

* DeviceManagement folder:
  * ASP.NET MVC 5 website to manage IoT devices (add, remove, view, etc)
  * Web API 2 REST API to support device management
* Simulator folder:
  * Simulator (Azure Worker Role) that simulates one or more devices
    that send data to the IoT Hub for testing and troubleshooting
* EventProcessor folder:
  * Azure Worker Role that hosts an Event Hub **EventProcessorHost** instance to
    handle the event data from the devices forwarding event data to other
    back-end services or to the Device Management site

* Visual Studio solution:
  * **IoTRefImplementation:** contains both the DeviceManagement web app, the EventProcessor worker role, and the Simulator worker role.

## Set up the solution
For information about how to set up, deploy, and run the sample solution, see the document [Asset Monitoring Sample Solution Walkthrough](https://github.com/Azure/azure-iot-solution/blob/master/Docs/iot-asset-monitoring-sample-walkthrough.md) in this repository.

## Customize the solution
For information about how to modify and customize the sample solution, see the document [Customizing the Asset Monitoring sample](https://github.com/Azure/azure-iot-solution/blob/master/Docs/iot-asset-monitoring-customization.md) in this repository.
