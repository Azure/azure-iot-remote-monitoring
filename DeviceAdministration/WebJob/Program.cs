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

namespace DeviceAdministration.WebJob
{
    class Program
    {

        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //static ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        static IContainer eventProcessorContainer;

        static void Main(string[] args)
        {
            try
            {
                BuildContainer();

                CreateInitialDataAsNeeded();
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

        static void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new EventProcessorModule());
            eventProcessorContainer = builder.Build();
        }

        static void CreateInitialDataAsNeeded()
        {
            Trace.TraceInformation("Preparing to add initial data");
            var creator = eventProcessorContainer.Resolve<IDataInitializer>();
            creator.CreateInitialDataIfNeeded();
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
