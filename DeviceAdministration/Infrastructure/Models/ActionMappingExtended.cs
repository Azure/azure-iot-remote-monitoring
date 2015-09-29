namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Model object that extends ActionMapping with additional data
    /// </summary>
    public class ActionMappingExtended : ActionMapping
    {
        public int NumberOfDevices { get; set; }
    }
}
