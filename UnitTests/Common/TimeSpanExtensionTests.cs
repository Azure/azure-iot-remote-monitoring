using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class TimeSpanExtensionTests
    {
        [Fact]
        public void TimeSpanToFloorShortStringTest()
        {
            TimeSpanExtension.Units = new TimeSpanExtension.TimeUnit[]
            {
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.FromDays(365),
                    Singular = "Year",
                    Plural = "Years"
                },
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.FromDays(1),
                    Singular = "Day",
                    Plural = "Days"
                },
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.Zero,
                    Singular = "< 1 Minute",
                    Plural = "< 1 Minute"
                }
            };

            TimeSpan? span;
            span = TimeSpan.FromDays(1);
            Assert.Equal(span.ToFloorShortString("{0} ago"), "1 Day ago");

            span += TimeSpan.FromDays(1);
            Assert.Equal(span.ToFloorShortString("{0} ago"), "2 Days ago");

            span += TimeSpan.FromDays(36500);
            Assert.Equal(span.ToFloorShortString("{0}"), "100 Years");

            span = TimeSpan.Zero;
            Assert.Equal(span.ToFloorShortString(string.Empty), "< 1 Minute");

            span = null;
            Assert.Equal(span.ToFloorShortString(string.Empty), string.Empty);
            Assert.Equal(span.ToFloorShortString("{0}"), string.Empty);
        }
    }
}
