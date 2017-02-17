using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public class FilterDuplicatedNameException : Exception
    {
        public string FilterId { get; internal set; }
        public string FilterName { get; internal set; }

        public FilterDuplicatedNameException(string filterId, string filterName)
            : base($"Failed to save Filter with ID = '{filterId}' and Name = '{filterName}', the filter name must be unique")
        {
            FilterId = filterId;
            FilterName = filterName;
        }
    }
}
