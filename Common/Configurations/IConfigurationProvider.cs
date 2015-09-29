namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations
{
    public interface IConfigurationProvider
    {
        string GetConfigurationSettingValue(string configurationSettingName);
        string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue);
    }
}
