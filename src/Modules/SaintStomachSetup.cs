using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using Menu.Remix.MixedUI;

namespace RWSQOL.Modules
{
    /// <summary>
    /// This class interrupts the setting of Saint's stomach rollover object at the start of the campaign and replaces it with the selected object in the remix menu.
    /// This does not overwrite the save item so when this mod is disabled (and the campaign wasn't finished), it will revert to the object in the save file.
    /// </summary>
    public class SaintStomachSetup
    {
        private static bool Toggled => Plugin.Instance.options.CustomSaintStomach.Value;
        public static void Apply()
        {
            IL.MoreSlugcats.MSCRoomSpecificScript.SI_SAINTINTRO_tut.Update += SI_SAINTINTRO_tut_Update;
            //On.AbstractPhysicalObject.ctor += AbstractPhysicalObject_ctor;
            //On.AbstractCreature.ctor += AbstractCreature_ctor;
        }

        // Helpers to find creature and item strings for the config menu
        private static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self,world,creatureTemplate,realizedCreature,pos,ID);
            if (self == null) return;
            try
            {
                Plugin.Logger.LogInfo(SaveState.AbstractCreatureToStringStoryWorld(self));
            }
            catch
            {
                return;
            }
        }
        private static void AbstractPhysicalObject_ctor(On.AbstractPhysicalObject.orig_ctor orig, AbstractPhysicalObject self, World world, AbstractPhysicalObject.AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, type, realizedObject, pos, ID);
            if (self == null) return;
            try
            {
                Plugin.Logger.LogInfo(self.ToString());
            }
            catch
            {
                return;
            }
        }

        

        /// <summary>
        /// Patch in to where the rolloverobject is being used and return the remix value, with a new ID given by the game.
        /// </summary>
        /// <param name="il"></param>
        private static void SI_SAINTINTRO_tut_Update(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchLdfld(typeof(PlayerProgression.MiscProgressionData), nameof(PlayerProgression.MiscProgressionData.saintStomachRolloverObject))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<string, MSCRoomSpecificScript.SI_C02_tut, string>>((originalObject, self) =>
                {
                    if (Toggled)
                    {
                        // new id instead of fixed because why not
                        return Plugin.Instance.options.CSSItemString.Value.Replace("{ID}", self.room.game.GetNewID().ToString());
                    }
                    return originalObject;
                });
            }
            else
            {
                Plugin.Logger.LogError("SI_SAINTINTRO_tut_Update failed to match: " + il);
            }
        }
    }
}
