namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceState
    {
        public DeviceState(string deviceId, string sensorValue)
        {
            DeviceId = deviceId;
            SensorValue = sensorValue;  
        }

        public string DeviceId { get; set; }
        public string SensorValue { get; set; }
    }
}
