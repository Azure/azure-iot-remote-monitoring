using System.Diagnostics;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Logging
{
    /// <summary>
    /// Default implementation of ILogger with the System.Diagnostics.Trace 
    /// object as the logger.
    /// </summary>
    public class DebugLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Debug.WriteLine($"INFORMATION: {message}");
        }

        public void LogInfo(string format, params object[] args)
        {
            Debug.WriteLine($"INFORMATION: {format}", args);
        }

        public void LogWarning(string message)
        {
            Debug.WriteLine($"WARNING: {message}");
        }

        public void LogWarning(string format, params object[] args)
        {
            Debug.WriteLine($"WARNING: {format}", args);
        }

        public void LogError(string message)
        {
            Debug.WriteLine($"ERROR: {message}");
        }

        public void LogError(string format, params object[] args)
        {
            Debug.WriteLine($"ERROR: {format}", args);
        }
    }
}
