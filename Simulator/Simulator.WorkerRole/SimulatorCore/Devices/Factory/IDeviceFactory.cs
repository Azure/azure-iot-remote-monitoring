using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Transport.Factory;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Devices.Factory
{
    public interface IDeviceFactory
    {
        IDevice CreateDevice(Logging.ILogger logger, ITransportFactory transportFactory,
            ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config);
    }
}
