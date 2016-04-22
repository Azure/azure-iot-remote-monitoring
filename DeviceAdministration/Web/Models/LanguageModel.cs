// ---------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public sealed class LanguageModel
    {
        public string Name { get; set; }

        public string CultureName { get; set; }

        public bool IsCurrent { get; set; }
    }
}