using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations
{
    public class EnvironmentDescription : IDisposable
    {
        bool isDisposed = false;
        XmlDocument document = null;
        XPathNavigator navigator = null;
        string fileName = null;
        int updatedValuesCount = 0;
        const string ValueAttributeName = "value";
        const string SettingXpath = "//setting[@name='{0}']";

        public EnvironmentDescription(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            this.fileName = fileName;
            this.document = new XmlDocument();
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                this.document.Load(reader);
            }
            this.navigator = this.document.CreateNavigator();
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.isDisposed = true;
                if (this.updatedValuesCount > 0)
                {
                    this.document.Save(this.fileName);
                    Console.Out.WriteLine("Successfully updated {0} mapping(s) in {1}", this.updatedValuesCount, Path.GetFileName(this.fileName).Split('.')[0]);
                }
            }
        }

        public bool SettingExists(string settingName)
        {
            return !string.IsNullOrEmpty(this.GetSetting(settingName, false));
        }

        public string GetSetting(string settingName, bool errorOnNull = true)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new ArgumentNullException("settingName");
            }

            string result = string.Empty;
            XmlNode node = this.GetSettingNode(settingName.Trim());
            if (node != null)
            {
                result = node.Attributes[ValueAttributeName].Value;
            }
            else
            {
                if (errorOnNull)
                {
                    throw new ArgumentException("{0} was not found".FormatInvariant(settingName));
                }
            }
            return result;
        }

        XmlNode GetSettingNode(string settingName)
        {
            string xpath = SettingXpath.FormatInvariant(settingName);
            return this.document.SelectSingleNode(xpath);
        }

        public bool SetSetting(string settingName, string settingValue)
        {
            return this.SetSetting(this.GetSettingNode(settingName), settingValue);
        }

        public bool SetSetting(IXPathNavigable node, string settingValue)
        {
            if (node != null)
            {
                ((XmlNode)node).Attributes[ValueAttributeName].Value = settingValue;
                this.updatedValuesCount++;
                return true;
            }
            return false;
        }
    }
}
