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
    /// Elsewhere in the app we try to always deal with this flexible schema as dynamic,
    /// but here we take a dependency on Json.Net where necessary to populate the objects
    /// behind the schema.
    /// </summary>
    public static class SchemaHelperND
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

        /// <summary>
        /// id is used internally by the DocDB and is sometimes required.
        /// </summary>
        /// <param name="document">Device data</param>
        /// <returns>Value of the id, or empty string if not found</returns>
        public static string GetDocDbId<T>(T document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            var id = ReflectionHelper.GetNamedPropertyValue(document, "id", true, false);

            if (id == null)
            {
                return "";
            }

            return id.ToString();
        }
    }
}
