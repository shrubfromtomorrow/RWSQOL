using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu.Remix;

namespace RWSQOL.RemixCheck
{
    /// <summary>
    /// This class is a template for a setting that is to be checked. It holds information relevant to the setting and defines how it is evaluated.
    /// </summary>
    public class SettingCheck
    {
        public string ModName { get; set; }
        public string ModID { get; set; }
        public string SettingKey { get; set; }
        public string SettingName { get; set; }
        public string TabName { get; set; }
        public string Expected { get; set; }
        public string Reason { get; set; }
        public bool Conditional { get; set; }

        public SettingCheck() { }

        /// <summary>
        /// Evaluates a SettingCheck. Takes a mod to be used to find the relevant setting, finds the setting value and checks equivalency.
        /// </summary>
        /// <param name="mod"></param>
        /// <returns>Returns a SettingCheckResult containing information relevant to the validity of the setting.</returns>
        public SettingCheckResult Evaluate(ModManager.Mod mod)
        {
            var result = new SettingCheckResult
            {
                ModName = ModName,
                SettingName = SettingName,
                TabName = TabName,
                Reason = Reason,
                IsConditional = Conditional
            };

            OptionInterface oi = MachineConnector.GetRegisteredOI(mod.id);
            Dictionary<string, ConfigurableBase> config = oi.config.configurables;
            

            if (config.TryGetValue(SettingKey, out var setting))
            {
                string value = ValueConverter.ConvertToString(setting.BoxedValue, setting.settingType);
                result.IsValid = value == Expected;
            }
            else
            {
                result.IsValid = false;
            }

            return result;
        }
    }
}
