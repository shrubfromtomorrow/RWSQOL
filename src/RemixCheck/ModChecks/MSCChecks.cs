using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.RemixCheck.ModChecks
{
    /// <summary>
    /// Settings checks for MSC
    /// </summary>
    public class MSCChecks
    {
        private static readonly string MSCModID = ModIDs.MSC;
        private static readonly string MSCModName = ModManager.GetModById(ModIDs.MSC).name;

        public static IEnumerable<SettingCheck> Checks => _checks;

        private static readonly SettingCheck[] _checks =
        {
            new SettingCheck
            {
                ModName = MSCModName,
                ModID = MSCModID,
                SettingKey = "cfgDisablePrecycles",
                SettingName = "Disable shelter failures",
                TabName = "Assists",
                Expected = "false",
                Reason = "Disable shelter failures must be disabled if playing Inv. Otherwise this setting is optional",
                Conditional = true,
            },
            new SettingCheck
            {
                ModName = MSCModName,
                ModID = MSCModID,
                SettingKey = "cfgDisablePrecycleFloods",
                SettingName = "Disable precycle flooding",
                TabName = "Assists",
                Expected = "false",
                Reason = "Disable precycle flooding must be disabled if playing Inv. Otherwise this setting is optional",
                Conditional = true,
            },
            new SettingCheck
            {
                ModName = MSCModName,
                ModID = MSCModID,
                SettingKey = "cfgArtificerCorpseMaxKarma",
                SettingName = "Scavenger corpses have max karma",
                TabName = "Assists",
                Expected = "false",
                Reason = "If Scavenger corpses have max karma is enabled for Artificer Pilgrimage%, you must obtain 5 echoes before ascending with a regular scavenger corpse. Otherwise this setting is optional",
                Conditional = true,
            },
            new SettingCheck
            {
                ModName = MSCModName,
                ModID = MSCModID,
                SettingKey = "cfgArtificerCorpseNoKarmaLoss",
                SettingName = "Lossless scavenger corpses",
                TabName = "Assists",
                Expected = "false",
                Reason = "Lossless scavenger corpses must be disabled for all speedruns",
            },
            new SettingCheck
            {
                ModName = MSCModName,
                ModID = MSCModID,
                SettingKey = "cfgArtificerExplosionCapacity",
                SettingName = "Artificer Explosion Capacity",
                TabName = "Assists",
                Expected = "10",
                Reason = "Artificer Explosion Capacity must be set to 10",
            },
        };
    }
}
