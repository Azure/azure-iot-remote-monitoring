using System.ComponentModel.DataAnnotations;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceConfigModel
    {
        [Required]
        public string DeviceId { get; set; }

        public SecurityKeys SecurityKeys { get; set; }

        [MaxLength(1024, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "AppConfigLength")]
        public string AppConfig { get; set; }

        public string Hostname { get; set; }
    }
}