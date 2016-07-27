using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.CommandProcessors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.CommandProcessors
{
    /// <summary>
    /// Command processor to handle the change in the temperature range
    /// </summary>
    public class ChangeSetPointTempCommandProcessor : CommandProcessorND
    {
        private const string CHANGE_SET_POINT_TEMP = "ChangeSetPointTemp";

        public ChangeSetPointTempCommandProcessor(CoolerDevice device)
            : base(device)
        {

        }

        public async override Task<CommandProcessingResultND> HandleCommandAsync(DeserializableCommandND deserializableCommand)
        {
            if (deserializableCommand.CommandName == CHANGE_SET_POINT_TEMP)
            {
                CommandHistoryND commandHistory = deserializableCommand.CommandHistory;

                try
                {
                    var device = Device as CoolerDevice;
                    if (device != null)
                    {
                        dynamic parameters = commandHistory.Parameters;
                        if (parameters != null)
                        {
                            dynamic setPointTempDynamic = ReflectionHelper.GetNamedPropertyValue(
                                parameters,
                                "SetPointTemp",
                                usesCaseSensitivePropertyNameMatch: true,
                                exceptionThrownIfNoMatch: true);

                            if (setPointTempDynamic != null)
                            {
                                double setPointTemp;
                                if (Double.TryParse(setPointTempDynamic.ToString(), out setPointTemp))
                                {
                                    device.ChangeSetPointTemp(setPointTemp);

                                    return CommandProcessingResultND.Success;
                                }
                                else
                                {
                                    // SetPointTemp cannot be parsed as a double.
                                    return CommandProcessingResultND.CannotComplete;
                                }
                            }
                            else
                            {
                                // setPointTempDynamic is a null reference.
                                return CommandProcessingResultND.CannotComplete;
                            }
                        }
                        else
                        {
                            // parameters is a null reference.
                            return CommandProcessingResultND.CannotComplete;
                        }
                    }
                    else
                    {
                        // Unsupported Device type.
                        return CommandProcessingResultND.CannotComplete;
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
