using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Represents a single set of filtering data for a device
    /// </summary>
    public class FilterInfo
    {
        public string ColumnName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public FilterType FilterType { get; set; }
        public string FilterValue { get; set; }
    }
}
