using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Testable logic for filtering devices in DocDB
    /// </summary>
    internal static class FilterHelper
    {
        /// <summary>
        /// Filters the device list with the supplied filters
        /// </summary>
        /// <param name="list">Devices to filter</param>
        /// <param name="filters">Filters to apply</param>
        /// <returns>Set of devices that pass all the filters</returns>
        public static IQueryable<dynamic> FilterDeviceList(
            IQueryable<dynamic> list, 
            List<FilterInfo> filters)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (filters == null)
            {
                return list;
            }

            list = list.Where(GetIsNotNull).AsQueryable();

            foreach (var f in filters)
            {
                if ((f != null) && !string.IsNullOrEmpty(f.ColumnName))
                {
                    list = FilterItems(list, f);
                }
            }

            return list;
        }

        private static IQueryable<dynamic> FilterItems(
            IQueryable<dynamic> list, 
            FilterInfo filter)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (string.IsNullOrEmpty(filter.ColumnName))
            {
                throw new ArgumentException(
                    "filter.ColumnName is a null reference or empty string.",
                    "filter");
            }

            Func<dynamic, dynamic> getValue =
                ReflectionHelper.ProducePropertyValueExtractor(
                    filter.ColumnName, 
                    false, 
                    false);

            Func<dynamic, bool> applyFilter =
                (item) =>
                {
                    dynamic columnValue;
                    dynamic deviceProperties;

                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }

                    if ((filter.FilterType == FilterType.Status) ||
                        string.Equals(
                            filter.ColumnName,
                            "Status",
                            StringComparison.CurrentCultureIgnoreCase))
                    {
                        return GetValueMatchesStatus(item, filter.FilterValue);
                    }

                    try
                    {
                        deviceProperties = DeviceSchemaHelper.GetDeviceProperties(item);
                    }
                    catch (DeviceRequiredPropertyNotFoundException)
                    {
                        return false;
                    }

                    columnValue = getValue(deviceProperties);
                    return GetValueSatisfiesFilter(columnValue, filter);
                };

            return list.Where(applyFilter).AsQueryable();
        }

        private static bool GetIsNotNull(dynamic item)
        {
            return item != null;
        }

        private static bool GetValueMatchesStatus(dynamic item, string statusName)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (string.IsNullOrEmpty(statusName))
            {
                return false;
            }

            string normalizedStatus = statusName.ToUpperInvariant();
            bool? value;
            try
            {
                value = DeviceSchemaHelper.GetHubEnabledState(item);
            }
            catch (DeviceRequiredPropertyNotFoundException)
            {
                value = null;
            }

            switch (normalizedStatus)
            {
                case "RUNNING":
                    return value == true;

                case "DISABLED":
                    return value == false;

                case "PENDING":
                    return !value.HasValue;

                default:
                    throw new ArgumentOutOfRangeException("statusName", statusName, "statusName has an unhandled status value.");
            }
        }

        private static bool GetValueSatisfiesFilter(
            dynamic value,
            FilterInfo filterInfo)
        {
            string strVal;

            if (value == null)
            {
                strVal = string.Empty;
            }
            else
            {
                strVal = value.ToString();
            }

            string match = filterInfo.FilterValue ?? string.Empty;

            switch (filterInfo.FilterType)
            {
                case FilterType.ContainsCaseInsensitive:
                    return strVal.IndexOf(match, StringComparison.CurrentCultureIgnoreCase) >= 0;

                case FilterType.ContainsCaseSensitive:
                    return strVal.IndexOf(match, StringComparison.CurrentCulture) >= 0;

                case FilterType.ExactMatchCaseInsensitive:
                    return string.Equals(strVal, match, StringComparison.CurrentCultureIgnoreCase);

                case FilterType.ExactMatchCaseSensitive:
                    return string.Equals(strVal, match, StringComparison.CurrentCulture);

                case FilterType.StartsWithCaseInsensitive:
                    return strVal.StartsWith(match, StringComparison.CurrentCultureIgnoreCase);

                case FilterType.StartsWithCaseSensitive:
                    return strVal.StartsWith(match, StringComparison.CurrentCulture);
            }

            return false;
        }
    }
}
