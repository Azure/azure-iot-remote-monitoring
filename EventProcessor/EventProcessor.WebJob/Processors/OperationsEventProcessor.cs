using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    class OperationsEventProcessor :
        EventProcessorHost<EventProcessorFactory<OperationsProcessor>>,
        IOperationsEventProcessor
    {
        public OperationsEventProcessor(
            IConfigurationProvider configurationProvider) :
            base(
                "messages/operationsmonitoringevents",
                configurationProvider.GetConfigurationSettingValue("iotHub.ConnectionString"),
                configurationProvider.GetConfigurationSettingValue("eventHub.StorageConnectionString"))
        { }
    }
}
