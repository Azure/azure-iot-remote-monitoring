using System.Diagnostics;
﻿using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Serialization;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport
{
    /// <summary>
    /// Wraps the byte array returned from the cloud so that it can be deserialized
    /// </summary>
    public class DeserializableCommand
    {
        private readonly dynamic _command;
        private readonly string _lockToken;

        public string CommandName
        {
            get { return _command.Name; }
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

            _command = serializer.DeserializeObject<dynamic>(messageBytes);
        }

        public dynamic Command
        {
            get { return _command;  }
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
