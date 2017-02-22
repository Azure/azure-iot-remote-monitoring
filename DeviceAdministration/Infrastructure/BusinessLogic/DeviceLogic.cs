using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

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
        private readonly INameCacheLogic _nameCacheLogic;
        private readonly IDeviceListFilterRepository _deviceListFilterRepository;

        public DeviceLogic(IIotHubRepository iotHubRepository, IDeviceRegistryCrudRepository deviceRegistryCrudRepository,
            IDeviceRegistryListRepository deviceRegistryListRepository, IVirtualDeviceStorage virtualDeviceStorage,
            ISecurityKeyGenerator securityKeyGenerator, IConfigurationProvider configProvider, IDeviceRulesLogic deviceRulesLogic,
            INameCacheLogic nameCacheLogic, IDeviceListFilterRepository deviceListFilterRepository)
        {
            _iotHubRepository = iotHubRepository;
            _deviceRegistryCrudRepository = deviceRegistryCrudRepository;
            _deviceRegistryListRepository = deviceRegistryListRepository;
            _virtualDeviceStorage = virtualDeviceStorage;
            _securityKeyGenerator = securityKeyGenerator;
            _configProvider = configProvider;
            _deviceRulesLogic = deviceRulesLogic;
            _nameCacheLogic = nameCacheLogic;
            _deviceListFilterRepository = deviceListFilterRepository;
        }

        public async Task<DeviceListFilterResult> GetDevices(DeviceListFilter filter)
        {
            await _deviceListFilterRepository.TouchFilterAsync(filter.Id);
            var task = _deviceListFilterRepository.SaveSuggestClausesAsync(filter.Clauses);

            var devices = await _deviceRegistryListRepository.GetDeviceList(filter);
            UpdateNameCache(devices.Results.Select(r => r.Twin));
            return devices;
        }

        private void UpdateNameCache(IEnumerable<Shared.Twin> twins)
        {
            // Reminder: None of the tasks updating the namecache need to be waited for completed

            var tags = twins.GetNameList(twin => twin.Tags);
            var tagTask = _nameCacheLogic.AddShortNamesAsync(NameCacheEntityType.Tag, tags);

            var desiredProperties = twins.GetNameList(twin => twin.Properties.Desired);
            var desiredPropertyTask = _nameCacheLogic.AddShortNamesAsync(NameCacheEntityType.DesiredProperty, desiredProperties);

            var reportedProperties = twins.GetNameList(twin => twin.Properties.Reported)
                .Where(name => !SupportedMethodsHelper.IsSupportedMethodProperty(name));
            var reportedPropertyTask = _nameCacheLogic.AddShortNamesAsync(NameCacheEntityType.ReportedProperty, reportedProperties);

            // No need to update Method here, since it will not change during device running
        }

        public async Task<DeviceModel> GetDeviceAsync(string deviceId)
        {
            return await _deviceRegistryCrudRepository.GetDeviceAsync(deviceId);
        }

        /// <summary>
        /// Adds a device to the Device Identity Store and Device Registry
        /// </summary>
        /// <param name="device">Device to add to the underlying repositories</param>
        /// <returns>Device created along with the device identity store keys</returns>
        public async Task<DeviceWithKeys> AddDeviceAsync(DeviceModel device)
        {
            // Validation logic throws an exception if it finds a validation error
            await this.ValidateDevice(device);

            SecurityKeys generatedSecurityKeys = this._securityKeyGenerator.CreateRandomKeys();

            DeviceModel savedDevice = await this.AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);
            return new DeviceWithKeys(savedDevice, generatedSecurityKeys);
        }

        /// <summary>
        /// Adds the given device and assigned keys to the underlying repositories
        /// </summary>
        /// <param name="device">Device to add to repositories</param>
        /// <param name="securityKeys">Keys to assign to the device</param>
        /// <returns>Device that was added to the device registry</returns>
        private async Task<DeviceModel> AddDeviceToRepositoriesAsync(DeviceModel device, SecurityKeys securityKeys)
        {
            DeviceModel registryRepositoryDevice = null;
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
                        DeviceId = device.DeviceProperties.DeviceID,
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
                await _iotHubRepository.TryRemoveDeviceAsync(device.DeviceProperties.DeviceID);
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
        public async Task<DeviceModel> UpdateDeviceAsync(DeviceModel device)
        {
            return await _deviceRegistryCrudRepository.UpdateDeviceAsync(device);
        }

        public async Task<DeviceModel> UpdateDeviceFromDeviceInfoPacketAsync(DeviceModel device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            // Get original device document
            DeviceModel existingDevice = await this.GetDeviceAsync(device.IoTHub.ConnectionDeviceId);

            SupportedMethodsHelper.AddSupportedMethodsFromReportedProperty(device, existingDevice.Twin);

            // Save the command history, original created date, and system properties (if any) of the existing device
            if (existingDevice.DeviceProperties != null)
            {
                DeviceProperties deviceProperties = device.DeviceProperties;
                deviceProperties.CreatedTime = existingDevice.DeviceProperties.CreatedTime;
                existingDevice.DeviceProperties = deviceProperties;
            }

            device.CommandHistory = existingDevice.CommandHistory;

            // Copy the existing system properties, or initialize them if they do not exist
            if (existingDevice.SystemProperties != null)
            {
                device.SystemProperties = existingDevice.SystemProperties;
            }
            else
            {
                device.SystemProperties = null;
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
        /// Send a command to a device based on the provided device id
        /// </summary>
        /// <param name="deviceId">The Device's ID</param>
        /// <param name="commandName">The name of the command</param>
        /// <param name="parameters">The parameters to send</param>
        /// <returns></returns>
        public async Task SendCommandAsync(string deviceId, string commandName, DeliveryType deliveryType, dynamic parameters)
        {
            DeviceModel device = await this.GetDeviceAsync(deviceId);

            if (device == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await SendCommandAsyncWithDevice(device, commandName, deliveryType, parameters);
        }

        /// <summary>
        /// Sends a command to the provided device and updates the command history of the device
        /// </summary>
        /// <param name="device">Device to send the command to</param>
        /// <param name="commandName">Name of the command to send</param>
        /// <param name="parameters">Parameters to send with the command</param>
        /// <returns></returns>
        private async Task<CommandHistory> SendCommandAsyncWithDevice(DeviceModel device, string commandName, DeliveryType deliveryType, dynamic parameters)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var deviceId = device.DeviceProperties.DeviceID;
            if (device.Commands.FirstOrDefault(x => x.Name == commandName) == null)
            {
                throw new UnsupportedCommandException(deviceId, commandName);
            }

            var commandHistory = new CommandHistory(commandName, deliveryType, parameters);

            if (device.CommandHistory == null)
            {
                device.CommandHistory = new List<CommandHistory>();
            }

            device.CommandHistory.Add(commandHistory);

            await _iotHubRepository.SendCommand(deviceId, commandHistory);
            await _deviceRegistryCrudRepository.UpdateDeviceAsync(device);

            return commandHistory;
        }

        public async Task<DeviceModel> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {

            DeviceModel repositoryDevice = null;
            ExceptionDispatchInfo capturedException = null;

            // if an exception happens at this point pass it up the stack to handle it
            await _iotHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);

            try
            {
                repositoryDevice = await _deviceRegistryCrudRepository.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);
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

            if (repositoryDevice == null || !repositoryDevice.IsSimulatedDevice) return repositoryDevice;
            return await this.AddOrRemoveSimulatedDevice(repositoryDevice, isEnabled);
        }

        private async Task<DeviceModel> AddOrRemoveSimulatedDevice(DeviceModel repositoryDevice, bool isEnabled)
        {
            var deviceId = repositoryDevice.DeviceProperties.DeviceID;
            if (isEnabled)
            {
                try
                {
                    var securityKeys = await this.GetIoTHubKeysAsync(deviceId);
                    await _virtualDeviceStorage.AddOrUpdateDeviceAsync(new InitialDeviceConfig()
                    {
                        DeviceId = deviceId,
                        HostName = _configProvider.GetConfigurationSettingValue("iotHub.HostName"),
                        Key = securityKeys.PrimaryKey
                    });
                }
                catch (Exception ex)
                {
                    //if we fail adding to table storage for the device simulator just continue
                    Trace.TraceError("Failed to add enabled device to simulated device storage. Device telemetry is expected not to be sent. : {0}", ex.Message);
                }
            }
            else
            {
                try
                {
                    await _virtualDeviceStorage.RemoveDeviceAsync(deviceId);
                }
                catch (Exception ex)
                {
                    //if an exception occurs while attempting to remove the
                    //simulated device from table storage do not roll back the changes.
                    Trace.TraceError("Failed to remove disabled device from simulated device store. Device will keep sending telemetry data. : {0}", ex.Message);
                }
            }

            return repositoryDevice;
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
        public virtual void ApplyDevicePropertyValueModels(
            DeviceModel device,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (devicePropertyValueModels == null)
            {
                throw new ArgumentNullException("devicePropertyValueModels");
            }

            if (device.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
            }
            ApplyPropertyValueModels(device.DeviceProperties, devicePropertyValueModels);
        }

        public virtual IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(
           DeviceModel device)
        {
            DeviceProperties deviceProperties;
            string hostNameValue;
            IEnumerable<DevicePropertyValueModel> propValModels;

            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            deviceProperties = device.DeviceProperties;
            if (deviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
            }

            propValModels = ExtractPropertyValueModels(deviceProperties);
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
            DeviceProperties deviceProperties,
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

        private static IEnumerable<DevicePropertyValueModel> ExtractPropertyValueModels(
            DeviceProperties deviceProperties)
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
            PropertyType propertyType;

            if (deviceProperties == null)
            {
                throw new ArgumentNullException("deviceProperties is a null reference.");
            }

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

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = true,
                Name = "Latitude",
                PropertyType = PropertyType.Real
            };

            yield return new DevicePropertyMetadata()
            {
                IsDisplayedForRegisteredDevices = true,
                IsDisplayedForUnregisteredDevices = false,
                IsEditable = true,
                Name = "Longitude",
                PropertyType = PropertyType.Real
            };
        }

        private async Task ValidateDevice(DeviceModel device)
        {
            List<string> validationErrors = new List<string>();

            if (ValidateDeviceId(device, validationErrors))
            {
                await CheckIfDeviceExists(device, validationErrors);
            }

            if (validationErrors.Count > 0)
            {
                var validationException =
                    new ValidationException(device.DeviceProperties != null ? device.DeviceProperties.DeviceID : null);

                foreach (string error in validationErrors)
                {
                    validationException.Errors.Add(error);
                }

                throw validationException;
            }
        }

        private async Task CheckIfDeviceExists(DeviceModel device, List<string> validationErrors)
        {
            // check if device exists
            if (await this.GetDeviceAsync(device.DeviceProperties.DeviceID) != null)
            {
                validationErrors.Add(Strings.ValidationDeviceExists);
            }
        }

        private bool ValidateDeviceId(DeviceModel device, List<string> validationErrors)
        {
            if (device.DeviceProperties == null || string.IsNullOrWhiteSpace(device.DeviceProperties.DeviceID))
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
                DeviceModel device = SampleDeviceFactory.GetSampleDevice(randomNumber, generatedSecurityKeys);
                await this.AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);
            }
        }

        public async Task<List<string>> BootstrapDefaultDevices()
        {
            List<string> sampleIds = SampleDeviceFactory.GetDefaultDeviceNames();
            foreach (string id in sampleIds)
            {
                DeviceModel device = DeviceCreatorHelper.BuildDeviceStructure(id, true, null);
                SecurityKeys generatedSecurityKeys = _securityKeyGenerator.CreateRandomKeys();
                SampleDeviceFactory.AssignDefaultTags(device);
                SampleDeviceFactory.AssignDefaultDesiredProperties(device);
                await this.AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);
            }
            return sampleIds;
        }

        public DeviceListLocationsModel ExtractLocationsData(List<DeviceModel> devices)
        {
            var result = new DeviceListLocationsModel();

            // Initialize defaults to opposite extremes to ensure mins and maxes are beyond any actual values
            double minLat = double.MaxValue;
            double maxLat = double.MinValue;
            double minLong = double.MaxValue;
            double maxLong = double.MinValue;

            var locationList = new List<DeviceLocationModel>();
            if (devices != null && devices.Count > 0)
            {
                foreach (DeviceModel device in devices)
                {
                    if (device.DeviceProperties == null)
                    {
                        throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
                    }

                    double latitude;
                    double longitude;

                    try
                    {
                        latitude = (double)device.Twin.Properties.Reported.Get("Device.Location.Latitude");
                        longitude = (double)device.Twin.Properties.Reported.Get("Device.Location.Longitude");
                    }
                    catch
                    {
                        continue;
                    }

                    var location = new DeviceLocationModel()
                    {
                        DeviceId = device.DeviceProperties.DeviceID,
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


        public IList<DeviceTelemetryFieldModel> ExtractTelemetry(DeviceModel device)
        {
            // Get Telemetry Fields
            if (device != null && device.Telemetry != null)
            {
                var deviceTelemetryFields = new List<DeviceTelemetryFieldModel>();

                foreach (var field in device.Telemetry)
                {
                    // Default displayName to null if not present
                    string displayName = field.DisplayName != null ?
                        field.DisplayName : null;

                    deviceTelemetryFields.Add(new DeviceTelemetryFieldModel
                    {
                        DisplayName = displayName,
                        Name = field.Name,
                        Type = field.Type
                    });
                }

                return deviceTelemetryFields;
            }
            else
            {
                return null;
            }
        }

        public async Task AddToNameCache(string deviceId)
        {
            var device = await this.GetDeviceAsync(deviceId);
            var twin = device.Twin;

            await _nameCacheLogic.AddNameAsync(nameof(device.Twin.DeviceId));

            await _nameCacheLogic.AddShortNamesAsync(
                NameCacheEntityType.Tag,
                twin.Tags
                    .AsEnumerableFlatten()
                    .Select(pair => pair.Key));

            await _nameCacheLogic.AddShortNamesAsync(
                NameCacheEntityType.DesiredProperty,
                twin.Properties.Desired
                    .AsEnumerableFlatten()
                    .Select(pair => pair.Key));

            await _nameCacheLogic.AddShortNamesAsync(
                NameCacheEntityType.ReportedProperty,
                twin.Properties.Reported
                    .AsEnumerableFlatten()
                    .Select(pair => pair.Key)
                    .Where(name => !SupportedMethodsHelper.IsSupportedMethodProperty(name)));

            foreach (var command in device.Commands.Where(c => c.DeliveryType == DeliveryType.Method))
            {
                await _nameCacheLogic.AddMethodAsync(command);
            }
        }
    }
}
