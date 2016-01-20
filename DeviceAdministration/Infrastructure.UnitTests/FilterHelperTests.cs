using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using NUnit.Framework;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests
{
    [TestFixture]
    public class FilterHelperTests
    {
        [Test]
        public void NoFilterShouldReturnEverything()
        {
            TestFilter(new List<FilterInfo>(), 10);
        }

        #region Test Group: filtering on missing value should remove all devices

        [Test]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_Exact_CaseSensitive()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseSensitive,
                    FilterValue = "DKFSLKFJDKKD"
                }
            }; 

            TestFilter(filters, 0);
        }

        [Test]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_Exact_CaseInsensitive()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseInsensitive,
                    FilterValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        [Test]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_StartsWith_CaseSensitive()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.StartsWithCaseSensitive,
                    FilterValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        [Test]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_StartsWith_CaseInsensitive()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.StartsWithCaseInsensitive,
                    FilterValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        [Test]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_Contains_CaseSensitive()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ContainsCaseSensitive,
                    FilterValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        [Test]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_Contains_CaseInsensitive()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        #endregion

        #region Test Group: filter should return 1 device

        [Test]
        public void FilterShouldReturnOneDevice_Exact_CaseSensitive()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseSensitive,
                    FilterValue = "The one special value"
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void FilterShouldReturnOneDevice_Exact_CaseInsensitive_SameCase()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseInsensitive,
                    FilterValue = "The one special value"
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void FilterShouldReturnOneDevice_Exact_CaseInsensitive_DiffCase()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseInsensitive,
                    FilterValue = "The ONE SPECIAL VALUe"
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void FilterShouldReturnOneDevice_StartsWith_CaseInsensitive_SameCase()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.StartsWithCaseInsensitive,
                    FilterValue = "The "
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void FilterShouldReturnOneDevice_StartsWith_CaseInsensitive_DiffCase()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.StartsWithCaseInsensitive,
                    FilterValue = "ThE "
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void FilterShouldReturnOneDevice_StartsWith_CaseSensitive()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.StartsWithCaseSensitive,
                    FilterValue = "The "
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void FilterShouldReturnOneDevice_Contains_CaseSensitive()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ContainsCaseSensitive,
                    FilterValue = " special "
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void FilterShouldReturnOneDevice_Contains_CaseInsensitive_SameCase()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = " special "
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void FilterShouldReturnOneDevice_Contains_CaseInsensitive_DiffCase()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = " spECial "
                }
            };

            TestFilter(filters, 1);
        }

        #endregion

        #region Test Group: case sensitive filters should be case sensitive

        [Test]
        public void CaseSensitiveFilterShouldReturnNothing_Exact_CaseSensitive_DiffCase()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseSensitive,
                    FilterValue = "The One Special Value"
                }
            };

            TestFilter(filters, 0);
        }

        [Test]
        public void CaseSensitiveFilterShouldReturnNothing_StartsWith_CaseSensitive_DiffCase()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.StartsWithCaseSensitive,
                    FilterValue = "ThE One"
                }
            };

            TestFilter(filters, 0);
        }

        [Test]
        public void CaseSensitiveFilterShouldReturnNothing_Contains_CaseSensitive_DiffCase()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ContainsCaseSensitive,
                    FilterValue = " speciAl "
                }
            };

            TestFilter(filters, 0);
        }

        #endregion

        #region Test Group: multiple filters should work together

        [Test]
        public void MultipleFiltersShouldWorkTogether_RemoveAllDevices()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseInsensitive,
                    FilterValue = "The One Special Value"
                },
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = "dog"
                }
            };

            TestFilter(filters, 0);
        }

        [Test]
        public void MultipleFiltersShouldWorkTogether_ReturnOneDevice()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseInsensitive,
                    FilterValue = "The One Special Value"
                },
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = "value"
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void MultipleFiltersShouldWorkTogether_DifferentColumns_ReturnOneDevice()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseInsensitive,
                    FilterValue = "The One Special Value"
                },
                new FilterInfo()
                {
                    ColumnName = "DeviceState",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = "devicestate"
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void MultipleFiltersShouldWorkTogether_DifferentColumns_ReturnNoDevices()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseInsensitive,
                    FilterValue = "The One Special Value"
                },
                new FilterInfo()
                {
                    ColumnName = "DeviceState",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = "Ada"
                }
            };

            TestFilter(filters, 0);
        }

        [Test]
        public void MultipleTripleFiltersShouldWorkTogether_DifferentColumns_ReturnOneDevice()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseInsensitive,
                    FilterValue = "The One Special Value"   // passes one device
                },
                new FilterInfo()
                {
                    ColumnName = "DeviceState",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = "State"                   // passes all devices
                },
                new FilterInfo()
                {
                    ColumnName = "FirmwareVersion",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = "WARE"                    // passes all devices
                }
            };

            TestFilter(filters, 1);
        }

        [Test]
        public void MultipleTripleFiltersShouldWorkTogether_DifferentColumns_ReturnNoDevices()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = FilterType.ExactMatchCaseInsensitive,
                    FilterValue = "The One Special Value"  // passes one device
                },
                new FilterInfo()
                {
                    ColumnName = "DeviceState",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = "State"                  // passes all devices
                },
                new FilterInfo()
                {
                    ColumnName = "FirmwareVersion",
                    FilterType = FilterType.ContainsCaseInsensitive,
                    FilterValue = "nope"                   // passes no devices
                }
            };

            TestFilter(filters, 0);
        }

        #endregion

        #region Test Group: column names should not be case sensitive

        [Test]
        public void FilterWithColumnNameDiffCaseShouldWork()
        {
            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DEViceid",
                    FilterType = FilterType.ExactMatchCaseSensitive,
                    FilterValue = "The one special value"
                }
            };

            TestFilter(filters, 1);
        }

        #endregion

        #region Test Group: missing data in devices should not throw

        #region Sub-test Group: non-null DeviceProperties but missing property off of DeviceProperties should not throw

        [Test]
        public void FilteringDeviceWithNullDeviceIdShouldWork_ContainsCaseInsensitive()
        {
            TestNullDeviceId(FilterType.ContainsCaseInsensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDeviceIdShouldWork_ContainsCaseSensitive()
        {
            TestNullDeviceId(FilterType.ContainsCaseSensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDeviceIdShouldWork_ExactMatchCaseInsensitive()
        {
            TestNullDeviceId(FilterType.ExactMatchCaseInsensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDeviceIdShouldWork_ExactMatchCaseSensitive()
        {
            TestNullDeviceId(FilterType.ExactMatchCaseSensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDeviceIdShouldWork_StartsWithCaseInsensitive()
        {
            TestNullDeviceId(FilterType.StartsWithCaseInsensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDeviceIdShouldWork_StartsWithCaseSensitive()
        {
            TestNullDeviceId(FilterType.StartsWithCaseSensitive);
        }

        private void TestNullDeviceId(FilterType filterType)
        {
            var devicesWithNullDeviceId = GetListWithOneSpecialDeviceIdValue(specialDeviceId: null);

            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = filterType,
                    FilterValue = "x"
                }
            };

            var filtered = FilterHelper.FilterDeviceList(devicesWithNullDeviceId, filters);

            Assert.AreEqual(0, filtered.Count());
        }

        #endregion

        #region Sub-test Group: null DeviceProperties should not throw

        [Test]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_ContainsCaseInsensitive()
        {
            TestNullDeviceProperties(FilterType.ContainsCaseInsensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_ContainsCaseSensitive()
        {
            TestNullDeviceProperties(FilterType.ContainsCaseSensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_ExactMatchCaseInsensitive()
        {
            TestNullDeviceProperties(FilterType.ExactMatchCaseInsensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_ExactMatchCaseSensitive()
        {
            TestNullDeviceProperties(FilterType.ExactMatchCaseSensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_StartsWithCaseInsensitive()
        {
            TestNullDeviceProperties(FilterType.StartsWithCaseInsensitive);
        }

        [Test]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_StartsWithCaseSensitive()
        {
            TestNullDeviceProperties(FilterType.StartsWithCaseSensitive);
        }

        private void TestNullDeviceProperties(FilterType filterType)
        {
            var device = DeviceSchemaHelper.BuildDeviceStructure(Guid.NewGuid().ToString(), true, null);

            device.DeviceProperties = null;

            var list = new List<dynamic>() { device };

            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "DeviceID",
                    FilterType = filterType,
                    FilterValue = "x"
                }
            };

            var filtered = FilterHelper.FilterDeviceList(list.AsQueryable(), filters);

            Assert.AreEqual(0, filtered.Count());
        }

        #endregion

        #endregion

        #region Test Group: status tests

        [Test]
        public void Status_PendingShouldReturnBothNullTypes()
        {
            var list = GetListWithEnabledTestValues();

            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "StatuS",  // intentionally use weird casing for test
                    FilterValue = "PendinG"
                }
            };

            var results = FilterHelper.FilterDeviceList(list, filters).ToList<dynamic>();

            Assert.AreEqual(2, results.Count());

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(
                () => DeviceSchemaHelper.GetDeviceProperties(results[0]));

            Assert.AreEqual("EnabledNull", results[1].DeviceProperties.DeviceID.ToString());
        }

        [Test]
        public void Status_RunningShouldReturnEnabled()
        {
            var list = GetListWithEnabledTestValues();

            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "StatuS",  // intentionally use weird casing for test
                    FilterValue = "RunninG"
                }
            };

            var results = FilterHelper.FilterDeviceList(list, filters).ToList<dynamic>();

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual("EnabledTrue", results[0].DeviceProperties.DeviceID.ToString());
        }

        [Test]
        public void Status_DisabledShouldReturnDisabled()
        {
            var list = GetListWithEnabledTestValues();

            var filters = new List<FilterInfo>()
            {
                new FilterInfo()
                {
                    ColumnName = "StatuS",  // intentionally use weird casing for test
                    FilterValue = "DisableD"
                }
            };

            var results = FilterHelper.FilterDeviceList(list, filters).ToList<dynamic>();

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual("EnabledFalse", results[0].DeviceProperties.DeviceID.ToString());
        }

        private static IQueryable<dynamic> GetListWithEnabledTestValues()
        {
            var list = GetSampleDevices(4).ToList();

            list[0].DeviceProperties = null;
            
            list[1].DeviceProperties.HubEnabledState = null;
            list[1].DeviceProperties.DeviceID = "EnabledNull";

            list[2].DeviceProperties.HubEnabledState = true;
            list[2].DeviceProperties.DeviceID = "EnabledTrue";

            list[3].DeviceProperties.HubEnabledState = false;
            list[3].DeviceProperties.DeviceID = "EnabledFalse";

            return list.AsQueryable<dynamic>();
        }

        #endregion

        #region Testing Infrastructure

        [Test]
        public void FilterTestInfrastructureShouldWork()
        {
            // make sure the infrastructure is working as expected
            var list = GetSampleDevices();

            Assert.AreEqual(10, list.Count());
        }

        private static void TestFilter(List<FilterInfo> filters, int expectedCount)
        {
            var list = GetListWithOneSpecialDeviceIdValue();

            var filtered = FilterHelper.FilterDeviceList(list, filters);

            Assert.AreEqual(expectedCount, filtered.Count());
        }

        private static IQueryable<dynamic> GetListWithOneSpecialDeviceIdValue(string specialDeviceId = "The one special value")
        {
            var list = GetSampleDevices().ToList();

            list[4].DeviceProperties.DeviceID = specialDeviceId;

            return list.AsQueryable<dynamic>();
        }

        private static IQueryable<dynamic> GetSampleDevices(int desiredNumberOfDevices = 10)
        {
            List<dynamic> devices = new List<dynamic>();

            for (int i = 0; i < desiredNumberOfDevices; ++i)
            {
                devices.Add(GetDefaultTestDevice());
            }

            return devices.AsQueryable<dynamic>();
        }

        private static dynamic GetDefaultTestDevice()
        {
            dynamic device = DeviceSchemaHelper.BuildDeviceStructure("DeviceID-Test", true, null);
            dynamic props = DeviceSchemaHelper.GetDeviceProperties(device);
            props.AvailablePowerSources = 123;
            props.BatteryLevel = 12;
            props.CreatedTime = new DateTime(2000, 01, 01);
            props.DeviceState = "DeviceState-Test";
            props.HubEnabledState = true;
            props.FirmwareVersion = "FirmwareVersion-Test";
            props.InstalledRAM = "InstalledRAM-Test";
            props.Manufacturer = "Manufacturer-Test";
            props.MemoryFree = 123;
            props.ModelNumber = "ModelNumber-Test";
            props.Platform = "Platform-Test";
            props.PowerSourceVoltage = 1234;
            props.Processor = "Processor-Test";
            props.SerialNumber = "SerialNumber-Test";
            props.UpdatedTime = new DateTime(2000, 01, 01);

            return device;
        }

        #endregion
    }
}
