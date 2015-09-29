using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class TableStorageResponse<T>
    {
        public T Entity { get; set; }
        public TableStorageResponseStatus Status { get; set; }
    }

    public enum TableStorageResponseStatus
    {
        Successful, ConflictError, UnknownError, DuplicateInsert, NotFound
    }
}
