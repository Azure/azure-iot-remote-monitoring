using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.ServiceBus.Messaging;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    public class DeviceEventProcessor : IDeviceEventProcessor, IDisposable
    {
        readonly IDeviceLogic _deviceLogic;

        EventProcessorHost _eventProcessorHost = null;
        DeviceAdministrationProcessorFactory _factory;
        IConfigurationProvider _configurationProvider;
        CancellationTokenSource _cancellationTokenSource;
        bool _running;
        bool _disposed = false;

        public DeviceEventProcessor(ILifetimeScope scope, IDeviceLogic deviceLogic)
        {
            _configurationProvider = scope.Resolve<IConfigurationProvider>();
            _deviceLogic = deviceLogic;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            this.Start(this._cancellationTokenSource.Token);
        }

        public void Start(CancellationToken cancellationToken)
        {
            _running = true;
            Task.Run(() => this.StartProcessor(cancellationToken), cancellationToken);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            TimeSpan timeout = TimeSpan.FromSeconds(30);
            TimeSpan sleepInterval = TimeSpan.FromSeconds(1);
            while (_running)
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
                _eventProcessorHost = new EventProcessorHost(
                    Environment.MachineName,
                    _configurationProvider.GetConfigurationSettingValue("eventHub.HubName").ToLowerInvariant(),
                    EventHubConsumerGroup.DefaultGroupName,
                    _configurationProvider.GetConfigurationSettingValue("eventHub.ConnectionString"),
                    _configurationProvider.GetConfigurationSettingValue("eventHub.StorageConnectionString"));

                _factory = new DeviceAdministrationProcessorFactory(_deviceLogic, _configurationProvider);
                Trace.TraceInformation("DeviceEventProcessor: Registering host...");
                var options = new EventProcessorOptions();
                options.ExceptionReceived += OptionsOnExceptionReceived;
                await _eventProcessorHost.RegisterEventProcessorFactoryAsync(_factory);

                // processing loop
                while (!token.IsCancellationRequested)
                {
                    Trace.TraceInformation("DeviceEventProcessor: Processing...");
                    await Task.Delay(TimeSpan.FromMinutes(5), token);

                    // Any additional incremental processing can be done here (like checking states, etc).
                }

                // cleanup
                await _eventProcessorHost.UnregisterEventProcessorAsync();
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Error in DeviceEventProcessor.StartProcessor, Exception: {0}", e.Message);
            }
            _running = false;
        }

        void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Trace.TraceError("Received exception, action: {0}, message: {1}", exceptionReceivedEventArgs.Action, exceptionReceivedEventArgs.Exception.ToString());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                }
            }

            _disposed = true;
        }

        ~DeviceEventProcessor()
        {
            Dispose(false);
        }
    }
}
