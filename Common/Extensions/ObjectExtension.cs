using System;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    static public class ObjectExtension
    {
        static public void SetProperty(this object obj, string name, JValue value, bool throwIfNoProperty = false)
        {
            var type = obj.GetType();
            var property = type.GetProperty(name);
            if (property == null)
            {
                if (throwIfNoProperty)
                {
                    throw new ArgumentOutOfRangeException(FormattableString.Invariant($"{type} has no property {name}"));
                }
                else
                {
                    return;
                }
            }

            var intermediaType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            property.SetValue(obj, Convert.ChangeType(value.Value, intermediaType, CultureInfo.InvariantCulture));
        }
    }
}
