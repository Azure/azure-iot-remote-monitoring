using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnumSafe<T>(this string s)
            where T : struct
        {
            return (IsEnum<T>(s) ? (T)Enum.Parse(typeof(T), s) : default(T));
        }

        public static bool IsEnum<T>(this string s)
        {
            return Enum.IsDefined(typeof(T), s);
        }

        public static DeviceManagement.Infrustructure.Connectivity.Models.Enums.CellularProviderEnum ConvertCellularProviderEnum(this CellularProviderEnum cellularProviderEnum)
        {
            switch (cellularProviderEnum)
            {
                case CellularProviderEnum.Jasper:
                    return DeviceManagement.Infrustructure.Connectivity.Models.Enums.CellularProviderEnum.Jasper;
                case CellularProviderEnum.Ericsson:
                    return DeviceManagement.Infrustructure.Connectivity.Models.Enums.CellularProviderEnum.Ericsson;
                default:
                    throw new IndexOutOfRangeException($"Could not match enum {cellularProviderEnum.ToString()}");
            }
        }
    }
}