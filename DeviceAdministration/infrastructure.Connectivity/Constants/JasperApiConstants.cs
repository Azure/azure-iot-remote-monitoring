using System.Reflection;

namespace DeviceManagement.Infrustructure.Connectivity.Constants
{
    public static class JasperApiConstants
    {
        public const int MESSAGE_ID = 0;
        public static string PROGRAM_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}