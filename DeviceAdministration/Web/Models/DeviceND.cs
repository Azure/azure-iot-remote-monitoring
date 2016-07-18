using System;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceND
    {
        public DeviceProperties DeviceProperties { get; set; }
        public SystemProperties SystemProperties { get; set; }
        public List<Command> Commands { get; set; }
        public List<CommandHistoryND> CommandHistory { get; set; }
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
        public IoTHub IoTHub { get; set; }
    }

    public class DeviceProperties
    {
        public string DeviceID { get; set; }
        public bool? HubEnabledState { get; set; }
        public DateTime CreatedTime { get; set; }
        public string DeviceState { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string Manufacturer { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string Platform { get; set; }
        public string Processor { get; set; }
        public string InstalledRAM { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class IoTHub
    {
        public string MessageId { get; set; }
        public string CorrelationId { get; set; }
        public string ConnectionDeviceId { get; set; }
        public string ConnectionDeviceGenerationId { get; set; }
        public DateTime EnqueuedTime { get; set; }
        public string StreamId { get; set; }
    }

    public class Telemetry
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
    }

    public class SystemProperties
    {
        public string ICCID { get; set; }
    }

    //public class Commmand
    //{
    //    public string Name { get; set; }
    //    public CommandParameter[] Parameters { get; set; }
    //}

    //public class CommandParameter
    //{
    //    public string Name { get; set; }
    //    public string Type { get; set; }
    //}
    public class CommandHistoryND
    {
        public string Name { get; set; }
        public string MessageId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}