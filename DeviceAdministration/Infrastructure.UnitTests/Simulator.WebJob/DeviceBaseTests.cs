using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Serialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Mono.Security.Authenticode;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Simulator.WebJob
{
    public class DeviceBaseTests
    {
        private Mock<ILogger> _loggerMock;
        private Mock<ITransportFactory> _transportFactory;
        private Mock<ITelemetryFactory> _telemetryFactoryMock;
        private Mock<IConfigurationProvider> _configurationProviderMock;
        private Mock<ITransport> _transport;
        private DeviceBase deviceBase;

        public DeviceBaseTests()
        {
            _loggerMock = new Mock<ILogger>();
            _transportFactory = new Mock<ITransportFactory>();
            _telemetryFactoryMock = new Mock<ITelemetryFactory>();
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _transport = new Mock<ITransport>();
            deviceBase = new DeviceBase(_loggerMock.Object, _transportFactory.Object, _telemetryFactoryMock.Object, _configurationProviderMock.Object) ;
        }

        [Fact]
        public void InitTests()
        {
            InitialDeviceConfig config = new InitialDeviceConfig();
            config.HostName = "HostName";
            config.DeviceId = "test";
            config.Key = "key";

            deviceBase.Init(config);

            Assert.Equal(deviceBase.DeviceID, "test");
            Assert.Equal(deviceBase.HostName, "HostName");
            Assert.Equal(deviceBase.PrimaryAuthKey, "key");
            Assert.NotNull(deviceBase.DeviceProperties);
            Assert.NotNull(deviceBase.Commands);
            Assert.NotNull(deviceBase.Telemetry); 
        }

        [Fact]
        public void GetDeviceInfoTests()
        {
            InitialDeviceConfig config = new InitialDeviceConfig();
            config.HostName = "HostName";
            config.DeviceId = "test";
            config.Key = "key";

            deviceBase.Init(config);
            DeviceModel device = deviceBase.GetDeviceInfo();
            Assert.Equal(device.DeviceProperties.DeviceID, "test");
        }




    }
}
