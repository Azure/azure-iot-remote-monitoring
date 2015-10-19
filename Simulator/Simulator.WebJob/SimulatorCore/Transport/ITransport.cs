using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport
{
    /// <summary>
    /// Interface to provide actions that can be performed against a cloud service such as IoT Hub
    /// </summary>
    public interface ITransport
    {
        void Open();
        Task CloseAsync();

        Task SendEventAsync(dynamic eventData);

        Task SendEventAsync(Guid eventId, dynamic eventData);

        Task SendEventBatchAsync(IEnumerable<Client.Message> messages);

        Task<DeserializableCommand> ReceiveAsync();

        Task SignalAbandonedCommand(DeserializableCommand command);

        Task SignalCompletedCommand(DeserializableCommand command);

        Task SignalRejectedCommand(DeserializableCommand command);
    }
}
