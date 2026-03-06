using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using static RWSQOL.Enums.Enums;

namespace RWSQOL
{
    public static class FastResetHandler
    {
        private static FastResetPhase phase = FastResetPhase.Idle;
        public static void TriggerReset()
        {
            phase = FastResetPhase.WaitingNextTick;
        }

        public static void Apply()
        {
            On.RainWorldGame.Update += RainWorldGame_Update;
            On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;
        }

        private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, Menu.SlugcatSelectMenu self)
        {
            orig(self);
            if (phase == FastResetPhase.WaitingNextTick || phase == FastResetPhase.WaitingMenu)
            {
                if (self.manager.upcomingProcess != null)
                {
                    self.restartCheckbox.Checked = true;
                    self.startButton.hasSignalled = true; // For autosplitter
                    self.Singal(null, "START");
                }
                phase = FastResetPhase.Idle;
            }
        }

        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);
            if (phase == FastResetPhase.WaitingNextTick)
            {
                if (self.manager.upcomingProcess != null) return;
                self.ExitGame(true, true);
                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect);
                phase = FastResetPhase.WaitingMenu;
            }
        }
    }
}
