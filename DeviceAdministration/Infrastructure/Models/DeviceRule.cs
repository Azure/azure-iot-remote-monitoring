using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Container object for a DeviceRule
    /// </summary>
    public class DeviceRule
    {
        public DeviceRule() { }

        public DeviceRule(string ruleId)
        {
            RuleId = ruleId;
        }

        public string RuleId { get; set; }
        public bool EnabledState { get; set; }
        public string DeviceID { get; set; }
        public string DataField { get; set; }
        public string Operator { get; set; }
        public double? Threshold { get; set; }
        public string RuleOutput { get; set; }
        public string Etag { get; set; }

        /// <summary>
        /// This method will initialize any required, and automatically-built properties for a new rule
        /// </summary>
        public void InitializeNewRule(string deviceId)
        {
            DeviceID = deviceId;
            RuleId = Guid.NewGuid().ToString();
            EnabledState = true;
            Operator = ">";
        }
    }
}
