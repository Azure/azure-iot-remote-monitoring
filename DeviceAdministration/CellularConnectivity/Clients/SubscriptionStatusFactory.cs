using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.EricssonSubscriptionService;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;

namespace DeviceManagement.Infrustructure.Connectivity.Clients
{
    public static class SubscriptionStatusFactory
    {
        public static subscriptionStatusRequest CreateEricssonSubscriptionStatusRequestEnum(string statusString)
        {
            subscriptionStatusRequest result;
            var statusEnum = Enum.TryParse(statusString, out result);
            if (!statusEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(statusString));
            }
            return result;
        }

        public static SimState CreateSimStateFromEricssonEnum(
            subscriptionStatusRequest ericssonSubscriptionStatusRequestEnum)
        {
            return new SimState()
            {
                Name = ericssonSubscriptionStatusRequestEnum.ToString(),
                IsActive = false
            };
        }
    }
}
