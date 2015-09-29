namespace Microsoft.Azure.IoT.Samples.EventProcessor.WorkerRole.Processors
{
    public interface IMessageFeedbackProcessor
    {
        void Start();

        void Stop();
    }
}
