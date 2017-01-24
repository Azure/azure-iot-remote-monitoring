using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IIccidRepository
    {
        bool AddIccid(Iccid iccid, string providerName);
        bool AddIccids(List<Iccid> iccids, string providerName);
        bool DeleteIccidTableEntity(IccidTableEntity iccidTableEntity);
        bool DeleteAllIccids();
        IList<Iccid> GetIccids();
        string GetLastSetLocaleServiceRequestId(string iccid);
        void SetLastSetLocaleServiceRequestId(string iccid, string serviceRequestId);
    }
}
