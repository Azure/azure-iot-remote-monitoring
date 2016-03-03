using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device
{
    /// <summary>
    /// Simulates a single IoT device that sends and recieves data from a transport
    /// </summary>
    public abstract class DeviceBase : IDevice, IDisposable
    {
        private const int REPORT_FREQUENCY_IN_SECONDS = 5;

        public virtual string Version => "1.0";

        protected readonly ILogger Logger;
        protected readonly ITransportFactory TransportFactory;
        protected readonly IConfigurationProvider ConfigProvider;
        protected ITransport Transport;
        protected CommandProcessor RootCommandProcessor;

        private ManualResetEventSlim processing = new ManualResetEventSlim(true);

        public string DeviceID
        {
            get { return DeviceProperties.DeviceID; }
            set { DeviceProperties.DeviceID = value; }
        }

        public string HostName { get; set; }
        public string PrimaryAuthKey { get; set; }

        private dynamic _deviceProperties;
        public dynamic DeviceProperties
        {
            get {  return _deviceProperties; }
            set { _deviceProperties = value; }
        }

        public dynamic Commands { get; set; }

        public virtual bool IsSimulated => true;

        private readonly List<ITelemetry> telemetries;

        public IReadOnlyList<ITelemetry> Telemetries { get { return telemetries; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger">Logger where this device will log information to</param>
        /// <param name="transport">Transport where the device will send and receive data to/from</param>
        /// <param name="config">Config to start this device with</param>
        public DeviceBase(ILogger logger, ITransportFactory transportFactory, IConfigurationProvider configurationProvider)
        {
            ConfigProvider = configurationProvider;
            Logger = logger;
            TransportFactory = transportFactory;
            telemetries = new List<ITelemetry>();
        }

        protected async Task RegisterTelemetry(Func<Task<ITelemetry>> telemetryFactory)
        {
            telemetries.Add(await telemetryFactory());
        }

        public virtual async Task Initialize(InitialDeviceConfig config)
        {
            await Task.Yield();

            InitDeviceInfo(config);

            Transport = TransportFactory.CreateTransport(this);

            InitCommandProcessors();
        }



        protected virtual void InitDeviceInfo(InitialDeviceConfig config)
        {
            dynamic device = DeviceSchemaHelper.BuildDeviceStructure(config.DeviceId, IsSimulated, null);
            DeviceProperties = DeviceSchemaHelper.GetDeviceProperties(device);
            Commands = CommandSchemaHelper.GetSupportedCommands(device);
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
        public virtual dynamic GetDeviceInfo()
        {
            dynamic device = DeviceSchemaHelper.BuildDeviceStructure(DeviceID, IsSimulated, null);
            device.DeviceProperties = DeviceSchemaHelper.GetDeviceProperties(this);
            device.Commands = CommandSchemaHelper.GetSupportedCommands(this);
            device.Version = Version;
            device.ObjectType = "DeviceInfo";

            // Remove the system properties from a device, to better emulate the behavior of real devices when sending device info messages.
            DeviceSchemaHelper.RemoveSystemPropertiesForSimulatedDeviceInfo(device);

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
                Transport.Open();

                var loopTasks = new []
                {
                    StartReceiveLoopAsync(token), 
                    StartSendLoopAsync(token)
                };

                // Wait both the send and receive loops
                await Task.WhenAll(loopTasks);

                // once the code makes it here the token has been canceled
                await Transport.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError("Unexpected Exception starting device: {0}", ex.ToString());
            }
        }

        public async Task PauseAsync()
        {
            if (disposed)
                throw new ObjectDisposedException(this.DeviceID);
            await Task.Yield();
            processing.Reset();
        }

        public async Task ResumeAsync()
        {
            if (disposed)
                throw new ObjectDisposedException(this.DeviceID);
            await Task.Yield();
            processing.Set();
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

                do
                {
                    if (!disposed)
                    {
                        processing.Wait(token);

                        var telemetry = 0;

                        Logger.LogInfo("Starting events list for device {0}...", DeviceID);

                        while (telemetry < Telemetries.Count && !token.IsCancellationRequested)
                        {
                            Logger.LogInfo("Device {0} starting IEventGroup {1}...", DeviceID, telemetry);

                            var eventGroup = Telemetries[telemetry];

                            await eventGroup.SendEventsAsync(token, async (object eventData) =>
                            {
                                await Transport.SendEventAsync(eventData);
                            });

                            telemetry++;
                        }

                        Logger.LogInfo("Device {0} finished sending all events in list...", DeviceID);

                        await Task.Delay(TimeSpan.FromSeconds(REPORT_FREQUENCY_IN_SECONDS), token);
                    }

                } while (!token.IsCancellationRequested && !disposed);

                Logger.LogWarning("Device {0} event loop finished", DeviceID);

            }
            catch (TaskCanceledException) 
            {
                //do nothing if the task was cancelled
            }
            catch (Exception ex)
            {
                Logger.LogError("Unexpected Exception starting device send loop: {0}", ex.ToString());
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
                while (!token.IsCancellationRequested && !disposed)
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

                        processingResult = 
                        await RootCommandProcessor.HandleCommandAsync(command);

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
                            Environment.NewLine,
                            DeviceID,
                            command.CommandName,
                            command.LockToken,
                            processingResult);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;

                        Logger.LogInfo(
                            "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Exception: {4}{0}",
                            Environment.NewLine,
                            DeviceID,
                            command.CommandName,
                            command.LockToken,
                            ex.ToString());
                    }

                    if ((command != null) &&
                        (exception != null))
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
                Logger.LogError("Unexpected Exception starting device receive loop: {0}", ex.ToString());
            }

            Logger.LogInfo("********** Processing Device {0} has been cancelled - StartReceiveLoopAsync Ending. **********", DeviceID);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                disposed = true;
                if(disposing)
                {
                    if(processing != null)
                    {
                        processing.Dispose();
                        processing = null;
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DeviceBase()
        {
            Dispose(false);
        }
    }
}
