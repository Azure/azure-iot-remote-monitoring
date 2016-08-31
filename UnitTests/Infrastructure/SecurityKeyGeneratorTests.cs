using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class SecurityKeyGeneratorTests
    {
        [Fact]
        public void CreateRandomKeysTest()
        {
            ISecurityKeyGenerator securityKeyGenerator = new SecurityKeyGenerator();
            var keys1 = securityKeyGenerator.CreateRandomKeys();
            var keys2 = securityKeyGenerator.CreateRandomKeys();

            Assert.NotNull(keys1);
            Assert.NotEqual(keys1.PrimaryKey, keys1.SecondaryKey);
            Assert.NotEqual(keys1.PrimaryKey, keys2.PrimaryKey);
            Assert.NotEqual(keys1.SecondaryKey, keys2.SecondaryKey);
        }
    }
}