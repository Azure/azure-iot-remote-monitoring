using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Device
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BMP280 temperatureSensor;
        private TCS34725 colorSensor;
        private DispatcherTimer timer;
        private const int LED_PIN = 12;
        private const int BUTTON_PIN = 4;
        private GpioPin led;
        private GpioPinValue ledState;
        private GpioPin button;


        public MainPage()
        {
            this.InitializeComponent();

        }

        //This method will be called by the application framework when the page is first loaded
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            timer = new DispatcherTimer();

            MakePinWebAPICall();

            try
            {
                await InitializeLedAndButton();

                temperatureSensor = new BMP280();
                await temperatureSensor.InitializeAsync();

                colorSensor = new TCS34725(6);
                await colorSensor.InitializeAsync();
                colorSensor.LedState = LedState.Off;

                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(500);
                timer.Tick += ReadSensors;
                timer.Start();


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private async Task InitializeLedAndButton()
        {
            await Task.Yield();
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
            button.DebounceTimeout = TimeSpan.FromMilliseconds(500);
            button.SetDriveMode(GpioPinDriveMode.Input);
            button.ValueChanged += ButtonChanged;
        }

        private void ButtonChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            var buttonState = sender.Read();
            if (buttonState == GpioPinValue.High)
            {
                Debug.WriteLine("Button high");
            }
            else
            {
                Debug.WriteLine("Button low");
            }
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                Debug.WriteLine("Button pressed");
                colorSensor.LedState = colorSensor.LedState == LedState.Off ? LedState.On : LedState.Off;
            }
        }

        private async void ReadSensors(object sender, object e)
        {
            float temp = 0;
            float pressure = 0;
            float altitude = 0;

            //Create a constant for pressure at sea level. 
            //This is based on your local sea level pressure (Unit: Hectopascal)
            const float seaLevelPressure = 1013.25f;

            temp = await temperatureSensor.ReadTemperature();
            pressure = await temperatureSensor.ReadPreasure();
            altitude = await temperatureSensor.ReadAltitude(seaLevelPressure);

            //Write the values to your debug console
            Debug.WriteLine("Temperature: " + temp.ToString() + " deg C");
            Debug.WriteLine("Pressure: " + pressure.ToString() + " Pa");
            Debug.WriteLine("Altitude: " + altitude.ToString() + " m");

            //Read the approximate color from the sensor
            var color = await colorSensor.GetClosestWindowsColor();
            var rgb = await colorSensor.GetRgbData();
            var lux = rgb.AsLux();
            Debug.WriteLine("Detected color:" + color);

            ledState = ledState == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;
            led.Write(ledState);

        }

        /// <summary>
        // This method will put your pin on the world map of makers using this lesson.
        // This uses imprecise location (for example, a location derived from your IP 
        // address with less precision such as at a city or postal code level). 
        // No personal information is stored.  It simply
        // collects the total count and other aggregate information.
        // http://www.microsoft.com/en-us/privacystatement/default.aspx
        // Comment out the line below to opt-out
        /// </summary>
        public void MakePinWebAPICall()
        {
            try
            {
                var client = new HttpClient();

                // Comment this line to opt out of the pin map.
                client.GetStringAsync("http://adafruitsample.azurewebsites.net/api?Lesson=203");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Web call failed: " + e.Message);
            }
        }

    }
}
