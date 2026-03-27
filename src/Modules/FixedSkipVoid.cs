using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace RWSQOL.Modules
{
    /// <summary>
    /// This class is an alternate implementation of the dev tools skip void setup value. It functions almost the same but triggers once the camera is in void sea mode, rather than when the player dips
    /// into the void sea.
    /// </summary>
    public class FixedSkipVoid
    {
        private static bool Toggled => Plugin.Instance.options.FixedSkipVoid.Value;
        public static void Apply()
        {
            IL.VoidSea.VoidSeaScene.UpdatePlayerInVoidSea += VoidSeaScene_UpdatePlayerInVoidSea;
        }

        /// <summary>
        /// Patch in when the current void sea mode doesn't match the previous one and is now active. The meat of the delegate is copied from the voidseascene update method right before ExitToVoidSeaSlideShow
        /// is called naturally.
        /// </summary>
        /// <param name="il"></param>
        private static void VoidSeaScene_UpdatePlayerInVoidSea(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchStfld(typeof(VoidSea.VoidSeaScene), nameof(VoidSea.VoidSeaScene.cameraOffset))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<VoidSea.VoidSeaScene>>((voidSea) =>
                {
                    if (Toggled && voidSea.playerDipped && voidSea.playerY < 1045f && 
                    voidSea.room.game.manager.upcomingProcess == null && 
                    (!ModManager.Watcher || !(voidSea.room.game.StoryCharacter == Watcher.WatcherEnums.SlugcatStatsName.Watcher)) && 
                    (!ModManager.MSC || !(voidSea.room.game.StoryCharacter == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)))
                    {
                        voidSea.room.game.ExitToVoidSeaSlideShow();
                    }
                });
            }
            else
            {
                Plugin.Logger.LogError("VoidSeaScene_UpdatePlayerInVoidSea failed to match: " + il);
            }
        }
    }
}
