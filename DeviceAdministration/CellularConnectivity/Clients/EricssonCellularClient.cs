using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.EricssonApiService;
using DeviceManagement.Infrustructure.Connectivity.EricssonSubscriptionService;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Clients
{
    public class EricssonCellularClient
    {
        private readonly ICredentialProvider _credentialProvider;
        public EricssonCellularClient(ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }


        public bool ValidateCredentials()
        {
            return true;
            var isValid = false;          

            //simple check - if it throws an exception then the creds are no good
            //todo: catch the correct error code
            try
            {
                var apiStatusClient = EricssonServiceBuilder.GetApiStatusClient(_credentialProvider.Provide());
                apiStatusClient.echo(new echo());
                isValid = true;
            }
            catch(Exception ex)
            {
                isValid = false;
            }    
            return isValid;
        }

        public List<Iccid> GetTerminals()
        {
            //todo : Stubbed out with real ICCIDs from test account until ericsson sort an endpoint to do what we need
            return new List<Iccid>()
            {
                new Iccid("89460800000105696001"), 
                new Iccid("89460800000105696050"), 
                new Iccid("89460800000105696068"), 
                new Iccid("89460800000105696100"), 
                new Iccid("89460800000105696159"), 
            };
        }

        public Terminal GetSingleTerminalDetails(Iccid iccid)
        {
            var terminal = new Terminal();
            try
            {
                var subManClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());
                var response = subManClient.QuerySimResource_v2(new QuerySimResource_v2() { resource = new resource() { id = iccid.Id , type = "icc"} });

                //check it even exists
                if(response.simResource.Length <=0) return terminal;
                var sim = response.simResource.First();

                terminal.Iccid = new Iccid(sim.icc);
                terminal.Imei = new Imei(sim.imei);
                terminal.Imsi = new Imsi(sim.imsi);

            }
            catch (Exception)
            {
                return terminal;
            }
            return terminal;
        }

        public List<SessionInfo> GetSingleSessionInfo(Iccid iccid)
        {
            throw new NotImplementedException();
        }



    }
}
