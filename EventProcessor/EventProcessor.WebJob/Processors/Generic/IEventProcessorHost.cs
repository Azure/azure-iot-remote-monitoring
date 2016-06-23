namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors.Generic
{
    using System.Threading;

    public interface IEventProcessorHost
    {
        void Start();

        void Start(CancellationToken token);

        void Stop();
    }
}