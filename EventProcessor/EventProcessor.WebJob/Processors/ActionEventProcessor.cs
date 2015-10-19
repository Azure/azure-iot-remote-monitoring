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
    public class ActionEventProcessor : IActionEventProcessor, IDisposable
    {
        private readonly IActionLogic _actionLogic;
        private readonly IActionMappingLogic _actionMappingLogic;

        private EventProcessorHost _eventProcessorHost;
        private ActionProcessorFactory _factory;
        private IConfigurationProvider _configurationProvider; 
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private bool _disposed = false;

        public ActionEventProcessor(
            ILifetimeScope lifetimeScope,
            IActionLogic actionLogic,
            IActionMappingLogic actionMappingLogic)
        {
            _configurationProvider = lifetimeScope.Resolve<IConfigurationProvider>();
            _actionLogic = actionLogic;
            _actionMappingLogic = actionMappingLogic;
        }

        public void Start()
        {
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => this.StartProcessor(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            TimeSpan timeout = TimeSpan.FromSeconds(30);
            TimeSpan sleepInterval = TimeSpan.FromSeconds(1);
            while (_isRunning)
            {
                if (timeout < sleepInterval)
                {
                    break;
                }
                Thread.Sleep(sleepInterval);
            }
        }

        private async Task StartProcessor(CancellationToken token)
        {
            try
            {
                string hostName = Environment.MachineName;
                string eventHubPath = _configurationProvider.GetConfigurationSettingValue("RulesEventHub.Name").ToLowerInvariant();
                string consumerGroup = EventHubConsumerGroup.DefaultGroupName;
                string eventHubConnectionString = _configurationProvider.GetConfigurationSettingValue("RulesEventHub.ConnectionString");
                string storageConnectionString = _configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");

                _eventProcessorHost = new EventProcessorHost(
                    hostName,
                    eventHubPath.ToLower(),
                    consumerGroup,
                    eventHubConnectionString,
                    storageConnectionString);

                _factory = new ActionProcessorFactory(
                    _actionLogic,
                    _actionMappingLogic,
                    _configurationProvider);

                Trace.TraceInformation("ActionEventProcessor: Registering host...");
                var options = new EventProcessorOptions();
                options.ExceptionReceived += OptionsOnExceptionReceived;
                await _eventProcessorHost.RegisterEventProcessorFactoryAsync(_factory);

                // processing loop
                while (!token.IsCancellationRequested)
                {
                    Trace.TraceInformation("ActionEventProcessor: Processing...");
                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                }

                // cleanup
                await _eventProcessorHost.UnregisterEventProcessorAsync();
            }
            catch (Exception e)
            {
                Trace.TraceError("Error in ActionProcessor.StartProcessor, Exception: {0}", e.ToString());
            }
            _isRunning = false;
        }

        private void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs args)
        {
            Trace.TraceError("Received exception, action: {0}, exception: {1}", args.Action, args.Exception.ToString());
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

        ~ActionEventProcessor()
        {
            Dispose(false);
        }
    }
}
