using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Moq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class DeviceBaseTests
    {
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ITelemetryFactory> _telemetryFactoryMock;
        private readonly Mock<ITransportFactory> _transportFactory;
        private readonly DeviceBase deviceBase;
        private Mock<ITransport> _transport;

        public DeviceBaseTests()
        {
            _loggerMock = new Mock<ILogger>();
            _transportFactory = new Mock<ITransportFactory>();
            _telemetryFactoryMock = new Mock<ITelemetryFactory>();
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _transport = new Mock<ITransport>();
            deviceBase = new DeviceBase(_loggerMock.Object, _transportFactory.Object, _telemetryFactoryMock.Object,
                _configurationProviderMock.Object);
        }

        [Fact]
        public void InitTests()
        {
            var config = new InitialDeviceConfig();
            config.HostName = "HostName";
            config.DeviceId = "test";
            config.Key = "key";

            deviceBase.Init(config);

            Assert.Equal(deviceBase.DeviceID, "test");
            Assert.Equal(deviceBase.HostName, "HostName");
            Assert.Equal(deviceBase.PrimaryAuthKey, "key");
            Assert.NotNull(deviceBase.DeviceProperties);
            Assert.NotNull(deviceBase.Commands);
            Assert.NotNull(deviceBase.Telemetry);
        }

        [Fact]
        public void GetDeviceInfoTests()
        {
            var config = new InitialDeviceConfig();
            config.HostName = "HostName";
            config.DeviceId = "test";
            config.Key = "key";

            deviceBase.Init(config);
            var device = deviceBase.GetDeviceInfo();
            Assert.Equal(device.DeviceProperties.DeviceID, "test");
            Assert.Null(device.SystemProperties);
        }
    }
}