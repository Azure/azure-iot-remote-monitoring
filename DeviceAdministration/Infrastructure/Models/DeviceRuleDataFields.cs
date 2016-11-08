using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public static class DeviceRuleDataFields
    {
        public static string Temperature
        { 
            get 
            { 
                return "Temperature"; 
            } 
        }

        public static string Humidity
        {
            get
            {
                return "Humidity";
            }
        }

        public static string ExternalTemperature
        {
            get { return "ExternalTemperature"; }
        }

        private static List<string> _availableDataFields = new List<string>
        {
            Temperature, Humidity, ExternalTemperature
        };

        public static List<string> GetListOfAvailableDataFields()
        {
            return _availableDataFields;
        }
    }
}
