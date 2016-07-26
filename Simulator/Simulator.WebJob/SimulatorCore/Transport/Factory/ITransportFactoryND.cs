using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory
{
    public interface ITransportFactoryND
    {
        ITransportND CreateTransport(IDeviceND device);
    }
}
