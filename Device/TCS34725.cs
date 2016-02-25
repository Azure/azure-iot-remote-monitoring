using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI;

namespace Device
{
    public enum LedState { On, Off };

    public class ColorData
    {
        public ushort Red { get; set; }
        public ushort Green { get; set; }
        public ushort Blue { get; set; }
        public ushort Clear { get; set; }
    }

    public class RgbData
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public float AsLux()
        {
            var illuminance = (-0.32466F * Red) + (1.57837F * Green) + (-0.73191F * Blue);
            return illuminance;
        }
    }

    class TCS34725
    {
        #region TCS34725 Data Channels & Byte Addresses
        const byte TCS34725_Address = 0x29;

        const byte TCS34725_ENABLE = 0x00;
        const byte TCS34725_ENABLE_PON = 0x01;
        const byte TCS34725_ENABLE_AEN = 0x02;

        const byte TCS34725_ID = 0x12;

        //	Clear Channel Data
        const byte TCS34725_CDATAL = 0x14;
        const byte TCS34725_CDATAH = 0x15;
        //	Red Channel Data
        const byte TCS34725_RDATAL = 0x16;
        const byte TCS34725_RDATAH = 0x17;
        //	Green Channel Data
        const byte TCS34725_GDATAL = 0x18;
        const byte TCS34725_GDATAH = 0x19;
        //	Blue Channel Data
        const byte TCS34725_BDATAL = 0x1A;
        const byte TCS34725_BDATAH = 0x1B;

        const byte TCS34725_ATIME = 0x01;
        const byte TCS34725_CONTROL = 0x0F;
        const byte TCS34725_COMMAND_BIT = 0x80;
        #endregion

        #region Instance Variables
        const string I2CControllerName = "I2C1";
        private I2cDevice colorSensor = null;

        private GpioController gpio;
        private GpioPin LedControlGPIOPin;
        private int LedControlPin;
        bool Init = false;

        #endregion

        #region Colorful Generics
        private string[] limitColorList = { "Black", "White", "Blue", "Red", "Green", "Purple", "Yellow", "Orange", "DarkSlateBlue", "DarkGrey", "Pink" };
        public struct KnownColor
        {
            public Color colorValue;
            public string colorName;

            public KnownColor(Color value, string name)
            {
                colorValue = value;
                colorName = name;
            }
        };

        private List<KnownColor> colorList;
        #endregion

        #region Class and Initialization
        public TCS34725(int ledControlPin = 12)
        {
            Debug.WriteLine(string.Format("TCS34725::New TCS34725({0})", ledControlPin));
            LedControlPin = ledControlPin;
        }

        public async Task InitializeAsync()
        {
            Debug.WriteLine("TCS34725::Initialiize");

            try
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(TCS34725_Address);
                settings.BusSpeed = I2cBusSpeed.FastMode;

                string aqs = I2cDevice.GetDeviceSelector(I2CControllerName);
                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs);

                colorSensor = await I2cDevice.FromIdAsync(dis[0].Id, settings);
                gpio = GpioController.GetDefault();

                LedControlGPIOPin = gpio.OpenPin(LedControlPin);
                LedControlGPIOPin.SetDriveMode(GpioPinDriveMode.Output);

                initColorList();
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("Exception: {0}\n{1}", e.Message, e.StackTrace));
                throw;
            }
        }

        public void initColorList()
        {
            Debug.WriteLine("TCS34725::InitColorList");
            colorList = new List<KnownColor>();

            foreach (PropertyInfo p in typeof(Colors).GetProperties())
            {
                if (limitColorList.Contains(p.Name))
                {
                    //KnownColor tmp = new KnownColor((Color)p.GetValue(null), p.Name);
                    colorList.Add(new KnownColor((Color)p.GetValue(null), p.Name));
                }
            }
        }
        #endregion

        #region Enumeration Handlers
        private LedState _LedState = LedState.On;

        public LedState LedState
        {
            get { return _LedState; }
            set
            {
                Debug.WriteLine("TCS34725::LedState::set " + value.ToString());

                if (LedControlGPIOPin != null)
                {
                    GpioPinValue newValue = (value == LedState.On ? GpioPinValue.High : GpioPinValue.Low);
                    LedControlGPIOPin.Write(newValue);
                    _LedState = value;
                }
            }
        }

        enum eTCS34725IntegrationTime
        {
            TCS34725_INTEGRATIONTIME_2_4MS = 0xFF,  //	2.4ms	- 1 Cycle		(Max Count: 1024)
            TCS34725_INTEGRATIONTIME_24MS = 0xF6,   //	24ms	- 10 Cycles		(Max Count: 10240)
            TCS34725_INTEGRATIONTIME_50MS = 0xEB,   //	50ms	- 20 Cycles		(Max Count: 20480)
            TCS34725_INTEGRATIONTIME_101MS = 0xD5,  //	101ms	- 42 Cycles		(Max Count: 43008)
            TCS34725_INTEGRATIONTIME_154MS = 0xC0,  //	154ms	- 64 Cycles		(Max Count: 65535)
            TCS34725_INTEGRATIONTIME_700MS = 0x00   //	700ms	- 256 Cycles	(Max Count: 65535)
        };

        eTCS34725IntegrationTime _tcs34725IntegrationTime = eTCS34725IntegrationTime.TCS34725_INTEGRATIONTIME_700MS;

        enum eTCS34725Gain
        {
            TCS34725_GAIN_1X = 0x00,    //	No Gain
            TCS34725_GAIN_4X = 0x01,    //	2x Gain
            TCS34725_GAIN_16X = 0x02,   //	16x Gain
            TCS34725_GAIN_60X = 0x03    //	60x	Gain
        };

        eTCS34725Gain _tcs34725Gain = eTCS34725Gain.TCS34725_GAIN_1X;
        #endregion

        private async Task begin()
        {
            Debug.WriteLine("TCS34725::Begin");
            byte[] WriteBuffer = new byte[] { TCS34725_ID | TCS34725_COMMAND_BIT };
            byte[] ReadBuffer = new byte[] { 0xFF };

            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            Debug.WriteLine(string.Format("TCS34725 Signature: {0}", ReadBuffer[0]));

            if (ReadBuffer[0] != 0x44)
            {
                Debug.WriteLine(string.Format("TCS34725::Begin - SIGNATURE MISMATCH"));
                return;
            }

            Init = true;

            setIntegrationTime(_tcs34725IntegrationTime);
            setGain(_tcs34725Gain);

            await Enable();
        }

        private async void setGain(eTCS34725Gain gain)
        {
            Debug.WriteLine("TCS34725::SetGain");
            if (!Init) await begin();
            _tcs34725Gain = gain;

            byte[] WriteBuffer = new byte[] { TCS34725_CONTROL | TCS34725_COMMAND_BIT, (byte)_tcs34725Gain };
            colorSensor.Write(WriteBuffer);
        }

        private async void setIntegrationTime(eTCS34725IntegrationTime integrationTime)
        {
            Debug.WriteLine("TCS34725::SetIntegrationTime");
            if (!Init) await begin();
            _tcs34725IntegrationTime = integrationTime;

            byte[] WriteBuffer = new byte[] { TCS34725_ATIME | TCS34725_COMMAND_BIT, (byte)_tcs34725IntegrationTime };
            colorSensor.Write(WriteBuffer);
        }

        public async Task Enable()
        {
            Debug.WriteLine("TCS34725::Enable");
            if (!Init) await begin();

            byte[] WriteBuffer = new byte[] { 0x00, 0x00 };
            WriteBuffer[0] = TCS34725_ENABLE | TCS34725_COMMAND_BIT;
            WriteBuffer[1] = TCS34725_ENABLE_PON;

            colorSensor.Write(WriteBuffer);

            await Task.Delay(3);

            WriteBuffer[1] = (TCS34725_ENABLE_PON | TCS34725_ENABLE_AEN);
            colorSensor.Write(WriteBuffer);
        }

        public async Task Disable()
        {
            Debug.WriteLine("TCS34725::Disable");
            if (!Init) await begin();

            byte[] WriteBuffer = new byte[] { TCS34725_ENABLE | TCS34725_COMMAND_BIT };
            byte[] ReadBuffer = new byte[] { 0xFF };

            colorSensor.WriteRead(WriteBuffer, ReadBuffer);

            byte onState = (TCS34725_ENABLE_PON | TCS34725_ENABLE_AEN);
            byte offState = (byte)~onState;
            offState &= ReadBuffer[0];
            byte[] OffBuffer = new byte[] { TCS34725_ENABLE, offState };

            colorSensor.Write(OffBuffer);
        }

        UInt16 ColorFromBuffer(byte[] buffer)
        {
            Debug.WriteLine(string.Format("TCS34725::ColorFromBuffer({0})", buffer));

            UInt16 color = buffer[1];
            color <<= 8;
            color |= buffer[0];

            return color;
        }

        public async Task<ColorData> getRawData()
        {
            Debug.WriteLine("TCS34725::getRawData");

            ColorData colorData = new ColorData();

            if (!Init) await begin();

            byte[] WriteBuffer = new byte[] { 0x00 };
            byte[] ReadBuffer = new byte[] { 0x00, 0x00 };

            //	Read and Write Clear Data
            WriteBuffer[0] = TCS34725_CDATAL | TCS34725_COMMAND_BIT;
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            colorData.Clear = ColorFromBuffer(ReadBuffer);

            //	Read and Write Red Data
            WriteBuffer[0] = TCS34725_RDATAL | TCS34725_COMMAND_BIT;
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            colorData.Red = ColorFromBuffer(ReadBuffer);

            //	Read and Write Green Data
            WriteBuffer[0] = TCS34725_GDATAL | TCS34725_COMMAND_BIT;
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            colorData.Green = ColorFromBuffer(ReadBuffer);

            //	Read and Write Blue Data
            WriteBuffer[0] = TCS34725_BDATAL | TCS34725_COMMAND_BIT;
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            colorData.Blue = ColorFromBuffer(ReadBuffer);

            Debug.WriteLine(string.Format("Raw Data - Clear: {0}, Red: {1}, Green: {2}, Blue: {3}", colorData.Clear, colorData.Red, colorData.Green, colorData.Blue));

            return colorData;
        }

        public async Task<RgbData> GetRgbData()
        {
            Debug.WriteLine("TCS34725::getRgbData");

            RgbData rgbData = new RgbData();

            ColorData rawColor = await getRawData();

            if (rawColor.Clear > 0)
            {
                rgbData.Red = (rawColor.Red * 255 / rawColor.Clear);
                rgbData.Green = (rawColor.Green * 255 / rawColor.Clear);
                rgbData.Blue = (rawColor.Blue * 255 / rawColor.Clear);
            }

            Debug.WriteLine(string.Format("RGB Data - Red: {0}, Green: {1}, Blue: {2}", rgbData.Red, rgbData.Green, rgbData.Blue));

            return rgbData;
        }

        public async Task<string> GetClosestWindowsColor()
        {
            Debug.WriteLine("TCS34725::GetClosestColor");
            RgbData rgbData = await GetRgbData();
            KnownColor closestColor = colorList[7];

            double minDiff = double.MaxValue;

            foreach (var c in colorList)
            {
                Color cValue = c.colorValue;

                double diff = Math.Pow((cValue.R - rgbData.Red), 2) +
                              Math.Pow((cValue.G - rgbData.Green), 2) +
                              Math.Pow((cValue.B - rgbData.Blue), 2);
                diff = (int)Math.Sqrt(diff);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    closestColor = c;
                }
            }

            Debug.WriteLine(string.Format("Approx Color: {0}\r\nValue: {1}", closestColor.colorName, closestColor.colorValue));
            return closestColor.colorName;
        }
    }
}