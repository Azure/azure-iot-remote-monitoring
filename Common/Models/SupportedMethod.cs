using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class SupportedMethod
    {
        [JsonConstructor]
        public SupportedMethod()
        {
            Parameters = new Dictionary<string, Parameter>();
        }

        public string Name { get; set; }
        public Dictionary<string, Parameter> Parameters { get; set; }
        public string Description { get; set; }
    }
}
