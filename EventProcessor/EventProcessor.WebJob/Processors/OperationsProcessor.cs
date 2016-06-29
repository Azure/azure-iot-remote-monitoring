using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    class OperationsProcessor : EventProcessor
    {
        private readonly IDeviceLogic _deviceLogic;
        private readonly IConfigurationProvider _configurationProvider;

        public OperationsProcessor(IDeviceLogic deviceLogic, IConfigurationProvider configurationProvider)
        {
            _deviceLogic = deviceLogic;
            _configurationProvider = configurationProvider;
        }

        public override async Task ProcessItem(dynamic eventData)
        {
            // If we get a successful device connection...
            if (eventData.category == "Connections" &&
                eventData.operationName == "deviceConnect" &&
                eventData.level == "Information")
            {
                // ...from the ARM bridge...
                string deviceId = eventData.deviceId.ToString();
                if (deviceId.StartsWith(_configurationProvider.GetConfigurationSettingValue("MbedPrefix")))
                {
                    // ...for an unknown device...
                    deviceId = deviceId.Substring(_configurationProvider.GetConfigurationSettingValue("MbedPrefix").Length);
                    if (null == await _deviceLogic.GetDeviceAsync(deviceId))
                    {
                        // ...add it to the device repository
                        await _deviceLogic.AddDeviceAsync(DeviceSchemaHelper.BuildDeviceStructure(deviceId, DeviceTypeConstants.MBED, null));
                    }
                }
            }
        }
    }
}
