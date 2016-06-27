using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    class OperationsProcessor : EventProcessor
    {
        public override async Task ProcessItem(dynamic eventData)
        {
            // Process operations-monitoring events here
        }
    }
}
