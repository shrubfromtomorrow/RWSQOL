using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RWCustom;
using static RWSQOL.Enums.Enums;
using UnityEngine;

namespace RWSQOL.Hooks
{
    /// <summary>
    /// This class handles whenever the reset keybind is pressed. It will either initiate, track, and complete a hold of the bind in-game to reset, or reset directly from the slugcat select menu.
    /// Any introduction cutscenes are also skipped from this class.
    /// </summary>
    public static class FastResetHandler
    {
        private static bool FastMenuReset => Plugin.Instance.options.FastMenuReset.Value;
        private static bool FastGameReset => Plugin.Instance.options.FastGameReset.Value;
        public static KeyCode FastResetKey => Plugin.Instance.options.FastResetKey.Value;

        private const float HOLDDURATION = 1.5f; // 1.5 seconds hold time
        private static float heldTime;

        private static FastResetPhase phase = FastResetPhase.Idle;
        public static void TriggerReset()
        {
            if (!FastGameReset && !FastMenuReset) return; // Never leave idle unless either of these is true

            phase = FastResetPhase.WaitingNextTick;
        }

        public static void Apply()
        {
            On.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate;
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;
            On.Menu.SlugcatSelectMenu.ctor += SlugcatSelectMenu_ctor;
            IL.Menu.SlugcatSelectMenu.ctor += SlugcatSelectMenu_ctorIL;
            IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
        }

        /// <summary>
        /// Steps in at the end of the process switch test to switch directly to game rather than any potential slideshow (akin to map button held when filling start game circle).
        /// Only triggers if FastGame/MenuReset is true;
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
                    return FastGameReset || FastMenuReset;
                });

                c.MarkLabel(skipDelegate);
            }
            else
            {
                Plugin.Logger.LogError("SlugcatSelectMenu_StartGame failed to match: " + il);
            }
        }

        /// <summary>
        /// Automatically checks the restart checkbox and fills start game circle for campaign. Also sets a specific bool to allow for autosplitter compatability.
        /// Triggers only if the FastResetHandler is in a waiting for next tick (reset pressed in menu) or waiting for menu to exist (reset pressed from in-game) phase.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, Menu.SlugcatSelectMenu self)
        {
            orig(self);
            // This should run from the menu directly only if the option is enabled. Can still run if game reset only is enabled.
            if ((phase == FastResetPhase.WaitingNextTick && FastMenuReset) || phase == FastResetPhase.WaitingMenu)
            {
                Plugin.Logger.LogInfo("In menu, phase is: " + phase.ToString());
                if (self.manager?.upcomingProcess == null)
                {
                    self.restartCheckbox.Checked = true;
                    self.startButton.hasSignalled = true; // For autosplitter
                    self.Singal(null, "START");
                }
                phase = FastResetPhase.Idle;
            }
        }

        /// <summary>
        /// Simple cleanup to make sure phase is idle when entering select menu. Excludes when entering menu from in-game reset.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="manager"></param>
        private static void SlugcatSelectMenu_ctor(On.Menu.SlugcatSelectMenu.orig_ctor orig, Menu.SlugcatSelectMenu self, ProcessManager manager)
        {
            orig(self, manager);
            if (phase != FastResetPhase.WaitingMenu) phase = FastResetPhase.Idle;

            foreach(var thing in self.slugcatColorOrder)
            {
                Plugin.Logger.LogInfo(thing);
            }
            Plugin.Logger.LogInfo("Selected slugcat: " + self.slugcatColorOrder[self.slugcatPageIndex]);
        }

        /// <summary>
        /// Prevent the game from selecting a different slugcat's campaign if they are the only campaign with savedata.
        /// Always select the campaign the player most recently began, regardless of savedata.
        /// </summary>
        /// <param name="il"></param>
        private static void SlugcatSelectMenu_ctorIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchStfld(typeof(Menu.SlugcatSelectMenu), nameof(Menu.SlugcatSelectMenu.slugcatPageIndex))) && 
                c.TryGotoPrev(MoveType.Before, x => x.MatchLdcI4(out _)))
            {
                c.Next.OpCode = OpCodes.Ldc_I4_M1;
            }
            else
            {
                Plugin.Logger.LogError("SlugcatSelectMenu_ctorIL failed to match: " + il);
            }
        }

        /// <summary>
        /// Tracks how long the reset bind has been held for in-game and increments to completion. Once complete, the game is exited and the process is switched to the slugcat select menu
        /// alongside the FastResetHandler being put in a waiting for menu phase
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="dt"></param>
        private static void RainWorldGame_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);
            if (!FastGameReset) return;

            if (phase == FastResetPhase.WaitingNextTick)
            {
                phase = FastResetPhase.HoldBegun;
                Plugin.Logger.LogInfo("Hold begun: " + Time.time);
            }
            else if (phase == FastResetPhase.HoldBegun)
            {
                if (Input.anyKey && Input.GetKey(FastResetKey))
                {
                    heldTime += dt;
                    if (heldTime > HOLDDURATION)
                    {
                        heldTime = 0f;
                        phase = FastResetPhase.HoldCompleted;
                    }
                }
                else
                {
                    Plugin.Logger.LogInfo("Hold failed");
                    heldTime = 0f;
                    phase = FastResetPhase.Idle;
                }
            }
            else if (phase == FastResetPhase.HoldCompleted)
            {
                Plugin.Logger.LogInfo("Hold completed: " + Time.time);
                if (self.manager?.upcomingProcess != null) return;

                self.ExitGame(true, true);
                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect);
                phase = FastResetPhase.WaitingMenu;
            }
        }

        /// <summary>
        /// Simple cleanup to make sure the phase is idle when beginning game.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="manager"></param>
        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);
            phase = FastResetPhase.Idle;
        }
    }
}
