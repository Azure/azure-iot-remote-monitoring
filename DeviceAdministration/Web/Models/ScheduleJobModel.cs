using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class ScheduleJobModel
    {
        public string QueryName { get; set; }

        public IEnumerable<NamedJobResponseModel> JobsSharingQuery { get; set; }
    }
}