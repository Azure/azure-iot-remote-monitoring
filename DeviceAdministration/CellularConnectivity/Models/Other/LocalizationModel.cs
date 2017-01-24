using System.Collections.Generic;

namespace DeviceManagement.Infrustructure.Connectivity.Models.Other
{
    public class LocalizationModel
    {
        public string Context { get; set; }
        public string CurrentLocale { get; set; }
        public IEnumerable<string> AvailableLocales { get; set; }
    }
}
