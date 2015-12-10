using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class ApiRegistrationModel
    {
        public string BaseUrl { get; set; }
        public string LicenceKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}