using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public class DynamicConverter
    {
        public static T ValidateAndConvert<T>(dynamic dynamicObj)
        {

            string dynamicObjStr = Newtonsoft.Json.JsonConvert.SerializeObject(dynamicObj);
            T strongObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dynamicObjStr);
            var strongObjStr = Newtonsoft.Json.JsonConvert.SerializeObject(strongObj);
            if (!Validate<T>(dynamicObj, strongObj))
            {
                throw new Exception(string.Format("Conversion failed for type: {0}", typeof(T)));
            }
            return strongObj;
        }


        public static bool Validate<T>(dynamic dynamicObject, T typedObject)
        {
            if (dynamicObject == null && typedObject == null)
            {
                return true;
            }
            else if ((dynamicObject == null && typedObject != null) || (dynamicObject != null && typedObject == null))
            {
                return false;
            }
            else
            {
                if (dynamicObject is IDynamicMetaObjectProvider)
                {
                    bool passed = true;
                    IEnumerable<string> members = dynamicObject.GetDynamicMemberNames();
                    //foreach (KeyValuePair<string, object> kvp in dynamicObject)
                    foreach (string prop in members)
                    {
                        var dynamicValue = dynamicObject.GetType().GetProperty(prop).GetValue(dynamicObject, null);

                        //string prop = kvp.Key;
                        var typedProp = typedObject.GetType().GetField(prop);
                        if (typedProp != null)
                        {
                            var typedType = typedObject.GetType();
                            Type nestedType = typedProp.GetValue(typedObject).GetType();
                            MethodInfo method = typeof(DynamicConverter).GetMethod("Validate").MakeGenericMethod(new Type[] { nestedType });
                            var typedValue = Convert.ChangeType(typedProp.GetValue(typedObject), nestedType);
                            //var dynamicValue = kvp.Value;
                            passed = (bool)method.Invoke(null, new object[] { dynamicValue, typedValue });
                            if (!passed)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return (dynamicObject == typedObject);
                }
            }
            return true;
        }
    }
}