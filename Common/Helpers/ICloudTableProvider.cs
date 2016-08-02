using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface ICloudTableProvider
    {
        CloudTable GetCloudTable();
        Task<CloudTable> GetCloudTableAsync();
    }
}