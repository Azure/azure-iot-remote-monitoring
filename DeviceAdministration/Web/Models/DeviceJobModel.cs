using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceJobModel
    {
        public DeviceJobModel(JobResponse jobResponse)
        {
            Status = jobResponse.Status;
            JobId = jobResponse.JobId;
            DeviceCount = jobResponse.DeviceJobStatistics?.DeviceCount;
            SucceededCount = jobResponse.DeviceJobStatistics?.SucceededCount;
            FailedCount = jobResponse.DeviceJobStatistics?.FailedCount;
            PendingCount = jobResponse.DeviceJobStatistics?.PendingCount;
            RunningCount = jobResponse.DeviceJobStatistics?.RunningCount;
            OperationType = jobResponse.Type.LocalizedString();
            StartTime = jobResponse.StartTimeUtc;
            EndTime = jobResponse.EndTimeUtc;
        }

        public string JobId { get; set; }
        public string JobName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public JobStatus Status { get; set; }
        public string QueryName { get; set; }
        public string OperationType { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DeviceCount { get; set; }
        public int? SucceededCount { get; set; }
        public int? FailedCount { get; set; }
        public int? PendingCount { get; set; }
        public int? RunningCount { get; set; }
    }
}