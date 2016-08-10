using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Moq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class ChangeDeviceStateCommandProcessorTests
    {

        private Mock<CoolerDevice> _coolerDevice;
        private ChangeDeviceStateCommandProcessor _changeDeviceStateCommandProcessor;
        private Mock<IConfigurationProvider> _configurationProviderMock;
        private Mock<ILogger> _loggerMock;
        private Mock<ITelemetryFactory> _telemetryFactoryMock;
        private Mock<ITransportFactory> _transportFactory;
        public ChangeDeviceStateCommandProcessorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _transportFactory = new Mock<ITransportFactory>();
            _telemetryFactoryMock = new Mock<ITelemetryFactory>();
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _coolerDevice = new Mock<CoolerDevice>(_loggerMock.Object, _transportFactory.Object,
                _telemetryFactoryMock.Object,
                _configurationProviderMock.Object);
            _changeDeviceStateCommandProcessor = new ChangeDeviceStateCommandProcessor(_coolerDevice.Object);
        }

        [Fact]
        public async void CannotCompleteCommandTests()
        {
            var history = new CommandHistory("CommandShouldNotComplete");
            var command = new DeserializableCommand(history, "LockToken");
            //null pararameters
            var r = await _changeDeviceStateCommandProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.CannotComplete);
        }
    }
}
