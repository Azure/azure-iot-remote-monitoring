using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Represents a response from a WebAPI call that includes details of errors (if any are present)
    /// and the data to return to the caller
    /// </summary>
    /// <typeparam name="T">Type of the data to return to the caller</typeparam>
    public class ServiceResponse<T>
    {
        public List<Error> Error{ get; set; }
        
        public T Data { get; set; }

        public ServiceResponse()
        {
            Error = new List<Error>();
        }

        /// <summary>
        /// Tells JSON.NET not to serlialize the Error property if there are no 
        /// items in the list.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeError()
        {
            return Error.Count > 0;
        }
    }
}
