using System;

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
        private readonly string _message;

        public DeviceRequiredPropertyNotFoundException(string message)
        {
            _message = message;
        }

        public DeviceRequiredPropertyNotFoundException(string message, Exception innerException)
        {
            _message = message;
        }

        public override string Message
        {
            get
            {
                return _message;
            }
        }
    }
}
