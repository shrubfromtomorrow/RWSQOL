using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu.Remix;

namespace RWSQOL.RemixCheck
{
    public class SettingCheck
    {
        private readonly Color RED = new Color(0.929f, 0.529f, 0.588f);
        private readonly Color YELLOW = new Color(0.933f, 0.831f, 0.624f);

        public string modName;
        public string settingName;
        public string settingKey;
        public string settingTabName;
        private string expected;
        private bool conditional;
        public Color color;
        public string reason;

        public SettingCheck(string modName, string settingKey, string settingName, string settingTabName, string expected, string reason, bool conditional = false)
        {
            this.modName = modName;
            this.settingKey = settingKey;
            this.settingName = settingName;
            this.settingTabName = settingTabName;
            this.reason = reason;
            this.expected = expected;
            this.conditional = conditional;
        }

        public bool Validate(ModManager.Mod mod)
        {
            OptionInterface registeredOI = MachineConnector.GetRegisteredOI(mod.id);
            Dictionary<string, ConfigurableBase> config = registeredOI.config.configurables;

            if (config.ContainsKey(settingKey))
            {
                string value = ValueConverter.ConvertToString(config[settingKey].BoxedValue, config[settingKey].settingType);
                if (value == expected) return true;
            }
            color = conditional ? YELLOW : RED;
            return false;
        }
    }
}
