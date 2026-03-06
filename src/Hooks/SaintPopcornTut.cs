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
    /// Remove randomness from Saint's tutorial popcorn. When you enter C02, this room script sets all popcorn plants in the room to be the cycle time + 200 ticks (5s) + 0-100 ticks random (0-2.5s).
    /// This is the time before the game even begins to check to pop the popcorn, which after that point it rolls a 0.2% each tick. This removes both the 0-100 extra ticks before pop check and the 
    /// 0.2% check entirely
    /// </summary>
    public static class SaintPopcornTut
    {
        public static bool Toggled => Plugin.Instance.options.SaintDetPopcorn.Value;
        public static void Apply()
        {
            IL.MoreSlugcats.MSCRoomSpecificScript.SI_SAINTINTRO_tut.Update += SI_SAINTINTRO_tut_Update;
            IL.SeedCob.Update += SeedCob_Update;
        }

        /// <summary>
        /// Paired with tutorial frozen time setting, remove the 0.2% random check on spawning seeds.
        /// </summary>
        /// <param name="il"></param>
        private static void SeedCob_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt(typeof(SeedCob), nameof(SeedCob.spawnUtilityFoods))))
            {
                c.Index--;
                ILLabel spawnLabel = c.DefineLabel();
                c.MarkLabel(spawnLabel);

                if (c.TryGotoPrev(MoveType.Before, x => x.MatchCall(typeof(UnityEngine.Random), "get_value")))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<SeedCob, bool>>((self) =>
                    {
                        if (Toggled && self.room.game.rainWorld.progression.currentSaveState.cycleNumber == 0 && self.room.abstractRoom.name == "SI_C02")
                        {
                            return true;
                        }
                        return false;
                    });
                    c.Emit(OpCodes.Brtrue_S, spawnLabel);
                }
                else
                {
                    Plugin.Logger.LogError("Random.value call not found.");
                }
            }
            else
            {
                Plugin.Logger.LogError("SeedCob_Update failed to match: " + il);
            }
        }

        /// <summary>
        /// Specifically remove random from saint popcorn tutorial popcorn's AllPlantsFrozenCycleTime. 
        /// SeedCob.Update uses an inverselerp between cycle end to frozen time, we want that frozen time to be consistent.
        /// </summary>
        /// <param name="il"></param>
        private static void SI_SAINTINTRO_tut_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(UnityEngine.Random), nameof(UnityEngine.Random.Range)), x => x.MatchLdloc(16)))
            {
                c.EmitDelegate<Func<int, int>>((originalRand) =>
                {
                    if (Toggled)
                    {
                        return 0;
                    }
                    return originalRand;
                });
            }
            else
            {
                Plugin.Logger.LogError("SI_SAINTINTRO_tut_Update failed to match: " + il);
            }
        }
    }
}
