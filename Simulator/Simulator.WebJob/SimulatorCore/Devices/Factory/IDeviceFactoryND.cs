using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices.Factory
{
    public interface IDeviceFactoryND
    {
        IDeviceND CreateDevice(Logging.ILogger logger, ITransportFactoryND transportFactory,
            ITelemetryFactoryND telemetryFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config);
    }
}
