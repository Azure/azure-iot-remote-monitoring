using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport;

namespace TestDevice
{
    /// <summary>
    /// Command processor to stop telemetry data
    /// </summary>
    internal class StopCommandProcessor : CommandProcessor 
    {
        private const string STOP_TELEMETRY = "StopTelemetry";

        public StopCommandProcessor(TestDevice device)
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
                    var device = Device as TestDevice;
                    await device.PauseAsync();
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

            return CommandProcessingResult.CannotComplete;
        }
    }
}
