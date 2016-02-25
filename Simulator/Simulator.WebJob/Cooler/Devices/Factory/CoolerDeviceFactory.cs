using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices.Factory
{
    public class CoolerDeviceFactory : IDeviceFactory
    {
        public IDevice CreateDevice(ILogger logger, ITransportFactory transportFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config)
        {
            var device = new CoolerDevice(logger, transportFactory, configurationProvider);
            device.Init(config);
            return device;
        }
    }
}
