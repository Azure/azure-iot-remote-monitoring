using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class PreScheduleJobModel
    {
        public string QueryName { get; set; }

        public IEnumerable<NamedJobResponseModel> JobsSharingQuery { get; set; }
    }
}