using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0414

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Use this class to invoke default Analytic Events for your game. You will
    /// need to invoke the following methods accordingly:
    ///
    /// - LevelStarted(levelId)
    /// - LevelFailed()
    /// - LevelCompleted()
    ///
    /// - TutorialStepStarted(stepId)
    /// - TutorialStepFailed()
    /// - TutorialStepCompleted()
    ///
    /// - SuggestedRewardedAd(string placementName)
    /// - RewardedAdTriggered(string placementName)
    /// - InterstitialAdTriggered(string placementName)
    /// </summary>
    public static class DefaultAnalytics
    {
        [Obsolete("This method will be removed in future versions. Please use Analytics.OnApplicationPause(bool pause) instead")]
        public static void OnApplicationPause(bool pause)
        {
            Analytics.OnApplicationPause(pause);
        }

        #region Level Tracking
        
        [Obsolete("This method will be removed in future versions. Please use Analytics.LevelStarted(int levelId) instead")]
        public static void LevelStarted(int levelId)
        {
            Analytics.LevelStarted(levelId);
        }

        [Obsolete("This method will be removed in future versions. Please use Analytics.LevelStarted(int levelId) instead")]
        public static void LevelStarted(string levelId)
        {
            if (int.TryParse(levelId, out int result))
            {
                Analytics.LevelStarted(result);
            }
            else
            {
                HomaGamesLog.Warning("LevelStarted only accepts an int as level id. If you want to use strings, use Checkpoints instead.");
            }
        }

        [Obsolete("This method will be removed in future versions. Please use Analytics.LevelFailed() instead")]
        public static void LevelFailed()
        {
            Analytics.LevelFailed("obsolete");
        }

        [Obsolete("This method will be removed in future versions. Please use Analytics.LevelCompleted() instead")]
        public static void LevelCompleted()
        {
            Analytics.LevelCompleted();
        }

        #endregion

        #region Tutorial Steps Tracking                                                                                                                                                                                                                                  

        [Obsolete("This method will be removed in future versions. Please use Analytics.TutorialStepStarted(int step) instead")]
        public static void TutorialStepStarted(int step)
        {
            Analytics.TutorialStepStarted(step);
        }

        [Obsolete("This method will be removed in future versions. Please use Analytics.TutorialStepStarted(int step) instead")]
        public static void TutorialStepStarted(string step)
        {
            if (int.TryParse(step, out int result))
            {
                Analytics.TutorialStepStarted(result);
            }
            else
            {
                HomaGamesLog.Warning("TutorialStepStarted only accepts an int as level id. If you want to use strings, use Checkpoints instead.");
            }
        }

        [Obsolete("This method will be removed in future versions. Please use Analytics.TutorialStepFailed() instead")]
        public static void TutorialStepFailed()
        {
            Analytics.TutorialStepFailed();
        }

        [Obsolete("This method will be removed in future versions. Please use Analytics.TutorialStepCompleted() instead")]
        public static void TutorialStepCompleted()
        {
            Analytics.TutorialStepCompleted();
        }

        #endregion

        #region Ads

        [Obsolete("This method will be removed in future versions. Please use Analytics.SuggestedRewardedAd(string placementName,AdPlacementType adPlacementType = AdPlacementType.Default) instead")]
        public static void SuggestedRewardedAd(string placementName,AdPlacementType adPlacementType = AdPlacementType.Default)
        {
            Analytics.RewardedAdSuggested(placementName, adPlacementType);
        }

        [Obsolete("This method will be removed in future versions. Please use Analytics.RewardedAdTriggered(string placementName,AdPlacementType adPlacementType) instead")]
        public static void RewardedAdTriggered(string placementName,AdPlacementType adPlacementType)
        {
            Analytics.RewardedAdTriggered(placementName, adPlacementType);
        }

        [Obsolete("This method will be removed in future versions. Please use Analytics.InterstitialAdTriggered(string placementName,AdPlacementType adPlacementType) instead")]
        public static void InterstitialAdTriggered(string placementName,AdPlacementType adPlacementType)
        {
            Analytics.InterstitialAdTriggered(placementName, adPlacementType);
        }

        #endregion

        #region Miscellaneous

        [Obsolete("This method will be removed in future versions. Please use Analytics.MainMenuLoaded() instead")]
        public static void MainMenuLoaded()
        {
            Analytics.MainMenuLoaded();
        }

        [Obsolete("This method will be removed in future versions. Please use Analytics.GameplayStarted() instead")]
        public static void GameplayStarted()
        {
            Analytics.GameplayStarted();
        }

        #endregion
    }
}
