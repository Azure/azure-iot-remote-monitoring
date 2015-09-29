using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.Devices.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Devices.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Serialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Transport.Factory;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole is running");

            try
            {
                this.RunAsync(this._cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this._runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole is stopping");

            this._cancellationTokenSource.Cancel();
            this._runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            // Dependencies to inject into the Bulk Device Tester
            var logger = new TraceLogger();
            var configProvider = new ConfigurationProvider();
            var telemetryFactory = new CoolerTelemetryFactory(logger);
            
            var serializer = new JsonSerialize();
            var transportFactory = new IotHubTransportFactory(serializer, logger, configProvider);

            IVirtualDeviceStorage deviceStorage = null;
            var useConfigforDeviceList = Convert.ToBoolean(configProvider.GetConfigurationSettingValueOrDefault("UseConfigForDeviceList", "False"));

            if (useConfigforDeviceList)
            {
                deviceStorage = new AppConfigRepository(configProvider, logger);
            }
            else 
            {
                deviceStorage = new VirtualDeviceTableStorage(configProvider);
            }

            IDeviceFactory deviceFactory = new CoolerDeviceFactory();

            var tester = new BulkDeviceTester(transportFactory, logger, configProvider, telemetryFactory, deviceFactory, deviceStorage);    
            await tester.ProcessDevicesAsync(cancellationToken);

            Trace.TraceInformation("");
            Trace.TraceInformation("*********************************************************************************************************************");
            Trace.TraceInformation("ELAPSED TIME: {0}ms", (DateTime.Now - startTime).TotalMilliseconds);
            Trace.TraceInformation("*********************************************************************************************************************");
            Trace.TraceInformation("");
        }
    }
}
