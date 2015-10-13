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
            new Location(47.583582, -122.130622)  // 3003 160th Ave SE Bellevue, WA 98008
        };

        public static dynamic GetSampleSimulatedDevice(string deviceId, string key)
        {
            dynamic device = DeviceSchemaHelper.BuildDeviceStructure(deviceId, true);

            AssignDeviceProperties(deviceId, device);
            device.ObjectType = OBJECT_TYPE_DEVICE_INFO;
            device.Version = VERSION_1_0;
            device.IsSimulatedDevice = IS_SIMULATED_DEVICE;

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

            dynamic device = DeviceSchemaHelper.BuildDeviceStructure(deviceId, false);
            device.ObjectName = "IoT Device Description";

            AssignDeviceProperties(deviceId, device);
            AssignCommands(device);

            return device;
        }

        private static void AssignDeviceProperties(string deviceId, dynamic device)
        {
            dynamic deviceProperties = DeviceSchemaHelper.GetDeviceProperties(device);
            deviceProperties.HubEnabledState = true;
            deviceProperties.Manufacturer = "Contoso Inc.";
            deviceProperties.ModelNumber = "MD-" + GetIntBasedOnString(deviceId + "ModelNumber", 1000);
            deviceProperties.SerialNumber = "SER" + GetIntBasedOnString(deviceId + "SerialNumber", 10000);
            deviceProperties.FirmwareVersion = "1." + GetIntBasedOnString(deviceId + "FirmwareVersion", 100);
            deviceProperties.Platform = "Plat-" + GetIntBasedOnString(deviceId + "Platform", 100);
            deviceProperties.Processor = "i3-" + GetIntBasedOnString(deviceId + "Processor", 10000);
            deviceProperties.InstalledRAM = GetIntBasedOnString(deviceId + "InstalledRAM", 100) + " MB";

            // Choose a location between the 3 above and set Lat and Long for device properties
            int chosenLocation = GetIntBasedOnString(deviceId + "Location", _possibleDeviceLocations.Count);
            deviceProperties.Latitude = _possibleDeviceLocations[chosenLocation].Latitude;
            deviceProperties.Longitude = _possibleDeviceLocations[chosenLocation].Longitude;
        }

        private static int GetIntBasedOnString(string input, int maxValueExclusive)
        {
            int hash = input.GetHashCode();

            //Keep the result positive
            if(hash < 0)
            {
                hash = -hash;
            }

            return hash % maxValueExclusive;
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
