using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string ConvertToString(this DateTime dateTime, string format = "ddMMyyyy", string defaultValue = "")
        {
            return dateTime == DateTime.MinValue ? defaultValue : dateTime.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a DateTime? into its string representation. Uses format supplied and valueIdDefaultDate as the result if the
        /// DateTime is DateTime.MinValue
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string ConvertToString(this DateTime? dateTime, string format = "ddMMyyyy", string defaultValue = "")
        {
            return !dateTime.HasValue || dateTime == DateTime.MinValue ? defaultValue : dateTime.ToString();
        }
    }
}