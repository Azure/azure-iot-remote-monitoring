using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.DataInitialization
{
    public class DataInitializer : IDataInitializer
    {
        private readonly IActionMappingLogic _actionMappingLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IDeviceRulesLogic _deviceRulesLogic;

        public DataInitializer(
            IActionMappingLogic actionMappingLogic,
            IDeviceLogic deviceLogic,
            IDeviceRulesLogic deviceRulesLogic)
        {
            if (actionMappingLogic == null)
            {
                throw new ArgumentNullException("actionMappingLogic");
            }

            if (deviceLogic == null)
            {
                throw new ArgumentNullException("deviceLogic");
            }

            if (deviceRulesLogic == null)
            {
                throw new ArgumentNullException("deviceRulesLogic");
            }

            _actionMappingLogic = actionMappingLogic;
            _deviceLogic = deviceLogic;
            _deviceRulesLogic = deviceRulesLogic;
        }

        public void CreateInitialDataIfNeeded()
        {
            try
            {
                bool initializationNeeded = false;

                // only create default data if the action mappings are missing

                // check if action mappings are there
                Task<bool>.Run(async () => initializationNeeded = await _actionMappingLogic.IsInitializationNeededAsync()).Wait();

                if (!initializationNeeded)
                {
                    Trace.TraceInformation("No initial data needed.");
                    return;
                }

                Trace.TraceInformation("Beginning initial data creation...");

                List<string> bootstrappedDevices = null;

                // 1) create default devices
                Task.Run(async () => bootstrappedDevices = await _deviceLogic.BootstrapDefaultDevices()).Wait();

                // 2) create default rules
                Task.Run(() => _deviceRulesLogic.BootstrapDefaultRulesAsync(bootstrappedDevices)).Wait();

                // 3) create action mappings (do this last to ensure that we'll try to 
                //    recreate if any of the above throws)
                _actionMappingLogic.InitializeDataIfNecessaryAsync();

                Trace.TraceInformation("Initial data creation completed.");
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to create initial default data: {0}", ex.ToString());
            }
        }

    }
}
