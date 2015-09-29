using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Stores all data needed to query the devices (sorting, filtering, searching, etc)
    /// </summary>
    public class DeviceListQuery
    {
        /// <summary>
        /// Column-level filter values (can have zero or more)
        /// </summary>
        public List<FilterInfo> Filters { get; set; }

        /// <summary>
        /// General, overarching search query (not specific to a column)
        /// </summary>
        public string SearchQuery { get; set; }
        
        /// <summary>
        /// Requested sorting column
        /// </summary>
        public string SortColumn { get; set; }
        
        /// <summary>
        /// Requested sorting order
        /// </summary>
        public QuerySortOrder SortOrder { get; set;}
        
        /// <summary>
        /// Number of devices to skip at start of list (if Skip = 50, then 
        /// the first 50 devices will be omitted from the list, and devices will
        /// be returned starting with #51 and on)
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Number of devices to return/display
        /// </summary>
        public int Take { get; set; }
    }
}
