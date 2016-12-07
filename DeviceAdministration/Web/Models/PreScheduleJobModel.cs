using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class PreScheduleJobModel
    {
        public string FilterId { get; set; }

        public string FilterName { get; set; }

        public IEnumerable<NamedJobResponseModel> JobsSharingQuery { get; set; }
    }
}