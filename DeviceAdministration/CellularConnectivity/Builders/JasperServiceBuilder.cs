using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.billing;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.echo;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.eventplan;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.terminal;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;

namespace DeviceManagement.Infrustructure.Connectivity.Builders
{
    public static class JasperServiceBuilder
    {
        public static BillingService GetBillingService(ICredentials jasperCredentials)
        {
            return new BillingService
            {
                securityHeader = GetSecurityHeader(jasperCredentials),
                Url = "https://" + jasperCredentials.BaseUrl + "/ws/service/billing"
            };
        }

        public static TerminalService GetTerminalService(ICredentials jasperCredentials)
        {
            return new TerminalService
            {
                securityHeader = GetSecurityHeader(jasperCredentials),
                Url = "https://" + jasperCredentials.BaseUrl + "/ws/service/erminal"
            };
        }

        public static EchoService GetEchoService(ICredentials jasperCredentials)
        {
            return new EchoService
            {
                securityHeader = GetSecurityHeader(jasperCredentials),
                Url = "https://" + jasperCredentials.BaseUrl + "/ws/service/echo"
            };
        }

        public static EventPlanService GetEventPlanService(ICredentials jasperCredentials)
        {
            return new EventPlanService()
            {
                securityHeader = GetSecurityHeader(jasperCredentials),
                Url = "https://" + jasperCredentials.BaseUrl + "/ws/service/eventplan"
            };
        }

        private static SecurityHeader GetSecurityHeader(ICredentials creds)
        {
            var securityHeader = new SecurityHeader();
            securityHeader.UsernameToken.SetUserPass(creds.Username, creds.Password, PasswordOption.SendPlainText);
            return securityHeader;
        }
    }
}