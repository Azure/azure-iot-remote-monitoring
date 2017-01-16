using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DesiredPropetiesEditViewModel
    {
        public string PropertyName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PropertyValue { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TwinDataType DataType { get; set; }
        public bool isDeleted { get; set; }
    }

    public class TagsEditViewModel
    {
        public string TagName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TagValue { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TwinDataType DataType { get; set; }
        public bool isDeleted { get; set; }
    }

    public class ScheduleTwinUpdateModel : ScheduleJobViewModel
    {
        public List<DesiredPropetiesEditViewModel> DesiredProperties { get; set; }

        public List<TagsEditViewModel> Tags { get; set; }
    }
}
