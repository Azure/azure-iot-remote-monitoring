namespace Microsoft.Azure.IoT.Samples.EventProcessor.WebJob.Processors
{
    public interface IMessageFeedbackProcessor
    {
        void Start();

        void Stop();
    }
}
