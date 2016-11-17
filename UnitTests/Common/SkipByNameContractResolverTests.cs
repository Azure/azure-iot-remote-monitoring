using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.JsonContractResolvers;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class SkipByNameContractResolverTests
    {
        [Fact]
        public void SerializeTest()
        {
            var obj = new
            {
                FieldA = "string value",
                FieldB = 1.0,
                FieldC = DateTime.Now
            };

            var text = JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
            {
                ContractResolver = new SkipByNameContractResolver("FieldA", "FieldC")
            });

            Assert.Equal(text, "{\"FieldB\":1.0}");
        }
    }
}
