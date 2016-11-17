using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// A model that describes how a Device's property should be displayed.
    /// </summary>
    public class DevicePropertyValueModel
    {
        /// <summary>
        /// Gets or sets a value suggesting the order in which the property 
        /// should be displayed, relative to its peers.
        /// </summary>
        public int DisplayOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the described property is 
        /// user-editable.
        /// </summary>
        public bool IsEditable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the described property 
        /// should be included when handling unregistered Devices.
        /// </summary>
        public bool IsIncludedWithUnregisteredDevices
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the described property's name--as opposed to its path.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the descriped property's <see cref="PropertyType" />.
        /// </summary>
        public PropertyType PropertyType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the described property's current value.
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Get the last updated date time of the described property
        /// </summary>
        public DateTime? LastUpdatedUtc
        {
            get;
            set;
        }
    }
}
