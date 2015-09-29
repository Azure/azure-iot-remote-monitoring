namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    /// <summary>
    /// Device config that is read from a repository to init a set of devices
    /// in a single simulator for testing.
    /// </summary>
    public class InitialDeviceConfig
    {
        /// <summary>
        /// IoT Hub HostName
        /// </summary>
        public string HostName { get; set; }
        public string DeviceId { get; set; }
        public string Key { get; set; }
    }
}
