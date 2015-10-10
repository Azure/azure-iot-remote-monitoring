using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Wraps calls to the IoT hub identity store.
    /// IDisposable is implemented in order to close out the connection to the IoT Hub when this object is no longer in use
    /// </summary>
    public class IotHubRepository : IIotHubRepository, IDisposable
    {
        readonly string _iotHubConnectionString;
        readonly RegistryManager _deviceManager;
        bool _disposed = false;

        public IotHubRepository(IConfigurationProvider configProvider)
        {
            // Temporary code to bypass https cert validation till DNS on IotHub is configured
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            _iotHubConnectionString = configProvider.GetConfigurationSettingValue("iotHub.ConnectionString");
            _deviceManager = RegistryManager.CreateFromConnectionString(_iotHubConnectionString);
        }

        /// <summary>
        /// Adds the provided device to the IoT hub with the provided security keys
        /// </summary>
        /// <param name="device"></param>
        /// <param name="securityKeys"></param>
        /// <returns></returns>
        public async Task<dynamic> AddDeviceAsync(dynamic device, SecurityKeys securityKeys)
        {

            Azure.Devices.Device iotHubDevice = new Azure.Devices.Device(DeviceSchemaHelper.GetDeviceID(device));

            var authentication = new AuthenticationMechanism
            {
                SymmetricKey = new SymmetricKey
                {
                    PrimaryKey = securityKeys.PrimaryKey,
                    SecondaryKey = securityKeys.SecondaryKey
                }
            };

            iotHubDevice.Authentication = authentication;

            await AzureRetryHelper.OperationWithBasicRetryAsync<Azure.Devices.Device>(async () =>
                await _deviceManager.AddDeviceAsync(iotHubDevice));

            return device;
        }

        /// <summary>
        /// Attempts to add the device as a new device and swallows all exceptions
        /// </summary>
        /// <param name="oldIotHubDevice">The IoT Hub Device to add back into the IoT Hub</param>
        /// <returns>true if the device was added successfully, false if there was a problem adding the device</returns>
        public async Task<bool> TryAddDeviceAsync(Azure.Devices.Device oldIotHubDevice)
        {
            try
            {
                // the device needs to be added as a new device as the one that was saved 
                // has an eTag value that cannot be provided when registering a new device
                var newIotHubDevice = new Azure.Devices.Device(oldIotHubDevice.Id)
                {
                    Authentication = oldIotHubDevice.Authentication,
                    Status = oldIotHubDevice.Status
                };

                await AzureRetryHelper.OperationWithBasicRetryAsync<Azure.Devices.Device>(async () =>
                    await _deviceManager.AddDeviceAsync(newIotHubDevice));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<Azure.Devices.Device> GetIotHubDeviceAsync(string deviceId)
        {
            return await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await _deviceManager.GetDeviceAsync(deviceId));
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await _deviceManager.RemoveDeviceAsync(deviceId));
        }

        /// <summary>
        /// Attempts to remove the device from the IoT Hub and eats any exceptions that are thrown during the 
        /// delete process.
        /// </summary>
        /// <param name="deviceId">ID of the device to remove</param>
        /// <returns>true if the remove was successful and false if the remove was not successful</returns>
        public async Task<bool> TryRemoveDeviceAsync(string deviceId)
        {
            try
            {
                await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                    await _deviceManager.RemoveDeviceAsync(deviceId));
            }
            catch (Exception)
            {
                // swallow any exceptions that happen during this remove
                return false;
            }

            return true;
        }

        public async Task UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            Azure.Devices.Device iotHubDevice =
                await AzureRetryHelper.OperationWithBasicRetryAsync<Azure.Devices.Device>(async () =>
                    await _deviceManager.GetDeviceAsync(deviceId));

            iotHubDevice.Status = isEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;
            
            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await _deviceManager.UpdateDeviceAsync(iotHubDevice));
        }

        /// <summary>
        /// Sends a fire and forget command to the device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task SendCommand(string deviceId, dynamic command)
        {
            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(_iotHubConnectionString);
            
            byte[] commandAsBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
            var notificationMessage = new Message(commandAsBytes);

            notificationMessage.Ack = DeliveryAcknowledgement.Full;
            notificationMessage.MessageId = command.MessageId;

            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await serviceClient.SendAsync(deviceId, notificationMessage));

            await serviceClient.CloseAsync();
        }

        public async Task<SecurityKeys> GetDeviceKeysAsync(string deviceId)
        {
            Azure.Devices.Device iotHubDevice = await _deviceManager.GetDeviceAsync(deviceId);

            if (iotHubDevice == null)
            {
                // this is the case if the device does not exist on the hub
                return null;
            }
            else
            {
                return new SecurityKeys(iotHubDevice.Authentication.SymmetricKey.PrimaryKey, iotHubDevice.Authentication.SymmetricKey.SecondaryKey);
            }
        }

        /// <summary>
        /// Implement the IDisposable interface in order to close the device manager
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            
            if (disposing)
            {
                if (_deviceManager != null)
                {
                    _deviceManager.CloseAsync().Wait();
                }
            }

            _disposed = true;
        }

        ~IotHubRepository()
        {
            Dispose(false);
        }
    }
}
