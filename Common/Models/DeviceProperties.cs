using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class DeviceProperties
    {
        public string DeviceID { get; set; }
        public bool? HubEnabledState { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string DeviceState { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string Manufacturer { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string AvailablePowerSources { get; set; }
        public string PowerSourceVoltage { get; set; }
        public string BatteryLevel { get; set; }
        public string MemoryFree { get; set; }
        public string HostName { get; set; }
        public string Platform { get; set; }
        public string Processor { get; set; }
        public string InstalledRAM { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public bool GetHubEnabledState()
        {
            return HubEnabledState.HasValue && HubEnabledState.Value;
        }
    }
}