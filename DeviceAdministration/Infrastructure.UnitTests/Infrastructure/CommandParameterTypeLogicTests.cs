using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

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
