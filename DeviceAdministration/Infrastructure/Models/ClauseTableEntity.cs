using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class ClauseTableEntity : TableEntity
    {
        public ClauseTableEntity(Clause clause)
        {
            ClauseValue = clause.ClauseValue;
            ClauseType = clause.ClauseType.ToString();
            RowKey = ColumnName = clause.ColumnName;
            PartitionKey = $"{clause.ColumnName} {clause.ClauseType.ToString()} {ClauseValue.NormalizedTableKey()}";
        }

        public ClauseTableEntity() { }

        public string ColumnName { get; set; }
        public string ClauseType { get; set; }
        public string ClauseValue { get; set; }
    }
}
