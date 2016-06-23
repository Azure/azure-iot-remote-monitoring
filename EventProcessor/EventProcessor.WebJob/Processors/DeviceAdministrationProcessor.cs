using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    public class DeviceAdministrationProcessor : EventProcessor
    {
        private readonly IDeviceLogic _deviceLogic;
        private readonly IConfigurationProvider _configurationProvider;

        public DeviceAdministrationProcessor(IDeviceLogic deviceLogic, IConfigurationProvider configurationProvider)
        {
            _deviceLogic = deviceLogic;
            _configurationProvider = configurationProvider;
        }

        public override async Task ProcessItem(dynamic eventData)
        {
            if (eventData == null || eventData.ObjectType == null)
            {
                Trace.TraceWarning("Event has no ObjectType defined.  No action was taken on Event packet.");
                return;
            }

            string objectType = eventData.ObjectType.ToString();
            
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


        private async Task ProcessDeviceInfo(dynamic deviceInfo)
        {
            string versionAsString = "";
            if(deviceInfo.Version != null)
            {
                dynamic version = deviceInfo.Version;
                versionAsString = version.ToString();
            }
            switch(versionAsString)
            {
                case SampleDeviceFactory.VERSION_1_0:
                    //Data coming in from the simulator can sometimes turn a boolean into 0 or 1.
                    //Check the HubEnabledState since this is actually displayed and make sure it's in a good format
                    DeviceSchemaHelper.FixDeviceSchema(deviceInfo);

                    dynamic id = deviceInfo.DeviceProperties.DeviceID;
                    string name = id.ToString();
                    Trace.TraceInformation("ProcessEventAsync -- DeviceInfo: {0}", name);
                    await _deviceLogic.UpdateDeviceFromDeviceInfoPacketAsync(deviceInfo);

                    break;
                default:
                    Trace.TraceInformation("Unknown version {0} provided in Device Info packet", versionAsString);
                    break;
            }
        }
    }
}
