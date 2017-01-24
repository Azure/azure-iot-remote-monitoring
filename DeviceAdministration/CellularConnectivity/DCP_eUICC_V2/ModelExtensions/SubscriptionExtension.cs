using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2
{
    // Model Class Extensions
    public partial class SubscriptionV1
    {
        public bool isEuicc()
        {
            if (ApiHandler.simulatedApiCalls)
                return true;
            else
                return (this.EuiccId != null);
        }
    }
}
