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
    }
}