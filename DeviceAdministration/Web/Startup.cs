using System;
using System.Web.Http;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Owin;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    public partial class Startup
    {
        public static HttpConfiguration HttpConfiguration { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            Startup.HttpConfiguration = new System.Web.Http.HttpConfiguration();
            ConfigurationProvider configProvider = new ConfigurationProvider();

            ConfigureAuth(app, configProvider);
            ConfigureAutofac(app);

            // WebAPI call must come after Autofac
            // Autofac hooks into the HttpConfiguration settings
            ConfigureWebApi(app);

            ConfigureJson(app);

            TimeSpanExtension.Units = new TimeSpanExtension.TimeUnit[]
            {
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.FromDays(365),
                    Singular = Strings.YearSingular,
                    Plural = Strings.YearPlural
                },
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.FromDays(31),
                    Singular = Strings.MonthSingular,
                    Plural = Strings.MonthPlural
                },
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.FromDays(7),
                    Singular = Strings.WeekSingular,
                    Plural = Strings.WeekPlural
                },
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.FromDays(1),
                    Singular = Strings.DaySingular,
                    Plural = Strings.DayPlural
                },
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.FromHours(1),
                    Singular = Strings.HourSingular,
                    Plural = Strings.HourPlural
                },
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.FromMinutes(1),
                    Singular = Strings.MinuteSingular,
                    Plural = Strings.MinutePlural
                },
                new TimeSpanExtension.TimeUnit
                {
                    Length = TimeSpan.Zero,
                    Singular = Strings.TimeSpanMin,
                    Plural = Strings.TimeSpanMin
                }
            };
        }
    }
}