using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry.Factory
{
    public interface ITelemetryFactory
    {
        /// <summary>
        /// Populates a device with telemetry events or logic
        /// </summary>
        /// <param name="device">Device interface to populate</param>
        /// <returns>
        /// Returns object as a way to handle returning the instance that is generating telemetry data
        /// so that it can be used by the caller of this method
        /// </returns>
        object PopulateDeviceWithTelemetryEvents(IDevice device);
    }
}
