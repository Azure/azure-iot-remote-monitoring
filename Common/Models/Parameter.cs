﻿namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class Parameter
    {
        /// <summary>
        /// Serialization deserialization constructor.
        /// </summary>
        internal Parameter()
        {
        }

        public Parameter(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public string Type { get; set; }
    }
}
