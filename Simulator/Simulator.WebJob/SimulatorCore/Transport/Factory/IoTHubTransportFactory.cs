using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory
{
    public class IotHubTransportFactory : ITransportFactory
    {
        private ILogger _logger;
        private IConfigurationProvider _configurationProvider;
        private ITransport _transport;
        
        public IotHubTransportFactory(ILogger logger,
            IConfigurationProvider configurationProvider) : this(logger, configurationProvider, null)
        {
        }

        public IotHubTransportFactory(ILogger logger,
            IConfigurationProvider configurationProvider, ITransport transport)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
            _transport = transport;
        }

        public ITransport CreateTransport(IDevice device)
        {
            return _transport ??
                   (_transport = new IoTHubTransport(_logger, _configurationProvider, device));
        }
    }
}
