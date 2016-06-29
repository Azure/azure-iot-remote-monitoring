using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    class OperationsProcessor : EventProcessor
    {
        private readonly IDeviceLogic _deviceLogic;

        public OperationsProcessor(IDeviceLogic deviceLogic)
        {
            _deviceLogic = deviceLogic;
        }

        public override async Task ProcessItem(dynamic eventData)
        {
            // If we get a non-error device connection...
            if (eventData.category == "Connections" &&
                eventData.operationName == "deviceConnect" &&
                eventData.level == "Information")
            {
                // ...for an unknown device...
                string deviceId = eventData.deviceId.ToString();
                if (null == await _deviceLogic.GetDeviceAsync(deviceId))
                {
                    // ...add it to the device repository
                    await _deviceLogic.AddDeviceAsync(DeviceSchemaHelper.BuildDeviceStructure(deviceId, false, null));
                }
            }
        }
    }
}
