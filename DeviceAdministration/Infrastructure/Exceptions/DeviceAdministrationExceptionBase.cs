using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    /// <summary>
    /// Simple base class for device administration based exceptions
    /// </summary>
    public abstract class DeviceAdministrationExceptionBase : Exception
    {
        public string DeviceId { get; set; }

        public DeviceAdministrationExceptionBase(string deviceId)
        {
            DeviceId = deviceId;
        }

        public DeviceAdministrationExceptionBase(string deviceId, Exception innerException)
            : base(deviceId, innerException)
        {
            DeviceId = deviceId;
        }
    }
}
