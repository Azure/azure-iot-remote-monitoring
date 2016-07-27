using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.CommandProcessors
{
    /// <summary>
    /// Command processor to handle activating external temperature
    /// </summary>
    public class DiagnosticTelemetryCommandProcessor : CommandProcessorND
    {
        private const string DIAGNOSTIC_TELEMETRY = "DiagnosticTelemetry";

        public DiagnosticTelemetryCommandProcessor(CoolerDevice device)
            : base(device)
        {

        }

        public async override Task<CommandProcessingResultND> HandleCommandAsync(DeserializableCommandND deserializableCommand)
        {
            if (deserializableCommand.CommandName == DIAGNOSTIC_TELEMETRY)
            {
                var command = deserializableCommand.Command;

                try
                {
                    var device = Device as CoolerDevice;
                    if (device != null)
                    {
                        dynamic parameters = WireCommandSchemaHelper.GetParameters(command);
                        if (parameters != null)
                        {
                            dynamic activeAsDynamic = 
                                ReflectionHelper.GetNamedPropertyValue(
                                    parameters, 
                                    "Active", 
                                    usesCaseSensitivePropertyNameMatch: true,
                                    exceptionThrownIfNoMatch: true);

                            if (activeAsDynamic != null)
                            {
                                var active = Convert.ToBoolean(activeAsDynamic.ToString());

                                if (active != null)
                                {
                                    device.DiagnosticTelemetry(active);
                                    return CommandProcessingResultND.Success;
                                }
                                else
                                {
                                    // Active is not a boolean.
                                    return CommandProcessingResultND.CannotComplete;
                                }
                            }
                            else
                            {
                                // Active is a null reference.
                                return CommandProcessingResultND.CannotComplete;
                            }
                        }
                        else
                        {
                            // parameters is a null reference.
                            return CommandProcessingResultND.CannotComplete;
                        }
                    }
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
