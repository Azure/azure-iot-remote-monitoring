using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using D = Dynamitey;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class DeviceLogic : IDeviceLogic
    {
        private readonly IIotHubRepository _iotHubRepository;
        private readonly IDeviceRegistryCrudRepository _deviceRegistryCrudRepository;
        private readonly IDeviceRegistryListRepository _deviceRegistryListRepository;
        private readonly IVirtualDeviceStorage _virtualDeviceStorage;
        private readonly IConfigurationProvider _configProvider;
        private readonly ISecurityKeyGenerator _securityKeyGenerator;
        private readonly IDeviceRulesLogic _deviceRulesLogic;

        public DeviceLogic(IIotHubRepository iotHubRepository, IDeviceRegistryCrudRepository deviceRegistryCrudRepository,
            IDeviceRegistryListRepository deviceRegistryListRepository, IVirtualDeviceStorage virtualDeviceStorage,
            ISecurityKeyGenerator securityKeyGenerator, IConfigurationProvider configProvider, IDeviceRulesLogic deviceRulesLogic)
        {
            _iotHubRepository = iotHubRepository;
            _deviceRegistryCrudRepository = deviceRegistryCrudRepository;
            _deviceRegistryListRepository = deviceRegistryListRepository;
            _virtualDeviceStorage = virtualDeviceStorage;
            _securityKeyGenerator = securityKeyGenerator;
            _configProvider = configProvider;
            _deviceRulesLogic = deviceRulesLogic;
        }

        public async Task<DeviceListQueryResult> GetDevices(DeviceListQuery q)
        {
            return await _deviceRegistryListRepository.GetDeviceList(q);
        }

        /// <summary>
        /// Retrieves the device with the provided device id from the device registry
        /// </summary>
        /// <param name="deviceId">ID of the device to retrieve</param>
        /// <returns>Fully populated device from the device registry</returns>
        public async Task<dynamic> GetDeviceAsync(string deviceId)
        {
            return await _deviceRegistryCrudRepository.GetDeviceAsync(deviceId);
        }

        /// <summary>
        /// Adds a device to the Device Identity Store and Device Registry
        /// </summary>
        /// <param name="device">Device to add to the underlying repositories</param>
        /// <returns>Device created along with the device identity store keys</returns>
        public async Task<DeviceWithKeys> AddDeviceAsync(dynamic device)
        {
            // Validation logic throws an exception if it finds a validation error
            await ValidateDevice(device);

            SecurityKeys generatedSecurityKeys = _securityKeyGenerator.CreateRandomKeys();

            dynamic savedDevice = await AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);
            return new DeviceWithKeys(savedDevice, generatedSecurityKeys);
        }

        /// <summary>
        /// Adds the given device and assigned keys to the underlying repositories 
        /// </summary>
        /// <param name="device">Device to add to repositories</param>
        /// <param name="securityKeys">Keys to assign to the device</param>
        /// <returns>Device that was added to the device registry</returns>
        private async Task<dynamic> AddDeviceToRepositoriesAsync(dynamic device, SecurityKeys securityKeys)
        {
            dynamic registryRepositoryDevice = null;
            ExceptionDispatchInfo capturedException = null;

            // if an exception happens at this point pass it up the stack to handle it
            // (Making this call first then the call against the Registry removes potential issues
            // with conflicting rollbacks if the operation happens to still be in progress.)
            await _iotHubRepository.AddDeviceAsync(device, securityKeys);

            try
            {
                registryRepositoryDevice = await _deviceRegistryCrudRepository.AddDeviceAsync(device);
            }
            catch (Exception ex)
            {
                // grab the exception so we can attempt an async removal of the device from the IotHub
                capturedException = ExceptionDispatchInfo.Capture(ex);

            }

            //Create a device in table storage if it is a simulated type of device 
            //and the document was stored correctly without an exception
            bool isSimulatedAsBool = false;
            try
            {
                isSimulatedAsBool = (bool)device.IsSimulatedDevice;
            }
            catch (InvalidCastException ex)
            {
                Trace.TraceError("The IsSimulatedDevice property was in an invalid format. Exception Error Message: {0}", ex.Message);
            }
            if (capturedException == null && isSimulatedAsBool)
            {
                try
                {
                    await _virtualDeviceStorage.AddOrUpdateDeviceAsync(new InitialDeviceConfig()
                    {
                        DeviceId = DeviceSchemaHelper.GetDeviceID(device),
                        HostName = _configProvider.GetConfigurationSettingValue("iotHub.HostName"),
                        Key = securityKeys.PrimaryKey
                    });
                }
                catch (Exception ex)
                {
                    //if we fail adding to table storage for the device simulator just continue
                    Trace.TraceError("Failed to add simulated device : {0}", ex.Message);
                }
            }


            // Since the rollback code runs async and async code cannot run within the catch block it is run here
            if (capturedException != null)
            {
                // This is a lazy attempt to remove the device from the Iot Hub.  If it fails
                // the device will still remain in the Iot Hub.  A more robust rollback may be needed
                // in some scenarios.
                await _iotHubRepository.TryRemoveDeviceAsync(DeviceSchemaHelper.GetDeviceID(device));
                capturedException.Throw();
            }

            return registryRepositoryDevice;
        }

        /// <summary>
        /// Removes a device from the underlying repositories
        /// </summary>
        /// <param name="deviceId">ID of the device to remove</param>
        /// <returns></returns>
        public async Task RemoveDeviceAsync(string deviceId)
        {
            ExceptionDispatchInfo capturedException = null;
            Azure.Devices.Device iotHubDevice = await _iotHubRepository.GetIotHubDeviceAsync(deviceId);

            // if the device isn't already in the IotHub throw an exception and let the caller know
            if (iotHubDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            // Attempt to remove the device from the IotHub.  If this fails an exception will be thrown
            // and the remainder of the code not run, which is by design
            await _iotHubRepository.RemoveDeviceAsync(deviceId);

            try
            {
                await _deviceRegistryCrudRepository.RemoveDeviceAsync(deviceId);
            }
            catch (Exception ex)
            {
                // if there is an exception while attempting to remove the device from the Device Registry
                // capture it so a rollback can be done on the Identity Registry
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }

            if (capturedException == null)
            {
                try
                {
                    await _virtualDeviceStorage.RemoveDeviceAsync(deviceId);
                }
                catch (Exception ex)
                {
                    //if an exception occurs while attempting to remove the
                    //simulated device from table storage do not roll back the changes.
                    Trace.TraceError("Failed to remove simulated device : {0}", ex.Message);
                }

                await _deviceRulesLogic.RemoveAllRulesForDeviceAsync(deviceId);
            }
            else
            {
                // The "rollback" is an attempt to add the device back in to the Identity Registry
                // It is assumed that if an exception has occured in the Device Registry, the device
                // is still in that store and this works to ensure that both repositories have the same
                // devices registered
                // A more robust rollback may be needed in some scenarios.  
                await _iotHubRepository.TryAddDeviceAsync(iotHubDevice);
                capturedException.Throw();
            }
        }

        /// <summary>
        /// Updates the device in the device registry with the exact device provided in this call.
        /// NOTE: The device provided here should represent the entire device that will be 
        /// serialized into the device registry.
        /// </summary>
        /// <param name="device">Device to update in the device registry</param>
        /// <returns>Device that was saved into the device registry</returns>
        public async Task<dynamic> UpdateDeviceAsync(dynamic device)
        {
            return await _deviceRegistryCrudRepository.UpdateDeviceAsync(device);
        }

        /// <summary>
        /// Used by the event processor to update the initial data for the device
        /// without deleting the CommandHistory and the original created date
        /// This assumes the device controls and has full knowledge of its metadata except for:
        /// - CreatedTime
        /// - CommandHistory
        /// </summary>
        /// <param name="device">Device information to save to the backend Device Registry</param>
        /// <returns>Combined device that was saved to registry</returns>
        public async Task<dynamic> UpdateDeviceFromDeviceInfoPacketAsync(dynamic device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            // Get original device document
            var connectionDeviceId = DeviceSchemaHelper.GetConnectionDeviceId(device);
            dynamic existingDevice = await GetDeviceAsync(connectionDeviceId);

            // Save the command history, original created date, and system properties (if any) of the existing device
            if (DeviceSchemaHelper.GetDeviceProperties(existingDevice) != null)
            {
                dynamic deviceProperties = DeviceSchemaHelper.GetDeviceProperties(device);
                deviceProperties.CreatedTime = DeviceSchemaHelper.GetCreatedTime(existingDevice);
            }

            device.CommandHistory = existingDevice.CommandHistory;

            // Copy the existing system properties, or initialize them if they do not exist
            if (existingDevice.SystemProperties != null)
            {
                device.SystemProperties = existingDevice.SystemProperties;
            }
            else
            {
                DeviceSchemaHelper.InitializeSystemProperties(device, null);
            }

            // Merge device back to existing so we don't drop missing data
            if (existingDevice is JObject)
            {
                existingDevice.Merge(device);
            }

            // If there is Telemetry or Command objects from device, replace instead of merge
            if (device.Telemetry != null)
            {
                existingDevice.Telemetry = device.Telemetry;
            }
            if (device.Commands != null)
            {
                existingDevice.Commands = device.Commands;
            }


            return await _deviceRegistryCrudRepository.UpdateDeviceAsync(existingDevice);
        }

        /// <summary>
        /// Retrieves the IoT Hub keys for the given device
        /// </summary>
        /// <param name="deviceId">ID of the device to retrieve</param>
        /// <returns>Primary and Secondary keys from the IoT Hub</returns>
        public async Task<SecurityKeys> GetIoTHubKeysAsync(string deviceId)
        {
            return await _iotHubRepository.GetDeviceKeysAsync(deviceId);
        }

        /// <summary>
        /// Updates the enabled status of the device in the IoT Hub
        /// NOTE: Disabling a device will mean that it can no longer make calls into the IoT Hub
        /// </summary>
        /// <param name="deviceId">ID of the device to update</param>
        /// <param name="isEnabled">True to enable, False to disable device</param>
        /// <returns></returns>
        public async Task<dynamic> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            dynamic registryRepositoryDevice = null;
            ExceptionDispatchInfo capturedException = null;

            // if an exception happens at this point pass it up the stack to handle it
            await _iotHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);

            try
            {
                registryRepositoryDevice =
                    await _deviceRegistryCrudRepository.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);
            }
            catch (Exception ex)
            {
                // grab the exception so we can attempt an async removal of the device from the IotHub
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }

            // Since the rollback code runs async and async code cannot run within the catch block it is run here
            if (capturedException != null)
            {
                // This is a lazy attempt to revert the enabled status of the device in the IotHub. 
                // If it fails the device status will still remain the same in the IotHub.  
                // A more robust rollback may be needed in some scenarios.
                await _iotHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, !isEnabled);
                capturedException.Throw();
            }

            return registryRepositoryDevice;
        }

        /// <summary>
        /// Send a command to a device based on the provided device id
        /// </summary>
        /// <param name="deviceId">The Device's ID</param>
        /// <param name="commandName">The name of the command</param>
        /// <param name="parameters">The parameters to send</param>
        /// <returns></returns>
        public async Task SendCommandAsync(string deviceId, string commandName, dynamic parameters)
        {
            dynamic device = await GetDeviceAsync(deviceId);

            if (device == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await SendCommandAsyncWithDevice(device, commandName, parameters);
        }

        /// <summary>
        /// Sends a command to the provided device and updates the command history of the device
        /// </summary>
        /// <param name="device">Device to send the command to</param>
        /// <param name="commandName">Name of the command to send</param>
        /// <param name="parameters">Parameters to send with the command</param>
        /// <returns></returns>
        private async Task<dynamic> SendCommandAsyncWithDevice(dynamic device, string commandName, dynamic parameters)
        {
            string deviceId;

            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            bool canDevicePerformCommand = CommandSchemaHelper.CanDevicePerformCommand(device, commandName);

            deviceId = DeviceSchemaHelper.GetDeviceID(device);

            if (!canDevicePerformCommand)
            {
                throw new UnsupportedCommandException(deviceId, commandName);
            }

            dynamic command = CommandHistorySchemaHelper.BuildNewCommandHistoryItem(commandName);
            CommandHistorySchemaHelper.AddParameterCollectionToCommandHistoryItem(command, parameters);

            CommandHistorySchemaHelper.AddCommandToHistory(device, command);

            await _iotHubRepository.SendCommand(deviceId, command);
            await _deviceRegistryCrudRepository.UpdateDeviceAsync(device);

            return command;
        }

        /// <summary>
        /// Modified a Device using a list of 
        /// <see cref="DevicePropertyValueModel" />.
        /// </summary>
        /// <param name="device">
        /// The Device to modify.
        /// </param>
        /// <param name="devicePropertyValueModels">
        /// The list of <see cref="DevicePropertyValueModel" />s for modifying 
        /// <paramref name="device" />.
        /// </param>
        public void ApplyDevicePropertyValueModels(
            dynamic device,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            dynamic deviceProperties;
            IDynamicMetaObjectProvider dynamicMetaObjectProvider;
            ICustomTypeDescriptor typeDescriptor;

            if (object.ReferenceEquals(device, null))
            {
                throw new ArgumentNullException("device");
            }

            if (devicePropertyValueModels == null)
            {
                throw new ArgumentNullException("devicePropertyValueModels");
            }

            deviceProperties = DeviceSchemaHelper.GetDeviceProperties(device);
            if (object.ReferenceEquals(deviceProperties, null))
            {
                throw new ArgumentException("device.DeviceProperties is a null reference.", "device");
            }

            if ((dynamicMetaObjectProvider = deviceProperties as IDynamicMetaObjectProvider) != null)
            {
                ApplyPropertyValueModels(dynamicMetaObjectProvider, devicePropertyValueModels);
            }
            else if ((typeDescriptor = deviceProperties as ICustomTypeDescriptor) != null)
            {
                ApplyPropertyValueModels(typeDescriptor, devicePropertyValueModels);
            }
            else
            {
                ApplyPropertyValueModels((object)deviceProperties, devicePropertyValueModels);
            }
        }

        /// <summary>
        /// Gets <see cref="DevicePropertyValueModel" /> for an edited Device's 
        /// properties.
        /// </summary>
        /// <param name="device">
        /// The edited Device.
        /// </param>
        /// <returns>
        /// <see cref="DevicePropertyValueModel" />s, representing 
        /// <paramref name="device" />'s properties.
        /// </returns>
        public IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(
            dynamic device)
        {
            dynamic deviceProperties;
            IDynamicMetaObjectProvider dynamicMetaObjectProvider;
            string hostNameValue;
            IEnumerable<DevicePropertyValueModel> propValModels;
            ICustomTypeDescriptor typeDescriptor;

            if (object.ReferenceEquals(device, null))
            {
                throw new ArgumentNullException("device");
            }

            deviceProperties = DeviceSchemaHelper.GetDeviceProperties(device);
            if (object.ReferenceEquals(deviceProperties, null))
            {
                throw new ArgumentException("device.DeviceProperties is a null reference.", "device");
            }

            if ((dynamicMetaObjectProvider = deviceProperties as IDynamicMetaObjectProvider) != null)
            {
                propValModels = ExtractPropertyValueModels(dynamicMetaObjectProvider);
            }
            else if ((typeDescriptor = deviceProperties as ICustomTypeDescriptor) != null)
            {
                propValModels = ExtractPropertyValueModels(typeDescriptor);
            }
            else
            {
                propValModels = ExtractPropertyValueModels((object)deviceProperties);
            }

            hostNameValue = _configProvider.GetConfigurationSettingValue("iotHub.HostName");

            if (!string.IsNullOrEmpty(hostNameValue))
            {
                propValModels = propValModels.Concat(
                        new DevicePropertyValueModel[]
                        {
                            new DevicePropertyValueModel()
                            {
                                DisplayOrder = 0,
                                IsEditable = false,
                                IsIncludedWithUnregisteredDevices = true,
                                Name = "HostName",
                                PropertyType = Models.PropertyType.String,
                                Value = hostNameValue
                            }
                        });
            }

            return propValModels;
        }

        private static void ApplyPropertyValueModels(
            object deviceProperties,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            object[] args;
            TypeConverter converter;
            Type devicePropertiesType;
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            Dictionary<string, PropertyInfo> propIndex;
            PropertyInfo propInfo;
            DevicePropertyMetadata propMetadata;
            MethodInfo setter;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            Debug.Assert(devicePropertyValueModels != null, "devicePropertyValueModels is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            devicePropertiesType = deviceProperties.GetType();
            propIndex = devicePropertiesType.GetProperties().ToDictionary(t => t.Name);

            args = new object[1];
            foreach (DevicePropertyValueModel propVal in devicePropertyValueModels)
            {
                if ((propVal == null) ||
                    string.IsNullOrEmpty(propVal.Name))
                {
                    continue;
                }

                // Pass through properties that don't have a specified 
                // configuration.
                if (devicePropertyIndex.TryGetValue(propVal.Name, out propMetadata) && !propMetadata.IsEditable)
                {
                    continue;
                }

                if (!propIndex.TryGetValue(propVal.Name, out propInfo) ||
                    ((setter = propInfo.GetSetMethod()) == null) ||
                    ((converter = TypeDescriptor.GetConverter(propInfo.PropertyType)) == null))
                {
                    continue;
                }

                try
                {
                    args[0] = converter.ConvertFromString(propVal.Value);
                }
                catch (NotSupportedException ex)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Unable to assign value, \"{0},\" to Device property, {1}.",
                            propVal.Value,
                            propInfo.Name),
                        ex);
                }

                setter.Invoke(deviceProperties, args);
            }
        }

        private static void ApplyPropertyValueModels(
            ICustomTypeDescriptor deviceProperties,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            Dictionary<string, PropertyDescriptor> propIndex;
            PropertyDescriptor propDesc;
            DevicePropertyMetadata propMetadata;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            Debug.Assert(devicePropertyValueModels != null, "devicePropertyValueModels is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            propIndex = new Dictionary<string, PropertyDescriptor>();
            foreach (PropertyDescriptor pd in deviceProperties.GetProperties())
            {
                propIndex[pd.Name] = pd;
            }

            foreach (DevicePropertyValueModel propVal in devicePropertyValueModels)
            {
                if ((propVal == null) || string.IsNullOrEmpty(propVal.Name))
                {
                    continue;
                }

                // Pass through properties that don't have a specified 
                // configuration.
                if (devicePropertyIndex.TryGetValue(propVal.Name, out propMetadata) && !propMetadata.IsEditable)
                {
                    continue;
                }

                if (!propIndex.TryGetValue(propVal.Name, out propDesc) || propDesc.IsReadOnly)
                {
                    continue;
                }

                propDesc.SetValue(deviceProperties, propVal.Value);
            }
        }

        private static void ApplyPropertyValueModels(
            IDynamicMetaObjectProvider deviceProperties,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            HashSet<string> dynamicProperties;
            DevicePropertyMetadata propMetadata;

            Debug.Assert(
                deviceProperties != null,
                "deviceProperties is a null reference.");

            Debug.Assert(
                devicePropertyValueModels != null,
                "devicePropertyValueModels is a null reference.");

            devicePropertyIndex =
                GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            dynamicProperties =
                new HashSet<string>(
                    D.Dynamic.GetMemberNames(deviceProperties, true));

            foreach (DevicePropertyValueModel propVal in devicePropertyValueModels)
            {
                if ((propVal == null) ||
                    string.IsNullOrEmpty(propVal.Name))
                {
                    continue;
                }

                if (!dynamicProperties.Contains(propVal.Name))
                {
                    continue;
                }

                // Pass through properties that don't have a specified 
                // configuration.
                if (devicePropertyIndex.TryGetValue(propVal.Name, out propMetadata) && !propMetadata.IsEditable)
                {
                    continue;
                }

                D.Dynamic.InvokeSet(
                    deviceProperties,
                    propVal.Name,
                    propVal.Value);
            }
        }


        private static IEnumerable<DevicePropertyValueModel> ExtractPropertyValueModels(
            ICustomTypeDescriptor deviceProperties)
        {
            DevicePropertyValueModel currentData;
            object currentValue;
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            int editableOrdering;
            bool isDisplayedRegistered;
            bool isDisplayedUnregistered;
            bool isEditable;
            int nonediableOrdering;
            DevicePropertyMetadata propertyMetadata;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            // For now, display r/o properties first.
            editableOrdering = 1;
            nonediableOrdering = int.MinValue;

            foreach (PropertyDescriptor prop in deviceProperties.GetProperties())
            {
                if (devicePropertyIndex.TryGetValue(prop.Name, out propertyMetadata))
                {
                    isDisplayedRegistered = propertyMetadata.IsDisplayedForRegisteredDevices;
                    isDisplayedUnregistered = propertyMetadata.IsDisplayedForUnregisteredDevices;
                    isEditable = propertyMetadata.IsEditable;

                }
                else
                {
                    isDisplayedRegistered = isEditable = true;
                    isDisplayedUnregistered = false;
                }

                if (!isDisplayedRegistered && !isDisplayedUnregistered)
                {
                    continue;
                }

                // Mark R/O properties as not-ediable.
                if (prop.IsReadOnly)
                {
                    isEditable = false;
                }

                currentData = new DevicePropertyValueModel()
                {
                    Name = prop.Name,
                    PropertyType = propertyMetadata.PropertyType
                };

                if (isEditable)
                {
                    currentData.IsEditable = true;
                    currentData.DisplayOrder = editableOrdering++;
                }
                else
                {
                    currentData.IsEditable = false;
                    currentData.DisplayOrder = nonediableOrdering++;
                }

                currentData.IsIncludedWithUnregisteredDevices = isDisplayedUnregistered;

                currentValue = prop.GetValue(deviceProperties);
                if (currentValue == null)
                {
                    currentData.Value = string.Empty;
                }
                else
                {
                    currentData.Value = string.Format(CultureInfo.InvariantCulture, "{0}", currentValue);
                }

                yield return currentData;
            }
        }

        private static IEnumerable<DevicePropertyValueModel> ExtractPropertyValueModels(
            object deviceProperties)
        {
            DevicePropertyValueModel currentData;
            object currentValue;
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            Type devicePropertiesType;
            bool isDisplayedRegistered;
            bool isDisplayedUnregistered;
            bool isEditable;
            int editableOrdering;
            MethodInfo getMethod;
            int nonediableOrdering;
            DevicePropertyMetadata propertyMetadata;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            // For now, display r/o properties first.
            editableOrdering = 1;
            nonediableOrdering = int.MinValue;

            devicePropertiesType = deviceProperties.GetType();
            foreach (PropertyInfo prop in devicePropertiesType.GetProperties())
            {
                if (devicePropertyIndex.TryGetValue(
                    prop.Name,
                    out propertyMetadata))
                {
                    isDisplayedRegistered = propertyMetadata.IsDisplayedForRegisteredDevices;
                    isDisplayedUnregistered = propertyMetadata.IsDisplayedForUnregisteredDevices;
                    isEditable = propertyMetadata.IsEditable;
                }
                else
                {
                    isDisplayedRegistered = isEditable = true;
                    isDisplayedUnregistered = false;
                }

                if (!isDisplayedRegistered && !isDisplayedUnregistered)
                {
                    continue;
                }

                if ((getMethod = prop.GetGetMethod()) == null)
                {
                    continue;
                }

                // Mark R/O properties as not-ediable.
                if (!prop.CanWrite)
                {
                    isEditable = false;
                }

                currentData = new DevicePropertyValueModel()
                {
                    Name = prop.Name,
                    PropertyType = propertyMetadata.PropertyType
                };

                if (isEditable)
                {
                    currentData.IsEditable = true;
                    currentData.DisplayOrder = editableOrdering++;
                }
                else
                {
                    currentData.IsEditable = false;
                    currentData.DisplayOrder = nonediableOrdering++;
                }

                currentData.IsIncludedWithUnregisteredDevices = isDisplayedUnregistered;

                currentValue = getMethod.Invoke(deviceProperties, ReflectionHelper.EmptyArray);
                if (currentValue == null)
                {
                    currentData.Value = string.Empty;
                }
                else
                {
                    currentData.Value = string.Format(CultureInfo.InvariantCulture, "{0}", currentValue);
                }

                yield return currentData;
            }
        }

        private static IEnumerable<DevicePropertyValueModel> ExtractPropertyValueModels(
            IDynamicMetaObjectProvider deviceProperties)
        {
            DevicePropertyValueModel currentData;
            object currentValue;
            Dictionary<string, DevicePropertyMetadata> devicePropertyIndex;
            int editableOrdering;
            bool isDisplayedRegistered;
            bool isDisplayedUnregistered;
            bool isEditable;
            int nonediableOrdering;
            DevicePropertyMetadata propertyMetadata;
            PropertyType propertyType;

            Debug.Assert(deviceProperties != null, "deviceProperties is a null reference.");

            devicePropertyIndex = GetDevicePropertyConfiguration().ToDictionary(t => t.Name);

            // For now, display r/o properties first.
            editableOrdering = 1;
            nonediableOrdering = int.MinValue;

            foreach (string propertyName in D.Dynamic.GetMemberNames(deviceProperties, true))
            {
                if (devicePropertyIndex.TryGetValue(propertyName, out propertyMetadata))
                {
                    isDisplayedRegistered = propertyMetadata.IsDisplayedForRegisteredDevices;
                    isDisplayedUnregistered = propertyMetadata.IsDisplayedForUnregisteredDevices;
                    isEditable = propertyMetadata.IsEditable;

                    propertyType = propertyMetadata.PropertyType;
                }
                else
                {
                    isDisplayedRegistered = isEditable = true;
                    isDisplayedUnregistered = false;

                    propertyType = PropertyType.String;
                }

                if (!isDisplayedRegistered && !isDisplayedUnregistered)
                {
                    continue;
                }

                currentData = new DevicePropertyValueModel()
                {
                    Name = propertyName,
                    PropertyType = propertyType
                };

                if (isEditable)
                {
                    currentData.IsEditable = true;
                    currentData.DisplayOrder = editableOrdering++;
                }
                else
                {
                    currentData.IsEditable = false;
                    currentData.DisplayOrder = nonediableOrdering++;
                }

                currentData.IsIncludedWithUnregisteredDevices =
                    isDisplayedUnregistered;

                currentValue = D.Dynamic.InvokeGet(deviceProperties, propertyName);
                if (currentValue == null)
                {
                    currentData.Value = string.Empty;
                }
                else
                {
                    currentData.Value = string.Format(CultureInfo.InvariantCulture, "{0}", currentValue);
                }

                yield return currentData;
            }
        }

        private static IEnumerable<DevicePropertyMetadata> GetDevicePropertyConfiguration()
        {
            // Only return metadata for fields that aren't handled in the 
            // standard way.

            // TODO: Drive this from data?
            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = true,
                IsEditable = false,
                Name = "DeviceID"
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = true,
                IsEditable = false,
                Name = "CreatedTime",
                PropertyType = PropertyType.DateTime
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "DeviceState",
                PropertyType = PropertyType.Status
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = false,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "HostName"
            };

            // Do not show a Device field, HubEnabledState.  One will be added 
            // programatically from settings.
            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "HubEnabledState",
                PropertyType = PropertyType.Status
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = false,
                Name = "UpdatedTime",
                PropertyType = PropertyType.DateTime
            };
        }

        private async Task ValidateDevice(dynamic device)
        {
            List<string> validationErrors = new List<string>();

            if (ValidateDeviceId(device, validationErrors))
            {
                await CheckIfDeviceExists(device, validationErrors);
            }

            if (validationErrors.Count > 0)
            {
                var validationException =
                    new ValidationException(DeviceSchemaHelper.GetDeviceProperties(device) != null ? DeviceSchemaHelper.GetDeviceID(device) : null);

                foreach (string error in validationErrors)
                {
                    validationException.Errors.Add(error);
                }

                throw validationException;
            }
        }

        private async Task CheckIfDeviceExists(dynamic device, List<string> validationErrors)
        {
            // check if device exists
            if (await GetDeviceAsync(DeviceSchemaHelper.GetDeviceID(device)) != null)
            {
                validationErrors.Add(Strings.ValidationDeviceExists);
            }
        }

        private bool ValidateDeviceId(dynamic device, List<string> validationErrors)
        {
            if (DeviceSchemaHelper.GetDeviceProperties(device) == null || string.IsNullOrWhiteSpace(DeviceSchemaHelper.GetDeviceID(device)))
            {
                validationErrors.Add(Strings.ValidationDeviceIdMissing);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates N devices with random data and properties for testing
        /// NOTE: Adds the devices to both the device registry and device identity repository
        /// </summary>
        /// <param name="deviceCount">Number of devices to generate</param>
        /// <returns></returns>
        /// <remarks>TEMPORARY DEVICE GENERATION CODE FOR TESTING PURPOSES!</remarks>
        public async Task GenerateNDevices(int deviceCount)
        {
            Random randomNumber = new Random();

            for (int i = 0; i < deviceCount; i++)
            {
                SecurityKeys generatedSecurityKeys = _securityKeyGenerator.CreateRandomKeys();
                dynamic device = SampleDeviceFactory.GetSampleDevice(randomNumber, generatedSecurityKeys);
                await AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);
            }
        }

        public async Task<List<string>> BootstrapDefaultDevices()
        {
            List<string> sampleIds = SampleDeviceFactory.GetDefaultDeviceNames();
            foreach (string id in sampleIds)
            {
                dynamic device = DeviceSchemaHelper.BuildDeviceStructure(id, true, null);
                SecurityKeys generatedSecurityKeys = _securityKeyGenerator.CreateRandomKeys();
                await AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);
            }
            return sampleIds;
        }

        public DeviceListLocationsModel ExtractLocationsData(List<dynamic> devices)
        {
            var result = new DeviceListLocationsModel();

            // Initialize defaults to opposite extremes to ensure mins and maxes are beyond any actual values
            double minLat = double.MaxValue;
            double maxLat = double.MinValue;
            double minLong = double.MaxValue;
            double maxLong = double.MinValue;

            var locationList = new List<DeviceLocationModel>();
            foreach (dynamic device in devices)
            {
                dynamic props = DeviceSchemaHelper.GetDeviceProperties(device);
                if (props.Longitude == null || props.Latitude == null)
                {
                    continue;
                }

                double latitude;
                double longitude;

                try
                {
                    latitude = DeviceSchemaHelper.GetDeviceProperties(device).Latitude;
                    longitude = DeviceSchemaHelper.GetDeviceProperties(device).Longitude;
                }
                catch (FormatException)
                {
                    continue;
                }

                var location = new DeviceLocationModel()
                {
                    DeviceId = DeviceSchemaHelper.GetDeviceID(device),
                    Longitude = longitude,
                    Latitude = latitude
                };
                locationList.Add(location);

                if (longitude < minLong)
                {
                    minLong = longitude;
                }
                if (longitude > maxLong)
                {
                    maxLong = longitude;
                }
                if (latitude < minLat)
                {
                    minLat = latitude;
                }
                if (latitude > maxLat)
                {
                    maxLat = latitude;
                }
            }

            if (locationList.Count == 0)
            {
                // reinitialize bounds to center on Seattle area if no devices
                minLat = 47.6;
                maxLat = 47.6;
                minLong = -122.3;
                maxLong = -122.3;
            }

            double offset = 0.05;

            result.DeviceLocationList = locationList;
            result.MinimumLatitude = minLat - offset;
            result.MaximumLatitude = maxLat + offset;
            result.MinimumLongitude = minLong - offset;
            result.MaximumLongitude = maxLong + offset;

            return result;
        }

        /// <summary>
        /// Converts the telemetry schema data in a device into a strongly-typed model
        /// </summary>
        /// <param name="device">Device with telemetry schema</param>
        /// <returns>Converted telemetry schema, or null if there is none</returns>
        public IList<DeviceTelemetryFieldModel> ExtractTelemetry(dynamic device)
        {
            // Get Telemetry Fields
            if (device.Telemetry != null)
            {
                var deviceTelemetryFields = new List<DeviceTelemetryFieldModel>();

                foreach (JObject field in device.Telemetry)
                {
                    // Default displayName to null if not present
                    string displayName = field.GetValue("DisplayName", StringComparison.OrdinalIgnoreCase) != null ?
                        field.GetValue("DisplayName", StringComparison.OrdinalIgnoreCase).ToString() :
                        null;

                    deviceTelemetryFields.Add(new DeviceTelemetryFieldModel
                    {
                        DisplayName = displayName,
                        Name = field.GetValue("Name", StringComparison.OrdinalIgnoreCase).ToString(),
                        Type = field.GetValue("Type", StringComparison.OrdinalIgnoreCase).ToString()
                    });
                }

                return deviceTelemetryFields;
            }
            else
            {
                return null;
            }
        }
    }
}
