using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IApiRegistrationRepository
    {
        bool AmendRegistration(ApiRegistrationModel apiRegistrationModel);
        ApiRegistrationModel RecieveDetails();
        bool IsApiRegisteredInAzure();
        bool DeleteApiDetails();
    }
}
