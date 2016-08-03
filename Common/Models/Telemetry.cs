namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class Telemetry
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}