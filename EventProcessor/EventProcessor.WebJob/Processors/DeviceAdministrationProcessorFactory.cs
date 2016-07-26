using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.ServiceBus.Messaging;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    public class DeviceAdministrationProcessorFactory : IEventProcessorFactory
    {
        private readonly IDeviceLogic _deviceLogic;
        private readonly IConfigurationProvider _configurationProvider;

        readonly ConcurrentDictionary<string, DeviceAdministrationProcessor> eventProcessors = new ConcurrentDictionary<string, DeviceAdministrationProcessor>();
        readonly ConcurrentQueue<DeviceAdministrationProcessor> closedProcessors = new ConcurrentQueue<DeviceAdministrationProcessor>();

        public DeviceAdministrationProcessorFactory(IDeviceLogic deviceLogic, IConfigurationProvider configurationProvider)
        {
            _deviceLogic = deviceLogic;
            _configurationProvider = configurationProvider;
        }

        public int ActiveProcessors
        {
            get { return this.eventProcessors.Count; }
        }

        public int TotalMessages
        {
            get
            {
                var amount = this.eventProcessors.Select(p => p.Value.TotalMessages).Sum();
                amount += this.closedProcessors.Select(p => p.TotalMessages).Sum();
                return amount;
            }
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            var processor = new DeviceAdministrationProcessor(_deviceLogic, _configurationProvider);
            processor.ProcessorClosed += this.ProcessorOnProcessorClosed;
            this.eventProcessors.TryAdd(context.Lease.PartitionId, processor);
            return processor;
        }

        public Task WaitForAllProcessorsInitialized(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsInitialized, timeout);
        }

        public Task WaitForAllProcessorsClosed(TimeSpan timeout)
        {
            return this.WaitForAllProcessorsCondition(p => p.IsClosed, timeout);
        }

        public async Task WaitForAllProcessorsCondition(Func<DeviceAdministrationProcessor, bool> predicate, TimeSpan timeout)
        {
            TimeSpan sleepInterval = TimeSpan.FromSeconds(2);
            while(!this.eventProcessors.Values.All(predicate))
            {
                if (timeout > sleepInterval)
                {
                    timeout = timeout.Subtract(sleepInterval);
                }
                else
                {
                    throw new TimeoutException("Condition not satisfied within expected timeout.");
                }
                await Task.Delay(sleepInterval);
            }
        }

        public void ProcessorOnProcessorClosed(object sender, EventArgs eventArgs)
        {
            var processor = sender as DeviceAdministrationProcessor;
            if (processor != null)
            {
                this.eventProcessors.TryRemove(processor.Context.Lease.PartitionId, out processor);
                this.closedProcessors.Enqueue(processor);
            }
        }
    }
}
