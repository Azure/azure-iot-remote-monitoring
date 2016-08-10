using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry
{
    /// <summary>
    /// Represents a static, pre-defined group of events that a
    /// simulated device will send to the cloud.
    /// 
    /// To create events using code, implement the ITelemetry interface
    /// directly.
    /// </summary>
    public class ConcreteTelemetry : ITelemetry
    {
        private readonly ILogger _logger;

        public ConcreteTelemetry(ILogger logger)
        {
            _logger = logger;
        }

        public bool RepeatForever { get; set; }

        public int RepeatCount { get; set; }

        public TimeSpan DelayBefore { get; set; }

        public string MessageBody { get; set; }


        public async Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            var groupCount = 0;

            while (RepeatForever || groupCount < RepeatCount)
            {
                token.ThrowIfCancellationRequested();

                await Task.Delay(DelayBefore, token);
                await sendMessageAsync(MessageBody);
                groupCount++;
            }
        }
    }
}
