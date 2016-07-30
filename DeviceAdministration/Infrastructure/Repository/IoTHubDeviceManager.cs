using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    ///     Wraps calls to the IoT hub identity store.
    ///     IDisposable is implemented in order to close out the connection to the IoT Hub when this object is no longer in use
    /// </summary>
    public class IoTHubDeviceManager : IIoTHubDeviceManager, IDisposable
    {
        private readonly RegistryManager _deviceManager;
        private readonly ServiceClient serviceClient;
        private bool _disposed;

        public IoTHubDeviceManager(IConfigurationProvider configProvider)
        {
            // Temporary code to bypass https cert validation till DNS on IotHub is configured
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            var _iotHubConnectionString = configProvider.GetConfigurationSettingValue("iotHub.ConnectionString");
            this._deviceManager = RegistryManager.CreateFromConnectionString(_iotHubConnectionString);
            this.serviceClient = ServiceClient.CreateFromConnectionString(_iotHubConnectionString);
        }

        public async Task<Device> AddDeviceAsync(Device device)
        {
            return await this._deviceManager.AddDeviceAsync(device);
        }

        public async Task<Device> GetDeviceAsync(string deviceId)
        {
            return await this._deviceManager.GetDeviceAsync(deviceId);
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            await this._deviceManager.RemoveDeviceAsync(deviceId);
        }

        public async Task<Device> UpdateDeviceAsync(Device device)
        {
            return await this._deviceManager.UpdateDeviceAsync(device);
        }

        public async Task SendAsync(string deviceId, Message message)
        {
            await this.serviceClient.SendAsync(deviceId, message);
        }

        public async Task CloseAsyncDevice()
        {
            await this.serviceClient.CloseAsync();
        }

        public async Task CloseAsyncService()
        {
            await this._deviceManager.CloseAsync();
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
                    this._deviceManager.CloseAsync().Wait();
                }
            }

            this._disposed = true;
        }

        ~IoTHubDeviceManager()
        {
            this.Dispose(false);
        }
    }
}
