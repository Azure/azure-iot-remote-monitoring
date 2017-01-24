using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using D = Dynamitey;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    /// <summary>
    /// Methods related to reflection.
    /// </summary>
    public static class ReflectionHelper
    {
        private static readonly object[] emptyArray = new object[0];

        /// <summary>
        /// Gets an empty object array.
        /// </summary>
        public static object[] EmptyArray
        {
            get
            {
                return emptyArray;
            }
        }

        /// <summary>
        /// Gets the value of an ICustomTypeDescriptor
        /// implementation's named property.
        /// </summary>
        /// <param name="item">
        /// The ICustomTypeDescriptor implementation, from
        /// which to extract a named property's value.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="usesCaseSensitivePropertyNameMatch">
        /// A value indicating whether the property name match should be
        /// case-sensitive.
        /// </param>
        /// <param name="exceptionThrownIfNoMatch">
        /// A value indicating whether an exception should be thrown if
        /// no matching property can be found.
        /// </param>
        /// <returns>
        /// The value of the property named by propertyName on item,
        /// or a null reference if exceptionThrownIfNoMatch is false and
        /// propertyName does not name a property on item.
        /// </returns>
        private static object GetNamedPropertyValue(
            ICustomTypeDescriptor item,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (string.IsNullOrEmpty("propertyName"))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", "propertyName");
            }

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            PropertyDescriptor descriptor = default(PropertyDescriptor);
            foreach (PropertyDescriptor pd in item.GetProperties())
            {
                if (string.Equals(propertyName, pd.Name, comparisonType))
                {
                    descriptor = pd;
                    break;
                }
            }

            if (descriptor == default(PropertyDescriptor))
            {
                if (exceptionThrownIfNoMatch)
                {
                    throw new ArgumentException("propertyName does not name a property on item.", "propertyName");
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return descriptor.GetValue(item);
            }
        }

        /// <summary>
        /// Gets the value of an IDynamicMetaObjectProvider
        /// implementation's named property.
        /// </summary>
        /// <param name="item">
        /// The IDynamicMetaObjectProvider implementation, from
        /// which to extract a named property's value.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="usesCaseSensitivePropertyNameMatch">
        /// A value indicating whether the property name match should be
        /// case-sensitive.
        /// </param>
        /// <param name="exceptionThrownIfNoMatch">
        /// A value indicating whether an exception should be thrown if
        /// no matching property can be found.
        /// </param>
        /// <returns>
        /// The value of the property named by propertyName on item,
        /// or a null reference if exceptionThrownIfNoMatch is false and
        /// propertyName does not name a property on item.
        /// </returns>
        private static object GetNamedPropertyValue(
            IDynamicMetaObjectProvider item,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", "propertyName");
            }

            if (!usesCaseSensitivePropertyNameMatch || exceptionThrownIfNoMatch)
            {
                StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
                if (usesCaseSensitivePropertyNameMatch)
                {
                    comparisonType = StringComparison.CurrentCulture;
                }

                propertyName = D.Dynamic.GetMemberNames(item, true).FirstOrDefault(
                        t => string.Equals(t, propertyName, comparisonType));

                if (string.IsNullOrEmpty(propertyName))
                {
                    if (exceptionThrownIfNoMatch)
                    {
                        throw new ArgumentException("propertyName does not name a property on item", "propertyName");
                    }

                    return null;
                }
            }

            try
            {
                return D.Dynamic.InvokeGet(item, propertyName);
            }
            catch (RuntimeBinderException)
            {
                return null;
            }
        }

        // <summary>
        /// Gets the value of an item's named property.
        /// </summary>
        /// <param name="item">
        /// The item from  which to extract a named property's value.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="usesCaseSensitivePropertyNameMatch">
        /// A value indicating whether the property name match should be
        /// case-sensitive.
        /// </param>
        /// <param name="exceptionThrownIfNoMatch">
        /// A value indicating whether an exception should be thrown if
        /// no matching property can be found.
        /// </param>
        /// <returns>
        /// The value of the property named by propertyName on item,
        /// or a null reference if exceptionThrownIfNoMatch is false and
        /// propertyName does not name a property on item.
        /// </returns>
        public static object GetNamedPropertyValue(
            object item,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (string.IsNullOrEmpty("propertyName"))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", "propertyName");
            }

            ICustomTypeDescriptor customProvider;
            IDynamicMetaObjectProvider dynamicProvider;
            if ((dynamicProvider = item as IDynamicMetaObjectProvider) != null)
            {
                return GetNamedPropertyValue(
                    dynamicProvider, 
                    propertyName, 
                    usesCaseSensitivePropertyNameMatch, 
                    exceptionThrownIfNoMatch);
            }
            else if ((customProvider = item as ICustomTypeDescriptor) != null)
            {
                return GetNamedPropertyValue(
                    customProvider,
                    propertyName,
                    usesCaseSensitivePropertyNameMatch,
                    exceptionThrownIfNoMatch);
            }

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            IEnumerable<PropertyInfo> matchingProps = item.GetType().GetProperties().Where(
                    t => string.Equals(t.Name, propertyName, comparisonType));

            MethodInfo methodInfo = matchingProps.Select(t => t.GetGetMethod()).FirstOrDefault(u => u != null);

            if (methodInfo == default(MethodInfo))
            {
                if (exceptionThrownIfNoMatch)
                {
                    throw new ArgumentException("propertyName does not name a property on item", "propertyName");
                }

                return null;
            }

            return methodInfo.Invoke(item, EmptyArray);
        }

        /// <summary>
        /// Produces a Func<dynamic, dynamic> for extracting a named
        /// property's value from a dynamic-typed item.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <param name="usesCaseSensitivePropertyNameMatch">
        /// A value indicating wether the property name match should be
        /// case-sensitive.
        /// </param>
        /// <param name="exceptionThrownIfNoMatch">
        /// A value indicating whether the produced
        /// Func<dynamic, dynamic> should throw an
        /// ArgumentException if no matching property can be
        /// found on the current item.
        /// </param>
        /// <returns>
        /// A Func<dynamic, dynamic> for extracting the value of
        /// a property, named by propertyName from a dynamic-typed item.
        /// </returns>
        public static Func<dynamic, dynamic> ProducePropertyValueExtractor(
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", "propertyName");
            }

            Func<Type, MethodInfo> getMethodInfo =
                FunctionalHelper.Memoize<Type, MethodInfo>(ProduceGetMethodExtractor(propertyName,usesCaseSensitivePropertyNameMatch));

            return (item) =>
            {
                if (object.ReferenceEquals(item, null))
                {
                    throw new ArgumentNullException("item");
                }

                ICustomTypeDescriptor customTypeDescriptor;
                IDynamicMetaObjectProvider dynamicMetaObjectProvider;
                if ((dynamicMetaObjectProvider = item as IDynamicMetaObjectProvider) != null)
                {
                    return GetNamedPropertyValue(
                        dynamicMetaObjectProvider,
                        propertyName,
                        usesCaseSensitivePropertyNameMatch,
                        exceptionThrownIfNoMatch);
                }
                else if ((customTypeDescriptor = item as ICustomTypeDescriptor) != null)
                {
                    return GetNamedPropertyValue(
                        customTypeDescriptor,
                        propertyName,
                        usesCaseSensitivePropertyNameMatch,
                        exceptionThrownIfNoMatch);
                }
                else
                {
                    MethodInfo methodInfo = getMethodInfo(item.GetType());

                    if (methodInfo == null)
                    {
                        if (exceptionThrownIfNoMatch)
                        {
                            throw new ArgumentException("propertyName does not name a property on item", "propertyName");
                        }
                    }
                    else
                    {
                        return methodInfo.Invoke(item, EmptyArray);
                    }
                }

                return null;
            };
        }

        /// <summary>
        /// Sets a named property's value.
        /// </summary>
        /// <param name="item">
        /// The ICustomTypeDescriptor implementation on which
        /// to set a named property's value.
        /// </param>
        /// <param name="newValue">
        /// The value to assign to the named property.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to set.
        /// </param>
        /// <param name="usesCaseSensitivePropertyNameMatch">
        /// A value indicating whether the property name matching should be
        /// case-sensitive.
        /// </param>
        /// <param name="exceptionThrownIfNoMatch">
        /// A value indicating whether an ArgumentException
        /// should be thrown if no matching property can be found.
        /// </param>
        public static void SetNamedPropertyValue(
            object item,
            object newValue,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (string.IsNullOrEmpty("propertyName"))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", "propertyName");
            }

            ICustomTypeDescriptor customProvider;
            if ((customProvider = item as ICustomTypeDescriptor) != null)
            {
                SetNamedPropertyValue(
                    customProvider,
                    newValue,
                    propertyName,
                    usesCaseSensitivePropertyNameMatch,
                    exceptionThrownIfNoMatch);
            }

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            IEnumerable<PropertyInfo> matchingProps = item.GetType().GetProperties().Where(
                    t => string.Equals(t.Name, propertyName, comparisonType));

            MethodInfo methodInfo = matchingProps.Select(t => t.GetSetMethod()).FirstOrDefault(u => u != null);

            if (methodInfo == default(MethodInfo))
            {
                if (exceptionThrownIfNoMatch)
                {
                    throw new ArgumentException("propertyName does not name a property on item", "propertyName");
                }
            }

            methodInfo.Invoke(item, new object[] {newValue});
        }
        public static void SetNamedPropertyValue(
            ICustomTypeDescriptor item,
            object newValue,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (string.IsNullOrEmpty("propertyName"))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", "propertyName");
            }

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            PropertyDescriptor descriptor = default(PropertyDescriptor);
            foreach (PropertyDescriptor pd in item.GetProperties())
            {
                if (string.Equals(propertyName, pd.Name, comparisonType))
                {
                    descriptor = pd;
                }
            }

            if (descriptor == default(PropertyDescriptor))
            {
                if (exceptionThrownIfNoMatch)
                {
                    throw new ArgumentException("propertyName does not name a property on item.", "propertyName");
                }
            }
            else
            {
                descriptor.SetValue(item, newValue);
            }
        }

        /// <summary>
        /// Gets an IDynamicMetaObjectProvider implementation's
        /// properties and their values as an
        /// IEnumerable<KeyValuePair<string, object>>.
        /// </summary>
        /// <param name="item">
        /// The IDynamicMetaObjectProvider implementation.
        /// </param>
        /// <returns>
        /// item's properties and their values as an IEnumerable<KeyValuePair<string, object>>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this IDynamicMetaObjectProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            foreach (string memberName in D.Dynamic.GetMemberNames(item, true))
            {
                yield return new KeyValuePair<string, object>(
                    memberName,
                    D.Dynamic.InvokeGet(item, memberName));
            }
        }

        /// <summary>
        /// Gets an ICustomTypeDescriptor implementation's
        /// properties and their values as an
        /// IEnumerable<KeyValuePair<string, object>>.
        /// </summary>
        /// <param name="item">
        /// The ICustomTypeDescriptor implementation.
        /// </param>
        /// <returns>
        /// item's properties and their values as an IEnumerable<KeyValuePair<string, object>>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this ICustomTypeDescriptor item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            foreach (PropertyDescriptor prop in item.GetProperties())
            {
                yield return new KeyValuePair<string, object>(
                    prop.Name,
                    prop.GetValue(item));
            }
        }

        /// <summary>
        /// Gets an object properties and their values as an
        /// IEnumerable<KeyValuePair<string, object>>.
        /// </summary>
        /// <param name="item">
        /// The object implementation.
        /// </param>
        /// <returns>
        /// item's properties and their values as an IEnumerable<KeyValuePair<string, object>>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this object item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            IDynamicMetaObjectProvider metaObjectProvider;
            ICustomTypeDescriptor typeDescriptor;
            if ((metaObjectProvider = item as IDynamicMetaObjectProvider) != null)
            {
                return metaObjectProvider.ToKeyValuePairs();
            }
            else if ((typeDescriptor = item as ICustomTypeDescriptor) != null)
            {
                return typeDescriptor.ToKeyValuePairs();
            }

            return item.ToKeyValuePairImpl();
        }

        private static Func<Type, MethodInfo> ProduceGetMethodExtractor(
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "propertyName is a null reference or empty string.");

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            return (type) =>
            {
                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }

                IEnumerable<PropertyInfo> properties =
                    type.GetProperties().Where(t => string.Equals(propertyName, t.Name, comparisonType));

                return properties.Select(t => t.GetGetMethod()).FirstOrDefault(u => u != null);
            };
        }

        private static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairImpl(this object item)
        {
            MethodInfo getter;

            Debug.Assert(item != null, "item is a null reference.");

            foreach (PropertyInfo prop in item.GetType().GetProperties())
            {
                if ((getter = prop.GetGetMethod()) != null)
                {
                    yield return new KeyValuePair<string, object>(
                        prop.Name,
                        getter.Invoke(item, EmptyArray));
                }
            }
        }
    }
}
