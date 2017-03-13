using System;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class Parameter
    {
        /// <summary>
        /// Serialization deserialization constructor.
        /// </summary>
        [JsonConstructor]
        public Parameter()
        {
        }

        public Parameter(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public string Type { get; set; }

        public string Serialize()
        {
            return FormattableString.Invariant($"{Name}-{Type}");
        }

        static public Parameter Deserialize(string serialized)
        {
            var parts = serialized.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new ArgumentException(FormattableString.Invariant($"Invalidate serialized parameter format: {serialized}"));
            }

            return new Parameter
            {
                Name = string.Join("-", parts.Take(parts.Length - 1)),
                Type = parts.Last()
            };
        }
    }
}
