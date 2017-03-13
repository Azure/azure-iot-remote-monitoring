using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    static public class TimeSpanExtension
    {
        public class TimeUnit
        {
            public TimeSpan Length { get; set; }
            public string Singular { get; set; }
            public string Plural { get; set; }
        }

        static public IEnumerable<TimeUnit> Units { get; set; } = null;

        static public string ToFloorShortString(this TimeSpan? input, string format)
        {
            if (!input.HasValue)
            {
                return string.Empty;
            }

            if (Units == null)
            {
                return string.Format(CultureInfo.InvariantCulture, format, input);
            }

            var timespan = input.Value;
            var unit = Units.First(u => timespan >= u.Length);

            if (unit.Length == TimeSpan.Zero)
            {
                return unit.Singular;
            }

            int number = (int)Math.Floor(timespan.TotalMinutes / unit.Length.TotalMinutes);
            string text = number == 1 ? unit.Singular : unit.Plural;
            return string.Format(CultureInfo.InvariantCulture, format, FormattableString.Invariant($"{number} {text}"));
        }
    }
}
