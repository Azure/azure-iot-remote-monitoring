using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class SimInformationViewModel
    {
        public string DeviceId { get; set; }
        public Terminal TerminalDevice { get; set; }
        public SessionInfo SessionInfo { get; set; }
        public string ApiRegistrationProvider { get; set;}
        public List<SimState> AvailableSimStates { get; set; }
        public List<SubscriptionPackage> AvailableSubscriptionPackages { get; set; }
    }
}