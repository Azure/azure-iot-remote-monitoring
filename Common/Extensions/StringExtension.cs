using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    static public class StringExtension
    {
        /// <summary>
        /// Try to remove prefix from input string
        /// </summary>
        /// <param name="text">The raw string</param>
        /// <param name="prefix">The target prefix string</param>
        /// <param name="result">The prefix-removed string (or the raw string if target prefix was not found)</param>
        /// <returns>True if the target prefix was found</returns>
        static public bool TryTrimPrefix(this string text, string prefix, out string result)
        {
            if (text.StartsWith(prefix, StringComparison.InvariantCulture))
            {
                result = text.Substring(prefix.Length);
                return true;
            }
            else
            {
                result = text;
                return false;
            }
        }

        /// <summary>
        /// Try to remove prefix from input string
        /// </summary>
        /// <param name="text">The raw string</param>
        /// <param name="prefix">The target prefix string</param>
        /// <returns>prefix-removed string (or the raw string if target prefix was not found)</returns>
        static public string TryTrimPrefix(this string text, string prefix)
        {
            string result;
            text.TryTrimPrefix(prefix, out result);
            return result;
        }
    }
}
