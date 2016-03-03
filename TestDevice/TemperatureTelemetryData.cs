namespace TestDevice
{
    internal class TemperatureTelemetryData
    {
        public string DeviceId { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }

        public double Humidity { get; set; }
    }
}
