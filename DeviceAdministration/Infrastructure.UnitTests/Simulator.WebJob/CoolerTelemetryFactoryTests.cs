using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Telemetry.Factory;
using Moq;
using Xunit;
using Ploeh.AutoFixture;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    class CoolerTelemetryFactoryTests
    {
        private CoolerTelemetryFactory factory;
        private Mock<ILogger> _loggerMock;
        private IFixture _fixture;

        public CoolerTelemetryFactoryTests()
        {
            this._loggerMock = new Mock<ILogger>();
            this.factory = new CoolerTelemetryFactory(this._loggerMock.Object);
            this._fixture = new Fixture();
        }

        [Fact]
        public void PopulateDeviceWithTelemetryEventsTest()
        {
            DeviceBase device = this._fixture.Create<DeviceBase>();

            this.factory.PopulateDeviceWithTelemetryEvents(device);

            Assert.Equal(device.TelemetryEvents.Count, 2);
        }
    }
}
