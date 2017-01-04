using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class ObjectExtensionTest
    {
        class Container
        {
            public int? IntValue { get; set; }
            public double? DoubleValue { get; set; }
            public DateTime? DateTimeValue { get; set; }
            public string StringValue { get; set; }
        }

        [Fact]
        public void SetPropertyTest()
        {
            var now = DateTime.Now;

            var container = new Container();
            container.SetProperty("IntValue", new JValue(10));
            container.SetProperty("DoubleValue", new JValue(1.5));
            container.SetProperty("DateTimeValue", new JValue(now));
            container.SetProperty("StringValue", new JValue("stringValue"));

            Assert.Equal(container.IntValue, 10);
            Assert.Equal(container.DoubleValue, 1.5);
            Assert.Equal(container.DateTimeValue, now);
            Assert.Equal(container.StringValue, "stringValue");
        }

        [Fact]
        public void SetPropertyExceptionTest()
        {
            var container = new Container();
            container.SetProperty("NotExist", new JValue(10));

            Assert.Throws<ArgumentOutOfRangeException>(() => container.SetProperty("NotExist", new JValue(10), true));
        }
    }
}
