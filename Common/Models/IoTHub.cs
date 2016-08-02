using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class IoTHub
    {
        public string MessageId { get; set; }
        public string CorrelationId { get; set; }
        public string ConnectionDeviceId { get; set; }
        public string ConnectionDeviceGenerationId { get; set; }
        public DateTime EnqueuedTime { get; set; }
        public string StreamId { get; set; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}