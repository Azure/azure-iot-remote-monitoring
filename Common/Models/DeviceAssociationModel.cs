using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class DeviceAssociationModel
    {
        public string ApiRegistrationProvider { get; set; }
        public bool HasRegistration { get; set; }
        public IEnumerable<string> UnassignedIccidList { get; set; }
        public IEnumerable<string> UnassignedDeviceIds { get; set; }
    }
}
