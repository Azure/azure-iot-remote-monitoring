using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceCommandModel
    {
        [DisplayName("Command")]
        public dynamic Command { get; set; }

        public List<dynamic> CommandHistory { get; set; }

        public string DeviceId { get; set; }

        public bool? DeviceIsEnabled { get; set; }

        public string CommandsJson { get; set; }

        public bool SupportDeviceCommand
        {
            get
            {
                return PermsChecker.HasPermission(Permission.SendCommandToDevices);
            }
        }

        public SendCommandModel SendCommandModel { get; set; }
    }

}