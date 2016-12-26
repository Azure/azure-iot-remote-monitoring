using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Stores all data needed to filter the devices (sorting, filtering etc)
    /// </summary>
    public class DeviceListFilter : ICloneable
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
        public string GetSQLQuery(bool isCountQuery = false)
        {
            string queryWithoutCondition = isCountQuery? "SELECT COUNT() AS total FROM devices":"SELECT * FROM devices";

            if (IsAdvanced)
            {
                if (!string.IsNullOrWhiteSpace(AdvancedClause))
                {
                    return $"{queryWithoutCondition} WHERE {AdvancedClause}";   
                }
            }
            else
            {
                string condition = GetSQLCondition();
                if (!string.IsNullOrWhiteSpace(condition))
                {
                    return $"{queryWithoutCondition} WHERE {condition}";
                }
            }

            return queryWithoutCondition;
        }

        /// <summary>
        /// Return a new DeviceListFilter and append newClause to new filter
        /// </summary>
        /// <param name="newClause">new DeviceListFilter with new clause</param>
        public DeviceListFilter AddClause(Clause newClause)
        {
            DeviceListFilter cloneFilter = this.Clone() as DeviceListFilter;
            cloneFilter.Clauses.Add(newClause);
            return cloneFilter;
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
                        case ClauseType.ISDEFINED: op = "is_defined({0})"; break;
                        default: throw new NotSupportedException();
                    }

                    var value = filter.ClauseValue;

                    // For syntax reason, the value should be surrounded by ''
                    // This feature will be skipped if the value is a number. To compare a number as string, user should surround it by '' manually                    
                    if (filter.ClauseType != ClauseType.IN &&
                        filter.ClauseType != ClauseType.ISDEFINED &&
                        !(value.All(c => char.IsDigit(c)) && value.Any()) &&
                        !value.StartsWith("\'") &&
                        !value.EndsWith("\'"))
                    {
                        var items = value.TrimStart('[').TrimEnd(']').Split(',');
                        for (var i = 0; i < items.Length; i++)
                        {
                            var item = items[i].Trim();
                            items[i] = AddQuoteIfNeeded(item);
                        }

                        value = $"[{string.Join(", ", items)}]";
                    }
                    else
                    {
                        value = AddQuoteIfNeeded(value);
                    }

                    var twinPropertyName = filter.ColumnName;
                    if (this.IsProperties(twinPropertyName))
                    {
                        twinPropertyName = $"properties.{filter.ColumnName}";
                    }

                    if (filter.ClauseType == ClauseType.ISDEFINED)
                    {
                        return string.Format(op, twinPropertyName);
                    }
                    else
                    {
                        return $"{twinPropertyName} {op} {value}";
                    }
                }).ToList();

            return filters == null ? string.Empty : string.Join(" AND ", filters);
        }

        private bool IsProperties(string name)
        {
            return name.StartsWith("reported.") || name.StartsWith("desired.");
        }

        public object Clone()
        {
            return new DeviceListFilter()
            {
                Id = this.Id,
                Name = this.Name,
                Clauses = this.Clauses.ToList(),
                SearchQuery = this.SearchQuery,
                SortColumn = this.SortColumn,
                SortOrder = this.SortOrder,
                AdvancedClause = this.AdvancedClause,
                Skip = this.Skip,
                Take = this.Take,
                IsAdvanced = this.IsAdvanced,
                IsTemporary = this.IsTemporary
            };
        }

        private string AddQuoteIfNeeded(string value)
        {
            if (!(value.All(c => char.IsDigit(c)) && value.Any()) &&
                !value.StartsWith("\'") &&
                !value.EndsWith("\'"))
            {
                value = $"\'{value}\'";
            }

            return value;
        }
    }
}
