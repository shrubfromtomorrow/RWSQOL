using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.RemixCheck.ModChecks
{
    /// <summary>
    /// Settings checks for Dress My Slugcat
    /// </summary>
    public class DMSChecks
    {
        private static readonly string DMSModID = ModIDs.DMS;
        private static readonly string DMSModName = ModManager.GetModById(ModIDs.DMS).name;

        public static IEnumerable<SettingCheck> Checks => _checks;

        private static readonly SettingCheck[] _checks =
        {
            new SettingCheck
            {
                ModName = DMSModName,
                ModID = DMSModID,
                SettingKey = "LoadInactiveMods",
                SettingName = "Load Inactive Mods",
                TabName = "",
                Expected = "true",
                Reason = "Load Inactive Mods must be enabled and all supporting sprite mods must be disabled",
            }
        };
    }
}
