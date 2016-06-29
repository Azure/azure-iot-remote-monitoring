using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    class OperationsEventProcessor :
        EventProcessorHost<EventProcessorFactory<OperationsProcessor>>,
        IOperationsEventProcessor
    {
        public OperationsEventProcessor(
            IConfigurationProvider configurationProvider,
            IDeviceLogic deviceLogic) :
            base(
                "messages/operationsmonitoringevents",
                configurationProvider.GetConfigurationSettingValue("iotHub.ConnectionString"),
                configurationProvider.GetConfigurationSettingValue("eventHub.StorageConnectionString"),
                deviceLogic, configurationProvider)
        { }
    }
}
