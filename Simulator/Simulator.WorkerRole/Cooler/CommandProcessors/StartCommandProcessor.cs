using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Transport;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.CommandProcessors
{
    /// <summary>
    /// Command processor to start telemetry data
    public class StartCommandProcessor : CommandProcessor
    {
        private const string START_TELEMETRY = "StartTelemetry";

        public StartCommandProcessor(CoolerDevice device)
            : base(device)
        {

        }

        public async override Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == START_TELEMETRY)
            {
                var command = deserializableCommand.Command;

                try
                {
                    var device = Device as CoolerDevice;
                    device.StartTelemetryData();
                    return CommandProcessingResult.Success;
                }
                catch (Exception)
                {
                    return CommandProcessingResult.RetryLater;
                }

            }
            else if (NextCommandProcessor != null)
            {
                return await NextCommandProcessor.HandleCommandAsync(deserializableCommand);
            }

            return CommandProcessingResult.Success;
        }
    }
}
