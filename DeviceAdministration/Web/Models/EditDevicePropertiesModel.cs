using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    /// <summary>
    /// A view model for backing the EditDeviceProperties page.
    /// </summary>
    public class EditDevicePropertiesModel
    {
        /// <summary>
        /// Gets or sets the edited Device's ID.
        /// </summary>
        public string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// indicates  if a device is simulated
        /// </summary>
        public bool IsSimulatedDevice
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of <see cref="DevicePropertyValueModel" />s, 
        /// describing the edited properties.
        /// </summary>
        public List<DevicePropertyValueModel> DevicePropertyValueModels
        {
            get;
            set;
        }
    }
}