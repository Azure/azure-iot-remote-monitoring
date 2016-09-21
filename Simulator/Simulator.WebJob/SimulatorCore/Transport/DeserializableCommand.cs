using System.Diagnostics;
using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using System.Text;
using Newtonsoft.Json;

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
           this._commandHistory = history;
           this._lockToken = lockToken;
        }

        public DeserializableCommand(Client.Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Debug.Assert(
                !string.IsNullOrEmpty(message.LockToken),
                "message.LockToken is a null reference or empty string.");
            _lockToken = message.LockToken;

            byte[] messageBytes = message.GetBytes(); // this needs to be saved if needed later, because it can only be read once from the original Message

            string jsonData = Encoding.UTF8.GetString(messageBytes);
            _commandHistory = JsonConvert.DeserializeObject<CommandHistory>(jsonData);
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
