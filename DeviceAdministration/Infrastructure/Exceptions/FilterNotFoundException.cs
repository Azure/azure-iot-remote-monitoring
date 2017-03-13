using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class FilterNotFoundException : Exception
    {
        public string FilterId { get; set; }

        public FilterNotFoundException(string filterId)
            : base(FormattableString.Invariant($"Filter with Id = '{filterId}' could not be found."))
        {
            FilterId = filterId;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
