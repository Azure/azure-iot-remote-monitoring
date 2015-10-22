namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging
{
    /// <summary>
    /// Basic implementation of a loger where all functions are noops.
    /// </summary>
    public class EmptyLogger : ILogger
    {
        public void LogInfo(string message)
        {
            // no operation (testing performance)
        }

        public void LogInfo(string format, params object[] args)
        {
            // no operation (testing performance)
        }

        public void LogWarning(string message)
        {
            // no operation (testing performance)
        }

        public void LogWarning(string format, params object[] args)
        {
            // no operation (testing performance)
        }

        public void LogError(string message)
        {
            // no operation (testing performance)
        }

        public void LogError(string format, params object[] args)
        {
            // no operation (testing performance)
        }
    }
}
