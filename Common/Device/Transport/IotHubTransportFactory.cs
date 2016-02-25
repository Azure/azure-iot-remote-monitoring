using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Logging;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport
{
    public class IotHubTransportFactory : ITransportFactory
    {
        private ISerializer _serializer;
        private ILogger _logger;
        private IConfigurationProvider _configurationProvider;

        public IotHubTransportFactory(ISerializer serializer, ILogger logger, IConfigurationProvider configurationProvider)
        {
            _serializer = serializer;
            _logger = logger;
            _configurationProvider = configurationProvider;
        }

        public ITransport CreateTransport(IDevice device)
        {
            return new IoTHubTransport(_serializer, _logger, _configurationProvider, device);
        }
    }
}
