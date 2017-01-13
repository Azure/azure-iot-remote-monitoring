using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class PropertyViewModel
    {
        public string Key { get; set; }
        public PropertyViewValue Value { get; set; }
        public bool IsDeleted { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TwinDataType DataType { get; set; }
    }

    public class PropertyViewValue
    {
        public dynamic Value { get; set; }
        public string LastUpdate { get; set; }
    }
}