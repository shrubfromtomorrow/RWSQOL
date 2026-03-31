using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Watcher;

namespace RWSQOL.Modules
{

    /// <summary>
    /// This class handles skipping Watcher's intro sequence with the three SpinningTop encounters and works in conjunction with the fast reset handler.
    /// </summary>

    // More candid comment here since this is by far the most confusing module:
    // While all of this could technically work just by setting warpPointTargetAfterWarpPointSave when the savestate is made, there is more that needs to be done to maintain save integrity.
    // We also manually update certain things like ripple and traversed regions. However, rotting the region without manually faking a regionstate in the savestate (miserable, don't try) is hard.
    // To do this, we wait until the region is through NewWorldLoaded_Room. Typically the game rots regions before going to the sleep screen and saving, but we must do it now.
    // So, pending rot is set, then orig is called literally just to rot the region and do the normal game stuff.
    // However, if we are to die now, none of this is saved to disk, so we must do a save as well. However, by this time, the game has set warpPointTargetAfterWarpPointSave to null.
    // So, we must again set warpPointTargetAfterWarpPointSave, then save everything to file (both the rot state and warpPointTargetAfterWarpPointSave).
    // After this, if the player were to die, warpPointTargetAfterWarpPointSave is set to not null in memory so the player is unnaturally brought to the ripple ladder screen to lose karma.
    // To fix this, warpPointTargetAfterWarpPointSave is set to null in memory. After this point, the save state should be as the game naturally expects, barring stuff with HI.

    public class WatcherIntroSkip
    {
        public static bool Toggled => Plugin.Instance.options.WatcherIntroSkip.Value;
        public static string EntryRegion => Plugin.Instance.options.WISRegionString.Value;
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
            On.Watcher.WarpPoint.NewWorldLoaded_Room += WarpPoint_NewWorldLoaded_Room;
        }

        /// <summary>
        /// Update savestate on construction to allow for landing in selected region with correct states.
        /// The core is warpPointTargetAfterWarpPointSave, which is set to the warp point data fudged using the regionToPO entry.
        /// The den position is also critically updated to allow for the player to be added to the room correctly as process switches to the game.
        /// Most of this is sourced from WarpPoint.NewWorldLoaded().
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

                WarpPoint.WarpPointData wpData = CreateSpecialWarpData();

                // The meat
                self.saveState.warpPointTargetAfterWarpPointSave = wpData;
                self.saveState.denPosition = wpData.destRoom.ToUpperInvariant();

                // The vegetables
                self.saveState.deathPersistentSaveData.minimumRippleLevel = 1f;
                self.saveState.deathPersistentSaveData.rippleLevel = 1f;
                self.saveState.deathPersistentSaveData.maximumRippleLevel = 1f;

                self.warpsTraversedThisCycle++;
                self.saveState.preserveWarpFatigueAfterWarpPointSave = 0;
                self.saveState.miscWorldSaveData.hasSkippedFirstWarpFatigueTransfer = 1;

                self.saveState.deathPersistentSaveData.reinforcedKarma = KarmaReinforced;
            }
        }

        /// <summary>
        /// Spread rot to the region as it is loaded, save said rot spread afterwards. Also set the warp point target so that when data is saved null isn't saved to disk. Save null to memory.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="newRoom"></param>
        private static void WarpPoint_NewWorldLoaded_Room(On.Watcher.WarpPoint.orig_NewWorldLoaded_Room orig, WarpPoint self, Room newRoom)
        {

            if (!ModManager.Watcher || newRoom.game.GetStorySession.saveStateNumber != WatcherEnums.SlugcatStatsName.Watcher || !Toggled)
            {
                orig(self, newRoom);
            }
            else
            {
                if (newRoom.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.New)
                {
                    newRoom.world.game.GetStorySession.pendingSentientRotInfectionFromWarp = SpreadRot;
                    orig(self, newRoom);
                    newRoom.world.game.GetStorySession.saveState.warpPointTargetAfterWarpPointSave = CreateSpecialWarpData();
                    newRoom.world.game.rainWorld.progression.SaveWorldStateAndProgression(false);
                    newRoom.world.game.GetStorySession.saveState.warpPointTargetAfterWarpPointSave = null; // gotta keep this null in memory in case the player dies (ripplmode karma ladder screen)
                }
                else
                {
                    orig(self, newRoom);
                }
            }
        }

        /// <summary>
        /// Helper method that is a clone of SpinningTopData.CreateWarpPointData (that also creates initial STData) but does not require Room as a parameter. For the purposes of fudging PO data.
        /// </summary>
        /// <returns>Returns fudged warp data</returns>
        private static WarpPoint.WarpPointData CreateSpecialWarpData()
        {
            string[] POString = Regex.Split(regionToPO[EntryRegion], "><");

            PlacedObject po = new PlacedObject(PlacedObject.Type.None, null);
            po.FromString(POString);
            SpinningTopData stData = po.data as SpinningTopData;

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