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
using HUD;

namespace RWSQOL.Modules
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
        private static float lastHeldTime;

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
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        }

        /// <summary>
        /// Adds a ResetMeter to the hud in-game if toggled.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="cam"></param>
        private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig(self, cam);
            if (FastGameReset && cam.game.IsStorySession && !cam.game.rainWorld.ExpeditionMode) self.AddPart(new ResetMeter(self));
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
            if (!FastGameReset || !self.IsStorySession || self.rainWorld.ExpeditionMode) return;

            if (phase == FastResetPhase.WaitingNextTick)
            {
                phase = FastResetPhase.HoldBegun;
                Plugin.Logger.LogInfo("Hold begun: " + Time.time);
            }
            else if (phase == FastResetPhase.HoldBegun)
            {
                if (Input.anyKey && Input.GetKey(FastResetKey))
                {
                    lastHeldTime = heldTime;
                    heldTime += dt;
                    if (heldTime > HOLDDURATION)
                    {
                        heldTime = 0f;
                        phase = FastResetPhase.HoldCompleted;
                    }
                }
                else
                {
                    Plugin.Logger.LogInfo("Hold failed: " + Time.time);
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

        /// <summary>
        /// This class is a visualizer for the progress on the held reset.
        /// </summary>
        private class ResetMeter : HudPart
        {
            public Vector2 pos;
            public Vector2 lastPos;
            public FSprite startLineSprite;
            public FSprite endLineSprite;
            public const float SEPARATIONDIST = 200f;

            public FContainer fContainer
            {
                get
                {
                    return this.hud.fContainers[1];
                }
            }

            public ResetMeter(HUD.HUD hud) : base(hud)
            {

                this.pos = new Vector2((float)((int)((this.hud.rainWorld.options.ScreenSize.x / 2f) - (SEPARATIONDIST / 2f))) + 4.2f, (float)((int)(this.hud.rainWorld.options.ScreenSize.y - (12f)) + 0.2f));
                this.lastPos = this.pos;

                this.startLineSprite = new FSprite("pixel", true);
                this.startLineSprite.scaleX = 0f;
                this.startLineSprite.scaleY = 10f;
                this.fContainer.AddChild(this.startLineSprite);

                this.endLineSprite = new FSprite("pixel", true);
                this.endLineSprite.scaleX = 1.5f;
                this.endLineSprite.scaleY = 10f;
                this.fContainer.AddChild(this.endLineSprite);
            }

            public override void Draw(float timeStacker)
            {
                float level = Mathf.InverseLerp(0, HOLDDURATION, heldTime);
                level = 1f - Mathf.Pow(1f - level, 2f);

                float scale = level * SEPARATIONDIST;

                this.startLineSprite.scaleX = scale;

                float offset = scale * 0.5f;

                this.startLineSprite.x = this.DrawPos(timeStacker).x + offset;
                this.startLineSprite.y = this.DrawPos(timeStacker).y;
                this.endLineSprite.x = this.DrawPos(timeStacker).x + SEPARATIONDIST + 2f;
                this.endLineSprite.y = this.DrawPos(timeStacker).y;

                this.startLineSprite.alpha = level;
                this.endLineSprite.alpha = level;

                Color startColor = Color.white;
                Color targetColor;
                if (hud.owner is Player p && p?.slugcatStats?.name != null)
                {
                    targetColor = PlayerGraphics.DefaultSlugcatColor(p.slugcatStats.name);
                }
                else
                {
                    // i likey this red
                    targetColor = new Color(0.929f, 0.529f, 0.588f);
                }
                Color currentColor = Color.Lerp(startColor, targetColor, level);

                this.startLineSprite.color = currentColor;
                this.endLineSprite.color = currentColor;
            }

            public override void ClearSprites()
            {
                this.endLineSprite.RemoveFromContainer();
                this.startLineSprite.RemoveFromContainer();
            }

            public Vector2 DrawPos(float timeStacker)
            {
                return this.pos;
            }
        }
    }
}
