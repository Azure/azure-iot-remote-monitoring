using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.DataInitialization
{
    /// <summary>
    /// Represents component to create initial data for the system
    /// </summary>
    public interface IDataInitializer
    {
        void CreateInitialDataIfNeeded();
    }
}
