using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MonoMod.RuntimeDetour;

namespace RWSQOL.Modules
{
    public static class Main
    {
        public static void Apply()
        {
            FastResetHandler.Apply();
            FixedSkipVoid.Apply();
            ForceValidation.Apply();
            MoonUncloak.Apply();
            SaintPopcornTut.Apply();
            SaintStomachSetup.Apply();
            SpeedrunTimerTweaks.Apply();
            WatcherIntroSkip.Apply();
        }
    }
}
