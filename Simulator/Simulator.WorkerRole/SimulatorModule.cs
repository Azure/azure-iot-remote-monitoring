using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole.DataInitialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WorkerRole
{
    public sealed class SimulatorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationProvider>()
                .As<IConfigurationProvider>()
                .SingleInstance();

            builder.RegisterType<DeviceLogic>()
                .As<IDeviceLogic>();

            builder.RegisterType<DeviceRulesLogic>()
                .As<IDeviceRulesLogic>();

            builder.RegisterType<DeviceRegistryRepository>()
                .As<IDeviceRegistryCrudRepository>();

            builder.RegisterType<DeviceRegistryRepository>()
                .As<IDeviceRegistryListRepository>();

            builder.RegisterType<DeviceRulesRepository>()
                .As<IDeviceRulesRepository>();

            builder.RegisterType<IotHubRepository>()
                .As<IIotHubRepository>();

            builder.RegisterType<SecurityKeyGenerator>()
                .As<ISecurityKeyGenerator>();

            builder.RegisterType<VirtualDeviceTableStorage>()
                .As<IVirtualDeviceStorage>();

            builder.RegisterType<ActionMappingLogic>()
                .As<IActionMappingLogic>();

            builder.RegisterType<ActionMappingRepository>()
                .As<IActionMappingRepository>();

            builder.RegisterType<ActionLogic>()
                .As<IActionLogic>();

            builder.RegisterType<DataInitializer>()
                .As<IDataInitializer>();

            builder.RegisterType<ActionRepository>()
                .As<IActionRepository>();

            builder.RegisterType<DocDbRestHelper>()
                .As<IDocDbRestHelper>();
        }
    }
}
