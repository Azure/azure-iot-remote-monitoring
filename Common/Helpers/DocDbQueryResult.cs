using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class DocDbRestQueryResult
    {
        public JArray Documents { get; set; }
        public int TotalDocuments { get; set; }
        public string ContinuationToken { get; set; }
    }
}
