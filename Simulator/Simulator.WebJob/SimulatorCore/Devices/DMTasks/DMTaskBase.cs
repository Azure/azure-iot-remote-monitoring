using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices.DMTasks
{
    enum DMTaskState
    {
        DM_NULL,
        DM_IDLE,

        FU_PENDING,
        FU_DOWNLOADING,
        FU_APPLYING,
        FU_REBOOTING,

        CU_PENDING,
        CU_DOWNLOADING,
        CU_APPLYING
    }

    class DMTaskStep
    {
        public DMTaskState CurrentState { get; set; }
        public TimeSpan ExecuteTime { get; set; }
        public DMTaskState NextState { get; set; }
    }

    abstract class DMTaskBase
    {
        abstract protected Task<bool> OnEnterStateProc(DMTaskState state, ITransport transport);
        abstract protected Task<bool> OnLeaveStateProc(DMTaskState state, ITransport transport);

        protected List<DMTaskStep> _steps = new List<DMTaskStep>();

        public async Task Run(ITransport transport)
        {
            DMTaskState state = _steps.First().CurrentState;

            while (true)
            {
                if (!await OnEnterStateProc(state, transport))
                {
                    break;
                }

                var step = _steps.SingleOrDefault(s => s.CurrentState == state);
                if (step == null)
                {
                    break;
                }

                await Task.Delay(step.ExecuteTime);

                if (!await OnLeaveStateProc(state, transport))
                {
                    break;
                }

                state = step.NextState;
            }
        }
    }

    class LogBuilder
    {
        private Stack<string> _segments = new Stack<string>();
        private bool _lastSegmentIsTemporary = false;

        public override string ToString()
        {
            return string.Join(" -> ", _segments.Reverse());
        }

        public string Append(string segment, bool isTemporary = false)
        {
            if (_lastSegmentIsTemporary)
            {
                _segments.Pop();
            }
            _segments.Push(segment);
            _lastSegmentIsTemporary = isTemporary;

            return ToString();
        }
    }
}
