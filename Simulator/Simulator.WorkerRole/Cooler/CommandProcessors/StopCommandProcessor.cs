using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.SimulatorCore.Transport;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.Cooler.CommandProcessors
{
    /// <summary>
    /// Command processor to stop telemetry data
    /// </summary>
    public class StopCommandProcessor : CommandProcessor 
    {
        private const string STOP_TELEMETRY = "StopTelemetry";

        public StopCommandProcessor(CoolerDevice device)
            : base(device)
        {

        }

        public async override Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == STOP_TELEMETRY)
            {
                var command = deserializableCommand.Command;

                try
                {
                    var device = Device as CoolerDevice;
                    device.StopTelemetryData();
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
