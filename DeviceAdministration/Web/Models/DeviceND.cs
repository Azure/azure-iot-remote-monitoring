using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceND
    {
        public DeviceProperties DeviceProperties;
        public SystemProperties SystemProperties;
        public dynamic[] Commands;
        public dynamic[] CommandHistory;
        public bool IsSimulatedDevice;
        public string id;
        public string _rid;
        public string _self;
        public string _etag;
        public int _ts;
        public string _attachments;
    }

    public class DeviceProperties
    {
        public string DeviceID;
        public string HubEnabledState;
        public string CreatedTime;
        public string DeviceState;
        public string UpdatedTime;
    }

    public class SystemProperties
    {
        public string ICCID;
    }

}