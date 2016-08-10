using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class CoolerDeviceFactoryTests
    {
        private CoolerDeviceFactory _coolerDeviceFactory;
        private IFixture _fixture;

        public CoolerDeviceFactoryTests()
        {
            _coolerDeviceFactory = new CoolerDeviceFactory();
            _fixture = new Fixture();
        }

        [Fact]
        public void CreateDeviceTest()
        {
            var loggerMock = new Mock<ILogger>();
            var transportFactoryMock = new Mock<ITransportFactory>();
            var telemetryFactoryMock = new Mock<ITelemetryFactory>();
            var configurationProviderMock = new Mock<IConfigurationProvider>();
            var initialConfig = _fixture.Create<InitialDeviceConfig>();


            var retDevice = _coolerDeviceFactory.CreateDevice(loggerMock.Object, transportFactoryMock.Object,
                telemetryFactoryMock.Object, configurationProviderMock.Object, initialConfig);
            Assert.NotNull(retDevice);
        }
    }
}