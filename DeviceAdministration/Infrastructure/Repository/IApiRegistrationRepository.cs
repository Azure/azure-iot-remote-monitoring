using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

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
