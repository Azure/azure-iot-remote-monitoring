using GlobalResources;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions
{
    static public class JobTypeExtension
    {
        static public string LocalizedString(this JobType jobType)
        {
            switch (jobType)
            {
                case JobType.ExportDevices:
                    return Strings.ExportDevicesJobType;
                case JobType.ImportDevices:
                    return Strings.ImportDevicesJobType;
                case JobType.ScheduleDeviceMethod:
                    return Strings.ScheduleDeviceMethodJobType;
                case JobType.ScheduleUpdateTwin:
                    return Strings.ScheduleUpdateTwinJobType;
                default:
                    return Strings.UnknownJobType;
            }
        }
    }
}