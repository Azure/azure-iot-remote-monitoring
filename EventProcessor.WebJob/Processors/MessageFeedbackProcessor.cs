using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Azure.IoT.Samples.EventProcessor.WebJob.Processors
{
    public class MessageFeedbackProcessor : IMessageFeedbackProcessor, IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly IDeviceLogic _deviceLogic;
        private readonly string _iotHubConnectionString;
        private bool _isRunning;
        private bool _disposed = false;

        public MessageFeedbackProcessor(
            ILifetimeScope scope,
            IDeviceLogic deviceLogic)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (deviceLogic == null)
            {
                throw new ArgumentNullException("deviceLogic");
            }

            var configProvider = scope.Resolve<IConfigurationProvider>();
            _iotHubConnectionString = configProvider.GetConfigurationSettingValue("iotHub.ConnectionString");

            if (string.IsNullOrEmpty(_iotHubConnectionString))
            {
                throw new InvalidOperationException("Cannot find configuration setting: \"iotHub.ConnectionString\".");
            }

            _deviceLogic = deviceLogic;
        }

        public void Start()
        {
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(
                () => this.RunProcess(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token);
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

        private async Task RunProcess(CancellationToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            ServiceClient serviceClient = null;
            try
            {
                serviceClient = ServiceClient.CreateFromConnectionString(_iotHubConnectionString);
                await serviceClient.OpenAsync();

                while (!token.IsCancellationRequested)
                {
                    var batchReceiver = serviceClient.GetFeedbackReceiver();
                    var batch = await batchReceiver.ReceiveAsync(TimeSpan.FromSeconds(10.0));

                    IEnumerable<FeedbackRecord> records;
                    if ((batch == null) || ((records = batch.Records) == null))
                    {
                        continue;
                    }

                    records = records.Where(t => t != null)
                        .Where(x => !string.IsNullOrEmpty(x.DeviceId))
                        .Where(x => !string.IsNullOrEmpty(x.OriginalMessageId));

                    foreach (FeedbackRecord record in records)
                    {
                        UpdateDeviceRecord(record, batch.EnqueuedTime);
                    }

                    await batchReceiver.CompleteAsync(batch);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error in MessageFeedbackProcessor.RunProcess, Exception: {0}", ex.Message);
            }
            finally
            {
                if (serviceClient != null)
                {
                    await serviceClient.CloseAsync();
                }

                _isRunning = false;
            }
            
        }

        private async void UpdateDeviceRecord(FeedbackRecord record, DateTime enqueuDateTime)
        {
            Trace.TraceInformation(
                            "{0}{0}*** Processing Feedback Record ***{0}{0}DeviceId: {1}{0}OriginalMessageId: {2}{0}Result: {3}{0}{0}",
                            Console.Out.NewLine,
                            record.DeviceId,
                            record.OriginalMessageId,
                            record.StatusCode);

            var device = await _deviceLogic.GetDeviceAsync(record.DeviceId);
            var existingCommand = device?.CommandHistory.FirstOrDefault(x => x.MessageId == record.OriginalMessageId);
            if (existingCommand == null)
            {
                return;
            }

            var updatedTime = record.EnqueuedTimeUtc;
            if (updatedTime == default(DateTime))
            {
                updatedTime = enqueuDateTime == default(DateTime) ? DateTime.UtcNow : enqueuDateTime;
            }

            existingCommand.UpdatedTime = updatedTime;
            existingCommand.Result = record.StatusCode.ToString();

            if (record.StatusCode == FeedbackStatusCode.Success)
            {
                existingCommand.ErrorMessage = string.Empty;
            }


            await _deviceLogic.UpdateDeviceAsync(device);
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

        ~MessageFeedbackProcessor()
        {
            Dispose(false);
        }
    }
}
