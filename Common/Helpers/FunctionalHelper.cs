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
        #region Public Methods

        #region Static Method: Memoize

        /// <summary>
        /// Saves the results of a single argument-function by argument value.
        /// </summary>
        /// <typeparam name="K">
        /// The argument's <see cref="Type" />.
        /// </typeparam>
        /// <typeparam name="R">
        /// The cached function return value's <see cref="Type" />.
        /// </typeparam>
        /// <param name="getDataFunc">
        /// A single-argument function that given a <c>K</c>, returns a 
        /// <c>R</c>.
        /// </param>
        /// <returns>
        /// A memoized version of <paramref name="getDataFunc" />.  It will 
        /// throw an <see cref="ArgumentNullException" /> if its argument is a 
        /// null reference. The returned Func is *not* thread safe.
        /// </returns>
        public static Func<K, R> Memoize<K, R>(Func<K, R> getDataFunc)
        {
            Dictionary<K, R> index;

            if (getDataFunc == null)
            {
                throw new ArgumentNullException("getDataFunc");
            }

            index = new Dictionary<K, R>();

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

        #endregion

        #region Static Method: MemoizeInHttpContext

        /// <summary>
        /// Saves the results of a single-argument function in the current 
        /// <see cref="HttpContext" />, for the duration of the current request.
        /// </summary>
        /// <typeparam name="K">
        /// The argument's <see cref="Type" />.
        /// </typeparam>
        /// <typeparam name="R">
        /// The cached function return value's <see cref="Type" />.
        /// </typeparam>
        /// <param name="getDataFunc">
        /// A single-argument function that given a <c>K</c>, returns a 
        /// <c>R</c>.
        /// </param>
        /// <returns>
        /// A memoized version of <paramref name="getDataFunc" />.  It will 
        /// throw an <see cref="ArgumentNullException" /> if its argument is a 
        /// null reference and an <see cref="InvalidOperationException" /> if 
        /// <see cref="HttpContext.Current" /> is a null reference. The
        /// returned Func is thread-safe.
        /// </returns>
        public static Func<K, R> MemoizeInHttpContext<K, R>(
            Func<K, R> getDataFunc)
        {
            string contextItemsKey;
            object sync;

            if (getDataFunc == null)
            {
                throw new ArgumentNullException("getDataFunc");
            }

            contextItemsKey = 
                Guid.NewGuid().ToString(
                    "S", 
                    CultureInfo.InvariantCulture);

            sync = new object();

            return (key) =>
            {
                HttpContext httpContext;
                Dictionary<K, R> index;
                R result;
                Exception thrownException;

                if (object.ReferenceEquals(key, null))
                {
                    throw new ArgumentNullException("key");
                }

                if ((httpContext = HttpContext.Current) == null)
                {
                    throw new InvalidOperationException(
                        "HttpContext.Current is a null reference.");
                }

                thrownException = null;
                lock (sync)
                {
                    index = 
                        httpContext.Items[contextItemsKey] as Dictionary<K, R>;
                    if (index == null)
                    {
                        index = new Dictionary<K, R>();
                        httpContext.Items.Add(contextItemsKey, index);
                    }

                    if (!index.TryGetValue(key, out result))
                    {
                        try
                        {
                            result = getDataFunc(key);
                            index.Add(key, result);
                        }
                        catch (Exception ex)
                        {
                            thrownException = ex;
                        }
                    }
                }

                // Throw the Exception outside of the lock so its handlers 
                // don't block access to the index.
                if (thrownException != null)
                {
                    throw thrownException;
                }

                return result;
            };
        }

        #endregion

        #endregion
    }
}
