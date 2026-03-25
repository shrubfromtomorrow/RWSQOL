using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using Watcher;

namespace RWSQOL.Modules
{
    /// <summary>
    /// This class handles skipping Watcher's intro sequence with the three SpinningTop encounters and works in conjunction with the fast reset handler.
    /// </summary>
    public class WatcherIntroSkip
    {
        public static bool Toggled => Plugin.Instance.options.WatcherIntroSkip.Value;
        public static string Region => Plugin.Instance.options.WISRegionString.Value;
        public static bool KarmaReinforced => Plugin.Instance.options.WISReinforcedKarma.Value;
        public static bool SpreadRot => Plugin.Instance.options.WISSpreadRot.Value;

        /// <summary>
        /// Mapping Config.WISRegionList items to corresponding placed objects
        /// </summary>
        public static readonly Dictionary<string, string> regionToPO = new Dictionary<string, string>
        {
            { "Sunbaked Alley", "SpinningTopSpot><933.9406><2132.439><18~77~Watcher~WSKB~wskb_c17~490~510~15"},
            { "Coral Caves", "SpinningTopSpot><591.3841><1316.087><37~40~Watcher~WRFA~wrfa_sk04~550~450~16"},
            { "Torrential Railways", "SpinningTopSpot><647.808><357.6914><44~106~Watcher~WSKA~wska_d27~470~410~17"}
        };
        public static void Apply()
        {
            On.StoryGameSession.ctor += StoryGameSession_ctor;
        }

        /// <summary>
        /// Update savestate on construction to allow for landing in selected region with correct states.
        /// The core is warpPointTargetAfterWarpPointSave, which is set to the warp point data fudged using the regionToPO entry.
        /// The den position is also critically updated to allow for the player to be added to the room correctly as process switches to the game.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="saveStateNumber"></param>
        /// <param name="game"></param>
        private static void StoryGameSession_ctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name saveStateNumber, RainWorldGame game)
        {
            orig(self, saveStateNumber, game);
            if (!ModManager.Watcher || saveStateNumber != WatcherEnums.SlugcatStatsName.Watcher || !Toggled) return;

            if (self.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.New)
            {
                string[] POString = Regex.Split(regionToPO[Region], "><");
                string denRoom = Regex.Split(POString[3], "~")[4].ToUpperInvariant(); // this makes me timbers shiver

                PlacedObject po = new PlacedObject(PlacedObject.Type.None, null);
                po.FromString(POString);

                WarpPoint.WarpPointData wpData = CreateSpecialWarpData(po.data as SpinningTopData);

                // The meat
                self.saveState.warpPointTargetAfterWarpPointSave = wpData;
                self.saveState.denPosition = denRoom;

                // The vegetables
                self.saveState.deathPersistentSaveData.minimumRippleLevel = 1f;
                self.saveState.deathPersistentSaveData.rippleLevel = 1f;
                self.saveState.deathPersistentSaveData.maximumRippleLevel = 1f;
                self.saveState.deathPersistentSaveData.reinforcedKarma = KarmaReinforced;
                self.pendingSentientRotInfectionFromWarp = SpreadRot;
                game.rainWorld.progression.SaveWorldStateAndProgression(false);
            }
        }

        /// <summary>
        /// Helper method that is a clone of SpinningTopData.CreateWarpPointData but does not require Room as a parameter. For the purposes of fudging PO data.
        /// </summary>
        /// <param name="stData"></param>
        /// <returns></returns>
        private static WarpPoint.WarpPointData CreateSpecialWarpData(SpinningTopData stData)
        {
            WarpPoint.WarpPointData warpPointData = new WarpPoint.WarpPointData(null);
            warpPointData.destPos = stData.destPos;
            warpPointData.RegionString = stData.RegionString;
            warpPointData.destRoom = stData.destRoom;
            warpPointData.destTimeline = stData.destTimeline;
            warpPointData.panelPos = stData.panelPos;
            warpPointData.deathPersistentWarpPoint = true;
            warpPointData.rippleWarp = stData.rippleWarp;
            warpPointData.oneWay = true;
            if (warpPointData.oneWay)
            {
                warpPointData.oneWayEntrance = true;
                warpPointData.oneWayEntranceIdentified = true;
            }
            warpPointData.cycleSpawnedOn = 0;
            warpPointData.destCam = WarpPoint.GetDestCam(warpPointData);
            return warpPointData;
        }
    }
}