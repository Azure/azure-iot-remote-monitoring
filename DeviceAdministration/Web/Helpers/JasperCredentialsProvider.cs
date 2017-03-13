using System;
using DeviceManagement.Infrustructure.Connectivity.Models.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Jasper;
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
            switch (apiRegistration.ApiRegistrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    return new JasperCredentials()
                    {
                        BaseUrl = apiRegistration.BaseUrl,
                        LicenceKey = apiRegistration.LicenceKey,
                        Password = apiRegistration.Password,
                        Username = apiRegistration.Username,
                        ApiRegistrationProvider = apiRegistration.ApiRegistrationProvider
                    };
                case ApiRegistrationProviderTypes.Ericsson:
                    return new EricssonCredentials()
                    {
                        BaseUrl = apiRegistration.BaseUrl,
                        LicenceKey = apiRegistration.LicenceKey,
                        Password = apiRegistration.Password,
                        Username = apiRegistration.Username,
                        ApiRegistrationProvider = apiRegistration.ApiRegistrationProvider,
                        EnterpriseSenderNumber = apiRegistration.EnterpriseSenderNumber,
                        RegistrationID = apiRegistration.RegistrationID,
                        SmsEndpointBaseUrl = apiRegistration.SmsEndpointBaseUrl
                    };
                default:
                    throw new IndexOutOfRangeException(FormattableString.Invariant($"Could not find a service for '{apiRegistration.ApiRegistrationProvider}' provider"));
            }

        }
    }
}