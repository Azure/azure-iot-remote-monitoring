using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations
{
    public class ConfigurationProvider : IConfigurationProvider, IDisposable
    {
        readonly Dictionary<string, string> configuration = new Dictionary<string, string>();
        EnvironmentDescription environment = null;
        const string ConfigToken = "config:";
        bool _disposed = false;

        public string GetConfigurationSettingValue(string configurationSettingName)
        {
            return this.GetConfigurationSettingValueOrDefault(configurationSettingName, string.Empty);
        }

        public string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue)
        {
            if (!this.configuration.ContainsKey(configurationSettingName))
            {
                string configValue = CloudConfigurationManager.GetSetting(configurationSettingName);
                bool isEmulated = Environment.CommandLine.Contains("iisexpress.exe") ||
                    Environment.CommandLine.Contains("w3wp.exe") ||
                    Process.GetCurrentProcess().GetAncestorNames().Contains("devenv"); // if debugging in VS, devenv will be the parent

                if (isEmulated && (configValue != null && configValue.StartsWith(ConfigToken, StringComparison.OrdinalIgnoreCase)))
                {
                    if (environment == null)
                    {
                        LoadEnvironmentConfig();
                    }

                    configValue = environment.GetSetting(
                        configValue.Substring(configValue.IndexOf(ConfigToken, StringComparison.Ordinal) + ConfigToken.Length));
                }

                try
                {
                    this.configuration.Add(configurationSettingName, configValue);
                }
                catch (ArgumentException)
                {
                    // at this point, this key has already been added on a different
                    // thread, so we're fine to continue
                }
            }

            return this.configuration[configurationSettingName] ?? defaultValue;
        }

        void LoadEnvironmentConfig()
        {
            var executingPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            // Check for build_output
            int buildLocation = executingPath.IndexOf("Build_Output", StringComparison.OrdinalIgnoreCase);
            if (buildLocation >= 0)
            {
                string fileName = executingPath.Substring(0, buildLocation) + "local.config.user";
                if (File.Exists(fileName))
                {
                    this.environment = new EnvironmentDescription(fileName);
                    return;
                }
            }

            // Web roles run in there app dir so look relative
            int location = executingPath.IndexOf("Web\\bin", StringComparison.OrdinalIgnoreCase);

            if (location == -1)
            {
                location = executingPath.IndexOf("WebJob\\bin", StringComparison.OrdinalIgnoreCase);
            }
            if (location >= 0)
            {
                string fileName = executingPath.Substring(0, location) + "..\\local.config.user";
                if (File.Exists(fileName))
                {
                    this.environment = new EnvironmentDescription(fileName);
                    return;
                }
            }

            throw new ArgumentException("Unable to locate local.config.user file.  Make sure you have run 'build.cmd local'.");
        }

        #region IDispose 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (environment != null)
                {
                    environment.Dispose();
                }
            }

            _disposed = true;
        }

        ~ConfigurationProvider()
        {
            Dispose(false);
        }
        #endregion
    }
}
