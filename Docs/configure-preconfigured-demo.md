# Configuring Azure IoT Suite preconfigured solutions for demo purposes

We’ve introduced the [Microsoft Azure IoT Suite] to jump start your IoT project. We created the Azure IoT Suite preconfigured solutions to get you started quickly and to realize business value from common IoT scenarios. The two preconfigured solutions available today are remote monitoring and predictive maintenance.

Each preconfigured solution exemplifies key patterns and practices of an end-to-end IoT scenario. To this end, the services that are pre provisioned in preconfigured solutions can be leveraged to meet the typical needs of IoT projects.

For demos and small proof-of-concepts, we recognize the pre-provisioned scalable architecture is not always necessary. To address this, this article walks through the steps to change the footprint of the underlying Azure services to enable you to do demos and to develop small PoCs at minimal cost.

> If you’re looking for a walkthrough on how to provision a remote monitoring preconfigured solution, see [Tutorial: Get started with the IoT preconfigured solutions].

Assuming you’ve provisioned a remote monitoring preconfigured solution, navigate to the resource group for that solution in the [Azure Portal]. The resource group has the same name as the solution name you specified when you provisioned your solution. In your resource group, you can see all the pre-provisioned Azure resources for your solution (with the exception of your AAD application that can be found in the [Azure Classic Portal]):

The **Resource Group** by default shows 9 different Azure services, to see the full list, click the "…" in the lower right corner of **Resources**:

-   Bing Maps API (internal1 – free tier)
-   IoT Hub (S2 – Standard tier)
-   DocumentDB Account (S1)
-   2 Event Hubs (Basic throughput unit)
-   Storage account (Standard-GRS)
-   3 Stream Analytics jobs (1 streaming unit per job)
-   2 App Service plans (S1 - Standard: 2 small per App Service plan)
-   1 Web app (included in App Service plan)
-   1 Web job (included in App Service plan)

Four ways to minimize costs, depending on your solution needs are:

1.  Change the IoT Hub from an **S2 – Standard** to an **S1 – Standard**.
2.  Change the Storage account from **Standard – GRS** to **Standard – LRS**.
3.  Change the App Service plans from **S1 - Standard** to **B1 – Basic**.
4.  Pause the simulated devices. The simulated devices run in a web job. To completely halt generation of new data when not in use, you can stop the web job in which the simulated devices are running.

To make these changes, click into each Azure resource from your resource group:

### \#1 IoT Hub

Navigate to **Settings&gt; Pricing and Scale &gt;** and change the pricing tier to **S1 – Standard**. Make sure to click **Save** in the top navigation.

![][img-iot-hub]

### \#2 Storage

Navigate to **Settings &gt; Configuration**. Select **Locally-redundant storage (LRS)**. Make sure to click **Save** in the top navigation.

![][img-storage]

### \#3 App Service plan

Navigate to **Settings&gt; Scale Up (App Service Plan) &gt;** and change the tier to **B1 – Basic** for both App Service plans. Make sure to click **Select** at the bottom.

![][img-service]

### \#4 Pause the simulated devices running in the web job

The web app containing the web jobs is named **&lt;solution name&gt;-jobhost**. Navigate to **Settings &gt; WebJobs**. Right click the web job named **DeviceSimulator-WebJob** and select **Stop**.

![][img-job]

If you redeploy your solution from the command line, we respect the scale changes you’ve made and will not overwrite these changes with the default template. In addition, you also have full control over scaling Azure services up or restoring services to their original scale units.

This article walked through changing the footprint of the underlying Azure services to enable you to conduct demos and build small PoCs at minimal cost. For further reading, take a look at our [Azure IoT Suite Documentation]. Curious about what’s coming next or want to chime in on feature enhancements? Participate in the [Azure IoT User Voice].


<!-- Images and links -->
[Microsoft Azure IoT Suite]: https://www.microsoft.com/en-us/internet-of-things/azure-iot-suite
[Tutorial: Get started with the IoT preconfigured solutions]: https://azure.microsoft.com/documentation/articles/iot-suite-getstarted-preconfigured-solutions/
[Azure Portal]: https://portal.azure.com/
[Azure Classic Portal]: https://manage.windowsazure.com/
[img-iot-hub]: media/image1.png
[img-storage]: media/image2.png
[img-service]: media/image3.png
[img-job]: media/image4.png
[Azure IoT Suite Documentation]: https://azure.microsoft.com/documentation/suites/iot-suite
[Azure IoT User Voice]: https://feedback.azure.com/forums/321918-azure-iot
