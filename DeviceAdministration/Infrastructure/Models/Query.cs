namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class Query
    {
        public string Name { get; set; }

        public string QueryString { get; set; }

        public bool IsTemporary { get; set; }
    }
}
