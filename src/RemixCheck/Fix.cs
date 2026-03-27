using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWSQOL.RemixCheck.ModChecks;
using Menu.Remix;
using Menu.Remix.MixedUI;
using Menu;

namespace RWSQOL.RemixCheck
{
    /// <summary>
    /// This class fixes invalid mods and settings.
    /// </summary>
    public static class Fix
    {
        /// <summary>
        /// Fixes invalid settings. Calculates all invalid settings that are not conditional, finds the settings in their respective mods, and sets the settings correctly. Repopulates the remix tab after.
        /// </summary>
        /// <param name="trigger"></param>
        public static void FixSettings(UIfocusable trigger)
        {
            List<SettingCheck> illegalSettings = new List<SettingCheck>();

            if (ModManager.MMF)
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.Remix);
                illegalSettings.AddRange(
                    MMFChecks.Checks.Where(c =>
                    {
                        var result = c.Evaluate(mod);
                        return !result.IsConditional && !result.IsValid;
                    })
                );
            }
            if (ModManager.MSC)
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.MSC);
                illegalSettings.AddRange(MSCChecks.Checks.Where(c =>
                {
                    var result = c.Evaluate(mod);
                    return !result.IsConditional && !result.IsValid;
                }));
            }
            if (ModManager.ActiveMods.Contains(ModManager.GetModById(ModIDs.InputDisplay)))
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.InputDisplay);
                illegalSettings.AddRange(InputDisplayChecks.Checks.Where(c =>
                {
                    var result = c.Evaluate(mod);
                    return !result.IsConditional && !result.IsValid;
                }));
            }
            if (ModManager.ActiveMods.Contains(ModManager.GetModById(ModIDs.ScoreGalore)))
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.ScoreGalore);
                illegalSettings.AddRange(ScoreGaloreChecks.Checks.Where(c =>
                {
                    var result = c.Evaluate(mod);
                    return !result.IsConditional && !result.IsValid;
                }));
            }
            if (ModManager.ActiveMods.Contains(ModManager.GetModById(ModIDs.DMS)))
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.DMS);
                illegalSettings.AddRange(DMSChecks.Checks.Where(c =>
                {
                    var result = c.Evaluate(mod);
                    return !result.IsConditional && !result.IsValid;
                }));
            }

            foreach (var group in illegalSettings.GroupBy(s => s.ModID))
            {
                ModManager.Mod mod = ModManager.GetModById(group.Key);
                OptionInterface oi = MachineConnector.GetRegisteredOI(mod.id);
                Dictionary<string, ConfigurableBase> config = oi.config.configurables;

                foreach (SettingCheck setting in group)
                {
                    if (config.TryGetValue(setting.SettingKey, out var configItem))
                    {
                        configItem.BoxedValue = ValueConverter.ConvertToValue(setting.Expected, configItem.settingType);
                    }
                    else
                    {
                        Plugin.Logger.LogError($"Cannot find setting to fix in {setting.ModName}: " +  setting.SettingKey);
                    }
                }
                oi.config.Save();
            }
            RemixCheck.Populate();
        }

        /// <summary>
        /// Takes a list of illegal mods and find their respective modlist buttons to be disabled. Enables remix if it is on the illegal mod list. Applies mods.
        /// </summary>
        /// <param name="illegalMods"></param>
        public static void FixMods(List<ModManager.Mod> illegalMods)
        {
            foreach (ModManager.Mod mod in illegalMods)
            {
                if (ConfigContainer.menuTab.modList.modButtons.Any((MenuModList.ModButton x) => x.ModID == mod.id))
                {
                    MenuModList.ModButton button = ConfigContainer.menuTab.modList.modButtons.First((MenuModList.ModButton x) => x.ModID == mod.id);
                    button.selectEnabled = false;
                }
            }
            
            if (illegalMods.Contains(ModManager.GetModById(ModIDs.Remix)))
            {
                if (ConfigContainer.menuTab.modList.modButtons.Any((MenuModList.ModButton x) => x.ModID == ModIDs.Remix)) 
                {
                    MenuModList.ModButton button = ConfigContainer.menuTab.modList.modButtons.First((MenuModList.ModButton x) => x.ModID == ModIDs.Remix);
                    button.selectEnabled = true;
                }
            }
            //ConfigContainer.menuTab.modList.RefreshAllButtons();
            ModdingMenu.instance.Singal(null, "APPLYMODS");
        }
    }
}
