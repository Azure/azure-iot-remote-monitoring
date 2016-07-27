using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors
{
    public class PingDeviceProcessorND : CommandProcessorND
    {
        public PingDeviceProcessorND(IDeviceND device)
            : base(device)
        {

        }

        public async override Task<CommandProcessingResultND> HandleCommandAsync(DeserializableCommandND deserializableCommand)
        {
            if (deserializableCommand.CommandName == "PingDevice")
            {
                Command command = deserializableCommand.Command;

                try
                {
                    return CommandProcessingResultND.Success;
                }
                catch (Exception)
                {
                    return CommandProcessingResultND.RetryLater;
                }
            }
            else if (NextCommandProcessor != null)
            {
                return await NextCommandProcessor.HandleCommandAsync(deserializableCommand);
            }

            return CommandProcessingResultND.CannotComplete;
        }
    }
}
