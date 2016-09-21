using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class CoolerDeviceTests
    {
        private CoolerDevice _coolerDevice;
        private InitialDeviceConfig _initConfig;
        private Mock<ITransport> _transportMock;
        private IFixture _fixture;

        public CoolerDeviceTests()
        {
            _fixture = new Fixture();
            _initConfig = _fixture.Create<InitialDeviceConfig>();
            var loggerMock = new Mock<ILogger>();
            _transportMock = new Mock<ITransport>();
            var transportFactoryMock = new Mock<ITransportFactory>();
            transportFactoryMock.Setup(x => x.CreateTransport(It.IsNotNull<IDevice>())).Returns(_transportMock.Object);
            var telemetryFactoryMock = new Mock<ITelemetryFactory>();
            var configurationProviderMock = new Mock<IConfigurationProvider>();
            var coolerDeviceFactory = new CoolerDeviceFactory();
            _coolerDevice = coolerDeviceFactory.CreateDevice(loggerMock.Object, transportFactoryMock.Object,
                telemetryFactoryMock.Object, configurationProviderMock.Object, _initConfig) as CoolerDevice;
            loggerMock.Setup(x => x.LogInfo(It.IsAny<string>(), It.IsAny<object[]>()));
        }

        [Fact]
        public async void ChangeDeviceStateTest()
        {
            _transportMock.Setup(x => x.SendEventAsync(It.IsAny<object>())).Returns(Task.FromResult(true)).Verifiable();
            var newState = "NewDeviceState";
            await _coolerDevice.ChangeDeviceState(newState);
            _transportMock.Verify();
        }
    }
}