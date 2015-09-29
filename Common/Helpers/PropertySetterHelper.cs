using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class PropertySetterHelper
    {
        /// <summary>
        /// Iterates through the properties of objectToSet, and looks for a corresponding
        /// property in objectToRead.  If that property exists and it's not null then 
        /// the objectToSet is updated with the new value.
        /// </summary>
        /// <typeparam name="T">Type of the objects being worked on</typeparam>
        /// <param name="objectToSet">The object that will get the new values</param>
        /// <param name="objectToRead">The object that contains the delta of new values to set</param>
        public static void SetPropertiesIgnoringNull<T>(T objectToSet, T objectToRead)
        {
            Type type = objectToRead.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var value = property.GetValue(objectToRead);

                if (value != null)
                {
                    var thisProperty = objectToSet.GetType().GetProperty(property.Name);

                    if (thisProperty != null)
                    {
                        thisProperty.SetValue(objectToSet, value);
                    }
                }
            }
        }

        /// <summary>
        /// Converts the provided dynamic type into a strongly type instance defined by T
        /// </summary>
        /// <typeparam name="T">Type to convert the dynamic object to</typeparam>
        /// <param name="source">Dynamic object to convert</param>
        /// <returns>Hydrated object of type T with all properties set based on the dynamic type</returns>
        public static T ToObject<T>(ExpandoObject source) where T : new()
        {
            var destination = new T();

            IDictionary<string, object> dict = source;
            var type = destination.GetType();

            foreach (var property in type.GetProperties())
            {
                var lower = property.Name.ToLower();
                var key = dict.Keys.SingleOrDefault(k => k.ToLower() == lower);

                if (key != null)
                {
                    Type underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    object safeValue = (dict[key] == null) ? null : Convert.ChangeType(dict[key], underlyingType);
                    property.SetValue(destination, safeValue);
                }
            }

            return destination;
        }
    }
}
