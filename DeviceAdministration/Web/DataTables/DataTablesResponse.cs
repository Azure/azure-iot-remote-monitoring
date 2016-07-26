using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables
{
    public class DataTablesResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
    }

    public class RuleDataTablesResponse : DataTablesResponse
    {
        public DeviceRule[] Data { get; set; }
    }

    public class ActionDataTablesResponse : DataTablesResponse
    {
        public ActionMappingExtended[] Data { get; set; }
    }
}