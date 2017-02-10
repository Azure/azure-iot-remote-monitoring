using System.Reflection;

namespace DeviceManagement.Infrustructure.Connectivity.Constants
{
    internal static class JasperApiConstants
    {
        public const int MESSAGE_ID = 0;
        public static string PROGRAM_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        internal class TerminalStates
        {
            public const string ACTIVATED_CODE = "ACTIVATED_NAME";
            public const string ACTIVATED_LABEL = "Activated";
            public const string DEACTIVATED_CODE = "DEACTIVATED_NAME";
            public const string DEACTIVATED_LABEL = "Deactivated";
            public const string ACTIVATION_READY_CODE = "ACTIVATION_READY_NAME";
            public const string ACTIVATION_READY_LABEL = "Activation Ready";
            public const string INVENTORY_LABEL = "Inventory";
            public const string INVENTORY_CODE = "INVENTORY_NAME";
            public const string RETIRED_LABEL = "Retired";
            public const string RETIRED_CODE = "RETIRED_NAME"; 

        }
    }
}