using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Translate the filters in current query to IoT Hub SQL query
        /// Full text searching, paging and sorting are not supported by the IoT Hub SQL query until now
        /// </summary>
        /// <returns>The full SQL query</returns>
        public string GetSQLQuery()
        {
            return IsAdvanced ? AdvancedClause : GetFilterQuery();
        }

        /// <summary>
        /// Translate the filters in current query to IoT Hub SQL query condition
        /// Full text searching, paging and sorting are not supported by the IoT Hub SQL query until now
        /// </summary>
        /// <returns>The query condition, or empty string if no valid filter found</returns>
        public string GetSQLCondition()
        {
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
                    if (filter.ClauseType != ClauseType.IN &&
                        !(value.All(c => char.IsDigit(c)) && value.Any()) &&
                        !value.StartsWith("\'") &&
                        !value.EndsWith("\'"))
                    {
                        value = $"\'{value}\'";
                    }
                    if (filter.ColumnName.StartsWith("reported.") || filter.ColumnName.StartsWith("desired."))
                    {
                        return $"properties.{filter.ColumnName} {op} {value}";
                    }
                    else
                    {
                        return $"{filter.ColumnName} {op} {value}";
                    }
                }).ToList();

            return filters == null ? string.Empty : string.Join(" AND ", filters);
        }

        public string GetFilterQuery()
        {
            var condition = GetSQLCondition();

            string filterQuery = "SELECT * FROM devices";
            if (!string.IsNullOrWhiteSpace(condition))
            {
                filterQuery = $"{filterQuery} WHERE {condition}";
            }
            return filterQuery;
        }
    }
}
