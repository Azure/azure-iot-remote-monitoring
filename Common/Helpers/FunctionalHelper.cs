using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    /// <summary>
    /// Methods related to functional programming.
    /// </summary>
    public static class FunctionalHelper
    {
        /// <summary>
        /// Saves the results of a single argument-function by argument value.
        /// </summary>
        /// <typeparam name="K">
        /// The argument's type.
        /// </typeparam>
        /// <typeparam name="R">
        /// The cached function return value's type.
        /// </typeparam>
        /// <param name="getDataFunc">
        /// A single-argument function that given a K, returns an R.
        /// </param>
        /// <returns>
        /// A memoized version of getDataFunc.  It will 
        /// throw an ArgumentNullException if its argument is a 
        /// null reference. The returned Func is *not* thread safe.
        /// </returns>
        public static Func<K, R> Memoize<K, R>(Func<K, R> getDataFunc)
        {
            if (getDataFunc == null)
            {
                throw new ArgumentNullException("getDataFunc");
            }

            var index = new Dictionary<K, R>();

            return (key) =>
            {
                R result;

                if (object.ReferenceEquals(key, null))
                {
                    throw new ArgumentNullException("key");
                }

                if (!index.TryGetValue(key, out result))
                {
                    result = getDataFunc(key);
                    index.Add(key, result);
                }

                return result;
            };
        }
    }
}
