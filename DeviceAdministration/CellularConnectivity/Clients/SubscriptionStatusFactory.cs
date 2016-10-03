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
            subscriptionStatus targetStatus;
            var statusEnumParsed = Enum.TryParse(statusString, out targetStatus);
            if (!statusEnumParsed)
            {
                throw new ArgumentOutOfRangeException(nameof(statusString));
            }
            var result = CreateSubscriptionStatusRequest(targetStatus);
            return result;
        }

        public static subscriptionStatusRequest CreateSubscriptionStatusRequest(subscriptionStatus targetStatus)
        {
            switch (targetStatus)
            {
                case subscriptionStatus.Active:
                    {
                        return subscriptionStatusRequest.Activate;
                    }
                case subscriptionStatus.Pause:
                    {
                        return subscriptionStatusRequest.Pause;
                    }
                case subscriptionStatus.Terminated:
                    {
                        return subscriptionStatusRequest.Terminate;
                    }
                case subscriptionStatus.Deactivated:
                    {
                        return subscriptionStatusRequest.Deactivate;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }
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
