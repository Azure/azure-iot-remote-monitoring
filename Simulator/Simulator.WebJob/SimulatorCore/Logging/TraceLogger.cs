using System.Diagnostics;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging
{
    /// <summary>
    /// Default implementation of ILogger with the System.Diagnostics.Trace 
    /// object as the logger.
    /// </summary>
    public class TraceLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Trace.TraceInformation(message);
        }

        public void LogInfo(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        public void LogWarning(string message)
        {
            Trace.TraceWarning(message);
        }

        public void LogWarning(string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
        }

        public void LogError(string message)
        {
            Trace.TraceError(message);
        }

        public void LogError(string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }
    }
}
