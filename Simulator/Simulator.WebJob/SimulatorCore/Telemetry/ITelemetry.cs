using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry
{
    /// <summary>
    /// Represents a group of one or more events that a simulated device will send to the cloud.
    ///
    /// The simulator has a list of ITelemetry objects that it executes in order. These
    /// can be either:
    /// 1) Instances of ConcreteTelemetry that inherit from ITelemetry with simple data
    /// 2) Custom class instances that implement ITelemetry directly with custom code
    /// </summary>
    public interface ITelemetry
    {
        /// <summary>
        /// Sends all events in the group to the cloud before the returned Task completes.
        /// </summary>
        /// <param name="device">Device object to send devices from</param>
        /// <param name="token">Cancellation token to cancel process</param>
        /// <returns>Task that completes when all tasks are sent</returns>
        Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync);
    }

    public interface ITelemetryWithInterval
    {
        uint TelemetryIntervalInSeconds { get; set; }
    }

    public interface ITelemetryWithTemperatureMeanValue
    {
        double TemperatureMeanValue { get; set; }
    }

    public interface ITelemetryFactoryResetSupport
    {
        void FactoryReset();
    }
}
