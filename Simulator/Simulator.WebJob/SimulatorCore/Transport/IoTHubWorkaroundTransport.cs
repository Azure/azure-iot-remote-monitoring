using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    enum DeviceClientState
    {
        Up,
        Down
    }

    class IoTHubWorkaroundTransport : ITransport
    {
        private readonly ILogger _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IDevice _device;
        private readonly string _connectionString;
        private readonly Client.TransportType _transportType;
        private DeviceClient _deviceClient;

        private Task _sendLoop;
        private CancellationTokenSource _cts;

        private Dictionary<string, MethodCallback> _savedMethodHandlers = new Dictionary<string, MethodCallback>();
        private DesiredPropertyUpdateCallback _savedDesiredpropertyHandler = null;
        private LinkedList<TwinCollection> _reports = new LinkedList<TwinCollection>();
        private List<Client.Message> _messages = new List<Client.Message>();

        public IoTHubWorkaroundTransport(ILogger logger, IConfigurationProvider configurationProvider, IDevice device)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
            _device = device;

            _connectionString = Client.IotHubConnectionStringBuilder.Create(
                _device.HostName,
                new DeviceAuthenticationWithRegistrySymmetricKey(
                    _device.DeviceID,
                    _device.PrimaryAuthKey)).ToString();

            var websiteHostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            _transportType = websiteHostName == null ? Client.TransportType.Mqtt : Client.TransportType.Mqtt_WebSocket_Only;
        }

        public async Task OpenAsync()
        {
            if (string.IsNullOrWhiteSpace(_device.DeviceID))
            {
                throw new ArgumentException("DeviceID value cannot be missing, null, or whitespace");
            }

            _deviceClient = await CreateDeviceClient();

            _cts = new CancellationTokenSource();
            _sendLoop = SendLoop(_cts.Token);
        }

        private async Task<DeviceClient> CreateDeviceClient()
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(_connectionString, _transportType);
            await deviceClient.OpenAsync();

            foreach (var handler in _savedMethodHandlers)
            {
                await deviceClient.SetMethodHandlerAsync(handler.Key, handler.Value, null);
            }

            if (_savedDesiredpropertyHandler != null)
            {
                await deviceClient.SetDesiredPropertyUpdateCallback(_savedDesiredpropertyHandler, null);
            }

            return deviceClient;
        }

        public async Task CloseAsync()
        {
            if (_sendLoop != null)
            {
                _cts.Cancel();

                await _sendLoop;
                _sendLoop = null;

                _cts.Dispose();
                _cts = null;
            }

            await _deviceClient.CloseAsync();
            StateCollection<DeviceClientState>.Remove(_device.DeviceID);
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
            string objectType = this.GetObjectType(eventData);
            var objectTypePrefix = _configurationProvider.GetConfigurationSettingValue("ObjectTypePrefix");
            if (!string.IsNullOrWhiteSpace(objectType) && !string.IsNullOrEmpty(objectTypePrefix))
            {
                eventData.ObjectType = objectTypePrefix + objectType;
            }

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventData));
            var message = new Client.Message(bytes);
            message.Properties["EventId"] = eventId.ToString();

            lock (_messages)
            {
                _messages.Add(message);
            }

            await Task.FromResult(0);
        }

        private string GetObjectType(dynamic eventData)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException("eventData");
            }

            var propertyInfo = eventData.GetType().GetProperty("ObjectType");
            if (propertyInfo == null)
            {
                return string.Empty;
            }

            var value = propertyInfo.GetValue(eventData, null);
            return value == null ? string.Empty : value.ToString();
        }

        public async Task SendEventBatchAsync(IEnumerable<Client.Message> messages)
        {
            lock (_messages)
            {
                _messages.AddRange(messages);
            }

            await Task.FromResult(0);
        }

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

        public async Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties)
        {
            lock (_reports)
            {
                _reports.AddLast(reportedProperties);
            }

            await Task.FromResult(0);
        }

        #region Initialize methods
        // Reminder: To avoid potential null reference exception, methods in this region MUST NOT be called after any reports/message sent to IoTHub
        public async Task<Twin> GetTwinAsync()
        {
            return await _deviceClient.GetTwinAsync();
        }

        public async Task SetMethodHandlerAsync(string methodName, MethodCallback callback)
        {
            await _deviceClient.SetMethodHandlerAsync(methodName, callback, null);

            if (!_savedMethodHandlers.ContainsKey(methodName))
            {
                _savedMethodHandlers.Add(methodName, callback);
            }
        }

        public void SetDesiredPropertyUpdateCallback(DesiredPropertyUpdateCallback callback)
        {
            _deviceClient.SetDesiredPropertyUpdateCallback(callback, null);
            _savedDesiredpropertyHandler = callback;
        }
        #endregion

        private async Task SendLoop(CancellationToken ct)
        {
            bool isDeviceClientAvailable = true;
            StateCollection<DeviceClientState>.Set(_device.DeviceID, DeviceClientState.Up);

            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Re-create device client if it is not available (due to any exception)
                if (!isDeviceClientAvailable)
                {
                    try
                    {
                        _deviceClient?.Dispose();
                        _deviceClient = null;   // Avoid duplicated disposing the old device client, if the new one failed to be created

                        _deviceClient = await CreateDeviceClient();

                        _logger.LogInfo($"Transport opened for device {_device.DeviceID} with type {_transportType}");
                        isDeviceClientAvailable = true;
                        StateCollection<DeviceClientState>.Set(_device.DeviceID, DeviceClientState.Up);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception raised while trying to open transport for device {_device.DeviceID}: {ex}");
                        continue;
                    }
                }

                if (_reports.Any())
                {
                    try
                    {
                        await _deviceClient.UpdateReportedPropertiesAsync(_reports.First.Value);

                        lock (_reports)
                        {
                            _reports.RemoveFirst();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception raised while device {_device.DeviceID} trying to update reported properties: {ex}");
                        isDeviceClientAvailable = false;
                        StateCollection<DeviceClientState>.Set(_device.DeviceID, DeviceClientState.Down);
                        continue;
                    }
                }

                if (_messages.Any())
                {
                    // Messages failed to be sent will be dropped
                    var sendingMessages = new List<Client.Message>();
                    lock (_messages)
                    {
                        sendingMessages.AddRange(_messages);
                        _messages.Clear();
                    }

                    try
                    {
                        await _deviceClient.SendEventBatchAsync(sendingMessages);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception raised while device {_device.DeviceID} trying to send events: {ex}");
                        isDeviceClientAvailable = false;
                        StateCollection<DeviceClientState>.Set(_device.DeviceID, DeviceClientState.Down);
                        continue;
                    }
                }
            }
        }

        #region IDispose
        private bool _disposed = false;

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

        ~IoTHubWorkaroundTransport()
        {
            Dispose(false);
        }
        #endregion
    }
}
