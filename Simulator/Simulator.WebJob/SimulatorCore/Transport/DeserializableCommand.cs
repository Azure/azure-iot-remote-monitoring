using System.Diagnostics;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Serialization;
using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport
{
    /// <summary>
    /// Wraps the byte array returned from the cloud so that it can be deserialized
    /// </summary>
    public class DeserializableCommand
    {
        private readonly CommandHistory _commandHistory;
        private readonly string _lockToken;

        public string CommandName
        {
            get { return _commandHistory.Name; }
        }

        public DeserializableCommand(CommandHistory history, string lockToken)
        {
            _commandHistory = history;
            _lockToken = lockToken;
        }

        public DeserializableCommand(Client.Message message, ISerialize serializer)
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

            _commandHistory = serializer.DeserializeObject<CommandHistory>(messageBytes);
        }

        public CommandHistory CommandHistory
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
