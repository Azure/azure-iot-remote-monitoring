namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using System.Threading;

    public interface IDeviceEventProcessor
    {
        void Start();

        void Start(CancellationToken token);

        void Stop();
    }
}
