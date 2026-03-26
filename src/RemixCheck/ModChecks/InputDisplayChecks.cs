using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.RemixCheck.ModChecks
{
    /// <summary>
    /// Settings checks for Input Display
    /// </summary>
    public class InputDisplayChecks
    {
        private static readonly string InputDisplayModID = ModIDs.InputDisplay;
        private static readonly string InputDisplayModName = ModManager.GetModById(ModIDs.InputDisplay).name;

        public static IEnumerable<SettingCheck> Checks => _checks;

        private static readonly SettingCheck[] _checks =
        {
            new SettingCheck
            {
                ModName = InputDisplayModName,
                ModID = InputDisplayModID,
                SettingKey = "high_performance",
                SettingName = "High Performance",
                TabName = "",
                Expected = "true",
                Reason = "High Performance must be enabled for all speedruns",
            }
        };
    }
}
