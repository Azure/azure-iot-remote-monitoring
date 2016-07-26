using System;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class ActionModel
    {
        // column names in ASA job output
        private const string DEVICE_ID_COLUMN_NAME = "deviceid";
        private const string READING_TYPE_COLUMN_NAME = "readingtype";
        private const string READING_VALUE_COLUMN_NAME = "reading";
        private const string THRESHOLD_VALUE_COLUMN_NAME = "threshold";
        private const string RULE_OUTPUT_COLUMN_NAME = "ruleoutput";
        private const string TIME_COLUMN_NAME = "time";

        [JsonProperty(PropertyName = DEVICE_ID_COLUMN_NAME)]
        public string DeviceID { get; set; }

        [JsonProperty(PropertyName = READING_TYPE_COLUMN_NAME)]
        public string ReadingType { get; set; }

        [JsonProperty(PropertyName = READING_VALUE_COLUMN_NAME)]
        public double Reading { get; set; }

        [JsonProperty(PropertyName = THRESHOLD_VALUE_COLUMN_NAME)]
        public double Threshold { get; set; }

        [JsonProperty(PropertyName = RULE_OUTPUT_COLUMN_NAME)]
        public string RuleOutput { get; set; }

        [JsonProperty(PropertyName = TIME_COLUMN_NAME)]
        public DateTime Time { get; set; }
    }
}