using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Moq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class StartCommandProcessorTests
    {
        private Mock<CoolerDevice> _coolerDeviceMock;
        private StartCommandProcessor _startCommandProcessor;

        public StartCommandProcessorTests()
        {
            _coolerDeviceMock = new Mock<CoolerDevice>();
            _startCommandProcessor = new StartCommandProcessor(_coolerDeviceMock.Object);
        }

        [Fact]
        public async void TestCommandCannotComplete()
        {
            var history = new CommandHistory("CommandShouldNotComplete");
            var command = new DeserializableCommand(history, "LockToken");

            var r = await _startCommandProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.CannotComplete);
        }

        [Fact]
        public async void TestCommandRetryLater()
        {
            var history = new CommandHistory("StartTelemetry");
            var command = new DeserializableCommand(history, "LockToken");

            var r = await _startCommandProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.RetryLater);
        }


        [Fact]
        public async void TestCommandSuccess()
        {
            var history = new CommandHistory("StartTelemetry");
            var command = new DeserializableCommand(history, "LockToken");
            _coolerDeviceMock.Setup(x => x.StartTelemetryData());
            var r = await _startCommandProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.RetryLater);
        }
    }
}

