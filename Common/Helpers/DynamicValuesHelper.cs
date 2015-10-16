using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    /// <summary>
    /// Methods related to dynamic values.
    /// </summary>
    public static class DynamicValuesHelper
    {
        /// <summary>
        /// Converts a dynamic value to a DateTime.
        /// </summary>
        /// <param name="value">
        /// The dynamic value to convert to a DateTime.
        /// </param>
        /// <returns>
        /// value converted to a DateTime, 
        /// or null, if no such conversion is possible.
        /// </returns>
        public static DateTime? ConvertToDateTime(dynamic value)
        {
            return ConvertToDateTime(CultureInfo.CurrentCulture, value);
        }

        /// <summary>
        /// Converts a dynamic value to a DateTime.
        /// </summary>
        /// <param name="value">
        /// The dynamic value to convert to a DateTime.
        /// </param>
        /// <param name="valueCultureInfo">
        /// The CultureInfo with which value would be 
        /// formatted if it's a string.
        /// </param>
        /// <returns>
        /// value converted to a DateTime, 
        /// or null, if no such conversion is possible.
        /// </returns>
        public static DateTime? ConvertToDateTime(CultureInfo valueCultureInfo, dynamic value)
        {
            if (valueCultureInfo == null)
            {
                throw new ArgumentNullException("valueCultureInfo");
            }

            DateTime dt = default(DateTime);
            if (value is DateTime)
            {
                return (DateTime)value;
            }
            else if (value is DateTime?)
            {
                return (DateTime?)value;
            }
            else if ((value != null) &&
                DateTime.TryParse(
                    value.ToString(),
                    valueCultureInfo,
                    DateTimeStyles.AllowWhiteSpaces,
                    out dt))
            {
                return dt;
            }

            return null;
        }

        /// <summary>
        /// Converts a dynamic value to a JSON string.
        /// </summary>
        /// <param name="value">
        /// The dynamic value to convert to a JSON string.
        /// </param>
        /// <returns>
        /// value, converted to a JSON string, or 
        /// null, if no such conversion is possible.
        /// </returns>
        public static string ConvertToJsonString(dynamic value)
        {
            if (value != null)
            {
                return JsonConvert.SerializeObject(value);
            }

            return null;
        }
    }
}
