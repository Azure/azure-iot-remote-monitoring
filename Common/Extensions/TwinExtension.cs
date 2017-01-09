using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    static public class TwinExtension
    {
        static private string PREFIX_TWIN = "twin.";
        static private string DEVICEID = "deviceId";

        static private readonly KeyValuePair<string, Func<Twin, TwinCollection>>[] _selectors = new[]
        {
            new KeyValuePair<string, Func<Twin, TwinCollection>>("tags.", twin => twin.Tags),
            new KeyValuePair<string, Func<Twin, TwinCollection>>("desired.", twin => twin.Properties.Desired),
            new KeyValuePair<string, Func<Twin, TwinCollection>>("properties.desired.", twin => twin.Properties.Desired),
            new KeyValuePair<string, Func<Twin, TwinCollection>>("reported.", twin => twin.Properties.Reported),
            new KeyValuePair<string, Func<Twin, TwinCollection>>("properties.reported.", twin => twin.Properties.Reported)
        };

        /// <summary>
        /// Read from the twin for the given flatten name
        /// </summary>
        /// <param name="twin">The source twin</param>
        /// <param name="flatName">The flat name, e.g. tags.city or properties.desired.TTL</param>
        /// <returns>The tag/property value</returns>
        static public dynamic Get(this Twin twin, string flatName)
        {
            // Remove the common prefix "twin."
            flatName = flatName.TryTrimPrefix(PREFIX_TWIN);

            // "deviceId" is a built-in property of the twin
            if (flatName == DEVICEID)
            {
                return twin.DeviceId;
            }

            // Pick the selector according to prefix of the flat name, then get the value
            string name;
            foreach (var selector in _selectors)
            {
                if (flatName.TryTrimPrefix(selector.Key, out name))
                {
                    var collection = selector.Value(twin);
                    return TwinCollectionExtension.Get(collection, name);
                }
            }

            return null;
        }

        /// <summary>
        /// Write to the twin for the given flatten
        /// </summary>
        /// <param name="twin">The destination twin</param>
        /// <param name="flatName">The flat name, e.g. tags.city or properties.desired.TTL</param>
        /// <param name="value">The tag/property value</param>
        static public void Set(this Twin twin, string flatName, dynamic value)
        {
            // Remove the common prefix "twin."
            flatName = flatName.TryTrimPrefix(PREFIX_TWIN);

            if (flatName == DEVICEID)
            {
                twin.DeviceId = value.ToString();
            }

            // Pick the selector according to prefix of the flat name, then set the value
            string name;
            foreach (var selector in _selectors)
            {
                if (flatName.TryTrimPrefix(selector.Key, out name))
                {
                    TwinCollectionExtension.Set(selector.Value(twin), name, value);
                    return;
                }
            }

            // Invalid flat name should not cause any exception
            // Write to reported properties is allowed here
        }

        /// <summary>
        /// Test if there is any twin change to be upload to IoT Hub
        /// </summary>
        /// <param name="current">Twin</param>
        /// <param name="existing">Twin</param>
        /// <returns>Returns true if tags or desired properties changed</returns>
        static public bool UpdateRequired(this Twin current, Twin existing)
        {
            return current.Tags.ToJson() != existing.Tags.ToJson() ||
                current.Properties.Desired.ToJson() != existing.Properties.Desired.ToJson();
        }

        static public IEnumerable<string> GetNameList(this IEnumerable<Twin> twins, Func<Twin, TwinCollection> selector, string prefix = "")
        {
            var nameGroups = twins.Select(twin => selector(twin).AsEnumerableFlatten(prefix).Select(pair => pair.Key));
            return nameGroups.Any() ?
                nameGroups.Aggregate((s1, s2) => s1.Union(s2)) :
                new List<string>();
        }
    }
}
