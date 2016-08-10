﻿using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
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
        private readonly Mock<CoolerDevice> _coolerDevice;
        private StartCommandProcessor _startCommandProcessor;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ITelemetryFactory> _telemetryFactoryMock;
        private readonly Mock<ITransportFactory> _transportFactory;
        private Mock<ITransport> _transport;
        public StartCommandProcessorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _transportFactory = new Mock<ITransportFactory>();
            _telemetryFactoryMock = new Mock<ITelemetryFactory>();
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _transport = new Mock<ITransport>();
            _coolerDevice = new Mock<CoolerDevice>(_loggerMock.Object, _transportFactory.Object, _telemetryFactoryMock.Object,
                _configurationProviderMock.Object);
            _startCommandProcessor = new StartCommandProcessor(_coolerDevice.Object);
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
    }
}

