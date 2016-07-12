#Microsoft Azure IoT Suite 
You can deploy preconfigured solutions that implement common Internet of Things (IoT) scenarios to Microsoft Azure using your Azure subscription. You can use preconfigured solutions: 
- as a starting point for your own IoT solution. 
- to learn about the most common patterns in IoT solution design and development. 

Each preconfigured solution implements a common IoT scenario and is a complete, end-to-end implementation. You can deploy the Azure IoT Suite remote monitoring preconfigured solution from [https://www.azureiotsuite.com](https://www.azureiotsuite.com), following the guidance outlined in this [document](https://azure.microsoft.com/en-us/documentation/articles/iot-suite-getstarted-preconfigured-solutions/). In addition, you can download the complete source code from this repository to customize and extend the solution to meet your specific requirements. 

##Remote Monitoring preconfigured solution
The remote monitoring preconfigured solution illustrates how you can perform end-to-end monitoring. It brings together key Azure IoT services to enable the following features: data ingestion, device identiy, command and control, rules and actions.

##Contents of this repository

### Docs folder:
  * [Set up development environment (Windows)](Docs/dev-setup.md) outlines the prerequisites for deploying the remote monitoring preconfigured solution.
  * [Local deployment and debugging](Docs/local-deployment.md) describes how to deploy locally and basic debugging.
  * [Cloud deployment](Docs/cloud-deployment.md) describes building and deploying the remote monitoring preconfigured solution fully on Azure.
  * [Configuring Azure IoT Suite preconfigured solutions for demo purposes](Docs/configure-preconfigured-demo.md) walks you through changing the footprint of the underlying Azure services for your solution.

Other useful [IoT Suite documentation](https://azure.microsoft.com/documentation/suites/iot-suite/):
  * [Frequently asked questions for IoT Suite](https://azure.microsoft.com/documentation/articles/iot-suite-faq/)
  * [Permissions on the azureiotsuite.com site](https://azure.microsoft.com/documentation/articles/iot-suite-permissions/). This includes instructions for adding co-administrators to your preconfigured solution.
  
### EventProcessor folder:
  * Azure Worker Role that hosts an Event Hub **EventProcessorHost** instance to handle the event data from the devices forwarding event data to other back-end services or to the remote monitoring site

### Visual Studio solution:
  * **RemoteMonitoring:** contains the source code for the complete preconfigured solution, including the solution portal web app, the EventProcessor web job, and the Simulator web job.
  
## Feedback

Have ideas for how we can improve Azure IoT? Give us [Feedback](http://feedback.azure.com/forums/321918-azure-iot).

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.