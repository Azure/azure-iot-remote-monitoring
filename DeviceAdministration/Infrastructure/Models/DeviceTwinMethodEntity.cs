namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceTwinMethodEntity
    {
        /// <summary>
        /// A set of tags, properties and methods defined for all devices.
        /// </summary>
        public string TagName { get; set; }
        public string PropertyName { get; set; }
        public DeviceMethod Method { get; set; }
        public string ETag { get; set; }
    }

    public class DeviceMethod
    {
        public string Name { get; set; }
        public string Parameters { get; set; }
        public string Description { get; set; }
    }
}
