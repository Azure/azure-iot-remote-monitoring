using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class ClauseTableEntity : TableEntity
    {
        public ClauseTableEntity(Clause clause)
        {
            ClauseValue = clause.ClauseValue;
            ClauseType = clause.ClauseType.ToString();
            ClauseDataType = clause.ClauseDataType.ToString();
            RowKey = ColumnName = clause.ColumnName;
            PartitionKey = FormattableString.Invariant($"{clause.ColumnName} {clause.ClauseType.ToString()} {ClauseValue.NormalizedTableKey()} {clause.ClauseDataType.ToString()}");
            HitCounter = 1;
        }

        public ClauseTableEntity() { }

        public string ColumnName { get; set; }
        public string ClauseType { get; set; }
        public string ClauseValue { get; set; }
        public string ClauseDataType { get; set; }
        public long HitCounter { get; set; }
    }
}
