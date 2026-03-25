using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.RemixCheck.ModChecks
{
    public static class MMFChecks
    {
        private static readonly ModManager.Mod MMF = ModManager.GetModById("rwremix");
        private static readonly string MMFModName = MMF.name;

        public static SettingCheck[] CheckMMFSettings() => _checks.Where(x => !x.Validate(MMF)).ToArray();

        private static readonly SettingCheck[] _checks =
        {
            new SettingCheck(MMFModName, "cfgSpeedrunTimer", "Speedrun Timer", "General", "true", "The speedrun timer must be on for all speedruns"),
            new SettingCheck(MMFModName, "cfgDislodgeSpears", "Pull spears from walls", "General", "false", "Pull spears from walls must be off for all speedruns"),
            new SettingCheck(MMFModName, "cfgNoRandomCycles", "No randomized cycle durations", "General", "false", "No randomized cycle durations (NRCD) must only be off if running base game Intended% runs / Hunter All Objectives", true),
        };
    }
}
