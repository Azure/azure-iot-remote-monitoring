using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    public static class ApiRegistrationProviderTypeExtensions
    {
        public static DeviceManagement.Infrustructure.Connectivity.Models.Enums.ApiRegistrationProviderType ConvertToExternalEnum(this ApiRegistrationProviderType? providerType)
        {
            if (!providerType.HasValue) throw new IndexOutOfRangeException($"Could not convert {providerType}.");
            switch (providerType)
            {
                case ApiRegistrationProviderType.Jasper:
                    {
                        return DeviceManagement.Infrustructure.Connectivity.Models.Enums.ApiRegistrationProviderType.Jasper;
                    }
                case ApiRegistrationProviderType.Ericsson:
                    {
                        return DeviceManagement.Infrustructure.Connectivity.Models.Enums.ApiRegistrationProviderType.Ericsson;
                    }
                default:
                    throw new IndexOutOfRangeException($"Could not convert {providerType}.");
            }
        }

        public static ApiRegistrationProviderType? ConvertFromExternalEnum(this DeviceManagement.Infrustructure.Connectivity.Models.Enums.ApiRegistrationProviderType? providerType)
        {
            if (!providerType.HasValue) throw new IndexOutOfRangeException($"Could not convert {providerType}.");
            switch (providerType)
            {
                case DeviceManagement.Infrustructure.Connectivity.Models.Enums.ApiRegistrationProviderType.Jasper:
                    {
                        return ApiRegistrationProviderType.Jasper;
                    }
                case DeviceManagement.Infrustructure.Connectivity.Models.Enums.ApiRegistrationProviderType.Ericsson:
                    {
                        return ApiRegistrationProviderType.Ericsson;
                    }
                default:
                    throw new IndexOutOfRangeException($"Could not convert {providerType}.");
            }
        }
    }
}
