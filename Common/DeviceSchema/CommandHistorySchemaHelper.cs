using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    /// <summary>
    /// Helper class to encapsulate interactions with the command history schema.
    /// 
    /// </summary>
    public static class CommandHistorySchemaHelper
    {
        public const string RESULT_PENDING ="Pending";
        public const string RESULT_SENT = "Sent";
        public const string RESULT_RECEIVED = "Received";
        public const string RESULT_SUCCESS = "Success";
        public const string RESULT_ERROR = "Error";

        public static CommandHistory BuildNewCommandHistoryItem(string command)
        {
            CommandHistory result = new CommandHistory();

            result.Name = command;
            result.MessageId = Guid.NewGuid().ToString();
            result.CreatedTime = DateTime.UtcNow;

            return result;
        }

        /// <summary>
        /// Given an existing command and a set of parameters, add the entire set of parameters to the command in the format needed to send the
        /// command to the device or to store it in the command history. The parameters must be either a JArray of JObjects, or a Dictionary with
        /// string keys and object values.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public static void AddParameterCollectionToCommandHistoryItem(CommandHistory command, dynamic parameters)
        {
            if (parameters == null)
            {
                return;
            }

            if (parameters.GetType() == typeof(Dictionary<string, object>))
            {
                JObject newParam = new JObject();
                //We have a strongly-typed Collection
                foreach (string key in ((Dictionary<string, object>)parameters).Keys)
                {
                    newParam.Add(key, new JValue(((Dictionary<string, object>)parameters)[key]));
                }
                command.Parameters = newParam;
            }
            else if (parameters.GetType() == typeof(JArray))
            {
                //We have JSON
                command.Parameters = parameters;
            }
            else
            {
                throw new ArgumentException("The parameters argument must be either a Dictionary with string keys or a JArray");
            }
        }

        public static List<CommandHistory> GetCommandHistory(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var history = device.CommandHistory;

            if (history == null)
            {
                history = new List<CommandHistory>();
                device.CommandHistory = history;
            }

            return history;
        }

        public static CommandHistory GetCommandHistoryItemOrDefault(Models.Device device, string messageId)
        {
            CommandHistory result = null;

            IList<CommandHistory> history = GetCommandHistory(device);

            int commandIndex = GetCommandHistoryItemIndex(history, messageId);
            if (commandIndex > -1)
            {
                result = history[commandIndex];
            }

            if (result == null)
            {
                result.CreatedTime = DateTime.UtcNow;
                result.MessageId = messageId;
                result.Parameters = new JObject();
            }

            return result;
        }

        private static int GetCommandHistoryItemIndex(IList<CommandHistory> commandHistory, string messageId)
        {
            int i = -1;
            int result = -1;

            if (commandHistory == null)
            {
                throw new ArgumentNullException("commandHistory");
            }

            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentException(
                    "messageId is a null reference or empty string.",
                    "messageId");
            }

            foreach (CommandHistory command in commandHistory)
            {
                ++i;

                if (command == null)
                {
                    continue;
                }

                string foundId = command.MessageId;

                if ((foundId != null) &&
                    (foundId == messageId))
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        public static void UpdateCommandHistoryItem(Models.Device device, CommandHistory command)
        {
            IList<CommandHistory> history = GetCommandHistory(device);

            int commandIndex = GetCommandHistoryItemIndex(history, command.MessageId);
            if (commandIndex > -1)
            {
                history[commandIndex] = command;
            }
        }

        /// <summary>
        /// Given a Device command (one that is on its way to the device, or from a device's command history) get the parameters as a Json string.
        /// This may be used for binding to a UI element such as a button to resend the same command. 
        /// </summary>
        /// <param name="deviceCommand"></param>
        /// <returns></returns>
        public static string GetCommandParametersAsJsonString(dynamic deviceCommand)
        {
            if (deviceCommand.Parameters == null)
            {
                return string.Empty;
            }
            return deviceCommand.Parameters.ToString();
        }
    }
}
