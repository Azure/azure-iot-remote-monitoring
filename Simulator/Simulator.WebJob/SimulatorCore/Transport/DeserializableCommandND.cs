using System.Diagnostics;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Serialization;
using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport
{
    /// <summary>
    /// Wraps the byte array returned from the cloud so that it can be deserialized
    /// </summary>
    public class DeserializableCommandND
    {
        private readonly CommandHistoryND _commandHistory;
        private readonly string _lockToken;

        public string CommandName
        {
            get { return _commandHistory.Name; }
        }

        public DeserializableCommandND(Client.Message message, ISerialize serializer)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            Debug.Assert(
                !string.IsNullOrEmpty(message.LockToken),
                "message.LockToken is a null reference or empty string.");
            _lockToken = message.LockToken;

            byte[] messageBytes = message.GetBytes(); // this needs to be saved if needed later, because it can only be read once from the original Message

            _commandHistory = serializer.DeserializeObject<CommandHistoryND>(messageBytes);
        }

        public CommandHistoryND CommandHistory
        {
            get { return _commandHistory; }
        }

        public string LockToken
        {
            get
            {
                return _lockToken;
            }
        }
    }
}
