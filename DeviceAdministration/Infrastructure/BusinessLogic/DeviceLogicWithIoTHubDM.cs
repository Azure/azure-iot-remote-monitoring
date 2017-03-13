using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class DeviceLogicWithIoTHubDM : DeviceLogic
    {
        private readonly IConfigurationProvider _configProvider;

        public DeviceLogicWithIoTHubDM(IIotHubRepository iotHubRepository, IDeviceRegistryCrudRepository deviceRegistryCrudRepository,
            IDeviceRegistryListRepository deviceRegistryListRepository, IVirtualDeviceStorage virtualDeviceStorage,
            ISecurityKeyGenerator securityKeyGenerator, IConfigurationProvider configProvider, IDeviceRulesLogic deviceRulesLogic,
            INameCacheLogic nameCacheLogic, IDeviceListFilterRepository deviceListFilterRepository) :
            base(iotHubRepository, deviceRegistryCrudRepository, deviceRegistryListRepository, virtualDeviceStorage, securityKeyGenerator, configProvider, deviceRulesLogic, nameCacheLogic, deviceListFilterRepository)
        {
            _configProvider = configProvider;
        }

        // Copy values from view model to twin
        // Only tags and desired properties will be put into the twin. DeviceID and reported properties could not be updated
        //
        // Reminder: Only DeviceModel.Twin will be effected. DeviceProperties and other properties of class DeviceModel will not be touched
        public override void ApplyDevicePropertyValueModels(
            DeviceModel device,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (devicePropertyValueModels == null)
            {
                throw new ArgumentNullException("devicePropertyValueModels");
            }

            foreach (var model in devicePropertyValueModels)
            {
                if (model.Name.StartsWith("tags.", StringComparison.Ordinal) || model.Name.StartsWith("properties.desired.", StringComparison.Ordinal))
                {
                    device.Twin.Set(model.Name, model.Value);
                }
            }
        }

        // Copy values from twin to view model
        // All of the tags, desired and reported properties will be copy to view model. While the reported properties will be marked as non-editable
        //
        // Reminder: Only DeviceModel.Twin and cloud service configuration 'iotHub.HostName' will be read. . DeviceProperties and other properties of class DeviceModel will not be touched        
        public override IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(
           DeviceModel device)
        {
            string hostNameValue;
            IEnumerable<DevicePropertyValueModel> propValModels;

            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var tags = device.Twin.Tags.AsEnumerableFlatten().OrderBy(pair => pair.Key).Select(pair => new DevicePropertyValueModel
            {
                DisplayOrder = 1,
                IsEditable = true,
                IsIncludedWithUnregisteredDevices = false,
                Name = FormattableString.Invariant($"tags.{pair.Key}"),
                PropertyType = GetObjectType(pair.Value.Value),
                Value = pair.Value.Value.ToString()
            });

            var desiredProperties = device.Twin.Properties.Desired.AsEnumerableFlatten().OrderBy(pair => pair.Key).Select(pair => new DevicePropertyValueModel
            {
                DisplayOrder = 2,
                IsEditable = true,
                IsIncludedWithUnregisteredDevices = false,
                Name = FormattableString.Invariant($"properties.desired.{pair.Key}"),
                PropertyType = GetObjectType(pair.Value.Value),
                Value = pair.Value.Value.ToString(),
                LastUpdatedUtc = pair.Value.LastUpdated
            });

            var reportedProperties = device.Twin.Properties.Reported.AsEnumerableFlatten().Where(pair => !SupportedMethodsHelper.IsSupportedMethodProperty(pair.Key)).OrderBy(pair => pair.Key).Select(pair => new DevicePropertyValueModel
            {
                DisplayOrder = 3,
                IsEditable = false,
                IsIncludedWithUnregisteredDevices = false,
                Name = FormattableString.Invariant($"properties.reported.{pair.Key}"),
                PropertyType = GetObjectType(pair.Value.Value),
                Value = pair.Value.Value.ToString(),
                LastUpdatedUtc = pair.Value.LastUpdated
            });

            propValModels = tags.Concat(desiredProperties).Concat(reportedProperties);

            hostNameValue = _configProvider.GetConfigurationSettingValue("iotHub.HostName");

            if (!string.IsNullOrEmpty(hostNameValue))
            {
                propValModels = propValModels.Concat(
                        new DevicePropertyValueModel[]
                        {
                            new DevicePropertyValueModel()
                            {
                                DisplayOrder = 0,
                                IsEditable = false,
                                IsIncludedWithUnregisteredDevices = true,
                                Name = "DeviceID",
                                PropertyType = PropertyType.String,
                                Value = device.DeviceProperties.DeviceID,
                            },
                            new DevicePropertyValueModel()
                            {
                                DisplayOrder = 0,
                                IsEditable = false,
                                IsIncludedWithUnregisteredDevices = true,
                                Name = "HostName",
                                PropertyType = PropertyType.String,
                                Value = hostNameValue
                            }
                        });
            }

            return propValModels;
        }

        private PropertyType GetObjectType(JValue val)
        {
            switch (val.Type)
            {
                case JTokenType.Date: return PropertyType.DateTime;
                case JTokenType.Integer: return PropertyType.Integer;
                case JTokenType.Float: return PropertyType.Real;
                default: return PropertyType.String;
            }
        }
    }
}
