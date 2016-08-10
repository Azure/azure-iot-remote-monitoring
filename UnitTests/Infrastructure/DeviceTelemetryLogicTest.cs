using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceTelemetryLogicTest
    {
        private readonly DeviceTelemetryLogic _deviceTelemetryLogic;
        private readonly Mock<IDeviceTelemetryRepository> _deviceTelemetryRepositoryMock;
        private readonly Fixture fixture;

        public DeviceTelemetryLogicTest()
        {
            _deviceTelemetryRepositoryMock = new Mock<IDeviceTelemetryRepository>();
            _deviceTelemetryLogic = new DeviceTelemetryLogic(_deviceTelemetryRepositoryMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public void ProduceGetLatestDeviceAlertTimeTest()
        {
            var history = fixture.Create<List<AlertHistoryItemModel>>();
            var getAlertTime = _deviceTelemetryLogic.ProduceGetLatestDeviceAlertTime(history);

            Assert.Equal(history[0].Timestamp, getAlertTime(history[0].DeviceId));
        }
    }
}