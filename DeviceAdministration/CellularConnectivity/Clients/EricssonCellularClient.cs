using System;
using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.EricssonApiService;
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
            var isValid = false;          
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
            //todo : Stubbed out with real ICCIDs from test account
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
            throw new NotImplementedException();
        }

        public List<SessionInfo> GetSingleSessionInfo(Iccid iccid)
        {
            throw new NotImplementedException();
        }
    }
}
