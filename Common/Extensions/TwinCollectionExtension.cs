using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    static public class TwinCollectionExtension
    {
        public class TwinValue
        {
            public JValue Value { get; set; }
            public DateTime? LastUpdated { get; set; }

            public override string ToString()
            {
                return JObject.FromObject(this).ToString(Newtonsoft.Json.Formatting.None);
            }
        }

        /// <summary>
        /// Enumerate all the items inside TwinCollection (tags or properties), output the flat name and value (as JVaule)
        /// Reminder: It could only be used on Twin which use TwinCollection or JContainer for hierarchical values. The Twin retrieved from IoT Hub is a good sample
        /// </summary>
        /// <param name="collection">Twin.Tag, Twin.Properties.Desired or Twin.Properties.Reported</param>
        /// <param name="prefix">Custom specified prefix for all items, e.g. "tags."</param>
        /// <param name="IgnoreNullValue">Indicate whether to skip items with null value</param>
        /// <returns>Enumerator returns the flat name and value</returns>
        static public IEnumerable<KeyValuePair<string, TwinValue>> AsEnumerableFlatten(this TwinCollection collection, string prefix = "", bool IgnoreNullValue = true)
        {
            if (collection == null)
            {
                throw new ArgumentNullException();
            }

            foreach (KeyValuePair<string, object> pair in collection)
            {
                if (pair.Value is TwinCollection)
                {
                    var results = AsEnumerableFlatten(pair.Value as TwinCollection, $"{prefix}{pair.Key}.", IgnoreNullValue);
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
                else if (pair.Value is JContainer)
                {
                    var results = AsEnumerableFlatten(pair.Value as JContainer, $"{prefix}{pair.Key}.", IgnoreNullValue);
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
                else if (pair.Value is JValue)
                {
                    var value = pair.Value as JValue;
                    if (value.Type != JTokenType.Null)
                    {
                        yield return new KeyValuePair<string, TwinValue>($"{prefix}{pair.Key}", new TwinValue
                        {
                            Value = value,
                            LastUpdated = (pair.Value as TwinCollectionValue)?.GetLastUpdated()
                        });
                    }
                    else
                    {
                        if (!IgnoreNullValue)
                        {
                            yield return new KeyValuePair<string, TwinValue>($"{prefix}{pair.Key}", new TwinValue
                            {
                                Value = JValue.CreateNull(),
                                LastUpdated = (pair.Value as TwinCollectionValue)?.GetLastUpdated()
                            });
                        }
                    }
                }
                else
                {
#if DEBUG
                    throw new ApplicationException($"Unexpected TwinCollection item type: {pair.Value.GetType().FullName} @ {prefix}{pair.Key}");
#endif
                }
            }
        }

        static private IEnumerable<KeyValuePair<string, TwinValue>> AsEnumerableFlatten(this JContainer container, string prefix = "", bool IgnoreNullValue = true)
        {
            foreach (var child in container.Children<JProperty>())
            {
                if (child.Value is JContainer)
                {
                    var results = AsEnumerableFlatten(child.Value as JContainer, $"{prefix}{child.Name}.", IgnoreNullValue);
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                }
                else if (child.Value is JValue)
                {
                    var value = child.Value as JValue;
                    if (value.Type != JTokenType.Null)
                    {
                        yield return new KeyValuePair<string, TwinValue>($"{prefix}{child.Name}", new TwinValue
                        {
                            Value = value,
                            LastUpdated = (child.Value as TwinCollectionValue)?.GetLastUpdated()
                        });
                    }
                    else
                    {
                        if (!IgnoreNullValue)
                        {
                            yield return new KeyValuePair<string, TwinValue>($"{prefix}{child.Name}", new TwinValue
                            {
                                Value = JValue.CreateNull(),
                                LastUpdated = (child.Value as TwinCollectionValue)?.GetLastUpdated()
                            });
                        }
                    }
                }
                else
                {
#if DEBUG
                    throw new ApplicationException($"Unexpected TwinCollection item JTokenType: {child.Value.Type} @ {prefix}{child.Name}");
#endif
                }
            }
        }

        /// <summary>
        /// Get value of given tag or property specified by the flat name
        /// </summary>
        /// <param name="collection">Twin.Tag, Twin.Properties.Desired or Twin.Properties.Reported</param>
        /// <param name="flatName">The flat name with prefix such as 'tags.' and so on</param>
        /// <returns>The value in dynamic, or null in case error</returns>
        static public dynamic Get(this TwinCollection collection, string flatName)
        {
            if (collection == null || string.IsNullOrWhiteSpace(flatName))
            {
                throw new ArgumentNullException();
            }

            return Get(collection, flatName.Split('.'));
        }

        static private dynamic Get(this TwinCollection collection, IEnumerable<string> names)
        {
            var name = names.First();

            // Pick node on current level
            if (!collection.Contains(name))
            {
                // No desired node found. Return null as error
                return null;
            }
            var child = collection[name];

            if (names.Count() == 1)
            {
                // Current node is the target node, , return the value
                return child;
            }
            else if (child is TwinCollection)
            {
                // Current node is container, go to next level
                return Get(child as TwinCollection, names.Skip(1));
            }
            else if (child is JContainer)
            {
                // Current node is container, go to next level
                return Get(child as JContainer, names.Skip(1));
            }
            else
            {
                // Currently, the container could only be TwinCollection or JContainer
#if DEBUG
                throw new ApplicationException(FormattableString.Invariant($"Unexpected TwinCollection item type: {child.GetType().FullName} @ ...{name}"));
#else
                return null;
#endif
            }
        }

        static private dynamic Get(this JContainer container, IEnumerable<string> names)
        {
            var name = names.First();

            // Pick node on current level
            var child = container[name];
            if (child == null)
            {
                // No desired node found. Return null as error
                return null;
            }

            if (names.Count() == 1)
            {
                // Current node is the target node, return the value
                return child;
            }
            else if (child is JContainer)
            {
                // Current node is container, go to next level
                return Get(child as JContainer, names.Skip(1));
            }
            else
            {
                // The next level of JContainer must be JContainer
#if DEBUG
                throw new ApplicationException(FormattableString.Invariant($"Unexpected TwinCollection item JTokenType: {child.Type} @ {child.Path}"));
#else
                return null;
#endif
            }
        }

        /// <summary>
        /// Add/Set value of given tag or property
        /// Reminder: it is not allow to set value on the node has children
        /// </summary>
        /// <param name="collection">Twin.Tag, Twin.Properties.Desired or Twin.Properties.Reported</param>
        /// <param name="flatName">The flat name with prefix such as 'tags.' and so on</param>
        /// <param name="value">The value to be set</param>
        static public void Set(this TwinCollection collection, string flatName, dynamic value)
        {
            if (collection == null || string.IsNullOrWhiteSpace(flatName))
            {
                throw new ArgumentNullException();
            }

            Set(collection, flatName.Split('.'), value);
        }

        static private void Set(this TwinCollection collection, IEnumerable<string> names, dynamic value)
        {
            var name = names.First();

            if (names.Count() == 1)
            {
                // Current node is the target node, set the value
                collection[name] = value;
            }
            else if (!collection.Contains(name))
            {
                // Current node is container, go to next level
                // The target collection is not exist, create and add it
                // Reminder: the 'add' operation perform 'copy' rather than 'link'
                var newCollection = new TwinCollection();
                Set(newCollection, names.Skip(1), value);
                collection[name] = newCollection;
            }
            else
            {
                var child = collection[name];
                if (child is TwinCollection)
                {
                    // The target collection is there. Go to next level
                    Set(child as TwinCollection, names.Skip(1), value);
                }
                else if (child is JContainer)
                {
                    // The target collection is there. Go to next level
                    Set(child as JContainer, names.Skip(1), value);
                }
                else
                {
                    // Currently, the container could only be TwinCollection or JContainer
#if DEBUG
                    throw new ApplicationException(FormattableString.Invariant($"Unexpected TwinCollection item type: {child.GetType().FullName} @ ...{name}"));
#endif
                }
            }
        }

        static private void Set(this JContainer container, IEnumerable<string> names, dynamic value)
        {
            var name = names.First();

            if (names.Count() == 1)
            {
                // Current node is the target node, set the value
                container[name] = value;
            }
            else
            {
                var child = container.SelectToken(name);
                if (child == null)
                {
                    // The target container is not exist, create and add it
                    var newContainer = new JObject();
                    Set(newContainer, names.Skip(1), value);
                    container[name] = newContainer;
                }
                else
                {
                    // Current node is container, go to next level
                    if (child is JContainer)
                    {
                        Set(child as JContainer, names.Skip(1), value);
                    }
                    else
                    {
                        // The next level of JContainer must be JContainer
#if DEBUG
                        throw new ApplicationException(FormattableString.Invariant($"Unexpected TwinCollection item JTokenType: {child.Type} @ {child.Path}"));
#endif
                    }
                }
            }
        }
    }
}
