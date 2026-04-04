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
        private OpTab speedrunTimerTab;
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

        public readonly Configurable<bool> PreventTimerFading;
        public readonly Configurable<bool> ShowCompletedAndLost;
        public readonly Configurable<bool> ShowOldTimer;
        public readonly Configurable<bool> ShowTimerInSleepScreen;
        public readonly Configurable<bool> ShowTotTime;
        public readonly Configurable<string> TimerPosition;
        public static readonly string[] TimerPositions =
        [
            "Top (Default)",
            "Top Left",
            "Top Right",
            "Bottom Left",
            "Bottom Right",
            "Bottom"
        ];
        public readonly Configurable<Color> TimerColor;

        private UIelement[] mainTabOptions;
        private UIelement[] speedrunTabOptions;

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

            PreventTimerFading = config.Bind("PreventTimerFading", false);
            ShowCompletedAndLost = config.Bind("ShowCompletedAndLost", false);
            ShowOldTimer = config.Bind("ShowOldTimer", false);
            ShowTimerInSleepScreen = config.Bind("ShowTimerInSleepScreen", false);
            ShowTotTime = config.Bind("ShowTotTime", false);
            TimerPosition = config.Bind("TimerPosition", TimerPositions[0]);
            TimerColor = config.Bind("TimerColor", Color.white);
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
                new ListItem("{ID}<oB>0<oA>FireEgg<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0", "FireEgg", 4),
                new ListItem("{ID}<oB>0<oA>FlareBomb<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0", "FlareBomb", 5),
                new ListItem("{ID}<oB>0<oA>FlyLure<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0", "FlyLure", 6),
                new ListItem("Hazer<cA>{ID}<cB>0<cA>SI_SAINTINTRO.-1<cA>", "Hazer", 7),
                new ListItem("{ID}<oB>0<oA>Lantern<oA>SI_SAINTINTRO.18.5.-1", "Lantern", 0),
                new ListItem("{ID}<oB>0<oA>PuffBall<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0", "PuffBall", 8),
                new ListItem("{ID}<oB>0<oA>Rock<oA>SI_SAINTINTRO.18.5.-1", "Rock", 9),
                new ListItem("{ID}<oB>0<oA>ScavengerBomb<oA>SI_SAINTINTRO.18.5.-1", "ScavengerBomb", 10),
                new ListItem("{ID}<oB>0<oA>SporePlant<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0<oA>0<oA>0", "SporePlant", 11),
                new ListItem("VultureGrub<cA>{ID}<cB>0<cA>SI_SAINTINTRO.-1<cA>", "VultureGrub", 12),
                new ListItem("{ID}<oB>0<oA>WaterNut<oA>SI_SAINTINTRO.18.5.-1<oA>0<oA>0<oA>0", "WaterNut", 13),
            };

            mainTab = new OpTab(this, "Main");
            speedrunTimerTab = new OpTab(this, "Timer Tweaks");
            remixCheckTab = new OpTab(this, "Remix Check");
            RemixCheck.RemixCheck.remixTab = remixCheckTab;
            remixCheckTab.OnPostActivate += () => RemixCheck.RemixCheck.Populate();

            Tabs = new[] { mainTab, speedrunTimerTab, remixCheckTab };


            var mainTitle = new OpLabel(new Vector2(150f, 575f), new Vector2(300f, 30f), "Speedrunning QOL", FLabelAlignment.Center, true);
            mainTitle.label.shader = Custom.rainWorld.Shaders["MenuText"];
            OpImage mainSeparator = new OpImage(new Vector2(0f, 564f), "pixel") { scale = new Vector2(600f, 2f), color = MenuColorEffect.rgbMediumGrey };

            var timerTitle = new OpLabel(new Vector2(150f, 575f), new Vector2(300f, 30f), "Speedrun Timer Tweaks", FLabelAlignment.Center, true);
            timerTitle.label.shader = Custom.rainWorld.Shaders["MenuText"];
            OpImage timerSeparator = new OpImage(new Vector2(0f, 564f), "pixel") { scale = new Vector2(600f, 2f), color = MenuColorEffect.rgbMediumGrey };

            mainTabOptions = new UIelement[]
            {
                mainTitle,
                mainSeparator,

                new OpCheckBox(FastMenuReset, 5f, 527f) { description = "Press keybind to instantly restart a campaign and skip cutscenes if applicable" },
                new OpLabel(37f, 530f, "Fast save restart (menu)") {alignment = FLabelAlignment.Left, description = "Press keybind to instantly restart a campaign and skip cutscenes if applicable. Illegal for the All Unlocks Vanilla speedrun"},
                new OpKeyBinder(FastResetKey, new Vector2(181f, 506f), new Vector2(120f, 20f), true, OpKeyBinder.BindController.AnyController) { description = "Keybind for game/menu fast restart" },

                new OpCheckBox(FastGameReset, 5f, 492f) { description = "(small flashing lights) Press and hold keyind in-game for 1.5 seconds to restart current campaign and skip cutscenes if applicable"},
                new OpLabel(37f, 495f, "Fast save restart (game)") {alignment = FLabelAlignment.Left, description = "(small flashing lights) Press and hold keyind in-game for 1.5 seconds to restart current campaign and skip cutscenes if applicable. Illegal for the All Unlocks Vanilla speedrun"},

                new OpCheckBox(SaintDetPopcorn, 5f, 457f) { description = "Make Saint tutorial popcorn always pop 5 seconds after entering SI_C02 for the first time, as though optimal RNG"},
                new OpLabel(37f, 460f, "Consistent Saint tutorial popcorn") {alignment = FLabelAlignment.Left, description = "Make Saint tutorial popcorn always pop 5 seconds after entering SI_C02 for the first time, as though optimal RNG"},

                new OpCheckBox(MoonUncloak, 5f, 422f) { description = "Moon's cloak will always exist in MS_FARSIDE for slugcats that can obtain it when restarting a save. Actions taken in other campaigns have no effect in a given campaign"},
                new OpLabel(37f, 425f, "Moon cloak campaign independence") {alignment = FLabelAlignment.Left, description = "Moon's cloak will always exist in MS_FARSIDE for slugcats that can obtain it when restarting a save. Actions taken in other campaigns have no effect in a given campaign"},

                new OpCheckBox(WatcherIntroSkip, 5f, 387f) { description = "Beginning Watcher's campaign will start Watcher in the selected starting region with the selected options"},
                new OpLabel(37f, 390f, "Watcher Intro Skip") {alignment = FLabelAlignment.Left, description = "Beginning Watcher's campaign will start Watcher in the selected starting region with the selected options"},
                new OpComboBox(WISRegionString, new Vector2(153f, 387f), 150f, WISRegionList) { description = "Starting region"},

                new OpCheckBox(WISReinforcedKarma, 5f, 352f) { description = "The Watcher starts their campaign with reinforced karma (karma flower effect)" },
                new OpLabel(37f, 355f, "Reinforced karma") {alignment = FLabelAlignment.Left, description = "The Watcher starts their campaign with reinforced karma (karma flower effect)"},

                new OpCheckBox(WISSpreadRot, 5f, 317f) { description = "The Watcher starts spreads rot to starting region (forced for Coral Caves to match game behavior)"},
                new OpLabel(37f, 320f, "Spread rot") {alignment = FLabelAlignment.Left, description = "The Watcher starts spreads rot to starting region (forced for Coral Caves to match game behavior)"},

                new OpCheckBox(FixedSkipVoid, 5f, 282f) { description = "Skip the void sea sequence when the speedrun timer finishes in SB_L01" },
                new OpLabel(37f, 285f, "Skip void sea") {alignment = FLabelAlignment.Left, description = "Skip the void sea sequence when the speedrun timer finishes in SB_L01"},

                new OpCheckBox(CustomSaintStomach, 5f, 247f) { description = "Begin Saint's campaign with the selected item in stomach" },
                new OpLabel(37f, 250f, "Override Saint stomach item") {alignment = FLabelAlignment.Left, description = "Begin Saint's campaign with the selected item in stomach. Does not save to save file unless campaign is completed"},
                new OpComboBox(CSSItemString, new Vector2(215f, 247f), 150f, CSSList) { description = "Item" },
            };
            mainTab.AddItems(mainTabOptions);

            speedrunTabOptions = new UIelement[]
            {
                timerTitle,
                timerSeparator,

                new OpCheckBox(PreventTimerFading, 5f, 527f) { description = "Prevent IGT from fading out, making it more visible all the time" },
                new OpLabel(37f, 530f, "Prevent Timer Fading") {alignment = FLabelAlignment.Left, description = "Prevent IGT from fading out, making it more visible all the time"},

                new OpCheckBox(ShowCompletedAndLost, 5f, 492f) { description = "Show complete time (cycles where player survived) and lost time (cycles where player died) on the select menu"},
                new OpLabel(37f, 495f, "Show Completed & Lost Time") {alignment = FLabelAlignment.Left, description = "Show complete time (cycles where player survived) and lost time (cycles where player died) on the select menu"},

                new OpCheckBox(ShowOldTimer, 5f, 457f) { description = "Display the old IGT below/beside the current one in game/on the select screen"},
                new OpLabel(37f, 460f, "Show Legacy Timer") {alignment = FLabelAlignment.Left, description = "Display the old IGT below/beside the current one in game/on the select screen"},

                new OpCheckBox(ShowTimerInSleepScreen, 5f, 422f) { description = "Display the speedrun timer in the sleep screen" },
                new OpLabel(37f, 425f, "Show Timer in Sleep Screen?") {alignment = FLabelAlignment.Left, description = "Display the speedrun timer in the sleep screen"},

                new OpCheckBox(ShowTotTime, 5f, 387f) { description = "Show totTime (relevant for several calculations) below/beside the timer in game/on the select screen"},
                new OpLabel(37f, 390f, "Show totTime") {alignment = FLabelAlignment.Left, description = "Show totTime (relevant for several calculations) below/beside the timer in game/on the select screen"},

                new OpLabel(5f, 355f, "Timer Position") {alignment = FLabelAlignment.Left, description = "Change speedrun timer position between several presets"},
                new OpComboBox(TimerPosition, new Vector2(94f, 352f), 150f, TimerPositions) { description = "Change speedrun timer position between several presets"},

                new OpLabel(365f, 528f, "Timer Color") {alignment = FLabelAlignment.Left, description = "Set a custom speedrun timer color"},
                new OpColorPicker(TimerColor, new Vector2(440f, 395f)) { description = "Set a custom speedrun timer color"},
            };
            speedrunTimerTab.AddItems(speedrunTabOptions);
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
