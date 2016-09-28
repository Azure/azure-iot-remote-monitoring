using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class CellularActionModel
    {
        public CellularActionType Type { get; set; }
        public string CurrentValue { get; set; }
        public string NewValue { get; set; }
    }

    public enum CellularActionType
    {
        UpdateStatus=1,
        UpdateSubscriptionPackage
    }
}