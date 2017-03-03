using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class DeviceModel
    {
        /// <summary>
        /// Creates a new instance of a DeviceModel.
        /// </summary>
        public DeviceModel()
        {
            Commands = new List<Command>();
            CommandHistory = new List<CommandHistory>();
            Telemetry = new List<Telemetry>();
        }

        /// <summary>
        /// Start from version 1.6, device properties in this class will be ignored.
        /// Please use 'reported properties' of twin to report properties
        /// </summary>
        public DeviceProperties DeviceProperties { get; set; }

        public SystemProperties SystemProperties { get; set; }
        public List<Command> Commands { get; set; }
        public List<CommandHistory> CommandHistory { get; set; }
        public bool IsSimulatedDevice { get; set; }
        public string id { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public int _ts { get; set; }
        public string _attachments { get; set; }

        public List<Telemetry> Telemetry { get; set; }
        public string Version { get; set; }
        public string ObjectType { get; set; }
        public string ObjectName { get; set; }
        public IoTHub IoTHub { get; set; }

        public Twin Twin { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}