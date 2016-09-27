using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class SubscriptionPackageModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
