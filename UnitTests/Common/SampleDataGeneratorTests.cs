using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.TestStubs;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class SampleDataGeneratorTests
    {
        [Fact]
        public void MinGreaterThanMaxShouldThrowException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(40, 20));
        }

        [Fact]
        public void MaxGreaterThanThresholdShouldThrowException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(5, 15, 10, 25));
        }

        [Fact]
        public void MaxEqualToMinShouldThrowException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(45, 45));
        }

        [Fact]
        public void MaxEqualToThresholdShouldThrowException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(5, 10, 10, 25));
        }

        [Fact]
        public void PeakIntervalZeroShouldThrowException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SampleDataGenerator(5, 10, 15, 0));
        }

        [Fact]
        public void InspectMaximumLimits()
        {
            double value = 0;
            IRandomGenerator randomGenerator = new RandomGeneratorStub(0.99);
            var sampleData = new SampleDataGenerator(5, 10, randomGenerator);
            for (var i = 0; i < 100; i++)
            {
                value = Math.Round(sampleData.GetNextValue(), 2);
                Assert.True(value <= 10);
            }
        }

        [Fact]
        public void InspectMinimumLimits()
        {
            double value = 0;
            IRandomGenerator randomGenerator = new RandomGeneratorStub(0.01);
            var sampleData = new SampleDataGenerator(5, 10, randomGenerator);
            for (var i = 0; i < 100; i++)
            {
                value = Math.Round(sampleData.GetNextValue(), 2);
                Assert.True(value >= 5);
            }
        }

        [Fact]
        public void ExpectingPeaks()
        {
            var numberExpectedPeaks = 4;
            var peaksSeen = 0;
            double value;
            var sampleData = new SampleDataGenerator(5, 10, 15, 25);
            for (var i = 0; i < 120; i++)
            {
                value = Math.Round(sampleData.GetNextValue(), 2);
                if (value > 15)
                {
                    ++peaksSeen;
                }
            }
            Assert.Equal(numberExpectedPeaks, peaksSeen);
        }

        [Fact]
        public void ExcludingPeaks()
        {
            var numberExpectedPeaks = 0;
            var peaksSeen = 0;
            double value;
            var sampleData = new SampleDataGenerator(5, 10);
            for (var i = 0; i < 120; i++)
            {
                value = Math.Round(sampleData.GetNextValue(), 2);
                if (value > 10)
                {
                    ++peaksSeen;
                }
            }
            Assert.Equal(numberExpectedPeaks, peaksSeen);
        }

        [Fact]
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

            Assert.True(result1 >= 1995);
            Assert.True(result1 <= 2005);

            sampleData.ShiftSubsequentData(-2000);

            // this one will just be -2000
            sampleData.GetNextValue();

            // this one will likely be different
            var result2 = sampleData.GetNextValue();

            Assert.True(result2 >= -2005);
            Assert.True(result2 <= -1995);
        }
    }
}