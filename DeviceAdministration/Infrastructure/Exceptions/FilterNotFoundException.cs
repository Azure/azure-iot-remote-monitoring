using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class FilterNotFoundException : Exception
    {
        public string FilterName { get; set; }

        public FilterNotFoundException (string filterName)
            : base($"Filter with name = '{filterName}' could not be found.")
        {
            FilterName = filterName;
        }
    }
}
