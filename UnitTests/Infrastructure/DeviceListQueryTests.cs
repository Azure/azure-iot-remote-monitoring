using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceListQueryTests
    {
        [Fact]
        public void GetSQLQueryTest()
        {
            string sql = BuildQuery().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices WHERE tags.x = 'one' AND properties.desired.y < 1 AND properties.reported.z > '1' AND tags.u != 'two' AND properties.desired.v <= 2 AND properties.reported.w >= '2' AND deviceId IN ['SampleDevice001', 'SampleDevice002', 'SampleDevice003']");

            sql = BuildEmptyQuery().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices");

            sql = BuildNullQuery().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices");

            sql = BuildQueryWithEmptyFilterValue().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices WHERE tag.x != ''");

            sql = BuildQueryWithoutPropertiesPrefix().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices WHERE tags.x = 'one' AND properties.desired.y < 1 AND properties.reported.z > '1'");
        }

        [Fact]
        public void GetSQLCondition()
        {
            string condition = BuildQuery().GetSQLCondition();
            Assert.Equal(condition, "tags.x = 'one' AND properties.desired.y < 1 AND properties.reported.z > '1' AND tags.u != 'two' AND properties.desired.v <= 2 AND properties.reported.w >= '2' AND deviceId IN ['SampleDevice001', 'SampleDevice002', 'SampleDevice003']");

            condition = BuildEmptyQuery().GetSQLCondition();
            Assert.Equal(condition, string.Empty);

            condition = BuildNullQuery().GetSQLCondition();
            Assert.Equal(condition, string.Empty);

            condition = BuildQueryWithEmptyFilterValue().GetSQLCondition();
            Assert.Equal(condition, "tag.x != ''");

            condition = BuildQueryWithoutPropertiesPrefix().GetSQLCondition();
            Assert.Equal(condition, "tags.x = 'one' AND properties.desired.y < 1 AND properties.reported.z > '1'");
        }

        [Fact]
        public void GetSQLConditionShouldThrowNotSupportedExceptionForInvalidOperators()
        {
            Assert.Throws<NotSupportedException>(() => BuildInvalidQuery().GetSQLCondition());
        }

        private DeviceListQuery BuildQuery()
        {
            return new DeviceListQuery
            {
                Filters = new List<FilterInfo>
                {
                    new FilterInfo
                    {
                        ColumnName = "tags.x",
                        FilterType = FilterType.EQ,
                        FilterValue = "one"
                    },
                    new FilterInfo
                    {
                        ColumnName = "properties.desired.y",
                        FilterType = FilterType.LT,
                        FilterValue = "1"
                    },
                    new FilterInfo
                    {
                        ColumnName = "properties.reported.z",
                        FilterType = FilterType.GT,
                        FilterValue = "\'1\'"
                    },
                    new FilterInfo
                    {
                        ColumnName = "tags.u",
                        FilterType = FilterType.NE,
                        FilterValue = "two",
                    },
                    new FilterInfo
                    {
                        ColumnName = "properties.desired.v",
                        FilterType = FilterType.LE,
                        FilterValue = "2"
                    },
                    new FilterInfo
                    {
                        ColumnName = "properties.reported.w",
                        FilterType = FilterType.GE,
                        FilterValue = "\'2\'"
                    },
                    new FilterInfo
                    {
                        ColumnName = "deviceId",
                        FilterType = FilterType.IN,
                        FilterValue = "['SampleDevice001', 'SampleDevice002', 'SampleDevice003']"
                    }
                }
            };
        }

        private DeviceListQuery BuildEmptyQuery()
        {
            return new DeviceListQuery
            {
                Filters = new List<FilterInfo>
                {
                    new FilterInfo(),
                    new FilterInfo(),
                }
            };
        }

        private DeviceListQuery BuildNullQuery()
        {
            return new DeviceListQuery();
        }

        private DeviceListQuery BuildInvalidQuery()
        {
            return new DeviceListQuery
            {
                Filters = new List<FilterInfo>
                {
                    new FilterInfo
                    {
                        ColumnName = "tags.x",
                        FilterType = FilterType.ContainsCaseInsensitive,
                        FilterValue = "one"
                    }
                }
            };
        }

        private DeviceListQuery BuildQueryWithEmptyFilterValue()
        {
            return new DeviceListQuery
            {
                Filters = new List<FilterInfo>
                {
                    new FilterInfo
                    {
                        ColumnName = "tag.x",
                        FilterType = FilterType.NE,
                        FilterValue = string.Empty
                    }
                }
            };
        }

        private DeviceListQuery BuildQueryWithoutPropertiesPrefix()
        {
            return new DeviceListQuery
            {
                Filters = new List<FilterInfo>
                {
                    new FilterInfo
                    {
                        ColumnName = "tags.x",
                        FilterType = FilterType.EQ,
                        FilterValue = "one"
                    },
                    new FilterInfo
                    {
                        ColumnName = "desired.y",
                        FilterType = FilterType.LT,
                        FilterValue = "1"
                    },
                    new FilterInfo
                    {
                        ColumnName = "reported.z",
                        FilterType = FilterType.GT,
                        FilterValue = "\'1\'"
                    },
                }
            };
        }
    }
}
