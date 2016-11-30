using System;
using Xunit;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.Extensions
{
    public class JobTypeExtensionTest
    {
        [Fact]
        public void JobTypeLocalizedTest()
        {
            Assert.Equal(Strings.ExportDevicesJobType, JobType.ExportDevices.LocalizedString());
            Assert.Equal(Strings.ImportDevicesJobType, JobType.ImportDevices.LocalizedString());
            Assert.Equal(Strings.ScheduleUpdateTwinJobType, JobType.ScheduleUpdateTwin.LocalizedString());
            Assert.Equal(Strings.ScheduleDeviceMethodJobType, JobType.ScheduleDeviceMethod.LocalizedString());
            Assert.Equal(Strings.UnknownJobType, JobType.Unknown.LocalizedString());
        }
    }
}
