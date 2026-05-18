using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using System.Diagnostics;

namespace RWSQOL.Modules
{
    /// <summary>
    /// This class steps in when the most recent player input buffer is written and alters the X or Y values based on the last-wins priority SOCD ruleset.
    /// </summary>
    public class SOCD
    {
        private static bool MenuToggle => Plugin.Instance.options.SOCD.Value;
        public static KeyCode SOCDKey => Plugin.Instance.options.SOCDKey.Value;

        public static bool Toggled = true;

        private static bool prevLeftHeld = false;
        private static bool prevRightHeld = false;
        private static int lastLeftFrame = -1;
        private static int lastRightFrame = -1;

        private static bool prevUpHeld = false;
        private static bool prevDownHeld = false;
        private static int lastUpFrame = -1;
        private static int lastDownFrame = -1;

        public static void Apply()
        {
            IL.Player.checkInput += Player_checkInput;
        }

        /// <summary>
        /// This IL hook steps in after player.input[0] (most recent buffer) is recorded. It checks if opposite cardinal directions are held and resolves the newest to be absolute.
        /// </summary>
        /// <param name="il"></param>
        private static void Player_checkInput(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(RWInput), nameof(RWInput.PlayerInput))))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);

                c.EmitDelegate((Player p) =>
                {
                    if (!MenuToggle || !Toggled) return;
                    var controls = p.room?.game?.rainWorld?.options?.controls[0];
                    if (controls == null) return;

                    bool leftHeld = Input.GetKey(controls.KeyboardLeft);
                    bool rightHeld = Input.GetKey(controls.KeyboardRight);
                    bool upHeld = Input.GetKey(controls.KeyboardUp);
                    bool downHeld = Input.GetKey(controls.KeyboardDown);

                    int frame = Time.frameCount;

                    // Gotta do this manually because getkeydown might actually be the most useless thing ever written
                    if (leftHeld && !prevLeftHeld) lastLeftFrame = frame;
                    if (rightHeld && !prevRightHeld) lastRightFrame = frame;
                    if (upHeld && !prevUpHeld) lastUpFrame = frame;
                    if (downHeld && !prevDownHeld) lastDownFrame = frame;

                    prevLeftHeld = leftHeld;
                    prevRightHeld = rightHeld;
                    prevUpHeld = upHeld;
                    prevDownHeld = downHeld;

                    // readabilityminning but performancemaxxing. Basically if opposite held, use the newer input
                    p.input[0].x = (leftHeld == rightHeld) ? (leftHeld ? (lastLeftFrame > lastRightFrame ? -1 : 1) : 0) : (leftHeld ? -1 : 1);
                    p.input[0].y = (upHeld == downHeld) ? (upHeld ? (lastUpFrame > lastDownFrame ? 1 : -1) : 0) : (upHeld ? 1 : -1);
                });
            }
            else
            {
                Plugin.Logger.LogError("Player_checkInput failed to match: " + il);
            }
        }
    }
}
