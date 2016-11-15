using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    static public class TwinCollectionExtension
    {
        public class TwinValue
        {
            public JValue Value { get; set; }
            public DateTime? LastUpdated { get; set; }
        }

        /// <summary>
        /// Enumerate all the items inside TwinCollection (tags or properties), output the flat name and value (as JVaule)
        /// Reminder: It could only be used on Twin which use TwinCollection or JContainer for hierarchical values. The Twin retrieved from IoT Hub is a good sample
        /// </summary>
        /// <param name="collection">Twin.Tag, Twin.Properties.Desired or Twin.Properties.Reported</param>
        /// <param name="prefix">Custom specified prefix for all items, e.g. "tags."</param>
        /// <returns>Enumerator returns the flat name and value</returns>
        static public IEnumerable<KeyValuePair<string, TwinValue>> AsEnumerableFlatten(this TwinCollection collection, string prefix = "")
        {
            foreach (KeyValuePair<string, object> pair in collection)
            {
                if (pair.Value is TwinCollection)
                {
                    var results = AsEnumerableFlatten(pair.Value as TwinCollection, $"{prefix}{pair.Key}.");
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
                else if (pair.Value is JContainer)
                {
                    var results = AsEnumerableFlatten(pair.Value as JContainer, $"{prefix}{pair.Key}.");
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
                else if (pair.Value is JValue)
                {
                    yield return new KeyValuePair<string, TwinValue>($"{prefix}{pair.Key}", new TwinValue
                    {
                        Value = pair.Value as JValue,
                        LastUpdated = (pair.Value as TwinCollectionValue)?.GetLastUpdated()
                    });
                }
#if DEBUG
                else
                {
                    throw new ApplicationException($"Unexpected TwinCollection item type: {pair.Value.GetType().FullName} @ {prefix}{pair.Key}");
                }
#endif
            }
        }

        static private IEnumerable<KeyValuePair<string, TwinValue>> AsEnumerableFlatten(this JContainer container, string prefix = "")
        {
            foreach (var child in container.Children<JProperty>())
            {
                if (child.Value is JContainer)
                {
                    var results = AsEnumerableFlatten(child.Value as JContainer, $"{prefix}{child.Name}.");
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
                else if (child.Value is JValue)
                {
                    yield return new KeyValuePair<string, TwinValue>($"{prefix}{child.Name}", new TwinValue
                    {
                        Value = child.Value as JValue,
                        LastUpdated = (child.Value as TwinCollectionValue)?.GetLastUpdated()
                    });
                }
#if DEBUG
                else
                {
                    throw new ApplicationException($"Unexpected TwinCollection item JTokenType: {child.Value.Type} @ {prefix}{child.Name}");
                }
#endif
            }
        }
    }
}
