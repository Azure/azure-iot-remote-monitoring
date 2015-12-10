﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class SimInformationViewModel
    {
        public Terminal TerminalDevice { get; set; }
        public SessionInfo SessionInfo { get; set; }
    }
}