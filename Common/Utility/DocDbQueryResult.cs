using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility
{
    public class DocDbRestQueryResult
    {
        public JArray ResultSet { get; set; }
        public int TotalResults { get; set; }
        public string ContinuationToken { get; set; }
    }
}
