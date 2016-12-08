using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public class FilterSaveException : Exception
    {
        public string FilterId { get; internal set; }
        public string FilterName { get; internal set; }

        public FilterSaveException (string filterId, string filterName)
            : base($"Failed to save Filter with ID = '{filterId}' and Name = '{filterName}' into the table")
        {
            FilterId = filterId;
            FilterName = filterName;
        }
    }
}
