#Microsoft Azure IoT Suite 
##Remote Monitoring preconfgured solution


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
