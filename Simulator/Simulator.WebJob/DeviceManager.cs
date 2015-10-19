using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob
{
    /// <summary>
    /// Manages and coordinates all devices
    /// </summary>
    public class DeviceManager
    {
        private readonly ILogger _logger;
        private readonly CancellationToken _token;

        private readonly Dictionary<string, CancellationTokenSource> _cancellationTokens;


        public DeviceManager(ILogger logger, CancellationToken token)
        {
            _logger = logger;
            _token = token;

            _cancellationTokens = new Dictionary<string, CancellationTokenSource>();
        }

        /// <summary>
        /// Starts all the devices in the list of devices in this class.
        /// 
        /// Note: This will not return until all devices have finished sending events,
        /// assuming no device has RepeatEventListForever == true
        /// </summary>
        public async Task StartDevicesAsync(List<IDevice> devices)
        {
            await Task.Run(async() => 
            {
                if (devices == null || !devices.Any())
                    return;

                var startDeviceTasks = new List<Task>();

                foreach (var device in devices)
                {
                    var deviceCancellationToken = new CancellationTokenSource();

                    startDeviceTasks.Add(device.StartAsync(deviceCancellationToken.Token));

                    _cancellationTokens.Add(device.DeviceID, deviceCancellationToken);
                }

                // wait here until all tasks complete
                await Task.WhenAll(startDeviceTasks);
            
            }, _token);
        }

        /// <summary>
        /// Cancel the asynchronous tasks for the devices specified
        /// </summary>
        /// <param name="deviceIds"></param>
        public void StopDevices(List<string> deviceIds) 
        {
            foreach (var deviceId in deviceIds)
            {
                var cancellationToken = _cancellationTokens[deviceId];

                if (cancellationToken != null)
                {
                    cancellationToken.Cancel();
                    _cancellationTokens.Remove(deviceId);

                    _logger.LogInfo("********** STOPPED DEVICE : {0} ********** ", deviceId);
                }
            }   
        }

        /// <summary>
        /// Cancel the asynchronous tasks for all devices
        /// </summary>
        public void StopAllDevices() 
        {
            foreach (var cancellationToken in _cancellationTokens)
            {
                cancellationToken.Value.Cancel();
                _logger.LogInfo("********** STOPPED DEVICE : {0} ********** ", cancellationToken.Key);
            }

            _cancellationTokens.Clear();
        }
    }
}
