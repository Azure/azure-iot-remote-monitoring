namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole.Processors
{
    public interface IActionEventProcessor
    {
        void Start();
        void Stop();
    }
}
