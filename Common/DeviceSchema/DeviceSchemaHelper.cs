using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Schema;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    /// <summary>
    /// Helper class to encapsulate interactions with the device schema.
    ///
    /// behind the schema.
    /// </summary>
    public static class DeviceSchemaHelper
    {
        /// <summary>
        /// Gets a DeviceProperties instance from a Device.
        /// </summary>
        /// <param name="device">
        /// The Device from which to extract a DeviceProperties instance.
        /// </param>
        /// <returns>
        /// A DeviceProperties instance, extracted from <paramref name="device"/>.
        /// </returns>
        public static DeviceProperties GetDeviceProperties(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = device.DeviceProperties;

            if (props == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
            }

            return props;
        }

        /// <summary>
        /// Gets a IoTHubProperties instance from a device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static IoTHub GetIoTHubProperties(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = device.IoTHub;

            if (props == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'IoTHubProperties' property is missing");
            }

            return props;
        }

        /// <summary>
        /// Gets a Device instance's Device ID.
        /// </summary>
        /// <param name="device">
        /// The Device instance from which to extract a Device ID.
        /// </param>
        /// <returns>
        /// The Device ID, extracted from <paramref name="device" />.
        /// </returns>
        public static string GetDeviceID(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            string deviceID = props.DeviceID;

            if (deviceID == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceID' property is missing");
            }

            return deviceID;
        }

        /// <summary>
        /// Get connection device id 
        /// </summary>
        /// <param name="device">Device instance from message</param>
        /// <returns>Connection device id from IoTHub</returns>
        public static string GetConnectionDeviceId(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetIoTHubProperties(device);

            string deviceID = props.ConnectionDeviceId;

            if (deviceID == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceID' property is missing");
            }

            return deviceID;
        }

        /// <summary>
        /// Extract's a Device instance's Created Time value.
        /// </summary>
        /// <param name="device">
        /// The Device instance from which to extract a Created Time value.
        /// </param>
        /// <returns>
        /// A Created Time value, extracted from <paramref name="device" />.
        /// </returns>
        public static DateTime GetCreatedTime(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            DateTime? createdTime = props.CreatedTime;

            if (!createdTime.HasValue)
            {
                throw new DeviceRequiredPropertyNotFoundException("'CreatedTime' property is missing");
            }

            return createdTime.Value;
        }

        /// <summary>
        /// Extracts an Updated Time value from a Device instance.
        /// </summary>
        /// <param name="device">
        /// The Device instance from which to extract an Updated Time value.
        /// </param>
        /// <returns>
        /// The Updated Time value, extracted from <paramref name="device" />, or
        /// null if it is null or does not exist.
        /// </returns>
        public static DateTime? GetUpdatedTime(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            // note that since null is a valid value, don't try to test if the actual UpdateTime is there

            return props.UpdatedTime;
        }

        /// <summary>
        /// Set the current time (in UTC) to the device's UpdatedTime Device Property
        /// </summary>
        /// <param name="device"></param>
        public static void UpdateUpdatedTime(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            props.UpdatedTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Extracts a Hub Enabled State value from a Device instance.
        /// </summary>
        /// <param name="device">
        /// The Device instance from which to extract a Hub Enabled State
        /// value.
        /// </param>
        /// <returns>
        /// The Hub Enabled State value extracted from <paramref name="device"/>,
        /// or null if the value is missing or null.
        /// </returns>
        public static bool? GetHubEnabledState(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            // note that since null is a valid value, don't try to test if the actual HubEnabledState is there

            return props.HubEnabledState;
        }

        /// <summary>
        /// Build a valid device representation used throughout the app.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="isSimulated"></param>
        /// <param name="iccid"></param>
        /// <returns></returns>
        public static Models.Device BuildDeviceStructure(string deviceId, bool isSimulated, string iccid)
        {
            Models.Device device = new Models.Device();

            InitializeDeviceProperties(device, deviceId, isSimulated);
            InitializeSystemProperties(device, iccid);

            device.Commands = new List<Command>();
            device.CommandHistory = new List<CommandHistory>();
            device.IsSimulatedDevice = isSimulated;

            return device;
        }

        /// <summary>
        /// Initialize the device properties for a new device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="deviceId"></param>
        /// <param name="isSimulated"></param>
        /// <returns></returns>
        public static void InitializeDeviceProperties(Models.Device device, string deviceId, bool isSimulated)
        {
            DeviceProperties deviceProps = new DeviceProperties();
            deviceProps.DeviceID = deviceId;
            deviceProps.HubEnabledState = null;
            deviceProps.CreatedTime = DateTime.UtcNow;
            deviceProps.DeviceState = "normal";
            deviceProps.UpdatedTime = null;

            device.DeviceProperties = deviceProps;
        }

        /// <summary>
        /// Initialize the system properties for a new device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="iccid"></param>
        /// <returns></returns>
        public static void InitializeSystemProperties(Models.Device device, string iccid)
        {
            SystemProperties systemProps = new SystemProperties();
            systemProps.ICCID = iccid;

            device.SystemProperties = systemProps;
        }

        /// <summary>
        /// Remove the system properties from a device, to better emulate the behavior of real devices when sending device info messages.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="iccid"></param>
        /// <param name="isSimulated"></param>
        /// <returns></returns>
        public static void RemoveSystemPropertiesForSimulatedDeviceInfo(Models.Device device)
        {
            // Our simulated devices share the structure code with the rest of the system,
            // so we need to explicitly handle this case; since this is only an issue when
            // the code is shared in this way, this special case is kept separate from the
            // rest of the initialization code which would be present in a non-simulated system
            device.SystemProperties = null;
        }
    }
}
