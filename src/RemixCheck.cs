using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu.Remix.MixedUI;
using RWSQOL.Hooks;
using RWCustom;
using UnityEngine;
using Menu;

namespace RWSQOL
{
    public static class RemixCheck
    {
        private static UIelement[] remixTabInfo;
        public static void Populate(OpTab remixTab)
        {
            var title = new OpLabel(new Vector2(150f, 575f), new Vector2(300f, 30f), "Remix Checker", FLabelAlignment.Center, true);
            title.label.shader = Custom.rainWorld.Shaders["MenuText"];

            remixTabInfo = new UIelement[]
            {
                title,
                new OpImage(new Vector2(0f, 564f), "pixel") { scale = new Vector2(600f, 1f), color = MenuColorEffect.rgbMediumGrey },

                
            };
            remixTab.AddItems(remixTabInfo);
        }
    }
}
