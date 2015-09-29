namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceRuleUpdateRequest
    {
        public DeviceRule UpdatedRule { get; set; }
        public DeviceRule UnmodifiedRule { get; set; }
    }
}
