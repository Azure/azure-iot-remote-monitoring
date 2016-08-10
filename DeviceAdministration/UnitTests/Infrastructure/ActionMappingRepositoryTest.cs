using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class ActionMappingRepositoryTest
    {
        private readonly Mock<IBlobStorageClient> _blobClientMock;
        private readonly ActionMappingRepository actionMappingRepository;
        private readonly Fixture fixture;

        public ActionMappingRepositoryTest()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _blobClientMock = new Mock<IBlobStorageClient>();
            var configurationProvicerMock = new Mock<IConfigurationProvider>();
            configurationProvicerMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            var blobStorageFactory = new BlobStorageClientFactory(_blobClientMock.Object);
            actionMappingRepository = new ActionMappingRepository(configurationProvicerMock.Object, blobStorageFactory);
        }

        [Fact]
        public async void GetAllMappingsAsyncTest()
        {
            var actionMappings = fixture.Create<List<ActionMapping>>();
            var actionMappingsString = JsonConvert.SerializeObject(actionMappings);
            var actionMappingBlobData = Encoding.UTF8.GetBytes(actionMappingsString);
            _blobClientMock.Setup(x => x.GetBlobData(It.IsNotNull<string>())).ReturnsAsync(actionMappingBlobData);
            _blobClientMock.Setup(x => x.GetBlobEtag(It.IsNotNull<string>())).ReturnsUsingFixture(fixture);
            var ret = await actionMappingRepository.GetAllMappingsAsync();
            Assert.NotNull(ret);
            Assert.Equal(actionMappingsString, JsonConvert.SerializeObject(ret));
        }

        [Fact]
        public async void SaveMappingAsyncTest()
        {
            var actionMappings = fixture.Create<List<ActionMapping>>();
            var actionMappingsString = JsonConvert.SerializeObject(actionMappings);
            var actionMappingBlobData = Encoding.UTF8.GetBytes(actionMappingsString);
            _blobClientMock.Setup(x => x.GetBlobData(It.IsNotNull<string>())).ReturnsAsync(actionMappingBlobData);
            _blobClientMock.Setup(x => x.GetBlobEtag(It.IsNotNull<string>())).ReturnsUsingFixture(fixture);

            string saveBuf = null;
            var newActionMapping = new ActionMapping
            {
                RuleOutput = "ruleXXXoutput",
                ActionId = "actionXXXid"
            };

            // New mapping
            actionMappings.Add(newActionMapping);
            actionMappingsString = JsonConvert.SerializeObject(actionMappings);
            actionMappings.Remove(newActionMapping);
            _blobClientMock.Setup(
                x =>
                    x.UploadFromByteArrayAsync(It.IsNotNull<string>(), It.IsNotNull<byte[]>(), It.IsNotNull<int>(),
                        It.IsNotNull<int>(),
                        It.IsNotNull<AccessCondition>(), It.IsAny<BlobRequestOptions>(), It.IsAny<OperationContext>()))
                .Callback<string, byte[], int, int, AccessCondition, BlobRequestOptions, OperationContext>(
                    (a, b, c, d, e, f, g) => saveBuf = Encoding.UTF8.GetString(b))
                .Returns(Task.FromResult(true));
            await actionMappingRepository.SaveMappingAsync(newActionMapping);
            Assert.NotNull(saveBuf);
            Assert.Equal(actionMappingsString, saveBuf);

            // Existing mapping
            actionMappingBlobData = Encoding.UTF8.GetBytes(actionMappingsString);
            _blobClientMock.Setup(x => x.GetBlobData(It.IsNotNull<string>())).ReturnsAsync(actionMappingBlobData);
            newActionMapping.ActionId = "actionYYYid";
            actionMappings.Add(newActionMapping);
            actionMappingsString = JsonConvert.SerializeObject(actionMappings);
            _blobClientMock.Setup(
                x =>
                    x.UploadFromByteArrayAsync(It.IsNotNull<string>(), It.IsNotNull<byte[]>(), It.IsNotNull<int>(),
                        It.IsNotNull<int>(),
                        It.IsNotNull<AccessCondition>(), It.IsAny<BlobRequestOptions>(), It.IsAny<OperationContext>()))
                .Callback<string, byte[], int, int, AccessCondition, BlobRequestOptions, OperationContext>(
                    (a, b, c, d, e, f, g) => saveBuf = Encoding.UTF8.GetString(b))
                .Returns(Task.FromResult(true));
            await actionMappingRepository.SaveMappingAsync(newActionMapping);
            Assert.NotNull(saveBuf);
            Assert.Equal(actionMappingsString, saveBuf);
        }
    }
}