using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class JobQueryResult
    {
        public class OutcomeObj
        {
            [JsonProperty("deviceMethodResponse")]
            string DeviceMethodResponse { get; set; }
        }

        [JsonProperty("deviceId")]
        public string DeviceID { get; set; }

        [JsonProperty("jobId")]
        public string JobID { get; set; }

        [JsonProperty("jobType")]
        public string JobType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("startTimeUtc")]
        public DateTime StartTimeUtc { get; set; }

        [JsonProperty("endTimeUtc")]
        public DateTime EndTimeUtc { get; set; }

        [JsonProperty("createdDateTimeUtc")]
        public DateTime CreatedTimeUtc { get; set; }

        [JsonProperty("lastUpdatedDateTimeUtc")]
        public DateTime LastUpdatedTimeUtc { get; set; }

        [JsonProperty("outcome")]
        public OutcomeObj Outcome { get; set; }
    }
}
