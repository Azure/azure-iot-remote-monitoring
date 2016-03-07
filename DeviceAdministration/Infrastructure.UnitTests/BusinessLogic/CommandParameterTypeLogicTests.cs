using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.BusinessLogic
{
    [TestFixture]
    public class CommandParameterTypeLogicTests
    {
        [Test]
        public void HandlesIntAsInt32()
        {
            Assert.AreEqual(CommandTypes.Types["int32"], CommandTypes.Types["int"]);
        }
    }
}
