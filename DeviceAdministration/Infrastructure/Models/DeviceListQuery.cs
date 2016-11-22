using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Stores all data needed to query the devices (sorting, filtering, searching, etc)
    /// </summary>
    public class DeviceListQuery
    {
        /// <summary>
        /// Name saved for the query
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Column-level filter values (can have zero or more)
        /// </summary>
        public List<FilterInfo> Filters { get; set; }

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
        public string Sql { get; set; }

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
        /// this is a filter based query or advanced query defined by user.
        /// </summary>
        public bool IsAdvanced { get; set; }

        /// <summary>
        /// Translate the filters in current query to IoT Hub SQL query
        /// Full text searching, paging and sorting are not supported by the IoT Hub SQL query until now
        /// </summary>
        /// <returns>The full SQL query</returns>
        public string GetSQLQuery()
        {
            return IsAdvanced ? Sql : GetFilterQuery();
        }

        /// <summary>
        /// Translate the filters in current query to IoT Hub SQL query condition
        /// Full text searching, paging and sorting are not supported by the IoT Hub SQL query until now
        /// </summary>
        /// <returns>The query condition, or empty string if no valid filter found</returns>
        public string GetSQLCondition()
        {
            var filters = Filters?.
                Where(filter => !string.IsNullOrWhiteSpace(filter.ColumnName))?.
                Select(filter =>
                {
                    string op = null;

                    switch (filter.FilterType)
                    {
                        case FilterType.EQ: op = "="; break;
                        case FilterType.NE: op = "!="; break;
                        case FilterType.LT: op = "<"; break;
                        case FilterType.GT: op = ">"; break;
                        case FilterType.LE: op = "<="; break;
                        case FilterType.GE: op = ">="; break;
                        case FilterType.IN: op = "IN"; break;
                        default: throw new NotSupportedException();
                    }

                    var value = filter.FilterValue;

                    // For syntax reason, the value should be surrounded by ''
                    // This feature will be skipped if the value is a number. To compare a number as string, user should surround it by '' manually                    
                    if (filter.FilterType != FilterType.IN &&
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
