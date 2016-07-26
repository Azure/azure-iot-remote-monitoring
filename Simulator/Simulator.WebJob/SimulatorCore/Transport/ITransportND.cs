using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport
{
    /// <summary>
    /// Interface to provide actions that can be performed against a cloud service such as IoT Hub
    /// </summary>
    public interface ITransportND
    {
        void Open();
        Task CloseAsync();

        Task SendEventAsync(DeviceND eventData);

        Task SendEventAsync(Guid eventId, DeviceND eventData);

        Task SendEventBatchAsync(IEnumerable<Client.Message> messages);

        Task<DeserializableCommand> ReceiveAsync();

        Task SignalAbandonedCommand(DeserializableCommand command);

        Task SignalCompletedCommand(DeserializableCommand command);

        Task SignalRejectedCommand(DeserializableCommand command);
    }
}
