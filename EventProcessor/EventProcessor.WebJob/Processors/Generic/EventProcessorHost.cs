namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors.Generic
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using ServiceBus.Messaging;

    public class EventProcessorHost<TEventProcessorFactory> : IEventProcessorHost, IDisposable
        where TEventProcessorFactory : class, IEventProcessorFactory
    {
        readonly object[] _arguments;
        readonly string _eventHubName;
        readonly string _eventHubConnectionString;
        readonly string _storageConnectionString;

        EventProcessorHost _eventProcessorHost;
        TEventProcessorFactory _factory;
        CancellationTokenSource _cancellationTokenSource;
        bool _running;
        bool _disposed;

        public EventProcessorHost(string eventHubName, string eventHubConnectionString, string storageConnectionString, params object[] arguments)
        {
            _arguments = arguments;
            _eventHubName = eventHubName;
            _eventHubConnectionString = eventHubConnectionString;
            _storageConnectionString = storageConnectionString;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Start(_cancellationTokenSource.Token);
        }

        public void Start(CancellationToken cancellationToken)
        {
            _running = true;
            Task.Run(() => StartProcessor(cancellationToken), cancellationToken);
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

        public async Task StartProcessor(CancellationToken token)
        {
            try
            {
                // Initialize
                _eventProcessorHost = new EventProcessorHost(
                    Guid.NewGuid().ToString(),
                    _eventHubName.ToLowerInvariant(),
                    EventHubConsumerGroup.DefaultGroupName,
                    _eventHubConnectionString,
                    _storageConnectionString,
                    _eventHubName.ToLowerInvariant().Replace('/','-'));

                _factory = Activator.CreateInstance(typeof(TEventProcessorFactory), _arguments) as TEventProcessorFactory;

                Trace.TraceInformation("{0}: Registering host...", GetType().Name);

                EventProcessorOptions options = new EventProcessorOptions();
                options.ExceptionReceived += OptionsOnExceptionReceived;
                await _eventProcessorHost.RegisterEventProcessorFactoryAsync(_factory);

                // processing loop
                while (!token.IsCancellationRequested)
                {
                    Trace.TraceInformation("{0}: Processing...", GetType().Name);
                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                }

                // cleanup
                await _eventProcessorHost.UnregisterEventProcessorAsync();
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Error in {0}.StartProcessor, Exception: {1}", GetType().Name, e.Message);
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

        ~EventProcessorHost()
        {
            Dispose(false);
        }
    }
}