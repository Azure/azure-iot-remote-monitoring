using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceKeysModel
    {
        public SecurityKeys DeviceKeys { get; set; }
        public string DeviceId { get; set; }

        public UpdateKeysModel GetUpdateKeysModel()
        {
            return new UpdateKeysModel
            {
                DeviceKeys = DeviceKeys,
                DeviceId = DeviceId
            };
        }
    }
}