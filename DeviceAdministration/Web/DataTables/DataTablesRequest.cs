using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables
{
    public class DataTablesRequest
    {
        public string QueryName { get; set; }
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public List<Column> Columns { get; set; }
        [JsonProperty("order")]
        public List<SortColumn> SortColumns { get; set; }
        public Search Search { get; set; }
        public List<FilterInfo> Filters { get; set; }
        public string Sql { get; set; }
        public bool IsAdvanced { get; set; }
    }
}