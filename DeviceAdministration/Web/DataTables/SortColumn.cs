using System.Globalization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables
{
    public class SortColumn
    {
        [JsonProperty("column")]
        public string ColumnIndexAsString { get; set; }
        public int ColumnIndex 
        {
            get { return int.Parse(this.ColumnIndexAsString, NumberStyles.Integer, CultureInfo.CurrentCulture); }
        }
        [JsonProperty("dir")]
        private string Direction { get; set; }

        public QuerySortOrder SortOrder 
        {
            get
            {
                return Direction == "asc" ? QuerySortOrder.Ascending : QuerySortOrder.Descending;
            }
        }
    }
}