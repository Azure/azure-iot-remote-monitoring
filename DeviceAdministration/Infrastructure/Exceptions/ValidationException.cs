using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public class ValidationException : DeviceAdministrationExceptionBase
    {
        public ValidationException(string deviceId)
            : base(deviceId)
        {
            Errors = new List<string>();
        }

        public ValidationException(string deviceId, Exception innerException)
            : base(deviceId, innerException)
        {
            Errors = new List<string>();
        }

        public List<string> Errors { get; set; }
    }
}
