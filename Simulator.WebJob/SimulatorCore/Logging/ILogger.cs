namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging
{
    /// <summary>
    /// Simple interface to insulate the app from the logging technology 
    /// used. Intended for various informational, warning, and error messages.
    /// </summary>
    public interface ILogger
    {
        void LogInfo(string message);

        void LogInfo(string format, params object[] args);

        void LogWarning(string message);

        void LogWarning(string format, params object[] args);

        void LogError(string message);

        void LogError(string format, params object[] args);
    }
}
