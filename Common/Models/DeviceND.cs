using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    [DataContract]
    public class DeviceND
    {

        [DataMember(Name = "deviceProperties")]
        public DeviceProperties DeviceProperties { get; set; }

        [DataMember(Name = "systemProperties")]
        public SystemProperties SystemProperties { get; set; }

        [DataMember(Name = "commands")]
        public List<Command> Commands { get; set; }

        [DataMember(Name = "commandHistory")]
        public List<CommandHistoryND> CommandHistory { get; set; }

        [DataMember(Name = "isSimulatedDevice")]
        public bool IsSimulatedDevice { get; set; }

        [DataMember(Name = "id")]
        public string id { get; set; }

        [DataMember(Name = "_rid")]
        public string _rid { get; set; }

        [DataMember(Name = "_self")]
        public string _self { get; set; }

        [DataMember(Name = "_etag")]
        public string _etag { get; set; }

        [DataMember(Name = "_ts")]
        public int _ts { get; set; }

        [DataMember(Name = "_attachments")]
        public string _attachments { get; set; }

        [DataMember(Name = "telemetry")]
        public List<Telemetry> Telemetry { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "objectType")]
        public string ObjectType { get; set; }

        [DataMember(Name = "objectName")]
        public string ObjectName { get; set; }

        [DataMember(Name = "ioTHub")]
        public IoTHub IoTHub { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public class DeviceProperties
    {

        [DataMember(Name = "deviceID")]
        public string DeviceID { get; set; }

        [DataMember(Name = "hubEnabledState")]
        public bool? HubEnabledState { get; set; }

        [DataMember(Name = "createdTime")]
        public DateTime? CreatedTime { get; set; }

        [DataMember(Name = "deviceState")]
        public string DeviceState { get; set; }

        [DataMember(Name = "updatedTime")]
        public DateTime? UpdatedTime { get; set; }

        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(Name = "modelNumber")]
        public string ModelNumber { get; set; }

        [DataMember(Name = "serialNumber")]
        public string SerialNumber { get; set; }

        [DataMember(Name = "firmwareVersion")]
        public string FirmwareVersion { get; set; }

        [DataMember(Name = "platform")]
        public string Platform { get; set; }

        [DataMember(Name = "processor")]
        public string Processor { get; set; }

        [DataMember(Name = "installedRAM")]
        public string InstalledRAM { get; set; }

        [DataMember(Name = "latitude")]
        public double? Latitude { get; set; }

        [DataMember(Name = "longitude")]
        public double? Longitude { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public class IoTHub
    {

        [DataMember(Name = "messageId")]
        public string MessageId { get; set; }

        [DataMember(Name = "correlationId")]
        public string CorrelationId { get; set; }

        [DataMember(Name = "connectionDeviceId")]
        public string ConnectionDeviceId { get; set; }

        [DataMember(Name = "connectionDeviceGenerationId")]
        public string ConnectionDeviceGenerationId { get; set; }

        [DataMember(Name = "enqueuedTime")]
        public DateTime EnqueuedTime { get; set; }

        [DataMember(Name = "streamId")]
        public string StreamId { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public class Telemetry
    {

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public class SystemProperties
    {

        [DataMember(Name = "iccid")]
        public string ICCID { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public class CommandHistoryND
    {

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "messageId")]
        public string MessageId { get; set; }

        [DataMember(Name = "createdTime")]
        public DateTime CreatedTime { get; set; }

        [DataMember(Name = "updatedTime")]
        public DateTime UpdatedTime { get; set; }

        [DataMember(Name = "result")]
        public string Result { get; set; }

        [DataMember(Name = "errorMessage")]
        public string ErrorMessage { get; set; }

        [DataMember(Name = "parameters")]
        public dynamic Parameters { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}