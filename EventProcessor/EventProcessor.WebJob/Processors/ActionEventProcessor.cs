using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    public class ActionEventProcessor :
        EventProcessorHost<EventProcessorFactory<ActionProcessor>>,
        IActionEventProcessor
    {
        public ActionEventProcessor(
            IConfigurationProvider configurationProvider,
            IActionLogic actionLogic,
            IActionMappingLogic actionMappingLogic)
            :
            base(configurationProvider.GetConfigurationSettingValue("RulesEventHub.Name"),
                configurationProvider.GetConfigurationSettingValue("RulesEventHub.ConnectionString"),
                configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString"),
                actionLogic, actionMappingLogic, configurationProvider)
        {
        }
    }
}
