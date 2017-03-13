using System;
using System.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    static public class StringExtension
    {
        /// <summary>
        /// Try to remove prefix from input string
        /// </summary>
        /// <param name="s">The raw string</param>
        /// <param name="prefix">The target prefix string</param>
        /// <param name="result">The prefix-removed string (or the raw string if target prefix was not found)</param>
        /// <returns>True if the target prefix was found</returns>
        static public bool TryTrimPrefix(this string s, string prefix, out string result)
        {
            if (s.StartsWith(prefix, StringComparison.Ordinal))
            {
                result = s.Substring(prefix.Length);
                return true;
            }
            else
            {
                result = s;
                return false;
            }
        }

        /// <summary>
        /// Try to remove prefix from input string
        /// </summary>
        /// <param name="s">The raw string</param>
        /// <param name="prefix">The target prefix string</param>
        /// <returns>prefix-removed string (or the raw string if target prefix was not found)</returns>
        static public string TryTrimPrefix(this string s, string prefix)
        {
            string result;
            s.TryTrimPrefix(prefix, out result);
            return result;
        }

        /// <summary>
        /// The following charactors are not allowed for PartitionKey
        /// and RowKey property of Storage Table:
        /// The forward slash (/) character
        /// The backslash (\) character
        /// The number sign (#) character
        /// The question mark (?) character
        /// Control characters U+0000 ~ U+001F, U+007F ~ U+009F
        /// The key length must be less than 1024
        /// We also add a constraint to not using pure white space
        /// string since it is useless here.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if it is a correct key</returns>
        static public bool IsAllowedTableKey(this string key)
        {
            char[] speicialChars = @"#?/\".ToCharArray();
            if (string.IsNullOrWhiteSpace(key)
                || key.Length > 1024
                || key.IndexOfAny(speicialChars) > -1
                || key.ToCharArray().Any(c => c < 0X1F || c > 0X7F && c < 0X9F))
            {
                return false;
            }
            return true;
        }

        static public string NormalizedTableKey(this string key)
        {
            char[] speicialChars = @"#?/\".ToCharArray();
            string[] str = key.Split(speicialChars, StringSplitOptions.RemoveEmptyEntries);
            return String.Join("_", str);
        }

        /// <summary>
        /// The twin's tag and property flat name start with "__" and end with "__" is reserved
        /// for internal usage purpose.
        /// e.g.
        ///     tags.HubEnabledState
        ///     tags.__icon__, tags.cpu.__version__
        ///     desired.__location__, desired.__location.latitude
        ///     reported.__location__, reported.__location__.__latitude__
        /// </summary>
        /// <param name="flatName"></param>
        /// <returns></returns>
        static public bool IsReservedTwinName(this string flatName)
        {
            if (string.IsNullOrEmpty(flatName)) return false;
            // this line should be removed once we change it to tags.__HubEnabledState__ in the future.
            if ("tags.HubEnabledState".Equals(flatName, StringComparison.Ordinal)) return true;
            string[] parts = flatName.Split('.');
            return parts.Any(p => p.StartsWith("__", StringComparison.Ordinal) && p.EndsWith("__", StringComparison.Ordinal));
        }
    }
}
