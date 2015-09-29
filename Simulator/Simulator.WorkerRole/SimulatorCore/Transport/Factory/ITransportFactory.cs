using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Devices;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Transport.Factory
{
    public interface ITransportFactory
    {
        ITransport CreateTransport(IDevice device);
    }
}
