using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Serialization;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    class JsonSerializeTests
    {
        private JsonSerialize serializer;

        public JsonSerializeTests() {
            this.serializer = new JsonSerialize();
        }

        [Fact]
        public void EndToEndTest()
        {
            List<int> list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            byte[] serializedObject = this.serializer.SerializeObject(list);
            List<int> deserializedList = this.serializer.DeserializeObject<List<int>>(serializedObject);

            Assert.Equal(list, deserializedList);
        }
    }
}
