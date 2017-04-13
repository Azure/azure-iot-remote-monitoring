using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory
{
    public static class SampleDeviceFactory
    {
        public const string OBJECT_TYPE_DEVICE_INFO = "DeviceInfo";

        public const string VERSION_1_0 = "1.0";

        private const int MAX_COMMANDS_SUPPORTED = 6;

        private const bool IS_SIMULATED_DEVICE = true;

        private static readonly Random Rand = new Random();

        private static readonly SortedDictionary<string, string> DefaultDeviceNames = new SortedDictionary<string, string>
        {
            { "CoolingDevice001", "制冷系统001" },
            { "CoolingDevice002", "制冷系统002" },
            { "CoolingDevice003", "制冷系统003" },
            { "CoolingDevice004", "制冷系统004" },
            { "CoolingDevice005", "制冷系统005" },
            { "CoolingDevice006", "制冷系统006" },
            { "CoolingDevice007", "制冷系统007" },
            { "CoolingDevice008", "制冷系统008" },
            { "CoolingDevice009", "制冷系统009" },
            { "CoolingDevice010", "制冷系统010" },
            { "CoolingDevice011", "制冷系统011" },
            { "CoolingDevice012", "制冷系统012" },
            { "CoolingDevice013", "制冷系统013" },
            { "CoolingDevice014", "制冷系统014" },
            { "CoolingDevice015", "制冷系统015" },
            { "CoolingDevice016", "制冷系统016" },
            { "CoolingDevice017", "制冷系统017" },
            { "CoolingDevice018", "制冷系统018" },
            { "CoolingDevice019", "制冷系统019" },
            { "CoolingDevice020", "制冷系统020" },
            { "CoolingDevice021", "制冷系统021" },
            { "CoolingDevice022", "制冷系统022" },
            { "CoolingDevice023", "制冷系统023" },
            { "CoolingDevice024", "制冷系统024" },
            { "CoolingDevice025", "制冷系统025" }
        };

        private static readonly IEnumerable<string> FreeFirmwareDeviceNames;

        private static readonly IEnumerable<string> HighTemperatureDeviceNames;

        private class Location
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Building { get; set; }
            public string[] Floors { get; set; }
        }

        static SampleDeviceFactory()
        {
            FreeFirmwareDeviceNames = DefaultDeviceNames.Keys.Take(8);
            HighTemperatureDeviceNames = DefaultDeviceNames.Keys.Take(5);
        }

        private static readonly List<Location> _possibleDeviceLocations = new List<Location>{
            new Location { Latitude = 39.979393, Longitude = 116.310282, Building = "微软1号楼", Floors = new[] { "1F", "2F" } },
            new Location { Latitude = 39.980554, Longitude = 116.310231, Building = "微软2号楼", Floors = new[] { "1F", "2F" } },
            new Location { Latitude = 39.980646, Longitude = 116.308796, Building = "立方庭", Floors = new[] { "5F", "6F", "7F" } },
            new Location { Latitude = 39.979622, Longitude = 116.312106, Building = "中国电子大厦", Floors = new[] { "1F", "2F", "10F", "11F" } },
            new Location { Latitude = 39.980970, Longitude = 116.312422, Building = "新东方", Floors = new[] { "1F", "2F" } },
            new Location { Latitude = 39.978241, Longitude = 116.309928, Building = "海兴大厦", Floors = new[] { "1F", "2F", "3F", "4F" } },
            new Location { Latitude = 39.979828, Longitude = 116.308995, Building = "1+1大厦", Floors = new[] { "1F", "2F" } },
            new Location { Latitude = 39.981661, Longitude = 116.309038, Building = "天创科技大厦", Floors = new[] { "1F", "2F" } }
        };

        public static DeviceModel GetSampleSimulatedDevice(InitialDeviceConfig config)
        {
            DeviceModel device = DeviceCreatorHelper.BuildDeviceStructure(config.DeviceId, true, null);

            AssignDeviceProperties(device, config);
            device.ObjectType = OBJECT_TYPE_DEVICE_INFO;
            device.Version = VERSION_1_0;
            device.IsSimulatedDevice = IS_SIMULATED_DEVICE;

            AssignTelemetry(device);
            AssignCommands(device);

            return device;
        }

        public static DeviceModel GetSampleDevice(Random randomNumber, SecurityKeys keys)
        {
            var deviceId = string.Format(
                    CultureInfo.InvariantCulture,
                    "00000-DEV-{0}C-{1}LK-{2}D-{3}",
                    MAX_COMMANDS_SUPPORTED,
                    randomNumber.Next(99999),
                    randomNumber.Next(99999),
                    randomNumber.Next(99999));

            var device = DeviceCreatorHelper.BuildDeviceStructure(deviceId, false, null);
            device.ObjectName = "IoT Device Description";

            AssignDeviceProperties(device, null);
            AssignTelemetry(device);
            AssignCommands(device);

            return device;
        }

        private static void AssignDeviceProperties(DeviceModel device, InitialDeviceConfig config)
        {
            int randomId = Rand.Next(0, _possibleDeviceLocations.Count - 1);
            if (device?.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
            }

            device.DeviceProperties.HubEnabledState = true;
            device.DeviceProperties.Manufacturer = "Contoso Inc.";
            device.DeviceProperties.ModelNumber = "MD-" + randomId;
            device.DeviceProperties.SerialNumber = "SER" + randomId;

            if (FreeFirmwareDeviceNames.Any(n => device.DeviceProperties.DeviceID.StartsWith(n, StringComparison.Ordinal)))
            {
                device.DeviceProperties.FirmwareVersion = "1." + randomId;
            }
            else
            {
                device.DeviceProperties.FirmwareVersion = "2.0";
            }

            device.DeviceProperties.Platform = "Plat-" + randomId;
            device.DeviceProperties.Processor = "i3-" + randomId;
            device.DeviceProperties.InstalledRAM = randomId + " MB";

            // Choose a location among the 16 above and set Lat and Long for device properties
            device.DeviceProperties.Latitude = config?.Latitude;
            device.DeviceProperties.Longitude = config?.Longitude;
        }

        private static void AssignTelemetry(DeviceModel device)
        {
            device.Telemetry.Add(new Telemetry("Temperature", "温度", "double"));
            device.Telemetry.Add(new Telemetry("Humidity", "湿度", "double"));
        }

        private static void AssignCommands(DeviceModel device)
        {
            // Device commands
            device.Commands.Add(new Command(
                "PingDevice",
                DeliveryType.Message,
                "The device responds to this command with an acknowledgement. This is useful for checking that the device is still active and listening."
            ));
            device.Commands.Add(new Command(
                "StartTelemetry",
                DeliveryType.Message,
                "Instructs the device to start sending telemetry."
            ));
            device.Commands.Add(new Command(
                "StopTelemetry",
                DeliveryType.Message,
                "Instructs the device to stop sending telemetry."
            ));
            device.Commands.Add(new Command(
                "ChangeSetPointTemp",
                DeliveryType.Message,
                "Controls the simulated temperature telemetry values the device sends. This is useful for testing back-end logic.",
                new[] { new Parameter("SetPointTemp", "double") }
            ));
            device.Commands.Add(new Command(
                "DiagnosticTelemetry",
                DeliveryType.Message,
                "Controls if the device should send the external temperature as telemetry.",
                new[] { new Parameter("Active", "boolean") }
            ));
            device.Commands.Add(new Command(
                "ChangeDeviceState",
                DeliveryType.Message,
                "Sets the device state metadata property that the device reports. This is useful for testing back-end logic.",
                new[] { new Parameter("DeviceState", "string") }
            ));

            // Device methods
            device.Commands.Add(new Command(
                "InitiateFirmwareUpdate",
                DeliveryType.Method,
                "Updates device Firmware. Use parameter 'FwPackageUri' to specifiy the URI of the firmware file, e.g. https://iotrmassets.blob.core.windows.net/firmwares/FW20.bin",
                new[] { new Parameter("FwPackageUri", "string") }
            ));
            device.Commands.Add(new Command(
                "Reboot",
                DeliveryType.Method,
                "Reboot the device"
            ));
            device.Commands.Add(new Command(
                "FactoryReset",
                DeliveryType.Method,
                "Reset the device (including firmware and configuration) to factory default state"
            ));
        }

        public static List<string> GetDefaultDeviceNames()
        {
            return DefaultDeviceNames.Keys.ToList();
        }

        public static void AssignDefaultTags(DeviceModel device)
        {
            if (device.Twin == null)
            {
                device.Twin = new Twin();
            }

            string displayName;
            if (DefaultDeviceNames.TryGetValue(device.DeviceProperties.DeviceID, out displayName))
            {
                device.Twin.Tags["DisplayName"] = displayName;
            }

            var location = Random(_possibleDeviceLocations);

            device.Twin.Tags["Building"] = location.Building;
            device.Twin.Tags["Floor"] = Random(location.Floors);

            const double deltaLatitude = 0.006334;
            const double deltaLongitude = 0.006933;
            device.DeviceProperties.Longitude = location.Longitude + (Rand.NextDouble() - 0.5) / 2000 + deltaLongitude;
            device.DeviceProperties.Latitude = location.Latitude + (Rand.NextDouble() - 0.5) / 2000 + deltaLatitude;
        }

        public static void AssignDefaultDesiredProperties(DeviceModel device)
        {
            if (HighTemperatureDeviceNames.Any(n => device.DeviceProperties.DeviceID.StartsWith(n, StringComparison.Ordinal)))
            {
                if (device.Twin == null)
                {
                    device.Twin = new Twin();
                }

                device.Twin.Properties.Desired.Set("Config.TemperatureMeanValue", 30);
            }
        }

        private static T Random<T>(IList<T> range)
        {
            return range[Rand.Next(range.Count)];
        }
    }
}
