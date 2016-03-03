using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when required device properties are not found.
    /// 
    /// Note that this cannot inherit from the DeviceAdminExceptionBase as we 
    /// may not know the DeviceID in this case.
    /// </summary>
    public class DeviceRequiredPropertyNotFoundException : Exception
    {
        public DeviceRequiredPropertyNotFoundException(string message) : base(message)
        {
        }

        public DeviceRequiredPropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
