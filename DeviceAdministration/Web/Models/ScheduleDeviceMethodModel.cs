using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{

    public class MethodParameterEditViewModel
    {
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        public string ParameterType { get; set; }
    }

    public class ScheduleDeviceMethodModel : ScheduleJobViewModel
    {
        public string MethodName { get; set; }
        public List<MethodParameterEditViewModel> Parameters { get; set; }
    }
}