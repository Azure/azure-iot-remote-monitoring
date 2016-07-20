using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    /// <summary>
    /// Helper class to encapsulate interactions with the device schema.
    ///
    /// Elsewhere in the app we try to always deal with this flexible schema as dynamic,
    /// but here we take a dependency on Json.Net where necessary to populate the objects
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
        public static dynamic GetDeviceProperties(dynamic device)
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
        public static dynamic GetIoTHubProperties(dynamic device)
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
        public static string GetDeviceID(dynamic device)
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
        public static string GetConnectionDeviceId(dynamic device)
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
        public static DateTime GetCreatedTime(dynamic device)
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
        public static DateTime? GetUpdatedTime(dynamic device)
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
        public static void UpdateUpdatedTime(dynamic device)
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
        public static bool? GetHubEnabledState(dynamic device)
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
        /// Several aspects of the device schema can be modified after passing through and ASA Event Stream
        /// or some other process. Fix up the schema to keep it clean.
        /// </summary>
        /// <param name="device"></param>
        public static void FixDeviceSchema(dynamic device)
        {
            FixHubEnabledStateFormat(device);
            RemoveUnwantedAsaEventProperties(device);
        }

        /// <summary>
        /// Verify that the hub enabled state is stored in the correct format,
        /// and try to fix incorrect formats if possible.
        /// </summary>
        /// <param name="device"></param>
        private static void FixHubEnabledStateFormat(dynamic device)
        {
            dynamic props = GetDeviceProperties(device);
            if (props.HubEnabledState != null && props.HubEnabledState == 1)
            {
                props.HubEnabledState = true;
            }
            else if (props.HubEnabledState != null && props.HubEnabledState == 0)
            {
                props.HubEnabledState = false;
            }
        }

        /// <summary>
        /// Running the device through ASA can add certain unwanted properties that will persist in
        /// non-strongly typed schemas like Json. Remove those unwanted properties. It may be necessary
        /// to check the type of data we are working with and pass the object on to another private
        /// helper method to handle that specific type of data.
        /// </summary>
        /// <param name="device"></param>
        private static void RemoveUnwantedAsaEventProperties(dynamic device)
        {
            if (device.GetType() == typeof(JObject))
            {
                RemoveUnwantedAsaEventPropertiesFromJObject((JObject)device);
            }
        }

        /// <summary>
        /// Remove unwanted properties that were added by ASA to a Json representation of a device.
        /// </summary>
        /// <param name="device"></param>
        private static void RemoveUnwantedAsaEventPropertiesFromJObject(JObject device)
        {
            device.Remove("EventProcessedUtcTime");
            device.Remove("EventEnqueuedUtcTime");
            device.Remove("PartitionId");
        }

        /// <summary>
        /// _rid is used internally by the DocDB and is required for use with DocDB.
        /// (_rid is resource id)
        /// </summary>
        /// <param name="device">Device data</param>
        /// <returns>_rid property value as string, or empty string if not found</returns>
        public static string GetDocDbRid(dynamic device)
        {
            return SchemaHelper.GetDocDbRid(device);
        }

        /// <summary>
        /// id is used internally by the DocDB and is sometimes required.
        /// </summary>
        /// <param name="device">Device data</param>
        /// <returns>Value of the id, or empty string if not found</returns>
        public static string GetDocDbId(dynamic device)
        {
            return SchemaHelper.GetDocDbId(device);
        }

        /// <summary>
        /// Build a valid device representation in the dynamic format used throughout the app.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="isSimulated"></param>
        /// <param name="iccid"></param>
        /// <returns></returns>
        public static dynamic BuildDeviceStructure(string deviceId, bool isSimulated, string iccid)
        {
            JObject device = new JObject();

            InitializeDeviceProperties(device, deviceId, isSimulated);
            InitializeSystemProperties(device, iccid);

            device.Add(DeviceModelConstants.COMMANDS, new JArray());
            device.Add(DeviceModelConstants.COMMAND_HISTORY, new JArray());
            device.Add(DeviceModelConstants.IS_SIMULATED_DEVICE, isSimulated);

            return device;
        }

        /// <summary>
        /// Initialize the device properties for a new device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="deviceId"></param>
        /// <param name="isSimulated"></param>
        /// <returns></returns>
        public static void InitializeDeviceProperties(dynamic device, string deviceId, bool isSimulated)
        {
            JObject deviceProps = new JObject();
            deviceProps.Add(DevicePropertiesConstants.DEVICE_ID, deviceId);
            deviceProps.Add(DevicePropertiesConstants.HUB_ENABLED_STATE, null);
            deviceProps.Add(DevicePropertiesConstants.CREATED_TIME, DateTime.UtcNow);
            deviceProps.Add(DevicePropertiesConstants.DEVICE_STATE, "normal");
            deviceProps.Add(DevicePropertiesConstants.UPDATED_TIME, null);

            (device as JObject).Add(DeviceModelConstants.DEVICE_PROPERTIES, deviceProps);
        }

        /// <summary>
        /// Initialize the system properties for a new device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="iccid"></param>
        /// <returns></returns>
        public static void InitializeSystemProperties(dynamic device, string iccid)
        {
            JObject systemProps = new JObject();
            systemProps.Add(SystemPropertiesConstants.ICCID, iccid);

            (device as JObject).Add(DeviceModelConstants.SYSTEM_PROPERTIES, systemProps);
        }

        /// <summary>
        /// Remove the system properties from a device, to better emulate the behavior of real devices when sending device info messages.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="iccid"></param>
        /// <param name="isSimulated"></param>
        /// <returns></returns>
        public static void RemoveSystemPropertiesForSimulatedDeviceInfo(dynamic device)
        {
            // Our simulated devices share the structure code with the rest of the system,
            // so we need to explicitly handle this case; since this is only an issue when
            // the code is shared in this way, this special case is kept separate from the
            // rest of the initialization code which would be present in a non-simulated system
            (device as JObject).Remove(DeviceModelConstants.SYSTEM_PROPERTIES);
        }
    }
}
