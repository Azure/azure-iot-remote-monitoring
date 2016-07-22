using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class SendCommandModel
    {
        public Command Command { get; set; }
        public IList<SelectListItem> CommandSelectList { get; set; }
        public string DeviceId { get; set; }
        public bool CanSendDeviceCommands { get; set; }
        public bool HasCommands
        {
            get
            {
                return CommandSelectList != null && CommandSelectList.Count > 0;
            }
        }
    }
}