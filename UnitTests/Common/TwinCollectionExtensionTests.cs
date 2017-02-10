using System;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class TwinCollectionExtensionTests
    {
        [Fact]
        public void AsEnumerableFlattenTest()
        {
            var twin = BuildRetrievedTwin();
            var tags = twin.Tags.AsEnumerableFlatten().ToArray();

            Assert.Equal(tags[0].Key, "Location.Country");
            Assert.Equal(tags[0].Value.Value.Type, JTokenType.String);
            Assert.Equal((string)tags[0].Value.Value.Value, "China");

            Assert.Equal(tags[1].Key, "Location.City");
            Assert.Equal(tags[1].Value.Value.Type, JTokenType.String);
            Assert.Equal((string)tags[1].Value.Value.Value, "Beijing");

            Assert.Equal(tags[2].Key, "Location.Zip");
            Assert.Equal(tags[2].Value.Value.Type, JTokenType.Integer);
            Assert.Equal((long)tags[2].Value.Value.Value, 100080);

            Assert.Equal(tags[3].Key, "LastTelemetry.Compress");
            Assert.Equal(tags[3].Value.Value.Type, JTokenType.Boolean);
            Assert.Equal((bool)tags[3].Value.Value.Value, false);

            Assert.Equal(tags[4].Key, "LastTelemetry.Timestamp");
            Assert.Equal(tags[4].Value.Value.Type, JTokenType.Date);
            Assert.Equal((DateTime)tags[4].Value.Value, new DateTime(2016, 1, 1));

            Assert.Equal(tags[5].Key, "LastTelemetry.Telemetry.Temperature");
            Assert.Equal(tags[5].Value.Value.Type, JTokenType.Float);
            Assert.Equal((double)tags[5].Value.Value.Value, 30.5);

            Assert.Equal(tags[6].Key, "LastTelemetry.Telemetry.Humidity");
            Assert.Equal(tags[6].Value.Value.Type, JTokenType.Integer);
            Assert.Equal((long)tags[6].Value.Value.Value, 20);

            Assert.Equal(tags[7].Key, "DisplayName");
            Assert.Equal(tags[7].Value.Value.Type, JTokenType.String);
            Assert.Equal((string)tags[7].Value.Value.Value, "Device001");
        }

        [Fact]
        public void AsEnumerableFlattenWithNullValueTest()
        {
            var twin = BuildRetrievedTwin();
            var tags = twin.Tags.AsEnumerableFlatten("", false).ToArray();

            Assert.Equal(tags[8].Key, "DeletedName");
            Assert.Equal(tags[8].Value.Value.Type, JTokenType.Null);
            Assert.Equal((string)tags[8].Value.Value.Value, null);

            var desired = twin.Properties.Desired.AsEnumerableFlatten("", false).ToArray();
            Assert.Equal(desired[4].Key, "DeletedName");
            Assert.Equal(desired[4].Value.Value.Type, JTokenType.Null);
            Assert.Equal((string)desired[4].Value.Value.Value, null);
        }

        [Fact]
        public void GetTest()
        {
            var twin = BuildRetrievedTwin();

            Assert.Equal(twin.Tags.Get("DisplayName").ToString(), "Device001");
            Assert.Equal(twin.Tags.Get("Location.City").ToString(), "Beijing");
            Assert.Equal((double)twin.Tags.Get("LastTelemetry.Telemetry.Temperature"), 30.5);

            Assert.Null(twin.Tags.Get("x"));
            Assert.Throws<ArgumentNullException>(() => twin.Tags.Get(null));
        }

        [Fact]
        public void SetTest()
        {
            var twin = BuildRetrievedTwin();

            // Replace leaf
            twin.Tags.Set("Location.City", "Shanghai");
            Assert.Equal(twin.Tags.Get("Location.City").ToString(), "Shanghai");

            // Add leaf (same level)
            twin.Tags.Set("Location.Dummy", "n/a");
            Assert.Equal(twin.Tags.Get("Location.Dummy").ToString(), "n/a");

            // Replace intermedia node
            twin.Tags.Set("LastTelemetry.Telemetry", 3);
            Assert.Equal((int)twin.Tags.Get("LastTelemetry.Telemetry"), 3);

            // Replace leaf
            twin.Properties.Desired.Set("FirmwareVersion.Minor", 1);
            Assert.Equal((int)twin.Properties.Desired.Get("FirmwareVersion.Minor"), 1);

            // Replace intermedia node
            twin.Properties.Desired.Set("FirmwareVersion.Build", 1002);
            Assert.Equal((int)twin.Properties.Desired.Get("FirmwareVersion.Build"), 1002);
        }

        private Twin BuildRetrievedTwin()
        {
            var twin = new Twin();
            twin.Tags["Location"] = new
            {
                Country = "China",
                City = "Beijing",
                Zip = 100080
            };

            twin.Tags["LastTelemetry"] = new
            {
                Compress = false,
                Timestamp = new DateTime(2016, 1, 1),
                Telemetry = new
                {
                    Temperature = 30.5,
                    Humidity = 20
                }
            };

            twin.Tags["DisplayName"] = "Device001";
            twin.Tags["DeletedName"] = null;

            var build = new TwinCollection();
            build["Year"] = 2016;
            build["Month"] = 11;

            var version = new TwinCollection();
            version["Major"] = 3;
            version["Minor"] = 0;
            version["Build"] = build;
            twin.Properties.Desired["FirmwareVersion"] = version;
            twin.Properties.Desired["DeletedName"] = null;

            return JsonConvert.DeserializeObject<Twin>(twin.ToJson());
        }
    }
}
