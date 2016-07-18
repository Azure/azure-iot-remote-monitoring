using System;
using Microsoft.Ajax.Utilities;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceND
    {
        public DeviceProperties DeviceProperties;
        public SystemProperties SystemProperties;
        public Commmand[] Commands;
        public CommandHistory[] CommandHistory;
        public bool IsSimulatedDevice;
        public string id;
        public string _rid;
        public string _self;
        public string _etag;
        public int _ts;
        public string _attachments;

        public Telemetry[] Telemetry;
        public string Version;
        public string ObjectType;
        public IoTHub IoTHub;
    }

    public class DeviceProperties
    {
        public string DeviceID;
        public bool HubEnabledState;
        public DateTime CreatedTime;
        public string DeviceState;
        public DateTime UpdatedTime;
        public string Manufacturer;
        public string ModelNumber;
        public string SerialNumber;
        public string FirmwareVersion;
        public string Platform;
        public string Processor;
        public string InstalledRAM;
        public double? Latitude;
        public double? Longitude;
    }

    public class IoTHub
    {
        public string MessageId;
        public string CorrelationId;
        public string ConnectionDeviceId;
        public string ConnectionDeviceGenerationId;
        public string EnqueuedTime;
        public string StreamId;
    }

    public class Telemetry
    {
        public string Name;
        public string DisplayName;
        public string Type;
    }

    public class SystemProperties
    {
        public string ICCID;
    }

    public class Commmand
    {
        public string Name;
        public CommandParameter[] Parameters;
    }

    public class CommandParameter
    {
        public string Name;
        public string Type;
    }
    public class CommandHistory
    {
        public string Name;
        public string MessageId;
        public string CreatedTime;
        public string UpdatedTime;
        public string Result;
        public string ErrorMessage;
    }
}