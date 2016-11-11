namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class Job : JobResponse
    {
        // TODO: mock code to replace JobId for testing
        public string Id { get; set; }

        public string Name { get; set; }

        public string QueryName { get; set; }

        public JobOperationType OperationType { get; set; }
        
        public enum JobOperationType
        {
            EditPropertyOrTag,
            InvokeMethod
        }
    }
}
