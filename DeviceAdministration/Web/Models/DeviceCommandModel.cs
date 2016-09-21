using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceCommandModel
    {
        [DisplayName("Command")]
        public Command Command { get; set; }

        public List<CommandHistory> CommandHistory { get; set; }

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