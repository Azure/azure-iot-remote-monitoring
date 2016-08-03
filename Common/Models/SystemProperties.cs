namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class SystemProperties
    {
        public string ICCID { get; set; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}