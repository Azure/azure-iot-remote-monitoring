using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport
{
    /// <summary>
    /// Implementation of ITransport that talks to IoT Hub.
    /// </summary>
    public class IoTHubTransport : ITransport
    {
        private readonly ILogger _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IDevice _device;
        private DeviceClient _deviceClient;
        private bool _disposed = false;

        public IoTHubTransport(ILogger logger, IConfigurationProvider configurationProvider, IDevice device)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
            _device = device;
        }

        public async Task OpenAsync()
        {
            if (string.IsNullOrWhiteSpace(_device.DeviceID))
            {
                throw new ArgumentException("DeviceID value cannot be missing, null, or whitespace");
            }

            var websiteHostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            var transportType = websiteHostName == null ? Client.TransportType.Mqtt : Client.TransportType.Mqtt_WebSocket_Only;
            _deviceClient = DeviceClient.CreateFromConnectionString(GetConnectionString(), transportType);
            await _deviceClient.OpenAsync();

            _logger.LogInfo($"Transport opened for device {_device.DeviceID} with type {transportType}");
        }

        public async Task CloseAsync()
        {
            await _deviceClient.CloseAsync();
        }

        /// <summary>
        /// Builds the IoT Hub connection string
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private string GetConnectionString()
        {
            string key = _device.PrimaryAuthKey;
            string deviceID = _device.DeviceID;
            string hostName = _device.HostName;

            var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceID, key);
            return Client.IotHubConnectionStringBuilder.Create(hostName, authMethod).ToString();
        }

        /// <summary>
        /// Sends an event to the IoT Hub
        /// </summary>
        /// <param name="device"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public async Task SendEventAsync(dynamic eventData)
        {
            var eventId = Guid.NewGuid();
            await SendEventAsync(eventId, eventData);
        }

        /// <summary>
        /// Sends an event to IoT Hub using the provided eventId GUID
        /// </summary>
        /// <param name="device"></param>
        /// <param name="eventId"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public async Task SendEventAsync(Guid eventId, dynamic eventData)
        {
            byte[] bytes;
            string objectType = this.GetObjectType(eventData);
            var objectTypePrefix = _configurationProvider.GetConfigurationSettingValue("ObjectTypePrefix");

            if (!string.IsNullOrWhiteSpace(objectType) && !string.IsNullOrEmpty(objectTypePrefix))
            {
                eventData.ObjectType = objectTypePrefix + objectType;
            }

            // sample code to trace the raw JSON that is being sent
            //string rawJson = JsonConvert.SerializeObject(eventData);
            //Trace.TraceInformation(rawJson);

            bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventData));

            var message = new Client.Message(bytes);
            message.Properties["EventId"] = eventId.ToString();

            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
            {
                try
                {
                    await _deviceClient.SendEventAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"SendEventAsync failed, device: {_device.DeviceID}, exception: {ex.Message}");
                }
            });
        }

        public async Task SendEventBatchAsync(IEnumerable<Client.Message> messages)
        {
            await _deviceClient.SendEventBatchAsync(messages);
        }

        /// <summary>
        /// Retrieves the next message from the IoT Hub
        /// </summary>
        /// <param name="device">The device to retieve the IoT Hub message for</param>
        /// <returns>Returns a DeserializableCommand that wraps the byte array of the message from IoT Hub</returns>
        public async Task<DeserializableCommand> ReceiveAsync()
        {
            Client.Message message = await AzureRetryHelper.OperationWithBasicRetryAsync(
                async () =>
                {
                    try
                    {
                        return await _deviceClient.ReceiveAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"ReceiveAsync failed, device: {_device.DeviceID}, exception: {ex.Message}");
                        return null;
                    }
                });

            if (message != null)
            {
                return new DeserializableCommand(message);
            }

            return null;
        }

        public async Task SignalAbandonedCommand(DeserializableCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            Debug.Assert(
                !string.IsNullOrEmpty(command.LockToken),
                "command.LockToken is a null reference or empty string.");

            await AzureRetryHelper.OperationWithBasicRetryAsync(
                async () =>
                {
                    try
                    {
                        await _deviceClient.AbandonAsync(command.LockToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Abandon Command failed, device: {_device.DeviceID}, exception: {ex.Message}");
                    }
                });
        }

        public async Task SignalCompletedCommand(DeserializableCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            Debug.Assert(
                !string.IsNullOrEmpty(command.LockToken),
                "command.LockToken is a null reference or empty string.");

            await AzureRetryHelper.OperationWithBasicRetryAsync(
                async () =>
                {
                    try
                    {
                        await _deviceClient.CompleteAsync(command.LockToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Complete Command failed, device: {_device.DeviceID}, exception: {ex.Message}");
                    }
                });
        }

        public async Task SignalRejectedCommand(DeserializableCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            Debug.Assert(
                !string.IsNullOrEmpty(command.LockToken),
                "command.LockToken is a null reference or empty string.");

            await AzureRetryHelper.OperationWithBasicRetryAsync(
                async () =>
                {
                    try
                    {
                        await _deviceClient.RejectAsync(command.LockToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Reject Command failed, device: {_device.DeviceID}, exception: {ex.Message}");
                    }
                });
        }

        private string GetObjectType(dynamic eventData)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException("eventData");
            }

            var propertyInfo = eventData.GetType().GetProperty("ObjectType");
            if (propertyInfo == null)
                return "";
            var value = propertyInfo.GetValue(eventData, null);
            return value == null ? "" : value.ToString();
        }

        public async Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties)
        {
            await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
        }

        public async Task<Twin> GetTwinAsync()
        {
            return await _deviceClient.GetTwinAsync();
        }

        public async Task SetMethodHandlerAsync(string methodName, MethodCallback callback)
        {
            await _deviceClient.SetMethodHandlerAsync(methodName, callback, null);
        }

        public void SetDesiredPropertyUpdateCallback(DesiredPropertyUpdateCallback callback)
        {
            _deviceClient.SetDesiredPropertyUpdateCallback(callback, null);
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
                if (_deviceClient != null)
                {
                    _deviceClient.CloseAsync().Wait();
                }
            }

            _disposed = true;
        }

        ~IoTHubTransport()
        {
            Dispose(false);
        }
    }
}
