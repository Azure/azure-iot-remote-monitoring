using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System.Linq.Expressions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    /// <summary>
    /// Helper class to encapsulate interactions with the event schema (messages from the device).
    /// </summary>
    public static class EventSchemaHelper
    {
        /// <summary>
        /// Returns ObjectType for a message. Used by ASA and EventProcessor processing.
        /// </summary>
        /// <param name="eventData">Message data</param>
        /// <returns>ObjectType value, or empty string if not found</returns>
        public static string GetObjectType(dynamic eventData)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException("eventData");
            }

            var members = new List<string>();

            var metaObjectProvider = eventData as IDynamicMetaObjectProvider;
            if (metaObjectProvider != null)
            {
                var dynamicMembers = metaObjectProvider.GetMetaObject(Expression.Constant(metaObjectProvider)).GetDynamicMemberNames();
                members.AddRange(dynamicMembers);
            }

            var reflectionMembers = ((object)eventData).GetType().GetProperties().Select(m => m.Name);
            members.AddRange(reflectionMembers);

            if (!members.Any(m => m == "ObjectType"))
            {
                return "";
            }

            dynamic objectType = eventData.ObjectType;

            if (objectType == null)
            {
                return "";
            }

            return objectType.ToString();
        }
    }
}
