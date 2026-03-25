using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RWCustom;
using UnityEngine;

namespace RWSQOL.Modules
{
    /// <summary>
    /// This class handles when Moon's cloak should appear in MS_FARSIDE. Restarting a campaign will mean it is always in place for any slugcat that can obtain it. 
    /// Delivering the cloak for x timeline position has no effect on x+1 timeline position.
    /// </summary>
    public static class MoonUncloak
    {
        public static bool Toggled => Plugin.Instance.options.MoonUncloak.Value;
        public static void Apply()
        {
            new Hook(typeof(PlayerProgression.MiscProgressionData).GetProperty(nameof(PlayerProgression.MiscProgressionData.CloakTimelinePosition)).GetGetMethod(), typeof(MoonUncloak).GetMethod(nameof(MoonUncloak.CloakTimelinePosition_Hook)));
        }

        /// <summary>
        /// Steps in when the cloak timeline position is grabbed and sets it to null for appropriate slugcats.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <returns></returns>
        public static SlugcatStats.Timeline CloakTimelinePosition_Hook(Func<PlayerProgression.MiscProgressionData, SlugcatStats.Timeline> orig, PlayerProgression.MiscProgressionData self)
        {
            if (!Toggled) return orig(self);

            if (self.owner?.currentSaveState?.saveStateNumber == null) return orig(self);

            // Saint and Rivulet should still have moon cloaked.
            if (ModManager.MSC && 
                (self.owner.currentSaveState.saveStateNumber == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet || 
                self.owner.currentSaveState.saveStateNumber == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)) return orig(self);

            return null;
        }
    }
}
