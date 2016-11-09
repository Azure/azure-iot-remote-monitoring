using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    static public class TwinCollectionExtension
    {
        /// <summary>
        /// Enumerate all the items inside TwinCollection (tags or properties), output the flat name and value (as JVaule)
        /// Reminder: It could only be used on Twin which use JContainer for hierarchical values. The Twin retrieved from IoT Hub is a good sample
        /// </summary>
        /// <param name="collection">Twin.Tag, Twin.Properties.Desired or Twin.Properties.Reported</param>
        /// <returns>Enumerator returns the flat name and value</returns>
        static public IEnumerable<KeyValuePair<string, JValue>> AsEnumerableFlatten(this TwinCollection collection)
        {
            var tokenCollection = collection
                .OfType<KeyValuePair<string, object>>()
                .Select(pair => new KeyValuePair<string, JToken>(pair.Key, pair.Value as JToken))
                .Where(pair => pair.Value != null);

            return GetFlattenProperties(tokenCollection);
        }

        static private IEnumerable<KeyValuePair<string, JValue>> GetFlattenProperties(IEnumerable<KeyValuePair<string, JToken>> collection, string prefix = "")
        {
            foreach (var pair in collection)
            {
                if (pair.Value is JArray)
                {
                    continue;
                }
                else if (pair.Value is JContainer)
                {
                    var subCollection = (pair.Value as JContainer)
                        .Children<JProperty>()
                        .Select(p => new KeyValuePair<string, JToken>(p.Name, p.Value));

                    foreach (var subProperty in GetFlattenProperties(subCollection, $"{prefix}{pair.Key}."))
                    {
                        yield return subProperty;
                    }
                }
                else if (pair.Value is JValue)
                {
                    yield return new KeyValuePair<string, JValue>($"{prefix}{pair.Key}", pair.Value as JValue);
                }
                else
                {
                    throw new ApplicationException();
                }
            }
        }
    }
}
