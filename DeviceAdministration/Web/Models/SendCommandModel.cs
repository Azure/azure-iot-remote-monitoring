using System.Collections.Generic;
using System.Web.Mvc;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class SendCommandModel
    {
        public dynamic Command { get; set; }
        public List<SelectListItem> CommandSelectList { get; set; }
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