using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Moq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class PingDeviceProcessorTests
    {
        private readonly Mock<IDevice> _deviceBase;
        private PingDeviceProcessor _pingDeviceProcessor;
        public PingDeviceProcessorTests()
        {
            _deviceBase = new Mock<IDevice>();
            _pingDeviceProcessor = new PingDeviceProcessor(_deviceBase.Object);
        }

        [Fact]
        public async void TestCommandCannotComplete()
        {
            var history = new CommandHistory("CommandShouldNotComplete");
            var command = new DeserializableCommand(history, "LockToken");
    
            var r = await _pingDeviceProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.CannotComplete);
        }

        [Fact]
        public async void TestCommandSuccess()
        {
            var history = new CommandHistory("PingDevice");
            var command = new DeserializableCommand(history, "LockToken");

            var r = await _pingDeviceProcessor.HandleCommandAsync(command);
            Assert.Equal(r, CommandProcessingResult.Success);
        }
    }
}