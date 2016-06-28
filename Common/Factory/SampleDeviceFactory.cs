using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory
{
    public static class SampleDeviceFactory
    {
        public const string OBJECT_TYPE_DEVICE_INFO = "DeviceInfo";

        public const string VERSION_1_0 = "1.0";

        private static Random rand = new Random();

        private const int MAX_COMMANDS_SUPPORTED = 6;

        private const bool IS_SIMULATED_DEVICE = true;

        private static List<string> DefaultDeviceNames = new List<string>{
            "SampleDevice001", 
            "SampleDevice002", 
            "SampleDevice003", 
            "SampleDevice004"
        };

        private class Location
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            
            public Location(double latitude, double longitude)
            {
                Latitude = latitude;
                Longitude = longitude;
            }

        }

        private static List<Location> _possibleDeviceLocations = new List<Location>{
            new Location(47.659159, -122.141515),  // Microsoft Red West Campus, Building A
            new Location(47.593307, -122.332165),  // 800 Occidental Ave S, Seattle, WA 98134
            new Location(47.617025, -122.191285),  // 11111 NE 8th St, Bellevue, WA 98004
            new Location(47.583582, -122.130622),  // 3003 160th Ave SE Bellevue, WA 98008
            new Location(47.639511, -122.134376),  // 15580 NE 31st St Redmond, WA 98008
            new Location(47.644328, -122.137036),  // 15255 NE 40th St Redmond, WA 98008
            new Location(47.621573, -122.338101),  // 320 Westlake Ave N, Seattle, WA 98109
            new Location(47.642357, -122.137152), // 15010 NE 36th St, Redmond, WA 98052
            new Location(47.614981, -122.195781), //500 108th Ave NE, Bellevue, WA 98004 
            new Location(47.642528, -122.130565), //3460 157th Ave NE, Redmond, WA 98052
            new Location(47.617187, -122.191685), //11155 NE 8th St, Bellevue, WA 98004
            new Location(47.677292, -122.093030), //18500 NE Union Hill Rd, Redmond, WA 98052
            new Location(47.642528, -122.130565), //3600 157th Ave NE, Redmond, WA 98052
            new Location(47.642876, -122.125492), //16070 NE 36th Way Bldg 33, Redmond, WA 98052
            new Location(47.637376, -122.140445), //14999 NE 31st Way, Redmond, WA 98052
            new Location(47.636121, -122.130254) //3009 157th Pl NE, Redmond, WA 98052
        };

        public static dynamic GetSampleSimulatedDevice(string deviceId, string key)
        {
            dynamic device = DeviceSchemaHelper.BuildDeviceStructure(deviceId, true, null);

            AssignDeviceProperties(deviceId, device);
            device.ObjectType = OBJECT_TYPE_DEVICE_INFO;
            device.Version = VERSION_1_0;
            device.IsSimulatedDevice = IS_SIMULATED_DEVICE;

            AssignTelemetry(device);
            AssignCommands(device);

            return device;
        }

        public static dynamic GetSampleDevice(Random randomNumber, SecurityKeys keys)
        {
            string deviceId = 
                string.Format(
                    CultureInfo.InvariantCulture,
                    "00000-DEV-{0}C-{1}LK-{2}D-{3}",
                    MAX_COMMANDS_SUPPORTED, 
                    randomNumber.Next(99999),
                    randomNumber.Next(99999),
                    randomNumber.Next(99999));

            dynamic device = DeviceSchemaHelper.BuildDeviceStructure(deviceId, false, null);
            device.ObjectName = "IoT Device Description";

            AssignDeviceProperties(deviceId, device);
            AssignTelemetry(device);
            AssignCommands(device);

            return device;
        }

        private static void AssignDeviceProperties(string deviceId, dynamic device)
        {
            int randomId = rand.Next(0, _possibleDeviceLocations.Count - 1); 
            dynamic deviceProperties = DeviceSchemaHelper.GetDeviceProperties(device);
            deviceProperties.HubEnabledState = true;
            deviceProperties.Manufacturer = "Contoso Inc.";
            deviceProperties.ModelNumber = "MD-" + randomId;
            deviceProperties.SerialNumber = "SER" + randomId;
            deviceProperties.FirmwareVersion = "1." + randomId;
            deviceProperties.Platform = "Plat-" + randomId;
            deviceProperties.Processor = "i3-" + randomId;
            deviceProperties.InstalledRAM = randomId + " MB";

            // Choose a location among the 16 above and set Lat and Long for device properties
            deviceProperties.Latitude = _possibleDeviceLocations[randomId].Latitude;
            deviceProperties.Longitude = _possibleDeviceLocations[randomId].Longitude;
        }

        private static void AssignTelemetry(dynamic device)
        {
            dynamic telemetry = CommandSchemaHelper.CreateNewTelemetry("Temperature", "Temperature", "double");
            CommandSchemaHelper.AddTelemetryToDevice(device, telemetry);

            telemetry = CommandSchemaHelper.CreateNewTelemetry("Humidity", "Humidity", "double");
            CommandSchemaHelper.AddTelemetryToDevice(device, telemetry);
        }

        private static void AssignCommands(dynamic device)
        {
            dynamic command = CommandSchemaHelper.CreateNewCommand("PingDevice");
            CommandSchemaHelper.AddCommandToDevice(device, command);
            
            command = CommandSchemaHelper.CreateNewCommand("StartTelemetry");
            CommandSchemaHelper.AddCommandToDevice(device, command);
            
            command = CommandSchemaHelper.CreateNewCommand("StopTelemetry");
            CommandSchemaHelper.AddCommandToDevice(device, command);
            
            command = CommandSchemaHelper.CreateNewCommand("ChangeSetPointTemp");
            CommandSchemaHelper.DefineNewParameterOnCommand(command, "SetPointTemp", "double");
            CommandSchemaHelper.AddCommandToDevice(device, command);
            
            command = CommandSchemaHelper.CreateNewCommand("DiagnosticTelemetry");
            CommandSchemaHelper.DefineNewParameterOnCommand(command, "Active", "boolean");
            CommandSchemaHelper.AddCommandToDevice(device, command);
            
            command = CommandSchemaHelper.CreateNewCommand("ChangeDeviceState");
            CommandSchemaHelper.DefineNewParameterOnCommand(command, "DeviceState", "string");
            CommandSchemaHelper.AddCommandToDevice(device, command);
        }

        public static List<string> GetDefaultDeviceNames()
        {
            long milliTime = DateTime.Now.Millisecond;
            return DefaultDeviceNames.Select(r => string.Concat(r, "_" + milliTime)).ToList();
        }
    }
}
