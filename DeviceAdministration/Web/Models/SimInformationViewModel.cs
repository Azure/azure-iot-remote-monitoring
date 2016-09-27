using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class SimInformationViewModel
    {
        public Terminal TerminalDevice { get; set; }
        public SessionInfo SessionInfo { get; set; }
        public string ApiRegistrationProvider { get; set;}
        public List<SimStateModel> AvailableSimStates { get; set; }
        public List<SubscriptionPackageModel> AvailableSubscriptionPackages { get; set; }
    }
}