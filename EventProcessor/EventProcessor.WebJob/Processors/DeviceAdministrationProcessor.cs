using System;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    public class DeviceAdministrationProcessor : EventProcessor
    {
        private readonly IDeviceLogic _deviceLogic;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IIotHubRepository _iotHubRepository;

        public DeviceAdministrationProcessor(IDeviceLogic deviceLogic, IIotHubRepository iotHubRepository, IConfigurationProvider configurationProvider)
        {
            _deviceLogic = deviceLogic;
            _iotHubRepository = iotHubRepository;
            _configurationProvider = configurationProvider;
        }

        public override async Task ProcessItem(dynamic eventData)
        {
            if (eventData == null)
            {
                return;
            }

            // If the event has an object type, handle it as a device info object
            if (eventData.ObjectType != null)
            {
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
            else if (eventData.path != null)
            {
                await HandleMbedResponse(eventData);
            }
            else
            {
                Trace.TraceWarning("Event is not a device-administration event. No action was taken on Event packet.");
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

        private async Task HandleMbedResponse(dynamic mbedResponse)
        {
            if (mbedResponse.path == "/5/0/1")
            {
                string deviceId = mbedResponse.ep.ToString();
                dynamic cmd = new ExpandoObject();
                cmd.path = "/5/0/2";
                cmd.new_value = "arm1234";
                cmd.ep = deviceId;
                cmd.coap_verb = "post";
                cmd.options = "noResp=true";
                cmd.MessageId = Guid.NewGuid().ToString();
                await _iotHubRepository.SendCommand(deviceId, cmd);
            }
            else
            {
                Trace.TraceWarning("Unexpected LWM2M path.");
            }
        }
    }
}
