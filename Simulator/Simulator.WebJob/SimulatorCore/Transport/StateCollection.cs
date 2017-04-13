using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport
{
    static class StateCollection<T> where T : struct
    {
        private static Dictionary<string, T> States = new Dictionary<string, T>();

        public static void Set(string key, T state)
        {
            lock (States)
            {
                States[key] = state;
            }
        }

        public static void Remove(string key)
        {
            lock (States)
            {
                States.Remove(key);
            }
        }

        public static void GetRatio(T state, out int selected, out int total)
        {
            lock (States)
            {
                selected = States.Values.Count(v => v.Equals(state));
                total = States.Count;
            }
        }
    }
}