using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class CommandHistory
    {
        /// <summary>
        /// Creates a new instance of a command history model.
        /// </summary>
        [JsonConstructor]
        internal CommandHistory()
        {
        }

        /// <summary>
        /// Creates a new instance of a command history model.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="parameters">Dynamic list of parameters issued with the command.</param>
        public CommandHistory(string name, DeliveryType deliveryType = DeliveryType.Message, dynamic parameters = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
            DeliveryType = deliveryType;
            MessageId = Guid.NewGuid().ToString();
            CreatedTime = DateTime.UtcNow;
            SetParameters(parameters);
        }

        public string Name { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public string MessageId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string Result { get; set; }
        public string ReturnValue { get; set; }
        public string ErrorMessage { get; set; }
        public dynamic Parameters { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Adds the entire set of parameters to the command history.
        /// </summary>
        /// <param name="parameters">The parameters to set on the history. Must be either a JArray, JObject or a <see cref="Dictionary"/> instance.</param>
        public void SetParameters(dynamic parameters)
        {
            if (parameters == null)
            {
                return;
            }

            if (parameters.GetType() == typeof(Dictionary<string, object>))
            {
                var newParam = new JObject();
                foreach (var parameter in ((Dictionary<string, object>) parameters))
                {
                    newParam.Add(parameter.Key, new JValue(parameter.Value));
                }
                Parameters = newParam;
            }
            else
            {
                Parameters = parameters;
            }
        }

        /// <summary>
        /// Gets the string representation of the history parameters.
        /// </summary>
        /// <returns></returns>
        public string GetParameterString()
        {
            if (Parameters == null)
            {
                return string.Empty;
            }

            return Parameters.ToString();
        }
    }
}