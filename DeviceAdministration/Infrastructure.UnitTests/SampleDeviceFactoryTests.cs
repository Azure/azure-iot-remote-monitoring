using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests
{
    [TestFixture]
    public class SampleDeviceFactoryTests
    {
        private Device d;
        [SetUp]
        public void BuildDevice()
        {
            d = DeviceSchemaHelper.BuildDeviceStructure("test", true, null);

        }
        [Test]
        public void testGetSampleSimulatedDevice()
        {

        }

    }
}
