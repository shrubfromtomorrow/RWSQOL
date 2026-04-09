using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.Enums
{
    public static class Enums
    {
        public enum FastResetPhase
        {
            Idle,
            WaitingNextTick,
            HoldBegun,
            HoldCompleted,
            WaitingMenu,
            Delay
        }
    }
}
