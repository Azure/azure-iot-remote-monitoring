using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// A model that provides information about a single telemetry field
    /// </summary>
    public class DeviceTelemetryFieldModel
    {
        /// <summary>
        /// The user-friendly name for the field
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the field in storage
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The type of the field, such as "double" or "integer"
        /// </summary>
        public string Type
        {
            get;
            set;
        }
    }
}
