using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class ActionPropertiesModel
    {
        public string RuleOutput { get; set; }
        public string ActionId { get; set; }
        public bool HasAssignActionPerm
        {
            get
            {
                return PermsChecker.HasPermission(Permission.AssignAction);
            }
        }
        public UpdateActionModel UpdateActionModel { get; set; }
    }
}