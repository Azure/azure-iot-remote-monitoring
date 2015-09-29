using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    /// <summary>
    /// Methods related to dynamic values.
    /// </summary>
    public static class DynamicValuesHelper
    {
        #region Public Methods

        #region Static Method: ConvertToDateTime

        /// <summary>
        /// Converts a <c>dynamic</c> value to a <see cref="DateTime" />.
        /// </summary>
        /// <param name="value">
        /// The <c>dynamic</c> value to convert to a <see cref="DateTime" />.
        /// </param>
        /// <returns>
        /// <paramref name="value" />, converted to a <see cref="DateTime" />, 
        /// or <c>null</c>, if no such conversion is possible.
        /// </returns>
        public static DateTime? ConvertToDateTime(dynamic value)
        {
            DateTime dt;

            dt = default(DateTime);
            if (value is DateTime)
            {
                return (DateTime)value;
            }
            else if (value is DateTime?)
            {
                return (DateTime?)value;
            }
            else if ((value != null) &&
                DateTime.TryParse(value.ToString(), out dt))
            {
                return dt;
            }

            return null;
        }

        #endregion

        #region Static Method: ConvertToJsonString

        /// <summary>
        /// Converts a <c>dynamic</c> value to a JSON string.
        /// </summary>
        /// <param name="value">
        /// The <c>dynamic</c> value to convert to a JSON string.
        /// </param>
        /// <returns>
        /// <paramref name="value" />, converted to a JSON string, or 
        /// <c>null</c>, if no such conversion is possible.
        /// </returns>
        public static string ConvertToJsonString(dynamic value)
        {
            if (value != null)
            {
                return JsonConvert.SerializeObject(value);
            }

            return null;
        }

        #endregion

        #endregion
    }
}
