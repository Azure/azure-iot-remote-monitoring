using System.Dynamic;
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
    public class ChangeSetPointTempCommandProcessorTests
    {

        private readonly Mock<CoolerDevice> _coolerDevice;
        private readonly ChangeSetPointTempCommandProcessor _changeSetPointTempCommandProcessor;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ITelemetryFactory> _telemetryFactoryMock;
        private readonly Mock<ITransportFactory> _transportFactory;
        public ChangeSetPointTempCommandProcessorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _transportFactory = new Mock<ITransportFactory>();
            _telemetryFactoryMock = new Mock<ITelemetryFactory>();
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _coolerDevice = new Mock<CoolerDevice>(_loggerMock.Object, _transportFactory.Object,
                _telemetryFactoryMock.Object,
                _configurationProviderMock.Object);
            _changeSetPointTempCommandProcessor = new ChangeSetPointTempCommandProcessor(_coolerDevice.Object);
        }

        [Fact]
        public async void CannotCompleteCommandTests()
        {
            var history = new CommandHistory("CommandShouldNotComplete");
            var command = new DeserializableCommand(history, "LockToken");
            //null pararameters
            var r = await _changeSetPointTempCommandProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.CannotComplete);
        }

        [Fact]
        public async void CannotCompleteExceptionCommandTests()
        {
            var history = new CommandHistory("ChangeSetPointTemp");
            var command = new DeserializableCommand(history, "LockToken");
        
            var r = await _changeSetPointTempCommandProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.CannotComplete);
        }

        [Fact]
        public async void NoSetPointParameterCommandTests()
        {
            var history = new CommandHistory("ChangeSetPointTemp");
            var command = new DeserializableCommand(history, "LockToken");
            history.Parameters = new ExpandoObject();
            history.Parameters.setpointtemp = "1.0";

            var r = await _changeSetPointTempCommandProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.RetryLater);
        }

        [Fact]
        public async void CannotParseAsDoubleCommandTests()
        {
            var history = new CommandHistory("ChangeSetPointTemp");
            var command = new DeserializableCommand(history, "LockToken");
            history.Parameters = new ExpandoObject();
            history.Parameters.SetPointTemp = "ThisIsNotADouble";

            var r = await _changeSetPointTempCommandProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.CannotComplete);
        }
    }
}