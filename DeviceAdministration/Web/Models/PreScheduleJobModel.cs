using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class PreScheduleJobModel
    {
        public string FilterId { get; set; }

        public IEnumerable<NamedJobResponseModel> JobsSharingQuery { get; set; }

        public bool HasManageJobsPerm
        {
            get
            {
                return PermsChecker.HasPermission(Permission.ManageJobs);
            }
        }
    }
}