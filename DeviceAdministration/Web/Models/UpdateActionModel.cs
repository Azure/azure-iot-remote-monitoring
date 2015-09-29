using System.Collections.Generic;
using System.Web.Mvc;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class UpdateActionModel
    {
        public string RuleOutput { get; set; }
        public string ActionId { get; set; }
        public List<SelectListItem> ActionSelectList { get; set; }
    }
}