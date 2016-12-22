using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.DataInitialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob
{
    public sealed class SimulatorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>().SingleInstance();
            builder.RegisterType<DeviceLogicWithIoTHubDM>().As<IDeviceLogic>();
            builder.RegisterType<IotHubRepository>().As<IIotHubRepository>();
            builder.RegisterType<IoTHubDeviceManager>().As<IIoTHubDeviceManager>();
            builder.RegisterType<DeviceRulesLogic>().As<IDeviceRulesLogic>();
            builder.RegisterType<DeviceRegistryRepositoryWithIoTHubDM>().As<IDeviceRegistryCrudRepository>();
            builder.RegisterType<DeviceRegistryRepositoryWithIoTHubDM>().As<IDeviceRegistryListRepository>();
            builder.RegisterType<DeviceRulesRepository>().As<IDeviceRulesRepository>();
            builder.RegisterType<SecurityKeyGenerator>().As<ISecurityKeyGenerator>();
            builder.RegisterType<VirtualDeviceTableStorage>().As<IVirtualDeviceStorage>();
            builder.RegisterType<ActionMappingLogic>().As<IActionMappingLogic>();
            builder.RegisterType<ActionMappingRepository>().As<IActionMappingRepository>();
            builder.RegisterType<ActionLogic>().As<IActionLogic>();
            builder.RegisterType<DataInitializer>().As<IDataInitializer>();
            builder.RegisterType<ActionRepository>().As<IActionRepository>();
            builder.RegisterType<AzureTableStorageClientFactory>().As<IAzureTableStorageClientFactory>();
            builder.RegisterType<BlobStorageClientFactory>().As<IBlobStorageClientFactory>();
            builder.RegisterGeneric(typeof(DocumentDBClient<>)).As(typeof(IDocumentDBClient<>));
            builder.RegisterType<NameCacheLogic>().As<INameCacheLogic>();
            builder.RegisterType<NameCacheRepository>().As<INameCacheRepository>();
            builder.RegisterType<DeviceListFilterRepository>().As<IDeviceListFilterRepository>();
        }
    }
}
