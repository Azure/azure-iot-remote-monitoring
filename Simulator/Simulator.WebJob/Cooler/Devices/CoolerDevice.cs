using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices
{
    /// <summary>
    /// Implementation of a specific device type that extends the BaseDevice functionality
    /// </summary>
    public class CoolerDevice : DeviceBase
    {

        public CoolerDevice(ILogger logger, ITransportFactory transportFactory,
            ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider)
            : base(logger, transportFactory, telemetryFactory, configurationProvider)
        {
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

        public void StartTelemetryData()
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            remoteMonitorTelemetry.TelemetryActive = true;
            Logger.LogInfo("Device {0}: Telemetry has started", DeviceID);
        }

        public void StopTelemetryData()
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            remoteMonitorTelemetry.TelemetryActive = false;
            Logger.LogInfo("Device {0}: Telemetry has stopped", DeviceID);
        }

        public void ChangeSetPointTemp(double setPointTemp)
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            remoteMonitorTelemetry.ChangeSetPointTemperature(setPointTemp);
            Logger.LogInfo("Device {0} temperature changed to {1}", DeviceID, setPointTemp);
        }

        public async void ChangeDeviceState(string deviceState)
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
    }
}
