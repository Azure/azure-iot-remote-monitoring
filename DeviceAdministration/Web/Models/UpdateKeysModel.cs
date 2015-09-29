using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class UpdateKeysModel
    {
        public SecurityKeys DeviceKeys { get; set; }
        public string KeyValue { get; set; }

        [Required]
        public string KeyType { get; set; }
        public string DeviceId { get; set; }
    }
}