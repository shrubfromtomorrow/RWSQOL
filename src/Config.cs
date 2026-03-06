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
        private bool FastResetGameKeyGreyed;

        public readonly Configurable<bool> FastResetMenu;
        public readonly Configurable<bool> FastResetGame;
        public readonly Configurable<KeyCode> FastResetGameKey;
        public readonly Configurable<bool> SaintDetPopcorn;

        private UIelement[] options;

        public Config()
        {
            FastResetMenu = config.Bind<bool>("FastResetMenu", true);
            FastResetGame = config.Bind<bool>("FastResetGame", true);
            FastResetGameKey = config.Bind<KeyCode>("FastResetGameKey", KeyCode.Backspace);
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

                new OpLabel(10f, 530f, "Fast save restart (menu):") {alignment = FLabelAlignment.Left, description = "Special + map when selecting a slugcat: instantly restarts a campaign and skips cutscenes if applicable. Illegal for multi-campaign speedruns."},
                new OpCheckBox(FastResetMenu, 151f, 527f),

                new OpLabel(10f, 495f, "Fast save restart (game):") {alignment = FLabelAlignment.Left, description = "Press and hold keyind in-game for 2 seconds to restart current campaign and skip cutscenes if applicable. Illegal for multi-campaign speedruns."},
                new OpCheckBox(FastResetGame, 150f, 492f),
                new OpKeyBinder(FastResetGameKey, new Vector2(180f, 488f), new Vector2(110f, 20f), false),

                new OpLabel(10f, 460f, "Consistent Saint tutorial popcorn:") {alignment = FLabelAlignment.Left, description = "Make Saint tutorial popcorn always pop 5 seconds after entering SI_C02 for the first time, as though optimal RNG."},
                new OpCheckBox(SaintDetPopcorn, 200f, 457f),
            };
            mainTab.AddItems(options);
        }

        public override void Update()
        {
            base.Update();

            foreach (var item in Tabs[0].items)
            {
                if (item is OpCheckBox b && b.cfgEntry == FastResetGame)
                {
                    FastResetGameKeyGreyed = b.GetValueBool();
                }

                if (item is OpKeyBinder k && (k.cfgEntry == FastResetGameKey))
                {
                    k.greyedOut = !FastResetGameKeyGreyed;
                }
            }
        }
    }
}
