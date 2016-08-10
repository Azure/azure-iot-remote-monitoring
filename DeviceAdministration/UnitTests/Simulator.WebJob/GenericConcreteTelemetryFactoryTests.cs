
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class GenericConcreteTelemetryFactoryTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private GenericConcreteTelemetryFactory telemetryFactory;
        private Mock<IDevice> deviceMock;

        public GenericConcreteTelemetryFactoryTests()
        {
            _loggerMock = new Mock<ILogger>();
            this.telemetryFactory = new GenericConcreteTelemetryFactory(this._loggerMock.Object);
            this.deviceMock = new Mock<IDevice>();
        }

        [Fact]
        public void PopulateDeviceWithTelemetryEventsTest()
        { 
            List<ITelemetry> list = new List<ITelemetry>();
            this.deviceMock.SetupGet<List<ITelemetry>>(mock => mock.TelemetryEvents).Returns(list);

            this.telemetryFactory.PopulateDeviceWithTelemetryEvents(this.deviceMock.Object);

            Assert.Equal(list.Count, 2);
        }
    }
}