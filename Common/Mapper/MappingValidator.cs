using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Mapper
{
    public class MappingValidator
    {
        public static void ValidateAndThrow<T>(dynamic dynamicObject, T typedObject)
        {
            if (!Validate(dynamicObject, typedObject))
            {
                Debug.WriteLine("ERROR");
                throw new Exception(FormattableString.Invariant($"Conversion failed for type: {typeof(T)}"));
            }
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
                string dstr = dynamicObject.ToString();
                Debug.WriteLine("1. DYNAMIC: " + dstr);
                Debug.WriteLine("1. STRONG: " + typedObject.ToString());
                Debug.WriteLine("1. Either dynamic or strongly types object is null");
                return false;
            }
            else
            {
                // If the object is an implementation of IDynamicMetaObjectProvider, then it is dynamic
                // else it should be a primitive typed object and we can do a == comparison
                if (dynamicObject is IDynamicMetaObjectProvider && !(typedObject is IDynamicMetaObjectProvider))
                {
                    bool passed = true;
                    foreach (var item in dynamicObject)
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
                                        typeof(MappingValidator).GetMethod("Validate")
                                            .MakeGenericMethod(new Type[] { nestedType });

                                    int index = 0;
                                    IEnumerable enumerableTypedValue = typedValue as IEnumerable;
                                    foreach (var valItem in enumerableTypedValue)
                                    {
                                        var dynamicValueAtIndex = dynamicValue[index];
                                        var typedValueAtIndex = Convert.ChangeType(valItem, nestedType, CultureInfo.InvariantCulture);
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
                                        typeof(MappingValidator).GetMethod("Validate")
                                            .MakeGenericMethod(new Type[] { nestedType });
                                    typedValue = Convert.ChangeType(typedProp.GetValue(typedObject), nestedType, CultureInfo.InvariantCulture);

                                    // dynamicValue is an object, so checking if it has any properties
                                    // else it is a primitive type object
                                    if (dynamicValue.GetType().GetProperty("HasValues").GetValue(dynamicValue, null) || dynamicValue.Value == null)
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
                                    Debug.WriteLine("Dynamic is not null but strongly typed is null");
                                    passed = false;
                                }
                            }
                            // if all properties pass, then pass, else fail
                            if (!passed)
                            {
                                if (typedValue != null)
                                {
                                    string dstr = dynamicValue.ToString();
                                    Debug.WriteLine("2. DYNAMIC VAL: " + dstr);
                                    Debug.WriteLine("2. STRONG VAL: " + typedValue.ToString());
                                }
                                return false;
                            }
                        }
                        // if strongly typed object doesn't contain a property present in the dynamic object, it fails
                        // but not vice-versa
                        else
                        {
                            string dstr = dynamicObject.ToString();
                            Debug.WriteLine("3. DYNAMIC: " + dstr);
                            Debug.WriteLine("3. STRONG: " + typedObject.ToString());
                            Debug.WriteLine("3. Property " + prop + " not found in strongly types object");
                            return false;
                        }
                    }
                    return true;
                }
                // Special case : Parameters in CommandHistoryND is dynamic
                else if (dynamicObject is IDynamicMetaObjectProvider && typedObject is IDynamicMetaObjectProvider)
                {
                    if (dynamicObject.GetType().GetProperty("Count").GetValue(dynamicObject, null) != 0)
                    {
                        //TODO: compare two dynamic objects
                        string dstr = dynamicObject.ToString();
                        string tstr = typedObject.ToString();
                        return (dstr == tstr);
                    }
                    else
                    {
                        return true;
                    }
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
