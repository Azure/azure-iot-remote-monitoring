# FAQ

__1. Sample of the `DeviceInfo` telemetry message__

Here is a "full-feature" `DeviceInfo` telemetry message captured from the built-in simulator:
```
{
  "DeviceProperties": {
    "DeviceID": "CoolingSampleDevice019_685",
    "HubEnabledState": true,
    "CreatedTime": "2017-02-22T02:36:59.0920072Z",
    "DeviceState": "normal",
    "Manufacturer": "Contoso Inc.",
    "ModelNumber": "MD-5",
    "SerialNumber": "SER5",
    "FirmwareVersion": "2.0",
    "Platform": "Plat-5",
    "Processor": "i3-5",
    "InstalledRAM": "5 MB",
    "Latitude": 39.97779242837,
    "Longitude": 116.308691852621
  },
  "Commands": [
    {
      "Name": "PingDevice",
      "DeliveryType": 0,
      "Parameters": [],
      "Description": "The device responds to this command with an acknowledgement. This is useful for checking that the device is still active and listening."
    },
    {
      "Name": "StartTelemetry",
      "DeliveryType": 0,
      "Parameters": [],
      "Description": "Instructs the device to start sending telemetry."
    },
    {
      "Name": "StopTelemetry",
      "DeliveryType": 0,
      "Parameters": [],
      "Description": "Instructs the device to stop sending telemetry."
    },
    {
      "Name": "ChangeSetPointTemp",
      "DeliveryType": 0,
      "Parameters": [
        {
          "Name": "SetPointTemp",
          "Type": "double"
        }
      ],
      "Description": "Controls the simulated temperature telemetry values the device sends. This is useful for testing back-end logic."
    },
    {
      "Name": "DiagnosticTelemetry",
      "DeliveryType": 0,
      "Parameters": [
        {
          "Name": "Active",
          "Type": "boolean"
        }
      ],
      "Description": "Controls if the device should send the external temperature as telemetry."
    },
    {
      "Name": "ChangeDeviceState",
      "DeliveryType": 0,
      "Parameters": [
        {
          "Name": "DeviceState",
          "Type": "string"
        }
      ],
      "Description": "Sets the device state metadata property that the device reports. This is useful for testing back-end logic."
    }
  ],
  "CommandHistory": [],
  "IsSimulatedDevice": true,
  "Telemetry": [
    {
      "Name": "Temperature",
      "DisplayName": "Temperature",
      "Type": "double"
    },
    {
      "Name": "Humidity",
      "DisplayName": "Humidity",
      "Type": "double"
    }
  ],
  "Version": "1.0",
  "ObjectType": "DeviceInfo"
}
```

Most of the items in the `DeviceInfo` are optional. Here is a minimized version:
```
{
  "DeviceProperties": {
    "HubEnabledState": true
  },
  "Version": "1.0",
  "ObjectType": "DeviceInfo"
}
```

Reminders:
* `DeviceId` is NOT required. The remote monitoring solution could retrieve it from the system properties of the IoT Hub message.
* The `HubEnabledState` must be set as `true`. Otherwise, the Remote Monitoring portal will treat the device as disabled, and show no command or methods for action.
* The value of `Version` and `ObjectType` must be exactly same as the sample.
* Since the `Telemetry` item was removed, the Remote Monitoring portal will treat all the fields (including `PartitionId`) in the telemetry message as telemetries.

__2. Sample of reported supported methods__

Here is a sample of the supported method, which reported by the built-in simulator:
```
"SupportedMethods": {
  "InitiateFirmwareUpdate--FwPackageUri-string": "Updates device Firmware. Use parameter 'FwPackageUri' to specifiy the URI of the firmware file, e.g. https://iotrmassets.blob.core.windows.net/firmwares/FW20.bin",
  "Reboot": "Reboot the device",
  "FactoryReset": "Reset the device (including firmware and configuration) to factory default state"
}
```
