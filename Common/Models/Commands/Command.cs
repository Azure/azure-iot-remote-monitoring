using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands
{
    public class Command
    {

        /// <summary>
        /// Serialziation deserialziation constructor.
        /// </summary>
        [JsonConstructor]
        public Command()
        {
            Parameters = new List<Parameter>();
        }

        public Command(string name, DeliveryType deliveryType, string description, IEnumerable<Parameter> parameters = null ) : this()
        {
            Name = name;
            DeliveryType = deliveryType;
            Description = description;
            if (parameters != null)
            {
                Parameters.AddRange(parameters);
            }
        }

        public string Name { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public List<Parameter> Parameters { get; set; }
        public string Description { get; set; }
    }

    public enum DeliveryType
    {
        Message,
        Method
    }
}
