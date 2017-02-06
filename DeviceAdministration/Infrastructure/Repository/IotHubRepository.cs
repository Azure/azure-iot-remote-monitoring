using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    ///     Wraps calls to the IoT hub identity store.
    ///     IDisposable is implemented in order to close out the connection to the IoT Hub when this object is no longer in use
    /// </summary>
    public class IotHubRepository : IIotHubRepository, IDisposable
    {
        private readonly IIoTHubDeviceManager _deviceManager;
        private bool _disposed;

        public IotHubRepository(IIoTHubDeviceManager deviceManager)
        {
            this._deviceManager = deviceManager;
        }

        /// <summary>
        ///     Adds the provided device to the IoT hub with the provided security keys
        /// </summary>
        /// <param name="device"></param>
        /// <param name="securityKeys"></param>
        /// <returns></returns>
        public async Task<DeviceModel> AddDeviceAsync(DeviceModel device, SecurityKeys securityKeys)
        {
            var iotHubDevice = new Device(device.DeviceProperties.DeviceID)
            {
                Authentication = new AuthenticationMechanism
                {
                    SymmetricKey = new SymmetricKey
                    {
                        PrimaryKey = securityKeys.PrimaryKey,
                        SecondaryKey = securityKeys.SecondaryKey
                    }
                }
            };

            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await this._deviceManager.AddDeviceAsync(iotHubDevice));

            if (device.Twin?.Tags.Count > 0 || device.Twin?.Properties.Desired.Count > 0)
            {
                device.Twin.ETag = "*";
                await this._deviceManager.UpdateTwinAsync(device.DeviceProperties.DeviceID, device.Twin);
            }

            return device;
        }

        /// <summary>
        ///     Attempts to add the device as a new device and swallows all exceptions
        /// </summary>
        /// <param name="oldIotHubDevice">The IoT Hub Device to add back into the IoT Hub</param>
        /// <returns>true if the device was added successfully, false if there was a problem adding the device</returns>
        public async Task<bool> TryAddDeviceAsync(Device oldIotHubDevice)
        {
            try
            {
                // the device needs to be added as a new device as the one that was saved 
                // has an eTag value that cannot be provided when registering a new device
                var newIotHubDevice = new Device(oldIotHubDevice.Id)
                {
                    Authentication = oldIotHubDevice.Authentication,
                    Status = oldIotHubDevice.Status
                };

                await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                    await this._deviceManager.AddDeviceAsync(newIotHubDevice));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<Device> GetIotHubDeviceAsync(string deviceId)
        {
            return await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                                                                       await this._deviceManager.GetDeviceAsync(deviceId));
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                                                                await this._deviceManager.RemoveDeviceAsync(deviceId));
        }

        /// <summary>
        ///     Attempts to remove the device from the IoT Hub and eats any exceptions that are thrown during the
        ///     delete process.
        /// </summary>
        /// <param name="deviceId">ID of the device to remove</param>
        /// <returns>true if the remove was successful and false if the remove was not successful</returns>
        public async Task<bool> TryRemoveDeviceAsync(string deviceId)
        {
            try
            {
                await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                                                                    await this._deviceManager.RemoveDeviceAsync(deviceId));
            }
            catch (Exception)
            {
                // swallow any exceptions that happen during this remove
                return false;
            }

            return true;
        }

        public async Task<Device> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            var iotHubDevice =
                await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                                                                    await this._deviceManager.GetDeviceAsync(deviceId));

            iotHubDevice.Status = isEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            return await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                                                                await this._deviceManager.UpdateDeviceAsync(iotHubDevice));
        }

        public async Task SendCommand(string deviceId, CommandHistory command)
        {
            if (command.DeliveryType == DeliveryType.Message)
            {
                var commandAsBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
                var notificationMessage = new Message(commandAsBytes);

                notificationMessage.Ack = DeliveryAcknowledgement.Full;
                notificationMessage.MessageId = command.MessageId;

                await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                                                                    await this._deviceManager.SendAsync(deviceId, notificationMessage));

                await this._deviceManager.CloseAsyncDevice();
            }
            else
            {
                var method = new CloudToDeviceMethod(command.Name);
                method.SetPayloadJson(JsonConvert.SerializeObject(command.Parameters));

                var result = await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                                                                    await this._deviceManager.InvokeDeviceMethodAsync(deviceId, method));
                command.Result = result.Status.ToString();
                command.ReturnValue = result.GetPayloadAsJson();
                command.UpdatedTime = DateTime.UtcNow;
            }
        }

        public async Task<SecurityKeys> GetDeviceKeysAsync(string deviceId)
        {
            var iotHubDevice = await this._deviceManager.GetDeviceAsync(deviceId);

            if (iotHubDevice == null)
            {
                // this is the case if the device does not exist on the hub
                return null;
            }
            return new SecurityKeys(iotHubDevice.Authentication.SymmetricKey.PrimaryKey, iotHubDevice.Authentication.SymmetricKey.SecondaryKey);
        }

        /// <summary>
        ///     Implement the IDisposable interface in order to close the device manager
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            if (disposing)
            {
                if (this._deviceManager != null)
                {
                    this._deviceManager.CloseAsyncService().Wait();
                }
            }

            this._disposed = true;
        }

        ~IotHubRepository()
        {
            this.Dispose(false);
        }
    }
}
