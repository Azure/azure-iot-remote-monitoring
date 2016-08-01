using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity
{
    internal static class Utilities
    {
        public static string[] ConvertIccidListToStringArray(List<Iccid> iccidList)
        {
            Argument.CheckIfNull(iccidList, "iccidList");

            var stringArray = new string[iccidList.Count];
            for (var i = 0; i < iccidList.Count; i++)
            {
                stringArray[i] = iccidList.ElementAt(i).Id;
            }
            return stringArray;
        }
    }
}