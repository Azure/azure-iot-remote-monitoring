using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    public class ActionProcessor : IEventProcessor
    {
        private readonly IActionLogic _actionLogic;
        private readonly IActionMappingLogic _actionMappingLogic;
        private readonly IConfigurationProvider _configurationProvider;

        private int _totalMessages = 0;
        private Stopwatch _checkpointStopwatch;

        public ActionProcessor(
            IActionLogic actionLogic,
            IActionMappingLogic actionMappingLogic,
            IConfigurationProvider configurationProvider)
        {
            this.LastMessageOffset = "-1";
            _actionLogic = actionLogic;
            _actionMappingLogic = actionMappingLogic;
            _configurationProvider = configurationProvider;
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
            Trace.TraceInformation("ActionProcessor: Open : Partition : {0}", context.Lease.PartitionId);
            this.Context = context;
            _checkpointStopwatch = new Stopwatch();
            _checkpointStopwatch.Start();

            this.IsInitialized = true;

            return Task.Delay(0);
        }


        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Trace.TraceInformation("ActionProcessor: In ProcessEventsAsync");

            foreach (EventData message in messages)
            {
                try
                {
                    Trace.TraceInformation("ActionProcessor: {0} - Partition {1}", message.Offset, context.Lease.PartitionId);
                    this.LastMessageOffset = message.Offset;

                    string jsonString = Encoding.UTF8.GetString(message.GetBytes());
                    object result = JsonConvert.DeserializeObject(jsonString);
                    JArray resultAsArray = result as JArray;

                    if (resultAsArray != null)
                    {
                        foreach (dynamic item in resultAsArray)
                        {
                            await ProcessAction(item);
                        }
                    }
                    else
                    {
                        await ProcessAction(result);
                    }

                    ++_totalMessages;
                }
                catch (Exception e)
                {
                    Trace.TraceError("ActionProcessor: Error in ProcessEventAsync -- " + e.ToString());
                }
            }

            // checkpoint after processing batch
            try
            {
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - ActionProcessor.ProcessEventsAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);
            }

            if (this.IsClosed)
            {
                this.IsReceivedMessageAfterClose = true;
            }
        }

        private async Task ProcessAction(dynamic eventData)
        {
            if (eventData == null)
            {
                Trace.TraceWarning("Action event is null");
                return;
            }

            try
            {
                // NOTE: all column names from ASA come out as lowercase; see 
                // https://social.msdn.microsoft.com/Forums/office/en-US/c79a662b-5db1-4775-ba1a-23df1310091d/azure-table-storage-account-output-property-names-are-lowercase?forum=AzureStreamAnalytics 

                string deviceId = eventData.deviceid;
                string ruleOutput = eventData.ruleoutput;

                if (ruleOutput.Equals("AlarmTemp", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: temperature rule triggered!");
                    double tempReading = ExtractDouble(eventData.reading);

                    string tempActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(tempActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                        tempActionId,
                        deviceId,
                        "Temperature",
                        tempReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: tempActionId value is empty for temperatureRuleOutput '{0}'", ruleOutput);
                    }
                }

                if (ruleOutput.Equals("AlarmHumidity", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: humidity rule triggered!");
                    double humidityReading = ExtractDouble(eventData.reading);

                    string humidityActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(humidityActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                            humidityActionId,
                            deviceId,
                            "Humidity",
                            humidityReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: humidityActionId value is empty for humidityRuleOutput '{0}'", ruleOutput);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("ActionProcessor: exception in ProcessAction:");
                Trace.TraceError(e.ToString());
            }
        }

        private double ExtractDouble(dynamic value)
        {
            if (value == null)
            {
                Trace.TraceError("ActionProcessor: unable to parse null double value");
                return -1;
            }

            string valueAsString = value.ToString();
            return double.Parse(valueAsString, CultureInfo.CurrentCulture);
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("ActionProcessor: Close : Partition : " + context.Lease.PartitionId);
            this.IsClosed = true;
            _checkpointStopwatch.Stop();
            this.CloseReason = reason;
            this.OnProcessorClosed();

            try
            {
                return context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - ActionProcessor.CloseAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);

                return Task.Run(() => { });
            }
        }

        public virtual void OnProcessorClosed()
        {
            EventHandler handler = this.ProcessorClosed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
