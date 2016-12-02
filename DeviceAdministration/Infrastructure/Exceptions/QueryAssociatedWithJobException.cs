using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public class QueryAssociatedWithJobException : Exception
    {
        public string QueryName { get; set; }
        public IEnumerable<string> JobNameList { get; set; }

        public QueryAssociatedWithJobException(string queryName, IEnumerable<string> jobNameList)
            : base($"Query with name = '{queryName}' is associated with jobs: {string.Join(",", jobNameList)}.")
        {
            QueryName = queryName;
            JobNameList = jobNameList;
        }
    }
}
