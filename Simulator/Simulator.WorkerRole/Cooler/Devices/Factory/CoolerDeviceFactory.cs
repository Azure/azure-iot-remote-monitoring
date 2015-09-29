using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Devices.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Transport.Factory;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.Devices.Factory
{
    public class CoolerDeviceFactory : IDeviceFactory
    {
        public IDevice CreateDevice(ILogger logger, ITransportFactory transportFactory,
            ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config)
        {
            var device = new CoolerDevice(logger, transportFactory, telemetryFactory, configurationProvider);
            device.Init(config);
            return device;
        }
    }
}
