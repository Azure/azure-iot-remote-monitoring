using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.com.jasper.api.sms;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.eventplan;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.terminal;
using DeviceManagement.Infrustructure.Connectivity.Clients;
using DeviceManagement.Infrustructure.Connectivity.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Proxies;
using Newtonsoft.Json;

namespace JasperApiTester
{
    public class Program
    {
        private static readonly ICredentials Credentials = new CellularCredentials()
        {
            BaseUrl = "api.jasper.com",
            LicenceKey = "69009587-232b-468f-9a41-49cee2687571",
            Password = "2Audfzq*NaJOboSA",
            Username = "RiteshCC"
        };
        private static readonly string Iccid = "89302720396917145190";
        private static readonly string ProgramVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        static void Main(string[] args)
        {

            var result = TestSendSms();

            Console.WriteLine(JsonConvert.SerializeObject(result));

            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do something
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }

        public static GetAvailableEventsResponse TestGetAvailableEvents(string iccid, ICredentials credentials, string programVersion)
        {
            var eventPlanService = JasperServiceBuilder.GetEventPlanService(credentials);

            var eventPlanResult = eventPlanService.GetAvailableEvents(new GetAvailableEventsRequest()
            {
                iccid = iccid,
                messageId = Guid.NewGuid() + "-" + "0",
                version = programVersion,
                licenseKey = credentials.LicenceKey
            });

            Console.WriteLine(JsonConvert.SerializeObject(eventPlanResult));

            return eventPlanResult;
        }

        public static EditTerminalResponse TestChangeTerminalStatus(string iccid, ICredentials credentials, string programVersion)
        {
            var terminalService = JasperServiceBuilder.GetTerminalService(credentials);
            var changeSimStatusResult = terminalService.EditTerminal(new EditTerminalRequest()
            {
                iccid = iccid,
                messageId = Guid.NewGuid() + "-" + "0",
                version = programVersion,
                licenseKey = credentials.LicenceKey,
                targetValue = "DEACTIVATED_NAME",
                changeType = 3
            });

            Console.WriteLine(JsonConvert.SerializeObject(changeSimStatusResult));

            return changeSimStatusResult;
        }

        public static bool TestSendSms()
        {
            var smsService = JasperServiceBuilder.GetSmsService(Credentials);

            var sendResult = smsService.SendSMS(new SendSMSRequest()
            {
                sentToIccid = Iccid,
                messageId = Guid.NewGuid() + "-" + "0",
                version = ProgramVersion,
                messageText = "hello",
                licenseKey = Credentials.LicenceKey
            });

            Console.WriteLine(JsonConvert.SerializeObject(sendResult));

            var messageId = sendResult.smsMsgId;

            var getMessagesResult = smsService.GetModifiedSMS(new GetModifiedSMSRequest()
            {
                messageId = Guid.NewGuid() + "-" + "0",
                version = ProgramVersion,
                licenseKey = Credentials.LicenceKey,
                fromDate = DateTime.Today.AddDays(-1),
                toDate = DateTime.Today.AddDays(1)
            });

            Console.WriteLine(JsonConvert.SerializeObject(getMessagesResult));

            var success = getMessagesResult.smsMsgIds.ToList().Contains(messageId);
            return success;
        }

    }
}
