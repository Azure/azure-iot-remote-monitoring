
namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables
{
    public class Column
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public string Searchable { get; set; }
        public string Orderable { get; set; }
        public Search Search { get; set; }
    }
}
