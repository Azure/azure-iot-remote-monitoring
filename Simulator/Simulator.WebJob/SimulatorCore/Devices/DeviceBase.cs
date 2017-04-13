using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices
{
    /// <summary>
    /// Simulates a single IoT device that sends and recieves data from a transport
    /// </summary>
    public class DeviceBase : IDevice
    {
        public const string DeviceStatePropertyName = "Device.DeviceState";
        public const string StartupTimePropertyName = "Device.StartupTime";
        public const string FirmwareVersionPropertyName = "System.FirmwareVersion";
        public const string ConfigurationVersionPropertyName = "System.ConfigurationVersion";
        public const string TemperatureMeanValuePropertyName = "Config.TemperatureMeanValue";
        public const string TelemetryIntervalPropertyName = "Config.TelemetryInterval";
        public const string LastDesiredPropertyChangePropertyName = "Device.LastDesiredPropertyChange";
        public const string LastFactoryResetTimePropertyName = "Device.LastFactoryResetTime";
        public const string LastRebootTimePropertyName = "Device.LastRebootTime";

        // pointer to the currently executing event group
        private int _currentEventGroup = 0;

        protected readonly ILogger Logger;
        protected readonly ITransportFactory TransportFactory;
        protected readonly ITelemetryFactory TelemetryFactory;
        protected readonly IConfigurationProvider ConfigProvider;
        protected ITransport Transport;
        protected CommandProcessor RootCommandProcessor;

        public string DeviceID
        {
            get { return DeviceProperties.DeviceID; }
            set { DeviceProperties.DeviceID = value; }
        }

        public string HostName { get; set; }
        public string PrimaryAuthKey { get; set; }

        private DeviceProperties _deviceProperties;
        public DeviceProperties DeviceProperties
        {
            get { return _deviceProperties; }
            set { _deviceProperties = value; }
        }

        public List<Command> Commands { get; set; }

        public List<Common.Models.Telemetry> Telemetry { get; set; }

        public List<ITelemetry> TelemetryEvents { get; private set; }
        public bool RepeatEventListForever { get; set; }

        protected object _telemetryController;

        private Dictionary<string, string> _propertyMapping = new Dictionary<string, string>
        {
            { "CreatedTime", "Device.CreatedTime" },
            { "UpdatedTime", "Device.UpdatedTime" },
            { "DeviceState", DeviceStatePropertyName },
            { "Manufacturer", "System.Manufacturer" },
            { "ModelNumber", "System.ModelNumber" },
            { "SerialNumber", "System.SerialNumber" },
            { "FirmwareVersion", FirmwareVersionPropertyName },
            { "AvailablePowerSources", "System.AvailablePowerSources" },
            { "PowerSourceVoltage", "System.PowerSourceVoltage" },
            { "BatteryLevel", "System.BatteryLevel" },
            { "MemoryFree", "System.MemoryFree" },
            { "HostName", "System.HostName" },
            { "Platform", "System.Platform" },
            { "Processor", "System.Processor" },
            { "InstalledRAM", "System.InstalledRAM" },
            { "Latitude", "Device.Location.Latitude" },
            { "Longitude", "Device.Location.Longitude" }
        };

        protected Dictionary<string, Func<object, Task>> _desiredPropertyUpdateHandlers = new Dictionary<string, Func<object, Task>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger">Logger where this device will log information to</param>
        /// <param name="transport">Transport where the device will send and receive data to/from</param>
        /// <param name="config">Config to start this device with</param>
        public DeviceBase(ILogger logger, ITransportFactory transportFactory, ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider)
        {
            ConfigProvider = configurationProvider;
            Logger = logger;
            TransportFactory = transportFactory;
            TelemetryFactory = telemetryFactory;
            TelemetryEvents = new List<ITelemetry>();
        }

        public void Init(InitialDeviceConfig config)
        {
            InitDeviceInfo(config);

            Transport = TransportFactory.CreateTransport(this);
            _telemetryController = TelemetryFactory.PopulateDeviceWithTelemetryEvents(this);

            InitCommandProcessors();
        }

        protected virtual void InitDeviceInfo(InitialDeviceConfig config)
        {
            DeviceModel initialDevice = SampleDeviceFactory.GetSampleSimulatedDevice(config.DeviceId, config.Key);
            DeviceProperties = initialDevice.DeviceProperties;
            Commands = initialDevice.Commands ?? new List<Command>();
            Telemetry = initialDevice.Telemetry ?? new List<Common.Models.Telemetry>();
            HostName = config.HostName;
            PrimaryAuthKey = config.Key;
        }

        /// <summary>
        /// Builds up a set of commands supported by this device
        /// </summary>
        protected virtual void InitCommandProcessors()
        {
            var pingDeviceProcessor = new PingDeviceProcessor(this);

            RootCommandProcessor = pingDeviceProcessor;
        }

        public async virtual Task SendDeviceInfo()
        {
            Logger.LogInfo("Sending Device Info for device {0}...", DeviceID);
            await Transport.SendEventAsync(GetDeviceInfo());
        }

        /// <summary>
        /// Generates a DeviceInfo packet for a simulated device to send over the wire
        /// </summary>
        /// <returns></returns>
        public virtual DeviceModel GetDeviceInfo()
        {
            DeviceModel device = DeviceCreatorHelper.BuildDeviceStructure(DeviceID, true, null);
            device.DeviceProperties = this.DeviceProperties;
            device.Commands = this.Commands?.Where(c => c.DeliveryType == DeliveryType.Message).ToList() ?? new List<Command>();
            device.Telemetry = this.Telemetry ?? new List<Common.Models.Telemetry>();
            device.Version = SampleDeviceFactory.VERSION_1_0;
            device.ObjectType = SampleDeviceFactory.OBJECT_TYPE_DEVICE_INFO;

            // Remove the system properties from a device, to better emulate the behavior of real devices when sending device info messages.
            device.SystemProperties = null;

            return device;
        }

        /// <summary>
        /// Starts the send event loop and runs the receive loop in the background
        /// to listen for commands that are sent to the device
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken token)
        {
            try
            {
                await InitializeAsync();

                var loopTasks = new List<Task>
                {
                    StartReceiveLoopAsync(token),
                    StartSendLoopAsync(token)
                };

                // Wait both the send and receive loops
                await Task.WhenAll(loopTasks.ToArray());
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception raise while starting device {DeviceID}: {ex}");
            }
            finally
            {
                // once the code makes it here the token has been canceled
                await Transport.CloseAsync();
            }
        }

        private async Task InitializeAsync()
        {
            await Transport.OpenAsync();
            await SetupCallbacksAsync();

            var twin = await Transport.GetTwinAsync();
            await UpdateReportedPropertiesAsync(twin.Properties.Reported);
            await OnDesiredPropertyUpdate(twin.Properties.Desired, null);
        }

        /// <summary>
        /// Iterates through the list of IEventGroups and fires off the events in a given event group before moving to the next.
        /// If RepeatEventListForever is true the device will continue to loop through each event group, if false
        /// once a single pass is made through all event groups the device will stop sending events
        /// </summary>
        /// <param name="token">Cancellation token to cancel out of the loop</param>
        /// <returns></returns>
        private async Task StartSendLoopAsync(CancellationToken token)
        {
            try
            {
                Logger.LogInfo("Booting device {0}...", DeviceID);

                var authMethod = new Client.DeviceAuthenticationWithRegistrySymmetricKey(DeviceID, PrimaryAuthKey);
                var deviceConnectionString = Client.IotHubConnectionStringBuilder.Create(HostName, authMethod).ToString();

                do
                {
                    _currentEventGroup = 0;

                    Logger.LogInfo("Starting events list for device {0}...", DeviceID);

                    while (_currentEventGroup < TelemetryEvents.Count && !token.IsCancellationRequested)
                    {
                        Logger.LogInfo("Device {0} starting IEventGroup {1}...", DeviceID, _currentEventGroup);

                        var eventGroup = TelemetryEvents[_currentEventGroup];

                        await eventGroup.SendEventsAsync(token, async (object eventData) =>
                        {
                            await Transport.SendEventAsync(eventData);
                        });

                        _currentEventGroup++;
                    }

                    Logger.LogInfo("Device {0} finished sending all events in list...", DeviceID);

                } while (RepeatEventListForever && !token.IsCancellationRequested);

                Logger.LogWarning("Device {0} sent all events and is shutting down send loop. (Set RepeatEventListForever = true on the device to loop forever.)", DeviceID);

            }
            catch (TaskCanceledException)
            {
                //do nothing if the task was cancelled
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception raised while starting device send loop {DeviceID}: {ex.Message}");
            }

            if (token.IsCancellationRequested)
            {
                Logger.LogInfo("********** Processing Device {0} has been cancelled - StartSendLoopAsync Ending. **********", DeviceID);
            }
        }

        /// <summary>
        /// Starts the loop that listens for events/commands from the IoT Hub to be sent to this device
        /// </summary>
        /// <param name="token">Cancellation token that can stop the loop if needed</param>
        private async Task StartReceiveLoopAsync(CancellationToken token)
        {
            DeserializableCommand command;
            Exception exception;
            CommandProcessingResult processingResult;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    command = null;
                    exception = null;

                    // Pause before running through the receive loop
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                    Logger.LogInfo("Device {0} checking for commands...", DeviceID);

                    try
                    {
                        // Retrieve the message from the IoT Hub
                        command = await Transport.ReceiveAsync();

                        if (command == null)
                        {
                            continue;
                        }

                        processingResult = await RootCommandProcessor.HandleCommandAsync(command);

                        switch (processingResult)
                        {
                            case CommandProcessingResult.CannotComplete:
                                await Transport.SignalRejectedCommand(command);
                                break;

                            case CommandProcessingResult.RetryLater:
                                await Transport.SignalAbandonedCommand(command);
                                break;

                            case CommandProcessingResult.Success:
                                await Transport.SignalCompletedCommand(command);
                                break;
                        }

                        Logger.LogInfo(
                            "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Result: {4}{0}",
                            Console.Out.NewLine,
                            DeviceID,
                            command.CommandName,
                            command.LockToken,
                            processingResult);
                    }
                    catch (IotHubException ex)
                    {
                        exception = ex;

                        Logger.LogInfo(
                            "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Error Type: {4}{0}Exception: {5}{0}",
                            Console.Out.NewLine,
                            DeviceID,
                            command?.CommandName,
                            command?.LockToken,
                            ex.IsTransient ? "Transient Error" : "Non-transient Error",
                            ex.ToString());
                    }
                    catch (Exception ex)
                    {
                        exception = ex;

                        Logger.LogInfo(
                            "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Exception: {4}{0}",
                            Console.Out.NewLine,
                            DeviceID,
                            command?.CommandName,
                            command?.LockToken,
                            ex.ToString());
                    }

                    if (command != null && exception != null)
                    {
                        await Transport.SignalAbandonedCommand(command);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                //do nothing if the task was cancelled
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception raised while starting device receive loop {DeviceID}: {ex}");
            }

            Logger.LogInfo("********** Processing Device {0} has been cancelled - StartReceiveLoopAsync Ending. **********", DeviceID);
        }

        protected async Task UpdateReportedPropertiesAsync(TwinCollection reported, bool regenerate = false)
        {
            var patch = new TwinCollection();
            CrossSyncProperties(patch, reported, regenerate);
            SupportedMethodsHelper.CreateSupportedMethodReport(patch, Commands, reported);
            AddConfigs(patch);

            // Update ReportedProperties to IoT Hub
            await Transport.UpdateReportedPropertiesAsync(patch);
        }

        /// <summary>
        /// Cross synchonize DeviceProperties and ReportedProperties
        /// </summary>
        /// <returns></returns>
        protected void CrossSyncProperties(TwinCollection patch, TwinCollection reported, bool regenerate)
        {
            var devicePropertiesType = DeviceProperties.GetType();
            var reportedPairs = reported.AsEnumerableFlatten().ToDictionary(pair => pair.Key, pair => pair.Value);

            if (!regenerate)
            {
                // Overwrite regenerated DeviceProperties by current ReportedProperties
                foreach (var pair in reportedPairs)
                {
                    string devicePropertyName = _propertyMapping.SingleOrDefault(p => p.Value == pair.Key).Key;
                    if (string.IsNullOrWhiteSpace(devicePropertyName))
                    {
                        continue;
                    }

                    try
                    {
                        DeviceProperties.SetProperty(devicePropertyName, pair.Value.Value);
                    }
                    catch
                    {
                        // Ignore any failure while overwriting the DeviceProperties
                    }
                }
            }

            // Add missing DeviceProperties to ReportedProperties
            foreach (var property in devicePropertiesType.GetProperties())
            {
                string reportedName;
                if (!_propertyMapping.TryGetValue(property.Name, out reportedName))
                {
                    continue;
                }

                var value = property.GetValue(DeviceProperties);
                if (regenerate || value != null && !reportedPairs.ContainsKey(reportedName))
                {
                    patch.Set(reportedName, value);
                }
            }
        }

        protected void AddConfigs(TwinCollection patch)
        {
            var telemetryWithInterval = _telemetryController as ITelemetryWithInterval;
            if (telemetryWithInterval != null)
            {
                patch.Set(TelemetryIntervalPropertyName, telemetryWithInterval.TelemetryIntervalInSeconds);
            }

            var telemetryWithTemperatureMeanValue = _telemetryController as ITelemetryWithTemperatureMeanValue;
            if (telemetryWithTemperatureMeanValue != null)
            {
                patch.Set(TemperatureMeanValuePropertyName, telemetryWithTemperatureMeanValue.TemperatureMeanValue);
            }

            patch.Set(StartupTimePropertyName, DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
        }

        private async Task SetupCallbacksAsync()
        {
            foreach (var method in Commands.Where(c => c.DeliveryType == DeliveryType.Method))
            {
                try
                {
                    var handler = GetType().GetMethod(FormattableString.Invariant($"On{method.Name}")).CreateDelegate(typeof(MethodCallback), this) as MethodCallback;

                    await Transport.SetMethodHandlerAsync(method.Name, handler);
                }
                catch (Exception ex)
                {
                    Logger.LogError(FormattableString.Invariant($"Exception raised while adding callback for method {method.Name} on device {DeviceID}: {ex.Message}"));
                }
            }

            Transport.SetDesiredPropertyUpdateCallback(OnDesiredPropertyUpdate);
        }

        protected async Task SetReportedPropertyAsync(string name, dynamic value)
        {
            var collection = new TwinCollection();
            TwinCollectionExtension.Set(collection, name, value);
            await Transport.UpdateReportedPropertiesAsync(collection);
        }

        protected async Task SetReportedPropertyAsync(Dictionary<string, dynamic> pairs)
        {
            var collection = new TwinCollection();
            foreach (var pair in pairs)
            {
                TwinCollectionExtension.Set(collection, pair.Key, pair.Value);
            }
            await Transport.UpdateReportedPropertiesAsync(collection);
        }

        public async Task OnDesiredPropertyUpdate(TwinCollection desiredProperties, object userContext)
        {
            await SetReportedPropertyAsync(LastDesiredPropertyChangePropertyName, desiredProperties.ToJson());
            Logger.LogInfo($"{DeviceID} received desired property update: {desiredProperties.ToJson()}");

            foreach (var pair in desiredProperties.AsEnumerableFlatten())
            {
                Func<object, Task> handler;
                if (_desiredPropertyUpdateHandlers.TryGetValue(pair.Key, out handler))
                {
                    try
                    {
                        await handler(pair.Value.Value.Value);
                        Logger.LogInfo($"Successfully called desired property update handler {handler.Method.Name} on {DeviceID}");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Exception raised while processing desired property {pair.Key} change on device {DeviceID}: {ex.Message}");
                    }
                }
                else
                {
                    Logger.LogWarning($"Cannot find desired property update handler for {pair.Key} on {DeviceID}");
                }
            }
        }
    }
}
