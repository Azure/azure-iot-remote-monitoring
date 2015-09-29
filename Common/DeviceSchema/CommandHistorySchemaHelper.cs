using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    /// <summary>
    /// Helper class to encapsulate interactions with the command history schema.
    /// 
    /// Elsewhere in the app we try to always deal with this flexible schema as dynamic,
    /// but here we take a dependency on Json.Net to populate the objects behind the schema.
    /// </summary>
    public static class CommandHistorySchemaHelper
    {
        public const string RESULT_PENDING ="Pending";
        public const string RESULT_SENT = "Sent";
        public const string RESULT_RECEIVED = "Received";
        public const string RESULT_SUCCESS = "Success";
        public const string RESULT_ERROR = "Error";

        public static dynamic BuildNewCommandHistoryItem(string command)
        {
            JObject result = new JObject();

            result.Add(DeviceCommandConstants.NAME, command);
            result.Add(DeviceCommandConstants.MESSAGE_ID, Guid.NewGuid());
            result.Add(DeviceCommandConstants.CREATED_TIME, DateTime.UtcNow);

            return result;
        }

        /// <summary>
        /// Given an existing command and a set of parameters, add the entire set of parameters to the command in the format needed to send the
        /// command to the device or to store it in the command history. The parameters must be either a JArray of JObjects, or a Dictionary with
        /// string keys and object values.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public static void AddParameterCollectionToCommandHistoryItem(dynamic command, dynamic parameters)
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
                command.Add(DeviceCommandConstants.PARAMETERS, newParam);
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

        public static dynamic GetCommandHistory(dynamic device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            dynamic history = device.CommandHistory;

            if (history == null)
            {
                history = new JArray();
                device.CommandHistory = history;
            }

            return history;
        }

        public static IEnumerable<object> GetCommandHistoryItemOrDefault(dynamic device, string messageId)
        {
            dynamic result = null;

            dynamic history = GetCommandHistory(device);

            int commandIndex = GetCommandHistoryItemIndex(history, messageId);
            if(commandIndex > -1)
            {
                result = history[commandIndex];
            }

            if(result == null)
            {
                result = new JObject();
                result.CreatedTime = DateTime.UtcNow;
                result.MessageId = messageId;
                result.Parameters = new JObject();
            }

            return result;
        }

        private static int GetCommandHistoryItemIndex(dynamic commandHistory, string messageId)
        {
            IEnumerable commands;
            object foundId;
            int i;
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

            if ((commands = commandHistory as IEnumerable) != null)
            {
                i = -1;
                foreach (object command in commands)
                {
                    ++i;

                    if (command == null)
                    {
                        continue;
                    }

                    foundId =
                        ReflectionHelper.GetNamedPropertyValue(
                            command,
                            DeviceCommandConstants.MESSAGE_ID,
                            true,
                            false);

                    if ((foundId != null) &&
                        (foundId.ToString() == messageId))
                    {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        }

        public static string GetParameterValueOrNull(dynamic command, string parameterName)
        {
            object obj;
            IEnumerable parameters;

            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentException(
                    "parameterName is a null reference or empty string.",
                    "parameterName");
            }

            obj =
                ReflectionHelper.GetNamedPropertyValue(
                    (object)command,
                    "Parameters",
                    true,
                    false);

            if ((parameters = obj as IEnumerable) != null)
            {
                foreach (object parameter in parameters)
                {
                    obj =
                        ReflectionHelper.GetNamedPropertyValue(
                            (object)parameter,
                            parameterName,
                            true,
                            false);

                    if (obj != null)
                    {
                        return obj.ToString();
                    }
                }
            }

            return default(string);
        }

        public static void UpdateCommandHistoryItem(dynamic device, dynamic command)
        {
            dynamic history = GetCommandHistory(device);

            int commandIndex = GetCommandHistoryItemIndex(history, (string)command.MessageId);
            if (commandIndex > -1)
            {
                history[commandIndex] = command;
            }
        }

        public static void AddCommandToHistory(dynamic device, dynamic command)
        {
            dynamic history = GetCommandHistory(device);
            ((JArray)history).Add(command);
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
