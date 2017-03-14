using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions
{
    public static class ProcessExtensions
    {
        static private string _processCategory = "Process";
        static private string _pidCounter = "ID Process";
        static private string _parentCounter = "Creating Process ID";

        public static IEnumerable<string> GetAncestorNames(this Process process)
        {
            while (true)
            {
                process = process.GetParent();
                if (process == null)
                {
                    break;
                }

                yield return process.ProcessName;
            }
        }

        public static Process GetParent(this Process process)
        {
            for (int idx = 0; ; idx++)
            {
                var name = process.ProcessName;
                if (idx > 0)
                {
                    name += FormattableString.Invariant($"#{idx}");
                }

                try
                {

                    using (var pidReader = new PerformanceCounter(_processCategory, _pidCounter, name))
                    {
                        if ((int)pidReader.NextValue() != process.Id)
                        {
                            continue;
                        }

                        using (var parentReader = new PerformanceCounter(_processCategory, _parentCounter, name))
                        {
                            return Process.GetProcessById((int)parentReader.NextValue());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(FormattableString.Invariant($"Exception raised in GetParentname: {ex}"));
                    return null;
                }
            }
        }
    }
}
