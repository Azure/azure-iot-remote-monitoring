using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class SimActionModel
    {
        public string Type { get; set; }
        public string CurrentValue { get; set; }
        public string NewValue { get; set; }
    }
}