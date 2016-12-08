using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class FilterNotFoundException : Exception
    {
        public string FilterId { get; set; }

        public FilterNotFoundException (string filterId)
            : base($"Filter with Id = '{filterId}' could not be found.")
        {
            FilterId = filterId;
        }
    }
}
