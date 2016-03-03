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
