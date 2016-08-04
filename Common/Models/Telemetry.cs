using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class Telemetry
    {
        [JsonConstructor]
        public Telemetry()
        {
            
        }

        public Telemetry(string name, string displayName, string type)
        {
            Name = name;
            DisplayName = displayName;
            Type = type;
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}