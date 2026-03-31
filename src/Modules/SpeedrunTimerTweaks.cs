using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Menu;
using MoreSlugcats;
using static MoreSlugcats.SpeedRunTimer;
using RWCustom;
using UnityEngine;
using UnityEngine.Diagnostics;

#nullable enable

namespace RWSQOL.Modules
{
    /// <summary>
    /// A collection of tweaks to the speerun timer. All code credit goes to forthfora who generously allowed this code merger.
    /// </summary>
    public class SpeedrunTimerTweaks
    {
        private static bool PreventTimerFading => Plugin.Instance.options.PreventTimerFading.Value;
        private static bool ShowCompletedAndLost => Plugin.Instance.options.ShowCompletedAndLost.Value;
        public static bool ShowOldTimer => Plugin.Instance.options.ShowOldTimer.Value;
        public static bool ShowTimerInSleepScreen => Plugin.Instance.options.ShowTimerInSleepScreen.Value;
        public static bool ShowTotTime => Plugin.Instance.options.ShowTotTime.Value;
        public static string TimerPosition => Plugin.Instance.options.TimerPosition.Value;
        public static Color TimerColor => Plugin.Instance.options.TimerColor.Value;

        public static void Apply()
        {
            On.MoreSlugcats.SpeedRunTimer.Update += SpeedRunTimer_Update;
            On.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += SlugcatPageContinue_ctor;
            On.ProcessManager.CreateValidationLabel += ProcessManager_CreateValidationLabel;
            On.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;

            On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;
        }

        // Allow a manual trigger of the new tracker to fallback to the old timer from the slugcat select menu, if SHIFT + R is pressed while the restart checkbox is checked
        private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, Menu.SlugcatSelectMenu self)
        {
            orig(self);

            if (self.restartChecked && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.R))
            {
                self.restartChecked = false;

                var slugcatPage = self.slugcatPages[self.slugcatPageIndex];
                var tracker = SpeedRunTimer.GetCampaignTimeTracker(slugcatPage.slugcatNumber);

                if (tracker == null) return;

                tracker.WipeTimes();

                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect);
            }
            else if (self.restartChecked && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.F))
            {
                self.restartChecked = false;

                var slugcatPage = self.slugcatPages[self.slugcatPageIndex];
                var tracker = SpeedRunTimer.GetCampaignTimeTracker(slugcatPage.slugcatNumber);

                if (tracker == null) return;

                tracker.CompletedFixedTime = tracker.CompletedFreeTime;
                tracker.LostFixedTime = tracker.LostFreeTime;
                tracker.UndeterminedFixedTime = tracker.UndeterminedFreeTime;

                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect);
            }
        }

        private static void SpeedRunTimer_Update(On.MoreSlugcats.SpeedRunTimer.orig_Update orig, SpeedRunTimer self)
        {
            // Last fade is a hack to get the timer to display in the fully faded position whilst being fully visible
            var lastPosX = self.pos.x;
            var lastFade = self.fade;

            if (PreventTimerFading)
            {
                self.fade = 0.0f;
            }


            orig(self);


            var tracker = Utils.GetCampaignTimeTracker();

            if (tracker == null) return;


            self.timeLabel.text = tracker.TotalFreeTimeSpan.GetIGTFormatConditionalMs();

            var additionalTimersShown = 0;

            var game = Utils.RainWorldGame;


            if (ShowOldTimer)
            {
                if (game != null && game.IsStorySession)
                {
                    var oldTime = game.GetStorySession.saveState.totTime
                                  + game.GetStorySession.saveState.deathPersistentSaveData.deathTime
                                  + game.GetStorySession.playerSessionRecords[0].time / 40
                                  + game.GetStorySession.playerSessionRecords[0].playerGrabbedTime / 40;

                    var oldTimeSpan = TimeSpan.FromSeconds(oldTime);

                    self.timeLabel.text += $"\nOLD ({SpeedRunTimer.TimeFormat(oldTimeSpan)})";

                    additionalTimersShown++;
                }
            }

            if (ShowTotTime)
            {
                if (game != null && game.IsStorySession)
                {
                    var totTime = game.GetStorySession.saveState.totTime
                                  + game.GetStorySession.playerSessionRecords[0].time / 40;

                    var totTimeSpan = TimeSpan.FromSeconds(totTime);

                    self.timeLabel.text += $"\nTOT ({SpeedRunTimer.TimeFormat(totTimeSpan)})";
                }

                additionalTimersShown++;
            }

            if (PreventTimerFading)
            {
                self.lastFade = lastFade;
                self.fade = 1.0f;
            }

            if (game != null)
            {
                if (game.cameras[0].voidSeaMode && game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Saint)
                {
                    Player? player = game.Players[0].realizedCreature as Player;
                    if (player != null && player.mainBodyChunk.pos.y > -3000)
                    {
                        self.timeLabel.color = Color.black;
                    }
                    else
                    {
                        self.timeLabel.color = TimerColor;
                    }
                }
                else
                {
                    self.timeLabel.color = TimerColor;
                }
            }


            var screenSize = self.hud.rainWorld.options.ScreenSize;
            var additionalTimerOffset = -15.0f;

            switch (TimerPosition)
            {
                case "Top (Default)":
                    break;

                case "Top Left":
                    self.pos.x += -(screenSize.x / 2.0f) + 125.0f;
                    break;

                case "Top Right":
                    self.pos.x += (screenSize.x / 2.0f) - 125.0f;
                    break;

                case "Bottom Left":
                    additionalTimerOffset = 15.0f;

                    self.pos.x += -(screenSize.x / 2.0f) + 125.0f;

                    self.pos.y += -screenSize.y + 75.0f;
                    break;

                case "Bottom Right":
                    additionalTimerOffset = 15.0f;

                    self.pos.x += (screenSize.x / 2.0f) - 125.0f;

                    self.pos.y += -screenSize.y + 75.0f;
                    break;

                case "Bottom":
                    additionalTimerOffset = 15.0f;

                    self.pos.y += -screenSize.y + 75.0f;
                    break;
            }

            if (additionalTimersShown > 1)
            {
                self.pos.y += additionalTimerOffset * (additionalTimersShown - 1);
            }

            if (TimerPosition == "Top (Default)" && additionalTimersShown > 0 && Utils.RainWorldGame?.devToolsActive == true)
            {
                self.pos.y += additionalTimerOffset;
            }

            if (TimerPosition == "Bottom Left" && self.hud.karmaMeter.fade > 0.0f)
            {
                self.pos.y += 65.0f;

                if (additionalTimersShown > 1)
                {
                    self.pos.y += 15.0f;
                }

                if (Utils.RainWorldGame?.GamePaused == true)
                {
                    self.pos.y += 55.0f;
                }
            }

            if (TimerPosition == "Bottom Right" && ((self.hud.karmaMeter.fade > 0.0f && ModManager.JollyCoop) || (Utils.RainWorldGame?.GamePaused == true)))
            {
                self.pos.y += 45.0f;

                if (additionalTimersShown > 1)
                {
                    self.pos.y += 15.0f;
                }
            }
        }


        // Replace timers on the slugcat select menu
        private static void SlugcatPageContinue_ctor(On.Menu.SlugcatSelectMenu.SlugcatPageContinue.orig_ctor orig, SlugcatSelectMenu.SlugcatPageContinue self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
        {
            orig(self, menu, owner, pageIndex, slugcatNumber);


            if (self.saveGameData.shelterName == null || self.saveGameData.shelterName.Length <= 2) return;


            var tracker = slugcatNumber.GetCampaignTimeTracker();

            if (tracker == null) return;


            var existingTimerFormatted = tracker.TotalFreeTimeSpan.GetIGTFormat(true);
            var existingTimerText = $" ({existingTimerFormatted})";

            var newTimerText = $" ({tracker.TotalFreeTimeSpan.GetIGTFormatConditionalMs()})";


            if (ShowOldTimer)
            {
                var oldTiming = TimeSpan.FromSeconds(self.saveGameData.gameTimeAlive + self.saveGameData.gameTimeDead);
                var oldTimerFormatted = $" ({SpeedRunTimer.TimeFormat(oldTiming)})";
                newTimerText += $" - OLD{oldTimerFormatted}";
            }

            if (ShowTotTime)
            {
                var totTime = TimeSpan.FromSeconds(self.saveGameData.gameTimeAlive);
                var totTimeFormatted = $" ({SpeedRunTimer.TimeFormat(totTime)})";
                newTimerText += $" - TOT{totTimeFormatted}";
            }


            if (ShowCompletedAndLost)
            {
                newTimerText += $"\n(Completed: {TimeSpan.FromMilliseconds(tracker.CompletedFreeTime).GetIGTFormatConditionalMs()} - Lost: {TimeSpan.FromMilliseconds(tracker.LostFreeTime).GetIGTFormatConditionalMs()}";

                if (tracker.UndeterminedFreeTime != 0.0f)
                {
                    newTimerText += $" - Undetermined: {TimeSpan.FromMilliseconds(tracker.UndeterminedFreeTime).GetIGTFormatConditionalMs()}";
                }

                newTimerText += ")";
            }

            self.regionLabel.text = self.regionLabel.text.Replace(existingTimerText, newTimerText);
        }


        // Replace the timer on the validation label
        private static void ProcessManager_CreateValidationLabel(On.ProcessManager.orig_CreateValidationLabel orig, ProcessManager self)
        {
            orig(self);

            var slugcat = self.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;
            var saveGameData = SlugcatSelectMenu.MineForSaveData(self, slugcat);

            if (saveGameData == null) return;


            var tracker = slugcat.GetCampaignTimeTracker();

            if (tracker == null) return;


            var existingTimerFormatted = tracker.TotalFreeTimeSpan.GetIGTFormat(true);
            var existingTimerText = $" ({existingTimerFormatted})";

            var newTimerText = $" ({tracker.TotalFreeTimeSpan.GetIGTFormatConditionalMs()})";


            if (ShowOldTimer)
            {
                var oldTiming = TimeSpan.FromSeconds(saveGameData.gameTimeAlive + saveGameData.gameTimeDead);
                var oldTimerFormatted = $" ({SpeedRunTimer.TimeFormat(oldTiming)})";
                newTimerText += $" - OLD{oldTimerFormatted}";
            }

            if (ShowTotTime)
            {
                var totTime = TimeSpan.FromSeconds(saveGameData.gameTimeAlive);
                var totTimeFormatted = $" ({SpeedRunTimer.TimeFormat(totTime)})";
                newTimerText += $" - TOT{totTimeFormatted}";
            }

            self.validationLabel.text = self.validationLabel.text.Replace(existingTimerText, newTimerText);
        }


        // Optionally add the timer to the sleep & death screen 
        private static void SleepAndDeathScreen_GetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, SleepAndDeathScreen self, KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            orig(self, package);

            if (!ModManager.MMF || !MMF.cfgSpeedrunTimer.Value) return;

            if (!ShowTimerInSleepScreen) return;


            var tracker = package?.characterStats?.name?.GetCampaignTimeTracker();

            if (tracker == null) return;


            var speedrunTimerText = tracker.TotalFreeTimeSpan.GetIGTFormatConditionalMs();

            var additionalTimersShown = 0;


            if (ShowOldTimer)
            {
                if (package?.saveState != null)
                {
                    var oldTimeAlive = package.saveState.totTime;
                    var oldTimeLost = package.saveState.deathPersistentSaveData.deathTime;

                    var oldTimerTimeSpan = TimeSpan.FromSeconds(oldTimeAlive + oldTimeLost);

                    speedrunTimerText += $"\nOLD ({SpeedRunTimer.TimeFormat(oldTimerTimeSpan)})";

                    additionalTimersShown++;
                }
            }

            if (ShowTotTime)
            {
                if (package?.saveState != null)
                {
                    var totTime = package.saveState.totTime;

                    var totTimeSpan = TimeSpan.FromSeconds(totTime);

                    speedrunTimerText += $"\nTOT ({SpeedRunTimer.TimeFormat(totTimeSpan)})";

                    additionalTimersShown++;
                }
            }

            var yOffset = additionalTimersShown > 1 ? -15.0f * (additionalTimersShown - 1) : 0.0f;

            var timerPos = new Vector2(0.0f, 700.0f + yOffset);
            var timerSize = new Vector2(1366.0f, 20.0f);

            var speedrunTimer = new MenuLabel(self, self.pages[0], speedrunTimerText, timerPos, timerSize, true);

            self.pages[0].subObjects.Add(speedrunTimer);
        }
    }
    public static class Utils
    {
        public static string GetIGTFormatConditionalMs(this TimeSpan timeSpan) => timeSpan.GetIGTFormat((ModManager.MMF && MMF.cfgSpeedrunTimer.Value) || Custom.rainWorld.options.validation);

        public static CampaignTimeTracker? GetCampaignTimeTracker() => (Custom.rainWorld?.processManager?.currentMainLoop as RainWorldGame)?.GetCampaignTimeTracker();
        public static CampaignTimeTracker? GetCampaignTimeTracker(this RainWorldGame? game) => game?.GetStorySession?.saveStateNumber?.GetCampaignTimeTracker();
        public static CampaignTimeTracker? GetCampaignTimeTracker(this SlugcatStats.Name? slugcat) => SpeedRunTimer.GetCampaignTimeTracker(slugcat);

        public static RainWorldGame? RainWorldGame => Custom.rainWorld?.processManager?.currentMainLoop as RainWorldGame;
    }
}
