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
    public abstract class CommandProcessorND
    {
        protected IDeviceND Device;

        public CommandProcessorND NextCommandProcessor { get; set; }

        protected CommandProcessorND(IDeviceND device)
        {
            Device = device;
        }

        public abstract Task<CommandProcessingResultND> HandleCommandAsync(DeserializableCommandND message);
    }

    /// <summary>
    /// The supported command processing results.
    /// </summary>
    public enum CommandProcessingResultND
    {
        Success = 0,
        RetryLater,
        CannotComplete
    }
}
