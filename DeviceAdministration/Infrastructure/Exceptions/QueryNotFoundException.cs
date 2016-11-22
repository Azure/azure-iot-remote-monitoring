using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class QueryNotFoundException : Exception
    {
        public string QueryName { get; set; }

        public QueryNotFoundException (string queryName)
            : base($"Query with name = '{queryName}' could not be found.")
        {
            QueryName = queryName;
        }
    }
}
