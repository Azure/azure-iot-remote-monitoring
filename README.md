#Microsoft Azure IoT Suite 
You can deploy preconfigured solutions that implement common Internet of Things (IoT) scenarios to Microsoft Azure using your Azure subscrption. You can use preconfigured solutions: 
- as a starting point for your own IoT solution. 
- to learn about the most common patterns in IoT solution design and development. 

Each preconfigured solution implements a common IoT scenario and is a complete, end-to-end implementation. You can deploy the Azure IoT Suite remote monitoring preconfigured solution from [https://www.azureiotsuite.com](https://www.azureiotsuite.com), following the guidance outlined in this [document](https://azure.microsoft.com/en-us/documentation/articles/iot-suite-getstarted-preconfigured-solutions/). In addition, you can download the complete source code from this repository to customize and extend the solution to meet your specific requirements. 

##Remote Monitoring preconfigured solution
The remote monitoring preconfigured solution illustrates how you can perform end-to-end monitoring. It brings together key Azure IoT services to enable the following features: data ingestion, device identiy, command and control, rules and actions.

#### Wiki: 
* Do you want to know how to deploy this preconfigured solution locally and to the cloud? Take a look at our [Wiki](https://github.com/Azure/azure-iot-remote-monitoring/wiki) for answers.
* Have ideas for how we can improve Azure IoT? Give us [Feedback](http://feedback.azure.com/forums/321918-azure-iot).

##Contents of this repository
#### Web folder:
  * ASP.NET MVC 5 website containing user dashboard and device portal to manage IoT devices (add, remove, view, etc)

#### Infrastructure folder:
  * APIs and application logic to support telemetry and device operations
 
### Simulator folder:
  * Simulator (Azure Web Job) that simulates one or more devices that send data to the IoT Hub for testing and troubleshooting

### EventProcessor folder:
  * Azure Worker Role that hosts an Event Hub **EventProcessorHost** instance to handle the event data from the devices forwarding event data to other back-end services or to the remote monitoring site

### Visual Studio solution:
  * **RemoteMonitoring:** contains both the Dashboard web app, the EventProcessor worker role, and the Simulator worker role.

