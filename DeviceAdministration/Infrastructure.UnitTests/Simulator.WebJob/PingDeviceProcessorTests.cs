using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Simulator.WebJob
{
    public class PingDeviceProcessorTests
    {
        public PingDeviceProcessorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _transportFactory = new Mock<ITransportFactory>();
            _telemetryFactoryMock = new Mock<ITelemetryFactory>();
            _configurationProviderMock = new Mock<IConfigurationProvider>();

            deviceBase = new DeviceBase(_loggerMock.Object, _transportFactory.Object, _telemetryFactoryMock.Object,
                _configurationProviderMock.Object);
        }

        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ITransportFactory> _transportFactory;
        private readonly Mock<ITelemetryFactory> _telemetryFactoryMock;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly IDevice deviceBase;

        [Fact]
        public async void testProcessor()
        {
            var history = new CommandHistory("test");
            var command = new DeserializableCommand(history, "LockToken");
            var processor = new PingDeviceProcessor(deviceBase);

            var r = await processor.HandleCommandAsync(command);
        }
    }
}