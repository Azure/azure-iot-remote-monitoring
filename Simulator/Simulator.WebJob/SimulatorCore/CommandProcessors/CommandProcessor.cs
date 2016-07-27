using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors
{
    /// <summary>
    /// The CommandProcessor classes implement the Gang of Four's
    /// "Chain of Responsibility" pattern that passes the command
    /// to the next command processor if the current one is unable to 
    /// satisfy the request.
    /// </summary>
    public abstract class CommandProcessor
    {
        protected IDevice Device;

        public CommandProcessor NextCommandProcessor { get; set; }

        protected CommandProcessor(IDevice device)
        {
            Device = device;
        }

        public abstract Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand message);
    }

    /// <summary>
    /// The supported command processing results.
    /// </summary>
    public enum CommandProcessingResult
    {
        Success = 0,
        RetryLater,
        CannotComplete
    }
}
