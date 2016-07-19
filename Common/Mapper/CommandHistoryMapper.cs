using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Mapper
{
    public class CommandHistoryMapper : Mapper<CommandHistoryND>
    {
        private static CommandHistoryMapper cm;
        private CommandHistoryMapper() : base(new CustomLogic<CommandHistoryND>())
        {
        }
        public static CommandHistoryMapper Get()
        {
            return cm ?? (cm = new CommandHistoryMapper());
        }
    }
}
