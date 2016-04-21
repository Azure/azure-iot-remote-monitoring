using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    using StringPair = KeyValuePair<string, string>;

    /// <summary>
    /// A view model for the Dashboard control.
    /// </summary>
    public class DashboardModel
    {
        /// <summary>
        /// Initializes a new instance of the DashboardModel class.
        /// </summary>
        public DashboardModel()
        {
            this.DeviceIdsForDropdown = new List<StringPair>();
        }

        /// <summary>
        /// Gets a list of ID pairs for populating the Device selection drop 
        /// down list.
        /// </summary>
        public List<StringPair> DeviceIdsForDropdown
        {
            get;
            private set;
        }

        public string MapApiQueryKey { get; set; }

        public string Username { get; set; }

        public string AppInsightsKey { get; set; }

        public IEnumerable<LanguageModel> AvailableLanguages { get; set; }

        public string CurrentLanguageNameIso { get; set; }

        public string CurrentLanguageName { get; set; }

        public string CurrentLanguageTextDirection { get; set; }
    }
}