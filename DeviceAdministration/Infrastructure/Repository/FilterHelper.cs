using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Testable logic for filtering devices in DocDB
    /// </summary>
    public static class FilterHelper
    {
        /// <summary>
        /// Filters the device list with the supplied filters
        /// </summary>
        /// <param name="list">Devices to filter</param>
        /// <param name="clauses">Clauses to apply</param>
        /// <returns>Set of devices that pass all the filters</returns>
        public static IQueryable<DeviceModel> FilterDeviceList(
            IQueryable<DeviceModel> list,
            List<Clause> clauses)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (clauses == null)
            {
                return list;
            }

            list = list.Where(GetIsNotNull).AsQueryable();

            foreach (var f in clauses)
            {
                if ((f != null) && !string.IsNullOrEmpty(f.ColumnName))
                {
                    list = FilterItems(list, f);
                }
            }

            return list;
        }

        private static IQueryable<DeviceModel> FilterItems(
            IQueryable<DeviceModel> list,
            Clause filter)
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

            Func<DeviceProperties, dynamic> getValue = ReflectionHelper.ProducePropertyValueExtractor(
                    filter.ColumnName,
                    false,
                    false);

            Func<DeviceModel, bool> applyFilter = (item) =>
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                if ((filter.ClauseType == ClauseType.Status) ||
                    string.Equals(
                        filter.ColumnName,
                        "Status",
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    return GetValueMatchesStatus(item, filter.ClauseValue);
                }

                if (item.DeviceProperties == null)
                {
                    return false;
                }

                dynamic columnValue = getValue(item.DeviceProperties);
                return GetValueSatisfiesFilter(columnValue, filter);
            };

            return list.Where(applyFilter).AsQueryable();
        }

        private static bool GetIsNotNull(dynamic item)
        {
            return item != null;
        }

        private static bool GetValueMatchesStatus(DeviceModel item, string statusName)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (string.IsNullOrEmpty(statusName))
            {
                return false;
            }

            var normalizedStatus = statusName.ToUpperInvariant();
            var enabledState = item.DeviceProperties?.HubEnabledState == null ? (bool?) null : item.DeviceProperties.GetHubEnabledState();

            switch (normalizedStatus)
            {
                case "RUNNING":
                    return enabledState == true;

                case "DISABLED":
                    return enabledState == false;

                case "PENDING":
                    return !enabledState.HasValue;

                default:
                    throw new ArgumentOutOfRangeException("statusName", statusName, "statusName has an unhandled status value.");
            }
        }

        private static bool GetValueSatisfiesFilter(
            dynamic value,
            Clause filterInfo)
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

            string match = filterInfo.ClauseValue ?? string.Empty;

            switch (filterInfo.ClauseType)
            {
                case ClauseType.ContainsCaseInsensitive:
                    return strVal.IndexOf(match, StringComparison.CurrentCultureIgnoreCase) >= 0;

                case ClauseType.ContainsCaseSensitive:
                    return strVal.IndexOf(match, StringComparison.CurrentCulture) >= 0;

                case ClauseType.ExactMatchCaseInsensitive:
                    return string.Equals(strVal, match, StringComparison.CurrentCultureIgnoreCase);

                case ClauseType.ExactMatchCaseSensitive:
                    return string.Equals(strVal, match, StringComparison.CurrentCulture);

                case ClauseType.StartsWithCaseInsensitive:
                    return strVal.StartsWith(match, StringComparison.CurrentCultureIgnoreCase);

                case ClauseType.StartsWithCaseSensitive:
                    return strVal.StartsWith(match, StringComparison.CurrentCulture);
            }

            return false;
        }
    }
}
