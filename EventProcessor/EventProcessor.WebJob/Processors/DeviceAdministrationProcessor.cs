using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    public class DeviceAdministrationProcessor : IEventProcessor
    {
        private readonly IDeviceLogic _deviceLogic;
        private readonly IConfigurationProvider _configurationProvider;

        int _totalMessages = 0;
        Stopwatch _checkpointStopWatch;

        public DeviceAdministrationProcessor(IDeviceLogic deviceLogic, IConfigurationProvider configurationProvider)
        {
            this.LastMessageOffset = "-1";
            _deviceLogic = deviceLogic;
            _configurationProvider = configurationProvider;
        }

        public event EventHandler ProcessorClosed;

        public bool IsInitialized { get; private set; }

        public bool IsClosed { get; private set; }

        public bool IsReceivedMessageAfterClose { get; set; }

        public int TotalMessages
        {
            get { return this._totalMessages; }
        }

        public CloseReason CloseReason { get; private set; }

        public PartitionContext Context { get; private set; }

        public string LastMessageOffset { get; private set; }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation("DeviceAdministrationProcessor: Open : Partition : {0}", context.Lease.PartitionId);
            this.Context = context;
            this._checkpointStopWatch = new Stopwatch();
            this._checkpointStopWatch.Start();

            this.IsInitialized = true;

            return Task.Delay(0);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Trace.TraceInformation("DeviceAdministrationProcessor: In ProcessEventsAsync");

            foreach (EventData message in messages)
            {
                string jsonString = string.Empty;
                try
                {
                    // Write out message
                    Trace.TraceInformation("DeviceAdministrationProcessor: {0} - Partition {1}", message.Offset, context.Lease.PartitionId);
                    this.LastMessageOffset = message.Offset;

                    jsonString = Encoding.UTF8.GetString(message.GetBytes());
                    IList<DeviceModel> results = JsonConvert.DeserializeObject<List<DeviceModel>>(jsonString);
                    if (results != null)
                    {
                        foreach (DeviceModel resultItem in results)
                        {
                            if (string.IsNullOrWhiteSpace(resultItem.DeviceProperties.DeviceID))
                            {
                                resultItem.DeviceProperties.DeviceID = resultItem.IoTHub.ConnectionDeviceId;
                            }

                            await ProcessEventItem(resultItem);
                        }
                    }
                    this._totalMessages++;
                }
                catch (Exception e)
                {
                    Trace.TraceInformation("DeviceAdministrationProcessor: Error in ProcessEventAsync -- " + e.Message);
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
                    "{0}{0}*** CheckpointAsync Exception - DeviceAdministrationProcessor.ProcessEventsAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);
            }



            if (this.IsClosed)
            {
                this.IsReceivedMessageAfterClose = true;
            }
        }

        private async Task ProcessEventItem(DeviceModel eventData)
        {
            if (eventData == null || eventData.ObjectType == null)
            {
                Trace.TraceWarning("Event has no ObjectType defined.  No action was taken on Event packet.");
                return;
            }

            string objectType = eventData.ObjectType;

            var objectTypePrefix = _configurationProvider.GetConfigurationSettingValue("ObjectTypePrefix");
            if (string.IsNullOrWhiteSpace(objectTypePrefix))
            {
                objectTypePrefix = "";
            }


            if (objectType == objectTypePrefix + SampleDeviceFactory.OBJECT_TYPE_DEVICE_INFO)
            {
                await ProcessDeviceInfo(eventData);
            }
            else
            {
                Trace.TraceWarning("Unknown ObjectType in event.");
            }
        }


        private async Task ProcessDeviceInfo(DeviceModel deviceInfo)
        {
            string versionAsString = "";
            if (deviceInfo.Version != null)
            {
                versionAsString = deviceInfo.Version;
            }
            switch (versionAsString)
            {
                case SampleDeviceFactory.VERSION_1_0:
                    //Data coming in from the simulator can sometimes turn a boolean into 0 or 1.
                    //Check the HubEnabledState since this is actually displayed and make sure it's in a good format
                    //Should not be required for strongly typed object
                    //DeviceSchemaHelperND.FixDeviceSchema(deviceInfo);
                    if (deviceInfo.IoTHub == null)
                    {
                        throw new DeviceRequiredPropertyNotFoundException("'IoTHubProperties' property is missing");
                    }

                    string name = deviceInfo.IoTHub.ConnectionDeviceId;
                    Trace.TraceInformation("ProcessEventAsync -- DeviceInfo: {0}", name);
                    await _deviceLogic.UpdateDeviceFromDeviceInfoPacketAsync(deviceInfo);

                    // Pick the task object rather than using await, since there is no need to wait until cache updated
                    var task = _deviceLogic.AddToNameCache(deviceInfo.DeviceProperties.DeviceID);

                    break;
                default:
                    Trace.TraceInformation("Unknown version {0} provided in Device Info packet", versionAsString);
                    break;
            }
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("DeviceAdministrationProcessor: Close : Partition : " + context.Lease.PartitionId);
            this.IsClosed = true;
            this._checkpointStopWatch.Stop();
            this.CloseReason = reason;
            this.OnProcessorClosed();

            try
            {
                return context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - DeviceAdministrationProcessor.CloseAsync ***{0}{0}{1}{0}{0}",
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
