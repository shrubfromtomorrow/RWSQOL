using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu.Remix.MixedUI;
using RWSQOL.Modules;
using RWCustom;
using UnityEngine;
using Menu;
using Menu.Remix;
using RWSQOL.RemixCheck.ModChecks;
using MoreSlugcats;
using Mono.Cecil;
using System.Configuration;

namespace RWSQOL.RemixCheck
{
    /// <summary>
    /// This class handles everything to do with building the remix checker. This includes running checks for settings and generating labels for the tab.
    /// </summary>
    public static class RemixCheck
    {
        public static OpTab remixTab;

        /// <summary>
        /// Populates remix check tab with settings to change and illegal mods
        /// </summary>
        public static void Populate()
        {
            // Clear
            OpTab.DestroyItems(remixTab.items.ToArray());

            OpLabel title = new OpLabel(new Vector2(150f, 575f), new Vector2(300f, 30f), "Remix Checker", FLabelAlignment.Center, true);
            title.label.shader = Custom.rainWorld.Shaders["MenuText"];

            OpImage separator = new OpImage(new Vector2(0f, 564f), "pixel")
            {
                scale = new Vector2(600f, 2f),
                color = MenuColorEffect.rgbMediumGrey
            };

            List<SettingCheckResult> invalidSettings = RunSettingsChecks().Where(r => !r.IsValid).ToList();

            List<UIelement> settingLabels = PopulateSettingsLabels(invalidSettings);

            List<UIelement> modsLabels = PopulateModsLabels(settingLabels);

            UIelement[] elements = new UIelement[]
            {
                title,
                separator
            }
            .Concat(settingLabels)
            .Concat(modsLabels)
            .ToArray();

            remixTab.AddItems(elements);
        }

        /// <summary>
        /// Generates labels for invalid settings
        /// </summary>
        /// <param name="invalidSettings"></param>
        /// <returns>Returns list of generated labels as well as the title label and fix button</returns>
        private static List<UIelement> PopulateSettingsLabels(List<SettingCheckResult> invalidSettings)
        {
            List<UIelement> settingsLabels = new List<UIelement>();
            if (invalidSettings.Count > 0)
            {
                OpLabel settingsTitle = new OpLabel(new Vector2(10f, 525f), new Vector2(300f, 30f), "Settings", FLabelAlignment.Left, true);
                settingsLabels.Add(settingsTitle);

                if (invalidSettings.Where(x => !x.IsConditional).Any())
                {
                    OpHoldButton fixSettingsButton = new OpHoldButton(new Vector2(95f, 525f), new Vector2(60f, 25f), "FIX", 40f);
                    fixSettingsButton.OnPressDone += Fix.FixSettings;
                    settingsLabels.Add(fixSettingsButton);
                }
            }

            for (int i = 0; i < invalidSettings.Count; i++)
            {
                SettingCheckResult r = invalidSettings[i];

                OpLabel label = new OpLabel(10f, 500f - (20f * i), $"{r.ModName} -> {(string.IsNullOrEmpty(r.TabName) ? "" : $"{r.TabName}: ")}{r.SettingName}");

                label.color = GetColor(r.IsConditional);
                label.description = r.Reason;

                settingsLabels.Add(label);
            }

            return settingsLabels;
        }

        /// <summary>
        /// Generates labels for invalid mods. Fits as many as possible between title and bottom of page. Truncates long mod names.
        /// </summary>
        /// <param name="settingLabels"></param>
        /// <returns>Returns list of generated labels as well as the title label and fix button</returns>
        private static List<UIelement> PopulateModsLabels(List<UIelement> settingLabels)
        {
            List<UIelement> modsLabels = new List<UIelement>();

            float yAnchor = settingLabels.Count > 0 ? (settingLabels[settingLabels.Count - 1].PosY - 32f) : 525f;
            int rowCount = (int)Math.Ceiling((double)(yAnchor / 20f)) - 1;

            List<ModManager.Mod> illegalMods = new List<ModManager.Mod> { ModManager.GetModById(ModIDs.Remix) };
            foreach (ModManager.Mod mod in ModManager.ActiveMods)
            {
                if (!LegalModsList.Contains(mod.id)) { illegalMods.Add(mod); }
                if (mod.id == ModIDs.Remix) illegalMods.Remove(ModManager.GetModById(ModIDs.Remix));
                if (mod.id == ModIDs.MSC) illegalMods.Add(ModManager.GetModById(ModIDs.MSC));
                if (mod.id == ModIDs.Watcher) illegalMods.Add(ModManager.GetModById(ModIDs.Watcher));
            }

            if (illegalMods.Count > 0)
            {
                OpLabel modsTitle = new OpLabel(new Vector2(10f, yAnchor), new Vector2(300f, 30f), "Mods", FLabelAlignment.Left, true);
                modsLabels.Add(modsTitle);

                List<ModManager.Mod> illegalModsSansConditionals = illegalMods.Where(x => x.id != ModIDs.MSC && x.id != ModIDs.Watcher).ToList();

                if (illegalModsSansConditionals.Count > 0)
                {
                    OpHoldButton fixModsButton = new OpHoldButton(new Vector2(70f, yAnchor), new Vector2(60f, 25f), "FIX", 40f);
                    fixModsButton.OnPressDone += trigger => Fix.FixMods(illegalModsSansConditionals);
                    modsLabels.Add(fixModsButton);
                }
            }

            for (int i = 0; i < illegalMods.Count; i++)
            {
                ModManager.Mod mod = illegalMods[i];
                OpLabel label = new OpLabel(10f + 150f * (i / rowCount), (yAnchor - 25f) - (20f * (i % rowCount)), mod.name.Length > 18 ? (mod.name.Substring(0, 18) + "...") : mod.name);
                label.color = GetColor(false);
                if (mod.id == ModIDs.Remix)
                {
                    label.description = $"{mod.name} must be enabled for all speedruns";
                }
                else if (mod.id == ModIDs.MSC)
                {
                    label.description = $"{mod.name} must be disabled for vanilla speedruns";
                    label.color = GetColor(true);
                }
                else if (mod.id == ModIDs.Watcher)
                {
                    label.description = $"{mod.name} must be disabled for vanilla speedruns";
                    label.color = GetColor(true);
                }
                else
                {
                    label.description = $"{mod.name} must be disabled for all speedruns";
                }
                modsLabels.Add(label);
            }

            return modsLabels;
        }

        /// <summary>
        /// Runs checks to find illegal settings.
        /// </summary>
        /// <returns>Returns list of illegal settings as SettingCheckResults</returns>
        private static List<SettingCheckResult> RunSettingsChecks()
        {
            List<SettingCheckResult> results = new List<SettingCheckResult>();

            if (ModManager.MMF)
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.Remix);
                results.AddRange(MMFChecks.Checks.Select(c => c.Evaluate(mod)));
            }
            if (ModManager.MSC)
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.MSC);
                results.AddRange(MSCChecks.Checks.Select(c => c.Evaluate(mod)));
            }
            if (ModManager.ActiveMods.Contains(ModManager.GetModById(ModIDs.InputDisplay)))
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.InputDisplay);
                results.AddRange(InputDisplayChecks.Checks.Select(c => c.Evaluate(mod)));
            }
            if (ModManager.ActiveMods.Contains(ModManager.GetModById(ModIDs.ScoreGalore)))
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.ScoreGalore);
                results.AddRange(ScoreGaloreChecks.Checks.Select(c => c.Evaluate(mod)));
            }
            if (ModManager.ActiveMods.Contains(ModManager.GetModById(ModIDs.DMS)))
            {
                ModManager.Mod mod = ModManager.GetModById(ModIDs.DMS);
                results.AddRange(DMSChecks.Checks.Select(c => c.Evaluate(mod)));
            }
            return results;
        }

        /// <summary>
        /// Helper method to get color for illegal settings and mods
        /// </summary>
        /// <param name="result"></param>
        /// <returns>Returns Color for label</returns>
        private static Color GetColor(bool result)
        {
            // I just like these :))
            return result ? new Color(0.933f, 0.831f, 0.624f) : new Color(0.929f, 0.529f, 0.588f);
        }

        /// <summary>
        /// All legal mods. A compilation ModID consts
        /// </summary>
        public static readonly List<string> LegalModsList = new()
        {
            ModIDs.RWSQOL,
            ModIDs.Remix,
            ModIDs.MSC,
            ModIDs.Watcher,
            ModIDs.Expedition,
            ModIDs.JollyCoop,
            ModIDs.InputDisplay,
            ModIDs.ScoreGalore,
            ModIDs.DMS,
            ModIDs.FastRollButton,
            ModIDs.Sharpener,
            ModIDs.NoTinnitus,
            ModIDs.SpeedrunTimerFix,
            ModIDs.MusicAnnouncements,
            ModIDs.MergeFix,
            ModIDs.ModPresets,
            ModIDs.NoMoreFullscreenBlur,
            ModIDs.MenuFixes,
            ModIDs.OptimizedRemix,
            ModIDs.RemixAutoRestart,
            ModIDs.NoModUpdateConfirm,
            ModIDs.ExactRequirements,
            ModIDs.WorkshopButton,
            ModIDs.ModInExplorerButton,
            ModIDs.ScrollFix,
            ModIDs.ModlistHotload,
            ModIDs.SixtyFPSMenus,
            ModIDs.YeekMaulingFix
        };
    }

}
