using System;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Telemetry;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices
{
    /// <summary>
    /// Implementation of a specific device type that extends the BaseDevice functionality
    /// </summary>
    public class CoolerDevice : DeviceBase
    {

        public CoolerDevice(ILogger logger, ITransportFactory transportFactory, IConfigurationProvider configurationProvider)
            : base(logger, transportFactory, configurationProvider)
        {
        }

        protected override void PopulateTelemetries(Action<ITelemetry> addTelemetry)
        {
            addTelemetry(new RemoteMonitorTelemetry(Logger, DeviceID));
            addTelemetry(new StartupTelemetry(Logger, this));
        }

        /// <summary>
        /// Builds up the set of commands that are supported by this device
        /// </summary>
        protected override void InitCommandProcessors()
        {
            var pingDeviceProcessor = new PingDeviceProcessor(this);
            var startCommandProcessor = new StartCommandProcessor(this);
            var stopCommandProcessor = new StopCommandProcessor(this);
            var changeSetPointTempCommandProcessor = new ChangeSetPointTempCommandProcessor(this);
            var changeDeviceStateCommmandProcessor = new ChangeDeviceStateCommandProcessor(this);

            pingDeviceProcessor.NextCommandProcessor = startCommandProcessor;
            startCommandProcessor.NextCommandProcessor = stopCommandProcessor;
            stopCommandProcessor.NextCommandProcessor = changeSetPointTempCommandProcessor;
            changeSetPointTempCommandProcessor.NextCommandProcessor = changeDeviceStateCommmandProcessor;

            RootCommandProcessor = pingDeviceProcessor;
        }
        
        public void ChangeSetPointTemp(double setPointTemp)
        {
            var remoteMonitorTelemetries = Telemetries.OfType<RemoteMonitorTelemetry>().ToList();
            remoteMonitorTelemetries.ForEach(t => t.ChangeSetPointTemperature(setPointTemp));
            Logger.LogInfo("Device {0} temperature changed to {1}", DeviceID, setPointTemp);
        }

        public async void ChangeDeviceState(string deviceState)
        {
            // simply update the DeviceState property and send updated device info packet
            DeviceProperties.DeviceState = deviceState;
            await SendDeviceInfo();
            Logger.LogInfo("Device {0} in {1} state", DeviceID, deviceState);
        }
    }
}
