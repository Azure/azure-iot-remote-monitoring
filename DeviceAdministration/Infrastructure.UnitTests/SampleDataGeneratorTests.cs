using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.TestStubs;
using NUnit.Framework;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests
{
    [TestFixture]
    public class SampleDataGeneratorTests
    {
        [Test]
        public void MinGreaterThanMaxShouldThrowException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(40, 20));
        }

        [Test]
        public void MaxGreaterThanThresholdShouldThrowException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(5, 15, 10, 25));
        }

        [Test]
        public void MaxEqualToMinShouldThrowException()
        {
             Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(45, 45));
        }

        [Test]
        public void MaxEqualToThresholdShouldThrowException()
        {
             Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(5, 10, 10, 25));

        }

        [Test]
        public void PeakIntervalZeroShouldThrowException()
        {
             Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(5, 10, 15, 0));
        }

        [Test]
        public void InspectMaximumLimits()
        {
            double value = 0;
            IRandomGenerator randomGenerator = new RandomGeneratorStub(0.99);
            var sampleData = new SampleDataGenerator(5, 10, randomGenerator);
            for (int i = 0; i < 100; i++)
            {
                value = Math.Round(sampleData.GetNextValue(), 2);
                Assert.LessOrEqual(value, 10);
            }
        }

        [Test]
        public void InspectMinimumLimits()
        {
            double value = 0;
            IRandomGenerator randomGenerator = new RandomGeneratorStub(0.01);
            var sampleData = new SampleDataGenerator(5, 10, randomGenerator);
            for (int i = 0; i < 100; i++)
            {
                value =  Math.Round(sampleData.GetNextValue(), 2);
                Assert.GreaterOrEqual(value, 5);
            }   
        }

        [Test]
        public void ExpectingPeaks()
        {
            int numberExpectedPeaks = 4;
            int peaksSeen = 0;
            double value;
            var sampleData = new SampleDataGenerator(5, 10, 15, 25);
            for (int i = 0; i < 120; i++)
            {
                value = Math.Round(sampleData.GetNextValue(), 2);
                if (value > 15)
                {
                    ++peaksSeen;
                }
            }
            Assert.That(numberExpectedPeaks, Is.EqualTo(peaksSeen));
        }

        [Test]
        public void ExcludingPeaks()
        {
            int numberExpectedPeaks = 0;
            int peaksSeen = 0;
            double value;
            var sampleData = new SampleDataGenerator(5, 10);
            for (int i = 0; i < 120; i++)
            {
                value = Math.Round(sampleData.GetNextValue(), 2);
                if (value > 10)
                {
                    ++peaksSeen;
                }
            }
            Assert.That(numberExpectedPeaks, Is.EqualTo(peaksSeen));
        }

        [Test]
        public void ChangingSetpointWorksRepeatably()
        {
            // this is to check for a bug that manifested when 
            // ShiftSubsequentData() was called several times with
            // various values

            IRandomGenerator randomGenerator = new RandomGeneratorStub(0.95);

            var sampleData = new SampleDataGenerator(10, 20, randomGenerator);

            sampleData.GetNextValue();

            sampleData.ShiftSubsequentData(200);
            
            sampleData.GetNextValue();

            sampleData.ShiftSubsequentData(2000);

            // this one will just be 2000
            sampleData.GetNextValue();

            // this one will likely be different
            var result1 = sampleData.GetNextValue();

            Assert.GreaterOrEqual(result1, 1995);
            Assert.LessOrEqual(result1, 2005);

            sampleData.ShiftSubsequentData(-2000);

            // this one will just be -2000
            sampleData.GetNextValue();

            // this one will likely be different
            var result2 = sampleData.GetNextValue();

            Assert.GreaterOrEqual(result2, -2005);
            Assert.LessOrEqual(result2, -1995);
        }
    }
}
