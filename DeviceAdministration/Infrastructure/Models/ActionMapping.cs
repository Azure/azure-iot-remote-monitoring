namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Defines a mapping between a RuleOutput value (output from the ASA job looking
    /// for alarm-like conditions), and an ActionId (which is currently
    /// associated with a logic app).
    /// </summary>
    public class ActionMapping
    {
        public string RuleOutput { get; set; }

        public string ActionId { get; set; }
    }
}
