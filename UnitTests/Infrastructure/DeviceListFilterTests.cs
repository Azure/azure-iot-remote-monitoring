using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceListFilterTests
    {
        [Fact]
        public void GetSQLQueryTest()
        {
            string sql = BuildFilter().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices WHERE tags.x = 'one' AND properties.desired.y < '1' AND properties.reported.z > '1' AND tags.u != 'two' AND properties.desired.v <= '2' AND properties.reported.w >= '2' AND deviceId IN ['CoolingSampleDevice001', 'CoolingSampleDevice002', 'CoolingSampleDevice003']");

            sql = BuildEmptyFilter().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices");

            sql = BuildNullFilter().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices");

            sql = BuildFilterWithEmptyClauseValue().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices WHERE tag.x != ''");

            sql = BuildFilterWithoutPropertiesPrefix().GetSQLQuery();
            Assert.Equal(sql, "SELECT * FROM devices WHERE tags.x = 'one' AND properties.desired.y < '1' AND properties.reported.z > '1'");
        }

        [Fact]
        public void GetSQLCondition()
        {
            string condition = BuildFilter().GetSQLCondition();
            Assert.Equal(condition, "tags.x = 'one' AND properties.desired.y < '1' AND properties.reported.z > '1' AND tags.u != 'two' AND properties.desired.v <= '2' AND properties.reported.w >= '2' AND deviceId IN ['CoolingSampleDevice001', 'CoolingSampleDevice002', 'CoolingSampleDevice003']");

            condition = BuildEmptyFilter().GetSQLCondition();
            Assert.Equal(condition, string.Empty);

            condition = BuildNullFilter().GetSQLCondition();
            Assert.Equal(condition, string.Empty);

            condition = BuildFilterWithEmptyClauseValue().GetSQLCondition();
            Assert.Equal(condition, "tag.x != ''");

            condition = BuildFilterWithoutPropertiesPrefix().GetSQLCondition();
            Assert.Equal(condition, "tags.x = 'one' AND properties.desired.y < '1' AND properties.reported.z > '1'");
        }

        [Fact]
        public void GetSQLConditionShouldThrowNotSupportedExceptionForInvalidOperators()
        {
            Assert.Throws<NotSupportedException>(() => BuildInvalidFilter().GetSQLCondition());
        }

        private DeviceListFilter BuildFilter()
        {
            return new DeviceListFilter
            {
                Clauses = new List<Clause>
                {
                    new Clause
                    {
                        ColumnName = "tags.x",
                        ClauseType = ClauseType.EQ,
                        ClauseValue = "one"
                    },
                    new Clause
                    {
                        ColumnName = "properties.desired.y",
                        ClauseType = ClauseType.LT,
                        ClauseValue = "1"
                    },
                    new Clause
                    {
                        ColumnName = "properties.reported.z",
                        ClauseType = ClauseType.GT,
                        ClauseValue = "\'1\'"
                    },
                    new Clause
                    {
                        ColumnName = "tags.u",
                        ClauseType = ClauseType.NE,
                        ClauseValue = "two",
                    },
                    new Clause
                    {
                        ColumnName = "properties.desired.v",
                        ClauseType = ClauseType.LE,
                        ClauseValue = "2"
                    },
                    new Clause
                    {
                        ColumnName = "properties.reported.w",
                        ClauseType = ClauseType.GE,
                        ClauseValue = "\'2\'"
                    },
                    new Clause
                    {
                        ColumnName = "deviceId",
                        ClauseType = ClauseType.IN,
                        ClauseValue = "['CoolingSampleDevice001', 'CoolingSampleDevice002', 'CoolingSampleDevice003']"
                    }
                }
            };
        }

        private DeviceListFilter BuildEmptyFilter()
        {
            return new DeviceListFilter
            {
                Clauses = new List<Clause>
                {
                    new Clause(),
                    new Clause(),
                }
            };
        }

        private DeviceListFilter BuildNullFilter()
        {
            return new DeviceListFilter();
        }

        private DeviceListFilter BuildInvalidFilter()
        {
            return new DeviceListFilter
            {
                Clauses = new List<Clause>
                {
                    new Clause
                    {
                        ColumnName = "tags.x",
                        ClauseType = ClauseType.ContainsCaseInsensitive,
                        ClauseValue = "one"
                    }
                }
            };
        }

        private DeviceListFilter BuildFilterWithEmptyClauseValue()
        {
            return new DeviceListFilter
            {
                Clauses = new List<Clause>
                {
                    new Clause
                    {
                        ColumnName = "tag.x",
                        ClauseType = ClauseType.NE,
                        ClauseValue = string.Empty
                    }
                }
            };
        }

        private DeviceListFilter BuildFilterWithoutPropertiesPrefix()
        {
            return new DeviceListFilter
            {
                Clauses = new List<Clause>
                {
                    new Clause
                    {
                        ColumnName = "tags.x",
                        ClauseType = ClauseType.EQ,
                        ClauseValue = "one"
                    },
                    new Clause
                    {
                        ColumnName = "desired.y",
                        ClauseType = ClauseType.LT,
                        ClauseValue = "1"
                    },
                    new Clause
                    {
                        ColumnName = "reported.z",
                        ClauseType = ClauseType.GT,
                        ClauseValue = "\'1\'"
                    },
                }
            };
        }
    }
}
