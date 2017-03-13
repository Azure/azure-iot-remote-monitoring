using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Stores all data needed to filter the devices (sorting, filtering etc)
    /// </summary>
    public class DeviceListFilter
    {
        /// <summary>
        /// An unique id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Name saved for the filter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Column-level filter values (can have zero or more)
        /// </summary>
        public List<Clause> Clauses { get; set; }

        /// <summary>
        /// General, overarching search query (not specific to a column)
        /// (To be obsoleted)
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// Requested sorting column
        /// </summary>
        public string SortColumn { get; set; }

        /// <summary>
        /// Requested sorting order
        /// </summary>
        public QuerySortOrder SortOrder { get; set; }

        /// <summary>
        /// The complete SQL string built from other fields
        /// </summary>
        public string AdvancedClause { get; set; }

        /// <summary>
        /// Number of devices to skip at start of list (if Skip = 50, then 
        /// the first 50 devices will be omitted from the list, and devices will
        /// be returned starting with #51 and on)
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Number of devices to return/display
        /// </summary>
        public int Take { get; set; }

        /// <summary>
        /// Indicate if it is a filter or advanced clause defined by user.
        /// </summary>
        public bool IsAdvanced { get; set; }

        /// <summary>
        /// Indicate if it is a temporary filter.
        /// </summary>
        public bool IsTemporary { get; set; }

        public DeviceListFilter() { }

        public DeviceListFilter(DeviceListFilterTableEntity entity)
        {
            Id = entity.PartitionKey;
            Name = entity.Name;
            Clauses = JsonConvert.DeserializeObject<List<Clause>>(entity.Clauses);
            AdvancedClause = entity.AdvancedClause;
            SortColumn = entity.SortColumn;
            IsAdvanced = entity.IsAdvanced;
            IsTemporary = entity.IsTemporary;
        }

        public DeviceListFilter(Filter filter)
        {
            Id = filter.Id;
            Name = filter.Name;
            Clauses = filter.Clauses;
            AdvancedClause = filter.AdvancedClause;
            IsAdvanced = filter.IsAdvanced;
            IsTemporary = filter.IsTemporary;
        }

        /// <summary>
        /// Translate the filters in current query to IoT Hub SQL query
        /// Full text searching, paging and sorting are not supported by the IoT Hub SQL query until now
        /// </summary>
        /// <returns>The full SQL query</returns>
        public string GetSQLQuery()
        {
            string queryWithoutCondition = "SELECT * FROM devices";

            if (IsAdvanced)
            {
                if (!string.IsNullOrWhiteSpace(AdvancedClause))
                {
                    return FormattableString.Invariant($"{queryWithoutCondition} WHERE {AdvancedClause}");
                }
            }
            else
            {
                string condition = GetSQLCondition();
                if (!string.IsNullOrWhiteSpace(condition))
                {
                    return FormattableString.Invariant($"{queryWithoutCondition} WHERE {condition}");
                }
            }

            return queryWithoutCondition;
        }

        /// <summary>
        /// Translate the filters in current query to IoT Hub SQL query condition
        /// Full text searching, paging and sorting are not supported by the IoT Hub SQL query until now
        /// </summary>
        /// <returns>The query condition, or empty string if no valid filter found</returns>
        public string GetSQLCondition()
        {
            if (IsAdvanced) return AdvancedClause;

            var filters = Clauses?.
                Where(filter => !string.IsNullOrWhiteSpace(filter.ColumnName))?.
                Select(filter =>
                {
                    string op = null;

                    switch (filter.ClauseType)
                    {
                        case ClauseType.EQ: op = "="; break;
                        case ClauseType.NE: op = "!="; break;
                        case ClauseType.LT: op = "<"; break;
                        case ClauseType.GT: op = ">"; break;
                        case ClauseType.LE: op = "<="; break;
                        case ClauseType.GE: op = ">="; break;
                        case ClauseType.IN: op = "IN"; break;
                        default: throw new NotSupportedException();
                    }

                    var value = filter.ClauseValue;

                    // For syntax reason, the value should be surrounded by ''
                    // This feature will be skipped if the value is a number. To compare a number as string, user should surround it by '' manually                    
                    if (filter.ClauseType == ClauseType.IN)
                    {
                        var items = value.TrimStart('[').TrimEnd(']').Split(',');
                        for (var i = 0; i < items.Length; i++)
                        {
                            var item = items[i].Trim();
                            items[i] = AddQuoteIfNeeded(filter.ClauseDataType, item);
                        }

                        value = FormattableString.Invariant($"[{string.Join(", ", items)}]");
                    }
                    else
                    {
                        value = AddQuoteIfNeeded(filter.ClauseDataType, value);
                    }

                    var twinPropertyName = filter.ColumnName;
                    if (this.IsProperties(twinPropertyName))
                    {
                        twinPropertyName = FormattableString.Invariant($"properties.{filter.ColumnName}");
                    }
                    return FormattableString.Invariant($"{twinPropertyName} {op} {value}");

                }).ToList();

            return filters == null ? string.Empty : string.Join(" AND ", filters);
        }

        private bool IsProperties(string name)
        {
            return name.StartsWith("reported.", StringComparison.Ordinal) || name.StartsWith("desired.", StringComparison.Ordinal);
        }

        private string AddQuoteIfNeeded(TwinDataType dataType, string value)
        {
            if (dataType == TwinDataType.String && !value.StartsWith("\'", StringComparison.Ordinal) && !value.EndsWith("\'", StringComparison.Ordinal))
            {
                value = FormattableString.Invariant($"\'{value}\'");
            }

            return value;
        }
    }
}
