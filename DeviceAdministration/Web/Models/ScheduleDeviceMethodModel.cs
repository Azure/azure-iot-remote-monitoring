using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{

    public class MethodParameterEditViewModel
    {
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TwinDataType Type { get; set; }
    }

    public class ScheduleDeviceMethodModel : ScheduleJobViewModel
    {
        [Required]
        public string MethodName { get; set; }
        public List<MethodParameterEditViewModel> Parameters { get; set; }
    }
}