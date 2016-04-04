
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    /// <summary>
    /// A model for holding data that the Dashboard Device pane shows.
    /// </summary>
    public class DashboardDevicePaneDataModel
    {
        /// <summary>
        /// Gets or sets the ID of the Device for which telemetry should be 
        /// shown.
        /// </summary>
        public string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an array of DeviceTelemetryModel for backing the 
        /// telemetry line graph.
        /// </summary>
        public DeviceTelemetryModel[] DeviceTelemetryModels
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a DeviceTelemetrySummaryModel for backing the 
        /// telemetry summary gauges.
        /// </summary>
        public DeviceTelemetrySummaryModel DeviceTelemetrySummaryModel
        {
            get;
            set;
        }

        public DeviceTelemetryFieldModel[] DeviceTelemetryFields
        {
            get;
            set;
        }
    }
}