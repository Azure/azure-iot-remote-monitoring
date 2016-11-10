using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    [Flags]
    public enum NameCacheEntityType
    {
        DeviceInfo = 1,
        Tag = 2,
        DesiredProperty = 4,
        ReportedProperty = 8,
        Method = 16,
        Property = DesiredProperty | ReportedProperty,
        All = DeviceInfo | Tag | DesiredProperty | ReportedProperty | Method,
    }
}
