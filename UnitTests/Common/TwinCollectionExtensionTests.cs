using System;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
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

            return JsonConvert.DeserializeObject<Twin>(twin.ToJson());
        }
    }
}
