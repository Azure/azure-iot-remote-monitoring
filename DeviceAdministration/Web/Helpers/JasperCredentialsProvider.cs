using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public class JasperCredentialsProvider : ICredentialProvider
    {
        private readonly IApiRegistrationRepository _registrationRepository;

        public JasperCredentialsProvider(IApiRegistrationRepository registrationRepository)
        {
            _registrationRepository = registrationRepository;
        }

        public ICredentials Provide()
        {
            var apiRegistration = _registrationRepository.RecieveDetails();
            return new CellularCredentials()
            {
                BaseUrl = apiRegistration.BaseUrl,
                LicenceKey = apiRegistration.LicenceKey,
                Password = apiRegistration.Password,
                Username = apiRegistration.Username,
                ApiRegistrationProvider = apiRegistration.ApiRegistrationProvider
            };
        }
    }
}