using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    /// <summary>
    /// View model for the device details pane.
    ///
    /// Includes methods to return related models for the config sections.
    /// </summary>
    public class DeviceDetailModel
    {
        public string DeviceID
        {
            get;
            set;
        }

        public bool HasKeyViewingPerm
        {
            get
            {
                return PermsChecker.HasPermission(Permission.ViewDeviceSecurityKeys);
            }
        }

        public bool HasConfigEditPerm
        {
            get
            {
                return PermsChecker.HasPermission(Permission.EditDeviceMetadata);
            }
        }

        public bool? HubEnabledState
        {
            get;
            set;
        }

        public bool DeviceIsEnabled
        {
            get
            {
                return this.HubEnabledState == true;
            }
        }

        public SecurityKeys IoTHubKeys { get; set; }

        public bool CanDisableDevice
        {
            get
            {
                return PermsChecker.HasPermission(Permission.DisableEnableDevices);
            }
        }

        public bool CanRemoveDevice
        {
            get
            {
                return PermsChecker.HasPermission(Permission.RemoveDevices);
            }
        }

        public bool IsDeviceEditEnabled
        {
            get
            {
                return PermsChecker.HasPermission(Permission.EditDeviceMetadata);
            }
        }

        public bool CanAddRule
        {
            get
            {
                return PermsChecker.HasPermission(Permission.EditRules);
            }
        }

        /// <summary>
        /// Drives visibility and display of keys and edit functionality on the device detail pane
        /// </summary>
        /// <returns></returns>
        public DeviceDetailsKeysModel GetKeys()
        {
            var keysModel = new DeviceDetailsKeysModel
            {
                DeviceId = this.DeviceID
            };

            return keysModel;
        }

        public List<DevicePropertyValueModel> DevicePropertyValueModels
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the described property's current value.
        /// </summary>
        public bool IsCellular
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the described property's current value.
        /// </summary>
        public string Iccid
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
    }
}