using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors
{
    public class PingDeviceProcessor : CommandProcessor
    {
        public PingDeviceProcessor(IDevice device)
            : base(device)
        {
        }

        public override async Task<CommandProcessingResult> HandleCommandAsync(
            DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == "PingDevice")
            {
                var command = deserializableCommand.CommandHistory;

                try
                {
                    return CommandProcessingResult.Success;
                }
                catch (Exception)
                {
                    return CommandProcessingResult.RetryLater;
                }
            }
            if (NextCommandProcessor != null)
            {
                return await NextCommandProcessor.HandleCommandAsync(deserializableCommand);
            }

            return CommandProcessingResult.CannotComplete;
        }
    }
}