using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

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

    static public class ExtendJobTypeExtension
    {
        static public string LocalizedString(this ExtendJobType jobType)
        {
            switch (jobType)
            {
                case ExtendJobType.ExportDevices:
                    return Strings.ExportDevicesJobType;
                case ExtendJobType.ImportDevices:
                    return Strings.ImportDevicesJobType;
                case ExtendJobType.ScheduleDeviceMethod:
                    return Strings.ScheduleDeviceMethodJobType;
                case ExtendJobType.ScheduleUpdateTwin:
                    return Strings.ScheduleUpdateTwinJobType;
                case ExtendJobType.ScheduleUpdateIcon:
                    return Strings.ScheduleUpdateIconJobType;
                case ExtendJobType.ScheduleRemoveIcon:
                    return Strings.ScheduleRemoveIconJobType;
                default:
                    return Strings.UnknownJobType;
            }
        }
    }
}