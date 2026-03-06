using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace RWSQOL
{
    public class Config : OptionInterface
    {
        private bool FastResetKeyGreyed;

        public readonly Configurable<bool> FastMenuReset;
        public readonly Configurable<bool> FastGameReset;
        public readonly Configurable<KeyCode> FastResetKey;
        public readonly Configurable<bool> SaintDetPopcorn;

        private UIelement[] options;

        public Config()
        {
            FastMenuReset = config.Bind<bool>("FastMenuReset", true);
            FastGameReset = config.Bind<bool>("FastGameReset", true);
            FastResetKey = config.Bind<KeyCode>("FastResetGameKey", KeyCode.Backspace);
            SaintDetPopcorn = config.Bind<bool>("SaintDetPopcorn", true);
        }

        public override void Initialize()
        {
            base.Initialize();

            OpTab mainTab = new OpTab(this, "Main");

            Tabs = new[] { mainTab };

            var title = new OpLabel(new Vector2(150f, 565f), new Vector2(300f, 30f), "Speedrunning QOL", FLabelAlignment.Center, true);
            title.label.shader = Custom.rainWorld.Shaders["MenuText"];

            options = new UIelement[]
            {
                title,

                new OpLabel(10f, 530f, "Fast save restart (menu):") {alignment = FLabelAlignment.Left, description = "Press keybind to instantly restart a campaign and skip cutscenes if applicable. ILLEGAL FOR MULTI-CAMPAIGN SPEEDRUNS."},
                new OpCheckBox(FastMenuReset, 151f, 527f),
                new OpKeyBinder(FastResetKey, new Vector2(181f, 506f), new Vector2(110f, 20f), false) { description = "Keybind for game/menu fast restart" },

                new OpLabel(10f, 495f, "Fast save restart (game):") {alignment = FLabelAlignment.Left, description = "(small flashing lights) Press and hold keyind in-game for 1.5 seconds to restart current campaign and skip cutscenes if applicable. ILLEGAL FOR MULTI-CAMPAIGN SPEEDRUNS."},
                new OpCheckBox(FastGameReset, 150f, 492f),

                new OpLabel(10f, 460f, "Consistent Saint tutorial popcorn:") {alignment = FLabelAlignment.Left, description = "Make Saint tutorial popcorn always pop 5 seconds after entering SI_C02 for the first time, as though optimal RNG."},
                new OpCheckBox(SaintDetPopcorn, 200f, 457f),
            };
            mainTab.AddItems(options);
        }

        public override void Update()
        {
            base.Update();

            bool fastGameResetValue = false;
            bool fastMenuResetValue = false;

            foreach (var item in Tabs[0].items)
            {
                if (item is OpCheckBox b)
                {
                    if (b.cfgEntry == FastGameReset) fastGameResetValue = b.GetValueBool();
                    if (b.cfgEntry == FastMenuReset) fastMenuResetValue = b.GetValueBool();
                }
            }

            foreach (var item in Tabs[0].items)
            {
                if (item is OpKeyBinder k && k.cfgEntry == FastResetKey)
                {
                    k.greyedOut = !(fastGameResetValue || fastMenuResetValue);
                }
            }
        }
    }
}
