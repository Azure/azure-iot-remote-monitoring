namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ServiceBus.Messaging;

    public abstract class EventProcessor : IEventProcessor
    {
        int _totalMessages;
        Stopwatch _checkpointStopWatch;

        public EventProcessor()
        {
            LastMessageOffset = "-1";
        }

        public event EventHandler ProcessorClosed;

        public bool IsInitialized { get; private set; }

        public bool IsClosed { get; private set; }

        public bool IsReceivedMessageAfterClose { get; set; }

        public int TotalMessages
        {
            get { return _totalMessages; }
        }

        public CloseReason CloseReason { get; private set; }

        public PartitionContext Context { get; private set; }

        public string LastMessageOffset { get; private set; }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation("{0}: Open : Partition : {1}", GetType().Name, context.Lease.PartitionId);
            Context = context;
            _checkpointStopWatch = new Stopwatch();
            _checkpointStopWatch.Start();

            IsInitialized = true;

            return Task.Delay(0);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Trace.TraceInformation("{0}: In ProcessEventsAsync", GetType().Name);

            foreach (EventData message in messages)
            {
                try
                {
                    // Write out message
                    Trace.TraceInformation("{0}: {1} - Partition {2}", GetType().Name, message.Offset, context.Lease.PartitionId);
                    LastMessageOffset = message.Offset;

                    string jsonString = Encoding.UTF8.GetString(message.GetBytes());
                    dynamic result = JsonConvert.DeserializeObject(jsonString);
                    JArray resultAsArray = result as JArray;

                    if (resultAsArray != null)
                    {
                        foreach (dynamic resultItem in resultAsArray)
                        {
                            await ProcessItem(resultItem);
                        }
                    }
                    else
                    {
                        await ProcessItem(result);
                    }

                    _totalMessages++;
                }
                catch (Exception e)
                {
                    Trace.TraceError("{0}: Error in ProcessEventAsync -- {1}", GetType().Name, e.Message);
                }
            }

            // batch has been processed, checkpoint 
            try
            {
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - {1}.ProcessEventsAsync ***{0}{0}{2}{0}{0}",
                    Console.Out.NewLine,
                    GetType().Name,
                    ex);
            }

            if (IsClosed)
            {
                IsReceivedMessageAfterClose = true;
            }
        }

        public abstract Task ProcessItem(dynamic data);

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("{0}: Close : Partition : {1}", GetType().Name, context.Lease.PartitionId);
            IsClosed = true;
            _checkpointStopWatch.Stop();
            CloseReason = reason;
            OnProcessorClosed();

            try
            {
                return context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - {1}.CloseAsync ***{0}{0}{2}{0}{0}",
                    Console.Out.NewLine,
                    GetType().Name,
                    ex);

                return Task.Run(() => { });
            }
        }

        public virtual void OnProcessorClosed()
        {
            EventHandler handler = ProcessorClosed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}