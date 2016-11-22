namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class NamedDeviceJob
    {
        public string Name { get; set; }
        public DeviceJob Job { get; set; }
    }
}