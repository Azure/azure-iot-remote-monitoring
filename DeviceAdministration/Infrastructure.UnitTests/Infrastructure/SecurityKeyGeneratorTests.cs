using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests
{
    public class SecurityKeyGeneratorTests
    {
        [Fact]
        public void CreateRandomKeysTest()
        {
            ISecurityKeyGenerator securityKeyGenerator = new SecurityKeyGenerator();
            SecurityKeys keys1 = securityKeyGenerator.CreateRandomKeys();
            SecurityKeys keys2 = securityKeyGenerator.CreateRandomKeys();

            Assert.NotNull(keys1);
            Assert.NotEqual(keys1.PrimaryKey, keys1.SecondaryKey);
            Assert.NotEqual(keys1, keys2); 
        }
    }
}