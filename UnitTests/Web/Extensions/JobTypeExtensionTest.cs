using System;
using Xunit;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

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

        [Fact]
        public void ExtendJobTypeLocalizedTest()
        {
            Assert.Equal(Strings.ExportDevicesJobType, ExtendJobType.ExportDevices.LocalizedString());
            Assert.Equal(Strings.ImportDevicesJobType, ExtendJobType.ImportDevices.LocalizedString());
            Assert.Equal(Strings.ScheduleUpdateTwinJobType, ExtendJobType.ScheduleUpdateTwin.LocalizedString());
            Assert.Equal(Strings.ScheduleUpdateTwinJobType, ExtendJobType.ScheduleUpdateTwin.LocalizedString());
            Assert.Equal(Strings.ScheduleRemoveIconJobType, ExtendJobType.ScheduleRemoveIcon.LocalizedString());
            Assert.Equal(Strings.ScheduleUpdateIconJobType, ExtendJobType.ScheduleUpdateIcon.LocalizedString());
            Assert.Equal(Strings.ScheduleDeviceMethodJobType, ExtendJobType.ScheduleDeviceMethod.LocalizedString());
            Assert.Equal(Strings.UnknownJobType, ExtendJobType.Unknown.LocalizedString());

        }
    }
}
