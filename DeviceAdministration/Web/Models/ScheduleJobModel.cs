using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class ScheduleJobViewModel
    {

        public string QueryName { get; set; }

        [Required]
        public string JobName { get; set; }

        [Required]
        public DateTime StartDateUtc { get; set; }

        [Required]
        public int MaxExecutionTimeInMinutes { get; set; }
    }

    public class ScheduleJobModel
    {
        public string QueryName { get; set; }

        public IEnumerable<NamedJobResponseModel> JobsSharingQuery { get; set; }
    }
}