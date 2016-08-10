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

        public Command(string name, IEnumerable<Parameter> parameters = null ) : this()
        {
            Name = name;
            if (parameters != null)
            {
                Parameters.AddRange(parameters);
            }
        }

        public string Name { get; set; }
        public List<Parameter> Parameters { get; set; }
    }
}
