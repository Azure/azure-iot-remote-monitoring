using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices.DMTasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices
{
    /// <summary>
    /// Implementation of a specific device type that extends the BaseDevice functionality
    /// </summary>
    public class CoolerDevice : DeviceBase
    {
        private Task _deviceManagementTask = null;

        public CoolerDevice(ILogger logger, ITransportFactory transportFactory,
            ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider)
            : base(logger, transportFactory, telemetryFactory, configurationProvider)
        {
            _desiredPropertyUdateHandlers.Add(SetPointTempPropertyName, OnSetPointTempUpdate);
            _desiredPropertyUdateHandlers.Add(TelemetryIntervalPropertyName, OnTelemetryIntervalUpdate);
        }

        /// <summary>
        /// Builds up the set of commands that are supported by this device
        /// </summary>
        protected override void InitCommandProcessors()
        {
            var pingDeviceProcessor = new PingDeviceProcessor(this);
            var startCommandProcessor = new StartCommandProcessor(this);
            var stopCommandProcessor = new StopCommandProcessor(this);
            var diagnosticTelemetryCommandProcessor = new DiagnosticTelemetryCommandProcessor(this);
            var changeSetPointTempCommandProcessor = new ChangeSetPointTempCommandProcessor(this);
            var changeDeviceStateCommmandProcessor = new ChangeDeviceStateCommandProcessor(this);

            pingDeviceProcessor.NextCommandProcessor = startCommandProcessor;
            startCommandProcessor.NextCommandProcessor = stopCommandProcessor;
            stopCommandProcessor.NextCommandProcessor = diagnosticTelemetryCommandProcessor;
            diagnosticTelemetryCommandProcessor.NextCommandProcessor = changeSetPointTempCommandProcessor;
            changeSetPointTempCommandProcessor.NextCommandProcessor = changeDeviceStateCommmandProcessor;

            RootCommandProcessor = pingDeviceProcessor;
        }

        public bool StartTelemetryData()
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            bool lastStatus = remoteMonitorTelemetry.TelemetryActive;
            remoteMonitorTelemetry.TelemetryActive = true;
            Logger.LogInfo("Device {0}: Telemetry has started", DeviceID);

            return lastStatus;
        }

        public bool StopTelemetryData()
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            bool lastStatus = remoteMonitorTelemetry.TelemetryActive;
            remoteMonitorTelemetry.TelemetryActive = false;
            Logger.LogInfo("Device {0}: Telemetry has stopped", DeviceID);

            return lastStatus;
        }

        public void ChangeSetPointTemp(double setPointTemp)
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            remoteMonitorTelemetry.SetPointTemperature = setPointTemp;
            Logger.LogInfo("Device {0} temperature changed to {1}", DeviceID, setPointTemp);
        }

        public async Task ChangeDeviceState(string deviceState)
        {
            // simply update the DeviceState property and send updated device info packet
            DeviceProperties.DeviceState = deviceState;
            await SendDeviceInfo();
            Logger.LogInfo("Device {0} in {1} state", DeviceID, deviceState);
        }

        public void DiagnosticTelemetry(bool active)
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            remoteMonitorTelemetry.ActivateExternalTemperature = active;
            string externalTempActive = active ? "on" : "off";
            Logger.LogInfo("Device {0}: External Temperature: {1}", DeviceID, externalTempActive);
        }

        public async Task<MethodResponse> OnChangeDeviceState(MethodRequest methodRequest, object userContext)
        {
            Logger.LogInfo($"Method {methodRequest.Name} invoked on device {DeviceID}, payload: {methodRequest.DataAsJson}");

            await SetReportedPropertyAsync(DeviceStatePropertyName, methodRequest.DataAsJson);

            return BuildMethodRespose(methodRequest.DataAsJson);
        }

        public async Task<MethodResponse> OnFirmwareUpdate(MethodRequest methodRequest, object userContext)
        {
            if (_deviceManagementTask != null && !_deviceManagementTask.IsCompleted)
            {
                return await Task.FromResult(BuildMethodRespose(new
                {
                    Message = "Device is busy"
                }, 409));
            }

            try
            {
                var operation = new FirmwareUpdate(methodRequest);
                _deviceManagementTask = operation.Run(Transport);

                return await Task.FromResult(BuildMethodRespose(new
                {
                    Message = "FirmwareUpdate accepted",
                    Uri = operation.Uri
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(BuildMethodRespose(new
                {
                    Message = ex.Message
                }, 400));
            }
        }

        public async Task<MethodResponse> OnConfigurationUpdate(MethodRequest methodRequest, object userContext)
        {
            if (_deviceManagementTask != null && !_deviceManagementTask.IsCompleted)
            {
                return await Task.FromResult(BuildMethodRespose(new
                {
                    Message = "Device is busy"
                }, 409));
            }

            try
            {
                var operation = new ConfigurationUpdate(methodRequest);
                _deviceManagementTask = operation.Run(Transport);

                return await Task.FromResult(BuildMethodRespose(new
                {
                    Message = "ConfigurationUpdate accepted",
                    Uri = operation.Uri
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(BuildMethodRespose(new
                {
                    Message = ex.Message
                }, 400));
            }
        }

        public async Task<MethodResponse> OnReboot(MethodRequest methodRequest, object userContext)
        {
            var task = RebootAsync();

            return await Task.FromResult(BuildMethodRespose(new
            {
                Message = "Reboot accepted"
            }));
        }

        private async Task RebootAsync()
        {
            const string LogPath = "Method.Reboot.Log";

            await SetReportedPropertyAsync(LogPath, "Rebooting");

            await Task.Delay(TimeSpan.FromSeconds(10));

            await SetReportedPropertyAsync(new Dictionary<string, dynamic>
            {
                { LogPath, "Rebooted" },
                { StartupTimePropertyName, DateTime.UtcNow.ToString() }
            });
        }

        public async Task<MethodResponse> OnFactoryReset(MethodRequest methodRequest, object userContext)
        {
            var task = FactoryResetAsync();

            return await Task.FromResult(BuildMethodRespose(new
            {
                Message = "FactoryReset accepted"
            }));
        }

        private async Task FactoryResetAsync()
        {
            const string LogPath = "Method.FactoryReset.Log";

            await SetReportedPropertyAsync(LogPath, "Reseting");

            await Task.Delay(TimeSpan.FromSeconds(10));

            await SetReportedPropertyAsync(new Dictionary<string, dynamic>
            {
                { LogPath, "Reset" },
                { StartupTimePropertyName, DateTime.UtcNow.ToString() },
                { FirmwareVersionPropertyName, "1.0" },
                { ConfigurationVersionPropertyName, null },
            });
        }

        public async Task<MethodResponse> OnPingDevice(MethodRequest methodRequest, object userContext)
        {
            return await Task.FromResult(BuildMethodRespose(new
            {
                DeviceUtcTime = DateTime.UtcNow
            }));
        }

        public async Task<MethodResponse> OnStartTelemetry(MethodRequest methodRequest, object userContext)
        {
            bool lastStatus = StartTelemetryData();

            return await Task.FromResult(BuildMethodRespose(new
            {
                LastStatus = lastStatus
            }));
        }

        public async Task<MethodResponse> OnStopTelemetry(MethodRequest methodRequest, object userContext)
        {
            bool lastStatus = StopTelemetryData();

            return await Task.FromResult(BuildMethodRespose(new
            {
                LastStatus = lastStatus
            }));
        }

        private MethodResponse BuildMethodRespose(string responseInJSON, int status = 200)
        {
            return new MethodResponse(Encoding.UTF8.GetBytes(responseInJSON), status);
        }

        private MethodResponse BuildMethodRespose(object response, int status = 200)
        {
            return BuildMethodRespose(JsonConvert.SerializeObject(response), status);
        }

        private async Task SetReportedPropertyAsync(string name, dynamic value)
        {
            var collection = new TwinCollection();
            TwinCollectionExtension.Set(collection, name, value);
            await Transport.UpdateReportedPropertiesAsync(collection);
        }

        private async Task SetReportedPropertyAsync(Dictionary<string, dynamic> pairs)
        {
            var collection = new TwinCollection();
            foreach (var pair in pairs)
            {
                TwinCollectionExtension.Set(collection, pair.Key, pair.Value);
            }
            await Transport.UpdateReportedPropertiesAsync(collection);
        }

        protected async Task OnSetPointTempUpdate(object value)
        {
            var telemetry = _telemetryController as ITelemetryWithSetPointTemperature;
            telemetry.SetPointTemperature = Convert.ToDouble(value);

            await SetReportedPropertyAsync(SetPointTempPropertyName, telemetry.SetPointTemperature);
        }

        protected async Task OnTelemetryIntervalUpdate(object value)
        {
            var telemetry = _telemetryController as ITelemetryWithInterval;
            telemetry.TelemetryIntervalInSeconds = Convert.ToInt32(value);

            await SetReportedPropertyAsync(TelemetryIntervalPropertyName, telemetry.TelemetryIntervalInSeconds);
        }
    }
}
