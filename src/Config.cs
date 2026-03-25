using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Menu;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace RWSQOL
{
    public class Config : OptionInterface
    {
        public readonly Configurable<bool> FastMenuReset;
        public readonly Configurable<bool> FastGameReset;
        public readonly Configurable<KeyCode> FastResetKey;
        public readonly Configurable<bool> SaintDetPopcorn;
        public readonly Configurable<bool> MoonUncloak;
        public readonly Configurable<bool> WatcherIntroSkip;
        public readonly Configurable<string> WISRegionString;
        public List<ListItem> WISRegionList;
        public readonly Configurable<bool> WISReinforcedKarma;
        public readonly Configurable<bool> WISSpreadRot;


        private UIelement[] mainTabOptions;

        public Config()
        {
            FastMenuReset = config.Bind<bool>("FastMenuReset", true);
            FastGameReset = config.Bind<bool>("FastGameReset", true);
            FastResetKey = config.Bind<KeyCode>("FastResetGameKey", KeyCode.Backspace);
            SaintDetPopcorn = config.Bind<bool>("SaintDetPopcorn", true);
            MoonUncloak = config.Bind<bool>("MoonUncloak", true);
            WatcherIntroSkip = config.Bind<bool>("WatcherIntroSkip", true);
            WISRegionString = config.Bind<string>("WISRegionString", "Sunbaked Alley");
            WISReinforcedKarma = config.Bind<bool>("WISReinforcedKarma", true);
            WISSpreadRot = config.Bind<bool>("WISSpreadRot", true);
        }

        public override void Initialize()
        {
            base.Initialize();

            WISRegionList = new List<ListItem>
            {
                new ListItem("Sunbaked Alley", "Sunbaked Alley", 0),
                new ListItem("Coral Caves", "Coral Caves", 1),
                new ListItem("Torrential Railways", "Torrential Railways", 2)
            };

            OpTab mainTab = new OpTab(this, "Main");
            OpTab remixCheckTab = new OpTab(this, "Remix Check");

            Tabs = new[] { mainTab, remixCheckTab };

            OpImage separator = new OpImage(new Vector2(0f, 564f), "pixel") { scale = new Vector2(600f, 1f), color = MenuColorEffect.rgbMediumGrey };

            var mainTitle = new OpLabel(new Vector2(150f, 575f), new Vector2(300f, 30f), "Speedrunning QOL", FLabelAlignment.Center, true);
            mainTitle.label.shader = Custom.rainWorld.Shaders["MenuText"];

            mainTabOptions = new UIelement[]
            {
                mainTitle,
                separator,

                new OpLabel(10f, 530f, "Fast save restart (menu):") {alignment = FLabelAlignment.Left, description = "Press keybind to instantly restart a campaign and skip cutscenes if applicable"},
                new OpCheckBox(FastMenuReset, 151f, 527f),
                new OpKeyBinder(FastResetKey, new Vector2(181f, 506f), new Vector2(110f, 20f), false) { description = "Keybind for game/menu fast restart" },

                new OpLabel(10f, 495f, "Fast save restart (game):") {alignment = FLabelAlignment.Left, description = "(small flashing lights) Press and hold keyind in-game for 1.5 seconds to restart current campaign and skip cutscenes if applicable"},
                new OpCheckBox(FastGameReset, 150f, 492f),

                new OpLabel(10f, 460f, "Consistent Saint tutorial popcorn:") {alignment = FLabelAlignment.Left, description = "Make Saint tutorial popcorn always pop 5 seconds after entering SI_C02 for the first time, as though optimal RNG"},
                new OpCheckBox(SaintDetPopcorn, 200f, 457f),

                new OpLabel(10f, 425f, "Moon cloak campaign independence:") {alignment = FLabelAlignment.Left, description = "Moon's cloak will always exist in MS_FARSIDE for slugcats that can obtain it when restarting a save. Actions taken in other campaigns have no effect in a given campaign"},
                new OpCheckBox(MoonUncloak, 220f, 422f),

                new OpLabel(10f, 390f, "Watcher Intro Skip:") {alignment = FLabelAlignment.Left, description = "Beginning Watcher's campaign will start Watcher in the selected starting region with the selected options"},
                new OpCheckBox(WatcherIntroSkip, 123f, 387f),
                new OpComboBox(WISRegionString, new Vector2(153f, 387f), 150f, WISRegionList) { description = "Starting region" },

                new OpLabel(10f, 355f, "Reinforced karma:") {alignment = FLabelAlignment.Left, description = "The Watcher starts their campaign with reinforced karma (karma flower effect)"},
                new OpCheckBox(WISReinforcedKarma, 117f, 352f),

                new OpLabel(10f, 320f, "Spread rot:") {alignment = FLabelAlignment.Left, description = "The Watcher starts spreads rot to starting region (forced for Coral Caves to match game behavior)"},
                new OpCheckBox(WISSpreadRot, 76f, 317f),
            };
            mainTab.AddItems(mainTabOptions);

            RemixCheck.RemixCheck.Populate(remixCheckTab);
        }

        public override void Update()
        {
            base.Update();

            bool fastGameResetValue = false;
            bool fastMenuResetValue = false;

            bool WISValue = false;

            bool WISRegionCoral = false;

            foreach (var item in Tabs[0].items)
            {
                if (item is OpCheckBox b)
                {
                    if (b.cfgEntry == FastGameReset) fastGameResetValue = b.GetValueBool();
                    if (b.cfgEntry == FastMenuReset) fastMenuResetValue = b.GetValueBool();
                    if (b.cfgEntry == WatcherIntroSkip) WISValue = b.GetValueBool();
                }
                if (item is OpComboBox b2)
                {
                    if (b2.cfgEntry == WISRegionString)
                    {
                        if (b2.value == "Coral Caves")
                        {
                            WISRegionCoral = true;
                        }
                        else WISRegionCoral = false;
                    }
                }
            }

            foreach (var item in Tabs[0].items)
            {
                if (item is OpKeyBinder k && k.cfgEntry == FastResetKey)
                {
                    k.greyedOut = !(fastGameResetValue || fastMenuResetValue);
                }
                if (item is OpComboBox k2 && k2.cfgEntry == WISRegionString)
                {
                    k2.greyedOut = !WISValue;
                }
                if (item is OpCheckBox b && (b.cfgEntry == WISReinforcedKarma || b.cfgEntry == WISSpreadRot))
                {
                    b.greyedOut = !WISValue;
                }
                if (item is OpCheckBox b2 && b2.cfgEntry == WISSpreadRot && WISRegionCoral)
                {
                    if (WISRegionCoral)
                    {
                        b2.SetValueBool(true);
                        b2.greyedOut = true;
                    }
                    else b2.greyedOut = !WISValue;
                }
            }
        }
    }
}
