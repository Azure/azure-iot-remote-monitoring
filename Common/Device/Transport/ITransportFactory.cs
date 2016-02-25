namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport
{
    public interface ITransportFactory
    {
        ITransport CreateTransport(IDevice device);
    }
}
