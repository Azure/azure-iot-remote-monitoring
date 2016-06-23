using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    public class DeviceEventProcessor :
        EventProcessorHost<EventProcessorFactory<DeviceAdministrationProcessor>>,
        IDeviceEventProcessor
    {
        public DeviceEventProcessor(
            IConfigurationProvider configurationProvider,
            IDeviceLogic deviceLogic)
            :
            base(configurationProvider.GetConfigurationSettingValue("eventHub.HubName"),
                configurationProvider.GetConfigurationSettingValue("eventHub.ConnectionString"),
                configurationProvider.GetConfigurationSettingValue("eventHub.StorageConnectionString"),
                deviceLogic, configurationProvider)
        {
        }
    }
}
