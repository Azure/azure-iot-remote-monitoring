using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Samples.Simulator.WorkerRole.SimulatorCore.Logging;
using Microsoft.Azure.IoT.Samples.Simulator.WorkerRole.SimulatorCore.Telemetry;
using Microsoft.Azure.IoT.Samples.Simulator.WorkerRole.VendingMachine.Telemetry.Data;

namespace Microsoft.Azure.IoT.Samples.Simulator.WorkerRole.VendingMachine.Telemetry
{
    public class PresenceTelemetry : ITelemetry
    {
        public enum PresenceStateType
        {
            SensingForPresence,
            PresenceDetected
        }

        private readonly ILogger _logger;

        private const int SENSING_PERIOD_IN_SECONDS = 3;
        private const int PROBABILITY_OF_PRESENCE = 10;
        private const int MAX_DURATION_OF_PRESENCE = 12;

        private const int SENSOR_PRESENCE_DETECTED = 2;
        private const int SENSOR_NO_PRESENCE_DETECTED = 30;
        
        private int _durationOfPresence = 0;

        private DateTime _startedPresence = DateTime.Now;
        private PresenceStateType _currentState = PresenceStateType.SensingForPresence;

        public string DeviceId { get; set; }

        public PresenceTelemetry(ILogger logger, string deviceId)
        {
            _logger = logger;
            DeviceId = deviceId;
        }

        public async Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            while (!token.IsCancellationRequested)
            {
                var random = new Random();

                switch (_currentState)
                {
                    case PresenceStateType.SensingForPresence:

                        _logger.LogInfo("Sensing for presence {0}", DeviceId);

                        // Is there someone in front of the vending machine?
                        var randomNumber = random.Next(100);

                        if (randomNumber < PROBABILITY_OF_PRESENCE)
                        {
                            // Detected a customer is standing in front of the machine
                            _logger.LogInfo("Presence Started {0}", DeviceId);

                            // How long will the customer be in front of the machine?
                            _durationOfPresence = random.Next(SENSING_PERIOD_IN_SECONDS, MAX_DURATION_OF_PRESENCE);
                            _startedPresence = DateTime.Now;

                            _currentState = PresenceStateType.PresenceDetected;
                        }
                        else
                        {
                            // No presence detected
                            // Send a non-sensing telemetry event
                            await SendPresenceTelemetry(false, sendMessageAsync);
                            await Task.Delay(TimeSpan.FromSeconds(SENSING_PERIOD_IN_SECONDS), token);
                        }

                        break;
                    case PresenceStateType.PresenceDetected:
                        _logger.LogInfo("Sending presence packet {0}", DeviceId);

                        // Send a presence telemetry event
                        await SendPresenceTelemetry(true, sendMessageAsync);

                        // Have we passed the durationOfPresence?
                        if (DateTime.Now > _startedPresence.AddMilliseconds(_durationOfPresence * 1000))
                        {
                            _logger.LogInfo("Presence Ended {0}", DeviceId);

                            _currentState = PresenceStateType.SensingForPresence;
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(SENSING_PERIOD_IN_SECONDS), token);
                        }

                        break;
                }
            }
        }

        private async Task SendPresenceTelemetry(bool isPresent, Func<object, Task> sendMessageAsync)
        {
            var presenceData = new PresenceTelemetryData
            {
                SensorValue = isPresent ? SENSOR_NO_PRESENCE_DETECTED : SENSOR_PRESENCE_DETECTED
            };

            await sendMessageAsync(presenceData);
        }
    }
}
