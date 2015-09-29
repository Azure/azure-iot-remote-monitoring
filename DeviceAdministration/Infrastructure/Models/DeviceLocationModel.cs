using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceLocationModel
    {
        public string DeviceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
