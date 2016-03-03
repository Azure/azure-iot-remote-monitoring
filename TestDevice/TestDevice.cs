using System;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Logging;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using System.Diagnostics;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace TestDevice
{
    /// <summary>
    /// Implementation of a specific device type that extends the BaseDevice functionality
    /// </summary>
    internal class TestDevice : DeviceBase
    {

        private const int LED_PIN = 12;
        private const int BUTTON_PIN = 4;
        private GpioPin led;
        private GpioPinValue ledState;
        private GpioPin button;

        private BMP280 temperatureSensor;
        private TCS34725 colorSensor;

        public override bool IsSimulated
        {
            get
            {
                return false;
            }
        }


        public TestDevice(ILogger logger, ITransportFactory transportFactory, IConfigurationProvider configurationProvider)
            : base(logger, transportFactory, configurationProvider)
        {
        }

        public override async Task Initialize(InitialDeviceConfig config)
        {
            await base.Initialize(config);
            DeviceProperties.HubEnabledState = true;
            DeviceProperties.Manufacturer = "MSHUDX";
            DeviceProperties.ModelNumber = "007";
            DeviceProperties.SerialNumber = "1337";
            DeviceProperties.FirmwareVersion = "1.0";
            DeviceProperties.Platform = "IoT Core";
            DeviceProperties.Processor = "ARM";
            DeviceProperties.Latitude = "47.561134";
            DeviceProperties.Longitude = "19.052567";

            await InitializeSensors();
            await InitializeLedAndButton();
            await RegisterTelemetry(() => Task.FromResult<ITelemetry>(new TemperatureTelemetry(Logger, this, temperatureSensor)));
            await RegisterTelemetry(() => Task.FromResult<ITelemetry>(new StartupTelemetry(Logger, this)));
        }

        private async Task InitializeLedAndButton()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                return;
            }

            led = gpio.OpenPin(LED_PIN);
            ledState = GpioPinValue.High;
            led.Write(ledState);
            led.SetDriveMode(GpioPinDriveMode.Output);

            button = gpio.OpenPin(BUTTON_PIN);
            button.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            button.SetDriveMode(GpioPinDriveMode.Input);
            button.ValueChanged += ButtonChanged;

            await Task.Yield();
        }

        private async Task InitializeSensors()
        {
            temperatureSensor = new BMP280();
            await temperatureSensor.InitializeAsync();

            colorSensor = new TCS34725(6);
            await colorSensor.InitializeAsync();
            colorSensor.LedState = LedState.Off;
        }

        private void ButtonChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                colorSensor.LedState = colorSensor.LedState == LedState.Off ? LedState.On : LedState.Off;
            }
        }

        /// <summary>
        /// Builds up the set of commands that are supported by this device
        /// </summary>
        protected override void InitCommandProcessors()
        {
            var pingDeviceProcessor = new PingDeviceProcessor(this);
            var startCommandProcessor = new StartCommandProcessor(this);
            var stopCommandProcessor = new StopCommandProcessor(this);

            pingDeviceProcessor.NextCommandProcessor = startCommandProcessor;
            startCommandProcessor.NextCommandProcessor = stopCommandProcessor;

            RootCommandProcessor = pingDeviceProcessor;
        }
        

        public async void ChangeDeviceState(string deviceState)
        {
            // simply update the DeviceState property and send updated device info packet
            DeviceProperties.DeviceState = deviceState;
            await SendDeviceInfo();
            Logger.LogInfo("Device {0} in {1} state", DeviceID, deviceState);
        }
    }
}
