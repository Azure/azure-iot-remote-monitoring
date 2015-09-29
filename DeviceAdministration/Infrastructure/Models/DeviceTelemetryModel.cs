using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// A model that represents a Device's telemetry recording.
    /// </summary>
    public class DeviceTelemetryModel
    {
        /// <summary>
        /// Gets or sets the ID of the Device for which telemetry applies.
        /// </summary>
        public string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the represented telemetry recording's external 
        /// temperature value.
        /// </summary>
        public double? ExternalTemperature
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the represented telemetry recording's humidity 
        /// value.
        /// </summary>
        public double? Humidity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the represented telemetry recording's temperature 
        /// value.
        /// </summary>
        public double? Temperature
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time of record for the represented telemetry 
        /// recording.
        /// </summary>
        public DateTime? Timestamp
        {
            get;
            set;
        }
    }
}
