namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables
{
    public class DataTablesResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public dynamic[] Data { get; set; }
    }
}