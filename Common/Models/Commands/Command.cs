using System;
using System.Collections.Generic;
using System.Linq;
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

        public Command(string name, DeliveryType deliveryType, string description, IEnumerable<Parameter> parameters = null) : this()
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

        public KeyValuePair<string, string> Serialize()
        {
            var parts = new string[] { Name }.Concat(Parameters.Select(p => FormattableString.Invariant($"{p.Name}-{p.Type}")));

            return new KeyValuePair<string, string>(string.Join("--", parts), Description);
        }

        static public Command Deserialize(string key, string value)
        {
            var parts = key.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);

            return new Command
            {
                Name = parts.First(),
                DeliveryType = DeliveryType.Method,
                Description = value,
                Parameters = parts.Skip(1).Select(s => Parameter.Deserialize(s)).ToList()
            };
        }
    }

    public enum DeliveryType
    {
        Message,
        Method
    }
}
