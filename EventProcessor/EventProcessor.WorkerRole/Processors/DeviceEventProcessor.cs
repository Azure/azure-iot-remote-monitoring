using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.ServiceBus.Messaging;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole.Processors
{
    public class DeviceEventProcessor : IDeviceEventProcessor
    {
        readonly IDeviceLogic _deviceLogic;

        EventProcessorHost eventProcessorHost = null;
        DeviceAdministrationProcessorFactory factory;
        IConfigurationProvider configurationProvider;
        CancellationTokenSource cancellationTokenSource;
        bool running;

        public DeviceEventProcessor(ILifetimeScope scope, IDeviceLogic deviceLogic)
        {
            this.configurationProvider = scope.Resolve<IConfigurationProvider>();
            _deviceLogic = deviceLogic;
        }

        public void Start()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.Start(this.cancellationTokenSource.Token);
        }

        public void Start(CancellationToken cancellationToken)
        {
            this.running = true;
            Task.Run(() => this.StartProcessor(cancellationToken), cancellationToken);
        }

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();
            TimeSpan timeout = TimeSpan.FromSeconds(30);
            TimeSpan sleepInterval = TimeSpan.FromSeconds(1);
            while (this.running)
            {
                if (timeout < sleepInterval)
                {
                    break;
                }
                Thread.Sleep(sleepInterval);
            }
        }

        async Task StartProcessor(CancellationToken token)
        {
            try
            {
                // Initialize
                this.eventProcessorHost = new EventProcessorHost(
                    Environment.MachineName,
                    configurationProvider.GetConfigurationSettingValue("eventHub.HubName").ToLower(),
                    EventHubConsumerGroup.DefaultGroupName,
                    configurationProvider.GetConfigurationSettingValue("eventHub.ConnectionString"),
                    configurationProvider.GetConfigurationSettingValue("eventHub.StorageConnectionString"));

                this.factory = new DeviceAdministrationProcessorFactory(_deviceLogic, configurationProvider);
                Trace.TraceInformation("DeviceEventProcessor: Registering host...");
                var options = new EventProcessorOptions();
                options.ExceptionReceived += OptionsOnExceptionReceived;
                await this.eventProcessorHost.RegisterEventProcessorFactoryAsync(factory);

                // processing loop
                while (!token.IsCancellationRequested)
                {
                    Trace.TraceInformation("DeviceEventProcessor: Processing...");
                    await Task.Delay(TimeSpan.FromMinutes(5), token);

                    // Any additional incremental processing can be done here (like checking states, etc).
                }

                // cleanup
                await this.eventProcessorHost.UnregisterEventProcessorAsync();
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Error in DeviceEventProcessor.StartProcessor, Exception: {0}", e.Message);
            }
            this.running = false;
        }

        void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Trace.TraceError("Received exception, action: {0}, message: {1}", exceptionReceivedEventArgs.Action, exceptionReceivedEventArgs.Exception.ToString());
        }
    }
}
