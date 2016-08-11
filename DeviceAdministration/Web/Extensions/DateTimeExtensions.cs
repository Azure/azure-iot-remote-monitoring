using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a DateTime into its string representation. Uses format supplied and valueIdDefaultDate as the result if the
        /// DateTime is DateTime.MinValue
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <param name="valueIfDefaultDate"></param>
        /// <returns></returns>
        public static string ConvertToString(this DateTime dateTime, string format = "ddMMyyyy", string valueIfDefaultDate = "")
        {
            if (dateTime == DateTime.MinValue)
            {
                return valueIfDefaultDate;
            }
            return dateTime.ToString();
        }
    }
}