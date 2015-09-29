using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Telemetry;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.Telemetry
{
    public class StartupTelemetry : ITelemetry
    {
        private readonly ILogger _logger;
        private readonly IDevice _device;
        
        public StartupTelemetry(ILogger logger, IDevice device)
        {
            _logger = logger;
            _device = device;
        }

        public async Task SendEventsAsync(System.Threading.CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            if (!token.IsCancellationRequested)
            {
                _logger.LogInfo("Sending initial data for device {0}", _device.DeviceID);
                await sendMessageAsync(_device.GetDeviceInfo());
            }
        }
    }
}