using System;
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
        class TaskDetail
        {
            public CancellationTokenSource CancellationTokenSource { get; private set; }
            public Task Task { get; private set; }

            public TaskDetail(Func<CancellationToken, Task> entry)
            {
                CancellationTokenSource = new CancellationTokenSource();
                Task = entry(CancellationTokenSource.Token);
            }
        }

        private readonly ILogger _logger;
        private readonly CancellationToken _token;
        private readonly Dictionary<string, TaskDetail> _tasks;

        public DeviceManager(ILogger logger, CancellationToken token)
        {
            _logger = logger;
            _token = token;

            _tasks = new Dictionary<string, TaskDetail>();
        }

        /// <summary>
        /// Starts all the devices in the list of devices in this class.
        /// </summary>
        public void StartDevices(List<IDevice> devices)
        {
            if (devices == null || !devices.Any())
            {
                return;
            }

            foreach (var device in devices)
            {
                _tasks.Add(device.DeviceID, new TaskDetail(device.StartAsync));
            }
        }

        public IEnumerable<string> GetLiveDevices()
        {
            _logger.LogInfo($"{_tasks.Count} devices were started");
            var completedTasks = _tasks
                .Where(pair => pair.Value.Task.IsCompleted)
                .ToList();

            foreach (var pair in completedTasks)
            {
                if (pair.Value.Task.IsFaulted)
                {
                    _logger.LogWarning($"Device {pair.Key} shut down due to fault");
                }
                else
                {
                    _logger.LogInfo($"Device {pair.Key} shut down as expected");
                }

                _tasks.Remove(pair.Key);
            }

            var devices = _tasks.Keys.ToList();
            foreach (var device in devices)
            {
                _logger.LogInfo($"Device {device} is still running");
            }

            return devices;
        }

        /// <summary>
        /// Cancel the asynchronous tasks for the devices specified
        /// </summary>
        /// <param name="deviceIds"></param>
        public void StopDevices(List<string> deviceIds)
        {
            foreach (var deviceId in deviceIds)
            {
                TaskDetail task;
                if (_tasks.TryGetValue(deviceId, out task))
                {
                    task.CancellationTokenSource.Cancel();
                    _tasks.Remove(deviceId);

                    _logger.LogInfo("********** STOPPED DEVICE : {0} ********** ", deviceId);
                }
            }
        }

        /// <summary>
        /// Cancel the asynchronous tasks for all devices
        /// </summary>
        public void StopAllDevices()
        {
            foreach (var pair in _tasks)
            {
                pair.Value.CancellationTokenSource.Cancel();
                _logger.LogInfo("********** STOPPED DEVICE : {0} ********** ", pair.Key);
            }

            _tasks.Clear();
        }
    }
}
