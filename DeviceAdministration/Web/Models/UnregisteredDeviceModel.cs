using System;
using System.ComponentModel.DataAnnotations;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class UnregisteredDeviceModel
    {
        public const int MinimumDeviceIdLength = 2;
        public const int MaximumDeviceIdLength = 128;

        [Required]
        [StringLength(MaximumDeviceIdLength, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "DeviceIDMustBeBetween2And128Characters", MinimumLength = MinimumDeviceIdLength)]
        [RegularExpression("^[a-zA-Z0-9-:.+%_#*?!(),=@;$']+$",
            ErrorMessageResourceType = typeof(Strings),
            ErrorMessageResourceName = "DeviceIdContainsLettersNumbersSpecialCharacters")]
        public String DeviceId { get; set; }

        [Required]
        public DeviceType DeviceType { get; set; }

        public bool IsDeviceIdSystemGenerated { get; set; }

        public bool IsDeviceIdUnique { get; set; }

        public string Iccid { get; set; }
    }
}