using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class FilterHelperTests
    {
        [Fact]
        public void NoFilterShouldReturnEverything()
        {
            TestFilter(new List<Clause>(), 10);
        }

        #region Test Group: column names should not be case sensitive

        [Fact]
        public void FilterWithColumnNameDiffCaseShouldWork()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DEViceid",
                    ClauseType = ClauseType.ExactMatchCaseSensitive,
                    ClauseValue = "The one special value"
                }
            };

            TestFilter(filters, 1);
        }

        #endregion

        #region Test Group: filtering on missing value should remove all devices

        [Fact]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_Exact_CaseSensitive()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseSensitive,
                    ClauseValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        [Fact]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_Exact_CaseInsensitive()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseInsensitive,
                    ClauseValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        [Fact]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_StartsWith_CaseSensitive()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.StartsWithCaseSensitive,
                    ClauseValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        [Fact]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_StartsWith_CaseInsensitive()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.StartsWithCaseInsensitive,
                    ClauseValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        [Fact]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_Contains_CaseSensitive()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ContainsCaseSensitive,
                    ClauseValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        [Fact]
        public void FilterWithMissingValueShouldBeAbleToRemoveAll_Contains_CaseInsensitive()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = "DKFSLKFJDKKD"
                }
            };

            TestFilter(filters, 0);
        }

        #endregion

        #region Test Group: filter should return 1 device

        [Fact]
        public void FilterShouldReturnOneDevice_Exact_CaseSensitive()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseSensitive,
                    ClauseValue = "The one special value"
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void FilterShouldReturnOneDevice_Exact_CaseInsensitive_SameCase()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseInsensitive,
                    ClauseValue = "The one special value"
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void FilterShouldReturnOneDevice_Exact_CaseInsensitive_DiffCase()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseInsensitive,
                    ClauseValue = "The ONE SPECIAL VALUe"
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void FilterShouldReturnOneDevice_StartsWith_CaseInsensitive_SameCase()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.StartsWithCaseInsensitive,
                    ClauseValue = "The "
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void FilterShouldReturnOneDevice_StartsWith_CaseInsensitive_DiffCase()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.StartsWithCaseInsensitive,
                    ClauseValue = "ThE "
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void FilterShouldReturnOneDevice_StartsWith_CaseSensitive()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.StartsWithCaseSensitive,
                    ClauseValue = "The "
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void FilterShouldReturnOneDevice_Contains_CaseSensitive()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ContainsCaseSensitive,
                    ClauseValue = " special "
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void FilterShouldReturnOneDevice_Contains_CaseInsensitive_SameCase()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = " special "
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void FilterShouldReturnOneDevice_Contains_CaseInsensitive_DiffCase()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = " spECial "
                }
            };

            TestFilter(filters, 1);
        }

        #endregion

        #region Test Group: case sensitive filters should be case sensitive

        [Fact]
        public void CaseSensitiveFilterShouldReturnNothing_Exact_CaseSensitive_DiffCase()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseSensitive,
                    ClauseValue = "The One Special Value"
                }
            };

            TestFilter(filters, 0);
        }

        [Fact]
        public void CaseSensitiveFilterShouldReturnNothing_StartsWith_CaseSensitive_DiffCase()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.StartsWithCaseSensitive,
                    ClauseValue = "ThE One"
                }
            };

            TestFilter(filters, 0);
        }

        [Fact]
        public void CaseSensitiveFilterShouldReturnNothing_Contains_CaseSensitive_DiffCase()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ContainsCaseSensitive,
                    ClauseValue = " speciAl "
                }
            };

            TestFilter(filters, 0);
        }

        #endregion

        #region Test Group: multiple filters should work together

        [Fact]
        public void MultipleFiltersShouldWorkTogether_RemoveAllDevices()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseInsensitive,
                    ClauseValue = "The One Special Value"
                },
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = "dog"
                }
            };

            TestFilter(filters, 0);
        }

        [Fact]
        public void MultipleFiltersShouldWorkTogether_ReturnOneDevice()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseInsensitive,
                    ClauseValue = "The One Special Value"
                },
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = "value"
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void MultipleFiltersShouldWorkTogether_DifferentColumns_ReturnOneDevice()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseInsensitive,
                    ClauseValue = "The One Special Value"
                },
                new Clause
                {
                    ColumnName = "DeviceState",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = "devicestate"
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void MultipleFiltersShouldWorkTogether_DifferentColumns_ReturnNoDevices()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseInsensitive,
                    ClauseValue = "The One Special Value"
                },
                new Clause
                {
                    ColumnName = "DeviceState",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = "Ada"
                }
            };

            TestFilter(filters, 0);
        }

        [Fact]
        public void MultipleTripleFiltersShouldWorkTogether_DifferentColumns_ReturnOneDevice()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseInsensitive,
                    ClauseValue = "The One Special Value" // passes one device
                },
                new Clause
                {
                    ColumnName = "DeviceState",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = "State" // passes all devices
                },
                new Clause
                {
                    ColumnName = "FirmwareVersion",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = "WARE" // passes all devices
                }
            };

            TestFilter(filters, 1);
        }

        [Fact]
        public void MultipleTripleFiltersShouldWorkTogether_DifferentColumns_ReturnNoDevices()
        {
            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = ClauseType.ExactMatchCaseInsensitive,
                    ClauseValue = "The One Special Value" // passes one device
                },
                new Clause
                {
                    ColumnName = "DeviceState",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = "State" // passes all devices
                },
                new Clause
                {
                    ColumnName = "FirmwareVersion",
                    ClauseType = ClauseType.ContainsCaseInsensitive,
                    ClauseValue = "nope" // passes no devices
                }
            };

            TestFilter(filters, 0);
        }

        #endregion

        #region Test Group: missing data in devices should not throw

        #region Sub-test Group: non-null DeviceProperties but missing property off of DeviceProperties should not throw

        [Fact]
        public void FilteringDeviceWithNullDeviceIdShouldWork_ContainsCaseInsensitive()
        {
            TestNullDeviceId(ClauseType.ContainsCaseInsensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDeviceIdShouldWork_ContainsCaseSensitive()
        {
            TestNullDeviceId(ClauseType.ContainsCaseSensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDeviceIdShouldWork_ExactMatchCaseInsensitive()
        {
            TestNullDeviceId(ClauseType.ExactMatchCaseInsensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDeviceIdShouldWork_ExactMatchCaseSensitive()
        {
            TestNullDeviceId(ClauseType.ExactMatchCaseSensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDeviceIdShouldWork_StartsWithCaseInsensitive()
        {
            TestNullDeviceId(ClauseType.StartsWithCaseInsensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDeviceIdShouldWork_StartsWithCaseSensitive()
        {
            TestNullDeviceId(ClauseType.StartsWithCaseSensitive);
        }

        private void TestNullDeviceId(ClauseType filterType)
        {
            var devicesWithNullDeviceId = GetListWithOneSpecialDeviceIdValue(null);

            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = filterType,
                    ClauseValue = "x"
                }
            };

            var filtered = FilterHelper.FilterDeviceList(devicesWithNullDeviceId, filters);

            Assert.Equal(0, filtered.Count());
        }

        #endregion

        #region Sub-test Group: null DeviceProperties should not throw

        [Fact]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_ContainsCaseInsensitive()
        {
            TestNullDeviceProperties(ClauseType.ContainsCaseInsensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_ContainsCaseSensitive()
        {
            TestNullDeviceProperties(ClauseType.ContainsCaseSensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_ExactMatchCaseInsensitive()
        {
            TestNullDeviceProperties(ClauseType.ExactMatchCaseInsensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_ExactMatchCaseSensitive()
        {
            TestNullDeviceProperties(ClauseType.ExactMatchCaseSensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_StartsWithCaseInsensitive()
        {
            TestNullDeviceProperties(ClauseType.StartsWithCaseInsensitive);
        }

        [Fact]
        public void FilteringDeviceWithNullDevicePropertiesShouldWork_StartsWithCaseSensitive()
        {
            TestNullDeviceProperties(ClauseType.StartsWithCaseSensitive);
        }

        private void TestNullDeviceProperties(ClauseType filterType)
        {
            var device = DeviceCreatorHelper.BuildDeviceStructure(Guid.NewGuid().ToString(), true, null);

            device.DeviceProperties = null;

            var list = new List<DeviceModel> {device};

            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "DeviceID",
                    ClauseType = filterType,
                    ClauseValue = "x"
                }
            };

            var filtered = FilterHelper.FilterDeviceList(list.AsQueryable(), filters);

            Assert.Equal(0, filtered.Count());
        }

        #endregion

        #endregion

        #region Test Group: status tests

        [Fact]
        public void Status_PendingShouldReturnBothNullTypes()
        {
            var list = GetListWithEnabledTestValues();

            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "StatuS", // intentionally use weird casing for test
                    ClauseValue = "PendinG"
                }
            };

            var results = FilterHelper.FilterDeviceList(list, filters).ToList();

            Assert.Equal(2, results.Count());
            Assert.Null(results[0].DeviceProperties);
            Assert.Equal("EnabledNull", results[1].DeviceProperties.DeviceID);
        }

        [Fact]
        public void Status_RunningShouldReturnEnabled()
        {
            var list = GetListWithEnabledTestValues();

            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "StatuS", // intentionally use weird casing for test
                    ClauseValue = "RunninG"
                }
            };

            var results = FilterHelper.FilterDeviceList(list, filters).ToList();

            Assert.Equal(1, results.Count());
            Assert.Equal("EnabledTrue", results[0].DeviceProperties.DeviceID);
        }

        [Fact]
        public void Status_DisabledShouldReturnDisabled()
        {
            var list = GetListWithEnabledTestValues();

            var filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "StatuS", // intentionally use weird casing for test
                    ClauseValue = "DisableD"
                }
            };

            var results = FilterHelper.FilterDeviceList(list, filters).ToList();

            Assert.Equal(1, results.Count());
            Assert.Equal("EnabledFalse", results[0].DeviceProperties.DeviceID);
        }

        private static IQueryable<DeviceModel> GetListWithEnabledTestValues()
        {
            var list = GetSampleDevices(4).ToList();

            list[0].DeviceProperties = null;

            list[1].DeviceProperties.HubEnabledState = null;
            list[1].DeviceProperties.DeviceID = "EnabledNull";

            list[2].DeviceProperties.HubEnabledState = true;
            list[2].DeviceProperties.DeviceID = "EnabledTrue";

            list[3].DeviceProperties.HubEnabledState = false;
            list[3].DeviceProperties.DeviceID = "EnabledFalse";

            return list.AsQueryable();
        }

        #endregion

        #region Testing Infrastructure

        [Fact]
        public void FilterTestInfrastructureShouldWork()
        {
            // make sure the infrastructure is working as expected
            var list = GetSampleDevices();

            Assert.Equal(10, list.Count());
        }

        private static void TestFilter(List<Clause> filters, int expectedCount)
        {
            var list = GetListWithOneSpecialDeviceIdValue();

            var filtered = FilterHelper.FilterDeviceList(list, filters);

            Assert.Equal(expectedCount, filtered.Count());
        }

        private static IQueryable<DeviceModel> GetListWithOneSpecialDeviceIdValue(
            string specialDeviceId = "The one special value")
        {
            var list = GetSampleDevices().ToList();

            list[4].DeviceProperties.DeviceID = specialDeviceId;

            return list.AsQueryable();
        }

        private static IQueryable<DeviceModel> GetSampleDevices(int desiredNumberOfDevices = 10)
        {
            var devices = new List<DeviceModel>();

            for (var i = 0; i < desiredNumberOfDevices; ++i)
            {
                devices.Add(GetDefaultTestDevice());
            }

            return devices.AsQueryable();
        }

        private static DeviceModel GetDefaultTestDevice()
        {
            var device = DeviceCreatorHelper.BuildDeviceStructure("DeviceID-Test", true, null);

            device.DeviceProperties.CreatedTime = new DateTime(2000, 01, 01);
            device.DeviceProperties.DeviceState = "DeviceState-Test";
            device.DeviceProperties.HubEnabledState = true;
            device.DeviceProperties.FirmwareVersion = "FirmwareVersion-Test";
            device.DeviceProperties.InstalledRAM = "InstalledRAM-Test";
            device.DeviceProperties.Manufacturer = "Manufacturer-Test";
            device.DeviceProperties.ModelNumber = "ModelNumber-Test";
            device.DeviceProperties.Platform = "Platform-Test";
            device.DeviceProperties.Processor = "Processor-Test";
            device.DeviceProperties.SerialNumber = "SerialNumber-Test";
            device.DeviceProperties.UpdatedTime = new DateTime(2000, 01, 01);

            return device;
        }

        #endregion
    }
}