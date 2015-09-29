using System.Collections.Generic;
using System.Web.Mvc;
using GlobalResources;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class EditDeviceRuleModel
    {
        public string RuleId { get; set; }
        public bool EnabledState { get; set; }
        public string DeviceID { get; set; }
        public string DataField { get; set; }
        public string Operator { get; set; }
        public string Threshold { get; set; }
        public string RuleOutput { get; set; }
        public string Etag { get; set; }
        public List<SelectListItem> AvailableDataFields { get; set; }
        public List<SelectListItem> AvailableOperators { get; set; }
        public List<SelectListItem> AvailableRuleOutputs { get; set; }

        public string CheckForErrorMessage()
        {
            double outDouble = 0;
            if (string.IsNullOrWhiteSpace(Threshold) || !double.TryParse(Threshold, out outDouble))
            {
                return Strings.ThresholdFormatError;
            }

            return null;
        }
    }
}