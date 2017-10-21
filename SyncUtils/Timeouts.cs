using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncUtils
{
    public struct TimeoutInstant
    {
        private readonly int _target;
        public TimeoutInstant(int timeout)
        {
            _target = timeout == Timeout.Infinite
                ? _target = -1
                : Environment.TickCount + timeout;
        }

        public int Remaining => _target == -1
            ? Timeout.Infinite
            : _target - Environment.TickCount;

        public bool IsTimeout => Remaining <= 0;

        public static bool ShouldWait(int timeout)
        {
            return timeout != 0;
        }
    }
}
