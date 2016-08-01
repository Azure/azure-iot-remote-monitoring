using System;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

/*
//outgoing SoapHeader will look something like this
<wsse:Security soapenv:mustUnderstand="1" xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
 <wsse:UsernameToken wsu:Id="UsernameToken-8164742" xmlns:wsu="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
    <wsse:Username>username</wsse:Username>
    <wsse:Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText">password</wsse:Password>
 </wsse:UsernameToken>
</wsse:Security>

//add public member to the auto generated Web Reference proxy, say TerminalService
public com.jaspersystems.api.SecurityHeader securityHeader;

//add to WebMethod that will be called on auto generated Web Reference proxy, say TerminalService.GetModifiedTerminals(request)
[System.Web.Services.Protocols.SoapHeaderAttribute( "securityHeader")]

//change client code to set UsernameToken info
com.jaspersystems.api.securityHeader = new com.jaspersystems.api.SecurityHeader();
com.jaspersystems.api.securityHeader.UsernameToken.SetUserPass(username, password, PasswordOption.SendPlainText);

*/

namespace DeviceManagement.Infrustructure.Connectivity.Models.Security
{
    [XmlRoot(Namespace = NsConstants.wsse, ElementName = "Security")]
    public class SecurityHeader : SoapHeader
    {
        [XmlElement(Namespace = NsConstants.wsse)] public UsernameToken UsernameToken;

        public SecurityHeader()
        {
            MustUnderstand = true;
            UsernameToken = new UsernameToken();
        }
    }

    public class UsernameToken
    {
        [XmlAttribute(Namespace = NsConstants.wsu)] public string Id;

        //optional
        [XmlElement(Namespace = NsConstants.wsse)] public string Username;

        [XmlElement(Namespace = NsConstants.wsse)] public Password Password;

        //required

        public void SetUserPass(string username, string password, PasswordOption passType)
        {
            Username = username;

            var g = Guid.NewGuid();
            Id = "SecurityToken-" + g.ToString("D");

            if (passType == PasswordOption.SendNone)
            {
                Password = null;
            }

            Password = new Password();

            if (passType == PasswordOption.SendPlainText)
            {
                Password.Type = NsConstants.passwdType;
                Password.Text = password;
            }
        }
    }


    public class Password
    {
        [XmlText] public string Text;

        [XmlAttribute] public string Type;
    }

    public class NsConstants
    {
        public const string wsse = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        public const string wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

        public const string passwdType =
            "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";
    }

    public enum PasswordOption
    {
        SendNone,
        SendPlainText
    }
}