using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.Modules
{
    public class Debug
    {
        private static bool Toggled => Plugin.Instance.options.Debug.Value;
        public static void Apply()
        {
            On.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;
        }

        private static void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
        {
            if (Toggled)
            {
                try
                {
                    if (self.room?.abstractRoom != null)
                    {
                        string time = self.game?.GetCampaignTimeTracker()?.TotalFreeTimeSpan != null ? self.game.GetCampaignTimeTracker().TotalFreeTimeSpan.ToString() : "BROKE";
                        Plugin.Logger.LogDebug($"ROOM CHANGE: {self.room.abstractRoom.name} -> {newRoom.abstractRoom.name} @ {time}");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError(ex.ToString());
                }
            }
            orig(self, newRoom, cameraPosition);
        }

        private static void SlugcatSelectMenu_StartGame(On.Menu.SlugcatSelectMenu.orig_StartGame orig, Menu.SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter)
        {
            orig(self, storyGameCharacter);
            if (!Toggled) return;
            try
            {
                if (self.restartChecked || !self.manager.rainWorld.progression.IsThereASavedGame(storyGameCharacter))
                {
                    Plugin.Logger.LogDebug($"CAMPAIGN START: {storyGameCharacter}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex.ToString());
            }
        }
    }
}
