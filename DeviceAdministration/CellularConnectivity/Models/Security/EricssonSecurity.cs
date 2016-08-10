using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DeviceManagement.Infrustructure.Connectivity.Models.Security
{
    [Serializable]
    [DataContract(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")] // This object serialize specific namespace
    public class EricssonSecurity
    {
        [DataMember] // This object serialize without namespace
        public EricssonUsernameToken UsernameToken;
    }


    public class EricssonUsernameToken : IXmlSerializable
    {

        public string Username
        {
            get; set;
        }
        public string Password
        {
            get; set;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();

            Username = reader.ReadElementString("Username");
            reader.ReadStartElement();

            Password = reader.ReadElementString("Password");
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Username", Username);
            writer.WriteElementString("Password", Password);
        }
    }
}
