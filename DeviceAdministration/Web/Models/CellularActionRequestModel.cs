using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class CellularActionRequestModel
    {
        public string DeviceId { get; set; }
        public List<CellularActionModel> CellularActions { get; set; }
    }
}