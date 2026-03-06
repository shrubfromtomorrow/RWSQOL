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
    public static class FastResetGame
    {
        public static bool triggerNextTick;
        private static bool resetWhenPossible;

        public static void Apply()
        {
            On.RainWorldGame.Update += RainWorldGame_Update;
            On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;
        }

        private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, Menu.SlugcatSelectMenu self)
        {
            orig(self);
            if (!resetWhenPossible || self.manager.upcomingProcess != null) return;
            resetWhenPossible = false;

            self.restartCheckbox.Checked = true;
            self.startButton.hasSignalled = true; // For autosplitter
            self.Singal(null, "START");
        }

        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);
            if (!triggerNextTick || self.manager.upcomingProcess != null) return;
            triggerNextTick = false;

            self.ExitGame(true, true);
            self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect);
            resetWhenPossible = true;
        }
    }
}
