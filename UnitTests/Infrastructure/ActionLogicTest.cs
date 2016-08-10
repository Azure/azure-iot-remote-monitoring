using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.TestStubs;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class ActionLogicTest
    {
        public static string ENDPOINT = "http://www.Test.Endpoint/";
        private readonly IActionRepository _actionRepository;
        private readonly ActionLogic actionLogic;

        public ActionLogicTest()
        {
            _actionRepository = new ActionRepository(new HttpMessageHandlerStub());
            actionLogic = new ActionLogic(_actionRepository);
        }

        [Fact]
        public async void ExecuteLogicAppAsyncTest()
        {
            var actionId = "Send Message";
            var deviceId = "TestDeviceID";
            var measurementName = "TestMeasurementName";
            var measuredValue = 10.0;

            var res = await actionLogic.ExecuteLogicAppAsync(actionId, deviceId, measurementName, measuredValue);
            Assert.False(res);

            await _actionRepository.AddActionEndpoint(actionId, ENDPOINT);
            res = await actionLogic.ExecuteLogicAppAsync(actionId, deviceId, measurementName, measuredValue);
            Assert.True(res);
        }
    }
}