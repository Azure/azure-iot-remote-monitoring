using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class IccidRepository: IIccidRepository
    {
        private const string ICCID_TABLE_NAME = "IccidTable";
        private readonly IAzureTableStorageClient _azureTableStorageClient;

        public IccidRepository(IConfigurationProvider configProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(configProvider.GetConfigurationSettingValue("device.StorageConnectionString"), ICCID_TABLE_NAME);
        }

        public bool AddIccid(Iccid iccid)
        {
            throw new NotImplementedException();
        }

        public bool AddIccids(List<Iccid> iccids)
        {
            throw new NotImplementedException();
        }

        public bool RemoveAllIccids()
        {
            throw new NotImplementedException();
        }

        public bool GetIccids()
        {
            throw new NotImplementedException();
        }
    }
}
