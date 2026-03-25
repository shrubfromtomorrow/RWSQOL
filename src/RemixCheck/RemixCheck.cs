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
using RWSQOL.RemixCheck.ModChecks;

namespace RWSQOL.RemixCheck
{
    public static class RemixCheck
    {
        public static void Populate(OpTab remixTab)
        {
            OpLabel title = new OpLabel(new Vector2(150f, 575f), new Vector2(300f, 30f), "Remix Checker", FLabelAlignment.Center, true);
            title.label.shader = Custom.rainWorld.Shaders["MenuText"];

            OpImage headerLine = new OpImage(new Vector2(0f, 564f), "pixel")
            {
                scale = new Vector2(600f, 1f),
                color = MenuColorEffect.rgbMediumGrey
            };

            List<SettingCheck> invalidSettings = CheckSettings();

            List<UIelement> settingLabels = new List<UIelement>();
            for (int i = 0; i < invalidSettings.Count; i++)
            {
                SettingCheck check = invalidSettings[i];
                OpLabel label = new OpLabel(10f, 495f - (20f * i), $"{check.modName} -> {check.settingTabName}: {check.settingName}");
                label.color = check.color;
                label.description = check.reason;
                settingLabels.Add(label);
            }

            UIelement[] elements = new UIelement[]
            {
                title,
                headerLine
            }
            .Concat(settingLabels)
            .ToArray();

            remixTab.AddItems(elements);
        }

        private static List<SettingCheck> CheckSettings()
        {
            List<SettingCheck> list = new List<SettingCheck>();

            if (ModManager.MMF)
                list.AddRange(MMFChecks.CheckMMFSettings());

            return list;
        }
    }
}
