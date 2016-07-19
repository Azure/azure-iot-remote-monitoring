using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Mapper
{
    public class CommandMapper : Mapper<Command>
    {
        private static CommandMapper cm;
        private CommandMapper() : base(new CustomLogic<Command>())
        {
        }
        public static CommandMapper Get()
        {
            return cm ?? (cm = new CommandMapper());
        }
    }
}
