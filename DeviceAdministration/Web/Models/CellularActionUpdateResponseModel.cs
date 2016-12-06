using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class CellularActionUpdateResponseModel
    {
        public List<CellularActionModel> CompletedActions { get; set; }
        public List<CellularActionModel> FailedActions { get; set; }
        public bool Success => !FailedActions.Any();
        public string Error { get; set; }
        public List<ErrorModel> Exceptions { get; set; }
    }
}