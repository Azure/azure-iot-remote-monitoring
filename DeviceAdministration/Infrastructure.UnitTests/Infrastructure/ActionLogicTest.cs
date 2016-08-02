using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.TestStubs;
using Moq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Infrastructure
{
    public class ActionLogicTest
    {
        private IActionRepository _actionRepository;
        private ActionLogic actionLogic;
        static public string ENDPOINT = "http://www.Test.Endpoint/";

        public ActionLogicTest()
        {
            _actionRepository = new ActionRepository(new HttpMessageHandlerStub());
            actionLogic = new ActionLogic(_actionRepository);
        }

        [Fact]
        public async void ExecuteLogicAppAsyncTest()
        {
            string actionId = "Send Message";
            string deviceId = "TestDeviceID";
            string measurementName = "TestMeasurementName";
            double measuredValue = 10.0;

            bool res = await actionLogic.ExecuteLogicAppAsync(actionId, deviceId, measurementName, measuredValue);
            Assert.False(res);

            await _actionRepository.AddActionEndpoint(actionId, ENDPOINT);
            res = await actionLogic.ExecuteLogicAppAsync(actionId, deviceId, measurementName, measuredValue);
            Assert.True(res);
        }
    }
}