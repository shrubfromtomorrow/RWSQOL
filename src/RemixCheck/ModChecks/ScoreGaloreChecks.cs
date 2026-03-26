using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.RemixCheck.ModChecks
{
    /// <summary>
    /// Settings checks for score galore
    /// </summary>
    public class ScoreGaloreChecks
    {
        private static readonly string ScoreGaloreModID = ModIDs.ScoreGalore;
        private static readonly string ScoreGaloreModName = ModManager.GetModById(ModIDs.ScoreGalore).name;

        public static IEnumerable<SettingCheck> Checks => _checks;

        private static readonly SettingCheck[] _checks =
        {
            new SettingCheck
            {
                ModName = ScoreGaloreModName,
                ModID = ScoreGaloreModID,
                SettingKey = "cfgShowRealTime",
                SettingName = "Show score in real-time",
                TabName = "",
                Expected = "false",
                Reason = "Show score in real-time must be disabled for all speedruns",
            }
        };
    }
}
