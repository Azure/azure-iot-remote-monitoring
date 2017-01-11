using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Represents a single set of filtering data for a device
    /// </summary>
    public class Clause
    {
        public string ColumnName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ClauseType ClauseType { get; set; }
        public string ClauseValue { get; set; }
        public TwinDataType ClauseDataType { get; set; }
    }
}
