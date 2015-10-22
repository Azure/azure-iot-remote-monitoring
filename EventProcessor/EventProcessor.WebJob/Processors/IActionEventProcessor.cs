namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    public interface IActionEventProcessor
    {
        void Start();
        void Stop();
    }
}
