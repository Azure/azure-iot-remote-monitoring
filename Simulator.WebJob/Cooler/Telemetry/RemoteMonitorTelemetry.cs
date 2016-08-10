using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Telemetry.Data;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Telemetry
{
    public class RemoteMonitorTelemetry : ITelemetry
    {
        private readonly ILogger _logger;
        private readonly string _deviceId;

        private const int REPORT_FREQUENCY_IN_SECONDS = 5;
        private const int PEAK_FREQUENCY_IN_SECONDS = 90;

        private SampleDataGenerator _temperatureGenerator;
        private SampleDataGenerator _humidityGenerator;
        private SampleDataGenerator _externalTemperatureGenerator;

        public bool ActivateExternalTemperature { get; set; }

        public bool TelemetryActive { get; set; }

        public RemoteMonitorTelemetry(ILogger logger, string deviceId)
        {
            _logger = logger;
            _deviceId = deviceId;

            ActivateExternalTemperature = false;
            TelemetryActive = true;

            int peakFrequencyInTicks = Convert.ToInt32(Math.Ceiling((double)PEAK_FREQUENCY_IN_SECONDS /  REPORT_FREQUENCY_IN_SECONDS));

            _temperatureGenerator = new SampleDataGenerator(33, 36, 42, peakFrequencyInTicks);
            _humidityGenerator = new SampleDataGenerator(20, 50);
            _externalTemperatureGenerator = new SampleDataGenerator(-20, 120);
        }

        public async Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            var monitorData = new RemoteMonitorTelemetryData();
            string messageBody;
            while (!token.IsCancellationRequested)
            {
                if (TelemetryActive)
                {
                    monitorData.DeviceId = _deviceId;
                    monitorData.Temperature = _temperatureGenerator.GetNextValue();
                    monitorData.Humidity = _humidityGenerator.GetNextValue();
                    messageBody = "Temperature: " + Math.Round(monitorData.Temperature, 2)
                        + " Humidity: " + Math.Round(monitorData.Humidity, 2);

                    if (ActivateExternalTemperature)
                    {
                        monitorData.ExternalTemperature = _externalTemperatureGenerator.GetNextValue();
                        messageBody += " External Temperature: " + Math.Round((double)monitorData.ExternalTemperature, 2);
                    }
                    else
                    {
                        monitorData.ExternalTemperature = null;
                    }

                    //_logger.LogInfo("Sending " + messageBody + " for Device: " + _deviceId);

                    await sendMessageAsync(monitorData);
                }
                await Task.Delay(TimeSpan.FromSeconds(REPORT_FREQUENCY_IN_SECONDS), token);
            }
        }

        public void ChangeSetPointTemperature(double newSetPointTemperature)
        {
            _temperatureGenerator.ShiftSubsequentData(newSetPointTemperature);
        }
    }
}