using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class DynamicConverter
    {
        public static T ValidateAndConvert<T>(dynamic dynamicObj)
        {

            string dynamicObjStr = Newtonsoft.Json.JsonConvert.SerializeObject(dynamicObj);
            T strongObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dynamicObjStr);
            var strongObjStr = Newtonsoft.Json.JsonConvert.SerializeObject(strongObj);
            //if (!Validate<T>(dynamicObj, strongObj))
            //{
            //    throw new Exception(string.Format("Conversion failed for type: {0}", typeof(T)));
            //}
            return strongObj;
        }

        public static bool Validate<T>(dynamic dynamicObject, T typedObject)
        {
            // Base cases
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
                // If the object is an implementation of IDynamicMetaObjectProvider, then it is dynamic
                // else it should be a primitive typed object and we can do a == comparison
                if (dynamicObject is IDynamicMetaObjectProvider)
                {
                    bool passed = true;
                    foreach(var item in dynamicObject)
                    {
                        string prop = item.Name;
                        var dynamicValue = item.Value;

                        var a = typedObject.GetType();
                        var typedProp = typedObject.GetType().GetProperty(prop);
                        if (typedProp != null)
                        {
                            var typedType = typedObject.GetType();
                            var typedValue = typedProp.GetValue(typedObject);
                            if (typedValue != null)
                            {
                                // Handle list of dynamic objects
                                if (typedValue.GetType().IsGenericType && (typedValue.GetType().GetGenericTypeDefinition() == typeof(List<>)))
                                {
                                    // Constructing call for generic function
                                    Type nestedType = typedValue.GetType().GetGenericArguments().Single();

                                    MethodInfo method =
                                        typeof(DynamicConverter).GetMethod("Validate")
                                            .MakeGenericMethod(new Type[] { nestedType });

                                    int index = 0;
                                    IEnumerable enumerableTypedValue = typedValue as IEnumerable;
                                    foreach (var valItem in enumerableTypedValue)
                                    {
                                        var dynamicValueAtIndex = dynamicValue[index];
                                        var typedValueAtIndex = Convert.ChangeType(valItem, nestedType);
                                        passed = (bool)method.Invoke(null, new object[] { dynamicValueAtIndex, typedValueAtIndex });
                                        if (!passed)
                                        {
                                            break;
                                        }
                                        index++;
                                    }
                                }
                                else
                                {
                                    // Constructing call for generic function
                                    Type nestedType = typedValue.GetType();

                                    MethodInfo method =
                                        typeof(DynamicConverter).GetMethod("Validate")
                                            .MakeGenericMethod(new Type[] { nestedType });
                                    typedValue = Convert.ChangeType(typedProp.GetValue(typedObject), nestedType);

                                    // dynamicValue is an object, so checking if it has any properties
                                    // else it is a primitive type object
                                    if (dynamicValue.GetType().GetProperty("HasValues").GetValue(dynamicValue, null))
                                    {
                                        passed = (bool)method.Invoke(null, new object[] { dynamicValue, typedValue });
                                    }
                                    else
                                    {
                                        passed =
                                            (bool)method.Invoke(null, new object[] { dynamicValue.Value, typedValue });
                                    }
                                }
                            }
                            else
                            {
                                // if both dynamic and typed object values are null, then pass, else fail
                                if (dynamicValue.Value != null)
                                {
                                    passed = false;
                                }
                            }
                            // if all properties pass, then pass, else fail
                            if (!passed)
                            {
                                return false;
                            }
                        }
                        // if strongly typed object doesn't contain a property present in the dynamic object, it fails
                        // but not vice-versa
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
                // compare primitive typed object
                else
                {
                    return (dynamicObject == typedObject);
                }
            }
        }
    }
}