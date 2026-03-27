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
        private OpTab mainTab;
        private OpTab remixCheckTab;

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
        public readonly Configurable<bool> FixedSkipVoid;
        public readonly Configurable<bool> CustomSaintStomach;
        public readonly Configurable<string> CSSItemString;
        public List<ListItem> CSSList;


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
            FixedSkipVoid = config.Bind<bool>("FixedSkipVoid", false);
            CustomSaintStomach = config.Bind<bool>("CustomSaintStomach", false);
            CSSItemString = config.Bind<string>("CSSItemString", "Lantern");
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

            CSSList = new List<ListItem>
            {
                new ListItem("{ID}<oB>0<oA>BubbleGrass<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0<oA>1", "BubbleGrass", 1),
                new ListItem("{ID}<oB>0<oA>DataPearl<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0<oA>", "DataPearl", 2),
                new ListItem("{ID}<oB>0<oA>FirecrackerPlant<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0", "FirecrackerPlant", 3),
                new ListItem("{ID}<oB>0<oA>FlareBomb<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0", "FlareBomb", 4),
                new ListItem("{ID}<oB>0<oA>FlyLure<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0", "FlyLure", 5),
                new ListItem("{ID}<cB>0<cA>SI_SAINTINTRO.-1<cA>", "Hazer", 6),
                new ListItem("ID.-1.6984<oB>0<oA>Lantern<oA>SI_SAINTINTRO.18.5.-1", "Lantern", 0),
                new ListItem("{ID}<oB>0<oA>PuffBall<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0", "PuffBall", 7),
                new ListItem("{ID}<oB>0<oA>Rock<oA>SI_SAINTINTRO.18.5.-1", "Rock", 8),
                new ListItem("{ID}<oB>0<oA>ScavengerBomb<oA>SI_SAINTINTRO.18.5.-1", "ScavengerBomb", 9),
                new ListItem("{ID}<oB>0<oA>SporePlant<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0<oA>0<oA>0", "SporePlant", 10),
                new ListItem("VultureGrub<cA>{ID}<cB>0<cA>SI_SAINTINTRO.-1<cA>", "VultureGrub", 11),
                new ListItem("{ID}<oB>0<oA>WaterNut<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0<oA>0", "WaterNut", 12),
            };

            mainTab = new OpTab(this, "Main");
            remixCheckTab = new OpTab(this, "Remix Check");
            RemixCheck.RemixCheck.remixTab = remixCheckTab;
            remixCheckTab.OnPostActivate += () => RemixCheck.RemixCheck.Populate();

            Tabs = new[] { mainTab, remixCheckTab };

            OpImage separator = new OpImage(new Vector2(0f, 564f), "pixel") { scale = new Vector2(600f, 1f), color = MenuColorEffect.rgbMediumGrey };

            var mainTitle = new OpLabel(new Vector2(150f, 575f), new Vector2(300f, 30f), "Speedrunning QOL", FLabelAlignment.Center, true);
            mainTitle.label.shader = Custom.rainWorld.Shaders["MenuText"];

            mainTabOptions = new UIelement[]
            {
                mainTitle,
                separator,

                new OpCheckBox(FastMenuReset, 5f, 527f) { description = "Press keybind to instantly restart a campaign and skip cutscenes if applicable" },
                new OpLabel(37f, 530f, "Fast save restart (menu)") {alignment = FLabelAlignment.Left, description = "Press keybind to instantly restart a campaign and skip cutscenes if applicable"},
                new OpKeyBinder(FastResetKey, new Vector2(181f, 506f), new Vector2(110f, 20f), false) { description = "Keybind for game/menu fast restart" },

                new OpCheckBox(FastGameReset, 5f, 492f) { description = "(small flashing lights) Press and hold keyind in-game for 1.5 seconds to restart current campaign and skip cutscenes if applicable" },
                new OpLabel(37f, 495f, "Fast save restart (game)") {alignment = FLabelAlignment.Left, description = "(small flashing lights) Press and hold keyind in-game for 1.5 seconds to restart current campaign and skip cutscenes if applicable"},

                new OpCheckBox(SaintDetPopcorn, 5f, 457f) { description = "Make Saint tutorial popcorn always pop 5 seconds after entering SI_C02 for the first time, as though optimal RNG" },
                new OpLabel(37f, 460f, "Consistent Saint tutorial popcorn") {alignment = FLabelAlignment.Left, description = "Make Saint tutorial popcorn always pop 5 seconds after entering SI_C02 for the first time, as though optimal RNG"},

                new OpCheckBox(MoonUncloak, 5f, 422f) { description = "Moon's cloak will always exist in MS_FARSIDE for slugcats that can obtain it when restarting a save. Actions taken in other campaigns have no effect in a given campaign" },
                new OpLabel(37f, 425f, "Moon cloak campaign independence") {alignment = FLabelAlignment.Left, description = "Moon's cloak will always exist in MS_FARSIDE for slugcats that can obtain it when restarting a save. Actions taken in other campaigns have no effect in a given campaign"},

                new OpCheckBox(WatcherIntroSkip, 5f, 387f) { description = "Beginning Watcher's campaign will start Watcher in the selected starting region with the selected options" },
                new OpLabel(37f, 390f, "Watcher Intro Skip") {alignment = FLabelAlignment.Left, description = "Beginning Watcher's campaign will start Watcher in the selected starting region with the selected options"},
                new OpComboBox(WISRegionString, new Vector2(153f, 387f), 150f, WISRegionList) { description = "Starting region" },

                new OpCheckBox(WISReinforcedKarma, 5f, 352f) { description = "The Watcher starts their campaign with reinforced karma (karma flower effect)" },
                new OpLabel(37f, 355f, "Reinforced karma") {alignment = FLabelAlignment.Left, description = "The Watcher starts their campaign with reinforced karma (karma flower effect)"},

                new OpCheckBox(WISSpreadRot, 5f, 317f) { description = "The Watcher starts spreads rot to starting region (forced for Coral Caves to match game behavior)" },
                new OpLabel(37f, 320f, "Spread rot") {alignment = FLabelAlignment.Left, description = "The Watcher starts spreads rot to starting region (forced for Coral Caves to match game behavior)"},

                new OpCheckBox(FixedSkipVoid, 5f, 282f) { description = "Skip the void sea sequence when the speedrun timer finishes in SB_L01" },
                new OpLabel(37f, 285f, "Skip void sea") {alignment = FLabelAlignment.Left, description = "Skip the void sea sequence when the speedrun timer finishes in SB_L01"},

                new OpCheckBox(CustomSaintStomach, 5f, 247f) { description = "Begin Saint's campaign with the selected item in stomach" },
                new OpLabel(37f, 250f, "Overwrite Saint stomach item") {alignment = FLabelAlignment.Left, description = "Begin Saint's campaign with the selected item in stomach"},
                new OpComboBox(CSSItemString, new Vector2(215f, 247f), 150f, CSSList) { description = "Item" },
            };
            mainTab.AddItems(mainTabOptions);

            //RemixCheck.RemixCheck.Populate();
        } 

        public override void Update()
        {
            base.Update();

            bool fastGameResetValue = false;
            bool fastMenuResetValue = false;

            bool WISValue = false;

            bool WISRegionCoral = false;

            bool CSSValue = false;

            foreach (var item in Tabs[0].items)
            {
                if (item is OpCheckBox b)
                {
                    if (b.cfgEntry == FastGameReset) fastGameResetValue = b.GetValueBool();
                    if (b.cfgEntry == FastMenuReset) fastMenuResetValue = b.GetValueBool();
                    if (b.cfgEntry == WatcherIntroSkip) WISValue = b.GetValueBool();
                    if (b.cfgEntry == CustomSaintStomach) CSSValue = b.GetValueBool();
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
                if (item is OpComboBox k3 && k3.cfgEntry == CSSItemString)
                {
                    k3.greyedOut = !CSSValue;
                }
            }
        }
    }
}
