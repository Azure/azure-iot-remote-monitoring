using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public class FilterAssociatedWithJobException : Exception
    {
        public string FilterName { get; set; }
        public IEnumerable<string> JobNameList { get; set; }

        public FilterAssociatedWithJobException(string filterName, IEnumerable<string> jobNameList)
            : base($"Filter with name = '{filterName}' is associated with jobs: {string.Join(",", jobNameList)}.")
        {
            FilterName = filterName;
            JobNameList = jobNameList;
        }
    }
}
