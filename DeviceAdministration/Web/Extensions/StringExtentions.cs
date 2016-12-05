using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions
{
    public static class StringExtentions
    {
        public static string DefaultIfNull(this string value, string defaultValue = "")
        {
            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
    }
}