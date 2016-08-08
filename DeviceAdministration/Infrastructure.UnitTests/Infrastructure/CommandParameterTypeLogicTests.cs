using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.BusinessLogic
{
    public class CommandParameterTypeLogicTests
    {
        [Fact]
        public void HandlesIntAsInt32()
        {
            Assert.Equal(CommandTypes.Types["int32"], CommandTypes.Types["int"]);
        }
    }
}