using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// A model representing an Alert History item.
    /// </summary>
    public class AlertHistoryItemModel
    {
        /// <summary>
        /// Gets or sets the ID of the Device that the represented Alert 
        /// History item covers.
        /// </summary>
        public string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the represented Alert History item's Rule Output.
        /// </summary>
        public string RuleOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time at whichthe Alert History item occurred.
        /// </summary>
        public DateTime? Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the represented Alert History item's Value.
        /// </summary>
        public string Value
        {
            get;
            set;
        }
    }
}
