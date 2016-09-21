using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class ActionMappingLogicTests
    {
        private Mock<IActionMappingRepository> _actionMappingRepositoryMock;
        private Mock<IDeviceRulesRepository> _deviceRulesRepositoryMock;
        private IFixture _fixture;
        private ActionMappingLogic _actionMappingLogic;

        public ActionMappingLogicTests()
        {
            _actionMappingRepositoryMock = new Mock<IActionMappingRepository>();
            _deviceRulesRepositoryMock = new Mock<IDeviceRulesRepository>();
            _fixture = new Fixture();
            _actionMappingLogic = new ActionMappingLogic(_actionMappingRepositoryMock.Object, _deviceRulesRepositoryMock.Object);
        }

        [Fact]
        public async void IsInitializationNeededAsyncTest()
        {
            _actionMappingRepositoryMock.SetupSequence(x => x.GetAllMappingsAsync())
                .ReturnsAsync(new List<ActionMapping>())
                .ReturnsAsync(_fixture.Create<List<ActionMapping>>());

            Assert.True(await _actionMappingLogic.IsInitializationNeededAsync());
            Assert.False(await _actionMappingLogic.IsInitializationNeededAsync());
        }

        [Fact]
        public async void InitializeDataIfNecessaryAsyncTest()
        {
            IList<ActionMapping> savedMappings = new List<ActionMapping>();
            _actionMappingRepositoryMock.SetupSequence(x => x.GetAllMappingsAsync())
                .ReturnsAsync(new List<ActionMapping>());
            _actionMappingRepositoryMock.Setup(x => x.SaveMappingAsync(It.IsNotNull<ActionMapping>()))
                .Callback<ActionMapping>((ob) => savedMappings.Add(ob))
                .Returns(Task.FromResult(true));

            Assert.True(await _actionMappingLogic.InitializeDataIfNecessaryAsync());
            Assert.Equal(2, savedMappings.Count);
            Assert.Equal("Send Message", savedMappings[0].ActionId);
            Assert.Equal("AlarmTemp", savedMappings[0].RuleOutput);
            Assert.Equal("Raise Alarm", savedMappings[1].ActionId);
            Assert.Equal("AlarmHumidity", savedMappings[1].RuleOutput);
        }

        [Fact]
        public async void GetAllMappingsAsyncTest()
        {
            var actionMappings = new List<ActionMapping>();
            _fixture.Customize<ActionMapping>(ob => ob.With(x => x.RuleOutput, "RuleXXXOutput"));
            actionMappings.AddRange(_fixture.CreateMany<ActionMapping>());
            _fixture.Customize<ActionMapping>(ob => ob.With(x => x.RuleOutput, "RuleYYYOutput"));
            actionMappings.AddRange(_fixture.CreateMany<ActionMapping>());
            var deviceRules = new List<DeviceRule>();
            _fixture.Customize<DeviceRule>(ob => ob.With(x => x.RuleOutput, "RuleXXXOutput"));
            deviceRules.AddRange(_fixture.CreateMany<DeviceRule>());
            var countXXXrules = deviceRules.Count;
            _fixture.Customize<DeviceRule>(ob => ob.With(x => x.RuleOutput, "RuleYYYOutput"));
            deviceRules.AddRange(_fixture.CreateMany<DeviceRule>());
            _actionMappingRepositoryMock.Setup(x => x.GetAllMappingsAsync()).ReturnsAsync(actionMappings);
            _deviceRulesRepositoryMock.Setup(x => x.GetAllRulesAsync()).ReturnsAsync(deviceRules);

            var ret = await _actionMappingLogic.GetAllMappingsAsync();
            Assert.NotNull(ret);
            Assert.Equal(actionMappings.Count, ret.Count);
            Assert.Equal(actionMappings[0].ActionId, ret[0].ActionId);
            Assert.Equal(actionMappings[0].RuleOutput, ret[0].RuleOutput);
            Assert.Equal(countXXXrules, ret[0].NumberOfDevices);
        }

        [Fact]
        public async void GetActionIdFromRuleOutputAsyncTest()
        {
            var mappings = _fixture.Create<List<ActionMapping>>();
            _actionMappingRepositoryMock.Setup(x => x.GetAllMappingsAsync()).ReturnsAsync(mappings);

            Assert.Equal("", await _actionMappingLogic.GetActionIdFromRuleOutputAsync("RuleDoesNotExist"));
            Assert.Equal(mappings[0].ActionId, await _actionMappingLogic.GetActionIdFromRuleOutputAsync(mappings[0].RuleOutput));
        }

        [Fact]
        public async void GetAvailableRuleOutputsAsyncTest()
        {
            var ret = await _actionMappingLogic.GetAvailableRuleOutputsAsync();
            Assert.NotNull(ret);
            Assert.Equal(2, ret.Count);
            Assert.Equal("AlarmTemp", ret[0]);
            Assert.Equal("AlarmHumidity", ret[1]);
        }
    }
}
