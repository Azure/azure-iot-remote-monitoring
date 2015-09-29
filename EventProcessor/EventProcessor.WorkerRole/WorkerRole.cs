using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole.Processors;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.Azure.IoT.Samples.EventProcessor.WorkerRole.Processors;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        IContainer container;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole-Processor is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            Trace.TraceInformation("Starting worker role");

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // Cancel the config change events
            // This causes the VMs in role to restart in orderly fashion and hence getting the new config change
            RoleEnvironment.Changing += (sender, e) => e.Cancel = true;

            bool result = false;
            try
            {
                result = base.OnStart();

                var builder = new ContainerBuilder();
                builder.RegisterModule(new EventProcessorModule());
                this.container = builder.Build();

                BootstrapDefaultsIfNeeded();

                // Start event processor
                Trace.TraceInformation("Starting event processor");
                var eventProcessor = this.container.Resolve<IDeviceEventProcessor>();
                eventProcessor.Start();
                Trace.TraceInformation("Event processor has been started");

                // start action processor
                Trace.TraceInformation("Starting action processor");
                var actionProcessor = this.container.Resolve<IActionEventProcessor>();
                actionProcessor.Start();
                Trace.TraceInformation("Action processor has been started");

                // Start Message Feedback Processor.
                Trace.TraceInformation("Starting Message Feedback Processor");
                var messageProcessor = this.container.Resolve<IMessageFeedbackProcessor>();
                messageProcessor.Start();
                Trace.TraceInformation("Message Feedback Processor started.");
            }
            catch(Exception ex)
            {
                Trace.WriteLine("Failed to start - Exception: {0}", ex.ToString());
                throw;
            }
            return result;
        }

        private void BootstrapDefaultsIfNeeded()
        {
            try
            {
                // Initialize data
                var actionMappingLogic = this.container.Resolve<IActionMappingLogic>();
                bool initializationNeeded = false;
                Task<bool>.Run(async () => initializationNeeded = await actionMappingLogic.IsInitializationNeededAsync()).Wait();
                if (initializationNeeded)
                {
                    List<string> bootstrappedDevices = null;
                    var deviceLogic = this.container.Resolve<IDeviceLogic>();
                    Task.Run(async () => bootstrappedDevices = await deviceLogic.BootstrapDefaultDevices()).Wait();

                    var rulesLogic = this.container.Resolve<IDeviceRulesLogic>();
                    Task.Run(() => rulesLogic.BootstrapDefaultRulesAsync(bootstrappedDevices)).Wait();

                    actionMappingLogic.InitializeDataIfNecessaryAsync();
                }                
            } 
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to bootstrap default data - Exception: {0}", ex.ToString());
            }
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            var eventProcessor = this.container.Resolve<IDeviceEventProcessor>();
            eventProcessor.Stop();
            Trace.TraceInformation("WorkerRole - Event processor has stopped");

            var actionProcessor = this.container.Resolve<IActionEventProcessor>();
            actionProcessor.Stop();
            Trace.TraceInformation("WorkerRole - Action processor has stopped");

            var messageProcessor = this.container.Resolve<IMessageFeedbackProcessor>();
            messageProcessor.Stop();
            Trace.TraceInformation("WorkerRole - Feedback Processor stopped.");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                }
                catch (TaskCanceledException) { }
            }
        }
    }
}
