using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole.Processors;
using Microsoft.Azure.IoT.Samples.EventProcessor.WorkerRole.Processors;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole
{
    public static class Program
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        static IContainer eventProcessorContainer;

        private const string SHUTDOWN_FILE_ENV_VAR = "WEBJOBS_SHUTDOWN_FILE";
        private static bool _shutdownSignalReceived = false;
        private static string _shutdownFile;

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

                StartEventProcessorHost();
                StartActionProcessorHost();
                StartMessageFeedbackProcessorHost();

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
