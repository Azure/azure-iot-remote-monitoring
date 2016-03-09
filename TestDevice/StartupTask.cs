using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace TestDevice
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += TaskInstance_Canceled;
            deferral = taskInstance.GetDeferral();

            var logger = new DebugLogger();
            var configProvider = new ConfigurationProvider();

            var serializer = new JsonSerializer();
            var transportFactory = new IotHubTransportFactory(serializer, logger, configProvider);

            var deviceFactory = new TestDeviceFactory();
            var deviceConfig = new InitialDeviceConfig()
            {
                DeviceId = "",
                HostName = ".azure-devices.net",
                Key = "",
            };
            var device = await deviceFactory.CreateDevice(logger, transportFactory, configProvider, deviceConfig);

            await device.StartAsync(cts.Token);

            deferral.Complete();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            cts.Cancel();
        }
    }
}
