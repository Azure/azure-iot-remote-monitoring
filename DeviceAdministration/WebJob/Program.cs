using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole.Processors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.Devices.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Devices.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Serialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Transport.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole.DataInitialization;
using Microsoft.Azure.IoT.Samples.EventProcessor.WorkerRole.Processors;
using System.IO;

namespace DeviceAdministration.WebJob
{
    class Program
    {

        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //static ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        static IContainer eventProcessorContainer;

        private const string SHUTDOWN_FILE_ENV_VAR = "WEBJOBS_SHUTDOWN_FILE";
        private static string _shutdownFile;
        private static bool _shutdownSignalReceived = false;
        private static Timer _timer;

        static void Main(string[] args)
        {
            try
            {
                //Cloud deploys often get staged and started to warm them up, then get a shutdown
                //signal from the framework before being moved to the production slot. We don't want 
                //to start initializing data if we have already gotten the shutdown message, so we'll 
                //monitor it. This environment variable is reliable
                //http://blog.amitapple.com/post/2014/05/webjobs-graceful-shutdown/#.VhVYO6L8-B4
                _shutdownFile = Environment.GetEnvironmentVariable(SHUTDOWN_FILE_ENV_VAR);

                // Setup a file system watcher on that file's directory to know when the file is created
                //First check for null, though. This does not exist on a localhost deploy, only cloud
                if (!string.IsNullOrWhiteSpace(_shutdownFile))
                {
                    var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(_shutdownFile));
                    fileSystemWatcher.Created += OnShutdownFileChanged;
                    fileSystemWatcher.Changed += OnShutdownFileChanged;
                    fileSystemWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
                    fileSystemWatcher.IncludeSubdirectories = false;
                    fileSystemWatcher.EnableRaisingEvents = true;

                    //In case the file had already been created before we started watching it.
                    if (System.IO.File.Exists(_shutdownFile))
                    {
                        _shutdownSignalReceived = true;

                    }
                }

                BuildContainer();

                StartDataInitializationAsNeeded();
                StartEventProcessorHost();
                
                StartActionProcessorHost();
                StartMessageFeedbackProcessorHost();
                StartSimulator();

                RunAsync().Wait();
            }
            catch (Exception ex)
            {
                cancellationTokenSource.Cancel();
                Trace.TraceError("Webjob terminating: {0}", ex.ToString());
            }
        }

        private static void OnShutdownFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.IndexOf(Path.GetFileName(_shutdownFile), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _shutdownSignalReceived = true;
            }
        }

        static void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new EventProcessorModule());
            eventProcessorContainer = builder.Build();
        }

        static void StartDataInitializationAsNeeded()
        {
            //We have observed that Azure reliably starts the web job twice on a fresh deploy. The second start
            //is reliably about 7 seconds after the first start (under current conditions -- this is admittedly
            //not a perfect solution, but absent visibility into the black box of Azure this is what works at
            //the time) with a shutdown command being received on the current instance in the interim. We want
            //to further bolster our guard against starting a data initialization process that may be aborted
            //in the middle of its work. So we want to delay the data initialization for about 10 seconds to
            //give ourselves the best chance of receiving the shutdown command if it is going to come in. After
            //this delay there is an extremely good chance that we are on a stable start that will remain in place.
            _timer = new Timer(CreateInitialDataAsNeeded, null, 10000, Timeout.Infinite);
        }

        static void CreateInitialDataAsNeeded(object state)
        {
            _timer.Dispose();
            if (!_shutdownSignalReceived && !cancellationTokenSource.Token.IsCancellationRequested)
            {

                Trace.TraceInformation("Preparing to add initial data");
                var creator = eventProcessorContainer.Resolve<IDataInitializer>();
                creator.CreateInitialDataIfNeeded();
            }
        }

        static void StartEventProcessorHost()
        {
            Trace.TraceInformation("Starting Event Processor");
            var eventProcessor = eventProcessorContainer.Resolve<IDeviceEventProcessor>();
            eventProcessor.Start(cancellationTokenSource.Token);
        }

        static void StartActionProcessorHost()
        {
            Trace.TraceInformation("Starting action processor");
            var actionProcessor = eventProcessorContainer.Resolve<IActionEventProcessor>();
            actionProcessor.Start();
        }

        static void StartMessageFeedbackProcessorHost()
        {
            Trace.TraceInformation("Starting command feedback processor");
            var feedbackProcessor = eventProcessorContainer.Resolve<IMessageFeedbackProcessor>();
            feedbackProcessor.Start();
        }

        static void StartSimulator()
        {
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

            // Start Simulator
            Trace.TraceInformation("Starting Simulator");
            var tester = new BulkDeviceTester(transportFactory, logger, configProvider, telemetryFactory, deviceFactory, deviceStorage);
            Task.Run(() => tester.ProcessDevicesAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        static async Task RunAsync()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                Trace.TraceInformation("Running");
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), cancellationTokenSource.Token);
                }
                catch (TaskCanceledException) { }
            }
        }
    }
}
