using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport
{
    public class EmptyTransport : ITransport
    {
        private readonly ILogger _logger;

        public EmptyTransport(ILogger logger)
        {
            _logger = logger;
        }

        public async Task OpenAsync()
        {
            await Task.FromResult(0);
        }

        public async Task CloseAsync()
        {
            await Task.FromResult(0);
        }

        public async Task SendEventAsync(dynamic eventData)
        {
            var eventId = Guid.NewGuid();
            await SendEventAsync(eventId, eventData);
        }

        public async Task SendEventAsync(Guid eventId, dynamic eventData)
        {
            _logger.LogInfo("SendEventAsync called:");
            _logger.LogInfo("SendEventAsync: EventId: " + eventId.ToString());
            _logger.LogInfo("SendEventAsync: message: " + eventData.ToString());

            await Task.FromResult(0);
        }

        public async Task SendEventBatchAsync(IEnumerable<Client.Message> messages)
        {
            _logger.LogInfo("SendEventBatchAsync called");

            await Task.FromResult(0);
        }

        public async Task<DeserializableCommand> ReceiveAsync()
        {
            _logger.LogInfo("ReceiveAsync: waiting...");

            return await Task.FromResult(new DeserializableCommand(new Client.Message()));
        }

        public async Task SignalAbandonedCommand(DeserializableCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            await Task.FromResult(0);
        }

        public async Task SignalCompletedCommand(DeserializableCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            await Task.FromResult(0);
        }

        public async Task SignalRejectedCommand(DeserializableCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            await Task.FromResult(0);
        }

        public async Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties)
        {
            _logger.LogInfo("UpdateReportedPropertiesAsync called");
            _logger.LogInfo($"UpdateReportedPropertiesAsync: reportedProperties: {reportedProperties.ToJson(Formatting.Indented)}");

            await Task.FromResult(0);
        }

        public async Task<Twin> GetTwinAsync()
        {
            _logger.LogInfo("GetTwinAsync called");

            return await Task.FromResult(new Twin());
        }

        public Task SetMethodHandlerAsync(string methodName, MethodCallback callback)
        {
            _logger.LogInfo(FormattableString.Invariant($"SetMethodHandler called: {methodName} -> {callback.Method.Name}"));

            return Task.FromResult(0);
        }

        public void SetDesiredPropertyUpdateCallback(DesiredPropertyUpdateCallback callback)
        {
            _logger.LogInfo(FormattableString.Invariant($"SetDesiredPropertyUpdateCallback called, callback = {callback.Method.Name}"));
        }
    }
}
