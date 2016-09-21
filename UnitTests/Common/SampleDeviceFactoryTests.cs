using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class SampleDeviceFactoryTests
    {
        [Fact]
        public void TestGetDefaultDeviceNames()
        {
            var s = SampleDeviceFactory.GetDefaultDeviceNames();
            Assert.NotEmpty(s);
        }

        [Fact]
        public void TestGetSampleDevice()
        {
            var randomnumber = new Random();
            ISecurityKeyGenerator securityKeyGenerator = new SecurityKeyGenerator();
            var keys = securityKeyGenerator.CreateRandomKeys();
            var d = SampleDeviceFactory.GetSampleDevice(randomnumber, keys);
            Assert.NotNull(d);
            Assert.NotNull(d.DeviceProperties);
            Assert.NotNull(d.DeviceProperties.DeviceID);
        }

        [Fact]
        public void TestGetSampleSimulatedDevice()
        {
            var d = DeviceCreatorHelper.BuildDeviceStructure("test", true, null);
            Assert.NotNull(d);
            Assert.Equal("test", d.DeviceProperties.DeviceID);
            Assert.Equal("normal", d.DeviceProperties.DeviceState);
            Assert.Equal(null, d.DeviceProperties.HubEnabledState);
        }
    }
}