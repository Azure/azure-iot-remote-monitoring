using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Schema
{
    /// <summary>
    /// Helper class to encapsulate interactions with the device schema.
    ///
    /// </summary>
    public static class SchemaHelper
    {
        /// <summary>
        /// _rid is used internally by the DocDB and is required for use with DocDB.
        /// (_rid is resource id)
        /// </summary>
        /// <param name="document">Device data</param>
        /// <returns>_rid property value as string, or empty string if not found</returns>
        public static string GetDocDbRid<T>(T document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            var rid = ReflectionHelper.GetNamedPropertyValue(document, "_rid", true, false);

            if (rid == null)
            {
                return "";
            }

            return rid.ToString();
        }
    }
}
