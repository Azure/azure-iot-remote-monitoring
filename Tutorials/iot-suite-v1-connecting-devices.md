# Connect your device to the remote monitoring preconfigured solution (Windows)

## Scenario overview
In this scenario, you create a device that sends the following telemetry to the remote monitoring [preconfigured solution][lnk-what-are-preconfig-solutions]:

* External temperature
* Internal temperature
* Humidity

For simplicity, the code on the device generates sample values, but we encourage you to extend the sample by connecting real sensors to your device and sending real telemetry.

The device is also able to respond to methods invoked from the solution dashboard and desired property values set in the solution dashboard.

To complete this tutorial, you need an active Azure account. If you don't have an account, you can create a free trial account in just a couple of minutes. For details, see [Azure Free Trial][lnk-free-trial].

## Before you start
Before you write any code for your device, you must provision your remote monitoring preconfigured solution and provision a new custom device in that solution.

### Provision your remote monitoring preconfigured solution
The device you create in this tutorial sends data to an instance of the [remote monitoring][lnk-remote-monitoring] preconfigured solution. If you haven't already provisioned the remote monitoring preconfigured solution in your Azure account, use the following steps:

1. On the <https://www.azureiotsuite.com/> page, click **+** to create a solution.
2. Click **Select** on the **Remote monitoring** panel to create your solution.
3. On the **Create Remote monitoring solution** page, enter a **Solution name** of your choice, select the **Region** you want to deploy to, and select the Azure subscription to want to use. Then click **Create solution**.
4. Wait until the provisioning process completes.


> The preconfigured solutions use billable Azure services. Be sure to remove the preconfigured solution from your subscription when you are done with it to avoid any unnecessary charges. You can completely remove a preconfigured solution from your subscription by visiting the <https://www.azureiotsuite.com/> page.
> 
> 

When the provisioning process for the remote monitoring solution finishes, click **Launch** to open the solution dashboard in your browser.

![Solution dashboard][img-dashboard]

### Provision your device in the remote monitoring solution

> If you have already provisioned a device in your solution, you can skip this step. You need to know the device credentials when you create the client application.
> 
> 

For a device to connect to the preconfigured solution, it must identify itself to IoT Hub using valid credentials. You can retrieve the device credentials from the solution dashboard. You include the device credentials in your client application later in this tutorial.

To add a device to your remote monitoring solution, complete the following steps in the solution dashboard:

1. In the lower left-hand corner of the dashboard, click **Add a device**.
   
   ![Add a device][1]
2. In the **Custom Device** panel, click **Add new**.
   
   ![Add a custom device][2]
3. Choose **Let me define my own Device ID**. Enter a Device ID such as **mydevice**, click **Check ID** to verify that name isn't already in use, and then click **Create** to provision the device.
   
   ![Add device ID][3]
4. Make a note the device credentials (Device ID, IoT Hub Hostname, and Device Key). Your client application needs these values to connect to the remote monitoring solution. Then click **Done**.
   
    ![View device credentials][4]
5. Select your device in the device list in the solution dashboard. Then, in the **Device Details** panel, click **Enable Device**. The status of your device is now **Running**. The remote monitoring solution can now receive telemetry from your device and invoke methods on the device.

[img-dashboard]: ./media/iot-suite-v1-selector-connecting/dashboard.png
[1]: ./media/iot-suite-v1-selector-connecting/suite0.png
[2]: ./media/iot-suite-v1-selector-connecting/suite1.png
[3]: ./media/iot-suite-v1-selector-connecting/suite2.png
[4]: ./media/iot-suite-v1-selector-connecting/suite3.png

[lnk-what-are-preconfig-solutions]: ../articles/iot-suite/iot-suite-v1-what-are-preconfigured-solutions.md
[lnk-remote-monitoring]: ../articles/iot-suite/iot-suite-v1-remote-monitoring-sample-walkthrough.md
[lnk-free-trial]: http://azure.microsoft.com/pricing/free-trial/

## Create a C sample solution on Windows
The following steps show you how to create a client application that communicates with the remote monitoring preconfigured solution. This application is written in C and built and run on Windows.

Create a starter project in Visual Studio 2015 or Visual Studio 2017 and add the IoT Hub device client NuGet packages:

1. In Visual Studio, create a C console application using the Visual C++ **Win32 Console Application** template. Name the project **RMDevice**.
2. On the **Application Settings** page in the **Win32 Application Wizard**, ensure that **Console application** is selected, and uncheck **Precompiled header** and **Security Development Lifecycle (SDL) checks**.
3. In **Solution Explorer**, delete the files stdafx.h, targetver.h, and stdafx.cpp.
4. In **Solution Explorer**, rename the file RMDevice.cpp to RMDevice.c.
5. In **Solution Explorer**, right-click on the **RMDevice** project and then click **Manage NuGet packages**. Click **Browse**, then search for and install the following NuGet packages:
   
   * Microsoft.Azure.IoTHub.Serializer
   * Microsoft.Azure.IoTHub.IoTHubClient
   * Microsoft.Azure.IoTHub.MqttTransport
6. In **Solution Explorer**, right-click on the **RMDevice** project and then click **Properties** to open the project's **Property Pages** dialog box. For details, see [Setting Visual C++ Project Properties][lnk-c-project-properties]. 
7. Click the **Linker** folder, then click the **Input** property page.
8. Add **crypt32.lib** to the **Additional Dependencies** property. Click **OK** and then **OK** again to save the project property values.

Add the Parson JSON library to the **RMDevice** project and add the required `#include` statements:

1. In a suitable folder on your computer, clone the Parson GitHub repository using the following command:

    ```
    git clone https://github.com/kgabis/parson.git
    ```

1. Copy the parson.h and parson.c files from the local copy of the Parson repository to your **RMDevice** project folder.

1. In Visual Studio, right-click the **RMDevice** project, click **Add**, and then click **Existing Item**.

1. In the **Add Existing Item** dialog, select the parson.h and parson.c files in the **RMDevice** project folder. Then click **Add** to add these two files to your project.

1. In Visual Studio, open the RMDevice.c file. Replace the existing `#include` statements with the following code:
   
    ```c
    #include "iothubtransportmqtt.h"
    #include "schemalib.h"
    #include "iothub_client.h"
    #include "serializer_devicetwin.h"
    #include "schemaserializer.h"
    #include "azure_c_shared_utility/threadapi.h"
    #include "azure_c_shared_utility/platform.h"
    #include "parson.h"
    ```

    
    > Now you can verify that your project has the correct dependencies set up by building it.

## Specify the behavior of the IoT device

The IoT Hub serializer client library uses a model to specify the format of the messages the device exchanges with IoT Hub.

1. Add the following variable declarations after the `#include` statements. Replace the placeholder values `[Device Id]` and `[Device connection string]` with the values you noted for the physical device you added to the remote monitoring solution:

    ```c
    static const char* deviceId = "[Device Id]";
    static const char* connectionString = "[Device connection string]";
    ```

1. Add the following code to define the model that enables the device to communicate with IoT Hub. This model specifies that the device:

    - Can send temperature, pressure, and humidity as telemetry.
    - Can send reported properties, to the device twin in IoT Hub. These reported properties include information about the telemetry schema and supported methods.
    - Can receive and act on desired properties set in the device twin in IoT Hub.
    - Can respond to the **Reboot**, **FirmwareUpdate**, **EmergencyValveRelease**, and **IncreasePressure** direct methods invoked from the UI. The device sends information about the direct methods it supports using reported properties.

    ```c
    // Define the Model
    BEGIN_NAMESPACE(Contoso);

    DECLARE_STRUCT(MessageSchema,
    ascii_char_ptr, Name,
    ascii_char_ptr, Format,
    ascii_char_ptr_no_quotes, Fields
    )

    DECLARE_STRUCT(TelemetrySchema,
    ascii_char_ptr, Interval,
    ascii_char_ptr, MessageTemplate,
    MessageSchema, MessageSchema
    )

    DECLARE_STRUCT(TelemetryProperties,
    TelemetrySchema, TemperatureSchema,
    TelemetrySchema, HumiditySchema,
    TelemetrySchema, PressureSchema
    )

    DECLARE_DEVICETWIN_MODEL(Chiller,
    /* Telemetry (temperature, external temperature and humidity) */
    WITH_DATA(double, temperature),
    WITH_DATA(ascii_char_ptr, temperature_unit),
    WITH_DATA(double, pressure),
    WITH_DATA(ascii_char_ptr, pressure_unit),
    WITH_DATA(double, humidity),
    WITH_DATA(ascii_char_ptr, humidity_unit),

    /* Manage firmware update process */
    WITH_DATA(ascii_char_ptr, new_firmware_URI),
    WITH_DATA(ascii_char_ptr, new_firmware_version),

    /* Device twin properties */
    WITH_REPORTED_PROPERTY(ascii_char_ptr, Protocol),
    WITH_REPORTED_PROPERTY(ascii_char_ptr, SupportedMethods),
    WITH_REPORTED_PROPERTY(TelemetryProperties, Telemetry),
    WITH_REPORTED_PROPERTY(ascii_char_ptr, Type),
    WITH_REPORTED_PROPERTY(ascii_char_ptr, Firmware),
    WITH_REPORTED_PROPERTY(ascii_char_ptr, FirmwareUpdateStatus),
    WITH_REPORTED_PROPERTY(ascii_char_ptr, Location),
    WITH_REPORTED_PROPERTY(double, Latitiude),
    WITH_REPORTED_PROPERTY(double, Longitude),

    WITH_DESIRED_PROPERTY(ascii_char_ptr, Interval, onDesiredInterval),

    /* Direct methods implemented by the device */
    WITH_METHOD(Reboot),
    WITH_METHOD(FirmwareUpdate, ascii_char_ptr, Firmware, ascii_char_ptr, FirmwareUri),
    WITH_METHOD(EmergencyValveRelease),
    WITH_METHOD(IncreasePressure)
    );

    END_NAMESPACE(Contoso);
    ```

## Implement the behavior of the device

Now add code that implements the behavior defined in the model.

1. Add the following callback handler that runs when the device has sent new reported property values to the solution accelerator:

    ```c
    /* Callback after sending reported properties */
    void deviceTwinCallback(int status_code, void* userContextCallback)
    {
      (void)(userContextCallback);
      printf("IoTHub: reported properties delivered with status_code = %u\n", status_code);
    }
    ```

1. Add the following function that simulates a firmware update process:

    ```c
    static int do_firmware_update(void *param)
    {
      Chiller *chiller = (Chiller *)param;
      printf("do_firmware_update('URI: %s, Version: %s')\r\n", chiller->new_firmware_URI, chiller->new_firmware_version);

      printf("Simulating download phase...\r\n");
      chiller->FirmwareUpdateStatus = "downloading";
      /* Send reported properties to IoT Hub */
      if (IoTHubDeviceTwin_SendReportedStateChiller(chiller, deviceTwinCallback, NULL) != IOTHUB_CLIENT_OK)
      {
        printf("Failed sending serialized reported state\r\n");
      }
      ThreadAPI_Sleep(5000);

      printf("Simulating applying phase...\r\n");
      chiller->FirmwareUpdateStatus = "applying";
      /* Send reported properties to IoT Hub */
      if (IoTHubDeviceTwin_SendReportedStateChiller(chiller, deviceTwinCallback, NULL) != IOTHUB_CLIENT_OK)
      {
        printf("Failed sending serialized reported state\r\n");
      }
      ThreadAPI_Sleep(5000);

      printf("Simulating reboot phase...\r\n");
      chiller->FirmwareUpdateStatus = "rebooting";
      /* Send reported properties to IoT Hub */
      if (IoTHubDeviceTwin_SendReportedStateChiller(chiller, deviceTwinCallback, NULL) != IOTHUB_CLIENT_OK)
      {
        printf("Failed sending serialized reported state\r\n");
      }
      ThreadAPI_Sleep(5000);

      chiller->Firmware = _strdup(chiller->new_firmware_version);
      chiller->FirmwareUpdateStatus = "waiting";
      /* Send reported properties to IoT Hub */
      if (IoTHubDeviceTwin_SendReportedStateChiller(chiller, deviceTwinCallback, NULL) != IOTHUB_CLIENT_OK)
      {
        printf("Failed sending serialized reported state\r\n");
      }

      return 0;
    }
    ```

1. Add the following function that handles the desired properties set in the solution dashboard. These desired properties are defined in the model:

    ```c
    void onDesiredInterval(void* argument)
    {
      /* By convention 'argument' is of the type of the MODEL */
      Chiller* chiller = argument;
      printf("Received a new desired Interval value: %s \r\n", chiller->Interval);
    }
    ```

1. Add the following functions that handle the direct methods invoked through the IoT hub. These direct methods are defined in the model:

    ```c
    /* Handlers for direct methods */
    METHODRETURN_HANDLE Reboot(Chiller* chiller)
    {
      (void)(chiller);

      METHODRETURN_HANDLE result = MethodReturn_Create(201, "\"Rebooting\"");
      printf("Received reboot request\r\n");
      return result;
    }

    METHODRETURN_HANDLE FirmwareUpdate(Chiller* chiller, ascii_char_ptr Firmware, ascii_char_ptr FirmwareUri)
    {
      printf("Recieved firmware update request request\r\n");
      METHODRETURN_HANDLE result = NULL;
      if (chiller->FirmwareUpdateStatus != "waiting")
      {
        LogError("Attempting to initiate a firmware update out of order");
        result = MethodReturn_Create(400, "\"Attempting to initiate a firmware update out of order\"");
      }
      else
      {
        chiller->new_firmware_version = _strdup(Firmware);
        chiller->new_firmware_URI = _strdup(FirmwareUri);
        THREAD_HANDLE thread_apply;
        THREADAPI_RESULT t_result = ThreadAPI_Create(&thread_apply, do_firmware_update, chiller);
        if (t_result == THREADAPI_OK)
        {
          result = MethodReturn_Create(201, "\"Starting firmware update thread\"");
        }
        else
        {
          LogError("Failed to start firmware update thread");
          result = MethodReturn_Create(500, "\"Failed to start firmware update thread\"");
        }
      }
      
      return result;
    }

    METHODRETURN_HANDLE EmergencyValveRelease(Chiller* chiller)
    {
      (void)(chiller);

      METHODRETURN_HANDLE result = MethodReturn_Create(201, "\"Releasing Emergency Valve\"");
      printf("Recieved emergency valve release request\r\n");
      return result;
    }

    METHODRETURN_HANDLE IncreasePressure(Chiller* chiller)
    {
      (void)(chiller);

      METHODRETURN_HANDLE result = MethodReturn_Create(201, "\"Increasing Pressure\"");
      printf("Received increase pressure request\r\n");
      return result;
    }
    ```

1. Add the following function that adds a property to a device-to-cloud message:

    ```c
    /* Add message property */
    static void addProperty(MAP_HANDLE propMap, char* propName, char* propValue)
    {
      if (Map_AddOrUpdate(propMap, propName, propValue) != MAP_OK)
      {
        (void)printf("ERROR: Map_AddOrUpdate Failed on %s!\r\n", propName);
      }
    }
    ```

1. Add the following function that sends a message with properties to the solution accelerator:

    ```c
    static void sendMessage(IOTHUB_CLIENT_HANDLE iotHubClientHandle, const unsigned char* buffer, size_t size, char* schema)
    {
      IOTHUB_MESSAGE_HANDLE messageHandle = IoTHubMessage_CreateFromByteArray(buffer, size);
      if (messageHandle == NULL)
      {
        printf("unable to create a new IoTHubMessage\r\n");
      }
      else
      {
        // Add properties
        MAP_HANDLE propMap = IoTHubMessage_Properties(messageHandle);
        addProperty(propMap, "$$MessageSchema", schema);
        addProperty(propMap, "$$ContentType", "JSON");
        time_t now = time(0);
        struct tm* timeinfo;
        #pragma warning(disable: 4996)
        timeinfo = gmtime(&now);
        char timebuff[50];
        strftime(timebuff, 50, "%Y-%m-%dT%H:%M:%SZ", timeinfo);
        addProperty(propMap, "$$CreationTimeUtc", timebuff);

        if (IoTHubClient_SendEventAsync(iotHubClientHandle, messageHandle, NULL, NULL) != IOTHUB_CLIENT_OK)
        {
          printf("failed to hand over the message to IoTHubClient");
        }
        else
        {
          printf("IoTHubClient accepted the message for delivery\r\n");
        }

        IoTHubMessage_Destroy(messageHandle);
      }
      free((void*)buffer);
    }
    ```

1. Add the following function to connect your device to the solution accelerator in the cloud, and exchange data. This function performs the following steps:

    - Initializes the platform.
    - Registers the Contoso namespace with the serialization library.
    - Initializes the client with the device connection string.
    - Create an instance of the **Chiller** model.
    - Creates and sends reported property values.
    - Creates a loop to send telemetry every five seconds while the firmware update status is **waiting**.
    - Deinitializes all resources.

    ```c
    void remote_monitoring_run(void)
    {
      if (platform_init() != 0)
      {
        printf("Failed to initialize the platform.\r\n");
      }
      else
      {
        if (SERIALIZER_REGISTER_NAMESPACE(Contoso) == NULL)
        {
          printf("Unable to SERIALIZER_REGISTER_NAMESPACE\r\n");
        }
        else
        {
          IOTHUB_CLIENT_HANDLE iotHubClientHandle = IoTHubClient_CreateFromConnectionString(connectionString, MQTT_Protocol);
          if (iotHubClientHandle == NULL)
          {
            printf("Failure in IoTHubClient_CreateFromConnectionString\r\n");
          }
          else
          {
            Chiller* chiller = IoTHubDeviceTwin_CreateChiller(iotHubClientHandle);
            if (chiller == NULL)
            {
              printf("Failure in IoTHubDeviceTwin_CreateChiller\r\n");
            }
            else
            {
              /* Set values for reported properties */
              chiller->Protocol = "MQTT";
              chiller->SupportedMethods = "Reboot,FirmwareUpdate,EmergencyValveRelease,IncreasePressure";
              chiller->Telemetry.TemperatureSchema.Interval = "00:00:05";
              chiller->Telemetry.TemperatureSchema.MessageTemplate = "{\"temperature\":${temperature},\"temperature_unit\":\"${temperature_unit}\"}";
              chiller->Telemetry.TemperatureSchema.MessageSchema.Name = "chiller-temperature;v1";
              chiller->Telemetry.TemperatureSchema.MessageSchema.Format = "JSON";
              chiller->Telemetry.TemperatureSchema.MessageSchema.Fields = "{\"temperature\":\"Double\",\"temperature_unit\":\"Text\"}";
              chiller->Telemetry.HumiditySchema.Interval = "00:00:05";
              chiller->Telemetry.HumiditySchema.MessageTemplate = "{\"humidity\":${humidity},\"humidity_unit\":\"${humidity_unit}\"}";
              chiller->Telemetry.HumiditySchema.MessageSchema.Name = "chiller-humidity;v1";
              chiller->Telemetry.HumiditySchema.MessageSchema.Format = "JSON";
              chiller->Telemetry.HumiditySchema.MessageSchema.Fields = "{\"humidity\":\"Double\",\"humidity_unit\":\"Text\"}";
              chiller->Telemetry.PressureSchema.Interval = "00:00:05";
              chiller->Telemetry.PressureSchema.MessageTemplate = "{\"pressure\":${pressure},\"pressure_unit\":\"${pressure_unit}\"}";
              chiller->Telemetry.PressureSchema.MessageSchema.Name = "chiller-pressure;v1";
              chiller->Telemetry.PressureSchema.MessageSchema.Format = "JSON";
              chiller->Telemetry.PressureSchema.MessageSchema.Fields = "{\"pressure\":\"Double\",\"pressure_unit\":\"Text\"}";
              chiller->Type = "Chiller";
              chiller->Firmware = "1.0.0";
              chiller->FirmwareUpdateStatus = "waiting";
              chiller->Location = "Building 44";
              chiller->Latitiude = 47.638928;
              chiller->Longitude = -122.13476;

              /* Send reported properties to IoT Hub */
              if (IoTHubDeviceTwin_SendReportedStateChiller(chiller, deviceTwinCallback, NULL) != IOTHUB_CLIENT_OK)
              {
                printf("Failed sending serialized reported state\r\n");
              }
              else
              {
                /* Send telemetry */
                chiller->temperature_unit = "F";
                chiller->pressure_unit = "psig";
                chiller->humidity_unit = "%";

                srand((unsigned int)time(NULL));
                while (1)
                {
                  chiller->temperature = 50 + ((rand() % 10) - 5);
                  chiller->pressure = 55 + ((rand() % 10) - 5);
                  chiller->humidity = 30 + ((rand() % 10) - 5);
                  unsigned char*buffer;
                  size_t bufferSize;

                  if (chiller->FirmwareUpdateStatus == "waiting")
                  {
                    (void)printf("Sending sensor value Temperature = %f %s,\r\n", chiller->temperature, chiller->temperature_unit);

                    if (SERIALIZE(&buffer, &bufferSize, chiller->temperature, chiller->temperature_unit) != CODEFIRST_OK)
                    {
                      (void)printf("Failed sending sensor value\r\n");
                    }
                    else
                    {
                      sendMessage(iotHubClientHandle, buffer, bufferSize, chiller->Telemetry.TemperatureSchema.MessageSchema.Name);
                    }

                    (void)printf("Sending sensor value Humidity = %f %s,\r\n", chiller->humidity, chiller->humidity_unit);

                    if (SERIALIZE(&buffer, &bufferSize, chiller->humidity, chiller->humidity_unit) != CODEFIRST_OK)
                    {
                      (void)printf("Failed sending sensor value\r\n");
                    }
                    else
                    {
                      sendMessage(iotHubClientHandle, buffer, bufferSize, chiller->Telemetry.HumiditySchema.MessageSchema.Name);
                    }

                    (void)printf("Sending sensor value Pressure = %f %s,\r\n", chiller->pressure, chiller->pressure_unit);

                    if (SERIALIZE(&buffer, &bufferSize, chiller->pressure, chiller->pressure_unit) != CODEFIRST_OK)
                    {
                      (void)printf("Failed sending sensor value\r\n");
                    }
                    else
                    {
                      sendMessage(iotHubClientHandle, buffer, bufferSize, chiller->Telemetry.PressureSchema.MessageSchema.Name);
                    }
                  }

                  ThreadAPI_Sleep(5000);
                }

                IoTHubDeviceTwin_DestroyChiller(chiller);
              }
          }
            IoTHubClient_Destroy(iotHubClientHandle);
        }
          serializer_deinit();
        }
      }
      platform_deinit();
    }
    ```

    For reference, here is a sample **Telemetry** message sent to the solution accelerator:

    ```
    Device: [myCDevice],
    Data:[{"humidity":50.000000000000000, "humidity_unit":"%"}]
    Properties:
    '$$MessageSchema': 'chiller-humidity;v1'
    '$$ContentType': 'JSON'
    '$$CreationTimeUtc': '2017-09-12T09:17:13Z'
    ```

## Build and run the sample

Add code to invoke the **remote\_monitoring\_run** function and then build and run the device application.

1. Replace the **main** function with following code to invoke the **remote\_monitoring\_run** function:
   
    ```c
    int main()
    {
      remote_monitoring_run();
      return 0;
    }
    ```

1. Click **Build** and then **Build Solution** to build the device application.

1. In **Solution Explorer**, right-click the **RMDevice** project, click **Debug**, and then click **Start new instance** to run the sample. The console displays messages as the application sends sample telemetry to the preconfigured solution, receives desired property values set in the solution dashboard, and responds to methods invoked from the solution dashboard.

## View device telemetry in the dashboard
The dashboard in the remote monitoring solution enables you to view the telemetry your devices send to IoT Hub.

1. In your browser, return to the remote monitoring solution dashboard, click **Devices** in the left-hand panel to navigate to the **Devices list**.
2. In the **Devices list**, you should see that the status of your device is **Running**. If not, click **Enable Device** in the **Device Details** panel.
   
    ![View device status][18]
3. Click **Dashboard** to return to the dashboard, select your device in the **Device to View** drop-down to view its telemetry. The telemetry from the sample application is 50 units for internal temperature, 55 units for external temperature, and 50 units for humidity.
   
    ![View device telemetry][img-telemetry]

## Invoke a method on your device
The dashboard in the remote monitoring solution enables you to invoke methods on your devices through IoT Hub. For example, in the remote monitoring solution you can invoke a method to simulate rebooting a device.

1. In the remote monitoring solution dashboard, click **Devices** in the left-hand panel to navigate to the **Devices list**.
2. Click **Device ID** for your device in the **Devices list**.
3. In the **Device details** panel, click **Methods**.
   
    ![Device methods][13]
4. In the **Method** drop-down, select **InitiateFirmwareUpdate**, and then in **FWPACKAGEURI** enter a dummy URL. Click **Invoke Method** to call the method on the device.
   
    ![Invoke a device method][14]
   

5. You see a message in the console running your device code when the device handles the method. The results of the method are added to the history in the solution portal:

    ![View method history][img-method-history]

## Next steps
The article [Customizing preconfigured solutions][lnk-customize] describes some ways you can extend this sample. Possible extensions include using real sensors and implementing additional commands.

You can learn more about the [permissions on the azureiotsuite.com site][lnk-permissions].

[13]: ./media/iot-suite-v1-visualize-connecting/suite4.png
[14]: ./media/iot-suite-v1-visualize-connecting/suite7-1.png
[18]: ./media/iot-suite-v1-visualize-connecting/suite10.png
[img-telemetry]: ./media/iot-suite-v1-visualize-connecting/telemetry.png
[img-method-history]: ./media/iot-suite-v1-visualize-connecting/history.png
[lnk-customize]: ../articles/iot-suite/iot-suite-v1-guidance-on-customizing-preconfigured-solutions.md
[lnk-permissions]: ../articles/iot-suite/iot-suite-v1-permissions.md


[lnk-c-project-properties]: https://msdn.microsoft.com/library/669zx6zc.aspx
