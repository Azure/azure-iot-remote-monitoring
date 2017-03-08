using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Owin;
using System.Reflection;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    public partial class Startup
    {
        public void ConfigureAutofac(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            // register the class that sets up bindings between interfaces and implementation
            builder.RegisterModule(new WebAutofacModule());

            // register configuration provider
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>();

            // register Autofac w/ the MVC application
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register the WebAPI controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();

            // Setup Autofac dependency resolver for MVC
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Setup Autofac dependency resolver for WebAPI
            Startup.HttpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            // 1.  Register the Autofac middleware 
            // 2.  Register Autofac Web API middleware,
            // 3.  Register the standard Web API middleware (this call is made in the Startup.WebApi.cs)
            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(Startup.HttpConfiguration);
        }
    }

    public sealed class WebAutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //Logic
            builder.RegisterType<KeyLogic>().As<IKeyLogic>();
            builder.RegisterType<DeviceLogicWithIoTHubDM>().As<IDeviceLogic>();
            builder.RegisterType<DeviceRulesLogic>().As<IDeviceRulesLogic>();
            builder.RegisterType<DeviceTypeLogic>().As<IDeviceTypeLogic>();
            builder.RegisterType<SecurityKeyGenerator>().As<ISecurityKeyGenerator>();
            builder.RegisterType<ActionMappingLogic>().As<IActionMappingLogic>();
            builder.RegisterType<ActionLogic>().As<IActionLogic>();

            builder.RegisterInstance(CommandParameterTypeLogic.Instance).As<ICommandParameterTypeLogic>();
            builder.RegisterType<DeviceTelemetryLogic>().As<IDeviceTelemetryLogic>();

            builder.RegisterType<AlertsLogic>().As<IAlertsLogic>();
            builder.RegisterType<NameCacheLogic>().As<INameCacheLogic>();
            builder.RegisterType<FilterLogic>().As<IFilterLogic>();
            builder.RegisterType<UserSettingsLogic>().As<IUserSettingsLogic>();

            //Repositories
            builder.RegisterType<IotHubRepository>().As<IIotHubRepository>();
            builder.RegisterType<IoTHubDeviceManager>().As<IIoTHubDeviceManager>();
            builder.RegisterType<DeviceRegistryRepositoryWithIoTHubDM>().As<IDeviceRegistryListRepository>();
            builder.RegisterType<DeviceRegistryRepositoryWithIoTHubDM>().As<IDeviceRegistryCrudRepository>();
            builder.RegisterType<DeviceRulesRepository>().As<IDeviceRulesRepository>();
            builder.RegisterType<SampleDeviceTypeRepository>().As<IDeviceTypeRepository>();
            builder.RegisterType<VirtualDeviceTableStorage>().As<IVirtualDeviceStorage>();
            builder.RegisterType<ActionMappingRepository>().As<IActionMappingRepository>();
            builder.RegisterType<ActionRepository>().As<IActionRepository>();
            builder.RegisterType<DeviceTelemetryRepository>().As<IDeviceTelemetryRepository>();
            builder.RegisterType<AlertsRepository>().As<IAlertsRepository>();
            builder.RegisterType<UserSettingsRepository>().As<IUserSettingsRepository>();
            builder.RegisterType<ApiRegistrationRepository>().As<IApiRegistrationRepository>();
            builder.RegisterType<IccidRepository>().As<IIccidRepository>();
            builder.RegisterType<JasperCredentialsProvider>().As<ICredentialProvider>();
            builder.RegisterType<ExternalCellularService>().As<IExternalCellularService>();
            builder.RegisterType<CellularExtensions>().As<ICellularExtensions>();
            builder.RegisterType<AzureTableStorageClientFactory>().As<IAzureTableStorageClientFactory>();
            builder.RegisterType<BlobStorageClientFactory>().As<IBlobStorageClientFactory>();
            builder.RegisterGeneric(typeof(DocumentDBClient<>)).As(typeof(IDocumentDBClient<>));
            builder.RegisterType<NameCacheRepository>().As<INameCacheRepository>();
            builder.RegisterType<JobRepository>().As<IJobRepository>();
            builder.RegisterType<DeviceListFilterRepository>().As<IDeviceListFilterRepository>();
            builder.RegisterType<UserSettingsRepository>().As<IUserSettingsRepository>();
            builder.RegisterType<DeviceIconRepository>().As<IDeviceIconRepository>();
        }
    }
}
