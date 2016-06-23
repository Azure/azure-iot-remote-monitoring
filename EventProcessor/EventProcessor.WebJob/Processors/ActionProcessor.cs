using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    using Generic;

    public class ActionProcessor : EventProcessor
    {
        private readonly IActionLogic _actionLogic;
        private readonly IActionMappingLogic _actionMappingLogic;
        private readonly IConfigurationProvider _configurationProvider;

        public ActionProcessor(
            IActionLogic actionLogic,
            IActionMappingLogic actionMappingLogic,
            IConfigurationProvider configurationProvider)
        {
            _actionLogic = actionLogic;
            _actionMappingLogic = actionMappingLogic;
            _configurationProvider = configurationProvider;
        }

        public override async Task ProcessItem(dynamic eventData)
        {
            if (eventData == null)
            {
                Trace.TraceWarning("Action event is null");
                return;
            }

            try
            {
                // NOTE: all column names from ASA come out as lowercase; see 
                // https://social.msdn.microsoft.com/Forums/office/en-US/c79a662b-5db1-4775-ba1a-23df1310091d/azure-table-storage-account-output-property-names-are-lowercase?forum=AzureStreamAnalytics 

                string deviceId = eventData.deviceid;
                string ruleOutput = eventData.ruleoutput;

                if (ruleOutput.Equals("AlarmTemp", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: temperature rule triggered!");
                    double tempReading = ExtractDouble(eventData.reading);

                    string tempActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(tempActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                        tempActionId,
                        deviceId,
                        "Temperature",
                        tempReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: tempActionId value is empty for temperatureRuleOutput '{0}'", ruleOutput);
                    }
                }

                if (ruleOutput.Equals("AlarmHumidity", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: humidity rule triggered!");
                    double humidityReading = ExtractDouble(eventData.reading);

                    string humidityActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(humidityActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                            humidityActionId,
                            deviceId,
                            "Humidity",
                            humidityReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: humidityActionId value is empty for humidityRuleOutput '{0}'", ruleOutput);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("ActionProcessor: exception in ProcessAction:");
                Trace.TraceError(e.ToString());
            }
        }

        private double ExtractDouble(dynamic value)
        {
            if (value == null)
            {
                Trace.TraceError("ActionProcessor: unable to parse null double value");
                return -1;
            }

            string valueAsString = value.ToString();
            return double.Parse(valueAsString, CultureInfo.CurrentCulture);
        }
    }
}
