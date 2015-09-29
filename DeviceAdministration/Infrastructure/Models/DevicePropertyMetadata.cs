namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// A type that provides metadata related to a Device's property.
    /// </summary>
    public class DevicePropertyMetadata
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the property is 
        /// displayed for Registered Devices.
        /// </summary>
        public bool IsDisplayedForRegisteredDevices
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the property is 
        /// displayed for Unregistered Devices.
        /// </summary>
        public bool IsDisplayedForUnregisteredDevices
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the property is 
        /// user-editable.
        /// </summary>
        public bool IsEditable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the described property's name.
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
    }
}
