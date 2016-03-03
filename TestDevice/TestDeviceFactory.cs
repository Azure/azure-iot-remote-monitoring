using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using System.Threading.Tasks;

namespace TestDevice
{
    internal class TestDeviceFactory : IDeviceFactory
    {
        public async Task<IDevice> CreateDevice(ILogger logger, ITransportFactory transportFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config)
        {
            var device = new TestDevice(logger, transportFactory, configurationProvider);
            await device.Initialize(config);
            return device;
        }
    }
}
