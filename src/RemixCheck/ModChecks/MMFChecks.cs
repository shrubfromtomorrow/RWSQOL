using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.RemixCheck.ModChecks
{
    /// <summary>
    /// Settings checks for Remix
    /// </summary>
    public static class MMFChecks
    {
        private static readonly string MMFModID = ModIDs.Remix;
        private static readonly string MMFModName = ModManager.GetModById(ModIDs.Remix).name;

        public static IEnumerable<SettingCheck> Checks => _checks;

        private static readonly SettingCheck[] _checks =
        {
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgSpeedrunTimer",
                SettingName = "Speedrun Timer",
                TabName = "General",
                Expected = "true",
                Reason = "The speedrun timer must be enabled for all speedruns"
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgDislodgeSpears",
                SettingName = "Pull spears from walls",
                TabName = "General",
                Expected = "false",
                Reason = "Pull spears from walls must be disabled for all speedruns"
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgKeyItemPassaging",
                SettingName = "Key items on Passage",
                TabName = "Assists",
                Expected = "false",
                Reason = "Key items on Passage must be disabled for all speedruns",
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgNoRandomCycles",
                SettingName = "No randomized cycle durations",
                TabName = "Assists",
                Expected = "false",
                Reason = "No randomized cycle durations (NRCD) must be disabled if running base game Intended% runs / Hunter All Objectives. Otherwise this setting is optional",
                Conditional = true,
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgFreeSwimBoosts",
                SettingName = "No swim boost penalty",
                TabName = "Assists",
                Expected = "false",
                Reason = "No swim boost penalty must be disabled for all speedruns",
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgHunterCycles",
                SettingName = "Hunter Cycles",
                TabName = "Assists",
                Expected = "20",
                Reason = "Hunter Cycles must be set to 20",
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgHunterBonusCycles",
                SettingName = "Hunter Bonus Cycle",
                TabName = "Assists",
                Expected = "5",
                Reason = "Hunter Bonus Cycles must be set to 5",
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgSlowTimeFactor",
                SettingName = "Slow Motion Factor",
                TabName = "Assists",
                Expected = "1", // this is a system.single in the mmf settings and they truncate
                Reason = "Slow Motion Factor must be set to 1.00",
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgGlobalMonkGates",
                SettingName = "Monk-style gates for all campaigns",
                TabName = "Assists",
                Expected = "false",
                Reason = "Monk-style gates for all campaigns must be disabled for all speedruns",
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgDisableGateKarma",
                SettingName = "Disabled all karma requirements",
                TabName = "Assists",
                Expected = "false",
                Reason = "Disabled all karma requirements must be disabled for all speedruns",
            },
            new SettingCheck
            {
                ModName = MMFModName,
                ModID = MMFModID,
                SettingKey = "cfgRainTimeMultiplier",
                SettingName = "Rain Timer Multiplier",
                TabName = "Assists",
                Expected = "1", 
                Reason = "Rain Timer Multiplier must 1.0 for all speedruns",
            },
        };
    }
}
