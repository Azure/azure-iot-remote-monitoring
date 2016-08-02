using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface ICloudBlobContainerProvider
    {
        Task<CloudBlobContainer> GetCloudBlobContainerAsync();
    }
}