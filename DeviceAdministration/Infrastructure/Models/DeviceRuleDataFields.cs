using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public static class DeviceRuleDataFields
    {
        public static string Temperature = "Temperature";
        public static string Humidity = "Humidity";

        private static List<string> _availableDataFields = new List<string>
        {
            Temperature, Humidity
        };

        public static List<string> GetListOfAvailableDataFields()
        {
            return _availableDataFields;
        }
    }
}
