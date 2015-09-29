using System;
using System.Globalization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    public static class StringExtensions
    {
        public static string FormatInvariant(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static bool IsBase64(this string base64String)
        {
            // Based on http://stackoverflow.com/a/6309439

            // pre-test to check if it's obviously invalid
            // (fast for long values like images and videos)
            if (string.IsNullOrEmpty(base64String) ||
                base64String.Length % 4 != 0 ||
                base64String.Contains(" ") ||
                base64String.Contains("\t") ||
                base64String.Contains("\r") ||
                base64String.Contains("\n"))
            {
                return false;
            }

            // now do the real test
            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
