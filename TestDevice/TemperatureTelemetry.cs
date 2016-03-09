using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device;
using Windows.Devices.Gpio;
using System.Diagnostics;

namespace TestDevice
{
    internal class TemperatureTelemetry : ITelemetry
    {
        private readonly ILogger logger;
        private readonly string deviceId;
        private readonly BMP280 temperatureSensor;

        private const int REPORT_FREQUENCY_IN_SECONDS = 5;


        public TemperatureTelemetry(ILogger logger, IDevice device, BMP280 temperatureSensor)
        {
            this.logger = logger;
            deviceId = device.DeviceID;
            this.temperatureSensor = temperatureSensor;
        }

        public async Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            var monitorData = new TemperatureTelemetryData();
            monitorData.DeviceId = deviceId;
            monitorData.Temperature = await temperatureSensor.ReadTemperature();
            monitorData.Pressure = await temperatureSensor.ReadPreasure();
            await sendMessageAsync(monitorData);
        }
    }
}