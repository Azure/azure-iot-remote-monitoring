using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class SimInformationViewModel
    {
        public string DeviceId { get; set; }
        public Terminal TerminalDevice { get; set; }
        public SessionInfo SessionInfo { get; set; }
        public string ApiRegistrationProvider { get; set; }
        public List<SimState> AvailableSimStates { get; set; }
        public List<SubscriptionPackage> AvailableSubscriptionPackages { get; set; }
        public CellularActionUpdateResponseModel CellularActionUpdateResponse { get; set; }
        public string CurrentLocaleName { get; set; }
        public IEnumerable<string> AvailableLocaleNames { get; set; }
        public string LastServiceRequestState { get; set; }
    }
}