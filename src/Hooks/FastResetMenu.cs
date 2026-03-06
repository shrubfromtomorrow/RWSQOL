using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using System.Configuration;
using RWCustom;

namespace RWSQOL.Hooks
{
    /// <summary>
    /// Enables keybind combo (special + map) to quickly restart saves anew. Also independently skips potential slideshow introductions.
    /// </summary>
    public static class FastResetMenu
    {
        public static bool Toggled => Plugin.Instance.options.FastResetMenu.Value;
        public static void Apply()
        {
            On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;
            IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
        }

        /// <summary>
        /// Steps in at the end of the process switch test to switch directly to game rather than any potential slideshow (akin to map button held when filling start game circle)
        /// </summary>
        /// <param name="il"></param>
        private static void SlugcatSelectMenu_StartGame(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(RWInput), nameof(RWInput.CheckSpecificButton))))
            {
                ILLabel skipDelegate = c.DefineLabel();

                c.Emit(OpCodes.Dup);

                c.Emit(OpCodes.Brtrue_S, skipDelegate);

                c.Emit(OpCodes.Pop);

                c.EmitDelegate<Func<bool>>(() =>
                {
                    return Toggled;
                });

                c.MarkLabel(skipDelegate);
            }
            else
            {
                Plugin.Logger.LogError("SlugcatSelectMenu_StartGame failed to match: " + il);
            }
        }

        /// <summary>
        /// Automatically enables the restart save checkbox and signals as though the start game button has been filled.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, Menu.SlugcatSelectMenu self)
        {
            orig(self);
            if (!Toggled || self.manager.upcomingProcess != null) return;
            // Special and map
            if ((RWInput.CheckSpecificButton(0, 34) && RWInput.CheckSpecificButton(0, 11)))
            {
                self.restartCheckbox.Checked = true;
                self.startButton.hasSignalled = true; // For autosplitter
                self.Singal(null, "START");
            }
        }
    }
}
