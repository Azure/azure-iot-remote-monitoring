using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// A model that summarizes a period of Device telemetry.
    /// </summary>
    public class DeviceTelemetrySummaryModel
    {
        /// <summary>
        /// Gets or sets the covered period's average humidity.
        /// </summary>
        public double? AverageHumidity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ID of the Device, covered by the summarized 
        /// telemetry.
        /// </summary>
        public string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the covered period's maximum humidity.
        /// </summary>
        public double? MaximumHumidity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the covered period's minimum humidity.
        /// </summary>
        public double? MinimumHumidity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of minutes the represented period covers.
        /// </summary>
        public double? TimeFrameMinutes
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
