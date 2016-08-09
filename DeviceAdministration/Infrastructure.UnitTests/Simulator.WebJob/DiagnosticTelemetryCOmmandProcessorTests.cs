using System;
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
    public class DiagnosticTelemetryCommandProcessorTests
    {
        private readonly Mock<CoolerDevice> _coolerDevice;
        private readonly DiagnosticTelemetryCommandProcessor _diagnosticTelemetryCommandProcessor;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ITelemetryFactory> _telemetryFactoryMock;
        private readonly Mock<ITransportFactory> _transportFactory;
        public DiagnosticTelemetryCommandProcessorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _transportFactory = new Mock<ITransportFactory>();
            _telemetryFactoryMock = new Mock<ITelemetryFactory>();
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _coolerDevice = new Mock<CoolerDevice>(_loggerMock.Object, _transportFactory.Object, _telemetryFactoryMock.Object,
                _configurationProviderMock.Object);
            _diagnosticTelemetryCommandProcessor = new DiagnosticTelemetryCommandProcessor(_coolerDevice.Object);
        }

        [Fact]
        public async void CannotCompleteCommandTests()
        {
            var history = new CommandHistory("CommandShouldNotComplete");
            var command = new DeserializableCommand(history, "LockToken");
            //null pararameters
            var r = await _diagnosticTelemetryCommandProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.CannotComplete);
        }

        [Fact]
        public async void CannotCompleteCommandFromParametersTest()
        { 
           CommandHistory history = new CommandHistory("DiagnosticTelemetry");
           var command = new DeserializableCommand(history, "LockToken");
           history.Parameters = new ExpandoObject();
           
            //no active property
           history.Parameters.Active = false;
           var r = await _diagnosticTelemetryCommandProcessor.HandleCommandAsync(command);
           Assert.Equal(r, CommandProcessingResult.RetryLater);
        }

        [Fact]
        public async void CommandSuccessTests()
        {
            var history = new CommandHistory("DiagnosticTelemetry");
            var command = new DeserializableCommand(history, "LockToken");


            var r = await _diagnosticTelemetryCommandProcessor.HandleCommandAsync(command);



        } 




    }
}
