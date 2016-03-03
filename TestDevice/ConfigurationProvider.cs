using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDevice
{
    internal sealed class ConfigurationProvider : IConfigurationProvider
    {
        public string GetConfigurationSettingValue(string configurationSettingName)
        {
            switch(configurationSettingName)
            {
                case "ObjectTypePrefix":
                    return "";
                default:
                    throw new NotImplementedException();
            }
        }

        public string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue)
        {
            switch (configurationSettingName)
            {
                case "ObjectTypePrefix":
                    return "";
                default:
                    return defaultValue;
            }
        }
    }
}
