using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.eventplan;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.terminal;
using DeviceManagement.Infrustructure.Connectivity.Clients;
using DeviceManagement.Infrustructure.Connectivity.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Proxies;

namespace JasperApiTester
{
    public class Program
    {


        static void Main(string[] args)
        {
            string iccid = "89302720396917145190";
            ICredentials credentials = new CellularCredentials()
            {
                BaseUrl = "api.jasper.com",
                LicenceKey = "69009587-232b-468f-9a41-49cee2687571",
                Password = "2Audfzq*NaJOboSA",
                Username = "RiteshCC"
            };
            string PROGRAM_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //var terminalService = JasperServiceBuilder.GetTerminalService(credentials);
            //var result = terminalService.EditTerminal(new EditTerminalRequest()
            //{
            //    iccid = iccid,
            //    messageId = Guid.NewGuid() + "-" + "0",
            //    version = PROGRAM_VERSION,
            //    licenseKey = credentials.LicenceKey,
            //    targetValue = "DEACTIVATED_NAME",
            //    changeType = 3
            //});


            var eventPlanService = JasperServiceBuilder.GetEventPlanService(credentials);
            var result = eventPlanService.GetTerminalEvents(new GetTerminalEventsRequest()
            {
                iccid = iccid,
                messageId = Guid.NewGuid() + "-" + "0",
                version = PROGRAM_VERSION,
                licenseKey = credentials.LicenceKey
            });

            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do something
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
