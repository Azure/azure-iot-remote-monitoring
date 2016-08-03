using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests
{
    public class SecurityKeyGeneratorTests
    {
        [Fact]
        public void CreateRandomKeysTest()
        {
            var securityKeyGenerator = new SecurityKeyGenerator();
            var keys = securityKeyGenerator.CreateRandomKeys();

            Assert.NotNull(keys);
            Assert.NotEqual(keys.PrimaryKey, keys.SecondaryKey);
        }
    }
}